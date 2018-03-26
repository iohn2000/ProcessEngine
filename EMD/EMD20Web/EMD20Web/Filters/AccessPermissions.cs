using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kapsch.IS.Util.Logging;


namespace Kapsch.IS.EMD.EMD20Web.Filters
{
    public class AccessPermissionsAttribute : ActionFilterAttribute
    {
        //To use call on controller method: //[Filters.AccessPermissions(AccessPermission="ONBOARDING,  EMDADMIN  ,  PRIME")]
        public string AccessPermission { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string userName = filterContext.HttpContext.User.Identity.Name;
            string actionName = filterContext.ActionDescriptor.ActionName;
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            Boolean hasPermission = false;
            hasPermission = true;

            if (!string.IsNullOrEmpty(AccessPermission))
            {
                List<string> ListAccessPermission = new List<string>();
                ListAccessPermission = AccessPermission.Split(',').ToList();
                ListAccessPermission = (from item in ListAccessPermission select item.Trim()).ToList();
                foreach (string perm in ListAccessPermission)
                {
                    //Check Permissions in DB
                    string x = userName + perm;
                }
            }

            if (!hasPermission)
            { 
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Home",
                        action = "AccessDenied"
                    }));
            }

            base.OnActionExecuting(filterContext);
        }
    }
}