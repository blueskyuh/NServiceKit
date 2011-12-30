﻿using ServiceStack.RazorEngine;
using ServiceStack.ServiceHost.Tests.AppData;

namespace ServiceStack.ServiceHost.Tests.Formats_Razor
{
	public abstract class CustomRazorBasePage<TModel> : RazorPageBase<TModel>
	{
		public FormatHelpers Fmt = new FormatHelpers();
		public NorthwindHelpers Nwnd = new NorthwindHelpers();
	}
}