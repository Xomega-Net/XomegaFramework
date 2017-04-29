@echo off

set PRJ=%1
set TGT=%2
set VER=%3
set INDIR=%PRJ%\bin\Debug\%4\
set OUT=pkg\lib\%4\
set ENDPOINT=%5

IF NOT EXIST "%OUT%" md "%OUT%"
copy "%INDIR%%PRJ%.xml" "%OUT%%PRJ%.xml" >nul

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
  copy "%INDIR%%PRJ%.dll" "%OUT%%PRJ%.dll" >nul
  echo Skipping instrumentation for %INDIR%.
  goto end
)

set DOTFCMD=%VS100COMNTOOLS%..\..\PreEmptive Solutions\Dotfuscator Community Edition\dotfuscatorCLI.exe

IF NOT EXIST "%DOTFCMD%" (
  copy "%INDIR%%PRJ%.dll" "%OUT%%PRJ%.dll" >nul
  echo Dotfuscator CE 5.0.2601 is needed to run instrumentation.
  goto end
)

@powershell (Get-Content -raw %PRJ%\Xomega.Dotfuscator.xml) -replace '{version}', '%VER%' -replace '{dllDir}', '%INDIR%' ^
-replace '{endpoint}', '%ENDPOINT%' -replace '{companyKey}', '%COMPKEY%' -replace '{companyName}', '%COMPNAME%' > Dotfuscator.xml

"%DOTFCMD%" Dotfuscator.xml -in:%INDIR%%PRJ%.dll -out:%OUT% >nul
del Dotfuscator.xml

:end
