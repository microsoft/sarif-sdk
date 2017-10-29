#!/bin/bash
#@ECHO off
#SETLOCAL

Exit()
{
    echo
    echo script %~n0 failed
    return 1
}

dotnet restore src/Sarif.Sdk.sln

if [ $? != 0 ]
then
    echo nuget restore failed.
    Exit
    return $?
fi

dotnet msbuild /verbosity:minimal /target:BuildAndInjectObjectModel src/Sarif/Sarif.csproj /fileloggerparameters:Verbosity=detailed

if [ $? != 0 ]
then
    echo SARIF object model generation failed.
    Exit
    return $?
fi