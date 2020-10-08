ARG tag=5.0
FROM mcr.microsoft.com/dotnet/runtime:${tag} AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443
COPY ${project}/ .
RUN ln -s  /var/run/secrets/kubernetes.io/serviceaccount/ca.crt /etc/ssl/certs/kubernetes.pem \
    && addgroup -g 1200 house \
    && adduser -u 1200 -G house -D house \
    && chown -R house:house /app
USER 1200
ENTRYPOINT ["dotnet", "${project}.dll"]