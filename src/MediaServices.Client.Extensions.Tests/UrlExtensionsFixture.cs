// <copyright file="UrlExtensionsFixture.cs" company="open-source">
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
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UrlExtensionsFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowSaveIfUriIsNull()
        {
            Uri nullUri = null;

            nullUri.Save("uri.txt");
        }

        [TestMethod]
        public void ShouldSaveUriAndCreateFile()
        {
            var fileName = "uri.txt";
            var uri = new Uri("http://origin.net/streaming-token/manifest.ism/manifest", UriKind.Absolute);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            uri.Save(fileName);

            Assert.IsTrue(File.Exists(fileName));

            var fileContent = File.ReadAllLines(fileName);

            Assert.AreEqual(1, fileContent.Length);
            Assert.AreEqual(uri.OriginalString, fileContent[0]);
        }

        [TestMethod]
        public void ShouldSaveUriAndAppendToExistingFile()
        {
            var fileName = "uri.txt";
            var uri = new Uri("http://origin.net/streaming-token/manifest.ism/manifest", UriKind.Absolute);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.AppendAllLines(fileName, new[] { uri.OriginalString });

            uri.Save(fileName);

            Assert.IsTrue(File.Exists(fileName));

            var fileContent = File.ReadAllLines(fileName);

            Assert.AreEqual(2, fileContent.Length);
            Assert.AreEqual(uri.OriginalString, fileContent[0]);
            Assert.AreEqual(uri.OriginalString, fileContent[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowSaveIfStringIsNull()
        {
            string nullString = null;

            nullString.Save("uri.txt");
        }

        [TestMethod]
        public void ShouldSaveUrlAndCreateFile()
        {
            var fileName = "uri.txt";
            var url = "http://origin.net/streaming-token/manifest.ism/manifest";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            url.Save(fileName);

            Assert.IsTrue(File.Exists(fileName));

            var fileContent = File.ReadAllLines(fileName);

            Assert.AreEqual(1, fileContent.Length);
            Assert.AreEqual(url, fileContent[0]);
        }

        [TestMethod]
        public void ShouldSaveUrlAndAppendToExistingFile()
        {
            var fileName = "uri.txt";
            var url = "http://origin.net/streaming-token/manifest.ism/manifest";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.AppendAllLines(fileName, new[] { url });

            url.Save(fileName);

            Assert.IsTrue(File.Exists(fileName));

            var fileContent = File.ReadAllLines(fileName);

            Assert.AreEqual(2, fileContent.Length);
            Assert.AreEqual(url, fileContent[0]);
            Assert.AreEqual(url, fileContent[1]);
        }
    }
}
