# RunCat Neo — Claude Code statusLine sample (PowerShell variant for Windows).
#
# Same behavior as runcat-statusline.py, without requiring Python: writes
# %USERPROFILE%\.claude\runcat-usage.json shaped like:
#
#     {
#       "title": "Claude Code",
#       "symbol": "staroflife",
#       "metricsBarValue": "67%",
#       "metrics": [
#         {"title": "Model",   "formattedValue": "Opus 4.7"},
#         {"title": "Context", "formattedValue": "67%", "normalizedValue": 0.67},
#         {"title": "5h",      "formattedValue": "3%",  "normalizedValue": 0.03},
#         {"title": "7d",      "formattedValue": "3%",  "normalizedValue": 0.03}
#       ],
#       "lastUpdatedDate": "2026-06-07T05:55:36Z"
#     }

$invariant = [System.Globalization.CultureInfo]::InvariantCulture

$outFile = if ($env:RUNCAT_OUT_FILE) {
    $env:RUNCAT_OUT_FILE
} else {
    Join-Path $HOME ".claude\runcat-usage.json"
}

try {
    $payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
} catch {
    $payload = $null
}

$model = $payload.model.display_name
if (-not $model) { $model = "Claude Code" }
$context = $payload.context_window.used_percentage
$fiveHour = $payload.rate_limits.five_hour.used_percentage
$sevenDay = $payload.rate_limits.seven_day.used_percentage

function New-PercentMetric($title, $value) {
    if ($null -eq $value) { return $null }
    [ordered]@{
        title           = $title
        formattedValue  = ([double]$value).ToString($invariant) + "%"
        normalizedValue = [math]::Round([double]$value / 100, 4)
    }
}

$metrics = [System.Collections.Generic.List[object]]::new()
$metrics.Add([ordered]@{ title = "Model"; formattedValue = [string]$model })
foreach ($metric in @(
    (New-PercentMetric "Context" $context),
    (New-PercentMetric "5h" $fiveHour),
    (New-PercentMetric "7d" $sevenDay)
)) {
    if ($null -ne $metric) { $metrics.Add($metric) }
}

$snapshot = [ordered]@{
    title           = "Claude Code"
    symbol          = "staroflife"
    metrics         = $metrics
    lastUpdatedDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", $invariant)
}
if ($null -ne $context) {
    $snapshot.metricsBarValue = ([double]$context).ToString($invariant) + "%"
}

$json = ConvertTo-Json -InputObject $snapshot -Depth 5 -Compress

# Write atomically: temp file in the same directory, then replace.
$directory = Split-Path -Parent $outFile
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$tempFile = Join-Path $directory (".runcat-" + [guid]::NewGuid().ToString("N") + ".tmp")
[System.IO.File]::WriteAllText($tempFile, $json, [System.Text.UTF8Encoding]::new($false))
Move-Item -Force -Path $tempFile -Destination $outFile

Write-Output $model
