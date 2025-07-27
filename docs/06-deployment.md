# Deployment Guide

## 🚀 Deployment Overview

The Travel Accommodation Booking Platform supports multiple deployment strategies including Docker containers, cloud platforms, and traditional server deployments. This guide covers the recommended deployment approaches for different environments.

## 🐳 Docker Deployment

### Dockerfile Configuration

The application includes a multi-stage Dockerfile for optimized production builds:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY Travel-Accommodation-Booking-Platform-F.Application ./Travel-Accommodation-Booking-Platform-F.Application
COPY Travel-Accommodation-Booking-Platform-F.Infrastructure ./Travel-Accommodation-Booking-Platform-F.Infrastructure
COPY Travel-Accommodation-Booking-Platform-F.Domain ./Travel-Accommodation-Booking-Platform-F.Domain
COPY Travel-Accommodation-Booking-Platform-F.API ./Travel-Accommodation-Booking-Platform-F.API

WORKDIR /src/Travel-Accommodation-Booking-Platform-F.API

# Restore and publish
RUN dotnet restore Travel-Accommodation-Booking-Platform-F.API.csproj
RUN dotnet publish Travel-Accommodation-Booking-Platform-F.API.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Travel-Accommodation-Booking-Platform-F.API.dll"]
```

### Docker Compose Setup

Create `docker-compose.yml` for local development:

```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TravelBookingDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true
    depends_on:
      - sqlserver
    networks:
      - travel-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - travel-network

volumes:
  sqlserver_data:

networks:
  travel-network:
    driver: bridge
```

### Docker Commands

```bash
# Build the image
docker build -t travel-booking-api .

# Run the container
docker run -p 5000:8080 travel-booking-api

# Run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

## ☁️ Cloud Deployment

### AWS EC2 Deployment

#### Prerequisites
- AWS EC2 instance (t3.medium or larger recommended)
- Ubuntu 20.04 LTS or Amazon Linux 2
- Security group allowing HTTP (80), HTTPS (443), and SSH (22)

#### Setup Steps

1. **Connect to EC2 Instance**:
```bash
ssh -i your-key.pem ubuntu@your-ec2-ip
```

2. **Install Docker**:
```bash
sudo apt update
sudo apt install docker.io docker-compose -y
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker ubuntu
```

3. **Clone Repository**:
```bash
git clone https://github.com/your-org/Travel-Accommodation-Booking-Platform-F.git
cd Travel-Accommodation-Booking-Platform-F
```

4. **Configure Environment**:
```bash
# Create production environment file
sudo nano .env
```

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TravelBookingDB;User Id=sa;Password=YourSecurePassword123!;TrustServerCertificate=true
Jwt__Key=your-super-secret-jwt-key-here-minimum-32-characters-for-production
EmailSettings__Password=your-email-app-password
```

5. **Deploy with Docker Compose**:
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Azure App Service Deployment

#### Using Azure CLI

1. **Login to Azure**:
```bash
az login
```

2. **Create Resource Group**:
```bash
az group create --name travel-booking-rg --location "East US"
```

3. **Create App Service Plan**:
```bash
az appservice plan create --name travel-booking-plan --resource-group travel-booking-rg --sku B1 --is-linux
```

4. **Create Web App**:
```bash
az webapp create --resource-group travel-booking-rg --plan travel-booking-plan --name travel-booking-api --deployment-container-image-name your-registry/travel-booking-api:latest
```

5. **Configure App Settings**:
```bash
az webapp config appsettings set --resource-group travel-booking-rg --name travel-booking-api --settings ConnectionStrings__DefaultConnection="your-connection-string"
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

The project includes a comprehensive GitHub Actions workflow (`.github/workflows/deploy.yml`):

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal

  security-scan:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Run Trivy secrets scan
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: fs
        scan-ref: .
        scanners: secret
        format: table
        exit-code: 1

  deploy:
    runs-on: ubuntu-latest
    needs: [test, security-scan]
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2

    - name: Login to DockerHub
      uses: docker/login-action@v2
      with:
        username: ${{ secrets.DOCKERHUB_USERNAME }}
        password: ${{ secrets.DOCKERHUB_TOKEN }}

    - name: Build and push Docker image
      uses: docker/build-push-action@v4
      with:
        context: .
        file: ./Travel-Accommodation-Booking-Platform-F.API/Dockerfile
        push: true
        tags: ${{ secrets.DOCKERHUB_USERNAME }}/travel-booking-api:latest

    - name: Run Trivy vulnerability scan
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: ${{ secrets.DOCKERHUB_USERNAME }}/travel-booking-api:latest
        format: table
        exit-code: 1
        ignore-unfixed: true
        severity: HIGH,CRITICAL
```

### Required Secrets

Configure these secrets in your GitHub repository:

- `DOCKERHUB_USERNAME`: Docker Hub username
- `DOCKERHUB_TOKEN`: Docker Hub access token
- `AWS_ACCESS_KEY_ID`: AWS access key (if deploying to AWS)
- `AWS_SECRET_ACCESS_KEY`: AWS secret key
- `DATABASE_CONNECTION_STRING`: Production database connection string

## 🔧 Production Configuration

### Environment Variables

```bash
# Application Settings
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Database
ConnectionStrings__DefaultConnection=your-production-connection-string

# JWT Configuration
Jwt__Key=your-super-secret-jwt-key-minimum-32-characters
Jwt__Issuer=https://your-domain.com
Jwt__Audience=https://your-domain.com
Jwt__ExpiresInMinutes=60

# Email Settings
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__Port=587
EmailSettings__SenderEmail=noreply@your-domain.com
EmailSettings__Password=your-app-password

# Logging
LOG_PATH=/app/logs
```

### SSL/TLS Configuration

#### Using Let's Encrypt with Nginx

1. **Install Nginx**:
```bash
sudo apt install nginx certbot python3-certbot-nginx
```

2. **Configure Nginx**:
```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

3. **Obtain SSL Certificate**:
```bash
sudo certbot --nginx -d your-domain.com
```

### Health Checks

Add health check endpoints for monitoring:

```csharp
// In Program.cs
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");
```

### Monitoring Setup

#### Application Insights (Azure)
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

#### Prometheus Metrics
```csharp
builder.Services.AddPrometheusMetrics();
app.UsePrometheusMetrics();
```

## 📊 Performance Optimization

### Production Optimizations

1. **Enable Response Compression**:
```csharp
builder.Services.AddResponseCompression();
app.UseResponseCompression();
```

2. **Configure Caching**:
```csharp
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
app.UseResponseCaching();
```

3. **Database Connection Pooling**:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.CommandTimeout(30);
    }));
```

### Load Balancing

For high-traffic scenarios, consider:
- **Application Load Balancer** (AWS ALB)
- **Azure Load Balancer**
- **Nginx Load Balancer**

Example Nginx load balancer configuration:
```nginx
upstream api_servers {
    server 10.0.1.10:5000;
    server 10.0.1.11:5000;
    server 10.0.1.12:5000;
}

server {
    listen 80;
    location / {
        proxy_pass http://api_servers;
    }
}
```

---

**Continue to**: [Security Documentation](07-security.md) for security best practices.
