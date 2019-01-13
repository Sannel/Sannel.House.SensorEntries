# base
FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 443

# escape=`
#build the software
FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY . .
WORKDIR "/src/src/Sannel.House.SensorLogging.Tests"
RUN dotnet restore --configfile "../../.nuget/nuget.config" "Sannel.House.SensorLogging.Tests.csproj"
RUN dotnet build "Sannel.House.SensorLogging.Tests.csproj" -c Debug
RUN dotnet test /p:AltCover=true "Sannel.House.SensorLogging.Tests.csproj"
RUN dotnet tool install --global dotnet-reportgenerator-globaltool
ENV PATH="/usr/bin:/bin:/root/.dotnet/tools:;C:\Program Files\dotnet;C:\Users\ContainerUser\.dotnet\tools"
RUN reportgenerator "-reports:coverage.xml" "-targetdir:/coveragereport" "-assemblyfilters:-*Tests*;-*Migrations*" "-classfilters:-*Startup;-*Program"

#prepare the image
FROM sannel/staticfileserver AS final
COPY --from=build /coveragereport /app/wwwroot
