using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.OffBoarding
{
    public class OffBoardingActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        private Variable RequestingPersonEmploymentGuid;
        private Variable EffectedPersonEmploymentGuid;
        private Variable RemoveEquipmentInfos;
        private DateTime ExitDate;
        private string RequestingPersonGuid = null;
        private DateTime LastDay;

        public OffBoardingActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                this.RequestingPersonEmploymentGuid = engineContext.GetWorkflowVariable("0.RequestingPersonEmploymentGuid");
                this.EffectedPersonEmploymentGuid = engineContext.GetWorkflowVariable("0.EffectedPersonEmploymentGuid");
                this.RemoveEquipmentInfos = engineContext.GetWorkflowVariable("0.RemoveEquipmentInfos");
                var varExitDate = engineContext.GetWorkflowVariable("0.ExitDateIso8601");
                var varLastDay = engineContext.GetWorkflowVariable("0.LastDayIso8601");

                logger.Debug(string.Format("{0} LastDay:{1} , ExitDate:{2}", base.getWorkflowLoggingContext(engineContext), varLastDay.VarValue, varExitDate.VarValue));

                this.ExitDate = DataHelper.Iso8601ToDateTime(varExitDate.VarValue);


                if (varLastDay != null)
                {
                    this.LastDay = DataHelper.Iso8601ToDateTime(varLastDay.VarValue);
                }
                else
                {
                    this.LastDay = this.ExitDate;
                }

                logger.Debug(string.Format("{0} LastDay:{1} , ExitDate:{2}", base.getWorkflowLoggingContext(engineContext), this.LastDay, this.ExitDate));

                EmploymentHandler emplH = new EmploymentHandler();
                EMDEmployment reqEmpl = (EMDEmployment)emplH.GetObject<EMDEmployment>(this.RequestingPersonEmploymentGuid.VarValue);
                if (reqEmpl != null)
                {
                    this.RequestingPersonGuid = reqEmpl.P_Guid;
                }
            }
            catch (BaseException bEx)
            {
                return base.logErrorAndReturnStepState(engineContext, bEx, bEx.Message, EnumStepState.ErrorToHandle);
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

            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            //return result;

            try
            {
                //
                // start subwf for equipments (NewEquipmentInfos)
                // 
                result = this.StartSubWorkflowsEquipmentRemove(engineContext, this.EffectedPersonEmploymentGuid, result);
                //
                // delete employment
                //
                this.offBoardEmployment(engineContext);
            }
            catch (Exception ex)
            {
                engineContext.SetActivityVariable("returnStatus", "Error");
                return base.logErrorAndReturnStepState(engineContext, ex, "Error in OffOnboarding Activity", EnumStepState.ErrorStop);
            }

            return result;
        }



        public override StepReturn Finish(EngineContext engineContext)
        {
            // nothing todo
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                //
                //fill ReturnStatus variable
                //
                engineContext.SetActivityVariable("returnStatus", "Complete");
            }
            catch (Exception ex)
            {
                return this.logErrorAndReturnStepState(engineContext, ex, ex.Message, EnumStepState.ErrorStop);
            }

            return result;
        }

        private StepReturn StartSubWorkflowsEquipmentRemove(EngineContext engineContext, Variable emplGuid, StepReturn ret)
        {
            //WorkflowHandler wfH = new WorkflowHandler();

            if (this.RemoveEquipmentInfos != null && emplGuid != null)
            {
                //CoreTransaction transaction = new CoreTransaction();
                //transaction.Begin();
                try
                {
                    EquipmentManager eqMgr = new EquipmentManager();
                    //remove quotes if there
                    if (this.RemoveEquipmentInfos.VarValue.StartsWith("\""))
                        this.RemoveEquipmentInfos.VarValue = this.RemoveEquipmentInfos.VarValue.Remove(0, 1);
                    if (this.RemoveEquipmentInfos.VarValue.EndsWith("\""))
                        this.RemoveEquipmentInfos.VarValue = this.RemoveEquipmentInfos.VarValue.Remove(this.RemoveEquipmentInfos.VarValue.Length - 1, 1);

                    XElement xElEqInfo = XElement.Parse(this.RemoveEquipmentInfos.VarValue);
                    var lstEqInfos = xElEqInfo.XPathSelectElements("/RemoveEquipmentInfo");

                    foreach (var eqInfoElement in lstEqInfos)
                    {
                        RemoveEquipmentInfo removeEqInfo = null;
                        try
                        {
                            removeEqInfo = XmlSerialiserHelper.DeserialiseFromXml<RemoveEquipmentInfo>(eqInfoElement.ToString());

                            DateTime tDate = removeEqInfo.DateOfAction != null ? removeEqInfo.DateOfAction.Value : DateTime.Now;

                            ObreRemoveWorkflowMessage obreRemoveWfMsg = WorkflowMessageHelper.GetObreRemoveWorkflowMessage(
                                transaction: null,
                                userGuid: this.RequestingPersonGuid,
                                effectedPersonEmploymentGuid: this.EffectedPersonEmploymentGuid.VarValue,
                                obreGuid: removeEqInfo.ObreGuid,
                                equipmentDefinitionGuid: removeEqInfo.EquipmentDefinitionGuid,
                                requestingPersEMPLGuid: this.RequestingPersonEmploymentGuid.VarValue,
                                targetDate: tDate,
                                doKeep: removeEqInfo.DoKeep,
                                businessCase: base.BusinessCase,
                                changeType: base.ChangeType);

                            obreRemoveWfMsg.CreateWorkflowInstance(this.RequestingPersonGuid, "OffboardingActivity.StartSubWorkflowsEquipmentRemove()");
                            //WorkflowMessageData workflowMessageDataItem = WfHelper.GetWorkflowMessageData(obreRemoveWfMsg);
                            //wfH.CreateNewWorkflowInstance(workflowMessageDataItem);
                        }
                        catch (Exception ex)
                        {
                            string errMsg = " : Error trying to start subworkflows for remove EQs." + emplGuid.VarValue + "\r\nOBRE Guid:" + removeEqInfo.ObreGuid;
                            logger.Error(base.getWorkflowLoggingContext(engineContext) + errMsg, ex);
                            try
                            {
                                ObjectContainerHandler objectContainerHandler = new ObjectContainerHandler();
                                EMDObjectRelation obre = (EMDObjectRelation)objectContainerHandler.GetObject<EMDObjectRelation>(removeEqInfo.ObreGuid);
                                if (obre != null)
                                {
                                    obre.Status = (byte)ProcessStatus.STATUSITEM_NOTSET; //TODO should be error ! byte != int -1
                                    objectContainerHandler.UpdateObject(obre);
                                }

                            }
                            catch (Exception)
                            {
                                errMsg = "faild to set status for OBRE Guid:" + removeEqInfo?.ObreGuid;
                                logger.Error(base.getWorkflowLoggingContext(engineContext) + errMsg, ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //transaction.Rollback();
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error trying to start subworkflows for remove EQs." + emplGuid.VarValue + "\r\nData was :\r\n" + this.RemoveEquipmentInfos.VarValue, ex);
                    ret.ReturnValue = "Activity Error";
                    ret.StepState = EnumStepState.ErrorStop;
                    ret.DetailedDescription = ex.ToString();
                    return ret;
                }
            }
            else
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in OffBoarding Activity. Cannot read RemoveEqsInfo and EffectedPersonEmploymentGuid");
                ret.ReturnValue = "Activity Error";
                ret.StepState = EnumStepState.ErrorStop;
                return ret;
            }

            return ret;
        }

        #region Helpers

        private void setEmploymentStatus(EngineContext engineContext)
        {
            EmploymentHandler emplH = new EmploymentHandler();
            EMDEmployment empl = (EMDEmployment)emplH.GetObject<EMDEmployment>(this.EffectedPersonEmploymentGuid.VarValue);
            empl.Status = ProcessStatus.STATUSITEM_ACTIVE;
            emplH.UpdateObject(empl, historize: true);
            empl.ModifyComment = "Offboarding Activity, start requested by: " + RequestingPersonEmploymentGuid;
            engineContext.SetActivityVariable("returnStatus", "ok");
        }
        private void offBoardEmployment(EngineContext engineContext)
        {
            OffboardingManager offboardingManager = new OffboardingManager(new CoreTransaction(), this.RequestingPersonGuid,  modifyComment: "Offboarding Activity, finish requested by: " + RequestingPersonEmploymentGuid.GetStringValue());
            offboardingManager.RemoveEmployment(this.EffectedPersonEmploymentGuid.VarValue);
        }
        #endregion

        #region  Validation
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
