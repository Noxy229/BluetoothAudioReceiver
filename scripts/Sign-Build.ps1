param (
    [string]$CertPath = "$PSScriptRoot\TestCert.pfx",
    [string]$CertPassword
)

$ErrorActionPreference = "Stop"
$projectPath = "$PSScriptRoot\..\BluetoothAudioReceiver.csproj"
$publishDir = "$PSScriptRoot\..\bin\Release\net8.0-windows10.0.19041.0\publish"

# 1. Build and Publish
Write-Host "Building and Publishing..."
Write-Host "Project Path: $projectPath"
Write-Host "Publish Dir: $publishDir"

try {
    dotnet publish $projectPath -c Release --self-contained true -p:PublishSingleFile=true -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
        exit 1
    }
}
catch {
    Write-Error "Build failed: $_"
    exit 1
}

$exePath = "$publishDir\BluetoothAudioReceiver.exe"

if (-not (Test-Path $exePath)) {
    Write-Error "Build failed. EXE not found at $exePath"
    exit 1
}

Write-Host "Build successful! EXE found at: $exePath"

# 2. Sign (optional - don't fail if signing has issues)
if (Test-Path $CertPath) {
    if ([string]::IsNullOrEmpty($CertPassword)) {
        if ($env:PFX_PASSWORD) {
            $CertPassword = $env:PFX_PASSWORD
        } else {
            Write-Error "Certificate found at $CertPath but no password provided via -CertPassword or PFX_PASSWORD env var."
        }
    }

    Write-Host "Signing $exePath with $CertPath..."
    
    try {
        $cert = Get-PfxCertificate -FilePath $CertPath -Password (ConvertTo-SecureString -String $CertPassword -Force -AsPlainText)
        Set-AuthenticodeSignature -FilePath $exePath -Certificate $cert -HashAlgorithm SHA256 -TimestampServer "http://timestamp.digicert.com"
        
        Write-Host "Signing Complete."
        
        # Verify
        $sig = Get-AuthenticodeSignature $exePath
        Write-Host "Signature Status: $($sig.Status)"
    }
    catch {
        Write-Warning "Signing failed: $_"
        Write-Host "Continuing without signature..."
    }
}
else {
    Write-Warning "Certificate not found at $CertPath. Skipping signing."
}

Write-Host "Done! EXE is at: $exePath"
exit 0
