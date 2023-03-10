using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using NServiceKit.Common;
using NServiceKit.Common.Web;
using NServiceKit.Configuration;
using NServiceKit.ServiceHost;
using NServiceKit.ServiceInterface;
using NServiceKit.ServiceInterface.Auth;
using NServiceKit.Text;

namespace NServiceKit.Authentication.OpenId
{
    /// <summary>An open identifier o authentication provider.</summary>
    public class OpenIdOAuthProvider : OAuthProvider
    {
        /// <summary>The default name.</summary>
        public const string DefaultName = "OpenId";

        /// <summary>Gets or sets the open identifier application store.</summary>
        ///
        /// <value>The open identifier application store.</value>
        public static IOpenIdApplicationStore OpenIdApplicationStore { get; set; }

        /// <summary>Initializes a new instance of the NServiceKit.Authentication.OpenId.OpenIdOAuthProvider class.</summary>
        ///
        /// <param name="appSettings">The application settings.</param>
        /// <param name="name">       The name.</param>
        /// <param name="realm">      The realm.</param>
        public OpenIdOAuthProvider(IResourceManager appSettings, string name = DefaultName, string realm = null)
            : base(appSettings, realm, name) { }

        /// <summary>Creates claims request.</summary>
        ///
        /// <param name="httpReq">The HTTP request.</param>
        ///
        /// <returns>The new claims request.</returns>
        public virtual ClaimsRequest CreateClaimsRequest(IHttpRequest httpReq)
        {
            return new ClaimsRequest {
                Country = DemandLevel.Request,
                Email = DemandLevel.Request,
                Gender = DemandLevel.Require,
                PostalCode = DemandLevel.Require,
                TimeZone = DemandLevel.Require,
            };
        }

        /// <summary>Creates open identifier relying party.</summary>
        ///
        /// <param name="store">The store.</param>
        ///
        /// <returns>The new open identifier relying party.</returns>
        protected virtual OpenIdRelyingParty CreateOpenIdRelyingParty(IOpenIdApplicationStore store)
        {
            // it matters
            return store != null
                ? new OpenIdRelyingParty(store)
                : new OpenIdRelyingParty();
        }

        /// <summary>The entry point for all AuthProvider providers. Runs inside the AuthService so exceptions are treated normally. Overridable so you can provide your own Auth implementation.</summary>
        ///
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or illegal values.</exception>
        ///
        /// <param name="authService">.</param>
        /// <param name="session">    .</param>
        /// <param name="request">    .</param>
        ///
        /// <returns>An object.</returns>
        public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            var tokens = Init(authService, ref session, request);

            var httpReq = authService.RequestContext.Get<IHttpRequest>();
            var isOpenIdRequest = !httpReq.GetParam("openid.mode").IsNullOrEmpty();

            if (!isOpenIdRequest)
            {
                var openIdUrl = httpReq.GetParam("OpenIdUrl") ?? base.AuthRealm;
                if (openIdUrl.IsNullOrEmpty())
                    throw new ArgumentException("'OpenIdUrl' is required a required field");

                try
                {
                    using (var openid = CreateOpenIdRelyingParty(OpenIdApplicationStore))
                    {
                        var openIdRequest = openid.CreateRequest(openIdUrl);

                        AddAttributeExchangeExtensions(openIdRequest);

                        // This is where you would add any OpenID extensions you wanted
                        // to include in the authentication request.
                        openIdRequest.AddExtension(CreateClaimsRequest(httpReq));

                        // Send your visitor to their Provider for authentication.
                        var openIdResponse = openIdRequest.RedirectingResponse;
                        var contentType = openIdResponse.Headers[HttpHeaders.ContentType];
                        var httpResult = new HttpResult(openIdResponse.ResponseStream, contentType) {
                            StatusCode = openIdResponse.Status,
                            StatusDescription = "Moved Temporarily",
                        };
                        foreach (string header in openIdResponse.Headers)
                        {
                            httpResult.Headers[header] = openIdResponse.Headers[header];
                        }
                        // Save the current session to keep the ReferrerUrl available (similar to Facebook provider)
                        authService.SaveSession(session, SessionExpiry);
                        return httpResult;
                    }
                }
                catch (ProtocolException ex)
                {
                    Log.Error("Failed to login to {0}".Fmt(openIdUrl), ex);
                    return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "Unknown"));
                }
            }

            if (isOpenIdRequest)
            {
                using (var openid = CreateOpenIdRelyingParty(OpenIdApplicationStore))
                {
                    var response = openid.GetResponse();
                    if (response != null)
                    {
                        switch (response.Status)
                        {
                            case AuthenticationStatus.Authenticated:

                                var authInfo = CreateAuthInfo(response);

                                // Use FormsAuthentication to tell ASP.NET that the user is now logged in,
                                // with the OpenID Claimed Identifier as their username.
                                session.IsAuthenticated = true;
                                authService.SaveSession(session, SessionExpiry);
                                OnAuthenticated(authService, session, tokens, authInfo);

                                //Haz access!
                                return authService.Redirect(session.ReferrerUrl.AddHashParam("s", "1"));

                            case AuthenticationStatus.Canceled:
                                return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "ProviderCancelled"));

                            case AuthenticationStatus.Failed:
                                return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "Unknown"));
                        }
                    }
                }
            }

            //Shouldn't get here
            return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "Unknown"));
        }

        /// <summary>Creates authentication information.</summary>
        ///
        /// <param name="response">The response.</param>
        ///
        /// <returns>The new authentication information.</returns>
        protected virtual Dictionary<string, string> CreateAuthInfo(IAuthenticationResponse response)
        {
            // This is where you would look for any OpenID extension responses included
            // in the authentication assertion.
            var claimsResponse = response.GetExtension<ClaimsResponse>();
            var authInfo = claimsResponse.ToDictionary();

            authInfo["user_id"] = response.ClaimedIdentifier; //a url

            // Store off the "friendly" username to display -- NOT for username lookup
            authInfo["openid_ref"] = response.FriendlyIdentifierForDisplay;

            var provided = GetAttributeEx(response);
            foreach (var entry in provided)
            {
                authInfo[entry.Key] = entry.Value;
            }

            return authInfo;
        }

        /// <summary>Loads user authentication information.</summary>
        ///
        /// <param name="userSession">The user session.</param>
        /// <param name="tokens">     The tokens.</param>
        /// <param name="authInfo">   Information describing the authentication.</param>
        protected override void LoadUserAuthInfo(AuthUserSession userSession, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            if (authInfo.ContainsKey("user_id"))
                tokens.UserId = authInfo.GetValueOrDefault("user_id");

            if (authInfo.ContainsKey("name"))
                tokens.DisplayName = authInfo.GetValueOrDefault("name");

            if (authInfo.ContainsKey("FullName"))
            {
                tokens.FullName = authInfo.GetValueOrDefault("FullName");
                if (tokens.DisplayName.IsNullOrEmpty())
                    tokens.DisplayName = tokens.FullName;
            }

            if (authInfo.ContainsKey("Email"))
                tokens.Email = authInfo.GetValueOrDefault("Email");

            if (authInfo.ContainsKey("BirthDate"))
                tokens.BirthDate = authInfo.GetValueOrDefault("BirthDate").FromJsv<DateTime?>();

            if (authInfo.ContainsKey("BirthDateRaw"))
                tokens.BirthDateRaw = authInfo.GetValueOrDefault("BirthDateRaw");

            if (authInfo.ContainsKey("Country"))
                tokens.Country = authInfo.GetValueOrDefault("Country");

            if (authInfo.ContainsKey("Culture"))
                tokens.Culture = authInfo.GetValueOrDefault("Culture");

            if (authInfo.ContainsKey("Gender"))
                tokens.Gender = authInfo.GetValueOrDefault("Gender");

            if (authInfo.ContainsKey("MailAddress"))
                tokens.MailAddress = authInfo.GetValueOrDefault("MailAddress");

            if (authInfo.ContainsKey("Nickname"))
                tokens.Nickname = authInfo.GetValueOrDefault("Nickname");

            if (authInfo.ContainsKey("PostalCode"))
                tokens.PostalCode = authInfo.GetValueOrDefault("PostalCode");

            if (authInfo.ContainsKey("TimeZone"))
                tokens.TimeZone = authInfo.GetValueOrDefault("TimeZone");

            LoadUserOAuthProvider(userSession, tokens);
        }

        /// <summary>Loads user o authentication provider.</summary>
        ///
        /// <param name="authSession">The user session.</param>
        /// <param name="tokens">     The tokens.</param>
        public override void LoadUserOAuthProvider(IAuthSession authSession, IOAuthTokens tokens)
        {
            var userSession = authSession as AuthUserSession;
            if (userSession == null) return;
        }

        private void AddAttributeExchangeExtensions(IAuthenticationRequest auth)
        {
            // Try to use OpenId 2.0's attribute exchange
            var fetch = new FetchRequest();
            //Technically, http://axschema.org/... are "standard", but we'll still find these in the wild
            fetch.Attributes.Add(new AttributeRequest("http://schema.openid.net/namePerson", false));
            fetch.Attributes.Add(new AttributeRequest("http://schema.openid.net/contact/email", false));

            fetch.Attributes.AddRequired("http://axschema.org/contact/country/home");
            fetch.Attributes.AddRequired("http://axschema.org/namePerson/first");
            fetch.Attributes.AddRequired("http://axschema.org/namePerson/last");
            fetch.Attributes.AddRequired("http://axschema.org/pref/language");
            fetch.Attributes.AddRequired("http://schemas.openid.net/ax/api/user_id");

            //Standard compliant AX schema
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.FullName, false));

            //For... no good reason, really, google OpenId requires you "require" an e-mail address to get it
            bool requireEmail = auth.Provider.Uri.AbsoluteUri.Contains(".google.com");
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, requireEmail));

            auth.AddExtension(fetch);
        }

        /// <summary>
        /// Extracts an Attribute Exchange response, if one exists
        /// </summary>
        private Dictionary<string, string> GetAttributeEx(IAuthenticationResponse response)
        {
            var ret = new Dictionary<string, string>();

            var fetchResponse = response.GetExtension<FetchResponse>();

            if (fetchResponse == null) return ret;

	        string fullName = null;
            var names = new List<string>();
            var emails = new List<string>();

            if (fetchResponse.Attributes.Contains("http://schema.openid.net/namePerson"))
                fullName = fetchResponse.Attributes["http://schema.openid.net/namePerson"].Values.FirstOrDefault();

            if (fullName == null && fetchResponse.Attributes.Contains(WellKnownAttributes.Name.FullName))
                fullName = fetchResponse.Attributes[WellKnownAttributes.Name.FullName].Values.FirstOrDefault();

            if (fullName == null && fetchResponse.Attributes.Contains(WellKnownAttributes.Name.Alias))
                fullName = fetchResponse.Attributes[WellKnownAttributes.Name.Alias].Values.FirstOrDefault();

            if (fetchResponse.Attributes.Contains(WellKnownAttributes.Name.First))
                names.AddRange(fetchResponse.Attributes[WellKnownAttributes.Name.First].Values);

            if (fetchResponse.Attributes.Contains(WellKnownAttributes.Name.Last))
                names.AddRange(fetchResponse.Attributes[WellKnownAttributes.Name.Last].Values);

            if (fetchResponse.Attributes.Contains("http://schema.openid.net/contact/email"))
                emails.AddRange(fetchResponse.Attributes["http://schema.openid.net/contact/email"].Values);

            if (fetchResponse.Attributes.Contains(WellKnownAttributes.Contact.Email))
                emails.AddRange(fetchResponse.Attributes[WellKnownAttributes.Contact.Email].Values);

			if (fullName == null && names.Count > 0) 
				fullName = string.Join(" ", names.ToArray());

			if (fullName != null)
                ret["FullName"] = fullName;

            if (emails.Count > 0)
                ret["Email"] = emails[0];

            return ret;
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

            // For OpenId, AccessTokenSecret is null/empty, but UserId is populated w/ authenticated url from openId providers            
            return tokens != null && !string.IsNullOrEmpty(tokens.UserId);
        }
    }


    /// <summary>An open identifier extensions.</summary>
    public static class OpenIdExtensions
    {
        /// <summary>A ClaimsResponse extension method that converts a response to a dictionary.</summary>
        ///
        /// <param name="response">The response to act on.</param>
        ///
        /// <returns>response as a Dictionary&lt;string,string&gt;</returns>
        public static Dictionary<string, string> ToDictionary(this ClaimsResponse response)
        {
            var map = new Dictionary<string, string>();
            if (response == null) return map;

            if (response.BirthDate.HasValue)
                map["BirthDate"] = response.BirthDate.Value.ToJsv();
            if (!response.BirthDateRaw.IsNullOrEmpty())
                map["BirthDateRaw"] = response.BirthDateRaw;
            if (!response.Country.IsNullOrEmpty())
                map["Country"] = response.Country;

            try
            {
                if (response.Culture != null)
                    map["Culture"] = response.Culture.TwoLetterISOLanguageName;
            }
            catch (Exception) //CultureNotFoundException (.NET 4.5)
            {
                map["Culture"] = "en";
            }

            if (!response.Email.IsNullOrEmpty())
                map["Email"] = response.Email;
            if (!response.FullName.IsNullOrEmpty())
                map["FullName"] = response.FullName;
            if (response.Gender.HasValue)
                map["Gender"] = response.Gender.Value.ToString();
            if (!response.Language.IsNullOrEmpty())
                map["Language"] = response.Language;
            if (response.MailAddress != null)
                map["MailAddress"] = response.MailAddress.ToJsv();
            if (!response.Nickname.IsNullOrEmpty())
                map["Nickname"] = response.Nickname;
            if (!response.PostalCode.IsNullOrEmpty())
                map["PostalCode"] = response.PostalCode;
            if (!response.TimeZone.IsNullOrEmpty())
                map["TimeZone"] = response.TimeZone;

            return map;
        }
    }
}
