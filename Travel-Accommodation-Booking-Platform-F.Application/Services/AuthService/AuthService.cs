using AutoMapper;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Generators;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Hashing;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;

    public AuthService(IAuthRepository authRepository, JwtTokenGenerator jwtTokenGenerator, IMapper mapper)
    {
        _authRepository = authRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
    }

    public async Task<LoginReadDto?> LoginAsync(LoginWriteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationAppException(CustomMessages.EmailOrUsernameNotFound);

        var user = !string.IsNullOrWhiteSpace(dto.Email)
            ? await _authRepository.GetUserByEmailAsync(dto.Email)
            : await _authRepository.GetUserByUsernameAsync(dto.Username);

        if (user == null)
            throw new NotFoundException(CustomMessages.UserNotFound);

        if (!PasswordHasher.VerifyPassword(user.Password, dto.Password))
            throw new ValidationAppException(CustomMessages.InvalidPassword);

        var token = _jwtTokenGenerator.GenerateToken(user.Email, user.Role);

        var loginReadDto = new LoginReadDto { Token = token, userId = user.UserId };

        return loginReadDto;
    }

    public async Task<UserReadDto?> RegisterAsync(UserWriteDto userDto)
    {
        await _authRepository.DeleteExpiredUnconfirmedUsersAsync();

        var isEmailExist = await _authRepository.EmailExistsAsync(userDto.Email);
        if (isEmailExist)
            throw new Exception(CustomMessages.DuplicatedEmail);

        var user = _mapper.Map<User>(userDto);

        await _authRepository.RegisterUserAsync(user);

        var otpCode = OtpGenerator.GenerateOtp();
        var otpRecord = new OtpRecord
        {
            Email = user.Email,
            Code = otpCode,
            Expiration = DateTime.UtcNow.AddMinutes(Constants.OtpExpirationMinutes)
        };

        await _authRepository.SaveOtpAsync(otpRecord);
        await _authRepository.SendOtpAsync(userDto.Email, otpCode);

        var userReadDto = _mapper.Map<UserReadDto>(user);

        return userReadDto;
    }

    public async Task<bool> SendOtpAsync(string email)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
            throw new Exception(CustomMessages.UserNotFound);

        var otp = OtpGenerator.GenerateOtp();
        var otpRecord = new OtpRecord
        {
            Email = email,
            Code = otp,
            Expiration = DateTime.Now.AddMinutes(Constants.OtpExpirationMinutes)
        };

        await _authRepository.SaveOtpAsync(otpRecord);
        await _authRepository.SendOtpAsync(email, otpRecord.Code);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordReadDto readDto)
    {
        var record = await _authRepository.GetOtpRecordAsync(readDto.Email, readDto.Otp);

        if (record == null)
            throw new Exception(CustomMessages.InvalidOrExpiredOtpCode);

        if (record.Expiration.ToUniversalTime() < DateTime.UtcNow)
            throw new Exception(CustomMessages.ExpiredOtpCode);

        var user = await _authRepository.GetUserByEmailAsync(readDto.Email);
        if (user == null)
            throw new Exception(CustomMessages.UserNotFound);

        await _authRepository.HashAndSavePasswordAsync(user, readDto.NewPassword);

        await _authRepository.RemoveAndSaveOtpAsync(record);

        return true;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otpCode)
    {
        var otpRecord = await _authRepository.GetOtpRecordAsync(email, otpCode);
        if (otpRecord == null)
            throw new Exception(CustomMessages.OtpNotFound);

        if (otpRecord.Expiration < DateTime.UtcNow)
            throw new Exception(CustomMessages.ExpiredOtpCode);

        if (otpRecord.Code != otpCode)
            throw new Exception(CustomMessages.InvalidOtp);

        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
            throw new Exception(CustomMessages.UserNotFound);

        user.IsEmailConfirmed = true;

        await _authRepository.UpdateUserAsync(user);

        await _authRepository.InvalidateOtpAsync(otpRecord);

        return true;
    }
}