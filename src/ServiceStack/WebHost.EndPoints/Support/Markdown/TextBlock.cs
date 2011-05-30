﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using ServiceStack.Common;
using ServiceStack.Logging;
using ServiceStack.Markdown;
using ServiceStack.Text;

namespace ServiceStack.WebHost.EndPoints.Support.Markdown
{
	public class PageContext
	{
		public PageContext() {}

		public PageContext(MarkdownPage markdownPage, Dictionary<string, object> scopeArgs, bool renderHtml)
		{
			MarkdownPage = markdownPage;
			ScopeArgs = scopeArgs;
			RenderHtml = renderHtml;
		}

		public MarkdownPage MarkdownPage { get; set; }
		public Dictionary<string, object> ScopeArgs { get; set; }
		public bool RenderHtml { get; set; }
	}


	public abstract class TemplateBlock : ITemplateWriter
	{
		protected MarkdownPage Page { get; set; }

		protected Evaluator Evaluator { get; set; }

		public bool IsNested { get; set; }

		protected bool WriteRawHtml { get; set; }

		protected bool RenderHtml { get; set; }

		protected PageContext PageContext { get; set; }

		protected Dictionary<string, object> ScopeArgs { get; set; }

		public void DoFirstRun(PageContext pageContext)
		{
			this.PageContext = pageContext;
			this.Page = pageContext.MarkdownPage;
			this.RenderHtml = pageContext.RenderHtml;
			this.ScopeArgs = pageContext.ScopeArgs;

			OnFirstRun();
		}

		public void AfterFirstRun(Evaluator evaluator)
		{
			this.Evaluator = evaluator;

			OnAfterFirstRun();
		}

		protected virtual void OnFirstRun() {}
		protected virtual void OnAfterFirstRun() { }

		public void AddEvalItem(EvaluatorItem evalItem)
		{
			this.Page.ExecutionContext.Items.Add(evalItem);
		}

		public string Transform(string markdownText)
		{
			return this.RenderHtml 
				? Page.Markdown.Transform(markdownText)
				: markdownText;
		}

		public abstract void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs);
	}

	public class TextBlock : TemplateBlock
	{
		public TextBlock(string content)
		{
			Content = content;
		}

		public string Content { get; set; }

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			textWriter.Write(Content);
		}
	}

	public class VarReferenceBlock : TemplateBlock
	{
		private readonly string varName;

		public VarReferenceBlock(string varName)
		{
			this.varName = varName;
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			object value = null;
			scopeArgs.TryGetValue(varName, out value);

			if (value == null)
				return;

			textWriter.Write(value);
		}
	}

	public class MemberExprBlock : TemplateBlock
	{
		private static ILog Log = LogManager.GetLogger(typeof(MemberExprBlock));

		private readonly string modelMemberExpr;
		private readonly string varName;

		private bool ReferencesSelf
		{
			get { return this.modelMemberExpr == null; }
		}

		public MemberExprBlock(string memberExpr)
		{
			try
			{
				this.varName = memberExpr.GetVarName();
				this.modelMemberExpr = varName != memberExpr
					? memberExpr.Substring(this.varName.Length + 1)
					: null;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private Func<object, string> valueFn;
		protected override void OnFirstRun()
		{
			base.OnFirstRun();

			object memberExprValue;
			if (ScopeArgs.TryGetValue(this.varName, out memberExprValue))
			{
				valueFn = this.ReferencesSelf
					? Convert.ToString
					: DataBinder.CompileToString(memberExprValue.GetType(), modelMemberExpr);
			}
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			object memberExprValue;
			if (!scopeArgs.TryGetValue(this.varName, out memberExprValue))
			{
				textWriter.Write(modelMemberExpr);
				return;
			}

			if (memberExprValue == null) return;

			try
			{
				var strValue = this.ReferencesSelf
					? Convert.ToString(memberExprValue)
					: valueFn(memberExprValue);

				textWriter.Write(HttpUtility.HtmlEncode(strValue));
			}
			catch (Exception ex)
			{
				Log.Error("MemberExprBlock: " + ex.Message, ex);
			}
		}
	}

	public class StatementExprBlock : TemplateBlock
	{
		public StatementExprBlock(string condition, string statement)
		{
			this.Condition = condition;
			this.Statement = statement;
			this.ChildBlocks = new List<TemplateBlock>();
		}

		public string Condition { get; set; }
		public string Statement { get; set; }

		public List<TemplateBlock> ChildBlocks { get; set; }

		protected void Prepare(List<StatementExprBlock> allStatements)
		{
			if (this.Statement.IsNullOrEmpty()) return;

			var parsedStatement = Extract(this.Statement, allStatements);

			this.ChildBlocks = parsedStatement.CreateTemplateBlocks(allStatements);
			this.ChildBlocks.ForEach(x => x.IsNested = true);

			RemoveTrailingNewLineIfProceedsStatement();
		}

		private void RemoveTrailingNewLineIfProceedsStatement()
		{
			if (this.ChildBlocks.Count < 2) return;

			var lastIndex = this.ChildBlocks.Count - 1;
			if (!(this.ChildBlocks[lastIndex - 1] is StatementExprBlock)) return;

			var textBlock = this.ChildBlocks[lastIndex] as TextBlock;
			if (textBlock == null) return;

			if (textBlock.Content == "\r\n")
			{
				this.ChildBlocks.RemoveAt(lastIndex);
			}
		}

		public int Id { get; set; }

		protected void OnFirstRun(bool applyToChildren)
		{
			if (applyToChildren)
				this.OnFirstRun();
			else
				base.OnFirstRun();
		}

		protected override void OnFirstRun()
		{
			base.OnFirstRun();

			this.Id = Page.GetNextId();

			foreach (var templateBlock in ChildBlocks)
			{
				templateBlock.DoFirstRun(this.PageContext);
			}
		}

		protected override void OnAfterFirstRun()
		{
			base.OnAfterFirstRun();

			foreach (var templateBlock in ChildBlocks)
			{
				templateBlock.AfterFirstRun(this.Evaluator);
			}
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			WriteInternal(instance, textWriter, scopeArgs);
		}

		private void WriteInternal(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			foreach (var templateBlock in ChildBlocks)
			{
				templateBlock.Write(instance, textWriter, scopeArgs);
			}
		}

		public static string Extract(string content, List<StatementExprBlock> allStatements)
		{
			var sb = new StringBuilder();

			var initialCount = allStatements.Count;
			int pos;
			var lastPos = 0;
			while ((pos = content.IndexOf('@', lastPos)) != -1)
			{
				var peekChar = content.Substring(pos+1, 1);
				var isComment = peekChar == "*";
				if (isComment)
				{
					var endPos = content.IndexOf("*@", pos);
					if (endPos == -1)
						throw new InvalidDataException("Unterminated Comment at charIndex: " + pos);
					lastPos = endPos + 2;
					continue;
				}

				var contentBlock = content.Substring(lastPos, pos - lastPos);

				var startPos = pos;
				pos++; //@

				var statementExpr = content.GetNextStatementExpr(ref pos);
				if (statementExpr != null)
				{
					contentBlock = contentBlock.TrimLineIfOnlyHasWhitespace();
					sb.Append(contentBlock);

					if (statementExpr is MethodStatementExprBlock)
						sb.Append(' '); //ensure a spacer between method blocks

					statementExpr.Prepare(allStatements);
					allStatements.Add(statementExpr);
					var placeholder = "@" + TemplateExtensions.StatementPlaceholderChar + allStatements.Count;
					sb.Append(placeholder);
					lastPos = pos;
				}
				else
				{
					sb.Append(contentBlock);

					sb.Append('@');
					lastPos = startPos + 1;
				}
			}

			if (lastPos != content.Length - 1)
			{
				var lastBlock = lastPos == 0 ? content : content.Substring(lastPos);
				sb.Append(lastBlock);
			}

			return allStatements.Count > initialCount ? sb.ToString() : content;
		}

		protected void WriteStatement(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			if (IsNested)
			{
				//Write Markdown
				WriteInternal(instance, textWriter, scopeArgs);
			}
			else
			{
				//Buffer Markdown output before converting and writing HTML
				var sb = new StringBuilder();
				using (var sw = new StringWriter(sb))
				{
					WriteInternal(instance, sw, scopeArgs);
				}

				var markdown = sb.ToString();
				var html = Transform(markdown);
				textWriter.Write(html);
			}
		}
	}

	public class DirectiveBlock : StatementExprBlock
	{
		public Type BaseType { get; set; }

		public Type[] GenericArgs { get; set; }

		public Dictionary<string, Type> Helpers { get; set; }

		protected Dictionary<string, Func<object, object>> VarDeclarations { get; set; }

		public Type GetType(string typeName)
		{
			var type = AssemblyUtils.FindType(typeName);
			if (type == null)
				throw new TypeLoadException("Could not load type: " + typeName);
			
			return type;
		}

		public DirectiveBlock(string directive, string line)
			: base(directive, null)
		{
			if (directive == null)
				throw new ArgumentNullException("directive");
			if (line == null)
				throw new ArgumentNullException("line");

			directive = directive.ToLower();
			line = line.Trim();

			if (directive == "model")
			{
				this.BaseType = typeof(MarkdownViewBase<>);
				this.GenericArgs = new[] { GetType(line) };
			}
			else if (directive == "inherits")
			{
				var parts = line.Split(new[] { '<', '>' })
					.Where(x => !x.IsNullOrEmpty()).ToArray();

				var isGenericType = parts.Length >= 2;

				this.BaseType = isGenericType ? GetType(parts[0] + "`1") : GetType(parts[0]);

				if (isGenericType)
				{
					this.GenericArgs = new[] { GetType(parts[1]) };
				}
			}
			else if (directive == "usehelper")
			{
				var helpers = line.Split(',');
				this.Helpers = new Dictionary<string, Type>();
				
				foreach (var helper in helpers)
				{
					var parts = helper.Split(':');
					if (parts.Length != 2)
						throw new InvalidDataException(
							"Invalid usehelper directive, should be 'TagName: Helper.Namespace.And.Type'");

					var tagName = parts[0].Trim();
					var typeName = parts[1].Trim();

					var helperType = GetType(typeName);
					if (helperType == null)
						throw new InvalidDataException("Unable to resolve helper type: " + typeName);

					this.Helpers[tagName] = helperType;
				}
			}
		}

		protected override void OnFirstRun()
		{
			base.OnFirstRun();

			if (this.BaseType != null)
				Page.ExecutionContext.BaseType = this.BaseType;

			Page.ExecutionContext.GenericArgs = this.GenericArgs;

			if (this.Helpers != null)
			{
				foreach (var helper in this.Helpers)
				{
					Page.ExecutionContext.TypeProperties[helper.Key] = helper.Value;
				}
			}
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs) {}
	}


	public class ForEachStatementExprBlock : StatementExprBlock
	{
		public ForEachStatementExprBlock(string condition, string statement)
			: base(condition, statement)
		{
			Prepare();
		}

		public string EnumeratorName { get; set; }
		public string MemberExpr { get; set; }
		public string MemberVarName { get; set; }

		private void Prepare()
		{
			var parts = Condition.SplitOnWhiteSpace();
			if (parts.Length < 3)
				throw new InvalidDataException("Invalid foreach condition: " + Condition);

			var i = parts[0] == "var" ? 1 : 0;
			this.EnumeratorName = parts[i++];
			if (parts[i++] != "in")
				throw new InvalidDataException("Invalid foreach 'in' condition: " + Condition);

			this.MemberExpr = parts[i++];
			this.MemberVarName = this.MemberExpr.GetVarName();
		}

		private object GetModel(Dictionary<string, object> scopeArgs)
		{
			object model;
			if (!scopeArgs.TryGetValue(this.MemberVarName, out model))
				throw new ArgumentException(this.MemberVarName + " does not exist");

			return model;
		}

		private IEnumerable GetMemberExprEnumerator(object model)
		{
			var memberExprEnumerator = getMemberFn(model) as IEnumerable;
			if (memberExprEnumerator == null)
				throw new ArgumentException(this.MemberExpr + " is not an IEnumerable");
			return memberExprEnumerator;
		}

		private Func<object, object> getMemberFn;
		protected override void OnFirstRun()
		{
			base.OnFirstRun(false);
			var model = GetModel(ScopeArgs);

			getMemberFn = DataBinder.Compile(model.GetType(), MemberExpr);
			var memberExprEnumerator = GetMemberExprEnumerator(model);

			foreach (var item in memberExprEnumerator)
			{
				ScopeArgs[this.EnumeratorName] = item;
				foreach (var templateBlock in ChildBlocks)
				{
					templateBlock.DoFirstRun(PageContext);
				}
			}
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			var model = GetModel(scopeArgs);
			var memberExprEnumerator = GetMemberExprEnumerator(model);

			if (IsNested)
			{
				//Write Markdown
				foreach (var item in memberExprEnumerator)
				{
					scopeArgs[this.EnumeratorName] = item;
					base.Write(instance, textWriter, scopeArgs);
				}
			}
			else
			{
				//Buffer Markdown output before converting and writing HTML
				var sb = new StringBuilder();
				using (var sw = new StringWriter(sb))
				{
					foreach (var item in memberExprEnumerator)
					{
						scopeArgs[this.EnumeratorName] = item;
						base.Write(instance, sw, scopeArgs);
					}
				}

				var markdown = sb.ToString();
				var renderedMarkup = Transform(markdown);
				textWriter.Write(renderedMarkup);
			}
		}
	}

	public abstract class EvalExprStatementBase : StatementExprBlock
	{
		protected EvalExprStatementBase(string condition, string statement)
			: base(condition, statement)
		{
		}

		protected Type ReturnType = typeof(string);
		private string[] paramNames;
		protected string CodeGenMethodName { get; set; }

		public string[] GetParamNames(Dictionary<string, object> scopeArgs)
		{
			return this.paramNames ?? (this.paramNames = scopeArgs.Keys.ToArray());
		}

		protected override void OnFirstRun()
		{
			base.OnFirstRun();

			CodeGenMethodName = "EvalExpr_" + this.Id;

			var exprParams = GetExprParams();
			var evalItem = new EvaluatorItem(ReturnType, CodeGenMethodName, Condition, exprParams);

			AddEvalItem(evalItem);
		}

		protected Dictionary<string, Type> GetExprParams()
		{
			var exprParams = new Dictionary<string, Type>();
			paramNames = GetParamNames(ScopeArgs);
			var paramValues = GetParamValues(ScopeArgs);
			for (var i = 0; i < paramNames.Length; i++)
			{
				var paramName = paramNames[i];
				var paramValue = paramValues[i];

				exprParams[paramName] = paramValue.GetType();
			}
			return exprParams;
		}

		protected List<object> GetParamValues(IDictionary<string, object> scopeArgs)
		{
			var results = new List<object>();
			foreach (var paramName in paramNames)
			{
				object paramValue;
				if (!scopeArgs.TryGetValue(paramName, out paramValue))
					throw new ArgumentException("Unresolved param " + paramName + " in " + Condition);

				results.Add(paramValue);
			}
			return results;
		}

		public T Evaluate<T>(Dictionary<string, object> scopeArgs)
		{
			var paramValues = GetParamValues(scopeArgs);
			return (T)Evaluator.Evaluate(CodeGenMethodName, paramValues.ToArray());
		}
	}

	public class VarStatementExprBlock : EvalExprStatementBase
	{
		private string varName;
		private string memberExpr;

		public VarStatementExprBlock(string directive, string line)
			: base(line, null)
		{
			if (directive != "var")
				throw new ArgumentException("Expected 'var' got: " + directive);

			this.ReturnType = typeof(object);
		}
		
		protected override void OnFirstRun()
		{
			if (varName != null)
				return;

			var declaration = Condition.TrimEnd().TrimEnd(';');

			var parts = declaration.Split('=');
			if (parts.Length != 2)
				throw new InvalidDataException(
					"Invalid var declaration, should be '@var varName = {MemberExpression} [, {VarDeclaration}]' was: " + declaration);

			varName = parts[0].Trim();
			memberExpr = parts[1].Trim();

			this.Condition = memberExpr;

			const string methodName = "resolveVarType";
			var exprParams = GetExprParams();
			var evaluator = new Evaluator(ReturnType, Condition, methodName, exprParams);
			var result = evaluator.Evaluate(methodName, GetParamValues(ScopeArgs).ToArray());
			ScopeArgs[varName] = result; 
			if (result != null)
				this.ReturnType = result.GetType();

			base.OnFirstRun();
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			//Resolve and add to ScopeArgs
			var resultCondition = Evaluate<object>(scopeArgs);
			scopeArgs[varName] = resultCondition;
		}
	}

	public class IfStatementExprBlock : EvalExprStatementBase
	{
		public IfStatementExprBlock(string condition, string statement)
			: base(condition, statement)
		{
			this.ReturnType = typeof(bool);
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			var resultCondition = Evaluate<bool>(scopeArgs);
			if (!resultCondition) return;

			WriteStatement(instance, textWriter, scopeArgs);
		}
	}

	public class MethodStatementExprBlock : EvalExprStatementBase
	{
		public MethodStatementExprBlock(string methodExpr, string condition, string statement)
			: base(condition, statement)
		{
			this.methodExpr = methodExpr;
		}

		private readonly string methodExpr;
		protected override void OnFirstRun()
		{
			Prepare(Page);
			base.OnFirstRun();
		}

		private void Prepare(MarkdownPage markdownPage)
		{
			var rawMethodExpr = methodExpr.Replace("Html.", "");
			this.WriteRawHtml = rawMethodExpr == "Raw";

			var parts = methodExpr.Split('.');
			if (parts.Length > 2)
				throw new ArgumentException("Unable to resolve method: " + methodExpr);

			var usesBaseType = parts.Length == 1;
			var typePropertyName = parts[0];
			var methodName = usesBaseType ? parts[0] : parts[1];

			Type type = null;
			if (typePropertyName == "Html")
			{
				type = Common.ReflectionExtensions.IsGenericType(markdownPage.ExecutionContext.BaseType)
				       ? typeof (HtmlHelper<>)
				       : typeof (HtmlHelper);
			}
			if (type == null)
			{
				type = usesBaseType
					? markdownPage.ExecutionContext.BaseType
					: markdownPage.Markdown.MarkdownGlobalHelpers.TryGetValue(typePropertyName, out type) ? type : null;
			}

			if (type == null)
				throw new InvalidDataException(string.Format(
					"Unable to resolve type '{0}'. Check type exists in Config.MarkdownBaseType or Page.Markdown.MarkdownGlobalHelpers",
					typePropertyName));

			var mi = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
			if (mi == null)
			{
				mi = HtmlHelper.GetMethod(methodName);
				if (mi == null) 
					throw new ArgumentException("Unable to resolve method '" + methodExpr + "' on type " + type.Name);
			}

			base.ReturnType = mi.ReturnType;

			var isMemberExpr = Condition.IndexOf('(') != -1;
			if (!isMemberExpr || this.WriteRawHtml)
			{
				base.Condition = methodExpr + "(" + Condition + ")";
			}
		}

		public override void Write(MarkdownViewBase instance, TextWriter textWriter, Dictionary<string, object> scopeArgs)
		{
			var paramValues = GetParamValues(scopeArgs);
			var result = Evaluator.Evaluate(instance, CodeGenMethodName, paramValues.ToArray());
			if (result == null) return;

			string strResult;

			var mvcString = result as MvcHtmlString;
			if (mvcString != null)
			{
				WriteRawHtml = true;
				strResult = mvcString.ToHtmlString();
			}
			else
			{
				strResult = result as string ?? Convert.ToString(result);
			}

			if (!WriteRawHtml)
				strResult = HttpUtility.HtmlEncode(strResult);

			textWriter.Write(strResult);
		}
	}
}