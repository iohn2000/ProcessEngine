using System;

namespace Kapsch.IS.EDP.Core.Entities
{

    public class EMDTaskItem : EMDObject<EMDTaskItem>
    {
        public string TSK_ProcessName { get; set; }
        public string TSK_ProcessGuid { get; set; }
        public string TSK_TaskActivityID { get; set; }
        public bool TSK_IsBulkActivity { get; set; }
        public string TSK_TaskTitle { get; set; }
        public string TSK_EffectedPerson_EmplGUID { get; set; }
        public string TSK_Requestor_EmplGUID { get; set; }
        public string TSK_Approver_EmplGUID { get; set; }
        public string TSK_ToDo { get; set; }
        public string TSK_DecisionOptions { get; set; }
        public string TSK_Information { get; set; }
        public string TSK_Notes { get; set; }
        public Nullable<System.DateTime> TSK_Duedate { get; set; }
        public Nullable<System.DateTime> TSK_DateRequested { get; set; }
        public string TSK_Decision { get; set; }
        public string TSK_LinkedTasks_ID { get; set; }
        public Nullable<System.DateTime> TSK_DateNextReminder { get; set; }
        public string TSK_Status { get; set; }

        public override String Prefix { get { return "TAIT"; } }

        public EMDTaskItem(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDTaskItem()
        { }
    }
}