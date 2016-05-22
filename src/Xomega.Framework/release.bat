@echo off

SET VER=%1
SET TGT=%2
SET NUGET_PATH=.nuget\NuGet.exe
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe

IF '%VER%'=='' (
  echo Please use the following format: release.bat {version} [target]
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
IF EXIST "pkg\Xomega.Framework.%VER%" rd /q /s "pkg\Xomega.Framework.%VER%"
md "pkg\Xomega.Framework.%VER%"
call instrument.bat %TGT% %VER% net40
call instrument.bat %TGT% %VER% net45
call instrument.bat None %VER% sl5

@powershell (Get-Content -raw Package.nuspec) -replace '{version}', '%VER%' -replace '.*^<dependencies.*', ^
((Get-Content -raw packages.config) -replace '^.\?.*\n', '' ^
-replace 'packages', 'dependencies' -replace 'package', 'dependency' -replace ' targetFramework=\".*?\"', '' ^
-replace '(^<)', '________$1' -replace ' +_', '_____' -replace '_', ' ') > pkg\Package.nuspec

%NUGET_PATH% pack "pkg\Package.nuspec" -OutputDirectory "pkg\Xomega.Framework.%VER%"

:end