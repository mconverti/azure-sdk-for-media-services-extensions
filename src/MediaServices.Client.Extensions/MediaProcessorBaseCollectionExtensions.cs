// <copyright file="MediaProcessorBaseCollectionExtensions.cs" company="open-source">
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
    using System.Linq;

    /// <summary>
    /// Contains extension methods and helpers for the <see cref="MediaProcessorBaseCollection"/> class.
    /// </summary>
    public static class MediaProcessorBaseCollectionExtensions
    {
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
    }
}
