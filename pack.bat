@echo off
call nuget restore src\NRoles.sln
call msbuild src\Build.proj /t:Build
call nuget pack src\NRoles\NRoles.csproj -Properties Configuration=Debug %*
