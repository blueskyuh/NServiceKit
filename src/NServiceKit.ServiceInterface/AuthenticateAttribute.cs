using System;
using System.Collections.Specialized;
using System.Linq;
using NServiceKit.Common;
using NServiceKit.Common.Web;
using NServiceKit.ServiceHost;
using NServiceKit.ServiceInterface.Auth;
using NServiceKit.Text;
using NServiceKit.WebHost.Endpoints;
using NServiceKit.WebHost.Endpoints.Extensions;

namespace NServiceKit.ServiceInterface
{
    /// <summary>
    /// Indicates that the request dto, which is associated with this attribute,
    /// requires authentication.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AuthenticateAttribute : RequestFilterAttribute
    {
        /// <summary>
        /// Restrict authentication to a specific <see cref="IAuthProvider"/>.
        /// For example, if this attribute should only permit access
        /// if the user is authenticated with <see cref="BasicAuthProvider"/>,
        /// you should set this property to <see cref="BasicAuthProvider.Name"/>.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Redirect the client to a specific URL if authentication failed.
        /// If this property is null, simply `401 Unauthorized` is returned.
        /// </summary>
        public string HtmlRedirect { get; set; }

        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.AuthenticateAttribute class.</summary>
        ///
        /// <param name="applyTo">The apply to.</param>
        public AuthenticateAttribute(ApplyTo applyTo)
            : base(applyTo)
        {
            this.Priority = (int) RequestFilterPriority.Authenticate;
        }

        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.AuthenticateAttribute class.</summary>
        public AuthenticateAttribute()
            : this(ApplyTo.All) {}

        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.AuthenticateAttribute class.</summary>
        ///
        /// <param name="provider">Restrict authentication to a specific <see cref="IAuthProvider"/>. For example, if this attribute should only permit access if the user is authenticated with
        /// <see cref="BasicAuthProvider"/>, you should set this property to <see cref="BasicAuthProvider.Name"/>.
        /// </param>
        public AuthenticateAttribute(string provider)
            : this(ApplyTo.All)
        {
            this.Provider = provider;
        }

        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.AuthenticateAttribute class.</summary>
        ///
        /// <param name="applyTo"> The apply to.</param>
        /// <param name="provider">Restrict authentication to a specific <see cref="IAuthProvider"/>. For example, if this attribute should only permit access if the user is authenticated with
        /// <see cref="BasicAuthProvider"/>, you should set this property to <see cref="BasicAuthProvider.Name"/>.
        /// </param>
        public AuthenticateAttribute(ApplyTo applyTo, string provider)
            : this(applyTo)
        {
            this.Provider = provider;
        }

        /// <summary>This method is only executed if the HTTP method matches the <see cref="ApplyTo"/> property.</summary>
        ///
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        ///
        /// <param name="req">       The http request wrapper.</param>
        /// <param name="res">       The http response wrapper.</param>
        /// <param name="requestDto">The request DTO.</param>
        public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            if (AuthService.AuthProviders == null) 
                throw new InvalidOperationException(
                    "The AuthService must be initialized by calling AuthService.Init to use an authenticate attribute");

            var matchingOAuthConfigs = AuthService.AuthProviders.Where(x =>
                this.Provider.IsNullOrEmpty()
                || x.Provider == this.Provider).ToList();

            if (matchingOAuthConfigs.Count == 0)
            {
                res.WriteError(req, requestDto, "No OAuth Configs found matching {0} provider"
                    .Fmt(this.Provider ?? "any"));
                res.EndRequest();
                return;
            }

            if (matchingOAuthConfigs.Any(x => x.Provider == DigestAuthProvider.Name))
                AuthenticateIfDigestAuth(req, res);

            if (matchingOAuthConfigs.Any(x => x.Provider == BasicAuthProvider.Name))
                AuthenticateIfBasicAuth(req, res);

            var session = req.GetSession();
            if (session == null || !matchingOAuthConfigs.Any(x => session.IsAuthorized(x.Provider)))
            {
                if (this.DoHtmlRedirectIfConfigured(req, res, true)) return;

                AuthProvider.HandleFailedAuth(matchingOAuthConfigs[0], session, req, res);
            }
        }

        /// <summary>Executes the HTML redirect if configured operation.</summary>
        ///
        /// <param name="req">                 The request.</param>
        /// <param name="res">                 The resource.</param>
        /// <param name="includeRedirectParam">true to include, false to exclude the redirect parameter.</param>
        ///
        /// <returns>true if it succeeds, false if it fails.</returns>
        protected bool DoHtmlRedirectIfConfigured(IHttpRequest req, IHttpResponse res, bool includeRedirectParam = false)
        {
            var htmlRedirect = this.HtmlRedirect ?? AuthService.HtmlRedirect;
            if (htmlRedirect != null && req.ResponseContentType.MatchesContentType(ContentType.Html))
            {
                var url = req.ResolveAbsoluteUrl(htmlRedirect);
                if (includeRedirectParam)
                {
                    var absoluteRequestPath = req.ResolveAbsoluteUrl("~" + req.PathInfo + ToQueryString(req.QueryString));
                    url = url.AddQueryParam("redirect", absoluteRequestPath);
                }

                res.RedirectToUrl(url);
                return true;
            }

            return false;
        }

        /// <summary>Authenticate if basic authentication.</summary>
        ///
        /// <param name="req">The request.</param>
        /// <param name="res">The resource.</param>
        public static void AuthenticateIfBasicAuth(IHttpRequest req, IHttpResponse res)
        {
            //Need to run SessionFeature filter since its not executed before this attribute (Priority -100)			
            SessionFeature.AddSessionIdToRequestFilter(req, res, null); //Required to get req.GetSessionId()

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

        /// <summary>Authenticate if digest authentication.</summary>
        ///
        /// <param name="req">The request.</param>
        /// <param name="res">The resource.</param>
        public static void AuthenticateIfDigestAuth(IHttpRequest req, IHttpResponse res)
        {
            //Need to run SessionFeature filter since its not executed before this attribute (Priority -100)			
            SessionFeature.AddSessionIdToRequestFilter(req, res, null); //Required to get req.GetSessionId()

            var digestAuth = req.GetDigestAuth();
            if (digestAuth != null)
            {
                var authService = req.TryResolve<AuthService>();
                authService.RequestContext = new HttpRequestContext(req, res, null);
                var response = authService.Post(new Auth.Auth
                {
                    provider = DigestAuthProvider.Name,
                    nonce = digestAuth["nonce"],
                    uri = digestAuth["uri"],
                    response = digestAuth["response"],
                    qop = digestAuth["qop"],
                    nc = digestAuth["nc"],
                    cnonce = digestAuth["cnonce"],
                    UserName = digestAuth["username"]
                });
            }
        }

        // Not sure if this should be made re-useable within SS.Text with the other NVC extension methods?
        private static string ToQueryString(NameValueCollection queryStringCollection)
        {
            if (queryStringCollection == null || queryStringCollection.Count == 0)
                return String.Empty;

            return "?" + queryStringCollection.ToFormUrlEncoded();
        }
    }
}
