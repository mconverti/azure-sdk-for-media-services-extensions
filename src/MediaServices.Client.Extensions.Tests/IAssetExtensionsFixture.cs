// <copyright file="IAssetExtensionsFixture.cs" company="open-source">
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
    public class IAssetExtensionsFixture
    {
        private CloudMediaContext context;
        private IAsset asset;

        [TestMethod]
        public void ShouldThrowGenerateAssetFilesFromStorageIfAssetIsNull()
        {
            IAsset nullAsset = null;

            try
            {
                nullAsset.GenerateFromStorage();
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldGenerateAssetFilesFromBlobStorage()
        {
            var fileName = "smallwmv1.wmv";

            // Create empty asset.
            this.asset = this.context.Assets.Create(Path.GetFileNameWithoutExtension(fileName), AssetCreationOptions.None);

            // Create a SAS locator for the empty asset with write access.
            var sasLocator = this.context.Locators.Create(LocatorType.Sas, this.asset, AccessPermissions.Write, TimeSpan.FromDays(1));

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
            this.asset.GenerateFromStorage();

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
            this.asset = this.context.Assets.CreateFromFolder(originalFolderPath, AssetCreationOptions.None);
            var assetId = this.asset.Id;

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

            Assert.AreEqual(0, this.context.Locators.Where(l => l.AssetId == assetId).Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldDownloadAssetFilesToFolderWithDownloadProgressChangedCallback()
        {
            var originalFolderPath = "Media";
            this.asset = this.context.Assets.CreateFromFolder(originalFolderPath, AssetCreationOptions.None);
            var assetId = this.asset.Id;

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

            Assert.AreEqual(0, this.context.Locators.Where(l => l.AssetId == assetId).Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldGetManifestAssetFile()
        {
            var folderName = "Media";
            this.asset = this.context.Assets.CreateFromFolder(folderName, AssetCreationOptions.None);

            var manifestAssetFile = this.asset.GetManifestAssetFile();

            Assert.IsNotNull(manifestAssetFile);
            Assert.AreEqual("dummy.ism", manifestAssetFile.Name);
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldGetManifestAssetFileReturnNullIfThereIsNoManifestFile()
        {
            this.asset = this.context.Assets.CreateFromFile("smallwmv1.wmv", AssetCreationOptions.None);

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
            this.asset = this.context.Assets.CreateFromFile("dummy.ism", AssetCreationOptions.None);

            var locator = this.context.Locators.Create(LocatorType.OnDemandOrigin, this.asset, AccessPermissions.Read, TimeSpan.FromDays(1));

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
            this.asset = this.context.Assets.CreateFromFile("dummy.ism", AssetCreationOptions.None);

            var locator = this.context.Locators.Create(LocatorType.OnDemandOrigin, this.asset, AccessPermissions.Read, TimeSpan.FromDays(1));

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
            this.asset = this.context.Assets.CreateFromFile("dummy.ism", AssetCreationOptions.None);

            var locator = this.context.Locators.Create(LocatorType.OnDemandOrigin, this.asset, AccessPermissions.Read, TimeSpan.FromDays(1));

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
            this.asset = this.context.Assets.CreateFromFile("smallwmv1.wmv", AssetCreationOptions.None);

            var locator = this.context.Locators.Create(LocatorType.Sas, this.asset, AccessPermissions.Read, TimeSpan.FromDays(1));

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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowGetMediaContextIfAssetIsNull()
        {
            IAsset nullAsset = null;

            nullAsset.GetMediaContext();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldGetMediaContext()
        {
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            var mediaContext = this.asset.GetMediaContext();

            Assert.IsNotNull(mediaContext);
            Assert.AreSame(this.context, mediaContext);
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
        
        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(
                ConfigurationManager.AppSettings["MediaServicesAccountName"],
                ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        }
    }
}
