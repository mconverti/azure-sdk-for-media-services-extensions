// <copyright file="MediaProcessorBaseCollectionExtensionsFixture.cs" company="open-source">
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
    using System.Configuration;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;

    [TestClass]
    public class MediaProcessorBaseCollectionExtensionsFixture
    {
        private CloudMediaContext context;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWhenGetLatestMediaProcessorByNameIfMediaProcessorCollectionIsNull()
        {
            MediaProcessorBaseCollection nullMediaProcessorCollection = null;

            nullMediaProcessorCollection.GetLatestMediaProcessorByName(MediaProcessorNames.WindowsAzureMediaEncoder);
        }

        [TestMethod]
        public void ShouldGetLatestMediaProcessorByNameReturnNullIfMediaProcessorNameIsNotValid()
        {
            var mediaProcessorName = "Invalid Media Processor Name";
            var mediaProcessor = this.context.MediaProcessors.GetLatestMediaProcessorByName(mediaProcessorName);

            Assert.IsNull(mediaProcessor);
        }

        [TestMethod]
        public void ShouldGetLatestMediaProcessorByName()
        {
            var mediaProcessor = this.context.MediaProcessors.GetLatestMediaProcessorByName(MediaProcessorNames.WindowsAzureMediaEncoder);

            Assert.IsNotNull(mediaProcessor);

            var expectedMediaProcessor = this.context.MediaProcessors
                .Where(mp => mp.Name == MediaProcessorNames.WindowsAzureMediaEncoder)
                .ToList()
                .Select(mp => new { mp.Id, mp.Name, Version = new Version(mp.Version) })
                .OrderBy(mp => mp.Version)
                .Last();

            Assert.AreEqual(expectedMediaProcessor.Id, mediaProcessor.Id);
            Assert.AreEqual(expectedMediaProcessor.Name, mediaProcessor.Name);
            Assert.AreEqual(expectedMediaProcessor.Version, new Version(mediaProcessor.Version));
        }

        [TestInitialize]
        public void Initialize()
        {
            this.context = this.CreateContext();
        }

        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(
                ConfigurationManager.AppSettings["MediaServicesAccountName"],
                ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        }
    }
}
