<#
.SYNOPSIS
    Provides a list of SARIF SDK projects and frameworks.
.DESCRIPTION
    The Projects module exports variables whose properties specify the
    various kinds of projects in the SARIF SDK, and the frameworks for which
    they are built.
#>

$Frameworks = @{}

# .NET Framework versions for which we build.
$Frameworks.NetFx = @("net461")

# Frameworks for which we build libraries.
$Frameworks.Library = @("netstandard2.0") + $Frameworks.NetFx

# Frameworks for which we build applications.
$Frameworks.Application = @("netcoreapp2.0") + $Frameworks.NetFx

$Frameworks.All = ($Frameworks.Library + $Frameworks.Application | Select -Unique)

$Projects = @{}

# Projects built with the VS 2017 project system.
$Projects.NewLibrary = @(
    "Sarif",
    "Sarif.Converters",
    "Sarif.Driver"
)

$Projects.NewApplication = @(
    "Sarif.Multitool"
    )

$Projects.NewProduct = $Projects.NewLibrary + $Projects.NewApplication

$Projects.NewTest = @(
    "Sarif.UnitTests",
    "Sarif.Converters.UnitTests",
    "Sarif.Driver.UnitTests",
    "Sarif.ValidationTests",
    "Sarif.FunctionalTests",
    "Sarif.Multitool.FunctionalTests"
    )

$Projects.New = $Projects.NewProduct + $Projects.NewTest

# Projects built with the old project system.
$Projects.OldProduct = @("Sarif.Viewer.VisualStudio")
$Projects.OldTest = @("Sarif.Viewer.VisualStudio.UnitTests")
$Projects.Old = $Projects.OldProduct + $Projects.OldTest

$Projects.All = $Projects.New + $Projects.Old

Export-ModuleMember -Variable Frameworks, Projects