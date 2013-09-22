// <copyright file="Program.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by mconverti
//   
//  Redistribution and use in source and binary forms, with or without modification, are permitted.
//
//  The names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

namespace SampleWorkflowUsingExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.WindowsAzure.MediaServices.Client;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string mediaServicesAccountName = ConfigurationManager.AppSettings["MediaServicesAccountName"];
                string mediaServicesAccountKey = ConfigurationManager.AppSettings["MediaServicesAccountKey"];

                CloudMediaContext context = new CloudMediaContext(mediaServicesAccountName, mediaServicesAccountKey);

                Console.WriteLine("Creating new asset from local file...");

                // 1. Create a new asset by uploading a mezzanine file from a local path.
                IAsset inputAsset = context.CreateAssetFromFile(
                    "smallwmv1.wmv",
                    AssetCreationOptions.None,
                    (af, p) =>
                    {
                        Console.WriteLine("Uploading '{0}' - Progress: {1:0.##}%", af.Name, p.Progress);
                    });

                Console.WriteLine("Asset created.");

                // 2. Prepare a job with a single task to transcode the previous mezzanine asset
                //    into a multi-bitrate asset.
                IJob job = context.PrepareJobWithSingleTask(
                    MediaProcessorNames.WindowsAzureMediaEncoder,
                    MediaEncoderTaskPresetStrings.H264AdaptiveBitrateMP4Set720p,
                    inputAsset,
                    "Sample Adaptive Bitrate MP4",
                    AssetCreationOptions.None);

                Console.WriteLine("Submitting transcoding job...");

                // 3. Submit the job and wait until it is completed.
                job.Submit();
                job = context.StartExecutionProgressTask(
                    job,
                    j =>
                    {
                        Console.WriteLine("Job state: {0}", j.State);
                        Console.WriteLine("Job progress: {0:0.##}%", j.GetOverallProgress());
                    },
                    CancellationToken.None).Result;

                Console.WriteLine("Transcoding job finished.");

                IAsset outputAsset = job.OutputMediaAssets[0];

                Console.WriteLine("Publishing output asset...");

                // 4. Publish the output asset by creating an Origin locator for adaptive streaming, and a SAS locator for progressive download.
                context.CreateLocator(outputAsset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(30));
                context.CreateLocator(outputAsset, LocatorType.Sas, AccessPermissions.Read, TimeSpan.FromDays(30));

                IEnumerable<IAssetFile> mp4AssetFiles = outputAsset
                        .AssetFiles
                        .ToList()
                        .Where(af => af.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));

                // 5. Generate the Smooth Streaming, HLS and MPEG-DASH URLs for adaptive streaming, and the Progressive Download URL.
                Uri smoothStreamingUri = outputAsset.GetSmoothStreamingUri();
                Uri hlsUri = outputAsset.GetHlsUri();
                Uri mpegDashUri = outputAsset.GetMpegDashUri();
                List<Uri> mp4ProgressiveDownloadUris = mp4AssetFiles.Select(af => af.GetSasUri()).ToList();

                string filePath = "asset-urls.txt";

                // 6. Save the URLs to a local file.
                smoothStreamingUri.Save(filePath);
                hlsUri.Save(filePath);
                mpegDashUri.Save(filePath);
                mp4ProgressiveDownloadUris.ForEach(uri => uri.Save(filePath));

                Console.WriteLine("Output asset available for adaptive streaming and progressive download.");
                Console.WriteLine("The URLs can be found at '{0}'.", Path.GetFullPath(filePath));

                string outputFolder = "job-output";
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                Console.WriteLine("Downloading output asset files to local folder...");

                // 7. Download the output asset to a local folder.
                context.DownloadAssetFilesToFolder(
                    outputAsset,
                    outputFolder,
                    (af, p) =>
                    {
                        Console.WriteLine("Downloading '{0}' - Progress: {1:0.##}%", af.Name, p.Progress);
                    });

                Console.WriteLine("Output asset files available at '{0}'.", Path.GetFullPath(outputFolder));

                Console.WriteLine("VOD workflow finished.");
            }
            catch (Exception exception)
            {
                // Parse the XML error message in the Media Services response and create a new 
                // exception with its content.
                exception = MediaServicesExceptionParser.Parse(exception);

                Console.Error.WriteLine(exception.Message);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
