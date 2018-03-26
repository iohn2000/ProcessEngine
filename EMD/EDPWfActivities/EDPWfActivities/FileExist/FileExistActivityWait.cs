using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.IO;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.FileExist
{
    public class FileExistActivityWait : BaseEDPAsyncActivity, IActivityValidator, IProcessStep
    {
        public string fileName = "waitforme.txt";
        public FileExistActivityWait() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override bool isResultAvailable(string AsyncRequestID, out BaseAsyncRequestResult AsyncRequestResult)
        {
            AsyncRequestResult = null;
            bool fileExists = File.Exists(fileName);
            return fileExists;
        }

        public override StepReturn PostInitialize(EngineContext engineContext, BaseAsyncRequestResult baseResult)
        {
            // if this is called isResultAvailable was true (file was found)
            // --> return stepstate complete

            StepReturn result = new StepReturn("file found", EnumStepState.Complete);
            return result;
        }

        public override StepReturn HandleReminder(EngineContext engineContext, BaseAsyncRequestResult baseResult, bool resultAvailable)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("file found", EnumStepState.Complete);
            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("file found", EnumStepState.Complete);
            return result;
        }

        #region validate
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
