using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NServiceKit.Common.Utils;
using NServiceKit.Html;
using NServiceKit.Markdown;

namespace CSharpEval
{


    /// <summary>An expression.</summary>
	[TestFixture]
	public class _Expr
	 : NServiceKit.ServiceHost.Tests.Formats.TemplateTests.CustomMarkdownViewBase
	{
        /// <summary>Eval expression 0.</summary>
        ///
        /// <returns>A MvcHtmlString.</returns>
		public MvcHtmlString EvalExpr_0()
		{
			return null;
		}

		//[Test]
        /// <summary>Compare access.</summary>
		public void Compare_access()
		{
			var filePath = "~/AppData/TestsResults/Customer.htm".MapProjectPath();
			const int Times = 10000;

			var start = DateTime.Now;
			var count = 0;
			for (var i=0; i< Times; i++)
			{
				var result = File.ReadAllText(filePath);
				if (result != null) count++;
			}
			var timeTaken = DateTime.Now - start;
			Console.WriteLine("File.ReadAllText: Times {0}: {1}ms", Times, timeTaken.TotalMilliseconds);

			start = DateTime.Now;
			count = 0;
			//var fi = new FileInfo(filePath);
			for (var i=0; i < Times; i++)
			{
				var result = File.GetLastWriteTime(filePath);
				if (result != default(DateTime)) count++;
			}
			timeTaken = DateTime.Now - start;
			Console.WriteLine("FileInfo.LastWriteTime: Times {0}: {1}ms", Times, timeTaken.TotalMilliseconds);
		}

        /// <summary>As this object.</summary>
		[Test]
		public void A()
		{
			var str = "https://github.com/NServiceKit/NServiceKit.Redis/wiki/RedisPubSub";
			var pos = str.IndexOf("/wiki");
			Console.WriteLine(str.Substring(pos));
		}
	}
}
