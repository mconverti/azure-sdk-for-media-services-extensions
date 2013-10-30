// <copyright file="AssetBaseCollectionExtensions.cs" company="open-source">
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
    /// Contains extension methods and helpers for the <see cref="AssetBaseCollection"/> class.
    /// </summary>
    public static class AssetBaseCollectionExtensions
    {
        internal static readonly TimeSpan DefaultAccessPolicyDuration = TimeSpan.FromDays(1);

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static async Task<IAsset> CreateFromFileAsync(this AssetBaseCollection assets, string filePath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
        {
            if (assets == null)
            {
                throw new ArgumentNullException("assets", "The assets collection cannot be null.");
            }

            MediaContextBase context = assets.MediaContext;

            string assetName = Path.GetFileName(filePath);

            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                storageAccountName = context.DefaultStorageAccount.Name;
            }

            IAsset asset = await assets.CreateAsync(assetName, storageAccountName, options, cancellationToken);

            ILocator sasLocator = await context.Locators.CreateAsync(LocatorType.Sas, asset, AccessPermissions.Write | AccessPermissions.List, DefaultAccessPolicyDuration);

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

            await asset.CreateAssetFileFromLocalFileAsync(filePath, sasLocator, uploadProgressChangedHandler, cancellationToken);

            await sasLocator.DeleteAsync();

            return asset;
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static Task<IAsset> CreateFromFileAsync(this AssetBaseCollection assets, string filePath, string storageAccountName, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return assets.CreateFromFileAsync(filePath, storageAccountName, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static Task<IAsset> CreateFromFileAsync(this AssetBaseCollection assets, string filePath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
        {
            return assets.CreateFromFileAsync(filePath, null, options, uploadProgressChangedCallback, cancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static Task<IAsset> CreateFromFileAsync(this AssetBaseCollection assets, string filePath, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return assets.CreateFromFileAsync(filePath, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateFromFile(this AssetBaseCollection assets, string filePath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            using (Task<IAsset> task = assets.CreateFromFileAsync(filePath, storageAccountName, options, uploadProgressChangedCallback, CancellationToken.None))
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateFromFile(this AssetBaseCollection assets, string filePath, string storageAccountName, AssetCreationOptions options)
        {
            return assets.CreateFromFile(filePath, storageAccountName, options, null);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report the upload progress of the file.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateFromFile(this AssetBaseCollection assets, string filePath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            return assets.CreateFromFile(filePath, null, options, uploadProgressChangedCallback);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="filePath">The path to the file to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the file in <paramref name="filePath"/>.</returns>
        public static IAsset CreateFromFile(this AssetBaseCollection assets, string filePath, AssetCreationOptions options)
        {
            return assets.CreateFromFile(filePath, options, null);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static async Task<IAsset> CreateFromFolderAsync(this AssetBaseCollection assets, string folderPath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
        {
            if (assets == null)
            {
                throw new ArgumentNullException("assets", "The assets collection cannot be null.");
            }

            IEnumerable<string> filePaths = Directory.EnumerateFiles(folderPath);
            if (!filePaths.Any())
            {
                throw new FileNotFoundException(
                    string.Format(CultureInfo.InvariantCulture, "No files in directory, check the folder path: '{0}'", folderPath));
            }

            MediaContextBase context = assets.MediaContext;

            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                storageAccountName = context.DefaultStorageAccount.Name;
            }

            string assetName = Path.GetFileName(Path.GetFullPath(folderPath.TrimEnd('\\')));
            IAsset asset = await context.Assets.CreateAsync(assetName, storageAccountName, options, cancellationToken);

            ILocator sasLocator = await context.Locators.CreateAsync(LocatorType.Sas, asset, AccessPermissions.Write | AccessPermissions.List, DefaultAccessPolicyDuration);

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
                uploadTasks.Add(asset.CreateAssetFileFromLocalFileAsync(filePath, sasLocator, uploadProgressChangedHandler, cancellationToken));
            }

            await Task.Factory.ContinueWhenAll(uploadTasks.ToArray(), t => t, TaskContinuationOptions.ExecuteSynchronously);

            await sasLocator.DeleteAsync();

            return asset;
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static Task<IAsset> CreateFromFolderAsync(this AssetBaseCollection assets, string folderPath, string storageAccountName, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return assets.CreateFromFolderAsync(folderPath, storageAccountName, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static Task<IAsset> CreateFromFolderAsync(this AssetBaseCollection assets, string folderPath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback, CancellationToken cancellationToken)
        {
            return assets.CreateFromFolderAsync(folderPath, null, options, uploadProgressChangedCallback, cancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for the new <see cref="IAsset"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task&lt;IAsset&gt;"/> instance for new <see cref="IAsset"/>.</returns>
        public static Task<IAsset> CreateFromFolderAsync(this AssetBaseCollection assets, string folderPath, AssetCreationOptions options, CancellationToken cancellationToken)
        {
            return assets.CreateFromFolderAsync(folderPath, options, null, cancellationToken);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateFromFolder(this AssetBaseCollection assets, string folderPath, string storageAccountName, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            using (Task<IAsset> task = assets.CreateFromFolderAsync(folderPath, storageAccountName, options, uploadProgressChangedCallback, CancellationToken.None))
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="storageAccountName">The name of the Storage Account where to store the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateFromFolder(this AssetBaseCollection assets, string folderPath, string storageAccountName, AssetCreationOptions options)
        {
            return assets.CreateFromFolder(folderPath, storageAccountName, options, null);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <param name="uploadProgressChangedCallback">A callback to report upload progress of the files.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateFromFolder(this AssetBaseCollection assets, string folderPath, AssetCreationOptions options, Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback)
        {
            return assets.CreateFromFolder(folderPath, null, options, uploadProgressChangedCallback);
        }

        /// <summary>
        /// Returns a new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="assets">The <see cref="AssetBaseCollection"/> instance.</param>
        /// <param name="folderPath">The path to the folder with the files to upload to the new <see cref="IAsset"/>.</param>
        /// <param name="options">The <see cref="AssetCreationOptions"/>.</param>
        /// <returns>A new <see cref="IAsset"/> with the files in <paramref name="folderPath"/>.</returns>
        public static IAsset CreateFromFolder(this AssetBaseCollection assets, string folderPath, AssetCreationOptions options)
        {
            return assets.CreateFromFolder(folderPath, options, null);
        }
    }
}
