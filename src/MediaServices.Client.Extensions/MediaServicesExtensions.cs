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

    /// <summary>
    /// Contains extension methods for the interfaces and classes in <see cref="Microsoft.WindowsAzure.MediaServices.Client"/> namespace.
    /// </summary>
    public static class MediaServicesExtensions
    {
        /// <summary>
        /// Represents the manifest file extension.
        /// </summary>
        public const string ManifestFileExtension = ".ism";

        /// <summary>
        /// Represents the URL dynamic packaging parameter for HLS.
        /// </summary>
        public const string HlsStreamingParameter = "(format=m3u8-aapl)";

        /// <summary>
        /// Represents the URL dynamic packaging parameter for MPEG-DASH.
        /// </summary>
        public const string MpegDashStreamingParameter = "(format=mpd-time-csf)";

        private const string BaseProgramUrlTemplate = "{0}/{1}/manifest{2}";

        private static readonly TimeSpan DefaultAccessPolicyDuration = TimeSpan.FromDays(1);

        #region Locator extensions

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;ILocator&gt;"/> instance for new <see cref="ILocator"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance for the new <see cref="ILocator"/>.</param>
        /// <param name="locatorType">The <see cref="LocatorType"/>.</param>
        /// <param name="permissions">The <see cref="AccessPermissions"/> of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <param name="duration">The duration of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <param name="startTime">The start time of the new <see cref="ILocator"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;ILocator&gt;"/> instance for new <see cref="ILocator"/>.</returns>
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

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;ILocator&gt;"/> instance for new <see cref="ILocator"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance for the new <see cref="ILocator"/>.</param>
        /// <param name="locatorType">The <see cref="LocatorType"/>.</param>
        /// <param name="permissions">The <see cref="AccessPermissions"/> of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <param name="duration">The duration of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;ILocator&gt;"/> instance for new <see cref="ILocator"/>.</returns>
        public static Task<ILocator> CreateLocatorAsync(this CloudMediaContext context, IAsset asset, LocatorType locatorType, AccessPermissions permissions, TimeSpan duration)
        {
            return context.CreateLocatorAsync(asset, locatorType, permissions, duration, null);
        }

        /// <summary>
        /// Returns a new <see cref="ILocator"/> instance.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance for the new <see cref="ILocator"/>.</param>
        /// <param name="locatorType">The <see cref="LocatorType"/>.</param>
        /// <param name="permissions">The <see cref="AccessPermissions"/> of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <param name="duration">The duration of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <param name="startTime">The start time of the new <see cref="ILocator"/>.</param>
        /// <returns>A a new <see cref="ILocator"/> instance.</returns>
        public static ILocator CreateLocator(this CloudMediaContext context, IAsset asset, LocatorType locatorType, AccessPermissions permissions, TimeSpan duration, DateTime? startTime)
        {
            using (Task<ILocator> task = context.CreateLocatorAsync(asset, locatorType, permissions, duration, startTime))
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Returns a new <see cref="ILocator"/> instance.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance for the new <see cref="ILocator"/>.</param>
        /// <param name="locatorType">The <see cref="LocatorType"/>.</param>
        /// <param name="permissions">The <see cref="AccessPermissions"/> of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <param name="duration">The duration of the <see cref="IAccessPolicy"/> associated with the new <see cref="ILocator"/>.</param>
        /// <returns>A a new <see cref="ILocator"/> instance.</returns>
        public static ILocator CreateLocator(this CloudMediaContext context, IAsset asset, LocatorType locatorType, AccessPermissions permissions, TimeSpan duration)
        {
            return context.CreateLocator(asset, locatorType, permissions, duration, null);
        }

        #endregion

        #region Asset extensions

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for the new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for new <see cref="IAsset"/>.</returns>
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

            BlobTransferClient blobTransferClient = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = context.NumberOfConcurrentTransfers,
                ParallelTransferThreadCount = context.ParallelTransferThreadCount
            };

            await asset.CreateAssetFileFromLocalFileAsync(filePath, blobTransferClient, sasLocator, cancellationToken);

            await sasLocator.DeleteAsync();

            return asset;
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for new the <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for new <see cref="IAsset"/>.</returns>
        public static Task<IAsset> CreateAssetFromFileAsync(this CloudMediaContext context, string filePath, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFileAsync(filePath, null, options, cancellationToken);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/>.</returns>
        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, string storageAccountName, AssetCreationOptions options)
        {
            using (Task<IAsset> task = context.CreateAssetFromFileAsync(filePath, storageAccountName, options, CancellationToken.None))
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/>.</returns>
        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, AssetCreationOptions options)
        {
            return context.CreateAssetFromFile(filePath, null, options);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for the new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for new <see cref="IAsset"/>.</returns>
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

            BlobTransferClient blobTransferClient = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = context.NumberOfConcurrentTransfers,
                ParallelTransferThreadCount = context.ParallelTransferThreadCount
            };

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

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for the new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for new <see cref="IAsset"/>.</returns>
        public static Task<IAsset> CreateAssetFromFolderAsync(this CloudMediaContext context, string folderPath, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFolderAsync(folderPath, null, options, cancellationToken);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/>.</returns>
        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, string storageAccountName, AssetCreationOptions options)
        {
            using (Task<IAsset> task = context.CreateAssetFromFolderAsync(folderPath, storageAccountName, options, CancellationToken.None))
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/>.</returns>
        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, AssetCreationOptions options)
        {
            return context.CreateAssetFromFolder(folderPath, null, options);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task"/> instance to generate <see cref="IAssetFile"/> for the <paramref name="asset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance where to generate its <see cref="IAssetFile"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to generate <see cref="IAssetFile"/>.</returns>
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

        /// <summary>
        /// Generates <see cref="IAssetFile"/> for the <paramref name="asset"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance where to generate its <see cref="IAssetFile"/>.</param>
        public static void CreateAssetFiles(this CloudMediaContext context, IAsset asset)
        {
            using (Task task = context.CreateAssetFilesAsync(asset))
            {
                task.Wait();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task"/> instance to download all the asset files in the <paramref name="asset"/> to the <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance where to download the asset files.</param>
        /// <param name="folderPath">The path to the folder where to download the asset files in the <paramref name="asset"/>.</param>
        /// <param name="downloadProgressChangedCallback">A callback to report download progress for each asset file in the <paramref name="asset"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> instance to download all the asset files in the <paramref name="asset"/>.</returns>
        public static async Task DownloadAssetFilesToFolderAsync(this CloudMediaContext context, IAsset asset, string folderPath, Action<IAssetFile, DownloadProgressChangedEventArgs> downloadProgressChangedCallback, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "The context cannot be null.");
            }

            if (asset == null)
            {
                throw new ArgumentNullException("asset", "The asset cannot be null.");
            }

            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "The folder '{0}' does not exist.", folderPath),
                    "folderPath");
            }

            ILocator sasLocator = await context.CreateLocatorAsync(asset, LocatorType.Sas, AccessPermissions.Read, DefaultAccessPolicyDuration);

            EventHandler<DownloadProgressChangedEventArgs> downloadProgressChangedHandler =
                (s, e) =>
                {
                    IAssetFile assetFile = (IAssetFile)s;
                    DownloadProgressChangedEventArgs eventArgs = e;

                    if (downloadProgressChangedCallback != null)
                    {
                        downloadProgressChangedCallback(assetFile, eventArgs);
                    }
                };

            List<Task> downloadTasks = new List<Task>();
            List<IAssetFile> assetFiles = asset.AssetFiles.ToList();
            foreach (IAssetFile assetFile in assetFiles)
            {
                string localDownloadPath = Path.Combine(folderPath, assetFile.Name);
                BlobTransferClient blobTransferClient = new BlobTransferClient
                {
                    NumberOfConcurrentTransfers = context.NumberOfConcurrentTransfers,
                    ParallelTransferThreadCount = context.ParallelTransferThreadCount
                };

                assetFile.DownloadProgressChanged += downloadProgressChangedHandler;

                downloadTasks.Add(
                    assetFile.DownloadAsync(Path.GetFullPath(localDownloadPath), blobTransferClient, sasLocator, cancellationToken));
            }

            await Task.WhenAll(downloadTasks);

            await sasLocator.DeleteAsync();

            assetFiles.ForEach(af => af.DownloadProgressChanged -= downloadProgressChangedHandler);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task"/> instance to download all the asset files in the <paramref name="asset"/> to the <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance where to download the asset files.</param>
        /// <param name="folderPath">The path to the folder where to download the asset files in the <paramref name="asset"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> instance to download all the asset files in the <paramref name="asset"/>.</returns>
        public static Task DownloadAssetFilesToFolderAsync(this CloudMediaContext context, IAsset asset, string folderPath, CancellationToken cancellationToken)
        {
            return context.DownloadAssetFilesToFolderAsync(asset, folderPath, null, cancellationToken);
        }

        /// <summary>
        /// Downloads all the asset files in the <paramref name="asset"/> to the <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance where to download the asset files.</param>
        /// <param name="folderPath">The path to the folder where to download the asset files in the <paramref name="asset"/>.</param>
        /// <param name="downloadProgressChangedCallback">A callback to report download progress for each asset file in the <paramref name="asset"/>.</param>
        public static void DownloadAssetFilesToFolder(this CloudMediaContext context, IAsset asset, string folderPath, Action<IAssetFile, DownloadProgressChangedEventArgs> downloadProgressChangedCallback)
        {
            using (Task task = context.DownloadAssetFilesToFolderAsync(asset, folderPath, downloadProgressChangedCallback, CancellationToken.None))
            {
                task.Wait();
            }
        }

        /// <summary>
        /// Downloads all the asset files in the <paramref name="asset"/> to the <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="asset">The <see cref="IAsset"/> instance where to download the asset files.</param>
        /// <param name="folderPath">The path to the folder where to download the asset files in the <paramref name="asset"/>.</param>
        public static void DownloadAssetFilesToFolder(this CloudMediaContext context, IAsset asset, string folderPath)
        {
            context.DownloadAssetFilesToFolder(asset, folderPath, null);
        }

        /// <summary>
        /// Returns the <see cref="IAssetFile"/> instance that represents the manifest file of the asset; otherwise, null.
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>A <see cref="IAssetFile"/> instance that represents the manifest file of the asset; otherwise, null.</returns>
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

        /// <summary>
        /// Returns the Smooth Streaming URL. 
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the Smooth Streaming URL.</returns>
        public static Uri GetSmoothStreamingUri(this IAsset asset)
        {
            return asset.GetStreamingUri(string.Empty);
        }

        /// <summary>
        /// Returns the HLS URL. 
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the HLS URL.</returns>
        public static Uri GetHlsUri(this IAsset asset)
        {
            return asset.GetStreamingUri(HlsStreamingParameter);
        }

        /// <summary>
        /// Returns the MPEG-DASH URL. 
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the MPEG-DASH URL.</returns>
        public static Uri GetMpegDashUri(this IAsset asset)
        {
            return asset.GetStreamingUri(MpegDashStreamingParameter);
        }

        #endregion

        #region Job extensions

        /// <summary>
        /// Returns the latest version of the <see cref="IMediaProcessor"/> by its <paramref name="mediaProcessorName"/>. 
        /// </summary>
        /// <param name="mediaProcessorCollection">The <see cref="MediaProcessorBaseCollection"/> instance.</param>
        /// <param name="mediaProcessorName">The name of the media processor.</param>
        /// <returns>The latest version of the <see cref="IMediaProcessor"/> by its <paramref name="mediaProcessorName"/>.</returns>
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

        /// <summary>
        /// Returns a <see cref="IJob"/> instance with a single <see cref="ITask"/> ready to be submitted.
        /// </summary>
        /// <param name="context">The <see cref="MediaContextBase"/> instance.</param>
        /// <param name="mediaProcessorName">The name of the media processor.</param>
        /// <param name="taskConfiguration">The task configuration.</param>
        /// <param name="inputAsset">The input <see cref="IAsset"/> instance.</param>
        /// <param name="outputAssetName">The name of the output asset.</param>
        /// <param name="outputAssetStorageAccountName">The name of the Storage Account where to store the output asset.</param>
        /// <param name="outputAssetOptions">The <see cref="AssetCreationOptions"/> of the output asset.</param>
        /// <returns>A <see cref="IJob"/> instance with a single <see cref="ITask"/> ready to be submitted.</returns>
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

        /// <summary>
        /// Returns a <see cref="IJob"/> instance with a single <see cref="ITask"/> ready to be submitted.
        /// </summary>
        /// <param name="context">The <see cref="MediaContextBase"/> instance.</param>
        /// <param name="mediaProcessorName">The name of the media processor.</param>
        /// <param name="taskConfiguration">The task configuration.</param>
        /// <param name="inputAsset">The input <see cref="IAsset"/> instance.</param>
        /// <param name="outputAssetName">The name of the output asset.</param>
        /// <param name="outputAssetOptions">The <see cref="AssetCreationOptions"/> of the output asset.</param>
        /// <returns>A <see cref="IJob"/> instance with a single <see cref="ITask"/> ready to be submitted.</returns>
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
