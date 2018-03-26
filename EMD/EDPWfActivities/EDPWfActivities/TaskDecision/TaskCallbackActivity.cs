using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class TaskCallbackActivity : BaseEDPCallbackActivity, IProcessStep
    {
        /// <summary>
        /// fieldname for Autoapproval example:
        /// </summary>
        private const string VAR_AUTOFIELD = "TaskAutoField";

        /// <summary>
        /// fieldvalue for Autoapproval
        /// </summary>
        private const string VAR_AUTOVALUE = "TaskAutoValue";

        private string ResultMessage = "";
        private bool AutoCompleted = false;

        public TaskCallbackActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn PreFinish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn(this.ResultMessage, EnumStepState.Complete);

            this.buildDynamicReturnVariables(engineContext);
            result.ReturnValue += " CallbackResult: " + this.ResultMessage;


            return result;
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            result.ReturnValue += " AutoCompleted: " + this.AutoCompleted;
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            return result;
        }

        /// <summary>
        /// this runs in context of Initialize 
        /// </summary>
        /// <param name="engineContext"></param>
        public override void UpdateStatusAndResultForAutoCallback(EngineContext engineContext)
        {
            string autoValue = "", autoField = "";
            Variable tmp = base.GetProcessedActivityVariable(engineContext, VAR_AUTOFIELD, true);
            if (tmp != null) autoField = tmp.GetStringValue();

            tmp = base.GetProcessedActivityVariable(engineContext, VAR_AUTOVALUE, true);
            if (tmp != null) autoValue = tmp.GetStringValue();

            List<Tuple<string, string>> response = new List<Tuple<string, string>>();
            response.Add(new Tuple<string, string>(autoField, autoValue));
            string taskGuiResponse = new TaskItemManager().ConvertTaskResponseToXmlString(response);

            this.AutoCompleted = true;
            this.CallbackResult = taskGuiResponse;
        }

        /// <summary>
        /// Take the task response (xml) and build workflow variables witt it.
        /// </summary>
        /// <param name="engineContext"></param>
        private void buildDynamicReturnVariables(EngineContext engineContext)
        {
            // step one foreach item create a variable mit key,value
            string currentNr = engineContext.CurrenActivity.Nr;

            XDocument x = XDocument.Parse(base.CallbackResult);
            var items = x.XPathSelectElements("/TaskGuiResponse/Items/Item");
            foreach (var i in items)
            {
                string name = i.Attribute("key").Value;
                string val = i.Value;
                engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel(
                    currentNr + ".Key_" + name,
                    val,
                    EnumVariablesDataType.stringType,
                    EnumVariableDirection.both);

                this.ResultMessage += " " + name + ": " + val + "; ";
            }

        }
    }
}
