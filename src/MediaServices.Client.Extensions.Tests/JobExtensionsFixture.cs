// <copyright file="JobExtensionsFixture.cs" company="open-source">
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
    using System.Linq;
    using System.Threading;
    using MediaServices.Client.Extensions.Tests.Mocks;
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
            MediaProcessorBaseCollection nullMediaProcessorCollection = null;

            nullMediaProcessorCollection.GetLatestMediaProcessorByName(MediaProcessorNames.WindowsAzureMediaEncoder);
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
            var mediaProcessor = this.context.MediaProcessors.GetLatestMediaProcessorByName(MediaProcessorNames.WindowsAzureMediaEncoder);

            Assert.IsNotNull(mediaProcessor);

            var expectedMediaProcessor = this.context.MediaProcessors
                .Where(mp => mp.Name == MediaProcessorNames.WindowsAzureMediaEncoder)
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
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
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
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
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
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            this.context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldPrepareJobWithSingleTask()
        {
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowGetOverallProgressIfJobIsNull()
        {
            IJob nullJob = null;

            nullJob.GetOverallProgress();
        }

        [TestMethod]
        public void ShouldGetOverallProgressWhenJobContainsSingleTask()
        {
            var task = new TaskMock();
            var taskCollection = new TaskCollectionMock();

            taskCollection.Add(task);

            var job = new JobMock();
            job.Tasks = taskCollection;

            task.Progress = 0;
            Assert.AreEqual(0, job.GetOverallProgress());

            task.Progress = 25;
            Assert.AreEqual(25, job.GetOverallProgress());

            task.Progress = 75;
            Assert.AreEqual(75, job.GetOverallProgress());

            task.Progress = 100;
            Assert.AreEqual(100, job.GetOverallProgress());
        }

        [TestMethod]
        public void ShouldGetOverallProgressWhenJobContainsMultipleTask()
        {
            var task1 = new TaskMock();
            var task2 = new TaskMock();
            var taskCollection = new TaskCollectionMock();

            taskCollection.Add(task1);
            taskCollection.Add(task2);

            var job = new JobMock();
            job.Tasks = taskCollection;

            task1.Progress = 0;
            task2.Progress = 0;
            Assert.AreEqual(0, job.GetOverallProgress());

            task1.Progress = 25;
            task2.Progress = 0;
            Assert.AreEqual(12.5, job.GetOverallProgress());

            task1.Progress = 25;
            task2.Progress = 50;
            Assert.AreEqual(37.5, job.GetOverallProgress());

            task1.Progress = 50;
            task2.Progress = 50;
            Assert.AreEqual(50, job.GetOverallProgress());

            task1.Progress = 75;
            task2.Progress = 25;
            Assert.AreEqual(50, job.GetOverallProgress());

            task1.Progress = 75;
            task2.Progress = 50;
            Assert.AreEqual(62.5, job.GetOverallProgress());

            task1.Progress = 50;
            task2.Progress = 100;
            Assert.AreEqual(75, job.GetOverallProgress());

            task1.Progress = 100;
            task2.Progress = 75;
            Assert.AreEqual(87.5, job.GetOverallProgress());

            task1.Progress = 100;
            task2.Progress = 100;
            Assert.AreEqual(100, job.GetOverallProgress());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowStartExecutionProgressTaskIfContextIsNull()
        {
            MediaContextBase nullContext = null;
            IJob job = this.context.Jobs.Create("TestJob");

            nullContext.StartExecutionProgressTask(job, j => { }, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowStartExecutionProgressTaskIfJobIsNull()
        {
            IJob nullJob = null;

            this.context.StartExecutionProgressTask(nullJob, j => { }, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowStartExecutionProgressTaskIfJobDoesNotHaveValidId()
        {
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.Assets.Create("TestAsset", AssetCreationOptions.None);

            var job = this.context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);

            this.context.StartExecutionProgressTask(job, j => { }, CancellationToken.None);
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldStartExecutionProgressTaskAndInvokeCallbackWhenStateOrOverallProgressChange()
        {
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.CreateAssetFromFile("smallwmv1.wmv", AssetCreationOptions.None);

            var job = this.context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);
            job.Submit();

            var previousState = job.State;
            var previousOverallProgress = job.GetOverallProgress();
            var callbackInvocations = 0;

            var executionProgressTask = this.context.StartExecutionProgressTask(
                job,
                j =>
                {
                    callbackInvocations++;

                    Assert.IsTrue((j.State != previousState) || (j.GetOverallProgress() != previousOverallProgress));

                    previousState = j.State;
                    previousOverallProgress = j.GetOverallProgress();
                },
                CancellationToken.None);
            job = executionProgressTask.Result;

            Assert.IsTrue(callbackInvocations > 0);
            Assert.AreEqual(JobState.Finished, previousState);
            Assert.AreEqual(100, previousOverallProgress);

            Assert.AreEqual(JobState.Finished, job.State);
            Assert.AreEqual(100, job.GetOverallProgress());
            Assert.AreEqual(1, job.OutputMediaAssets.Count);

            this.outputAsset = job.OutputMediaAssets[0];

            Assert.IsNotNull(this.outputAsset);
            Assert.AreEqual(outputAssetName, this.outputAsset.Name);
            Assert.AreEqual(outputAssetOptions, this.outputAsset.Options);
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldStartExecutionProgressTaskWhenExecutionProgressChangedCallbackIsNull()
        {
            var mediaProcessorName = MediaProcessorNames.WindowsAzureMediaEncoder;
            var taskConfiguration = MediaEncoderTaskPresetStrings.H264SmoothStreaming720p;
            var outputAssetName = "Output Asset Name";
            var outputAssetOptions = AssetCreationOptions.None;
            this.asset = this.context.CreateAssetFromFile("smallwmv1.wmv", AssetCreationOptions.None);

            var job = this.context.PrepareJobWithSingleTask(mediaProcessorName, taskConfiguration, this.asset, outputAssetName, outputAssetOptions);
            job.Submit();

            job = this.context.StartExecutionProgressTask(job, null, CancellationToken.None).Result;

            Assert.AreEqual(JobState.Finished, job.State);
            Assert.AreEqual(100, job.GetOverallProgress());
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
