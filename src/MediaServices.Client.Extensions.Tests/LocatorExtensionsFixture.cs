// <copyright file="LocatorExtensionsFixture.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by Mariano Converti
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
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;

    [TestClass]
    public class LocatorExtensionsFixture
    {
        private CloudMediaContext context;
        private IAsset asset;

        [TestMethod]
        public void ShouldThrowWhenCreateAccessPolicyAndLocatorIfContextIsNull()
        {
            CloudMediaContext nullContext = null;
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            try
            {
                nullContext.CreateLocator(this.asset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(1));
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldThrowWhenCreateAccessPolicyAndLocatorIfAssetIsNull()
        {
            IAsset nullAsset = null;

            try
            {
                this.context.CreateLocator(nullAsset, LocatorType.OnDemandOrigin, AccessPermissions.Read, TimeSpan.FromDays(1));
            }
            catch (AggregateException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void ShouldCreateAccessPolicyAndOriginLocator()
        {
            var locatorType = LocatorType.OnDemandOrigin;
            DateTime? locatorStartTime = null;
            var accessPolicyPermissions = AccessPermissions.Read;
            var accessPolicyDuration = TimeSpan.FromDays(1);
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, locatorType, accessPolicyPermissions, accessPolicyDuration, locatorStartTime);

            Assert.IsNotNull(locator);
            Assert.AreEqual(locatorType, locator.Type);
            Assert.AreEqual(locatorStartTime, locator.StartTime);

            var accessPolicy = locator.AccessPolicy;

            Assert.IsNotNull(accessPolicy);
            Assert.AreEqual(accessPolicyPermissions, accessPolicy.Permissions);
            Assert.AreEqual(accessPolicyDuration, accessPolicy.Duration);
        }

        [TestMethod]
        public void ShouldCreateAccessPolicyAndSasLocator()
        {
            var locatorType = LocatorType.Sas;
            DateTime? locatorStartTime = DateTime.Today;
            var accessPolicyPermissions = AccessPermissions.Read;
            var accessPolicyDuration = TimeSpan.FromDays(1);
            this.asset = this.context.Assets.Create("empty", AssetCreationOptions.None);

            var locator = this.context.CreateLocator(this.asset, locatorType, accessPolicyPermissions, accessPolicyDuration, locatorStartTime);

            Assert.IsNotNull(locator);
            Assert.AreEqual(locatorType, locator.Type);
            Assert.AreEqual(locatorStartTime, locator.StartTime);

            var accessPolicy = locator.AccessPolicy;

            Assert.IsNotNull(accessPolicy);
            Assert.AreEqual(accessPolicyPermissions, accessPolicy.Permissions);
            Assert.AreEqual(accessPolicyDuration, accessPolicy.Duration);
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

        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(
                ConfigurationManager.AppSettings["MediaServicesAccountName"],
                ConfigurationManager.AppSettings["MediaServicesAccountKey"]);
        }
    }
}
