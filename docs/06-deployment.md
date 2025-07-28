# Deployment Guide

## 🚀 Deployment Overview

The Travel Accommodation Booking Platform is designed exclusively for cloud deployment using Docker Swarm on AWS EC2. This guide covers the deployment process and CI/CD pipeline configuration.

## 🐳 Docker Configuration

### Dockerfile

The application uses a multi-stage Dockerfile for production builds:

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

### Docker Compose Configuration

```yaml
services:
  api:
    image: myproject-api
    ports:
      - "5000:80"
    deploy:
      replicas: 1
      resources:
        limits:
          cpus: "0.5"
          memory: 256M
```

## ☁️ AWS EC2 Setup

### Prerequisites

- AWS EC2 instance (t3.medium or larger)
- Ubuntu 20.04 LTS
- Security group allowing ports: 22 (SSH), 5000 (API), 2377, 7946, 4789 (Docker Swarm)

### Installation Steps

**1. Connect to EC2 Instance:**

```bash
ssh -i your-key.pem ubuntu@your-ec2-public-ip
```

**2. Install Docker:**

```bash
sudo apt update && sudo apt upgrade -y
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker ubuntu
exit
```

**3. Initialize Docker Swarm:**

```bash
docker swarm init
docker node ls
```

**4. Configure Firewall:**

```bash
sudo ufw allow ssh
sudo ufw allow 5000
sudo ufw allow 2377
sudo ufw allow 7946
sudo ufw allow 4789/udp
sudo ufw --force enable
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

The deployment pipeline (`deploy.yml`) performs these steps:

1. **Build and Test**: Runs unit and integration tests
2. **Security Scanning**: Uses Trivy for vulnerability scanning
3. **Docker Build**: Creates and pushes Docker image to Docker Hub
4. **Deploy**: SSH to EC2 and update Docker service
5. **Notifications**: Sends Slack notifications

### Required GitHub Secrets

- `DOCKERHUB_USERNAME`: Docker Hub username
- `DOCKERHUB_TOKEN`: Docker Hub access token
- `EC2_HOST`: EC2 instance public IP
- `EC2_SSH_KEY`: Private SSH key for EC2 access
- `SQLSERVER_CONNECTIONSTRING`: Database connection string
- `SECRET_KEY`: JWT secret key
- `APP_PASSWORD`: Email service password
- `SLACK_WEBHOOK_URL`: Slack webhook URL

### Deployment Command

The pipeline deploys using this command:

```bash
docker service update \
  --env SECRET_KEY=$SECRET_KEY \
  --env TRAVEL_ACCOMMODATION_CONNECTION_STRING="$CONNECTION_STRING" \
  --env APP_PASSWORD=$APP_PASSWORD \
  --image abdullahgsholi/myproject-api:latest myproject_api || \
docker service create --name myproject_api --replicas 1 -p 5000:80 \
  --env SECRET_KEY=$SECRET_KEY \
  --env TRAVEL_ACCOMMODATION_CONNECTION_STRING="$CONNECTION_STRING" \
  --env APP_PASSWORD=$APP_PASSWORD \
  abdullahgsholi/myproject-api:latest
```

## 🛠️ Service Management

### Basic Commands

```bash
# Check service status
docker service ls

# View service logs
docker service logs myproject_api

# Update service image
docker service update --image abdullahgsholi/myproject-api:latest myproject_api

# Scale service
docker service scale myproject_api=2

# Remove service
docker service rm myproject_api
```

### Using Docker Compose

```bash
# Deploy stack
docker stack deploy -c docker-compose.yml myproject

# Check stack status
docker stack services myproject

# Remove stack
docker stack rm myproject
```

## 📱 Slack Notifications

The pipeline sends deployment status notifications to Slack:

- ✅ **Success**: "🚀 The update was successfully deployed to Docker Swarm!"
- ❌ **Failure**: "❌ Deployment failed. Check pipeline logs."
- ⚠️ **Cancelled**: "⚠️ Deployment was cancelled."

### Setup Slack Integration

1. Create Slack app at https://api.slack.com/apps
2. Enable Incoming Webhooks
3. Add webhook URL to GitHub secrets as `SLACK_WEBHOOK_URL`

## 🧪 Testing

The pipeline runs two test categories:

**Unit Tests:**

```bash
dotnet test --filter "Category~UnitTests"
```

**Integration Tests:**

```bash
dotnet test --filter "Category~IntegrationTests"
```

## 🔒 Security Scanning

The pipeline includes Trivy security scanning:

- **Secrets scanning**: Detects exposed secrets in code
- **Vulnerability scanning**: Scans Docker images for vulnerabilities
- **SARIF reports**: Uploads security reports to GitHub Security tab

## 📊 Monitoring

Install Prometheus and Grafana on the EC2 server to collect and display application metrics.

---
