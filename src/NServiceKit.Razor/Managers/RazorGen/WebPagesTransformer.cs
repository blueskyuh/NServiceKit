using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NServiceKit.Razor.Compilation;
using NServiceKit.Razor.Compilation.CodeTransformers;

namespace NServiceKit.Razor.Managers.RazorGen
{
//    [Export("WebPage", typeof(IRazorCodeTransformer))]
    /// <summary>A web page transformer.</summary>
    public class WebPageTransformer : AggregateCodeTransformer
    {
        private readonly List<RazorCodeTransformerBase> _transformers = new List<RazorCodeTransformerBase> { 
            new DirectivesBasedTransformers(),
            new AddGeneratedClassAttribute(),
            new AddPageVirtualPathAttribute(),
            new RemoveLineHiddenPragmas(),
            new SetImports(new[] { "System.Web.WebPages.Html" }, replaceExisting: false),
        };

        /// <summary>Gets the code transformers.</summary>
        ///
        /// <value>The code transformers.</value>
        protected override IEnumerable<RazorCodeTransformerBase> CodeTransformers
        {
            get
            {
                return _transformers;
            }
        }

        /// <summary>Initializes this object.</summary>
        ///
        /// <param name="razorHost"> The razor host.</param>
        /// <param name="directives">The directives.</param>
        public override void Initialize(RazorPageHost razorHost, IDictionary<string, string> directives)
        {
            base.Initialize(razorHost, directives);

            // Remove the extension and replace path separator slashes with underscores
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(razorHost.File.VirtualPath);

            // If its a PageStart page, set the base type to StartPage.
            if (fileNameWithoutExtension.Equals("_pagestart", StringComparison.OrdinalIgnoreCase))
            {
//                razorHost.DefaultBaseClass = typeof(System.Web.WebPages.StartPage).FullName;
            }
        }

        /// <summary>Process the generated code.</summary>
        ///
        /// <param name="codeCompileUnit">   The code compile unit.</param>
        /// <param name="generatedNamespace">The generated namespace.</param>
        /// <param name="generatedClass">    The generated class.</param>
        /// <param name="executeMethod">     The execute method.</param>
        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit,
                                                  CodeNamespace generatedNamespace,
                                                  CodeTypeDeclaration generatedClass,
                                                  CodeMemberMethod executeMethod)
        {
            base.ProcessGeneratedCode(codeCompileUnit, generatedNamespace, generatedClass, executeMethod);


            // Create the Href wrapper
            CodeTypeMember hrefMethod = new CodeSnippetTypeMember(@"
                // Resolve package relative syntax
                // Also, if it comes from a static embedded resource, change the path accordingly
                public override string Href(string virtualPath, params object[] pathParts) {
                    virtualPath = ApplicationPart.ProcessVirtualPath(GetType().Assembly, VirtualPath, virtualPath);
                    return base.Href(virtualPath, pathParts);
                }");

            generatedClass.Members.Add(hrefMethod);

            Debug.Assert(generatedClass.Name.Length > 0);
            if (!(Char.IsLetter(generatedClass.Name[0]) || generatedClass.Name[0] == '_'))
            {
                generatedClass.Name = '_' + generatedClass.Name;
            }

            // If the generatedClass starts with an underscore, add a ClsCompliant(false) attribute.
            if (generatedClass.Name[0] == '_')
            {
                generatedClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(CLSCompliantAttribute).FullName,
                                                        new CodeAttributeArgument(new CodePrimitiveExpression(false))));
            }
        }
    }
}
