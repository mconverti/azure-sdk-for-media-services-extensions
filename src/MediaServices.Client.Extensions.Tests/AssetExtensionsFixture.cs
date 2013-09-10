// <copyright file="AssetExtensionsFixture.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by mconverti
//   
//  Redistribution and use in source and binary forms, with or without modification, are permitted.
//
//  The names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

namespace MediaServices.Client.Extensions.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;
    using Microsoft.WindowsAzure.Storage.Blob;

    [TestClass]
    public class AssetExtensionsFixture
    {
        private CloudMediaContext context;
        private IAsset asset;

        [TestMethod]
        public void ShouldThrowCreateAssetFromFileIfContextIsNull()
        {
            CloudMediaContext nullContext = null;

            try
            {
                nullContext.CreateAssetFromFileAsync(string.Empty, AssetCreationOptions.None, CancellationToken.None).Wait();
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldCreateAssetFromFile()
        {
            var fileName = "smallwmv1.wmv";
            this.asset = this.context.CreateAssetFromFile(fileName, null, AssetCreationOptions.None);

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(fileName, this.asset.Name);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(1, assetFiles.Count());
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(0).Name);

            Assert.AreEqual(0, this.asset.Locators.Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldCreateAssetFromFileWithUploadProgressChangedCallback()
        {
            var uploadResults = new ConcurrentDictionary<string, UploadProgressChangedEventArgs>();
            Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback =
                (af, e) =>
                {
                    IAssetFile assetFile = af;
                    UploadProgressChangedEventArgs eventArgs = e;

                    Assert.IsNotNull(assetFile);
                    Assert.IsNotNull(eventArgs);

                    uploadResults.AddOrUpdate(assetFile.Name, eventArgs, (k, e2) => eventArgs);
                };

            var fileName = "smallwmv1.wmv";
            this.asset = this.context.CreateAssetFromFile(fileName, AssetCreationOptions.None, uploadProgressChangedCallback);

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(fileName, this.asset.Name);

            Assert.AreEqual(1, uploadResults.Count);

            AssertUploadedFile(".\\", fileName, uploadResults[fileName]);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(1, assetFiles.Count());
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(0).Name);

            Assert.AreEqual(0, this.asset.Locators.Count());
        }

        [TestMethod]
        public void ShouldThrowCreateAssetFromFolderIfContextIsNull()
        {
            CloudMediaContext nullContext = null;

            try
            {
                nullContext.CreateAssetFromFolderAsync(string.Empty, null, AssetCreationOptions.None, CancellationToken.None);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowCreateAssetFromFolderIfFolderDoesNotContainAnyFiles()
        {
            var emptyFolderName = "EmptyMediaFolder";
            if (Directory.Exists(emptyFolderName))
            {
                Directory.Delete(emptyFolderName, true);
            }

            Directory.CreateDirectory(emptyFolderName);

            try
            {
                this.context.CreateAssetFromFolderAsync(emptyFolderName, AssetCreationOptions.None, CancellationToken.None);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(FileNotFoundException));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldCreateAssetFromFolder()
        {
            var folderName = "Media";
            this.asset = this.context.CreateAssetFromFolder(folderName, null, AssetCreationOptions.None);

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(folderName, this.asset.Name);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(3, assetFiles.Count());
            Assert.AreEqual("dummy.ism", assetFiles.ElementAt(0).Name);
            Assert.IsTrue(assetFiles.ElementAt(0).IsPrimary);
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(1).Name);
            Assert.IsFalse(assetFiles.ElementAt(1).IsPrimary);
            Assert.AreEqual("smallwmv2.wmv", assetFiles.ElementAt(2).Name);
            Assert.IsFalse(assetFiles.ElementAt(2).IsPrimary);

            Assert.AreEqual(0, this.asset.Locators.Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldCreateAssetFromFolderWithUploadProgressChangedCallback()
        {
            var uploadResults = new ConcurrentDictionary<string, UploadProgressChangedEventArgs>();
            Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback =
                (af, e) =>
                {
                    IAssetFile assetFile = af;
                    UploadProgressChangedEventArgs eventArgs = e;

                    Assert.IsNotNull(assetFile);
                    Assert.IsNotNull(eventArgs);

                    uploadResults.AddOrUpdate(assetFile.Name, eventArgs, (k, e2) => eventArgs);
                };

            var folderName = "Media";
            this.asset = this.context.CreateAssetFromFolder(folderName, AssetCreationOptions.None, uploadProgressChangedCallback);

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(folderName, this.asset.Name);

            Assert.AreEqual(3, uploadResults.Count);

            AssertUploadedFile(folderName, "smallwmv1.wmv", uploadResults["smallwmv1.wmv"]);
            AssertUploadedFile(folderName, "smallwmv2.wmv", uploadResults["smallwmv2.wmv"]);
            AssertUploadedFile(folderName, "dummy.ism", uploadResults["dummy.ism"]);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(3, assetFiles.Count());
            Assert.AreEqual("dummy.ism", assetFiles.ElementAt(0).Name);
            Assert.IsTrue(assetFiles.ElementAt(0).IsPrimary);
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(1).Name);
            Assert.IsFalse(assetFiles.ElementAt(1).IsPrimary);
            Assert.AreEqual("smallwmv2.wmv", assetFiles.ElementAt(2).Name);
            Assert.IsFalse(assetFiles.ElementAt(2).IsPrimary);

            Assert.AreEqual(0, this.asset.Locators.Count());
        }

        [TestMethod]
        public void ShouldThrowCreateAssetFilesIfContextIsNull()
        {
            CloudMediaContext nullContext = null;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            try
            {
                nullContext.CreateAssetFiles(this.asset);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowCreateAssetFilesIfAssetIsNull()
        {
            try
            {
                this.context.CreateAssetFiles(null);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldCreateAssetFilesFromBlobStorage()
        {
            var fileName = "smallwmv1.wmv";

            // Create empty asset.
            this.asset = this.context.Assets.Create(Path.GetFileNameWithoutExtension(fileName), AssetCreationOptions.None);

            // Create a SAS locator for the empty asset with write access.
            var sasLocator = this.context.CreateLocator(this.asset, LocatorType.Sas, AccessPermissions.Write, TimeSpan.FromDays(1));

            // Get a refence to the asset container in Blob storage.
            var locatorUri = new Uri(sasLocator.Path, UriKind.Absolute);
            var assetContainer = new CloudBlobContainer(locatorUri);

            // Upload a blob directly to the asset container.
            var blob = assetContainer.GetBlockBlobReference(fileName);
            blob.UploadFromStream(File.OpenRead(fileName));

            // Refresh the asset reference.
            this.asset = this.context.Assets.Where(a => a.Id == this.asset.Id).First();

            Assert.AreEqual(0, this.asset.AssetFiles.Count());

            // Create the AssetFiles from Blob storage.
            this.context.CreateAssetFiles(this.asset);

            // Refresh the asset reference.
            this.asset = this.context.Assets.Where(a => a.Id == this.asset.Id).First();

            Assert.AreEqual(1, this.asset.AssetFiles.Count());
            Assert.AreEqual(fileName, this.asset.AssetFiles.ToArray()[0].Name);
        }

        [TestMethod]
        public void ShouldThrowDownloadAssetFilesToFolderIfContextIsNull()
        {
            CloudMediaContext nullContext = null;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);
            var downloadFolderPath = "Media-Downloaded";
            if (Directory.Exists(downloadFolderPath))
            {
                Directory.Delete(downloadFolderPath, true);
            }

            Directory.CreateDirectory(downloadFolderPath);

            try
            {
                nullContext.DownloadAssetFilesToFolder(this.asset, downloadFolderPath);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowDownloadAssetFilesToFolderIfAssetIsNull()
        {
            IAsset nullAsset = null;
            var downloadFolderPath = "Media-Downloaded";
            if (Directory.Exists(downloadFolderPath))
            {
                Directory.Delete(downloadFolderPath, true);
            }

            Directory.CreateDirectory(downloadFolderPath);

            try
            {
                this.context.DownloadAssetFilesToFolderAsync(nullAsset, downloadFolderPath, CancellationToken.None);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowDownloadAssetFilesToFolderIfFolderPathDoesNotExist()
        {
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);
            var downloadFolderPath = "Media-Downloaded";
            if (Directory.Exists(downloadFolderPath))
            {
                Directory.Delete(downloadFolderPath, true);
            }

            try
            {
                this.context.DownloadAssetFilesToFolder(this.asset, downloadFolderPath);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentException));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldDownloadAssetFilesToFolder()
        {
            var originalFolderPath = "Media";
            this.asset = this.context.CreateAssetFromFolder(originalFolderPath, AssetCreationOptions.None);

            var downloadFolderPath = "Media-Downloaded";
            if (Directory.Exists(downloadFolderPath))
            {
                Directory.Delete(downloadFolderPath, true);
            }

            Directory.CreateDirectory(downloadFolderPath);

            this.context.DownloadAssetFilesToFolder(this.asset, downloadFolderPath);

            Assert.AreEqual(3, Directory.GetFiles(downloadFolderPath).Length);

            AssertDownloadedFile(originalFolderPath, downloadFolderPath, "smallwmv1.wmv");
            AssertDownloadedFile(originalFolderPath, downloadFolderPath, "smallwmv2.wmv");
            AssertDownloadedFile(originalFolderPath, downloadFolderPath, "dummy.ism");

            Assert.AreEqual(0, this.asset.Locators.Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldDownloadAssetFilesToFolderWithDownloadProgressChangedCallback()
        {
            var originalFolderPath = "Media";
            this.asset = this.context.CreateAssetFromFolder(originalFolderPath, AssetCreationOptions.None);

            var downloadFolderPath = "Media-Downloaded";
            if (Directory.Exists(downloadFolderPath))
            {
                Directory.Delete(downloadFolderPath, true);
            }

            Directory.CreateDirectory(downloadFolderPath);
            var downloadResults = new ConcurrentDictionary<string, DownloadProgressChangedEventArgs>();
            Action<IAssetFile, DownloadProgressChangedEventArgs> downloadProgressChangedCallback =
                (af, e) =>
                {
                    IAssetFile assetFile = af;
                    DownloadProgressChangedEventArgs eventArgs = e;

                    Assert.IsNotNull(assetFile);
                    Assert.IsNotNull(eventArgs);

                    downloadResults.AddOrUpdate(assetFile.Name, eventArgs, (k, e2) => eventArgs);
                };

            this.context.DownloadAssetFilesToFolder(this.asset, downloadFolderPath, downloadProgressChangedCallback);

            Assert.AreEqual(3, downloadResults.Count);
            Assert.AreEqual(3, Directory.GetFiles(downloadFolderPath).Length);

            AssertDownloadedFile(originalFolderPath, downloadFolderPath, "smallwmv1.wmv", downloadResults["smallwmv1.wmv"]);
            AssertDownloadedFile(originalFolderPath, downloadFolderPath, "smallwmv2.wmv", downloadResults["smallwmv2.wmv"]);
            AssertDownloadedFile(originalFolderPath, downloadFolderPath, "dummy.ism", downloadResults["dummy.ism"]);

            Assert.AreEqual(0, this.asset.Locators.Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldGetManifestAssetFile()
        {
            var folderName = "Media";
            this.asset = this.context.CreateAssetFromFolder(folderName, AssetCreationOptions.None);

            var manifestAssetFile = this.asset.GetManifestAssetFile();

            Assert.IsNotNull(manifestAssetFile);
            Assert.AreEqual("dummy.ism", manifestAssetFile.Name);
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldGetManifestAssetFileReturnNullIfThereIsNoManifestFile()
        {
            this.asset = this.context.CreateAssetFromFile("smallwmv1.wmv", AssetCreationOptions.None);

            var manifestAssetFile = this.asset.GetManifestAssetFile();

            Assert.IsNull(manifestAssetFile);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowGetManifestAssetFileIfAssetIsNull()
        {
            IAsset nullAsset = null;

            nullAsset.GetManifestAssetFile();
        }

        [TestMethod]
        [DeploymentItem(@"Media\dummy.ism")]
        public void ShouldGetSmoothStreamingUri()
        {
            this.asset = this.context.CreateAssetFromFile("dummy.ism", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(1));

            var smoothStreamingUrl = this.asset.GetSmoothStreamingUri();

            Assert.IsNotNull(smoothStreamingUrl);
            Assert.IsTrue(
                smoothStreamingUrl
                    .AbsoluteUri
                    .EndsWith(locator.ContentAccessComponent + "/dummy.ism/manifest", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [DeploymentItem(@"Media\dummy.ism")]
        public void ShouldGetHlsUri()
        {
            this.asset = this.context.CreateAssetFromFile("dummy.ism", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(1));

            var hlsUri = this.asset.GetHlsUri();

            Assert.IsNotNull(hlsUri);
            Assert.IsTrue(
                hlsUri
                    .AbsoluteUri
                    .EndsWith(locator.ContentAccessComponent + "/dummy.ism/manifest(format=m3u8-aapl)", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [DeploymentItem(@"Media\dummy.ism")]
        public void ShouldGetMpegDashUri()
        {
            this.asset = this.context.CreateAssetFromFile("dummy.ism", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(1));

            var mpegDashUri = this.asset.GetMpegDashUri();

            Assert.IsNotNull(mpegDashUri);
            Assert.IsTrue(
                mpegDashUri
                    .AbsoluteUri
                    .EndsWith(locator.ContentAccessComponent + "/dummy.ism/manifest(format=mpd-time-csf)", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowGetStreamingUriIfAssetIsNull()
        {
            IAsset nullAsset = null;

            nullAsset.GetSmoothStreamingUri();
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldGetSasUri()
        {
            this.asset = this.context.CreateAssetFromFile("smallwmv1.wmv", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, LocatorType.Sas, AccessPermissions.Read, TimeSpan.FromDays(1));

            var assetFiles = this.asset.AssetFiles.First();

            var sasUri = assetFiles.GetSasUri();

            Assert.IsNotNull(sasUri);

            var client = new HttpClient();
            var response = client.GetAsync(sasUri, HttpCompletionOption.ResponseHeadersRead).Result;

            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowGetSasUriIfAssetFileIsNull()
        {
            IAssetFile nullAssetFile = null;

            nullAssetFile.GetSasUri();
        }

        [TestInitialize]
        public void Initialize()
        {
            this.context = this.CreateContext();
            this.asset = null;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (this.asset != null)
            {
                this.asset.Delete();
            }
        }

        private static void AssertDownloadedFile(string originalFolderPath, string downloadFolderPath, string fileName, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs = null)
        {
            var expected = new FileInfo(Path.Combine(originalFolderPath, fileName));
            var result = new FileInfo(Path.Combine(downloadFolderPath, fileName));

            Assert.AreEqual(expected.Length, result.Length);

            if (downloadProgressChangedEventArgs != null)
            {
                Assert.AreEqual(expected.Length, downloadProgressChangedEventArgs.BytesDownloaded);
                Assert.AreEqual(expected.Length, downloadProgressChangedEventArgs.TotalBytes);
                Assert.AreEqual(100, downloadProgressChangedEventArgs.Progress);
            }
        }

        private static void AssertUploadedFile(string originalFolderPath, string fileName, UploadProgressChangedEventArgs uploadProgressChangedEventArgs)
        {
            var expected = new FileInfo(Path.Combine(originalFolderPath, fileName));

            Assert.AreEqual(expected.Length, uploadProgressChangedEventArgs.BytesUploaded);
            Assert.AreEqual(expected.Length, uploadProgressChangedEventArgs.TotalBytes);
            Assert.AreEqual(100, uploadProgressChangedEventArgs.Progress);
        }

        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(
                ConfigurationManager.AppSettings["MediaServicesAccountName"],
                ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        }
    }
}
