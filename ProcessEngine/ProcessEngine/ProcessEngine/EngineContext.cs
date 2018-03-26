using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   The class EngineContext will be used to provide a context for workflow steps. 
    ///             This is going to be passed into <see cref="IProcessStep"/> interface. </summary>
    ///
    /// <remarks>   Fleckj, 06.02.2015. </remarks>
    #endregion
    public class EngineContext
    {
        private IEDPLogger logger = EDPLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Activity CurrenActivity;
        public WorkflowModel WorkflowModel;
        public Transition CurrentTransistion;
        public EnumWorkflowInstanceStatus ExecutionStatus;
        public ExecutionIteration ExecutionIteration;
        /// <summary>
        /// unique ID per RunTime.RunEngine() call
        /// </summary>
        public Guid uniqueRunTimeID;
        public Stopwatch stopWatch;
        /// <summary>
        /// always contains the last stepreturn infos (state, msg, details)
        /// </summary>
        public StepReturn LastStepReturn = new StepReturn();
        /// <summary>
        /// name of the workflow definition 
        /// </summary>
        public string WorkflowDefinitionName { get; set; }

        private SortedList<string, Variable> wfVariables;
        private static string executionPath;
        private string instanceId;

        public EngineAlert EngineAlert { get; internal set; }


        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   The constructor sets up all the objects need to get
        ///             access to the state of the workflow. </summary>
        ///
        /// <remarks>   Fleckj, 18.03.2015. </remarks>
        ///
        /// <param name="instanceID">   Identifier for the instance. </param>
        /// <param name="wfModel">        <see cref="WorkflowModel"/> </param>
        /// <param name="execIteration">  <see cref="ExecutionIteration"/> </param>
        /// <param name="curActivity">    <see cref="Activity"/> </param>
        /// <param name="currTransition"> <see cref="Transition"/> </param>
        /// <param name="wfVariablesList">      The variable dictionary </param>
        /// <param name="executionStatus">    <see cref="enumWorkflowExecutionStatus"/> </param>
        #endregion
        public EngineContext(string instanceID, WorkflowModel wfModel, ExecutionIteration execIteration,
            Activity curActivity, Transition currTransition, SortedList<string, Variable> wfVariablesList, EnumWorkflowInstanceStatus executionStatus, Guid runtimeID)
        {
            this.WorkflowModel = wfModel;
            this.ExecutionIteration = execIteration;
            this.CurrenActivity = curActivity;
            this.CurrentTransistion = currTransition;
            this.wfVariables = wfVariablesList;
            this.instanceId = instanceID;
            this.ExecutionStatus = executionStatus;
            this.uniqueRunTimeID = runtimeID;
            this.stopWatch = new Stopwatch();
            if (string.IsNullOrWhiteSpace(EngineContext.executionPath))
            {
                EngineContext.executionPath = Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
            }
        }

        /// <summary>
        ///  Gets a variable from the xml section 'variables'. Returns the variable or null if it does not exist. 
        ///  This is not the same as GetPropertyValue!
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public Variable GetActivityVariable(string variableName)
        {
            // build the unique name that is used for variables e.b. 1.requestor
            string uniquename = this.CurrenActivity.Nr + "." + variableName;
            return GetWorkflowVariable(uniquename);
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets workflow variable. Here you need to know the full unique name of variable (0.irgendwas) </summary>
        ///
        /// <param name="variableName"> Name of the variable. 4.xxx</param>
        ///
        /// <returns>   The workflow variable or null if variable doesn not exist. </returns>
        #endregion
        public Variable GetWorkflowVariable(string variableName)
        {
            Variable result = null;
            string ctx = "";
            ctx = DataHelper.BuildLogContextPrefix(this.uniqueRunTimeID.ToString(), this.WorkflowDefinitionName, this.WorkflowModel.InstanceID, this.CurrenActivity.Instance);

            //logger.Debug(string.Format("{0} GetWorkflowVariable('{1}')", ctx, variableName));
            
            //XElement newVariable = this.WorkflowModel.wfModel.XPathSelectElement("/workflow/variables/variable[lower-case(@name)='" + variableName.ToLower() + "']");
            XElement newVariable = this.WorkflowModel.wfModel.XPathSelectElement("/workflow/variables/variable[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + variableName.ToLower() + "']");

            if (newVariable != null)
            {
                string varValue;
                string uniqueName;
                EnumVariableDirection d;
                EnumVariablesDataType t;
                this.WorkflowModel.ExtraktVariableProperties(newVariable, out varValue, out uniqueName, out d, out t);
                result = new ProcessEngine.Variable(uniqueName, varValue, t, d);
            }

            return result;
        }

        [Obsolete("do not use", true)]
        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets processed activity property from CurrentActivity (other activities not allowed).
        ///             Throws Exception if 
        ///             Processed means : variables, e.g. {{example}}, are replaced with values from list of variables section (wfVariables).
        ///             This does not evaluate the resulting expression (if it is an expression) 
        ///             </summary>
        /// <param name="variableName"> Name of the property. </param>
        ///
        /// <returns>   The processed activity property. </returns>
        #endregion
        public Variable GetProcessedActivityVariable(string propertyName)
        {
            string ctx = "";
            ctx = DataHelper.BuildLogContextPrefix(this.uniqueRunTimeID.ToString(), this.WorkflowDefinitionName, this.WorkflowModel.InstanceID, this.CurrenActivity.Instance);

            string uniquePropertyName = this.CurrenActivity.Nr + "." + propertyName;
            string unprocessedProp; 

            string dataType; 
            EnumVariableDirection varDirection; 
            EnumVariablesDataType varDataType;

            
            //string xpathQuery = "/workflow/variables/variable[lower-case(@name)='" + uniquePropertyName.ToLower() + "']";
            string xpathQuery = "/workflow/variables/variable[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + uniquePropertyName.ToLower() + "']";
            XElement xx = this.WorkflowModel.wfModel.XPathSelectElement(xpathQuery);
            if (xx != null)
            {
                this.WorkflowModel.ExtraktVariableProperties(xx, out unprocessedProp, out uniquePropertyName, out varDirection, out varDataType);

                ExpressionEvaluator eva = new ExpressionEvaluator(this.WorkflowModel);
                object returnValue = eva.Evaluate(unprocessedProp);

                if (returnValue != null)
                {
                    Variable newV = new Variable(uniquePropertyName,
                        returnValue.ToString(),
                        varDataType,
                        varDirection
                    );
                    return newV;
                }
                else
                {
                    string msg = "{0} Cannot evaluate Variable value with Name: '{1}' and Value: '{2}'.";
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, string.Format(msg, ctx, propertyName, unprocessedProp));
                }
            }
            else
            {
                string msg = "{0} Cannot get Variable with Name: '{1}'; xpath expression was:'{2}'";
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, string.Format(msg, ctx, propertyName, xpathQuery));
            }
        }

        public static string RemoveBorderQuotes(string unprocessedProp)
        {
            if (unprocessedProp.StartsWith("\"") && unprocessedProp.EndsWith("\""))
            {
                return unprocessedProp.Substring(1, unprocessedProp.Length - 2);
            }
            else return unprocessedProp;
        }

        /// <summary>   Sets the variable property value. The current activity number is appended automatically.
        ///             This function can only set output variables that belong to the current activity.
        ///             e.g., activity 'A' can only set variables (properties) defined in activity A
        ///             e.g., emailactivity cannot set a variable from task activity
        ///             Only properties with direction='out' can be set</summary>
        ///
        /// <param name="variableName">     Name of the property. </param>
        /// <param name="variableValue">    The property value. </param>
        /// <param name="createNewIfNotExist">    in some cases (subworkflow) activity variables need to be crated dynamically </param>
        public void SetActivityVariable(string variableName, string variableValue, bool createNewIfNotExist = false)
        {
            string ctx = "";
            ctx = DataHelper.BuildLogContextPrefix(this.uniqueRunTimeID.ToString(), this.WorkflowDefinitionName, this.WorkflowModel.InstanceID, this.CurrenActivity.Instance);

            string uniquename = this.CurrenActivity.Nr + "." + variableName;

            this.WorkflowModel.UpdateWorkflowVariableInXmlModel(uniquename, variableValue, EnumVariablesDataType.stringType, EnumVariableDirection.both);
            logger.Debug(string.Format("{0} New Workflow variable created. VariableName='{1}'; VarValue='{2}'", ctx, variableName, variableValue));


            //Variable updatedVariable;
            //if (this.wfVariables.TryGetValue(uniquename, out updatedVariable))
            //{
            //    // update
            //    if (updatedVariable.Direction != EnumVariableDirection.input)
            //    {
            //        //only write to "ouput" or "both" direction
            //        updatedVariable.VarValue = variableValue;
            //        logger.Debug(string.Format("{0} Workflow variable updated. VariableName='{1}'; VarValue='{2}'", ctx, variableName, variableValue));
            //    }
            //    else
            //    {
            //        logger.Error(string.Format("{0} Cannot update workflow variable because EnumVariableDirection is 'input'. VariableName='{1}'; VarValue='{2}'", ctx, variableName, variableValue));
            //    }
            //}
            //else
            //{
            //    // create new
            //    if (createNewIfNotExist)
            //    {
            //        Variable newOne = new Variable(uniquename, variableValue, EnumVariablesDataType.stringType, EnumVariableDirection.both);
            //        this.wfVariables.Add(uniquename, newOne);
            //        logger.Debug(string.Format("{0} New Workflow variable created. VariableName='{1}'; VarValue='{2}'", ctx, variableName, variableValue));
            //    }
            //    else
            //    {
            //        string msg = string.Format("{0} Trying to create new worfklow variable but createNewIfNotExist is set to false. VariableName='{1}'; VarValue='{2}'",
            //            ctx, variableName, variableValue);
            //        logger.Error(msg);
            //    }
            //}
        }

        /// <summary>
        /// update the whole list of workflow variables. extract them from workflow xml and store in SortedList 
        /// </summary>
        /// <param name="wfVariablesList"></param>
        public void SetListofWorkflowVariables(SortedList<string, Variable> wfVariablesList)
        {
            this.wfVariables = wfVariablesList;
        }
    }
}
