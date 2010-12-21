using System;
using System.Net;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.Text;

namespace ServiceStack.WebHost.Endpoints.Support
{
	public class RedirectHttpHandler
		: IServiceStackHttpHandler, IHttpHandler
	{
		public string RelativeUrl { get; set; }

		public string AbsoluteUrl { get; set; }

		/// <summary>
		/// Non ASP.NET requests
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <param name="operationName"></param>
		public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
		{
			if (string.IsNullOrEmpty(RelativeUrl) && string.IsNullOrEmpty(AbsoluteUrl))
				throw new ArgumentNullException("RelativeUrl or AbsoluteUrl");

			if (!string.IsNullOrEmpty(RelativeUrl))
			{
				var absoluteUrl = request.RawUrl.WithTrailingSlash() + this.RelativeUrl;
				response.StatusCode = (int)HttpStatusCode.Redirect;
				response.AddHeader(HttpHeaders.Location, absoluteUrl);
			}
			else
			{
				response.StatusCode = (int)HttpStatusCode.Redirect;
				response.AddHeader(HttpHeaders.Location, this.AbsoluteUrl);
			}
		}

        /// <summary>
        /// ASP.NET requests
        /// </summary>
        /// <param name="context"></param>
		public void ProcessRequest(HttpContext context)
		{
        	var request = context.Request;
			var response = context.Response;

			if (string.IsNullOrEmpty(RelativeUrl) && string.IsNullOrEmpty(AbsoluteUrl))
				throw new ArgumentNullException("RelativeUrl or AbsoluteUrl");

			if (!string.IsNullOrEmpty(RelativeUrl))
			{
				var absoluteUrl = request.RawUrl.WithTrailingSlash() + this.RelativeUrl;
				response.StatusCode = (int)HttpStatusCode.Redirect;
				response.AddHeader(HttpHeaders.Location, absoluteUrl);
			}
			else
			{
				response.StatusCode = (int)HttpStatusCode.Redirect;
				response.AddHeader(HttpHeaders.Location, this.AbsoluteUrl);
			}

		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}