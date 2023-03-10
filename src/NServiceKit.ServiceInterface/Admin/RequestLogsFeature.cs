using System;
using System.Collections.Generic;
using NServiceKit.ServiceHost;
using NServiceKit.ServiceInterface.Auth;
using NServiceKit.ServiceInterface.Providers;
using NServiceKit.WebHost.Endpoints;

namespace NServiceKit.ServiceInterface.Admin
{
    /// <summary>A request logs feature.</summary>
    public class RequestLogsFeature : IPlugin
    {
        /// <summary>
        /// RequestLogs service Route, default is /requestlogs
        /// </summary>
        public string AtRestPath { get; set; }

        /// <summary>
        /// Turn On/Off Session Tracking
        /// </summary>
        public bool EnableSessionTracking { get; set; }

        /// <summary>
        /// Turn On/Off Logging of Raw Request Body, default is Off
        /// </summary>
        public bool EnableRequestBodyTracking { get; set; }

        /// <summary>
        /// Turn On/Off Tracking of Responses
        /// </summary>
        public bool EnableResponseTracking { get; set; }

        /// <summary>
        /// Turn On/Off Tracking of Exceptions
        /// </summary>
        public bool EnableErrorTracking { get; set; }

        /// <summary>
        /// Size of InMemoryRollingRequestLogger circular buffer
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// Limit access to /requestlogs service to these roles
        /// </summary>
        public string[] RequiredRoles { get; set; }

        /// <summary>
        /// Change the RequestLogger provider. Default is InMemoryRollingRequestLogger
        /// </summary>
        public IRequestLogger RequestLogger { get; set; }

        /// <summary>
        /// Don't log requests of these types. By default RequestLog's are excluded
        /// </summary>
        public Type[] ExcludeRequestDtoTypes { get; set; }

        /// <summary>
        /// Don't log request bodys for services with sensitive information.
        /// By default Auth and Registration requests are hidden.
        /// </summary>
        public Type[] HideRequestBodyForRequestDtoTypes { get; set; }

        /// <summary>Initializes a new instance of the NServiceKit.ServiceInterface.Admin.RequestLogsFeature class.</summary>
        ///
        /// <param name="capacity">Size of InMemoryRollingRequestLogger circular buffer.</param>
        public RequestLogsFeature(int? capacity = null)
        {
            this.AtRestPath = "/requestlogs";
            this.Capacity = capacity;
            this.RequiredRoles = new [] { RoleNames.Admin };
            this.EnableErrorTracking = true;
            this.EnableRequestBodyTracking = false;
            this.ExcludeRequestDtoTypes = new[] { typeof(RequestLogs) };
            this.HideRequestBodyForRequestDtoTypes = new[] {
                typeof(Auth.Auth), typeof(Registration)
            };
        }

        /// <summary>Registers this object.</summary>
        ///
        /// <param name="appHost">The application host.</param>
        public void Register(IAppHost appHost)
        {
            appHost.RegisterService<RequestLogsService>(AtRestPath);

            var requestLogger = RequestLogger ?? new InMemoryRollingRequestLogger(Capacity);
            requestLogger.EnableSessionTracking = EnableSessionTracking;
            requestLogger.EnableResponseTracking = EnableResponseTracking;
            requestLogger.EnableRequestBodyTracking = EnableRequestBodyTracking;
            requestLogger.EnableErrorTracking = EnableErrorTracking;
            requestLogger.RequiredRoles = RequiredRoles;
            requestLogger.ExcludeRequestDtoTypes = ExcludeRequestDtoTypes;
            requestLogger.HideRequestBodyForRequestDtoTypes = HideRequestBodyForRequestDtoTypes;

            appHost.Register(requestLogger);

            if (EnableRequestBodyTracking)
            {
                appHost.PreRequestFilters.Insert(0, (httpReq, httpRes) => {
                    httpReq.UseBufferedStream = EnableRequestBodyTracking;
                });
            }
        }
    }
}