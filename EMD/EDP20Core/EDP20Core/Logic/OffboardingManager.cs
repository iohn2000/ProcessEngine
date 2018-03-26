using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class OffboardingManager : BaseManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const int OFFBOARDING_DELAY_IN_DAYS = 9;

        private EmploymentManager employmentManager;


        public OffboardingManager()
            : base()
        {
        }

        public OffboardingManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public OffboardingManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public OffboardingManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }


        public EmploymentManager EmploymentManager
        {
            get
            {
                if (this.employmentManager == null)
                {
                    this.employmentManager = new EmploymentManager(this.Transaction, guid_ModifiedBy: this.Guid_ModifiedBy, modifyComment: this.ModifyComment);
                }
                return this.employmentManager;
            }
        }

        /// <summary>
        /// Handles all related entities of the given employment to prepare it for offboarding
        /// Remember: The employment itself is not handled
        /// Throws a BaseException:
        /// MainEmployment must not be offboarded, if other employments exist
        /// +++ IMPORTANT +++
        /// AT THE MEANTIME NO RELATIONS ARE DELETED, BECAUSE THE WORKFLOW NEED THE RELEATIONS FOR ITS SUB-WORKFLOWS
        /// TODO: CALL THIS METHOD AFTER ALL SUBWORFKLOWS ARE FINISHED
        /// +++
        /// </summary>
        /// <param name="emplGuid"></param>
        public void RemoveEmployment(string emplGuid)
        {
            bool handleTransactionLocal = false;
            if (this.Transaction == null)
            {
                this.Transaction = new CoreTransaction();
                handleTransactionLocal = true;
            }

            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            UserHandler userHandler = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            OrgUnitRoleHandler orgunitRoleHandler = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            GroupMemberHandler groupMemberHandler = new GroupMemberHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            ContactHandler contactHandler = new ContactHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            AccountHandler accountHandler = new AccountHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EmploymentAccountHandler employmentAccountHandler = new EmploymentAccountHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            AccountGroupHandler accountGroupHandler = new AccountGroupHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            UserManager userManager = new UserManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            try
            {
                if (handleTransactionLocal)
                {
                    this.Transaction.Begin();
                }
                EMDEmployment employment = EmploymentManager.GetEmployment(emplGuid);

                List<EMDEmployment> systemActiveEmployments = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment).GetEmploymentsForPerson(employment.P_Guid);
                systemActiveEmployments = (from a in systemActiveEmployments where a.IsSystemActive == true && a.Guid != emplGuid select a).OrderBy(a => a.Created).ToList();


                // TODO: delete them when workflow calls this method at the end of all subworkflows
                // delete orgunit roles
                //List<EMDOrgUnitRole> orgUnitRoles = orgunitRoleHandler.GetObjects<EMDOrgUnitRole, OrgUnitRole>("EP_Guid =  \"" + emplGuid + "\"").Cast<EMDOrgUnitRole>().ToList();
                //foreach (EMDOrgUnitRole orgunitRole in orgUnitRoles)
                //{
                //    orgunitRoleHandler.DeleteObject(orgunitRole);
                //}

                bool isMainEmployment = EmploymentManager.IsMainEmployment(emplGuid);


                // Uer logic
                if (isMainEmployment && systemActiveEmployments.Count >= 1)
                {
                    userManager.MoveEmploymentUserAndSynchronizePerson(emplGuid, systemActiveEmployments[0].Guid);

                }
                else
                {
                    userManager.DisableUsersOnEmployment(emplGuid);
                }


                // TODO: delete them when workflow calls this method at the end of all subworkflows
                // delete groupmember (assistances)
                //List<EMDGroupMember> groupMembers = groupMemberHandler.GetObjects<EMDGroupMember, GroupMember>("EP_Guid =  \"" + emplGuid + "\"").Cast<EMDGroupMember>().ToList();
                //foreach (EMDGroupMember groupMember in groupMembers)
                //{
                //    groupMemberHandler.DeleteObject(groupMember);
                //}

                // delete contact >> not necessary >> keep them at the moment
                //List<EMDContact> contacts = contactHandler.GetObjects<EMDContact, Contact>("EP_Guid =  \"" + emplGuid + "\"").Cast<EMDContact>().ToList();
                //foreach (EMDContact contact in contacts)
                //{
                //    contactHandler.DeleteObject(contact);
                //}
                // delete accounts with dependencies

                // TODO: delete them when workflow calls this method at the end of all subworkflows
                //List<EMDEmploymentAccount> employmentAccounts = employmentAccountHandler.GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid =  \"" + emplGuid + "\"").Cast<EMDEmploymentAccount>().ToList();
                //foreach (EMDEmploymentAccount employmentAccount in employmentAccounts)
                //{
                //    employmentAccountHandler.DeleteObject(employmentAccount);
                //}

                if (isMainEmployment)
                {
                    if (systemActiveEmployments.Count >= 1)
                    {
                        EmploymentManager.ChangeMainEmployment(emplGuid, systemActiveEmployments[0].Guid);
                    }
                    else
                    {
                        EmploymentManager.RemoveMainEmploymentFlag(employment.Guid);
                    }

                }

                employment.Status = ProcessStatus.STATUSITEM_REMOVED;
                EmploymentManager.Update(employment);

                if (handleTransactionLocal)
                {
                    this.Transaction.Commit();
                }

            }
            catch (Exception ex)
            {
                if (handleTransactionLocal)
                {
                    this.Transaction.Rollback();
                }
                throw new Exception("RemoveEmployment failed", ex);
            }
        }


        /// <summary>
        /// Updates an existing Employment with EmploymentProcessStatus.STATUSITEM_ORDERED, sets End and Exit Dates and returns the workflow message for process engine
        /// </summary>
        /// <param name="effectedPersonEmploymentGuid"></param>
        /// <param name="removeEquipmentInfos"></param>
        /// <param name="requestingPersEMPLGuid"></param>
        /// <param name="exitDate"></param>
        /// <param name="lastDay"></param>
        /// <param name="resourceNumber"></param>
        /// <returns></returns>
        public EmplRemoveWorkflowMessage PrepareOffboarding(string effectedPersonEmploymentGuid, List<RemoveEquipmentInfo> removeEquipmentInfos, string requestingPersEMPLGuid, DateTime exitDate, DateTime lastDay, string resourceNumber)
        {

            EMDEmployment employment = EmploymentManager.GetEmployment(effectedPersonEmploymentGuid);

            //Delete assistence relation
            AccountGroupManager accGrpManager = new AccountGroupManager();
            if (accGrpManager.IsEmpoymentAssistence(effectedPersonEmploymentGuid))
            {
                accGrpManager.RemoveEmploymentFromAllAssistenceGroups(effectedPersonEmploymentGuid);
            }

            employment.SetExitAndLastDay(exitDate, lastDay);
            //          employment.Exit = exitDate;
            //          employment.LastDay = lastDay;
            employment.Status = EmploymentProcessStatus.STATUSITEM_ORDERED;

            EmploymentManager.Update(employment);

            EquipmentManager eqdeMgr = new EquipmentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            XDocument xEqs = new XDocument();
            XElement rootEq = new XElement("RemoveEquipments");

            foreach (RemoveEquipmentInfo nEq in removeEquipmentInfos)
            {
                XElement e = eqdeMgr.SerializeToXml(nEq);
                rootEq.Add(e);
            }

            xEqs.Add(rootEq);

            // code here to add msg to queue for workflow
            // get mapping for eq
            ProcessMappingHandler prmpH = new ProcessMappingHandler(this.Transaction);

            //returns a list of variables to be filled
            FilterCriteria filterCriteria = EmploymentManager.GetFilterCriteria(effectedPersonEmploymentGuid);

            EmplRemoveWorkflowMessage dataHelper = new EmplRemoveWorkflowMessage();
            dataHelper = prmpH.GetWorkflowMapping<EmplRemoveWorkflowMessage>(null, filterCriteria);

            dataHelper.RequestingPersonEmploymentGuid = requestingPersEMPLGuid;
            dataHelper.EffectedPersonEmploymentGuid = effectedPersonEmploymentGuid;
            dataHelper.RemoveEquipmentInfos = string.Concat(xEqs.Nodes());

            dataHelper.ExitDate = exitDate;
            dataHelper.LastDay = lastDay;
            dataHelper.TargetDateIso8601 = DateTimeHelper.DateTimeToIso8601(EMDEmployment.GetTargetOffboardingDate(exitDate, lastDay));
            dataHelper.ResourceNumber = resourceNumber;


            return dataHelper;
        }


        /// <summary>
        /// Starts the offboarding of an employment.
        /// </summary>
        /// <remarks>Used for e.g. from GUI</remarks>
        /// <param name="emplGuid">Employment Guid</param>
        /// <param name="exitDate">Exit-Date</param>
        /// <param name="removeEquipmentInfos">Equipmentinfos</param>
        /// <param name="requestingPersEMPLGuid"></param>
        /// <param name="modifyComment"></param>
        /// <returns>Returns the created EMDProcessEntity of the offboarding process</returns>
        public bool StartOffBoardingWorkflow(string emplGuid, DateTime exitDate, DateTime lastDay, List<RemoveEquipmentInfo> removeEquipmentInfos, string requestingPersEMPLGuid = "", string resourceNumber = "", EnumBusinessCase enumBusinessCase = EnumBusinessCase.Offboarding)
        {
            bool dataIsValid = false;

            dataIsValid = EmploymentIsValidForOffboardingProcess(emplGuid, true);

            if (dataIsValid)
            {
                if (this.ReadOnlyMode == false)
                {
                    EmplRemoveWorkflowMessage removeMessage = this.PrepareOffboarding(emplGuid, removeEquipmentInfos, requestingPersEMPLGuid, exitDate, lastDay, resourceNumber);
                    removeMessage.BusinessCase = enumBusinessCase;
                    EMDProcessEntity processEntity = removeMessage.CreateWorkflowInstance(this.Guid_ModifiedBy, this.ModifyComment);
                }
                return true;
            }
            else
            {
                return dataIsValid;
            }
        }

        /// <summary>
        /// Starts the offboarding of an employment.
        /// </summary>
        /// <remarks>Used from e.g. jobs</remarks>
        /// <param name="emplGuid"></param>
        /// <param name="requestorEmplGuid"></param>
        /// <param name="delayInDays">This is the max timespan that the lastday of the employment can be in the future. Only then the offboarding will take place.</param>
        /// <param name="enumBusinessCase"></param>
        /// <returns></returns>
        public bool StartOffBoardingWorkflow(string emplGuid, string requestorEmplGuid, int delayInDays = OFFBOARDING_DELAY_IN_DAYS, EnumBusinessCase enumBusinessCase = EnumBusinessCase.Offboarding)
        {
            bool returnValue = false;
            bool dataIsValid = false;

            dataIsValid = EmploymentIsValidForOffboardingProcess(emplGuid, false);
            if (dataIsValid)
            {
                EMDEmployment empl = EmploymentManager.GetEmployment(emplGuid);

                if (empl.LastDay == null || empl.Exit == null)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("LastDay or Exit day is null for Guid {0}!", emplGuid));
                }

                if (Convert.ToDateTime(empl.LastDay).AddDays(-delayInDays) <= DateTime.Now)
                {
                    // check if all equipments are in state active, for auto offboarding job
                    if (enumBusinessCase == EnumBusinessCase.OffboardingAuto)
                    {
                        if (!AreAllEquipmentsActive(emplGuid))
                        {
                            return false;
                        }
                    }

                    List<RemoveEquipmentInfo> removeEquipmentInfos = GetEquipmentInfosForOffboarding(emplGuid, Convert.ToDateTime(empl.LastDay));
                    //TODO: Klären RequesterEmplGuid, ResourceNumber
                    string resourceNumber = string.Empty;
                    returnValue = StartOffBoardingWorkflow(emplGuid, Convert.ToDateTime(empl.Exit), Convert.ToDateTime(empl.LastDay), removeEquipmentInfos, requestorEmplGuid, resourceNumber, enumBusinessCase);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Checks if there are any equipments with a running process
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns></returns>
        public bool AreAllEquipmentsActive(string emplGuid)
        {
            List<EMDEquipmentInstance> equipmentInstances = EmploymentManager.GetConfiguredListOfEquipmentIntancesForEmployment(emplGuid);
            int countNotRemoved = equipmentInstances.FindAll(a => a.ProcessStatus != ProcessStatus.STATUSITEM_REMOVED && a.ProcessStatus != ProcessStatus.STATUSITEM_DECLINED).Count();
            int countActive = equipmentInstances.FindAll(a => a.ProcessStatus == ProcessStatus.STATUSITEM_ACTIVE).Count();

            if (countNotRemoved != countActive)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an employment exists and the status is activ
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <param name="throwErrorIfInactiv"></param>
        /// <returns></returns>
        public bool EmploymentIsValidForOffboardingProcess(string emplGuid, bool throwErrorIfInactiv)
        {
            bool returnValue = false;
            if (string.IsNullOrWhiteSpace(emplGuid))
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "EmplGuid not suplied!");

            EMDEmployment empl = EmploymentManager.GetEmployment(emplGuid);

            if (empl == null)
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Employment for Guid {0} not found!", emplGuid));

            if (empl.Status == ProcessStatus.STATUSITEM_ACTIVE)
                returnValue = true;
            else
            {
                if (throwErrorIfInactiv)
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Employment for Guid {0} is not active!", emplGuid));
                else
                    returnValue = false;
            }
            return returnValue;
        }


        /// <summary>
        /// Gets all equipmentinfos of an employment to remove these equipments in the offboarding process.
        /// </summary>
        /// <param name="emplGuid">Employment Guid</param>
        /// <param name="exitDate">Exit-Date</param>
        /// <returns>returns a list of RemoveEquipmentInfo containing the data needed for the offboarding process</returns>
        public List<RemoveEquipmentInfo> GetEquipmentInfosForOffboarding(string emplGuid, DateTime exitDate)
        {
            List<EMDEquipmentInstance> emdEquipmentInstances = EmploymentManager.GetConfiguredListOfEquipmentIntancesForEmployment(emplGuid, false);
            List<RemoveEquipmentInfo> removeEquipmentInfos = new List<RemoveEquipmentInfo>();
            ObjectRelationHandler handler = new ObjectRelationHandler();

            if (emdEquipmentInstances != null)
            {
                foreach (EMDEquipmentInstance emdEquipmentInstance in emdEquipmentInstances)
                {
                    if (emdEquipmentInstance.ObjectRelationGuid != null)
                    {
                        removeEquipmentInfos.Add(new RemoveEquipmentInfo()
                        {
                            TargetDate = exitDate,
                            ObreGuid = emdEquipmentInstance.ObjectRelationGuid,
                            DoKeep = false,
                            EquipmentDefinitionGuid = ((EMDObjectRelation)handler.GetObject<EMDObjectRelation>(emdEquipmentInstance.ObjectRelationGuid)).Object2
                        });
                    }
                }
            }

            return removeEquipmentInfos;
        }
    }
}
