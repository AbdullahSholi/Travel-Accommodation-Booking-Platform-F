using AutoMapper;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.RoomExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RoomService> _logger;

    public RoomService(IRoomRepository roomRepository, IMapper mapper, ILogger<RoomService> logger)
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RoomReadDto?> CreateRoomAsync(RoomWriteDto dto)
    {
        _logger.LogInformation(RoomServiceLogMessages.CreateRoomRequestReceived);

        if (dto == null)
        {
            _logger.LogWarning(RoomServiceLogMessages.InvalidRoomDataReceived);
            throw new InvalidRoomDataReceivedException(RoomServiceCustomMessages.InvalidRoomDataReceived);
        }

        _logger.LogInformation(RoomServiceLogMessages.CorrectRoomInformationSent);

        var room = _mapper.Map<Room>(dto);

        await _roomRepository.AddAsync(room);

        var roomReadDto = _mapper.Map<RoomReadDto>(room);
        return roomReadDto;
    }

    public async Task<List<RoomReadDto>?> GetRoomsAsync()
    {
        _logger.LogInformation(RoomServiceLogMessages.FetchingRoomsFromRepository);

        var rooms = await _roomRepository.GetAllAsync();
        if (rooms == null)
        {
            _logger.LogWarning(RoomServiceCustomMessages.FailedFetchingRoomsFromRepository);
            throw new FailedToFetchRoomsException(RoomServiceCustomMessages.FailedFetchingRoomsFromRepository);
        }

        _logger.LogInformation(RoomServiceLogMessages.FetchedRoomsFromRepositorySuccessfully);

        var roomsReadDto = _mapper.Map<List<RoomReadDto>>(rooms);
        return roomsReadDto;
    }

    public async Task<RoomReadDto?> GetRoomAsync(int roomId)
    {
        _logger.LogInformation(RoomServiceLogMessages.GetRoomRequestReceived, roomId);

        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null) return null;

        _logger.LogInformation(RoomServiceLogMessages.FetchedRoomFromRepositorySuccessfully, roomId);

        var roomReadDto = _mapper.Map<RoomReadDto>(room);
        return roomReadDto;
    }

    public async Task<RoomReadDto?> UpdateRoomAsync(int roomId, RoomPatchDto dto)
    {
        _logger.LogInformation(RoomServiceLogMessages.UpdateRoomRequestReceived, roomId);

        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null) return null;

        _logger.LogInformation(RoomServiceLogMessages.RetrieveRoomSuccessfullyFromRepository, roomId);

        room.RoomType = dto.RoomType ?? room.RoomType;
        room.Description = dto.Description ?? room.Description;
        room.PricePerNight = dto.PricePerNight ?? room.PricePerNight;
        room.IsAvailable = dto.IsAvailable ?? room.IsAvailable;
        room.AdultCapacity = dto.AdultCapacity ?? room.AdultCapacity;
        room.ChildrenCapacity = dto.ChildrenCapacity ?? room.ChildrenCapacity;
        room.UpdatedAt = DateTime.UtcNow;
        room.LastUpdated = DateTime.UtcNow;

        await _roomRepository.UpdateAsync(room);

        var roomReadDto = _mapper.Map<RoomReadDto>(room);
        return roomReadDto;
    }

    public async Task DeleteRoomAsync(int roomId)
    {
        _logger.LogInformation(RoomServiceLogMessages.DeleteRoomRequestReceived, roomId);

        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null) return;
        _logger.LogInformation(RoomServiceLogMessages.RetrieveRoomSuccessfullyFromRepository, roomId);


        await _roomRepository.DeleteAsync(room);
        _logger.LogInformation(RoomServiceLogMessages.RoomDeletedSuccessfully, roomId);
    }
}