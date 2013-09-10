// <copyright file="MediaServicesExceptionParserFixture.cs" company="open-source">
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;

    [TestClass]
    public class MediaServicesExceptionParserFixture
    {
        [TestMethod]
        public void ShoudParseMediaServicesExceptionErrorMessage()
        {
            var context = this.CreateContext();
            var asset = context.Assets.Create("EmptyAsset", AssetCreationOptions.None);

            try
            {
                asset.Delete();
                asset.Delete();
            }
            catch (Exception exception)
            {
                var parsedException = MediaServicesExceptionParser.Parse(exception);

                Assert.IsNotNull(parsedException);
                Assert.AreEqual("Resource Asset not found", parsedException.Message);
            }
        }

        [TestMethod]
        public void ShoudParseMediaServicesExceptionErrorMessageFromAggregateException()
        {
            var context = this.CreateContext();
            var asset = context.Assets.Create("EmptyAsset", AssetCreationOptions.None);

            try
            {
                asset.DeleteAsync().Wait();
                asset.DeleteAsync().Wait();
            }
            catch (Exception exception)
            {
                var parsedException = MediaServicesExceptionParser.Parse(exception);

                Assert.IsNotNull(parsedException);
                Assert.AreEqual("Resource Asset not found", parsedException.Message);
            }
        }

        [TestMethod]
        public void ShoudReturnOriginalExceptionIfCannotParseIt()
        {
            var exception = new Exception("exception message");
            var parsedException = MediaServicesExceptionParser.Parse(exception);

            Assert.AreSame(exception, parsedException);
        }

        [TestMethod]
        public void ShoudReturnNullIfOriginalExceptionIsNull()
        {
            var parsedException = MediaServicesExceptionParser.Parse(null);

            Assert.IsNull(parsedException);
        }

        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(
                ConfigurationManager.AppSettings["MediaServicesAccountName"],
                ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        }
    }
}
