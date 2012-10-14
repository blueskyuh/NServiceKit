using System;
using System.Web;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.Logging;
using ServiceStack.WebHost.Endpoints.Extensions;
using ServiceStack.WebHost.Endpoints.Metadata;
using ServiceStack.WebHost.Endpoints.Support.Templates;

namespace ServiceStack.WebHost.Endpoints.Support.Metadata
{
	public abstract class WsdlMetadataHandlerBase : HttpHandlerBase
	{
		private readonly ILog log = LogManager.GetLogger(typeof(WsdlMetadataHandlerBase));

		protected abstract WsdlTemplateBase GetWsdlTemplate();

		public override void Execute(HttpContext context)
		{
			EndpointHost.Config.AssertFeatures(Feature.Metadata);

			context.Response.ContentType = "text/xml";

			var baseUri = context.Request.GetParentBaseUrl();
			var optimizeForFlash = context.Request.QueryString["flash"] != null;
			var includeAllTypesInAssembly = context.Request.QueryString["includeAllTypes"] != null;
			var operations = includeAllTypesInAssembly ? EndpointHost.AllServiceOperations : EndpointHost.ServiceOperations;

			try
			{
				var wsdlTemplate = GetWsdlTemplate(operations, baseUri, optimizeForFlash, includeAllTypesInAssembly, context.Request.GetBaseUrl());
				context.Response.Write(wsdlTemplate.ToString());
			}
			catch (Exception ex)
			{
				log.Error("Autogeneration of WSDL failed.", ex);

				context.Response.Write("Autogenerated WSDLs are not supported "
					+ (Env.IsMono ? "on Mono" : "with this configuration"));
			}
		}

		public WsdlTemplateBase GetWsdlTemplate(ServiceOperations operations, string baseUri, bool optimizeForFlash, bool includeAllTypesInAssembly, string rawUrl)
		{
			var xsd = new XsdGenerator {
				OperationTypes = operations.AllOperations.Types,
				OptimizeForFlash = optimizeForFlash,
				IncludeAllTypesInAssembly = includeAllTypesInAssembly,
			}.ToString();

			var wsdlTemplate = GetWsdlTemplate();
			wsdlTemplate.Xsd = xsd;
			wsdlTemplate.ReplyOperationNames = operations.ReplyOperations.Names;
			wsdlTemplate.OneWayOperationNames = operations.OneWayOperations.Names;

			if (rawUrl.ToLower().StartsWith(baseUri))
			{
				wsdlTemplate.ReplyEndpointUri = rawUrl;
				wsdlTemplate.OneWayEndpointUri = rawUrl;
			}
			else
			{
				var suffix = GetType().Name.StartsWith("Soap11") ? "soap11" : "soap12";
				wsdlTemplate.ReplyEndpointUri = baseUri + suffix;
				wsdlTemplate.OneWayEndpointUri = baseUri + suffix;
			}

			return wsdlTemplate;
		}
	}
}