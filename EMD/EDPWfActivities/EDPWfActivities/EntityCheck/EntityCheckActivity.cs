using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.EntityCheck
{
    /// <summary>
    /// EntityCheckActivity is used to handle EntityCheck for Reset, Deletion and TaskItem-Update
    /// </summary>
    public class EntityCheckActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        private const string ModifyComment = "modified by EntityCheckActivity requested by sponsor";

        private const string VAR_ENTITY_CHECK_GUID = "EntityCheckGuid";
        private const string VAR_DODELETEENTITYCHECK = "DoDeleteEntityCheck";
        private const string VAR_DORESETENTITYCHECK = "DoResetEntityCheck";
        private const string VAR_TASKITEMGUID = "TaskItemGuid";

        private string EntityCheckGuid;
        private string TaskItemGuid;
        private bool DoDeleteEntityCheck;
        private bool DoResetEntityCheck;




        private EntityCheckManager enchManager;

        public EntityCheckActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            // setup return object 
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            string errorMessage = "error trying to get Workflow-variables";

            try
            {
                this.enchManager = new EntityCheckManager(null, ModifyComment);



                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_ENTITY_CHECK_GUID, false);
                this.EntityCheckGuid = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_DODELETEENTITYCHECK, true);
                if (tmp != null) this.DoDeleteEntityCheck = (bool)tmp.GetBooleanValue();


                tmp = base.GetProcessedActivityVariable(engineContext, VAR_DORESETENTITYCHECK, true);
                if (tmp != null) this.DoResetEntityCheck = (bool)tmp.GetBooleanValue();


                tmp = base.GetProcessedActivityVariable(engineContext, VAR_TASKITEMGUID, true);
                this.TaskItemGuid = tmp.VarValue;

                // do logical checks for misconfiguration
                if (string.IsNullOrWhiteSpace(this.EntityCheckGuid))
                {
                    string message = "EntityCheckGuid is not configured but necessary";
                    this.logger.Error(message);
                    throw new Exception(message);
                }

                if (!this.DoDeleteEntityCheck && !this.DoResetEntityCheck && string.IsNullOrEmpty(this.TaskItemGuid))
                {
                    string message = "DoDeleteEntityCheck, DoResetEntityCheck and TaskItemGuid are not set. Nothing to do!";
                    this.logger.Error(message);
                    throw new Exception(message);
                }

                if ((this.DoDeleteEntityCheck && this.DoResetEntityCheck) || (this.DoDeleteEntityCheck && !string.IsNullOrEmpty(this.TaskItemGuid)))
                {
                    string message = "DoDeleteEntityCheck can't be combined with DoReset or TaskItemGuid";
                    this.logger.Error(message);
                    throw new Exception(message);
                }
            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            if (this.DoDeleteEntityCheck)
            {
                string errorMessage = "error trying to delete EntityCheckk item:" + this.EntityCheckGuid;
                try
                {
                    this.enchManager.Delete(this.EntityCheckGuid);
                }
                catch (BaseException bEx)
                {
                    return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
                }
                catch (Exception ex)
                {
                    return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
                }
            }

            if (this.DoResetEntityCheck)
            {
                string errorMessage = "error trying to set IsWorkflowInProgress: " + this.EntityCheckGuid;
                try
                {
                    EMDEntityCheck ench = this.enchManager.Get(this.EntityCheckGuid);
                    enchManager.Reset(ench);
                }
                catch (BaseException bEx)
                {
                    return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
                }
                catch (Exception ex)
                {
                    return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
                }
            }

            if (!string.IsNullOrEmpty(this.TaskItemGuid))
            {
                string errorMessage = string.Format("error trying to update TaitGuid:{0} on EntityCheckGuid:{1}", this.TaskItemGuid, this.EntityCheckGuid);
                try
                {
                    EMDEntityCheck ench = this.enchManager.Get(this.EntityCheckGuid);
                    ench.TaitGuid = this.TaskItemGuid;
                    enchManager.Update(ench);
                }
                catch (BaseException bEx)
                {
                    return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
                }
                catch (Exception ex)
                {
                    return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
                }
            }

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            // setup return object 
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
