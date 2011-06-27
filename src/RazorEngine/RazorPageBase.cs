﻿using System;
using System.Collections.Generic;
using RazorEngine.Templating;
using ServiceStack.Markdown;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints;

namespace RazorEngine
{
	public abstract class RazorPageBase<TModel> : TemplateBase<TModel>, IRazorTemplate
	{
		public UrlHelper Url = new UrlHelper();
		public HtmlHelper<TModel> Html = new HtmlHelper<TModel>();

		public IAppHost AppHost { get; set; }
		public IHttpResponse Response { get; set; }

		public Dictionary<string, object> ScopeArgs { get; set; }

		public void Init(IViewEngine viewEngine, ViewDataDictionary viewData, IHttpResponse httpRes)
		{
			this.Response = httpRes;
			Html.Init(viewEngine, viewData);
		}

		public string Layout { get; set; }

		public void Prepend(string contents)
		{
			if (contents == null) return;
			Builder.Insert(0, contents);
		}

		protected RazorPageBase()
		{
			this.ScopeArgs = new Dictionary<string, object>();
		}

		public T TryResolve<T>()
		{
			return this.AppHost.TryResolve<T>();
		}

		public string Href(string url)
		{
			return Url.Content(url);
		}
	}
}