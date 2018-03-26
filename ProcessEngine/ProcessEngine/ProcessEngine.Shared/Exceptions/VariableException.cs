namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{
    public enum VariableErrorType
    {
        /// <summary>
        /// All Properites must have a matching Variable
        /// </summary>
        PropertyNotImplementedAsVariable,
        /// <summary>
        /// The ID of a custom variable is defined with a prefix of 0
        /// </summary>
        CustomVariableMustBeZero,
        /// <summary>
        /// The value of the variable must follow schema
        /// </summary>
        SyntaxError,
        /// <summary>
        /// All variables must have a value
        /// </summary>
        Empty
    }


    public class VariableException : BaseWorkflowException
    {
        public string Id { get; set; }

        public VariableErrorType ErrorType { get; set; }
    }
}
