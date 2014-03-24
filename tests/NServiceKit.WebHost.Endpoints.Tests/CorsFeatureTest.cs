﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NServiceKit.Common.Web;
using NServiceKit.ServiceHost;
using NUnit.Framework;
using NServiceKit.ServiceClient.Web;
using NServiceKit.Service;
using NServiceKit.ServiceInterface;
using NServiceKit.CacheAccess;
using NServiceKit.CacheAccess.Providers;
using NServiceKit.ServiceInterface.Cors;
using NServiceKit.Text;
using NServiceKit.WebHost.Endpoints.Utils;

namespace NServiceKit.WebHost.Endpoints.Tests
{
    [Route("/corsmethod")]
    public class CorsFeatureRequest
    {
    }

    [CorsSupport("http://localhost http://localhost2", "POST, GET", "Type1, Type2", true)]
    public class CorsFeatureResponse
    {
        public bool IsSuccess { get; set; }
    }

    public class CorsFeatureService : IService<CorsFeatureRequest>
    {
        public object Execute(CorsFeatureRequest request)
        {
            return new CorsFeatureResponse { IsSuccess = true };
        }
    }

    [Route("/globalcorsfeature")]
    public class GlobalCorsFeatureRequest
    {
    }

    public class GlobalCorsFeatureResponse
    {
        public bool IsSuccess { get; set; }
    }

    public class GlobalCorsFeatureService : IService<GlobalCorsFeatureRequest>
    {
        public object Execute(GlobalCorsFeatureRequest request)
        {
            return new GlobalCorsFeatureResponse { IsSuccess = true };
        }
    }

    [TestFixture]
    public class CorsFeatureServiceTest
    {
        private const string ListeningOn = "http://localhost:8022/";
        private const string ServiceClientBaseUri = "http://localhost:8022/";

        public class CorsFeatureAppHostHttpListener
            : AppHostHttpListenerBase
        {

            public CorsFeatureAppHostHttpListener()
                : base("Cors Feature Tests", typeof(CorsFeatureService).Assembly) { }

            public override void Configure(Funq.Container container)
            {
            }
        }

        CorsFeatureAppHostHttpListener appHost;

        [TestFixtureSetUp]
        public void OnTestFixtureSetUp()
        {
            appHost = new CorsFeatureAppHostHttpListener();
            appHost.Init();
            appHost.Start(ListeningOn);
        }

        [TestFixtureTearDown]
        public void OnTestFixtureTearDown()
        {
            appHost.Dispose();
        }

        static IRestClient[] RestClients = 
        {
            new JsonServiceClient(ServiceClientBaseUri),
            new XmlServiceClient(ServiceClientBaseUri),
            new JsvServiceClient(ServiceClientBaseUri)
        };

        [Test, Explicit]
        public void RunFor5Mins()
        {
            Thread.Sleep(TimeSpan.FromMinutes(5));
        }

        [Test, TestCaseSource("RestClients")]
        public void CorsMethodHasAccessControlHeaders(IRestClient client)
        {
            appHost.Config.GlobalResponseHeaders.Clear();

            var response = RequestContextTests.GetResponseHeaders(ListeningOn + "/corsmethod");
            Assert.That(response[HttpHeaders.AllowOrigin], Is.EqualTo("http://localhost http://localhost2"));
            Assert.That(response[HttpHeaders.AllowMethods], Is.EqualTo("POST, GET"));
            Assert.That(response[HttpHeaders.AllowHeaders], Is.EqualTo("Type1, Type2"));
            Assert.That(response[HttpHeaders.AllowCredentials], Is.EqualTo("true"));
        }

        [Test, TestCaseSource("RestClients")]
        public void GlobalCorsHasAccessControlHeaders(IRestClient client)
        {
            appHost.LoadPlugin(new CorsFeature());

            var response = RequestContextTests.GetResponseHeaders(ListeningOn + "/globalcorsfeature");
            Assert.That(response[HttpHeaders.AllowOrigin], Is.EqualTo("*"));
            Assert.That(response[HttpHeaders.AllowMethods], Is.EqualTo("GET, POST, PUT, DELETE, OPTIONS"));
            Assert.False(response.ContainsKey(HttpHeaders.AllowCredentials));
            Assert.That(response[HttpHeaders.AllowHeaders], Is.EqualTo("Content-Type"));
        }

    }
}
