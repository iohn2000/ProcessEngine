using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EDP.Core.DB;


using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Kapsch.IS.EDP.Core.Logic.Interface;
using System.Text;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Costcenter")]
    public class CostcenterController : BaseController
    {
        internal new IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Route("Manage")]
        [Route("Manage/{isPartialView}")]
        [HandleError()]
        public ActionResult Manage(bool isPartialView = false)
        {
            AccountModel model = new AccountModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            if (isPartialView)
            {
                return PartialView("Manage", model);
            }
            else
            {
                return View("Manage", model);
            }
        }


        [HttpGet]
        [Route("Edit")]
        [Route("Add")]
        [Route("Add/{isPartialView}")]
        [Route("Edit/{guid}/{isPartialView}")]
        [Route("Edit/{guid}")]
        public ActionResult Edit(string guid, bool isPartialView = false)
        {
            AccountModel mappedModel = new AccountModel();
            mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            if (!mappedModel.CanManage)
                return GetNoPermissionView(isPartialView);

            try
            {
                if (!string.IsNullOrEmpty(guid))
                {
                    if (!mappedModel.IsAllowedObject(this.UserName, guid, true))
                        return GetNoPermissionView(isPartialView);

                    ViewBag.Titel = "Edit Costcenter";

                    EMDAccount emdAccount = Manager.AccountManager.Get(guid);
                    mappedModel = AccountModel.Initialize(emdAccount);


                    // fill groups
                    List<EMDGroup> availableGroups = Manager.GroupManager.GetGroups(emdAccount.E_Guid);
                    List<TextValueModel> availableGroupTextValues = new List<TextValueModel>();
                    availableGroups.ForEach(entity =>
                    {
                        availableGroupTextValues.Add(new TextValueModel(entity.Name, entity.Guid));
                    });
                    mappedModel.AvailableGroups = availableGroupTextValues;

                    List<EMDGroup> assignedGroups = Manager.GroupManager.GetAssignedCostCenterGroups(emdAccount.Guid);
                    List<TextValueModel> assignedGroupTextValues = new List<TextValueModel>();
                    assignedGroups.ForEach(entity =>
                    {
                        assignedGroupTextValues.Add(new TextValueModel(entity.Name, entity.Guid));
                    });
                    mappedModel.ConfiguredGroups = assignedGroupTextValues;

                    foreach (TextValueModel item in mappedModel.ConfiguredGroups)
                    {
                        var foundItem = (from a in mappedModel.AvailableGroups where a.Value == item.Value select a).FirstOrDefault();

                        if (foundItem != null)
                        {
                            mappedModel.AvailableGroups.Remove(foundItem);
                        }
                    }
                }
                else
                {
                    ViewBag.Titel = "Add new Costcenter";
                }



                mappedModel.ResponsibleSelection = new Models.Shared.SelectionViewModel()
                {
                    ReferencePropertyName = "Responsible",
                    ObjectLabel = "Responsible",
                    ObjectValue = mappedModel.Responsible,
                    ObjectText = new PersonManager().getFullDisplayNameWithUserIdAndPersNr(mappedModel.Responsible),
                    TargetControllerMethodName = "GetEmploymentList",
                    TargetControllerName = "Employment"
                };

                mappedModel.EnterpriseSelection = new Models.Shared.SelectionViewModel()
                {
                    IsDisabled = !string.IsNullOrEmpty(guid),
                    SelectionEvent = "costcenter.Events.OnDropDownlistSelectionSelect(this)",
                    ReferencePropertyName = "E_Guid",
                    ObjectLabel = "Enterprise",
                    ObjectValue = mappedModel.E_Guid,
                    ObjectText = new EnterpriseManager().Get(mappedModel.E_Guid)?.NameShort,
                    TargetControllerMethodName = "ReadForSelectDs",
                    TargetControllerName = "Enterprise"
                };
            }
            catch (Exception ex)
            {
                //TODO: write HelperMethod for generalizing this kind of handling

                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (isPartialView)
            {
                return PartialView("Edit", mappedModel);
            }
            else
            {
                return View("Edit", mappedModel);
            }
        }

        [Route("ReadAvailableGroups")]
        public ActionResult ReadAvailableGroups([DataSourceRequest]DataSourceRequest request, string guid_ente)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();

            List<EMDGroup> groups = Manager.GroupManager.GetGroups(guid_ente);

            groups.ForEach(entity =>
            {
                keyValuePairs.Add(new TextValueModel(entity.Name, entity.Guid));
            });

            DataSourceResult myresult = keyValuePairs.ToDataSourceResult(request);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        [HttpPost]
        [Route("Edit")]
        public ActionResult Edit(AccountModel accountModel)
        {
            Exception handledException = null;
            ViewBag.Title = "Edit User";
            string message = string.Empty;

            CoreTransaction transaction = new CoreTransaction();
            AccountGroupHandler accountGroupHandler = new AccountGroupHandler(transaction, this.PersonGuid);
            IAccountManager accountManager = Manager.AccountManager;
            // check if CostCenter ID is unique for an enterprise
            if (ModelState.IsValid && !accountManager.IsAccountIdAvailable(accountModel.Guid, accountModel.KstID, accountModel.E_Guid))
            {
                ModelState.AddModelError("KstID", string.Format("The CostcenterID {0} for the given enterprise already exists!", accountModel.KstID));
            }


            if (accountModel != null && ModelState.IsValid)
            {
                try
                {
                    transaction.Begin();
                    accountModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                    if (!accountModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }


                    accountManager.Transaction = transaction;
                    accountManager.Guid_ModifiedBy = this.PersonGuid;

                    if (!string.IsNullOrEmpty(accountModel.Guid))
                    {
                        if (!accountModel.IsAllowedObject(this.UserName, accountModel.Guid, true))
                        {
                            throw new Exception(SecurityHelper.NoPermissionText);
                        }
                        // update assigned Groups
                        List<EMDGroup> currentAssignedGroups = Manager.GroupManager.GetAssignedCostCenterGroups(accountModel.Guid);

                        EMDAccount emdAccount = accountManager.Get(accountModel.Guid);

                        emdAccount = AccountModel.Update(emdAccount, accountModel);

                        ViewBag.Title = "Edit Costcenter";
                        accountManager.Update(emdAccount);



                        // search for removed groups
                        foreach (EMDGroup currentAssignedGroup in currentAssignedGroups)
                        {
                            //var items = accountModel.ConfiguredGroups.Select(a => a.Value == currentAssignedGroup.Guid).ToList();
                            var items = (from a in accountModel.ConfiguredGroups where a.Value == currentAssignedGroup.Guid select a).ToList();

                            if (items.Count == 0)
                            {
                                List<EMDAccountGroup> relatedAccountGroups = new AccountGroupHandler().GetObjects<EMDAccountGroup, AccountGroup>("G_Guid = \"" + currentAssignedGroup.Guid + "\" && AC_Guid = \"" + accountModel.Guid + "\"").Cast<EMDAccountGroup>().ToList();
                                if (relatedAccountGroups.Count == 1)
                                {
                                    accountGroupHandler.DeleteObject(relatedAccountGroups[0]);
                                }
                            }
                        }

                        // search for newly added groups
                        foreach (TextValueModel configuredUiGroup in accountModel.ConfiguredGroups)
                        {
                            if (!currentAssignedGroups.Exists(a => a.Guid == configuredUiGroup.Value))
                            {
                                EMDAccountGroup emdAccountGroup = new EMDAccountGroup();

                                emdAccountGroup.AC_Guid = accountModel.Guid;
                                emdAccountGroup.AC_ID = accountModel.AC_ID;
                                emdAccountGroup.G_Guid = configuredUiGroup.Value;
                                emdAccountGroup.G_ID = -1;
                                emdAccountGroup.Key = string.Empty;

                                accountGroupHandler.CreateObject(emdAccountGroup);
                            }
                        }


                        message = "The Costcenter has been updated!";
                    }
                    else
                    {
                        SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                        if (!secUser.IsAllowedEnterprise(accountModel.CanManagePermissionString, accountModel.E_Guid))
                        {
                            throw new Exception(SecurityHelper.NoPermissionText);
                        }
                        EMDAccount emdAccount = AccountModel.Map(accountModel);
                        ViewBag.Title = "Add new Costcenter";

                        EMDAccount account = accountManager.Create(emdAccount);

                        // add to group
                        foreach (TextValueModel configuredUiGroup in accountModel.ConfiguredGroups)
                        {
                            EMDAccountGroup emdAccountGroup = new EMDAccountGroup();

                            emdAccountGroup.AC_Guid = account.Guid;
                            emdAccountGroup.AC_ID = -1;
                            emdAccountGroup.G_Guid = configuredUiGroup.Value;
                            emdAccountGroup.G_ID = -1;
                            emdAccountGroup.Key = string.Empty;

                            accountGroupHandler.CreateObject(emdAccountGroup);
                        }


                        message = "The Costcenter has been created!";
                    }

                    transaction.Commit();
                }

                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    //TODO: write HelperMethod for generalizing this kind of handling
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                    logger.Error(ex, ControllerContext?.HttpContext);
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", accountModel, handledException, "The Costcenter couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = message });
            }
        }

        [HttpGet]
        [Route("View/{guid}/{isPartialView}")]
        [Route("View/{guid}")]
        public ActionResult Viewer(string guid, bool isPartialView = false)
        {
            AccountModel mappedModel = new AccountModel();

            ViewBag.Titel = "View Costcenter";
            try
            {
                mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                //SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                //List<EMDEnterprise> enterprises = secUser.AllowedEnterprises(SecurityPermission.CostCenterManager_View);

                //foreach (EMDEnterprise ente in enterprises)
                //{
                //    mappedModel.EnterpriseList.Add(new Models.TextValueModel(ente.NameShort, ente.Guid));
                //}

                if (!string.IsNullOrEmpty(guid))
                {
                    ViewBag.Titel = "View Costcenter";

                    if (!mappedModel.CanView || !mappedModel.IsAllowedObject(this.UserName, guid, false))
                        return GetNoPermissionView(isPartialView);

                    EMDAccount emdAccount = Manager.AccountManager.Get(guid);
                    mappedModel = AccountModel.Initialize(emdAccount);
                    mappedModel.ResponsibleName = Manager.PersonManager.getFullDisplayNameWithUserIdAndPersNr(emdAccount.Responsible);
                    mappedModel.EnterpriseDisplayName = Manager.EnterpriseManager.Get(emdAccount.E_Guid).NameShort;
                }
                else
                {
                    ViewBag.Titel = "Add new Costcenter";
                }

            }
            catch (Exception ex)
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                logger.Error(ex, ControllerContext?.HttpContext);

            }


            if (isPartialView)
            {
                return PartialView("View", mappedModel);
            }
            else
            {
                return View("View", mappedModel);
            }
        }

        [HttpPost]
        [Route("DeleteCostcenter")]
        public ActionResult DeleteCostcenter(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                AccountModel accountModel = new AccountModel();
                accountModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                //if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.CostCenterManager_View_Manage))
                if (!accountModel.CanManage || !accountModel.IsAllowedObject(this.UserName, guid, true))
                    throw new Exception(SecurityHelper.NoPermissionText);

                AccountManager manager = new AccountManager(this.PersonGuid);
                manager.Delete(guid);

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

        [Route("Read")]
        [HandleError()]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            //TODO: Implement Historical
            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            EnterpriseHandler eh = new EnterpriseHandler();
            eh.DeliverInActive = true;
            AccountHandler accHandler = new AccountHandler();

            List<EMDAccount> accounts = null;

            if (secUser.IsAdmin)
            {
                accounts = (from acc in accHandler.GetObjects<EMDAccount, Account>().Cast<EMDAccount>().ToList() select acc).ToList();
            }
            else
            {
                accounts = (from acc in secUser.AllowedCostCenters(SecurityPermission.CostCenterManager_View) select acc).ToList();
            }

            var accountItems = (from acc in accounts join ente in eh.GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList() on acc.E_Guid equals ente.Guid select new { acc, ente }).ToList();

            List<AccountModel> accountModelList = new List<AccountModel>();

            AccountModel dummySecurityModel = AccountModel.Initialize(new EMDAccount());
            dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            foreach (var item in accountItems)
            {
                AccountModel model = AccountModel.Initialize((EMDAccount)item.acc, (EMDEnterprise)item.ente);
                model.CanManage = dummySecurityModel.CanManage;
                model.CanView = dummySecurityModel.CanView;

                accountModelList.Add(model);
            }

            DataSourceResult myresult = accountModelList.ToDataSourceResult(request);

            foreach (AccountModel accountModel in myresult.Data)
            {
                try
                {
                    accountModel.ResponsibleName = Manager.PersonManager.getFullDisplayNameWithUserIdAndPersNr(accountModel.Responsible);

                    List<EMDGroup> assignedGroups = Manager.GroupManager.GetAssignedCostCenterGroups(accountModel.Guid);
                    StringBuilder groupBuilder = new StringBuilder();
                    assignedGroups.ForEach(entity =>
                    {
                        groupBuilder.Append(entity.Name);
                        groupBuilder.Append(", ");
                    });
                    if (groupBuilder.Length >= 2)
                    {
                        groupBuilder.Remove(groupBuilder.Length - 2, 2);
                    }

                    accountModel.AssistanceGroups = groupBuilder.ToString();
                }
                catch (Exception ex)
                {
                    logger.Warn("Costcenter Grid: Getting additional information failed.", ex);
                }
            }


            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("GetCostCenterResponsibleName")]
        public ActionResult GetCostCenterResponsibleName(string guid)
        {
            bool success = true;

            string name = string.Empty;
            try
            {


                name = new AccountManager().GetCostCenterResponsibleName(guid);
            }
            catch (Exception)
            {
                success = false;
            }

            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = success, name = name }), "application/json");
        }

    }
}