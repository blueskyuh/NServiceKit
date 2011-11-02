﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using Funq;

namespace ServiceStack.ServiceHost
{
	public class ServiceManager
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (ServiceManager));

		public Container Container { get; private set; }
		public ServiceController ServiceController { get; private set; }

		public ServiceOperations ServiceOperations { get; set; }
		public ServiceOperations AllServiceOperations { get; set; }

		public ServiceManager(params Assembly[] assembliesWithServices)
		{
			if (assembliesWithServices == null || assembliesWithServices.Length == 0)
				throw new ArgumentException(
					"No Assemblies provided in your AppHost's base constructor.\n"
					+ "To register your services, please provide the assemblies where your web services are defined.");

			this.Container = new Container();
//			this.ServiceController = new ServiceController(
//				() => assembliesWithServices.ToList().SelectMany(x => x.GetTypes()));
			this.ServiceController = new ServiceController(() => GetAssemblyTypes(assembliesWithServices));
		}

		public ServiceManager(bool autoInitialize, params Assembly[] assembliesWithServices)
			: this(assembliesWithServices)
		{
			if (autoInitialize)
			{
				this.Init();
			}
		}
		
		private List<Type> GetAssemblyTypes(Assembly[] assembliesWithServices)
		{
			var results = new List<Type>();
			string assemblyName = null;
			string typeName = null;
			
			try
			{
				foreach (var assembly in assembliesWithServices)
				{
					assemblyName = assembly.FullName;
					foreach (var type in assembly.GetTypes())
					{
						typeName = type.Name;
						results.Add(type);
					}
				}
				return results;
			}
			catch (Exception ex)
			{
				var msg = string.Format("Failed loading types, last assembly '{0}', type: '{1}'", assemblyName, typeName);
				Log.Error(msg, ex);
				throw new Exception(msg, ex);
			}
		}

		/// <summary>
		/// Inject alternative container and strategy for resolving Service Types
		/// </summary>
		public ServiceManager(Container container, ServiceController serviceController)
		{
			if (serviceController == null)
				throw new ArgumentNullException("serviceController");

			this.Container = container ?? new Container();
			this.ServiceController = serviceController;
		}

		private ExpressionTypeFunqContainer typeFactory;

		public void Init()
		{
			typeFactory = new ExpressionTypeFunqContainer(this.Container);

			this.ServiceController.Register(typeFactory);

			ReloadServiceOperations();

			typeFactory.RegisterTypes(this.ServiceController.ServiceTypes);
		}

		public void ReloadServiceOperations()
		{
			this.ServiceOperations = new ServiceOperations(this.ServiceController.OperationTypes);
			this.AllServiceOperations = new ServiceOperations(this.ServiceController.AllOperationTypes);
		}

		public void RegisterService<T>()
		{
			this.ServiceController.RegisterService(typeFactory, typeof(T));
			typeFactory.Register<T>();
		}

		public void RegisterService(Type serviceType)
		{
			this.ServiceController.RegisterService(typeFactory, serviceType);
			typeFactory.RegisterTypes(serviceType);
		}
	
		public object Execute(object dto)
		{
			return this.ServiceController.Execute(dto, null);
		}

		public void Dispose()
		{
			if (this.Container != null)
			{
				this.Container.Dispose();
			}
		}

		public void AfterInit()
		{
			this.ServiceController.AfterInit();
		}
	}

}
