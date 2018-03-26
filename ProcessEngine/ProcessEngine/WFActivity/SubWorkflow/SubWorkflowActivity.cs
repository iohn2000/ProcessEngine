using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Kapsch.IS.ProcessEngine.WFActivity.SubWorkflow
{
    public class SubWorkflowActivity : BaseActivity, IProcessStep, IActivityValidator
    {

        public SubWorkflowActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn Initialize(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("", EnumStepState.Complete);
            ret = this.doRun(engineContext);
            return ret;
        }

        private StepReturn doRun(EngineContext engineContext)
        {
            // error handling 
            try
            {

                // check if this subWf activity is called first time (--> cretae subWF) or 
                // when the subwf is finished (--> extract variables)

                Variable subWorkflowFinished = null;
                Variable subworkflowInstance = engineContext.GetWorkflowVariable("0.subWorkflowInstanceID");
                if (subworkflowInstance != null)
                {
                    subWorkflowFinished = engineContext.GetWorkflowVariable("0.subWorkflowFinished_" + subworkflowInstance.VarValue);
                }

                if (subWorkflowFinished != null && subWorkflowFinished.VarValue.ToLowerInvariant() == "true")
                {
                    // subwf finished - extract variables from it
                    this.extractVariablesFromSubworkflow(engineContext, subworkflowInstance.VarValue);
                    // subwf is finished continue
                    StepReturn ret = new StepReturn("", EnumStepState.Complete);
                    return ret;
                }
                else
                {
                    // first call - create subwf and start it
                    this.createSubworkflowAndStart(engineContext);
                    // wait here until subwf is finished
                    StepReturn ret = new StepReturn("", EnumStepState.Wait);
                    return ret;
                }

            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in SubWorkflowActivity Activity", ex);
                StepReturn ret = new StepReturn("SubWorkflow Activity Error", EnumStepState.ErrorStop);
                return ret;
            }
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("", EnumStepState.Complete);
            return ret;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("", EnumStepState.Complete);
            return ret;
        }

        private void extractVariablesFromSubworkflow(EngineContext engineContext, string subWorkflowInstanceID)
        {
            ProcessInstance subWfInstance = new ProcessInstance(subWorkflowInstanceID);
            //
            // extract configured returnVariables
            //
            List<Variable> allSubVars = (List<Variable>)subWfInstance.WorkflowModel.GetPunktVariables("");
            Variable returnVarList = this.GetProcessedActivityVariable(engineContext, "returnVariables", false); // ; separated
            if (returnVarList != null && (!string.IsNullOrWhiteSpace(returnVarList.VarValue)))
            {
                // get them
                foreach (string subWfRetVarName in returnVarList.VarValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    //find var in subworkflow
                    Variable subVar;
                    subVar = allSubVars.Find(x=>x.Name==subWfRetVarName); 
                   
                    if (subVar != null)
                    {
                        engineContext.SetActivityVariable(subVar.Name.Replace("0.", ""), subVar.VarValue, createNewIfNotExist: true);
                    }
                    else
                    {
                        string msg = "could not find variable '{0}' in subworkflow '{1}', as configured in parent workflow ({2}) subworkflow activity '{3}'";
                        msg = string.Format(msg, subWfRetVarName, subWorkflowInstanceID, engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance);
                        logger.Warn(msg);
                    }
                }
            }
            else
            {
                // no return variables configured... maybe not necessary
                string msg = "not return variables in parentworkflow '{0}' defined for subworkflow '{1}' in subworkflow activity '{2}'";
                msg = string.Format(msg, engineContext.WorkflowModel.InstanceID, subWorkflowInstanceID, engineContext.CurrenActivity.Instance);
                logger.Warn(msg);
            }

        }

        private void createSubworkflowAndStart(EngineContext engineContext)
        {
            // get id of workflowdefinition
            Variable subWfDefId = this.GetProcessedActivityVariable(engineContext, "subWorfklowDefinitionID", false);

            if (subWfDefId == null || string.IsNullOrWhiteSpace(subWfDefId.VarValue))
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "worflow variable 'subWorkflowDefinitionID' cannot be null or empy and must be set.");

            //create WF instance
            ProcessDefinition def = new ProcessDefinition();
            ProcessInstance subWfInst = def.CreateWorkflowInstance(
                workflowDefinitionID: subWfDefId.VarValue,
                workflowInstanceID: "WOIN_" + Guid.NewGuid().ToString("N"),
                parentWorkflowInstanceID: engineContext.WorkflowModel.InstanceID,
                nextActivity: engineContext.CurrenActivity.Instance);

            // this activity must contain all input properties needed for the subworkflow
            // get all variable names; type List<WfVariable> serialised

            var allProperties = (List<Variable>) engineContext.WorkflowModel.GetPunktVariables("");

            List<WorkflowMessageVariable> inputList = new List<WorkflowMessageVariable>();
            foreach (var v in allProperties)
            {
                // only take 0. variables because these are the initial input variables that workflow needs
                if (v.Direction == EnumVariableDirection.subworkflow)
                {
                    string vNameNoNumber = this.removeNumber(v.Name);
                    Variable dummy = this.GetProcessedActivityVariable(engineContext, vNameNoNumber, false);
                    if (dummy != null)
                    {
                        inputList.Add(new WorkflowMessageVariable(vNameNoNumber, dummy.VarValue));
                    }
                }
            }

            string serializedInputVariables = "";
            serializedInputVariables = XmlSerialiserHelper.SerialiseIntoXmlString(inputList);
            subWfInst.Execute(serializedInputVariables);
        }

        private string removeNumber(string name)
        {
            int firstDot = name.IndexOf(".");
            return name.Substring(firstDot + 1);
        }

        #region Validation
        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
