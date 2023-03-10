using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NServiceKit.Common.Tests.ServiceClient.Web
{
    /// <summary>A service client base tests.</summary>
    [TestFixture]
    public class ServiceClientBaseTests
    {
        /// <summary>Sets base URI format loaded format used in synchronise and asynchronous URI.</summary>
        [Test]
        public void SetBaseUri_FormatLoaded_LoadedFormatUsedInSyncAndAsyncUri()
        {
            var serviceClientBaseTester = new ServiceClientBaseTester();
            String baseUri = "BaseURI";

            serviceClientBaseTester.SetBaseUri(baseUri);

            String expectedBaseUri = baseUri;
            String expectedSyncReplyBaseUri = baseUri + "/" + serviceClientBaseTester.Format + "/syncreply/";
            String expectedAsyncOneWayBaseUri = baseUri + "/" + serviceClientBaseTester.Format + "/asynconeway/";
            Assert.That(serviceClientBaseTester.BaseUri, Is.EqualTo(expectedBaseUri));
            Assert.That(serviceClientBaseTester.SyncReplyBaseUri, Is.EqualTo(expectedSyncReplyBaseUri));
            Assert.That(serviceClientBaseTester.AsyncOneWayBaseUri, Is.EqualTo(expectedAsyncOneWayBaseUri));
        }
    }
}
