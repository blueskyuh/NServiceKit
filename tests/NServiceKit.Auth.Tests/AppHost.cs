#define HTTP_LISTENER
using Funq;
using NServiceKit.Authentication.OAuth2;
using NServiceKit.Authentication.OpenId;
using NServiceKit.CacheAccess;
using NServiceKit.CacheAccess.Providers;
using NServiceKit.Configuration;
using NServiceKit.FluentValidation;
using NServiceKit.MiniProfiler;
using NServiceKit.MiniProfiler.Data;
using NServiceKit.OrmLite;
using NServiceKit.Razor;
using NServiceKit.ServiceInterface;
using NServiceKit.ServiceInterface.Admin;
using NServiceKit.ServiceInterface.Auth;
using NServiceKit.Text;
using NServiceKit.WebHost.Endpoints;

#if HTTP_LISTENER
namespace NServiceKit.Auth.Tests
#else
namespace NServiceKit.AuthWeb.Tests
#endif
{
#if HTTP_LISTENER
    /// <summary>An application host.</summary>
    public class AppHost : AppHostHttpListenerBase
#else
    public class AppHost : AppHostBase
#endif
    {
        /// <summary>Initializes a new instance of the NServiceKit.Auth.Tests.AppHost class.</summary>
        public AppHost()
            : base("Test Auth", typeof(AppHost).Assembly) { }

        /// <summary>Configures the given container.</summary>
        ///
        /// <param name="container">The container.</param>
        public override void Configure(Container container)
        {
            Plugins.Add(new RazorFormat());

            container.Register(new DataSource());

            container.Register<IDbConnectionFactory>(
                new OrmLiteConnectionFactory(":memory:", false, //ConnectionString in Web.Config
                    SqliteDialect.Provider)
                    {
                        ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current)
                    });

            using (var db = container.Resolve<IDbConnectionFactory>().Open())
            {
                db.CreateTableIfNotExists<Rockstar>();
                db.Insert(Rockstar.SeedData);
            }

            JsConfig.EmitCamelCaseNames = true;

            //Register Typed Config some services might need to access
            var appSettings = new AppSettings();

            //Register a external dependency-free 
            container.Register<ICacheClient>(new MemoryCacheClient());
            //Configure an alt. distributed persistent cache that survives AppDomain restarts. e.g Redis
            //container.Register<IRedisClientsManager>(c => new PooledRedisClientManager("localhost:6379"));

            //Enable Authentication an Registration
            ConfigureAuth(container);

            //Create your own custom User table
            var dbFactory = container.Resolve<IDbConnectionFactory>();
            dbFactory.Run(db => db.DropAndCreateTable<UserTable>());
        }

        private void ConfigureAuth(Container container)
        {
            //Enable and register existing services you want this host to make use of.
            //Look in Web.config for examples on how to configure your oauth providers, e.g. oauth.facebook.AppId, etc.
            var appSettings = new AppSettings();

            //Register all Authentication methods you want to enable for this web app.            
            Plugins.Add(new AuthFeature(
                () => new CustomUserSession(), //Use your own typed Custom UserSession type
                new IAuthProvider[] {
                    new CredentialsAuthProvider(),              //HTML Form post of UserName/Password credentials
                    new TwitterAuthProvider(appSettings),       //Sign-in with Twitter
                    new FacebookAuthProvider(appSettings),      //Sign-in with Facebook
                    new DigestAuthProvider(appSettings),        //Sign-in with Digest Auth
                    new BasicAuthProvider(),                    //Sign-in with Basic Auth
                    new GoogleOpenIdOAuthProvider(appSettings), //Sign-in with Google OpenId
                    new YahooOpenIdOAuthProvider(appSettings),  //Sign-in with Yahoo OpenId
                    new OpenIdOAuthProvider(appSettings),       //Sign-in with Custom OpenId
                    new GoogleOAuth2Provider(appSettings),      //Sign-in with Google OAuth2 Provider
                    new LinkedInOAuth2Provider(appSettings),    //Sign-in with LinkedIn OAuth2 Provider
                }));

#if HTTP_LISTENER
            //Required for DotNetOpenAuth in HttpListener 
            OpenIdOAuthProvider.OpenIdApplicationStore = new InMemoryOpenIdApplicationStore();
#endif

            //Provide service for new users to register so they can login with supplied credentials.
            Plugins.Add(new RegistrationFeature());

            //override the default registration validation with your own custom implementation
            container.RegisterAs<CustomRegistrationValidator, IValidator<Registration>>();

            //Store User Data into the referenced SqlServer database
            container.Register<IUserAuthRepository>(c =>
                new OrmLiteAuthRepository(c.Resolve<IDbConnectionFactory>())); //Use OrmLite DB Connection to persist the UserAuth and AuthProvider info

            var authRepo = (OrmLiteAuthRepository)container.Resolve<IUserAuthRepository>(); //If using and RDBMS to persist UserAuth, we must create required tables
            if (appSettings.Get("RecreateAuthTables", false))
                authRepo.DropAndReCreateTables(); //Drop and re-create all Auth and registration tables
            else
                authRepo.CreateMissingTables();   //Create only the missing tables

            Plugins.Add(new RequestLogsFeature());
        }
    }

    //Provide extra validation for the registration process
    public class CustomRegistrationValidator : RegistrationValidator
    {
        /// <summary>Initializes a new instance of the NServiceKit.Auth.Tests.CustomRegistrationValidator class.</summary>
        public CustomRegistrationValidator()
        {
            RuleSet(ApplyTo.Post, () =>
            {
                RuleFor(x => x.DisplayName).NotEmpty();
            });
        }
    }

    /// <summary>A custom user session.</summary>
    public class CustomUserSession : AuthUserSession
    {
        /// <summary>Executes the authenticated action.</summary>
        ///
        /// <param name="authService">The authentication service.</param>
        /// <param name="session">    The session.</param>
        /// <param name="tokens">     The tokens.</param>
        /// <param name="authInfo">   Information describing the authentication.</param>
        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, System.Collections.Generic.Dictionary<string, string> authInfo)
        {
            "OnAuthenticated()".Print();
        }
    }

    /// <summary>A data source.</summary>
    public class DataSource
    {
        /// <summary>The items.</summary>
        public string[] Items = new[] { "Eeny", "meeny", "miny", "moe" };
    }

    /// <summary>A user table.</summary>
    public class UserTable
    {
        /// <summary>Gets or sets the identifier.</summary>
        ///
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>Gets or sets the custom field.</summary>
        ///
        /// <value>The custom field.</value>
        public string CustomField { get; set; }
    }
}
