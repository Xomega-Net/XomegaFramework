@echo off

SET PRJ=%1
SET VER=%2
SET TGT=nuget
SET NUGET_PATH=.nuget\NuGet.exe
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe

IF '%PRJ%'=='' (
  echo Please use the following format: push_nuget.bat {project} {version}
  goto end
)


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

%NUGET_PATH% push "pkg\nuget\%PRJ%.%VER%\%PRJ%.%VER%.nupkg" -Source https://api.nuget.org/v3/index.json

:end