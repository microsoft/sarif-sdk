$targetFrameworks = @(
    "netcoreapp2.0",
    "net452"
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
            %xunit20%\
        }
    }
    else
    {

    }
}