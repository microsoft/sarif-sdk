SETLOCAL

  set INCLUDE=%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\include;%INCLUDE%
  set CL="%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\bin\amd64\cl.exe"

  for /R %%i in (*.cpp) do %CL% -c -analyze -analyze:quiet- -W4 -analyze:log %%~ni.cpp.xml %%i
  for /R %%i in (*.cxx) do %CL% -c -analyze -analyze:quiet- -W4 -analyze:log %%~ni.cxx.xml %%i
  del /q *.obj

ENDLOCAL