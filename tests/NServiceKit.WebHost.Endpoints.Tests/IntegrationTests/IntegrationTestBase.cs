using System;
using Funq;
using NServiceKit.Configuration;
using NServiceKit.Logging;
using NServiceKit.Logging.Support.Logging;
using NServiceKit.OrmLite;
using NServiceKit.OrmLite.Sqlite;
using NServiceKit.ServiceClient.Web;
using NServiceKit.WebHost.Endpoints.Tests.Support.Host;

namespace NServiceKit.WebHost.Endpoints.Tests.IntegrationTests
{
    /// <summary>An integration test base.</summary>
	public class IntegrationTestBase
		: AppHostHttpListenerBase
	{
        /// <summary>URL of the base.</summary>
		protected const string BaseUrl = "http://localhost:82/";

		//Fiddler can debug local HTTP requests when using the hostname
		//private const string BaseUrl = "http://io:8081/";

		//private static ILog log;

        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.Tests.IntegrationTests.IntegrationTestBase class.</summary>
		public IntegrationTestBase()
			: base("NServiceKit Examples", typeof(RestMovieService).Assembly)
		{
			LogManager.LogFactory = new DebugLogFactory();
			//log = LogManager.GetLogger(GetType());
			Instance = null;

			Init();
			try
			{
				Start(BaseUrl);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error trying to run ConsoleHost: " + ex.Message);
			}
		}

        /// <summary>Configures the given container.</summary>
        ///
        /// <param name="container">The container.</param>
		public override void Configure(Container container)
		{
			container.Register<IResourceManager>(new ConfigurationResourceManager());

			container.Register(c => new ExampleConfig(c.Resolve<IResourceManager>()));
			//var appConfig = container.Resolve<ExampleConfig>();

			container.Register<IDbConnectionFactory>(c =>
				 new OrmLiteConnectionFactory(
					":memory:",			//Use an in-memory database instead
					false,				//keep the same in-memory db connection open
					SqliteOrmLiteDialectProvider.Instance));

			Routes.Add<Movies>("/custom-movies", "GET")
				  .Add<Movies>("/custom-movies/genres/{Genre}")
				  .Add<Movie>("/custom-movies", "POST,PUT")
				  .Add<Movie>("/custom-movies/{Id}");

			ConfigureDatabase.Init(container.Resolve<IDbConnectionFactory>());
		}

        /// <summary>Run the request against each Endpoint.</summary>
        ///
        /// <typeparam name="TRes">Type of the resource.</typeparam>
        /// <param name="request"> .</param>
        /// <param name="validate">.</param>
		public void SendToEachEndpoint<TRes>(object request, Action<TRes> validate)
		{
			SendToEachEndpoint(request, null, validate);
		}

		/// <summary>
		/// Run the request against each Endpoint
		/// </summary>
		/// <typeparam name="TRes"></typeparam>
		/// <param name="request"></param>
		/// <param name="validate"></param>
		/// <param name="httpMethod"></param>
		public void SendToEachEndpoint<TRes>(object request, string httpMethod, Action<TRes> validate)
		{
			using (var xmlClient = new XmlServiceClient(BaseUrl))
			using (var jsonClient = new JsonServiceClient(BaseUrl))
			using (var jsvClient = new JsvServiceClient(BaseUrl))
			{
				xmlClient.HttpMethod = httpMethod;
				jsonClient.HttpMethod = httpMethod;
				jsvClient.HttpMethod = httpMethod;

				var xmlResponse = xmlClient.Send<TRes>(request);
				if (validate != null) validate(xmlResponse);

				var jsonResponse = jsonClient.Send<TRes>(request);
				if (validate != null) validate(jsonResponse);

				var jsvResponse = jsvClient.Send<TRes>(request);
				if (validate != null) validate(jsvResponse);
			}
		}

        /// <summary>Deletes the on each endpoint.</summary>
        ///
        /// <typeparam name="TRes">Type of the resource.</typeparam>
        /// <param name="relativePathOrAbsoluteUri">URI of the relative path or absolute.</param>
        /// <param name="validate">                 .</param>
		public void DeleteOnEachEndpoint<TRes>(string relativePathOrAbsoluteUri, Action<TRes> validate)
		{
			using (var xmlClient = new XmlServiceClient(BaseUrl))
			using (var jsonClient = new JsonServiceClient(BaseUrl))
			using (var jsvClient = new JsvServiceClient(BaseUrl))
			{
				var xmlResponse = xmlClient.Delete<TRes>(relativePathOrAbsoluteUri);
				if (validate != null) validate(xmlResponse);

				var jsonResponse = jsonClient.Delete<TRes>(relativePathOrAbsoluteUri);
				if (validate != null) validate(jsonResponse);

				var jsvResponse = jsvClient.Delete<TRes>(relativePathOrAbsoluteUri);
				if (validate != null) validate(jsvResponse);
			}
		}
	}
}