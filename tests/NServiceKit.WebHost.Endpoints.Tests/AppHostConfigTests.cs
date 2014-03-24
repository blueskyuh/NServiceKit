using System;
using NUnit.Framework;
using NServiceKit.Logging;
using NServiceKit.Logging.Support.Logging;
using NServiceKit.ServiceClient.Web;
using NServiceKit.Text;
using NServiceKit.WebHost.Endpoints.Tests.Support.Host;

namespace NServiceKit.WebHost.Endpoints.Tests
{
	[TestFixture]
	public class AppHostConfigTests
	{
		protected const string ListeningOn = "http://localhost:85/";

		TestConfigAppHostHttpListener appHost;

		[TestFixtureSetUp]
		public void OnTestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

			appHost = new TestConfigAppHostHttpListener();
			appHost.Init();
			appHost.Start(ListeningOn);
		}

		[TestFixtureTearDown]
		public void OnTestFixtureTearDown()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (appHost == null) return;
			appHost.Dispose();
			appHost = null;
		}

		
		[Test]
		public void Actually_uses_the_BclJsonSerializers()
		{
			var json = (ListeningOn + "login/user/pass").GetJsonFromUrl();

			Console.WriteLine(json);
			Assert.That(json, Is.EqualTo("{\"pwd\":\"pass\",\"uname\":\"user\"}"));
		}
	}
}