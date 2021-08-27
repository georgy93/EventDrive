#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Worker/EventDrive.Worker.Host/EventDrive.Worker.Host.csproj", "Worker/EventDrive.Worker.Host/"]
COPY ["Common/EventDrive.Utils/EventDrive.Utils.csproj", "Common/EventDrive.Utils/"]
COPY ["Worker/EventDrive.Worker.Infrastructure/EventDrive.Worker.Infrastructure.csproj", "Worker/EventDrive.Worker.Infrastructure/"]
COPY ["Common/EventDrive.RabbitMq/EventDrive.RabbitMq.csproj", "Common/EventDrive.RabbitMq/"]
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
