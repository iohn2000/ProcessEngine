using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using System;

namespace Kapsch.IS.EDP.WFActivity.StartTimed
{
    public class StartTimedActivity : BaseEDPActivity
    {
        private Variable StartDate;
        private bool IsExecuted;
        private DateTime? lastPollingDate;
        private int OffsetInDays;

        public StartTimedActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            
            logger.Debug(base.getWorkflowLoggingContext(engineContext) + " started StartTimedActivity.Initialze()");
            StepReturn result = new StepReturn("", EnumStepState.Complete);
            try
            {
                try
                {
                    this.StartDate = base.GetProcessedActivityVariable(engineContext, "startDate");                    
                }
                catch (Exception ex)
                {                    
                    return logErrorAndReturnStepState(engineContext, ex, "failed to read startDate", EnumStepState.ErrorToHandle);
                }

                try
                {
                    var tmp = base.GetProcessedActivityVariable(engineContext, "offsetInDays", true);
                    if (tmp != null && !string.IsNullOrWhiteSpace(tmp.VarValue))
                    {
                        if (!int.TryParse(tmp.VarValue, out this.OffsetInDays))
                        {
                            //an invalid value has been entered
                            String msg = "could not parse offsetInDays; wrong format." + this.StartDate.VarValue;
                            return logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                        }
                    }
                    else
                        this.OffsetInDays = 0;
                }
                catch (Exception)
                {
                    // no value given so assume 0 hours
                    this.OffsetInDays = 0;
                }

                Variable executed = base.GetProcessedActivityVariable(engineContext, "isExecuted");
                if (executed != null && executed.VarValue.ToLowerInvariant() == "true")
                    this.IsExecuted = true;
                else
                    this.IsExecuted = false;

                if (this.StartDate != null)
                {
                    if (this.StartDate.VarValue.ToLowerInvariant().StartsWith("now+")) // format now+xx minutes, now+2
                    {
                        try
                        {
                            int mins = int.Parse(this.StartDate.VarValue.Substring(this.StartDate.VarValue.IndexOf("+") + 1));
                            this.lastPollingDate = DateTime.Now.AddHours(mins);
                        }
                        catch (Exception)
                        {

                            String msg = "could not parse startDate; wrong 'now+' format." + this.StartDate.VarValue;
                            return logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                        }
                    }
                    else
                    {
                        if (! string.IsNullOrWhiteSpace(this.StartDate.VarValue))
                        {
                            try
                            {
                                this.lastPollingDate = this.StartDate.GetDateValue();
                                if (this.lastPollingDate == null)
                                {
                                    //convert didnt work
                                    String msg = "could not parse startDate" + this.StartDate.VarValue;
                                    return logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                                }
                            }
                            catch (Exception)
                            {
                                //convert didnt work
                                String msg = "could not parse startDate" + this.StartDate.VarValue;
                                return logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                            }
                        }
                        else
                        {
                            return logErrorAndReturnStepState(engineContext, null, "failed to read startDate", EnumStepState.ErrorToHandle);
                        }

                    }
                }
            }
            catch (BaseException bEx)
            {
                logger.Warn("got no startDate or isExecuted Variable in WorkflowConfig");
                result.StepState = EnumStepState.ErrorToHandle;
                result.ReturnValue = bEx.Message;
                return result;
            }
            catch (Exception exc)
            {
                String msg = "could not read startDate  or isExecuted from WorkflowConfig";
                return logErrorAndReturnStepState(engineContext, exc, msg, EnumStepState.ErrorToHandle);
            }

            DatabaseAccess db = new DatabaseAccess();
            
            if (this.IsExecuted)
            {
                db.FinishEngineAlert(engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID);
                result = new StepReturn("", EnumStepState.Complete);
                
            }
            else
            {
                db.CreateEngineAlert(engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID,
                                        "", "",
                                        EnumAlertTypes.Polling, 0,
                                        this.lastPollingDate.Value.AddDays(this.OffsetInDays));

                result = new StepReturn("", EnumStepState.Wait);
                engineContext.SetActivityVariable("isExecuted", "true", true);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            logger.Debug(base.getWorkflowLoggingContext(engineContext) + " started StartTimedActivity.Run()");
            StepReturn result = new StepReturn("", EnumStepState.Complete);
            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("", EnumStepState.Complete);
            return ret;
        }

    }
}
