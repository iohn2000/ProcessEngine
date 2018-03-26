using System.Runtime.Serialization;

namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{
    public enum ActivityErrorType
    {
        GeneralException,
        ActivityNotFound,
        RuleViolation,
        TaskLinkError
    }
    
    public class ActivityException : BaseWorkflowException
    {

        public string IdActivityInstance { get; set; }

        [DataMember]
        public ActivityErrorType ErrorType { get; set; }
    }
}
