# Monitoring & Logging Documentation

## 📊 Monitoring Overview

The Travel Accommodation Booking Platform implements comprehensive monitoring and logging to ensure system reliability, performance tracking, and effective troubleshooting. This document covers logging configuration, monitoring setup, and operational procedures.

## 📝 Logging Architecture

### Serilog Configuration

The system uses **Serilog** for structured logging with multiple sinks:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File($"{logPath}/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("Application", "TravelBookingAPI")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
    .CreateLogger();
```

### Log Sinks Configuration

#### Console Sink
```csharp
.WriteTo.Console(
    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
)
```

#### File Sink
```csharp
.WriteTo.File(
    path: $"{logPath}/log-.txt",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30,
    fileSizeLimitBytes: 100_000_000,
    rollOnFileSizeLimit: true,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
)
```

### Structured Logging Examples

#### Authentication Events
```csharp
// Successful login
_logger.LogInformation("User {Email} logged in successfully from {IPAddress}", 
    email, httpContext.Connection.RemoteIpAddress);

// Failed login attempt
_logger.LogWarning("Failed login attempt for {Email} from {IPAddress}. Reason: {Reason}", 
    email, ipAddress, "Invalid password");

// Account lockout
_logger.LogWarning("Account {Email} locked due to multiple failed attempts from {IPAddress}", 
    email, ipAddress);
```

#### Business Operations
```csharp
// Booking creation
_logger.LogInformation("Booking {BookingId} created for user {UserId} - Room {RoomId} from {CheckIn} to {CheckOut}", 
    booking.BookingId, booking.UserId, booking.RoomId, booking.CheckInDate, booking.CheckOutDate);

// Payment processing
_logger.LogInformation("Payment processed for booking {BookingId} - Amount: {Amount}, Status: {Status}", 
    bookingId, amount, paymentStatus);

// Error scenarios
_logger.LogError(ex, "Failed to process booking for user {UserId} - Room {RoomId}", 
    userId, roomId);
```

#### Performance Monitoring
```csharp
// Database query performance
using (_logger.BeginScope("Database Query: {QueryType}", "GetAvailableRooms"))
{
    var stopwatch = Stopwatch.StartNew();
    var rooms = await _repository.GetAvailableRoomsAsync(criteria);
    stopwatch.Stop();
    
    _logger.LogInformation("Query completed in {ElapsedMs}ms, returned {ResultCount} rooms", 
        stopwatch.ElapsedMilliseconds, rooms.Count);
}
```

## 🔍 Log Levels & Categories

### Log Level Guidelines

#### Debug
- Detailed diagnostic information
- Variable values and method entry/exit
- Only enabled in development

```csharp
_logger.LogDebug("Processing booking request with data: {@BookingData}", bookingDto);
```

#### Information
- General application flow
- Business events and milestones
- User actions

```csharp
_logger.LogInformation("User {UserId} successfully created booking {BookingId}", userId, bookingId);
```

#### Warning
- Unexpected situations that don't stop the application
- Validation failures
- Recoverable errors

```csharp
_logger.LogWarning("Room {RoomId} is no longer available for requested dates {CheckIn} to {CheckOut}", 
    roomId, checkIn, checkOut);
```

#### Error
- Error events that might still allow the application to continue
- Handled exceptions
- External service failures

```csharp
_logger.LogError(ex, "Failed to send confirmation email for booking {BookingId}", bookingId);
```

#### Critical
- Serious errors that might cause the application to abort
- System-level failures
- Security breaches

```csharp
_logger.LogCritical("Database connection failed - Application cannot continue");
```

### Log Categories

#### Authentication & Authorization
```csharp
private readonly ILogger<AuthService> _logger;

_logger.LogInformation("JWT token generated for user {UserId}", userId);
_logger.LogWarning("Unauthorized access attempt to {Endpoint}", endpoint);
```

#### Business Logic
```csharp
private readonly ILogger<BookingService> _logger;

_logger.LogInformation("Booking validation completed for {BookingId}", bookingId);
_logger.LogError("Business rule violation: {Rule} for booking {BookingId}", rule, bookingId);
```

#### Data Access
```csharp
private readonly ILogger<BookingRepository> _logger;

_logger.LogDebug("Executing query: {Query}", query);
_logger.LogWarning("Query timeout for {Operation} after {TimeoutMs}ms", operation, timeout);
```

## 📈 Monitoring Stack

### Grafana Loki Integration

#### Promtail Configuration
```yaml
server:
  http_listen_port: 9080
  grpc_listen_port: 0

positions:
  filename: /tmp/positions.yaml

clients:
  - url: http://loki:3100/loki/api/v1/push

scrape_configs:
  - job_name: travel-booking-api
    static_configs:
      - targets:
          - localhost
        labels:
          job: travel-booking-api
          __path__: /app/logs/*.txt
    pipeline_stages:
      - regex:
          expression: '^(?P<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}) \[(?P<level>\w+)\] (?P<message>.*)'
      - labels:
          level:
          timestamp:
```

#### Loki Queries
```logql
# Error rate by endpoint
rate({job="travel-booking-api"} |= "ERROR" [5m])

# Authentication failures
{job="travel-booking-api"} |= "Failed login attempt"

# Performance issues
{job="travel-booking-api"} |= "Query completed" | regex "(?P<duration>\\d+)ms" | duration > 1000

# Business metrics
{job="travel-booking-api"} |= "Booking" |= "created"
```

### Application Metrics

#### Custom Metrics with Prometheus
```csharp
public class MetricsService
{
    private static readonly Counter BookingCreatedCounter = Metrics
        .CreateCounter("bookings_created_total", "Total number of bookings created");
    
    private static readonly Histogram BookingProcessingDuration = Metrics
        .CreateHistogram("booking_processing_duration_seconds", "Time spent processing bookings");
    
    private static readonly Gauge ActiveUsersGauge = Metrics
        .CreateGauge("active_users", "Number of currently active users");

    public void RecordBookingCreated()
    {
        BookingCreatedCounter.Inc();
    }

    public void RecordBookingProcessingTime(double seconds)
    {
        BookingProcessingDuration.Observe(seconds);
    }

    public void SetActiveUsers(int count)
    {
        ActiveUsersGauge.Set(count);
    }
}
```

#### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddCheck<EmailServiceHealthCheck>("email-service")
    .AddCheck<ExternalApiHealthCheck>("external-api");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## 🚨 Alerting & Notifications

### Alert Rules

#### Error Rate Alerts
```yaml
groups:
  - name: travel-booking-api
    rules:
      - alert: HighErrorRate
        expr: rate(log_entries{level="ERROR"}[5m]) > 0.1
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value }} errors per second"

      - alert: DatabaseConnectionFailure
        expr: up{job="travel-booking-api-db"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Database connection failed"
          description: "Database is unreachable"
```

#### Performance Alerts
```yaml
      - alert: SlowResponseTime
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Slow response times detected"
          description: "95th percentile response time is {{ $value }}s"

      - alert: HighMemoryUsage
        expr: process_resident_memory_bytes / 1024 / 1024 > 1000
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High memory usage"
          description: "Memory usage is {{ $value }}MB"
```

### Notification Channels

#### Email Notifications
```csharp
public class AlertNotificationService
{
    public async Task SendAlertAsync(AlertLevel level, string message, string details)
    {
        var subject = $"[{level}] Travel Booking API Alert";
        var body = $@"
            Alert Level: {level}
            Message: {message}
            Details: {details}
            Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
            Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}
        ";

        await _emailService.SendAsync("alerts@company.com", subject, body);
    }
}
```

#### Slack Integration
```csharp
public class SlackNotificationService
{
    public async Task SendSlackAlertAsync(string channel, AlertLevel level, string message)
    {
        var color = level switch
        {
            AlertLevel.Critical => "danger",
            AlertLevel.Warning => "warning",
            AlertLevel.Info => "good",
            _ => "good"
        };

        var payload = new
        {
            channel = channel,
            username = "Travel Booking API",
            attachments = new[]
            {
                new
                {
                    color = color,
                    title = $"{level} Alert",
                    text = message,
                    ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };

        await _httpClient.PostAsJsonAsync(_webhookUrl, payload);
    }
}
```

## 🔧 Operational Procedures

### Log Analysis

#### Common Log Queries

**Find authentication issues:**
```bash
grep "Failed login attempt" /app/logs/log-*.txt | tail -100
```

**Monitor booking patterns:**
```bash
grep "Booking.*created" /app/logs/log-*.txt | awk '{print $1, $2}' | sort | uniq -c
```

**Performance analysis:**
```bash
grep "Query completed" /app/logs/log-*.txt | grep -o "[0-9]*ms" | sort -n | tail -20
```

#### Log Rotation & Cleanup

```bash
#!/bin/bash
# Log cleanup script
LOG_DIR="/app/logs"
RETENTION_DAYS=30

find $LOG_DIR -name "log-*.txt" -type f -mtime +$RETENTION_DAYS -delete
find $LOG_DIR -name "*.gz" -type f -mtime +$RETENTION_DAYS -delete

# Compress old logs
find $LOG_DIR -name "log-*.txt" -type f -mtime +7 -exec gzip {} \;
```

### Troubleshooting Guide

#### High CPU Usage
1. Check active connections: `netstat -an | grep :5000 | wc -l`
2. Review slow queries in logs
3. Monitor garbage collection: `dotnet-counters monitor --process-id <pid>`

#### Memory Leaks
1. Generate memory dump: `dotnet-dump collect -p <pid>`
2. Analyze with dotMemory or PerfView
3. Check for unclosed connections in logs

#### Database Issues
1. Check connection pool: Monitor `DbContext` creation/disposal
2. Review query performance logs
3. Verify database connectivity: `telnet db-server 1433`

#### Authentication Problems
1. Verify JWT configuration
2. Check token expiration logs
3. Validate certificate/key configuration

### Performance Monitoring

#### Key Performance Indicators (KPIs)

**Response Time Metrics:**
- Average response time: < 200ms
- 95th percentile: < 500ms
- 99th percentile: < 1000ms

**Throughput Metrics:**
- Requests per second: Monitor baseline and peaks
- Concurrent users: Track active sessions
- Database connections: Monitor pool usage

**Error Metrics:**
- Error rate: < 1% of total requests
- 5xx errors: < 0.1% of total requests
- Authentication failures: Monitor for attacks

#### Monitoring Dashboards

**Grafana Dashboard Panels:**
1. Request rate and response times
2. Error rates by endpoint
3. Database query performance
4. Memory and CPU usage
5. Active user sessions
6. Business metrics (bookings, registrations)

### Maintenance Tasks

#### Daily Tasks
- Review error logs for new issues
- Check system resource usage
- Verify backup completion
- Monitor alert notifications

#### Weekly Tasks
- Analyze performance trends
- Review security logs
- Update monitoring thresholds
- Clean up old log files

#### Monthly Tasks
- Performance baseline review
- Capacity planning analysis
- Security audit log review
- Monitoring system updates

---

**Congratulations!** You have completed the comprehensive documentation for the Travel Accommodation Booking Platform. This documentation covers all aspects from project overview to operational monitoring.
