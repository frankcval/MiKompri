$ErrorActionPreference='Stop'
$inventory=Get-Content (Join-Path $PSScriptRoot '.local/qa-inventory.json') -Raw|ConvertFrom-Json
if ($inventory.domain -ne 'mikompri010qa20261030.onmicrosoft.com') { throw 'Only isolated QA is permitted.' }
$prior=@{}
foreach($key in @('MIKOMPRI_SPIKE_TENANT','MIKOMPRI_SPIKE_AUDIENCE','MIKOMPRI_SPIKE_SPA','MIKOMPRI_SPIKE_NEGATIVE_SPA','MIKOMPRI_SPIKE_GOOGLE_CLIENT_ID','MIKOMPRI_SPIKE_G3_CONTEXT','MIKOMPRI_SPIKE_MSAL_JS')) { $prior[$key]=[Environment]::GetEnvironmentVariable($key) }
try {
    $env:MIKOMPRI_SPIKE_TENANT=$inventory.tenantId
    $env:MIKOMPRI_SPIKE_AUDIENCE=$inventory.apiAppId
    $env:MIKOMPRI_SPIKE_SPA=$inventory.spaAppId
    $env:MIKOMPRI_SPIKE_NEGATIVE_SPA=$inventory.negativeSpaAppId
    $g3InventoryPath=Join-Path $PSScriptRoot '.local/g3-qa-inventory.json'
    if (Test-Path -LiteralPath $g3InventoryPath) {
        $g3Inventory=Get-Content -LiteralPath $g3InventoryPath -Raw|ConvertFrom-Json
        if ($g3Inventory.tenantId -ne $inventory.tenantId -or $g3Inventory.contextName -ne 'MiKompri010G3EmailOtpQA') {
            throw 'Unexpected G3 QA context inventory.'
        }
        $env:MIKOMPRI_SPIKE_G3_CONTEXT=$g3Inventory.contextId
    }
    $googleInventoryPath=Join-Path $PSScriptRoot '.local/google-inventory.json'
    if (Test-Path -LiteralPath $googleInventoryPath) {
        $googleInventory=Get-Content -LiteralPath $googleInventoryPath -Raw|ConvertFrom-Json
        $env:MIKOMPRI_SPIKE_GOOGLE_CLIENT_ID=$googleInventory.clientId
    }
    $env:MIKOMPRI_SPIKE_MSAL_JS=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../MiKompri.Web/node_modules/@azure/msal-browser/lib/msal-browser.min.js'))
    & dotnet (Join-Path $PSScriptRoot 'bin/Release/net8.0/ExternalIdSpike.dll') auth-harness
}
finally { foreach($key in $prior.Keys) { [Environment]::SetEnvironmentVariable($key,$prior[$key]) } }
