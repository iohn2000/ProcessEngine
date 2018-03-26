using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.WF.Message;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEmployment : EMDObject<EMDEmployment>, IProcessMapping, IEntityCheck
    {
        public string P_Guid { get; set; }

        public string ET_Guid { get; set; }
        public string DGT_Guid { get; set; }
        public int EP_ID { get; set; }

        public string ENLO_Guid { get; set; }
        public int P_ID { get; set; }

        public int ET_ID { get; set; }
        public Nullable<System.DateTime> Entry { get; set; }

        /// <summary>
        /// real legal exit date of employment
        /// </summary>
        public Nullable<System.DateTime> Exit { get; set; } //TODO make setter private

        /// <summary>
        /// real last working day of the employee >> technical
        /// (i.e. delete in systems)
        /// </summary>
        public Nullable<System.DateTime> LastDay { get; set; } //TODO make setter private
        public string PersNr { get; set; }
        public string dpwKey { get; set; }
        public Nullable<System.DateTime> Exit_Report { get; set; }
        public Nullable<int> DGT_ID { get; set; }
        public Nullable<int> Sponsor { get; set; }
        public string Sponsor_Guid { get; set; }
        public Nullable<System.DateTime> FirstWorkDay { get; set; }
        public byte Status { get; set; }
        public System.DateTime LeaveFrom { get; set; }
        public System.DateTime LeaveTo { get; set; }

        public Nullable<System.DateTime> NextValidityCheckDate { get; set; }

        public override String Prefix { get { return "EMPL"; } }

        /// <summary>
        /// returns true if LastDay
        /// </summary>
        public bool IsSystemActive
        {
            get
            {
                if (IsActive && FirstWorkDay < DateTime.Now && LastDay > DateTime.Now)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// returns true if Exit
        /// </summary>
        public bool IsLegalActive
        {
            get
            {
                if (IsActive && FirstWorkDay < DateTime.Now && Exit > DateTime.Now)
                {
                    return true;
                }
                return false;
            }
        }

        private int checkIntervalInDays;

        public int CheckIntervalInDays
        {
            get
            {
                return checkIntervalInDays;
            }

            set
            {
                checkIntervalInDays = value;
            }
        }

        private int reminderIntervalInDays;

        public int ReminderIntervalInDays
        {
            get
            {
                return reminderIntervalInDays;
            }

            set
            {
                reminderIntervalInDays = value;
            }
        }

        /// <summary>
        /// Setter for Exit or Lastday
        /// If no lastday is given it is set equal to exitday
        /// </summary>
        /// <param name="exitDay">Day of Exitday (legalactive)</param>
        /// <param name="lastDay">Day Last Workday (systemactive)</param>
        public void SetExitAndLastDay(DateTime exitDay, DateTime? lastDay)
        {
            if (exitDay != null)
            {
                this.Exit = exitDay;
                if (lastDay != null)
                {
                    this.LastDay = lastDay;
                }
                else
                {
                    this.LastDay = this.Exit;
                }
                return;
            }
            else
            {
                //do nothing
                return;
            }
        }

        public EMDEmployment(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEmployment()
        { }

        public bool HasEntities()
        {
            return false;
        }

        public override DateTime SetAsNew()
        {
            DateTime stamp = base.SetAsNew();

            this.LeaveFrom = INFINITY;
            this.LeaveTo = INFINITY;

            return stamp;
        }

        public override int FillEmptyDates(DateTime? effectDate = null)
        {
            int fillCount = base.FillEmptyDates(effectDate);

            if (this.LeaveFrom == null)
            {
                this.LeaveFrom = INFINITY;
                fillCount++;
            }

            if (this.LeaveTo == null)
            {
                this.LeaveTo = INFINITY;
                fillCount++;
            }

            return fillCount;
        }

        public List<KeyValuePair<string, string>> GetEntityList()
        {
            return new List<KeyValuePair<string, string>>();
        }

        public List<WorkflowAction> GetMappingMethods()
        {
            List<WorkflowAction> processingMethods = new List<WorkflowAction>();

            processingMethods.Add(WorkflowAction.Add);
            processingMethods.Add(WorkflowAction.Remove);
            processingMethods.Add(WorkflowAction.Change);
            processingMethods.Add(WorkflowAction.Check);

            return processingMethods;
        }

        public static DateTime GetTargetOffboardingDate(DateTime exitDate, DateTime lastDay)
        {
            DateTime result;
            result = exitDate < lastDay ? exitDate : lastDay;
            return result;
        }

        public string GetPrefix()
        {
            return Prefix;
        }

        public string GetGuid()
        {
            return Guid;
        }

        public EnumManagedByType GetManagedBy()
        {
            return EnumManagedByType.Task;
        }
    }
}
