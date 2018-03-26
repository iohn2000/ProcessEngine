using Kapsch.IS.EDP.Core.WF.Message;
using System;
using System.Collections.Generic;

namespace Kapsch.IS.EDP.Core.Entities
{

    public class EMDUser : EMDObject<EMDUser>, IProcessMapping
    {
        public string USDO_Guid { get; set; }
        public string Username { get; set; }
        public string EMPL_Guid { get; set; }
        public byte UserType { get; set; }
        public int Status { get; set; }
        public override String Prefix { get { return "USER"; } }
        
        public EMDUser(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        {
            this.Status = (byte)EnumUserStatus.Reserverd;
        }

        public EMDUser()
        {
            this.Status = (byte)EnumUserStatus.Reserverd;
        }

        public EMDUser(string useridstring, string personGuid) : base()
        {
            this.Username = useridstring;
            this.Status = (byte)EnumUserStatus.Reserverd;
        }

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
