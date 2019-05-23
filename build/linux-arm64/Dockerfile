ARG tag=2.2
FROM mcr.microsoft.com/dotnet/core/runtime:${tag} AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443
COPY  qemu-aarch64-static /usr/bin/
COPY Sannel.House.SensorLogging/ .
RUN useradd -m house -u 1100 && chown -R house /app && rm -f /usr/bin/qemu-aarch64-static
USER house
ENTRYPOINT ["dotnet", "Sannel.House.Devices.dll"]