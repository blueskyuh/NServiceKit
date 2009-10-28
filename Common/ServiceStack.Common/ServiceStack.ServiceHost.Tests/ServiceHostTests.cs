﻿using System;
using System.Runtime.Serialization;
using Funq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using ServiceStack.LogicFacade;
using ServiceStack.ServiceHost.Tests.Support;
using ServiceStack.ServiceHost.Tests.TypeFactory;

namespace ServiceStack.ServiceHost.Tests
{
	[TestFixture]
	public class ServiceHostTests
	{
	
		[Test]
		public void Can_execute_BasicService()
		{
			var serviceController = new ServiceController();

			serviceController.Register(() => new BasicService());
			var result = serviceController.Execute(new BasicRequest()) as BasicRequest;

			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Can_execute_BasicService_from_dynamic_Type()
		{
			var requestType = typeof(BasicRequest);

			var serviceController = new ServiceController();
			serviceController.Register(requestType, typeof(BasicService));

			object request = Activator.CreateInstance(requestType);

			var result = serviceController.Execute(request) as BasicRequest;

			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Can_AutoWire_types_dynamically()
		{
			var serviceType = typeof (AutoWireService);

			var container = new Container();
			container.Register<IFoo>(c => new Foo());
			container.Register<IBar>(c => new Bar());

			var funqlet = new Funqlet(serviceType);
			funqlet.Configure(container);

			var service = container.Resolve<AutoWireService>();

			Assert.That(service.Foo, Is.Not.Null);
			Assert.That(service.Bar, Is.Not.Null);
		}

	}
}
