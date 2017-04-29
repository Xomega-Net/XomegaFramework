@echo off

SET VER=%1
SET TGT=%2
SET EPT=%3

IF '%VER%'=='' (
  echo Please use the following format: release.bat {version} [target]
  goto end
)
call release Xomega.Framework %VER% %TGT% %EPT%
call release Xomega.Framework.Wcf %VER% %TGT% %EPT%
call release Xomega.Framework.Web %VER% %TGT% %EPT%
call release Xomega.Framework.Wpf %VER% %TGT% %EPT%

IF EXIST "pkg\lib\" rd /q /s "pkg\lib"
cd pkg
del Package.nuspec

:end