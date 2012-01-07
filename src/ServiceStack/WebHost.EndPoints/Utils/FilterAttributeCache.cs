﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using System.Threading;
using ServiceStack.WebHost.Endpoints;

namespace ServiceStack.WebHost.EndPoints.Utils
{
    public static class FilterAttributeCache
    {
		private static Dictionary<Type, IHasRequestFilter[]> requestFilterAttributes
            = new Dictionary<Type, IHasRequestFilter[]>();

		private static Dictionary<Type, IHasResponseFilter[]> responseFilterAttributes
            = new Dictionary<Type, IHasResponseFilter[]>();

        public static IHasRequestFilter[] GetRequestFilterAttributes(Type requestDtoType)
        {
        	IHasRequestFilter[] attrs;
			if (requestFilterAttributes.TryGetValue(requestDtoType, out attrs)) return attrs;

			var attributes = new List<IHasRequestFilter>(
				(IHasRequestFilter[])requestDtoType.GetCustomAttributes(typeof(IHasRequestFilter), true));

        	var serviceType = EndpointHost.ServiceManager.ServiceController.RequestServiceTypeMap[requestDtoType];
			attributes.AddRange(
				(IHasRequestFilter[])serviceType.GetCustomAttributes(typeof(IHasRequestFilter), true)); 

			attrs = attributes.ToArray();

            Dictionary<Type, IHasRequestFilter[]> snapshot, newCache;
            do
            {
                snapshot = requestFilterAttributes;
                newCache = new Dictionary<Type, IHasRequestFilter[]>(requestFilterAttributes);
				newCache[requestDtoType] = attrs;

            } while (!ReferenceEquals(
            Interlocked.CompareExchange(ref requestFilterAttributes, newCache, snapshot), snapshot));

			return attrs;
        }

        public static IHasResponseFilter[] GetResponseFilterAttributes(Type responseDtoType)
        {
			IHasResponseFilter[] attrs;
			if (responseFilterAttributes.TryGetValue(responseDtoType, out attrs)) return attrs;

			var attributes = new List<IHasResponseFilter>(
	            (IHasResponseFilter[])responseDtoType.GetCustomAttributes(typeof(IHasResponseFilter), true));

			var serviceType = EndpointHost.ServiceManager.ServiceController.ResponseServiceTypeMap[responseDtoType];
			attributes.AddRange(
				(IHasResponseFilter[])serviceType.GetCustomAttributes(typeof(IHasResponseFilter), true));

			attrs = attributes.ToArray();

            Dictionary<Type, IHasResponseFilter[]> snapshot, newCache;
            do
            {
                snapshot = responseFilterAttributes;
                newCache = new Dictionary<Type, IHasResponseFilter[]>(responseFilterAttributes);
				newCache[responseDtoType] = attrs;

            } while (!ReferenceEquals(
            Interlocked.CompareExchange(ref responseFilterAttributes, newCache, snapshot), snapshot));

			return attrs;
        }
    }
}
