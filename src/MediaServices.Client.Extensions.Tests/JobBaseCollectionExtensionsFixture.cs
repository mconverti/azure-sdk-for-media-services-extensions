// <copyright file="JobBaseCollectionExtensionsFixture.cs" company="open-source">
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
    using System.Configuration;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;

    [TestClass]
    public class JobBaseCollectionExtensionsFixture
    {
        private CloudMediaContext context;
        private IAsset asset;
        private IAsset outputAsset;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowCreateWithSingleTaskIfJobCollectiontIsNull()
        {
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);
            JobBaseCollection nullJobs = null;

            nullJobs.CreateWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowCreateWithSingleTaskIfInputAssetIsNull()
        {
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            IAsset inputAsset = null;

            this.context.Jobs.CreateWithSingleTask(mediaProcessorName, taskConfiguration, inputAsset, outputAssetName, outputAssetOptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowCreateWithSingleTaskIfMediaProcessorNameIsUnknown()
        {
            var mediaProcessorName = "Unknown Media Processor";
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            this.context.Jobs.CreateWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldCreateWithSingleTask()
        {
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.Assets.CreateFromFile("smallwmv1.wmv", AssetCreationOptions.None);

            var job = this.context.Jobs.CreateWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);

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

        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(
                ConfigurationManager.AppSettings["MediaServicesAccountName"],
                ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        }
    }
}
