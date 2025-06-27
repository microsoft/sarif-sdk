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
# net462 is this minimal support .NET framework. We allow linkage
# to net461 for compatibility reasons.
# and any upstream consumer of them.
$Frameworks.NetFx = @("net461")

# Frameworks for which we build libraries.
$Frameworks.Library = @("netstandard2.0") + $Frameworks.NetFx

# net462 is the current minimum supported .NET
# framework, so we use it for all client tools.
# it is fine to support a down-level, unsupported
# .NET framework for a library, because the client 
# app can then control timing for updating to a more
# current .NET framework. When we control the app
# we will require the minimal supported version to 
# ensure that the runtime we are executing on is secure.
$Frameworks.ApplicationNetFx = @("net462")

# Frameworks for which we build applications.
$Frameworks.Application = @("net8.0") + $Frameworks.ApplicationNetFx

$Frameworks.All = ($Frameworks.Library + $Frameworks.Application | Select -Unique)

$Projects = @{}

$Projects.Libraries = @(
    "Sarif",
    "Sarif.Converters",
    "Sarif.Driver",
    "Sarif.Multitool.Library"
    "Sarif.WorkItems",
    "WorkItems"
)

$Projects.Applications = @(
    "Sarif.Multitool"
    )

$Projects.Products = $Projects.Libraries + $Projects.Applications

$Projects.Tests = @(
    "Test.EndToEnd.Baselining"
    "Test.FunctionalTests.Sarif",
    "Test.UnitTests.Sarif",
    "Test.UnitTests.Sarif.Converters",
    "Test.UnitTests.Sarif.Driver",
    "Test.UnitTests.Sarif.Multitool",
    "Test.UnitTests.Sarif.Multitool.Library",
    "Test.UnitTests.Sarif.WorkItems",
    "Test.Utilities.Sarif"
    )

$Projects.All = $Projects.Products + $Projects.Tests

Export-ModuleMember -Variable Frameworks, Projects