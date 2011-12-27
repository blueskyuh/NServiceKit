﻿using System;
using System.Data.Common;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Common;
using ServiceStack.Common.Utils;
using ServiceStack.Messaging;
using ServiceStack.MiniProfiler;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.Redis;
using ServiceStack.Redis.Messaging;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.IntegrationTests.Services;

namespace ServiceStack.WebHost.IntegrationTests
{
	public class Global : System.Web.HttpApplication
	{
		public class AppHost
			: AppHostBase
		{
			public AppHost()
				: base("ServiceStack WebHost IntegrationTests", typeof(Reverse).Assembly)
			{
			}

			public override void Configure(Container container)
			{
				this.RequestFilters.Add((req, res, dto) => {
					var requestFilter = dto as RequestFilter;
					if (requestFilter != null)
					{
						res.StatusCode = requestFilter.StatusCode;
						if (!requestFilter.HeaderName.IsNullOrEmpty())
						{
							res.AddHeader(requestFilter.HeaderName, requestFilter.HeaderValue);
						}
						res.Close();
					}

					var secureRequests = dto as IRequiresSession;
					if (secureRequests != null)
					{
						res.ReturnAuthRequired();
					}
				});

				this.Container.Register<IDbConnectionFactory>(c =>
					new OrmLiteConnectionFactory(
						"~/App_Data/db.sqlite".MapHostAbsolutePath(),
						SqliteOrmLiteDialectProvider.Instance) {
							ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current)
						});

				this.Container.Register<ICacheClient>(new MemoryCacheClient());
				//this.Container.Register<ICacheClient>(new BasicRedisClientManager());

				//this.Container.Register<ISessionFactory>(
				//    c => new SessionFactory(c.Resolve<ICacheClient>()));

				var dbFactory = this.Container.Resolve<IDbConnectionFactory>();
				dbFactory.Exec(dbCmd => dbCmd.CreateTable<Movie>(true));
				ModelConfig<Movie>.Id(x => x.Title);
				Routes
					.Add<Movies>("/custom-movies", "GET")
					.Add<Movies>("/custom-movies/genres/{Genre}")
					.Add<Movie>("/custom-movies", "POST,PUT")
					.Add<Movie>("/custom-movies/{Id}")
					.Add<MqHostStats>("/mqstats");

				var resetMovies = this.Container.Resolve<ResetMoviesService>();
				resetMovies.Post(null);

				ValidationFeature.Init(this);
				container.RegisterValidators(typeof(CustomersValidator).Assembly);

				//var onlyEnableFeatures = Feature.All.Remove(Feature.Jsv | Feature.Soap);
				SetConfig(new EndpointHostConfig {
					//EnableFeatures = onlyEnableFeatures,
					DebugMode = true, //Show StackTraces for easier debugging
				});

				var redisManager = new BasicRedisClientManager();
				var mqHost = new RedisMqHost(redisManager, 2, null);
				mqHost.RegisterHandler<Reverse>(this.Container.Resolve<ReverseService>().Execute);
				mqHost.Start();

				this.Container.Register((IMessageService)mqHost);
			}
		}

		protected void Application_Start(object sender, EventArgs e)
		{
			var appHost = new AppHost();
			appHost.Init();
		}

		protected void Application_BeginRequest(object src, EventArgs e)
		{
			if (Request.IsLocal)
				Profiler.Start();
		}

		protected void Application_EndRequest(object src, EventArgs e)
		{
			Profiler.Stop();

			var mqHost = AppHostBase.Instance.Container.TryResolve<IMessageService>();
			if (mqHost != null)
				mqHost.Start();
		}

	}
}