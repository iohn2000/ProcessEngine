using System;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using Ciloci.Flee;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.ProcessEngine.DataLayer;

namespace Kapsch.IS.EDP.WFActivity
{
    public class EdpFeatures : /*BaseActivity,*/ IEdpFeatures
    {
        static EdpFeatures instance;

        private EntityQuery entityQuery;

        IEDPLogger logger;

        private EdpFeatures()
        {
            logger = EDPLogger.GetLogger(this.GetType().FullName);
            entityQuery = new EntityQuery();
        }

        public static EdpFeatures GetInstance()
        {
            if (instance == null)
            {
                instance = new EdpFeatures();
            }
            return instance;
        }

        [Obsolete]
        public Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName)
        {
            return this.GetProcessedActivityVariable(engineContext, propertyName, false);
        }

        //public EdpFeatures(Type type) : base(type)
        //{
        //}

        /// <summary>
        /// calculates workflow variables and additionally resolves all @@ queries from core.
        /// Its recommended to always use this for CORE related activities. (becauee EntityQuery is available)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        [Obsolete("do not use", true)]
        public Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName, bool nullable)
        {
            //example: "Approval: " + "FirstName@@P_Guid@@{{0.EffectedPersonEmploymentGuid}}"

            //1. Evaluate All Workflow Variables {{#.whatever}}
            //2. Evaluate All @@-Queries
            //3. Evaluate Functions (Coloqi)

            Ciloci.Flee.IDynamicExpression wf_Expresssion;
            Variable procEngVar = null;
            ExpressionContext context = new ExpressionContext();

            try
            {
                string nullablemsg = "must not be null";
                if (nullable) nullablemsg = "is nullable null";
                
                //0.
                logger.Debug(string.Format("{0} GetProcessedActivityVariable working on Variable ({1}) name:'{2}' ",
                    this.UpdateLoggingContext(engineContext),
                    nullablemsg,
                    propertyName
                    ));

                //1.
                procEngVar = engineContext.GetProcessedActivityVariable(propertyName);
                
                //2.
                Type stringType = "".GetType();
                object edpVariable = entityQuery.QueryMixedString(procEngVar.VarValue, out stringType);
                
                //3.
                // check whether it is safe to avaluate ciloqi evaluator
                //A) 0.irgendwas
                //B) "hallo" + irgendwas
                //C) <d>aöld</asdk>
                bool evaluateFunctions = true;

                if (evaluateFunctions && (String.IsNullOrEmpty(edpVariable.ToString().Trim())))
                {
                    evaluateFunctions = false;
                }

                if (evaluateFunctions && edpVariable.ToString().Contains("</"))
                {
                    evaluateFunctions = false;
                }
                
                // second variation of empty xml tag
                if (evaluateFunctions && edpVariable.ToString().Contains("/>"))
                {
                    evaluateFunctions = false;
                }

                if (evaluateFunctions && edpVariable.ToString().Contains("\\\""))
                {
                    evaluateFunctions = false;
                }

                if (evaluateFunctions)
                {
                    wf_Expresssion = context.CompileDynamic(edpVariable.ToString());
                    edpVariable = wf_Expresssion.Evaluate();
                }
                                
                logger.Debug(string.Format("{0}GetProcessedActivityVariable calculated Result: ({1})",
                    GetWorkflowLoggingContext(engineContext), edpVariable));

                // create a Variable and return as a result
                Variable finishedVar = new Variable(procEngVar.Name, edpVariable.ToString(), procEngVar.DataType, procEngVar.Direction);
                return finishedVar;
            }
            catch (BaseException bEx)
            {                
                string errMsg = string.Format("{0} BaseException in GetProcessedActivityVariable. Variable Name: {1}", GetWorkflowLoggingContext(engineContext), propertyName);
                if (nullable) {
                    logger.Warn(errMsg, bEx);
                    return null;
                };
                logger.Error(errMsg, bEx);
                throw bEx;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0} General error in GetProcessedActivityVariable. Variable Name: {1}", GetWorkflowLoggingContext(engineContext), propertyName);
                if (nullable)
                {
                    logger.Warn(errMsg, ex);
                    return null;
                };
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
            }
        }

        private string UpdateLoggingContext(EngineContext engineContext)
        {
            return DataHelper.BuildLogContextPrefix(engineContext.uniqueRunTimeID.ToString(),
                                engineContext.WorkflowDefinitionName,
                                engineContext.WorkflowModel.InstanceID,
                                engineContext.CurrenActivity.Instance,
                                engineContext.stopWatch.ElapsedMilliseconds);
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


    }
}
