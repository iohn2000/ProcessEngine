using Kapsch.IS.EDP.Core.WF.Message;
using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDProcessMapping : EMDObject<EMDProcessMapping>
    {
        public string TypePrefix { get; set; }
        public string EntityGuid { get; set; }
        public string Method { get; set; }

        public WorkflowAction MethodEnum
        {
            get
            {
                return (WorkflowAction)Enum.Parse(typeof(WorkflowAction), Method, true);
            }
            set
            {
                Method = value.ToString();
            }
        }


        public string WorkflowID { get; set; }
        public string WorkflowVariables { get; set; }

        public override String Prefix
        {
            get
            {
                return "PRMA";
            }
        }

        public EMDProcessMapping(string guid, DateTime created, DateTime? modified) :
           base(guid, created, modified)
        { }

        public EMDProcessMapping()
        { }
    }
}