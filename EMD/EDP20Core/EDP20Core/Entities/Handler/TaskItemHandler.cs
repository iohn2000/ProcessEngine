using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class TaskItemHandler : EMDObjectHandler
    {
        public TaskItemHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public TaskItemHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public TaskItemHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public TaskItemHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new TaskItem().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            TaskItem tait = (TaskItem) dbObject;
            EMDTaskItem emdObject = new EMDTaskItem(tait.Guid, tait.Created, tait.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }

       

        /// <summary>
        /// task needs to be prefilled with everything except approver and linkedtasks
        /// </summary>
        /// <param name="taskItem"></param>
        /// <returns></returns>
        [Obsolete("Use TaskItemManager with same function")]
        public List<EMDTaskItem> CreateTasks(EMDTaskItem taskItem, List<Tuple<string, string>> approver_EmplGUIDs /*List<string> */)
        {
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
                EMDTaskItem newTask = (EMDTaskItem) this.CreateObject(taskItem);
                newTasks.Add(newTask);
            }

            return newTasks;
        }

        /// <summary>
        /// Set task status to closed and close also all linked tasks (parallel tasks)
        /// </summary>
        /// <param name="decision"></param>
        /// <param name="taskGUID"></param>
        /// <returns></returns>
        [Obsolete("User Manager method instead")]
        public void FinishTasks(List<string> taskGuids, List<Tuple<string, string>> response, string taskNotes, enumTaskStatus taskStatus = enumTaskStatus.closed)
        {
            foreach (string taskGuid in taskGuids)
            {
                FinishTask(taskGuid, response, taskNotes, taskStatus);
            }
        }


        /// <summary>
        /// Set task status to closed and close also all linked tasks (parallel tasks)
        /// </summary>
        /// <param name="decision"></param>
        /// <param name="taskGUID"></param>
        /// <returns></returns>
        [Obsolete("User Manager method instead")]
        public EMDTaskItem FinishTask(string taskGuid, List<Tuple<string, string>> response, string taskNotes, enumTaskStatus taskStatus = enumTaskStatus.closed)
        {
            Historical = false; //only valid ones
            EMDTaskItem finishedTaskItem = (EMDTaskItem) GetObject<EMDTaskItem>(taskGuid);

            if (finishedTaskItem != null)
            {
                List<IEMDObject<EMDTaskItem>> linkedTasks =
                                    (List<IEMDObject<EMDTaskItem>>) GetObjects<EMDTaskItem, TaskItem>("TSK_LinkedTasks_ID = \"" + finishedTaskItem.TSK_LinkedTasks_ID + "\"");
                if (linkedTasks != null && linkedTasks.Count > 0)
                {
                    foreach (EMDTaskItem item in linkedTasks)
                    {
                        // set status to closed for all tasks linked
                        item.TSK_Status = taskStatus.ToString();

                        // save dicision for task that has been finished only
                        if (item.Guid == taskGuid)
                        {
                            // transfer tuple into something serialisable
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

                            item.TSK_Decision = x.ToString(SaveOptions.None);
                        }
                        item.TSK_Notes = taskNotes;
                        UpdateObject(item, historize: true);
                    }
                }
            }

            return finishedTaskItem;

        }

        public EMDTaskItem FinishDemoTask(string decision, string taskGUID)
        {
            // make a List<Tuple<string,string>> from xml

            List<Tuple<string, string>> tuplo = new List<Tuple<string, string>>();


            /*  <TaskGuiResponse>
                   <Items>
                      <Item key="key1">value1</Item>
                   </Items>
                </TaskGuiResponse>
*/
            XDocument x = XDocument.Parse(decision);
            var items = x.XPathSelectElements("/TaskGuiResponse/Items/Item");
            foreach (var i in items)
            {
                string name = i.Attribute("key").Value;
                string val = i.Value;
                tuplo.Add(Tuple.Create(name, val));
            }

            return this.FinishTask(taskGUID, tuplo, "demo decision was used.");
        }

        /// <summary>
        /// return nulls or a list of linked task items
        /// </summary>
        /// <param name="taskLinkID"></param>
        /// <returns></returns>
        public List<EMDTaskItem> GetAllTaskItemFromLinkedID(string taskLinkID)
        {
            List<EMDTaskItem> allLinkedTasks = new List<Entities.EMDTaskItem>();

            string whereAllTasks = string.Format("TSK_LinkedTasks_ID = \"{0}\"", taskLinkID);
            try
            {
                allLinkedTasks = GetObjects<EMDTaskItem, TaskItem>(whereAllTasks, paging: null).Cast<EMDTaskItem>().ToList();
            }
            catch (Exception ex)
            {
                string innerMsg = "none";
                if (ex.InnerException != null)
                    innerMsg = ex.InnerException.Message + "\r\nStack:\r\n" + ex.InnerException.StackTrace;
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, "Error querying linked tasks. InnerException:" + ex.InnerException, ex);
            }
            return allLinkedTasks;
        }

        /// <summary>
        /// this will work for multiple approvers for a task, first approver the finish a task will count
        /// </summary>
        /// <param name="taskLinkID"></param>
        /// <returns>returns the finished task or null if task not finished</returns>
        public EMDTaskItem CheckForFinishedTask(string taskLinkID)
        {
            Historical = false; //only valid ones
            List<EMDTaskItem> allLinkedTasks = null;
            string whereAllTasks = string.Format("TSK_LinkedTasks_ID = \"{0}\"", taskLinkID);
            try
            {
                allLinkedTasks = this.GetAllTaskItemFromLinkedID(taskLinkID);
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();

            }
            if (allLinkedTasks.Count > 0)
            {
                string closedStatus = enumTaskStatus.closed.ToString();
                string where = string.Format("TSK_Decision != \"\" && TSK_Decision != null && TSK_Status = \"" + closedStatus + "\" && TSK_LinkedTasks_ID = \"{0}\"", taskLinkID);
                List<IEMDObject<EMDTaskItem>> finishedTasks = GetObjects<EMDTaskItem, TaskItem>(where, paging: null);

                if (finishedTasks == null || finishedTasks.Count == 0)
                {
                    return null; // task is still open
                }
                else
                {
                    return (EMDTaskItem) finishedTasks[0];
                }
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "no linked tasks found for: " + taskLinkID);
            }
        }
    }

    public enum enumTaskStatus
    {
        open,
        closed,
        Saved,
        Reminded
    }
}
