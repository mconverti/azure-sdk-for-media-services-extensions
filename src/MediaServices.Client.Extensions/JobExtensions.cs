// <copyright file="JobExtensions.cs" company="open-source">
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
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains extension methods and helpers related to the <see cref="IJob"/> interface.
    /// </summary>
    public static class JobExtensions
    {
        private const int DefaultJobRefreshIntervalInMilliseconds = 2500;

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

        /// <summary>
        /// Returns the overall progress of the <paramref name="job"/> by aggregating the progress of all its tasks.
        /// </summary>
        /// <param name="job">The <see cref="IJob"/> instance.</param>
        /// <returns>The overall progress of the <paramref name="job"/> by aggregating the progress of all its tasks.</returns>
        public static double GetOverallProgress(this IJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job", "The job cannot be null.");
            }

            return job.Tasks.Sum(t => t.Progress) / job.Tasks.Count;
        }

        /// <summary>
        /// Returns a started <see cref="System.Threading.Tasks.Task"/> to monitor the <paramref name="job"/> progress by invoking the <paramref name="executionProgressChangedCallback"/> when its state or overall progress change.
        /// </summary>
        /// <param name="context">The <see cref="MediaContextBase"/> instance.</param>
        /// <param name="job">The <see cref="IJob"/> instance.</param>
        /// <param name="jobRefreshIntervalInMilliseconds">The time interval in milliseconds to refresh the <paramref name="job"/>.</param>
        /// <param name="executionProgressChangedCallback">A callback that is invoked when the <paramref name="job"/> state or overall progress change.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A started <see cref="System.Threading.Tasks.Task&lt;IJob&gt;"/> instance to monitor the <paramref name="job"/> progress by invoking the <paramref name="executionProgressChangedCallback"/> when its state or overall progress change.</returns>
        public static Task<IJob> StartExecutionProgressTask(this MediaContextBase context, IJob job, int jobRefreshIntervalInMilliseconds, Action<IJob> executionProgressChangedCallback, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "The context cannot be null.");
            }

            if (job == null)
            {
                throw new ArgumentNullException("job", "The job cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(job.Id))
            {
                throw new ArgumentException("The job does not have a valid Id. Please, make sure to submit it first.", "job");
            }

            return Task.Factory.StartNew(
                    originalJob =>
                    {
                        IJob refreshedJob = (IJob)originalJob;
                        while ((refreshedJob.State != JobState.Canceled) && (refreshedJob.State != JobState.Error) && (refreshedJob.State != JobState.Finished))
                        {
                            Thread.Sleep(jobRefreshIntervalInMilliseconds);

                            cancellationToken.ThrowIfCancellationRequested();

                            JobState previousState = refreshedJob.State;
                            double previousOverallProgress = refreshedJob.GetOverallProgress();

                            refreshedJob = context.Jobs.Where(j => j.Id == refreshedJob.Id).First();

                            if ((executionProgressChangedCallback != null) && ((refreshedJob.State != previousState) || (refreshedJob.GetOverallProgress() != previousOverallProgress)))
                            {
                                executionProgressChangedCallback(refreshedJob);
                            }
                        }

                        return refreshedJob;
                    },
                    job,
                    cancellationToken);
        }

        /// <summary>
        /// Returns a started <see cref="System.Threading.Tasks.Task"/> to monitor the <paramref name="job"/> progress by invoking the <paramref name="executionProgressChangedCallback"/> when its state or overall progress change.
        /// </summary>
        /// <param name="context">The <see cref="MediaContextBase"/> instance.</param>
        /// <param name="job">The <see cref="IJob"/> instance.</param>
        /// <param name="executionProgressChangedCallback">A callback that is invoked when the <paramref name="job"/> state or overall progress change.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A started <see cref="System.Threading.Tasks.Task&lt;IJob&gt;"/> instance to monitor the <paramref name="job"/> progress by invoking the <paramref name="executionProgressChangedCallback"/> when its state or overall progress change.</returns>
        public static Task<IJob> StartExecutionProgressTask(this MediaContextBase context, IJob job, Action<IJob> executionProgressChangedCallback, CancellationToken cancellationToken)
        {
            return context.StartExecutionProgressTask(job, DefaultJobRefreshIntervalInMilliseconds, executionProgressChangedCallback, cancellationToken);
        }
    }
}
