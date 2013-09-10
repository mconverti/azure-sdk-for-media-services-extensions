// <copyright file="TaskMock.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by mconverti
//   
//  Redistribution and use in source and binary forms, with or without modification, are permitted.
//
//  The names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

namespace MediaServices.Client.Extensions.Tests.Mocks
{
    using System;
    using System.Collections.ObjectModel;
    using Microsoft.WindowsAzure.MediaServices.Client;

    public class TaskMock : ITask
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string MediaProcessorId { get; set; }

        public TaskOptions Options { get; set; }

        public string Configuration { get; set; }

        public string EncryptionKeyId { get; set; }

        public string EncryptionScheme { get; set; }

        public string EncryptionVersion { get; set; }

        public string InitializationVector { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
        
        public string PerfMessage { get; set; }

        public int Priority { get; set; }

        public double Progress { get; set; }

        public TimeSpan RunningDuration { get; set; }

        public JobState State { get; set; }

        public string TaskBody { get; set; }

        public ReadOnlyCollection<ErrorDetail> ErrorDetails { get; set; }

        public ReadOnlyCollection<TaskHistoricalEvent> HistoricalEvents { get; set; }

        public InputAssetCollection<IAsset> InputAssets { get; set; }

        public OutputAssetCollection OutputAssets { get; set; }

        public string GetClearConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}
