using Kapsch.IS.EDP.Core.WF.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Workflow
{
    public class WorkflowActionMethodModel
    {
        public WorkflowAction Method { get; set; }


        public WorkflowActionMethodModel()
        {

        }

        public WorkflowActionMethodModel(WorkflowAction method)
        {
            this.Method = method;
        }

        public string MethodName
        {
            get
            {
                return Method.ToString();
            }
            set
            {
                Method = (WorkflowAction)System.Enum.Parse(typeof(WorkflowAction), value, true);
            }
        }

        public int MethodValue
        {
            get
            {
                return (int)Method;
            }
            set
            {
                Method = (WorkflowAction)value;
            }
        }

        public static List<WorkflowActionMethodModel> GetAvailableMethods()
        {
            List<WorkflowActionMethodModel> methods = new List<WorkflowActionMethodModel>();

            foreach (WorkflowAction method in System.Enum.GetValues(typeof(WorkflowAction)).Cast<WorkflowAction>())
            {
                methods.Add(new WorkflowActionMethodModel() { Method = method });
            }

            return methods;
        }
    }
}