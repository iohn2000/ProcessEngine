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
    public class ObreRemoveWorkflowMessage : WorkflowBaseMessage
    {
        public string RequestingPersonEmploymentGuid { get; set; }
        public string EffectedPersonEmploymentGuid { get; set; }
        public string ObreGuid { get; set; }

        public bool DoKeep { get; set; }

        public ObreRemoveWorkflowMessage()
        {
            this.Prefix = "EQDE";
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
            return iProcessEntityManager.Create(woinGuid, ObreGuid, wodeGuid, wodeName, RequestingPersonEmploymentGuid, effectedPerson.Guid, TargetDate);
        }


    }
}
