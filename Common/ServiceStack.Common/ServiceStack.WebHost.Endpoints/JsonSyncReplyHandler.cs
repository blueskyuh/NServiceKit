using System.IO;
using System.Web;
using ServiceStack.Service;
using ServiceStack.ServiceModel.Serialization;
using ServiceStack.WebHost.Endpoints.Support;

namespace ServiceStack.WebHost.Endpoints
{
	public class JsonSyncReplyHandler : JsonHandlerBase
	{
		public override void ProcessRequest(HttpContext context)
		{
			if (string.IsNullOrEmpty(context.Request.PathInfo)) return;

			var operationName = context.Request.PathInfo.Substring("/".Length);
			var request = CreateRequest(context.Request, operationName);

			var endpointAttributes = EndpointAttributes.SyncReply | EndpointAttributes.Json 
				| GetEndpointAttributes(context.Request);
			var response = ExecuteService(request, endpointAttributes);
			if (response == null) return;

			var responseJson = JsonDataContractSerializer.Instance.Parse(response);
			context.Response.ContentType = "application/json";
			context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			context.Response.Write(responseJson);
			context.Response.End();
		}

	}
}