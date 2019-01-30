function SetBuildType
{
	$branch = $env:BUILD_SOURCEBRANCHNAME;
	$buildType = "local";

	if($branch -eq "develop")
	{
		$buildType = "beta";
	}
	elseif($branch -eq "master")
	{
		$buildType = "release";
	}

	$env:BuildType = $buildType;
	return $buildType;
}

function GetImageName
{
	$name = $env:ImageName
	if($null -eq $name)
	{
		$name = (Get-Item ..).Name.ToLower() -replace "\.",""
		$env:ImageName = $name
	}

	return $name
}

function TryLogin()
{
	if($null -ne $env:docker_user)
	{
		docker login -u $env:docker_user -p $env:docker_password
	}
}

function GetVersion
{
	$version = $env:BUILD_BUILDNUMBER;

	if($null -eq $version -or $version -eq "")
	{
		$version = (Get-Date -Format yyMMdd);
	}

	return $version;
}

function GetArchString
{
	if($IsLinux -eq $true -or $IsMacOS -eq $true)
	{
		$uname = uname -m
		if($uname -eq "x86_64")
		{
			$uname = "x64";
		}
		if($uname -eq "armv7l") 
		{
			if([Environment]::Is64BitOperatingSystem)
			{
				$uname = "arm64";
			}
			else
			{
				$uname = "arm32";
			}
		}
		return "linux-$uname"
	}
	else 
	{
		if([Environment]::Is64BitOperatingSystem)
		{
			return "win-x64";
		}
		else
		{
			return "win-x86";	
		}
	}
}

function SetDockerComposeVariables
{
	param(
		[string]$version
	)
	if($IsLinux -eq $true -or $IsMacOS -eq $true)
	{
		$env:USER="root"
		$env:SANNEL_ARCH=GetArchString
		$env:SANNEL_VERSION=$version
	}
	else
	{
		Write-Host "Windows"
		$env:USER="administrator"
		$env:SANNEL_ARCH=GetArchString
		$env:SANNEL_VERSION=$version
	}
}

function RunDockerCompose
{
	param(
		[string]$type,
		[string]$version,
		[string]$target,
		[string]$options=""
	)

	SetDockerComposeVariables $version

	if($IsLinux -eq $true -or $IsMacOS -eq $true)
	{
		return docker-compose -f "$PSScriptRoot/../docker-compose.yml" -f "$PSScriptRoot/../docker-compose.unix.yml" $type $options $target | Write-Host
	}
	else
	{
		return docker-compose -f "$PSScriptRoot/../docker-compose.yml" -f "$PSScriptRoot/../docker-compose.windows.yml" $type $options $target | Write-Host
	}

}

function CleanDevFiles
{
	Remove-Item -ErrorAction SilentlyContinue src/Sannel.House.Devices/app_data/data.db
}