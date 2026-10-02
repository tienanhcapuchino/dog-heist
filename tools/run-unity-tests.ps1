# Chạy Unity ở chế độ dòng lệnh để kiểm tra biên dịch hoặc chạy EditMode test mà không cần mở Editor.
# Cách dùng (Unity Editor phải đang ĐÓNG project này):
#   powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1
#   powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -Filter Greybox
#   powershell -ExecutionPolicy Bypass -File tools/run-unity-tests.ps1 -CompileOnly
param(
    [string]$UnityPath = $env:UNITY_EDITOR,
    [string]$Filter = "",
    [switch]$CompileOnly
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($UnityPath) -or -not (Test-Path $UnityPath))
{
    Write-Host "[Tests] Không tìm thấy Unity.exe. Đặt biến môi trường UNITY_EDITOR hoặc truyền -UnityPath."
    exit 2
}

# Unity giữ khóa độc quyền trên lockfile khi đang mở project. Lockfile còn sót mà không bị khóa
# (ví dụ batchmode dừng vì lỗi biên dịch) thì bỏ qua, Unity sẽ tự ghi đè.
$lockFile = Join-Path $projectPath "Temp/UnityLockfile"
if (Test-Path $lockFile)
{
    try
    {
        $stream = [IO.File]::Open($lockFile, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
        $stream.Close()
    }
    catch [IO.IOException]
    {
        Write-Host "[Tests] Unity Editor đang mở project này. Đóng Unity rồi chạy lại."
        exit 3
    }
}

$outDir = Join-Path $projectPath "Logs/TestRuns"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$logFile = Join-Path $outDir "unity.log"
$resultsFile = Join-Path $outDir "editmode-results.xml"
if (Test-Path $resultsFile)
{
    Remove-Item $resultsFile
}

$arguments = @("-batchmode", "-nographics", "-projectPath", "`"$projectPath`"", "-logFile", "`"$logFile`"")
if ($CompileOnly)
{
    $arguments += "-quit"
}
else
{
    $arguments += @("-runTests", "-testPlatform", "EditMode", "-testResults", "`"$resultsFile`"")
    if ($Filter)
    {
        $arguments += @("-testFilter", $Filter)
    }
}

$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow

$compileErrors = @()
if (Test-Path $logFile)
{
    $compileErrors = Select-String -Path $logFile -Pattern "error CS\d+"
}

if ($compileErrors.Count -gt 0)
{
    Write-Host "[Tests] Lỗi biên dịch:"
    $compileErrors | ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique | ForEach-Object { Write-Host "  $_" }
    exit 1
}

if ($CompileOnly)
{
    Write-Host "[Tests] Biên dịch không lỗi (Unity exit code $($process.ExitCode))."
    exit $process.ExitCode
}

if (-not (Test-Path $resultsFile))
{
    Write-Host "[Tests] Không có file kết quả. Xem log: $logFile"
    exit 1
}

[xml]$results = Get-Content -Path $resultsFile -Encoding UTF8
$run = $results.'test-run'
Write-Host "[Tests] Tổng $($run.total) | Đạt $($run.passed) | Lỗi $($run.failed) | Bỏ qua $($run.skipped)"
foreach ($case in $results.SelectNodes("//test-case[@result='Failed']"))
{
    Write-Host "  FAIL $($case.fullname)"
    Write-Host "       $($case.SelectSingleNode('failure/message').InnerText)"
}

if ([int]$run.failed -gt 0)
{
    exit 1
}

exit 0
