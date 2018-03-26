using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EMD.EMD20Web.Models.Change;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Change")]
    public class ChangeController : BaseController
    {
        internal new IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Route()]
        public ActionResult Index()
        {
            return View();
        }





        [Route("GetChangeTypeModelList")]
        public ActionResult GetChangeTypeModelList()
        {
            return Json(ChangeTypeModel.GetChangeTypeModelList(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Route("ChangeEmployment")]
        [Route("ChangeEmployment/{empl_guid}/{ente_guid}")]
        [Route("ChangeEmployment/{empl_guid}/{ente_guid}/{isPartialView}")]
        public ActionResult ChangeEmployment(string empl_guid, string ente_guid, bool isPartialView = false)
        {
            EMDEmployment emdEmployment = new EmploymentManager().GetEmployment(empl_guid);


            Models.Change.ChangeEmploymentModel model = new Models.Change.ChangeEmploymentModel()
            {
                SourceEmploymentGuid = empl_guid,
                SourceEnterpriseGuid = ente_guid,
                GuidEmploymentType = emdEmployment?.ET_Guid
            };

            if (isPartialView)
            {
                return PartialView("ChangeEmployment", model);
            }
            else
            {
                return View("ChangeEmployment", model);
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        [Route("DoChange")]
        public ActionResult DoChange(ChangeEmploymentModel changeEmploymentModel)
        {
            Exception handledException = null;
            bool dateError = false;
            string enterpriseName = null;
            bool isBigChange = false;

            if (!ModelState.IsValid)
            {
                switch (changeEmploymentModel.SelectedChangeType)
                {
                    case EnumChangeType.Organisation:
                        ModelState.Remove("GuidTargetEnte");
                        ModelState.Remove("GuidEmploymentType");
                        ModelState.Remove("EMailType");

                        ModelState.Remove("GuidCostcenter");
                        ModelState.Remove("GuidDistributionGroup");
                        ModelState.Remove("GuidLocation");
                        ModelState.Remove("PersonnelRequisitionNumber");
                        break;
                    case EnumChangeType.EmploymentType:
                        // Pause is optional
                        ModelState.Remove("GuidTargetEnte");
                        ModelState.Remove("EMailType");

                        ModelState.Remove("GuidCostcenter");
                        ModelState.Remove("GuidOrgUnit");
                        ModelState.Remove("GuidLocation");
                        ModelState.Remove("PersonnelRequisitionNumber");
                        break;
                    case EnumChangeType.Enterprise:
                        ModelState.Remove("GuidDistributionGroup");
                        ModelState.Remove("Simcard");
                        ModelState.Remove("Datacard");
                        break;
                    default:
                        break;
                }


            }

            if (dateError)
            {
                ModelState.AddModelError("error", "All date fields must have a value!");
            }

            string message = string.Empty;

            if (changeEmploymentModel.GuidEmploymentType != null)
            {
                EmploymentTypeHandler employmentTypeHandler = new EmploymentTypeHandler();
                EMDEmploymentType emdEmploymentType = (EMDEmploymentType)employmentTypeHandler.GetObject<EMDEmploymentType>(changeEmploymentModel.GuidEmploymentType);

                if (!emdEmploymentType.MustHaveSponsor)
                {
                    ModelState.Remove("GuidSponsorEmployment");
                }

                if (((EnumEmploymentTypeCategory)emdEmploymentType.ETC_ID) == EnumEmploymentTypeCategory.Internal)
                {
                    ModelState.Remove("EMailType");
                }
            }
            else
            {
                ModelState.Remove("GuidSponsorEmployment");
            }

            bool doStatusUpdateOnSourceEmployment = false;

            if (changeEmploymentModel != null && ModelState.IsValid)
            {
                EMDEmployment oldEmployment = new EmploymentManager().GetEmployment(changeEmploymentModel.SourceEmploymentGuid);

                try
                {
                    EmplChangeWorkflowMessage emplChangeWorfklowMessage = null;
                    List<RemoveEquipmentInfo> equipmentInfos = new List<RemoveEquipmentInfo>();


                    ObjectRelationHandler handler = new ObjectRelationHandler();

                    foreach (var equipmentInstanceModel in changeEmploymentModel.EquipmentInstanceModels)
                    {
                        equipmentInfos.Add(new RemoveEquipmentInfo()
                        {
                            TargetDate = equipmentInstanceModel.TargetDate,
                            ObreGuid = equipmentInstanceModel.ObjectRelationGuid,
                            DoKeep = equipmentInstanceModel.DoKeep,
                            EquipmentDefinitionGuid = equipmentInstanceModel.EquipmentDefinitionGuid
                        });
                    }

                    EmploymentManager manager = new EmploymentManager(this.PersonGuid);
                    OnboardingManager onboardingManager = new OnboardingManager(this.PersonGuid);

                    switch (changeEmploymentModel.SelectedChangeType)
                    {
                        case EnumChangeType.Organisation:
                            emplChangeWorfklowMessage = WorkflowMessageHelper.GetEmplSmallChangeWorkflowMessage(
                               effectedPersonEmploymentGuid: changeEmploymentModel.SourceEmploymentGuid,
                               requestingPersEMPLGuid: this.UserMainEmplGuid,
                               targetDate: changeEmploymentModel.TargetDate.Value,
                               changeType: EnumEmploymentChangeType.Organisation,
                               businessCase: EnumBusinessCase.Change,
                               guidCostCenter: changeEmploymentModel.GuidCostcenter,
                               guidOrgunit: changeEmploymentModel.GuidOrgUnit,
                               guidSponsor: changeEmploymentModel.GuidSponsorEmployment,
                               personalNumber: changeEmploymentModel.PersonalNumber,
                               guidLocation: changeEmploymentModel.GuidLocation,
                               approveEquipments: changeEmploymentModel.ApproveEquipmentMove,
                               equipmentInfos: equipmentInfos,
                               moveAllRoles: changeEmploymentModel.MoveAllRoles,
                               leaveFrom: null,
                               leaveTo: null);

                            doStatusUpdateOnSourceEmployment = true;


                            break;
                        case EnumChangeType.Enterprise:
                            // TODO wollerc: check enterprise
                            isBigChange = true;
                            emplChangeWorfklowMessage = DoBigChange(ref manager, ref onboardingManager, ref equipmentInfos, changeEmploymentModel, EnumBusinessCase.Change);

                            break;
                        case EnumChangeType.EmploymentType:
                            if (oldEmployment.ET_Guid == changeEmploymentModel.GuidEmploymentType)
                            {
                                if (!changeEmploymentModel.LeaveFrom.HasValue || !changeEmploymentModel.LeaveTo.HasValue)
                                {
                                    if (oldEmployment.LeaveFrom == EMDEmployment.INFINITY || oldEmployment.LeaveFrom == EMDEmployment.INFINITY)
                                    {
                                        throw new Exception("No changes were detected! You have to change the employment-type or to set both inactive dates.");
                                    }
                                }

                                emplChangeWorfklowMessage = WorkflowMessageHelper.GetEmplSmallChangeWorkflowMessage(
                                   effectedPersonEmploymentGuid: changeEmploymentModel.SourceEmploymentGuid,
                                   requestingPersEMPLGuid: this.UserMainEmplGuid,
                                   targetDate: changeEmploymentModel.TargetDate.Value,
                                   changeType: EnumEmploymentChangeType.EmploymentType,
                                   businessCase: EnumBusinessCase.Change,
                                   guidCostCenter: changeEmploymentModel.GuidCostcenter,
                                   guidOrgunit: changeEmploymentModel.GuidOrgUnit,
                                   guidSponsor: changeEmploymentModel.GuidSponsorEmployment,
                                   personalNumber: changeEmploymentModel.PersonalNumber,
                                   guidLocation: changeEmploymentModel.GuidLocation,
                                   approveEquipments: changeEmploymentModel.ApproveEquipmentMove,
                                   equipmentInfos: equipmentInfos,
                                   moveAllRoles: changeEmploymentModel.MoveAllRoles,
                                   leaveFrom: changeEmploymentModel.LeaveFrom,
                                   leaveTo: changeEmploymentModel.LeaveTo);

                                doStatusUpdateOnSourceEmployment = true;

                            }
                            else
                            {
                                isBigChange = true;
                                emplChangeWorfklowMessage = DoBigChange(ref manager, ref onboardingManager, ref equipmentInfos, changeEmploymentModel, EnumBusinessCase.Change);

                            }
                            break;

                        default:
                            throw new NotImplementedException("There is no Worfklowmessage designed yet");
                    }

                    if (emplChangeWorfklowMessage != null)
                    {
                        EMDEnterpriseLocation oldEnterpriseLocation = new EnterpriseLocationManager().Get(oldEmployment.ENLO_Guid);

                        if (doStatusUpdateOnSourceEmployment)
                        {
                            EMDEmployment emdEmployment = (EMDEmployment)new EmploymentHandler(this.PersonGuid).GetObject<EMDEmployment>(changeEmploymentModel.SourceEmploymentGuid);
                            emdEmployment.Status = EDP.Core.Logic.ProcessStatus.STATUSITEM_ORDERED;
                            if (emdEmployment != null)
                            {
                                // set the enterprise name to get the right tab for disable change buttons
                                enterpriseName = ((EMDEnterprise)new EnterpriseHandler().GetObject<EMDEnterprise>(changeEmploymentModel.SourceEnterpriseGuid)).NameShort;
                                new EmploymentHandler(this.PersonGuid).UpdateObject(emdEmployment);
                            }
                        }


                        string errmsg = string.Empty;
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        try
                        {
                            OrgUnitRoleSearch searcher = new OrgUnitRoleSearch();
                            AccountHandler accountHandler = new AccountHandler();
                            AccountManager accountManager = new AccountManager();
                            OrgUnitManager orgunitManager = new OrgUnitManager();

                            IEmploymentInfos infos = emplChangeWorfklowMessage;

                            if (!isBigChange) // For BigChange removing assistence is implemented in core (OnBoardingManager.PrepareOnboarding)
                            {
                                //Delete assistence relation
                                AccountGroupManager accGrpManager = new AccountGroupManager();
                                if (accGrpManager.IsEmpoymentAssistence(changeEmploymentModel.SourceEmploymentGuid))
                                {
                                    accGrpManager.RemoveEmploymentFromAllAssistenceGroups(changeEmploymentModel.SourceEmploymentGuid);
                                }
                            }

                            string costCenterOldGuid = accountManager.GetAccountForEmployment(oldEmployment.Guid)?.Guid;
                            WorkflowHelper.SetEmploymentInfos(ref infos,
                                costcenterOldGuid: costCenterOldGuid,
                                costcenterNewGuid: changeEmploymentModel.GuidCostcenter,
                                costCenterResponsibleOldEmplGuid: accountHandler.GetResponsible(costCenterOldGuid)?.Guid,
                                costCenterResponsibleNewEmplGuid: accountHandler.GetResponsible(changeEmploymentModel.GuidCostcenter).Guid,
                                orgunitOldGuid: orgunitManager.GetOrgunitForEmployment(oldEmployment.Guid).Guid,
                                orgunitNewGuid: changeEmploymentModel.GuidOrgUnit,
                                lineManagerOldEmplGuid: searcher.SearchOrgUnitRoleForEmployment(RoleHandler.LINEMANAGER, oldEmployment.Guid).FirstOrDefault(),
                                lineManagerNewEmplGuid: searcher.SearchOrgUnitRoleForOrgUnit(RoleHandler.LINEMANAGER, changeEmploymentModel.GuidOrgUnit).FirstOrDefault(),
                                teamleaderOldEmplGuid: searcher.SearchOrgUnitRoleForEmployment(RoleHandler.TEAMLEADER, oldEmployment.Guid).FirstOrDefault(),
                                teamleaderNewEmplGuid: searcher.SearchOrgUnitRoleForOrgUnit(RoleHandler.TEAMLEADER, changeEmploymentModel.GuidOrgUnit).FirstOrDefault(),
                                assistanceOldEmplGuid: manager.GetAssistence(oldEmployment.Guid).FirstOrDefault()?.Guid,
                                assistanceNewEmplGuid: manager.GetAssistence(changeEmploymentModel.SourceEmploymentGuid).FirstOrDefault()?.Guid,
                                persNrOld: oldEmployment.PersNr,
                                persNrNew: changeEmploymentModel.PersonalNumber,
                                locationOldGuid: oldEnterpriseLocation?.L_Guid,
                                locationNewGuid: changeEmploymentModel.GuidLocation
                                );

                            ((EmplChangeWorkflowMessage)infos).CreateWorkflowInstance(this.PersonGuid, MODIFY_COMMENT);
                            stopWatch.Stop();

                            logger.Info(string.Format("The Webservice Call 'CreateWorkflowInstance' (Change) was successfully started for empl {0}. Call time in ms: {1}", changeEmploymentModel.SourceEmploymentGuid, stopWatch.ElapsedMilliseconds));
                        }
                        catch (Exception ex)
                        {
                            AddResponseCloseWindow();
                            errmsg = "The Employment was created with status \"NOT SET\", because the Process-Engine didn't response.";
                            ModelState.AddModelError("error", errmsg);
                            try
                            {
                                EMDEmployment newEmployment = new EmploymentManager().GetEmployment(changeEmploymentModel.SourceEmploymentGuid);
                                logger.Error("The webservice Call CreateWorkflowInstance failed", ex);
                                if (newEmployment != null)
                                {
                                    // Update the status of the obre
                                    newEmployment.Status = (byte)EDP.Core.Logic.ProcessStatus.STATUSITEM_NOTSET;

                                    new EmploymentHandler().UpdateObject<EMDEmployment>(newEmployment);
                                    logger.Info(string.Format("Employment {0} was updated to status STATUSITEM_NOTSET.", newEmployment.Guid));
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error("The existing Employment couldn't be updated to Status STATUSITEM_NOTSET", e);
                            }
                        }
                    }
                }
                catch (EntityNotFoundException ex)
                {
                    handledException = ex;
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                }
                catch (FaultException ex)
                {
                    handledException = ex;
                    //TODO: write HelperMethod for generalizing this kind of handling
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    //TODO: write HelperMethod for generalizing this kind of handling
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("ChangeEmployment", changeEmploymentModel, handledException, "The User couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = message, enterpriseName = enterpriseName });
            }
        }

        private EmplChangeWorkflowMessage DoBigChange(ref EmploymentManager manager, ref OnboardingManager onboardingManager, ref List<RemoveEquipmentInfo> equipmentInfos, ChangeEmploymentModel changeEmploymentModel, EnumBusinessCase enumBusinessCase)
        {
            changeEmploymentModel.ApproveEquipmentMove = true;

            XElement rootKCCData = new XElement("KCCData");

            XElement migration = new XElement("Items");
            migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Personnel requisition number", changeEmploymentModel.PersonnelRequisitionNumber));
            migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("No Approval Needed", changeEmploymentModel.NoApprovalNeeded.ToString()));
            migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Reason for no Approval", changeEmploymentModel.NoApprovalNeededReason));
            migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Simcard", changeEmploymentModel.Simcard.ToString()));
            migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Datacard", changeEmploymentModel.Datacard.ToString()));

            rootKCCData.Add(migration);

            EMDEmployment oldEmployment = new EmploymentManager().GetEmployment(changeEmploymentModel.SourceEmploymentGuid);
            EMDPerson effectedPerson = new PersonManager().GetPersonByEmployment(oldEmployment.Guid);

            EMDEmployment empl = new EMDEmployment();
            empl.Entry = changeEmploymentModel.TargetDate;
            empl.FirstWorkDay = changeEmploymentModel.TargetDate;
            empl.LastDay = oldEmployment.LastDay;
            empl.Exit = oldEmployment.Exit;
            empl.PersNr = changeEmploymentModel.PersonalNumber;

            // string ente_guid = changeEmploymentModel.SelectedChangeType == EnumChangeType.Enterprise ? changeEmploymentModel.GuidTargetEnte : changeEmploymentModel.SourceEnterpriseGuid;

            // 1. start onboarding for new employment or change leave Dates
            string changeEmplGuid = null;

            EmplAddWorkflowMessage emplAddWorkflowMessage = onboardingManager.PrepareOnboarding(
                requestingPersonEmplGuid: this.UserMainEmplGuid,
                empl: empl,
                effectedPersonGuid: effectedPerson.Guid,
                enteGuid: string.IsNullOrEmpty(changeEmploymentModel.GuidTargetEnte) ? changeEmploymentModel.SourceEnterpriseGuid : changeEmploymentModel.GuidTargetEnte,
                locaGuid: changeEmploymentModel.GuidLocation,
                accoGuid: changeEmploymentModel.GuidCostcenter,
                orguGuid: changeEmploymentModel.GuidOrgUnit,
                emtyGuid: changeEmploymentModel.GuidEmploymentType,
                userdomainGuid: null,
                digrGuid: oldEmployment.DGT_Guid,
                sponsorGuid: changeEmploymentModel.GuidSponsorEmployment,
                emailType: null,
                contactList: CreateContactsFromExistingEmployment(changeEmploymentModel.SourceEmploymentGuid),
                xmlData: rootKCCData,
                newEquipments: null,
                leaveFrom: changeEmploymentModel.LeaveFrom,
                leaveTo: changeEmploymentModel.LeaveTo,
                oldEmplChangeExit: oldEmployment,
                businessCase: enumBusinessCase,
                changeType: (EnumEmploymentChangeType)changeEmploymentModel.SelectedChangeType);

            if (string.IsNullOrEmpty(emplAddWorkflowMessage.NewEmploymentGuid))
            {
                throw new Exception("The new employment couldn't be created!<br>Please try it again");
            }

            changeEmplGuid = emplAddWorkflowMessage.NewEmploymentGuid;
            emplAddWorkflowMessage.CreateWorkflowInstance(this.PersonGuid, MODIFY_COMMENT);



            // 2. start change process only for equipments
            EmplChangeWorkflowMessage smallChangeData = WorkflowMessageHelper.GetEmplSmallChangeWorkflowMessage(
               effectedPersonEmploymentGuid: changeEmploymentModel.SourceEmploymentGuid,
               requestingPersEMPLGuid: this.UserMainEmplGuid,
               targetDate: changeEmploymentModel.TargetDate.Value,
               changeType: (EnumEmploymentChangeType)changeEmploymentModel.SelectedChangeType,
               businessCase: enumBusinessCase,
               new_empl_guid: changeEmplGuid,
               approveEquipments: changeEmploymentModel.ApproveEquipmentMove,
               equipmentInfos: equipmentInfos);




            return smallChangeData;
            //   return WorkflowHelper.GetWorkflowMessageData(smallChangeData);
        }

        private List<EMDContact> CreateContactsFromExistingEmployment(string empl_guid)
        {
            // copy contact data
            List<EMDContact> emdContacts = new ContactHandler().GetObjects<EMDContact, Contact>(string.Format("EP_GUID = \"{0}\"", empl_guid)).Cast<EMDContact>().ToList();
            foreach (EMDContact item in emdContacts)
            {
                item.Guid = null;
                item.EP_Guid = null;
            }
            return emdContacts;
        }


        [Route("GetChangeSelectionView")]
        public ActionResult GetChangeSelectionView(EnumChangeType changeType, string empl_guid, string guidEnterprise)
        {
            ChangeEmploymentModel model = new ChangeEmploymentModel();
            model.SourceEmploymentGuid = empl_guid;
            model.SourceEnterpriseGuid = guidEnterprise;
            model.SelectedChangeType = changeType;
            model.ShowEquipmentApprovement = !new EmploymentChangeTypeHelper((EnumEmploymentChangeType)(int)changeType).isNeeded(EnumChangeValueType.EquipmentProc);
            //model.SourceEnterpriseGuid =  ViewData["EnterpriseGuid"].ToString();

            // initialize current values
            EMDEmployment emdEmployment = new EmploymentManager().GetEmployment(empl_guid);
            EMDAccount emdAccount = new AccountManager().GetAccountForEmployment(empl_guid);
            EMDEnterpriseLocation emdEnterpriseLocation = new EnterpriseLocationManager().Get(emdEmployment.ENLO_Guid);
            EMDOrgUnit emdOrgunit = new OrgUnitManager().GetOrgunitForEmployment(empl_guid);
            model.GuidEmploymentType = emdEmployment?.ET_Guid;
            model.SourceEnterpriseGuid = emdEnterpriseLocation?.E_Guid;
            if (changeType != EnumChangeType.Enterprise)
            {
                model.GuidCostcenter = emdAccount?.Guid;
                model.CostCenterResponsibleName = "not set";
                if (emdAccount != null)
                {
                    model.CostCenterResponsibleName = new AccountManager().GetCostCenterResponsibleName(emdAccount?.Guid);
                }
                model.GuidOrgUnit = emdOrgunit?.Guid;
            }
            model.PersonalNumber = emdEmployment.PersNr.Replace("\n", string.Empty).Trim();
            model.GuidSponsorEmployment = emdEmployment.Sponsor_Guid;

            model.SponsorSelection = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "GuidSponsorEmployment",
                ObjectLabel = "Sponsor",
                ObjectValue = model.GuidSponsorEmployment,
                ObjectText = new PersonManager().getFullDisplayNameWithUserIdAndPersNr(model.GuidSponsorEmployment),
                TargetControllerMethodName = "GetEmploymentList",
                TargetControllerName = "Employment"
            };

            if (emdEmployment.LeaveFrom < EMDEmployment.INFINITY)
            {
                model.LeaveFrom = emdEmployment.LeaveFrom;
            }
            if (emdEmployment.LeaveTo < EMDEmployment.INFINITY)
            {
                model.LeaveTo = emdEmployment.LeaveTo;
            }

            if (changeType != EnumChangeType.Enterprise)
            {
                model.GuidLocation = emdEnterpriseLocation.L_Guid;
            }

            return PartialView("ChangeSelection", model);
        }


        [Route("GetEquipmentsView")]
        public ActionResult GetEquipmentsView(EnumChangeType changeType, string targetDateIso8601, string guidCurrentEnterprise, string guidCurrentEmployment, string guidEmploymentType, string guidCostCenter, string guidEnterprise, string guidLocation)
        {
            DateTime? targetDate = null;
            if (!string.IsNullOrEmpty(targetDateIso8601))
            {
                targetDate = DateTimeHelper.Iso8601ToDateTime(targetDateIso8601);
            }

            DateTime now = DateTime.Now;

            if (!(targetDate.Value.Month == now.Month && targetDate.Value.Day == now.Day))
            {
                targetDate = targetDate.Value.AddDays(-(targetDate.Value.Day - 1));
            }

            if (string.IsNullOrEmpty(guidEnterprise))
            {
                guidEnterprise = guidCurrentEnterprise;
            }
            ObjectRelationHandler obreHandler = new ObjectRelationHandler();

            ChangeEmploymentModel model = new ChangeEmploymentModel();

            model.SourceEnterpriseGuid = guidCurrentEnterprise;
            model.SelectedChangeType = changeType;
            //model.SourceEnterpriseGuid =  ViewData["EnterpriseGuid"].ToString();

            if (string.IsNullOrEmpty(guidLocation))
            {
                EmploymentHandler employmentHandler = new EmploymentHandler();
                EMDEmployment employment = (EMDEmployment)employmentHandler.GetObject<EMDEmployment>(guidCurrentEmployment);
                EMDEnterpriseLocation enterpriseLocation = (EMDEnterpriseLocation)new EnterpriseLocationHandler().GetObject<EMDEnterpriseLocation>(employment.ENLO_Guid);
                guidLocation = enterpriseLocation.L_Guid;
            }

            model.EquipmentInstanceModels = EquipmentInstanceModel.GetEquipmentInstanceModels(guidCurrentEmployment, this.UserName).ToList();



            string userTypeForFilter = null;

            try
            {
                UserManager userManager = new UserManager();

                string mainUserGuid = new PersonManager().GetPersonByEmployment(guidCurrentEmployment).USER_GUID;

                byte userTypeByte = userManager.GetEmploymentUsers(guidCurrentEmployment).Find(i => i.Guid == mainUserGuid).UserType;
                userTypeForFilter = ((EnumUserType)userTypeByte).ToString();
            }
            catch (Exception ex)
            {
                logger.Error("EquipmentsView: error trying to UserType for main User." + ex.ToString());
            }


            FilterManager filterManager;
            foreach (EquipmentInstanceModel equipmentInstanceModel in model.EquipmentInstanceModels.ToList())
            {
                if (equipmentInstanceModel.ObjectRelationGuid != null)
                {
                    EMDObjectRelation objectRelation = (EMDObjectRelation)obreHandler.GetObject<EMDObjectRelation>(equipmentInstanceModel.ObjectRelationGuid);
                    equipmentInstanceModel.EquipmentDefinitionGuid = objectRelation.Object2;
                    filterManager = new FilterManager(objectRelation.Object2);
                    equipmentInstanceModel.TargetDate = targetDate;

                    if (equipmentInstanceModel.CanKeep && equipmentInstanceModel.IsDefault)
                    {
                        equipmentInstanceModel.DoKeep = true;
                    }
                    else
                    {
                        equipmentInstanceModel.DoKeep = false;
                    }

                    if (!filterManager.CheckRule(objectRelation.Object2, guidEnterprise, guidLocation, guidEmploymentType, guidCostCenter, userTypeForFilter))
                    {
                        equipmentInstanceModel.CanKeep = false;
                        equipmentInstanceModel.DoKeep = false;
                        equipmentInstanceModel.Info = "The Equipment is not allowed for this change";
                    }
                }
                else
                {
                    //do remove this model from list since its empty
                    model.EquipmentInstanceModels.Remove(equipmentInstanceModel);
                }
            }


            return PartialView("ChangeEmploymentEquipments", model);
        }

        /// <summary>
        /// checks if all items are set for the selected change type 
        /// </summary>
        /// <param name="changeType"></param>
        /// <param name="valueTypes"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckInputValidationFinishedForChangeType")]
        public ActionResult CheckInputValidationFinishedForChangeType(EnumChangeType changeType, List<EnumChangeValueType> valueTypes, int? idEnterprise)
        {
            bool success = true;
            bool isFinished = true;
            bool showAdditionalData = false;
            bool mustShowEquipments = false;

            try
            {
                EmploymentChangeTypeHelper ecth = new EmploymentChangeTypeHelper((EnumEmploymentChangeType)(int)changeType);



                if (idEnterprise.HasValue && idEnterprise.Value == 20)
                {
                    showAdditionalData = true;
                }


                foreach (EnumChangeValueType currentChangeType in Enum.GetValues(typeof(EnumChangeValueType)))
                {
                    bool contains = valueTypes.Exists(a => a == currentChangeType);
                    if (currentChangeType != EnumChangeValueType.EquipmentProc && currentChangeType != EnumChangeValueType.NewEmpl)
                    {
                        if (ecth.isNeeded(currentChangeType))
                        {
                            if (!contains)
                            {
                                isFinished = false;
                                break;
                            }
                        }
                    }

                    if (currentChangeType == EnumChangeValueType.EquipmentProc)
                    {
                        mustShowEquipments = new EmploymentChangeTypeHelper((EnumEmploymentChangeType)(int)changeType).isNeeded(EnumChangeValueType.EquipmentProc);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                logger.Error("CheckInputValidationFinishedForChangeType failed!", ex);
            }

            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = success, isFinished = isFinished, mustShowEquipments = mustShowEquipments, showAdditionalData = showAdditionalData }), "application/json");
        }


    }
}