﻿using System.Net.Mime;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;

namespace ServiceStack.Mvc
{
	public abstract class ControllerBase<T> : ControllerBase
		where T : class, IAuthSession, new()
	{
		private T userSession;
		protected T UserSession
		{
			get
			{
				if (userSession != null) return userSession;
				if (SessionKey != null)
					userSession = this.Cache.Get<T>(SessionKey);
				else
					SessionFeature.CreateSessionIds();

				var unAuthorizedSession = new T();
				return userSession ?? (userSession = unAuthorizedSession);
			}
		}

		public override IAuthSession AuthSession
		{
			get { return UserSession; }
		}

		public override void ClearSession()
		{
			userSession = null;
			this.Cache.Remove(SessionKey);
		}
	}

	[ExecuteServiceStackFilters]
	public abstract class ControllerBase : Controller
	{
		public virtual string LoginRedirectUrl
		{
			get { return "/login?redirect={0}"; }
		}

		public virtual ActionResult AuthorizationErrorResult
		{
			get
			{
				return new RedirectToRouteResult(new RouteValueDictionary(new {
					controller = "Error",
					action = "Unauthorized"
				}));
			}
		}

		public ICacheClient Cache { get; set; }
		public ISessionFactory SessionFactory { get; set; }

		private ISession session;
		public new ISession Session
		{
			get
			{
				return session ?? (session = SessionFactory.GetOrCreateSession());
			}
		}

		public abstract IAuthSession AuthSession { get; }

		protected string SessionKey
		{
			get
			{
				var sessionId = SessionFeature.GetSessionId();
				return sessionId == null ? null : SessionFeature.GetSessionKey(sessionId);
			}
		}

		protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
		{
			return new ServiceStackJsonResult {
				Data = data,
				ContentType = contentType,
				ContentEncoding = contentEncoding
			};
		}

		public virtual void ClearSession()
		{
			this.Cache.Remove(SessionKey);
		}
		
		protected override void HandleUnknownAction(string actionName)
		{
			// If controller is ErrorController dont 'nest' exceptions
			//if (this.GetType() != typeof(ErrorController))
			//    this.InvokeHttp404(HttpContext);
		}
	}

	public class ServiceStackJsonResult : JsonResult
	{
		public override void ExecuteResult(ControllerContext context)
		{
			var response = context.HttpContext.Response;
			response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

			if (ContentEncoding != null)
			{
				response.ContentEncoding = ContentEncoding;
			}

			if (Data != null)
			{
				response.Write(JsonSerializer.SerializeToString(Data));
			}
		}
	}
}