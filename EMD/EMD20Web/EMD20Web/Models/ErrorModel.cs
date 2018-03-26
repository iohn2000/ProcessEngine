using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.ServiceModel;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class ErrorModel
    {
        //public Exception ErrorException { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public int KapschErrorNumber { get; set; }
        public string Source { get; set; }
        public string KapschErrorType { get; set; }

        public ErrorModel()
        {
            //TODO who calls that??        
        }

        public ErrorModel(Exception ex, string message = null)
        {
            this.ErrorMessage = ex.Message;
            if (message != null)
            {
                this.ErrorMessage = string.Format("{0} ({1})", message, ex.Message);
            }

            if (ex is FaultException<FcPermissionException>)
            {
                this.ErrorMessage = MessageHelper.GetMessage(ex as FaultException<FcPermissionException>);
            }

            if (ex is FaultException<FcWorkflowException>)
            {
                this.ErrorMessage = MessageHelper.GetMessage(ex as FaultException<FcWorkflowException>);
            }

            if (ex is RelatedEntitiesException)
            {
                this.ErrorMessage = MessageHelper.GetMessage(ex as RelatedEntitiesException);
            }
            if (ex.StackTrace != null)
            {
                this.StackTrace = ex.StackTrace.Trim();
            }
            this.Source = ex.Source;

            if (ex.GetType().ToString() == "Kapsch.IS.Util.ErrorHandling.BaseException")
            {
                BaseException baseEx = (BaseException)ex;
                this.KapschErrorNumber = baseEx.ErrorCode;
                this.KapschErrorType = ErrorCodeHandler.GetMessage(baseEx.ErrorCode);
            }
            else
            {
                this.KapschErrorNumber = -1;
                this.KapschErrorType = "UNKNOWN";
            }
        }

        public ErrorModel(BaseException ex)
        {
            this.ErrorMessage = ex.Message;
            this.StackTrace = ex.StackTrace;
            this.Source = ex.Source;
            this.KapschErrorNumber = ex.ErrorCode;
            this.KapschErrorType = ErrorCodeHandler.GetMessage(ex.ErrorCode);
        }

    }
}