using Kapsch.IS.EDP.WFActivity.Variables;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Variables;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity
{
    public abstract class BaseEDPCallbackActivity : BaseCallbackActivity, IEdpFeatures, IProcessStep
    {

        public BaseEDPCallbackActivity(Type type) : base(type)
        {
        }

        #region from IEdpFeatures
        /// <summary>
        /// calculates workflow variables and additionally resolves all @@ queries from core.
        /// Its recommended to always use this for CORE related activities. (becauee EntityQuery is available)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName)
        {
            return this.GetProcessedActivityVariable(engineContext, propertyName, false);
        }

        public Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName, bool nullable)
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
                string errMsg = string.Format("{0} BaseException in GetProcessedActivityVariable. Variable Name: {1}",
                    this.getWorkflowLoggingContext(engineContext), propertyName);
                if (nullable)
                {
                    logger.Warn(errMsg, bEx);
                    return null;
                };
                logger.Error(errMsg, bEx);
                throw bEx;
            }

            catch (Exception ex)
            {
                string errMsg = string.Format("{0} General error in GetProcessedActivityVariable. Variable Name: {1}",
                    this.getWorkflowLoggingContext(engineContext), propertyName);
                if (nullable)
                {
                    logger.Warn(errMsg, ex);
                    return null;
                };
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
            }
        }



        #endregion
    }
}
