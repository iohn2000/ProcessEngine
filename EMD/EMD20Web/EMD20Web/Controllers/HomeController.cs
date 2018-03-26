using Kapsch.IS.EMD.EMD20Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Home")]
    public class HomeController : BaseController
    {

        [Route()]
        [Route("~/", Name = "default")]
        public ActionResult Home()
        {
            return RedirectToAction("Index");
        }

        [Route("Index")]
        public ActionResult Index()
        {
            PhonebookModel phonebookModel = new PhonebookModel();

            StringBuilder stringBuilder = new StringBuilder(ConfigurationManager.AppSettings["EMD20Web.PhoneBookWebpath"]);

            if (System.Web.HttpContext.Current.Request.QueryString.ToString() != null)
            {
                stringBuilder.Append("?");
                stringBuilder.Append(System.Web.HttpContext.Current.Request.QueryString.ToString());
            }
            phonebookModel.Url = stringBuilder.ToString();

            return View(phonebookModel);
        }

        [Route("AccessDenied")]
        public ActionResult AccessDenied()
        {
            ViewBag.Message = "Access Denied.";

            return View();
        }

        [Route("Blank")]
        public ActionResult Blank()
        {
            return View();
        }
    }
}