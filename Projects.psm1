<#
.SYNOPSIS
    Provides a list of SARIF SDK projects.
.DESCRIPTION
    The Projects module exports an object whose properties contain arrays of the
    various kinds of projects in the SARIF SDK.
#>

$Projects = @{}

# Projects built with the VS 2017 project system.
$Projects.NewProduct = @(
    "Sarif",
    "Sarif.Converters",
    "Sarif.Driver",
    "Sarif.Multitool"
    )

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

Export-ModuleMember -Variable Projects