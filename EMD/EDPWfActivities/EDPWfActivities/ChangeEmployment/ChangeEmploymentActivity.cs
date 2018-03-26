using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Reflection;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.ChangeEmployment
{
    public class ChangeEmploymentActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        // EmplChangeWorkflowMessage ... 0.WFVariables
        private string ChangeType = null;
        private Variable ChangeTypeVariable;
        private string RequestingPersonEmploymentGuid = null;
        private string EffectedPersonEmploymentGuid = null;
        private DateTime TargetDate;
        private string GuidCostCenter = null;
        private string GuidEmploymentType = null;
        private string GuidOrgUnit = null;
        private string GuidDistributionGroup = null;
        private string PersonalNumber = null;
        private string RemoveEquipmentInfos = null;
        private string KCCData = null;
        private string EmailType = null;
        private string MoveAllRoles = null;
        private bool boolMoveAllRoles = false;
        private string GuidSponsor = null;
        private string GuidLocation = null;
        private DateTime? LeaveTo = null;
        private DateTime? LeaveFrom = null;


        /// <summary>
        /// Only called for SMALL Changes without Employment Move. Does just 
        /// </summary>
        public ChangeEmploymentActivity() : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            logger.Debug(string.Format("{0} : Initialize() started", getWorkflowLoggingContext(engineContext)));
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            Variable tmp;
            // initialize all Variables here, read settings etc.
            try
            {
                tmp = base.GetProcessedActivityVariable(engineContext, "TargetDate", false);
                if (!string.IsNullOrWhiteSpace(tmp.VarValue))
                {
                    this.TargetDate = tmp.GetDateValue().Value;
                }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "Errro parsing date:" + tmp.VarValue?? "");
                }

                this.RequestingPersonEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "RequestingPersonEmploymentGuid", false).GetStringValue();
                this.EffectedPersonEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "EffectedPersonEmploymentGuid", false).GetStringValue();

                this.ChangeTypeVariable = base.GetProcessedActivityVariable(engineContext, "ChangeType", true);
                if (tmp != null) this.ChangeType = this.ChangeTypeVariable.GetStringValue();

                this.GuidCostCenter = base.GetProcessedActivityVariable(engineContext, "GuidCostCenter", false).GetStringValue();

                this.GuidOrgUnit = base.GetProcessedActivityVariable(engineContext, "GuidOrgUnit", false).GetStringValue();

                this.GuidLocation = base.GetProcessedActivityVariable(engineContext, "GuidLocation", false).GetStringValue();

                tmp = base.GetProcessedActivityVariable(engineContext, "GuidDistributionGroup", true);
                if (tmp != null) this.GuidDistributionGroup = tmp.GetStringValue();

                this.RemoveEquipmentInfos = base.GetProcessedActivityVariable(engineContext, "RemoveEquipmentInfos", false).GetStringValue();

                tmp = base.GetProcessedActivityVariable(engineContext, "KCCData", true);
                if (tmp != null) this.KCCData = tmp.GetStringValue();

                tmp = base.GetProcessedActivityVariable(engineContext, "EmailType", true);
                if (tmp != null) this.EmailType = tmp.GetStringValue();

                tmp = base.GetProcessedActivityVariable(engineContext, "MoveAllRoles", false);
                if (tmp != null) boolMoveAllRoles = (bool)tmp.GetBooleanValue();

                tmp = base.GetProcessedActivityVariable(engineContext, "GuidSponsor", true);
                if (tmp != null) this.GuidSponsor = tmp.GetStringValue();


                tmp = engineContext.GetWorkflowVariable("0.LeaveFromIso8601");
                if (tmp != null && !string.IsNullOrWhiteSpace(tmp.VarValue))
                    this.LeaveFrom = tmp.GetDateValue();
                else
                    this.LeaveFrom = null;

                tmp = engineContext.GetWorkflowVariable("0.LeaveToIso8601");
                if (tmp != null && !string.IsNullOrWhiteSpace(tmp.VarValue))
                    this.LeaveTo = tmp.GetDateValue();
                else
                    this.LeaveTo = null;

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

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            // nur meta code
            try
            {
                EnumEmploymentChangeType t = this.ChangeTypeVariable.ConvertToEnum<EnumEmploymentChangeType>();
                EmploymentManager emplM = new EmploymentManager();
                emplM.DoChange(t,
                    this.RequestingPersonEmploymentGuid,
                    this.EffectedPersonEmploymentGuid,
                    null,
                    this.GuidCostCenter,
                    this.GuidEmploymentType,
                    this.GuidOrgUnit,
                    this.GuidDistributionGroup,
                    this.GuidLocation,
                    this.GuidSponsor,
                    this.boolMoveAllRoles,
                    this.PersonalNumber,
                    this.KCCData,
                    false,
                    this.LeaveFrom,
                    this.LeaveTo); //TODO Mail always keeps the same actually
                                   //this.EmailType.GetStringValue());
            }
            catch (EntityNotFoundException enfEx)
            {
                string msg = "error EntityNotFoundException : " + enfEx.EntityClassName;
                return this.logErrorAndReturnStepState(engineContext, enfEx, msg, EnumStepState.ErrorToHandle);
            }
            catch (BaseException bEx)
            {
                string msg = string.Format("BaseException while trying to do change employment. {0}", bEx.Message);
                return this.logErrorAndReturnStepState(engineContext, bEx, msg, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error callling DoChange() ";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            // write wf variables - only here!

            return result;
        }

        #region Helpers
        #endregion

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
