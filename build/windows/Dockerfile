ARG tag=2.2
FROM mcr.microsoft.com/dotnet/core/runtime:${tag} AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

COPY Sannel.House.SensorLogging/ .
USER ContainerUser
ENTRYPOINT ["dotnet", "Sannel.House.Devices.dll"]