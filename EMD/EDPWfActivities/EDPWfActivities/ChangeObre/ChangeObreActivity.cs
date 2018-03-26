using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.ProcessEngine;
using System.Reflection;
using System.Xml.Linq;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.WFActivity.ChangeObre
{
    public class ChangeObreActivity : BaseEDPActivity, IActivityValidator
    {
        private string ObreGuid;
        private string OldEmploymentGuid;
        private string NewEmploymentGuid;
        private bool HasNewEmploymentGuid = false;

        public ChangeObreActivity() : base(MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                Variable tmp;

                tmp = base.GetProcessedActivityVariable(engineContext, "obreGuid", false);
                this.ObreGuid = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, "newEmploymentGuid", true);
                if (tmp != null)
                {
                    this.NewEmploymentGuid = tmp.VarValue;
                    this.HasNewEmploymentGuid = true;
                }
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
            CoreTransaction ct = new CoreTransaction();
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            PackageManager pm = new PackageManager(ct, null, "by ChangeObreActivity");
            try
            {
                ct.Begin();

                if ((!String.IsNullOrWhiteSpace(this.ObreGuid)) && this.HasNewEmploymentGuid)
                {
                    pm.MoveEquipmentToEmploymentPackage(this.ObreGuid, this.NewEmploymentGuid);
                    logger.Debug("ChangeObre: Putting Equipment to the new Employment.");
                }
                else if (!this.HasNewEmploymentGuid)
                {
                    logger.Debug("ChangeObre: Keeping Equipment on the same Employment. Do Nothing");
                }
                else
                {
                    string msg = "could not find OBRE with Guid = " + this.ObreGuid;
                    return this.logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                }
                //set Status for this Obre to be active again
                logger.Debug("ChangeObre: Set EquipmentStatus to Active.");
                pm.SetEquipmentStatus(this.ObreGuid, ProcessStatus.STATUSITEM_ACTIVE);
                ct.Commit();
                return result;
            }
            catch (BaseException bEx)
            {
                ct.Rollback();
                string msg = "error trying to update obre " + this.ObreGuid;
                return this.logErrorAndReturnStepState(engineContext, bEx, bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                ct.Rollback();
                string msg = "error trying to update obre " + this.ObreGuid;
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        #region Validation
        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
