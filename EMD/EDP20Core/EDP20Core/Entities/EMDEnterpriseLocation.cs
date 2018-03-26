using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.WF.Message;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEnterpriseLocation : EMDObject<EMDEnterpriseLocation>, IProcessMapping
    {
        public string E_Guid { get; set; }
        public string L_Guid { get; set; }
        /// <summary>
        /// The L_ID is the Enterprise-LocationID and is necessary for EDP-Jobs
        /// </summary>
        public Nullable<int> L_ID { get; set; }
        public Nullable<int> E_ID { get; set; }

        public string LocationString { get; set; }

        public string EnterpriseString { get; set; }


        public string DistList_int { get; set; }
        public string DistList_ext { get; set; }
        /// <summary>
        /// Process Status.. refer to EnterpriseLocationProcessStatus
        /// </summary>
        public byte Status { get; set; }

        public override String Prefix { get { return "ENLO"; } }
        
        public EMDEnterpriseLocation(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEnterpriseLocation()
        { }

        public bool HasEntities()
        {
            return false;
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


            return processingMethods;
        }
    }
}
