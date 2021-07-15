@echo off

SET VER=%1

IF '%VER%'=='' (
  echo Please use the following format: push_all.bat {version}
  goto end
)

call push_nuget.bat Xomega.Framework %VER%
call push_nuget.bat Xomega.Framework.AspNetCore %VER%
call push_nuget.bat Xomega.Framework.Blazor %VER%
call push_nuget.bat Xomega.Framework.Wcf %VER%
call push_nuget.bat Xomega.Framework.Web %VER%
call push_nuget.bat Xomega.Framework.Wpf %VER%
call push_nuget.bat Xomega.Syncfusion.Blazor %VER%

:end