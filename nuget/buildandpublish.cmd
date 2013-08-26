@ECHO OFF

SETLOCAL
%~d0
CD "%~dp0"

mkdir content\net40\MediaServicesExtensions
copy ..\src\MediaServices.Client.Extensions\*.cs content\net40\MediaServicesExtensions

NuGet Update -self
NuGet Pack windowsazure.mediaservices.extensions.nuspec
NuGet Push windowsazure.mediaservices.extensions.1.0.4.nupkg

PAUSE