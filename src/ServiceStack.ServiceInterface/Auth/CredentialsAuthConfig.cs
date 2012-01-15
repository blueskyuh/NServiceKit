using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.FluentValidation;

namespace ServiceStack.ServiceInterface.Auth
{
	public class CredentialsAuthConfig : AuthConfig
	{
		class CredentialsAuthValidator : AbstractValidator<Auth>
		{
			public CredentialsAuthValidator()
			{
				RuleFor(x => x.UserName).NotEmpty();
				RuleFor(x => x.Password).NotEmpty();
			}
		}

		public static string Name = AuthService.CredentialsProvider;
		public static string Realm = "/auth/" + AuthService.CredentialsProvider;

		public CredentialsAuthConfig()
		{
			this.Provider = Name;
			this.AuthRealm = Realm;
		}

		public CredentialsAuthConfig(IResourceManager appSettings, string authRealm, string oAuthProvider)
			: base(appSettings, authRealm, oAuthProvider) { }

		public CredentialsAuthConfig(IResourceManager appSettings)
			: base(appSettings, Realm, Name) { }

		public virtual bool TryAuthenticate(IServiceBase authService, string userName, string password)
		{
			var authRepo = authService.TryResolve<IUserAuthRepository>();
			if (authRepo == null)
			{
				Log.WarnFormat("Tried to authenticate without a registered IUserAuthRepository");
				return false;
			}

			var session = authService.GetSession();
			string useUserName = null;
			if (authRepo.TryAuthenticate(userName, password, out useUserName))
			{
				session.UserName = userName;
				authRepo.LoadUserAuth(session, null);

				OnAuthenticatedCredentials(authService, session, userName);
				return true;
			}
			return false;
		}

		public override bool IsAuthorized(IAuthSession session, IOAuthTokens tokens)
		{
			return !session.UserName.IsNullOrEmpty();
		}

		public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
		{
			new CredentialsAuthValidator().ValidateAndThrow(request);
			return Authenticate(authService, session, request.UserName, request.Password);
		}

		protected object Authenticate(IServiceBase authService, IAuthSession session, string userName, string password)
		{
			if (TryAuthenticate(authService, userName, password))
			{
				if (session.UserName == null)
					session.UserName = userName;

				authService.SaveSession(session);

				return new AuthResponse {
					UserName = userName,
					SessionId = session.Id,
				};
			}

			throw HttpError.Unauthorized("Invalid UserName or Password");
		}

		protected virtual void OnAuthenticatedCredentials(IServiceBase oAuthService, IAuthSession session, string userName)
		{
			var provider = oAuthService.TryResolve<IUserAuthRepository>();
			if (provider != null)
				provider.SaveUserAuth(session);

			OnSaveUserAuth(oAuthService, session.UserAuthId);
		}
	}
}