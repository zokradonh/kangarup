<#
.SYNOPSIS

Deploys your application to a WebDAV server.

.DESCRIPTION

This script deploys the application in the current directory to the WebDAV server specified by UpdateURI parameter. It generates a self signed certificate if not already present.

.PARAMETER UpdateURI

The WebDAV Server URI. The value given here is not modified. The application name is NOT appended.

.PARAMETER PatchTitle

Some text message for end users to see what's new in the update.

.PARAMETER Version

The new version of this update. The kangarup update checks if the version is bigger than the current.

.PARAMETER Credentials

The WebDAV credentials needed for authentication.

.PARAMETER ApplicationName

The name of your application. Used in the generated certificate.

.EXAMPLE

kangtools\kdeploy.ps1 -UpdateURI https://mywebdavserver.org/myapplication -PatchTitle "Everything is much slower now" -Version 1.2.3.4 -Credentials (Get-Credential) -ApplicationName myapplication
#>

param (
[Parameter(Mandatory=$true)][Uri]$UpdateUri,
[Parameter(Mandatory=$true)][String]$PatchTitle,
[Parameter(Mandatory=$true)][Version]$Version,
[Parameter(Mandatory=$true)][System.Net.NetworkCredential]$Credentials,
[Parameter(Mandatory=$true)][String]$ApplicationName
)

# sync .NET working directory with PowerShell working directory
[Environment]::CurrentDirectory = (Get-Location -PSProvider FileSystem).ProviderPath

# find certificate
$storedCertificate = Get-ChildItem cert:\CurrentUser\My | Where-Object Subject -eq "CN=$ApplicationName"

if (-not $storedCertificate) {
	# create new self signed certificate
	$cert = New-SelfSignedCertificate -CertStoreLocation cert:\CurrentUser\My -Subject $ApplicationName -KeyLength 2048 -KeyAlgorithm "RSA" -KeyUsage DigitalSignature

	Export-Certificate -FilePath "public.cer" -Cert $cert

	Write-Output "Created certificate in your computer's user store (certmgr.msc) and exported public certificate to file 'public.cer' to bundle with application."

	$storedCertificate = $cert
}
else {
	Write-Output "Using certificate $($storedCertificate.Thumbprint) from local user certificate store."
}

# load kangarup deployment libraries
$_ = [Reflection.Assembly]::LoadFrom([System.IO.Path]::Combine([Environment]::CurrentDirectory, "YamlDotNet.dll"))
$_ = [Reflection.Assembly]::LoadFrom([System.IO.Path]::Combine([Environment]::CurrentDirectory, "kangarup.dll"))

# deploy
$deploy = New-Object kangarup.Deployment($UpdateUri)
$deploy.SignaturePrivateCertificate = $storedCertificate

$updateInfo = $deploy.CreateUpdateInfoAsync(".",$PatchTitle, $Version).GetAwaiter().GetResult()

if ($deploy.DeployAsync($updateInfo, $Credentials).GetAwaiter().GetResult()) {
	Write-Output "Successfully deployed."
}
