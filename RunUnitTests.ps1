$targetFrameworks = @(
    #@{ Name = "netcoreapp2.0"; NetCoreCompatible = $True },
    @{ Name = "net452"; NetCoreCompatible = $False }
)

$testProjects = @(
    @{ Name = "Sarif.Converters.UnitTests"; IsMultiTargeting = $True },
    @{ Name = "Sarif.UnitTests"; IsMultiTargeting = $True },
    @{ Name = "Sarif.Driver.UnitTests"; IsMultiTargeting = $True },
    @{ Name = "Sarif.FunctionalTests"; IsMultiTargeting = $True },
    @{ Name = "Sarif.Multitool.FunctionalTests"; IsMultiTargeting = $True },
    @{ Name = "Sarif.ValidationTests"; IsMultiTargeting = $False },
    @{ Name = "Sarif.Viewer.VisualStudio.UnitTests"; IsMultiTargeting = $False }
)

Foreach ($project in $testProjects)
{
    If ($project.IsMultiTargeting)
    {
        Foreach ($framework in $targetFrameworks)
        {
            $toolPath = ".\src\packages\xunit.runner.console.2.3.0\tools\$($framework.Name)\xunit.console"
            $projectPath = ".\bld\bin\$($project.Name)\AnyCPU_Release\$($framework.Name)\$($project.Name).dll"

            Write-Host "Running Tests For $($project.Name): $($framework.Name)" -ForegroundColor Cyan

			If ($framework.NetCoreCompatible)
			{
				Invoke-Expression "dotnet $toolPath.dll $projectPath -appveyor"
			}
			else
			{
				Invoke-Expression "$toolPath.exe $projectPath -appveyor"
			}

            Write-Host ""
            Write-Host ""

			If ($LastExitCode -ne 0)
			{
				exit 1
			}
        }
    }
    else
    {
        Write-Host "Running Tests For $($project.Name)" -ForegroundColor Cyan

        $toolPath = ".\src\packages\xunit.runner.console.2.3.0\tools\net452\xunit.console.exe"
        Invoke-Expression "$toolPath .\bld\bin\$($project.Name)\AnyCPU_Release\$($project.Name).dll -appveyor"

        Write-Host ""
        Write-Host ""

        If ($LastExitCode -ne 0)
		{
		    exit 1
		}
    }
}