@echo off

SET PRJ=%1
SET VER=%2
SET TGT=%3
SET EPT=%4
SET NUGET_PATH=.nuget\NuGet.exe
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe

IF '%PRJ%'=='' (
  echo Please use the following format: release.bat {project} {version} [target]
  goto end
)

IF '%TGT%'=='' SET TGT=None


IF NOT EXIST .nuget md .nuget
IF NOT EXIST %NUGET_PATH% (
  IF NOT EXIST %CACHED_NUGET% (
    echo Downloading latest version of NuGet.exe...
    IF NOT EXIST %LocalAppData%\NuGet ( 
      md %LocalAppData%\NuGet
    )
    @powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/%NUGET_VERSION%/nuget.exe' -OutFile '%CACHED_NUGET%'"
  )

  copy %CACHED_NUGET% %NUGET_PATH% > nul
)

IF EXIST "pkg\lib\" rd /q /s "pkg\lib"
IF EXIST "pkg\%TGT%\%PRJ%.%VER%" rd /q /s "pkg\%TGT%\%PRJ%.%VER%"
md "pkg\%TGT%\%PRJ%.%VER%"
call instrument.bat %PRJ% %TGT% %VER% net45 %EPT%

@powershell (Get-Content -raw %PRJ%\Package.nuspec) -replace '{version}', '%VER%' > pkg\Package.nuspec

%NUGET_PATH% pack "pkg\Package.nuspec" -OutputDirectory "pkg\%TGT%\%PRJ%.%VER%"

:end