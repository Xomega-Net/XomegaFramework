@echo off

set TGT=%1
set VER=%2
set INDIR=bin\Debug\%3\
set DLL=Xomega.Framework
set OUT=%INDIR:bin\Debug=pkg\lib%

IF NOT EXIST "%OUT%" md "%OUT%"
copy "%INDIR%%DLL%.xml" "%OUT%%DLL%.xml" >nul

IF %TGT%==nuget (
  set COMPKEY=44A1A421-EE24-4391-92ED-3C5CE2C4CC69
  set COMPNAME=NuGet
) ELSE IF %TGT%==xomega (
  set COMPKEY=F44EA787-0629-4964-ABAD-A3DFE7D60800
  set COMPNAME=Xomega.Net
) ELSE IF %TGT%==cplex (
  set COMPKEY=3E35F098-CE43-4F82-9E9D-05C8B1046A45
  set COMPNAME=CodePlex
) ELSE (
  copy "%INDIR%%DLL%.dll" "%OUT%%DLL%.dll" >nul
  echo Skipping instrumentation for %INDIR%.
  goto end
)

set DOTFCMD=%VS100COMNTOOLS%..\..\PreEmptive Solutions\Dotfuscator Community Edition\dotfuscatorCLI.exe

IF NOT EXIST "%DOTFCMD%" (
  copy "%INDIR%%DLL%.dll" "%OUT%%DLL%.dll" >nul
  echo Dotfuscator CE 5.0.2601 is needed to run instrumentation.
  goto end
)

@powershell (Get-Content -raw Xomega.Dotfuscator.xml) -replace '{version}', '%VER%' -replace '{dllDir}', '%INDIR%' ^
-replace '{companyKey}', '%COMPKEY%' -replace '{companyName}', '%COMPNAME%' > Dotfuscator.xml

"%DOTFCMD%" Dotfuscator.xml -in:%INDIR%%DLL%.dll -out:%OUT% >nul
del Dotfuscator.xml

:end
