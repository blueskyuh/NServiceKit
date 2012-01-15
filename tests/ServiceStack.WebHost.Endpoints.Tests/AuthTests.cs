﻿using System;
using System.Net;
using Funq;
using NUnit.Framework;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Service;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.Text;
using ServiceStack.WebHost.IntegrationTests.Services;

namespace ServiceStack.WebHost.Endpoints.Tests
{
	public class Secured
	{
		public string Name { get; set; }
	}

	public class SecuredResponse
	{
		public string Result { get; set; }

		public ResponseStatus ResponseStatus { get; set; }
	}

	[Authenticate]
	public class SecuredService : ServiceBase<Secured>
	{
		protected override object Run(Secured request)
		{
			return new SecuredResponse { Result = request.Name };
		}
	}

	public class CustomUserSession : AuthUserSession
	{
	}

	public class AuthTests
	{
		private const string ListeningOn = "http://localhost:82/";

		private const string UserName = "user";
		private const string Password = "p@55word";

		public class AuthAppHostHttpListener
			: AppHostHttpListenerBase
		{
			public AuthAppHostHttpListener()
				: base("Validation Tests", typeof(CustomerService).Assembly) { }

			public override void Configure(Container container)
			{
				AuthFeature.Init(this, () => new CustomUserSession(),
					new AuthProvider[] {
						new CredentialsAuthProvider(), //HTML Form post of UserName/Password credentials
						new BasicAuthProvider(), //Sign-in with Basic Auth
					});

				container.Register<ICacheClient>(new MemoryCacheClient());
				var userRep = new InMemoryAuthRepository();
				container.Register<IUserAuthRepository>(userRep);

				string hash;
				string salt;
				new SaltedHash().GetHashAndSaltString(Password, out hash, out salt);

				userRep.CreateUserAuth(new UserAuth {
					Id = 1,
					DisplayName = "DisplayName",
					Email = "as@if.com",
					UserName = UserName,
					FirstName = "FirstName",
					LastName = "LastName",
					PasswordHash = hash,
					Salt = salt,
				}, Password);
			}
		}

		AuthAppHostHttpListener appHost;

		[TestFixtureSetUp]
		public void OnTestFixtureSetUp()
		{
			appHost = new AuthAppHostHttpListener();
			appHost.Init();
			appHost.Start(ListeningOn);
		}

		[TestFixtureTearDown]
		public void OnTestFixtureTearDown()
		{
			appHost.Dispose();
		}

		IServiceClient GetClient()
		{
			return new JsonServiceClient(ListeningOn);
		}

		IServiceClient GetClientWithUserPassword()
		{
			return new JsonServiceClient(ListeningOn) {
				UserName = UserName,
				Password = Password,
			};
		}

		[Test]
		public void No_Credentials_throws_UnAuthorized()
		{
			try
			{
				var client = GetClient();
				var request = new Secured { Name = "test" };
				var response = client.Send<SecureResponse>(request);

				Assert.Fail("Shouldn't be allowed");
			}
			catch (WebServiceException webEx)
			{
				Assert.That(webEx.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
				Console.WriteLine(webEx.ResponseDto.Dump());
			}
		}

		[Test]
		public void Does_work_with_BasicAuth()
		{
			try
			{
				var client = GetClientWithUserPassword();
				var request = new Secured { Name = "test" };
				var response = client.Send<SecureResponse>(request);
				Assert.That(response.Result, Is.EqualTo(request.Name));
			}
			catch (WebServiceException webEx)
			{
				Assert.Fail(webEx.Message);
			}
		}

		[Test]
		public void Does_always_send_BasicAuth()
		{
			try
			{
				var client = (ServiceClientBase)GetClientWithUserPassword();
				client.AlwaysSendBasicAuthHeader = true;
				client.LocalHttpWebRequestFilter = req => {
						bool hasAuthentication = false;
						foreach (var key in req.Headers.Keys)
						{
							if (key.ToString() == "Authorization")
								hasAuthentication = true;
						}
						Assert.IsTrue(hasAuthentication);
					};

				var request = new Secured { Name = "test" };
				var response = client.Send<SecureResponse>(request);
				Assert.That(response.Result, Is.EqualTo(request.Name));
			}
			catch (WebServiceException webEx)
			{
				Assert.Fail(webEx.Message);
			}
		}

		[Test]
		public void Does_work_with_CredentailsAuth()
		{
			try
			{
				var client = GetClient();

				var authResponse = client.Send<AuthResponse>(new Auth {
					provider = CredentialsAuthProvider.Name,
					UserName = "user",
					Password = "p@55word",
					RememberMe = true,
				});

				Console.WriteLine(authResponse.Dump());

				var request = new Secured { Name = "test" };
				var response = client.Send<SecureResponse>(request);
				Assert.That(response.Result, Is.EqualTo(request.Name));
			}
			catch (WebServiceException webEx)
			{
				Assert.Fail(webEx.Message);
			}
		}

	}
}