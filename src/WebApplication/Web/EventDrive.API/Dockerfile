#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WebApplication/Web/EventDrive.API/EventDrive.API.csproj", "WebApplication/Web/EventDrive.API/"]
COPY ["Common/EventDrive.Utils/EventDrive.Utils.csproj", "Common/EventDrive.Utils/"]
COPY ["WebApplication/Infrastructure/EventDrive.Infrastructure/EventDrive.Infrastructure.csproj", "WebApplication/Infrastructure/EventDrive.Infrastructure/"]
COPY ["Common/EventDrive.DTOs/EventDrive.DTOs.csproj", "Common/EventDrive.DTOs/"]
COPY ["Common/EventDrive.RabbitMq/EventDrive.RabbitMq.csproj", "Common/EventDrive.RabbitMq/"]
RUN dotnet restore "WebApplication/Web/EventDrive.API/EventDrive.API.csproj"
COPY . .
WORKDIR "/src/WebApplication/Web/EventDrive.API"
RUN dotnet build "EventDrive.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventDrive.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventDrive.API.dll"]