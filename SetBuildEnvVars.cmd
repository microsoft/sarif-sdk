rem SetBuildEnvVars.cmd
rem
rem This script sets certain environment variables that are used in both BeforeBuild.cmd
rem and BuildAndTest.cmd. Setting them here avoids code duplication.

@ECHO off

rem Projects built with the VS 2017 project system.
set NewProductProjects=Sarif Sarif.Converters Sarif.Driver Sarif.Multitool
set NewTestProjects=Sarif.UnitTests Sarif.Converters.UnitTests Sarif.Driver.UnitTests Sarif.ValidationTests Sarif.FunctionalTests Sarif.Multitool.FunctionalTests
set NewProjects=%NewProductProjects% %NewTestProjects%

rem Projects built with the old project system.
set OldProductProjects=Sarif.Viewer.VisualStudio
set OldTestProjects=Sarif.Viewer.VisualStudio.UnitTests
set OldProjects=%OldProductProjects% %OldTestProjects%
