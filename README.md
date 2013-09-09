Extensions for Windows Azure Media Services .NET SDK
====================================================

## What is it?
A NuGet package that contains a set of extension methods and helpers for the Windows Azure Media Services SDK for .NET.

## Usage
Install the [WindowsAzure.MediaServices.Extensions Nuget package](https://www.nuget.org/packages/WindowsAzure.MediaServices.Extensions) by running `Install-Package WindowsAzure.MediaServices.Extensions` in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console/).

After installing the package, a **MediaServicesExtensions** folder will be added to your project's root directory containing the following files:
- AssetExtensions.cs: Contains useful extension methods and helpers related to the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface.
- JobExtensions.cs: Contains useful extension methods and helpers related to the [IJob](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.ijob.aspx) interface.
- LocatorExtensions.cs: Contains useful extension methods and helpers related to the [ILocator](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.ilocator.aspx) interface.
- UrlExtensionsFixture.cs: Contains extension methods and helpers related to the [Uri](http://msdn.microsoft.com/library/system.uri.aspx) and [String](http://msdn.microsoft.com/library/system.string.aspx) classes.
- MediaServicesExceptionParser.cs: Contains helper methods to parse Windows Azure Media Services error messages in XML format.
- MediaEncoderTaskPresetStrings.cs: Contains constants with the names of the available [Task Preset Strings for the Windows Azure Media Encoder](http://msdn.microsoft.com/en-us/library/windowsazure/jj129582.aspx).
- MediaProcessorNames.cs: Contains constants with the names of the available [Media Processors](http://msdn.microsoft.com/en-us/library/windowsazure/jj129580.aspx).

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

### Get Smooth Streaming URL for Asset
Get the Smooth Streaming URL of a multi-bitrate Smooth Streaming or MP4 asset using a single extension method for the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface. This methods requires the asset to contain an ISM manifest asset file and that you previously created an Origin locator for the asset; otherwise it returns _null_.
```csharp
// The asset with multi-bitrate Smooth Streaming or MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Make sure to create an Origin locator for the asset.

// Get the Smooth Streaming URL of the asset for adaptive streaming.
Uri smoothStreamingUri = asset.GetSmoothStreamingUri();
```

### Get HLS URL for Asset
Get the HLS URL of a multi-bitrate Smooth Streaming or MP4 asset using a single extension method for the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface. This methods requires the asset to contain an ISM manifest asset file and that you previously created an Origin locator for the asset; otherwise it returns _null_.
```csharp
// The asset with multi-bitrate Smooth Streaming or MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Make sure to create an Origin locator for the asset.

// Get the HLS URL of the asset for adaptive streaming.
Uri hlsUri = asset.GetHlsUri();
```

### Get MPEG-DASH URL for Asset
Get the MPEG-DASH URL of a multi-bitrate Smooth Streaming or MP4 asset using a single extension method for the [IAsset](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iasset.aspx) interface. This methods requires the asset to contain an ISM manifest asset file and that you previously created an Origin locator for the asset; otherwise it returns _null_.
```csharp
// The asset with multi-bitrate Smooth Streaming or MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Make sure to create an Origin locator for the asset.

// Get the MPEG-DASH URL of the asset for adaptive streaming.
Uri mpegDashUri = asset.GetMpegDashUri();
```

### Get SAS URL for Asset File
Get the SAS URL of an asset file for progressive download using a single extension method for the [IAssetFile](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.iassetfile.aspx) interface. This methods requires the parent asset to contain a SAS locator for the asset; otherwise it returns _null_.
```csharp
// The asset with multi-bitrate MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Make sure to create a SAS locator for the asset.

IAssetFile assetFile = asset.AssetFiles.ToList().Where(af => af.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)).First();

// Get the SAS URL of the asset file for progressive download.
Uri sasUri = assetFile.GetSasUri();
```

### Save Uri to file
Save an Uri to a local file using a extension method for the [Uri](http://msdn.microsoft.com/library/system.uri.aspx) class. It creates the file if does not exist; otherwise, appends a new line to the end.
```csharp
// The asset with multi-bitrate MP4 content. Get a reference to it from the context.
IAsset asset = null;

// Make sure to create an Origin locator for the asset.
// Make sure to create a SAS locator for the asset.

IEnumerable<IAssetFile> mp4AssetFiles = asset
        .AssetFiles
        .ToList()
        .Where(af => af.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));

// Get the URL's for the asset.
Uri smoothStreamingUri = asset.GetSmoothStreamingUri();
Uri hlsUri = asset.GetHlsUri();
Uri mpegDashUri = asset.GetMpegDashUri();
List<Uri> mp4ProgressiveDownloadUris = mp4AssetFiles.Select(af => af.GetSasUri()).ToList();

string filePath = @"C:\asset-urls.txt";

// Save the URL's to a file.
smoothStreamingUri.Save(filePath);
hlsUri.Save(filePath);
mpegDashUri.Save(filePath);
mp4ProgressiveDownloadUris.ForEach(uri => uri.Save(filePath));
```

### Get latest Media Processor by name
Get the latest version of a media processor filtering by its name using a single extension method for the [MediaProcessorBaseCollection](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.mediaprocessorbasecollection.aspx) class.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// Get the latest version of a media processor by its name using a single extension method.
IMediaProcessor processor = context.MediaProcessors.GetLatestMediaProcessorByName(MediaProcessorNames.WindowsAzureMediaEncoder);
```

### Prepare a Job with a single Task
Prepare a job with a single task ready to be submitted using a single extension method for the [MediaContextBase](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.mediacontextbase.aspx) class. There is an additional overload with different parameters. 
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The media processor name used in the job's task.
string mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;

// The task configuration.
string taskConfiguration = MediaEncoderTaskPresetStrings.H264AdaptiveBitrateMP4Set720p;

// The input asset for the task. Get a reference to it from the context.
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

### Get Job overall progress
Get the overall progress of a job by calculating the average progress of all its tasks using a single extension method for the [IJob](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.ijob.aspx) interface.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The input asset for the task. Get a reference to it from the context.
IAsset inputAsset = null;

// Prepare a job ready to be submitted with a single task with one input/output asset using a single extension method.
IJob job = context.PrepareJobWithSingleTask(MediaProcessorNames.WindowsAzureMediaEncoder, MediaEncoderTaskPresetStrings.H264AdaptiveBitrateMP4Set720p, inputAsset, "OutputAssetName", AssetCreationOptions.None);

// Submit the job.
job.Submit();

// ...

// Refresh the job instance.
job = context.Jobs.Where(j => j.Id == job.Id).First();

// Get the overall progress of the job by calculating the average progress of all its tasks using a single extension method.
double jobOverallProgress = job.GetOverallProgress();
```

### Start Job execution progress task to notify when its state or overall progress change
Start a [Task](http://msdn.microsoft.com/library/system.threading.tasks.task.aspx) to monitor a job progress using a single extension method for the [IJob](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.ijob.aspx) interface. The difference with the [IJob.GetExecutionProgressTask](http://msdn.microsoft.com/library/microsoft.windowsazure.mediaservices.client.ijob.getexecutionprogresstask.aspx) method is that this extension invokes a callback when the job state or overall progress change. There is an additional overload with different parameters.
```csharp
CloudMediaContext context = new CloudMediaContext("%accountName%", "%accountKey%");

// The input asset for the task. Get a reference to it from the context.
IAsset inputAsset = null;

// Prepare a job ready to be submitted with a single task with one input/output asset using a single extension method.
IJob job = context.PrepareJobWithSingleTask(MediaProcessorNames.WindowsAzureMediaEncoder, MediaEncoderTaskPresetStrings.H264AdaptiveBitrateMP4Set720p, inputAsset, "OutputAssetName", AssetCreationOptions.None);

// Submit the job.
job.Submit();

// Start a task to monitor the job progress by invoking a callback when its state or overall progress change in a single extension method.
job = await context.StartExecutionProgressTask(
    job,
    j =>
    {
        Console.WriteLine("Current job state: {0}", j.State);
        Console.WriteLine("Current job progress: {0}", j.GetOverallProgress());
    },
    CancellationToken.None);
```

### Parse Media Services error messages in XML format
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
