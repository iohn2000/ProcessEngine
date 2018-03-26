using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.WF
{
    public class WorkflowCallback
    {
        /// <summary>
        /// WOIN of the Workflow Instance
        /// </summary>
        public string WorkflowInstanceID { get; private set; }
        /// <summary>
        /// ID of the entity change for this callback 
        /// </summary>
        public string ClientReferenceID { get; private set; }
        /// <summary>
        /// Result as text for this callback
        /// </summary>
        public String CallbackResult { get; private set; }
        /// <summary>
        /// Status as text for this callback
        /// </summary>
        public String CallbackStatus { get; private set; }

        /// <summary>
        /// Sending the WorklflowCallback, if no workflowInstanceId is given we try to assume it from clientreference.
        /// </summary>
        public void Do()
        {
            if ( ClientReferenceID != null && CallbackResult != null && CallbackStatus != null)
            {
                try
                {
                    new ProcessServiceClient().DoCallback(
                                this.WorkflowInstanceID,
                                this.ClientReferenceID,
                                this.CallbackResult,
                                this.CallbackStatus);
                }
                catch (Exception exc)
                {
                    throw new WorkflowCallbackException("Exception thrown by ProcessService wenn trying to create the callback",exc);
                }
            } else
            {
                throw new WorkflowCallbackException("Can't send WorkflowCallback with missing data. One of the parameters was null");
            } 
        }

        /// <summary>
        /// Standard Constructr for the WorkflowCallback
        /// </summary>
        /// <param name="workflowInstanceID"></param>
        /// <param name="clientReferenceID"></param>
        /// <param name="callbackResult"></param>
        /// <param name="callbackStatus"></param>
        public WorkflowCallback(string workflowInstanceID, string clientReferenceID, string callbackResult, string callbackStatus)
        {
            WorkflowInstanceID = workflowInstanceID ?? throw new ArgumentNullException(nameof(workflowInstanceID));
            ClientReferenceID = clientReferenceID ?? throw new ArgumentNullException(nameof(clientReferenceID));
            CallbackResult = callbackResult ?? throw new ArgumentNullException(nameof(callbackResult));
            CallbackStatus = callbackStatus ?? throw new ArgumentNullException(nameof(callbackStatus));
        }
    }
}
