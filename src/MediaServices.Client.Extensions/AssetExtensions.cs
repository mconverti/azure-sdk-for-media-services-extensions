// <copyright file="AssetExtensions.cs" company="open-source">
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
    /// Contains extension methods and helpers related to the <see cref="IAsset"/> interface.
    /// </summary>
    public static class AssetExtensions
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

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static async Task<IAsset> CreateAssetFromFileAsync(this CloudMediaContext context, string filePath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
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

            EventHandler<UploadProgressChangedEventArgs> uploadProgressChangedHandler =
                (s, e) =>
                {
                    IAssetFile assetFile = (IAssetFile)s;
                    UploadProgressChangedEventArgs eventArgs = e;

                    if (uploadProgressChangedCallback != null)
                    {
                        uploadProgressChangedCallback(assetFile, eventArgs);
                    }
                };

            await context.CreateAssetFileFromLocalFileAsync(asset, filePath, sasLocator, uploadProgressChangedHandler, cancellationToken);

            await sasLocator.DeleteAsync();

            return asset;
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static Task<IAsset> CreateAssetFromFileAsync(this CloudMediaContext context, string filePath, string storageAccountName, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFileAsync(filePath, storageAccountName, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static Task<IAsset> CreateAssetFromFileAsync(this CloudMediaContext context, string filePath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFileAsync(filePath, null, options, uploadProgressChangedCallback, cancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static Task<IAsset> CreateAssetFromFileAsync(this CloudMediaContext context, string filePath, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFileAsync(filePath, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            using (Task<IAsset> task = context.CreateAssetFromFileAsync(filePath, storageAccountName, options, uploadProgressChangedCallback, CancellationToken.None))
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, string storageAccountName, AssetCreationOptions options)
        {
            return context.CreateAssetFromFile(filePath, storageAccountName, options, null);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            return context.CreateAssetFromFile(filePath, null, options, uploadProgressChangedCallback);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateAssetFromFile(this CloudMediaContext context, string filePath, AssetCreationOptions options)
        {
            return context.CreateAssetFromFile(filePath, options, null);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static async Task<IAsset> CreateAssetFromFolderAsync(this CloudMediaContext context, string folderPath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
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

            EventHandler<UploadProgressChangedEventArgs> uploadProgressChangedHandler =
                (s, e) =>
                {
                    IAssetFile assetFile = (IAssetFile)s;
                    UploadProgressChangedEventArgs eventArgs = e;

                    if (uploadProgressChangedCallback != null)
                    {
                        uploadProgressChangedCallback(assetFile, eventArgs);
                    }
                };

            IList<Task> uploadTasks = new List<Task>();
            foreach (string filePath in filePaths)
            {
                uploadTasks.Add(
                    context.CreateAssetFileFromLocalFileAsync(asset, filePath, sasLocator, uploadProgressChangedHandler, cancellationToken));
            }

            await Task.Factory.ContinueWhenAll(uploadTasks.ToArray(), t => t, TaskContinuationOptions.ExecuteSynchronously);

            await sasLocator.DeleteAsync();

            return asset;
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static Task<IAsset> CreateAssetFromFolderAsync(this CloudMediaContext context, string folderPath, string storageAccountName, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFolderAsync(folderPath, storageAccountName, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static Task<IAsset> CreateAssetFromFolderAsync(this CloudMediaContext context, string folderPath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
        {
            return context.CreateAssetFromFolderAsync(folderPath, null, options, uploadProgressChangedCallback, cancellationToken);
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
            return context.CreateAssetFromFolderAsync(folderPath, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            using (Task<IAsset> task = context.CreateAssetFromFolderAsync(folderPath, storageAccountName, options, uploadProgressChangedCallback, CancellationToken.None))
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, string storageAccountName, AssetCreationOptions options)
        {
            return context.CreateAssetFromFolder(folderPath, storageAccountName, options, null);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            return context.CreateAssetFromFolder(folderPath, null, options, uploadProgressChangedCallback);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateAssetFromFolder(this CloudMediaContext context, string folderPath, AssetCreationOptions options)
        {
            return context.CreateAssetFromFolder(folderPath, options, null);
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

        private static async Task<IAssetFile> CreateAssetFileFromLocalFileAsync(this CloudMediaContext context, IAsset asset, string filePath, ILocator sasLocator, EventHandler<UploadProgressChangedEventArgs> uploadProgressChangedEventArgs, CancellationToken cancellationToken)
        {
            string assetFileName = Path.GetFileName(filePath);
            IAssetFile assetFile = await asset.AssetFiles.CreateAsync(assetFileName, cancellationToken);

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
