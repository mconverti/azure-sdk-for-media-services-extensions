// <copyright file="IJobExtensions.cs" company="open-source">
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
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains extension methods and helpers for the <see cref="IJob"/> interface.
    /// </summary>
    public static class IJobExtensions
    {
        private const int DefaultJobRefreshIntervalInMilliseconds = 2500;

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
        /// <param name="job">The <see cref="IJob"/> instance.</param>
        /// <param name="jobRefreshIntervalInMilliseconds">The time interval in milliseconds to refresh the <paramref name="job"/>.</param>
        /// <param name="executionProgressChangedCallback">A callback that is invoked when the <paramref name="job"/> state or overall progress change.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A started <see cref="System.Threading.Tasks.Task&lt;IJob&gt;"/> instance to monitor the <paramref name="job"/> progress by invoking the <paramref name="executionProgressChangedCallback"/> when its state or overall progress change.</returns>
        public static Task<IJob> StartExecutionProgressTask(this IJob job, int jobRefreshIntervalInMilliseconds, Action<IJob> executionProgressChangedCallback, CancellationToken cancellationToken)
        {
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

                            IMediaDataServiceContext dataContext = refreshedJob.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                            refreshedJob.JobEntityRefresh(dataContext);

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
        /// <param name="job">The <see cref="IJob"/> instance.</param>
        /// <param name="executionProgressChangedCallback">A callback that is invoked when the <paramref name="job"/> state or overall progress change.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
        /// <returns>A started <see cref="System.Threading.Tasks.Task&lt;IJob&gt;"/> instance to monitor the <paramref name="job"/> progress by invoking the <paramref name="executionProgressChangedCallback"/> when its state or overall progress change.</returns>
        public static Task<IJob> StartExecutionProgressTask(this IJob job, Action<IJob> executionProgressChangedCallback, CancellationToken cancellationToken)
        {
            return job.StartExecutionProgressTask(DefaultJobRefreshIntervalInMilliseconds, executionProgressChangedCallback, cancellationToken);
        }

        /// <summary>
        /// Returns the parent <see cref="MediaContextBase"/> instance.
        /// </summary>
        /// <param name="job">The <see cref="IJob"/> instance.</param>
        /// <returns>The parent <see cref="MediaContextBase"/> instance.</returns>
        public static MediaContextBase GetMediaContext(this IJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job", "The job cannot be null.");
            }

            IMediaContextContainer mediaContextContainer = job as IMediaContextContainer;
            MediaContextBase context = null;

            if (mediaContextContainer != null)
            {
                context = mediaContextContainer.GetMediaContext();
            }

            return context;
        }

        private static void JobEntityRefresh(this IJob job, IMediaDataServiceContext dataContext)
        {
            const string JobEntityRefreshMethodName = "JobEntityRefresh";

            System.Reflection.MethodInfo jobEntityRefreshMethod = job
                .GetType()
                .GetMethod(JobEntityRefreshMethodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            jobEntityRefreshMethod.Invoke(job, new object[] { dataContext });
        }
    }
}
