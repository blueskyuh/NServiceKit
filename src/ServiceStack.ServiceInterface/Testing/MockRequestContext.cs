using System;
using System.Collections.Generic;
using System.Net;
using Funq;
using ServiceStack.ServiceHost;

namespace ServiceStack.ServiceInterface.Testing
{
	public class MockRequestContext : IRequestContext
	{
		public MockRequestContext()
		{
			this.Cookies = new Dictionary<string, Cookie>();
			this.Files = new IFile[0];
			this.Container = new Container();
			this.Container.Register<IHttpRequest>(
				new MockHttpRequest { Container = this.Container });
			this.Container.Register<IHttpResponse>(new MockHttpResponse());
		}

		public T Get<T>() where T : class
		{
			return Container.TryResolve<T>();
		}

		public string IpAddress { get; private set; }

		public string GetHeader(string headerName)
		{
			return Get<IHttpRequest>().Headers[headerName];
		}

		public Container Container { get; private set; }
		public IDictionary<string, Cookie> Cookies { get; private set; }
		public EndpointAttributes EndpointAttributes { get; private set; }
		public IRequestAttributes RequestAttributes { get; private set; }
		public string ContentType { get; private set; }
		public string ResponseContentType { get; private set; }
		public string CompressionType { get; private set; }
		public string AbsoluteUri { get; private set; }
		public string PathInfo { get; private set; }
		public IFile[] Files { get; private set; }

		public void Dispose()
		{
		}
	}
}