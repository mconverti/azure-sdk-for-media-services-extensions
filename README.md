Extensions for Windows Azure Media Services .NET SDK
====================================================

## What is it?
A NuGet package that contains a set of extension methods and helpers for the Windows Azure Media Services SDK for .NET.

## Usage
Install the [WindowsAzure.MediaServices.Extensions Nuget package](https://www.nuget.org/packages/WindowsAzure.MediaServices.Extensions) by running `Install-Package WindowsAzure.MediaServices.Extensions` in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console/).

After installing the package, a **MediaServicesExtensions** folder will be added to your project's root directory containing the following two files:
- MediaServicesExtensions.cs: Contains useful extension methods for the interfaces and classes in _Microsoft.WindowsAzure.MediaServices.Client_ namespace. 
- MediaServicesExceptionParser.cs: Contains helper methods to parse Windows Azure Media Services error messages in XML format.

