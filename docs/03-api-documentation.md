# API Documentation

## 🌐 API Overview

The Travel Accommodation Booking Platform provides a comprehensive RESTful API built with ASP.NET Core. The API follows REST principles and includes features like JWT authentication, rate limiting, caching, and comprehensive error handling.

### Base URL
- **Development**: `http://localhost:5000`
- **Production**: `https://your-domain.com`

### API Versioning
The API supports versioning through URL path:
- Current version: `v1`
- Example: `/api/v1/auth/login`

## 🔐 Authentication

### Authentication Method
The API uses **JWT (JSON Web Token)** for authentication with Bearer token scheme.

#### Headers
```http
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

### Token Lifecycle
- **Expiration**: 60 minutes (configurable)
- **Refresh**: Not implemented (tokens must be renewed via login)
- **Blacklisting**: Supported for secure logout

## 📋 API Endpoints

### 🔑 Authentication Endpoints

#### POST /api/auth/login
Authenticate user and receive JWT token.

**Request Body:**
```json
{
  "email": "user@example.com",     // Optional if username provided
  "username": "johndoe",           // Optional if email provided
  "password": "securePassword123"  // Required
}
```

**Response (200 OK):**
```json
{
  "result": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": 123
  }
}
```

**Error Responses:**
- `400 Bad Request`: Invalid credentials
- `404 Not Found`: User not found

#### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "username": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "securePassword123",
  "confirmPassword": "securePassword123",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "1990-01-01",
  "address1": "123 Main St",
  "address2": "Apt 4B",
  "city": "New York",
  "country": "USA",
  "driverLicense": "DL123456789",
  "role": "User"
}
```

**Response (200 OK):**
```json
{
  "user": {
    "userId": 123,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "username": "johndoe",
    "role": "User",
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
}
```

#### POST /api/auth/forgot-password
**Authorization Required**: User or Admin

Send OTP for password reset.

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

#### POST /api/auth/verify-otp
**Authorization Required**: User or Admin

Verify OTP code for password reset.

**Request Body:**
```json
{
  "email": "user@example.com",
  "otpCode": "123456"
}
```

#### POST /api/auth/reset-password
**Authorization Required**: User or Admin

Reset password using verified OTP.

**Request Body:**
```json
{
  "email": "user@example.com",
  "otpCode": "123456",
  "newPassword": "newSecurePassword123"
}
```

#### POST /api/auth/logout
**Authorization Required**: User or Admin

Logout and blacklist current token.

### 🏨 Hotel Management

#### GET /api/hotels
Get all hotels with caching support.

**Headers:**
- `If-None-Match`: ETag for cache validation

**Response (200 OK):**
```json
[
  {
    "hotelId": 1,
    "hotelName": "Grand Hotel",
    "starRating": 4.5,
    "location": "Downtown",
    "description": "Luxury hotel in city center",
    "cityId": 1,
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

**Response Headers:**
- `ETag`: Cache validation tag

#### GET /api/hotels/{id}
**Authorization Required**: User or Admin

Get hotel by ID.

**Response (200 OK):**
```json
{
  "hotelId": 1,
  "hotelName": "Grand Hotel",
  "starRating": 4.5,
  "location": "Downtown",
  "description": "Luxury hotel in city center",
  "cityId": 1,
  "lastUpdated": "2025-01-01T00:00:00Z"
}
```

#### POST /api/hotels
**Authorization Required**: Admin

Create a new hotel.

**Request Body:**
```json
{
  "hotelName": "New Hotel",
  "ownerName": "Hotel Owner",
  "starRating": 4.0,
  "location": "City Center",
  "description": "Beautiful hotel description",
  "cityId": 1
}
```

#### PATCH /api/hotels/{id}
**Authorization Required**: Admin

Update hotel information.

**Request Body:**
```json
{
  "hotelName": "Updated Hotel Name",
  "starRating": 4.5,
  "description": "Updated description"
}
```

#### DELETE /api/hotels/{id}
**Authorization Required**: Admin

Delete a hotel.

### 🏠 Room Management

#### GET /api/rooms
Get all rooms with caching support.

**Response (200 OK):**
```json
[
  {
    "roomId": 1,
    "roomType": "Standard",
    "images": ["image1.jpg", "image2.jpg"],
    "description": "Comfortable standard room",
    "pricePerNight": 100.00,
    "isAvailable": true,
    "adultCapacity": 2,
    "childrenCapacity": 1,
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": "2025-01-01T00:00:00Z",
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

#### GET /api/rooms/{id}
**Authorization Required**: User or Admin

Get room by ID.

#### POST /api/rooms
**Authorization Required**: Admin

Create a new room.

**Request Body:**
```json
{
  "roomType": "Standard",
  "images": ["image1.jpg", "image2.jpg"],
  "description": "Room description",
  "pricePerNight": 100.00,
  "isAvailable": true,
  "adultCapacity": 2,
  "childrenCapacity": 1,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-01T00:00:00Z",
  "hotelId": 1
}
```

#### PATCH /api/rooms/{id}
**Authorization Required**: Admin

Update room information.

#### DELETE /api/rooms/{id}
**Authorization Required**: Admin

Delete a room.

### 📅 Booking Management

#### GET /api/bookings
**Authorization Required**: User or Admin

Get all bookings (filtered by user role).

**Response (200 OK):**
```json
[
  {
    "bookingId": 1,
    "userId": 123,
    "roomId": 1,
    "checkInDate": "2025-02-01T00:00:00Z",
    "checkOutDate": "2025-02-05T00:00:00Z",
    "createdAt": "2025-01-01T00:00:00Z",
    "totalPrice": 400.00,
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

#### GET /api/bookings/{id}
**Authorization Required**: User or Admin

Get booking by ID.

#### POST /api/bookings
**Authorization Required**: User or Admin

Create a new booking.

**Request Body:**
```json
{
  "userId": 123,
  "roomId": 1,
  "checkInDate": "2025-02-01T00:00:00Z",
  "checkOutDate": "2025-02-05T00:00:00Z",
  "createdAt": "2025-01-01T00:00:00Z",
  "totalPrice": 400.00,
  "lastUpdated": "2025-01-01T00:00:00Z"
}
```

#### PATCH /api/bookings/{id}
**Authorization Required**: Admin

Update booking information.

**Request Body:**
```json
{
  "roomId": 2,
  "checkInDate": "2025-02-02T00:00:00Z",
  "checkOutDate": "2025-02-06T00:00:00Z",
  "totalPrice": 450.00
}
```

#### DELETE /api/bookings/{id}
**Authorization Required**: Admin

Cancel/delete a booking.

### 🌆 City Management

#### GET /api/cities
Get all cities with caching support.

**Response (200 OK):**
```json
[
  {
    "cityId": 1,
    "name": "New York",
    "country": "USA",
    "postOffice": "10001",
    "numberOfHotels": 25,
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": "2025-01-01T00:00:00Z",
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

#### GET /api/cities/{id}
**Authorization Required**: User or Admin

Get city by ID.

#### POST /api/cities
**Authorization Required**: Admin

Create a new city.

**Request Body:**
```json
{
  "name": "Los Angeles",
  "country": "USA",
  "postOffice": "90001",
  "numberOfHotels": 15,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-01T00:00:00Z"
}
```

#### PATCH /api/cities/{id}
**Authorization Required**: Admin

Update city information.

#### DELETE /api/cities/{id}
**Authorization Required**: Admin

Delete a city.

### ⭐ Review Management

#### GET /api/reviews
Get all reviews with caching support.

**Response (200 OK):**
```json
[
  {
    "reviewId": 1,
    "userId": 123,
    "hotelId": 1,
    "rating": 5,
    "comment": "Excellent hotel with great service!",
    "createdAt": "2025-01-01T00:00:00Z",
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

#### GET /api/reviews/{id}
**Authorization Required**: User or Admin

Get review by ID.

#### POST /api/reviews
**Authorization Required**: User or Admin

Create a new review.

**Request Body:**
```json
{
  "userId": 123,
  "hotelId": 1,
  "rating": 5,
  "comment": "Amazing experience at this hotel!",
  "createdAt": "2025-01-01T00:00:00Z",
  "lastUpdated": "2025-01-01T00:00:00Z"
}
```

#### PATCH /api/reviews/{id}
**Authorization Required**: Admin

Update review information.

#### DELETE /api/reviews/{id}
**Authorization Required**: Admin

Delete a review.

### 👥 Admin Management

#### POST /api/admin/users
**Authorization Required**: Admin

Create a new user (admin function).

**Request Body:** Same as `/api/auth/register`

#### GET /api/admin/users
**Authorization Required**: Admin

Get all users in the system.

**Response (200 OK):**
```json
[
  {
    "userId": 123,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "username": "johndoe",
    "role": "User",
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

#### GET /api/admin/users/{id}
**Authorization Required**: Admin

Get user by ID.

#### PATCH /api/admin/users/{id}
**Authorization Required**: Admin

Update user information.

**Request Body:**
```json
{
  "firstName": "Updated Name",
  "email": "newemail@example.com",
  "role": "Admin"
}
```

#### DELETE /api/admin/users/{id}
**Authorization Required**: Admin

Delete a user.

#### GET /api/admin/top-visited-cities
**Authorization Required**: Admin

Get analytics of most visited cities.

**Response (200 OK):**
```json
[
  {
    "cityId": 1,
    "name": "New York",
    "country": "USA",
    "visitCount": 150,
    "lastUpdated": "2025-01-01T00:00:00Z"
  }
]
```

#### GET /api/admin/search
**Authorization Required**: User or Admin

Advanced room search with filters.

**Query Parameters:**
- `roomType`: Room type filter (Standard, Deluxe, Suite)
- `minPrice`: Minimum price per night
- `maxPrice`: Maximum price per night
- `isAvailable`: Availability filter (true/false)
- `adultCapacity`: Minimum adult capacity
- `childrenCapacity`: Minimum children capacity
- `createdAt`: Filter by creation date

**Example:**
```
GET /api/admin/search?roomType=Standard&minPrice=50&maxPrice=200&isAvailable=true&adultCapacity=2
```

**Response (200 OK):**
```json
[
  {
    "roomId": 1,
    "roomType": "Standard",
    "pricePerNight": 100.00,
    "isAvailable": true,
    "adultCapacity": 2,
    "childrenCapacity": 1
  }
]
```

## 🔧 API Features

### Caching & Performance
- **ETag Support**: All GET endpoints support ETag caching
- **Memory Caching**: Frequently accessed data is cached
- **Response Compression**: Automatic response compression

### Rate Limiting
- **Default Limit**: 10 requests per minute per IP
- **Configurable**: Limits can be adjusted per endpoint
- **Headers**: Rate limit information in response headers

### Error Handling
All endpoints return consistent error responses:

```json
{
  "message": "Error description",
  "details": "Additional error details (in development)"
}
```

### HTTP Status Codes
- `200 OK`: Successful GET/PATCH requests
- `201 Created`: Successful POST requests
- `204 No Content`: Successful DELETE requests
- `304 Not Modified`: Cached content is still valid
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict (e.g., duplicate email)
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error

### Security Headers
All responses include security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: no-referrer`

## 📝 Data Validation

### Request Validation
All write operations include comprehensive validation:
- **Required Fields**: Marked with `[Required]` attribute
- **String Length**: Min/max length validation
- **Email Format**: Valid email address format
- **Date Format**: ISO 8601 date format
- **Range Validation**: Numeric ranges for ratings, prices, etc.

### Response Format
All responses follow consistent JSON structure with camelCase naming convention.

---

**Continue to**: [Database Documentation](04-database.md) for detailed schema information.
