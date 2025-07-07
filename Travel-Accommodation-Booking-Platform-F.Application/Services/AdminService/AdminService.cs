using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.AdminExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminService> _logger;
    private readonly IMemoryCache _memoryCache;

    private const string UsersCacheKey = "users-list";
    private const string UserCacheKey = "user";

    public AdminService(IAdminRepository adminRepository, IMapper mapper, ILogger<AdminService> logger,
        IMemoryCache memoryCache)
    {
        _adminRepository = adminRepository;
        _mapper = mapper;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<UserReadDto?> CreateUserAsync(UserWriteDto dto)
    {
        _logger.LogInformation(AdminServiceLogMessages.CreateUserRequestReceived);

        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Username))
        {
            _logger.LogWarning(AdminServiceLogMessages.InvalidUserDataReceived);
            throw new InvalidUserDataReceivedException(AdminServiceCustomMessages.InvalidUserDataReceived);
        }

        var isEmailExists = await _adminRepository.EmailExistsAsync(dto.Email);
        if (isEmailExists)
        {
            _logger.LogWarning(AdminServiceLogMessages.EmailAlreadyExists, dto.Email);
            throw new DuplicatedEmailException(AdminServiceCustomMessages.DuplicatedEmails);
        }

        _logger.LogInformation(AdminServiceLogMessages.CorrectUserInformationSent);

        var user = _mapper.Map<User>(dto);
        user.IsEmailConfirmed = true;

        await _adminRepository.AddAsync(user);

        _logger.LogInformation(AdminServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(UsersCacheKey);
        _memoryCache.Remove(UserCacheKey);

        var userReadDto = _mapper.Map<UserReadDto>(user);
        return userReadDto;
    }

    public async Task<List<UserReadDto>?> GetUsersAsync()
    {
        _logger.LogInformation(AdminServiceLogMessages.FetchingUsersFromRepository);

        if (_memoryCache.TryGetValue(UsersCacheKey, out List<UserReadDto> cachedUsers))
        {
            _logger.LogInformation(AdminServiceLogMessages.ReturningUsersFromCache);
            return cachedUsers;
        }

        var users = await _adminRepository.GetAllAsync();
        if (users == null)
        {
            _logger.LogWarning(AdminServiceCustomMessages.FailedFetchingUsersFromRepository);
            throw new FailedToFetchUsersException(AdminServiceCustomMessages.FailedFetchingUsersFromRepository);
        }

        _logger.LogInformation(AdminServiceLogMessages.FetchedUsersFromRepositorySuccessfully);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveUsersMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var usersReadDto = _mapper.Map<List<UserReadDto>>(users);

        _memoryCache.Set(UsersCacheKey, usersReadDto, cacheEntryOptions);
        return usersReadDto;
    }

    public async Task<UserReadDto?> GetUserAsync(int userId)
    {
        _logger.LogInformation(AdminServiceLogMessages.GetUserRequestReceived, userId);

        if (_memoryCache.TryGetValue(UserCacheKey, out UserReadDto cachedUser))
        {
            _logger.LogInformation(AdminServiceLogMessages.ReturningUserFromCache);
            return cachedUser;
        }

        var user = await _adminRepository.GetByIdAsync(userId);
        if (user == null) return null;

        _logger.LogInformation(AdminServiceLogMessages.FetchedUserFromRepositorySuccessfully, userId);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveUsersMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var userReadDto = _mapper.Map<UserReadDto>(user);

        _memoryCache.Set(UserCacheKey, userReadDto, cacheEntryOptions);
        return userReadDto;
    }

    public async Task<UserReadDto?> UpdateUserAsync(int userId, UserPatchDto dto)
    {
        _logger.LogInformation(AdminServiceLogMessages.UpdateUserRequestReceived, userId);

        var user = await _adminRepository.GetByIdAsync(userId);
        if (user == null) return null;

        _logger.LogInformation(AdminServiceLogMessages.RetrieveUserSuccessfullyFromRepository, userId);

        user.Username = dto.Username ?? user.Username;
        user.FirstName = dto.FirstName ?? user.FirstName;
        user.LastName = dto.LastName ?? user.LastName;
        user.Email = dto.Email ?? user.Email;
        user.Address1 = dto.Address1 ?? user.Address1;
        user.Address2 = dto.Address2 ?? user.Address2;
        user.City = dto.City ?? user.City;
        user.Country = dto.Country ?? user.Country;
        user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
        user.DriverLicense = dto.DriverLicense ?? user.DriverLicense;
        user.LastUpdated = DateTime.UtcNow;

        await _adminRepository.UpdateAsync(user);

        _logger.LogInformation(AdminServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(UsersCacheKey);
        _memoryCache.Remove(UserCacheKey);

        var userReadDto = _mapper.Map<UserReadDto>(user);
        return userReadDto;
    }

    public async Task DeleteUserAsync(int userId)
    {
        _logger.LogInformation(AdminServiceLogMessages.DeleteUserRequestReceived, userId);

        var user = await _adminRepository.GetByIdAsync(userId);
        if (user == null) return;
        _logger.LogInformation(AdminServiceLogMessages.RetrieveUserSuccessfullyFromRepository, userId);


        await _adminRepository.DeleteAsync(user);
        _logger.LogInformation(AdminServiceLogMessages.UserDeletedSuccessfully, userId);

        _logger.LogInformation(AdminServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(UsersCacheKey);
        _memoryCache.Remove(UserCacheKey);
    }
}