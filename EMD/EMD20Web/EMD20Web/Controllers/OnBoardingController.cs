using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.WF;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EMD.EMD20Web.Models.Onboarding;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("OnBoarding")]
    //[Filters.AccessPermissions(AccessPermission="ONBOARDING,  EMDADMIN  ,  PRIME")]
    public class OnBoardingController : BaseController
    {
        [Route()]
        public ActionResult Index()
        {
            return View("Create");
        }

        [Route("Finished")]
        public ActionResult Finished()
        {
            return View();
        }

        [Route("Create/{pers_guid}")]
        [Route("Create/{pers_guid}/{isPartialView}")]
        public ActionResult Create(string pers_guid, bool isPartialView = false)
        {
            OnboardingModel onboardMod = new OnboardingModel();

            onboardMod.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            onboardMod.pers_guid = pers_guid;
            onboardMod.EntryDate = null;
            onboardMod.FirstDayOfWork = null;
            onboardMod.UntilDate = null;
            onboardMod.LastDay = null;
            onboardMod.NoApprovalNeededReason = String.Empty;

            if (!onboardMod.CanManage)
                return GetNoPermissionView(isPartialView);

            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            List<EMDEnterprise> allowedEnterprises = secUser.AllowedEnterprises(SecurityPermission.Onboarding);

            // prefill the user-domain
            UserDomainHandler userDomainHandler = new UserDomainHandler();
            onboardMod.GuidDomain = (string)userDomainHandler.GetObjects<EMDUserDomain, UserDomain>("Name = \"kapsch.co.at\"").FirstOrDefault().Guid;


            EnterpriseManager enteManager = new EnterpriseManager();
            List<EMDEnterprise> kccEnterprises = enteManager.GetEnterpriseLeafes(new List<int> { 2000, 1310 });

            foreach (EMDEnterprise ente in allowedEnterprises)
            {

                if (kccEnterprises.FindLast(e => e.Guid == ente.Guid) != null)
                {
                    if (onboardMod.EnterpriseList.Find(a => a.Value == ente.Guid) == null)
                    {
                        if (ente.HasEmployees)
                        {
                            onboardMod.EnterpriseList.Add(new Models.TextValueModel(ente.NameShort, ente.Guid, new { E_ID = ente.E_ID, isKcc = true }));
                        }
                    }
                }
                else
                {
                    if (onboardMod.EnterpriseList.Find(a => a.Value == ente.Guid) == null)
                    {
                        if (ente.HasEmployees)
                        {
                            onboardMod.EnterpriseList.Add(new Models.TextValueModel(ente.NameShort, ente.Guid, new { E_ID = ente.E_ID, isKcc = false }));
                        }
                    }
                }

            }

            onboardMod.SponsorSelection = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "SponsorGuid",
                ObjectLabel = "Sponsor",
                ObjectValue = onboardMod.SponsorGuid,
                ObjectText = new PersonManager().getFullDisplayNameWithUserIdAndPersNr(onboardMod.SponsorGuid),
                TargetControllerMethodName = "GetEmploymentList",
                TargetControllerName = "Employment",
                ParentFormControlWidth = "350px;"
            };

            if (isPartialView)
            {
                return PartialView("Create", onboardMod);
            }
            else
            {
                return View("Create", onboardMod);
            }

        }


        [ValidateInput(false)]
        [Route("DoCreate")]
        [HttpPost]
        public ActionResult DoCreate(OnboardingModel pModel)
        {
            Exception handledException = null;
            CoreTransaction transaction = new CoreTransaction();
            string errmsg = String.Empty;

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (pModel != null && pModel.Enterprise != null)
            {
                EnterpriseHandler eh = new EnterpriseHandler(this.PersonGuid);
                EMDEnterprise ent = (EMDEnterprise)eh.GetObject<EMDEnterprise>(pModel.Enterprise);

                List<EMDEnterprise> kccEnterprises = new EnterpriseManager(this.PersonGuid).GetEnterpriseLeafes(new List<int> { 2000, 1310 });

                if (kccEnterprises.FindLast(e => e.Guid == pModel.Enterprise) == null)
                {
                    ModelState.Remove("PersonnelRequisitionNumber");
                    ModelState.Remove("Simcard");
                    ModelState.Remove("Datacard");
                }
            }
            if (pModel != null && ModelState.IsValid)
            {
                // Workaround, because list of EquipmentDefinitionModel is empty in model
                List<EquipmentDefinitionModel> equipments = new List<EquipmentDefinitionModel>();

                // Read values from form post
                var keys = Request.Form.AllKeys;
                EquipmentDefinitionModel eq = null;
                foreach (string key in keys)
                {

                    if (key.Contains("EquipmentDefinitionModelList"))
                    {
                        if (key.Contains("PackageGuid"))
                        {
                            eq = new EquipmentDefinitionModel() { PackageGuid = Request.Form.GetValues(key)[0].ToString() };
                        }

                        if (key.Contains("IsSelected"))
                        {
                            eq.IsSelected = Boolean.Parse(Request.Form.GetValues(key)[0].ToString());

                            equipments.Add(eq);
                        }
                    }
                }

                ContactManager contMgr = new ContactManager(this.PersonGuid);
                List<EMDContact> listCont = new List<EMDContact>();

                if (!String.IsNullOrWhiteSpace(pModel.Room))
                {
                    EMDContact contRoom = new EMDContact();
                    contRoom.Text = pModel.Room.Trim();
                    contRoom = contMgr.CreateContactRoom(contRoom);
                    listCont.Add(contRoom);
                }

                if (!String.IsNullOrWhiteSpace(pModel.FixedLine))
                {
                    EMDContact contFixedLine = new EMDContact();
                    contFixedLine.Text = pModel.FixedLine.Trim();
                    contFixedLine = contMgr.CreateContactDirectDial(contFixedLine);
                    listCont.Add(contFixedLine);
                }

                if (!String.IsNullOrWhiteSpace(pModel.Extension))
                {
                    EMDContact contExtension = new EMDContact();
                    contExtension.Text = pModel.Extension.Trim();
                    contExtension = contMgr.CreateContactPhone(contExtension);
                    listCont.Add(contExtension);
                }

                if (!String.IsNullOrWhiteSpace(pModel.MobilePhone))
                {
                    EMDContact contMobilephone = new EMDContact();
                    contMobilephone.Text = pModel.MobilePhone.Trim();
                    contMobilephone = contMgr.CreateContactMobile(contMobilephone);
                    listCont.Add(contMobilephone);
                }

                if (!String.IsNullOrWhiteSpace(pModel.JobTitle))
                {
                    EMDContact contJobtitle = new EMDContact();
                    contJobtitle.Text = pModel.JobTitle.Trim();
                    contJobtitle = contMgr.CreateContactJobtitle(contJobtitle);
                    listCont.Add(contJobtitle);
                }



                EmploymentManager emplMgr = new EmploymentManager(this.PersonGuid);
                EMDEmployment empl = new EMDEmployment();
                empl.Entry = pModel.EntryDate;
                empl.FirstWorkDay = pModel.FirstDayOfWork;
                empl.LastDay = pModel.LastDay;
                empl.Exit = pModel.UntilDate;
                empl.PersNr = pModel.PersNr;

                //XDocument xDoc = new XDocument();
                XElement rootKCCData = new XElement("KCCData");

                XElement migration = new XElement("Items");
                migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Personnel requisition number", pModel.PersonnelRequisitionNumber));
                migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("No Approval Needed", pModel.NoApprovalNeeded.ToString()));
                migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Reason for no Approval", pModel.NoApprovalNeededReason));
                migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Simcard", pModel.Simcard.ToString()));
                migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Datacard", pModel.Datacard.ToString()));
                migration.Add(WorkflowHelper.CreateXElementForKCCAdditionalData("Software Equipment", pModel.SoftwareEquipment));


                rootKCCData.Add(migration);

                string requester_pers_guid = ViewData["EMD_pers_guid"].ToString();

                EmploymentHandler emplHandler = new EmploymentHandler(this.PersonGuid);
                EMDEmployment requester_empl = emplHandler.GetMainEmploymentForPerson(requester_pers_guid);


                if (requester_empl != null)
                {
                    try
                    {
                        // TODO: Hannes - Create Method to send a list of all Equipments with their package Guid

                        //pModel.EnterprisePackages == obco guid??
                        List<NewEquipmentInfo> coreEQDEs = pModel.GetEquipmentsFromPackages(new List<string>() { pModel.EnterprisePackages, pModel.LocationPackages });

                        transaction.Begin();

                        OnboardingManager onboardingManager = new OnboardingManager(transaction, this.PersonGuid);

                        EmplAddWorkflowMessage emplAddWorkflowMessage = onboardingManager.PrepareOnboarding(
                            requestingPersonEmplGuid: requester_empl.Guid,
                            empl: empl,
                            effectedPersonGuid: pModel.pers_guid,
                            enteGuid: pModel.Enterprise,
                            locaGuid: pModel.Location,
                            accoGuid: pModel.CostCenter,
                            orguGuid: pModel.OrgUnit,
                            emtyGuid: pModel.EmploymentType,
                            userdomainGuid: pModel.GuidDomain,
                            digrGuid: pModel.DistributionGroup,
                            sponsorGuid: pModel.SponsorGuid,
                            emailType: pModel.EMailType,
                            contactList: listCont,
                            xmlData: rootKCCData,
                            newEquipments: coreEQDEs,
                            leaveFrom: null,
                            leaveTo: null,
                            oldEmplChangeExit: null,
                            businessCase: EnumBusinessCase.Onboarding);
                        //emplAddWorkflowMessage.BusinessCase = EnumBusinessCase.Onboarding;
                        //emplAddWorkflowMessage.ChangeType = EnumEmploymentChangeType.NoChange;


                        transaction.Commit();

                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        try
                        {
                            emplAddWorkflowMessage.CreateWorkflowInstance(this.PersonGuid, MODIFY_COMMENT);
                            stopWatch.Stop();

                            logger.Info(string.Format("The Webservice Call 'CreateWorkflowInstance' (Onboarding) was successfully started for empl {0}. Call time in ms: {1}", emplAddWorkflowMessage.NewEmploymentGuid, stopWatch.ElapsedMilliseconds));
                        }
                        catch (Exception ex)
                        {
                            AddResponseCloseWindow();
                            errmsg = "The Employment was created with status \"NOT SET\", because the Process-Engine didn't response.";
                            ModelState.AddModelError("error", errmsg);
                            try
                            {
                                EMDEmployment newEmployment = new EmploymentManager().GetEmployment(emplAddWorkflowMessage.NewEmploymentGuid);
                                logger.Error("The webservice Call CreateWorkflowInstance failed", ex);
                                if (newEmployment != null)
                                {
                                    // Update the status of the obre
                                    newEmployment.Status = (byte)ProcessStatus.STATUSITEM_NOTSET;

                                    new EmploymentHandler().UpdateObject<EMDEmployment>(newEmployment);
                                    logger.Info(string.Format("Employment {0} was updated to status STATUSITEM_NOTSET.", newEmployment.Guid));
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error("The created Employment couldn't be updated to Status STATUSITEM_NOTSET", e);
                            }
                        }

                    }
                    catch (WorkflowMappingException ex)
                    {
                        handledException = ex;
                        transaction.Rollback();
                        errmsg = "The was no Onboarding-Workflow found, which matches your criteria.<br/>Technical error: " + ex.InnerException?.Message.ToString();
                        ModelState.AddModelError("error", errmsg);
                    }
                    catch (Exception ex)
                    {
                        handledException = ex;
                        transaction.Rollback();
                        errmsg = "Error from Onboarding Service: " + ex.Message;
                        ModelState.AddModelError("error", errmsg);
                    }
                }
                else
                {
                    errmsg = "Requester has no Main-Employment. Onboarding canceled!";
                    ModelState.AddModelError("error", errmsg);
                }
            }


            PartialViewResult result = GetPartialFormWithErrors("Create", pModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = "Person" });
            }

        }





        [Route()]
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            //string submitType = "unknown";

            //if (collection["submit"] != null)
            //{
            //    submitType = "submit";
            //}
            //else if (collection["process"] != null)
            //{
            //    submitType = "process";
            //}
            return View();
        }


        public ActionResult EquipmentDefinitions()
        {
            return View("EquipmentDefinitions");
        }



        [Route("GetEquipmentView")]
        public ActionResult GetEquipmentView(string package1, string package2)
        {
            OnboardingModel onboardingModel = new OnboardingModel();




            onboardingModel.EquipmentDefinitionModelList = EquipmentDefinitionModel.GetEquipmentDefinitionsFromPackages(new List<string>() { package1, package2 });


            return PartialView("EquipmentDefinitions", onboardingModel);


        }


        [Route("ReadForSelectForUserDomains")]
        public ActionResult ReadForSelectForUserDomains(string ente_guid)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                List<EMDUserDomain> userDomains = new UserDomainManager().GetUserDomains();



                foreach (EMDUserDomain userDomain in userDomains)
                {
                    keyValuePairs.Add(new TextValueModel(userDomain.Name, userDomain.Guid));
                }


                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading userdomains";
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