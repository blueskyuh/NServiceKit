using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using Funq;
using ServiceStack.Common.Extensions;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints.Tests.IntegrationTests;

namespace ServiceStack.WebHost.Endpoints.Tests.Support.Host
{

	[RestService("/factorial/{ForNumber}")]
	[DataContract]
	public class GetFactorial
	{
		[DataMember]
		public long ForNumber { get; set; }
	}

	[DataContract]
	public class GetFactorialResponse
	{
		[DataMember]
		public long Result { get; set; }
	}

	public class GetFactorialService
		: IService<GetFactorial>
	{
		public object Execute(GetFactorial request)
		{
			return new GetFactorialResponse { Result = GetFactorial(request.ForNumber) };
		}

		public static long GetFactorial(long n)
		{
			return n > 1 ? n * GetFactorial(n - 1) : 1;
		}
	}

	[DataContract]
	public class AlwaysThrows { }

	[DataContract]
	public class AlwaysThrowsResponse : IHasResponseStatus
	{
		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}

	public class AlwaysThrowsService
		: ServiceBase<AlwaysThrows>
	{
		protected override object Run(AlwaysThrows request)
		{
			throw new ArgumentException("This service always throws an error");
		}
	}


	[RestService("/movies", "POST,PUT")]
	[RestService("/movies/{Id}")]
	[DataContract]
	public class Movie
	{
		public Movie()
		{
			this.Genres = new List<string>();
		}

		[DataMember]
		[AutoIncrement]
		public int Id { get; set; }

		[DataMember]
		public string ImdbId { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public decimal Rating { get; set; }

		[DataMember]
		public string Director { get; set; }

		[DataMember]
		public DateTime ReleaseDate { get; set; }

		[DataMember]
		public string TagLine { get; set; }

		[DataMember]
		public List<string> Genres { get; set; }

		#region AutoGen ReSharper code, only required by tests
		public bool Equals(Movie other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.ImdbId, ImdbId)
				&& Equals(other.Title, Title)
				&& other.Rating == Rating
				&& Equals(other.Director, Director)
				&& other.ReleaseDate.Equals(ReleaseDate)
				&& Equals(other.TagLine, TagLine)
				&& Genres.EquivalentTo(other.Genres);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(Movie)) return false;
			return Equals((Movie)obj);
		}

		public override int GetHashCode()
		{
			return ImdbId != null ? ImdbId.GetHashCode() : 0;
		}
		#endregion
	}

	[DataContract]
	public class MovieResponse
	{
		[DataMember]
		public Movie Movie { get; set; }
	}


	public class MovieService : RestServiceBase<Movie>
	{
		public IDbConnectionFactory DbFactory { get; set; }

		/// <summary>
		/// GET /movies/{Id} 
		/// </summary>
		public override object OnGet(Movie movie)
		{
			return new MovieResponse
			{
				Movie = DbFactory.Exec(dbCmd => dbCmd.GetById<Movie>(movie.Id))
			};
		}

		/// <summary>
		/// POST /movies
		/// </summary>
		public override object OnPost(Movie movie)
		{
			var newMovieId = DbFactory.Exec(dbCmd =>
			{
				dbCmd.Insert(movie);
				return dbCmd.GetLastInsertId();
			});

			var newMovie = new MovieResponse
			{
				Movie = DbFactory.Exec(dbCmd => dbCmd.GetById<Movie>(newMovieId))
			};
			return new HttpResult(newMovie)
			{
				StatusCode = HttpStatusCode.Created,
				Headers = {
					{ HttpHeaders.Location, this.RequestContext.AbsoluteUri.WithTrailingSlash() + newMovieId }
				}
			};
		}

		/// <summary>
		/// PUT /movies
		/// </summary>
		public override object OnPut(Movie movie)
		{
			DbFactory.Exec(dbCmd => dbCmd.Save(movie));
			return new MovieResponse();
		}

		/// <summary>
		/// DELETE /movies/{Id}
		/// </summary>
		public override object OnDelete(Movie request)
		{
			DbFactory.Exec(dbCmd => dbCmd.DeleteById<Movie>(request.Id));
			return new MovieResponse();
		}
	}


	[DataContract]
	[RestService("/movies", "GET")]
	[RestService("/movies/genres/{Genre}")]
	public class Movies
	{
		[DataMember]
		public string Genre { get; set; }

		[DataMember]
		public Movie Movie { get; set; }
	}

	[DataContract]
	public class MoviesResponse
	{
		public MoviesResponse()
		{
			Movies = new List<Movie>();
		}

		[DataMember]
		public List<Movie> Movies { get; set; }
	}

	public class MoviesService : RestServiceBase<Movies>
	{
		public IDbConnectionFactory DbFactory { get; set; }

		/// <summary>
		/// GET /movies 
		/// GET /movies/genres/{Genre}
		/// </summary>
		public override object OnGet(Movies request)
		{
			var response = new MoviesResponse
			{
				Movies = request.Genre.IsNullOrEmpty()
					? DbFactory.Exec(dbCmd => dbCmd.Select<Movie>())
					: DbFactory.Exec(dbCmd => dbCmd.Select<Movie>("Genres LIKE {0}", "%" + request.Genre + "%"))
			};

			return response;
		}
	}

	[DataContract]
	[RestService("/reset-movies")]
	public class ResetMovies { }

	[DataContract]
	public class ResetMoviesResponse
		: IHasResponseStatus
	{
		public ResetMoviesResponse()
		{
			this.ResponseStatus = new ResponseStatus();
		}

		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}

	public class ResetMoviesService : RestServiceBase<ResetMovies>
	{
		public static List<Movie> Top5Movies = new List<Movie>
		{
			new Movie { ImdbId = "tt0111161", Title = "The Shawshank Redemption", Rating = 9.2m, Director = "Frank Darabont", ReleaseDate = new DateTime(1995,2,17), TagLine = "Fear can hold you prisoner. Hope can set you free.", Genres = new List<string>{"Crime","Drama"}, },
			new Movie { ImdbId = "tt0068646", Title = "The Godfather", Rating = 9.2m, Director = "Francis Ford Coppola", ReleaseDate = new DateTime(1972,3,24), TagLine = "An offer you can't refuse.", Genres = new List<string> {"Crime","Drama", "Thriller"}, },
			new Movie { ImdbId = "tt1375666", Title = "Inception", Rating = 9.2m, Director = "Christopher Nolan", ReleaseDate = new DateTime(2010,7,16), TagLine = "Your mind is the scene of the crime", Genres = new List<string>{"Action", "Mystery", "Sci-Fi", "Thriller"}, },
			new Movie { ImdbId = "tt0071562", Title = "The Godfather: Part II", Rating = 9.0m, Director = "Francis Ford Coppola", ReleaseDate = new DateTime(1974,12,20), Genres = new List<string> {"Crime","Drama", "Thriller"}, },
			new Movie { ImdbId = "tt0060196", Title = "The Good, the Bad and the Ugly", Rating = 9.0m, Director = "Sergio Leone", ReleaseDate = new DateTime(1967,12,29), TagLine = "They formed an alliance of hate to steal a fortune in dead man's gold", Genres = new List<string>{"Adventure","Western"}, },
		};

		public IDbConnectionFactory DbFactory { get; set; }

		public override object OnPost(ResetMovies request)
		{
			DbFactory.Exec(dbCmd =>
			{
				const bool overwriteTable = true;
				dbCmd.CreateTable<Movie>(overwriteTable);
				dbCmd.SaveAll(Top5Movies);
			});

			return new ResetMoviesResponse();
		}
	}


	public class ExampleAppHostHttpListener
		: AppHostHttpListenerBase
	{
		private static ILog log;

		public ExampleAppHostHttpListener()
			: base("ServiceStack Examples", typeof(GetFactorialService).Assembly)
		{
			LogManager.LogFactory = new DebugLogFactory();
			log = LogManager.GetLogger(typeof(ExampleAppHostHttpListener));
		}

		public override void Configure(Container container)
		{
			//Signal advanced web browsers what HTTP Methods you accept
			base.SetConfig(new EndpointHostConfig
			{
				GlobalResponseHeaders =
				{
					{ "Access-Control-Allow-Origin", "*" },
					{ "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
				},
				WsdlServiceNamespace = "http://www.servicestack.net/types",
				WsdlServiceTypesNamespace = "http://www.servicestack.net/types",
				LogFactory = new ConsoleLogFactory()
			});

			container.Register<IResourceManager>(new ConfigurationResourceManager());

			var appSettings = container.Resolve<IResourceManager>();

			container.Register(c => new ExampleConfig(c.Resolve<IResourceManager>()));
			var appConfig = container.Resolve<ExampleConfig>();

			container.Register<IDbConnectionFactory>(c =>
				new OrmLiteConnectionFactory(
					":memory:", false,
					SqliteOrmLiteDialectProvider.Instance));

			var resetMovies = container.Resolve<ResetMoviesService>();
			resetMovies.Post(null);

			//var movies = container.Resolve<IDbConnectionFactory>().Exec(x => x.Select<Movie>());
			//Console.WriteLine(movies.Dump());
		}
	}


}