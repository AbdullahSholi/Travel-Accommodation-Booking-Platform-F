namespace Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;

public static class RoomServiceLogMessages
{
    public const string CreateRoomRequestReceived = "Create room request received";
    public const string InvalidRoomDataReceived = "Invalid room data received";
    public const string CorrectRoomInformationSent = "Correct room information sent";

    public const string FetchingRoomsFromRepository = "Fetching rooms from repository";
    public const string FetchedRoomsFromRepositorySuccessfully = "Fetching rooms from repository successfully";

    public const string GetRoomRequestReceived = "Get room request received for room: {RoomId}";
    public const string FailedGetRoomFromRepository = "Failed to get room from repository for room: {RoomId}";

    public const string FetchedRoomFromRepositorySuccessfully =
        "Fetched room from repository successfully for room: {RoomId}";

    public const string UpdateRoomRequestReceived = "Update room request received for room: {RoomId}";
    public const string RetrieveRoomSuccessfullyFromRepository = "Retrieve room with id: {RoomId} successfully";

    public const string DeleteRoomRequestReceived = "Delete room request received for room: {RoomId}";
    public const string RoomDeletedSuccessfully = "Room deleted successfully for room: {RoomId}";
}