using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    /*
    <linkToWait instance=".activity" />
    */

    public class TaskDecisionActivityWait : BaseEDPAsyncActivity, IActivityValidator, IProcessStep
    {
        AsyncTaskRequestResult retTask;

        public TaskDecisionActivityWait() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext, BaseAsyncRequestResult baseResult)
        {
            //set StepReturn Default Value
            StepReturn result = new StepReturn("ok", EnumStepState.NotCompleted);

            //
            // step 1) get polling result
            //
            retTask = (AsyncTaskRequestResult) baseResult;

            logger.Debug(string.Format("{0} : PostInitialize polling result = [ taskState={1} decision={2} ]", base.getWorkflowLoggingContext(engineContext), retTask.TaskState.ToString(), retTask.ReturnValue));

            // map Taskstates into activity states
            switch (retTask.TaskState)
            {

                // task is overdue (AWI_Duedate)
                // do set this activity into Paused so we can configure an escalation activity
                case ProcessEngine.Shared.Enums.EnumTaskStatus.Timeout:
                    base.finishEngineAlert(engineContext);
                    logger.Debug(string.Format("{0} : finished Engine Alert because of AWI_Duedate overdue.", base.getWorkflowLoggingContext(engineContext)));
                    //
                    // do not finish wait item, we might restart the workflow out of "Pause" mode and need it then.
                    // drawback : waitItem will never be closed if nobody restarts 
                    //
                    //base.finishWaitItem(this.waitItemWFE);
                    //logger.Debug(string.Format("{0} : finished Wait Item.", base.getWorkflowLoggingContext(engineContext)));
                    //engineContext.SetActivityVariable("returnStatus", EnumStepState.Timeout.ToString().ToLowerInvariant());
                    result = new StepReturn("EDP Task has timed out!", EnumStepState.Paused);
                    break;

                // all finished continue with workflow
                case ProcessEngine.Shared.Enums.EnumTaskStatus.Completed:
                    result = new StepReturn("EDP Task was completed!", EnumStepState.Complete);
                    break;

                // error that causes workflow to stop in error state
                case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorStop:
                    result = new StepReturn(retTask.ReturnValue, EnumStepState.ErrorStop);
                    result.DetailedDescription = retTask.DetailedMessage;
                    break;

                // general error pause workflow but offer manual error correction
                case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorToHandle:
                    result = new StepReturn(retTask.ReturnValue, EnumStepState.ErrorToHandle);
                    result.DetailedDescription = retTask.DetailedMessage;
                    break;

                default:
                    //base.updateEngineAlertLastPollingToNow(engineContext);
                    string detailedMsg = string.Format("initialize(): unexpected enumTaskStatus! " + retTask.TaskState + "{ 0} : updated Engine Alert.", base.getWorkflowLoggingContext(engineContext));
                    logger.Warn(detailedMsg);
                    result = new StepReturn("unexpected enumTaskStatus " + retTask.TaskState, EnumStepState.ErrorToHandle);
                    result.DetailedDescription = detailedMsg;
                    break;
            }
            logger.Debug(string.Format("{0} : PostInitialize results with: " + result.ReturnValue + " - " + result.StepState, base.getWorkflowLoggingContext(engineContext)));
            return result;
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
                switch (retTask.TaskState)
                {
                    // all finished continue with workflow
                    case ProcessEngine.Shared.Enums.EnumTaskStatus.Completed:
                        result = this.ProcessCompletedStatus(engineContext, retTask);
                        break;

                    // error that causes workflow to stop in error state
                    case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorStop:
                        result = new StepReturn("", EnumStepState.ErrorStop);
                        result.DetailedDescription = retTask.DetailedMessage;
                        break;

                    // general error pause workflow but offer manual error correction
                    case ProcessEngine.Shared.Enums.EnumTaskStatus.ErrorToHandle:
                        result = new StepReturn("", EnumStepState.ErrorToHandle);
                        result.DetailedDescription = retTask.DetailedMessage;
                        break;

                    default:
                        result = new StepReturn("unexpected state for TaskwaitActivity finish" + retTask.TaskState.ToString(), EnumStepState.ErrorStop);
                        result.DetailedDescription = retTask.DetailedMessage;
                        break;
                }
            }

            catch (Exception ex)
            {
                return base.logErrorAndReturnStepState(engineContext, ex, "Error in TaskDecisionActivityWait.Finish", EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override bool isResultAvailable(string asyncRequestID, out BaseAsyncRequestResult asyncRequestResult)
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
                    asyncRequestResult = new AsyncTaskRequestResult("EDP task is overdue: " + waitItemWFE.AWI_DueDate, Kapsch.IS.ProcessEngine.Shared.Enums.EnumTaskStatus.Timeout);
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
                    XDocument awiCfgDoc = XDocument.Parse(this.waitItemWFE.AWI_Config);
                    linkedTaskID = awiCfgDoc.XPathSelectElement("/root/item[@name='newLinkedTaskID']").Attribute("value").Value;
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + " search for tasks with linkedTaskID: " + linkedTaskID);

                    try
                    {
                        TaskItemHandler taskHandler = new TaskItemHandler();
                        emdTaskItem = taskHandler.CheckForFinishedTask(linkedTaskID); // this works for tasks with 1 approver or multiple
                    }
                    catch (BaseException bEx)
                    {
                        logger.Error(base.getWorkflowLoggingContext(engineContext) + " error finding linked TaskDecisionActivity (linkedTo = '" + linkedInstance + "')");
                        asyncRequestResult = new AsyncTaskRequestResult(bEx.Message, EnumTaskStatus.ErrorToHandle);
                        ((AsyncTaskRequestResult) asyncRequestResult).DetailedMessage = bEx.ToString();
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
                    ((AsyncTaskRequestResult) asyncRequestResult).DetailedMessage = ex.ToString();
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
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + " " + ((AsyncTaskRequestResult) asyncRequestResult).ReturnValue + "; linkedTaskId:" + linkedTaskID);
            }
            return result;
        }

        #region privates
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
                engineContext.SetActivityVariable("returnStatus", EnumStepState.Complete.ToString());
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


        #endregion
        public override StepReturn HandleReminder(EngineContext engineContext, BaseAsyncRequestResult baseResult, bool resultAvailable)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }



        #region MyRegion
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
