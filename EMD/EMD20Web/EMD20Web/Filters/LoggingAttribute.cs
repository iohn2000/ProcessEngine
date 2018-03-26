using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;

namespace Kapsch.IS.EMD.EMD20Web.Filters
{
    public class LoggingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IISLogger logger = ISLogger.GetLogger("MVCLogger");
            logger.Debug("URL: " + filterContext.HttpContext.Request.Url.ToString());

            string action = filterContext.RouteData.Values["action"].ToString();
            string controller = filterContext.RouteData.Values["controller"].ToString();
            string user = (filterContext?.HttpContext?.User?.Identity?.Name) ?? "not set";

            logger.Debug("action: " + action);
            logger.Debug("controller: " + controller);
            logger.Debug("user: " + user);

            try
            {
                Controllers.BaseController currentControler = (Controllers.BaseController)filterContext.Controller;
                currentControler.ActionName = action;
                currentControler.ControllerName = controller;
            }
            catch (Exception ex)
            {
                //ratlos in die Gegend schauen.
                logger.Error(ex, filterContext.RequestContext.HttpContext);
            }


            foreach (string key in filterContext.HttpContext.Request.QueryString)
            {
                logger.Debug("Key: " + key + " Value: " + filterContext.HttpContext.Request.QueryString[key]);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}