using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("DistributionGroup")]
    public class DistributionGroupController : BaseController
    {
        [Route]
        // GET: DistributionGroup
        public ActionResult Index()
        {
            return View();
        }

        [Route("ReadForSelect")]
        public ActionResult ReadForSelect([DataSourceRequest]DataSourceRequest request)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                DistributionGroupHandler handler = new DistributionGroupHandler();
                List<IEMDObject<EMDDistributionGroup>> emdEntities = handler.GetObjects<EMDDistributionGroup, DistributiongroupType>();


                emdEntities.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDDistributionGroup)entity).Name, entity.Guid));
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }

            catch (Exception ex)
            {
                string errorMessage = "Error in reading distributiongroups";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }
    }
}