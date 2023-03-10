using System;
using System.Web;

namespace NServiceKit.ServiceInterface
{
    //adapted from  https://github.com/Haacked/CodeHaacks/blob/master/src/AspNetHaack/SuppressFormsAuthenticationRedirectModule.cs
    /// <summary>
    /// This class interecepts 401 requests and changes them to 402 errors.   When this happens the FormAuthentication module
    /// will no longer hijack it and redirect back to login because it is a 402 error, not a 401.
    /// When the request ends, this class sets the status code back to 401 and everything works as it should.
    /// 
    /// PathToSupress is the path inside your website where the above swap should happen.
    /// 
    /// If you can build for .net 4.5, you do not have to do this swap. You can take advantage of a new flag (SuppressFormsAuthenticationRedirect)
    /// that tells the FormAuthenticationModule to not redirect, which also means you will not need the EndRequest code.
    /// </summary>
    public class SuppressFormsAuthenticationRedirectModule : IHttpModule
    {
        /// <summary>Gets or sets the path to supress.</summary>
        ///
        /// <value>The path to supress.</value>
        public static string PathToSupress { get; set; }

        /// <summary>Initializes a module and prepares it to handle requests.</summary>
        ///
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.
        /// </param>
        public virtual void Init(HttpApplication context)
        {
            if (string.IsNullOrEmpty(PathToSupress))
                PathToSupress = "/api";
            context.PostReleaseRequestState += OnPostReleaseRequestState;
            context.EndRequest += OnEndRequest;  //not needed if .net 4.5 
        }

        //not needed if .net 4.5 
        void OnEndRequest(object source, EventArgs e)
        {
            var context = (HttpApplication)source;
            if (context.Response.StatusCode == 402 && context.Request.Url.PathAndQuery.StartsWith(PathToSupress))
                context.Response.StatusCode = 401;
        }

        /// <summary>Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.</summary>
        public void Dispose()
        {

        }

        
        private void OnPostReleaseRequestState(object source, EventArgs args)
        {
          //System.Web.Security.FormsAuthenticationModule  //swap error code to 402 ...then put it back on endrequest?
            var context = (HttpApplication)source;
            if (context.Response.StatusCode == 401 && context.Request.Url.PathAndQuery.StartsWith(PathToSupress))
                context.Response.StatusCode = 402;                              //.net 4.0 solution.
                //context.Response.SuppressFormsAuthenticationRedirect = true;  //.net 4.5 solution.
        }
    }
}
