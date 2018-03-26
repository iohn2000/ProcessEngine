using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.EquipmentDef;
using Kapsch.IS.EDP.Core.Framework;
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
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class EquipmentTaskActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        public EquipmentTaskActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public const string VAR_TASKTITLE = "taskTitle";
        public const string VAR_EFFECTEDPERSON_EMPLGUID = "effectedPerson_EmplGUID";
        public const string VAR_REQUESTINGPERSON_EMPLGUID = "requestingPerson_EmplGUID";
        public const string VAR_TASKRECEIVER = "taskreceiver_EmplGUID"; //old approver
        public const string VAR_TODO = "toDo";
        public const string VAR_OBRE = "obre";
        public const string VAR_DECISIONOPTIONS = "decisionOptions";
        public const string VAR_TASKINFORMATION = "taskInformation";
        public const string VAR_DUEDATE = "dueDate";
        public const string VAR_NEXTREMINDER = "nextReminder";
        public const string VAR_ISBULK = "isBulk";
        public const string VAR_TASKEMAILTEMPLATE = "taskMailTemplate";
        public const string VAR_DEMORECIPIENT = "demoRecipient";
        public const string VAR_ISBODYHTML = "isBodyHtml";
        public const string VAR_HASDYNAMICFIELDSSELECTED = "hasDynamicFieldsSelected";


        private Dictionary<string, string> renderDictionary = new Dictionary<string, string>();
        private EMDTaskItem task = new EMDTaskItem();
        private WorkflowLogger wfLogger;

        private string taskTitle;
        private string effectedPersEmplGuid;
        private string requestingPersEmplGuid;
        private string toDo;
        private string obreGuid;
        private string decisionOptions;
        private string taskInformation;
        private int dueDateInDays;
        private DateTime dueDate;
        private int nextReminderInDays = -1;
        private DateTime nextReminderDate;
        private bool? isBulk;
        private string taskEmailTemplate;
        private bool? isBodyHtml;
        private string demoRecipient;
        private List<Tuple<string, string>> approverEMPLGuids;
        private List<DynamicField> myEqDynFields;
        private string decisionOptionsWithEqDynFields;
        private bool HasDynamicFieldsSelected = false;
        private int amountHardCodedFields = 0;

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);
            TaskItemManager tMgr = new TaskItemManager();
            string procEngCtx = base.getWorkflowLoggingContext(engineContext);
            //
            // read variables
            //
            try
            {
                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_TASKTITLE, false);
                this.taskTitle = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_EFFECTEDPERSON_EMPLGUID, false);
                this.effectedPersEmplGuid = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_REQUESTINGPERSON_EMPLGUID, false);
                this.requestingPersEmplGuid = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_TODO, false);
                this.toDo = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_OBRE, false);
                this.obreGuid = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_DECISIONOPTIONS, false);
                this.decisionOptions = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_TASKINFORMATION, false);
                this.taskInformation = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_DUEDATE, false);
                this.dueDateInDays = tmp.GetIntValue();
                this.dueDate = DateTime.Now.AddDays(dueDateInDays);
                this.renderDictionary = base.WriteDateTimeToDictionary(engineContext, renderDictionary, dueDate, VAR_DUEDATE);

                // TODO nextReminder is not implemented at the moment
                if (this.nextReminderInDays > -1)
                {
                    tmp = base.GetProcessedActivityVariable(engineContext, VAR_NEXTREMINDER, false);
                    this.nextReminderInDays = tmp.GetIntValue();
                    this.nextReminderDate = DateTime.Now.AddDays(nextReminderInDays);
                    this.renderDictionary = base.WriteDateTimeToDictionary(engineContext, renderDictionary, nextReminderDate, VAR_NEXTREMINDER);
                }

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_ISBULK, false);
                this.isBulk = tmp.GetBooleanValue();

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_TASKEMAILTEMPLATE, false);
                this.taskEmailTemplate = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_ISBODYHTML, false);
                this.isBodyHtml = tmp.GetBooleanValue();

                this.demoRecipient = base.GetSettingWithFallbackToAppConfig(engineContext, VAR_DEMORECIPIENT, "DefaultDemoModeRecipient", "KIBSI-EDP-admin@kapsch.net");
                if (base.isDemoModeOn(engineContext))
                    logger.Debug(procEngCtx + " : DEMO Mode is ON. Recipient is now : " + this.demoRecipient);

                //VAR_TASKRECEIVER
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_TASKRECEIVER, false);
                string strApprover_Guids = tmp.GetStringValue();
                if (string.IsNullOrWhiteSpace(strApprover_Guids))
                {
                    string msg = procEngCtx + "No approver found in approver_EmplGUIDs";
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, msg);
                }
                else
                    logger.Debug(procEngCtx + "Approvers configured: " + strApprover_Guids);

                this.approverEMPLGuids = tMgr.FindTaskApproverForEffectedPerson(strApprover_Guids.Split(',').ToList(), this.effectedPersEmplGuid);
                if (approverEMPLGuids == null || approverEMPLGuids.Count < 1)
                {
                    string msg = procEngCtx + "Given approver not found by FindTaskApproverForEffectedPerson: " + this.effectedPersEmplGuid;
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, msg);
                }
            }
            catch (BaseException bEx)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, bEx, msg + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }


            try
            {
                //
                // get configures EqDyn Variables 
                //
                ObjectRelationHandler obreH = new ObjectRelationHandler();
                this.myEqDynFields = obreH.GetDynamicEquipmentFieldsForObreGuid(this.obreGuid);
                this.HasDynamicFieldsSelected = this.myEqDynFields.Count > 0;

                try
                {
                    if (string.IsNullOrWhiteSpace(this.decisionOptions))
                    {
                        this.decisionOptions = "<Taskfields></Taskfields>";
                    }
                    var amountFields = XDocument.Parse(this.decisionOptions).XPathSelectElements("/Taskfields/Field");
                    this.amountHardCodedFields = amountFields.Count();
                }
                catch (Exception)
                {
                    this.amountHardCodedFields = 0;
                }

                //
                // add eq dyn vars to task
                //
                this.decisionOptionsWithEqDynFields = TaskHelper.AddDynamicFieldsToDecisionOptions(this.decisionOptions, this.myEqDynFields);

                if (this.amountHardCodedFields == 0)
                {
                    logger.Debug(string.Format("{0} No taskfields for EquipmentTask configured in Workflow XML.", procEngCtx));
                }

                this.PrefillTaskObject(engineContext);
            }
            catch (BaseException bEx)
            {
                string msg = "Could not successfully prepare task item";
                return this.logErrorAndReturnStepState(engineContext, bEx, msg + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "Could not successfully prepare task item";
                return this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("", EnumStepState.Complete);

            // if there is no dynamic fields configured dont create any tasks
            if (!this.HasDynamicFieldsSelected)
                return result;

            CoreTransaction coreTransaction = new CoreTransaction();

            string requestingPersonPERSGuid = null;
            try
            {
                // unique id for this async item
                string uniqueID = engineContext.WorkflowModel.InstanceID + "__" + engineContext.CurrenActivity.Instance;

                // pers guid requesting pers
                try
                {
                    requestingPersonPERSGuid = ((EMDEmployment) new EmploymentHandler().GetObject<EMDEmployment>(this.requestingPersEmplGuid)).P_Guid;
                }
                catch
                {
                    // do nothing just use null
                    logger.Warn(string.Format("{0} : Cannot find PersGuid for requesting person: {1}",
                                    base.getWorkflowLoggingContext(engineContext), this.requestingPersEmplGuid));
                }

                // CREATE TASK(S)
                try
                {
                    coreTransaction.Begin();
                    TaskItemHandler taskItemHandler = new TaskItemHandler(coreTransaction, requestingPersonPERSGuid, "task created by EquipmentTaskActivity");
                    List<EMDTaskItem> newTasks = null;
                    newTasks = taskItemHandler.CreateTasks(task, this.approverEMPLGuids);

                    if (newTasks == null)
                    {
                        // throw execption and rollback
                        throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN, "Error creating task item in database.");
                    }

                    StringWriter sw = new StringWriter(); ObjectDumper.Write(task, 1, sw);
                    logger.Debug(string.Format("{0} : TaskData: {1}", base.getWorkflowLoggingContext(engineContext), sw.ToString()));
                    StringWriter sw2 = new StringWriter(); ObjectDumper.Write(this.approverEMPLGuids, 1, sw2);
                    logger.Debug(string.Format("{0} : Dump ApproverEMPL Guids: {1}", base.getWorkflowLoggingContext(engineContext), sw2.ToString()));

                    //
                    // WAIT ITEM
                    // create info so that WAIT item can poll later...
                    //
                    XDocument xmlWaitItemConfig = new XDocument();
                    xmlWaitItemConfig = this.createWaitItem(uniqueID, newTasks, xmlWaitItemConfig);
                    string strWaitItemConfig = xmlWaitItemConfig.ToString(SaveOptions.None);

                    //
                    // creating waititem to be read by isResultAvailable()
                    //
                    DatabaseAccess db = new DatabaseAccess();
                    db.CreateWaitItem(engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance, strWaitItemConfig, dueDate);


                    logger.Debug(string.Format("{0} : ApprovalTask : {1}  Approver : {2}", base.getWorkflowLoggingContext(engineContext), taskTitle, this.approverEMPLGuids));
                    coreTransaction.Commit();
                }
                catch (Exception ex)
                {
                    coreTransaction.Rollback();
                    string errmMsg = base.getWorkflowLoggingContext(engineContext) + " : Error creating tasks. Rollback() executed.";
                    return this.logErrorAndReturnStepState(engineContext,ex,errmMsg,EnumStepState.ErrorToHandle);
                }

                try
                {
                    TaskHelper.SendTaskMail(this, engineContext, renderDictionary, approverEMPLGuids, this.taskEmailTemplate, this.taskTitle, this.isBodyHtml.Value);
                }
                catch (Exception ex)
                {
                    // log error sending email but dont stop workflow
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error trying to send an approver email.", ex);
                }
                return result; // all ok
            }
            catch (Exception ex)
            {
                string errmMsg = base.getWorkflowLoggingContext(engineContext) + " : Error creating tasks.";
                return this.logErrorAndReturnStepState(engineContext,ex,errmMsg,EnumStepState.ErrorToHandle);
            }
        }

        private XDocument createWaitItem(string uniqueID, List<EMDTaskItem> newTasks, XDocument xmlWaitItemConfig)
        {
            XElement root = new XElement("root");

            XElement item = new XElement("item");
            string taskLinkID = newTasks[0].TSK_LinkedTasks_ID; // this id links tasks together, can be used even if only 1 task is created
            item.SetAttributeValue("name", "newLinkedTaskID");
            item.SetAttributeValue("value", taskLinkID);
            root.Add(item);

            item = new XElement("item");
            item.SetAttributeValue("name", "wfUniqueUD");
            item.SetAttributeValue("value", uniqueID);
            root.Add(item);

            item = new XElement("item");
            item.SetAttributeValue("name", "taskGUID");
            item.SetAttributeValue("value", newTasks[0].Guid.ToString());
            root.Add(item);

            xmlWaitItemConfig.Add(root);
            return xmlWaitItemConfig;
        }

        /// <summary>
        /// pre-fill task objet with data that is the same for every approver (receiver)
        /// </summary>
        /// <param name="engineContext"></param>
        private void PrefillTaskObject(EngineContext engineContext)
        {
            task.TSK_DateNextReminder = DateTime.Now.AddDays(this.nextReminderInDays);
            task.TSK_DateRequested = DateTime.Now;
            task.TSK_Decision = string.Empty;
            task.TSK_DecisionOptions = this.decisionOptionsWithEqDynFields;
            task.TSK_Duedate = DateTime.Now.AddDays(this.dueDateInDays);
            task.TSK_EffectedPerson_EmplGUID = this.effectedPersEmplGuid;
            task.TSK_Information = this.taskInformation;
            task.TSK_Status = enumTaskStatus.open.ToString();

            //read Workflow Specific Processdata from Engine and write it to Task
            DatabaseAccess databaseAccess = new DatabaseAccess();
            WFEWorkflowInstance instance = databaseAccess.GetWorkflowInstance(engineContext.WorkflowModel.InstanceID);
            WFEWorkflowDefinition wfDefinition = databaseAccess.GetWorkflowDefinition(instance.WFI_WFD_ID);

            task.TSK_ProcessGuid = instance.WFI_WFD_ID;
            task.TSK_ProcessName = wfDefinition.WFD_Name;
            task.TSK_IsBulkActivity = isBulk == null ? false : isBulk.Value;
            task.TSK_TaskActivityID = engineContext.CurrenActivity.Instance;

            task.TSK_Requestor_EmplGUID = this.requestingPersEmplGuid;
            task.TSK_TaskTitle = taskTitle;
            task.TSK_ToDo = toDo;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("", EnumStepState.Complete);

            try
            {
                engineContext.SetActivityVariable(VAR_HASDYNAMICFIELDSSELECTED, this.HasDynamicFieldsSelected.ToString(), true);
            }
            catch (BaseException bEx)
            {
                result.StepState = EnumStepState.ErrorToHandle;
                return this.logErrorAndReturnStepState(engineContext, bEx, "Finish(): " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                result.StepState = EnumStepState.ErrorToHandle;
                return this.logErrorAndReturnStepState(engineContext, ex, "Finish(): Exception", EnumStepState.ErrorToHandle);
            }
            return result;
        }

        #region Validate
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
