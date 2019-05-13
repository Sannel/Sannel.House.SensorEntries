function Convert-PfxToPem {
<#
.ExternalHelp PSPKI.Help.xml
#>
[CmdletBinding(DefaultParameterSetName = '__pfxfile')]
    param(
        [Parameter(Mandatory = $true, ParameterSetName = '__pfxfile', Position = 0)]
        [IO.FileInfo]$InputFile,
        [Parameter(Mandatory = $true, ParameterSetName = '__cert', Position = 0)]
        [Security.Cryptography.X509Certificates.X509Certificate2]$Certificate,
        [Parameter(Mandatory = $true, ParameterSetName = '__pfxfile', Position = 1)]
        [Security.SecureString]$Password,
        [Parameter(Mandatory = $true, Position = 2)]
        [IO.FileInfo]$OutputFile,
        [Parameter(Position = 3)]
        [ValidateSet("Pkcs1","Pkcs8")]
        [string]$OutputType = "Pkcs8",
		[switch]$IncludeChain
    )
$signature = @"
[DllImport("crypt32.dll", CharSet=CharSet.Auto, SetLastError=true)]
public static extern bool CryptAcquireCertificatePrivateKey(
    IntPtr pCert,
    uint dwFlags,
    IntPtr pvReserved,
    ref IntPtr phCryptProv,
    ref uint pdwKeySpec,
    ref bool pfCallerFreeProv
);
[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
public static extern bool CryptGetUserKey(
    IntPtr hProv,
    uint dwKeySpec,
    ref IntPtr phUserKey
);
[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
public static extern bool CryptExportKey(
    IntPtr hKey,
    IntPtr hExpKey,
    uint dwBlobType,
    uint dwFlags,
    byte[] pbData,
    ref uint pdwDataLen
);
[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
public static extern bool CryptDestroyKey(
    IntPtr hKey
);
[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
public static extern bool PFXIsPFXBlob(
    CRYPTOAPI_BLOB pPFX
);
[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
public static extern bool PFXVerifyPassword(
    CRYPTOAPI_BLOB pPFX,
    [MarshalAs(UnmanagedType.LPWStr)]
    string szPassword,
    int dwFlags
);
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct CRYPTOAPI_BLOB {
    public int cbData;
    public IntPtr pbData;
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct PUBKEYBLOBHEADERS {
    public byte bType;
    public byte bVersion;
    public short reserved;
    public uint aiKeyAlg;
    public uint magic;
    public uint bitlen;
    public uint pubexp;
 }
"@
    Add-Type -MemberDefinition $signature -Namespace PKI -Name PfxTools
#region helper functions
    function Encode-Integer ([Byte[]]$RawData) {
        # since CryptoAPI is little-endian by nature, we have to change byte ordering
        # to big-endian.
        [array]::Reverse($RawData)
        # if high byte contains more than 7 bits, an extra zero byte is added
        if ($RawData[0] -ge 128) {$RawData = ,0 + $RawData}
        [SysadminsLV.Asn1Parser.Asn1Utils]::Encode($RawData, 2)
    }
#endregion

#region parameterset processing
    switch ($PsCmdlet.ParameterSetName) {
        "__pfxfile" {
            $bytes = [IO.File]::ReadAllBytes($InputFile)
            $ptr = [Runtime.InteropServices.Marshal]::AllocHGlobal($bytes.Length)
            [Runtime.InteropServices.Marshal]::Copy($bytes,0,$ptr,$bytes.Length)
            $pfx = New-Object PKI.PfxTools+CRYPTOAPI_BLOB -Property @{
                cbData = $bytes.Length;
                pbData = $ptr
            }
            # just check whether input file is valid PKCS#12/PFX file.
            if ([PKI.PfxTools]::PFXIsPFXBlob($pfx)) {
				$certs = New-Object Security.Cryptography.X509Certificates.X509Certificate2Collection
				try {
					$certs.Import(
						$bytes,
						[Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)),
						"Exportable"
					)
					$Certificate = ($certs | Where-Object {$_.HasPrivateKey})[0]
				} catch {
					throw $_
					return
				} finally {
                    [Runtime.InteropServices.Marshal]::FreeHGlobal($ptr)
                    Remove-Variable bytes, ptr, pfx -Force
                }
            } else {
                [Runtime.InteropServices.Marshal]::FreeHGlobal($ptr)
                Remove-Variable bytes, ptr, pfx -Force
                Write-Error -Category InvalidData -Message "Input file is not valid PKCS#12/PFX file." -ErrorAction Stop
            }
        }
        "__cert" {
            if (!$Certificate.HasPrivateKey) {
                Write-Error -Category InvalidOperation -Message "Specified certificate object does not contain associated private key." -ErrorAction Stop
            }
        }
    }
#endregion

#region constants
	$CRYPT_ACQUIRE_SILENT_FLAG = 0x40
	$PRIVATEKEYBLOB = 0x7
	$CRYPT_OAEP = 0x40
#endregion

#region private key export routine
    $phCryptProv = [IntPtr]::Zero
    $pdwKeySpec = 0
    $pfCallerFreeProv = $false
    # attempt to acquire private key container
    if (![PKI.PfxTools]::CryptAcquireCertificatePrivateKey($Certificate.Handle,$CRYPT_ACQUIRE_SILENT_FLAG,0,[ref]$phCryptProv,[ref]$pdwKeySpec,[ref]$pfCallerFreeProv)) {
		throw New-Object ComponentModel.Win32Exception ([Runtime.InteropServices.Marshal]::GetLastWin32Error())
		return
	}
	$phUserKey = [IntPtr]::Zero
	# attempt to acquire private key handle
	if (![PKI.PfxTools]::CryptGetUserKey($phCryptProv,$pdwKeySpec,[ref]$phUserKey)) {
		throw New-Object ComponentModel.Win32Exception ([Runtime.InteropServices.Marshal]::GetLastWin32Error())
		return
	}
	$pdwDataLen = 0
	# attempt to export private key. This method fails if certificate has non-exportable private key.
	if (![PKI.PfxTools]::CryptExportKey($phUserKey,0,$PRIVATEKEYBLOB,$CRYPT_OAEP,$null,[ref]$pdwDataLen)) {
		throw New-Object ComponentModel.Win32Exception ([Runtime.InteropServices.Marshal]::GetLastWin32Error())
		return
	}
	$pbytes = New-Object byte[] -ArgumentList $pdwDataLen
	[void][PKI.PfxTools]::CryptExportKey($phUserKey,0,$PRIVATEKEYBLOB,$CRYPT_OAEP,$pbytes,[ref]$pdwDataLen)
	# release private key handle
	[void][PKI.PfxTools]::CryptDestroyKey($phUserKey)
#endregion

#region private key blob splitter
    # extracting private key blob header.
    $headerblob = $pbytes[0..19]
    # extracting actual private key data exluding header.
    $keyblob = $pbytes[20..($pbytes.Length - 1)]
    Remove-Variable pbytes -Force
    # public key structure header has fixed length: 20 bytes: http://msdn.microsoft.com/en-us/library/aa387689(VS.85).aspx
    # copy header information to unmanaged memory and copy it to structure.
    $ptr = [Runtime.InteropServices.Marshal]::AllocHGlobal(20)
    [Runtime.InteropServices.Marshal]::Copy($headerblob,0,$ptr,20)
    $header = [Runtime.InteropServices.Marshal]::PtrToStructure($ptr,[Type][PKI.PfxTools+PUBKEYBLOBHEADERS])
    [Runtime.InteropServices.Marshal]::FreeHGlobal($ptr)
    # extract public exponent from blob header and convert it to a byte array
    $pubExponentHex = "{0:x2}" -f $header.pubexp
    if ($pubExponentHex.Length % 2) {$pubExponentHex = "0" + $pubExponentHex}
    $publicExponent = $pubExponentHex -split "([a-f0-9]{2})" | Where-Object {$_} | ForEach-Object {[Convert]::ToByte($_,16)}
    # this object is created to reduce code size. This object has properties, where each property represents
    # a part (component) of the private key and property value contains private key component length.
    # 8 means that the length of the component is KeyLength / 8. Resulting length is measured in bytes.
    # for details see private key structure description: http://msdn.microsoft.com/en-us/library/aa387689(VS.85).aspx
    $obj = New-Object psobject -Property @{
        modulus = 8; privateExponent = 8;
        prime1 = 16; prime2 = 16; exponent1 = 16; exponent2 = 16; coefficient = 16;
    }
    $offset = 0
    # I pass variable names (each name represents the component of the private key) to foreach loop
    # in the order as they follow in the private key structure and parse private key for
    # appropriate offsets and write component information to variable.
    "modulus","prime1","prime2","exponent1","exponent2","coefficient","privateExponent" | ForEach-Object {
        Set-Variable -Name $_ -Value ($keyblob[$offset..($offset + $header.bitlen / $obj.$_ - 1)])
        $offset = $offset + $header.bitlen / $obj.$_
    }
    # PKCS#1/PKCS#8 uses slightly different component order, therefore I reorder private key
    # components and pass them to a simplified ASN encoder.
    $asnblob = Encode-Integer 0
    $asnblob += "modulus","publicExponent","privateExponent","prime1","prime2","exponent1","exponent2","coefficient" | ForEach-Object {
        Encode-Integer (Get-Variable -Name $_).Value
    }
    # remove unused variables
    Remove-Variable modulus,publicExponent,privateExponent,prime1,prime2,exponent1,exponent2,coefficient -Force
    # encode resulting set of INTEGERs to a SEQUENCE
    $asnblob = [SysadminsLV.Asn1Parser.Asn1Utils]::Encode($asnblob, 48)
    # $out variable just holds output file. The file will contain private key and public certificate
    # each will be enclosed with header and footer.
	$out = New-Object Text.StringBuilder
    if ($OutputType -eq "Pkcs8") {
        $asnblob = [SysadminsLV.Asn1Parser.Asn1Utils]::Encode($asnblob, 4)
        $algid = [Security.Cryptography.CryptoConfig]::EncodeOID("1.2.840.113549.1.1.1") + 5,0
        $algid = [SysadminsLV.Asn1Parser.Asn1Utils]::Encode($algid, 48)
        $asnblob = 2,1,0 + $algid + $asnblob
        $asnblob = [SysadminsLV.Asn1Parser.Asn1Utils]::Encode($asnblob, 48)
		$base64 = [SysadminsLV.Asn1Parser.AsnFormatter]::BinaryToString($asnblob,"Base64").Trim()
		[void]$out.AppendFormat("{0}{1}", "-----BEGIN PRIVATE KEY-----", [Environment]::NewLine)
		[void]$out.AppendFormat("{0}{1}", $base64, [Environment]::NewLine)
		[void]$out.AppendFormat("{0}{1}", "-----END PRIVATE KEY-----", [Environment]::NewLine)
    } else {
        # PKCS#1 requires RSA identifier in the header.
        # PKCS#1 is an inner structure of PKCS#8 message, therefore no additional encodings are required.
		$base64 = [SysadminsLV.Asn1Parser.AsnFormatter]::BinaryToString($asnblob,"Base64").Trim()
		[void]$out.AppendFormat("{0}{1}", "-----BEGIN RSA PRIVATE KEY-----", [Environment]::NewLine)
		[void]$out.AppendFormat("{0}{1}", $base64, [Environment]::NewLine)
		[void]$out.AppendFormat("{0}{1}", "-----END RSA PRIVATE KEY-----", [Environment]::NewLine)
    }
    $base64 = [SysadminsLV.Asn1Parser.AsnFormatter]::BinaryToString($Certificate.RawData,"Base64Header")
	$out.Append($base64)
	if ($IncludeChain) {
		$chain = New-Object Security.Cryptography.X509Certificates.X509Chain
		$chain.ChainPolicy.RevocationMode = "NoCheck"
		if ($certs) {
			$chain.ChainPolicy.ExtraStore.AddRange($certs)
		}
		[void]$chain.Build($Certificate)
		for ($n = 1; $n -lt $chain.ChainElements.Count; $n++) {
			$base64 = [SysadminsLV.Asn1Parser.AsnFormatter]::BinaryToString($chain.ChainElements[$n].Certificate.RawData,"Base64Header")
			$out.Append($base64)
		}
	}
    [IO.File]::WriteAllLines($OutputFile,$out.ToString())
#endregion
}
# SIG # Begin signature block
# MIIcgAYJKoZIhvcNAQcCoIIccTCCHG0CAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCC3M9HxcEr6xez6
# SDJ0wWfBLj3Lb9a8lRhGqlmkXrYaKqCCF4owggUTMIID+6ADAgECAhAJwnVp5a70
# RHscglFEfEqLMA0GCSqGSIb3DQEBCwUAMHIxCzAJBgNVBAYTAlVTMRUwEwYDVQQK
# EwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5jb20xMTAvBgNV
# BAMTKERpZ2lDZXJ0IFNIQTIgQXNzdXJlZCBJRCBDb2RlIFNpZ25pbmcgQ0EwHhcN
# MTcwNDE3MDAwMDAwWhcNMjAwNDIxMTIwMDAwWjBQMQswCQYDVQQGEwJMVjENMAsG
# A1UEBxMEUmlnYTEYMBYGA1UEChMPU3lzYWRtaW5zIExWIElLMRgwFgYDVQQDEw9T
# eXNhZG1pbnMgTFYgSUswggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCo
# NCCuzEogktL+1+lvPHu7ctNtCD7wA5Nalebh0FaKz3v1944APtg7A5oQfh6c20f7
# xYyTw4wVuo6L6S3dlMUa+bfXvTXIco0ilTIz0uqUKW8WGYwJtbFpu6PvCs0LHDRD
# rD8sEFgGHQhbz+J4gtV8BI7OID+yNfgbUk4JeSBGNzgeqZMdf/xceMoLx+fHi9tU
# OdTtgs/dXQYg3M3J+rGxFdpxOO7JmUZ8nqVALlnU9cHBGKUY4hDvDxfp7EukhnHv
# RpkhacZB1RBw0q8q+ekvLVCZwpG4N1Pnq2ksHiBzqRWQQE89iV+UwgRnLx2igywk
# 2kX+JPSZYsQCbDGo4DqBAgMBAAGjggHFMIIBwTAfBgNVHSMEGDAWgBRaxLl7Kgqj
# pepxA8Bg+S32ZXUOWDAdBgNVHQ4EFgQU9Mh+66y4uf1WQl9FmsWMHdk2HrswDgYD
# VR0PAQH/BAQDAgeAMBMGA1UdJQQMMAoGCCsGAQUFBwMDMHcGA1UdHwRwMG4wNaAz
# oDGGL2h0dHA6Ly9jcmwzLmRpZ2ljZXJ0LmNvbS9zaGEyLWFzc3VyZWQtY3MtZzEu
# Y3JsMDWgM6Axhi9odHRwOi8vY3JsNC5kaWdpY2VydC5jb20vc2hhMi1hc3N1cmVk
# LWNzLWcxLmNybDBMBgNVHSAERTBDMDcGCWCGSAGG/WwDATAqMCgGCCsGAQUFBwIB
# FhxodHRwczovL3d3dy5kaWdpY2VydC5jb20vQ1BTMAgGBmeBDAEEATCBhAYIKwYB
# BQUHAQEEeDB2MCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5kaWdpY2VydC5jb20w
# TgYIKwYBBQUHMAKGQmh0dHA6Ly9jYWNlcnRzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2Vy
# dFNIQTJBc3N1cmVkSURDb2RlU2lnbmluZ0NBLmNydDAMBgNVHRMBAf8EAjAAMA0G
# CSqGSIb3DQEBCwUAA4IBAQCfpLMUerP0WXkcb9+dunMLt3jowZEd8X6ISxxzdsdB
# 8jOZ92L88qKqjWD1I9HBceba4tdJZCLV33S9a3eoxUunIlJH4GmYH/HSrc2qgNxg
# PyobmWf556c7Wd3q6ZUKgos0bw++TtLqb/jvoKN19epTEkwQDIwVFzOAxZ4T+sYr
# jmFhd9KeaRhTLZRBVdKNTKtXaoWFrfNSQTp8NcNYdkEM05cUnEUMDOoeLSmxPnIv
# pl8KbripxtVQ591rCLJN2uMtrtSE1nvjiYfSFQI00EiB33ZoI2T1eCNuP1M6c+ex
# KzQQC8UDp7J+duzl1j605TwSfLS/MJsaiwftNzc3FfgSMIIFMDCCBBigAwIBAgIQ
# BAkYG1/Vu2Z1U0O1b5VQCDANBgkqhkiG9w0BAQsFADBlMQswCQYDVQQGEwJVUzEV
# MBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29t
# MSQwIgYDVQQDExtEaWdpQ2VydCBBc3N1cmVkIElEIFJvb3QgQ0EwHhcNMTMxMDIy
# MTIwMDAwWhcNMjgxMDIyMTIwMDAwWjByMQswCQYDVQQGEwJVUzEVMBMGA1UEChMM
# RGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMTEwLwYDVQQD
# EyhEaWdpQ2VydCBTSEEyIEFzc3VyZWQgSUQgQ29kZSBTaWduaW5nIENBMIIBIjAN
# BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA+NOzHH8OEa9ndwfTCzFJGc/Q+0WZ
# sTrbRPV/5aid2zLXcep2nQUut4/6kkPApfmJ1DcZ17aq8JyGpdglrA55KDp+6dFn
# 08b7KSfH03sjlOSRI5aQd4L5oYQjZhJUM1B0sSgmuyRpwsJS8hRniolF1C2ho+mI
# LCCVrhxKhwjfDPXiTWAYvqrEsq5wMWYzcT6scKKrzn/pfMuSoeU7MRzP6vIK5Fe7
# SrXpdOYr/mzLfnQ5Ng2Q7+S1TqSp6moKq4TzrGdOtcT3jNEgJSPrCGQ+UpbB8g8S
# 9MWOD8Gi6CxR93O8vYWxYoNzQYIH5DiLanMg0A9kczyen6Yzqf0Z3yWT0QIDAQAB
# o4IBzTCCAckwEgYDVR0TAQH/BAgwBgEB/wIBADAOBgNVHQ8BAf8EBAMCAYYwEwYD
# VR0lBAwwCgYIKwYBBQUHAwMweQYIKwYBBQUHAQEEbTBrMCQGCCsGAQUFBzABhhho
# dHRwOi8vb2NzcC5kaWdpY2VydC5jb20wQwYIKwYBBQUHMAKGN2h0dHA6Ly9jYWNl
# cnRzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydEFzc3VyZWRJRFJvb3RDQS5jcnQwgYEG
# A1UdHwR6MHgwOqA4oDaGNGh0dHA6Ly9jcmw0LmRpZ2ljZXJ0LmNvbS9EaWdpQ2Vy
# dEFzc3VyZWRJRFJvb3RDQS5jcmwwOqA4oDaGNGh0dHA6Ly9jcmwzLmRpZ2ljZXJ0
# LmNvbS9EaWdpQ2VydEFzc3VyZWRJRFJvb3RDQS5jcmwwTwYDVR0gBEgwRjA4Bgpg
# hkgBhv1sAAIEMCowKAYIKwYBBQUHAgEWHGh0dHBzOi8vd3d3LmRpZ2ljZXJ0LmNv
# bS9DUFMwCgYIYIZIAYb9bAMwHQYDVR0OBBYEFFrEuXsqCqOl6nEDwGD5LfZldQ5Y
# MB8GA1UdIwQYMBaAFEXroq/0ksuCMS1Ri6enIZ3zbcgPMA0GCSqGSIb3DQEBCwUA
# A4IBAQA+7A1aJLPzItEVyCx8JSl2qB1dHC06GsTvMGHXfgtg/cM9D8Svi/3vKt8g
# VTew4fbRknUPUbRupY5a4l4kgU4QpO4/cY5jDhNLrddfRHnzNhQGivecRk5c/5Cx
# GwcOkRX7uq+1UcKNJK4kxscnKqEpKBo6cSgCPC6Ro8AlEeKcFEehemhor5unXCBc
# 2XGxDI+7qPjFEmifz0DLQESlE/DmZAwlCEIysjaKJAL+L3J+HNdJRZboWR3p+nRk
# a7LrZkPas7CM1ekN3fYBIM6ZMWM9CBoYs4GbT8aTEAb8B4H6i9r5gkn3Ym6hU/oS
# lBiFLpKR6mhsRDKyZqHnGKSaZFHvMIIGajCCBVKgAwIBAgIQAwGaAjr/WLFr1tXq
# 5hfwZjANBgkqhkiG9w0BAQUFADBiMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGln
# aUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMSEwHwYDVQQDExhE
# aWdpQ2VydCBBc3N1cmVkIElEIENBLTEwHhcNMTQxMDIyMDAwMDAwWhcNMjQxMDIy
# MDAwMDAwWjBHMQswCQYDVQQGEwJVUzERMA8GA1UEChMIRGlnaUNlcnQxJTAjBgNV
# BAMTHERpZ2lDZXJ0IFRpbWVzdGFtcCBSZXNwb25kZXIwggEiMA0GCSqGSIb3DQEB
# AQUAA4IBDwAwggEKAoIBAQCjZF38fLPggjXg4PbGKuZJdTvMbuBTqZ8fZFnmfGt/
# a4ydVfiS457VWmNbAklQ2YPOb2bu3cuF6V+l+dSHdIhEOxnJ5fWRn8YUOawk6qhL
# LJGJzF4o9GS2ULf1ErNzlgpno75hn67z/RJ4dQ6mWxT9RSOOhkRVfRiGBYxVh3lI
# RvfKDo2n3k5f4qi2LVkCYYhhchhoubh87ubnNC8xd4EwH7s2AY3vJ+P3mvBMMWSN
# 4+v6GYeofs/sjAw2W3rBerh4x8kGLkYQyI3oBGDbvHN0+k7Y/qpA8bLOcEaD6dpA
# oVk62RUJV5lWMJPzyWHM0AjMa+xiQpGsAsDvpPCJEY93AgMBAAGjggM1MIIDMTAO
# BgNVHQ8BAf8EBAMCB4AwDAYDVR0TAQH/BAIwADAWBgNVHSUBAf8EDDAKBggrBgEF
# BQcDCDCCAb8GA1UdIASCAbYwggGyMIIBoQYJYIZIAYb9bAcBMIIBkjAoBggrBgEF
# BQcCARYcaHR0cHM6Ly93d3cuZGlnaWNlcnQuY29tL0NQUzCCAWQGCCsGAQUFBwIC
# MIIBVh6CAVIAQQBuAHkAIAB1AHMAZQAgAG8AZgAgAHQAaABpAHMAIABDAGUAcgB0
# AGkAZgBpAGMAYQB0AGUAIABjAG8AbgBzAHQAaQB0AHUAdABlAHMAIABhAGMAYwBl
# AHAAdABhAG4AYwBlACAAbwBmACAAdABoAGUAIABEAGkAZwBpAEMAZQByAHQAIABD
# AFAALwBDAFAAUwAgAGEAbgBkACAAdABoAGUAIABSAGUAbAB5AGkAbgBnACAAUABh
# AHIAdAB5ACAAQQBnAHIAZQBlAG0AZQBuAHQAIAB3AGgAaQBjAGgAIABsAGkAbQBp
# AHQAIABsAGkAYQBiAGkAbABpAHQAeQAgAGEAbgBkACAAYQByAGUAIABpAG4AYwBv
# AHIAcABvAHIAYQB0AGUAZAAgAGgAZQByAGUAaQBuACAAYgB5ACAAcgBlAGYAZQBy
# AGUAbgBjAGUALjALBglghkgBhv1sAxUwHwYDVR0jBBgwFoAUFQASKxOYspkH7R7f
# or5XDStnAs0wHQYDVR0OBBYEFGFaTSS2STKdSip5GoNL9B6Jwcp9MH0GA1UdHwR2
# MHQwOKA2oDSGMmh0dHA6Ly9jcmwzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydEFzc3Vy
# ZWRJRENBLTEuY3JsMDigNqA0hjJodHRwOi8vY3JsNC5kaWdpY2VydC5jb20vRGln
# aUNlcnRBc3N1cmVkSURDQS0xLmNybDB3BggrBgEFBQcBAQRrMGkwJAYIKwYBBQUH
# MAGGGGh0dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBBBggrBgEFBQcwAoY1aHR0cDov
# L2NhY2VydHMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0QXNzdXJlZElEQ0EtMS5jcnQw
# DQYJKoZIhvcNAQEFBQADggEBAJ0lfhszTbImgVybhs4jIA+Ah+WI//+x1GosMe06
# FxlxF82pG7xaFjkAneNshORaQPveBgGMN/qbsZ0kfv4gpFetW7easGAm6mlXIV00
# Lx9xsIOUGQVrNZAQoHuXx/Y/5+IRQaa9YtnwJz04HShvOlIJ8OxwYtNiS7Dgc6aS
# wNOOMdgv420XEwbu5AO2FKvzj0OncZ0h3RTKFV2SQdr5D4HRmXQNJsQOfxu19aDx
# xncGKBXp2JPlVRbwuwqrHNtcSCdmyKOLChzlldquxC5ZoGHd2vNtomHpigtt7BIY
# vfdVVEADkitrwlHCCkivsNRu4PQUCjob4489yq9qjXvc2EQwggbNMIIFtaADAgEC
# AhAG/fkDlgOt6gAK6z8nu7obMA0GCSqGSIb3DQEBBQUAMGUxCzAJBgNVBAYTAlVT
# MRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5j
# b20xJDAiBgNVBAMTG0RpZ2lDZXJ0IEFzc3VyZWQgSUQgUm9vdCBDQTAeFw0wNjEx
# MTAwMDAwMDBaFw0yMTExMTAwMDAwMDBaMGIxCzAJBgNVBAYTAlVTMRUwEwYDVQQK
# EwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5jb20xITAfBgNV
# BAMTGERpZ2lDZXJ0IEFzc3VyZWQgSUQgQ0EtMTCCASIwDQYJKoZIhvcNAQEBBQAD
# ggEPADCCAQoCggEBAOiCLZn5ysJClaWAc0Bw0p5WVFypxNJBBo/JM/xNRZFcgZ/t
# LJz4FlnfnrUkFcKYubR3SdyJxArar8tea+2tsHEx6886QAxGTZPsi3o2CAOrDDT+
# GEmC/sfHMUiAfB6iD5IOUMnGh+s2P9gww/+m9/uizW9zI/6sVgWQ8DIhFonGcIj5
# BZd9o8dD3QLoOz3tsUGj7T++25VIxO4es/K8DCuZ0MZdEkKB4YNugnM/JksUkK5Z
# ZgrEjb7SzgaurYRvSISbT0C58Uzyr5j79s5AXVz2qPEvr+yJIvJrGGWxwXOt1/HY
# zx4KdFxCuGh+t9V3CidWfA9ipD8yFGCV/QcEogkCAwEAAaOCA3owggN2MA4GA1Ud
# DwEB/wQEAwIBhjA7BgNVHSUENDAyBggrBgEFBQcDAQYIKwYBBQUHAwIGCCsGAQUF
# BwMDBggrBgEFBQcDBAYIKwYBBQUHAwgwggHSBgNVHSAEggHJMIIBxTCCAbQGCmCG
# SAGG/WwAAQQwggGkMDoGCCsGAQUFBwIBFi5odHRwOi8vd3d3LmRpZ2ljZXJ0LmNv
# bS9zc2wtY3BzLXJlcG9zaXRvcnkuaHRtMIIBZAYIKwYBBQUHAgIwggFWHoIBUgBB
# AG4AeQAgAHUAcwBlACAAbwBmACAAdABoAGkAcwAgAEMAZQByAHQAaQBmAGkAYwBh
# AHQAZQAgAGMAbwBuAHMAdABpAHQAdQB0AGUAcwAgAGEAYwBjAGUAcAB0AGEAbgBj
# AGUAIABvAGYAIAB0AGgAZQAgAEQAaQBnAGkAQwBlAHIAdAAgAEMAUAAvAEMAUABT
# ACAAYQBuAGQAIAB0AGgAZQAgAFIAZQBsAHkAaQBuAGcAIABQAGEAcgB0AHkAIABB
# AGcAcgBlAGUAbQBlAG4AdAAgAHcAaABpAGMAaAAgAGwAaQBtAGkAdAAgAGwAaQBh
# AGIAaQBsAGkAdAB5ACAAYQBuAGQAIABhAHIAZQAgAGkAbgBjAG8AcgBwAG8AcgBh
# AHQAZQBkACAAaABlAHIAZQBpAG4AIABiAHkAIAByAGUAZgBlAHIAZQBuAGMAZQAu
# MAsGCWCGSAGG/WwDFTASBgNVHRMBAf8ECDAGAQH/AgEAMHkGCCsGAQUFBwEBBG0w
# azAkBggrBgEFBQcwAYYYaHR0cDovL29jc3AuZGlnaWNlcnQuY29tMEMGCCsGAQUF
# BzAChjdodHRwOi8vY2FjZXJ0cy5kaWdpY2VydC5jb20vRGlnaUNlcnRBc3N1cmVk
# SURSb290Q0EuY3J0MIGBBgNVHR8EejB4MDqgOKA2hjRodHRwOi8vY3JsMy5kaWdp
# Y2VydC5jb20vRGlnaUNlcnRBc3N1cmVkSURSb290Q0EuY3JsMDqgOKA2hjRodHRw
# Oi8vY3JsNC5kaWdpY2VydC5jb20vRGlnaUNlcnRBc3N1cmVkSURSb290Q0EuY3Js
# MB0GA1UdDgQWBBQVABIrE5iymQftHt+ivlcNK2cCzTAfBgNVHSMEGDAWgBRF66Kv
# 9JLLgjEtUYunpyGd823IDzANBgkqhkiG9w0BAQUFAAOCAQEARlA+ybcoJKc4HbZb
# Ka9Sz1LpMUerVlx71Q0LQbPv7HUfdDjyslxhopyVw1Dkgrkj0bo6hnKtOHisdV0X
# FzRyR4WUVtHruzaEd8wkpfMEGVWp5+Pnq2LN+4stkMLA0rWUvV5PsQXSDj0aqRRb
# poYxYqioM+SbOafE9c4deHaUJXPkKqvPnHZL7V/CSxbkS3BMAIke/MV5vEwSV/5f
# 4R68Al2o/vsHOE8Nxl2RuQ9nRc3Wg+3nkg2NsWmMT/tZ4CMP0qquAHzunEIOz5HX
# J7cW7g/DvXwKoO4sCFWFIrjrGBpN/CohrUkxg0eVd3HcsRtLSxwQnHcUwZ1PL1qV
# CCkQJjGCBEwwggRIAgEBMIGGMHIxCzAJBgNVBAYTAlVTMRUwEwYDVQQKEwxEaWdp
# Q2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5jb20xMTAvBgNVBAMTKERp
# Z2lDZXJ0IFNIQTIgQXNzdXJlZCBJRCBDb2RlIFNpZ25pbmcgQ0ECEAnCdWnlrvRE
# exyCUUR8SoswDQYJYIZIAWUDBAIBBQCggYQwGAYKKwYBBAGCNwIBDDEKMAigAoAA
# oQKAADAZBgkqhkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4w
# DAYKKwYBBAGCNwIBFTAvBgkqhkiG9w0BCQQxIgQg4XXnuqxaQ3yqmW9gBJ3acsJg
# WpAPGbzkryI/DKQ5rH4wDQYJKoZIhvcNAQEBBQAEggEAN+4VLUVs74jzJjUXp/co
# jZbvyAXakPIvhV5MTa5GzZ2ima3gV6FLh4vRI0q949XTCf15P8QTEzjIbM5b6ufy
# zKNvdSaKS5D6bbMUrnQ480qGBFBD97/+oOtXCGp1xrajHEzXOlzcSa9ph0wVsNjn
# z4ASG3g74jQc3rGYYzhjwOo9kdm5FzFqe6oDpUzlp1COJraIclFEcnAXQsj7/vyX
# erohAwIKCG1vWLRstNfJOa0R3ljFqsOWh/AYS5A7dEunssoW1qyn8VRUccdz5BTP
# qoU9Jb47FWn6T1L3sbJA51TBpzIuGEeCAp+1AXhFCUEzzsJ2XyZNmp4j41Ca2Dxg
# 9qGCAg8wggILBgkqhkiG9w0BCQYxggH8MIIB+AIBATB2MGIxCzAJBgNVBAYTAlVT
# MRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5j
# b20xITAfBgNVBAMTGERpZ2lDZXJ0IEFzc3VyZWQgSUQgQ0EtMQIQAwGaAjr/WLFr
# 1tXq5hfwZjAJBgUrDgMCGgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAc
# BgkqhkiG9w0BCQUxDxcNMTgxMDIyMTYyODEzWjAjBgkqhkiG9w0BCQQxFgQU0Gbh
# xvDGeUcb/brPS0RusQmZ+uMwDQYJKoZIhvcNAQEBBQAEggEAXV16mgJRl5xHx0n/
# op1vhGFz58pROe49N4MmTvAbEmTbpTJ+aPEgY5jxZ+JN+TciwTXkc99FmUhHpBVn
# 99cTo7/VHC0e8zn3YM+hOYlfGn4iJUWeH8FUdHrKuh4vV5K+99/b8SpbXqvQ5Y56
# Wir0QIN6LFLpWsSWesgRxDcA/6+Jn6tIkUTgWiWYP0sWmHrVOHs28MynScWiiiN3
# hbsS8xmVlCYB80488GHgljcgeECYxKcuXvgRFIQQUuJLWOQrHpYGT4376aknB7Zk
# izIkEBnQ8Zwit8BSMSJqueSw/D1drwaB6pLtGNFX7ejUrC8X0rwMq4FwhhlUFGYD
# W/jHGg==
# SIG # End signature block
