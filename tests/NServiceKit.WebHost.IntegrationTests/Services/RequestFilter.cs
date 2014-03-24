using System;
using System.Runtime.Serialization;
using NServiceKit.ServiceHost;
using NServiceKit.ServiceInterface;

namespace NServiceKit.WebHost.IntegrationTests.Services
{
	[DataContract]
	public class RequestFilter
	{
		[DataMember]
		public int StatusCode { get; set; }

		[DataMember]
		public string HeaderName { get; set; }

		[DataMember]
		public string HeaderValue { get; set; }
	}

	[DataContract]
	public class RequestFilterResponse
	{
		[DataMember]
		public string Value { get; set; }
	}

	public class StatusCodeService : ServiceInterface.Service, IRequiresRequestContext
	{
		new public IRequestContext RequestContext { get; set; }

        public object Any(RequestFilter request)
		{
			return new RequestFilterResponse();
		}
	}
}