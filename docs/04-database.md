# Database Documentation

## 🗄️ Database Overview

The Travel Accommodation Booking Platform uses **SQL Server** as the primary database with **Entity Framework Core** as the ORM. The database follows a relational design with proper normalization and foreign key relationships.

### Database Provider
- **Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core 9.0
- **Migration Strategy**: Code-First with automatic migrations
- **Connection**: Configured via connection strings

## 📊 Database Schema

### Entity Relationship Diagram
<img width="676" height="937" alt="image" src="https://github.com/user-attachments/assets/83cb83dd-9ea0-4690-9733-12da6c52c880" />


## 🏗️ Core Entities

### 1. User Entity
**Table**: `Users`
**Purpose**: Store user account information and authentication data

```sql
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(MAX) NOT NULL,
    LastName NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    Password NVARCHAR(MAX) NOT NULL,
    PhoneNumber NVARCHAR(MAX) NOT NULL,
    DateOfBirth DATETIME2 NOT NULL,
    Address1 NVARCHAR(MAX) NOT NULL,
    Address2 NVARCHAR(MAX),
    City NVARCHAR(MAX) NOT NULL,
    Country NVARCHAR(MAX) NOT NULL,
    DriverLicense NVARCHAR(MAX) NOT NULL,
    IsEmailConfirmed BIT NOT NULL DEFAULT 0,
    Role NVARCHAR(MAX) NOT NULL DEFAULT 'User',
    LastUpdated DATETIME2 NOT NULL
);
```

**Key Properties**:
- `UserId`: Primary key, auto-increment
- `Email`: Unique identifier for authentication
- `Password`: Argon2 hashed password
- `Role`: User role (User, Admin)
- `IsEmailConfirmed`: Email verification status

**Relationships**:
- One-to-Many with `Booking`
- One-to-Many with `Review`
- One-to-Many with `OtpRecord`

### 2. City Entity
**Table**: `Cities`
**Purpose**: Store city information for hotel locations

```sql
CREATE TABLE Cities (
    CityId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL,
    Country NVARCHAR(MAX) NOT NULL,
    PostOffice NVARCHAR(MAX) NOT NULL,
    NumberOfHotels INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    LastUpdated DATETIME2 NOT NULL
);
```

**Key Properties**:
- `CityId`: Primary key, auto-increment
- `Name`: City name
- `Country`: Country name
- `PostOffice`: Postal code
- `NumberOfHotels`: Count of hotels in the city

**Relationships**:
- One-to-Many with `Hotel`

### 3. Hotel Entity
**Table**: `Hotels`
**Purpose**: Store hotel information and metadata

```sql
CREATE TABLE Hotels (
    HotelId INT IDENTITY(1,1) PRIMARY KEY,
    HotelName NVARCHAR(MAX) NOT NULL,
    OwnerName NVARCHAR(MAX) NOT NULL,
    StarRating FLOAT NOT NULL,
    Location NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    CityId INT NOT NULL,
    LastUpdated DATETIME2 NOT NULL,
    FOREIGN KEY (CityId) REFERENCES Cities(CityId) ON DELETE CASCADE
);
```

**Key Properties**:
- `HotelId`: Primary key, auto-increment
- `HotelName`: Hotel name
- `StarRating`: Hotel rating (0-5 stars)
- `CityId`: Foreign key to City

**Relationships**:
- Many-to-One with `City`
- One-to-Many with `Room`
- One-to-Many with `Review`

### 4. Room Entity
**Table**: `Rooms`
**Purpose**: Store room information and availability

```sql
CREATE TABLE Rooms (
    RoomId INT IDENTITY(1,1) PRIMARY KEY,
    RoomType INT NOT NULL, -- Enum: Luxury=0, Budget=1, Boutique=2
    Images NVARCHAR(MAX) NOT NULL, -- JSON array of image URLs
    Description NVARCHAR(MAX) NOT NULL,
    PricePerNight DECIMAL(18,2) NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    AdultCapacity INT NOT NULL DEFAULT 2,
    ChildrenCapacity INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    HotelId INT NOT NULL,
    LastUpdated DATETIME2 NOT NULL,
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId) ON DELETE CASCADE
);
```

**Key Properties**:
- `RoomId`: Primary key, auto-increment
- `RoomType`: Enum (Luxury, Budget, Boutique)
- `PricePerNight`: Decimal with precision (18,2)
- `IsAvailable`: Availability status
- `HotelId`: Foreign key to Hotel

**Relationships**:
- Many-to-One with `Hotel`
- One-to-Many with `Booking`

### 5. Booking Entity
**Table**: `Bookings`
**Purpose**: Store booking information and transactions

```sql
CREATE TABLE Bookings (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RoomId INT NOT NULL,
    CheckInDate DATETIME2 NOT NULL,
    CheckOutDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    LastUpdated DATETIME2 NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId) ON DELETE CASCADE
);
```

**Key Properties**:
- `BookingId`: Primary key, auto-increment
- `UserId`: Foreign key to User
- `RoomId`: Foreign key to Room
- `TotalPrice`: Decimal with precision (18,2)

**Relationships**:
- Many-to-One with `User`
- Many-to-One with `Room`

### 6. Review Entity
**Table**: `Reviews`
**Purpose**: Store user reviews and ratings for hotels

```sql
CREATE TABLE Reviews (
    ReviewId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    HotelId INT NOT NULL,
    Rating INT NOT NULL, -- 0-5 stars
    Comment NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    LastUpdated DATETIME2 NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId) ON DELETE CASCADE
);
```

**Key Properties**:
- `ReviewId`: Primary key, auto-increment
- `Rating`: Integer rating (0-5)
- `Comment`: Review text

**Relationships**:
- Many-to-One with `User`
- Many-to-One with `Hotel`

## 🔧 Supporting Entities

### 7. OtpRecord Entity
**Table**: `OtpRecords`
**Purpose**: Store OTP codes for password reset functionality

```sql
CREATE TABLE OtpRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(MAX) NOT NULL,
    Code NVARCHAR(MAX) NOT NULL,
    Expiration DATETIME2 NOT NULL,
    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
```

**Key Properties**:
- `Code`: 6-digit OTP code
- `Expiration`: UTC expiration time (5 minutes)
- `UserId`: Foreign key to User

### 8. BlacklistedToken Entity
**Table**: `BlacklistedTokens`
**Purpose**: Store blacklisted JWT tokens for secure logout

```sql
CREATE TABLE BlacklistedTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Jti NVARCHAR(MAX) NOT NULL, -- JWT ID
    Expiration DATETIME2 NOT NULL
);
```

**Key Properties**:
- `Jti`: JWT token identifier
- `Expiration`: Token expiration time

## 🔧 Entity Configurations

### Precision Configurations
The system uses specific Entity Framework configurations for decimal precision:

**BookingConfiguration**:
```csharp
builder.Property(b => b.TotalPrice)
    .HasPrecision(18, 2);
```

**RoomConfiguration**:
```csharp
builder.Property(r => r.PricePerNight)
    .HasPrecision(18, 2);
```

**OtpRecordConfiguration**:
```csharp
builder.Property(e => e.Expiration)
    .HasConversion(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
```

### Enumerations

**RoomType Enum**:
```csharp
public enum RoomType
{
    Luxury = 0,
    Budget = 1,
    Boutique = 2
}
```

## 🗃️ Database Relationships

### Foreign Key Relationships
1. **City → Hotel**: One-to-Many (CASCADE DELETE)
2. **Hotel → Room**: One-to-Many (CASCADE DELETE)
3. **Hotel → Review**: One-to-Many (CASCADE DELETE)
4. **User → Booking**: One-to-Many (CASCADE DELETE)
5. **User → Review**: One-to-Many (CASCADE DELETE)
6. **User → OtpRecord**: One-to-Many (CASCADE DELETE)
7. **Room → Booking**: One-to-Many (CASCADE DELETE)


## 📈 Database Indexing

### Primary Keys
All entities have auto-incrementing integer primary keys:
- `UserId`, `CityId`, `HotelId`, `RoomId`, `BookingId`, `ReviewId`

### Foreign Key Indexes
Entity Framework automatically creates indexes for foreign keys:
- `IX_Hotels_CityId`
- `IX_Rooms_HotelId`
- `IX_Bookings_UserId`
- `IX_Bookings_RoomId`
- `IX_Reviews_UserId`
- `IX_Reviews_HotelId`
- `IX_OtpRecords_UserId`

## 🔄 Database Migrations

### Migration History
The database schema has evolved through several migrations:

1. **Initial Migration**: Basic User entity
2. **AddOtherEntitiesWithTheirRelationships**: Added Hotel, Room, City, Booking entities
3. **AddDbSetForAllEntities**: Added Review, OtpRecord, BlacklistedToken entities
4. **AddLastUpdatedPropertyForApplyETagHTTPCaching**: Added LastUpdated properties for caching
5. **AddReviewEntity**: Enhanced Review entity relationships

### Migration Commands
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

## 💾 Data Storage Considerations

### String Storage
- Most string fields use `NVARCHAR(MAX)` for flexibility
- Consider adding length constraints for production optimization

### DateTime Handling
- All DateTime fields use `DATETIME2` for precision
- OTP expiration uses UTC conversion for consistency
- LastUpdated fields support ETag caching

### Decimal Precision
- Monetary values use `DECIMAL(18,2)` for accuracy
- Supports values up to 999,999,999,999,999.99

### JSON Storage
- Room images stored as JSON array in string field
- Consider using SQL Server JSON functions for complex queries

### Cleanup Procedures

**Remove expired OTP records**:
```sql
DELETE FROM OtpRecords
WHERE Expiration < GETUTCDATE();
```

---

**Continue to**: [Development Guide](05-development.md) for setup instructions.
