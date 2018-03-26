using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class ApprovalTaskDecisionActivity : TaskDecisionActivity
    {
        private bool CanTaskBeApprovedAutomatically()
        {
            bool result = false;
            //Tuple(approverEMPLGuid, approverPERSGuid)
            bool effPersIsApprover = this.ApproverEMPLGuids.Exists(a => a.Item1 == this.EffectedPersonEMPLGuid);
            bool reqPersIsApprover = this.ApproverEMPLGuids.Exists(a => a.Item1 == this.RequestingPersonEMPLGuid);

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("", EnumStepState.Complete);
            DatabaseAccess db = new DatabaseAccess();

            try
            {
                bool autoApproveTask = this.CanTaskBeApprovedAutomatically();

                // unique id for this async item
                string uniqueID = engineContext.WorkflowModel.InstanceID + "__" + engineContext.CurrenActivity.Instance;

                // CREATE TASK(S)
                TaskItemHandler taskItemHandler = new TaskItemHandler();
                List<EMDTaskItem> newTasks = null;
                newTasks = taskItemHandler.CreateTasks(Task, ApproverEMPLGuids);

                StringWriter sw = new StringWriter();
                ObjectDumper.Write(Task, 1, sw);
                logger.Debug("TaskData: " + sw.ToString());

                StringWriter sw2 = new StringWriter();
                ObjectDumper.Write(ApproverEMPLGuids, 1, sw2);
                logger.Debug("Dump ApproverEMPL Guids: " + sw2.ToString());

                if (newTasks == null)
                {
                    string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error creating tasks";
                    logger.Error(errMsg);
                    ret.ReturnValue = errMsg;
                    ret.StepState = EnumStepState.ErrorToHandle;
                    return ret;
                }

                //
                // check if approver in [ requesting person or eff. pers. ] -> auto approve
                //


                // WAIT ITEM
                // create info so that WAIT item can poll later...
                XDocument xmlWaitItemConfig = new XDocument();
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

                string strWaitItemConfig = xmlWaitItemConfig.ToString(SaveOptions.None);

                //creating waititem to be read by isResultAvailable()

                db.CreateWaitItem(engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance, strWaitItemConfig, dueDate);

                logger.Debug(string.Format("{0} : ApprovalTask : {1}  Approver : {2}", base.getWorkflowLoggingContext(engineContext), TaskTitle, strApprover_Guids));

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
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error trying to send an approver email.", ex);
                    ret.ReturnValue = base.getWorkflowLoggingContext(engineContext) + " Email Activity Error";
                    ret.StepState = EnumStepState.ErrorToHandle;
                    return ret;
                }

                return ret; // all ok
            }
            catch (Exception ex)
            {
                string errmMsg = base.getWorkflowLoggingContext(engineContext) + " : Error creating tasks";
                logger.Error(errmMsg, ex);
                ret.ReturnValue = errmMsg;
                ret.StepState = EnumStepState.ErrorToHandle;
                return ret;
            }

        }
    }
}
