using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.SetEquipmentStatus
{
    public class SetEquipmentStatusActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        private Variable processStatus = null;
        private Variable obreGuid = null;
        private Variable ExpectedInitializeState = null;
        private EMDObjectRelation obre = null;
        private ObjectRelationHandler obreH;
        private bool DoKeep;

        public SetEquipmentStatusActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                this.processStatus = base.GetProcessedActivityVariable(engineContext, "status", false);
                this.obreGuid = base.GetProcessedActivityVariable(engineContext, "obreGuid", false);
                try
                {
                    Variable temp = engineContext.GetWorkflowVariable("0.DoKeep");
                    if (temp != null)
                    {
                        bool? doKeep = temp.GetBooleanValue();
                        if (doKeep.HasValue)
                        {
                            this.DoKeep = doKeep.Value;
                        }
                    }
                }
                catch (Exception) { }
            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get workflowvariables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            PackageManager pm = new PackageManager();
            pm.ModifyComment = "by SetEquipmentStatusActivity";
            try
            {
                int status = this.processStatus.GetIntValue();
                pm.SetEquipmentStatus(this.obreGuid.GetStringValue(), status, this.DoKeep);

                if (status == ProcessStatus.STATUSITEM_REMOVED && this.DoKeep)
                {
                    logger.Debug(string.Format("Set Remove Status to EquipmentStatus.STATUSITEM_REMOVED_GIFTEDTOEMPLOYMENT for OBRE:{0}", this.obreGuid));
                }
            }
            catch (BaseException bEx)
            {
                result.StepState = EnumStepState.ErrorToHandle;
                return this.logErrorAndReturnStepState(engineContext, bEx, "SetEQStatus.Run(): " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                result.StepState = EnumStepState.ErrorToHandle;
                return this.logErrorAndReturnStepState(engineContext, ex, "SetEQStatus.Run(): Exception", EnumStepState.ErrorToHandle);
            }
            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                engineContext.SetActivityVariable("returnStatus", "ok");
            }
            catch (BaseException bEx)
            {
                result.StepState = EnumStepState.ErrorToHandle;
                return this.logErrorAndReturnStepState(engineContext, bEx, "SetEQStatus.Finish(): " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                result.StepState = EnumStepState.ErrorToHandle;
                return this.logErrorAndReturnStepState(engineContext, ex, "SetEQStatus.Finish(): Exception", EnumStepState.ErrorToHandle);
            }
            return result;

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
