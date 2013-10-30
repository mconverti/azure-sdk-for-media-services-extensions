// <copyright file="JobBaseCollectionExtensions.cs" company="open-source">
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

    /// <summary>
    /// Contains extension methods and helpers for the <see cref="JobBaseCollection"/> class.
    /// </summary>
    public static class JobBaseCollectionExtensions
    {
        /// <summary>
        /// Returns a <see cref="IJob"/> instance with a single <see cref="ITask"/> ready to be submitted.
        /// </summary>
        /// <param name="jobs">The <see cref="JobBaseCollection"/> instance.</param>
        /// <param name="mediaProcessorName">The name of the media processor.</param>
        /// <param name="taskConfiguration">The task configuration.</param>
        /// <param name="inputAsset">The input <see cref="IAsset"/> instance.</param>
        /// <param name="outputAssetName">The name of the output asset.</param>
        /// <param name="outputAssetStorageAccountName">The name of the Storage Account where to store the output asset.</param>
        /// <param name="outputAssetOptions">The <see cref="AssetCreationOptions"/> of the output asset.</param>
        /// <returns>A <see cref="IJob"/> instance with a single <see cref="ITask"/> ready to be submitted.</returns>
        public static IJob CreateWithSingleTask(this JobBaseCollection jobs, string mediaProcessorName, string taskConfiguration, IAsset inputAsset, string outputAssetName, string outputAssetStorageAccountName, AssetCreationOptions outputAssetOptions)
        {
            if (jobs == null)
            {
                throw new ArgumentNullException("jobs", "The jobs collection cannot be null.");
            }

            if (inputAsset == null)
            {
                throw new ArgumentNullException("inputAsset", "The input asset cannot be null.");
            }

            MediaContextBase context = jobs.MediaContext;

            IMediaProcessor processor = context.MediaProcessors.GetLatestMediaProcessorByName(mediaProcessorName);
            if (processor == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "Unknown media processor: '{0}'", mediaProcessorName), "mediaProcessorName");
            }

            IJob job = jobs.Create(string.Format(CultureInfo.InvariantCulture, "Job for {0}", inputAsset.Name));

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
        /// <param name="jobs">The <see cref="JobBaseCollection"/> instance.</param>
        /// <param name="mediaProcessorName">The name of the media processor.</param>
        /// <param name="taskConfiguration">The task configuration.</param>
        /// <param name="inputAsset">The input <see cref="IAsset"/> instance.</param>
        /// <param name="outputAssetName">The name of the output asset.</param>
        /// <param name="outputAssetOptions">The <see cref="AssetCreationOptions"/> of the output asset.</param>
        /// <returns>A <see cref="IJob"/> instance with a single <see cref="ITask"/> ready to be submitted.</returns>
        public static IJob CreateWithSingleTask(this JobBaseCollection jobs, string mediaProcessorName, string taskConfiguration, IAsset inputAsset, string outputAssetName, AssetCreationOptions outputAssetOptions)
        {
            return jobs.CreateWithSingleTask(mediaProcessorName, taskConfiguration, inputAsset, outputAssetName, null, outputAssetOptions);
        }
    }
}
