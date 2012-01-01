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
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class AuthenticateAttribute : RequestFilterAttribute
	{
		public string Provider { get; set; }
        public ApplyTo ApplyTo { get; set; }

        public AuthenticateAttribute()
            : this(ApplyTo.All)
        {
        }

		public AuthenticateAttribute(string provider)
            : this(ApplyTo.All, provider)
		{
		}

        public AuthenticateAttribute(ApplyTo applyTo)
        {
            this.ApplyTo = applyTo;
        }

        public AuthenticateAttribute(ApplyTo applyTo, string provider)
        {
            this.Provider = provider;
            this.ApplyTo = applyTo;
        }

        public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            var matchingOAuthConfigs = AuthService.AuthConfigs.Where(x =>
                            this.Provider.IsNullOrEmpty()
                            || x.Provider == this.Provider).ToList();

            if (matchingOAuthConfigs.Count == 0)
            {
                res.WriteError(req, requestDto, "No OAuth Configs found matching {0} provider"
                    .Fmt(this.Provider ?? "any"));
                res.Close();
                return;
            }

            using (var cache = req.GetCacheClient())
            {
                var sessionId = req.GetPermanentSessionId();
                var session = sessionId != null ? cache.GetSession(sessionId) : null;

                if (session == null || !matchingOAuthConfigs.Any(x => session.IsAuthorized(x.Provider)))
                {
                    res.StatusCode = (int)HttpStatusCode.Unauthorized;
                    res.AddHeader(HttpHeaders.WwwAuthenticate, "OAuth realm=\"{0}\"".Fmt(matchingOAuthConfigs[0].AuthRealm));
                    res.Close();
                    return;
                }
            }
        }
    }
}