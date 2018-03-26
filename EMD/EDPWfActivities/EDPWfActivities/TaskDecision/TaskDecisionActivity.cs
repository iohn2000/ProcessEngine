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
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class TaskDecisionActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        internal EMDTaskItem Task = new EMDTaskItem();
        internal Dictionary<string, string> renderDictionary = new Dictionary<string, string>();

        public const string VAR_AUTOFIELD = "autoField";
        public const string VAR_AUTOVALUE = "autoValue";
        public const string VAR_AUTOAPPROVAL = "autoApproval";
        public const string VAR_TASKTITLE = "taskTitle";
        public const string VAR_DECISIONOPTIONS = "decisionOptions";
        public const string VAR_TASKINFORMATION = "taskInformation";
        public const string VAR_REQUESTOR_EMPLGUID = "requestor_EmplGUID";
        public const string VAR_toDo = "toDo";

        internal string EffectedPersonEMPLGuid;
        internal string RequestingPersonEMPLGuid;
        /// <summary>
        /// Tuple (string,tring) == Tuple(approverEMPLGuid, approverPERSGuid)
        /// </summary>
        internal List<Tuple<string, string>> ApproverEMPLGuids;

        internal string TaskTitle, strApprover_Guids;
        internal int nextReminderInDays = -1;
        internal int dueDateInDays = 30;
        internal DateTime dueDate;
        internal DateTime? NextReminder;

        internal string AutoField = "approvalDecision";
        internal string AutoValue = "approve";
        internal bool IsAutoApproval = true;
        internal bool IsEligibleForAutoApprove = false;
        internal List<EMDTaskItem> NewTasks = null;

        public TaskDecisionActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            Variable tmp = null;
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                // read 0-Variables and add them to dictionary 
                try
                {
                    //AddWorkflowZeroVariablesToRenderDictionary(engineContext);
                    this.renderDictionary = base.AddWorkflowZeroVariablesToRenderDictionary(engineContext, renderDictionary);
                }
                catch (Exception ex)
                {
                    // log and continue
                    logger.Warn(base.getWorkflowLoggingContext(engineContext) + " Could not successfully read WorkflowZeroVariables", ex);
                }

                // get taskData values from WF Activity Variables
                this.TaskTitle = base.GetProcessedActivityVariable(engineContext, VAR_TASKTITLE, false).GetStringValue();
                renderDictionary.Add("taskTitle", this.TaskTitle);

                try
                {
                    Variable varContent = base.GetProcessedActivityVariable(engineContext, "nextReminder", false);
                    nextReminderInDays = varContent.GetIntValue();
                    if (nextReminderInDays > -1)
                    {
                        this.NextReminder = DateTime.Now.AddDays(dueDateInDays);
                        WriteDateTimeToRenderDictionary(engineContext, renderDictionary, DateTime.Now.AddDays(Convert.ToDouble(nextReminderInDays)), "nextReminder");
                    }
                    else
                    {
                        this.NextReminder = null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(base.getWorkflowLoggingContext(engineContext) + " Could not read nextReminder.Value  => set empty ", ex);
                    renderDictionary.Add("nextReminder", "");
                    this.NextReminder = null;
                }

                try
                {
                    Variable varContent = base.GetProcessedActivityVariable(engineContext, "dueDate", false);
                    dueDateInDays = varContent.GetIntValue();
                    dueDate = DateTime.Now.AddDays(dueDateInDays);
                    WriteDateTimeToRenderDictionary(engineContext, renderDictionary, dueDate, "dueDate");
                }
                catch (Exception ex)
                {
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " Could not read dueDate.Value  => cannot continue!", ex);
                    throw ex;
                }

                string strDecisionOptions = base.GetProcessedActivityVariable(engineContext, VAR_DECISIONOPTIONS, false).GetStringValue();
                string taskInformation = base.GetProcessedActivityVariable(engineContext, VAR_TASKINFORMATION, false).GetStringValue();

                RequestingPersonEMPLGuid = base.GetProcessedActivityVariable(engineContext, VAR_REQUESTOR_EMPLGUID, false).GetStringValue();
                string toDo = base.GetProcessedActivityVariable(engineContext, VAR_toDo, false).GetStringValue();
                bool? isBulk = base.GetProcessedActivityVariable(engineContext, "isBulk", false).GetBooleanValue();
                //
                // build list of approver EMPL Guids
                // need core with approver search
                //
                EffectedPersonEMPLGuid = base.GetProcessedActivityVariable(engineContext, "effectedPerson_EmplGUID", false).GetStringValue();
                strApprover_Guids = base.GetProcessedActivityVariable(engineContext, "approver_EmplGUIDs", false).GetStringValue();

                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " Approvers configured: " + strApprover_Guids);

                TaskItemManager tMgr = new TaskItemManager();
                this.ApproverEMPLGuids = tMgr.FindTaskApproverForEffectedPerson(strApprover_Guids.Split(',').ToList(), this.EffectedPersonEMPLGuid);

                if (this.ApproverEMPLGuids == null || this.ApproverEMPLGuids.Count < 1)
                {
                    string errmMsg = string.Format("{0} Cannot find approver with given approver codes:'{1}'; eff.Pers.:'{2}'",
                        base.getWorkflowLoggingContext(engineContext),
                        strApprover_Guids,
                        this.EffectedPersonEMPLGuid
                        );
                    logger.Error(errmMsg);
                    ret.ReturnValue = errmMsg;
                    ret.StepState = EnumStepState.ErrorToHandle;
                    return ret;
                }
                //
                // auto approve
                //
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_AUTOFIELD, true);
                if (tmp != null && !string.IsNullOrWhiteSpace(tmp.VarValue))
                    this.AutoField = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_AUTOVALUE, true);
                if (tmp != null && !string.IsNullOrWhiteSpace(tmp.VarValue))
                    this.AutoValue = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_AUTOAPPROVAL, true);
                if (tmp != null && !string.IsNullOrWhiteSpace(tmp.VarValue))
                    this.IsAutoApproval = tmp.GetBooleanValue().Value;

                this.IsEligibleForAutoApprove = this.CheckIfAutoApproveIsOK();
                //
                // prefill task object, all the info that is common to all tasks (in case same task is sent to multiple approvers 
                //
                this.Task = PreFillTaskObject(engineContext, strDecisionOptions, taskInformation, toDo, isBulk);

                logger.Debug(string.Format("{0} Taskdata provided: {1}", base.getWorkflowLoggingContext(engineContext), this.Dump(this.Task)));
                // done creation of taskObject
            }
            catch (Exception ex)
            {
                string errmMsg = base.getWorkflowLoggingContext(engineContext) + " : Error collecting infos for creating tasks.";
                logger.Error(errmMsg, ex);
                logger.Debug("Errornous Taskdata created: " + this.Dump(this.Task));
                ret.ReturnValue = errmMsg;
                ret.DetailedDescription = ex.ToString();
                ret.StepState = EnumStepState.ErrorToHandle;
                return ret;
            }

            return ret;

        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                // unique id for this async item
                string uniqueID = engineContext.WorkflowModel.InstanceID + "__" + engineContext.CurrenActivity.Instance;

                // CREATE TASK(S)
                TaskItemHandler taskItemHandler = new TaskItemHandler();

                this.NewTasks = taskItemHandler.CreateTasks(Task, ApproverEMPLGuids);

                StringWriter sw = new StringWriter();
                ObjectDumper.Write(Task, 1, sw);
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " TaskData: " + sw.ToString());

                StringWriter sw2 = new StringWriter();
                ObjectDumper.Write(ApproverEMPLGuids, 1, sw2);
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " Dump ApproverEMPL Guids: " + sw2.ToString());

                if (NewTasks == null)
                {
                    string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error creating tasks";
                    logger.Error(errMsg);
                    ret.ReturnValue = errMsg;
                    ret.StepState = EnumStepState.ErrorToHandle;
                    return ret;
                }

                // WAIT ITEM
                // create info so that WAIT item can poll later...
                XDocument xmlWaitItemConfig = new XDocument();
                XElement root = new XElement("root");

                XElement item = new XElement("item");
                string taskLinkID = NewTasks[0].TSK_LinkedTasks_ID; // this id links tasks together, can be used even if only 1 task is created
                item.SetAttributeValue("name", "newLinkedTaskID");
                item.SetAttributeValue("value", taskLinkID);
                root.Add(item);

                item = new XElement("item");
                item.SetAttributeValue("name", "wfUniqueUD");
                item.SetAttributeValue("value", uniqueID);
                root.Add(item);

                item = new XElement("item");
                item.SetAttributeValue("name", "taskGUID");
                item.SetAttributeValue("value", NewTasks[0].Guid.ToString());
                root.Add(item);

                xmlWaitItemConfig.Add(root);

                string strWaitItemConfig = xmlWaitItemConfig.ToString(SaveOptions.None);

                //creating waititem to be read by isResultAvailable()
                DatabaseAccess db = new DatabaseAccess();
                db.CreateWaitItem(engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance, strWaitItemConfig, dueDate);

                logger.Debug(string.Format("{0} : ApprovalTask : {1}  Approver : {2}", base.getWorkflowLoggingContext(engineContext), TaskTitle, strApprover_Guids));
                //
                // auto finish tasks if IsEligible AND autoApproval is turned on
                //
                if (this.IsEligibleForAutoApprove && this.IsAutoApproval)
                {
                    try
                    {
                        this.AutoFinishTask(NewTasks);
                    }
                    catch (BaseException bEx)
                    {
                        string errMsg = "Error to auto finish tasks.";
                        return this.logErrorAndReturnStepState(engineContext, bEx, errMsg, EnumStepState.ErrorToHandle);
                    }
                }
                //
                // send Task Email, but only if not auto approval
                //
                if (!this.IsEligibleForAutoApprove)
                {
                    try
                    {
                        string bodyTemplate = base.GetProcessedActivityVariable(engineContext, "emailBody", false).GetStringValue();
                        string taskTitle = base.GetProcessedActivityVariable(engineContext, "taskTitle", false).GetStringValue();
                        Variable tmp = null;
                        tmp = base.GetProcessedActivityVariable(engineContext, "isBodyHtml", true);
                        bool htmlBody;
                        if (tmp != null)
                            if (!bool.TryParse(tmp.VarValue, out htmlBody))
                                htmlBody = true;
                            else
                                htmlBody = true;
                        else
                            htmlBody = true;
                        TaskHelper.SendTaskMail(this, engineContext, renderDictionary, ApproverEMPLGuids, bodyTemplate, taskTitle, htmlBody);
                    }
                    catch (Exception ex)
                    {
                        string errMsg = "Error trying to send an approver email.";
                        return this.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.ErrorToHandle);
                    }
                }

                return ret; // all ok
            }
            catch (Exception ex)
            {
                string errmMsg = base.getWorkflowLoggingContext(engineContext) + " : Error creating tasks";
                return this.logErrorAndReturnStepState(engineContext, ex, errmMsg, EnumStepState.ErrorToHandle);
            }

        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                engineContext.SetActivityVariable("FirstTaskItemGuid", this.NewTasks[0].Guid, true);
            }
            catch(Exception ex)
            {
                ret = base.logErrorAndReturnStepState(engineContext,ex,ex.Message,EnumStepState.ErrorToHandle);
            }

            return ret;
        }

        private EMDTaskItem PreFillTaskObject(EngineContext engineContext, string strDecisionOptions, string taskInformation, string toDo, bool? isBulk)
        {
            this.Task.TSK_DateNextReminder = DateTime.Now.AddDays(nextReminderInDays);
            this.Task.TSK_DateRequested = DateTime.Now;
            this.Task.TSK_Decision = string.Empty;
            this.Task.TSK_DecisionOptions = strDecisionOptions;
            this.Task.TSK_Duedate = DateTime.Now.AddDays(dueDateInDays);
            this.Task.TSK_EffectedPerson_EmplGUID = EffectedPersonEMPLGuid;
            this.Task.TSK_Information = taskInformation;
            this.Task.TSK_Status = enumTaskStatus.open.ToString();
            this.Task.TSK_DateNextReminder = this.NextReminder;
            //
            //read Workflow Specific Processdata from Engine and write it to Task
            //
            DatabaseAccess databaseAccess = new DatabaseAccess();
            WFEWorkflowInstance instance = databaseAccess.GetWorkflowInstance(engineContext.WorkflowModel.InstanceID);
            WFEWorkflowDefinition wfDefinition = databaseAccess.GetWorkflowDefinition(instance.WFI_WFD_ID);

            this.Task.TSK_ProcessGuid = instance.WFI_WFD_ID;
            this.Task.TSK_ProcessName = wfDefinition.WFD_Name;
            this.Task.TSK_IsBulkActivity = isBulk == null ? false : isBulk.Value;
            this.Task.TSK_TaskActivityID = engineContext.CurrenActivity.Instance;

            this.Task.TSK_Requestor_EmplGUID = RequestingPersonEMPLGuid;
            this.Task.TSK_TaskTitle = TaskTitle;
            this.Task.TSK_ToDo = toDo;

            return this.Task;
        }

        /// <summary>
        /// logic to check if auto approve is allowed
        /// requesting person needs to be in the list of approvers
        /// </summary>
        /// <returns></returns>
        private bool CheckIfAutoApproveIsOK()
        {
            bool result = false;

            bool reqPersIsApprover = this.ApproverEMPLGuids.Exists(a => a.Item1 == this.RequestingPersonEMPLGuid);

            result = reqPersIsApprover;

            return result;
        }

        /// <summary>
        /// call this this automatically finish a task (before even first polling)
        /// </summary>
        private void AutoFinishTask(List<EMDTaskItem> newTasks)
        {
            TaskItemHandler taitH = new TaskItemHandler();
            List<string> taskGuids = new List<string>();

            //
            // build the auto response according to config
            //
            List<Tuple<string, string>> response = new List<Tuple<string, string>>();
            response.Add(new Tuple<string, string>(this.AutoField, this.AutoValue));

            newTasks.ForEach(i => taskGuids.Add(i.Guid));

            taitH.FinishTasks(taskGuids, response, "automatically closed", enumTaskStatus.closed);
        }

        private DateTime? WriteDateTimeToRenderDictionary(EngineContext engineContext, Dictionary<string, string> renderDictionary, DateTime date, string varName)
        {
            if (date == null)
                renderDictionary.Add(varName, "");
            else
                renderDictionary.Add(varName, date.ToString("dd MM yyyy"));

            return date;
        }

        /// <summary>
        /// TODO: Put this Function into Baseactivity
        /// </summary>
        /// <param name="engineContext"></param>
        private void AddWorkflowZeroVariablesToRenderDictionary(EngineContext engineContext)
        {
            Variable tempVariable;
            var nullPunkts = (List<Variable>) engineContext.WorkflowModel.GetPunktVariables("[starts-with(@name,'0.')]");
            foreach (var nullVariable in nullPunkts)
            {
                tempVariable = engineContext.GetWorkflowVariable(nullVariable.Name);
                renderDictionary.Add(nullVariable.Name, nullVariable.VarValue);
            }
        }

        //TODO: But in Base Library
        private string Dump(EMDTaskItem task)
        {
            StringWriter sw = new StringWriter();
            ObjectDumper.Write(task, 2, sw);
            return sw.ToString();
        }

        #region validation


        public string Validate(XElement activity)
        {
            return null;
        }

        public string Validate(string activityXml)
        {
            return null;
        }


        #endregion
    }
}
