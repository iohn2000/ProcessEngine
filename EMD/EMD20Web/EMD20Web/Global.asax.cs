using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System.Reflection;
using System.IO;

namespace Kapsch.IS.EMD.EMD20Web
{
    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {

            ViewEngines.Engines.Clear();    //Removes WebForms View Engine
            ViewEngines.Engines.Add(new RazorViewEngine());

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            Server.ClearError();

            Models.ErrorModel errModel = new Models.ErrorModel(ex);

            HttpContext.Current.Session["ErrorMod"] = errModel;
            Response.Redirect("/Error/ErrorDetails");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        private void WriteReqeusts()
        {
            StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream);
            string requestFromPost = reader.ReadToEnd();



            try
            {
                // If you want it formated in some other way.
                string headers = String.Empty;
                foreach (var key in Request.Headers.AllKeys)
                    headers += key + "=" + Request.Headers[key] + Environment.NewLine;

                string path = @"E:\wwwroot\EMD20WebDEV\requests.txt";

                // This text is always added, making the file longer over time
                // if it is not deleted.
                using (StreamWriter sw = File.AppendText(path))
                {
                    //sw.WriteLine(string.Format("Header", HttpContext.Current.Request);
                    sw.WriteLine(headers);
                    sw.WriteLine(requestFromPost);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
