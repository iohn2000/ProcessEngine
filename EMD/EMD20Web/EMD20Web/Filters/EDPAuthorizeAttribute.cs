using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;

using Kapsch.IS.Util.Logging;

namespace Kapsch.IS.EMD.EMD20Web.Filters
{
    public class EDPAuthorizeAttribute : AuthorizeAttribute
    {
        private const string IS_AUTHORIZED = "isAuthorized";

        public string RedirectUrl = "~/Error/Unauthorized";

        public static bool IsAdminConfig(string username)
        {
            bool isEDPAdminFromWebConfig = false;
            if (ConfigurationManager.AppSettings["EMD20Web.Admins"] != null && ConfigurationManager.AppSettings["EMD20Web.Admins"] != String.Empty)
            {
                List<string> listAdmins = ConfigurationManager.AppSettings["EMD20Web.Admins"].ToLower().Split(',').ToList();
                foreach (string entry in listAdmins)
                {
                    if (entry == username.ToLower())
                    {
                        isEDPAdminFromWebConfig = true;
                        break;
                    }
                }
            }
            return isEDPAdminFromWebConfig;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                // no user is authenticated => no need to go any further
                return false;
            }

            bool isAuthorized = base.AuthorizeCore(httpContext);
            //bool isAuthorized = true;
            string username = httpContext.User.Identity.Name;
            int slashPos = username.IndexOf("\\");
            if (slashPos > -1)
            {
                username = username.Substring(slashPos + 1);
            }
            bool isEDPAdminFromWebConfig = IsAdminConfig(username);

            if (isEDPAdminFromWebConfig)
            {
                //if (httpContext.Request.Form["UserId"] != null)
                //{
                //    if (!String.IsNullOrWhiteSpace(httpContext.Request.Form["UserId"]))
                //    {
                //        username = httpContext.Request.Form["UserId"].ToString().Trim();
                        isAuthorized = true;
                //    }
                //}
            }
            else
            {
                isAuthorized = IsInRole(username, this.Roles, "");
            }
            httpContext.Items.Add(IS_AUTHORIZED, isAuthorized);

            return isAuthorized;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var isAuthorized = filterContext.HttpContext.Items[IS_AUTHORIZED] != null
                ? Convert.ToBoolean(filterContext.HttpContext.Items[IS_AUTHORIZED])
                : false;

            if (!isAuthorized && filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                string Controller = HttpContext.Current.Request.RequestContext.RouteData.Values["Controller"].ToString();
                string Action = HttpContext.Current.Request.RequestContext.RouteData.Values["Action"].ToString();
                string RedirectUrlWithParams = RedirectUrl + "?C=" + Controller + "&A=" + Action;
                filterContext.RequestContext.HttpContext.Response.Redirect(RedirectUrlWithParams);
            }
        }

        public static bool IsInRole(string userId, string permission, string ente_guid = "")
        {
            string controller = HttpContext.Current.Request.RequestContext.RouteData.Values["Controller"].ToString();
            string action = HttpContext.Current.Request.RequestContext.RouteData.Values["Action"].ToString();

            EDP.Core.Framework.EDPSecurityHandler securityHandler = new EDP.Core.Framework.EDPSecurityHandler();

            if (IsAdminConfig(userId))
            {
                return true;
            }

            return securityHandler.CheckForAccessPermission(permission, userId, ente_guid);

            //Get Company of User
            //return IsInRole(userId, permission, controller, action);
        }

        public static bool IsInRole(string username, string roles, string controller, string action,  string ente_guid = "")
        {
            if (IsAdminConfig(username))
            {
                return true;
            }

            if (roles.ToLower() == "prime")
                return true;
            else
                return false;
        }



    }
}