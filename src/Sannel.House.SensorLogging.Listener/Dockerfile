#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY [".nuget/nuget.config", ".nuget/nuget.config"]
COPY ["src/Sannel.House.SensorLogging.Listener/Sannel.House.SensorLogging.Listener.csproj", "src/Sannel.House.SensorLogging.Listener/"]
COPY ["src/Sannel.House.SensorLogging.Interfaces/Sannel.House.SensorLogging.Interfaces.csproj", "src/Sannel.House.SensorLogging.Interfaces/"]
COPY ["src/Sannel.House.SensorLogging.Models/Sannel.House.SensorLogging.Models.csproj", "src/Sannel.House.SensorLogging.Models/"]
COPY ["src/Migrations/Sannel.House.SensorLogging.Data.Migrations.Sqlite/Sannel.House.SensorLogging.Data.Migrations.Sqlite.csproj", "src/Migrations/Sannel.House.SensorLogging.Data.Migrations.Sqlite/"]
COPY ["src/Sannel.House.SensorLogging.Data/Sannel.House.SensorLogging.Data.csproj", "src/Sannel.House.SensorLogging.Data/"]
COPY ["src/Migrations/Sannel.House.SensorLogging.Data.Migrations.PostgreSQL/Sannel.House.SensorLogging.Data.Migrations.PostgreSQL.csproj", "src/Migrations/Sannel.House.SensorLogging.Data.Migrations.PostgreSQL/"]
COPY ["src/Sannel.House.SensorLogging.Services/Sannel.House.SensorLogging.Services.csproj", "src/Sannel.House.SensorLogging.Services/"]
COPY ["src/Migrations/Sannel.House.SensorLogging.Data.Migrations.SqlServer/Sannel.House.SensorLogging.Data.Migrations.SqlServer.csproj", "src/Migrations/Sannel.House.SensorLogging.Data.Migrations.SqlServer/"]
COPY ["src/Sannel.House.SensorLogging.Repositories/Sannel.House.SensorLogging.Repositories.csproj", "src/Sannel.House.SensorLogging.Repositories/"]
RUN dotnet restore --configfile ".nuget/nuget.config" "src/Sannel.House.SensorLogging.Listener/Sannel.House.SensorLogging.Listener.csproj"
COPY . .
WORKDIR "/src/src/Sannel.House.SensorLogging.Listener"
RUN dotnet build "Sannel.House.SensorLogging.Listener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sannel.House.SensorLogging.Listener.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sannel.House.SensorLogging.Listener.dll"]