# Development Guide

## 🚀 Getting Started

This guide will help you set up the development environment and start contributing to the Travel Accommodation Booking Platform.

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** - Local instance or SQL Server Express
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Git** - For version control
- **Docker** (optional) - For containerized development

## 🛠️ Development Environment Setup

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/Travel-Accommodation-Booking-Platform-F.git
cd Travel-Accommodation-Booking-Platform-F
```

### 2. Database Setup

#### Option A: Local SQL Server
1. Install SQL Server or SQL Server Express
2. Create a new database named `TravelAccommodationPlatformDb`
3. set the connectionString at Environment Variables
  setx TRAVEL_ACCOMMODATION_CONNECTION_STRING ""

#### Option B: Docker SQL Server at AWS EC2 Server
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```
  also you need to update the host name or ip address at connection string

### 3. Install Dependencies

Navigate to the API project and restore packages:

```bash
cd Travel-Accommodation-Booking-Platform-F.API
dotnet restore
```

### 4. Apply Database Migrations

```bash
dotnet ef database update
```

### 5. Configure Application Settings

Create `appsettings.json` in the API project:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Your-Connection-String-Here"
  },
  "Jwt": {
    "Key": "",
    "Issuer": "http://localhost:5000",
    "Audience": "http://localhost:5000",
    "ExpiresInMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Travel Accommodation System",
    "SenderEmail": "your-email@gmail.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```
* set JWT Key at Environment Variables by
    setx SECRET_KEY ""
  
### 6. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`

## 🏗️ Project Structure

### Solution Organization

```
Travel-Accommodation-Booking-Platform-F/
├── Travel-Accommodation-Booking-Platform-F.API/          # Presentation Layer
│   ├── Controllers/                                      # API Controllers
│   ├── Extensions/                                       # Service Extensions
│   ├── Utils/                                           # API Utilities
│   └── Program.cs                                       # Application Entry Point
├── Travel-Accommodation-Booking-Platform-F.Application/ # Application Layer
│   ├── DTOs/                                           # Data Transfer Objects
│   │   ├── ReadDTOs/                                   # Response DTOs
│   │   └── WriteDTOs/                                  # Request DTOs
│   ├── Services/                                       # Business Logic Services
│   ├── Mapping/                                        # AutoMapper Profiles
│   └── Utils/                                          # Application Utilities
├── Travel-Accommodation-Booking-Platform-F.Domain/     # Domain Layer
│   ├── Entities/                                       # Domain Entities
│   ├── Interfaces/                                     # Domain Interfaces
│   ├── Enums/                                         # Domain Enumerations
│   ├── CustomExceptions/                              # Domain Exceptions
│   └── QueryDTOs/                                     # Query Objects
├── Travel-Accommodation-Booking-Platform-F.Infrastructure/ # Infrastructure Layer
│   ├── Repositories/                                   # Data Access
│   ├── Persistence/                                    # Database Context
│   ├── ExternalServices/                              # External Integrations
│   └── Migrations/                                    # Database Migrations
├── Travel-Accommodation-Booking-Platform-F.UnitTests/     # Unit Tests
└── Travel-Accommodation-Booking-Platform-F.IntegrationTests/ # Integration Tests
```

### Layer Dependencies

```
API Layer → Application Layer → Domain Layer
                ↓
Infrastructure Layer → Domain Layer
```

## 📝 Coding Standards

### C# Coding Conventions

#### Naming Conventions
- **Classes**: PascalCase (`UserService`, `BookingController`)
- **Methods**: PascalCase (`GetUserAsync`, `CreateBooking`)
- **Properties**: PascalCase (`UserId`, `FirstName`)
- **Fields**: camelCase with underscore prefix (`_logger`, `_context`)
- **Parameters**: camelCase (`userId`, `bookingDto`)
- **Constants**: PascalCase (`OtpExpirationMinutes`)

#### Code Organization
```csharp
// 1. Using statements (grouped and sorted)
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.Services;

// 2. Namespace
namespace Travel_Accommodation_Booking_Platform_F.Controllers;

// 3. Class with proper access modifiers
public class BookingsController : ControllerBase
{
    // 4. Private fields
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    // 5. Constructor
    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    // 6. Public methods
    [HttpGet]
    public async Task<IActionResult> GetBookings()
    {
        // Implementation
    }

    // 7. Private methods
    private void ValidateBooking(BookingDto booking)
    {
        // Implementation
    }
}
```

#### Async/Await Guidelines
- Always use `async`/`await` for I/O operations
- Append `Async` suffix to async method names

```csharp
public async Task<UserReadDto?> GetUserAsync(int userId)
{
    var user = await _userRepository.GetByIdAsync(userId);
    return _mapper.Map<UserReadDto>(user);
}
```

### Error Handling

#### Custom Exceptions
```csharp
public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(int userId) 
        : base($"User with ID {userId} was not found.")
    {
    }
}
```

#### Controller Error Handling
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    try
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }
    catch (UserNotFoundException ex)
    {
        _logger.LogWarning(ex, "User not found: {UserId}", id);
        return NotFound(new { Message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving user: {UserId}", id);
        return StatusCode(500, new { Message = "Internal server error" });
    }
}
```

### Logging Guidelines

#### Structured Logging with Serilog
```csharp
// Information logging
_logger.LogInformation("User {UserId} created booking {BookingId}", userId, bookingId);

// Warning logging
_logger.LogWarning("Invalid booking attempt for user {UserId}", userId);

// Error logging
_logger.LogError(ex, "Failed to process booking {BookingId}", bookingId);
```

#### Log Levels
- **Debug**: Detailed information for debugging
- **Information**: General application flow
- **Warning**: Unexpected situations that don't stop the application
- **Error**: Error events that might still allow the application to continue

## 🧪 Testing Guidelines

### Unit Testing with xUnit

#### Test Structure
```csharp
[Fact]
public async Task GetUserAsync_WithValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { UserId = userId, Email = "test@example.com" };
    _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

    // Act
    var result = await _userService.GetUserAsync(userId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(expectedUser.Email, result.Email);
}
```

#### Test Naming Convention
- `MethodName_Scenario_ExpectedResult`
- Example: `CreateBooking_WithValidData_ReturnsBookingId`

### Integration Testing

#### Test Database Setup
```csharp
public class IntegrationTestBase : IDisposable
{
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext Context;

    public IntegrationTestBase()
    {
        var factory = new WebApplicationFactory<Program>();
        Client = factory.CreateClient();
        Context = factory.Services.GetRequiredService<ApplicationDbContext>();
    }
}
```

### Useful Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Watch for changes and rebuild
dotnet watch run

# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Generate code coverage report
dotnet test --collect:"XPlat Code Coverage"
```

---

**Continue to**: [Deployment Guide](06-deployment.md) for production deployment.
