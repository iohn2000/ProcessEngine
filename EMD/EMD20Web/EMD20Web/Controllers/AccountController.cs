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
using Kapsch.IS.Util.Logging;
using System.Reflection;

using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EDP.Core.Logic;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Account")]
    public class AccountController : BaseController
    {
        internal new IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [Route("Read")]
        [HandleError()]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            //List<EMDAccount> emdAccounts = EDP.Core.Logic.Manager.AccountManager.GetAccounts();

            EnterpriseHandler eh = new EnterpriseHandler();
            var accountItems = (from acc in EDP.Core.Logic.Manager.AccountManager.GetAccounts() join ente in eh.GetObjects<EMDEnterprise, Enterprise>() on acc.E_Guid equals ente.Guid select new { acc, ente }).ToList();

            //EMD_Entities db_Context = new EMD_Entities();
            //var accounts = (from acc in db_Context.Account join ente in db_Context.Enterprise on acc.E_Guid equals ente.Guid where acc.ValidFrom < DateTime.Now && acc.ValidTo > DateTime.Now && acc.ActiveFrom < DateTime.Now && acc.ActiveTo > DateTime.Now select new { ente.NameShort,acc.Guid, acc.Name, acc.KstID, acc.ActiveFrom, acc.ActiveTo }).ToList();

            //List<AccountModel> accountModelList = AccountModel.Map(emdAccounts);

            List<AccountModel> accountModelList = new List<AccountModel>();

            AccountModel dummySecurityModel = AccountModel.Initialize(new EMDAccount());
            dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            //foreach (var item in accounts)
            foreach (var item in accountItems)
            {
                AccountModel model = AccountModel.Initialize((EMDAccount)item.acc, (EMDEnterprise)item.ente);
                model.CanManage = dummySecurityModel.CanManage;
                model.CanView = dummySecurityModel.CanView;

                accountModelList.Add(model);
            }

            DataSourceResult myresult = accountModelList.ToDataSourceResult(request);

            //DataSourceResult myresult = emdAccounts.ToDataSourceResult(request, ModelState);

            //List<EMDAccount> resultEmdAccounts = myresult.Data as List<EMDAccount>;
            //List<AccountModel> accountModelList = AccountModel.Map(resultEmdAccounts);

            //myresult.Data = accountModelList;

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        private List<TextValueModel> GetAccountTvList(string text, int maxItemsResult = 0)
        {
            PersonHandler persHandler = new PersonHandler();
            EmploymentHandler emplHandler = new EmploymentHandler();
            List<TextValueModel> listTextValueModels = new List<TextValueModel>();

            AccountHandler handler = new AccountHandler();
            List<IEMDObject<EMDAccount>> emdEntities = handler.GetObjects<EMDAccount, Account>(null);


            emdEntities.ForEach(entity =>
            {
                listTextValueModels.Add(new TextValueModel(((EMDAccount)entity).KstID + " - " + ((EMDAccount)entity).Name, entity.Guid));
            });

            if (text != null && text != "%")
            {
                listTextValueModels = (from item in listTextValueModels where item.Text.ToLower().Contains(text.ToLower()) select item).ToList();
            }

            listTextValueModels = listTextValueModels.OrderBy(pair => pair.Text).ToList();

            if (maxItemsResult > 0)
            {
                listTextValueModels = listTextValueModels.Take(maxItemsResult).ToList();
            }



            return listTextValueModels.OrderBy(item => item.Text).ToList();
        }


        [Route("ReadForSelect")]
        public JsonResult ReadForSelect(string text = "%")
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                keyValuePairs = GetAccountTvList(text);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading accounts";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadForSelectDs")]
        [HandleError()]
        public ActionResult ReadForSelectDs([DataSourceRequest] DataSourceRequest request, string text)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                keyValuePairs = GetAccountTvList(text);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading accounts";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }


        //http://localhost:8021/Account/ReadForSelectForEnterprise?ente_guid=
        [Route("ReadForSelectForEnterprise")]
        //[Route("ReadForSelectForEnterprise/{ente_guid}")]
        public JsonResult ReadForSelectForEnterprise(string ente_guid, string text = "%")
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                AccountHandler handler = new AccountHandler();
                List<IEMDObject<EMDAccount>> emdEntities = handler.GetObjects<EMDAccount, Account>(string.Format("E_Guid = \"{0}\"", ente_guid));

                emdEntities.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(string.Format("{0} - {1}", ((EMDAccount)entity).KstID, ((EMDAccount)entity).Name), entity.Guid, new { E_ID = ((EMDAccount)entity).E_ID }));
                });

                if (text != "%")
                {
                    keyValuePairs = (from item in keyValuePairs where item.Text.ToLower().Contains(text.ToLower()) select item).ToList();
                }

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading roles";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("CleanupAccounts")]
        public ActionResult CleanupAccounts()
        {
            ErrorModel errorModel = null;
            bool success = false;

            CoreTransaction transaction = new CoreTransaction();

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.OrgUnitManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                OrgUnitModel orgUnitModel = new OrgUnitModel();
                orgUnitModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                if (!orgUnitModel.CanManage)
                    throw new Exception(SecurityHelper.NoPermissionText);

                AccountManager manager = new AccountManager(transaction, this.UserName, MODIFY_COMMENT);

                transaction.dbContext.Configuration.AutoDetectChangesEnabled = false;
                transaction.dbContext.Configuration.ValidateOnSaveEnabled = false;
                transaction.Begin();

                manager.CleanupEmploymentAccountRelations();
                transaction.Commit();

                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success });
        }
    }
}