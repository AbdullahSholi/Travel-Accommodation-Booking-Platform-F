FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Travel-Accommodation-Booking-Platform-F.Application ./Travel-Accommodation-Booking-Platform-F.Application
COPY Travel-Accommodation-Booking-Platform-F.Infrastructure ./Travel-Accommodation-Booking-Platform-F.Infrastructure
COPY Travel-Accommodation-Booking-Platform-F.Domain ./Travel-Accommodation-Booking-Platform-F.Domain
COPY Travel-Accommodation-Booking-Platform-F.API ./Travel-Accommodation-Booking-Platform-F.API

WORKDIR /src/Travel-Accommodation-Booking-Platform-F.API

RUN dotnet restore Travel-Accommodation-Booking-Platform-F.API.csproj
RUN dotnet publish Travel-Accommodation-Booking-Platform-F.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Travel-Accommodation-Booking-Platform-F.API.dll"]
