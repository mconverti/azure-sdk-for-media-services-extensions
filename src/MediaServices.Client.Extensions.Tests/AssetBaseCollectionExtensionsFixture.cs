// <copyright file="AssetBaseCollectionExtensionsFixture.cs" company="open-source">
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
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;

    [TestClass]
    public class AssetBaseCollectionExtensionsFixture
    {
        private CloudMediaContext context;
        private IAsset asset;

        [TestMethod]
        public void ShouldThrowCreateAssetFromFileIfAssetCollectionIsNull()
        {
            AssetBaseCollection nullAssets = null;

            try
            {
                nullAssets.CreateFromFileAsync(string.Empty, AssetCreationOptions.None, CancellationToken.None).Wait();
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldCreateAssetFromFile()
        {
            var fileName = "smallwmv1.wmv";
            this.asset = this.context.Assets.CreateFromFile(fileName, null, AssetCreationOptions.None);
            var assetId = this.asset.Id;

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(fileName, this.asset.Name);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(1, assetFiles.Count());
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(0).Name);

            this.context = this.CreateContext();
            Assert.AreEqual(0, this.context.Locators.Where(l => l.AssetId == assetId).Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv")]
        public void ShouldCreateAssetFromFileWithUploadProgressChangedCallback()
        {
            var uploadResults = new ConcurrentDictionary<string, UploadProgressChangedEventArgs>();
            Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback =
                (af, e) =>
                {
                    IAssetFile assetFile = af;
                    UploadProgressChangedEventArgs eventArgs = e;

                    Assert.IsNotNull(assetFile);
                    Assert.IsNotNull(eventArgs);

                    uploadResults.AddOrUpdate(assetFile.Name, eventArgs, (k, e2) => eventArgs);
                };

            var fileName = "smallwmv1.wmv";
            this.asset = this.context.Assets.CreateFromFile(fileName, AssetCreationOptions.None, uploadProgressChangedCallback);
            var assetId = this.asset.Id;

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(fileName, this.asset.Name);

            Assert.AreEqual(1, uploadResults.Count);

            AssertUploadedFile(".\\", fileName, uploadResults[fileName]);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(1, assetFiles.Count());
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(0).Name);

            this.context = this.CreateContext();
            Assert.AreEqual(0, this.context.Locators.Where(l => l.AssetId == assetId).Count());
        }

        [TestMethod]
        public void ShouldThrowCreateAssetFromFolderIfAssetCollectionIsNull()
        {
            AssetBaseCollection nullAssets = null;

            try
            {
                nullAssets.CreateFromFolderAsync(string.Empty, null, AssetCreationOptions.None, CancellationToken.None);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowCreateAssetFromFolderIfFolderDoesNotContainAnyFiles()
        {
            var emptyFolderName = "EmptyMediaFolder";
            if (Directory.Exists(emptyFolderName))
            {
                Directory.Delete(emptyFolderName, true);
            }

            Directory.CreateDirectory(emptyFolderName);

            try
            {
                this.context.Assets.CreateFromFolderAsync(emptyFolderName, AssetCreationOptions.None, CancellationToken.None);
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(FileNotFoundException));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldCreateAssetFromFolder()
        {
            var folderName = "Media";
            this.asset = this.context.Assets.CreateFromFolder(folderName, null, AssetCreationOptions.None);
            var assetId = this.asset.Id;

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(folderName, this.asset.Name);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(3, assetFiles.Count());
            Assert.AreEqual("dummy.ism", assetFiles.ElementAt(0).Name);
            Assert.IsTrue(assetFiles.ElementAt(0).IsPrimary);
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(1).Name);
            Assert.IsFalse(assetFiles.ElementAt(1).IsPrimary);
            Assert.AreEqual("smallwmv2.wmv", assetFiles.ElementAt(2).Name);
            Assert.IsFalse(assetFiles.ElementAt(2).IsPrimary);

            this.context = this.CreateContext();
            Assert.AreEqual(0, this.context.Locators.Where(l => l.AssetId == assetId).Count());
        }

        [TestMethod]
        [DeploymentItem(@"Media\smallwmv1.wmv", "Media")]
        [DeploymentItem(@"Media\smallwmv2.wmv", "Media")]
        [DeploymentItem(@"Media\dummy.ism", "Media")]
        public void ShouldCreateAssetFromFolderWithUploadProgressChangedCallback()
        {
            var uploadResults = new ConcurrentDictionary<string, UploadProgressChangedEventArgs>();
            Action<IAssetFile, UploadProgressChangedEventArgs> uploadProgressChangedCallback =
                (af, e) =>
                {
                    IAssetFile assetFile = af;
                    UploadProgressChangedEventArgs eventArgs = e;

                    Assert.IsNotNull(assetFile);
                    Assert.IsNotNull(eventArgs);

                    uploadResults.AddOrUpdate(assetFile.Name, eventArgs, (k, e2) => eventArgs);
                };

            var folderName = "Media";
            this.asset = this.context.Assets.CreateFromFolder(folderName, AssetCreationOptions.None, uploadProgressChangedCallback);
            var assetId = this.asset.Id;

            Assert.IsNotNull(this.asset);
            Assert.AreEqual(folderName, this.asset.Name);

            Assert.AreEqual(3, uploadResults.Count);

            AssertUploadedFile(folderName, "smallwmv1.wmv", uploadResults["smallwmv1.wmv"]);
            AssertUploadedFile(folderName, "smallwmv2.wmv", uploadResults["smallwmv2.wmv"]);
            AssertUploadedFile(folderName, "dummy.ism", uploadResults["dummy.ism"]);

            var assetFiles = this.asset.AssetFiles.ToList().OrderBy(a => a.Name);

            Assert.AreEqual(3, assetFiles.Count());
            Assert.AreEqual("dummy.ism", assetFiles.ElementAt(0).Name);
            Assert.IsTrue(assetFiles.ElementAt(0).IsPrimary);
            Assert.AreEqual("smallwmv1.wmv", assetFiles.ElementAt(1).Name);
            Assert.IsFalse(assetFiles.ElementAt(1).IsPrimary);
            Assert.AreEqual("smallwmv2.wmv", assetFiles.ElementAt(2).Name);
            Assert.IsFalse(assetFiles.ElementAt(2).IsPrimary);

            this.context = this.CreateContext();
            Assert.AreEqual(0, this.context.Locators.Where(l => l.AssetId == assetId).Count());
        }

        [TestInitialize]
        public void Initialize()
        {
            this.context = this.CreateContext();
            this.asset = null;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (this.asset != null)
            {
                this.asset.Delete();
            }
        }

        private static void AssertUploadedFile(string originalFolderPath, string fileName, UploadProgressChangedEventArgs uploadProgressChangedEventArgs)
        {
            var expected = new FileInfo(Path.Combine(originalFolderPath, fileName));

            Assert.AreEqual(expected.Length, uploadProgressChangedEventArgs.BytesUploaded);
            Assert.AreEqual(expected.Length, uploadProgressChangedEventArgs.TotalBytes);
            Assert.AreEqual(100, uploadProgressChangedEventArgs.Progress);
        }

        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(
                ConfigurationManager.AppSettings["MediaServicesAccountName"],
                ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        }
    }
}
