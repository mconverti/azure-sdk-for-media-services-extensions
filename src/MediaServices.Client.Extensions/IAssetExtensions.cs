// <copyright file="IAssetExtensions.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by mconverti
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
    /// Contains extension methods and helpers for the <see cref="IAsset"/> interface.
    /// </summary>
    public static class IAssetExtensions
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

        private const string BaseStreamingUrlTemplate = "{0}/{1}/manifest{2}";

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task"/> instance to generate <see cref="IAssetFile"/> for the <paramref name="asset"/>.
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance where to generate its <see cref="IAssetFile"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to generate <see cref="IAssetFile"/>.</returns>
        public static async Task GenerateFromStorageAsync(this IAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("asset", "The asset cannot be null.");
            }

            MediaContextBase context = asset.GetMediaContext();

            Uri uriCreateFileInfos = new Uri(
                string.Format(CultureInfo.InvariantCulture, "/CreateFileInfos?assetid='{0}'", asset.Id),
                UriKind.Relative);

            await context
                .MediaServicesClassFactory
                .CreateDataServiceContext()
                .ExecuteAsync(uriCreateFileInfos, null, "GET");
        }

        /// <summary>
        /// Generates <see cref="IAssetFile"/> for the <paramref name="asset"/>.
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance where to generate its <see cref="IAssetFile"/>.</param>
        public static void GenerateFromStorage(this IAsset asset)
        {
            using (Task task = asset.GenerateFromStorageAsync())
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

            ILocator sasLocator = await context.Locators.CreateAsync(LocatorType.Sas, asset, AccessPermissions.Read, AssetBaseCollectionExtensions.DefaultAccessPolicyDuration);

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

            await Task.Factory.ContinueWhenAll(downloadTasks.ToArray(), t => t, TaskContinuationOptions.ExecuteSynchronously);

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
        /// Returns the Smooth Streaming URL of the <paramref name="asset"/>; otherwise, null.
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the Smooth Streaming URL of the <paramref name="asset"/>; otherwise, null.</returns>
        public static Uri GetSmoothStreamingUri(this IAsset asset)
        {
            return asset.GetStreamingUri(string.Empty);
        }

        /// <summary>
        /// Returns the HLS URL of the <paramref name="asset"/>; otherwise, null.
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the HLS URL of the <paramref name="asset"/>; otherwise, null.</returns>
        public static Uri GetHlsUri(this IAsset asset)
        {
            return asset.GetStreamingUri(HlsStreamingParameter);
        }

        /// <summary>
        /// Returns the MPEG-DASH URL of the <paramref name="asset"/>; otherwise, null.
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the MPEG-DASH URL of the <paramref name="asset"/>; otherwise, null.</returns>
        public static Uri GetMpegDashUri(this IAsset asset)
        {
            return asset.GetStreamingUri(MpegDashStreamingParameter);
        }

        /// <summary>
        /// Returns the SAS URL of the <paramref name="assetFile"/> for progressive download; otherwise, null.
        /// </summary>
        /// <param name="assetFile">The <see cref="IAssetFile"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the SAS URL of the <paramref name="assetFile"/> for progressive download; otherwise, null.</returns>
        public static Uri GetSasUri(this IAssetFile assetFile)
        {
            if (assetFile == null)
            {
                throw new ArgumentNullException("assetFile", "The asset file cannot be null.");
            }

            Uri sasUri = null;
            IAsset asset = assetFile.Asset;
            if (asset != null)
            {
                ILocator sasLocator = asset
                    .Locators
                    .ToList()
                    .Where(l => l.Type == LocatorType.Sas)
                    .OrderBy(l => l.ExpirationDateTime)
                    .LastOrDefault();
                if (sasLocator != null)
                {
                    UriBuilder builder = new UriBuilder(new Uri(sasLocator.Path, UriKind.Absolute));
                    builder.Path = Path.Combine(builder.Path, assetFile.Name);

                    sasUri = builder.Uri;
                }
            }

            return sasUri;
        }

        /// <summary>
        /// Returns the parent <see cref="MediaContextBase"/> instance.
        /// </summary>
        /// <param name="asset">The <see cref="IAsset"/> instance.</param>
        /// <returns>The parent <see cref="MediaContextBase"/> instance.</returns>
        public static MediaContextBase GetMediaContext(this IAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("asset", "The asset cannot be null.");
            }

            IMediaContextContainer mediaContextContainer = asset as IMediaContextContainer;
            MediaContextBase context = null;
            
            if (mediaContextContainer != null)
            {
                context = mediaContextContainer.GetMediaContext();
            }

            return context;
        }

        internal static async Task<IAssetFile> CreateAssetFileFromLocalFileAsync(this IAsset asset, string filePath, ILocator sasLocator, EventHandler<UploadProgressChangedEventArgs> uploadProgressChangedEventArgs, CancellationToken cancellationToken)
        {
            string assetFileName = Path.GetFileName(filePath);
            IAssetFile assetFile = await asset.AssetFiles.CreateAsync(assetFileName, cancellationToken);
            MediaContextBase context = asset.GetMediaContext();

            assetFile.UploadProgressChanged += uploadProgressChangedEventArgs;

            BlobTransferClient blobTransferClient = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = context.NumberOfConcurrentTransfers,
                ParallelTransferThreadCount = context.ParallelTransferThreadCount
            };

            await assetFile.UploadAsync(filePath, blobTransferClient, sasLocator, cancellationToken);

            assetFile.UploadProgressChanged -= uploadProgressChangedEventArgs;

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
                ILocator originLocator = asset
                    .Locators
                    .ToList()
                    .Where(l => l.Type == LocatorType.OnDemandOrigin)
                    .OrderBy(l => l.ExpirationDateTime)
                    .FirstOrDefault();
                if (originLocator != null)
                {
                    smoothStreamingUri = new Uri(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            BaseStreamingUrlTemplate,
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
