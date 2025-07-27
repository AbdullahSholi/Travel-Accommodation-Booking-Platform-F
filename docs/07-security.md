# Security Documentation

## 🔒 Security Overview

The Travel Accommodation Booking Platform implements comprehensive security measures following industry best practices and OWASP guidelines. This document outlines the security architecture, authentication mechanisms, and protection strategies.

## 🔐 Authentication & Authorization

### JWT (JSON Web Token) Authentication

The system uses stateless JWT authentication with the following configuration:

#### Token Structure
```json
{
  "sub": "user@example.com",
  "jti": "unique-token-id",
  "name": "user@example.com",
  "role": "User",
  "exp": 1640995200,
  "iss": "http://localhost:5000",
  "aud": "http://localhost:5000"
}
```

#### JWT Configuration
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });
```

#### Token Security Features
- **Expiration**: Configurable token lifetime (default: 60 minutes)
- **Issuer Validation**: Prevents token reuse across different applications
- **Audience Validation**: Ensures tokens are intended for this API
- **Signature Validation**: Cryptographic verification of token integrity
- **Clock Skew**: Zero tolerance for time-based attacks

### Role-Based Access Control (RBAC)

#### User Roles
- **User**: Standard user with booking and review capabilities
- **Admin**: Administrative access with user and content management

#### Authorization Implementation
```csharp
[Authorize(Roles = "User, Admin")]
public async Task<IActionResult> CreateBooking([FromBody] BookingWriteDto dto)
{
    // Implementation
}

[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteUser([FromRoute] int id)
{
    // Implementation
}
```

### Token Blacklisting

Secure logout implementation with token blacklisting:

```csharp
public class BlacklistedToken
{
    public int Id { get; set; }
    public string Jti { get; set; }  // JWT ID
    public DateTime Expiration { get; set; }
}
```

#### Blacklist Validation
```csharp
public async Task<bool> CheckIfTokenBlacklistedAsync(string jti)
{
    return await _context.BlacklistedTokens
        .AnyAsync(t => t.Jti == jti && t.Expiration > DateTime.UtcNow);
}
```

## 🔑 Password Security

### Argon2 Password Hashing

The system uses Argon2, the winner of the Password Hashing Competition:

```csharp
public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        return Argon2.Hash(password, timeCost: 3, memoryCost: 65536, parallelism: 1, 
                          type: Argon2Type.Argon2id, hashLength: 32);
    }

    public static bool VerifyPassword(string hashedPassword, string password)
    {
        return Argon2.Verify(hashedPassword, password);
    }
}
```

#### Argon2 Configuration
- **Time Cost**: 3 iterations
- **Memory Cost**: 64 MB (65536 KB)
- **Parallelism**: 1 thread
- **Hash Length**: 32 bytes
- **Type**: Argon2id (hybrid version)

### Password Requirements
- **Minimum Length**: 8 characters
- **Complexity**: Enforced through validation attributes
- **Confirmation**: Required during registration and password reset

## 🛡️ Input Validation & Sanitization

### Data Transfer Object Validation

Comprehensive validation using Data Annotations:

```csharp
public class UserWriteDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"[A-Z][a-zA-Z\s]*$")]
    public string FirstName { get; set; }

    [Required]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$")]
    public string PhoneNumber { get; set; }
}
```

### SQL Injection Prevention

#### Entity Framework Protection
- **Parameterized Queries**: All queries use parameters
- **LINQ to Entities**: Type-safe query construction
- **No Raw SQL**: Avoiding dynamic SQL construction

```csharp
// Safe: Parameterized query
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == email);

// Safe: LINQ expression
var bookings = await _context.Bookings
    .Where(b => b.UserId == userId && b.CheckInDate >= startDate)
    .ToListAsync();
```

## 🔒 Security Headers

### NWebsec Implementation

Comprehensive security headers using NWebsec middleware:

```csharp
app.UseHsts();                                    // HTTP Strict Transport Security
app.UseXContentTypeOptions();                     // X-Content-Type-Options: nosniff
app.UseReferrerPolicy(opts => opts.NoReferrer()); // Referrer-Policy: no-referrer
app.UseXXssProtection(options => options.EnabledWithBlockMode()); // X-XSS-Protection
app.UseXfo(options => options.Deny());            // X-Frame-Options: DENY
```

#### Security Headers Explained
- **HSTS**: Forces HTTPS connections
- **X-Content-Type-Options**: Prevents MIME type sniffing
- **Referrer-Policy**: Controls referrer information
- **X-XSS-Protection**: Enables XSS filtering
- **X-Frame-Options**: Prevents clickjacking attacks

### Content Security Policy (CSP)

```csharp
app.UseCsp(options => options
    .DefaultSources(s => s.Self())
    .ScriptSources(s => s.Self().UnsafeInline())
    .StyleSources(s => s.Self().UnsafeInline())
    .ImageSources(s => s.Self().Data())
    .ConnectSources(s => s.Self())
);
```

## 🚫 Rate Limiting

### AspNetCoreRateLimit Configuration

Protection against brute force and DDoS attacks:

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 10
      },
      {
        "Endpoint": "POST:/api/auth/login",
        "Period": "1m",
        "Limit": 5
      }
    ]
  }
}
```

#### Rate Limiting Features
- **IP-based Limiting**: Per-IP address restrictions
- **Endpoint-specific Limits**: Different limits for different endpoints
- **Configurable Periods**: Flexible time windows
- **Custom Headers**: Support for proxy environments

## 🔐 Multi-Factor Authentication (MFA)

### OTP (One-Time Password) System

Email-based OTP for password reset:

```csharp
public class OtpRecord
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }        // 6-digit numeric code
    public DateTime Expiration { get; set; } // 5-minute expiration
    public int UserId { get; set; }
}
```

#### OTP Security Features
- **Short Expiration**: 5-minute validity window
- **Single Use**: OTP invalidated after use
- **Secure Generation**: Cryptographically secure random generation
- **Rate Limited**: Limited OTP requests per user

### OTP Generation
```csharp
public static string GenerateOtp()
{
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[4];
    rng.GetBytes(bytes);
    var code = BitConverter.ToUInt32(bytes, 0) % 1000000;
    return code.ToString("D6");
}
```

## 🔍 Security Monitoring & Logging

### Structured Logging with Serilog

Security-focused logging implementation:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File($"{logPath}/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("Application", "TravelBookingAPI")
    .CreateLogger();
```

#### Security Events Logged
- **Authentication Attempts**: Success and failure
- **Authorization Failures**: Unauthorized access attempts
- **Input Validation Errors**: Malicious input detection
- **Rate Limit Violations**: Potential attack patterns
- **Token Blacklisting**: Logout events

### Example Security Logs
```csharp
// Successful login
_logger.LogInformation("User {Email} logged in successfully", email);

// Failed login attempt
_logger.LogWarning("Failed login attempt for {Email} from {IP}", email, ipAddress);

// Authorization failure
_logger.LogWarning("Unauthorized access attempt to {Endpoint} by {UserId}", endpoint, userId);

// Rate limit exceeded
_logger.LogWarning("Rate limit exceeded for IP {IP} on endpoint {Endpoint}", ip, endpoint);
```

## 🔒 Data Protection

### Sensitive Data Handling

#### Email Masking in Logs
```csharp
public static string MaskEmail(string email)
{
    if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        return email;
    
    var parts = email.Split('@');
    var username = parts[0];
    var domain = parts[1];
    
    var maskedUsername = username.Length > 2 
        ? $"{username[0]}***{username[^1]}" 
        : "***";
    
    return $"{maskedUsername}@{domain}";
}
```

#### Database Encryption
- **Connection String Encryption**: Encrypted in production
- **Sensitive Fields**: Consider field-level encryption for PII
- **Backup Encryption**: Encrypted database backups

### GDPR Compliance

#### Data Subject Rights
- **Right to Access**: User data export functionality
- **Right to Rectification**: User profile update capabilities
- **Right to Erasure**: User account deletion with data cleanup
- **Data Portability**: Export user data in machine-readable format

#### Data Retention
```csharp
// Cleanup expired OTP records
public async Task CleanupExpiredOtpRecordsAsync()
{
    var expiredRecords = await _context.OtpRecords
        .Where(o => o.Expiration < DateTime.UtcNow)
        .ToListAsync();
    
    _context.OtpRecords.RemoveRange(expiredRecords);
    await _context.SaveChangesAsync();
}
```

## 🛡️ Security Best Practices

### Development Security
1. **Secrets Management**: Use Azure Key Vault or AWS Secrets Manager
2. **Environment Separation**: Different keys for dev/staging/production
3. **Code Reviews**: Security-focused code review process
4. **Dependency Scanning**: Regular vulnerability scanning of NuGet packages

### Production Security
1. **HTTPS Only**: Force HTTPS in production
2. **Security Headers**: Implement all recommended security headers
3. **Regular Updates**: Keep dependencies and runtime updated
4. **Monitoring**: Implement security monitoring and alerting

### Security Checklist
- [ ] JWT tokens properly configured and validated
- [ ] Passwords hashed with Argon2
- [ ] Input validation on all endpoints
- [ ] Rate limiting implemented
- [ ] Security headers configured
- [ ] HTTPS enforced
- [ ] Sensitive data properly handled
- [ ] Security logging implemented
- [ ] Regular security updates applied
- [ ] Vulnerability scanning in CI/CD

---

**Continue to**: [Testing Documentation](08-testing.md) for testing strategies.
