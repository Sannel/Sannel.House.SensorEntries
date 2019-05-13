
## Converts a SDDL string into an object-based representation of a security
## descriptor
function ConvertFrom-SddlString
{
    [CmdletBinding(HelpUri = "https://go.microsoft.com/fwlink/?LinkId=623636")]
    param(
        ## The string representing the security descriptor in SDDL syntax
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true)]
        [String] $Sddl,
        
        ## The type of rights that this SDDL string represents, if any.
        [Parameter()]
        [ValidateSet(
            "FileSystemRights", "RegistryRights", "ActiveDirectoryRights",
            "MutexRights", "SemaphoreRights", "CryptoKeyRights",
            "EventWaitHandleRights")]
        $Type
    )

    Begin
    {
        # On CoreCLR CryptoKeyRights and ActiveDirectoryRights are not supported.
        if ($PSEdition -eq "Core" -and ($Type -eq "CryptoKeyRights" -or $Type -eq "ActiveDirectoryRights"))
        {
            $errorId = "TypeNotSupported"
            $errorCategory = [System.Management.Automation.ErrorCategory]::InvalidArgument
            $errorMessage = [Microsoft.PowerShell.Commands.UtilityResources]::TypeNotSupported -f $Type
            $exception = [System.ArgumentException]::New($errorMessage)
            $errorRecord = [System.Management.Automation.ErrorRecord]::New($exception, $errorId, $errorCategory, $null)
            $PSCmdlet.ThrowTerminatingError($errorRecord)
        }

        ## Translates a SID into a NT Account
        function ConvertTo-NtAccount
        {
            param($Sid)

            if($Sid)
            {
                $securityIdentifier = [System.Security.Principal.SecurityIdentifier] $Sid
        
                try
                {
                    $ntAccount = $securityIdentifier.Translate([System.Security.Principal.NTAccount]).ToString()
                }
                catch{}

                $ntAccount
            }
        }

        ## Gets the access rights that apply to an access mask, preferring right types
        ## of 'Type' if specified.
        function Get-AccessRights
        {
            param($AccessMask, $Type)

            if ($PSEdition -eq "Core")
            {
                ## All the types of access rights understood by .NET Core
                $rightTypes = [Ordered] @{
                    "FileSystemRights" = [System.Security.AccessControl.FileSystemRights]
                    "RegistryRights" = [System.Security.AccessControl.RegistryRights]
                    "MutexRights" = [System.Security.AccessControl.MutexRights]
                    "SemaphoreRights" = [System.Security.AccessControl.SemaphoreRights]
                    "EventWaitHandleRights" = [System.Security.AccessControl.EventWaitHandleRights]
                }
            }
            else
            {
                ## All the types of access rights understood by .NET
                $rightTypes = [Ordered] @{
                    "FileSystemRights" = [System.Security.AccessControl.FileSystemRights]
                    "RegistryRights" = [System.Security.AccessControl.RegistryRights]
                    "ActiveDirectoryRights" = [System.DirectoryServices.ActiveDirectoryRights]
                    "MutexRights" = [System.Security.AccessControl.MutexRights]
                    "SemaphoreRights" = [System.Security.AccessControl.SemaphoreRights]
                    "CryptoKeyRights" = [System.Security.AccessControl.CryptoKeyRights]
                    "EventWaitHandleRights" = [System.Security.AccessControl.EventWaitHandleRights]
                }
            }
            $typesToExamine = $rightTypes.Values
        
            ## If they know the access mask represents a certain type, prefer its names
            ## (i.e.: CreateLink for the registry over CreateDirectories for the filesystem)
            if($Type)
            {
                $typesToExamine = @($rightTypes[$Type]) + $typesToExamine
            }
            
       
            ## Stores the access types we've found that apply
            $foundAccess = @()
        
            ## Store the access types we've already seen, so that we don't report access
            ## flags that are essentially duplicate. Many of the access values in the different
            ## enumerations have the same value but with different names.
            $foundValues = @{}

            ## Go through the entries in the different right types, and see if they apply to the
            ## provided access mask. If they do, then add that to the result.   
            foreach($rightType in $typesToExamine)
            {
                foreach($accessFlag in [Enum]::GetNames($rightType))
                {
                    $longKeyValue = [long] $rightType::$accessFlag
                    if(-not $foundValues.ContainsKey($longKeyValue))
                    {
                        $foundValues[$longKeyValue] = $true
                        if(($AccessMask -band $longKeyValue) -eq ($longKeyValue))
                        {
                            $foundAccess += $accessFlag
                        }
                    }
                }
            }

            $foundAccess | Sort-Object
        }

        ## Converts an ACE into a string representation
        function ConvertTo-AceString
        {
            param(
                [Parameter(ValueFromPipeline)]
                $Ace,
                $Type
            )

            process
            {
                foreach($aceEntry in $Ace)
                {
                    $AceString = (ConvertTo-NtAccount $aceEntry.SecurityIdentifier) + ": " + $aceEntry.AceQualifier
                    if($aceEntry.AceFlags -ne "None")
                    {
                        $AceString += " " + $aceEntry.AceFlags
                    }

                    if($aceEntry.AccessMask)
                    {
                        $foundAccess = Get-AccessRights $aceEntry.AccessMask $Type

                        if($foundAccess)
                        {
                            $AceString += " ({0})" -f ($foundAccess -join ", ")
                        }
                    }

                    $AceString
                }
            }
        }
    }

    Process
    {
        $rawSecurityDescriptor = [Security.AccessControl.CommonSecurityDescriptor]::new($false,$false,$Sddl)

        $owner = ConvertTo-NtAccount $rawSecurityDescriptor.Owner
        $group = ConvertTo-NtAccount $rawSecurityDescriptor.Group
        $discretionaryAcl = ConvertTo-AceString $rawSecurityDescriptor.DiscretionaryAcl $Type
        $systemAcl = ConvertTo-AceString $rawSecurityDescriptor.SystemAcl $Type

        [PSCustomObject] @{
            Owner = $owner
            Group = $group
            DiscretionaryAcl = @($discretionaryAcl)
            SystemAcl = @($systemAcl)
            RawDescriptor = $rawSecurityDescriptor
        }
    }
}

# SIG # Begin signature block
# MIIkHQYJKoZIhvcNAQcCoIIkDjCCJAoCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCBHtZSX/SSpG0iz
# aSdpvzkRnSdHsFLvLOqPhofkMy5iqaCCDYMwggYBMIID6aADAgECAhMzAAAAxOmJ
# +HqBUOn/AAAAAADEMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMTcwODExMjAyMDI0WhcNMTgwODExMjAyMDI0WjB0MQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMR4wHAYDVQQDExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQCIirgkwwePmoB5FfwmYPxyiCz69KOXiJZGt6PLX4kvOjMuHpF4+nypH4IBtXrL
# GrwDykbrxZn3+wQd8oUK/yJuofJnPcUnGOUoH/UElEFj7OO6FYztE5o13jhwVG87
# 7K1FCTBJwb6PMJkMy3bJ93OVFnfRi7uUxwiFIO0eqDXxccLgdABLitLckevWeP6N
# +q1giD29uR+uYpe/xYSxkK7WryvTVPs12s1xkuYe/+xxa8t/CHZ04BBRSNTxAMhI
# TKMHNeVZDf18nMjmWuOF9daaDx+OpuSEF8HWyp8dAcf9SKcTkjOXIUgy+MIkogCy
# vlPKg24pW4HvOG6A87vsEwvrAgMBAAGjggGAMIIBfDAfBgNVHSUEGDAWBgorBgEE
# AYI3TAgBBggrBgEFBQcDAzAdBgNVHQ4EFgQUy9ZihM9gOer/Z8Jc0si7q7fDE5gw
# UgYDVR0RBEswSaRHMEUxDTALBgNVBAsTBE1PUFIxNDAyBgNVBAUTKzIzMDAxMitj
# ODA0YjVlYS00OWI0LTQyMzgtODM2Mi1kODUxZmEyMjU0ZmMwHwYDVR0jBBgwFoAU
# SG5k5VAF04KqFzc3IrVtqMp1ApUwVAYDVR0fBE0wSzBJoEegRYZDaHR0cDovL3d3
# dy5taWNyb3NvZnQuY29tL3BraW9wcy9jcmwvTWljQ29kU2lnUENBMjAxMV8yMDEx
# LTA3LTA4LmNybDBhBggrBgEFBQcBAQRVMFMwUQYIKwYBBQUHMAKGRWh0dHA6Ly93
# d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY2VydHMvTWljQ29kU2lnUENBMjAxMV8y
# MDExLTA3LTA4LmNydDAMBgNVHRMBAf8EAjAAMA0GCSqGSIb3DQEBCwUAA4ICAQAG
# Fh/bV8JQyCNPolF41+34/c291cDx+RtW7VPIaUcF1cTL7OL8mVuVXxE4KMAFRRPg
# mnmIvGar27vrAlUjtz0jeEFtrvjxAFqUmYoczAmV0JocRDCppRbHukdb9Ss0i5+P
# WDfDThyvIsoQzdiCEKk18K4iyI8kpoGL3ycc5GYdiT4u/1cDTcFug6Ay67SzL1BW
# XQaxFYzIHWO3cwzj1nomDyqWRacygz6WPldJdyOJ/rEQx4rlCBVRxStaMVs5apao
# pIhrlihv8cSu6r1FF8xiToG1VBpHjpilbcBuJ8b4Jx/I7SCpC7HxzgualOJqnWmD
# oTbXbSD+hdX/w7iXNgn+PRTBmBSpwIbM74LBq1UkQxi1SIV4htD50p0/GdkUieeN
# n2gkiGg7qceATibnCCFMY/2ckxVNM7VWYE/XSrk4jv8u3bFfpENryXjPsbtrj4Ns
# h3Kq6qX7n90a1jn8ZMltPgjlfIOxrbyjunvPllakeljLEkdi0iHv/DzEMQv3Lz5k
# pTdvYFA/t0SQT6ALi75+WPbHZ4dh256YxMiMy29H4cAulO2x9rAwbexqSajplnbI
# vQjE/jv1rnM3BrJWzxnUu/WUyocc8oBqAU+2G4Fzs9NbIj86WBjfiO5nxEmnL9wl
# iz1e0Ow0RJEdvJEMdoI+78TYLaEEAo5I+e/dAs8DojCCB3owggVioAMCAQICCmEO
# kNIAAAAAAAMwDQYJKoZIhvcNAQELBQAwgYgxCzAJBgNVBAYTAlVTMRMwEQYDVQQI
# EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
# ZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29mdCBSb290IENlcnRpZmlj
# YXRlIEF1dGhvcml0eSAyMDExMB4XDTExMDcwODIwNTkwOVoXDTI2MDcwODIxMDkw
# OVowfjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcT
# B1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEoMCYGA1UE
# AxMfTWljcm9zb2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMTCCAiIwDQYJKoZIhvcN
# AQEBBQADggIPADCCAgoCggIBAKvw+nIQHC6t2G6qghBNNLrytlghn0IbKmvpWlCq
# uAY4GgRJun/DDB7dN2vGEtgL8DjCmQawyDnVARQxQtOJDXlkh36UYCRsr55JnOlo
# XtLfm1OyCizDr9mpK656Ca/XllnKYBoF6WZ26DJSJhIv56sIUM+zRLdd2MQuA3Wr
# aPPLbfM6XKEW9Ea64DhkrG5kNXimoGMPLdNAk/jj3gcN1Vx5pUkp5w2+oBN3vpQ9
# 7/vjK1oQH01WKKJ6cuASOrdJXtjt7UORg9l7snuGG9k+sYxd6IlPhBryoS9Z5JA7
# La4zWMW3Pv4y07MDPbGyr5I4ftKdgCz1TlaRITUlwzluZH9TupwPrRkjhMv0ugOG
# jfdf8NBSv4yUh7zAIXQlXxgotswnKDglmDlKNs98sZKuHCOnqWbsYR9q4ShJnV+I
# 4iVd0yFLPlLEtVc/JAPw0XpbL9Uj43BdD1FGd7P4AOG8rAKCX9vAFbO9G9RVS+c5
# oQ/pI0m8GLhEfEXkwcNyeuBy5yTfv0aZxe/CHFfbg43sTUkwp6uO3+xbn6/83bBm
# 4sGXgXvt1u1L50kppxMopqd9Z4DmimJ4X7IvhNdXnFy/dygo8e1twyiPLI9AN0/B
# 4YVEicQJTMXUpUMvdJX3bvh4IFgsE11glZo+TzOE2rCIF96eTvSWsLxGoGyY0uDW
# iIwLAgMBAAGjggHtMIIB6TAQBgkrBgEEAYI3FQEEAwIBADAdBgNVHQ4EFgQUSG5k
# 5VAF04KqFzc3IrVtqMp1ApUwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBDAEEwCwYD
# VR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAUci06AjGQQ7kU
# BU7h6qfHMdEjiTQwWgYDVR0fBFMwUTBPoE2gS4ZJaHR0cDovL2NybC5taWNyb3Nv
# ZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9vQ2VyQXV0MjAxMV8yMDExXzAz
# XzIyLmNybDBeBggrBgEFBQcBAQRSMFAwTgYIKwYBBQUHMAKGQmh0dHA6Ly93d3cu
# bWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2VyQXV0MjAxMV8yMDExXzAz
# XzIyLmNydDCBnwYDVR0gBIGXMIGUMIGRBgkrBgEEAYI3LgMwgYMwPwYIKwYBBQUH
# AgEWM2h0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvZG9jcy9wcmltYXJ5
# Y3BzLmh0bTBABggrBgEFBQcCAjA0HjIgHQBMAGUAZwBhAGwAXwBwAG8AbABpAGMA
# eQBfAHMAdABhAHQAZQBtAGUAbgB0AC4gHTANBgkqhkiG9w0BAQsFAAOCAgEAZ/KG
# pZjgVHkaLtPYdGcimwuWEeFjkplCln3SeQyQwWVfLiw++MNy0W2D/r4/6ArKO79H
# qaPzadtjvyI1pZddZYSQfYtGUFXYDJJ80hpLHPM8QotS0LD9a+M+By4pm+Y9G6XU
# tR13lDni6WTJRD14eiPzE32mkHSDjfTLJgJGKsKKELukqQUMm+1o+mgulaAqPypr
# WEljHwlpblqYluSD9MCP80Yr3vw70L01724lruWvJ+3Q3fMOr5kol5hNDj0L8giJ
# 1h/DMhji8MUtzluetEk5CsYKwsatruWy2dsViFFFWDgycScaf7H0J/jeLDogaZiy
# WYlobm+nt3TDQAUGpgEqKD6CPxNNZgvAs0314Y9/HG8VfUWnduVAKmWjw11SYobD
# HWM2l4bf2vP48hahmifhzaWX0O5dY0HjWwechz4GdwbRBrF1HxS+YWG18NzGGwS+
# 30HHDiju3mUv7Jf2oVyW2ADWoUa9WfOXpQlLSBCZgB/QACnFsZulP0V3HjXG0qKi
# n3p6IvpIlR+r+0cjgPWe+L9rt0uX4ut1eBrs6jeZeRhL/9azI2h15q/6/IvrC4Dq
# aTuv/DDtBEyO3991bWORPdGdVk5Pv4BXIqF4ETIheu9BCrE/+6jMpF3BoYibV3FW
# TkhFwELJm3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xghXwMIIV7AIBATCBlTB+MQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQDEx9NaWNy
# b3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDExAhMzAAAAxOmJ+HqBUOn/AAAAAADE
# MA0GCWCGSAFlAwQCAQUAoIHeMBkGCSqGSIb3DQEJAzEMBgorBgEEAYI3AgEEMBwG
# CisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMC8GCSqGSIb3DQEJBDEiBCDPaUpR
# UbsRox/zyW7qomuVoRQchYvx0/dOy0Dk5etZTzByBgorBgEEAYI3AgEMMWQwYqAg
# gB4AUABvAHcAZQByAFMAaABlAGwAbAAgAEMAbwByAGWhPoA8aHR0cDovL2Vkd2Vi
# L3NpdGVzL0lTU0VuZ2luZWVyaW5nL0VuZ0Z1bi9TaXRlUGFnZXMvSG9tZS5hc3B4
# MA0GCSqGSIb3DQEBAQUABIIBAG1w1c8AxyYEUnL2ZWnJja57kjDf7IHzruERrdgD
# 788F1VKKuBQTahmlr6jRYdZDprYV0dPi6axNv50KCsBhlqOXrhS0JSaEpgmL404R
# ng0wVENiqaTDlG/R4tAJAzGb+mkl0ix++0y9rYy/moNbV4Mkbgi7BLWdcsBshWc1
# thBoZN+ecBoC4omF4Q+qxcQQ8GJrc17m4kgxJcXbi7phMBUyb82RtwVKsKoh6F9p
# ASChX0jKd7UC180yl75JbRlmuDX8La6LL0VLm7KIPj01H228mntiSXfm9TIXue7L
# +mv0qhxLwssAtqJ1GjPOtrCjTNK48eAtx7lI1GK5wAJMjWOhghNKMIITRgYKKwYB
# BAGCNwMDATGCEzYwghMyBgkqhkiG9w0BBwKgghMjMIITHwIBAzEPMA0GCWCGSAFl
# AwQCAQUAMIIBPQYLKoZIhvcNAQkQAQSgggEsBIIBKDCCASQCAQEGCisGAQQBhFkK
# AwEwMTANBglghkgBZQMEAgEFAAQg4RVnXv4p8YG37HtzR2C0RgI4CmFrMIk9V2wC
# MC5uN94CBlsqm95qWhgTMjAxODA3MTAxOTExMDAuMDI5WjAHAgEBgAIB9KCBuaSB
# tjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcT
# B1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UE
# CxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNOOkI4RUMtMzBBNC03MTQ0
# MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNloIIOzTCCBnEw
# ggRZoAMCAQICCmEJgSoAAAAAAAIwDQYJKoZIhvcNAQELBQAwgYgxCzAJBgNVBAYT
# AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYD
# VQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29mdCBS
# b290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDEwMB4XDTEwMDcwMTIxMzY1NVoX
# DTI1MDcwMTIxNDY1NVowfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
# b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3Jh
# dGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAwggEi
# MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCpHQ28dxGKOiDs/BOX9fp/aZRr
# dFQQ1aUKAIKF++18aEssX8XD5WHCdrc+Zitb8BVTJwQxH0EbGpUdzgkTjnxhMFmx
# MEQP8WCIhFRDDNdNuDgIs0Ldk6zWczBXJoKjRQ3Q6vVHgc2/JGAyWGBG8lhHhjKE
# HnRhZ5FfgVSxz5NMksHEpl3RYRNuKMYa+YaAu99h/EbBJx0kZxJyGiGKr0tkiVBi
# sV39dx898Fd1rL2KQk1AUdEPnAY+Z3/1ZsADlkR+79BL/W7lmsqxqPJ6Kgox8NpO
# BpG2iAg16HgcsOmZzTznL0S6p/TcZL2kAcEgCZN4zfy8wMlEXV4WnAEFTyJNAgMB
# AAGjggHmMIIB4jAQBgkrBgEEAYI3FQEEAwIBADAdBgNVHQ4EFgQU1WM6XIoxkPND
# e3xGG8UzaFqFbVUwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBDAEEwCwYDVR0PBAQD
# AgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAU1fZWy4/oolxiaNE9lJBb
# 186aGMQwVgYDVR0fBE8wTTBLoEmgR4ZFaHR0cDovL2NybC5taWNyb3NvZnQuY29t
# L3BraS9jcmwvcHJvZHVjdHMvTWljUm9vQ2VyQXV0XzIwMTAtMDYtMjMuY3JsMFoG
# CCsGAQUFBwEBBE4wTDBKBggrBgEFBQcwAoY+aHR0cDovL3d3dy5taWNyb3NvZnQu
# Y29tL3BraS9jZXJ0cy9NaWNSb29DZXJBdXRfMjAxMC0wNi0yMy5jcnQwgaAGA1Ud
# IAEB/wSBlTCBkjCBjwYJKwYBBAGCNy4DMIGBMD0GCCsGAQUFBwIBFjFodHRwOi8v
# d3d3Lm1pY3Jvc29mdC5jb20vUEtJL2RvY3MvQ1BTL2RlZmF1bHQuaHRtMEAGCCsG
# AQUFBwICMDQeMiAdAEwAZQBnAGEAbABfAFAAbwBsAGkAYwB5AF8AUwB0AGEAdABl
# AG0AZQBuAHQALiAdMA0GCSqGSIb3DQEBCwUAA4ICAQAH5ohRDeLG4Jg/gXEDPZ2j
# oSFvs+umzPUxvs8F4qn++ldtGTCzwsVmyWrf9efweL3HqJ4l4/m87WtUVwgrUYJE
# Evu5U4zM9GASinbMQEBBm9xcF/9c+V4XNZgkVkt070IQyK+/f8Z/8jd9Wj8c8pl5
# SpFSAK84Dxf1L3mBZdmptWvkx872ynoAb0swRCQiPM/tA6WWj1kpvLb9BOFwnzJK
# J/1Vry/+tuWOM7tiX5rbV0Dp8c6ZZpCM/2pif93FSguRJuI57BlKcWOdeyFtw5yj
# ojz6f32WapB4pm3S4Zz5Hfw42JT0xqUKloakvZ4argRCg7i1gJsiOCC1JeVk7Pf0
# v35jWSUPei45V3aicaoGig+JFrphpxHLmtgOR5qAxdDNp9DvfYPw4TtxCd9ddJgi
# CGHasFAeb73x4QDf5zEHpJM692VHeOj4qEir995yfmFrb3epgcunCaw5u+zGy9iC
# tHLNHfS4hQEegPsbiSpUObJb2sgNVZl6h3M7COaYLeqN4DMuEin1wC9UJyH3yKxO
# 2ii4sanblrKnQqLJzxlBTeCG+SqaoxFmMNO7dDJL32N79ZmKLxvHIa9Zta7cRDyX
# UHHXodLFVeNp3lfB0d4wwP3M5k37Db9dT+mdHhk4L7zPWAUu7w2gUDXa7wknHNWz
# fjUeCLraNtvTX4/edIhJEjCCBNowggPCoAMCAQICEzMAAACfZ/K1qCMGW3sAAAAA
# AJ8wDQYJKoZIhvcNAQELBQAwfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAw
# HhcNMTYwOTA3MTc1NjQ3WhcNMTgwOTA3MTc1NjQ3WjCBszELMAkGA1UEBhMCVVMx
# EzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoT
# FU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMe
# bkNpcGhlciBEU0UgRVNOOkI4RUMtMzBBNC03MTQ0MSUwIwYDVQQDExxNaWNyb3Nv
# ZnQgVGltZS1TdGFtcCBTZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIB
# CgKCAQEAuQjxI5zdxAIvAoWhoyeXZPkDnBJUP1OCWrg+631uGMVywSfVcCkM8JZL
# 1o+ExxY5Yp77sQ0jhKLjMPfSdVAL09nQ0O76kr1dXzc5+MZyEWQrM4FF106GmxCT
# EWAwXdF8tM1cASp9+c1pF5fC1VSSIYQm9boqYAGLHM/Rp5RWYnowecmeaj5Mpl2h
# WXtyDpNjosKjN78XquE5eaL8/df8reMe2YBrEv067neOMOA7lGPG3pkRqZ0SwYXZ
# JZnrAfoOaD0bqJk/GDD6aM4PBF4vqPCHsfZeGy/OgUytIREzMgh/Z4kYAz0LQZHQ
# FkfJG2LXtCovlNoK5Y+MzFMpdfgOWQIDAQABo4IBGzCCARcwHQYDVR0OBBYEFP2L
# GyLDfSNHdqYe3+Bm1FLptvmgMB8GA1UdIwQYMBaAFNVjOlyKMZDzQ3t8RhvFM2ha
# hW1VMFYGA1UdHwRPME0wS6BJoEeGRWh0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9w
# a2kvY3JsL3Byb2R1Y3RzL01pY1RpbVN0YVBDQV8yMDEwLTA3LTAxLmNybDBaBggr
# BgEFBQcBAQROMEwwSgYIKwYBBQUHMAKGPmh0dHA6Ly93d3cubWljcm9zb2Z0LmNv
# bS9wa2kvY2VydHMvTWljVGltU3RhUENBXzIwMTAtMDctMDEuY3J0MAwGA1UdEwEB
# /wQCMAAwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZIhvcNAQELBQADggEBAGUQ
# wWxrzxUerw9INuvfLQu8AADmkWYaUJZluTEPZYyp8XTLx+eW+BvzvjPyzPxBnMHI
# KZjWMfIdNz3xl6TPsvZjlIA1QhryPJTfpzrgKTl9jo972FQDVEb/XM/56rTzRyFQ
# 8IXbN7OF/C7P05vShs7rgHBbQZmBhjPWGOyr4MGRIIFFXn2vIWnOApHCFYXyq5e0
# cOmKaInH52zZVlLARWT9BFjuku5S9503w/kM24tppHDeglyzZbGHaNZLlPxjcl69
# SjcrdVO0c+LYgFYhKQQbtM6c0RRxRcMwZI55nbuS48XMqQNVu3O/ARV6mQauxnVb
# 7XG4Ng9DVvcEwbwLv0ehggN2MIICXgIBATCB46GBuaSBtjCBszELMAkGA1UEBhMC
# VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
# BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UE
# CxMebkNpcGhlciBEU0UgRVNOOkI4RUMtMzBBNC03MTQ0MSUwIwYDVQQDExxNaWNy
# b3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNloiUKAQEwCQYFKw4DAhoFAAMVAGzTJwjy
# +dmoy/kZ3pJLSq3bGaPBoIHCMIG/pIG8MIG5MQswCQYDVQQGEwJVUzETMBEGA1UE
# CBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9z
# b2Z0IENvcnBvcmF0aW9uMQ0wCwYDVQQLEwRNT1BSMScwJQYDVQQLEx5uQ2lwaGVy
# IE5UUyBFU046NTdGNi1DMUUwLTU1NEMxKzApBgNVBAMTIk1pY3Jvc29mdCBUaW1l
# IFNvdXJjZSBNYXN0ZXIgQ2xvY2swDQYJKoZIhvcNAQEFBQACBQDe7yPEMCIYDzIw
# MTgwNzEwMTIyMzMyWhgPMjAxODA3MTExMjIzMzJaMHQwOgYKKwYBBAGEWQoEATEs
# MCowCgIFAN7vI8QCAQAwBwIBAAICCvowBwIBAAICGv8wCgIFAN7wdUQCAQAwNgYK
# KwYBBAGEWQoEAjEoMCYwDAYKKwYBBAGEWQoDAaAKMAgCAQACAxbjYKEKMAgCAQAC
# AwehIDANBgkqhkiG9w0BAQUFAAOCAQEAIHleQhjTV4v4Fo8oMBpFChrLq1IAn50R
# t4CkYtXdJwkhJeW+HNQDwGkBOtzkxlFVK17DB+PthrybgyEITkkJoa720cbU4jl+
# cYs1q37YIu7KvThyhXLwkptwayca5vz6Fih8T3r87lFbphjzjjqCt37s3lol0dlQ
# T4OrBBlUs+itZ6cag15zh3JayOjZprITyDnPmvONWRunLfzUp9epu5/O1t+zm1vK
# vM2JDZgu6sgcryQEc/OwPbdE2Bu0VNBL5vF5MjbS0W/7hopb3UgPuam/dIovOeFT
# OVDA3S2P0+60IHRjrM2ZibTwLsHJlnCpL8qeWkw8+p/g1e5TqVhJ4DGCAvUwggLx
# AgEBMIGTMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYD
# VQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAk
# BgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAyMDEwAhMzAAAAn2fytagj
# Blt7AAAAAACfMA0GCWCGSAFlAwQCAQUAoIIBMjAaBgkqhkiG9w0BCQMxDQYLKoZI
# hvcNAQkQAQQwLwYJKoZIhvcNAQkEMSIEIMd1NL8xVmmGlWWOA+zf3EjY5v2M7WTW
# FlrNDEyQAjgGMIHiBgsqhkiG9w0BCRACDDGB0jCBzzCBzDCBsQQUbNMnCPL52ajL
# +RnekktKrdsZo8EwgZgwgYCkfjB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
# aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
# cnBvcmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAx
# MAITMwAAAJ9n8rWoIwZbewAAAAAAnzAWBBTCDuzhmLxCIaOXjDXVQIJ8ZcTqvDAN
# BgkqhkiG9w0BAQsFAASCAQCMY1Bi3iSr1ak6fqFTNDvmKUNh4QCYjBuIi3Uy3WF6
# yVci9JmjCit7Buibt233uJyZQKM1e/O8rdrY86l43T1iZ40OyFshX3tpJTwfz7sv
# Zs3NpkqnDRiDlA8YXpeu5bUbb9BdBJkVyobJ25mkX5G28NC1KIMvcjxBW3T2dRAv
# x6qJPn6OQYwHxK5F3kgEnGmYhoYHOziXhhAUepoiWP4EsLlo1GeN24XpXUlsttob
# OnHv0znOu0epi5nU1Nxv7HZrD9UYII8H1B99IjEuetKAVQmuPDZnWNXe+eWajKiY
# WyRiuvKkG+27wCAJPH2pKVMxgvbn2KsIiKdDUJToUx6z
# SIG # End signature block
