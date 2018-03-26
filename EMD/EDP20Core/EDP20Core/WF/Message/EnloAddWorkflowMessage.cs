using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.WF.Message
{
    public class EnloAddWorkflowMessage : WorkflowBaseMessage
    {

        public string RequestingPersonEmploymentGuid { get; set; }
        public string EnteGuid { get; set; }
        public string LocaGuid { get; set; }

        public string EnloGuid { get; set; }

        public EnloAddWorkflowMessage()
        {
            this.Prefix = "ENLO";
            this.Method = WorkflowAction.Add;
        }

        internal override EMDProcessEntity CreateProcessEntity(string woinGuid, string wodeGuid, string wodeName, string guid_modifiedBy, string modifyComment = null)
        {
            IProcessEntityManager iProcessEntityManager = Manager.ProcessEntityManager;

            iProcessEntityManager.Guid_ModifiedBy = guid_modifiedBy;
            iProcessEntityManager.ModifyComment = string.IsNullOrWhiteSpace(modifyComment) ? "initial created" : modifyComment;

            return iProcessEntityManager.Create(woinGuid, EnloGuid, wodeGuid, wodeName, RequestingPersonEmploymentGuid, null, TargetDate);
        }


    }
}
