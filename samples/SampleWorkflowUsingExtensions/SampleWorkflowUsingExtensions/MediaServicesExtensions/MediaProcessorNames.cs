// <copyright file="MediaProcessorNames.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by mcabral
//   
//  Redistribution and use in source and binary forms, with or without modification, are permitted.
//
//  The names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Contains string constants with the available media processors' names.
    /// </summary>
    public static class MediaProcessorNames
    {
        /// <summary>
        /// Lets you run encoding tasks using the Media Encoder.
        /// </summary>
        public const string WindowsAzureMediaEncoder = "Windows Azure Media Encoder";

        /// <summary>
        /// Lets you convert media assets from .mp4 to smooth streaming format. Also, lets you convert media assets 
        /// from smooth streaming to the Apple HTTP Live Streaming (HLS) format.
        /// </summary>
        public const string WindowsAzureMediaPackager = "Windows Azure Media Packager";

        /// <summary>
        /// Lets you encrypt media assets using PlayReady Protection.
        /// </summary>
        public const string WindowsAzureMediaEncryptor = "Windows Azure Media Encryptor";

        /// <summary>
        /// Lets you decrypt media assets that were encrypted using storage encryption.
        /// </summary>
        public const string StorageDecryption = "Storage Decryption";
    }
}
