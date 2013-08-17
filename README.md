Extensions for Windows Azure Media Services .NET SDK
====================================================

## What is it?
A NuGet package that contains a set of extension methods and helpers for the Windows Azure Media Services SDK for .NET.

## Usage
Install the [WindowsAzure.MediaServices.Extensions Nuget package](https://www.nuget.org/packages/WindowsAzure.MediaServices.Extensions) by running `Install-Package WindowsAzure.MediaServices.Extensions` in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console/).

After installing the package, a **MediaServicesExtensions** folder will be added to your project's root directory containing the following two files:
- MediaServicesExtensions.cs: Contains useful extension methods for the interfaces and classes in _Microsoft.WindowsAzure.MediaServices.Client_ namespace. 
- MediaServicesExceptionParser.cs: Contains helper methods to parse Windows Azure Media Services error messages in XML format.

## Extension Methods and Helpers available

### Create a Locator
Create a locator and its associated access policy using a single extension method for the [CloudMediaContext](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.cloudmediacontext.aspx) class. There are additional overloads with different parameters and _async_ support.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The asset used to create the locator. Get a reference to it from the context.
IAsset asset = null;

// The locator type.
LocatorType locatorType = LocatorType.OnDemandOrigin;

// The permissions for the locator's access policy.
AccessPermissions accessPolicyPermissions = AccessPermissions.Read | AccessPermissions.List;

// The duration for the locator's access policy.
TimeSpan accessPolicyDuration = TimeSpan.FromDays(30);

// Create a locator and its associated access policy using a single extension method.
ILocator locator = context.CreateLocator(asset, locatorType, accessPolicyPermissions, accessPolicyDuration);
```

### Create an Asset from a single local file
Create a new asset by uploading a local file using a single extension method for the [CloudMediaContext](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.cloudmediacontext.aspx) class. There are additional overloads with different parameters and _async_ support.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The local path to the file to upload to the new asset.
string filePath = @"C:\AssetFile.wmv";

// The options for creating the new asset.
AssetCreationOptions assetCreationOptions = AssetCreationOptions.None;

// Create a new asset and upload a local file using a single extension method.
IAsset asset = context.CreateAssetFromFile(filePath, assetCreationOptions);
```

### Create an Asset from a local folder
Create a new asset by uploading all the files in a local folder using a single extension method for the [CloudMediaContext](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.cloudmediacontext.aspx) class. There are additional overloads with different parameters and _async_ support.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The path to an existing local folder with the files to upload to the new asset.
string folderPath = @"C:\AssetFilesFolder";

// The options for creating the new asset.
AssetCreationOptions assetCreationOptions = AssetCreationOptions.None;

// Create a new asset and upload all the files in a local folder using a single extension method.
IAsset asset = context.CreateAssetFromFolder(folderPath, assetCreationOptions);
```

### Generate Asset Files from Blob storage
Generate the asset files of an existing asset using a single extension method for the [CloudMediaContext](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.cloudmediacontext.aspx) class. You can use this method after uploading content directly to the asset container in Blob storage. This method leverages the [CreateFileInfos REST API Function](http://msdn.microsoft.com/library/windowsazure/jj683097.aspx#createfileinfos). There is an additional overload with _async_ support.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// Create an empty asset.
IAsset asset = context.Assets.Create("MyAssetName", AssetCreationOptions.None);

// Upload content to the previous asset directly to its Blob storage container.
// You can use a SAS locator with Write permissions to do this.
// ...

// Generate all the asset files in the asset from its Blob storage container using a single extension method.
context.CreateAssetFiles(asset);
```

### Download Asset Files to a local folder
Download all the asset files in an asset using a single extension method for the [CloudMediaContext](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.cloudmediacontext.aspx) class. There are additional overloads with different parameters and _async_ support.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The asset with the asset files to download. Get a reference to it from the context.
IAsset asset = null;

// The path to an existing local folder where to download all the asset files in the asset.
string folderPath = @"C:\AssetFilesFolder";

// Download all the asset files to a local folder using a single extension method.
context.DownloadAssetFilesToFolder(asset, folderPath);
```

### Get manifest Asset File
Get a reference to the asset file that represents the ISM manifest using a single extension method for the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface.
```csharp
// The asset with multi-bitrate content. Get a reference to it from the context.
IAsset asset = null;

// Get the asset file representing the ISM manifest.
IAssetFile manifestAssetFile = asset.GetManifestAssetFile();
```

### Get Smooth Streaming URL
Get the Smooth Streaming URL of a multi-bitrate Smooth Streaming or MP4 asset using a single extension method for the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface. This methods requires the asset to contain an ISM manifest asset file and that you previously created an Origin locator for the asset; otherwise it returns _null_.
```csharp
// The asset with multi-bitrate Smooth Streaming or MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Get the Smooth Streaming URL for the asset.
Uri smoothStreamingUri = asset.GetSmoothStreamingUri();
```

### Get HLS URL
Get the HLS URL of a multi-bitrate Smooth Streaming or MP4 asset using a single extension method for the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface. This methods requires the asset to contain an ISM manifest asset file and that you previously created an Origin locator for the asset; otherwise it returns _null_.
```csharp
// The asset with multi-bitrate Smooth Streaming or MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Get the HLS URL for the asset.
Uri hlsUri = asset.GetHlsUri();
```

### Get MPEG-DASH URL
Get the MPEG-DASH URL of a multi-bitrate Smooth Streaming or MP4 asset using a single extension method for the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface. This methods requires the asset to contain an ISM manifest asset file and that you previously created an Origin locator for the asset; otherwise it returns _null_.
```csharp
// The asset with multi-bitrate Smooth Streaming or MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Get the MPEG-DASH URL for the asset.
Uri mpegDashUri = asset.GetMpegDashUri();
```

## Get the latest Media Processor by name
Get the latest version of a media processor filtering by its name using a single extension method for the [MediaProcessorBaseCollection](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.mediaprocessorbasecollection.aspx) class.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// Get the latest version of a media processor by its name using a single extension method.
IMediaProcessor processor = context.MediaProcessors.GetLatestMediaProcessorByName("Windows Azure Media Encoder");
```

## Prepare a Job with a single Task
Prepare a job with a single task ready to be submitted using a single extension method for the [MediaContextBase](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.mediacontextbase.aspx) class. There is an additional overload with different parameters. 
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The media processor name used in the job's task.
string mediaProcessorName = "Windows Azure Media Encoder";

// The task configuration.
string taskConfiguration = "H264 Smooth Streaming 720p";

// The input asset for the task.  Get a reference to it from the context.
IAsset inputAsset = null;

// The name for the output asset of the task.
string outputAssetName = "OutputAssetName";

// The options for creating the output asset of the task.
AssetCreationOptions outputAssetOptions = AssetCreationOptions.None;

// Prepare a job ready to be submitted with a single task with one input/output asset using a single extension method.
IJob job = context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, inputAsset, outputAssetName, outputAssetOptions);

// Submit the job and wait until it is completed to get the output asset.
// ...
```

## Parse Media Services error messages in XML format
Parse exceptions with Windows Azure Media Services error messages in XML format.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");
            
// Create an empty asset.
IAsset asset = context.Assets.Create("MyAssetName", AssetCreationOptions.None);

try
{
    // Generate an error trying to delete the asset twice.
    asset.Delete();
    asset.Delete();
}
catch (Exception exception)
{
    // Parse the exception to get the error message from the Media Services XML response.
    Exception parsedException = MediaServicesExceptionParser.Parse(exception);
    string mediaServicesErrorMessage = parsedException.Message;
}
```
