// <copyright file="UrlExtensions.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by Mariano Converti
//   
//  Redistribution and use in source and binary forms, with or without modification, are permitted.
//
//  The names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

namespace System
{
    using System.IO;

    /// <summary>
    /// Contains extension methods and helpers related to the <see cref="Uri"/> and <see cref="String"/> classes.
    /// </summary>
    public static class UrlExtensions
    {
        /// <summary>
        /// Saves the <paramref name="uri"/> to a local file path. It creates the file if does not exist; otherwise, appends a new line to the end.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> instance to save to a file.</param>
        /// <param name="filePath">The path to the file to save the <paramref name="uri"/>.</param>
        public static void Save(this Uri uri, string filePath)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri", "The uri cannot be null.");
            }

            File.AppendAllLines(filePath, new[] { uri.OriginalString });
        }

        /// <summary>
        /// Saves the <paramref name="url"/> to a local file path. It creates the file if does not exist; otherwise, appends a new line to the end.
        /// </summary>
        /// <param name="url">The <see cref="String"/> instance to save to a file.</param>
        /// <param name="filePath">The path to the file to save the <paramref name="url"/>.</param>
        public static void Save(this string url, string filePath)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url", "The url cannot be null.");
            }

            File.AppendAllLines(filePath, new[] { url });
        }
    }
}
