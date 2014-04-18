using System;
using System.Net;
using NUnit.Framework;
using NServiceKit.Service;
using NServiceKit.ServiceClient.Web;
using NServiceKit.ServiceHost;
using NServiceKit.ServiceInterface.Testing;
using NServiceKit.Text;
using NServiceKit.WebHost.Endpoints.Support;
using NServiceKit.WebHost.IntegrationTests.Services;

namespace NServiceKit.WebHost.IntegrationTests.Tests
{
	/// <summary>
	/// This base class executes all Web Services ignorant of the endpoints its hosted on.
	/// The same tests below are re-used by the Unit and Integration TestFixture's declared below
	/// </summary>
	[TestFixture]
	public abstract class WebServicesTests
		: TestBase
	{
		private const string TestString = "NServiceKit";

		protected WebServicesTests()
			: base(Config.NServiceKitBaseUri, typeof(ReverseService).Assembly)
		{
		}

		protected override void Configure(Funq.Container container) { }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            EndpointHandlerBase.ServiceManager = null;
        }

		[Test]
		public void Does_Execute_ReverseService()
		{
			var client = CreateNewServiceClient();
			var response = client.Send<ReverseResponse>(
				new Reverse { Value = TestString });

			var expectedValue = ReverseService.Execute(TestString);
			Assert.That(response.Result, Is.EqualTo(expectedValue));
		}

		[Test]
		public void Does_Execute_Rot13Service()
		{
			var client = CreateNewServiceClient();
			var response = client.Send<Rot13Response>(new Rot13 { Value = TestString });

			var expectedValue = TestString.ToRot13();
			Assert.That(response.Result, Is.EqualTo(expectedValue));
		}

		[Test]
		public void Can_Handle_Exception_from_AlwaysThrowService()
		{
			var client = CreateNewServiceClient();
			try
			{
				var response = client.Send<AlwaysThrowsResponse>(
					new AlwaysThrows { Value = TestString });

				response.PrintDump();
				Assert.Fail("Should throw HTTP errors");
			}
			catch (WebServiceException webEx)
			{
				var response = (AlwaysThrowsResponse) webEx.ResponseDto;
				var expectedError = AlwaysThrowsService.GetErrorMessage(TestString);
				Assert.That(response.ResponseStatus.ErrorCode,
					Is.EqualTo(typeof(NotImplementedException).Name));
				Assert.That(response.ResponseStatus.Message,
					Is.EqualTo(expectedError));
			}
		}

        [Test]
        public void Request_items_are_preserved_between_filters()
        {
            var client = CreateNewServiceClient();
            if (client is DirectServiceClient) return;
            var response = client.Send<RequestItemsResponse>(new RequestItems { });
            Assert.That(response.Result, Is.EqualTo("MissionSuccess"));
        }
    }


	/// <summary>
	/// Unit tests simulates an in-process NServiceKit host where all services 
	/// are executed all in-memory so DTO's are not even serialized.
	/// </summary>
	public class UnitTests : WebServicesTests
	{
		protected override IServiceClient CreateNewServiceClient()
		{
            EndpointHandlerBase.ServiceManager = new ServiceManager(typeof(ReverseService).Assembly).Init();
			return new DirectServiceClient(this, EndpointHandlerBase.ServiceManager);
		}
	}

	public class XmlIntegrationTests : WebServicesTests
	{
		protected override IServiceClient CreateNewServiceClient()
		{
			return new XmlServiceClient(ServiceClientBaseUri);
		}
	}

	public class JsonIntegrationTests : WebServicesTests
	{
		protected override IServiceClient CreateNewServiceClient()
		{
			return new JsonServiceClient(ServiceClientBaseUri);
		}
	}

	public class JsvIntegrationTests : WebServicesTests
	{
		protected override IServiceClient CreateNewServiceClient()
		{
			return new JsvServiceClient(ServiceClientBaseUri);
		}
	}

	public class Soap11IntegrationTests : WebServicesTests
	{
		protected override IServiceClient CreateNewServiceClient()
		{
			return new Soap11ServiceClient(ServiceClientBaseUri);
		}
	}

	public class Soap12IntegrationTests : WebServicesTests
	{
		protected override IServiceClient CreateNewServiceClient()
		{
			return new Soap12ServiceClient(ServiceClientBaseUri);
		}
	}

}