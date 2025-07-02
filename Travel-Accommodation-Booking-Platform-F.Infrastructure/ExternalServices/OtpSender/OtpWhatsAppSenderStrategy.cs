using System.Text;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.StrategyPattern;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.ExternalServices.OtpSender;

public class OtpWhatsAppSenderStrategy : IOtpSenderStrategy
{
    private const string InstanceId = "instance129637";
    private const string Token = "iiemdat9c8liiorb";
    
    public async Task SendOtpAsync(string to, string otp)
    {
        using var client = new HttpClient();
        var url = $"https://api.ultramsg.com/{InstanceId}/messages/chat";

        var json = $@"{{
            ""token"": ""{Token}"",
            ""to"": ""{to}"",
            ""body"": ""Your OTP is: {otp}""
        }}";

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send OTP: {error}");
        }
    }
}