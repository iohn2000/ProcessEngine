using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EDP.Core.Framework;
using System;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EDP.Core.Entities.Enhanced;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("AccountGroup")]
    public class AccountGroupController : BaseController
    {
        [Route("Manage")]
        public ActionResult Manage()
        {
            GroupAccountManagementModel model = new GroupAccountManagementModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            //just return an empty view since all data is Ajax-driven
            return View("Manage", model);
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            List<GroupAccountManagementModel> models = new List<GroupAccountManagementModel>();

            List<EMDGroup> emdGroups = new GroupManager().GetCostCenterGroups();

            List<EMDEnterprise> emdEnterprises = new EnterpriseHandler().GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList();

            List<EMDAccount> emdAccounts = new AccountHandler().GetObjects<EMDAccount, Account>().Cast<EMDAccount>().ToList();
            List<EMDAccountGroup> emdAccountGroups = new AccountGroupHandler().GetObjects<EMDAccountGroup, AccountGroup>().Cast<EMDAccountGroup>().ToList();

            emdAccountGroups.ForEach(accountGroup =>
            {
                EMDGroup emdGroup = emdGroups.Find(group => group.Guid == accountGroup.G_Guid);
                if (emdGroup != null)
                {
                    GroupAccountManagementModel groupAccountModel = GroupAccountManagementModel.Map(emdGroup);
                    groupAccountModel.Acgr_Guid = accountGroup.Guid;
                    EMDAccount emdAccount = emdAccounts.Find(a => a.Guid == accountGroup.AC_Guid);
                    EMDEnterprise emdEnterprise = emdEnterprises.Find(a => a.Guid == emdGroup.E_Guid);
                    if (emdAccount != null && emdEnterprise != null)
                    {
                        groupAccountModel.AccountName = emdAccount.Name;
                        groupAccountModel.EnterpriseName = emdEnterprise.NameShort;

                        models.Add(groupAccountModel);
                    }
                }

            });

            DataSourceResult myresult;
            myresult = models.ToDataSourceResult(request, ModelState);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Route("DeleteAccountGroup")]
        public ActionResult DeleteAccountGroup(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.AccountGroup_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                AccountGroupHandler accountGroupHandler = new AccountGroupHandler(this.PersonGuid);
                EMDAccountGroup emdAccountGroup = (EMDAccountGroup)accountGroupHandler.GetObject<EMDAccountGroup>(guid);

                accountGroupHandler.DeleteObject(emdAccountGroup);
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


        [Route("Edit")]
        [Route("Add")]
        [Route("Add/{isPartialView}")]
        [Route("Edit/{acgr_guid}")]
        [Route("Edit/{acgr_guid}/{isPartialView}")]
        public ActionResult Edit(string acgr_guid, bool isPartialView = false)
        {
            GroupAccountManagementModel mappedModel = new GroupAccountManagementModel();
            mappedModel.Acgr_Guid = acgr_guid;

            Models.Shared.SelectionViewModel selectionViewModelCostCenter = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "Acco_Guid",
                ObjectLabel = "Costcenter",
                ObjectValue = mappedModel.Acco_Guid,
                TargetControllerMethodName = "ReadForSelectDs",
                TargetControllerName = "Account"
            };

            Models.Shared.SelectionViewModel selectionViewModelEnterprise = new Models.Shared.SelectionViewModel()
            {
                SelectionEvent = "accountGroup.Events.OnEnterpriseDropDownlistSelectionSelect(this)",
                ReferencePropertyName = "E_Guid",
                ObjectLabel = "Enterprise",
                TargetControllerMethodName = "ReadForSelectDs",
                TargetControllerName = "Enterprise"
            };

            try
            {
                if (!string.IsNullOrEmpty(acgr_guid))
                {
                    ViewBag.Titel = "Edit costcenter group";

                    EMDAccountGroup emdAccountGroup = (EMDAccountGroup)new AccountGroupHandler().GetObject<EMDAccountGroup>(acgr_guid);

                    EMDGroup emdGroup = (EMDGroup)new GroupHandler().GetObject<EMDGroup>(emdAccountGroup.G_Guid);
                    EMDAccount emdAccount = (EMDAccount)new AccountHandler().GetObject<EMDAccount>(emdAccountGroup.AC_Guid);

                    mappedModel = GroupAccountManagementModel.Map(emdGroup);
                    mappedModel.Acgr_Guid = acgr_guid;

                    mappedModel.Acco_Guid = emdAccount.Guid;
                    mappedModel.AccountName = emdAccount.Name;

                    // fill assigend members
                    List<EMDGroupMember> emdGroupMembers = new GroupMemberHandler().GetObjects<EMDGroupMember, GroupMember>(string.Format("G_Guid=\"{0}\"", emdGroup.Guid)).Cast<EMDGroupMember>().ToList();

                    mappedModel.AssignedEmployments = new List<TextValueModel>();

                    PersonManager personManager = new PersonManager();
                    EmploymentManager employmentManager = new EmploymentManager();
                    foreach (EMDGroupMember groupMember in emdGroupMembers)
                    {
                        EMDPerson emdPerson = personManager.GetPersonByEmployment(groupMember.EP_Guid);
                        EMDEmployment emdEmployment = employmentManager.GetEmployment(groupMember.EP_Guid);
                        if (emdEmployment.IsSystemActive) //Workaround because groupmember entries are not removed when employment is offboarded
                        {
                            mappedModel.AssignedEmployments.Add(new TextValueModel(string.Format("{0} {1} (EP-ID: {2})", emdPerson.FamilyName, emdPerson.FirstName, groupMember.EP_ID), groupMember.EP_Guid));
                        }
                    }

                    mappedModel.AccountSelection = selectionViewModelCostCenter;

                    mappedModel.AccountSelection.ObjectValue = mappedModel.Acco_Guid;
                    mappedModel.AccountSelection.ObjectText = emdAccount.KstID + " - " + emdAccount.Name;

                    mappedModel.EnterpriseSelection = selectionViewModelEnterprise;
                    mappedModel.EnterpriseSelection.ObjectValue = mappedModel.E_Guid;
                    mappedModel.EnterpriseSelection.ObjectText = new EnterpriseManager().Get(mappedModel.E_Guid)?.NameShort;
                }
                else
                {
                    mappedModel.EnterpriseSelection = selectionViewModelEnterprise;
                    mappedModel.AccountSelection = selectionViewModelCostCenter;
                    ViewBag.Titel = "Add new costcenter group";
                }
            }
            catch (Exception e)
            {
                logger.Error("AccountGroupController throws an Exception in Edit", e);
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

        [Route("ReadAvailableEmployments")]
        public ActionResult ReadAvailableEmployments([DataSourceRequest]DataSourceRequest request, string guid_ente, List<string> assignedGuids)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();

            List<EMDPersonEmployment> emdEmployments = null;
            if (!string.IsNullOrEmpty(guid_ente))
            {
                emdEmployments = new EmploymentManager().GetAllPersonEmployments();

                foreach (EMDPersonEmployment emdEmployment in emdEmployments)
                {
                    if (assignedGuids == null || !assignedGuids.Contains(emdEmployment.Empl.Guid))
                    {
                        keyValuePairs.Add(new TextValueModel(string.Format("{0} {1} (EP-ID: {2})", emdEmployment.Pers.FamilyName, emdEmployment.Pers.FirstName, emdEmployment.Empl.EP_ID), emdEmployment.Empl.Guid));
                    }
                }
            }

            DataSourceResult myresult = keyValuePairs.ToDataSourceResult(request);
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        [HttpPost]
        [Route("Edit")]
        public ActionResult Edit(GroupAccountManagementModel groupAccountManagementModel)
        {

            ViewBag.Title = "Edit costcenter group";
            string message = string.Empty;
            Exception handledException = null;
            if (groupAccountManagementModel != null && ModelState.IsValid)
            {
                CoreTransaction transaction = new CoreTransaction();
                try
                {
                    transaction.Begin();
                    CheckForExistingAccountGroups(groupAccountManagementModel);

                    if (!string.IsNullOrEmpty(groupAccountManagementModel.Acgr_Guid))
                    {
                        EMDAccountGroup emdAccountGroup = (EMDAccountGroup)new AccountGroupHandler().GetObject<EMDAccountGroup>(groupAccountManagementModel.Acgr_Guid);

                        EMDGroup emdGroup = (EMDGroup)new GroupHandler().GetObject<EMDGroup>(emdAccountGroup.G_Guid);
                        EMDAccount emdAccount = (EMDAccount)new AccountHandler().GetObject<EMDAccount>(emdAccountGroup.AC_Guid);

                        // update group name
                        if (emdGroup.Name != groupAccountManagementModel.Name || emdGroup.E_Guid != groupAccountManagementModel.E_Guid)
                        {
                            emdGroup.Name = groupAccountManagementModel.Name;
                            emdGroup.E_Guid = groupAccountManagementModel.E_Guid;
                            new GroupHandler(transaction, this.PersonGuid, "EDP20Userinterface").UpdateObject<EMDGroup>(emdGroup);
                        }

                        // update account
                        if (emdAccountGroup.AC_Guid != groupAccountManagementModel.Acco_Guid)
                        {
                            emdAccountGroup.AC_Guid = groupAccountManagementModel.Acco_Guid;
                            new AccountGroupHandler(transaction, this.PersonGuid, "EDP20Userinterface").UpdateObject<EMDAccountGroup>(emdAccountGroup);
                        }

                        message = "The costcenter group has been updated!";
                    }
                    else
                    {

                        // If the check is OK, create the new objects

                        EMDGroup emdGroup = new EMDGroup();
                        emdGroup.Name = groupAccountManagementModel.Name;
                        emdGroup.E_Guid = groupAccountManagementModel.E_Guid;

                        emdGroup = (EMDGroup)new GroupHandler(transaction, this.PersonGuid, "EDP20Userinterface").CreateObject<EMDGroup>(emdGroup);

                        if (emdGroup == null)
                        {
                            message = string.Format("The group with name {0} couldn't be created", groupAccountManagementModel.Name);
                            throw new Exception(message);
                        }
                        groupAccountManagementModel.Guid = emdGroup.Guid;

                        EMDAccountGroup emdAccountGroup = new EMDAccountGroup();
                        emdAccountGroup.AC_Guid = groupAccountManagementModel.Acco_Guid;
                        emdAccountGroup.G_Guid = emdGroup.Guid;
                        emdAccountGroup.Key = string.Empty;
                        new AccountGroupHandler(transaction, this.PersonGuid, "EDP20Userinterface").CreateObject<EMDAccountGroup>(emdAccountGroup);

                        groupAccountManagementModel.Acgr_Guid = emdAccountGroup.Guid;

                        message = "The costcenter group has been created!";
                    }
                    transaction.Commit();
                    transaction.Begin();

                    try
                    {
                        // get assigend members
                        List<EMDGroupMember> oldGroupMembers = new GroupMemberHandler().GetObjects<EMDGroupMember, GroupMember>(string.Format("G_Guid=\"{0}\"", groupAccountManagementModel.Guid)).Cast<EMDGroupMember>().ToList();

                        GroupMemberHandler groupMemberHandler = new GroupMemberHandler();
                        EmploymentManager employmentManager = new EmploymentManager();
                        // add new groupmembers
                        foreach (TextValueModel assigned in groupAccountManagementModel.AssignedEmployments)
                        {
                            if (!oldGroupMembers.Exists(a => a.EP_Guid == assigned.Value))
                            {
                                EMDGroupMember emdGroupMember = new EMDGroupMember();
                                emdGroupMember.G_Guid = groupAccountManagementModel.Guid;
                                emdGroupMember.EP_Guid = assigned.Value;

                                EMDEmployment empl = employmentManager.GetEmployment(assigned.Value);
                                if (empl != null)
                                {
                                    emdGroupMember.EP_ID = empl.EP_ID;
                                }
                                groupMemberHandler.CreateObject<EMDGroupMember>(emdGroupMember);
                            }
                        }

                        // remove groupmembers
                        foreach (EMDGroupMember item in oldGroupMembers)
                        {
                            if (!groupAccountManagementModel.AssignedEmployments.ToList().Exists(a => a.Value == item.EP_Guid))
                            {
                                groupMemberHandler.DeleteObject<EMDGroupMember>(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("The new costcenter group is created. But there was a problem with saving the assigned employments!", ex);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    //TODO: write HelperMethod for generalizing this kind of handling
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", groupAccountManagementModel, handledException, "The Costcenter group couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                //((GroupAccountManagementModel)result.Model).AccountSelection = new Models.Shared.SelectionViewModel()
                //{
                //    ReferencePropertyName = "Acco_Guid",
                //    ObjectLabel = "Costcenter",
                //    ObjectValue = mappedModel.Acco_Guid,
                //    TargetControllerMethodName = "ReadForSelectDs",
                //    TargetControllerName = "Account"
                //};
                return result;
            }
            else
            {
                return Json(new { message = message });
            }
        }

        /// <summary>
        /// check if there is already a Group with same name and ENTE Guid >> not necessary to create a new one
        /// </summary>
        /// <param name="groupAccountManagementModel"></param>
        private void CheckForExistingAccountGroups(GroupAccountManagementModel groupAccountManagementModel)
        {
            List<EMDAccountGroup> emdAccountGroups = new AccountGroupHandler().GetObjects<EMDAccountGroup, AccountGroup>().Cast<EMDAccountGroup>().ToList();

            // 
            List<EMDGroup> emdGroups = new GroupManager().GetCostCenterGroups();
            emdGroups = emdGroups.FindAll(group => group.Name.Equals(groupAccountManagementModel.Name, StringComparison.CurrentCultureIgnoreCase) && group.E_Guid == groupAccountManagementModel.E_Guid);
            foreach (EMDGroup currentEmdGroup in emdGroups)
            {
                EMDAccountGroup foundItem = emdAccountGroups.Find(a => a.AC_Guid == groupAccountManagementModel.Acco_Guid && a.G_Guid == groupAccountManagementModel.Guid);

                if (foundItem != null)
                {
                    if ((!string.IsNullOrEmpty(groupAccountManagementModel.Guid) && foundItem.Guid == groupAccountManagementModel.Acgr_Guid && foundItem.AC_Guid != groupAccountManagementModel.Acco_Guid)
                        || string.IsNullOrEmpty(groupAccountManagementModel.Guid))
                    {


                        throw new Exception("There is already an accountgroup with your parameters created");

                    }
                }

                //EMDAccountGroup foundItem = emdAccountGroups.Find(a => a.AC_Guid == groupAccountManagementModel.Acco_Guid);
                //if (foundItem != null)
                //{

                //    throw new Exception("There is already an accountgroup with your parameters created");
                //}
            }
        }

    }
}