// <copyright file="MediaServicesExtensions.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by Mariano Converti
//   
//  Redistribution and use in source and binary forms, with or without modification, are permitted.
//
//  The names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class MediaServicesExtensions
    {
        public const string ManifestFileExtension = ".ism";
        public const string HlsStreamingParameter = "(format=m3u8-aapl)";
        public const string MpegDashStreamingParameter = "(format=mpd-time-csf)";

        public static readonly TimeSpan DefaultAccessPolicyDuration = TimeSpan.FromDays(1);

        private const string BaseProgramUrlTemplate = "{0}/{1}/manifest{2}";

        #region Locator extensions

        public static async Task<ILocator> CreateLocatorAsync(this CloudMediaContext context, IAsset asset, LocatorType locatorType, AccessPermissions permissions, TimeSpan duration, DateTime? startTime)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "The context cannot be null.");
            }

            if (asset == null)
            {
                throw new ArgumentNullException("asset", "The asset cannot be null.");
            }

            var policy = await context.AccessPolicies.CreateAsync(asset.Name, duration, permissions);

            return await context.Locators.CreateLocatorAsync(locatorType, asset, policy, startTime);
        }

        public static Task<ILocator> CreateLocatorAsync(this CloudMediaContext context, IAsset asset, LocatorType locatorType, AccessPermissions permissions, TimeSpan duration)
        {
            return context.CreateLocatorAsync(asset, locatorType, permissions, duration, null);
        }

        public static ILocator CreateLocator(this CloudMediaContext context, IAsset asset, LocatorType locatorType, AccessPermissions permissions, TimeSpan duration, DateTime? startTime)
        {
            using (Task<ILocator> task = context.CreateLocatorAsync(asset, locatorType, permissions, duration, startTime))
            {
                return task.Result;
            }
        }

        public static ILocator CreateLocator(this CloudMediaContext context, IAsset asset, LocatorType locatorType, AccessPermissions permissions, TimeSpan duration)
        {
            return context.CreateLocator(asset, locatorType, permissions, duration, null);
        }

        #endregion

        #region Asset extensions

        public static async Task<IAsset> CreateAssetFromFileAsync(this CloudMediaContext context, string filePath, string storageAccountName, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "The context cannot be null.");
            }

            string assetName = Path.GetFileName(filePath);

            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                storageAccountName = context.DefaultStorageAccount.Name;
            }

            IAsset asset = await context.Assets.CreateAsync(assetName, storageAccountName, options, cancellationToken);

            ILocator sasLocator = await context.CreateLocatorAsync(asset, LocatorType.Sas, AccessPermissions.Write | AccessPermissions.List, DefaultAccessPolicyDuration);

            BlobTransferClient blobTransferClient = new BlobTransferClient();
            blobTransferClient.NumberOfConcurrentTransfers = context.NumberOfConcurrentTransfers;
            blobTransferClient.ParallelTransferThreadCount = context.ParallelTransferThreadCount;

            await asset.CreateAssetFileFromLocalFileAsync(filePath, blobTransferClient, sasLocator, cancellationToken);

            await sasLocator.DeleteAsync();

            return asset;
        }

        public static Task<IAsset> CreateAssetFromFileAsync(this CloudMediaContext context, string filePath, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFileAsync(filePath, null, options, cancellationToken);
        }

        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, string storageAccountName, AssetCreationOptions options)
        {
            using (Task<IAsset> task = context.CreateAssetFromFileAsync(filePath, storageAccountName, options, CancellationToken.None))
            {
                return task.Result;
            }
        }

        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, AssetCreationOptions options)
        {
            return context.CreateAssetFromFile(filePath, null, options);
        }

        public static async Task<IAsset> CreateAssetFromFolderAsync(this CloudMediaContext context, string folderPath, string storageAccountName, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "The context cannot be null.");
            }

            IEnumerable<string> filePaths = Directory.EnumerateFiles(folderPath);
            if (!filePaths.Any())
            {
                throw new FileNotFoundException(
                    string.Format(CultureInfo.InvariantCulture, "No files in directory, check the folder path: '{0}'", folderPath));
            }

            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                storageAccountName = context.DefaultStorageAccount.Name;
            }

            string assetName = Path.GetFileName(Path.GetFullPath(folderPath.TrimEnd('\\')));
            IAsset asset = await context.Assets.CreateAsync(assetName, storageAccountName, options, cancellationToken);

            ILocator sasLocator = await context.CreateLocatorAsync(asset, LocatorType.Sas, AccessPermissions.Write | AccessPermissions.List, DefaultAccessPolicyDuration);

            BlobTransferClient blobTransferClient = new BlobTransferClient();
            blobTransferClient.NumberOfConcurrentTransfers = context.NumberOfConcurrentTransfers;
            blobTransferClient.ParallelTransferThreadCount = context.ParallelTransferThreadCount;

            IList<Task> uploadTasks = new List<Task>();
            foreach (string filePath in filePaths)
            {
                uploadTasks.Add(
                    asset.CreateAssetFileFromLocalFileAsync(filePath, blobTransferClient, sasLocator, cancellationToken));
            }

            await Task.WhenAll(uploadTasks);

            await sasLocator.DeleteAsync();

            return asset;
        }

        public static Task<IAsset> CreateAssetFromFolderAsync(this CloudMediaContext context, string folderPath, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFolderAsync(folderPath, null, options, cancellationToken);
        }

        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, string storageAccountName, AssetCreationOptions options)
        {
            using (Task<IAsset> task = context.CreateAssetFromFolderAsync(folderPath, storageAccountName, options, CancellationToken.None))
            {
                return task.Result;
            }
        }

        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, AssetCreationOptions options)
        {
            return context.CreateAssetFromFolder(folderPath, null, options);
        }

        public static async Task CreateAssetFilesAsync(this CloudMediaContext context, IAsset asset)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "The context cannot be null.");
            }

            if (asset == null)
            {
                throw new ArgumentNullException("asset", "The asset cannot be null.");
            }

            Uri uriCreateFileInfos = new Uri(
                string.Format(CultureInfo.InvariantCulture, "/CreateFileInfos?assetid='{0}'", asset.Id),
                UriKind.Relative);

            await context
                .DataContextFactory
                .CreateDataServiceContext()
                .ExecuteAsync(uriCreateFileInfos, null, "GET");
        }

        public static void CreateAssetFiles(this CloudMediaContext context, IAsset asset)
        {
            using (Task task = context.CreateAssetFilesAsync(asset))
            {
                task.Wait();
            }
        }

        public static IAssetFile GetManifestAssetFile(this IAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("asset", "The asset cannot be null.");
            }

            return asset
                .AssetFiles
                .ToList()
                .Where(af => af.Name.EndsWith(ManifestFileExtension, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        public static Uri GetSmoothStreamingUri(this IAsset asset)
        {
            return asset.GetStreamingUri(string.Empty);
        }

        public static Uri GetHlsUri(this IAsset asset)
        {
            return asset.GetStreamingUri(HlsStreamingParameter);
        }

        public static Uri GetMpegDashUri(this IAsset asset)
        {
            return asset.GetStreamingUri(MpegDashStreamingParameter);
        }

        #endregion

        #region Job extensions

        public static IMediaProcessor GetLatestMediaProcessorByName(this MediaProcessorBaseCollection mediaProcessorCollection, string mediaProcessorName)
        {
            if (mediaProcessorCollection == null)
            {
                throw new ArgumentNullException("mediaProcessorCollection", "The media processor collection cannot be null.");
            }

            return mediaProcessorCollection
                .Where(mp => mp.Name == mediaProcessorName)
                .ToList()
                .OrderBy(mp => new Version(mp.Version))
                .LastOrDefault();
        }

        public static IJob PrepareJobWithSingleTask(this MediaContextBase context, string mediaProcessorName, string taskConfiguration, IAsset inputAsset, string outputAssetName, string outputAssetStorageAccountName, AssetCreationOptions outputAssetOptions)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "The context cannot be null.");
            }

            if (inputAsset == null)
            {
                throw new ArgumentNullException("inputAsset", "The input asset cannot be null.");
            }

            IMediaProcessor processor = context.MediaProcessors.GetLatestMediaProcessorByName(mediaProcessorName);

            if (processor == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "Unknown media processor: '{0}'", mediaProcessorName), "mediaProcessorName");
            }

            IJob job = context.Jobs.Create(
                string.Format(CultureInfo.InvariantCulture, "Job for {0}", inputAsset.Name));

            ITask task = job.Tasks.AddNew(
                string.Format(CultureInfo.InvariantCulture, "Task for {0}", inputAsset.Name),
                processor,
                taskConfiguration,
                TaskOptions.ProtectedConfiguration);

            task.InputAssets.Add(inputAsset);

            if (string.IsNullOrWhiteSpace(outputAssetStorageAccountName))
            {
                outputAssetStorageAccountName = context.DefaultStorageAccount.Name;
            }

            task.OutputAssets.AddNew(outputAssetName, outputAssetStorageAccountName, outputAssetOptions);

            return job;
        }

        public static IJob PrepareJobWithSingleTask(this MediaContextBase context, string mediaProcessorName, string taskConfiguration, IAsset inputAsset, string outputAssetName, AssetCreationOptions outputAssetOptions)
        {
            return context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, inputAsset, outputAssetName, null, outputAssetOptions);
        }

        #endregion

        private static async Task<IAssetFile> CreateAssetFileFromLocalFileAsync(this IAsset asset, string filePath, BlobTransferClient blobTransferClient, ILocator sasLocator, CancellationToken cancellationToken)
        {
            string assetFileName = Path.GetFileName(filePath);
            IAssetFile assetFile = await asset.AssetFiles.CreateAsync(assetFileName, cancellationToken);

            await assetFile.UploadAsync(filePath, blobTransferClient, sasLocator, cancellationToken);

            if (assetFileName.EndsWith(ManifestFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                assetFile.IsPrimary = true;
                await assetFile.UpdateAsync();
            }

            return assetFile;
        }

        private static Uri GetStreamingUri(this IAsset asset, string streamingParameter)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("asset", "The asset cannot be null.");
            }

            Uri smoothStreamingUri = null;
            IAssetFile manifestAssetFile = asset.GetManifestAssetFile();
            if (manifestAssetFile != null)
            {
                var originLocator = asset.Locators.ToList().Where(l => l.Type == LocatorType.OnDemandOrigin).FirstOrDefault();
                if (originLocator != null)
                {
                    smoothStreamingUri = new Uri(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            BaseProgramUrlTemplate,
                            originLocator.Path.TrimEnd('/'),
                            manifestAssetFile.Name,
                            streamingParameter),
                        UriKind.Absolute);
                }
            }

            return smoothStreamingUri;
        }
    }
}
