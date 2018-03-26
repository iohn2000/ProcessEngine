using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class OnboardingManager : BaseManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private EmploymentManager employmentManager;
        #region Constructors

        public OnboardingManager()
            : base()
        {
        }

        public OnboardingManager(CoreTransaction transaction)
            : base()
        {
        }

        public OnboardingManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public OnboardingManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EmploymentManager EmploymentManager
        {
            get
            {
                if (this.employmentManager == null)
                {
                    this.employmentManager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                }
                return this.employmentManager;
            }
        }


        public EMDPerson CreateNewPerson(String familyName, String firstName, string sex, string Degree = "", string DegreeSuffix = "")
        {

            //   Framework.TransactionHandler ta = Framework.TransactionHandler.Instance;
            Framework.CoreTransaction cta = new CoreTransaction();
            EMDPerson newPerson = new EMDPerson();
            newPerson.Guid = newPerson.CreateDBGuid();
            cta.Begin();

            try
            {
                //handle Names
                //TODO check Name Handling for compatibility with old
                newPerson.FamilyName = familyName;
                newPerson.FirstName = firstName;

                newPerson.DegreePrefix = Degree;
                newPerson.DegreeSuffix = DegreeSuffix;

                newPerson.generateC128Strings();

                newPerson.Display_FamilyName = familyName;
                newPerson.Display_FirstName = firstName;

                PersonHandler ph = new PersonHandler(cta, this.Guid_ModifiedBy, this.ModifyComment);

                //Picturehandling
                //Picture shall not be shown in AD by default. A date when this setting was lastly updated is set to now.

                newPerson.AD_Picture_UpdDT = DateTime.Now;

                // get a new PID for the new Person
                int P_ID = ph.GetNextFreePIDForPerson();
                newPerson.P_ID = P_ID;

                //check gender 
                List<string> ps = new List<string> { PersonSex.FEMALE, PersonSex.MALE, PersonSex.NEUTRAL };
                if (ps.Contains(sex)) newPerson.Sex = sex; else newPerson.Sex = PersonSex.NEUTRAL;

                //create eMailProposal
                //TODO does this make sense? Mail dpends on employment which does not exist at the moment
                newPerson.MainMail = ph.CreateMainMailProposalForPerson(newPerson);

                //create a 8 characters long UserID for logins an also store it to person
                //TODO check for existance etc.

                // createUser now done once emplyoment exists (finishonboarding activity)
                //UserHandler uh = new UserHandler(cta);
                //newPerson.UserID = uh.CreateUserIDs(newPerson.Guid, uh.CreateUserIDProposalForPerson(newPerson.C128_FamilyName, newPerson.C128_FirstName));


                //create person in DB to get a working uid for this person
                newPerson = (EMDPerson)ph.CreateObject(newPerson);

                //finish this transaction
                cta.Commit();

                //AD_Picture false => wird nicht eingetragen
                ObjectFlagHandler objfHandler = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                //EMDObjectFlag objFlagADPicture = new EMDObjectFlag();
                //objFlagADPicture.FlagType = EnumObjectFlagType.PictureVisibleAD.ToString();
                //objFlagADPicture.Obj_Guid = newPerson.Guid;
                //objfHandler.CreateObject(objFlagADPicture);

                //PictureVisible false => wird nicht eingetragen
                //EMDObjectFlag objFlagPictureVisible = new EMDObjectFlag();

                //VisiblePhone = true => wird eingetragen
                EMDObjectFlag objFlagVisiblePhone = new EMDObjectFlag();
                objFlagVisiblePhone.FlagType = EnumObjectFlagType.PictureVisibleAD.ToString();
                objFlagVisiblePhone.Obj_Guid = newPerson.Guid;
                objfHandler.CreateObject(objFlagVisiblePhone);

                //MainFlag ???
            }
            catch (Exception e)
            {
                cta.Rollback();
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Cannot create person:", e);
            }

            //TODO Write to AuditLog if necessary

            return newPerson;
        }

        /// <summary>
        /// Creates a new Employment with EmploymentProcessStatus.STATUSITEM_ORDERED and returns the workflow message for process engine
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="requestingPersonEmplGuid"></param>
        /// <param name="empl"></param>
        /// <param name="effectedPersonGuid"></param>
        /// <param name="enteGuid"></param>
        /// <param name="locaGuid"></param>
        /// <param name="accoGuid"></param>
        /// <param name="orguGuid"></param>
        /// <param name="emtyGuid"></param>
        /// <param name="digrGuid"></param>
        /// <param name="emailType">not necessary for change or existing employments(also history), because no new mail will be generated</param>
        /// <param name="contactList"></param>
        /// <param name="xmlData"></param>
        /// <param name="newEquipments"></param>
        /// <param name="oldEmplChangeExit">if not null set the old employments last day to newEmployment firstWorkingDay</param>
        /// <returns></returns>
        public EmplAddWorkflowMessage PrepareOnboarding(
            string requestingPersonEmplGuid,
            EMDEmployment empl,
            string effectedPersonGuid,
            string enteGuid,
            string locaGuid,
            string accoGuid,
            string orguGuid,
            string emtyGuid,
            string userdomainGuid,
            string digrGuid,
            string sponsorGuid,
            string emailType,
            List<EMDContact> contactList,
            XElement xmlData,
            List<NewEquipmentInfo> newEquipments,
            DateTime? leaveFrom = null,
            DateTime? leaveTo = null,
            EMDEmployment oldEmplChangeExit = null,
            EnumBusinessCase businessCase = EnumBusinessCase.NotDefined,
            EnumEmploymentChangeType changeType = EnumEmploymentChangeType.NoChange)
        {
            logger.Info(string.Format("Starting doOnboarding for {0} requested by {1}", effectedPersonGuid, requestingPersonEmplGuid));

            EmploymentHandler emplHdl = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            bool hasEmployments = false;
            bool hasMainEmployment = false;
            try
            {
                ObjectFlagManager flagManager = new ObjectFlagManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);


                PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                ContactHandler contactHandler = new ContactHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                OrgUnitRoleHandler orgunitRoleHandler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

                //Get Objects
                EMDPerson effPers = (EMDPerson)personHandler.GetObject<EMDPerson>(effectedPersonGuid);
                EMDEmploymentType emty = (EMDEmploymentType)new EmploymentTypeHandler(this.Transaction).GetObject<EMDEmploymentType>(emtyGuid);
                EMDDistributionGroup digr = (EMDDistributionGroup)new DistributionGroupHandler(this.Transaction).GetObject<EMDDistributionGroup>(digrGuid);
                EMDEnterprise ente = (EMDEnterprise)new EnterpriseHandler(this.Transaction).GetObject<EMDEnterprise>(enteGuid);
                EMDLocation loca = (EMDLocation)new LocationHandler(this.Transaction).GetObject<EMDLocation>(locaGuid);
                UserManager userManager = new UserManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                PersonManager personManager = new PersonManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                EntityCheckManager entityCheckManager = new EntityCheckManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

                EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)new EnterpriseLocationHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment).CheckForOrCreateEnLo(enteGuid, locaGuid);
                hasMainEmployment = EmploymentManager.HasMainEmployments(effectedPersonGuid);

                List<EMDEmployment> existingEmployments = EmploymentManager.GetEmploymentsForPerson(effectedPersonGuid, true);
                hasEmployments = existingEmployments.Count > 0;

                // Create new Employment
                empl.P_Guid = effPers.Guid;
                empl.P_ID = effPers.P_ID;
                empl.EP_ID = emplHdl.GetNextFreeEP_ID();
                empl.ET_Guid = emty.Guid;
                empl.ET_ID = emty.ET_ID;
                empl.DGT_ID = digr.DGT_ID;
                empl.DGT_Guid = digr.Guid;
                empl.ENLO_Guid = enlo.Guid;

                if (!string.IsNullOrWhiteSpace(sponsorGuid))
                {
                    empl.Sponsor_Guid = sponsorGuid;
                    EMDEmployment employment = EmploymentManager.GetEmployment(sponsorGuid);
                    if (employment != null)
                    {
                        empl.Sponsor = employment.EP_ID;
                    }
                }


                if (leaveFrom.HasValue)
                {
                    empl.LeaveFrom = leaveFrom.Value;
                }
                if (leaveTo.HasValue)
                {
                    empl.LeaveTo = leaveTo.Value;
                }

                if (empl.LastDay == null)
                {
                    if (empl.Exit != null && empl.Exit < EMDObject<EMDEmployment>.INFINITY)
                    {
                        empl.LastDay = empl.Exit;
                    }
                    else
                    {
                        empl.LastDay = EMDObject<EMDEmployment>.INFINITY;
                    }
                }

                if (empl.Exit == null)
                {
                    if (empl.LastDay != null && empl.LastDay < EMDObject<EMDEmployment>.INFINITY)
                    {
                        empl.Exit = empl.LastDay;
                    }
                    else
                    {
                        empl.Exit = EMDObject<EMDEmployment>.INFINITY;
                    }
                }

                if (string.IsNullOrEmpty(empl.PersNr))
                {
                    empl.PersNr = string.Empty;
                }
                empl.Status = EmploymentProcessStatus.STATUSITEM_ORDERED;
                empl = (EMDEmployment)emplHdl.CreateObject(empl);

                // add Entity Check
                entityCheckManager.AddEntity(empl);

                // Create OrgunitRole
                orgunitRoleHandler.AddOrgUnitRoleToEmployment(empl.Guid, orguGuid, OrgUnitRoleHandler.ROLE_ID_PERSON);


                // create users


                EMDUser mainAccount = null;
                // create only new users if this is the first employment
                if (!hasEmployments)
                {
                    mainAccount = userManager.CreateNewUsersToEmployment(empl, userdomainGuid, emty);
                }

                // set MainEmployment if not exist and also other default flags
                if (!hasMainEmployment)
                {
                    EmploymentManager.SetMainEmploymentFlag(empl.Guid);

                    // if only offbaorded main-empls were found, move the main user to the new employment
                    bool areAllEmploymentsDeleted = existingEmployments.FindAll(a => !a.IsSystemActive).Count == existingEmployments.Count;

                    if (areAllEmploymentsDeleted)
                    {
                        userManager.MoveUserFullAccountToMainEmployment(effPers.Guid);
                    }


                    //set ADUpdateFlag
                    flagManager.UpdateIsAD(empl.Guid, true, this.Transaction);

                    // check if we should show all pictures, depending on enterprise settings
                    // hotfix INM17/0064295
                    if (flagManager.SyncPictureForEnterprise(ente.Guid))
                    {
                        flagManager.UpdateIsPictureVisibleAD(effPers.Guid, true, this.Transaction);
                    }
                    flagManager.UpdateIsPersonVisibleInPhonebook(effPers.Guid, true, this.Transaction);
                    flagManager.UpdateIsPictureVisible(effPers.Guid, true, this.Transaction);

                    if (mainAccount != null)
                    {
                        effPers.USER_GUID = mainAccount.Guid;
                        effPers.UserID = mainAccount.Username;
                    }

                    // add a new e-mail address only if it doesn't exist
                    if (string.IsNullOrEmpty(effPers.MainMail))
                    {
                        effPers.MainMail = personManager.CreateMainMailForPerson(ref effPers, emty, this.Transaction, this.Guid_ModifiedBy, this.ModifyComment, emailType?.ToLower() == "extern");
                    }
                    personHandler.UpdateObject(effPers);
                }
                //Set employment visible in phone
                flagManager.UpdateIsEmploymentVisibleInPhonebook(empl.Guid, true, this.Transaction);

                // Create EmploymentAccount
                // TODO: remember for change the EmploymentAccount is created OnChange (not prepare)
                EmploymentAccountHandler employmentAccountHandler = new EmploymentAccountHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                EMDEmploymentAccount newEmploymentAccount = new EMDEmploymentAccount();
                newEmploymentAccount.AC_Guid = accoGuid;
                newEmploymentAccount.EP_Guid = empl.Guid;
                newEmploymentAccount = (EMDEmploymentAccount)employmentAccountHandler.CreateObject<EMDEmploymentAccount>(newEmploymentAccount);

                //if (!hasMainEmployment)
                //{
                flagManager.UpdateIsMainAccount(newEmploymentAccount.Guid, true, this.Transaction);
                //}

                //
                // create contacts as xml
                // add empl guid
                //
                ContactManager contMgr = new ContactManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

                XDocument xConts = new XDocument();
                XElement root = new XElement("Contacts");

                if (contactList != null)
                {
                    foreach (EMDContact cont in contactList)
                    {
                        cont.EP_Guid = empl.Guid;
                        cont.E_Guid = ente.Guid;
                        contactHandler.CreateObject(cont);
                    }
                }

                xConts.Add(root);

                XDocument xEqs = new XDocument();
                if (newEquipments != null)
                {
                    //
                    //create new eq infos as xml
                    // 
                    EquipmentManager eqdeMgr = new EquipmentManager();

                    XElement rootEq = new XElement("NewEquipments");

                    foreach (NewEquipmentInfo nEq in newEquipments)
                    {
                        XElement e = eqdeMgr.SerializeToXml(nEq);
                        rootEq.Add(e);
                    }

                    xEqs.Add(rootEq);
                }


                if (oldEmplChangeExit != null)
                {
                    if (empl.FirstWorkDay != null)
                    {
                        oldEmplChangeExit.SetExitAndLastDay((DateTime)empl.FirstWorkDay, empl.FirstWorkDay);
                        //oldEmplChangeExit.Exit = empl.FirstWorkDay;
                        //oldEmplChangeExit.LastDay = empl.FirstWorkDay;
                    }
                    else
                    {
                        //TODO ... do nothing???
                    }

                    //Delete assistence relation
                    AccountGroupManager accGrpManager = new AccountGroupManager();
                    if (accGrpManager.IsEmpoymentAssistence(oldEmplChangeExit.Guid))
                    {
                        accGrpManager.RemoveEmploymentFromAllAssistenceGroups(oldEmplChangeExit.Guid);
                    }

                    emplHdl.UpdateDBObject(oldEmplChangeExit);
                }


                //
                //Now fill in input vars for onboarding workflow
                //
                try
                {
                    ProcessMappingHandler prmpH = new ProcessMappingHandler(this.Transaction);

                    //returns a list of variables to be filled
                    FilterCriteria filterCriteria = new FilterCriteria();
                    filterCriteria.Company = enlo.E_Guid;
                    filterCriteria.CostCenter = accoGuid;
                    filterCriteria.EmploymentType = emty.Guid;
                    filterCriteria.Location = enlo.L_Guid;

                    EmplAddWorkflowMessage emplAddData = new EmplAddWorkflowMessage();
                    emplAddData = prmpH.GetWorkflowMapping<EmplAddWorkflowMessage>(null, filterCriteria);
                    emplAddData.NewEmploymentGuid = empl.Guid;

                    logger.Info(string.Format("Mapped process to Workflow with Definition-ID {0}", emplAddData.WFDefID));

                    emplAddData.RequestingPersonEmploymentGuid = requestingPersonEmplGuid;
                    emplAddData.EffectedPersonEmploymentGuid = empl.Guid;
                    emplAddData.EffectedAccountGuid = accoGuid;
                    emplAddData.EffectedOrgUnitGuid = orguGuid;
                    emplAddData.ContactsXdoc = string.Concat(xConts.Nodes());
                    emplAddData.EmailType = emailType;
                    emplAddData.NewEquipmentInfos = string.Concat(xEqs.Nodes());
                    emplAddData.BusinessCase = businessCase;
                    emplAddData.ChangeType = changeType;

                    XElement xroot = new XElement("EmploymentApproval");
                    xroot.Add(xmlData);

                    emplAddData.additionalXmlData = xroot;

                    return emplAddData;

                }
                catch (Exception exc)
                {
                    emplHdl.DeleteObject(empl);
                    throw new WorkflowMappingException("EMPL", WorkflowAction.Add, ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "could not successfully order Employment therefor rolled back", exc);
                }
            }
            catch (WorkflowMappingException ex)
            {
                throw ex;
            }
            catch (Exception exc)
            {
                if (empl != null)
                    emplHdl.DeleteObject(empl);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Error while trying to doOnboarding: {0}, {1}. Rolled backed employment", enteGuid, locaGuid), exc);
            }
        }



    }
}
