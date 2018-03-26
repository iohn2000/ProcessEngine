using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.WFActivity.Variables;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Variables;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Kapsch.IS.EDP.WFActivity
{
    public abstract class BaseEDPActivity : BaseActivity, IEdpFeatures
    {
        private const string VAR_BUSINESSCASE = "BusinessCase";
        private const string VAR_CHANGETYPE = "ChangeType";
        
        private EdpFeatures edpFeatures;
        internal DatabaseAccess database;
        private String strWaitItemConfig;
        private DateTime? timeoutDate;
        private string ResultMessage;
        private readonly string VAR_RESULTMESSAGE = "ResultMessage";
        private readonly string VAR_ACTIVITYMESSAGE = "__Activity.ResultValue__";

        public EnumEmploymentChangeType ChangeType { get; set; }

        /// <summary>
        /// Which kind of Businesscase was choosen in the UI
        /// </summary>
        public EnumBusinessCase BusinessCase { get; set; }

        public BaseEDPActivity(Type type) : base(type)
        {
            this.edpFeatures = EdpFeatures.GetInstance();
        }


        /// <summary>
        /// Standard method to be implemented by any Async Activity for its doing
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public abstract StepReturn PostInitialize(EngineContext engineContext);

        public override StepReturn Initialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);
            // get BusinessType and ChangeType

            Variable tmp = engineContext.GetWorkflowVariable("0." + VAR_BUSINESSCASE);
            if (tmp != null)
            {
                try
                {
                    this.BusinessCase = tmp.ConvertToEnum<EnumBusinessCase>();
                }
                catch (BaseException bEx)
                {
                    // make only a warning. set default value, be backward compatible 
                    string errMsg = string.Format("{0} Error converting variable: 0.{1}. Value = '{2}' Msg = {3}",
                        base.getWorkflowLoggingContext(engineContext), VAR_BUSINESSCASE, tmp.VarValue, bEx.Message);
                    logger.Warn(errMsg);
                    this.BusinessCase = EnumBusinessCase.NotDefined;
                }
            }
            else
            {
                // make only a warning? set default value, be backward compatible ??
                string errMsg = string.Format("{0} BusinessCase variable: 0.{1} doesnt exist.",
                    base.getWorkflowLoggingContext(engineContext), VAR_BUSINESSCASE);
                logger.Warn(errMsg);
                this.BusinessCase = EnumBusinessCase.NotDefined;
            }

            tmp = engineContext.GetWorkflowVariable("0." + VAR_CHANGETYPE);
            if (tmp != null)
            {
                try
                {
                    this.ChangeType = tmp.ConvertToEnum<EnumEmploymentChangeType>();
                }
                catch (BaseException bEx)
                {
                    // make only a warning. set default value, be backward compatible 
                    string errMsg = string.Format("{0} Error converting variable: 0.{1}. Value = '{2}' Msg = {3}",
                        base.getWorkflowLoggingContext(engineContext), VAR_CHANGETYPE, tmp.VarValue, bEx.Message);
                    logger.Warn(errMsg);
                    this.ChangeType = EnumEmploymentChangeType.NoChange;
                }
            }
            else
            {
                // make only a warning? set default value, be backward compatible ??
                string errMsg = string.Format("{0} BusinessCase variable: 0.{1} doesnt exist.",
                    base.getWorkflowLoggingContext(engineContext), VAR_CHANGETYPE);
                logger.Warn(errMsg);
                this.ChangeType = EnumEmploymentChangeType.NoChange;
            }
            // 
            // call PostInitialize
            //
            result = PostInitialize(engineContext);

            return result;
        }

        public override void StoreResult(StepReturn stepReturn, EngineContext engineContext, bool isWarning, string resultMsg = "")
        {
            try
            {
                string logMsg = string.Format("{0} resulted: {1} with state: {2} ", engineContext.CurrenActivity.Instance, stepReturn.ReturnValue, stepReturn.StepState.ToString());

                Variable tmp;
                tmp = this.GetProcessedActivityVariable(engineContext, VAR_RESULTMESSAGE, true);
                if (tmp != null)
                {
                    this.ResultMessage = tmp.VarValue;
                    this.ResultMessage = this.ResultMessage.Replace(this.VAR_ACTIVITYMESSAGE, logMsg);
                }
                else
                {
                    this.ResultMessage = logMsg;
                }

                base.StoreResult(stepReturn, engineContext, isWarning, this.ResultMessage);

                ProcessEntityManager prenM = new ProcessEntityManager(null, this.getWorkflowLoggingContext(engineContext));
                EMDProcessEntity pren = null;
                string wfInstance = engineContext.WorkflowModel.InstanceID;

                //TODO <ResultMessage>"Set employment status to declined for " + "{{x.EmplGuid}}" + ", " + "{{Activity.ResultValue}}"</ResultMessage> 

                var entities = prenM.GetList("WFI_ID=\"" + wfInstance + "\"");

                if (entities.Count > 1)
                {
                    //found more than one processentity for this wf: Write Warning and all but first one.
                    logger.Warn(base.getWorkflowLoggingContext(engineContext) + " Duplicate Entries in ProcessEntity found for this WFI_instance");
                    pren = entities[0];
                }
                else
                {
                    //standard case ... write data
                    pren = entities[0];
                }
                pren.WFResultMessages += this.ResultMessage;

                prenM.UpdateOrCreate(pren);
            }
            catch (BaseException bEx)
            {
                // log eror but ignore return value
                this.logErrorAndReturnStepState(engineContext, bEx, bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                // log eror but ignore return value
                this.logErrorAndReturnStepState(engineContext, ex, ex.Message, EnumStepState.ErrorToHandle);
            }


        }

        [Obsolete]
        public Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName)
        {
            return this.edpFeatures.GetProcessedActivityVariable(engineContext, propertyName, false);
        }

        /// <summary>
        /// calculates workflow variables and additionally resolves all @@ queries from core.
        /// Its recommended to always use this for CORE related activities. (becauee EntityQuery is available)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="propertyName"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public override Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName, bool nullable)
        {
            try
            {
                Variable procEngVar = engineContext.GetActivityVariable(propertyName);

                string processedValue = new WFReplacer(procEngVar.VarValue, engineContext.WorkflowModel).Replace().ProcessedValue;

                processedValue = new EDPReplacer(processedValue).Replace().ProcessedValue;

                processedValue = base.Evaluate(processedValue, engineContext);

                procEngVar.VarValue = processedValue;

                return new Variable(procEngVar.Name, processedValue.ToString(), procEngVar.DataType, procEngVar.Direction);
            }
            catch (BaseException bEx)
            {
                string errMsg = string.Format("{0} BaseException in GetProcessedActivityVariable. Variable Name: {1}", this.GetWorkflowLoggingContext(engineContext), propertyName);
                if (nullable)
                {
                    //ignore this exception
                    return null;
                };
                logger.Error(errMsg, bEx);
                throw bEx;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0} General error in GetProcessedActivityVariable. Variable Name: {1}", this.GetWorkflowLoggingContext(engineContext), propertyName);
                if (nullable)
                {
                    logger.Warn(errMsg, ex);
                    return null;
                };
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
            }

        }

        /// <summary>
        /// 2) Create a async wait item with
        ///    AWI_InstanceID    
        ///    AWI_ActivityID    
        ///    AWI_Status = Wait  
        ///    
        ///    AWI_DueDate = 2 versions : a) for ANGELEGT = 30 mins and b) BEENDET/STORNO = 2 Wochen??
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns>waitItem Guid</returns>
        public int createAWIItem(EngineContext engineContext, DbContextTransaction transaction)
        {
            // WAIT ITEM
            // create info so that WAIT item can poll later...
            int waitItemID = this.database.CreateWaitItem(
                transaction,
                engineContext.WorkflowModel.InstanceID,
                engineContext.CurrenActivity.Instance,
                this.strWaitItemConfig,
                this.timeoutDate);

            return waitItemID;
        }

        public int createAWIItem(EngineContext engineContext, DbContextTransaction transaction, string waitItemConfig, DateTime dueDate)
        {
            // WAIT ITEM
            // create info so that WAIT item can poll later...
            int waitItemID = this.database.CreateWaitItem(
                transaction,
                engineContext.WorkflowModel.InstanceID,
                engineContext.CurrenActivity.Instance,
                waitItemConfig,
                dueDate);

            return waitItemID;
        }

        private string GetWorkflowLoggingContext(EngineContext engineContext)
        {
            string loggingContext = "";
            try
            {
                loggingContext = DataHelper.BuildLogContextPrefix(
                                engineContext.uniqueRunTimeID.ToString(),
                            engineContext.WorkflowDefinitionName,
                            engineContext.WorkflowModel.InstanceID,
                                engineContext.CurrenActivity.Instance);
                return loggingContext;
            }
            catch (Exception ex)
            {
                //just catch and do nothing. Otherwise Logging fails and creates error to activity!
                //TODO: WorkflowModel getter und setter must be reengineered to NOT throw an exception when calling empty values!
                logger.Error("EdpFeatures.GetWorkflowLoggingContext() failed", ex);
            }
            return loggingContext;
        }

        public Dictionary<string, string> WriteDateTimeToDictionary(EngineContext engineContext, Dictionary<string, string> renderDictionary, DateTime date, String varName)
        {
            renderDictionary.Add(varName, date.ToString("dd MM yyyy"));
            if (date == null)
            {
                renderDictionary.Add(varName, "");
            }
            return renderDictionary;
        }

    }
}
