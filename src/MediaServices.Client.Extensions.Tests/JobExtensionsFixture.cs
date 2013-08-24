// <copyright file="JobExtensionsFixture.cs" company="open-source">
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
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;

    [TestClass]
    public class JobExtensionsFixture
    {
        private CloudMediaContext context;
        private IAsset asset;
        private IAsset outputAsset;

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
