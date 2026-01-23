param (
    [string]$CertPath = "$PSScriptRoot\TestCert.pfx",
    [string]$CertPassword
)

$ErrorActionPreference = "Stop"
$projectPath = "$PSScriptRoot\..\BluetoothAudioReceiver.csproj"
$publishDir = "$PSScriptRoot\..\bin\Release\net8.0-windows10.0.19041.0\publish"

# 1. Build and Publish
Write-Host "Building and Publishing..."
dotnet publish $projectPath -c Release --self-contained true -p:PublishSingleFile=true -o $publishDir

$exePath = "$publishDir\BluetoothAudioReceiver.exe"

if (-not (Test-Path $exePath)) {
    Write-Error "Build failed. EXE not found at $exePath"
}

# 2. Sign
if (Test-Path $CertPath) {
    if ([string]::IsNullOrEmpty($CertPassword)) {
        if ($env:PFX_PASSWORD) {
            $CertPassword = $env:PFX_PASSWORD
        } else {
            Write-Error "Certificate found at $CertPath but no password provided via -CertPassword or PFX_PASSWORD env var."
        }
    }

    Write-Host "Signing $exePath with $CertPath..."
    
    # Try using built-in Set-AuthenticodeSignature (works without signtool in many cases)
    $cert = Get-PfxCertificate -FilePath $CertPath -Password (ConvertTo-SecureString -String $CertPassword -Force -AsPlainText)
    Set-AuthenticodeSignature -FilePath $exePath -Certificate $cert -HashAlgorithm SHA256 -TimestampServer "http://timestamp.digicert.com"
    
    Write-Host "Signing Complete."
    
    # Verify
    $sig = Get-AuthenticodeSignature $exePath
    if ($sig.Status -eq 'Valid') {
        Write-Host "SUCCESS: Signature is valid." -ForegroundColor Green
    }
    else {
        Write-Warning "Signature Status: $($sig.Status). The certificate might not be trusted on this machine yet, but the file is signed."
    }
}
else {
    Write-Warning "Certificate not found at $CertPath. Skipping signing."
    Write-Host "To create a test cert, run scripts\Create-Test-Cert.ps1"
}
