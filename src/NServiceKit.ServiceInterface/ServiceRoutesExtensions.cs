using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NServiceKit.Common;
using NServiceKit.Common.Utils;
using NServiceKit.Common.Web;
using NServiceKit.Text;
using NServiceKit.ServiceHost;
using NServiceKit.WebHost.Endpoints;

namespace NServiceKit.ServiceInterface
{
    /// <summary>A service routes extensions.</summary>
    public static class ServiceRoutesExtensions
    {
        /// <summary>
        ///     Scans the supplied Assemblies to infer REST paths and HTTP verbs.
        /// </summary>
        ///<param name="routes">The <see cref="IServiceRoutes"/> instance.</param>
        ///<param name="assembliesWithServices">
        ///     The assemblies with REST services.
        /// </param>
        /// <returns>The same <see cref="IServiceRoutes"/> instance;
        ///		never <see langword="null"/>.</returns>
        public static IServiceRoutes AddFromAssembly(this IServiceRoutes routes,
                                                     params Assembly[] assembliesWithServices)
        {
            foreach (Assembly assembly in assembliesWithServices)
            {
                AddOldApiRoutes(routes, assembly);
                AddNewApiRoutes(routes, assembly);
            }

            return routes;
        }

        private static void AddNewApiRoutes(IServiceRoutes routes, Assembly assembly)
        {
            var services = assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract
                            && t.HasInterface(typeof(IService)));

            foreach (Type service in services)
            {
                var allServiceActions = service.GetActions();
                foreach (var requestDtoActions in allServiceActions.GroupBy(x => x.GetParameters()[0].ParameterType))
                {
                    var requestType = requestDtoActions.Key;
                    var hasWildcard = requestDtoActions.Any(x => x.Name.EqualsIgnoreCase(ActionContext.AnyAction));
                    string allowedVerbs = null; //null == All Routes
                    if (!hasWildcard)
                    {
                        var allowedMethods = new List<string>();
                        foreach (var action in requestDtoActions)
                        {
                            allowedMethods.Add(action.Name.ToUpper());
                        }

                        if (allowedMethods.Count == 0) continue;
                        allowedVerbs = string.Join(" ", allowedMethods.ToArray());
                    }

                    routes.AddRoute(requestType, allowedVerbs);
                }
            }
        }

        
        private static void AddOldApiRoutes(IServiceRoutes routes, Assembly assembly)
        {
#pragma warning disable 618
            var services = assembly.GetExportedTypes().Where(t => !t.IsAbstract && t.IsSubclassOfRawGeneric(typeof(ServiceBase<>)));

            foreach (Type service in services)
            {
                Type baseType = service.BaseType;
                //go up the hierarchy to the first generic base type
                while (!baseType.IsGenericType)
                {
                    baseType = baseType.BaseType;
                }

                Type requestType = baseType.GetGenericArguments()[0];

                string allowedVerbs = null; //null == All Routes

                if (service.IsSubclassOfRawGeneric(typeof(RestServiceBase<>)))
                {
                    //find overriden REST methods
                    var allowedMethods = new List<string>();
                    if (service.GetMethod("OnGet").DeclaringType == service)
                    {
                        allowedMethods.Add(HttpMethods.Get);
                    }

                    if (service.GetMethod("OnPost").DeclaringType == service)
                    {
                        allowedMethods.Add(HttpMethods.Post);
                    }

                    if (service.GetMethod("OnPut").DeclaringType == service)
                    {
                        allowedMethods.Add(HttpMethods.Put);
                    }

                    if (service.GetMethod("OnDelete").DeclaringType == service)
                    {
                        allowedMethods.Add(HttpMethods.Delete);
                    }

                    if (service.GetMethod("OnPatch").DeclaringType == service)
                    {
                        allowedMethods.Add(HttpMethods.Patch);
                    }

                    if (allowedMethods.Count == 0) continue;
                    allowedVerbs = string.Join(" ", allowedMethods.ToArray());
                }

                routes.AddRoute(requestType, allowedVerbs);
            }
#pragma warning restore 618
        }

        private static void AddRoute(this IServiceRoutes routes, Type requestType, string allowedVerbs)
        {
            foreach (var strategy in EndpointHost.Config.RouteNamingConventions)
            {
                strategy(routes, requestType, allowedVerbs);
            }
        }

        /// <summary>The IServiceRoutes extension method that adds serviceRoutes.</summary>
        ///
        /// <typeparam name="TRequest">Type of the request.</typeparam>
        /// <param name="routes">  The <see cref="IServiceRoutes"/> instance.</param>
        /// <param name="restPath">Full pathname of the rest file.</param>
        /// <param name="verbs">   The verbs.</param>
        ///
        /// <returns>The IServiceRoutes.</returns>
        public static IServiceRoutes Add<TRequest>(this IServiceRoutes routes, string restPath, ApplyTo verbs)
        {
            return routes.Add<TRequest>(restPath, verbs.ToVerbsString());
        }

        /// <summary>The IServiceRoutes extension method that adds routes.</summary>
        ///
        /// <param name="routes">     The <see cref="IServiceRoutes"/> instance.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="restPath">   Full pathname of the rest file.</param>
        /// <param name="verbs">      The verbs.</param>
        ///
        /// <returns>The IServiceRoutes.</returns>
        public static IServiceRoutes Add(this IServiceRoutes routes, Type requestType, string restPath, ApplyTo verbs)
        {
            return routes.Add(requestType, restPath, verbs.ToVerbsString());
        }

        private static string ToVerbsString(this ApplyTo verbs)
        {
            var allowedMethods = new List<string>();
            foreach (var entry in ApplyToUtils.ApplyToVerbs)
            {
                if (verbs.Has(entry.Key))
                    allowedMethods.Add(entry.Value);
            }

            return string.Join(" ", allowedMethods.ToArray());
        }

        /// <summary>A Type extension method that query if 'toCheck' is subclass of raw generic.</summary>
        ///
        /// <param name="toCheck">The toCheck to act on.</param>
        /// <param name="generic">The generic.</param>
        ///
        /// <returns>true if subclass of raw generic, false if not.</returns>
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != typeof(object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        private static string FormatRoute<T>(string restPath, params Expression<Func<T, object>>[] propertyExpressions)
        {
            var properties = propertyExpressions.Select(x => string.Format("{{{0}}}", PropertyName(x))).ToArray();
            return string.Format(restPath, properties);
        }

        private static string PropertyName(LambdaExpression lambdaExpression)
        {
            return (lambdaExpression.Body is UnaryExpression ? (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand : (MemberExpression)lambdaExpression.Body).Member.Name;
        }

        /// <summary>The IServiceRoutes extension method that adds serviceRoutes.</summary>
        ///
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="serviceRoutes">      The serviceRoutes to act on.</param>
        /// <param name="restPath">           Full pathname of the rest file.</param>
        /// <param name="verbs">              The verbs.</param>
        /// <param name="propertyExpressions">A variable-length parameters list containing property expressions.</param>
        ///
        /// <returns>The IServiceRoutes.</returns>
        public static IServiceRoutes Add<T>(this IServiceRoutes serviceRoutes, string restPath, ApplyTo verbs, params Expression<Func<T, object>>[] propertyExpressions)
        {
            return serviceRoutes.Add<T>(FormatRoute(restPath, propertyExpressions), verbs);
        }
    }
}