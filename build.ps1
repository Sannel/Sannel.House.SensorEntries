#!/usr/local/bin/pwsh
param(
	[switch]$DevicesOnly
)

$target = "";
if($DevicesOnly)
{
	$target = "devices"
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
	return docker-compose -f docker-compose.yml -f docker-compose.unix.yml build $target
}
else
{
	$env:USER="administrator"
	$env:SANNEL_ARCH="win"
	$env:SANNEL_VERSION=Get-Date -format yyMM.dd
	return docker-compose -f docker-compose.yml -f docker-compose.windows.yml build $target
}
