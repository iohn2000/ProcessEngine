using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System.Reflection;

using Kapsch.IS.EMD.EMD20Web.HelperExtensions;

namespace Kapsch.IS.EMD.EMD20Web.Filters
{
    
    public class EDPHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Exception ex = filterContext.Exception;

            IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            logger.Error(ex, new HttpContextWrapper(HttpContext.Current));
            
            //filterContext.ExceptionHandled = true;
            var model = new HandleErrorInfo(filterContext.Exception, "Error", "Index");
            
            //filterContext.Result = new ViewResult()
            //{

            //    ViewName = "Index",
            //    ViewData = new ViewDataDictionary(model)
            //};

            //filterContext.Result = new ViewResult { ViewName = "Exception", ViewData = new ViewDataDictionary(new CmsExceptionViewData(filterContext.Exception, action, controllerName, errorMessage)) };

            Models.ErrorModel errModel = new Models.ErrorModel(ex);

            //filterContext.Controller.ViewBag.error = errString;

            //filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            //     {{"controller", "Error"}, {"action", "Index"}, {"errorMessage", ex.Message}});



            //filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            //     {{"controller", "Error"}, {"action", "ErrorDetails"}, {"errorModel", errModel}});

            if (filterContext.ExceptionHandled)
            {
                return;
            }

            HttpContext.Current.Session["ErrorMod"] = errModel;

            filterContext.Result = new ViewResult
            {
                //ViewName = "~/Views/Shared/Error.aspx"
                //ViewName = "~/ErrorHandling/ErrorDetail.cshtml"
                ViewName = "../Error/ErrorDetails"
                //TempData = errString
            };
            filterContext.ExceptionHandled = true;
        }
    }
}