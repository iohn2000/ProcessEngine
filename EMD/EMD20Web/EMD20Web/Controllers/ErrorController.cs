using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Error")]
    public class ErrorController : BaseController
    {
        // GET: Error
        [Route()]
        public ActionResult Index()
        {
            Models.ErrorModel errModel = new Models.ErrorModel();
            errModel.ErrorMessage = ViewBag.error;
            return View("ErrorDetails");
        }

        [Route("ErrorDetails")]
        public ActionResult ErrorDetails()
        {
            return View("ErrorDetails");
        }

        [Route("ErrorList")]
        public ActionResult ErrorList()
        {
            return View();
        }

        //[Route("HandleError")]
        //public ActionResult HandleError(Models.ErrorModel errorModel)
        //{
        //    errorModel.ErrorMessage = ViewBag.error;
        //    //return errModel;
        //    return View("ErrorDetails",errorModel);
        //}

        [Route("Unauthorized")]
        public ActionResult Unauthorized()
        {
            Models.ErrorModel errModel = new Models.ErrorModel();
            errModel.ErrorMessage = ViewBag.error;
            //return errModel;
            return View("Unauthorized");
        }
        
    }
}

