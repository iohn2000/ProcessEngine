using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.WF.Message
{
    public class EmplChangeWorkflowMessage : WorkflowBaseMessage, IEmploymentInfos
    {
        public string RequestingPersonEmploymentGuid { get; set; }
        public string EffectedPersonEmploymentGuid { get; set; }

        #region IEmploymentInfos

        public string CostcenterOldGuid { get; set; }
        public string CostcenterNewGuid { get; set; }
        public string CostCenterResponsibleOldEmplGuid { get; set; }
        public string CostCenterResponsibleNewEmplGuid { get; set; }
        public string OrgunitOldGuid { get; set; }
        public string OrgunitNewGuid { get; set; }
        public string LineManagerOldEmplGuid { get; set; }
        public string LineManagerNewEmplGuid { get; set; }
        public string TeamleaderOldEmplGuid { get; set; }
        public string TeamleaderNewEmplGuid { get; set; }
        public string AssistanceOldEmplGuid { get; set; }
        public string AssistanceNewEmplGuid { get; set; }
        public string PersNrOld { get; set; }
        public string PersNrNew { get; set; }
        public string LocationOldGuid { get; set; }
        public string LocationNewGuid { get; set; }

        #endregion


        /// <summary>
        ///  if set to true, the equipments must be moved or deleted
        ///  otherwise all equipments can be ignored
        /// </summary>
        public bool ApproveEquipments { get; set; }

        public bool HasNewEmployment
        {
            get
            {
                return !string.IsNullOrEmpty(NewEmploymentGuid);
            }
        }

        /// <summary>
        /// Create employment for EnumEmploymentChangeType Enterprise and EmploymentType
        /// </summary>
        public string NewEmploymentGuid { get; set; }


        public string GuidTargetEnterprise { get; set; }

        public string GuidCostCenter { get; set; }

        public string GuidEmploymentType { get; set; }

        public string GuidOrgUnit { get; set; }


        public string PersonalNumber { get; set; }

        /// <summary>
        /// (xml string) contains list of all equipments to be changed for selected employment
        /// </summary>
        public string EquipmentInfos { get; set; }


        #region Leave 

        /// <summary>
        /// Get Iso String from DateLeaveFrom
        /// (only getter)
        /// </summary>
        public string LeaveFromIso8601 { get; private set; }

        private DateTime? dateLeaveFrom;

        public DateTime? DateLeaveFrom
        {
            protected get
            {
                return dateLeaveFrom;
            }
            set
            {
                this.dateLeaveFrom = value;
                if (this.dateLeaveFrom.HasValue)
                {
                    this.LeaveFromIso8601 = DateTimeHelper.DateTimeToIso8601(this.dateLeaveFrom.Value);
                }
                else
                {
                    this.LeaveFromIso8601 = null;
                }
            }
        }

        /// <summary>
        /// Get Iso String from DateLeaveTo
        /// (only getter)
        /// </summary>
        public string LeaveToIso8601 { get; private set; }

        private DateTime? dateLeaveTo;

        public DateTime? DateLeaveTo
        {
            protected get
            {
                return dateLeaveTo;
            }
            set
            {
                this.dateLeaveTo = value;
                if (this.dateLeaveTo.HasValue)
                {
                    this.LeaveToIso8601 = DateTimeHelper.DateTimeToIso8601(this.dateLeaveTo.Value);
                }
                else
                {
                    this.LeaveToIso8601 = null;
                }
            }
        }


        #endregion



        /// <summary>
        /// (xml string) contains items for additional KCC enterpise settings
        /// </summary>
        public string KCCData { get; set; }
        public string EmailType { get; set; }
        public bool MoveAllRoles { get; set; }
        public string GuidSponsor { get; set; }
        public string GuidLocation { get; set; }


        public EmplChangeWorkflowMessage()
        {
            this.Prefix = "EMPL";
            this.Method = WorkflowAction.Change;
        }

        public EmplChangeWorkflowMessage(EnumEmploymentChangeType changeType)
        {
            this.ChangeType = changeType;
            this.Prefix = "EMPL";
            this.Method = WorkflowAction.Change;
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

            return iProcessEntityManager.Create(woinGuid, string.IsNullOrEmpty(NewEmploymentGuid) ? EffectedPersonEmploymentGuid : NewEmploymentGuid, wodeGuid, wodeName, RequestingPersonEmploymentGuid, effectedPerson.Guid, TargetDate);
        }


    }
}
