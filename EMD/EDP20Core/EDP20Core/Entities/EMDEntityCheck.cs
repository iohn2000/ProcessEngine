using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEntityCheck : EMDObject<EMDEntityCheck>
    {
        public EMDEntityCheck(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEntityCheck()
        { }

        /// <summary>
        /// Prefix of the Entity-GUID: "PREN".
        /// </summary>
        public override String Prefix { get { return "ENCH"; } }

        public string EntityGuid { get; set; }

        internal int ManagedBy
        {
            get; set;

        }

        public EnumManagedByType ManagedByType
        {
            get
            {
                return (EnumManagedByType)ManagedBy;
            }
            set
            {
                ManagedBy = (int)value;
            }
        }

        /// <summary>
        /// a new worklow process must be started if DateTime.Now equals NextCheckDateJob
        /// </summary>
        public DateTime NextCheckDate { get; set; }

        /// <summary>
        /// Is written when the EmdPerson was reminded on, to answer a task
        /// </summary>
        public DateTime RemindedTime { get; set; }

        public bool IsWorkflowInProgress { get; set; }
        public string TaitGuid { get; set; }
    }
}
