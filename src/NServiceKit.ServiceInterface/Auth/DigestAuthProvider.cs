using System.Globalization;
using System.Collections.Generic;
using NServiceKit.Common;
using NServiceKit.Common.Web;
using NServiceKit.ServiceHost;
using NServiceKit.Configuration;
using NServiceKit.FluentValidation;
using NServiceKit.Text;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using NServiceKit.WebHost.Endpoints.Extensions;
using HttpResponseExtensions = NServiceKit.WebHost.Endpoints.Extensions.HttpResponseExtensions;

namespace NServiceKit.ServiceInterface.Auth
{
    /// <summary>A digest authentication provider.</summary>
    public class DigestAuthProvider : AuthProvider
    {
        class DigestAuthValidator : AbstractValidator<Auth>
        {
            /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.Auth.DigestAuthProvider.DigestAuthValidator class.</summary>
            public DigestAuthValidator()
            {
                RuleFor(x => x.UserName).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }

        /// <summary>The name.</summary>
        public static string Name = AuthService.DigestProvider;
        /// <summary>The realm.</summary>
        public static string Realm = "/auth/" + AuthService.DigestProvider;
        /// <summary>The nonce time out.</summary>
        public static int NonceTimeOut = 600;
        /// <summary>The private key.</summary>
        public string PrivateKey;

        /// <summary>Gets or sets the application settings.</summary>
        ///
        /// <value>The application settings.</value>
        public IResourceManager AppSettings { get; set; }
        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.Auth.DigestAuthProvider class.</summary>
        public DigestAuthProvider()
        {
            this.Provider = Name;
            PrivateKey = Guid.NewGuid().ToString();
            this.AuthRealm = Realm;
        }

        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.Auth.DigestAuthProvider class.</summary>
        ///
        /// <param name="appSettings">  The application settings.</param>
        /// <param name="authRealm">    The authentication realm.</param>
        /// <param name="oAuthProvider">The authentication provider.</param>
        public DigestAuthProvider(IResourceManager appSettings, string authRealm, string oAuthProvider)
            : base(appSettings, authRealm, oAuthProvider) { }

        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.Auth.DigestAuthProvider class.</summary>
        ///
        /// <param name="appSettings">The application settings.</param>
        public DigestAuthProvider(IResourceManager appSettings)
            : base(appSettings, Realm, Name) { }

        /// <summary>Attempts to authenticate from the given data.</summary>
        ///
        /// <param name="authService">The authentication service.</param>
        /// <param name="userName">   Name of the user.</param>
        /// <param name="password">   The password.</param>
        ///
        /// <returns>true if it succeeds, false if it fails.</returns>
        public virtual bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            var authRepo = authService.TryResolve<IUserAuthRepository>();
            if (authRepo == null)
            {
                Log.WarnFormat("Tried to authenticate without a registered IUserAuthRepository");
                return false;
            }

            var session = authService.GetSession();
            var digestInfo = authService.RequestContext.Get<IHttpRequest>().GetDigestAuth();
            UserAuth userAuth = null;
            if (authRepo.TryAuthenticate(digestInfo,PrivateKey,NonceTimeOut, session.Sequence, out userAuth))
            {
                session.PopulateWith(userAuth);
                session.IsAuthenticated = true;
                session.Sequence = digestInfo["nc"];
                session.UserAuthId = userAuth.Id.ToString(CultureInfo.InvariantCulture);
                session.ProviderOAuthAccess = authRepo.GetUserOAuthProviders(session.UserAuthId)
                .ConvertAll(x => (IOAuthTokens)x);
                
                return true;
            }
            return false;
        }

        /// <summary>Determine if the current session is already authenticated with this AuthProvider.</summary>
        ///
        /// <param name="session">The session.</param>
        /// <param name="tokens"> The tokens.</param>
        /// <param name="request">The request.</param>
        ///
        /// <returns>true if authorized, false if not.</returns>
        public override bool IsAuthorized(IAuthSession session, IOAuthTokens tokens, Auth request = null)
        {
            if (request != null)
            {
                if (!LoginMatchesSession(session, request.UserName)) return false;
            }

            return !session.UserAuthName.IsNullOrEmpty();
        }

        /// <summary>The entry point for all AuthProvider providers. Runs inside the AuthService so exceptions are treated normally. Overridable so you can provide your own Auth implementation.</summary>
        ///
        /// <param name="authService">The authentication service.</param>
        /// <param name="session">    The session.</param>
        /// <param name="request">    The request.</param>
        ///
        /// <returns>An object.</returns>
        public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            //new CredentialsAuthValidator().ValidateAndThrow(request);
            return Authenticate(authService, session, request.UserName, request.Password);
        }

        /// <summary>Authenticates.</summary>
        ///
        /// <exception cref="Unauthorized">Thrown when an unauthorized error condition occurs.</exception>
        ///
        /// <param name="authService">The authentication service.</param>
        /// <param name="session">    The session.</param>
        /// <param name="userName">   Name of the user.</param>
        /// <param name="password">   The password.</param>
        ///
        /// <returns>An object.</returns>
        protected object Authenticate(IServiceBase authService, IAuthSession session, string userName, string password)
        {
            if (!LoginMatchesSession(session, userName))
            {
                authService.RemoveSession();
                session = authService.GetSession();
            }

            if (TryAuthenticate(authService, userName, password))
            {
                if (session.UserAuthName == null)
                    session.UserAuthName = userName;

                OnAuthenticated(authService, session, null, null);

                return new AuthResponse
                {
                    UserName = userName,
                    SessionId = session.Id,
                };
            }

            throw HttpError.Unauthorized("Invalid UserName or Password");
        }

        /// <summary>Executes the authenticated action.</summary>
        ///
        /// <param name="authService">The authentication service.</param>
        /// <param name="session">    The session.</param>
        /// <param name="tokens">     The tokens.</param>
        /// <param name="authInfo">   Information describing the authentication.</param>
        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            var userSession = session as AuthUserSession;
            if (userSession != null)
            {
                LoadUserAuthInfo(userSession, tokens, authInfo);
            }

            var authRepo = authService.TryResolve<IUserAuthRepository>();
            if (authRepo != null)
            {
                if (tokens != null)
                {
                    authInfo.ForEach((x, y) => tokens.Items[x] = y);
                    session.UserAuthId = authRepo.CreateOrMergeAuthSession(session, tokens);
                }

                foreach (var oAuthToken in session.ProviderOAuthAccess)
                {
                    var authProvider = AuthService.GetAuthProvider(oAuthToken.Provider);
                    if (authProvider == null) continue;
                    var userAuthProvider = authProvider as OAuthProvider;
                    if (userAuthProvider != null)
                    {
                        userAuthProvider.LoadUserOAuthProvider(session, oAuthToken);
                    }
                }

                //var httpRes = authService.RequestContext.Get<IHttpResponse>();
                //if (httpRes != null)
                //{
                //    httpRes.Cookies.AddPermanentCookie(HttpHeaders.XUserAuthId, session.UserAuthId);
                //}

            }

            authService.SaveSession(session, SessionExpiry);
            session.OnAuthenticated(authService, session, tokens, authInfo);
        }

        /// <summary>Executes the failed authentication action.</summary>
        ///
        /// <param name="session">The session.</param>
        /// <param name="httpReq">The HTTP request.</param>
        /// <param name="httpRes">The HTTP resource.</param>
        public override void OnFailedAuthentication(IAuthSession session, IHttpRequest httpReq, IHttpResponse httpRes)
        {
            var digestHelper = new DigestAuthFunctions();
            httpRes.StatusCode = (int)HttpStatusCode.Unauthorized;
            httpRes.AddHeader(HttpHeaders.WwwAuthenticate, "{0} realm=\"{1}\", nonce=\"{2}\", qop=\"auth\"".Fmt(Provider, AuthRealm,digestHelper.GetNonce(httpReq.UserHostAddress,PrivateKey)));
            httpRes.EndRequest();
        }
    }
}
