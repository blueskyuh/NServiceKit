using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using NServiceKit.Common.Utils;
using NServiceKit.Razor;
using NServiceKit.VirtualPath;

namespace NServiceKit.ServiceHost.Tests.Formats_Razor
{
    /// <summary>A precompiled razor engine tests.</summary>
    [TestFixture]
    public class PrecompiledRazorEngineTests : RazorEngineTests
    {
        /// <summary>Gets a value indicating whether the precompile is enabled.</summary>
        ///
        /// <value>true if precompile enabled, false if not.</value>
        public override bool PrecompileEnabled { get { return true; } }

        const string View1Html = "<div class='view1'>@DateTime.Now</div>";
        const string View2Html = "<div class='view2'>@DateTime.Now</div>";
        const string View3Html = "<div class='view3'>@DateTime.Now</div>";

        /// <summary>Initializes the file system.</summary>
        ///
        /// <param name="fileSystem">The file system.</param>
        protected override void InitializeFileSystem(InMemoryVirtualPathProvider fileSystem)
        {
            base.InitializeFileSystem(fileSystem);

            fileSystem.AddFile("/views/v1.cshtml", View1Html);
            fileSystem.AddFile("/views/v2.cshtml", View2Html);
            fileSystem.AddFile("/views/v3.cshtml", View3Html);
        }

        /// <summary>Pages begin compilation startup.</summary>
        [Test]
        public void Pages_begin_compilation_on_startup()
        {
            foreach (var page in new[] {"v1", "v2", "v3"}.Select(name => RazorFormat.GetPageByName(name)))
            {
                Assert.That(page.MarkedForCompilation || page.IsCompiling || page.IsValid);
            }
        }

        /// <summary>Creates a new pages begin compilation when added.</summary>
        [Test]
        public void New_pages_begin_compilation_when_added()
        {
            const string template = "This is my sample template, Hello @Model.Name!";
            RazorFormat.AddFileAndPage("/simple.cshtml", template);

            var page = RazorFormat.GetPageByPathInfo("/simple.cshtml");
            FuncUtils.WaitWhile(() => page.MarkedForCompilation || page.IsCompiling, millisecondTimeout: 5000 );
            Assert.That(page.IsValid);
        }

        /// <summary>Pages with errors dont cause exceptions on thread starting the precompilation.</summary>
        [Test]
        public void Pages_with_errors_dont_cause_exceptions_on_thread_starting_the_precompilation()
        {
            const string template = "This is a bad template, Hello @SomeInvalidMember.Name!";
            RazorFormat.AddFileAndPage("/simple.cshtml", template);

            var page = RazorFormat.GetPageByPathInfo("/simple.cshtml");
            FuncUtils.WaitWhile(() => page.MarkedForCompilation || page.IsCompiling, millisecondTimeout: 5000);
            Assert.That(page.CompileException, Is.Not.Null);
        }

        /// <summary>Pages with errors still throw exceptions when rendering.</summary>
        [ExpectedException(typeof(HttpCompileException))]
        [Test]
        public void Pages_with_errors_still_throw_exceptions_when_rendering()
        {
            const string template = "This is a bad template, Hello @SomeInvalidMember.Name!";
            RazorFormat.AddFileAndPage("/simple.cshtml", template);

            RazorFormat.RenderToHtml("/simple.cshtml", new { Name = "World" });
        }
    }

    /// <summary>A startup precompiled razor engine tests.</summary>
    [TestFixture]
    public class StartupPrecompiledRazorEngineTests : PrecompiledRazorEngineTests
    {
        /// <summary>Gets a value indicating whether the precompile is enabled.</summary>
        ///
        /// <value>true if precompile enabled, false if not.</value>
        public override bool PrecompileEnabled { get { return true; } }

        /// <summary>Gets a value indicating whether the wait for precompile is enabled.</summary>
        ///
        /// <value>true if wait for precompile enabled, false if not.</value>
        public override bool WaitForPrecompileEnabled { get { return true; } }

        /// <summary>Precompilation finishes before returning from initialise.</summary>
        [Test]
        public void Precompilation_finishes_before_returning_from_init()
        {
            foreach (var page in new[] { "v1", "v2", "v3" }.Select(name => RazorFormat.GetPageByName(name)))
            {
                Assert.That(page.IsValid);
            }
        }
    }
}
