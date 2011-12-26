﻿using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using ServiceStack.Service;
using ServiceStack.ServiceClient.Web;
using ServiceStack.WebHost.IntegrationTests.Services;

namespace ServiceStack.WebHost.IntegrationTests.Tests
{
	[TestFixture]
	public class HelloWorldServiceClientTests
	{
		public static IEnumerable ServiceClients
		{
			get
			{
				yield return new JsonServiceClient(Config.AbsoluteBaseUri);
				yield return new JsvServiceClient(Config.AbsoluteBaseUri);
				yield return new XmlServiceClient(Config.AbsoluteBaseUri);
				yield return new Soap11ServiceClient(Config.AbsoluteBaseUri);
				yield return new Soap12ServiceClient(Config.AbsoluteBaseUri);
			}
		}

		public static IEnumerable RestClients
		{
			get
			{
				yield return new JsonServiceClient(Config.AbsoluteBaseUri);
				yield return new JsvServiceClient(Config.AbsoluteBaseUri);
				yield return new XmlServiceClient(Config.AbsoluteBaseUri);
			}
		}

		[Test, TestCaseSource(typeof(HelloWorldServiceClientTests), "ServiceClients")]
		public void Sync_Call_HelloWorld_with_Sync_ServiceClients_on_Automatic_Routes(IServiceClient client)
		{
			var response = client.Send<HelloResponse>(new Hello { Name = "World!" });

			Assert.That(response.Result, Is.EqualTo("Hello, World!"));
		}

		[Test, TestCaseSource(typeof(HelloWorldServiceClientTests), "RestClients")]
		public void Async_Call_HelloWorld_with_ServiceClients_on_Automatic_Routes(IServiceClient client)
		{
			HelloResponse response = null;
			client.SendAsync<HelloResponse>(new Hello { Name = "World!" },
				r => response = r, (r,e) => Assert.Fail("NetworkError"));

			Thread.Sleep(TimeSpan.FromSeconds(1));

			Assert.That(response.Result, Is.EqualTo("Hello, World!"));
		}

		[Test, TestCaseSource(typeof(HelloWorldServiceClientTests), "RestClients")]
		public void Sync_Call_HelloWorld_with_RestClients_on_Custom_UserDefined_Routes(IRestClient client)
		{
			var response = client.Get<HelloResponse>("/hello/World!");

			Assert.That(response.Result, Is.EqualTo("Hello, World!"));
		}

		[Test, TestCaseSource(typeof(HelloWorldServiceClientTests), "RestClients")]
		public void Async_Call_HelloWorld_with_Async_ServiceClients_on_Automatic_Routes(IServiceClient client)
		{
			HelloResponse response = null;
			client.GetAsync<HelloResponse>("/hello/World!",
				r => response = r, (r, e) => Assert.Fail("NetworkError"));

			Thread.Sleep(TimeSpan.FromSeconds(1));

			Assert.That(response.Result, Is.EqualTo("Hello, World!"));
		}
	}
}