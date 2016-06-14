@REM Remove existing build data
rd /s /q bld
md bld\bin\nuget

if NOT exist "GeneratedLey.snk" (
sn -k GeneratedKey.snk
)