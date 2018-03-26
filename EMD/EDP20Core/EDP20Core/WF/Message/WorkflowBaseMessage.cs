using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Xml.Linq;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;

namespace Kapsch.IS.EDP.Core.WF.Message
{
    public abstract class WorkflowBaseMessage
    {
        public string Prefix;
        public WorkflowAction Method;

        public EnumEmploymentChangeType ChangeType { get; set; }

        /// <summary>
        /// Which kind of Businesscase was choosen in the UI
        /// </summary>
        public EnumBusinessCase BusinessCase { get; set; }

        #region ActionDates for workflows - must be cleaned up in workflow-XMLs - actually all dates are set to be stable in the processengine

        /// <summary>
        /// The DateOfAction in IsoString Format includes also the timezone
        /// </summary>
        public string DateOfActionIso8601 { get; set; }

        private DateTime dateOfAction;

        public DateTime DateOfAction
        {
            get
            {
                return this.dateOfAction;
            }
            set
            {
                this.dateOfAction = value;
                this.DateOfActionIso8601 = DateTimeHelper.DateTimeToIso8601(dateOfAction);

                // set also the targetDate, so it doesn't mind which Date is taken in the activities
                if (this.TargetDate != this.dateOfAction)
                {
                    this.TargetDate = value;
                }
            }
        }

        /// <summary>
        /// The TargetDate in IsoString Format includes also the timezone
        /// </summary>
        public string TargetDateIso8601 { get; set; }

        private DateTime targetDate;

        public DateTime TargetDate
        {
            get
            {
                return targetDate;
            }
            set
            {
                this.targetDate = value;
                this.TargetDateIso8601 = DateTimeHelper.DateTimeToIso8601(targetDate);

                // set also the targetDate, so it doesn't mind which Date is taken in the activities
                if (this.DateOfAction != this.targetDate)
                {
                    this.DateOfAction = this.targetDate;
                }
            }
        }

        #endregion


        // TODO wollerc: set ID
        public string WFDefID { get; set; }

        public XElement additionalXmlData { get; set; }

        /// <summary>
        /// returns workflow input variables as xml
        /// </summary>
        /// <param name="typeName">name of the datahelper class for workflow. e.g. EmploymentAddWorkflowVariables or EnterpriseLocationAddWorkflowVariables</param>
        /// <returns></returns>
        public static string GetWorkflowVariablesAsString(string typeName)
        {
            Type type = Type.GetType(string.Format("Kapsch.IS.EDP.Core.WF.Message.{0}", typeName));
            //get instance of class with given namespace
            var instance = Activator.CreateInstance(type);

            var mi = type.GetMethod("GetWorkflowVariablesAsString");

            var result = (string)mi.Invoke(instance, null);

            return result;
        }

        public String SerializeXElement(XElement x)
        {
            if (x != null)
                return String.Concat(x.Nodes());
            else
                return null;
        }



        internal abstract EMDProcessEntity CreateProcessEntity(string woinGuid, string wodeGuid, string wodeName, string guid_modifiedBy, string modifyComment = null);

        public EMDProcessEntity CreateWorkflowInstance(string guid_modifiedBy, string modifyComment = null)
        {
            WorkflowMessageDataItem dataItem = WorkflowHelper.GetWorkflowMessageData(this);
            WorkflowInstanceItem instanceItem = new ProcessServiceClient().CreateWorkflowInstance(dataItem);
            return CreateProcessEntity(instanceItem.InstanceID, instanceItem.DefinitionID, instanceItem.Name, guid_modifiedBy, modifyComment);
        }

        /// <summary>
        /// create workflow instance BUT add additional 0.Variables (going into EngineAlerts.EA_InputParameters)
        /// </summary>
        /// <param name="additionalListOfWorkflowVariables"></param>
        /// <param name="guid_modifiedBy"></param>
        /// <param name="modifyComment"></param>
        /// <returns></returns>
        public EMDProcessEntity CreateWorkflowInstance(Dictionary<string,string> additionalListOfWorkflowVariables, string guid_modifiedBy, string modifyComment = null)
        {

            WorkflowMessageDataItem dataItem = WorkflowHelper.GetWorkflowMessageData(this);


            dataItem.WorkflowVariables = WorkflowHelper.MergeWorkflowVariableLists(dataItem.WorkflowVariables, additionalListOfWorkflowVariables);
            

            WorkflowInstanceItem instanceItem = new ProcessServiceClient().CreateWorkflowInstance(dataItem);
            return CreateProcessEntity(instanceItem.InstanceID, instanceItem.DefinitionID, instanceItem.Name, guid_modifiedBy, modifyComment);
        }


    }
}
