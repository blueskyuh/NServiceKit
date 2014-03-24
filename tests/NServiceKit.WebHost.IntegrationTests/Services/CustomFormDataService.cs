using System.Runtime.Serialization;
using NServiceKit.ServiceHost;
using NServiceKit.ServiceInterface;
using NServiceKit.ServiceInterface.ServiceModel;

namespace NServiceKit.WebHost.IntegrationTests.Services
{
	[Route("/customformdata")]
	[DataContract]
	public class CustomFormData { }

	[DataContract]
	public class CustomFormDataResponse : IHasResponseStatus
	{
		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string Item0 { get; set; }

		[DataMember]
		public string Item1Delete { get; set; }

		[DataMember]
		public ResponseStatus ResponseStatus { get; set; }
	}

	public class CustomFormDataService : ServiceInterface.Service
	{
		//Parsing: &first-name=tom&item-0=blah&item-1-delete=1
		public object Post(CustomFormData request)
		{
			var httpReq = base.RequestContext.Get<IHttpRequest>();

			return new CustomFormDataResponse
			{
				FirstName = httpReq.FormData["first-name"],
				Item0 = httpReq.FormData["item-0"],
				Item1Delete = httpReq.FormData["item-1-delete"]
			};
		}
	}
}