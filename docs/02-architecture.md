# System Architecture

## 🏗️ Architectural Overview

The Travel Accommodation Booking Platform follows **Clean Architecture** principles, ensuring separation of concerns, testability, and maintainability. The system is designed with Domain-Driven Design (DDD) concepts and implements several design patterns for optimal structure.

## 📁 Project Structure

```
Travel-Accommodation-Booking-Platform-F/
├── Travel-Accommodation-Booking-Platform-F.API/          # Presentation Layer
│   ├── Controllers/                                      # API Controllers
│   ├── Extensions/                                       # Service Extensions
│   ├── Utils/                                           # API Utilities
│   └── Program.cs                                       # Application Entry Point
├── Travel-Accommodation-Booking-Platform-F.Application/ # Application Layer
│   ├── DTOs/                                           # Data Transfer Objects
│   ├── Services/                                       # Application Services
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

## 🎯 Clean Architecture Layers

### 1. Presentation Layer (API)
**Responsibility**: Handle HTTP requests, routing, and response formatting

**Components**:
- **Controllers**: RESTful API endpoints
- **Middleware**: Cross-cutting concerns (authentication, logging, error handling)
- **Extensions**: Dependency injection configuration
- **Utilities**: API-specific helper classes

**Key Files**:
- `AuthController.cs` - Authentication endpoints
- `BookingsController.cs` - Booking management
- `HotelsController.cs` - Hotel operations
- `UsersController.cs` - User management

### 2. Application Layer
**Responsibility**: Business logic orchestration and use case implementation

**Components**:
- **Services**: Business logic implementation
- **DTOs**: Data transfer objects for API communication
- **Mapping**: Object-to-object mapping configurations
- **Validators**: Input validation logic

**Key Services**:
- `AuthService` - Authentication and authorization
- `BookingService` - Booking business logic
- `HotelService` - Hotel management
- `UserService` - User operations

### 3. Domain Layer
**Responsibility**: Core business entities and domain logic

**Components**:
- **Entities**: Core business objects
- **Interfaces**: Contracts for repositories and services
- **Enums**: Domain-specific enumerations
- **Exceptions**: Domain-specific exceptions

**Core Entities**:
- `User` - System users (customers and admins)
- `Hotel` - Hotel information and metadata
- `Room` - Room details and availability
- `Booking` - Booking records and status
- `Review` - User reviews and ratings
- `City` - Location information

### 4. Infrastructure Layer
**Responsibility**: External concerns and data persistence

**Components**:
- **Repositories**: Data access implementations
- **Persistence**: Database context and configurations
- **External Services**: Third-party integrations
- **Migrations**: Database schema versioning

## 🎨 Design Patterns Implementation

### 1. Repository Pattern
**Purpose**: Abstract data access logic and provide testable data layer

```csharp
public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetAllAsync();
    Task<Booking> CreateAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(int id);
}
```

**Benefits**:
- Separation of data access from business logic
- Easy unit testing with mock repositories
- Consistent data access patterns

### 2. CQRS (Command Query Responsibility Segregation)
**Purpose**: Separate read and write operations for better scalability

**Implementation**:
- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations (Get, Search, Filter)
- **MediatR**: Mediator pattern for request/response handling

### 3. Observer Pattern
**Purpose**: Implement event-driven notifications

**Implementation**:
```csharp
public interface INotifyUsersObserver
{
    Task NotifyAsync(string email, string subject, string message);
}

public class NotifyUsersEmailObserver : INotifyUsersObserver
{
    // Email notification implementation
}
```

**Use Cases**:
- Booking confirmations
- Password reset notifications
- System alerts

### 4. Factory Pattern
**Purpose**: Create objects without specifying exact classes

**Implementation**:
```csharp
public interface IOtpSenderFactory
{
    IOtpSender CreateOtpSender(string type);
}
```

**Benefits**:
- Flexible object creation
- Easy to extend with new types
- Reduced coupling

### 5. Strategy Pattern
**Purpose**: Define family of algorithms and make them interchangeable

**Use Cases**:
- Different OTP sending strategies (Email, SMS)
- Various payment processing methods
- Multiple search algorithms

## 🔄 Data Flow Architecture

### Request Processing Flow
```
1. HTTP Request → Controller
2. Controller → Application Service
3. Service → Domain Logic
4. Domain → Repository Interface
5. Repository → Database
6. Response ← Controller ← Service ← Repository
```

### Authentication Flow
```
1. Login Request → AuthController
2. AuthController → AuthService
3. AuthService → UserRepository
4. Password Verification → Argon2
5. JWT Token Generation
6. Token Response
```

### Booking Flow
```
1. Booking Request → BookingsController
2. Validation → BookingService
3. Availability Check → RoomRepository
4. Price Calculation → Domain Logic
5. Booking Creation → BookingRepository
6. Notification → Observer Pattern
7. Confirmation Response
```

## 🗄️ Database Architecture

### Entity Relationships
```
User (1) ←→ (N) Booking (N) ←→ (1) Room (N) ←→ (1) Hotel (N) ←→ (1) City
User (1) ←→ (N) Review (N) ←→ (1) Hotel
User (1) ←→ (N) OtpRecord
```

### Key Relationships
- **User to Booking**: One-to-Many (Users can have multiple bookings)
- **Room to Booking**: One-to-Many (Rooms can have multiple bookings)
- **Hotel to Room**: One-to-Many (Hotels have multiple rooms)
- **City to Hotel**: One-to-Many (Cities have multiple hotels)
- **User to Review**: One-to-Many (Users can write multiple reviews)
- **Hotel to Review**: One-to-Many (Hotels can have multiple reviews)

## 🔧 Dependency Injection

### Service Registration
```csharp
// Application Services
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IBookingService, BookingService>();
services.AddScoped<IHotelService, HotelService>();

// Repositories
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<IHotelRepository, HotelRepository>();

// External Services
services.AddScoped<INotifyUsersObserver, NotifyUsersEmailObserver>();
services.AddScoped<IOtpSenderFactory, OtpSenderFactory>();
```

### Configuration Extensions
- `AddApplicationServices()` - Register application layer services
- `AddDatabase()` - Configure Entity Framework
- `AddJwtAuthentication()` - Setup JWT authentication
- `AddRateLimiting()` - Configure API rate limiting
- `AddCaching()` - Setup memory caching

## 🚀 Performance Considerations

### Caching Strategy
- **Memory Caching**: Frequently accessed data (cities, hotels)
- **Response Caching**: API response caching with ETags
- **Query Optimization**: Efficient database queries with proper indexing

### Async/Await Pattern
- All I/O operations use async/await for better scalability
- Non-blocking operations for database access
- Improved thread pool utilization

### Database Optimization
- **Lazy Loading**: Efficient entity loading strategies
- **Projection**: Select only required fields
- **Indexing**: Proper database indexing for search operations

## 🔒 Security Architecture

### Authentication & Authorization
- **JWT Tokens**: Stateless authentication
- **Role-based Access**: User and Admin roles
- **Token Blacklisting**: Secure logout implementation

### Data Protection
- **Input Validation**: Comprehensive validation at all layers
- **SQL Injection Prevention**: Parameterized queries
- **XSS Protection**: Output encoding and validation

### Security Headers
- **HSTS**: HTTP Strict Transport Security
- **CSP**: Content Security Policy
- **X-Frame-Options**: Clickjacking protection

---

**Next**: Explore the [API Documentation](03-api-documentation.md) for detailed endpoint information.
