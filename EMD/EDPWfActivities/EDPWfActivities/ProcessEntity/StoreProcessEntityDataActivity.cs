using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.ProcessEntity
{
    /// <summary>
    ///   Store ProcessEntity Data
    ///       Store specific WF data to ProcessEntity in EDP
    /// </description>
    /// <properties>
    ///     <property name = "WFResultMessages"  direction="input"  dataType="stringType" ></property>
    ///     <property name = "RequestorEmplGuid"  direction="input"  dataType="stringType" ></property>
    ///     <property name = "EffectedPersGuid"  direction="input"  dataType="stringType" ></property>
    ///     <property name = "WFStartTime"  direction="input"  dataType="stringType" ></property>
    ///     <property name = "WFTargetDate"  direction="input"  dataType="stringType" ></property>
    /// </properties>
    /// </summary>
    public class StoreProcessEntityDataActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        public const string VAR_EntityGuid = "EntityGuid";
        public const string VAR_WFResultMessages = "WFResultMessages";
        public const string VAR_RequestorEmplGuid = "RequestorEmplGuid";
        public const string VAR_EffectedPersGuid = "EffectedPersGuid";
        public const string VAR_WFStartTime = "WFStartTime";
        public const string VAR_WFTargetDate = "WFTargetDate";

        private string WFResultMessages = "stored ProcessEntity";
        private string RequestorEmplGuid = null;
        private string EffectedPersGuid = null;
        private DateTime? WFStartTime = null;
        private DateTime? WFTargetDate = null;
        private EMDProcessEntity pren = null;
        private string EntityGuid = null;

        public StoreProcessEntityDataActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);
            try
            {
                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_EntityGuid, false);
                this.EntityGuid = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_RequestorEmplGuid, false);
                this.RequestorEmplGuid = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_EffectedPersGuid, true);
                this.EffectedPersGuid = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_WFResultMessages, true);
                this.WFResultMessages = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_WFStartTime, true);
                string timeTmp = tmp.VarValue;
                if (timeTmp.Trim().Equals("_NOW"))
                {
                    this.WFStartTime = DateTime.Now;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(timeTmp))
                    {
                        this.WFStartTime = tmp.GetDateValue();
                    }
                    else
                    {
                        logger.Error(base.getWorkflowLoggingContext(engineContext) + " Error parsing WFStartTime date: " + timeTmp ?? "" + " continue without Error2Handle");
                    }
                }
                

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_WFTargetDate, true);
                timeTmp = tmp.VarValue;
                if (timeTmp.Trim().Equals("_NOW"))
                {
                    this.WFTargetDate = DateTime.Now;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(timeTmp))
                    {
                        this.WFTargetDate = tmp.GetDateValue();
                    }
                    else
                    {
                        logger.Error(base.getWorkflowLoggingContext(engineContext) + " Error parsing WFTargetDate date: " + timeTmp ?? "" + " continue without Error2Handle");
                    }

                }

                string msg = String.Format(
                    base.getWorkflowLoggingContext(engineContext)
                    + " Called StoreProcessEntityDataActivity with values: "
                    + "EntityGuid: {0}, "
                    + "RequestorEmplGuid: {1}, "
                    + "EffectedPersGuid: {2}, "
                    + "WFResultMessages: {3}, "
                    + "WFStartTime: {4}, "
                    + "WFTargetDate: {5}"
                    , this.EntityGuid, this.RequestorEmplGuid, this.EffectedPersGuid, this.WFResultMessages, this.WFStartTime, this.WFTargetDate);

                logger.Info(msg);
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

            string wfInstance = engineContext.WorkflowModel.InstanceID;

            ProcessEntityManager prenM = new ProcessEntityManager();
            var entities = prenM.GetList("WFI_ID=\"" + wfInstance + "\"");
            if (entities == null || entities.Count == 0)
            {
                //ProcessEntity not found => create new one. Since this could be a subworkflow
                this.pren = new EMDProcessEntity();
                this.pren.WFI_ID = wfInstance;
                this.pren.WFD_ID = "empty"; // engineContext.WorkflowModel
                this.pren.WFD_Name = engineContext.WorkflowDefinitionName;
                this.pren.EntityGuid = this.EntityGuid;
                if (!string.IsNullOrEmpty(this.EffectedPersGuid)) this.pren.EffectedPersGuid = this.EffectedPersGuid;
                this.pren.RequestorEmplGuid = this.RequestorEmplGuid;
            }
            else if (entities.Count > 1)
            {
                //found more than one processentity for this wf: Write Warning and all but first one.
                logger.Warn(base.getWorkflowLoggingContext(engineContext) + " Duplicate Entries in ProcessEntity found for this WFI_instance");
                this.pren = (EMDProcessEntity) entities[0];
            }
            else
            {
                //standard case ... write data
                this.pren = (EMDProcessEntity) entities[0];
            }


            if (!string.IsNullOrEmpty(this.WFResultMessages)) this.pren.WFResultMessages = this.WFResultMessages;
            if (this.WFStartTime != null) this.pren.WFStartTime = (DateTime) this.WFStartTime;
            if (this.WFTargetDate != null) this.pren.WFTargetDate = (DateTime) this.WFTargetDate;

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);

            //store pren to EDP
            ProcessEntityManager prenM = new ProcessEntityManager();
            prenM.ModifyComment = engineContext.WorkflowDefinitionName + "-" + engineContext.WorkflowModel.InstanceID + " Update by StoreProcessEntityDataActivity";
            prenM.UpdateOrCreate(this.pren);

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            // setup return object 
            StepReturn result = new StepReturn("Successfully stored ProcessEntity", EnumStepState.Complete);
            return result;
        }
    }
}
