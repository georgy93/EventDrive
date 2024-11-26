#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Worker/EventDrive.Worker.Host/EventDrive.Worker.Host.csproj", "Worker/EventDrive.Worker.Host/"]
COPY ["WebApplication/Web/EventDrive.API/EventDrive.API.csproj", "WebApplication/Web/EventDrive.API/"]
COPY ["Common/EventDrive.Utils/EventDrive.Utils.csproj", "Common/EventDrive.Utils/"]
COPY ["WebApplication/Infrastructure/EventDrive.Infrastructure/EventDrive.Infrastructure.csproj", "WebApplication/Infrastructure/EventDrive.Infrastructure/"]
COPY ["Common/EventDrive.DTOs/EventDrive.DTOs.csproj", "Common/EventDrive.DTOs/"]
COPY ["Common/EventDrive.RabbitMq/EventDrive.RabbitMq.csproj", "Common/EventDrive.RabbitMq/"]
COPY ["Worker/EventDrive.Worker.Infrastructure/EventDrive.Worker.Infrastructure.csproj", "Worker/EventDrive.Worker.Infrastructure/"]
RUN dotnet restore "Worker/EventDrive.Worker.Host/EventDrive.Worker.Host.csproj"
COPY . .
WORKDIR "/src/Worker/EventDrive.Worker.Host"
RUN dotnet build "EventDrive.Worker.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventDrive.Worker.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventDrive.Worker.Host.dll"]