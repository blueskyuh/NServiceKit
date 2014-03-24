using System.Collections.Generic;
using System.Runtime.Serialization;
using NServiceKit.Common.Extensions;
using NServiceKit.OrmLite;
using NServiceKit.ServiceHost;

namespace NServiceKit.WebHost.IntegrationTests.Services
{
	[DataContract]
	[Route("/movies", "GET, OPTIONS")]
	[Route("/movies/genres/{Genre}")]
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
		[DataMember(Order = 1)]
		public List<Movie> Movies { get; set; }
	}

	public class MoviesService : ServiceInterface.Service
	{
		/// <summary>
		/// GET /movies 
		/// GET /movies/genres/{Genre}
		/// </summary>
		public object Get(Movies request)
		{
			return new MoviesResponse
			{
				Movies = request.Genre.IsNullOrEmpty()
					? Db.Select<Movie>()
					: Db.Select<Movie>("Genres LIKE {0}", "%" + request.Genre + "%")
			};
		}
	}
}