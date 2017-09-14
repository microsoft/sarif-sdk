#!/bin/bash
#@ECHO off
#SETLOCAL

Exit()
{
    echo
    echo script %~n0 failed
    return 1
}

GeneratedKey=./GeneratedKey.snk

if [ ! -f "$GeneratedKey" ]
then
    sn -k GeneratedKey.snk
fi

if [ $? != 0 ]
then
    echo "command sn -k failed"
    Exit
    return $?
fi

if [ ! -f "$GeneratedKey" ]
then
    echo "GeneratedKey.snk not found"
    Exit
    return $?
fi

dotnet restore src/Sarif.Sdk.sln

if [ $? != 0 ]
then
    echo nuget restore failed
    Exit
    return $?
fi