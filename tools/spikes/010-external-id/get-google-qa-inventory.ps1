param([string]$CredentialPath = (Join-Path $env:LOCALAPPDATA 'MiKompri/qa-010/google-client.json'))
$ErrorActionPreference='Stop'
$evidencePath=Join-Path $PSScriptRoot '../../../specs/010-account-management-social-auth/evidence/google-qa-configuration.json'
$evidence=Get-Content -LiteralPath $evidencePath -Raw|ConvertFrom-Json
$privateClient=Get-Content -LiteralPath $CredentialPath -Raw|ConvertFrom-Json
function Digest([string]$Value) { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($Value))).ToLowerInvariant() }
try {
    if (!$privateClient.web.client_id -or !$privateClient.web.project_id `
        -or (Digest $privateClient.web.client_id) -ne $evidence.clientIdSha256 `
        -or (Digest $privateClient.web.project_id) -ne $evidence.projectIdSha256) {
        throw 'Private file does not match documented QA client/project; no inventory written.'
    }
    $local=Join-Path $PSScriptRoot '.local'
    New-Item -ItemType Directory -Path $local -Force|Out-Null
    # Only public identifiers, never client_secret or other credential properties.
    @{projectId=$privateClient.web.project_id;clientId=$privateClient.web.client_id;displayName='MiKompri 010 QA spike'}|
        ConvertTo-Json|Set-Content -LiteralPath (Join-Path $local 'google-inventory.json') -Encoding utf8NoBOM
    Write-Output 'Public Google QA inventory reconstructed; no secret output or copy.'
}
finally {$privateClient=$null}
