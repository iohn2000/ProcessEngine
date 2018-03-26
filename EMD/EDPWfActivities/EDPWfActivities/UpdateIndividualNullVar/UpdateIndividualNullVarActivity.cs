using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.UpdateIndividualNullVar
{
    public class UpdateIndividualNullVarActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {

        public UpdateIndividualNullVarActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn Finish(EngineContext engineContext)

        {            
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);
            return ret;
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            return this.doRun(engineContext);
        }

        private StepReturn doRun(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                Variable nullName = base.GetProcessedActivityVariable(engineContext, "nullVariableName");
                Variable nullValue = base.GetProcessedActivityVariable(engineContext, "nullVariableValue");
                if (nullName != null & nullValue != null)
                {
                    engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel(
                        nullName.VarValue,
                        nullValue.VarValue,
                        EnumVariablesDataType.stringType,
                        EnumVariableDirection.both);

                    result.ReturnValue = "upddate Variable: " + nullName.VarValue + " with value " + nullValue.VarValue; 
                }
                else
                {
                    result = new StepReturn("cannot read activity properties", EnumStepState.ErrorToHandle);
                }

            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext,bEx,bEx.Message,EnumStepState.ErrorToHandle);
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
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);
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
