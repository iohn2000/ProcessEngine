using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class RemoveEquipmentInfo
    {
        #region ActionDates for workflows - must be cleaned up in workflow-XMLs - actually all dates are set to be stable in the processengine

        /// <summary>
        /// The target date in IsoString Format includes also the timezone
        /// </summary>
        private string DateOfActionIso8601 { get; set; }

        private DateTime? dateOfAction;

        public DateTime? DateOfAction
        {
            get
            {
                return dateOfAction;
            }
            set
            {
                dateOfAction = value;

                if (dateOfAction.HasValue)
                {
                    this.DateOfActionIso8601 = DateTimeHelper.DateTimeToIso8601(dateOfAction.Value);
                }

                if (this.TargetDate != this.dateOfAction)
                {
                    this.TargetDate = this.dateOfAction;
                }
            }
        }


        /// <summary>
        /// The target date in IsoString Format includes also the timezone
        /// </summary>
        private string TargetDateIso8601 { get; set; }

        private DateTime? targetDate;

        public DateTime? TargetDate
        {
            get
            {
                return targetDate;
            }
            set
            {
                targetDate = value;

                if (targetDate.HasValue)
                {
                    this.TargetDateIso8601 = DateTimeHelper.DateTimeToIso8601(targetDate.Value);
                }

                if (this.DateOfAction != this.targetDate)
                {
                    this.DateOfAction = this.targetDate;
                }
            }
        }

        #endregion

        /// <summary>
        /// Equipment definition to create an equipment from 
        /// </summary>
        public string ObreGuid { get; set; }

        public string EquipmentDefinitionGuid { get; set; }

        public bool DoKeep { get; set; }



        public RemoveEquipmentInfo()
        {
        }

        public RemoveEquipmentInfo(string eqdeGuid, bool doKeep, DateTime? targetDate)
        {
            this.DoKeep = doKeep;
            this.TargetDate = targetDate;
            this.EquipmentDefinitionGuid = eqdeGuid;
        }
    }
}
