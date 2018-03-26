using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class ProcessStatusLogic
    {
        internal List<StatusItem> statusItems = new List<StatusItem>();
        public StatusItem GetProcessStatusItem(int statusNumber)
        {
            StatusItem statusItem = new StatusItem();
            statusItem = this.statusItems.SingleOrDefault(p => p.StatusNumber == statusNumber);
            return statusItem;
        }

        public StatusItem GetProcessStatusItemByShortName(string shortName)
        {
            StatusItem statusItem = new StatusItem();
            statusItem = this.statusItems.SingleOrDefault(p => p.StatusShort == shortName);
            return statusItem;
        }
    }

    /// <summary>
    /// Default status class for all objecttypes wich are handled via workflow.
    /// </summary>
    public class ProcessStatus : ProcessStatusLogic
    {
        public const int STATUSITEM_ERROR = -1;
        public const int STATUSITEM_NOTSET = 0;
        public const int STATUSITEM_ACTIVE = 50;


        public const int STATUSITEM_ORDERED = 10;
        public const int STATUSITEM_QUEUED = 20;
        public const int STATUSITEM_INPROGRESS = 30;
        public const int STATUSITEM_TIMEOUT = 40;
        public const int STATUSITEM_DECLINED = 60;

        public const int STATUSITEM_REMOVED = 70;


        public ProcessStatus()
        {
            statusItems.Add(new StatusItem(STATUSITEM_NOTSET, "NotSet", "Not Set"));
            statusItems.Add(new StatusItem(STATUSITEM_ACTIVE, "Active", "Attached to the employment"));


            statusItems.Add(new StatusItem(STATUSITEM_ORDERED, "Ordered", "Not Queued by Queue Manager"));
            statusItems.Add(new StatusItem(STATUSITEM_QUEUED, "Queued", "Queued by QueueManager"));
            statusItems.Add(new StatusItem(STATUSITEM_INPROGRESS, "InProgress", "In Progress by Workflow"));
            statusItems.Add(new StatusItem(STATUSITEM_TIMEOUT, "TimeOut", "Timed out process"));
            statusItems.Add(new StatusItem(STATUSITEM_DECLINED, "Declined", "Declined by approver"));

            statusItems.Add(new StatusItem(STATUSITEM_REMOVED, "Removed", "Removed from employment"));

        }

    }

    public class StatusItem
    {
        public int StatusNumber { get; set; }
        public string StatusShort { get; set; }
        public string StatusLong { get; set; }

        public StatusItem()
        {

        }

        public StatusItem(int statusNumber, string statusShort, string statusLong)
        {
            this.StatusNumber = statusNumber;
            this.StatusShort = statusShort;
            this.StatusLong = statusLong;

        }

    }
}
