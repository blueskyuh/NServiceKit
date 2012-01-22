﻿using System;
using System.Linq;
using System.Net;
using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints.Extensions;

namespace ServiceStack.ServiceInterface
{
	/// <summary>
	/// Indicates that the request dto, which is associated with this attribute,
	/// requires authentication.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method /*MVC Actions*/, Inherited = false, AllowMultiple = false)]
	public class AuthenticateAttribute : RequestFilterAttribute
	{
		public string Provider { get; set; }

		public AuthenticateAttribute()
			: base(ApplyTo.All)
		{
		}

		public AuthenticateAttribute(string provider)
			: base(ApplyTo.All)
		{
			this.Provider = provider;
		}

		public AuthenticateAttribute(ApplyTo applyTo)
			: base(applyTo)
		{
		}

		public AuthenticateAttribute(ApplyTo applyTo, string provider)
			: base(applyTo)
		{
			this.Provider = provider;
		}

		public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
		{
			if (AuthService.AuthProviders == null) throw new InvalidOperationException("The AuthService must be initialized by calling "
				 + "AuthService.Init to use an authenticate attribute");

			var matchingOAuthConfigs = AuthService.AuthProviders.Where(x =>
							this.Provider.IsNullOrEmpty()
							|| x.Provider == this.Provider).ToList();

			if (matchingOAuthConfigs.Count == 0)
			{
				res.WriteError(req, requestDto, "No OAuth Configs found matching {0} provider"
					.Fmt(this.Provider ?? "any"));
				res.Close();
				return;
			}

			AuthenticateIfBasicAuth(req, res);

			using (var cache = req.GetCacheClient())
			{
				var sessionId = req.GetSessionId();
				var session = sessionId != null ? cache.GetSession(sessionId) : null;

				if (session == null || !matchingOAuthConfigs.Any(x => session.IsAuthorized(x.Provider)))
				{
					res.StatusCode = (int)HttpStatusCode.Unauthorized;
					res.AddHeader(HttpHeaders.WwwAuthenticate, "{0} realm=\"{1}\""
						.Fmt(matchingOAuthConfigs[0].Provider, matchingOAuthConfigs[0].AuthRealm));

					res.Close();
				}
			}
		}

		public static void AuthenticateIfBasicAuth(IHttpRequest req, IHttpResponse res)
		{
			var userPass = req.GetBasicAuthUserAndPassword();
			if (userPass != null)
			{
				var authService = req.TryResolve<AuthService>();
				authService.RequestContext = new HttpRequestContext(req, res, null);
				var response = authService.Post(new Auth.Auth {
					provider = BasicAuthProvider.Name,
					UserName = userPass.Value.Key,
					Password = userPass.Value.Value
				});
			}
		}
	}
}