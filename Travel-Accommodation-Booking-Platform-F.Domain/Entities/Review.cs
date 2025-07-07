﻿namespace Travel_Accommodation_Booking_Platform_F.Domain.Entities;

public class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }

    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}