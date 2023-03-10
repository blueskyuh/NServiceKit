using System;
using Funq;
using NUnit.Framework;
using NServiceKit.ServiceHost.Tests.Support;
using NServiceKit.ServiceHost.Tests.TypeFactory;
using NServiceKit.Text;
using NServiceKit.Text.Common;

namespace NServiceKit.ServiceHost.Tests
{
    /// <summary>A service host tests.</summary>
	[TestFixture]
	public class ServiceHostTests
	{
		private ServiceController serviceController;

        /// <summary>Executes the before each test action.</summary>
		[SetUp]
		public void OnBeforeEachTest()
		{
			serviceController = new ServiceController(null);
		}

        /// <summary>Can execute basic service.</summary>
		[Test]
		public void Can_execute_BasicService()
		{
			serviceController.Register(typeof(BasicRequest), typeof(BasicService));
			var result = serviceController.Execute(new BasicRequest()) as BasicRequestResponse;

			Assert.That(result, Is.Not.Null);
		}

        /// <summary>Can execute basic service from dynamic type.</summary>
		[Test]
		public void Can_execute_BasicService_from_dynamic_Type()
		{
			var requestType = typeof(BasicRequest);

			serviceController.Register(requestType, typeof(BasicService));

			object request = Activator.CreateInstance(requestType);

			var result = serviceController.Execute(request) as BasicRequestResponse;

			Assert.That(result, Is.Not.Null);
		}

        /// <summary>Can automatic wire types dynamically with reflection.</summary>
		[Test]
		public void Can_AutoWire_types_dynamically_with_reflection()
		{
			var serviceType = typeof(AutoWireService);

			var container = new Container();
			container.Register<IFoo>(c => new Foo());
			container.Register<IBar>(c => new Bar());

			var typeContainer = new ReflectionTypeFunqContainer(container);
			typeContainer.Register(serviceType);

			var service = container.Resolve<AutoWireService>();

			Assert.That(service.Foo, Is.Not.Null);
			Assert.That(service.Bar, Is.Not.Null);
		}

        /// <summary>Can automatic wire types dynamically with expressions.</summary>
		[Test]
		public void Can_AutoWire_types_dynamically_with_expressions()
		{
			var serviceType = typeof(AutoWireService);

			var container = new Container();
			container.Register<IFoo>(c => new Foo());
			container.Register<IBar>(c => new Bar());

			container.RegisterAutoWiredType(serviceType);

			var service = container.Resolve<AutoWireService>();

			Assert.That(service.Foo, Is.Not.Null);
			Assert.That(service.Bar, Is.Not.Null);
		}

        /// <summary>Can execute rest test service.</summary>
		[Test]
		public void Can_execute_RestTestService()
		{
            serviceController.Register(typeof(RestTest), typeof(RestTestService));
			var result = serviceController.Execute(new RestTest()) as RestTestResponse;

			Assert.That(result, Is.Not.Null);
			Assert.That(result.MethodName, Is.EqualTo("Execute"));
		}

        /// <summary>Can rest test service get.</summary>
		[Test]
		public void Can_RestTestService_GET()
		{
            serviceController.Register(typeof(RestTest), typeof(RestTestService));
			var result = serviceController.Execute(new RestTest(),
				new HttpRequestContext((object)null, EndpointAttributes.HttpGet)) as RestTestResponse;

			Assert.That(result, Is.Not.Null);
			Assert.That(result.MethodName, Is.EqualTo("Get"));
		}

        /// <summary>Can rest test service put.</summary>
		[Test]
		public void Can_RestTestService_PUT()
		{
            serviceController.Register(typeof(RestTest), typeof(RestTestService));
			var result = serviceController.Execute(new RestTest(),
				new HttpRequestContext((object)null, EndpointAttributes.HttpPut)) as RestTestResponse;

			Assert.That(result, Is.Not.Null);
			Assert.That(result.MethodName, Is.EqualTo("Put"));
		}

        /// <summary>Can rest test service post.</summary>
		[Test]
		public void Can_RestTestService_POST()
		{
            serviceController.Register(typeof(RestTest), typeof(RestTestService));
			var result = serviceController.Execute(new RestTest(),
				new HttpRequestContext((object)null, EndpointAttributes.HttpPost)) as RestTestResponse;

			Assert.That(result, Is.Not.Null);
			Assert.That(result.MethodName, Is.EqualTo("Post"));
		}

        /// <summary>Can rest test service delete.</summary>
		[Test]
		public void Can_RestTestService_DELETE()
		{
            serviceController.Register(typeof(RestTest), typeof(RestTestService));
			var result = serviceController.Execute(new RestTest(),
				new HttpRequestContext((object)null, EndpointAttributes.HttpDelete)) as RestTestResponse;

			Assert.That(result, Is.Not.Null);
			Assert.That(result.MethodName, Is.EqualTo("Delete"));
		}
	}
}
