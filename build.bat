@setlocal 
@set local=%~dp0

@pushd %WINDIR%\Microsoft.NET\Framework\v4.0.30319\
@goto build

:build
msbuild "%local%\Chuye.Persistent.sln" /t:Rebuild /P:Configuration=Release
@goto copy

:copy
robocopy "%local%src\Chuye.Persistent\bin\Release" "%local%release\Chuye.Persistent" /mir
robocopy "%local%src\Chuye.Persistent.NHibernate\bin\Release" "%local%release\Chuye.Persistent.NHibernate" /mir
@goto pack

:pack
@pushd "%local%"
.nuget\NuGet.exe pack build\Chuye.Persistent.nuspec -Prop Configuration=Release -OutputDirectory release
.nuget\NuGet.exe pack build\Chuye.Persistent.NHibernate.nuspec -Prop Configuration=Release -OutputDirectory release
@goto end

:end
@pushd %local%
@pause