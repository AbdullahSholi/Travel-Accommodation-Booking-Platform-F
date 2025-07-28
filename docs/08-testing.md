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
- **AutoFixture**: Test data generation
- **AutoMapper**: Object mapping in tests

### Unit Test Structure

#### City Service Unit Test Example

Here's a real example from the City service testing the `GetCitiesAsync` method with in-memory caching:

```csharp
public class CityServiceGetCitiesTests
{
    private readonly Mock<ICityRepository> _mockRepo;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CityService>> _mockLogger;
    private readonly CityService _sut;
    private readonly IFixture _fixture;

    public CityServiceGetCitiesTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockRepo = new Mock<ICityRepository>();
        _mockCache = new Mock<IMemoryCache>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CityService>>();

        _sut = new CityService(_mockRepo.Object, _mockCache.Object,
                              _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    [Trait("UnitTests - City", "GetCities")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var cityName = "Nablus";
        var cachedCities = new List<CityReadDto>
        {
            _fixture.Build<CityReadDto>().With(x => x.Name, cityName).Create(),
            _fixture.Create<CityReadDto>(),
            _fixture.Create<CityReadDto>()
        };

        object cachedObject = cachedCities;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject))
                  .Returns(true);

        // Act
        var result = await _sut.GetCitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedCities[0].Name, result[0].Name);
        Assert.Equal(3, result.Count);

        // Verify cache was checked
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);

        // Verify repository was NOT called (data came from cache)
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    [Trait("UnitTests - City", "GetCities")]
    public async Task Should_ReturnDataFromRepository_When_CacheIsEmpty()
    {
        // Arrange
        var cities = new List<City>
        {
            _fixture.Build<City>()
                .Without(x => x.CityId)
                .Without(x => x.Hotels)
                .With(x => x.Name, "Nablus")
                .Create()
        };

        var citiesReadDto = new List<CityReadDto>
        {
            _fixture.Build<CityReadDto>()
                .With(x => x.Name, "Nablus")
                .Create()
        };

        object cachedObject = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject))
                  .Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cities);
        _mockMapper.Setup(x => x.Map<List<CityReadDto>>(It.IsAny<List<City>>()))
                   .Returns(citiesReadDto);

        // Act
        var result = await _sut.GetCitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cities[0].Name, result[0].Name);

        // Verify cache was checked
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);

        // Verify repository was called
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Once);

        // Verify data was cached
        _mockCache.Verify(x => x.Set(It.IsAny<object>(), It.IsAny<object>(),
                                    It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - City", "GetCities")]
    public async Task Should_ThrowException_When_RepositoryReturnsNull()
    {
        // Arrange
        object cachedObject = null;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject))
                  .Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync((List<City>)null);

        // Act & Assert
        await Assert.ThrowsAsync<FailedToFetchCitiesException>(() => _sut.GetCitiesAsync());

        // Verify repository was called
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Once);

        // Verify cache was not set
        _mockCache.Verify(x => x.Set(It.IsAny<object>(), It.IsAny<object>(),
                                    It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
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

Integration tests use TestContainers for real SQL Server database testing. Here's the actual base class from the project:

```csharp
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;
    protected WebApplicationFactory<Program> Factory;
    protected HttpClient Client;
    protected ApplicationDbContext DbContext { get; private set; }

    protected IntegrationTestBase()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Sholi@971")
            .WithCleanUp(true)
            .Build();
    }

    public virtual async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                    services.RemoveAll(typeof(ApplicationDbContext));

                    // Add test database with SQL Server container
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_sqlContainer.GetConnectionString());
                    });

                    services.AddMemoryCache();
                    services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
                });

                builder.UseEnvironment("Testing");
            });

        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await Factory.DisposeAsync();
        Client.Dispose();
    }

    // Helper methods for test data seeding
    protected async Task SeedCitiesAsync(params City[] cities)
    {
        using var context = GetDbContext();
        foreach (var city in cities)
        {
            var cityEntity = new City
            {
                Name = city.Name,
                Country = city.Country,
                PostOffice = city.PostOffice,
                NumberOfHotels = city.NumberOfHotels,
                CreatedAt = city.CreatedAt,
                UpdatedAt = city.UpdatedAt,
                LastUpdated = city.LastUpdated
            };
            context.Cities.Add(cityEntity);
        }
        await context.SaveChangesAsync();
    }

    protected async Task ClearDatabaseAsync()
    {
        using var context = GetDbContext();
        context.OtpRecords.RemoveRange(context.OtpRecords);
        context.Bookings.RemoveRange(context.Bookings);
        context.Reviews.RemoveRange(context.Reviews);
        context.Rooms.RemoveRange(context.Rooms);
        context.Hotels.RemoveRange(context.Hotels);
        context.Users.RemoveRange(context.Users);
        context.Cities.RemoveRange(context.Cities);
        context.BlacklistedTokens.RemoveRange(context.BlacklistedTokens);
        await context.SaveChangesAsync();
    }

    protected ApplicationDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}
```

### City Service Integration Tests

Here are real integration test examples from the City service that test caching, repository, and database interactions:

```csharp
public class CityRepositoryGetCitiesIntegrationTests : IntegrationTestBase
{
    private ICityRepository _cityRepository;
    private ICityService _cityService;
    private IMemoryCache _memoryCache;
    private readonly IFixture _fixture;

    public CityRepositoryGetCitiesIntegrationTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _cityService = provider.GetRequiredService<ICityService>();
        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetCities")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var citiesCacheKey = "cities-list";
        await ClearDatabaseAsync();

        var city = _fixture.Build<City>()
            .Without(x => x.CityId)
            .Without(x => x.Hotels)
            .With(x => x.Name, "Nablus")
            .With(x => x.Country, "Palestine")
            .Create();

        await SeedCitiesAsync(city);

        // Act - First call loads from database and caches
        var cities = await _cityService.GetCitiesAsync();

        // Assert
        Assert.NotNull(cities);
        Assert.Single(cities);
        Assert.Equal("Nablus", cities[0].Name);

        // Verify data was cached
        var cacheHit = _memoryCache.TryGetValue(citiesCacheKey, out List<CityReadDto> cachedCities);
        Assert.True(cacheHit);
        Assert.Equal(cities.Count, cachedCities.Count);
        Assert.Equal("Nablus", cachedCities[0].Name);
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetCities")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
        var citiesCacheKey = "cities-list";
        var cityMock = _fixture.Build<City>()
            .Without(x => x.CityId)
            .Without(x => x.Hotels)
            .With(x => x.Name, "Ramallah")
            .With(x => x.Country, "Palestine")
            .Create();

        // Clear cache and database
        _memoryCache.Remove(citiesCacheKey);
        await ClearDatabaseAsync();
        await SeedCitiesAsync(cityMock);

        // Verify cache is empty
        var cacheHit = _memoryCache.TryGetValue(citiesCacheKey, out List<CityReadDto> cachedCities);
        Assert.False(cacheHit);

        // Act
        var cities = await _cityService.GetCitiesAsync();

        // Assert
        Assert.NotNull(cities);
        Assert.Single(cities);
        Assert.Equal("Ramallah", cities[0].Name);

        // Verify data was cached after database call
        cacheHit = _memoryCache.TryGetValue(citiesCacheKey, out List<CityReadDto> newCachedCities);
        Assert.True(cacheHit);
        Assert.Single(newCachedCities);
        Assert.Equal("Ramallah", newCachedCities[0].Name);
    }

    [Fact]
    [Trait("IntegrationTests - Repository", "GetCities")]
    public async Task Should_ReturnCitiesWithHotels_When_CitiesExistInDatabase()
    {
        // Arrange
        await ClearDatabaseAsync();

        var city = _fixture.Build<City>()
            .Without(x => x.CityId)
            .Without(x => x.Hotels)
            .With(x => x.Name, "Jerusalem")
            .With(x => x.Country, "Palestine")
            .With(x => x.NumberOfHotels, 5)
            .Create();

        await SeedCitiesAsync(city);

        // Act - Direct repository call
        var cities = await _cityRepository.GetAllAsync();

        // Assert
        Assert.NotNull(cities);
        Assert.Single(cities);
        Assert.Equal("Jerusalem", cities[0].Name);
        Assert.Equal("Palestine", cities[0].Country);
        Assert.Equal(5, cities[0].NumberOfHotels);

        // Verify Hotels navigation property is included
        Assert.NotNull(cities[0].Hotels);
    }
}
```

### City Entity Structure

The City entity used in tests has the following structure:

```csharp
public class City
{
    public int CityId { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string PostOffice { get; set; }
    public int NumberOfHotels { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    public List<Hotel>? Hotels { get; set; }
}
```

### City Write DTO for API Tests

```csharp
public class CityWriteDto
{
    [Required] [MinLength(3)] public string Name { get; set; }
    [Required] [MinLength(3)] public string Country { get; set; }
    [Required] [MinLength(3)] public string PostOffice { get; set; }
    [Required] [Range(1, int.MaxValue)] public int NumberOfHotels { get; set; }
    [Required] [DataType(DataType.Date)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required] [DataType(DataType.Date)] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### Test Data Generation with AutoFixture

The tests use AutoFixture for generating test data with specific configurations:

```csharp
public CityServiceTests()
{
    _fixture = new Fixture();
    _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
        .ForEach(b => _fixture.Behaviors.Remove(b));
    _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
}

// Example of creating test city data
var cityMock = _fixture.Build<City>()
    .Without(x => x.CityId)        // Exclude auto-generated ID
    .Without(x => x.Hotels)        // Exclude navigation property
    .With(x => x.Name, "Nablus")   // Set specific name
    .With(x => x.Country, "Palestine")
    .Create();
```

```

## 🎯 Test Categories & Attributes

### Test Organization

The project uses specific trait attributes for test categorization:

```csharp
[Trait("UnitTests - City", "GetCities")]
public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache() { }

[Trait("IntegrationTests - Admin", "GetCities")]
public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache() { }

[Trait("IntegrationTests - Repository", "GetCities")]
public async Task Should_ReturnCitiesWithHotels_When_CitiesExistInDatabase() { }
```

### Test Categories Used in Project

- **UnitTests**: Unit tests for All project services
- **IntegrationTests**: Integration Tests for all Services(with In-Memory Caching) + Repository + Database(With TestContainers) 

### Running Tests by Category

```bash
# Run all unit tests
dotnet test --filter "Category~UnitTests"

# Run all integration tests
dotnet test --filter "Category~IntegrationTests"

# Run specific service tests
dotnet test --filter "UnitTests - City"

# Run admin integration tests
dotnet test --filter "IntegrationTests - Admin"
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
```
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

### Mock Guidelines

- **Mock external dependencies** only
- **Verify important interactions**

---

**Continue to**: [Monitoring & Logging](09-monitoring-logging.md) for operational insights.
