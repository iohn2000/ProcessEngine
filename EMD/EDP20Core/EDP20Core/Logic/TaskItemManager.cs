using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.WF;
using Kapsch.IS.Util.Email;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    public enum enumApproverTypes { OrgUnitRole, AssistenceGroup, CostCenterResponsible, SponsorUser, UserID, EffectedPerson }

    public class TaskItemManager
        : BaseManager
    {
        internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public TaskItemManager()
            : base()
        {
        }

        public TaskItemManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public TaskItemManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public TaskItemManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        /// <summary>
        /// Returns all persons with assigned workflow tasks
        /// </summary>
        /// <returns></returns>
        public List<Person> GetPersonsWithAssignedTasks()
        {
            DateTime now = DateTime.Now;
            EMD_Entities emdEntities = new EMD_Entities();
            List<Person> persons = (from taskItem in emdEntities.TaskItem
                                    join empl in emdEntities.Employment on taskItem.TSK_Approver_EmplGUID equals empl.Guid
                                    join pers in emdEntities.Person on empl.P_Guid equals pers.Guid
                                    where (taskItem.TSK_Status != "closed" || taskItem.TSK_Status == null) &&
                                    taskItem.ActiveFrom <= now && taskItem.ValidFrom <= now && taskItem.ActiveTo >= now && taskItem.ValidTo >= now &&
                                    empl.ActiveFrom <= now && empl.ValidFrom <= now && empl.ActiveTo >= now && empl.ValidTo >= now &&
                                    pers.ActiveFrom <= now && pers.ValidFrom <= now && pers.ActiveTo >= now && pers.ValidTo >= now
                                    select pers
                         ).Distinct().ToList();
            return persons;
        }

        /// <summary>
        /// Returns all TaskItems for a specific person guid
        /// </summary>
        /// <param name="personGuid"></param>
        /// <returns></returns>
        public List<TaskItem> GetAssignedPersonTasks(string personGuid)
        {
            DateTime now = DateTime.Now;
            EMD_Entities emdEntities = new EMD_Entities();
            List<TaskItem> taskItems = (from taskItem in emdEntities.TaskItem
                                        join empl in emdEntities.Employment on taskItem.TSK_Approver_EmplGUID equals empl.Guid
                                        join pers in emdEntities.Person on empl.P_Guid equals pers.Guid
                                        where (taskItem.TSK_Status != "closed" || taskItem.TSK_Status == null) &&
                                        taskItem.ActiveFrom <= now && taskItem.ValidFrom <= now && taskItem.ActiveTo >= now && taskItem.ValidTo >= now &&
                                        empl.ActiveFrom <= now && empl.ValidFrom <= now && empl.ActiveTo >= now && empl.ValidTo >= now &&
                                        pers.ActiveFrom <= now && pers.ValidFrom <= now && pers.ActiveTo >= now && pers.ValidTo >= now
                                        && pers.Guid == personGuid
                                        select taskItem
                         ).Distinct().ToList();
            return taskItems;

        }

        public List<EMDTaskItem> GetAssignedEmploymentTasks(string employmentGuid)
        {
            //DateTime now = DateTime.Now;
            //EMD_Entities emdEntities = new EMD_Entities();
            TaskItemHandler taskItemHandler = new TaskItemHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            List<EMDTaskItem> taskItems = (from ti in taskItemHandler.GetObjects<EMDTaskItem, TaskItem>().Cast<EMDTaskItem>().ToList() select ti)
                .ToList()
                .Where(item => item.TSK_Approver_EmplGUID == employmentGuid
                    && item.TSK_Status == null
                    && item.TSK_Status != Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.closed))
                .Distinct()
                .ToList();

            //List<TaskItem> taskItems = (from taskItem in emdEntities.TaskItem
            //                            join empl in emdEntities.Employment on taskItem.TSK_Approver_EmplGUID equals empl.Guid
            //                            where (taskItem.TSK_Status != "closed" || taskItem.TSK_Status == null) &&
            //                            taskItem.ActiveFrom <= now && taskItem.ValidFrom <= now && taskItem.ActiveTo >= now && taskItem.ValidTo >= now &&
            //                            empl.ActiveFrom <= now && empl.ValidFrom <= now && empl.ActiveTo >= now && empl.ValidTo >= now
            //                            && empl.Guid == employmentGuid
            //                            select taskItem
            //             ).Distinct().ToList();

            return taskItems;

        }

        /// <summary>
        ///  returns a list of Tuple(approverEMPLGuid, approverPERSGuid)
        /// </summary>
        /// <param name="approverCode"></param>
        /// <param name="effectedPersonEmployment"></param>
        /// <returns> returns a list of Tuple(approverEMPLGuid, approverPERSGuid)</returns>
        public List<Tuple<string, string>> FindTaskApproverForEffectedPerson(string approverCode, string effectedPersonEmployment)
        {
            List<string> apprCodes = new List<string>();
            apprCodes.Add(approverCode);
            return this.FindTaskApproverForEffectedPerson(apprCodes, effectedPersonEmployment);
        }

        /// <summary>
        ///  returns a list of Tuple(approverEMPLGuid, approverPERSGuid)
        /// </summary>
        /// <param name="approverCodes"></param>
        /// <param name="effectedPersonEmployment"></param>
        /// <returns> returns a list of Tuple(approverEMPLGuid, approverPERSGuid)</returns>
        public List<Tuple<string, string>> FindTaskApproverForEffectedPerson(List<string> approverCodes, string effectedPersonEmployment)
        {
            string apprType = "";
            PersonHandler persH = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EmploymentHandler emplH = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            OrgUnitRoleHandler orroH = new Entities.OrgUnitRoleHandler();
            EMDEmployment empl;
            List<Tuple<string, string>> allApprovers = new List<Tuple<string, string>>();
            foreach (var approverID in approverCodes)
            {
                try
                {
                    string approverEMPLGuid = "", approverPERSGuid = "";

                    apprType = approverID.Substring(0, approverID.IndexOf("_", 0)); //take everything from start until "_" --> MAIL
                    string apprValue = approverID.Substring(approverID.IndexOf("_", 0) + 1); // everything behind "_" --> managedservices@kapsch.net

                    switch (apprType)
                    {
                        case "MAIL": // MAIL_ManagedServicesKBC@kapsch.net
                            allApprovers.Add(new Tuple<string, string>(null, apprValue));
                            break;

                        case "R": //roleId
                                  // use the orgunit search
                            var listEmployments = new OrgUnitManager().SearchOrgUnitRoleForEmployment(int.Parse(apprValue), effectedPersonEmployment);

                            if (listEmployments.Count > 0)
                            {
                                approverEMPLGuid = listEmployments[0];
                                empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(approverEMPLGuid);
                                approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;
                                allApprovers.Add(new Tuple<string, string>(approverEMPLGuid, approverPERSGuid));
                            }
                            break;

                        case "RS": //roleId
                                   // use the orgunit search
                            listEmployments = new OrgUnitManager().SearchOrgUnitRoleForEmployment(int.Parse(apprValue), effectedPersonEmployment);

                            if (listEmployments.Count > 0)
                            {
                                foreach (var emplItem in listEmployments)
                                {
                                    approverEMPLGuid = emplItem;
                                    empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(approverEMPLGuid);
                                    approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;
                                    allApprovers.Add(new Tuple<string, string>(approverEMPLGuid, approverPERSGuid));
                                }
                            }
                            break;

                        case "DROLE": // direct role keine vorgesetzen suche verwenden

                            var listOrro = orroH.GetObjects<EMDOrgUnitRole, OrgUnitRole>("R_ID = " + apprValue);
                            if (listOrro != null && listOrro.Count > 0)
                            {
                                EMDOrgUnitRole orro = (EMDOrgUnitRole)listOrro[0];
                                approverEMPLGuid = orro.EP_Guid;
                                empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(approverEMPLGuid);
                                approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;
                                allApprovers.Add(new Tuple<string, string>(approverEMPLGuid, approverPERSGuid));
                            }
                            break;

                        case "DROLES": // direct role keine vorgesetzen suche verwenden

                            var listOrro1 = orroH.GetObjects<EMDOrgUnitRole, OrgUnitRole>("R_ID = " + apprValue);
                            if (listOrro1.Count > 0)
                            {
                                foreach (var orroItem in listOrro1)
                                {
                                    EMDOrgUnitRole orro = (EMDOrgUnitRole)orroItem;
                                    approverEMPLGuid = orro.EP_Guid;
                                    empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(approverEMPLGuid);
                                    approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;
                                    allApprovers.Add(new Tuple<string, string>(approverEMPLGuid, approverPERSGuid));
                                }
                            }
                            break;


                        case "SELF":
                            approverEMPLGuid = effectedPersonEmployment;
                            empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(approverEMPLGuid);
                            approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;
                            allApprovers.Add(new Tuple<string, string>(approverEMPLGuid, approverPERSGuid));
                            break;

                        case "EMPL": // EMPL_Guid; find pers to empl and add both
                            empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(approverID);
                            approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;
                            allApprovers.Add(new Tuple<string, string>(approverID, approverPERSGuid));
                            break;

                        case "ASSIST": // input eff.pers.emplguid output list<assist> 

                            EMDEmploymentAccount emac2 = new EMDEmploymentAccount();
                            var tmp = new EmploymentAccountHandler().GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + apprValue + "\"").ToList();
                            emac2 = (EMDEmploymentAccount)tmp[0];


                            AccountGroupHandler acgrH = new AccountGroupHandler();
                            var acgrTmp = acgrH.GetObjects<EMDAccountGroup, AccountGroup>("AC_Guid = \"" + emac2.AC_Guid + "\"");

                            if (acgrTmp.Count == 1)
                            {
                                EMDAccountGroup acgr = (EMDAccountGroup)acgrTmp[0];
                                var lstGroupMembers = new GroupMemberHandler().GetObjects<EMDGroupMember, GroupMember>("G_Guid = \"" + acgr.G_Guid + "\"");
                                if (lstGroupMembers.Count > 0)
                                {
                                    foreach (var gMember in lstGroupMembers)
                                    {
                                        EMDGroupMember gm = (EMDGroupMember)gMember;

                                        empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(gm.EP_Guid);
                                        approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;

                                        allApprovers.Add(new Tuple<string, string>(gm.EP_Guid, approverPERSGuid));
                                    }
                                }
                            }
                            break;
                        case "KSTL": // KSTL_EMPL_xxxx -> kostenstellen leiter(in) for EMPL_xxx
                            /*select acco.Responsible from Employment as empl
                                join EmploymentAccount as emac on empl.Guid = emac.EP_Guid
                                join Account as acco on emac.AC_Guid = acco.Guid
                             where empl.Guid like 'EMPL_3a94846a1f0648ebb120d043d6a44036'*/
                            // apprValue = empl guid
                            string ccRespEmplGuid = "";
                            EmploymentAccountHandler emacH = new EmploymentAccountHandler();

                            if (string.IsNullOrWhiteSpace(apprValue))
                                apprValue = effectedPersonEmployment;

                            var result = emacH.GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + apprValue + "\"").ToList();
                            if (result != null && result.Count > 0)
                            {
                                // take first one
                                EMDEmploymentAccount emac = (EMDEmploymentAccount)result[0];
                                AccountHandler accoH = new AccountHandler();
                                var acco = (EMDAccount)accoH.GetObject<EMDAccount>(emac.AC_Guid);
                                if (acco != null)
                                {
                                    ccRespEmplGuid = acco.Responsible;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(ccRespEmplGuid))
                            {
                                empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(ccRespEmplGuid);
                                approverPERSGuid = persH.GetObject<EMDPerson>(empl.P_Guid).Guid;
                                allApprovers.Add(new Tuple<string, string>(ccRespEmplGuid, approverPERSGuid));
                            }
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("could not find responsible person for apprType = '{0}',  approverID = '{1}' and effectedPersonEmployment-Guid = '{2}'",
                        apprType, approverID, effectedPersonEmployment);
                    logger.Error(msg, ex);
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
                }

            }

            return allApprovers;
        }

        /// <summary>
        /// Forwards all tasks to another employment
        /// </summary>
        /// <param name="taskGuids">List of Task Guids to forward</param>
        /// <param name="forwardToEmplGuid">The Employment Guid of the new Task owner</param>
        /// <param name="forwardedFromUserId">The origin task owner userID</param>
        public void ForwardTasks(List<string> taskGuids, string forwardToEmplGuid, string forwardedFromUserId)
        {

            TaskItemHandler taskItemHandler = new TaskItemHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<EMDTaskItem> taskItems = taskItemHandler.GetObjects<EMDTaskItem, TaskItem>().Cast<EMDTaskItem>().ToList().Where(item => taskGuids.Contains(item.Guid)).ToList();
            PersonManager persManager = new PersonManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EmploymentManager emplManager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDPerson persTo = persManager.GetPersonByEmployment(forwardToEmplGuid);
            EMDPerson persFrom = persManager.GetPersonByUserId(forwardedFromUserId);

            foreach (EMDTaskItem item in taskItems)
            {
                EMDTaskItem newTask = new EMDTaskItem();
                newTask.TSK_Approver_EmplGUID = forwardToEmplGuid;
                newTask.TSK_DateNextReminder = item.TSK_DateNextReminder;
                newTask.TSK_DateRequested = item.TSK_DateRequested;
                newTask.TSK_Decision = item.TSK_Decision;
                newTask.TSK_DecisionOptions = item.TSK_DecisionOptions;
                newTask.TSK_Duedate = item.TSK_Duedate;
                newTask.TSK_EffectedPerson_EmplGUID = item.TSK_EffectedPerson_EmplGUID;
                newTask.TSK_Information = item.TSK_Information;
                newTask.TSK_LinkedTasks_ID = item.TSK_LinkedTasks_ID;
                string note = "Task forwarded from user " + persManager.getFullDisplayNameWithUserId(persFrom);
                if (item.TSK_Notes != null || !string.IsNullOrWhiteSpace(item.TSK_Notes))
                    newTask.TSK_Notes += ", " + note;
                else
                    newTask.TSK_Notes = note;

                newTask.TSK_ProcessName = item.TSK_ProcessName;
                newTask.TSK_Requestor_EmplGUID = item.TSK_Requestor_EmplGUID;
                newTask.TSK_Status = item.TSK_Status;
                newTask.TSK_TaskTitle = item.TSK_TaskTitle;
                newTask.TSK_ToDo = item.TSK_ToDo;
                taskItemHandler.CreateObject<EMDTaskItem>(newTask);

                item.TSK_Status = Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.closed);
                item.TSK_Notes = "Task forwarded to user: " + persManager.getFullDisplayNameWithUserId(persTo);
                taskItemHandler.UpdateObject<EMDTaskItem>(item);

                SendMail(persTo, emplManager.GetEmployment(forwardToEmplGuid), item);
            }

        }


        /// <summary>
        /// Helper method to send an info mail to the person which gets a task from another person
        /// </summary>
        /// <param name="personTo">EMDPerson to forward the task</param>
        /// <param name="emplTo">The Employment to forward the task</param>
        /// <param name="taskItem">TaskItem Information</param>
        private void SendMail(EMDPerson personTo, EMDEmployment emplTo, EMDTaskItem taskItem)
        {
            EmploymentManager emplManager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonManager persManager = new PersonManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            string fromSystem = string.IsNullOrEmpty(Utils.Configuration.TESTSYSTEMNAME) ? string.Empty : string.Format("{0}: ", Utils.Configuration.TESTSYSTEMNAME);

            EMDEmployment emplEffected = emplManager.GetEmployment(taskItem.TSK_EffectedPerson_EmplGUID);
            EMDPerson persEffected = persManager.GetPersonByEmployment(emplEffected.Guid);

            IEmailSender mailSender = new WebServiceEmailSender();
            string from = Utils.Configuration.EMAILSENDERADDRESS;
            string subject = string.Format("{0}Task-Forwarding: {1}", fromSystem, taskItem.TSK_TaskTitle);
            string templateFileName = "ForwardTask.html";

            // TO List
            List<String> to = new List<String>();
            if (Utils.Configuration.ISTESTSYSTEM && !string.IsNullOrEmpty(Utils.Configuration.TESTMAILRECEIVER))
            {
                to.Add(Utils.Configuration.TESTMAILRECEIVER);
            }
            else
            {
                to.Add(personTo.MainMail);
            }

            Dictionary<string, string> renderDictionary = new Dictionary<string, string>();
            renderDictionary.Add("0.EffectedPersonFullName", string.Format("{0} {1}", persEffected.FirstName, persEffected.FamilyName));
            renderDictionary.Add("0.EffectedPersonUserID", persEffected.UserID);


            Guid? result = mailSender.SendEmailFromTemplate(from, to, null, null, subject, true, null, templateFileName, renderDictionary);
            if (result != null)
            {
                logger.Debug(string.Format("return value from WebServiceEmailSender: {0}", result));
            }
            else
            {
                logger.Debug(string.Format("return value from WebServiceEmailSender is empty - no mail sent"));
            }

        }

        /// <summary>
        /// Copied from TaskItemHandler, creates tasks for a given tuple of Persons and their Employments        
        /// </summary>
        /// <param name="taskItem"></param>
        /// <param name="approver_EmplGUIDs"></param>
        /// <returns></returns>
        public List<EMDTaskItem> CreateTasks(EMDTaskItem taskItem, List<Tuple<string, string>> approver_EmplGUIDs /*List<string> */)
        {
            TaskItemHandler taskItemHandler = new TaskItemHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            if (approver_EmplGUIDs == null || approver_EmplGUIDs.Count < 1)
                return null;

            List<EMDTaskItem> newTasks = new List<EMDTaskItem>();
            string uniqueID = Guid.NewGuid().ToString("N");

            foreach (var approverUserID in approver_EmplGUIDs)
            {
                // add approver specific data
                taskItem.TSK_Approver_EmplGUID = approverUserID.Item1;
                // approverUserID.Item2 == pers guid of approver
                taskItem.TSK_LinkedTasks_ID = uniqueID;
                EMDTaskItem newTask = (EMDTaskItem)taskItemHandler.CreateObject(taskItem);
                newTasks.Add(newTask);
            }

            return newTasks;
        }

        /// <summary>
        /// call this this automatically finish a task (before even first polling) WITHOUT doing a callback
        /// </summary>
        public void AutoFinishTask(List<EMDTaskItem> newTasks, string autoField, string autoValue, string taskNote)
        {
            TaskItemHandler taitH = new TaskItemHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<string> taskGuids = new List<string>();

            //
            // build the auto response according to config
            //
            List<Tuple<string, string>> response = new List<Tuple<string, string>>();
            response.Add(new Tuple<string, string>(autoField, autoValue));

            newTasks.ForEach(i => taskGuids.Add(i.Guid));

            //FinishTasks(taskGuids, response, taskNote , enumTaskStatus.closed);
            foreach (string taskGuid in taskGuids)
            {
                EMDTaskItem finishedTaskItem = (EMDTaskItem)taitH.GetObject<EMDTaskItem>(taskGuid);
                this.CloseTask(taskGuid, response, taskNote, enumTaskStatus.closed, taitH, finishedTaskItem);
            }

        }

        /// <summary>
        /// logic to check if auto approve is allowed
        /// requesting person needs to be in the list of approvers
        /// </summary>
        /// <returns></returns>
        public bool CheckIfAutoApproveIsOK(List<Tuple<string, string>> approverEMPLGuids, string requestingEmplGuid)
        {
            bool result = false;

            result = approverEMPLGuids.Exists(a => a.Item1 == requestingEmplGuid);

            return result;
        }

        /// <summary>
        /// Set task status to closed and close also all linked tasks (parallel tasks) by doing a callback
        /// </summary>
        /// <param name="taskGuids"></param>
        /// <param name="response"></param>
        /// <param name="taskNotes"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public void FinishTasks(List<string> taskGuids, List<Tuple<string, string>> response, string taskNotes, enumTaskStatus taskStatus = enumTaskStatus.closed)
        {
            foreach (string taskGuid in taskGuids)
            {
                FinishTask(taskGuid, response, taskNotes, taskStatus);
            }
        }

        /// <summary>
        /// Set task status to closed and close also all linked tasks (parallel tasks) by doing a callback
        /// </summary>
        /// <param name="taskGuid"></param>
        /// <param name="response"></param>
        /// <param name="taskNotes"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public EMDTaskItem FinishTask(string taskGuid, List<Tuple<string, string>> response, string taskNotes, enumTaskStatus taskStatus = enumTaskStatus.closed)
        {
            TaskItemHandler taitH = new TaskItemHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDTaskItem finishedTaskItem = (EMDTaskItem)taitH.GetObject<EMDTaskItem>(taskGuid);

            if (finishedTaskItem != null)
            {
                this.CloseTask(taskGuid, response, taskNotes, taskStatus, taitH, finishedTaskItem);

                string responseAsXmlString = this.ConvertTaskResponseToXmlString(response);

                logger.Debug("calling callback webservice with parameters: linkedTaskID:" + finishedTaskItem.TSK_LinkedTasks_ID +
                    " Status:" + finishedTaskItem.TSK_Status +
                    " Decision:" + responseAsXmlString ?? string.Empty);

                //now call the Callback in ProcessEngine
                WorkflowCallback workflowCallback = new WorkflowCallback(
                    string.Empty,
                    finishedTaskItem.TSK_LinkedTasks_ID,
                    responseAsXmlString ?? string.Empty,
                    finishedTaskItem.TSK_Status.ToString()
                );
                workflowCallback.Do();

            }

            return finishedTaskItem;
        }

        private void CloseTask(string taskGuid, List<Tuple<string, string>> response, string taskNotes, enumTaskStatus taskStatus, TaskItemHandler taitH, EMDTaskItem finishedTaskItem)
        {

            List<IEMDObject<EMDTaskItem>> linkedTasks =
                        (List<IEMDObject<EMDTaskItem>>)taitH.GetObjects<EMDTaskItem, TaskItem>("TSK_LinkedTasks_ID = \"" + finishedTaskItem.TSK_LinkedTasks_ID + "\"");
            if (linkedTasks != null && linkedTasks.Count > 0)
            {
                // transfer tuple into something serialisable
                string responseXmlAsString = this.ConvertTaskResponseToXmlString(response);

                foreach (EMDTaskItem item in linkedTasks)
                {
                    // set status to closed for all tasks linked
                    try
                    {
                        logger.Debug("set status to closed for all tasks linked. TSK_Guid:" + item.Guid);
                    }
                    catch { }
                    item.TSK_Status = taskStatus.ToString();
                    item.TSK_Decision = responseXmlAsString;
                    item.TSK_Notes = taskNotes;
                    taitH.UpdateObject(item, historize: true);
                }
            }
        }

        public string ConvertTaskResponseToXmlString(List<Tuple<string, string>> response)
        {
            if (response == null) return string.Empty;

            XDocument x = new XDocument();
            XElement root = new XElement("TaskGuiResponse");
            XElement items = new XElement("Items");

            foreach (var responseItem in response)
            {
                XElement colItem = new XElement("Item");
                colItem.SetAttributeValue("key", responseItem.Item1);
                colItem.Value = responseItem.Item2;
                items.Add(colItem);
            }
            root.Add(items);
            x.Add(root);
            string responseXmlAsString = x.ToString(SaveOptions.None);
            return responseXmlAsString;
        }
    }
}
