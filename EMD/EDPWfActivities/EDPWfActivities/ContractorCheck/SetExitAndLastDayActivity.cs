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

namespace Kapsch.IS.EDP.WFActivity.ContractorCheck
{
    /// <summary>
    ///   Set Exit And LastDay
    ///       Set the exit and last day for employment checks (e.g. contractor check wf)
    /// </description>
    /// <properties>
    ///     <property name = "EmplGuid"  direction="input"  dataType="stringType" ></property>
    ///     <property name = "Exit"  direction="input"  dataType="stringType" ></property>
    ///     <property name = "LastDay"  direction="input"  dataType="stringType" ></property>
    /// </properties>
    /// </summary>
    public class SetExitAndLastDayActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {

        public string modifyComment = "modified by SetExitAndLastDayActivity requested by sponsor";
        private EMDEmployment Employment;
        private DateTime Exit = DateTime.Now;
        private DateTime? LastDay = null;

        public const string VAR_EmplGuid = "EmplGuid";
        public const string VAR_Exit = "Exit";
        public const string VAR_LastDay = "LastDay";

        public SetExitAndLastDayActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);
            string emplGuid = "";
            try
            {
                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_EmplGuid, false);
                emplGuid = tmp.VarValue;

                EmploymentHandler emplH = new EmploymentHandler(null, modifyComment);
                this.Employment = (EMDEmployment)emplH.GetObject<EMDEmployment>(emplGuid);

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_Exit, false);
                string timeTmp = tmp.VarValue;
                if (timeTmp.Trim().Equals("_NOW"))
                {
                    this.Exit = DateTime.Now;
                }
                else if (timeTmp.Trim().Equals("_FIRSTOFNEXTMONTH"))
                {
                    //TODO: auslagern in WF Helper.
                    DateTime dayone = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1); 
                    this.Exit = dayone;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(tmp.VarValue))
                    {
                        this.Exit = tmp.GetDateValue().Value;
                    }
                    else
                    {
                        logger.Error(base.getWorkflowLoggingContext(engineContext) + " Error parsing Exit date: " + tmp.VarValue ?? "" + " continue without Error2Handle");
                    }
                }

                tmp = base.GetProcessedActivityVariable(engineContext, VAR_LastDay, true);
                if (tmp != null) {  
                    if (timeTmp.Trim().Equals("_NOW"))
                    {
                        this.LastDay = DateTime.Now;
                    }
                    else if (timeTmp.Trim().Equals("_FIRSTOFNEXTMONTH"))
                    {
                        //TODO: auslagern in WF Helper.
                        DateTime dayone = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1);
                        this.LastDay = dayone;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(tmp.VarValue))
                        {
                            this.LastDay = tmp.GetDateValue();
                        }
                        else
                        {
                            logger.Error(base.getWorkflowLoggingContext(engineContext) + " Error parsing LastDay date: " + tmp.VarValue ?? "" + " continue without Error2Handle");
                        }
                    }
                }
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

            return result;

        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);

            //store empl to EDP
            EmploymentHandler emplH = new EmploymentHandler(null, this.modifyComment);           
            this.Employment.SetExitAndLastDay(this.Exit, this.LastDay);
            emplH.UpdateObject<EMDEmployment>(this.Employment);

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            // setup return object 
            StepReturn result = new StepReturn("Successfully set EmploymentDates for Exit", EnumStepState.Complete);
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
