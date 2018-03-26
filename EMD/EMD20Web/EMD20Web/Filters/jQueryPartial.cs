using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Kapsch.IS.EMD.EMD20Web.Controllers;

namespace Kapsch.IS.EMD.EMD20Web.Filters
{
    public class jQueryPartial : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Verify if a XMLHttpRequest is fired.  
            // This can be done by checking the X-Requested-With  
            // HTTP header.  
            BaseController myController = filterContext.Controller as BaseController;
            if (myController != null)
            {
                if (filterContext.HttpContext.Request.Headers["X-Requested-With"] != null
                    && filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    myController.IsAjaxRequest = true;
                }
                else
                {
                    myController.IsAjaxRequest = false;
                }
            }
        }
    }
}