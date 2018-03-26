using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using System.Xml.Linq;
using System.Xml.XPath;
using Kapsch.IS.EDP.Core.Entities.EquipmentDef;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class TaskItemModel : BaseModel
    {
        [Display(Name = "Guid")]
        public string Guid { get; set; }
        [Display(Name = "Workflow")]
        public string TSK_ProcessName { get; set; }
        [Display(Name = "Taskinfo")]

        public string TSK_ProcessGuid { get; set; }
        public string TSK_TaskActivityID { get; set; }
        public bool TSK_IsBulkActivity { get; set; }

        [Display(Name = "Title")]
        public string TSK_TaskTitle { get; set; }
        public string TSK_EffectedPerson_EmplGUID { get; set; }
        public string TSK_Requestor_EmplGUID { get; set; }
        public string TSK_Approver_EmplGUID { get; set; }
        [Display(Name = "Task")]
        public string TSK_ToDo { get; set; }
        public string TSK_DecisionOptions { get; set; }
        [Display(Name = "Information")]
        public string TSK_Information { get; set; }
        [Display(Name = "Note")]
        public string TSK_Notes { get; set; }


        public string TempTypeId { get; set; }

        [Display(Name = "WorkflowTask Type")]
        public string TempTypeDisplayName { get; internal set; }

        public Nullable<System.DateTime> TSK_Duedate { get; set; }
        [Display(Name = "Requested")]
        public Nullable<System.DateTime> TSK_DateRequested { get; set; }
        [Display(Name = "Requested")]
        public Nullable<DateTime> TSK_DateRequestedDateOnly
        {
            get { return this.TSK_DateRequested?.Date; }
        }
        [Display(Name = "Decisiontest")]
        public string TSK_Decision { get; set; }

        public List<KeyValuePair<string, string>> DecisionResults { get; set; }


        public void SetApprovalResults()
        {
            DecisionResults = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(TSK_Decision))
            {
                System.Xml.Linq.XDocument xDocument = XDocument.Parse(TSK_Decision);
                XElement approvalDecision = xDocument.XPathSelectElement("//Item[@key='approvalDecision']");
                if (approvalDecision != null)
                {
                    ApprovalDecision = approvalDecision.Value;
                }

                XDocument taskRequestDocument = XDocument.Parse(TSK_DecisionOptions);
                List<XElement> itemList = taskRequestDocument.XPathSelectElements("//Field").ToList();

                foreach (XElement item in itemList)
                {
                    string itemId = item.Attribute("id").Value;
                    string itemName = item.Attribute("id").Value;

                    if (itemName.Equals("approvalDecision", StringComparison.InvariantCultureIgnoreCase))
                    {
                        itemName = "Approval Decision";
                    }
                    else if (itemName.Equals("approvalComment", StringComparison.InvariantCultureIgnoreCase))
                    {
                        itemName = "Approval Comment";
                    }
                    else if (itemName.Equals("EqDyn_ComputerName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        itemName = "Computer Name";
                    }
                    else if (itemName.Equals("EqDyn_EmailAddress", StringComparison.InvariantCultureIgnoreCase))
                    {
                        itemName = "E-Mail";
                    }
                    else
                    {
                        itemName = itemName.Replace(DynamicField.CONST_DYNPREFIX, string.Empty);
                    }

                    XElement itemValueElement = xDocument.XPathSelectElement(string.Format("//Item[@key='{0}']", itemId));

                    if (itemValueElement != null)
                    {
                        DecisionResults.Add(new KeyValuePair<string, string>(itemName, itemValueElement.Value));
                    }
                }
            }
        }



        public string ApprovalDecision
        {
            get; private set;
        }




        public string TSK_LinkedTasks_ID { get; set; }
        public Nullable<System.DateTime> TSK_DateNextReminder { get; set; }

        [Display(Name = "Status")]
        public string TSK_Status { get; set; }

        [Display(Name = "Status")]
        public string StatusString
        {
            get
            {
                string statusString = "open";

                if (!string.IsNullOrEmpty(TSK_Status))
                {
                    if (TSK_Status.ToLower() == System.Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.closed).ToLower())
                    {
                        statusString = "closed";
                    }
                    if (TSK_Status.ToLower() == System.Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.Saved).ToLower())
                    {
                        statusString = "saved";
                    }
                    if (TSK_Status.ToLower() == System.Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.Reminded).ToLower())
                    {
                        statusString = "reminded";
                    }
                }

                return statusString;
            }
        }

        public string AdditionalStatusClassMaterialIcon
        {
            get
            {
                string classString = "open";

                if (!string.IsNullOrEmpty(TSK_Status))
                {
                    if (TSK_Status.ToLower() == System.Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.closed).ToLower())
                    {
                        classString = "check";
                    }
                    if (TSK_Status.ToLower() == System.Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.Saved).ToLower())
                    {
                        classString = "save";
                    }
                    if (TSK_Status.ToLower() == System.Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.Reminded).ToLower())
                    {
                        classString = "watch_later";
                    }
                }

                return classString;
            }
        }

        public bool IsClosed
        {
            get
            {
                bool isClosed = false;

                if (!string.IsNullOrEmpty(TSK_Status))
                {
                    if (TSK_Status.ToLower() == System.Enum.GetName(typeof(enumTaskStatus), enumTaskStatus.closed).ToLower())
                    {
                        isClosed = true;
                    }
                }
                return isClosed;
            }
        }

        [Display(Name = "Bulk")]
        public bool IsBulkTask
        {
            get
            {
                return TSK_IsBulkActivity;
                //return TSK_DecisionOptions?.ToLower().IndexOf("type=\"textbox\"") == -1;
            }
        }

        public string HtmlVisibilityString
        {
            get
            {
                return IsClosed ? "hidden" : "visible";
            }
        }

        public string MaterialIconViewLink
        {
            get
            {
                return IsClosed ? "pageview" : "playlist_add_check";
            }
        }

        public string ViewLinkText
        {
            get
            {
                return IsClosed ? "Task (View only)" : "Approve Task";
            }
        }


        [Display(Name = "Requestor")]
        public string Requestor_Name { get; set; }

        public string ApproverPersonGuid { get; set; }


        public string EffectedPersonPersonGuid { get; set; }

        [Display(Name = "Effected Person")]
        public string Effected_Person_Name { get; set; }

        //[Required(ErrorMessage = "Required field")]
        public List<string> DecisionList { get; set; }
        public string ApproverName { get; internal set; }
        public string Requestor_pers_guid { get; internal set; }
        public List<string> BatchItems { get; internal set; }
        public bool HasMultipleItems { get; internal set; }

        public TaskItemModel()
        {
            BatchItems = new List<string>();

        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public void InitializeSecurity(string userName, string emplGuid)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(userName);
            PersonManager persManager = new PersonManager();

            EMDPerson pers = persManager.GetPersonByEmployment(emplGuid);

            if (secUser.IsAdmin)
            {
                this.CanManage = true;
                this.CanView = true;
            }
            if (secUser.IsItSelf(pers.Guid, true))
            {
                this.CanManage = true;
                this.CanView = true;
            }
            else
            {
                this.CanView = secUser.IsAllowedEmployment(userName, emplGuid);
                if (secUser.IsAllowedEmploymentForCostcenterManager(userName, emplGuid) || secUser.IsAllowedEmploymentForAssistence(userName, emplGuid) || (secUser.IsAllowedEmploymentForEnterprise(userName, emplGuid) && secUser.hasPermission(SecurityPermission.TaskManagement_View_Manage_Approver, new SecurityUserParameterFlags(), null, emplGuid, null)))
                {
                    this.CanManage = true;
                }
            }
        }

        public static TaskItemModel copyFromDBObject(TaskItem taskItem)
        {
            TaskItemModel taskItemModel = new TaskItemModel();
            ReflectionHelper.CopyProperties(ref taskItem, ref taskItemModel);
            return taskItemModel;
        }

        public static TaskItemModel copyFromObject(EMDTaskItem taskItem)
        {
            TaskItemModel taskItemModel = new TaskItemModel();
            ReflectionHelper.CopyProperties(ref taskItem, ref taskItemModel);
            return taskItemModel;
        }

        //public bool IsAllowedObject(string userId, string emplGuid)
        //{
        //    bool allowedObject = false;
        //    SecurityUser secUser = SecurityUser.NewSecurityUser(userId);
        //    if (secUser.IsMyEmployment(userId, emplGuid))
        //        allowedObject = true;
        //    else
        //    {
        //        allowedObject = secUser.IsAllowedEmployment(userId, emplGuid);
        //        this.CanView = allowedObject;
        //        if (secUser.IsAllowedEmploymentForCostcenterManager(userId, emplGuid) || secUser.IsAllowedEmploymentForAssistence(userId, emplGuid) || (secUser.IsAllowedEmploymentForEnterprise(userId, emplGuid) && secUser.hasPermission(SecurityPermission.TaskManagement_View_Manage_Approver, new SecurityUserParameterFlags(), null, emplGuid, null)))
        //        {
        //            this.CanManage = true;
        //        }
        //        //allowedObject = allowedObject && secUser.hasPermission(SecurityPermission.TaskManagement_View_Manage_Approver,new SecurityUserParameterFlags(), null, emplGuid, null);
        //    }
        //    return allowedObject;
        //}
    }
}