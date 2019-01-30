#!/usr/local/bin/pwsh
param(
	[switch]$SensorOnly,
	[switch]$MainOnly
)

. "$PSScriptRoot/_common.ps1"

SetBuildType

$target = "";
if($DevicesOnly -or $MainOnly)
{
	$target = "sensorlogging";
}

# Pull latest images 
docker pull microsoft/dotnet:2.2-aspnetcore-runtime
docker pull microsoft/dotnet:2.2-sdk

CleanDevFiles

$version = GetVersion

return RunDockerCompose "build" $version $target


