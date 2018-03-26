using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.WF.Message
{
    public class ObreChangeWorkflowMessage : WorkflowBaseMessage, IEmploymentInfos
    {
        public string RequestingPersonEmploymentGuid { get; set; }
        public string EffectedPersonEmploymentGuid { get; set; }
        public string ObreGuid { get; set; }

        /// <summary>
        /// New EmploymentGUID for subworkflow (EnumEmploymentChangeType Enterprise and EmploymentType)
        /// </summary>
        public string NewEmploymentGuid { get; set; }

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

        public ObreChangeWorkflowMessage()
        {
            this.Prefix = "EQDE";
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
            return iProcessEntityManager.Create(woinGuid, ObreGuid, wodeGuid, wodeName, RequestingPersonEmploymentGuid, effectedPerson.Guid, TargetDate);
        }

    }
}
