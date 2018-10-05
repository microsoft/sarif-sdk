Param(
    [string]$ToolName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$utility = "$PSScriptRoot\..\..\bld\bin\AnyCPU_Release\Sarif.Multitool\net461\Sarif.Multitool.exe"

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

    if ($ToolName -eq "FortifyFpr")
    {
      $sourceExtension = "fpr"
    }

    if ($ToolName -eq "FxCopProject")
    {
      $sourceExtension = "fxcop"
      $toolName = "FxCop"
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
        &$utility convert "$input" --tool $toolName --output "$outputTemp" --pretty-print
        # Next, perform some rewriting. The PREfast converter in particular cannot embed file contents as the source      
        # SARIF emitted by the compiler does not contain the optional 'files' member of the 'run' object.
        &$utility rewrite ""$outputTemp"" --output ""$outputTemp"" --pretty-print --insert """TextFiles;Hashes;RegionSnippets;ContextRegionSnippets""" --force

        Move-Item $outputTemp $output -Force
    }
}

$allTools = (Get-ChildItem "$PSScriptRoot\v2\ConverterTestData" -Directory | ForEach-Object { $_.Name })
if ($ToolName -and $ToolName -ne "FxCopProject" -and ($allTools -inotcontains $ToolName)) {
    Throw "Unrecognized tool name $ToolName"
}

$EnlistmentRoot = "D:\src\sarif-sdk\"
$EnlistmentDrive = Split-Path $EnlistmentRoot -Qualifier
$EnlistmentDirectory = $EnlistmentRoot.Substring($EnlistmentDrive.Length)

if (-not ($PSCommandPath.StartsWith($EnlistmentRoot, [System.StringComparison]::OrdinalIgnoreCase))) {
    $message = `
        "ERROR: For this script to work, your enlistment in the sarif-sdk repo must be rooted`n" +
        "in the directory $EnlistmentRoot. The reason is that the script injects sarif-sdk`n" +
        "source file content into some of the log files, and the source file paths specified`n" +
        "in those log files are under $EnlistmentRoot.`n`n" +
        "If your enlistment is on a drive other than $EnlistmentDrive (for example, if it is on C:),`n" +
        "you can give the command:`n`n" +
        "    net use $EnlistmentDrive \\$env:COMPUTERNAME\C$ D:`n`n" +
        "If your $EnlistmentDrive drive is already in use (for example, if it is assigned to a CD-ROM drive),`n" +
        "you will need to use the Windows Disk Management UI to reassign the drive letter.`n" +
        "In any case, the enlistment root directory must be $EnlistmentDirectory."
    Write-Information $message
    exit 1
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
