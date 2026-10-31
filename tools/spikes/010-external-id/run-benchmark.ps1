param([string]$OutputDirectory = (Join-Path $PSScriptRoot '.local'))
$ErrorActionPreference = 'Stop'
$containerName = 'mikompri-010-spike-pg'
$fixtureLabel = 'mikompri.spike=010-controlled'
$project = Join-Path $PSScriptRoot 'ExternalIdSpike.csproj'
$assembly = Join-Path $PSScriptRoot 'bin/Release/net8.0/ExternalIdSpike.dll'
$created = $false
$previousPassword = $env:POSTGRES_PASSWORD
$previousConnection = $env:MIKOMPRI_SPIKE_PG

try {
    $existing = docker ps -a --filter "name=^/$containerName$" --format '{{.Names}}'
    if ($LASTEXITCODE -ne 0 -or $existing) { throw 'Docker unavailable or QA container already exists; refusing reuse.' }
    docker image inspect postgres:15 --format '{{.Id}}' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'The pre-existing postgres:15 image is required; no automatic pull.' }
    dotnet build $project --configuration Release --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw 'Spike build failed.' }
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    $jwtResult = & dotnet $assembly jwt-controls
    if ($LASTEXITCODE -ne 0) { throw 'Synthetic JWT controls failed.' }
    $jwtResult | Set-Content -LiteralPath (Join-Path $OutputDirectory 'jwt-controls.json') -Encoding utf8NoBOM

    # Passed by environment name: password is not an argument, file or output.
    $env:POSTGRES_PASSWORD = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
    docker run --detach --pull never --name $containerName --label $fixtureLabel --memory 384m --cpus 1 `
        --publish 127.0.0.1:55410:5432 --tmpfs /var/lib/postgresql/data:rw,size=256m `
        --env POSTGRES_PASSWORD --env POSTGRES_DB=mikompri_010_spike postgres:15 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Isolated PostgreSQL creation failed.' }
    $created = $true
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        docker exec $containerName pg_isready -U postgres -d mikompri_010_spike 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Start-Sleep -Seconds 1
    }
    if (!$ready) { throw 'QA PostgreSQL did not become ready.' }
    $env:MIKOMPRI_SPIKE_PG = "Host=127.0.0.1;Port=55410;Database=mikompri_010_spike;Username=postgres;Password=$env:POSTGRES_PASSWORD;Pooling=true;Maximum Pool Size=20;Timeout=1;Command Timeout=1;Include Error Detail=false"
    $benchmarkResult = & dotnet $assembly benchmark
    if ($LASTEXITCODE -ne 0) { throw 'Benchmark failed; do not publish partial results as acceptance.' }
    $benchmarkResult | Set-Content -LiteralPath (Join-Path $OutputDirectory 'revocation-benchmark.json') -Encoding utf8NoBOM
    Write-Output 'Synthetic controls and local benchmark completed; JSON contains no tokens or connection strings.'
}
finally {
    $env:POSTGRES_PASSWORD = $previousPassword
    $env:MIKOMPRI_SPIKE_PG = $previousConnection
    if ($created) {
        $label = docker inspect $containerName --format '{{index .Config.Labels "mikompri.spike"}}'
        if ($LASTEXITCODE -eq 0 -and $label -eq '010-controlled') {
            docker rm --force $containerName | Out-Null
            if ($LASTEXITCODE -ne 0) { throw 'QA cleanup failed; remove the documented container manually.' }
            Write-Output 'Isolated PostgreSQL container removed (tmpfs only, no volume).'
        } else { throw 'QA ownership label mismatch; cleanup refused.' }
    }
}
