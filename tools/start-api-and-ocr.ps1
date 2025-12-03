$repoRoot = Split-Path -Parent $PSScriptRoot
$apiJob = $null
$ocrJob = $null

try {
    $apiJob = Start-Job -Name "BookWiseApi" -ScriptBlock {
        Set-Location $using:repoRoot
        dotnet watch run --project .\src\backend\BookWise.Api\BookWise.Api.csproj
    }

    $ocrJob = Start-Job -Name "BookWiseOcr" -ScriptBlock {
        Set-Location $using:repoRoot
        dotnet run --project .\src\backend\BookWise.OcrWorker\BookWise.OcrWorker.csproj
    }

    Receive-Job -Job @($apiJob, $ocrJob) -Wait -AutoRemoveJob
}
finally {
    foreach ($job in @($apiJob, $ocrJob)) {
        if ($job -and $job.State -ne 'Completed') {
            Stop-Job $job -ErrorAction SilentlyContinue
        }
    }
}
