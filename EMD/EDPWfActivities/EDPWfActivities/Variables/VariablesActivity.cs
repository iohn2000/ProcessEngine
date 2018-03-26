using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.Variables
{
    /// <summary><![CDATA[<variable name="12.batchVariables" direction="input" dataType="stringType">
    ///   <vars>
    ///       <var>
    ///           <name>"EMTY"</name>
    ///           <value>"Name@@ET_Guid@@{{0.EffectedPersonEmploymentGuid}}"</value>
    ///       </var>
    ///       <var>
    ///           <name>"ENTE"</name>
    ///           <value>"NameShort@@E_Guid@@ENLO_Guid@@{{0.EffectedPersonEmploymentGuid}}"</value>
    ///       </var>
    ///       <var>
    ///           <name>"EffPersFullName"</name>
    ///           <value>"FamilyName@@P_Guid@@{{0.RequestingPersonEmploymentGuid}}" + " " + "FirstName@@P_Guid@@{{0.RequestingPersonEmploymentGuid}}"</value>
    ///       </var>	
    ///   </vars>
    ///   </variable> ]]>
    /// </summary>
    public class VariablesActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        public const string VARIABLES_LIST = "variablesList";

        public VariablesActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Variables successfully created", EnumStepState.Complete);
            string currentNr = engineContext.CurrenActivity.Nr;

            try
            {
                //Variable batchVariables = base.GetProcessedActivityVariable(engineContext, VARIABLES_LIST, false);
                Variable batchVariables = engineContext.GetActivityVariable(VARIABLES_LIST);
                
                XDocument xdoc = XDocument.Parse(batchVariables.VarValue);
                var vars = xdoc.XPathSelectElements("//var");

                Dictionary<String,String> variablesList = new Dictionary<String,String>();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                logger.Debug("[SW] Start Stopwatch");

                foreach (var item in vars)
                {
                    string name = null;
                    string nameWithout = null;
                    string expre = null;

                    if (item.XPathSelectElement("name") != null)
                    {
                        name = currentNr + "." + item.XPathSelectElement("name").Value.Replace("\"", "");
                        nameWithout = item.XPathSelectElement("name").Value.Replace("\"", "");
                    }
                    else
                    {
                        string msg = string.Format("Error calculating the variable name.");
                        // return with Error2Handle
                        return logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                    }

                    if (item.XPathSelectElement("value") != null)
                    {
                        expre = item.XPathSelectElement("value").Value;
                    }
                    else
                    {
                        string msg = string.Format("Error calculating the variable name.");
                        // return with Error2Handle
                        return logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                    }

                    //
                    //  create workflow variable 
                    //
                    engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel(
                       variableName: name,
                       variableValue: expre,
                       dataType: EnumVariablesDataType.stringType,
                       direction: EnumVariableDirection.both);

                    variablesList.Add(nameWithout, expre);                    

                }

                // unten brauchen wir nicht mehr da hier nur die sortedlist aktualisiert wird die wir ja nicht haben.
                //engineContext.SetListofWorkflowVariables(engineContext.WorkflowModel.GetVariables());
                //logger.Debug("[SW] SetListofWorkflowVariables: " + sw.ElapsedMilliseconds);

                foreach (var variable in variablesList) { 
                    String name = currentNr + "." + variable.Key;
                    //
                    // calculate expression
                    //
                    Variable tmpVar = base.GetProcessedActivityVariable(engineContext, variable.Key, false);
                    //Variable tmpVar = base.get
                    //
                    // fill workflow variable
                    //
                    engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel(
                        variableName: "0." + variable.Key,
                        variableValue: tmpVar.VarValue,
                        dataType: EnumVariablesDataType.stringType,
                        direction: EnumVariableDirection.both);
                    
                    logger.Debug(string.Format("Variable created : {0} - {1}", name , tmpVar.VarValue));
                }
                logger.Debug("[SW] Activity done: " + sw.ElapsedMilliseconds);
                sw.Stop();
                sw.Reset();
            }
            catch (BaseException bEx)
            {
                result.StepState = EnumStepState.ErrorToHandle;
                result.ReturnValue = bEx.Message;
                result.DetailedDescription = bEx.ToString();
                return result;
            }
            catch (Exception ex)
            {
                string msg = "error trying to get workflowvariables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            // setup return object 
            StepReturn ret = new StepReturn("Variables successfully created", EnumStepState.Complete);
            return ret;
        }

        public override StepReturn Finish(EngineContext engineContext)

        {
            StepReturn ret = new StepReturn("Variables successfully created", EnumStepState.Complete);
            return ret;
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }
        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }
    }
}
