# Testing Documentation

## 🧪 Testing Overview

The Travel Accommodation Booking Platform implements a comprehensive testing strategy with multiple layers of testing to ensure code quality, reliability, and maintainability. This document covers testing approaches, frameworks, and best practices.

## 🏗️ Testing Architecture

### Testing Pyramid

```
                    ┌─────────────────┐
                    │   E2E Tests     │ ← Few, High-level
                    │   (Manual)      │
                ┌───┴─────────────────┴───┐
                │  Integration Tests      │ ← Some, API-level
                │     (TestContainers)    │
            ┌───┴─────────────────────────┴───┐
            │        Unit Tests               │ ← Many, Fast
            │      (xUnit, Moq)               │
            └─────────────────────────────────┘
```

### Test Projects Structure

```
├── Travel-Accommodation-Booking-Platform-F.UnitTests/
│   ├── Application/
│   │   ├── Services/
│   │   └── Mapping/
│   ├── Domain/
│   │   └── Entities/
│   └── Infrastructure/
│       └── Repositories/
└── Travel-Accommodation-Booking-Platform-F.IntegrationTests/
    ├── Controllers/
    ├── Repositories/
    └── TestBase/
```

## 🔬 Unit Testing

### Framework & Tools
- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Readable assertions
- **AutoFixture**: Test data generation

### Unit Test Structure

#### Service Layer Testing
```csharp
public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _mockAuthRepository;
    private readonly Mock<IJwtTokenGenerator> _mockTokenGenerator;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockAuthRepository = new Mock<IAuthRepository>();
        _mockTokenGenerator = new Mock<IJwtTokenGenerator>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        
        _authService = new AuthService(
            _mockAuthRepository.Object,
            _mockTokenGenerator.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginDto()
    {
        // Arrange
        var loginDto = new LoginWriteDto 
        { 
            Email = "test@example.com", 
            Password = "password123" 
        };
        
        var user = new User 
        { 
            UserId = 1, 
            Email = "test@example.com", 
            Password = PasswordHasher.HashPassword("password123"),
            Role = "User"
        };
        
        var expectedToken = "jwt-token";

        _mockAuthRepository
            .Setup(r => r.GetUserByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
            
        _mockTokenGenerator
            .Setup(t => t.GenerateToken(user.Email, user.Role))
            .Returns(expectedToken);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.userId.Should().Be(user.UserId);
        
        _mockAuthRepository.Verify(r => r.GetUserByEmailAsync(loginDto.Email), Times.Once);
        _mockTokenGenerator.Verify(t => t.GenerateToken(user.Email, user.Role), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsNotFoundException()
    {
        // Arrange
        var loginDto = new LoginWriteDto 
        { 
            Email = "nonexistent@example.com", 
            Password = "password123" 
        };

        _mockAuthRepository
            .Setup(r => r.GetUserByEmailAsync(loginDto.Email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(loginDto))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task LoginAsync_WithInvalidEmail_ThrowsValidationException(string email)
    {
        // Arrange
        var loginDto = new LoginWriteDto 
        { 
            Email = email, 
            Password = "password123" 
        };

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(loginDto))
            .Should().ThrowAsync<ValidationAppException>();
    }
}
```

#### Repository Layer Testing
```csharp
public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Password = "hashedpassword",
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task CreateAsync_WithValidUser_ReturnsCreatedUser()
    {
        // Arrange
        var user = new User
        {
            Email = "new@example.com",
            Username = "newuser",
            FirstName = "New",
            LastName = "User",
            Password = "hashedpassword",
            Role = "User"
        };

        // Act
        var result = await _userRepository.CreateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().BeGreaterThan(0);
        
        var savedUser = await _context.Users.FindAsync(result.UserId);
        savedUser.Should().NotBeNull();
        savedUser.Email.Should().Be("new@example.com");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Test Data Generation

#### Using AutoFixture
```csharp
public class BookingServiceTests
{
    private readonly IFixture _fixture;

    public BookingServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateBookingAsync_WithValidData_ReturnsBooking()
    {
        // Arrange
        var bookingDto = _fixture.Create<BookingWriteDto>();
        var expectedBooking = _fixture.Create<Booking>();

        // Setup mocks and test...
    }
}
```

## 🔗 Integration Testing

### TestContainers Setup

Integration tests use TestContainers for real database testing:

```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext Context;

    public IntegrationTestBase()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithPassword("Test123!")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add test database
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(_msSqlContainer.GetConnectionString()));
                });
            });

        Client = factory.CreateClient();
        Context = factory.Services.GetRequiredService<ApplicationDbContext>();
        
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        Client?.Dispose();
        Context?.Dispose();
    }
}
```

### Controller Integration Tests

```csharp
public class AuthControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = PasswordHasher.HashPassword("password123"),
            FirstName = "Test",
            LastName = "User",
            Role = "User"
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var loginRequest = new LoginWriteDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(content);
        
        result.Should().NotBeNull();
        // Additional assertions for token structure
    }

    [Fact]
    public async Task Register_WithValidData_CreatesUser()
    {
        // Arrange
        var registerRequest = new UserWriteDto
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "password123",
            ConfirmPassword = "password123",
            FirstName = "New",
            LastName = "User",
            PhoneNumber = "+1234567890",
            DateOfBirth = DateTime.Now.AddYears(-25),
            Address1 = "123 Main St",
            City = "Test City",
            Country = "Test Country",
            DriverLicense = "DL123456"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userInDb = await Context.Users
            .FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        
        userInDb.Should().NotBeNull();
        userInDb.Username.Should().Be("newuser");
    }
}
```

### Repository Integration Tests

```csharp
public class BookingRepositoryIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetBookingsByUserIdAsync_WithExistingBookings_ReturnsBookings()
    {
        // Arrange
        var user = new User { /* user data */ };
        var hotel = new Hotel { /* hotel data */ };
        var room = new Room { /* room data */ };
        
        Context.Users.Add(user);
        Context.Hotels.Add(hotel);
        Context.Rooms.Add(room);
        await Context.SaveChangesAsync();

        var booking1 = new Booking
        {
            UserId = user.UserId,
            RoomId = room.RoomId,
            CheckInDate = DateTime.Now.AddDays(1),
            CheckOutDate = DateTime.Now.AddDays(3),
            TotalPrice = 200m
        };

        var booking2 = new Booking
        {
            UserId = user.UserId,
            RoomId = room.RoomId,
            CheckInDate = DateTime.Now.AddDays(5),
            CheckOutDate = DateTime.Now.AddDays(7),
            TotalPrice = 300m
        };

        Context.Bookings.AddRange(booking1, booking2);
        await Context.SaveChangesAsync();

        var repository = new BookingRepository(Context);

        // Act
        var result = await repository.GetBookingsByUserIdAsync(user.UserId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.UserId == user.UserId);
    }
}
```

## 🎯 Test Categories & Attributes

### Test Organization

```csharp
[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
public class AuthServiceTests { }

[Trait("Category", "Integration")]
[Trait("Layer", "Controller")]
public class AuthControllerIntegrationTests { }

[Trait("Category", "Performance")]
public class PerformanceTests { }
```

### Custom Test Attributes

```csharp
public class UnitTestAttribute : FactAttribute
{
    public UnitTestAttribute()
    {
        Traits.Add("Category", "Unit");
    }
}

public class IntegrationTestAttribute : FactAttribute
{
    public IntegrationTestAttribute()
    {
        Traits.Add("Category", "Integration");
    }
}
```

## 🚀 Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in parallel
dotnet test --parallel

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Run tests with detailed output
dotnet test --verbosity detailed
```

### Visual Studio

1. **Test Explorer**: View → Test Explorer
2. **Run All Tests**: Ctrl+R, A
3. **Debug Tests**: Right-click → Debug
4. **Live Unit Testing**: Test → Live Unit Testing → Start

### Test Configuration

#### xunit.runner.json
```json
{
  "methodDisplay": "method",
  "methodDisplayOptions": "all",
  "preEnumerateTheories": false,
  "diagnosticMessages": true,
  "internalDiagnosticMessages": false
}
```

## 📊 Code Coverage

### Coverage Tools
- **Coverlet**: Cross-platform code coverage
- **ReportGenerator**: Coverage report generation

### Coverage Commands

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate HTML report
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html

# Coverage with threshold
dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Threshold=80
```

### Coverage Targets
- **Minimum**: 70% overall coverage
- **Target**: 85% overall coverage
- **Critical Paths**: 95% coverage (authentication, payment)

## 🔧 Test Best Practices

### Naming Conventions
- **Method**: `MethodName_Scenario_ExpectedResult`
- **Class**: `ClassUnderTest + Tests`
- **Variables**: Descriptive names

### AAA Pattern
```csharp
[Fact]
public async Task Method_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and mocks
    var input = new InputDto { /* data */ };
    _mockRepository.Setup(/* mock setup */);

    // Act - Execute the method under test
    var result = await _service.MethodAsync(input);

    // Assert - Verify the results
    result.Should().NotBeNull();
    result.Property.Should().Be(expectedValue);
}
```

### Test Data Management
- **Use builders** for complex object creation
- **Avoid magic numbers** - use constants
- **Isolate test data** - each test should be independent

### Mock Guidelines
- **Mock external dependencies** only
- **Verify important interactions**
- **Use strict mocks** for critical paths

---

**Continue to**: [Monitoring & Logging](09-monitoring-logging.md) for operational insights.
