using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Razor.Generator;
using NServiceKit.Razor.Compilation;
using NServiceKit.Razor.Compilation.CodeTransformers;

namespace NServiceKit.Razor.Managers.RazorGen
{
    //[Export("MvcView", typeof(IRazorCodeTransformer))]
    /// <summary>A mvc view transformer.</summary>
    public class MvcViewTransformer : AggregateCodeTransformer
    {
        private const string DefaultModelTypeName = "dynamic";
        private const string ViewStartFileName = "_ViewStart";
        private static readonly IEnumerable<string> _namespaces = new[] { 
            "System.Web.Mvc", 
            "System.Web.Mvc.Html",
            "System.Web.Mvc.Ajax",
            "System.Web.Routing",
        };

        private readonly RazorCodeTransformerBase[] _codeTransformers = new RazorCodeTransformerBase[] { 
            new DirectivesBasedTransformers(),
            new AddGeneratedClassAttribute(),
            new AddPageVirtualPathAttribute(),
            new SetImports(_namespaces, replaceExisting: false),
//            new SetBaseType(typeof(WebViewPage)),
            new RemoveLineHiddenPragmas(),
            new MvcWebConfigTransformer(),
            new MakeTypePartial(),
        };
        private bool _isSpecialPage;

        internal static IEnumerable<string> MvcNamespaces
        {
            get { return _namespaces; }
        }

        /// <summary>Gets the code transformers.</summary>
        ///
        /// <value>The code transformers.</value>
        protected override IEnumerable<RazorCodeTransformerBase> CodeTransformers
        {
            get { return _codeTransformers; }
        }

        /// <summary>Initializes this object.</summary>
        ///
        /// <param name="razorHost"> The razor host.</param>
        /// <param name="directives">The directives.</param>
        public override void Initialize(RazorPageHost razorHost, IDictionary<string, string> directives)
        {
            base.Initialize(razorHost, directives);


            string baseClass = razorHost.DefaultBaseClass;
            _isSpecialPage = IsSpecialPage(razorHost.File.VirtualPath);

            // The CSharpRazorCodeGenerator decides to generate line pragmas based on if the file path is available. Set it to an empty string if we 
            // do not want to generate them.
            string path = razorHost.EnableLinePragmas ? razorHost.File.RealPath : String.Empty;
            razorHost.CodeGenerator = new CSharpRazorCodeGenerator(razorHost.DefaultClassName, razorHost.DefaultNamespace, path, razorHost)
            {
                GenerateLinePragmas = razorHost.EnableLinePragmas
            };
            //razorHost.Parser = new MvcCSharpRazorCodeParser();
        }

        /// <summary>Process the generated code.</summary>
        ///
        /// <param name="codeCompileUnit">   The code compile unit.</param>
        /// <param name="generatedNamespace">The generated namespace.</param>
        /// <param name="generatedClass">    The generated class.</param>
        /// <param name="executeMethod">     The execute method.</param>
        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeNamespace generatedNamespace, CodeTypeDeclaration generatedClass, CodeMemberMethod executeMethod)
        {
            base.ProcessGeneratedCode(codeCompileUnit, generatedNamespace, generatedClass, executeMethod);
            if (generatedClass.BaseTypes.Count > 0)
            {
                var codeTypeReference = (CodeTypeReference)generatedClass.BaseTypes[0];
                if (_isSpecialPage)
                {
                    //                    codeTypeReference.BaseType = typeof(ViewStartPage).FullName;
                }
                else if (!codeTypeReference.BaseType.Contains('<'))
                {
                    // Use the default model if it wasn't specified by the user.
                    codeTypeReference.BaseType += '<' + DefaultModelTypeName + '>';
                }
            }
        }

        private bool IsSpecialPage(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            return fileName.Equals(ViewStartFileName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
