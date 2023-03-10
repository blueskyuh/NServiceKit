using System;
using NServiceKit.Common;
using NServiceKit.Service;
using NServiceKit.ServiceClient.Web;

namespace NServiceKit.Messaging
{
    /// <summary>A client factory.</summary>
    public static class ClientFactory
    {
         /// <summary>Creates a new IOneWayClient.</summary>
         ///
         /// <exception cref="NotImplementedException">Thrown when the requested operation is unimplemented.</exception>
         ///
         /// <param name="endpointUrl">URL of the endpoint.</param>
         ///
         /// <returns>An IOneWayClient.</returns>
         public static IOneWayClient Create(string endpointUrl)
        {
             if (endpointUrl.IsNullOrEmpty() || !endpointUrl.StartsWith("http"))
                return null;

             if (endpointUrl.IndexOf("format=") == -1 || endpointUrl.IndexOf("format=json") >= 0)
                 return new JsonServiceClient(endpointUrl);

             if (endpointUrl.IndexOf("format=xml") >= 0)
                 return new XmlServiceClient(endpointUrl);

             if (endpointUrl.IndexOf("format=jsv") >= 0)
                 return new JsvServiceClient(endpointUrl);

             if (endpointUrl.IndexOf("format=soap11") >= 0)
                 return new Soap11ServiceClient(endpointUrl);

#if !(SILVERLIGHT || MONOTOUCH || XBOX || __ANDROID__)
             if (endpointUrl.IndexOf("format=soap12") >= 0)
                 return new Soap12ServiceClient(endpointUrl);
#endif

             throw new NotImplementedException("could not find service client for " + endpointUrl);
         }
    }
}