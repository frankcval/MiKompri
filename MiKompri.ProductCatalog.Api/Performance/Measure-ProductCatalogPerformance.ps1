param(
	[string]$BaseUrl = "http://localhost:5081",
	[int]$Iterations = 20,
	[string]$OutputPath = "./MiKompri.ProductCatalog.Api/Performance/perf-results.json",
	[switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-Percentile {
	param(
		[double[]]$Values,
		[double]$Percentile
	)

	if (-not $Values -or $Values.Count -eq 0) {
		return 0
	}

	$sorted = $Values | Sort-Object
	$index = [Math]::Ceiling(($Percentile / 100) * $sorted.Count) - 1
	if ($index -lt 0) { $index = 0 }
	if ($index -ge $sorted.Count) { $index = $sorted.Count - 1 }

	return [Math]::Round([double]$sorted[$index], 2)
}

function Invoke-Timed {
	param(
		[scriptblock]$Action
	)

	$sw = [System.Diagnostics.Stopwatch]::StartNew()
	$result = & $Action
	$sw.Stop()

	return [PSCustomObject]@{
		Result = $result
		ElapsedSeconds = [Math]::Round($sw.Elapsed.TotalSeconds, 2)
	}
}

if ($DryRun) {
	Write-Host "[DRY-RUN] Script listo para medir ProductCatalog."
	Write-Host "BaseUrl=$BaseUrl Iterations=$Iterations OutputPath=$OutputPath"
	exit 0
}

Write-Host "Validando API en $BaseUrl ..."
try {
	Invoke-RestMethod -Method Get -Uri "$BaseUrl/health" | Out-Null
}
catch {
	throw "No se pudo conectar a $BaseUrl/health. Inicia MiKompri.ProductCatalog.Api antes de medir."
}

$productCreateTimes = New-Object System.Collections.Generic.List[double]
$productUpdateTimes = New-Object System.Collections.Generic.List[double]
$productDeactivateTimes = New-Object System.Collections.Generic.List[double]
$priceRegisterTimes = New-Object System.Collections.Generic.List[double]
$historyQueryTimes = New-Object System.Collections.Generic.List[double]

Write-Host "Creando mercado base para pruebas..."
$marketResponse = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/v1/markets" -ContentType "application/json" -Body (@{
	name = "Mercado Performance"
	locationHint = "Zona Benchmark"
} | ConvertTo-Json)

$marketId = [Guid]$marketResponse.id
if ($marketId -eq [Guid]::Empty) {
	throw "No se pudo obtener marketId del mercado base."
}

for ($i = 1; $i -le $Iterations; $i++) {
	$suffix = [Guid]::NewGuid().ToString('N').Substring(0, 8)
	$productName = "Producto Perf $i $suffix"

	$createTimed = Invoke-Timed {
		Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/v1/catalog-products" -ContentType "application/json" -Body (@{
			name = $productName
			purchaseUnit = "unit"
		} | ConvertTo-Json)
	}

	$productCreateTimes.Add($createTimed.ElapsedSeconds)

	$productId = [Guid]$createTimed.Result.id

	$updateTimed = Invoke-Timed {
		Invoke-RestMethod -Method Put -Uri "$BaseUrl/api/v1/catalog-products/$productId" -ContentType "application/json" -Body (@{
			name = "$productName Updated"
			purchaseUnit = "unit"
		} | ConvertTo-Json) | Out-Null
	}

	$productUpdateTimes.Add($updateTimed.ElapsedSeconds)

	$effectiveDate = [DateOnly]::FromDateTime([DateTime]::UtcNow.AddDays(-$i))

	$priceTimed = Invoke-Timed {
		Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/v1/product-prices" -ContentType "application/json" -Body (@{
			catalogProductId = $productId
			marketId = $marketId
			effectiveDate = $effectiveDate.ToString('yyyy-MM-dd')
			priceAmount = 1.0 + $i
		} | ConvertTo-Json)
	}

	$priceRegisterTimes.Add($priceTimed.ElapsedSeconds)

	$historyTimed = Invoke-Timed {
		Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/v1/catalog-products/$productId/price-history" | Out-Null
	}

	$historyQueryTimes.Add($historyTimed.ElapsedSeconds)

	$deactivateTimed = Invoke-Timed {
		Invoke-RestMethod -Method Patch -Uri "$BaseUrl/api/v1/catalog-products/$productId/deactivate" | Out-Null
	}

	$productDeactivateTimes.Add($deactivateTimed.ElapsedSeconds)
}

$summary = [PSCustomObject]@{
	baseUrl = $BaseUrl
	iterations = $Iterations
	measuredAtUtc = [DateTime]::UtcNow.ToString('o')
	metrics = [PSCustomObject]@{
		createProductP95Seconds = (Get-Percentile -Values $productCreateTimes.ToArray() -Percentile 95)
		updateProductP95Seconds = (Get-Percentile -Values $productUpdateTimes.ToArray() -Percentile 95)
		deactivateProductP95Seconds = (Get-Percentile -Values $productDeactivateTimes.ToArray() -Percentile 95)
		registerPriceP95Seconds = (Get-Percentile -Values $priceRegisterTimes.ToArray() -Percentile 95)
		priceHistoryP95Seconds = (Get-Percentile -Values $historyQueryTimes.ToArray() -Percentile 95)
	}
	successCriteria = [PSCustomObject]@{
		sc001 = [PSCustomObject]@{
			description = "95% de registros de productos completados en menos de 120 segundos"
			thresholdSeconds = 120
			measuredP95Seconds = (Get-Percentile -Values $productCreateTimes.ToArray() -Percentile 95)
			pass = ((Get-Percentile -Values $productCreateTimes.ToArray() -Percentile 95) -lt 120)
		}
		sc002 = [PSCustomObject]@{
			description = "95% de registros de precios válidos completados en menos de 30 segundos"
			thresholdSeconds = 30
			measuredP95Seconds = (Get-Percentile -Values $priceRegisterTimes.ToArray() -Percentile 95)
			pass = ((Get-Percentile -Values $priceRegisterTimes.ToArray() -Percentile 95) -lt 30)
		}
	}
}

$directory = Split-Path -Path $OutputPath -Parent
if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path $directory)) {
	New-Item -ItemType Directory -Path $directory | Out-Null
}

$summary | ConvertTo-Json -Depth 8 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "Medición completada."
Write-Host "SC-001 PASS: $($summary.successCriteria.sc001.pass) (P95=$($summary.successCriteria.sc001.measuredP95Seconds)s)"
Write-Host "SC-002 PASS: $($summary.successCriteria.sc002.pass) (P95=$($summary.successCriteria.sc002.measuredP95Seconds)s)"
Write-Host "Resultados: $OutputPath"
