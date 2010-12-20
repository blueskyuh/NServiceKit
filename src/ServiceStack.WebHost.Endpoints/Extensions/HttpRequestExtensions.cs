using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.Logging;
using ServiceStack.ServiceHost;

namespace ServiceStack.WebHost.Endpoints.Extensions
{
	/**
	 * 
		 Input: http://localhost:96/Cambia3/Temp/Test.aspx/path/info?q=item#fragment

		Some HttpRequest path and URL properties:
		Request.ApplicationPath:	/Cambia3
		Request.CurrentExecutionFilePath:	/Cambia3/Temp/Test.aspx
		Request.FilePath:			/Cambia3/Temp/Test.aspx
		Request.Path:				/Cambia3/Temp/Test.aspx/path/info
		Request.PathInfo:			/path/info
		Request.PhysicalApplicationPath:	D:\Inetpub\wwwroot\CambiaWeb\Cambia3\
		Request.QueryString:		/Cambia3/Temp/Test.aspx/path/info?query=arg
		Request.Url.AbsolutePath:	/Cambia3/Temp/Test.aspx/path/info
		Request.Url.AbsoluteUri:	http://localhost:96/Cambia3/Temp/Test.aspx/path/info?query=arg
		Request.Url.Fragment:	
		Request.Url.Host:			localhost
		Request.Url.LocalPath:		/Cambia3/Temp/Test.aspx/path/info
		Request.Url.PathAndQuery:	/Cambia3/Temp/Test.aspx/path/info?query=arg
		Request.Url.Port:			96
		Request.Url.Query:			?query=arg
		Request.Url.Scheme:			http
		Request.Url.Segments:		/
									Cambia3/
									Temp/
									Test.aspx/
									path/
									info
	 * */
	public static class HttpRequestExtensions
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(HttpRequestExtensions));

		public static string GetOperationName(this HttpRequest request)
		{
			var pathInfo = request.GetLastPathInfo();
			return GetOperationNameFromLastPathInfo(pathInfo);
		}

		public static string GetOperationNameFromLastPathInfo(string lastPathInfo)
		{
			if (string.IsNullOrEmpty(lastPathInfo)) return null;

			var operationName = lastPathInfo.Substring("/".Length);

			return operationName;
		}

		private static string GetLastPathInfoFromRawUrl(string rawUrl)
		{
			var pathInfo = rawUrl.IndexOf("?") != -1
				? rawUrl.Substring(0, rawUrl.IndexOf("?"))
				: rawUrl;

			pathInfo = pathInfo.Substring(pathInfo.LastIndexOf("/"));

			return pathInfo;
		}

		public static string GetLastPathInfo(this HttpRequest request)
		{
			var pathInfo = request.PathInfo;
			if (string.IsNullOrEmpty(pathInfo))
			{
				pathInfo = GetLastPathInfoFromRawUrl(request.RawUrl);
			}

			//Log.DebugFormat("Request.PathInfo: {0}, Request.RawUrl: {1}, pathInfo:{2}",
			//    request.PathInfo, request.RawUrl, pathInfo);

			return pathInfo;
		}

		public static string GetUrlHostName(this HttpRequest request)
		{
			//TODO: Fix bug in mono fastcgi, when trying to get 'Request.Url.Host'
			try
			{
				return request.Url.Host;
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Error trying to get 'Request.Url.Host'", ex);

				return request.UserHostName;
			}
		}

		// http://localhost/ServiceStack.Examples.Host.Web/Public/Public/Soap12/Wsdl => 
		// http://localhost/ServiceStack.Examples.Host.Web/Public/Soap12/
		public static string GetParentBaseUrl(this HttpRequest request)
		{
			var rawUrl = request.RawUrl; // /Cambia3/Temp/Test.aspx/path/info
			var endpointsPath = rawUrl.Substring(0, rawUrl.LastIndexOf('/') + 1);  // /Cambia3/Temp/Test.aspx/path
			return GetAuthority(request) + endpointsPath;
		}

		public static string GetBaseUrl(this HttpRequest request)
		{
			return GetAuthority(request) + request.RawUrl;
		}

		//=> http://localhost:96 ?? ex=> http://localhost
		private static string GetAuthority(HttpRequest request)
		{
			try
			{
				return request.Url.GetLeftPart(UriPartial.Authority);
			}
			catch (Exception ex)
			{
				Log.Error("Error trying to get: request.Url.GetLeftPart(UriPartial.Authority): " + ex.Message, ex);
				return "http://" + request.UserHostName;
			}
		}

		public static string GetOperationName(this HttpListenerRequest request)
		{
			return request.Url.Segments[request.Url.Segments.Length - 1];
		}

		public static string GetLastPathInfo(this HttpListenerRequest request)
		{
			return GetLastPathInfoFromRawUrl(request.RawUrl);
		}

		public static string GetPathInfo(this HttpRequest request)
		{
			return GetPathInfo(request.PathInfo, request.Path);
		}

		private static string GetPathInfo(string pathInfo, string fullPath)
		{
			if (!string.IsNullOrEmpty(pathInfo)) return pathInfo;

			var mappedPathRoot = EndpointHost.Config.ServiceStackHandlerFactoryPath;

			var sbPathInfo = new StringBuilder();
			var fullPathParts = fullPath.Split('/');
			var pathRootFound = false;
			foreach (var fullPathPart in fullPathParts)
			{
				if (pathRootFound)
				{
					sbPathInfo.Append("/" + fullPathPart);
				}
				else
				{
					pathRootFound = string.Equals(fullPathPart, mappedPathRoot, StringComparison.InvariantCultureIgnoreCase);
				}
			}
			return sbPathInfo.ToString();
		}

		public static bool IsContentType(this IHttpRequest request, string contentType)
		{
			return request.ContentType.StartsWith(contentType, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool HasAnyOfContentTypes(this IHttpRequest request, params string[] contentTypes)
		{
			foreach (var contentType in contentTypes)
			{
				if (IsContentType(request, contentType)) return true;
			}
			return false;
		}

		public static IHttpRequest GetHttpRequest(this HttpRequest request)
		{
			return new HttpRequestWrapper(null, request);
		}

		public static IHttpRequest GetHttpRequest(this HttpListenerRequest request)
		{
			return new HttpListenerRequestWrapper(null, request);
		}

		public static Dictionary<string, string> GetRequestParams(this IHttpRequest request)
		{
			var map = new Dictionary<string, string>();

			foreach (var name in request.QueryString.AllKeys)
			{
				map[name] = request.QueryString[name];
			}

			if ((request.HttpMethod == HttpMethods.Post || request.HttpMethod == HttpMethods.Put) 
				&& request.FormData != null)
			{
				foreach (var name in request.FormData.AllKeys)
				{
					map[name] = request.FormData[name];
				}
			}

			return map;
		}

		public static string GetQueryStringContentType(this IHttpRequest httpReq)
		{
			var callback = httpReq.QueryString["callback"];
			if (!string.IsNullOrEmpty(callback)) return ContentType.Json;

			var format = httpReq.QueryString["format"];
			if (format == null) return null;

			format = format.ToLower();
			if (format.Contains("json")) return ContentType.Json;
			if (format.Contains("xml")) return ContentType.Xml;
			if (format.Contains("jsv")) return ContentType.Jsv;

			return null;
		}

		public static string[] PreferredContentTypes = new[] {
			ContentType.Json, ContentType.Xml, ContentType.Jsv
		};

		public static string GetResponseContentType(this IHttpRequest httpReq)
		{
			var specifiedContentType = GetQueryStringContentType(httpReq);
			if (!string.IsNullOrEmpty(specifiedContentType)) return specifiedContentType;

			var acceptContentType = httpReq.AcceptTypes;
			var defaultContentType = httpReq.ContentType;
			if (httpReq.HasAnyOfContentTypes(ContentType.FormUrlEncoded, ContentType.MultiPartFormData))
			{
				defaultContentType = EndpointHost.Config.DefaultContentType;
			}

			var acceptsAnything = false;
			var hasDefaultContentType = !string.IsNullOrEmpty(defaultContentType);
			if (acceptContentType != null)
			{
				foreach (var contentType in acceptContentType)
				{
					acceptsAnything = acceptsAnything || contentType == "*/*";
					if (acceptsAnything && hasDefaultContentType) return defaultContentType;

					foreach (var preferredContentType in PreferredContentTypes)
					{
						if (contentType.StartsWith(preferredContentType)) return preferredContentType;
					}
				}
			}

			//We could also send a '406 Not Acceptable', but this is allowed also
			return EndpointHost.Config.DefaultContentType;
		}
	}


}