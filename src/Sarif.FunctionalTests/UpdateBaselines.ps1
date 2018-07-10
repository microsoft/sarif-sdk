Param(
    [string]$ToolName
)

$utility = "$PSScriptRoot\..\..\bld\bin\AnyCPU_Release\net452\Sarif.Multitool.exe"

function Build-ConverterTool()
{
    Write-Host "Building the converter tool..."  -NoNewline
    # Out-Null *and* /noconsolelogger here because our scripts call out to batch files and similar
    # that don't respect msbuild's /noconsolelogger switch.
    msbuild $PSScriptRoot\..\Sarif.Multitool\Sarif.Multitool.csproj /p:Platform=AnyCPU`;Configuration=Release`;RunCodeAnalysis=false /m /noconsolelogger | Out-Null
    Write-Host " done."
}

function Build-Baselines($toolName)
{
    $sourceExtension = "xml"

    if ($ToolName -eq "StaticDriverVerifier")
    {
      $sourceExtension = "tt"
    }

    if ($ToolName -eq "SemmleQL")
    {
      $sourceExtension = "csv"
    }

    if ($ToolName -eq "Pylint")
    {
      $sourceExtension = "json"
    }

    if ($ToolName -eq "TSLint")
    {
      $sourceExtension = "json"
    }

    Write-Host "Building baselines for $toolName..."
    $toolDirectory = Join-Path "$PSScriptRoot\v2\ConverterTestData" $toolName
    $sourceExtension = "*.$sourceExtension"
    Get-ChildItem $toolDirectory -Filter $sourceExtension | ForEach-Object {
        Write-Host "    $_ -> $_.sarif"
        $input = $_.FullName
        $output = "$input.sarif"
        $outputTemp = "$output.temp"
		
        # Actually run the converter
        Remove-Item $outputTemp -ErrorAction SilentlyContinue
        & Write-Host "$utility convert ""$input"" --tool $toolName --output ""$outputTemp"" --pretty-print --insert TextFiles"
        &$utility convert "$input" --tool $toolName --output "$outputTemp" --pretty-print --insert TextFiles

        # Next, perform some rewriting. The PREfast converter in particular cannot embed file contents as the source      
        # SARIF emitted by the compiler does not contain the optional 'files' member of the 'run' object.
        Write-Host "$utility rewrite ""$outputTemp"" --output ""$outputTemp"" --pretty-print --insert ""TextFiles;Hashes"" --force"
        &$utility rewrite ""$outputTemp"" --output ""$outputTemp"" --pretty-print --insert "TextFiles;Hashes" --force

        Move-Item $outputTemp $output -Force
    }
}

$allTools = (Get-ChildItem "$PSScriptRoot\v2\ConverterTestData" -Directory | ForEach-Object { $_.Name })
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
