using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Xml.Linq;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.ProcessEngine.WFActivity.AuditLog
{
    public class AuditLogActivity : BaseActivity, IProcessStep, IActivityValidator
    {
        public Variable LogMessage = null;
        public Variable LogLevel = null;
       

        public AuditLogActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
            
        }


        public override StepReturn Initialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                LogMessage = this.GetProcessedActivityVariable(engineContext, "AuditLogMessage", false);
                LogLevel = this.GetProcessedActivityVariable(engineContext, "AuditLogLevel", false);
            }
            catch (BaseException)
            {
                logger.Warn("got no AuditLogMessage Variable in WorkflowConfig");
                return result;
            }
            catch (Exception exc)
            {
                String msg = "could not read AuditLogMessage from WorkflowConfig";
                return logErrorAndReturnStepState(engineContext, exc, msg, EnumStepState.Complete);
            }
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            string msg = "";
            if (LogMessage != null)
            {
                try
                {
                    msg = LogMessage.VarValue;
                    WebService.AuditLogServiceClient auditLogService = new WebService.AuditLogServiceClient();
                    auditLogService.WriteEntry(LogLevel.GetIntValue(), "EDP-ProcessEngine", null, 
                        engineContext.CurrenActivity.Name, msg, engineContext.WorkflowModel.InstanceID);
                }
                catch (Exception ex)
                {
                    // do nothing although not reachable
                    logger.Error("Could not create Audit Log Entry: ", ex);
                }
            }

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
             return result;
        }


        public string Validate(XElement activity)
        {
            return "";
        }
        public string Validate(string activityXml)
        {
            return this.Validate(XElement.Parse(activityXml));
        }
    }
}
