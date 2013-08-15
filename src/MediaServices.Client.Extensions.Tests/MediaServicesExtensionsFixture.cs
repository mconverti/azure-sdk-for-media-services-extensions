// <copyright file="MediaServicesExtensionsFixture.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by Mariano Converti
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
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;
    using Microsoft.WindowsAzure.Storage.Blob;

    [TestClass]
    public class MediaServicesExtensionsFixture
    {
        private CloudMediaContext context;
        private IAsset asset;
        private IAsset outputAsset;

        #region Locator extension tests

        [TestMethod]
        public void ShouldThrowWhenCreateAccessPolicyAndLocatorIfContextIsNull()
        {
            CloudMediaContext nullContext = null;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            try
            {
                nullContext.CreateLocator(this.asset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(1));
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowWhenCreateAccessPolicyAndLocatorIfAssetIsNull()
        {
            IAsset nullAsset = null;

            try
            {
                this.context.CreateLocator(nullAsset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(1));
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldCreateAccessPolicyAndOriginLocator()
        {
            var locatorType = LocatorType.OnDemandOrigin;
            DateTime? locatorStartTime = null;
            var accessPolicyPermissions = AccessPermissions.Read;
            var accessPolicyDuration = TimeSpan.FromDays(1);
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, locatorType, accessPolicyPermissions, accessPolicyDuration, locatorStartTime);

            Assert.IsNotNull(locator);
            Assert.AreEqual(locatorType, locator.Type);
            Assert.AreEqual(locatorStartTime, locator.StartTime);

            var accessPolicy = locator.AccessPolicy;

            Assert.IsNotNull(accessPolicy);
            Assert.AreEqual(accessPolicyPermissions, accessPolicy.Permissions);
            Assert.AreEqual(accessPolicyDuration, accessPolicy.Duration);
        }

        [TestMethod]
        public void ShouldCreateAccessPolicyAndSasLocator()
        {
            var locatorType = LocatorType.Sas;
            DateTime? locatorStartTime = DateTime.Today;
            var accessPolicyPermissions = AccessPermissions.Read;
            var accessPolicyDuration = TimeSpan.FromDays(1);
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, locatorType, accessPolicyPermissions, accessPolicyDuration, locatorStartTime);

            Assert.IsNotNull(locator);
            Assert.AreEqual(locatorType, locator.Type);
            Assert.AreEqual(locatorStartTime, locator.StartTime);

            var accessPolicy = locator.AccessPolicy;

            Assert.IsNotNull(accessPolicy);
            Assert.AreEqual(accessPolicyPermissions, accessPolicy.Permissions);
            Assert.AreEqual(accessPolicyDuration, accessPolicy.Duration);
        }

        #endregion

        #region Asset extensions tests

        [TestMethod]
        public void ShouldThrowWhenCreateAssetFromFileIfContextIsNull()
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
            this.asset = this.context.CreateAssetFromFile(fileName, AssetCreationOptions.None);

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(fileName, this.asset.Name);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(1, assetFiles.Count());
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(0).Name);

            Assert.AreEqual(0, this.asset.Locators.Count());
        }

        [TestMethod]
        public void ShouldThrowWhenCreateAssetFromFolderIfContextIsNull()
        {
            CloudMediaContext nullContext = null;

            try
            {
                nullContext.CreateAssetFromFolderAsync(string.Empty, AssetCreationOptions.None, CancellationToken.None);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowWhenCreateAssetFromFolderIfFolderDoesNotContainAnyFiles()
        {
            var emptyFolderName = "EmptyMediaFolder";
            if (Directory.Exists(emptyFolderName))
            {
                Directory.Delete(emptyFolderName);
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
            this.asset = this.context.CreateAssetFromFolder(folderName, AssetCreationOptions.None);

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
                Directory.Delete(downloadFolderPath);
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
                Directory.Delete(downloadFolderPath);
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
                Directory.Delete(downloadFolderPath);
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
                Directory.Delete(downloadFolderPath);
            }

            Directory.CreateDirectory(downloadFolderPath);

            this.context.DownloadAssetFilesToFolder(this.asset, downloadFolderPath);

            Assert.AreEqual(3, Directory.GetFiles(downloadFolderPath).Length);

            AssertFilesInFolders(originalFolderPath, downloadFolderPath, "smallwmv1.wmv");
            AssertFilesInFolders(originalFolderPath, downloadFolderPath, "smallwmv2.wmv");
            AssertFilesInFolders(originalFolderPath, downloadFolderPath, "dummy.ism");

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
                Directory.Delete(downloadFolderPath);
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

            AssertFilesInFolders(originalFolderPath, downloadFolderPath, "smallwmv1.wmv", downloadResults["smallwmv1.wmv"]);
            AssertFilesInFolders(originalFolderPath, downloadFolderPath, "smallwmv2.wmv", downloadResults["smallwmv2.wmv"]);
            AssertFilesInFolders(originalFolderPath, downloadFolderPath, "dummy.ism", downloadResults["dummy.ism"]);

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

        #endregion

        #region Job extension tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWhenGetLatestMediaProcessorByNameIfMediaProcessorCollectionIsNull()
        {
            var mediaProcessorName = "Windows Azure Media Encoder";
            MediaProcessorBaseCollection nullMediaProcessorCollection = null;

            nullMediaProcessorCollection.GetLatestMediaProcessorByName(mediaProcessorName);
        }

        [TestMethod]
        public void ShouldGetLatestMediaProcessorByNameReturnNullIfMediaProcessorNameIsNotValid()
        {
            var mediaProcessorName = "Invalid Media Processor Name";
            var mediaProcessor = this.context.MediaProcessors.GetLatestMediaProcessorByName(mediaProcessorName);

            Assert.IsNull(mediaProcessor);
        }

        [TestMethod]
        public void ShouldGetLatestMediaProcessorByName()
        {
            var mediaProcessorName = "Windows Azure Media Encoder";
            var mediaProcessor = this.context.MediaProcessors.GetLatestMediaProcessorByName(mediaProcessorName);

            Assert.IsNotNull(mediaProcessor);

            var expectedMediaProcessor = this.context.MediaProcessors
                .Where(mp => mp.Name == mediaProcessorName)
                .ToList()
                .Select(mp => new { mp.Id, mp.Name, Version = new Version(mp.Version) })
                .OrderBy(mp => mp.Version)
                .Last();

            Assert.AreEqual(expectedMediaProcessor.Id, mediaProcessor.Id);
            Assert.AreEqual(expectedMediaProcessor.Name, mediaProcessor.Name);
            Assert.AreEqual(expectedMediaProcessor.Version, new Version(mediaProcessor.Version));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowPrepareJobWithSingleTaskIfContextIsNull()
        {
            var mediaProcessorName = "Windows Azure Media Encoder";
            var taskConfiguration = "H264 Smooth Streaming 720p";
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);
            CloudMediaContext nullContext = null;

            nullContext.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowPrepareJobWithSingleTaskIfInputAssetIsNull()
        {
            var mediaProcessorName = "Windows Azure Media Encoder";
            var taskConfiguration = "H264 Smooth Streaming 720p";
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            IAsset inputAsset = null;

            this.context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, inputAsset, outputAssetName, outputAssetOptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowPrepareJobWithSingleTaskIfMediaProcessorNameIsUnknown()
        {
            var mediaProcessorName = "Unknown Media Processor";
            var taskConfiguration = "H264 Smooth Streaming 720p";
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            this.context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldPrepareJobWithSingleTask()
        {
            var mediaProcessorName = "Windows Azure Media Encoder";
            var taskConfiguration = "H264 Smooth Streaming 720p";
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.CreateAssetFromFile("smallwmv1.wmv", AssetCreationOptions.None);

            var job = this.context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);

            Assert.IsNotNull(job);
            Assert.AreEqual(1, job.Tasks.Count);

            var task = job.Tasks[0];

            Assert.IsNotNull(task);
            Assert.AreEqual(taskConfiguration, task.Configuration);
            Assert.AreEqual(1, task.InputAssets.Count);
            Assert.AreSame(this.asset, task.InputAssets[0]);
            Assert.AreEqual(1, task.OutputAssets.Count);
            Assert.AreEqual(outputAssetName, task.OutputAssets[0].Name);
            Assert.AreEqual(outputAssetOptions, task.OutputAssets[0].Options);
            Assert.AreEqual(this.context.MediaProcessors.GetLatestMediaProcessorByName(mediaProcessorName).Id, task.MediaProcessorId);

            job.Submit();
            job.GetExecutionProgressTask(CancellationToken.None).Wait();

            Assert.AreEqual(JobState.Finished, job.State);
            Assert.AreEqual(1, job.OutputMediaAssets.Count);

            this.outputAsset = job.OutputMediaAssets[0];

            Assert.IsNotNull(this.outputAsset);
            Assert.AreEqual(outputAssetName, this.outputAsset.Name);
            Assert.AreEqual(outputAssetOptions, this.outputAsset.Options);
        }

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            this.context = this.CreateContext();
            this.asset = null;
            this.outputAsset = null;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (this.asset != null)
            {
                this.asset.Delete();
            }

            if (this.outputAsset != null)
            {
                this.outputAsset.Delete();
            }
        }

        private static void AssertFilesInFolders(string originalFolderPath, string downloadFolderPath, string fileName, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs = null)
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
