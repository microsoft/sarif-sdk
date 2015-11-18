Param(
    [string]$ToolName
)

$utility = "$env:ENLISTMENT_ROOT\bin\AnyCPU\Release\SarifSupport\ConvertToSarif.exe"

function Build-ConverterTool()
{
    Write-Host "Building the converter tool..."  -NoNewline
    # Out-Null *and* /noconsolelogger here because our scripts call out to batch files and similar
    # that don't respect msbuild's /noconsolelogger switch.
    msbuild $PSScriptRoot\..\ConvertToSarif\ConvertToSarif.csproj /p:Platform=AnyCPU`;Configuration=Release`;RunCodeAnalysis=false /m /noconsolelogger | Out-Null
    Write-Host " done."
}

function Build-Baselines($toolName)
{
    $sourceExtension = "xml"
    if ($ToolName -eq "ApiScan") {
        $sourceExtension = "csv";
    }

    Write-Host "Building baselines for $toolName..."
    $toolDirectory = Join-Path "$PSScriptRoot\SarifConverterTestData" $toolName
    $sourceExtension = "*.$sourceExtension"
    Get-ChildItem $toolDirectory -Filter $sourceExtension | ForEach-Object {
        Write-Host "    $_ -> $_.sarif"
        $input = $_.FullName
        $output = "$input.sarif"
        $outputTemp = "$output.temp"

        # Actually run the converter
        Remove-Item $outputTemp -ErrorAction SilentlyContinue
        &$utility -InputType $toolName -Input "$input" -Output "$outputTemp" -Pretty

        # Check if the output already exists.
        # If it does, we're updating an existing baseline. Do a `tf edit` on it.
        # If it does not exist, then we're doing a new baseline, so `tf add` it.
        $outExists = Test-Path $output
        if ($outExists)
        {
            $ErrorActionPreference = "SilentlyContinue"
            tf edit "$output" | Out-Null
            $ErrorActionPreference = "Continue"
        }

        Move-Item $outputTemp $output -Force

        if (-not $outExists)
        {
            $ErrorActionPreference = "SilentlyContinue"
            tf add "$output" | Out-Null
            $ErrorActionPreference = "Continue"
        }
    }
}

$allTools = (Get-ChildItem "$PSScriptRoot\SarifConverterTestData" -Directory | ForEach-Object { $_.Name })
if ($ToolName -and ($allTools -inotcontains $ToolName)) {
    Throw "Unrecognized tool name $ToolName"
}

Build-ConverterTool

if ($ToolName) {
    # Tool name provided; build only baselines for that tool.
    Build-Baselines $ToolName
} else {
    # No tool name provided; build all baselines.
    $allTools | ForEach-Object {
        Build-Baselines $_
    }
}

Write-Host "Finished! Terminate."
