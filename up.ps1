#!/usr/local/bin/pwsh
param(
	[switch]$SensorLogging,
	[switch]$Devices,
	[Switch]$Users
)

$target = "";
if($SensorLogging)
{
	$target = "sensorlogging"
}
elseif($Devices)
{
	$target = "devices"
}
elseif($Users)
{
	$target = "users"
}

if($IsLinux -eq $true -or $IsMacOS -eq $true)
{
	$uname = uname -p
	if($uname -eq "x86_64" -or $uname -eq "i386")
	{
		$uname = "x86";
	}
	if($uname -eq "armv7l" -or $uname -eq "unknown") # for now assume unknown is arm
	{
		$uname = "arm";
	}

	$env:USER="root"
	$env:SANNEL_ARCH="linux-$uname"
	$env:SANNEL_VERSION=Get-Date -format yyMM.dd
	docker-compose -f docker-compose.yml -f docker-compose.unix.yml up $target
}
else
{
	$env:USER="administrator"
	$env:SANNEL_ARCH="win"
	$env:SANNEL_VERSION=Get-Date -format yyMM.dd
	docker-compose -f docker-compose.yml -f docker-compose.windows.yml up $target
}