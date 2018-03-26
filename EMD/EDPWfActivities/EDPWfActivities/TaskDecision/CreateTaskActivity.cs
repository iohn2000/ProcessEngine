using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    /// <summary>
    /// Create a task for a given list of so called approvers.
    /// First implementation of Autocompleteing Callback Activity
    /// </summary>
    class CreateTaskActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        private const string DATEFORMAT = "dd MM yyyy";

        private const string VAR_CLIENTREFERENCEID = "ClientReferenceID";
        private const string VAR_AUTOCOMPLETED = "AutoCompleted";
        
        /// <summary>
        /// fieldname for Autoapproval example:
        /// </summary>
        private const string VAR_AUTOFIELD = "autoField";
        
        /// <summary>
        /// fieldvalue for Autoapproval
        /// </summary>
        private const string VAR_AUTOVALUE = "autoValue";

        /// <summary>
        /// Text to be set with autoapproval as info
        /// </summary>
        private const string VAR_AUTOTEXT = "autoText";

        /// <summary>
        /// Name of the task in Taskmanagement example: "Approval for: " + "{{0.EffectedPersonFullName}}" + " for Equipment: "  + "{{0.EQDE_Name}}"
        /// </summary>
        private const string VAR_TASKTITLE = "taskTitle";

        /// <summary>
        /// Example:
        /// <!-- <Taskfields>
        /// <Field id = "approvalDecision" auto="true" type="dropdown" name="Decision" description="Please make a decision:">
        ///  <option auto="true" value = "approved" > approved </ option >
        ///  < option value="declined">declined</option>
        /// </Field>
        /// <Field id = "approvalComment" type="textbox" name="Comment" description="Further comments:"></Field>
        /// </Taskfields> -->
        /// </summary>
        private const string VAR_DECISIONOPTIONS = "decisionOptions";

        /// <summary>
        /// free text with information for the taskowner
        /// Example: "The equipment " + "{{0.EQDE_Name}}" + " has been requested for: " + "{{0.EffectedPersonFullName}}" + ". Please select your decision
        /// </summary>
        private const string VAR_TASKINFORMATION = "taskInformation";

        /// <summary>
        /// employment of effected person
        /// </summary>
        private const string VAR_EFFECTEDPERSON_EMPLGUID = "effectedPerson_EmplGUID";

        /// <summary>
        /// Task initiator (important for autoapproval)
        /// </summary>
        private const string VAR_REQUESTOR_EMPLGUID = "requestor_EmplGUID";

        /// <summary>
        /// reminder in days to be sent via job lateron
        /// </summary>
        private const string VAR_NEXTREMINDERINDAYS = "nextReminderInDays";

        /// <summary>
        /// Output variable for date of the next reminder
        /// </summary>
        private const string VAR_NEXTREMINDERDATE = "nextReminderDate";

        /// <summary>
        /// date until this task must be done otherwise
        /// </summary>
        private const string VAR_DUEDATEINDAYS = "dueDateInDays";

        /// <summary>
        /// date until this task must be done otherwise
        /// </summary>
        private const string VAR_DUEDATE = "dueDate";

        /// <summary>
        /// This is a text to inform what the taskowner has to do: "Please approve the assigned task!" 
        /// </summary>
        private const string VAR_TODO = "toDo";

        /// <summary>
        /// List of employments to become taskowner EMPL_29349342...;EMPL_34952034...;EMPL_2342034ae...
        /// or: "KSTL_" + "{{0.EffectedPersonEmploymentGuid}}" 
        /// </summary>
        private const string VAR_APPROVER_EMPLGUIDS = "approver_EmplGUIDs";

        /// <summary>
        /// override for bulkability
        /// </summary>
        private const string VAR_ISBULK = "isBulk";

        internal string ClientReferenceID = "empty";
        internal string AutoField = "approvalDecision";
        internal string AutoValue = "approved";
        internal bool IsAutoApproval = false;
        internal string TaskTitle;
        internal string DecisionOptions;
        internal string TaskInformation;
        internal string RequestingPersonEMPLGuid;
        internal int NextReminderInDays;
        internal DateTime NextReminderDate;
        internal int DueDateInDays;
        internal DateTime DueDate;
        internal string ToDo;
        internal string EffectedPersonEMPLGuid;
        internal string ApproverEmplGuids;
        internal bool? IsBulk = true;

        internal List<Tuple<string, string>> ApproverEMPLandPERSGuidsTuple;
        internal EMDTaskItem PreTask;
        internal List<EMDTaskItem> NewTasks;
        private string AutoText = "Autoapproval because Requestor is one of the approvers by ApproverRole";


        
        public CreateTaskActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            //read variables from config
            //private const string VAR_CLIENTREFERENCEID = "ClientReferenceID"; => this will be the linkedtaskid
            this.ClientReferenceID = null;

            //AUTOFIELD = "autoField"; ex: "approvalDecision"
            this.AutoField = base.GetProcessedActivityVariable(engineContext, VAR_AUTOFIELD, false).GetStringValue();

            //AUTOVALUE = "autoValue"; ex: "approved"
            this.AutoValue= base.GetProcessedActivityVariable(engineContext, VAR_AUTOVALUE, false).GetStringValue();

            //AUTOTEXT
            Variable varContent = base.GetProcessedActivityVariable(engineContext, VAR_AUTOTEXT, true);
            if (varContent != null) {
                this.AutoText = varContent.GetStringValue();
            }

            //TASKTITLE
            this.TaskTitle = base.GetProcessedActivityVariable(engineContext, VAR_TASKTITLE, false).GetStringValue();

            //DECISIONOPTIONS
            this.DecisionOptions = base.GetProcessedActivityVariable(engineContext, VAR_DECISIONOPTIONS, false).GetStringValue();

            //TASKINFORMATION 
            this.TaskInformation = base.GetProcessedActivityVariable(engineContext, VAR_TASKINFORMATION, false).GetStringValue();

            //REQUESTOR_EMPLGUID
            this.RequestingPersonEMPLGuid = base.GetProcessedActivityVariable(engineContext, VAR_REQUESTOR_EMPLGUID, false).GetStringValue();

            //NEXTREMINDER
            varContent = base.GetProcessedActivityVariable(engineContext, VAR_NEXTREMINDERINDAYS, false);
            this.NextReminderInDays = varContent.GetIntValue();
            if (this.NextReminderInDays > 0)
            {
                this.NextReminderDate = DateTime.Now.AddDays(NextReminderInDays);                
            }
            else
            {
                BaseException bex = new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "no nextReminder in days > 0 given for Task");
                return this.logErrorAndReturnStepState(engineContext, bex, bex.Message ,EnumStepState.ErrorToHandle);
            }

            //DUEDATE
            varContent = base.GetProcessedActivityVariable(engineContext, VAR_DUEDATEINDAYS, false);
            this.DueDateInDays = varContent.GetIntValue();
            if (this.DueDateInDays > 0)
            {               
                this.DueDate = DateTime.Now.AddDays(this.DueDateInDays);                
            }
            else
            {
                BaseException bex = new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "no dueDate in days > 0 given for Task");
                return this.logErrorAndReturnStepState(engineContext, bex, bex.Message, EnumStepState.ErrorToHandle);
            }

            //TODO
            this.ToDo = base.GetProcessedActivityVariable(engineContext, VAR_TODO, false).GetStringValue();

            //EFFECTEDPERSON_EMPLGUID
            this.EffectedPersonEMPLGuid = base.GetProcessedActivityVariable(engineContext, VAR_EFFECTEDPERSON_EMPLGUID, false).GetStringValue();

            //APPROVER_EMPLGUIDS
            this.ApproverEmplGuids = base.GetProcessedActivityVariable(engineContext, VAR_APPROVER_EMPLGUIDS, false).GetStringValue();            

            //VAR_ISBULK
            this.IsBulk = base.GetProcessedActivityVariable(engineContext, VAR_ISBULK, false).GetBooleanValue();

            return result;

        }



        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                this.ProcessApproverGuidDefinition(engineContext, result);
                this.NewTasks = this.CreateTaskItems(engineContext);
                if (this.NewTasks == null || this.NewTasks.Count() == 0)
                {
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN, "TaskCreation resulted in 0 Tasks to create");                    
                }
                this.ClientReferenceID = this.NewTasks[0].TSK_LinkedTasks_ID;

                StringWriter sw = new StringWriter();
                ObjectDumper.Write(this.PreTask, 1, sw);
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " TaskData: " + sw.ToString());

                StringWriter sw2 = new StringWriter();
                ObjectDumper.Write(ApproverEMPLandPERSGuidsTuple, 1, sw2);
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " Dump ApproverEMPL Guids: " + sw2.ToString());

                TaskItemManager taskItemManager = new TaskItemManager(null, "Workflow via CreateTaskActivity");

                bool isEligibleForAutoApprove = taskItemManager.CheckIfAutoApproveIsOK(this.ApproverEMPLandPERSGuidsTuple,this.RequestingPersonEMPLGuid);

                if (isEligibleForAutoApprove)
                {
                    try
                    {
                        logger.Debug(base.getWorkflowLoggingContext(engineContext) + "try to AutoFinish task => Autoapproval because Requestor is one of the approvers by ApproverRole");
                        taskItemManager.AutoFinishTask(NewTasks,this.AutoField,this.AutoValue,this.AutoText);
                        IsAutoApproval = true;
                    }
                    catch (BaseException bEx)
                    {
                        string errMsg = "Error by trying to auto finish tasks.";
                        return this.logErrorAndReturnStepState(engineContext, bEx, errMsg, EnumStepState.ErrorToHandle);
                    }
                } else
                {
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + "no autoapproval possible => standard behavior");
                }

                result = new StepReturn("Created EDP Tasks with linked id: " + this.ClientReferenceID, EnumStepState.Complete);
            }
            catch (BaseException bEx)
            {
                string errorMessage = "Error while creating EDP Task. Please contact the EDP Team.";
                result = this.logErrorAndReturnStepState(engineContext, bEx, errorMessage, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error while creating EDP Task. Please contact the EDP Team.";
                result = this.logErrorAndReturnStepState(engineContext, ex, errorMessage, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            engineContext.SetActivityVariable(VAR_CLIENTREFERENCEID, this.ClientReferenceID);
            engineContext.SetActivityVariable(VAR_DUEDATE, this.DueDate.ToString(DATEFORMAT));
            engineContext.SetActivityVariable(VAR_NEXTREMINDERDATE, this.NextReminderDate.ToString(DATEFORMAT));
            engineContext.SetActivityVariable(VAR_AUTOCOMPLETED, this.IsAutoApproval.ToString());

            return result;
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        #region helperfunctions
        private void ProcessApproverGuidDefinition(EngineContext engineContext, StepReturn actualStepResult)
        {
            StepReturn ret = actualStepResult;
            logger.Debug(base.getWorkflowLoggingContext(engineContext) + " calculating Approvers configured: " + this.ApproverEmplGuids);

            TaskItemManager tMgr = new TaskItemManager();
            ApproverEMPLandPERSGuidsTuple = tMgr.FindTaskApproverForEffectedPerson(this.ApproverEmplGuids.Split(',').ToList(), this.EffectedPersonEMPLGuid);

            if (ApproverEMPLandPERSGuidsTuple == null || this.ApproverEMPLandPERSGuidsTuple.Count < 1)
            {                
                string errMsg = string.Format("{0} Cannot find approver with given approver codes:'{1}'; eff.Pers.:'{2}'",
                    base.getWorkflowLoggingContext(engineContext),
                    this.ApproverEmplGuids,
                    this.EffectedPersonEMPLGuid
                    );

                BaseException bex = new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, errMsg);
                throw bex;
            }
        }
 
        /// <summary>
        /// encapsulated method to create a task.
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        private List<EMDTaskItem> CreateTaskItems(EngineContext engineContext)
        {
            DatabaseAccess databaseAccess = new DatabaseAccess();
            WFEWorkflowInstance instance = databaseAccess.GetWorkflowInstance(engineContext.WorkflowModel.InstanceID);
            WFEWorkflowDefinition wfDefinition = databaseAccess.GetWorkflowDefinition(instance.WFI_WFD_ID);

            this.PreTask = this.CreatePreTask(engineContext, instance, wfDefinition);

            TaskItemManager taskItemManager = new TaskItemManager(null, "Workflow via CreateTaskActivity");

            return taskItemManager.CreateTasks(this.PreTask, ApproverEMPLandPERSGuidsTuple);
        }

        private EMDTaskItem CreatePreTask(EngineContext engineContext, WFEWorkflowInstance instance, WFEWorkflowDefinition wfDefinition)
        {
          EMDTaskItem preTask = new EMDTaskItem();

           preTask.TSK_DateNextReminder = DateTime.Now.AddDays(NextReminderInDays);
           preTask.TSK_DateRequested = DateTime.Now;
           preTask.TSK_Decision = string.Empty;
           preTask.TSK_DecisionOptions = this.DecisionOptions;
           preTask.TSK_Duedate = DateTime.Now.AddDays(DueDateInDays);
           preTask.TSK_EffectedPerson_EmplGUID = EffectedPersonEMPLGuid;
           preTask.TSK_Information = this.TaskInformation;
           preTask.TSK_Status = enumTaskStatus.open.ToString();
           
           preTask.TSK_ProcessGuid = instance.WFI_WFD_ID;
           preTask.TSK_ProcessName = wfDefinition.WFD_Name;
           preTask.TSK_IsBulkActivity = this.IsBulk == null ? false : IsBulk.Value;
           preTask.TSK_TaskActivityID = engineContext.CurrenActivity.Instance;
           
           preTask.TSK_Requestor_EmplGUID = RequestingPersonEMPLGuid;
           preTask.TSK_TaskTitle = TaskTitle;
           preTask.TSK_ToDo = this.ToDo;

            return preTask;
        }



        #endregion
    }
}
