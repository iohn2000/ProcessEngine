using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.ProcessEngine.WFActivity.XORGateway
{
    public class XORGatewayActivity : SimpleBaseActivity, IProcessStep, IActivityValidator
    {

        public XORGatewayActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }
        public override StepReturn Run(EngineContext engineContext)
        {
            // simply return complete to move to next activity 
            StepReturn ret = new StepReturn("", EnumStepState.Complete);

            try
            {
                string next = "";
                var outTrans = engineContext.CurrenActivity.GetOutgoingTransistions();
                if (outTrans != null)
                {
                    foreach (var outgoing in outTrans)
                    {
                        if (outgoing != null)
                        {
                            var target = outgoing.GetTargetActivity();
                            if (target != null)
                            {
                                next += target.Instance + ", ";
                            }
                        }
                    } 
                }
                string logLine = string.Format("{0} : XOR Gateway '{1}' ---> '{2}' , Next =[ {3} ]",
                   base.getWorkflowLoggingContext(engineContext),
                   engineContext.CurrentTransistion.FromActivityID,
                   engineContext.CurrentTransistion.ToActivityID,
                   next
                   );

                logger.Info(logLine);
                ret.ReturnValue = logLine;
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in XOR Activity", ex);
                ret.ReturnValue = "XOR Activity Error";
                ret.StepState = EnumStepState.ErrorToHandle;
            }
            return ret;
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
