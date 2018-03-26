using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EMD.EMD20Web.Models.Change;
using Kapsch.IS.EMD.EMD20Web.Models.PersonProfile;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("PersonProfile")]
    //[Route("{action = index}")] 
    public class PersonProfileController : BaseController
    {
        public PersonProfileController()
        {
            logger = ISLogger.GetLogger("PersonProfileController");
        }

        [Route]
        // GET: Package
        public ActionResult Index()
        {
            //return View("Manage");
            return RedirectToAction("Profile");
        }

        [Route("ProfileByPId/{PID}")]
        [Route("ProfileByPId/{PID}/{isPartialView}")]
        public ActionResult ProfileByPId(string PID, bool isPartialView = false)
        {

            PersonHandler persHandler = new PersonHandler(this.PersonGuid);
            List<IEMDObject<EMDPerson>> persons = persHandler.GetObjects<EMDPerson, Person>("P_ID = " + PID.ToString());

            if (persons.Count == 0)
            {
                //TODO Implement ErrorHandling
                return null;
            }
            else if (persons.Count > 1)
            {
                //TODO Implement ErrorHandling
                return null;
            }
            else
            {
                return ViewProfile(persons[0].Guid, isPartialView);
            }
        }

        [Route("Profile/{pers_guid:minlength(37)}")]
        [Route("Profile/{pers_guid:minlength(37)}/{isPartialView}")]
        public ActionResult ViewProfile(string pers_guid, bool isPartialView = false)
        {
            SecurityUser securityUser = SecurityUser.NewSecurityUser(this.UserName, pers_guid);
            PersonProfileModel ppm = PersonProfileModel.createObject(pers_guid, securityUser);
            ppm.InitializeSecurity(securityUser);


            if (isPartialView)
            {
                return PartialView("Profile", ppm);
            }
            else
            {
                return View("Profile", ppm);
            }
        }


        [Route("ProfileEmployments")]
        [Route("ProfileEmployments/{pers_guid:minlength(37)}")]
        public ActionResult ProfileEmployments(string pers_guid)
        {
            PersonProfileEmploymentModels EmpProfileModels = new PersonProfileEmploymentModels();
            Exception handledException = null;
            SecurityUser secUser = GetSecurityUserFromCache();
            try
            {
                PersonManager pm = new PersonManager(this.PersonGuid);
                EmploymentHandler eh = new EmploymentHandler(this.PersonGuid);
                eh.DeliverInActive = true;

                EmploymentManager emplmgr = new EmploymentManager(this.PersonGuid);


                EMDPerson person = pm.Get(pers_guid);

                List<EMDEmployment> tempEmployments = eh.GetEmploymentsForPerson(pers_guid).Cast<EMDEmployment>().OrderByDescending(a => a.LastDay).ToList();

                // show permissions for line managers and admins
                List<EMDEmployment> employments = new List<EMDEmployment>();
                foreach (var employment in tempEmployments)
                {
                    if (employment.ActiveTo > DateTime.Now || secUser.hasPermission(string.Empty, new SecurityUserParameterFlags(isLineManager: true), null, emplGuid: employment.Guid) || secUser.hasPermission(SecurityPermission.Personprofile_View_Historical, new SecurityUserParameterFlags(), null, employment.Guid))
                    {
                        employments.Add(employment);
                    }
                }


                EnterpriseHandler enth = new EnterpriseHandler(this.PersonGuid);
                EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler(this.PersonGuid);
                EmploymentAccountHandler empAch = new EmploymentAccountHandler(this.PersonGuid);
                ContactHandler ch = new ContactHandler(this.PersonGuid);
                LocationHandler lh = new LocationHandler(this.PersonGuid);
                AccountHandler ach = new AccountHandler(this.PersonGuid);
                OrgUnitRoleSearch searcher = new OrgUnitRoleSearch();

                ObjectFlagManager ofm = new ObjectFlagManager(this.PersonGuid);

                try
                {
                    foreach (EMDEmployment emp in employments)
                    {
                        PersonProfileEmploymentModel ppem = new PersonProfileEmploymentModel();
                        ppem.Status = emp.Status;
                        ppem.CanOffboard = secUser.hasPermission(SecurityPermission.Offboarding, new SecurityUserParameterFlags(checkPlainPermisson: true), null, emp.Guid);
                        ppem.CanChange = secUser.hasPermission(SecurityPermission.Change, new SecurityUserParameterFlags(checkPlainPermisson: true), null, emp.Guid);

                        // bool offboardingIsMainDisabled = emplmgr.CheckIfOffboardingAllowed(emp.Guid).Count > 0;
                        ppem.IsOffboardingDisabled = ppem.IsWorkflowDisabled;

                        if (ppem.IsWorkflowDisabled)
                        {
                            ppem.DisabledTitle = "Another worfklow process is running";
                        }


                        bool IsAllowedEmployment = secUser.IsAllowedEmployment(secUser.UserId, emp.Guid);
                        bool emplIsActive = false;
                        bool emplIsPast = false;
                        bool emplIsFuture = false;
                        if (emp.Entry > DateTime.Now && emp.FirstWorkDay > DateTime.Now)
                        {
                            emplIsFuture = true;
                        }
                        else if (emp.Exit < DateTime.Now && emp.LastDay < DateTime.Now)
                        {
                            emplIsPast = true;
                        }
                        else
                        {
                            emplIsActive = true;
                        }

                        bool mayManage = (ppem.CanOffboard || ppem.CanChange);
                        bool showEmployment = false;

                        showEmployment = (showEmployment || ((emplIsFuture || emplIsActive || emplIsPast) && mayManage));
                        showEmployment = (showEmployment || (emplIsActive && (ofm.IsEmploymentVisibleInPhonebook(emp.Guid) || secUser.hasPermission(String.Empty, new SecurityUserParameterFlags(isItself: true), null, emp.Guid)))
                            || secUser.hasPermission(SecurityPermission.AdvancedSearch_View, new SecurityUserParameterFlags(checkPlainPermisson: true)));

                        if (showEmployment)
                        {
                            // search for historical entries if it is an old employment
                            searcher.ActiveTo = emp.ActiveTo;
                            searcher.ValidTo = emp.ValidTo;
                            searcher.ValidFrom = emp.ValidFrom;
                            searcher.ActiveFrom = emp.ActiveFrom;


                            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(emp.ENLO_Guid);
                            EMDEnterprise enterprise = (EMDEnterprise)enth.GetObject<EMDEnterprise>(enlo.E_Guid);


                            EMDEmploymentAccount empAcc = (EMDEmploymentAccount)empAch.GetMainEmploymentAccount(emp.Guid);
                            EMDLocation loc = (EMDLocation)lh.GetObject<EMDLocation>(enlo.L_Guid);

                            ppem.ElId = string.Empty;
                            if (loc != null && loc.EL_ID != null)
                            {
                                ppem.ElId = loc.EL_ID.ToString();
                            }
                            EmploymentTypeHandler eth = new EmploymentTypeHandler(this.PersonGuid);
                            EMDEmploymentType et = (EMDEmploymentType)eth.GetObject<EMDEmploymentType>(emp.ET_Guid);
                            OrgUnitRoleHandler orh = new OrgUnitRoleHandler(this.PersonGuid);
                            OrgUnitHandler oh = new OrgUnitHandler(this.PersonGuid);
                            RoleHandler rh = new RoleHandler(this.PersonGuid);

                            ppem.CanViewUser = secUser.hasPermission(SecurityPermission.Personprofile_User_View, new SecurityUserParameterFlags(isItself: true, checkPlainPermisson: true), null, emp.Guid);
                            ppem.CanManageUser = secUser.hasPermission(SecurityPermission.Personprofile_User_View_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true), null, emp.Guid);
                            ppem.CanManageContactData = IsAllowedEmployment && secUser.hasPermission(SecurityPermission.ContactManager_View_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true), null, emp.Guid);
                            ppem.ShowGuidEntities = secUser.hasPermission(SecurityPermission.Personprofile_View_Guids_View, new SecurityUserParameterFlags(checkPlainPermisson: true), null, emp.Guid);
                            ppem.IsSelf = secUser.IsItSelf(emp.Guid);

                            string assistences = string.Empty;
                            List<EMDEmployment> assistenceList = emplmgr.GetAssistence(emp);

                            assistenceList.ForEach(item =>
                            {
                                if (assistences.Length > 0)
                                {
                                    assistences += ", ";
                                }
                                EMDPerson persAssistence = pm.Get(item.P_Guid);
                                //assistences += "<a target='_blank' href='/PersonProfile/Profile/" + persAssistence.Guid  + "'>" + persAssistence.FamilyName + " " + persAssistence.FirstName + "</a> ";
                                assistences += "<a target='_blank' href='/PersonProfile/Profile/" + persAssistence.Guid + "'>" + pm.getFullDisplayNameWithUserId(persAssistence) + "</a> "; ;
                            }
                            );

                            ppem.Assistance = assistences;
                            if (empAcc != null)
                            {

                                EMDAccount Acc = (EMDAccount)ach.GetObject<EMDAccount>(empAcc.AC_Guid);
                                if (Acc != null)
                                {
                                    ppem.CostCenterKstID = Acc.KstID;
                                    ppem.CostCenterName = Acc.KstID + " - " + Acc.Name;
                                    ppem.CostCenterAcId = Acc.AC_ID.ToString();
                                    EMDEmployment costCenterResponsibleEmpl = ach.GetResponsible(Acc.Guid);
                                    if (costCenterResponsibleEmpl != null)
                                    {
                                        EMDPerson costCenterResponsiblePers = pm.GetPersonByEmployment(costCenterResponsibleEmpl.Guid);
                                        if (costCenterResponsiblePers != null)
                                        {
                                            ppem.CostCenterResponsible = pm.getFullDisplayNameWithUserId(costCenterResponsiblePers);
                                            ppem.CostCenterResponsibleGuid = costCenterResponsiblePers.Guid;
                                        }
                                    }
                                }
                            }

                            ppem.JobTitle = ch.GetContactStringByContactType(emp.Guid, ContactTypeHandler.JOBTITLE);
                            ppem.JobTitleFuture = ContactModel.GetFutureTextInfo(ch.GetContactByContactType(emp.Guid, ContactTypeHandler.JOBTITLE, true));
                            ppem.DirectPhone = ch.GetContactStringByContactType(emp.Guid, ContactTypeHandler.DIRECTDIAL);
                            ppem.DirectPhoneFuture = ContactModel.GetFutureTextInfo(ch.GetContactByContactType(emp.Guid, ContactTypeHandler.DIRECTDIAL, true));
                            ppem.eFax = ch.GetContactStringByContactType(emp.Guid, ContactTypeHandler.EFAX);
                            ppem.eFaxFuture = ContactModel.GetFutureTextInfo(ch.GetContactByContactType(emp.Guid, ContactTypeHandler.EFAX, true));
                            if (et != null)
                            {
                                //TODO EmplomentType einbinden!!!
                                ppem.EmploymentType = et.Name + " (" + Enum.GetName(typeof(EnumEmploymentTypeCategory), et.ETC_ID) + ")";
                                ppem.EmploymentTypeShort = et.Name;
                                //ppem.EmploymentType = et.Name + " (" + Enum.GetName(typeof(EMDEmploymentTypeCategory), EMDEmploymentTypeCategory.External) + ")";
                            }
                            else
                                ppem.EmploymentType = "";

                            if (enterprise != null)
                            {
                                ppem.ente_guid = enterprise.Guid;
                                if (enterprise.NameShort != null)
                                {
                                    ppem.Enterprise = enterprise.NameShort;
                                    ppem.EnterpriseNameShort = enterprise.NameShort;
                                }
                                if (enterprise.NameShort != null)
                                {
                                    ppem.Enterprise += " - " + enterprise.NameLong;
                                }
                            }

                            ppem.EP_ID = emp.EP_ID;
                            ppem.Fax = ch.GetContactStringByContactType(emp.Guid, ContactTypeHandler.DIRECTEFAX);
                            ppem.FaxFuture = ContactModel.GetFutureTextInfo(ch.GetContactByContactType(emp.Guid, ContactTypeHandler.DIRECTEFAX, true));
                            ppem.Guid = emp.Guid;
                            // ppem.IsHistorical = emp.ActiveTo < DateTime.Now;
                            ppem.IsHistorical = emplIsPast;
                            ppem.HtmlCssClass = string.Empty;
                            ppem.HtmlHoverText = string.Empty;
                            if (ppem.IsHistorical)
                            {
                                // ppem.EnterpriseNameShort = string.Format("* {0}", ppem.EnterpriseNameShort);
                                ppem.HtmlCssClass = "tabHistorical";
                                ppem.HtmlHoverText = "This is an historical entry";
                            }



                            List<string> supervisors = new List<string>();
                            try
                            {
                                supervisors = searcher.SearchOrgUnitRoleForEmployment(RoleHandler.LINEMANAGER, emp.Guid);
                                if (supervisors.Count > 0)
                                {
                                    EMD.Data.EmploymentPerson supervisor = new EMD.Data.EmploymentPerson(supervisors[0]);
                                    if (supervisor != null)
                                    {
                                        ppem.LineManager = pm.getFullDisplayNameWithUserId(supervisor.person);
                                        ppem.LineManagerGuid = supervisor.person.Guid;
                                    }

                                    if (string.IsNullOrWhiteSpace(ppem.LineManager))
                                    {
                                        ppem.LineManager = "Data not available - please contact your business-prime!";
                                    }
                                }
                                else
                                {
                                    ppem.LineManager = "Data not available - please contact your business-prime!";
                                }
                            }
                            catch (Exception ex)
                            {
                                //handledException = ex;
                                ppem.LineManager = "Data not available - please contact your business-prime!";
                                logger.Error("SearchOrgUnitRoleForEmployment failed for Linemanager", ex);
                            }

                            try
                            {
                                supervisors = searcher.SearchOrgUnitRoleForEmployment(RoleHandler.TEAMLEADER, emp.Guid);
                                if (supervisors.Count > 0)
                                {
                                    EMD.Data.EmploymentPerson supervisor = new EMD.Data.EmploymentPerson(supervisors[0]);
                                    if (supervisor != null)
                                    {
                                        ppem.Teamleader = pm.getFullDisplayNameWithUserId(supervisor.person);
                                        ppem.TeamleaderGuid = supervisor.person.Guid;
                                    }

                                    if (string.IsNullOrWhiteSpace(ppem.Teamleader))
                                    {
                                        ppem.Teamleader = "Data not available - please contact your business-prime!";
                                    }
                                }
                                else
                                {
                                    ppem.Teamleader = "Data not available - please contact your business-prime!";
                                }
                            }
                            catch (Exception ex)
                            {
                                //handledException = ex;
                                ppem.Teamleader = "Data not available - please contact your business-prime!";
                                logger.Warn("SearchOrgUnitRoleForEmployment failed for Teamleader", ex);
                            }

                            if (loc != null)
                            {
                                CountryHandler countryH = new CountryHandler(this.PersonGuid);
                                EMDCountry country = (EMDCountry)countryH.GetObject<EMDCountry>(loc.CTY_Guid);
                                string UN_RoadCode = String.Empty;
                                if (country != null)
                                    UN_RoadCode = country.UN_RoadCode;

                                ppem.Location = "Object ";
                                if (loc.EL_ID != null)
                                    ppem.Location += loc.EL_ID.ToString() + " - ";

                                if (loc.Street != null)
                                    ppem.Location += loc.Street + ", ";

                                ppem.Location += UN_RoadCode + "-";

                                if (loc.ZipCode != null)
                                    ppem.Location += loc.ZipCode + " ";

                                if (loc.Name != null)
                                    ppem.Location += loc.Name;

                                ppem.Mobile = ch.GetContactStringByContactType(emp.Guid, ContactTypeHandler.MOBILE);
                                ppem.MobileFuture = ContactModel.GetFutureTextInfo(ch.GetContactByContactType(emp.Guid, ContactTypeHandler.MOBILE, true));
                            }

                            EMDRole r = null;
                            try
                            {
                                r = (EMDRole)rh.GetRoleById(RoleHandler.PERSON);
                            }
                            catch (Exception ex)
                            {
                                handledException = ex;
                                logger.Error("Get RoleById failed for " + RoleHandler.PERSON, ex);
                            }

                            if (r != null)
                            {
                                try
                                {
                                    EMDOrgUnitRole our = (EMDOrgUnitRole)orh.GetOrgUnitRole(emp.Guid, r.Guid);
                                    if (our != null)
                                    {
                                        EMDOrgUnit ou = (EMDOrgUnit)oh.GetObject<EMDOrgUnit>(our.O_Guid);
                                        if (secUser.hasPermission("OrgUnitManager.View", new SecurityUserParameterFlags(checkPlainPermisson: true)))
                                        {
                                            ppem.OrgUnit = "<a target='_blank' href=\"/OrgUnitRole/Manage/false?Guid=" + our.O_Guid + "\">" + ou.Name + "</a>";
                                        }
                                        else
                                        {
                                            ppem.OrgUnit = ou.Name;
                                        }

                                    }
                                }
                                catch (Exception)
                                {
                                    throw;
                                }

                            }

                            ppem.PersonalNumber = emp.PersNr;
                            ppem.Phone = ch.GetContactStringByContactType(emp.Guid, ContactTypeHandler.PHONE);
                            ppem.PhoneFuture = ContactModel.GetFutureTextInfo(ch.GetContactByContactType(emp.Guid, ContactTypeHandler.PHONE, true));
                            ppem.Room = ch.GetContactStringByContactType(emp.Guid, ContactTypeHandler.ROOM);
                            ppem.RoomFutureNumber = ch.GetContactByContactType(emp.Guid, ContactTypeHandler.ROOM, true)?.Text;
                            ppem.RoomFuture = ContactModel.GetFutureTextInfo(ch.GetContactByContactType(emp.Guid, ContactTypeHandler.ROOM, true));

                            if (!String.IsNullOrWhiteSpace(emp.Sponsor_Guid))
                            {
                                EMDPerson sponsor = pm.GetPersonByEmployment(emp.Sponsor_Guid);
                                if (sponsor != null)
                                {
                                    ppem.Sponsor = pm.getFullDisplayNameWithUserId(sponsor);
                                    ppem.SponsorGuid = sponsor.Guid;
                                }
                            }

                            if (emp.Entry != null)
                            {
                                ppem.StartDate = Convert.ToDateTime(emp.Entry);
                                ppem.LeaveFrom = Convert.ToDateTime(emp.LeaveFrom);
                                ppem.LeaveTo = Convert.ToDateTime(emp.LeaveTo);
                            }
                            ppem.IsMainEmployment = new ObjectFlagManager().IsMainEmployment(emp.Guid);


                            if (emplIsPast)
                            {
                                ppem.HtmlCssClass = "tabHistorical";
                                ppem.HtmlHoverText = "This is an historical entry";
                            }
                            else if (ppem.IsMainEmployment)
                            {
                                ppem.HtmlCssClass = "tabIsMain";
                                ppem.HtmlHoverText = "This is the main employment";
                            }
                            else
                            {
                                ppem.HtmlCssClass = "tabIsAdditional";
                            }

                            ppem.ExitDate = emp.Exit;

                            EmpProfileModels.Add(ppem);
                        }


                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    logger.Error("SearchOrgUnitRoleForEmployment failed for Linemanager", ex);
                }


            }
            catch (Exception ex)
            {
                handledException = ex;
                logger.Error("Unhandled Exception for loop in EMDEmployments", ex);
            }

            AddSecurityUserToCache(secUser);

            //return View(EmpProfileModels);

            if (handledException == null)
            {
                return View(EmpProfileModels);
            }
            else
            {
                return GetNoDataFoundView(handledException);
            }

        }

        [Route("ProfilePackages")]
        public ActionResult ProfilePackages(string pers_guid, string empl_guid, string obre_guid)
        {
            EmploymentPackageTabModels employmentPackageTabModels = null;
            Exception handledException = null;
            try
            {
                employmentPackageTabModels = new EmploymentPackageTabModels(pers_guid, SecurityUser.NewSecurityUser(this.UserName, pers_guid));
                if (!string.IsNullOrWhiteSpace(empl_guid))
                {
                    employmentPackageTabModels.SetSelectedPackageTabEquipment(empl_guid, obre_guid);
                }
                return PartialView("ProfilePackages", employmentPackageTabModels);
            }
            catch (Exception ex)
            {
                handledException = ex;
                logger.Error("Error creating employmentPackageTabModel for " + pers_guid, ex);
            }

            if (handledException == null)
            {
                return View(employmentPackageTabModels);
            }
            else
            {
                return GetNoDataFoundView(handledException);
            }

            //try
            //{
            //PersonHandler ph = new PersonHandler();
            //EMDPerson per = (EMDPerson) ph.GetObject<EMDPerson>(pers_guid);

            //EmploymentHandler eh = new EmploymentHandler();
            //List<IEMDObject<EMDEmployment>> employments = eh.GetEmploymentsForPerson(pers_guid);

            //EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();

            //foreach (EMDEmployment emp in employments)
            //{
            //    EnterpriseHandler enth = new EnterpriseHandler();
            //    EMDEnterpriseLocation enlo = (EMDEnterpriseLocation) enloHandler.GetObject<EMDEnterpriseLocation>(emp.ENLO_Guid);

            //    if (enlo != null)
            //    {
            //        try
            //        {
            //            //EMDEnterprise enterprise = (EMDEnterprise) enth.GetObject<EMDEnterprise>(enlo.E_Guid);
            //            //EmploymentPackageTabModel etm = new EmploymentPackageTabModel();
            //            //ObjectFlagManager ofm = new ObjectFlagManager();
            //            //etm.IsMainEmployment = ofm.IsMainEmployment(emp.Guid);
            //            //etm.EP_Guid = emp.Guid;

            //            //etm.E_Guid = enterprise.Guid;
            //            //etm.EnterpriseNameShort = enterprise.NameShort;
            //            //SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            //            //etm.CanManagePackages = secUser.hasPermission("Personprofile.View.Packages.Manage", new SecurityUserParameterFlags(checkPlainPermisson: true));
            //            EmploymentPackageTabModel eptm = EmploymentPackageTabModel.CreateObject(emp, this.UserName);
            //            employmentPackageTabModels.Add(eptm);
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.Error("ProfilePackages loop for employment failed", ex);
            //        }
            //    }
            //}
            //}
            //catch (Exception ex)
            //{
            //    logger.Error("Unhandled Exception for ProfilePackages", ex);
            //}

            //ToDo Implement Sorting by MainEmployment in EmploymentTabModels


        }

        [Route("ProfilePackage")]
        public ActionResult ProfilePackage(EmploymentTabModel tab)
        {
            tab.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName, tab.EP_Guid));
            return PartialView(tab);
        }

        [Route("ProfilePackageAddPackageToEmployment/{ep_guid}")]
        [Route("ProfilePackageAddPackageToEmployment/{ep_guid}/{isPartialView}")]
        public ActionResult ProfilePackageAddPackageToEmployment(string ep_guid, bool isPartialView = false)
        {
            EmploymentHandler empHandler = new EmploymentHandler(this.PersonGuid);
            EMDEmployment emp = (EMDEmployment)empHandler.GetObject<EMDEmployment>(ep_guid);
            AddPackageToEmploymentModel empTabModel = new AddPackageToEmploymentModel();
            empTabModel.EP_Guid = ep_guid;

            if (isPartialView)
            {
                return PartialView("ProfilePackageAddPackageToEmployment", empTabModel);
            }
            else
            {
                return View("ProfilePackageAddPackageToEmployment", empTabModel);
            }
        }

        [Route("DoProfilePackageAddPackageToEmployment")]
        [Route("DoProfilePackageAddPackageToEmployment/{ep_guid}/{oc_guid}")]
        //public ActionResult DoProfilePackageAddPackageToEmployment(string ep_guid, string oc_guid)
        public ActionResult DoProfilePackageAddPackageToEmployment(AddPackageToEmploymentModel model)
        {
            Exception handledException = null;
            InitializeCurrentUserProperties();
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName, model.EP_Guid));
            if (!model.CanManage)
            {
                errmsg = "The Package could not be added: " + SecurityHelper.NoPermissionText;
                ModelState.AddModelError("error", errmsg);
            }

            if (model != null && ModelState.IsValid)
            {
                CoreTransaction transaction = new CoreTransaction();
                transaction.Begin();
                try
                {
                    EmploymentManager em = new EmploymentManager(transaction, this.PersonGuid);
                    EmploymentHandler emplHandler = new EmploymentHandler(transaction, this.PersonGuid);

                    EMDEmployment requesterEmpl = null;
                    string emdpersguid = null;
                    if (ViewData != null)
                    {

                        emdpersguid = this.PersonGuid;//ViewData["EMD_pers_guid"];
                        if (emdpersguid != null)
                        {
                            requesterEmpl = emplHandler.GetMainEmploymentForPerson(emdpersguid.ToString());
                            if (requesterEmpl == null)
                            {
                                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No MainEmployment found for " + emdpersguid.ToString());
                            }
                        }
                        else
                        {
                            throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No EMD_pers_guid found in ViewData ");
                        }
                    }
                    else
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "ViewData is null");
                    }


                    //  this.service.AddPackageToEmployment(model.EP_Guid, model.oc_guid, requesterEmpl.Guid);
                    ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(this.PersonGuid);
                    //        ObjectContainerContentHandler obccH = new ObjectContainerContentHandler(this.UserGuid);

                    List<EMDObjectRelation> equipmentInstances = em.AddPackageToEmploymentWithEquipments(transaction, model.EP_Guid, model.oc_guid, requesterEmpl.Guid);

                    //       List<IEMDObject<EMDObjectContainerContent>> allObjContContent = (List<IEMDObject<EMDObjectContainerContent>>)obccH.GetObjects<EMDObjectContainerContent, ObjectContainerContent>("OC_Guid = \"" + model.oc_guid + "\"", null);

                    transaction.Commit();


                    foreach (EMDObjectRelation objectRelation in equipmentInstances)
                    {
                        ObreAddWorkflowMessage message = em.GetWorkflowVariablesForExistingEquipmentInstanceEmployment(transaction,
                                objectRelation.Object1,
                                requesterEmpl.Guid,
                                objectRelation.Guid,
                                objectRelation.Object2,
                                model.TargetDate.Value);
                        message.BusinessCase = EnumBusinessCase.EquipmentRequest;



                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        try
                        {
                            message.CreateWorkflowInstance(this.PersonGuid, MODIFY_COMMENT);
                            stopWatch.Stop();

                            logger.Info(string.Format("The Webservice Call 'CreateWorkflowInstance' was successfully started for OBRE {0}. Call time in ms: {1}", objectRelation.Guid, stopWatch.ElapsedMilliseconds));
                        }
                        catch (Exception ex)
                        {
                            AddResponseCloseWindow();
                            errmsg = "Equipments were created with status \"NOT SET\", because the Process-Engine didn't response.";

                            ModelState.AddModelError("error", errmsg);
                            try
                            {
                                logger.Error("The webservice Call CreateWorkflowInstance failed", ex);

                                // Update the status of the obre
                                objectRelation.Status = (byte)EquipmentStatus.STATUSITEM_NOTSET;
                                ObjectRelationHandler obreH = new ObjectRelationHandler(this.PersonGuid);
                                obreH.UpdateObject(objectRelation);
                                logger.Info(string.Format("OBRE {0} was updated to status STATUSITEM_NOTSET.", objectRelation.Guid));

                            }
                            catch (Exception e)
                            {
                                logger.Error("The created OBRE couldn't be updated to Status STATUSITEM_NOTSET", e);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    errmsg = "The Package could not be added: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("ProfilePackageAddPackageToEmployment", model, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { success = "success", errorMessage = errmsg });
            }
        }

        [Route("ProfilePackageAddEquipmentToEmployment/{ep_guid}")]
        [Route("ProfilePackageAddEquipmentToEmployment/{ep_guid}/{isPartialView}")]
        public ActionResult ProfilePackageAddEquipmentToEmployment(string ep_guid, bool isPartialView = false)
        {
            EmploymentHandler empHandler = new EmploymentHandler(this.PersonGuid);
            EMDEmployment emp = (EMDEmployment)empHandler.GetObject<EMDEmployment>(ep_guid);
            AddEquipmentToEmploymentModel empTabModel = new AddEquipmentToEmploymentModel();
            empTabModel.ActingUserPersGuid = this.PersonGuid;
            empTabModel.Empl_Guid = ep_guid;

            empTabModel.EquipmentSelection = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "obre_guid",
                ObjectLabel = "Equipment",
                TargetControllerMethodName = "ReadAvailableListOfEquipmentDefinitionsForEmploymentDs",
                TargetControllerName = "Employment",
                ClientTemplate = "<div>#=Text# - <span style=\"font-size:0.9em\">#=Data#</span></div>",
                TargetOptionalMethodParameters = "personprofile.data.GetEmploymentGuid",
                ControlHeight = "80%",
                ControlWidth = "90%",
                HideDeleteButton = true,
                GridHeight = "600px"
            };

            if (isPartialView)
            {
                return PartialView("ProfilePackageAddEquipmentToEmployment", empTabModel);
            }
            else
            {
                return View("ProfilePackageAddEquipmentToEmployment", empTabModel);
            }
        }

        [Route("DoProfilePackageAddEquipmentToEmployment")]
        [Route("DoProfilePackageAddEquipmentToEmployment/{ep_guid}/{obre_guid}")]
        //public ActionResult DoProfilePackageAddEquipmentToEmployment(string ep_guid, string obre_guid)
        public ActionResult DoProfilePackageAddEquipmentToEmployment(AddEquipmentToEmploymentModel model)
        {
            Exception handledException = null;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName, model.Empl_Guid));
            if (!model.CanManage && !model.IsEquipmentOwner)
            {
                errmsg = "The Equipment could not be added: " + SecurityHelper.NoPermissionText;
                ModelState.AddModelError("error", errmsg);
            }

            if (model != null && ModelState.IsValid)
            {
                CoreTransaction transaction = new CoreTransaction();
                transaction.Begin();
                try
                {
                    EmploymentManager em = new EmploymentManager(transaction, this.PersonGuid);

                    EmploymentHandler emplHandler = new EmploymentHandler(transaction, this.PersonGuid);
                    EMDEmployment requesterEmpl = emplHandler.GetMainEmploymentForPerson(ViewData["EMD_pers_guid"].ToString());

                    if (requesterEmpl == null)
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No MainEmployment found for " + ViewData["EMD_pers_guid"].ToString());
                    }

                    EMDObjectRelation equipmentInstance = em.AddEquipmentToEmployment(transaction, model.Empl_Guid, model.obre_guid, requesterEmpl.Guid);

                    ObreAddWorkflowMessage message = em.GetWorkflowVariablesForExistingEquipmentInstanceEmployment(
                        transaction, model.Empl_Guid, requesterEmpl.Guid, equipmentInstance.Guid, model.obre_guid, model.TargetDate.Value);


                    message.BusinessCase = EnumBusinessCase.EquipmentRequest;

                    transaction.Commit();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    try
                    {
                        message.CreateWorkflowInstance(this.PersonGuid, MODIFY_COMMENT);
                        stopWatch.Stop();

                        logger.Info(string.Format("The Webservice Call 'CreateWorkflowInstance' was successfully started for OBRE {0}. Call time in ms: {1}", equipmentInstance.Guid, stopWatch.ElapsedMilliseconds));
                    }
                    catch (Exception ex)
                    {
                        AddResponseCloseWindow();
                        errmsg = "The Equipment was created with status \"NOT SET\", because the Process-Engine didn't response.";
                        ModelState.AddModelError("error", errmsg);
                        try
                        {
                            logger.Error("The webservice Call CreateWorkflowInstance failed", ex);
                            if (equipmentInstance != null)
                            {
                                // Update the status of the obre
                                equipmentInstance.Status = (byte)EquipmentStatus.STATUSITEM_NOTSET;
                                ObjectRelationHandler obreH = new ObjectRelationHandler(this.PersonGuid);
                                obreH.UpdateObject(equipmentInstance);
                                logger.Info(string.Format("OBRE {0} was updated to status STATUSITEM_NOTSET.", equipmentInstance.Guid));
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("The created OBRE couldn't be updated to Status STATUSITEM_NOTSET", e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    errmsg = "The Equipment could not be added: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("ProfilePackageAddEquipmentToEmployment", model, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { success = "success", errorMessage = errmsg });
            }
        }

        [HttpPost]
        [Route("UploadPersonImage")]
        public ActionResult UploadPersonImage(IEnumerable<HttpPostedFileBase> imageFiles, string userId, string persGuid)
        {
            try
            {
                // The Name of the Upload component is "files"
                if (imageFiles != null)
                {
                    string directoryPath = String.Empty;
                    int PersonPortraitJobId = 0;
                    Image image = null;
                    Size size = new System.Drawing.Size(180, 230); //The dafault size from PersonPortrait is 180x230
                    string PersonPortraitUploadPath = null;
                    string PersonPortraitPath = null;

                    directoryPath = ConfigurationManager.AppSettings["EMD20Web.FolderPathAdPersonImage"].ToString();

                    //Get the EMD20Web.FolderPathAdPersonImage from Web.config
                    try
                    {
                        //PicturePath = ConfigurationManager.AppSettings["EMD20Web.FolderPathAdPersonImage"].ToString() + this.FileUpload.FileName;
                        PersonPortraitUploadPath = ConfigurationManager.AppSettings["EMD20Web.FolderPathAdPersonImage"].ToString() + @"PersonPortraitUploadFolder\" + userId + ".jpg";
                        PersonPortraitPath = ConfigurationManager.AppSettings["EMD20Web.FolderPathAdPersonImage"].ToString() + userId + ".jpg";
                    }
                    catch (Exception ex)
                    {
                        string error = "Can't read web.config key: EMD20Web.FolderPathAdPersonImage).";
                        logger.Error(error, ex);
                        return Content(error + " - " + ex.Message);
                    }

                    //Get the ActiveDirectoryPersonPictureJobID from Web.config
                    try
                    {
                        PersonPortraitJobId = Convert.ToInt32(ConfigurationManager.AppSettings["EMD20Web.PersonPictureJobID"].ToString());

                        if (PersonPortraitJobId == 0)
                        {
                            string error = "The web.config key: ActiveDirectoryPersonPictureJobID = 0. Can't find a Job with ID 0.";
                            logger.Error(error);
                            return Content(error);
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = "Can't read web.config key: ActiveDirectoryPersonPictureJobID).";
                        logger.Error(error, ex);
                        return Content(error + " - " + ex.Message);
                    }

                    //Save the uploaded file
                    try
                    {
                        //Save the image
                        if (imageFiles.Count() > 0)
                        {
                            var fileName = Path.GetFileName(imageFiles.FirstOrDefault().FileName);
                            imageFiles.FirstOrDefault().SaveAs(PersonPortraitUploadPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = "Can't save PersonPicture for Active Directory in: " + PersonPortraitUploadPath;
                        logger.Error(error, ex);
                        return Content(error + " - " + ex.Message.ToString());
                    }


                    //Load the image
                    try
                    {
                        image = System.Drawing.Image.FromFile(PersonPortraitUploadPath);
                    }
                    catch (Exception ex)
                    {
                        if (image != null)
                            image.Dispose();

                        string error = "Can't load PersonPicture for Active Directory in: " + PersonPortraitUploadPath;
                        logger.Error(error, ex);
                        return Content(error + " - " + ex.Message.ToString());
                    }

                    //Check the image dimensions
                    if (image.Height == size.Height && image.Width == size.Width)
                    {
                        try
                        {   //SaveMD5HashToPersonPortraitTableInDatabase
                            //PersonManager persManager = new PersonManager();
                            this.SaveMD5GUIHashToPersonPortrait(image, persGuid, this.UserName, PersonPortraitJobId, "");
                            //Kapsch.IS.EDP.Core.PersonPortraitHelper.SaveMD5GUIHashToPersonPortrait(image, P_ID, this.UserName, PersonPortraitJobId, "");

                            //Dispose the image
                            if (image != null)
                                image.Dispose();

                        }
                        catch (Exception ex)
                        {
                            if (image != null)
                                image.Dispose();

                            string error = "Can't load PersonPicture for Active Directory in: " + PersonPortraitUploadPath;
                            logger.Error(error, ex);
                            return Content(error + " - " + ex.Message.ToString());
                        }

                        //Add row in JobQueue --> TODO --> see image_helper.cs in edpcore


                        try
                        {
                            //Copy PersonPortrait 
                            System.IO.File.Copy(PersonPortraitUploadPath, PersonPortraitPath, true);
                        }
                        catch (Exception ex)
                        {
                            string error = "Can't copy PersonPicture for Active Directory: " + PersonPortraitPath + "User: " + userId;
                            logger.Error(error, ex);
                            return Content(error + " - " + ex.Message.ToString());
                        }

                        try
                        {
                            //Delete PersonPortrait                    
                            System.IO.File.Delete(PersonPortraitUploadPath);
                        }
                        catch (Exception ex)
                        {
                            string error = "Can't delete PersonPicture for Active Directory: " + PersonPortraitUploadPath + "User: " + userId;
                            logger.Error(error, ex);
                            return Content(error + " - " + ex.Message.ToString());
                        }
                    }
                    else
                    {
                        //Write warning to Gui
                        //writePictureUploadWarningToGUI("The Personportrait is not valid. The size should be 180x230px.");
                        BaseException be = new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The Personportrait is not valid. The size should be 180x230px.");
                        throw be;


                    }
                }
                return Content("");

            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message.ToString());
            }

            // Return an empty string to signify success

        }


        [HttpGet]
        [Route("RemoveEquipment")]
        [Route("RemoveEquipment/{empl_guid}/{obre_guid}/{isPartialView}")]
        [Route("RemoveEquipment/{empl_guid}/{obre_guid}")]
        public ActionResult RemoveEquipment(string empl_guid, string obre_guid, bool isPartialView = false)
        {
            RemoveEquipmentModel equipmentRemoveModel = new RemoveEquipmentModel();
            equipmentRemoveModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName, empl_guid));
            try
            {
                if (!string.IsNullOrEmpty(empl_guid) && !string.IsNullOrEmpty(obre_guid))
                {
                    equipmentRemoveModel.GuidEmployment = empl_guid;
                    equipmentRemoveModel.GuidObre = obre_guid;

                    EMDObjectRelation emdObjectRelation = (EMDObjectRelation)new ObjectRelationHandler(this.PersonGuid).GetObject<EMDObjectRelation>(obre_guid);


                    EMDEquipmentDefinition emdEquipmentDefinition = (EMDEquipmentDefinition)new EquipmentDefinitionHandler(this.PersonGuid).GetObject<EMDEquipmentDefinition>(emdObjectRelation.Object2);

                    equipmentRemoveModel.EquipmentName = emdEquipmentDefinition.Name;

                    equipmentRemoveModel.GuidEquipmentDefinition = ((EMDObjectRelation)new ObjectRelationHandler(this.PersonGuid).GetObject<EMDObjectRelation>(obre_guid)).Object2;

                }
                else
                {
                    throw new Exception("No URL Parameters found!");
                }

            }
            catch (Exception e)
            {
                logger.Error(e);
            }



            if (isPartialView)
            {
                return PartialView("RemoveEquipment", equipmentRemoveModel);
            }
            else
            {
                return View("RemoveEquipment", equipmentRemoveModel);
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        [Route("RemoveEquipment")]
        public ActionResult RemoveEquipment(RemoveEquipmentModel equipmentRemoveModel)
        {
            Exception handledException = null;
            string errmsg = "The Equipment couldn't be removed. Please check all comments on the depending fields.";
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (equipmentRemoveModel != null && ModelState.IsValid)
            {
                CoreTransaction transaction = new CoreTransaction();
                try
                {
                    AddEquipmentToEmploymentModel model = new AddEquipmentToEmploymentModel();
                    model.Empl_Guid = equipmentRemoveModel.GuidEmployment;
                    model.obre_guid = equipmentRemoveModel.GuidObre;
                    model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName, equipmentRemoveModel.GuidEmployment));

                    if (!model.CanManage && !model.IsEquipmentOwner)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    if (!equipmentRemoveModel.DoDeleteWithoutProcesses)
                    {
                        transaction.Begin();


                        ObreRemoveWorkflowMessage obreRemoveMessage = WorkflowMessageHelper.GetObreRemoveWorkflowMessage(
                            equipmentRemoveModel.GuidEmployment,
                            this.UserMainEmplGuid,
                            equipmentRemoveModel.TargetDate.Value,
                            EnumEmploymentChangeType.NoChange,
                            EnumBusinessCase.EquipmentRequest,
                            this.PersonGuid,
                            equipmentRemoveModel.GuidObre,
                            equipmentRemoveModel.GuidEquipmentDefinition,
                            false,
                            transaction);
                        obreRemoveMessage.BusinessCase = EnumBusinessCase.EquipmentRequest;

                        transaction.Commit();


                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();


                        try
                        {
                            obreRemoveMessage.CreateWorkflowInstance(this.PersonGuid, MODIFY_COMMENT);
                            stopWatch.Stop();

                            logger.Info(string.Format("The Webservice Call 'CreateWorkflowInstance' (GetObreRemoveWorkflowMessage) was successfully started for OBRE {0}. Call time in ms: {1}", obreRemoveMessage.ObreGuid, stopWatch.ElapsedMilliseconds));
                        }
                        catch (Exception ex)
                        {
                            EMDObjectRelation equipmentInstance = (EMDObjectRelation)new ObjectRelationHandler(transaction).GetObject<EMDObjectRelation>(obreRemoveMessage.ObreGuid);
                            errmsg = "The Equipment is not deleted, because the Process-Engine is not reachable. Please try it again.";
                            ModelState.AddModelError("error", errmsg);
                            try
                            {
                                logger.Error("The webservice Call CreateWorkflowInstance (GetObreRemoveWorkflowMessage) failed", ex);
                                if (equipmentInstance != null)
                                {
                                    // Update the status of the obre
                                    equipmentInstance.Status = (byte)EquipmentStatus.STATUSITEM_ACTIVE;
                                    ObjectRelationHandler obreH = new ObjectRelationHandler(this.PersonGuid);
                                    obreH.UpdateObject(equipmentInstance);
                                    logger.Info(string.Format("OBRE {0} was updated (undo) to status STATUSITEM_ACTIVE, because deletion failed", equipmentInstance.Guid));
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error("The created OBRE couldn't be updated to Status STATUSITEM_ACTIVE after deletion failed.", e);
                            }
                        }
                    }
                    else
                    {
                        EMDObjectRelation equipmentInstance = (EMDObjectRelation)new ObjectRelationHandler(transaction).GetObject<EMDObjectRelation>(equipmentRemoveModel.GuidObre);

                        try
                        {
                            if (equipmentInstance != null)
                            {
                                // Set Equipment-Status to removed
                                ObjectRelationHandler obreH = new ObjectRelationHandler(this.PersonGuid);
                                equipmentInstance.Status = EquipmentStatus.STATUSITEM_REMOVED;
                                obreH.UpdateObject(equipmentInstance);
                                logger.Info(string.Format("OBRE {0} was removed from Administrator: {1}", equipmentInstance.Guid, this.PersonGuid));

                                if (equipmentInstance.Status == EquipmentStatus.STATUSITEM_ORDERED || equipmentInstance.Status == EquipmentStatus.STATUSITEM_INPROGRESS)
                                {
                                    EMDProcessEntity emdProcessEntity = new ProcessEntityManager(this.PersonGuid).GetLastProcessEntity(equipmentInstance.Guid);
                                    if (emdProcessEntity != null)
                                    {
                                        Service.SetWorkflowInstanceStatus(new WorkflowInstanceStatusItem() { InstanceID = emdProcessEntity.WFI_ID, Status = ProcessEngine.Shared.Enums.EnumWorkflowInstanceStatus.Aborted });
                                    }

                                    logger.Info(string.Format("OBRE {0} was deleted from Administrator: {1}", equipmentInstance.Guid, this.PersonGuid));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("The created OBRE couldn't be updated to Status STATUSITEM_ACTIVE after deletion failed.", e);
                        }
                    }

                }
                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    ModelState.AddModelError("error", string.Format("Could not remove equipment: {0}", ex.Message));
                }
            }


            PartialViewResult result = GetPartialFormWithErrors("Edit", equipmentRemoveModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = "The Equipment was successfully removed!" });
            }
        }

        private void SaveMD5GUIHashToPersonPortrait(Image image, string persGuid, string P_UserID, int PersonPortraitJobId, string Configuration)
        {
            int JQ_JD_ID = PersonPortraitJobId;
            string JQ_Configuration = Configuration;
            string JQ_Requester = P_UserID;
            DateTime JQ_InsDT = DateTime.Now;
            SqlConnection connection = null;
            //string QueryStringGetUserInformation = "INSERT INTO JobQueue (JQ_JD_ID, JQ_Configuration, JQ_Requester, JQ_InsDT) VALUES ('" + JQ_JD_ID + "','" + JQ_Configuration + "','" + P_UserID + "','" + JQ_InsDT + "'); ";

            //Get the MD5-Hash for the image and dispose.
            string MD5Hash = Util.Crypto.HashGenerator.GenerateKey(image);

            //Image dispose
            if (image != null)
                image.Dispose();

            //Throw error when MD5Hash is null or emty
            if (String.IsNullOrEmpty(MD5Hash))
            {
                logger.Error("The MD5-Hash for PersonPortrait is null or emty. : " + persGuid);
                BaseException be = new BaseException(ErrorCodeHandler.E_MD5_GENERAL);
                throw be;
            }
            else
            {
                logger.Info("Calculate the MD5-Hash for PersonPortrait: " + P_UserID + " done. MD5Hash: " + MD5Hash);

                //Save hash in database
                try
                {
                    PersonManager pmgr = new PersonManager(this.PersonGuid, "GUI: Personprofile Picture Upload");
                    pmgr.UpdatePersonPictureHash(persGuid, MD5Hash, false);
                    logger.Info("Insert or Update PersonPortrait data for User: " + persGuid + " done");
                }
                catch (Exception ex)
                {
                    logger.Error("Can't update or insert MD5-Hash in database.", ex);
                    BaseException be = new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
                    throw be;
                }

                // TODO --> Add job in JobQueue --> move to JobManager.cs in edpcore
                try
                {
                    CoreTransaction x = new CoreTransaction();
                    //using (connection = new SqlConnection(EMDDB_Helper.GetEMDConnectionString()))
                    using (connection = new SqlConnection(ConfigurationManager.ConnectionStrings["JobScheduler"].ConnectionString))
                    {
                        // Create the command and set its properties.
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO JobQueue (JQ_JD_ID, JQ_Configuration, JQ_Requester, JQ_InsDT) VALUES (@JQ_JD_ID, @JQ_Configuration, @P_UserID, @JQ_InsDT);";
                        command.CommandType = System.Data.CommandType.Text;

                        // Add the input parameter and set its properties.
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = "@JQ_JD_ID";
                        parameter.SqlDbType = SqlDbType.Int;
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = JQ_JD_ID;

                        // Add the parameter to the Parameters collection. 
                        command.Parameters.Add(parameter);


                        if (!String.IsNullOrEmpty(JQ_Configuration))
                        {
                            parameter = new SqlParameter();
                            parameter.ParameterName = "@JQ_Configuration";
                            parameter.SqlDbType = SqlDbType.Xml;
                            parameter.Direction = ParameterDirection.Input;
                            parameter.Value = JQ_Configuration;
                            // Add the parameter to the Parameters collection. 
                            command.Parameters.Add(parameter);
                        }
                        else
                        {
                            command.CommandText = "INSERT INTO JobQueue (JQ_JD_ID, JQ_Requester, JQ_InsDT) VALUES (@JQ_JD_ID, @P_UserID, @JQ_InsDT);";
                        }

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@P_UserID";
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = P_UserID;

                        // Add the parameter to the Parameters collection. 
                        command.Parameters.Add(parameter);

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@JQ_InsDT";
                        parameter.SqlDbType = SqlDbType.DateTime;
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = JQ_InsDT;

                        // Add the parameter to the Parameters collection. 
                        command.Parameters.Add(parameter);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    //Kapsch.IS.Util.DB.DBUtils.ExecuteQueryOrStoredProcedureDataset(EMDDB_Helper.GetEMDConnectionString(), QueryStringGetUserInformation, Kapsch.IS.Util.DB.QueryType.QUERY);
                    logger.Info("Insert job in database (JobQueue) done. " + P_UserID);
                }
                catch (Exception ex)
                {
                    connection.Close();
                    logger.Error("Can't insert job in database (JobQueue).", ex);
                    BaseException be = new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
                    throw be;
                }
            }
        }



        [Route("ChangeEquipmentStatus/{obre_guid}/")]
        [Route("ChangeEquipmentStatus/{obre_guid}/{isPartialView}")]
        public ActionResult ChangeEquipmentStatus(string obre_guid, bool isPartialView = false)
        {
            SecurityUser securityUser = SecurityUser.NewSecurityUser(this.UserName);
            ChangeEquipmentStatusModel model = new ChangeEquipmentStatusModel();
            model.InitializeSecurity(securityUser);


            model.Obre_Guid = obre_guid;
            model.AvailableStatus = new List<TextValueModel>();

            model.AvailableStatus.Add(new TextValueModel("Active", EDP.Core.Logic.ProcessStatus.STATUSITEM_ACTIVE.ToString()));
            model.AvailableStatus.Add(new TextValueModel("Declined", EDP.Core.Logic.ProcessStatus.STATUSITEM_DECLINED.ToString()));

            if (isPartialView)
            {
                return PartialView("ChangeEquipmentStatus", model);
            }
            else
            {
                return View("ChangeEquipmentStatus", model);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoChangeEquipmentStatus")]
        public ActionResult DoChangeEquipmentStatus(ChangeEquipmentStatusModel model)
        {
            Exception handledException = null;
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            ProcessEntityManager processEntityManager = new ProcessEntityManager();

            if (model != null && ModelState.IsValid)
            {
                try
                {
                    if (!model.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }


                    EMDObjectRelation equipmentInstance = (EMDObjectRelation)new ObjectRelationHandler().GetObject<EMDObjectRelation>(model.Obre_Guid);
                    equipmentInstance.Status = (byte)int.Parse(model.Status);
                    new ObjectRelationHandler().UpdateObject(equipmentInstance);

                    EMDProcessEntity processEntity = processEntityManager.GetLastProcessEntity(model.Obre_Guid);
                    if (processEntity != null)
                    {
                        WorkflowInstanceItem instanceItem = Service.GetWorkflowInstanceItem(processEntity.WFI_ID);
                        if (instanceItem != null)
                        {
                            if (instanceItem.Status != EnumWorkflowInstanceStatus.Finish && instanceItem.Status != EnumWorkflowInstanceStatus.Aborted)
                            {
                                Service.SetWorkflowInstanceStatus(new WorkflowInstanceStatusItem() { InstanceID = processEntity.WFI_ID, Status = EnumWorkflowInstanceStatus.Aborted });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Equipment-Status: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("ChangeEquipmentStatus", model, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = "The Equipment-Status has been updated!" });
            }
        }
    }
}