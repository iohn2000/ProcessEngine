using Kapsch.IS.ProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using System.Xml.Linq;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.DataLayer;
using System.Data.SqlClient;
using System.IO;
using Kapsch.IS.Util.Logging;
using System.Xml.XPath;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.TemplateEngine;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class TaskDecisionActivityReminderWait : BaseEDPAsyncActivity, IActivityValidator, IProcessStep
    {
        /*
        Variable            timeOutIntervalDays;
        WFEAsyncWaitItem    waitItemWFE;
        Variable            pollingIntervalSeconds;
        EngineContext       engineContext;        
         */
        public const string VAR_effectedPersonEmplGuid = "effectedPersonEmplGuid";
        public const string VAR_reminderEmailTemplate = "reminderEmailTemplate";
        public const string VAR_reminderEmailRecipient = "reminderEmailRecipient";
        public const string VAR_timeoutAction = "timeoutAction";
        public const string VAR_defaultField = "defaultField";
        public const string VAR_defaultValue = "defaultValue";
        public const string VAR_defaultDecisionTriggered = "defaultDecisionTriggered";
        public const string VAR_reminderEmailSubject = "reminderEmailSubject";
        public const string VAR_reminderMailSender = "reminderMailSender";
        public const string VAR_reminderHasBeenSentFlag = "reminderHasBeenSentFlag";

        private enum EnumTaskOverdueActions { pause, defaultdecision };

        private DateTime? ReminderDate = null;

        private AsyncTaskRequestResult TaskRequestResult;
        private EnumTaskOverdueActions OverdueAction;
        private string ReminderEmailTemplate = "";
        private string ReminderRecipientCodedList;
        private string EffectedPersonEmplGuid;
        internal Dictionary<string, string> renderDictionary = new Dictionary<string, string>();
        private string ReminderEmailSubject;
        private string DemoRecipient;
        private string ReminderEmailSender;
        private bool ReminderHasBeenSentFlag = false;
        private string DefaultField;
        private string DefaultValue;

        public TaskDecisionActivityReminderWait() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AsyncRequestID"></param>
        /// <param name="asyncRequestResult"></param>
        /// <returns></returns>
        public override bool isResultAvailable(string AsyncRequestID, out BaseAsyncRequestResult asyncRequestResult)
        {
            bool result = false;
            string linkedTaskID = "";
            EMDTaskItem emdTaskItem = null;
            string linkedInstance = this.engineContext.CurrenActivity.WaitInstanceID;

            #region connect to database
            DatabaseAccess db = new DatabaseAccess();
            try
            {

                this.waitItemWFE = db.GetWaitItem(engineContext.WorkflowModel.InstanceID, linkedInstance);
                logger.Debug(string.Format("{0} - after db.GetWaitItem('{1}','{2}') - Result: waitItem-ID={3} ",
                    base.getWorkflowLoggingContext(engineContext), engineContext.WorkflowModel.InstanceID, linkedInstance,
                    this.waitItemWFE == null ? "null" : this.waitItemWFE.AWI_ID.ToString()
                    ));
            }
            catch (SqlException sqlEx) //https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlerror.number(v=vs.110).aspx
            {
                if (sqlEx.Number == -2 || sqlEx.Number == 2) // timeout when connecting or db server not found
                {
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in TaskDecisionActivityWait", sqlEx);
                    // just try again next polling internval
                    asyncRequestResult = new AsyncTaskRequestResult("", ProcessEngine.Shared.Enums.EnumTaskStatus.Wait);
                    return true;
                }
            }
            #endregion

            if (this.waitItemWFE != null)
            {
                //
                // check if task is overdue, but continue to check whether task is done
                //
                asyncRequestResult = new AsyncTaskRequestResult("no finished Task found => Wait", EnumTaskStatus.Wait);

                bool isOverDue = this.waitItemWFE.AWI_DueDate != null && DateTime.Now > this.waitItemWFE.AWI_DueDate.Value;
                if (isOverDue)
                {
                    asyncRequestResult = new AsyncTaskRequestResult("EDP task is overdue: " + waitItemWFE.AWI_DueDate, EnumTaskStatus.Timeout);
                    logger.Debug(
                        string.Format("{0} - EDP task is overdue (AWI_ID={1}; AWI_DueDate='{2}') ",
                            base.getWorkflowLoggingContext(engineContext),
                            this.waitItemWFE.AWI_ID,
                            this.waitItemWFE.AWI_DueDate.ToString()
                        )
                    );
                    result = true;
                }
                StringWriter sw = new StringWriter();
                ObjectDumper.Write(this.waitItemWFE, 2, sw);
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " Dumping waitItemWFE: " + Environment.NewLine + sw.ToString());
                /*
                <root>
                  <item name="newLinkedTaskID" value="472e5f818b6142f58f2ddf009a1d2950" />
                  <item name="wfUniqueUD"      value="b-Variablen_und_Entscheidung___20151216_140548" />
                  <item name="taskGUID"        value ="asljfladsjfldsajflödjföals" />
                </root>                
                */
                try
                {
                    TaskItemHandler taskHandler = new TaskItemHandler();
                    XDocument awiCfgDoc = XDocument.Parse(this.waitItemWFE.AWI_Config);
                    linkedTaskID = awiCfgDoc.XPathSelectElement("/root/item[@name='newLinkedTaskID']").Attribute("value").Value;
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + " search for tasks with linkedTaskID: " + linkedTaskID);

                    try
                    {
                        var listTasks = taskHandler.GetAllTaskItemFromLinkedID(linkedTaskID);
                        if (listTasks.Count() > 0)
                        {
                            ((AsyncTaskRequestResult)asyncRequestResult).NextReminderDate = listTasks[0].TSK_DateNextReminder;
                            ((AsyncTaskRequestResult)asyncRequestResult).OneTaskGuid = listTasks[0].Guid;
                        }
                        emdTaskItem = taskHandler.CheckForFinishedTask(linkedTaskID); // this works for tasks with 1 approver or multiple
                    }
                    catch (BaseException bEx)
                    {
                        logger.Error(base.getWorkflowLoggingContext(engineContext) + " error finding linked TaskDecisionActivity (linkedTo = '" + linkedInstance + "')");
                        asyncRequestResult = new AsyncTaskRequestResult(bEx.Message, EnumTaskStatus.ErrorToHandle);
                        ((AsyncTaskRequestResult)asyncRequestResult).DetailedMessage = bEx.ToString();
                        result = true;
                    }

                    if (emdTaskItem != null && emdTaskItem.TSK_Status == enumTaskStatus.closed.ToString())
                    {
                        logger.Debug(base.getWorkflowLoggingContext(engineContext) + " Found TaskStatus Closed for Task '" + emdTaskItem.TSK_TaskTitle + " => Completed')");
                        asyncRequestResult = new AsyncTaskRequestResult(emdTaskItem.TSK_Decision, EnumTaskStatus.Completed);
                        result = true;
                    }
                    else if (emdTaskItem != null && !isOverDue)
                    {
                        logger.Error(base.getWorkflowLoggingContext(engineContext) + " Unexpected TaskStatus: " + emdTaskItem.TSK_Status + " for Task '" + emdTaskItem.TSK_TaskTitle + "'");
                        asyncRequestResult = new AsyncTaskRequestResult(emdTaskItem.TSK_Decision, EnumTaskStatus.ErrorToHandle);
                        result = true;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " error parsing waititem for linkedTaskID", ex);
                    asyncRequestResult = new AsyncTaskRequestResult("error parsing waititem for linkedTaskID", EnumTaskStatus.ErrorToHandle);
                    ((AsyncTaskRequestResult)asyncRequestResult).DetailedMessage = ex.ToString();
                    result = true;
                }
            }
            else
            {
                // waitItem not found
                logger.Error(base.getWorkflowLoggingContext(engineContext) + "waitItem not found. unique id = " + engineContext.WorkflowModel.InstanceID + "," + linkedInstance);
                asyncRequestResult = new AsyncTaskRequestResult("no waitItem found for task.", EnumTaskStatus.ErrorToHandle);
                result = true;
            }

            if (result == false)
            {
                // no result found -> meaning Task has not been completed yet
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " " + ((AsyncTaskRequestResult)asyncRequestResult).ReturnValue + "; linkedTaskId:" + linkedTaskID);
            }
            return result;
        }

        /// <summary>
        /// sends a reminder email if necessary (task not closed and reminder date is due)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="baseResult"></param>
        /// <param name="resultAvailable"></param>
        /// <returns></returns>
        public override StepReturn HandleReminder(EngineContext engineContext, BaseAsyncRequestResult baseResult, bool resultAvailable)
        {
            Variable tmp;
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                this.TaskRequestResult = (AsyncTaskRequestResult)baseResult;

                // read flag if reminder has been set; default or error case is "false"
                this.ReminderHasBeenSentFlag = false;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_reminderHasBeenSentFlag, true);
                if (!bool.TryParse(tmp.VarValue, out this.ReminderHasBeenSentFlag))
                    this.ReminderHasBeenSentFlag = false;

                if (this.ReminderHasBeenSentFlag == false)
                {
                    //
                    // no reminder ist sent if task has timeout or is completed
                    //
                    if (this.TaskRequestResult.TaskState != EnumTaskStatus.Completed &&
                        this.TaskRequestResult.TaskState != EnumTaskStatus.Timeout)
                    {
                        // check if a reminder is due
                        if (this.TaskRequestResult.NextReminderDate != null && this.TaskRequestResult.NextReminderDate.Value <= DateTime.Now)
                        {

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

                            // reminder is due
                            ITemplateEngine renderer = new NustacheRenderer();
                            WfHelper wfHelper = new WfHelper();
                            WorkflowMailer mailer = new WFActivity.WorkflowMailer(base.GetEmailSubjectPrefix());

                            // get mail template
                            tmp = base.GetProcessedActivityVariable(engineContext, VAR_reminderEmailTemplate, false);
                            this.ReminderEmailTemplate = tmp.VarValue;

                            DatabaseAccess db = new DatabaseAccess();
                            DocumentTemplate docTemplate = db.GetDocumentTemplateByName(this.ReminderEmailTemplate);

                            // get recipient
                            tmp = base.GetProcessedActivityVariable(engineContext, VAR_reminderEmailRecipient, false);
                            this.ReminderRecipientCodedList = tmp.VarValue;

                            // get eff pers empl guid
                            tmp = base.GetProcessedActivityVariable(engineContext, VAR_effectedPersonEmplGuid, false);
                            this.EffectedPersonEmplGuid = tmp.VarValue;

                            // get email subject
                            tmp = base.GetProcessedActivityVariable(engineContext, VAR_reminderEmailSubject, false);
                            this.ReminderEmailSubject = tmp.VarValue;

                            // get recipients
                            List<Tuple<string, string, string>> recipientList = wfHelper.BuildRecipientListForMailing(this.ReminderRecipientCodedList, this.EffectedPersonEmplGuid);

                            // build demo recipient
                            this.DemoRecipient = base.GetSettingWithFallbackToAppConfig(engineContext, "demoRecipient", "DefaultDemoModeRecipient", "KIBSI-EDP-admin@kapsch.net");
                            if (base.isDemoModeOn(engineContext))
                            {
                                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : DEMO Mode is ON. Recipient is now : " + DemoRecipient);
                            }

                            // sender
                            this.ReminderEmailSender = base.GetSettingWithFallbackToAppConfig(engineContext, VAR_reminderMailSender, "xxxxxxxxx", "KIBSI-EDP-NoReply@kapsch.net");

                            foreach (var rec in recipientList)
                            {
                                // item1 = mail, 2=firname, 3=familyname
                                wfHelper.AddOrUpdateDictionary(this.renderDictionary, "RecipientFamilyname", rec.Item3);
                                wfHelper.AddOrUpdateDictionary(this.renderDictionary, "RecipientFirstname", rec.Item2);
                                wfHelper.AddOrUpdateDictionary(this.renderDictionary, "RecipientMainMail", rec.Item1);
                                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : RecipientMainMail : " + rec.Item1);

                                string renderedContent = renderer.RenderTemplateFromString(docTemplate.TMPL_Content, this.renderDictionary);

                                var recpMails = base.isDemoModeOn(engineContext) ? new List<string>() { this.DemoRecipient } : new List<string>() { rec.Item1 };

                                mailer.SendEmail(this.ReminderEmailSender, recpMails, this.ReminderEmailSubject, renderedContent, true);

                                logger.Debug(string.Format("{0} : ReminderEmail-Activity : To={1} Subject={2}", base.getWorkflowLoggingContext(engineContext), rec.Item1, this.ReminderEmailSubject));
                            }
                            // set reminderemail has been sent flag to true
                            engineContext.SetActivityVariable(VAR_reminderHasBeenSentFlag, "true");
                            //
                            // set task(s) to status reminded for GUI to show (no other reason here)
                            //
                            this.SetTaskStatusToReminded();
                        }
                        else
                        {
                            // do not send a reminder, b/c its not due
                        }
                    }
                    else
                    {
                        // no reminder needed (task is completed or timeout)

                    }
                } //reminderflaghasbeenset = false
                else
                {
                    // flag reminderhasbeenSet = true -> do nothing
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in HandleReminder base function.", ex);
            }
            return result;
        }

        /// <summary>
        /// once a reminder has been sent to task responsible the status is set to reminded in order to find all task that have been reminded
        /// </summary>
        private void SetTaskStatusToReminded()
        {
            try
            {
                TaskItemHandler taskHandler = new TaskItemHandler();
                EMDTaskItem oneTask = (EMDTaskItem)taskHandler.GetObject<EMDTaskItem>(this.TaskRequestResult.OneTaskGuid);
                if (oneTask != null)
                {
                    var allTasks = taskHandler.GetAllTaskItemFromLinkedID(oneTask.TSK_LinkedTasks_ID);
                    foreach (EMDTaskItem t in allTasks)
                    {
                        t.TSK_Status = enumTaskStatus.Reminded.ToString();
                        taskHandler.UpdateObject(t);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in HandleReminder base function.", ex);
            }
        }

        public override StepReturn PostInitialize(EngineContext engineContext, BaseAsyncRequestResult baseResult)
        {
            //set StepReturn Default Value
            StepReturn result = new StepReturn("ok", EnumStepState.NotCompleted);

            //
            // read activity properties
            //
            result = this.ReadActivitProperties(result);

            //
            // step 1) get polling result
            //
            this.TaskRequestResult = (AsyncTaskRequestResult)baseResult;
            logger.Debug(string.Format("{0} : PostInitialize polling result = [ taskState={1} decision={2} ]", base.getWorkflowLoggingContext(engineContext), this.TaskRequestResult.TaskState.ToString(), TaskRequestResult.ReturnValue));

            // map Taskstates into activity states
            switch (this.TaskRequestResult.TaskState)
            {
                case ProcessEngine.Shared.Enums.EnumTaskStatus.Timeout:
                    result = this.HandleDueDateOverdueEvent(engineContext);
                    break;

                // all finished continue with workflow
                case ProcessEngine.Shared.Enums.EnumTaskStatus.Completed:
                    result = new StepReturn("EDP Task was completed!", EnumStepState.Complete);
                    break;

                // error that causes workflow to stop in error state
                case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorStop:
                    result = new StepReturn(TaskRequestResult.ReturnValue, EnumStepState.ErrorStop);
                    result.DetailedDescription = TaskRequestResult.DetailedMessage;
                    break;

                // general error pause workflow but offer manual error correction
                case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorToHandle:
                    result = new StepReturn(TaskRequestResult.ReturnValue, EnumStepState.ErrorToHandle);
                    result.DetailedDescription = TaskRequestResult.DetailedMessage;
                    break;

                default:
                    //base.updateEngineAlertLastPollingToNow(engineContext);
                    string detailedMsg = string.Format("initialize(): unexpected enumTaskStatus! " + TaskRequestResult.TaskState + "{ 0} : updated Engine Alert.", base.getWorkflowLoggingContext(engineContext));
                    logger.Warn(detailedMsg);
                    result = new StepReturn("unexpected enumTaskStatus " + TaskRequestResult.TaskState, EnumStepState.ErrorToHandle);
                    result.DetailedDescription = detailedMsg;
                    break;
            }
            logger.Debug(string.Format("{0} : PostInitialize results with: " + result.ReturnValue + " - " + result.StepState, base.getWorkflowLoggingContext(engineContext)));
            return result;
        }

        /// <summary>
        /// task is overdue (AWI_Duedate), duedate property
        /// Handle is task is overdue.
        ///   -) if a default action is defined, do it and continue as if task has been finished
        ///   -) if no default action send workflow into pause mode (as done so far)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        private StepReturn HandleDueDateOverdueEvent(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            switch (this.OverdueAction)
            {

                case EnumTaskOverdueActions.defaultdecision:
                    // finish task with default decision
                    this.ApplyDefaultDecisionToTask();
                    result = new StepReturn("Autodecision applied", EnumStepState.Complete);
                    break;
                case EnumTaskOverdueActions.pause:
                default:
                    base.finishEngineAlert(engineContext);
                    logger.Debug(string.Format("{0} : finished Engine Alert because of AWI_Duedate overdue.", base.getWorkflowLoggingContext(engineContext)));
                    result = new StepReturn("EDP Task has timed out!", EnumStepState.Paused);
                    break;
            }
            return result;
        }

        /// <summary>
        /// apply default decision.
        /// if multiple tasks are linked, only one is needed.
        /// </summary>
        /// <param name="theTask"></param>
        private void ApplyDefaultDecisionToTask()
        {
            TaskItemHandler taitH = new TaskItemHandler();
            List<string> taskGuids = new List<string>();

            //
            // build the auto response according to config
            //
            List<Tuple<string, string>> response = new List<Tuple<string, string>>();
            response.Add(new Tuple<string, string>(this.DefaultField, this.DefaultValue));
            taskGuids.Add(this.TaskRequestResult.OneTaskGuid);
            taitH.FinishTasks(taskGuids, response, "default decision applied by process engine", enumTaskStatus.closed);

            // fill AsyncTaskRequestResult so that activity can handle ProcessComplete later
            XDocument x = new XDocument();
            XElement root = new XElement("TaskGuiResponse");
            XElement items = new XElement("Items");
            XElement colItem = new XElement("Item");
            colItem.SetAttributeValue("key", this.DefaultField);
            colItem.Value = this.DefaultValue;
            items.Add(colItem);
            root.Add(items);
            x.Add(root);
            this.TaskRequestResult.ReturnValue = x.ToString(SaveOptions.None);
            this.TaskRequestResult.TaskState = EnumTaskStatus.Completed;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn retStep = new StepReturn("ok", EnumStepState.Complete);
            return retStep;
        }


        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                switch (this.TaskRequestResult.TaskState)
                {
                    // all finished continue with workflow
                    case ProcessEngine.Shared.Enums.EnumTaskStatus.Completed:
                        result = this.ProcessCompletedStatus(engineContext, this.TaskRequestResult);
                        break;

                    // error that causes workflow to stop in error state
                    case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorStop:
                        result = new StepReturn("", EnumStepState.ErrorStop);
                        result.DetailedDescription = this.TaskRequestResult.DetailedMessage;
                        break;

                    // general error pause workflow but offer manual error correction
                    case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorToHandle:
                        result = new StepReturn("", EnumStepState.ErrorToHandle);
                        result.DetailedDescription = this.TaskRequestResult.DetailedMessage;
                        break;

                    default:
                        result = new StepReturn("unexpected state for TaskwaitActivity finish" + this.TaskRequestResult.TaskState.ToString(), EnumStepState.ErrorStop);
                        result.DetailedDescription = this.TaskRequestResult.DetailedMessage;
                        break;
                }
            }

            catch (Exception ex)
            {
                return base.logErrorAndReturnStepState(engineContext, ex, "Error in TaskDecisionActivityWait.Finish", EnumStepState.ErrorToHandle);
            }

            return result;
        }



        private StepReturn ReadActivitProperties(StepReturn result)
        {

            /*
                    public const string VAR_defaultDecisionTriggered = "defaultDecisionTriggered";
             */
            try
            {
                Variable tmp;

                // pause or defaultDecision action
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_timeoutAction, true);
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " defaultDecision: '" + tmp.VarValue + "'");

                if (tmp == null || string.IsNullOrWhiteSpace(tmp.VarValue))
                    this.OverdueAction = EnumTaskOverdueActions.pause;
                else
                {
                    bool isDefined = Enum.IsDefined(typeof(EnumTaskOverdueActions), tmp.VarValue.ToLower());
                    if (isDefined)
                        this.OverdueAction = (EnumTaskOverdueActions)Enum.Parse(
                            enumType: typeof(EnumTaskOverdueActions), 
                            value: tmp.VarValue.ToLower(), 
                            ignoreCase: true);
                    else
                        this.OverdueAction = EnumTaskOverdueActions.pause;
                }
                // default field and value
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_defaultField, true);
                this.DefaultField = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_defaultValue, true);
                this.DefaultValue = tmp.VarValue;


            }

            catch (BaseException bEx)
            {
                string msg = "error trying to get Workflow-variables";
                result = this.logErrorAndReturnStepState(engineContext, bEx, msg + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get Workflow-variables";
                result = this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        private StepReturn ProcessCompletedStatus(EngineContext engineContext, AsyncTaskRequestResult retTask)
        {
            StepReturn retStep;
            try
            {
                base.finishEngineAlert(engineContext);
                logger.Debug(string.Format("{0} : finished Engine Alert.", base.getWorkflowLoggingContext(engineContext)));
                base.finishWaitItem(this.waitItemWFE);
                logger.Debug(string.Format("{0} : finished Wait Item.", base.getWorkflowLoggingContext(engineContext)));
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in TaskDecisionActivityWait when finishing engine alert and wait item.", ex);
                retStep = new StepReturn("", EnumStepState.ErrorToHandle);
                return retStep;
            }

            try
            {
                this.buildDynamicReturnVariables(retTask);
                engineContext.SetActivityVariable("returnStatus", EnumStepState.Complete.ToString(), true);
                retStep = new StepReturn("", EnumStepState.Complete);
                return retStep;
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in TaskDecisionActivityWait when writing workflow variables.", ex);
                retStep = new StepReturn("", EnumStepState.ErrorToHandle);
                return retStep;
            }
        }

        private void buildDynamicReturnVariables(AsyncTaskRequestResult ret)
        {
            // step one foreach item create a variable mit key,value
            string currentNr = engineContext.CurrenActivity.Nr;

            XDocument x = XDocument.Parse(ret.ReturnValue);
            var items = x.XPathSelectElements("/TaskGuiResponse/Items/Item");
            foreach (var i in items)
            {
                string name = i.Attribute("key").Value;
                string val = i.Value;
                engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel(
                    currentNr + ".Key_" + name,
                    val,
                    EnumVariablesDataType.stringType,
                    EnumVariableDirection.both);
            }

        }

        #region validation
        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}