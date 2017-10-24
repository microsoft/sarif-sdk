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
    echo nuget restore failed
    Exit
    return $?
fi