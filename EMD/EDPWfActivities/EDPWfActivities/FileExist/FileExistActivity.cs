using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.Email;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.TemplateEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.FileExist
{
    public class FileExistActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        //public const string VAR_FILENAME = "fileName";
        //string fileName = string.Empty;

        public FileExistActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("", EnumStepState.Complete);

            //try
            //{
            //    fileName = base.GetProcessedActivityVariable(engineContext, VAR_FILENAME, false).VarValue;
            //}
            //catch (BaseException bEx)
            //{
            //    string msg = "error trying to get Workflow-variables";
            //    return this.logErrorAndReturnStepState(engineContext, bEx, msg + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            //}
            //catch (Exception ex)
            //{
            //    string msg = "error trying to get Workflow-variables";
            //    return this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            //}

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("", EnumStepState.Complete);

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("", EnumStepState.Complete);

            return result;
        }

        #region validation
        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
