// <copyright file="ILocatorExtensions.cs" company="open-source">
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
    /// Contains extension methods and helpers for the <see cref="ILocator"/> interface.
    /// </summary>
    public static class ILocatorExtensions
    {
        /// <summary>
        /// Represents the manifest file extension.
        /// </summary>
        public const string ManifestFileExtension = ".ism";

        /// <summary>
        /// Represents the URL dynamic packaging parameter for HLS.
        /// </summary>
        public const string HlsStreamingParameter = "(format=m3u8-aapl)";

        /// <summary>
        /// Represents the URL dynamic packaging parameter for MPEG-DASH.
        /// </summary>
        public const string MpegDashStreamingParameter = "(format=mpd-time-csf)";

        internal const string BaseStreamingUrlTemplate = "{0}/{1}/manifest{2}";

        /// <summary>
        /// Returns the Smooth Streaming URL of the <paramref name="originLocator"/>; otherwise, null.
        /// </summary>
        /// <param name="originLocator">The <see cref="ILocator"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the Smooth Streaming URL of the <paramref name="originLocator"/>; otherwise, null.</returns>
        public static Uri GetSmoothStreamingUri(this ILocator originLocator)
        {
            return originLocator.GetStreamingUri(string.Empty);
        }

        /// <summary>
        /// Returns the HLS URL of the <paramref name="originLocator"/>; otherwise, null.
        /// </summary>
        /// <param name="originLocator">The <see cref="ILocator"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the HLS URL of the <paramref name="originLocator"/>; otherwise, null.</returns>
        public static Uri GetHlsUri(this ILocator originLocator)
        {
            return originLocator.GetStreamingUri(HlsStreamingParameter);
        }

        /// <summary>
        /// Returns the MPEG-DASH URL of the <paramref name="originLocator"/>; otherwise, null.
        /// </summary>
        /// <param name="originLocator">The <see cref="ILocator"/> instance.</param>
        /// <returns>A <see cref="System.Uri"/> representing the MPEG-DASH URL of the <paramref name="originLocator"/>; otherwise, null.</returns>
        public static Uri GetMpegDashUri(this ILocator originLocator)
        {
            return originLocator.GetStreamingUri(MpegDashStreamingParameter);
        }

        private static Uri GetStreamingUri(this ILocator originLocator, string streamingParameter)
        {
            if (originLocator == null)
            {
                throw new ArgumentNullException("locator", "The locator cannot be null.");
            }

            if (originLocator.Type != LocatorType.OnDemandOrigin)
            {
                throw new ArgumentException("The locator type must be on-demand origin.", "originLocator");
            }

            Uri smoothStreamingUri = null;
            IAsset asset = originLocator.Asset;
            IAssetFile manifestAssetFile = asset.GetManifestAssetFile();
            if (manifestAssetFile != null)
            {
                smoothStreamingUri = new Uri(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        BaseStreamingUrlTemplate,
                        originLocator.Path.TrimEnd('/'),
                        manifestAssetFile.Name,
                        streamingParameter),
                    UriKind.Absolute);
            }

            return smoothStreamingUri;
        }
    }
}
