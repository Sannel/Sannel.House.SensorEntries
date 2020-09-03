ARG tag=3.1
ARG project=Sannel.House.SensorLogging
FROM mcr.microsoft.com/dotnet/core/runtime:${tag} AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443
COPY qemu-arm-static /usr/bin/
COPY ${project}/ .
RUN ln -s  /var/run/secrets/kubernetes.io/serviceaccount/ca.crt /etc/ssl/certs/kubernetes.pem \
    && mkdir /app/app_data \
    && groupadd -g 1200 house \
    && useradd -m house -g house -u 1100 \
    && chown -R house:house /app \
    && rm -f /usr/bin/qemu-arm-static
USER 1200
ENTRYPOINT ["dotnet", "${project}.dll"]