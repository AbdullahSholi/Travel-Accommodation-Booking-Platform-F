using System.Net.Mail;
using AutoMapper;
using Microsoft.Extensions.Options;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils;
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
    private readonly EmailSettings _emailSettings;

    public AuthService(IAuthRepository authRepository, JwtTokenGenerator jwtTokenGenerator, IMapper mapper,
        IOptions<EmailSettings> emailSettings)
    {
        _authRepository = authRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
        _emailSettings = emailSettings.Value;
    }

    public async Task<LoginReadDto?> LoginAsync(LoginWriteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationAppException("Please provide either an email or a username.");

        var user = !string.IsNullOrWhiteSpace(dto.Email)
            ? await _authRepository.GetUserByEmailAsync(dto.Email)
            : await _authRepository.GetUserByUsernameAsync(dto.Username);
        
        if(user == null)
            throw new NotFoundException("User not found");

        if (!PasswordHasher.VerifyPassword(user.Password, dto.Password))
            throw new ValidationAppException("Invalid password");

        var token = _jwtTokenGenerator.GenerateToken(user.Email, user.Role);

        var loginReadDto = new LoginReadDto { Token = token, userId = user.UserId };

        return loginReadDto;
    }

    public async Task<UserReadDto?> RegisterAsync(UserWriteDto userDto)
    {
        await _authRepository.DeleteExpiredUnconfirmedUsersAsync();
        
        var isEmailExist = await _authRepository.EmailExistsAsync(userDto.Email);
        if (isEmailExist)
            throw new Exception(CustomMessages.CustomMessages.DuplicatedEmail);

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
            throw new Exception("User not found");

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
            throw new Exception("Invalid or Expired OTP code");
        
        if(record.Expiration.ToUniversalTime() < DateTime.UtcNow)
            throw new Exception("Expired OTP");

        var user = await _authRepository.GetUserByEmailAsync(readDto.Email);
        if (user == null)
            throw new Exception("User not found");
        
        await _authRepository.HashAndSavePasswordAsync(user, readDto.NewPassword);
        
        await _authRepository.RemoveAndSaveOtpAsync(record);

        return true;
    }
    
    public async Task<bool> VerifyOtpAsync(string email, string otpCode)
    {
        var otpRecord = await _authRepository.GetOtpRecordAsync(email, otpCode);
        if (otpRecord == null)
            throw new Exception("OTP not found. Please request a new one.");

        if (otpRecord.Expiration < DateTime.UtcNow)
            throw new Exception("OTP expired.");

        if (otpRecord.Code != otpCode)
            throw new Exception("Invalid OTP.");

        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found.");
        
        user.IsEmailConfirmed = true;

        await _authRepository.UpdateUserAsync(user);

        await _authRepository.InvalidateOtpAsync(otpRecord);

        return true;
    }

}