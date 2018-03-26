using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.Core.WF.Message
{
    /// <summary>
    /// workflow message for the contractor check workflow
    /// this workflow sends a task to sponsor
    /// </summary>
    public class EmplCheckWorkflowMessage : WorkflowBaseMessage
    {
        /// <summary>
        /// requesting person = EDP for this case
        /// </summary>
        public string RequestingPersonEmploymentGuid { get; set; }

        /// <summary>
        /// Which person-employment is effected >> maybe null (if it is not an employment or equipment)
        /// </summary>
        public string EffectedPersonEmploymentGuid { get; set; }

        /// <summary>
        /// The effected entity to check
        /// All entities from interface IEntityCheck are possible
        /// </summary>
        public string EffectedEntityGuid { get; set; }

        /// <summary>
        /// Guid of EntityCheck Item
        /// </summary>
        public string EntityCheckGuid { get; set; }

        /// <summary>
        /// this will be the sponsor in this case
        /// </summary>
        public string ApproverEmplGuid { get; set; }

        /// <summary>
        /// How many days until a reminder ist sent to sponsor 
        /// </summary>
        public int ReminderIntervalInDays { get; set; }

        /// <summary>
        /// Days between two Checkdates - between t1 and t2 
        /// = CheckDays + 1
        /// </summary>
        public int OverdueIntervalInDays { get; set; }

        /// <summary>
        /// Default constructor sets the prefix and the Methods
        /// </summary>
        public EmplCheckWorkflowMessage()
        {
            this.Prefix = "EMPL";
            this.Method = WorkflowAction.Check;
        }


        internal override EMDProcessEntity CreateProcessEntity(string woinGuid, string wodeGuid, string wodeName, string guid_modifiedBy, string modifyComment = null)
        {
            IProcessEntityManager iProcessEntityManager = Manager.ProcessEntityManager;

            iProcessEntityManager.Guid_ModifiedBy = guid_modifiedBy;
            iProcessEntityManager.ModifyComment = string.IsNullOrWhiteSpace(modifyComment) ? "contractor check" : modifyComment;

            string effectedPersonGuid = null;
            string prefix = EMDEmployment.GetPrefix(EffectedEntityGuid);


            switch (prefix)
            {
                case "empl":
                    EMDEmployment employment = new EmploymentManager().GetEmployment(EffectedEntityGuid);
                    if (employment != null)
                    {
                        effectedPersonGuid = employment.P_Guid;
                    }

                    break;

                default:
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "");
            }


            return iProcessEntityManager.Create(woinGuid, EffectedEntityGuid, wodeGuid, wodeName, RequestingPersonEmploymentGuid, effectedPersonGuid, TargetDate);
        }
    }
}
