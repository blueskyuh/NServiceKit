using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Funq;

namespace NServiceKit.ServiceHost
{
    /// <summary>
    /// Extension methods on <see cref="Container"/>.
    /// </summary>
	public static class ContainerTypeExtensions
	{
        /// <summary>
        /// Registers the type in the IoC container and
        /// adds auto-wiring to the specified type.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="serviceType"></param>
        /// <param name="inFunqAsType"></param>
        /// <param name="scope"></param>
        public static void RegisterAutoWiredType(this Container container, Type serviceType, Type inFunqAsType,
			ReuseScope scope = ReuseScope.None)
		{
			if (serviceType.IsAbstract || serviceType.ContainsGenericParameters)
				return;

			var methodInfo = typeof(Container).GetMethod("RegisterAutoWiredAs", Type.EmptyTypes);
			var registerMethodInfo = methodInfo.MakeGenericMethod(new[] { serviceType, inFunqAsType });

			var registration = registerMethodInfo.Invoke(container, new object[0]) as IRegistration;
			registration.ReusedWithin(scope);
		}

		/// <summary>
		/// Registers the type in the IoC container and
		/// adds auto-wiring to the specified type.
		/// The reuse scope is set to none (transient).
		/// </summary>
		public static void RegisterAutoWiredType(this Container container, Type serviceType,
            ReuseScope scope = ReuseScope.None)
		{
			//Don't try to register base service classes
			if (serviceType.IsAbstract || serviceType.ContainsGenericParameters)
				return;

			var methodInfo = typeof(Container).GetMethod("RegisterAutoWired", Type.EmptyTypes);
			var registerMethodInfo = methodInfo.MakeGenericMethod(new[] { serviceType });

			var registration = registerMethodInfo.Invoke(container, new object[0]) as IRegistration;
			registration.ReusedWithin(scope);
		}

		/// <summary>
		/// Registers the types in the IoC container and
		/// adds auto-wiring to the specified types.
		/// The reuse scope is set to none (transient).
		/// </summary>
        public static void RegisterAutoWiredTypes(this Container container, IEnumerable<Type> serviceTypes, 
            ReuseScope scope = ReuseScope.None)
		{
			foreach (var serviceType in serviceTypes)
                container.RegisterAutoWiredType(serviceType, scope);
		}
	}
}
