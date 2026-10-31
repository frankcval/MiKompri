$ErrorActionPreference='Stop'
$subscription=az account show --query id -o tsv --only-show-errors
if ($LASTEXITCODE -ne 0) { throw 'Azure account unavailable.' }
$resource=az rest --method GET --url "https://management.azure.com/subscriptions/$subscription/resourceGroups/rg-mikompri-010-spike-20261030/providers/Microsoft.AzureActiveDirectory/ciamDirectories/mikompri010qa20261030?api-version=2023-05-17-preview" --only-show-errors -o json 2>$null|ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $resource.properties.domainName -ne 'mikompri010qa20261030.onmicrosoft.com') { throw 'Expected isolated QA resource unavailable.' }
$token=az account get-access-token --tenant $resource.properties.tenantId --resource https://graph.microsoft.com/ --query accessToken -o tsv --only-show-errors 2>$null
if ($LASTEXITCODE -ne 0) { throw 'QA Graph token unavailable.' }
try {
    $headers=@{Authorization="Bearer $token"}
    $organization=Invoke-RestMethod -Uri 'https://graph.microsoft.com/v1.0/organization?$select=tenantType' -Headers $headers
    if (@($organization.value).Count -ne 1 -or $organization.value[0].tenantType -ne 'CIAM') { throw 'Not CIAM QA.' }
    $apps=Invoke-RestMethod -Uri 'https://graph.microsoft.com/v1.0/applications?$select=id,appId,displayName,api' -Headers $headers
    $api=@($apps.value|Where-Object {$_.displayName -eq 'MiKompri 010 spike QA API'})
    $spa=@($apps.value|Where-Object {$_.displayName -eq 'MiKompri 010 spike QA SPA'})
    if ($api.Count -ne 1 -or $spa.Count -ne 1) { throw 'Expected exactly one QA API and one QA SPA.' }
    $servicePrincipals=Invoke-RestMethod -Uri 'https://graph.microsoft.com/v1.0/servicePrincipals?$select=id,appId' -Headers $headers
    $apiSp=@($servicePrincipals.value|Where-Object {$_.appId -eq $api[0].appId})
    $spaSp=@($servicePrincipals.value|Where-Object {$_.appId -eq $spa[0].appId})
    if ($apiSp.Count -ne 1 -or $spaSp.Count -ne 1) { throw 'Expected QA service principals missing.' }
    $grants=Invoke-RestMethod -Uri 'https://graph.microsoft.com/v1.0/oauth2PermissionGrants' -Headers $headers
    $grant=@($grants.value|Where-Object {$_.clientId -eq $spaSp[0].id -and $_.resourceId -eq $apiSp[0].id})
    if ($grant.Count -ne 1) { throw 'QA resource grant missing or ambiguous.' }
    $flows=Invoke-RestMethod -Uri 'https://graph.microsoft.com/v1.0/identity/authenticationEventsFlows' -Headers $headers
    $flow=@($flows.value|Where-Object {$_.displayName -eq 'MiKompri010QA'})
    if ($flow.Count -ne 1) { throw 'Expected QA flow missing or ambiguous.' }
    $inventory=@{tenantId=$resource.properties.tenantId;domain=$resource.properties.domainName;apiObjectId=$api[0].id;apiAppId=$api[0].appId;spaObjectId=$spa[0].id;spaAppId=$spa[0].appId;apiServicePrincipalId=$apiSp[0].id;spaServicePrincipalId=$spaSp[0].id;apiConsentId=$grant[0].id;flowId=$flow[0].id}
    $negative=@($apps.value|Where-Object {$_.displayName -eq 'MiKompri 010 spike QA scope negative'})
    if ($negative.Count -gt 1) { throw 'Ambiguous negative client.' }
    if ($negative.Count -eq 1) {
        $inventory.negativeSpaObjectId=$negative[0].id;$inventory.negativeSpaAppId=$negative[0].appId
        $negativeSp=@($servicePrincipals.value|Where-Object {$_.appId -eq $negative[0].appId})
        if ($negativeSp.Count -eq 1) {
            $inventory.negativeSpaServicePrincipalId=$negativeSp[0].id
            $negativeGrant=@($grants.value|Where-Object {$_.clientId -eq $negativeSp[0].id -and $_.resourceId -eq $apiSp[0].id})
            if ($negativeGrant.Count -eq 1) { $inventory.negativeConsentId=$negativeGrant[0].id }
        }
    }
    foreach($scope in $api[0].api.oauth2PermissionScopes) {
        if($scope.value -eq 'access_as_user') {$inventory.scopeId=$scope.id}
        if($scope.value -eq 'diagnostic_only') {$inventory.diagnosticScopeId=$scope.id}
    }
    $local=Join-Path $PSScriptRoot '.local'
    New-Item -ItemType Directory -Path $local -Force|Out-Null
    $inventory|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $local 'qa-inventory.json') -Encoding utf8NoBOM
    Write-Output 'Public isolated QA inventory refreshed (no credentials, tokens or user attributes).'
}
finally {$token=$null;$headers=$null}
