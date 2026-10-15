@echo off
chcp 65001 >nul
cd /d "%~dp0"
if not exist _b64_full (
  echo Folder _b64_full not found. Run git pull first.
  pause
  exit /b 1
)
powershell -NoProfile -Command "Get-ChildItem '_b64_full\*.b64' | ForEach-Object { $n = $_.BaseName; $data = Get-Content $_.FullName -Raw; [IO.File]::WriteAllBytes($n, [Convert]::FromBase64String($data)); Write-Host ('OK ' + $n) }"
echo.
echo All docx restored into this folder. You can delete _b64_full after that.
pause
