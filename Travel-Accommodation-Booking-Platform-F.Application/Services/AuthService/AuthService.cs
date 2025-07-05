using AutoMapper;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Generators;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.Hashing;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.FactoryPattern;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Utils;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IOtpSenderFactory _otpSenderFactory;

    public AuthService(IAuthRepository authRepository, ITokenGenerator jwtTokenGenerator, IMapper mapper,
        ILogger<AuthService> logger, IOtpSenderFactory otpSenderFactory)
    {
        _authRepository = authRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
        _logger = logger;
        _otpSenderFactory = otpSenderFactory;
    }

    public async Task<LoginReadDto?> LoginAsync(LoginWriteDto dto)
    {
        _logger.LogInformation(AuthServiceLogMessages.LoginRequestReceived);

        if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Username))
        {
            _logger.LogWarning(AuthServiceLogMessages.LoginMissingEmailAndUsername);
            throw new ValidationAppException(AuthServiceCustomMessages.EmailOrUsernameNotFound);
        }

        _logger.LogInformation(AuthServiceLogMessages.FetchingUserFromRepository,
            string.IsNullOrWhiteSpace(dto.Email) ? "username" : "email");

        var user = !string.IsNullOrWhiteSpace(dto.Email)
            ? await _authRepository.GetUserByEmailAsync(dto.Email)
            : await _authRepository.GetUserByUsernameAsync(dto.Username);

        if (user == null)
        {
            _logger.LogWarning(AuthServiceLogMessages.UserNotFound, dto.Email ?? dto.Username);
            throw new NotFoundException(AuthServiceCustomMessages.UserNotFound);
        }

        if (!PasswordHasher.VerifyPassword(user.Password, dto.Password))
        {
            _logger.LogWarning(AuthServiceLogMessages.InvalidPasswordAttempt, user.UserId);
            throw new ValidationAppException(AuthServiceCustomMessages.InvalidPassword);
        }

        _logger.LogInformation(AuthServiceLogMessages.PasswordVerified, user.UserId);

        var token = _jwtTokenGenerator.GenerateToken(user.Email, user.Role);

        _logger.LogInformation(AuthServiceLogMessages.TokenGenerated, user.UserId);

        var loginReadDto = new LoginReadDto { Token = token, userId = user.UserId };

        return loginReadDto;
    }

    public async Task<UserReadDto?> RegisterAsync(UserWriteDto userDto)
    {
        _logger.LogInformation(AuthServiceLogMessages.RegistrationRequestReceived, userDto.Email);

        await _authRepository.DeleteExpiredUnconfirmedUsersAsync();
        _logger.LogInformation(AuthServiceLogMessages.DeletedExpiredUnconfirmedUsers);

        var isEmailExist = await _authRepository.EmailExistsAsync(userDto.Email);
        if (isEmailExist)
        {
            _logger.LogWarning(AuthServiceLogMessages.EmailAlreadyExists, userDto.Email);
            throw new Exception(AuthServiceCustomMessages.DuplicatedEmail);
        }

        var user = _mapper.Map<User>(userDto);
        _logger.LogInformation(AuthServiceLogMessages.UserRegistered, userDto.Email);
        await _authRepository.RegisterUserAsync(user);

        var otpCode = OtpGenerator.GenerateOtp();
        var otpRecord = new OtpRecord
        {
            UserId = user.UserId,
            Email = user.Email,
            Code = otpCode,
            Expiration = DateTime.UtcNow.AddMinutes(Constants.OtpExpirationMinutes).ToUniversalTime()
        };

        await _authRepository.SaveOtpAsync(otpRecord);
        _logger.LogInformation(AuthServiceLogMessages.OtpSaved, user.Email);

        var strategy = _otpSenderFactory.Factory(OtpChannel.Email);
        if (strategy == null)
            throw new ValidationAppException(AuthServiceCustomMessages.InvalidStrategy);

        await strategy.SendOtpAsync(user.Email, otpRecord.Code);
        _logger.LogInformation(AuthServiceLogMessages.OtpSent, userDto.Email);

        var userReadDto = _mapper.Map<UserReadDto>(user);

        _logger.LogInformation(AuthServiceLogMessages.RegistrationCompleted, user.Email);
        return userReadDto;
    }

    public async Task<bool> SendOtpAsync(string email)
    {
        _logger.LogInformation(AuthServiceLogMessages.SendOtpRequestReceived, email);

        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning(AuthServiceLogMessages.UserNotFoundForOtp, email);
            throw new Exception(AuthServiceCustomMessages.UserNotFound);
        }

        var otp = OtpGenerator.GenerateOtp();
        var otpRecord = new OtpRecord
        {
            UserId = user.UserId,
            Email = email,
            Code = otp,
            Expiration = DateTime.UtcNow.AddMinutes(Constants.OtpExpirationMinutes).ToUniversalTime()
        };

        await _authRepository.SaveOtpAsync(otpRecord);
        _logger.LogInformation(AuthServiceLogMessages.OtpSaved, email);

        var strategy = _otpSenderFactory.Factory(OtpChannel.Email);
        if (strategy == null)
            throw new ValidationAppException(AuthServiceCustomMessages.InvalidStrategy);

        await strategy.SendOtpAsync(email, otpRecord.Code);
        _logger.LogInformation(AuthServiceLogMessages.OtpSent, email);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordReadDto readDto)
    {
        _logger.LogInformation(AuthServiceLogMessages.ResetPasswordRequestReceived, readDto.Email);

        var record = await _authRepository.GetOtpRecordAsync(readDto.Email, readDto.Otp);
        if (record == null)
        {
            _logger.LogWarning(AuthServiceLogMessages.InvalidOrExpiredOtpAttempt, readDto.Email);
            throw new Exception(AuthServiceCustomMessages.InvalidOrExpiredOtpCode);
        }

        if (record.Expiration.ToUniversalTime() < DateTime.UtcNow.ToUniversalTime())
        {
            _logger.LogWarning(AuthServiceLogMessages.ExpiredOtpCodeUsed, readDto.Email);
            throw new Exception(AuthServiceCustomMessages.ExpiredOtpCode);
        }

        var user = await _authRepository.GetUserByEmailAsync(readDto.Email);
        if (user == null)
        {
            _logger.LogWarning(AuthServiceLogMessages.UserNotFoundForReset, readDto.Email);
            throw new Exception(AuthServiceCustomMessages.UserNotFound);
        }

        await _authRepository.HashAndSavePasswordAsync(user, readDto.NewPassword);
        _logger.LogInformation(AuthServiceLogMessages.PasswordResetSuccessfully, readDto.Email);

        await _authRepository.RemoveAndSaveOtpAsync(record);
        _logger.LogInformation(AuthServiceLogMessages.OtpRemovedAfterReset, readDto.Email);

        return true;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otpCode)
    {
        _logger.LogInformation(AuthServiceLogMessages.VerifyOtpRequestReceived, email);

        var otpRecord = await _authRepository.GetOtpRecordAsync(email, otpCode);
        if (otpRecord == null)
        {
            _logger.LogWarning(AuthServiceLogMessages.OtpNotFound, email);
            throw new Exception(AuthServiceCustomMessages.OtpNotFound);
        }

        if (otpRecord.Expiration.ToUniversalTime() < DateTime.UtcNow.ToUniversalTime())
        {
            _logger.LogWarning(AuthServiceLogMessages.ExpiredOtpCodeUsed, email);
            throw new Exception(AuthServiceCustomMessages.ExpiredOtpCode);
        }

        var user = await _authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning(AuthServiceLogMessages.UserNotFoundForOtpVerification, email);
            throw new Exception(AuthServiceCustomMessages.UserNotFound);
        }

        user.IsEmailConfirmed = true;

        await _authRepository.UpdateUserAsync(user);
        _logger.LogInformation(AuthServiceLogMessages.UserEmailConfirmed, email);

        await _authRepository.InvalidateOtpAsync(otpRecord);
        _logger.LogInformation(AuthServiceLogMessages.OtpInvalidated, email);

        return true;
    }
}