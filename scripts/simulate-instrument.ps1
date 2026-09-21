<#
Stands in for a lab analyzer writing a result file.
Usage:
  ./scripts/simulate-instrument.ps1          # a well-formed file
  ./scripts/simulate-instrument.ps1 -Bad     # includes malformed lines
#>
param(
    [switch]$Bad
)
$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Drop = Join-Path $Root "instrument-drop"
$Stamp = Get-Date -Format "yyyyMMddHHmmss"
$Now = Get-Date -Format "yyyyMMddHHmm"

New-Item -ItemType Directory -Force -Path $Drop | Out-Null

if ($Bad) {
    $File = Join-Path $Drop "ANALYZER-02_$Stamp.txt"
    $Content = "1234|Test^B123^T3 Uptake|31|$Now`n" +
               "this line is not a valid message`n" +
               "9999|Test^X999^Unknown Barcode|12|$Now`n" +
               "5678|Test^U123^Nitrite|Trace|not-a-timestamp`n"
    [System.IO.File]::WriteAllText($File, $Content)
    Write-Host "Dropped malformed file: $File"
    Write-Host "Expect it in instrument-drop/_failed/ with a .error.txt beside it."
} else {
    $File = Join-Path $Drop "ANALYZER-01_$Stamp.txt"
    $Content = "1234|Test^B123^T3 Uptake|28|$Now`n" +
               "1234|Test^B124^TSH|2.1|$Now`n" +
               "5678|Test^U123^Nitrite|Absent|$Now`n" +
               "5678|Test^U124^Leukocytes|Negative|$Now`n"
    [System.IO.File]::WriteAllText($File, $Content)
    Write-Host "Dropped file: $File"
    Write-Host "Expect it in instrument-drop/_processed/ within ~5s, and results on /orders."
}
