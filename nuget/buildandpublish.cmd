@ECHO OFF

SETLOCAL
%~d0
CD "%~dp0"

SET NuGetPath=..\src\.nuget\NuGet.exe

MKDIR windowsazure.mediaservices.extensions\content\net40\MediaServicesExtensions
COPY ..\src\MediaServices.Client.Extensions\*.cs windowsazure.mediaservices.extensions\content\net40\MediaServicesExtensions

%NuGetPath% Update -self
%NuGetPath% Pack windowsazure.mediaservices.extensions\windowsazure.mediaservices.extensions.nuspec
%NuGetPath% Push windowsazure.mediaservices.extensions.1.0.6.nupkg

PAUSE