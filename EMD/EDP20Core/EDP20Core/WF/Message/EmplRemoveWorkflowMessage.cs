using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System;

namespace Kapsch.IS.EDP.Core.WF.Message
{
    public class EmplRemoveWorkflowMessage : WorkflowBaseMessage
    {
        public string RequestingPersonEmploymentGuid { get; set; }
        public string EffectedPersonEmploymentGuid { get; set; }

        /// <summary>
        /// The target date in IsoString Format includes also the timezone
        /// </summary>
        public string ExitDateIso8601 { get; set; }

        private DateTime exitDate;

        public DateTime ExitDate
        {
            get
            {
                return exitDate;
            }
            set
            {
                exitDate = value;


                this.ExitDateIso8601 = DateTimeHelper.DateTimeToIso8601(exitDate);

            }
        }

        /// <summary>
        /// The last day in IsoString Format includes also the timezone
        /// </summary>
        public string LastDayIso8601 { get; set; }

        private DateTime lastDay;

        public DateTime LastDay
        {
            get
            {
                return lastDay;
            }
            set
            {
                lastDay = value;
                this.LastDayIso8601 = DateTimeHelper.DateTimeToIso8601(lastDay);

            }
        }

        public string ResourceNumber { get; set; }

        /// <summary>
        /// (xml string) contains list of all equipments to be changed for selected employment
        /// </summary>
        public string RemoveEquipmentInfos { get; set; }

        public EmplRemoveWorkflowMessage()
        {
            this.Prefix = "EMPL";
            this.Method = WorkflowAction.Remove;
        }

        internal override EMDProcessEntity CreateProcessEntity(string woinGuid, string wodeGuid, string wodeName, string guid_modifiedBy, string modifyComment = null)
        {
            IProcessEntityManager iProcessEntityManager = Manager.ProcessEntityManager;

            iProcessEntityManager.Guid_ModifiedBy = guid_modifiedBy;
            iProcessEntityManager.ModifyComment = string.IsNullOrWhiteSpace(modifyComment) ? "initial created" : modifyComment;
            EMDPerson effectedPerson = new PersonManager().GetPersonByEmployment(EffectedPersonEmploymentGuid);
            if (effectedPerson == null)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No effected person found");
            }
            return iProcessEntityManager.Create(woinGuid, EffectedPersonEmploymentGuid, wodeGuid, wodeName, RequestingPersonEmploymentGuid, effectedPerson.Guid, TargetDate);
        }


    }
}
