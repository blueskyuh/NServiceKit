using System;
using NServiceKit.Common.Web;
using NServiceKit.ServiceHost;

namespace NServiceKit.ServiceInterface.Cors
{
    /// <summary>An enable cors.</summary>
    public class EnableCors : IHasRequestFilter
    {
        /// <summary>Order in which Request Filters are executed. &lt;0 Executed before global request filters &gt;0 Executed after global request filters.</summary>
        ///
        /// <value>The priority.</value>
        public int Priority { get { return 0; } }

        private readonly string allowedOrigins;
        private readonly string allowedMethods;
        private readonly string allowedHeaders;

        private readonly bool allowCredentials;

        private readonly Func<IHttpRequest, object, bool> applyWhere;

        /// <summary>
        /// Represents a default constructor with Allow Origin equals to "*", Allowed GET, POST, PUT, DELETE, OPTIONS request and allowed "Content-Type" header.
        /// </summary>
        public EnableCors(
            string allowedOrigins = "*", 
            string allowedMethods = CorsFeature.DefaultMethods, 
            string allowedHeaders = CorsFeature.DefaultHeaders, 
            bool allowCredentials = false,
            Func<IHttpRequest, object, bool> applyWhere = null)
        {
            this.allowedOrigins = allowedOrigins;
            this.allowedMethods = allowedMethods;
            this.allowedHeaders = allowedHeaders;
            this.allowCredentials = allowCredentials;
            this.applyWhere = applyWhere;
        }

        /// <summary>The request filter is executed before the service.</summary>
        ///
        /// <param name="req">       The http request wrapper.</param>
        /// <param name="res">       The http response wrapper.</param>
        /// <param name="requestDto">The request DTO.</param>
        public void RequestFilter(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            if (applyWhere != null && !applyWhere(req, requestDto))
                return;

            if (!string.IsNullOrEmpty(allowedOrigins))
                res.AddHeader(HttpHeaders.AllowOrigin, allowedOrigins);
            if (!string.IsNullOrEmpty(allowedMethods))
                res.AddHeader(HttpHeaders.AllowMethods, allowedMethods);
            if (!string.IsNullOrEmpty(allowedHeaders))
                res.AddHeader(HttpHeaders.AllowHeaders, allowedHeaders);
            if (allowCredentials)
                res.AddHeader(HttpHeaders.AllowCredentials, "true");
        }

        /// <summary>A new shallow copy of this filter is used on every request.</summary>
        ///
        /// <returns>An IHasRequestFilter.</returns>
        public IHasRequestFilter Copy()
        {
            return (IHasRequestFilter)MemberwiseClone();
        }
    }
}