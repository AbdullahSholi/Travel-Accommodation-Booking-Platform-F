# Monitoring & Logging Documentation

## 📊 Monitoring Overview

The Travel Accommodation Booking Platform uses Serilog for application logging and Grafana with Prometheus for AWS EC2 server metrics monitoring. This document covers the logging configuration and monitoring setup used in the project.

## 📝 Serilog Logging Configuration

### Basic Serilog Setup

The application uses **Serilog** for logging with two main outputs:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File($"{logPath}/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Console Logging

Console output for development and debugging:

```csharp
.WriteTo.Console(
    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
)
```

### File Logging

File-based logging for production with daily rolling:

```csharp
.WriteTo.File(
    path: $"{logPath}/log-.txt",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30,
    fileSizeLimitBytes: 100_000_000,
    rollOnFileSizeLimit: true,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
)
```

### Log File Management

- **Rolling Interval**: Daily log files (log-20250128.txt)
- **Retention**: 30 days of log files
- **File Size Limit**: 100MB per file
- **Location**: `/app/logs/` directory

## 📊 Application Logging Examples

### Authentication Events

```csharp
// Successful login
_logger.LogInformation("User {Email} logged in successfully", email);

// Failed login attempt
_logger.LogWarning("Failed login attempt for {Email}", email);

// JWT token generation
_logger.LogInformation("JWT token generated for user {UserId}", userId);
```

### Business Operations

```csharp
// Booking creation
_logger.LogInformation("Booking {BookingId} created for user {UserId}",
    booking.BookingId, booking.UserId);

// City operations
_logger.LogInformation("City {CityName} created successfully", cityName);

// Hotel operations
_logger.LogInformation("Hotel {HotelName} added to city {CityId}",
    hotelName, cityId);
```

### Error Logging

```csharp
// Service errors
_logger.LogError(ex, "Failed to create booking for user {UserId}", userId);

// Repository errors
_logger.LogError(ex, "Database error while fetching cities");

// Validation errors
_logger.LogWarning("Invalid data provided for city creation: {ValidationErrors}",
    validationErrors);
```

## 📈 AWS EC2 Monitoring with Grafana & Prometheus

### Monitoring Stack Overview

The application uses **Grafana** and **Prometheus** for monitoring AWS EC2 server metrics:

- **Prometheus**: Collects and stores server metrics
- **Grafana**: Visualizes metrics in dashboards
- **Node Exporter**: Exposes system metrics for Prometheus

### Prometheus Setup on EC2

#### 1. Install Prometheus

```bash
# Download Prometheus
wget https://github.com/prometheus/prometheus/releases/download/v2.40.0/prometheus-2.40.0.linux-amd64.tar.gz
tar xvfz prometheus-2.40.0.linux-amd64.tar.gz
sudo mv prometheus-2.40.0.linux-amd64 /opt/prometheus

# Create Prometheus user
sudo useradd --no-create-home --shell /bin/false prometheus
sudo chown -R prometheus:prometheus /opt/prometheus
```

#### 2. Configure Prometheus

Create `/opt/prometheus/prometheus.yml`:

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: 'node-exporter'
    static_configs:
      - targets: ['localhost:9100']

  - job_name: 'travel-booking-api'
    static_configs:
      - targets: ['localhost:5000']
```

#### 3. Install Node Exporter

```bash
# Download Node Exporter
wget https://github.com/prometheus/node_exporter/releases/download/v1.5.0/node_exporter-1.5.0.linux-amd64.tar.gz
tar xvfz node_exporter-1.5.0.linux-amd64.tar.gz
sudo mv node_exporter-1.5.0.linux-amd64/node_exporter /usr/local/bin/

# Create systemd service
sudo tee /etc/systemd/system/node_exporter.service > /dev/null <<EOF
[Unit]
Description=Node Exporter
After=network.target

[Service]
User=prometheus
Group=prometheus
Type=simple
ExecStart=/usr/local/bin/node_exporter

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable node_exporter
sudo systemctl start node_exporter
```

### Grafana Setup

#### 1. Install Grafana

```bash
# Add Grafana repository
sudo apt-get install -y software-properties-common
wget -q -O - https://packages.grafana.com/gpg.key | sudo apt-key add -
echo "deb https://packages.grafana.com/oss/deb stable main" | sudo tee -a /etc/apt/sources.list.d/grafana.list

# Install Grafana
sudo apt-get update
sudo apt-get install grafana

# Start Grafana service
sudo systemctl daemon-reload
sudo systemctl enable grafana-server
sudo systemctl start grafana-server
```

#### 2. Configure Grafana

1. **Access Grafana**: Open `http://your-ec2-ip:3000`
2. **Default Login**: admin/admin (change on first login)
3. **Add Prometheus Data Source**:
   - Go to Configuration → Data Sources
   - Add Prometheus data source
   - URL: `http://localhost:9090`
   - Save & Test

#### 3. Create Dashboard

Import or create dashboards for monitoring:

**System Metrics Dashboard**:

- CPU Usage: `100 - (avg(rate(node_cpu_seconds_total{mode="idle"}[5m])) * 100)`
- Memory Usage: `(1 - (node_memory_MemAvailable_bytes / node_memory_MemTotal_bytes)) * 100`
- Disk Usage: `100 - ((node_filesystem_avail_bytes * 100) / node_filesystem_size_bytes)`
- Network I/O: `rate(node_network_receive_bytes_total[5m])`, `rate(node_network_transmit_bytes_total[5m])`

### Key Metrics to Monitor

#### System Metrics

- **CPU Usage**: Server processing load
- **Memory Usage**: RAM consumption
- **Disk Usage**: Storage utilization
- **Network I/O**: Network traffic
- **Load Average**: System load over time

#### Application Metrics

- **HTTP Requests**: Request rate and response times
- **Database Connections**: Connection pool usage
- **Error Rates**: Application error frequency
- **Docker Container Stats**: Container resource usage

### Prometheus Systemd Service

Create `/etc/systemd/system/prometheus.service`:

```ini
[Unit]
Description=Prometheus
After=network.target

[Service]
User=prometheus
Group=prometheus
Type=simple
ExecStart=/opt/prometheus/prometheus \
    --config.file=/opt/prometheus/prometheus.yml \
    --storage.tsdb.path=/opt/prometheus/data \
    --web.console.templates=/opt/prometheus/consoles \
    --web.console.libraries=/opt/prometheus/console_libraries \
    --web.listen-address=0.0.0.0:9090

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable prometheus
sudo systemctl start prometheus
```

## Monitoring Dashboard Access

#### Grafana Dashboard

1. **Access**: `http://your-ec2-ip:3000`
2. **Login**: Use configured admin credentials
3. **View Metrics**: Navigate to created dashboards
4. **Time Range**: Adjust time range for analysis

#### Prometheus Metrics

1. **Access**: `http://your-ec2-ip:9090`
2. **Query Interface**: Use PromQL for custom queries
3. **Targets**: Check `/targets` for service health
4. **Graph**: Visualize metrics directly in Prometheus

---

This monitoring setup provides essential visibility into your Travel Accommodation Booking Platform's performance and health using Serilog for application logging and Grafana with Prometheus for system metrics.
