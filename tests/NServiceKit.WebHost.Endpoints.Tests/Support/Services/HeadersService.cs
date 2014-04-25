using System;
using System.Runtime.Serialization;
using NServiceKit.ServiceHost;

namespace NServiceKit.WebHost.Endpoints.Tests.Support.Services
{
	[DataContract]
	public class Headers
	{
		[DataMember]
		public string Name { get; set; }
	}

	[DataContract]
	public class HeadersResponse
	{
		[DataMember]
		public string Value { get; set; }
	}

	public class HeadersService
		: TestServiceBase<Headers>, IRequiresHttpRequest
	{
		public IHttpRequest HttpRequest { get; set; }

		protected override object Run(Headers request)
		{
			var header = RequestContext.GetHeader(request.Name);
			if (header != HttpRequest.Headers[request.Name])
				throw new NullReferenceException();

			return new HeadersResponse
			{
				Value = header
			};
		}
	}

}