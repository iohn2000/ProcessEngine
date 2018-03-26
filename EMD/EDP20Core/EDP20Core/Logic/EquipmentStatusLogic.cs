using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class PackageEquipmentStatus : ProcessStatusLogic
    {
        public const int STATUSITEM_NOTSET = 0;
        public const int STATUSITEM_PACKAGED = 10;
        public const int STATUSITEM_EMPLOYMENTPACKAGE = 11;
        public const int STATUSITEM_REMOVEDFROMPACKAGE = 20;
        public const int STATUSITEM_ELIGIBLEFOR = 30;
        public const int STATUSITEM_FROMUNLINKEDPACKAGE = 40;
        public const int STATUSITEM_FROMFORMERPACKAGE = 50;
        public PackageEquipmentStatus()
        {
            base.statusItems.Add(new StatusItem(STATUSITEM_NOTSET, "NotSet", "Not Set"));
            base.statusItems.Add(new StatusItem(STATUSITEM_PACKAGED, "Packaged", "EQ and Package linked"));
            base.statusItems.Add(new StatusItem(STATUSITEM_EMPLOYMENTPACKAGE, "EmploymentPackage", "EQ in Employment Package"));
            base.statusItems.Add(new StatusItem(STATUSITEM_REMOVEDFROMPACKAGE, "RemovedFromPackage", "Has EQ but no longer in package"));
            base.statusItems.Add(new StatusItem(STATUSITEM_ELIGIBLEFOR, "EligibleFor", "Has not EQ but still package"));
            base.statusItems.Add(new StatusItem(STATUSITEM_FROMUNLINKEDPACKAGE, "FromUnlinkedPackage", "Has EQ but no longer linked package"));
            base.statusItems.Add(new StatusItem(STATUSITEM_FROMFORMERPACKAGE, "FromFormerPackage", "Has EQ but no package deleted"));
        }
    }


    public class ObjectRelationStatus : ProcessStatus
    {
    }

    public class EquipmentStatus : ObjectRelationStatus
    {

        public EquipmentStatus()
        {
            //statusItems.Add(new StatusItem(STATUSITEM_NOTSET, "NotSet", "Not Set"));
            //statusItems.Add(new StatusItem(STATUSITEM_ACTIVE, "Active", "Attached to the employment"));


            //statusItems.Add(new StatusItem(STATUSITEM_ORDERED, "Ordered", "Not Queued by Queue Manager"));
            //statusItems.Add(new StatusItem(STATUSITEM_QUEUED, "Queued", "Queued by QueueManager"));
            //statusItems.Add(new StatusItem(STATUSITEM_INPROGRESS, "InProgress", "In Progress by Workflow"));
            //statusItems.Add(new StatusItem(STATUSITEM_TIMEOUT, "TimeOut", "Timed out process"));
            //statusItems.Add(new StatusItem(STATUSITEM_DECLINED, "Declined", "Declined by approver"));

            //statusItems.Add(new StatusItem(STATUSITEM_REMOVED, "Removed", "Removed from employment"));
            statusItems.Add(new StatusItem(STATUSITEM_REMOVED_GIFTEDTOEMPLOYMENT, "Gifted", "Gifted to employment"));
        }

        /// <summary>
        /// This is a special remove case, which shows that an item was removed, but no further steps are necessary
        /// because the employment got the item as a gift
        /// </summary>
        public const int STATUSITEM_REMOVED_GIFTEDTOEMPLOYMENT = 71;
    }
    public class PackageStatus : ObjectRelationStatus
    {
    }



}
