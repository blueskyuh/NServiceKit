using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints.Support;

namespace ServiceStack.WebHost.Endpoints
{
	public class Soap11AsyncOneWayHandler : SoapHandler
	{
		public override EndpointAttributes SoapType
		{
			get { return EndpointAttributes.Soap11; }
		}
	}
}