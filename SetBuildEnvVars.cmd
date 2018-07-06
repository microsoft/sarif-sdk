:: SetBuildEnvVars.cmd
::
:: This script sets certain environment variables that are used in both BeforeBuild.cmd
:: and BuildAndTest.cmd. Setting them here avoids code duplication.

@ECHO off

set CrossPlatformProductProjects=Sarif Sarif.Converters Sarif.Driver Sarif.Multitool
set CrossPlatformTestProjects=Sarif.UnitTests Sarif.Converters.UnitTests Sarif.Driver.UnitTests Sarif.ValidationTests Sarif.FunctionalTests Sarif.Multitool.FunctionalTests
set CrossPlatformProjects=%CrossPlatformProductProjects% %CrossPlatformTestProjects%
