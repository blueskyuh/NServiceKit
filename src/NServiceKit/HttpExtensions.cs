using NServiceKit.Common;
using NServiceKit.Common.Web;
using NServiceKit.ServiceClient.Web;
using NServiceKit.ServiceHost;
using NServiceKit.WebHost.Endpoints;
using NServiceKit.WebHost.Endpoints.Extensions;
using System;
using System.Net;
using System.Web;

namespace NServiceKit
{
    /// <summary>
    /// HttpExtensions 
    /// </summary>
    public static class HttpExtensions
    {
        /// <summary>A HttpListenerContext extension method that converts this object to a request context.</summary>
        ///
        /// <param name="httpContext">The httpContext to act on.</param>
        /// <param name="requestDto"> The request dto.</param>
        ///
        /// <returns>The given data converted to a HttpRequestContext.</returns>
        public static HttpRequestContext ToRequestContext(this HttpContext httpContext, object requestDto = null)
        {
            return new HttpRequestContext(
                httpContext.Request.ToRequest(),
                httpContext.Response.ToResponse(),
                requestDto);
        }

        /// <summary>A HttpListenerContext extension method that converts this object to a request context.</summary>
        ///
        /// <param name="httpContext">The httpContext to act on.</param>
        /// <param name="requestDto"> The request dto.</param>
        ///
        /// <returns>The given data converted to a HttpRequestContext.</returns>
        public static HttpRequestContext ToRequestContext(this HttpListenerContext httpContext, object requestDto = null)
        {
            return new HttpRequestContext(
                httpContext.Request.ToRequest(),
                httpContext.Response.ToResponse(),
                requestDto);
        }

        /// <summary>An IReturn extension method that converts this object to an absolute URI.</summary>
        ///
        /// <param name="request">                        The request to act on.</param>
        /// <param name="httpMethod">                     The HTTP method.</param>
        /// <param name="formatFallbackToPredefinedRoute">The format fallback to predefined route.</param>
        ///
        /// <returns>The given data converted to a string.</returns>
        public static string ToAbsoluteUri(this IReturn request, string httpMethod = null, string formatFallbackToPredefinedRoute = null)
        {
            var relativeUrl = request.ToUrl(
                httpMethod ?? HttpMethods.Get,
                formatFallbackToPredefinedRoute ?? EndpointHost.Config.DefaultContentType.ToContentFormat());

            var absoluteUrl = EndpointHost.Config.WebHostUrl.CombineWith(relativeUrl);
            return absoluteUrl;
        }

        /// <summary>
        /// End a NServiceKit Request
        /// </summary>
        public static void EndRequest(this HttpResponse httpRes, bool skipHeaders = false)
        {
            if (!skipHeaders) httpRes.ApplyGlobalResponseHeaders();
            httpRes.Close();
            EndpointHost.CompleteRequest();
        }

        /// <summary>
        /// End a NServiceKit Request
        /// </summary>
        public static void EndRequest(this IHttpResponse httpRes, bool skipHeaders = false)
        {
            httpRes.EndHttpHandlerRequest(skipHeaders: skipHeaders);
            EndpointHost.CompleteRequest();
        }

        /// <summary>
        /// End a HttpHandler Request
        /// </summary>
        public static void EndHttpHandlerRequest(this HttpResponse httpRes, bool skipHeaders = false, bool skipClose = false, bool closeOutputStream = false, Action<HttpResponse> afterBody = null)
        {
            if (!skipHeaders) httpRes.ApplyGlobalResponseHeaders();
            if (afterBody != null) afterBody(httpRes);
            if (closeOutputStream) httpRes.CloseOutputStream();
            else if (!skipClose) httpRes.Close();

            //skipHeaders used when Apache+mod_mono doesn't like:
            //response.OutputStream.Flush();
            //response.Close();
        }

        /// <summary>
        /// End a HttpHandler Request
        /// </summary>
        public static void EndHttpHandlerRequest(this IHttpResponse httpRes, bool skipHeaders = false, bool skipClose = false, Action<IHttpResponse> afterBody = null)
        {
            if (!skipHeaders) httpRes.ApplyGlobalResponseHeaders();
            if (afterBody != null) afterBody(httpRes);
            if (!skipClose) httpRes.Close();

            //skipHeaders used when Apache+mod_mono doesn't like:
            //response.OutputStream.Flush();
            //response.Close();
        }

        /// <summary>
        /// End a NServiceKit Request with no content
        /// </summary>
        public static void EndRequestWithNoContent(this IHttpResponse httpRes)
        {
            if (EndpointHost.Config == null || EndpointHost.Config.Return204NoContentForEmptyResponse)
            {
                if (httpRes.StatusCode == (int)HttpStatusCode.OK)
                {
                    httpRes.StatusCode = (int)HttpStatusCode.NoContent;
                }
            }

            httpRes.SetContentLength(0);
            httpRes.EndRequest();
        }
    }
}