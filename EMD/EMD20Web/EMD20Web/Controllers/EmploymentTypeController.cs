using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("EmploymentType")]
    public class EmploymentTypeController : BaseController
    {
        // GET: EmploymentType
        public ActionResult Index()
        {
            return View();
        }

        [Route("ReadForSelect")]
        public ActionResult ReadForSelect(string emty_guid = null)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                EmploymentTypeHandler handler = new EmploymentTypeHandler();
                List<EMDEmploymentType> emdEntities = handler.GetObjects<EMDEmploymentType, EmploymentType>(null).Cast<EMDEmploymentType>().ToList();


                emdEntities.ForEach(entity =>
                {
                    // bool isExternal = ((EnumEmploymentTypeCategory)((EMDEmploymentType)entity).ETC_ID) == EnumEmploymentTypeCategory.External;
                    if (string.IsNullOrEmpty(emty_guid) || emty_guid != entity.Guid)
                    {
                        // hide EmploymentType Inactive, because the employment has new properties leaveFrom, leaveTo
                        if (entity.ET_ID != 13)
                        {
                            keyValuePairs.Add(new TextValueModel(entity.Name, entity.Guid, new { EnumEmploymentTypeCategory = entity.ETC_ID, MustHaveSponsor = entity.MustHaveSponsor }));
                        }
                    }
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading employmentTypes";
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