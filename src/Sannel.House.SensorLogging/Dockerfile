#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY [".nuget/nuget.config", ".nuget/nuget.config"]
COPY ["Directory.Build.props", "./"]
COPY ["src/Sannel.House.SensorLogging/Sannel.House.SensorLogging.csproj", "src/Sannel.House.SensorLogging/"]
COPY ["src/Sannel.House.SensorLogging.Interfaces/Sannel.House.SensorLogging.Interfaces.csproj", "src/Sannel.House.SensorLogging.Interfaces/"]
COPY ["src/Sannel.House.SensorLogging.Models/Sannel.House.SensorLogging.Models.csproj", "src/Sannel.House.SensorLogging.Models/"]
COPY ["src/Migrations/Sannel.House.SensorLogging.Data.Migrations.Sqlite/Sannel.House.SensorLogging.Data.Migrations.Sqlite.csproj", "src/Migrations/Sannel.House.SensorLogging.Data.Migrations.Sqlite/"]
COPY ["src/Sannel.House.SensorLogging.Data/Sannel.House.SensorLogging.Data.csproj", "src/Sannel.House.SensorLogging.Data/"]
COPY ["src/Migrations/Sannel.House.SensorLogging.Data.Migrations.MySql/Sannel.House.SensorLogging.Data.Migrations.MySql.csproj", "src/Migrations/Sannel.House.SensorLogging.Data.Migrations.MySql/"]
COPY ["src/Migrations/Sannel.House.SensorLogging.Data.Migrations.SqlServer/Sannel.House.SensorLogging.Data.Migrations.SqlServer.csproj", "src/Migrations/Sannel.House.SensorLogging.Data.Migrations.SqlServer/"]
COPY ["src/Sannel.House.SensorLogging.Repositories/Sannel.House.SensorLogging.Repositories.csproj", "src/Sannel.House.SensorLogging.Repositories/"]
RUN dotnet restore --configfile ".nuget/nuget.config" "src/Sannel.House.SensorLogging/Sannel.House.SensorLogging.csproj"
COPY . .
WORKDIR "/src/src/Sannel.House.SensorLogging"
RUN dotnet build "Sannel.House.SensorLogging.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Sannel.House.SensorLogging.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY --from=build /src/src/Sannel.House.SensorLogging/app_config /app/app_config
COPY --from=build /src/src/Sannel.House.SensorLogging/app_data /app/app_data
ARG USER=root
USER ${USER}
ENTRYPOINT ["dotnet", "Sannel.House.SensorLogging.dll"]