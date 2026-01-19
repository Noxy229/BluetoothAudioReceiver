$ErrorActionPreference = "Stop"

$certName = "BluetoothAudioReceiverTestCert"
$certPassword = "password" # Default password for the test cert
$pfxPath = "$PSScriptRoot\TestCert.pfx"

Write-Host "Creating Self-Signed Certificate: $certName"

$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My -Subject "CN=$certName" -Type CodeSigningCert -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048

$password = ConvertTo-SecureString -String $certPassword -Force -AsPlainText

Write-Host "Exporting to PFX: $pfxPath"
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $password

Write-Host "Done! Test Certificate created at: $pfxPath"
Write-Host "Password: $certPassword"
Write-Host ""
Write-Host "IMPORTANT: To test SmartScreen removal locally, you must import this PFX into 'Trusted Root Certification Authorities' on your machine."
