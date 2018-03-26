using Ciloci.Flee;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine.WFActivity
{
    /// <summary>
    /// Base class for all Activities
    /// </summary>
    public abstract class BaseActivity : IProcessStep
    {
        /// <summary>
        /// const for setting email subject prefix
        /// </summary>
        public const string CONST_EMAILSUBJECTPREFIX = "EmailSubjectPrefix";
        //Get logger
        protected internal IEDPLogger logger;

        /// <summary>
        /// provide a standard pre-text for logging.
        /// this includes a unique ID for a call of runtime and the workflowinstance and activity
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public string getWorkflowLoggingContext(EngineContext engineContext)
        {
            string loggingContext = "";
            try
            {
                loggingContext = DataHelper.BuildLogContextPrefix(engineContext.uniqueRunTimeID.ToString(),
                                engineContext.WorkflowDefinitionName, engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance);
                return loggingContext;
            }
            catch (Exception ex)
            {
                //just catch and do nothing. Otherwise Logging fails and creates error to activity!
                //TODO: WorkflowModel getter und setter must be reengineered to NOT throw an exception when calling empty values!
                logger.Error("BaseActivity.GetWorkflowLoggingContext() failed", ex);
            }
            return loggingContext;
        }

        /// <summary>
        /// logs error and sets StepReturn value
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="ex"></param>
        /// <param name="errMsg"></param>
        /// <param name="returnState"></param>
        /// <returns></returns>
        public StepReturn logErrorAndReturnStepState(EngineContext engineContext, Exception ex, string errMsg, EnumStepState returnState)
        {
            StepReturn result = new StepReturn("", EnumStepState.ErrorToHandle);
            string msg = string.Format("{0} {1}", this.getWorkflowLoggingContext(engineContext), errMsg);
            if (ex != null)
            {
                logger.Error(msg, ex);
                result.DetailedDescription = ex.ToString();
            }
            else
            {
                logger.Error(msg);
                result.DetailedDescription = "";
            }
            result.StepState = returnState;
            result.ReturnValue = errMsg;

            return result;
        }

        /// <summary>
        /// WARNING !! This is NOT working for @@ queries !! (EDP specific wf variables)
        /// Needs to be duplicated into EDPWfActivities solution to work with @@ queries
        /// tries to read a activity variable, with possible fallback paths via app.config and hard coded
        /// tries to read activity varialbe ( variableNameyActivity )
        /// if acitivity variable doesnt exits --> check in app.config ( settingsKeyDefault )
        /// if app.config setting doesnt exists --> use hard coded value ( lastResoutValue )
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="variableNameyActivity"></param>
        /// <param name="settingsKeyDefault"></param>
        /// <param name="lastResourtValue"></param>
        /// <returns></returns>
        ///[Obsolete("WARNING !! This is NOT working for @@ queries !! (EDP specific wf variables).")]
        public string GetSettingWithFallbackToAppConfig(EngineContext engineContext, string variableNameyActivity, string settingsKeyDefault, string lastResourtValue)
        {
            string demoRecipient = null;

            Variable varDemoRecipient = null;

            try
            {
                varDemoRecipient = this.GetProcessedActivityVariable(engineContext, variableNameyActivity, false);
            }
            catch (Exception ex)
            {
                string msg = getWorkflowLoggingContext(engineContext) + "No demo mode value found for" + variableNameyActivity;
                logger.Info(msg);
            }
            if (varDemoRecipient != null && !string.IsNullOrWhiteSpace(varDemoRecipient.VarValue))
            {
                demoRecipient = varDemoRecipient.VarValue;
                demoRecipient = demoRecipient.Replace("\"", ""); // remove quotes, trying to be fehlertolerant
            }
            else
            {
                string defaultDemoModeRecipient;
                try
                {
                    defaultDemoModeRecipient = ConfigurationManager.AppSettings[settingsKeyDefault];
                }
                catch (ConfigurationErrorsException ex)
                {
                    defaultDemoModeRecipient = null;
                    logger.Error("Could not find application setting key for demo mode. Key =" + settingsKeyDefault, ex);
                }

                if (defaultDemoModeRecipient != null)
                    demoRecipient = defaultDemoModeRecipient;
                else
                {
                    demoRecipient = lastResourtValue;
                    logger.Warn(string.Format("Using last resourt value (='{0}') for demo mode value (='{1}')", lastResourtValue, variableNameyActivity));
                }
            }

            return demoRecipient;
        }

        public virtual Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName, bool nullable)
        {
            string ctx = "";
            ctx = DataHelper.BuildLogContextPrefix(engineContext.uniqueRunTimeID.ToString(), engineContext.WorkflowDefinitionName, engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance);

            string uniquePropertyName = engineContext.CurrenActivity.Nr + "." + propertyName;
            string unprocessedProp;

            string dataType;
            EnumVariableDirection varDirection;
            EnumVariablesDataType varDataType;


            //string xpathQuery = "/workflow/variables/variable[lower-case(@name)='" + uniquePropertyName.ToLower() + "']";
            string xpathQuery = "/workflow/variables/variable[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + uniquePropertyName.ToLower() + "']";
            XElement xx = engineContext.WorkflowModel.wfModel.XPathSelectElement(xpathQuery);
            if (xx != null)
            {
                engineContext.WorkflowModel.ExtraktVariableProperties(xx, out unprocessedProp, out uniquePropertyName, out varDirection, out varDataType);

                ExpressionEvaluator eva = new ExpressionEvaluator(engineContext.WorkflowModel);
                object returnValue = eva.Evaluate(unprocessedProp);

                if (returnValue != null)
                {
                    Variable newV = new Variable(uniquePropertyName,
                        returnValue.ToString(),
                        varDataType,
                        varDirection
                    );
                    return newV;
                }
                else
                {
                    string msg = "{0} Cannot evaluate Variable value with Name: '{1}' and Value: '{2}'.";
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, string.Format(msg, ctx, propertyName, unprocessedProp));
                }
            }
            else
            {
                if (nullable)
                {
                    return null;
                }
                else
                {
                    string msg = "{0} Cannot get Variable with Name: '{1}'; xpath expression was:'{2}'";
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, string.Format(msg, ctx, propertyName, xpathQuery));
                }
            }
        }

        public string Evaluate(string expression, EngineContext engineContext)
        {

            Ciloci.Flee.IDynamicExpression wf_Expresssion;
            ExpressionContext context = new ExpressionContext();

            //3.
            // check whether it is safe to avaluate ciloqi evaluator
            //A) 0.irgendwas
            //B) "hallo" + irgendwas
            //C) <d>aöld</asdk>
            bool evaluateFunctions = true;

            if (evaluateFunctions && (String.IsNullOrEmpty(expression.ToString().Trim())))
            {
                evaluateFunctions = false;
            }

            if (evaluateFunctions && expression.ToString().Contains("</"))
            {
                evaluateFunctions = false;
            }

            // second variation of empty xml tag
            if (evaluateFunctions && expression.ToString().Contains("/>"))
            {
                evaluateFunctions = false;
            }

            // i dont think we need this anymore -> jira rest call needs double quotes in string
            /*
            if (evaluateFunctions && expression.ToString().Contains("\\\""))
            {
                evaluateFunctions = false;
            }
            */

            if (evaluateFunctions)
            {
                wf_Expresssion = context.CompileDynamic(expression.ToString());
                expression = wf_Expresssion.Evaluate().ToString();
            }

            logger.Debug(string.Format("{0}GetProcessedActivityVariable calculated Result: ({1})",
                this.getWorkflowLoggingContext(engineContext), expression));

            return expression;
        }

        /// <summary>
        /// is this workflow instance in demo mode?
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public bool isDemoModeOn(EngineContext engineContext)
        {
            bool isProd = this.IsProduction();
            logger.Debug(string.Format("{0} settings IsProduction ='{1}'", this.getWorkflowLoggingContext(engineContext), isProd));
            if (isProd) //setting from ConfigFile
                return engineContext.WorkflowModel.IsDemoMode;
            else //defaults to demomode on of not production
            {
                return true;
            }
        }

        /// <summary>
        /// is this processengine installation productive?
        /// </summary>
        /// <returns></returns>
        public bool IsProduction()
        {
            bool result = false;
            string config = ConfigurationManager.AppSettings["ProcessEngine.IsProduction"];
            if (config == null)
                result = false;
            else
            {
                if (config.ToLowerInvariant().Equals("true"))
                    result = true;
                else
                    result = false;
            }
            return result;
        }

        /// <summary>
        /// gets a prefix for email subject, no setting will result in no prefix
        /// whitespace will be trimmed
        /// </summary>
        /// <returns></returns>
        public string GetEmailSubjectPrefix()
        {
            string prefix = "";
            try
            {
                prefix = ConfigurationManager.AppSettings[CONST_EMAILSUBJECTPREFIX];
            }
            catch
            {
            }

            if (prefix != null)
                return prefix.Trim();
            else
                return "";
        }

        /// <summary>
        /// get base dir for email templates
        /// throws exception
        /// </summary>
        /// <returns></returns>
        protected internal string getEmailTemplateBaseDir()
        {
            try
            {
                return ConfigurationManager.AppSettings["EmailTemplateBaseDir"].ToString();
            }
            catch (ConfigurationErrorsException ex)
            {
                logger.Error(ex.ToString(), ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "error reding email template basedir setting", ex);
            }
        }

        public StepReturn Execute(EngineContext engineContext)
        {

            //NotCompleted = 0, technically this state is not allowed and should always be replaced by error if happens.
            //Complete = 1,
            //Wait = 2,
            //ErrorStop = 3,
            //Timeout = 4,
            //Paused = 5,
            //ErrorToHandle = 6
            String logmsg = (this.getWorkflowLoggingContext(engineContext) + ".Initialize() resulted: {0} with state {1}");
            StepReturn resultInitialize;
            //
            // INITIALIZE
            //
            try
            {
                resultInitialize = this.Initialize(engineContext);
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE,
                    "Error running " + this.getWorkflowLoggingContext(engineContext) + ".Initialize()", ex);
            }
            if (resultInitialize.StepState == Shared.Enums.EnumStepState.Wait)
            {
                return resultInitialize;
            }
            if (resultInitialize.StepState != Shared.Enums.EnumStepState.Complete)
            {
                switch (resultInitialize.StepState)
                {
                    case Shared.Enums.EnumStepState.NotCompleted:
                        resultInitialize.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultInitialize.ReturnValue = "Not expected StepReturn 'NotCompleted' happened in Initialize()";
                        break;
                    case Shared.Enums.EnumStepState.ErrorStop:
                        break;
                    case Shared.Enums.EnumStepState.Timeout:
                        //TODO: handle Timeout with maximum timeout of Processengine, actually just set to paused.
                        resultInitialize.StepState = Shared.Enums.EnumStepState.Paused;
                        resultInitialize.ReturnValue = "Set to Paused since 'Timeout' was not handled in Initialize()";
                        break;
                    case Shared.Enums.EnumStepState.Paused:
                        break;
                    case Shared.Enums.EnumStepState.ErrorToHandle:
                        break;
                    default:
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Unexpected enumStepState " + resultInitialize.StepState.ToString() + "! Was EnumStepState extended without handling in BaseActivity?");
                }
                this.finishEngineAlert(engineContext);
                resultInitialize.ReturnValue = "Initialize: " + resultInitialize.ReturnValue + " ";
                this.StoreResult(resultInitialize, engineContext, isWarning: true);
                return resultInitialize;
            }

            //TODO : write stepReturn

            //
            // RUN
            //
            StepReturn resultRun;
            try
            {
                resultRun = this.Run(engineContext);
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN,
                    "Error running " + this.getWorkflowLoggingContext(engineContext) + ".Run()", ex);
            }
            logmsg = (engineContext.CurrenActivity.Instance + ".Run() resulted: {0} with state {1}");
            if (resultRun.StepState != Shared.Enums.EnumStepState.Complete)
            {
                switch (resultRun.StepState)
                {
                    case Shared.Enums.EnumStepState.NotCompleted:
                        resultRun.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultRun.ReturnValue = "Not expected StepReturn 'NotCompleted' happened in Run()";
                        break;
                    case Shared.Enums.EnumStepState.Wait:
                        resultRun.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultRun.ReturnValue = "Not expected StepReturn 'Wait' happened in Run()";
                        break;
                    case Shared.Enums.EnumStepState.ErrorStop:
                        break;
                    case Shared.Enums.EnumStepState.Timeout:
                        resultRun.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultRun.ReturnValue = "Not expected StepReturn 'Timeout' happened in Run()";
                        break;
                    case Shared.Enums.EnumStepState.Paused:
                        resultRun.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultRun.ReturnValue = "Not expected StepReturn 'Paused' happened in Run()";
                        break;
                    case Shared.Enums.EnumStepState.ErrorToHandle:
                        break;
                    default:
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Unexpected enumStepState " + resultRun.StepState.ToString() + "! Was EnumStepState extended without handling in BaseActivity?");
                }
                this.finishEngineAlert(engineContext);
                resultRun.ReturnValue = "Initialize: " + resultInitialize.ReturnValue + " Run: " + resultRun.ReturnValue + " ";
                this.StoreResult(resultRun, engineContext, isWarning: true);
                return resultRun;
            }
            //
            // FINISH
            //
            StepReturn resultFinish;
            try
            {
                resultFinish = this.Finish(engineContext);
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN,
                      "Error running " + this.getWorkflowLoggingContext(engineContext) + ".Run()", ex);
            }

            if (resultFinish.StepState != Shared.Enums.EnumStepState.Complete)
            {
                switch (resultFinish.StepState)
                {
                    case Shared.Enums.EnumStepState.NotCompleted:
                        resultFinish.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultFinish.ReturnValue = "Not expected StepReturn 'NotCompleted' happened in Finish()";
                        break;
                    case Shared.Enums.EnumStepState.Wait:
                        resultFinish.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultFinish.ReturnValue = "Not expected StepReturn 'Wait' happened in Finish()";
                        break;
                    case Shared.Enums.EnumStepState.ErrorStop:
                        break;
                    case Shared.Enums.EnumStepState.Timeout:
                        resultFinish.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultFinish.ReturnValue = "Not expected StepReturn 'Timeout' happened in Finish()";
                        break;
                    case Shared.Enums.EnumStepState.Paused:
                        resultFinish.StepState = Shared.Enums.EnumStepState.ErrorToHandle;
                        resultFinish.ReturnValue = "Not expected StepReturn 'Paused' happened in Finish()";
                        break;
                    case Shared.Enums.EnumStepState.ErrorToHandle:
                        break;
                    default:
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Unexpected enumStepState " + resultFinish.StepState.ToString() + "! Was EnumStepState extended without handling in BaseActivity?");
                }
                this.finishEngineAlert(engineContext);
                resultFinish.ReturnValue = "Initialize: " + resultInitialize.ReturnValue + " Run: " + resultRun.ReturnValue + " Finish: " + resultFinish.ReturnValue + " ";
                this.StoreResult(resultFinish, engineContext, isWarning: true);
                return resultFinish;
            }

            resultFinish.ReturnValue = "Initialize: " + resultInitialize.ReturnValue + " Run: " + resultRun.ReturnValue + " Finish: " + resultFinish.ReturnValue + " ";
            this.StoreResult(resultFinish, engineContext, isWarning: false);
            return resultFinish;
        }

        public virtual void StoreResult(StepReturn stepReturn, EngineContext engineContext, bool isWarning, string resultMsg = "")
        {
            string logMsg = "";
            if (string.IsNullOrWhiteSpace(resultMsg))
            {
                logMsg = string.Format("{0} resulted: {1} with state: {2}", 
                    engineContext.CurrenActivity.Instance, stepReturn.ReturnValue, stepReturn.StepState.ToString());
            }
            else
            {
                logMsg = resultMsg;
            }

            if (isWarning)
                logger.Warn(String.Format(logMsg, stepReturn.ReturnValue, stepReturn.StepState.ToString()));
            else
                logger.Info(String.Format(logMsg, stepReturn.ReturnValue, stepReturn.StepState.ToString()));

            DatabaseAccess db = new DatabaseAccess();
            db.SaveActivityResultMessageToWorkflowInstance(engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance, logMsg);

        }

        /// <summary>
        /// Set AlertStatus in DB to Finish
        /// </summary>
        /// <param name="engineContext"></param>
        public void finishEngineAlert(EngineContext engineContext)
        {
            DatabaseAccess db = new DatabaseAccess();
            db.FinishEngineAlert(engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID);
        }

        /// <summary>
        /// is called first when an activity was instanciated. Here we do 
        /// - read input variables 
        /// - set equivalent internal properties
        /// - do polling when asynchronous
        /// Allowed Stepstates are correctly handled in BaseActivity
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public abstract StepReturn Initialize(EngineContext engineContext);

        /// <summary>
        /// is called after Initialization was done.
        /// - does complete business-logic of the activity
        /// - do NOT read input data from anywhere
        /// - do NOT set output vars.
        /// Allowed Stepstates are correctly handled in BaseActivity
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public abstract StepReturn Run(EngineContext engineContext);

        /// <summary>
        /// is called after Activvity ran.
        /// - do write Output Vars here.
        /// Allowed Stepstates are correctly handled in BaseActivity
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public abstract StepReturn Finish(EngineContext engineContext);

        /// <summary>
        /// constructor to create the activity.
        /// </summary>
        /// <param name="type"></param>
        public BaseActivity(Type type)
        {
            this.logger = EDPLogger.GetLogger(type);
        }

        /// <summary>
        /// empty Constructor for DLLCacheHandler. DO NOT CALL!
        /// </summary>        
        public BaseActivity()
        {
            this.logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        /// get all 0 punkt variable and add to a dictionary. this can be used in document templates (emails)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="renderDictionary"></param>
        /// <returns></returns>
        public Dictionary<string, string> AddWorkflowZeroVariablesToRenderDictionary(EngineContext engineContext, Dictionary<string, string> renderDictionary)
        {
            Variable tempVariable;
            var nullPunkts = (List<Variable>)engineContext.WorkflowModel.GetPunktVariables("[starts-with(@name,'0.')]");
            foreach (var nullVariable in nullPunkts)
            {
                tempVariable = engineContext.GetWorkflowVariable(nullVariable.Name);
                renderDictionary.Add(nullVariable.Name, nullVariable.VarValue);
            }
            return renderDictionary;
        }
    }
}
