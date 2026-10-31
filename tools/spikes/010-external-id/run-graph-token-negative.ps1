param([string]$OutputDirectory = (Join-Path $PSScriptRoot '.local'))
$ErrorActionPreference = 'Stop'
$inventory = Get-Content (Join-Path $PSScriptRoot '.local/qa-inventory.json') -Raw | ConvertFrom-Json
if ($inventory.domain -ne 'mikompri010qa20261030.onmicrosoft.com') { throw 'Only the isolated QA tenant is permitted.' }
$previousTenant=$env:MIKOMPRI_SPIKE_TENANT
$previousAudience=$env:MIKOMPRI_SPIKE_AUDIENCE
try {
    $token=az account get-access-token --tenant $inventory.tenantId --resource https://graph.microsoft.com/ --query accessToken -o tsv --only-show-errors 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'No real QA Graph token obtained; test is inconclusive.' }
    $env:MIKOMPRI_SPIKE_TENANT=$inventory.tenantId
    $env:MIKOMPRI_SPIKE_AUDIENCE=$inventory.apiAppId
    $assembly=Join-Path $PSScriptRoot 'bin/Release/net8.0/ExternalIdSpike.dll'
    $validation=($token | & dotnet $assembly validate-external-token) | ConvertFrom-Json
    $validationExit=$LASTEXITCODE
    if ($validationExit -ne 1 -or $validation.Accepted -ne $false) { throw 'Graph token was not rejected as expected.' }
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    @{Kind='REAL_QA_MICROSOFT_GRAPH_ACCESS_TOKEN_NEGATIVE';ExpectedAccepted=$false;Validation=$validation;Result='PASS'}|
        ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $OutputDirectory 'graph-token-negative.json') -Encoding utf8NoBOM
    Write-Output 'Real Microsoft Graph token rejected against QA API contract; no token persisted.'
}
finally {
    $token=$null
    $env:MIKOMPRI_SPIKE_TENANT=$previousTenant
    $env:MIKOMPRI_SPIKE_AUDIENCE=$previousAudience
}
