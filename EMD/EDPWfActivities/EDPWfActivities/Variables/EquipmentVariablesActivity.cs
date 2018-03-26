using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.Variables
{
    public class EquipmentVariablesActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        public const string VAR_OBJECTRELATION = "ObjectRelation";
        public const string VAR_EQUIPMENTDEFINITION = "EquipmentDefinition";
        private string ObjectRelationGuid;
        private string EquipmentDefinitionGuid;
        private EMDEquipmentDefinition EquipmentDefinition;
        private EquipmentDefinitionConfig EquipmentDefinitionConfig;

        public EquipmentVariablesActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            string eqdeGuid = "";
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_OBJECTRELATION, true);
                this.ObjectRelationGuid = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_EQUIPMENTDEFINITION, true);
                this.EquipmentDefinitionGuid = tmp.VarValue;

                if (this.ObjectRelationGuid == null && this.EquipmentDefinitionGuid == null)
                {
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "one of" + VAR_OBJECTRELATION + ", " + VAR_EQUIPMENTDEFINITION + "must be set for EquipmentVariablesActivity");
                }
            }
            catch (BaseException bEx)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, bEx, msg + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            if (this.ObjectRelationGuid != null)
            {
                try
                {
                    this.EquipmentDefinition = null;
                    EquipmentManager eqdeMgr = new EquipmentManager();
                    eqdeGuid = eqdeMgr.GetEquipmentDefinitionGuidFromObre(this.ObjectRelationGuid);
                }
                catch (Exception ex)
                {
                    logger.Warn(base.getWorkflowLoggingContext(engineContext) + "could not get eqde from obre, fallback to eqde.", ex);
                    if (this.EquipmentDefinitionGuid != null)
                        eqdeGuid = this.EquipmentDefinitionGuid;
                }
            }
            else
            {
                // get equipment from eqde guid
                eqdeGuid = this.EquipmentDefinitionGuid;
            }

            try
            {
                EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler();
                this.EquipmentDefinition = (EMDEquipmentDefinition) eqdeH.GetObject<EMDEquipmentDefinition>(eqdeGuid);
                this.EquipmentDefinitionConfig = this.EquipmentDefinition.GetEquipmentDefinitionConfig();
            }
            catch (Exception ex)
            {
                string msg = "error trying to read equipment definition and config.";
                return this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }


            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            foreach (string propName in EquipmentDefinitionConfig.Fields)
            {
                try
                {
                    string value = ReflectionHelper.GetPropValue(this.EquipmentDefinitionConfig, propName).ToString();
                    engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel(
                            variableName: "0.EQ_" + propName,
                            variableValue: value,
                            dataType: EnumVariablesDataType.stringType,
                            direction: EnumVariableDirection.both);
                }
                catch (Exception ex)
                {
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " could not read value for:" + propName, ex);
                }
            }
            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
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
