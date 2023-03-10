using NUnit.Framework;
using NServiceKit.WebHost.Endpoints.Support.Markdown;

namespace NServiceKit.ServiceHost.Tests.Formats
{
    /// <summary>A template extention tests.</summary>
	[TestFixture]
	public class TemplateExtentionTests
	{
        /// <summary>Does all remove white space.</summary>
		[Test]
		public void Does_all_remove_WhiteSpace()
		{
			var test = "this[ is ]\ta\ntest";
			var result = test.RemoveAllWhiteSpace();

			Assert.That(result, Is.EqualTo("this[is]atest"));
		}
	}
}