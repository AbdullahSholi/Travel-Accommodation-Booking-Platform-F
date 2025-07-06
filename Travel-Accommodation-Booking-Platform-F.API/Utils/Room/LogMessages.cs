namespace Travel_Accommodation_Booking_Platform_F.Utils.Room;

public static class LogMessages
{
    public const string CreateRoomRequestReceived = "Create Room request received";
    public const string CreateRoomFailed = "Create Room failed";
    public const string CreateRoomSuccess = "Registered Room success for Room: {RoomId}";
    public const string InvalidRoomDataReceivedFromRequest = "Invalid Room data received from request";

    public const string GetRoomsRequestReceived = "Get Rooms request received";
    public const string GetRoomsFailed = "Get Rooms failed";
    public const string GetRoomsSuccess = "Get Rooms success";

    public const string GetRoomRequestReceived = "Get Room request received for Room with idn: {RoomId}";
    public const string GetRoomFailed = "Get Room failed for Room with id: {RoomId}";
    public const string GetRoomSuccess = "Get Room success for Room with id: {RoomId}";

    public const string UpdateRoomRequestReceived = "Update Room request received for Room with id: {RoomId}";
    public const string UpdateRoomFailed = "Update Room failed for Room with id: {RoomId}";
    public const string RoomUpdatedSuccessfully = "Updated successfully, for Room with id: {RoomId}";

    public const string DeleteRoomRequestReceived = "Delete Room request received for Room with id: {RoomId}";
    public const string DeleteRoomFailed = "Delete Room failed for Room with id: {RoomId}";
}