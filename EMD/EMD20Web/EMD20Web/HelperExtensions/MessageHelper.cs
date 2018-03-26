using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using System;
using System.Collections;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Kapsch.IS.EMD.EMD20Web.HelperExtensions
{
    /// <summary>
    /// Helper class to generalize Userinterface messages
    /// </summary>
    public class MessageHelper
    {
        public static string GetStackTrace(Exception ex)
        {
            return ex.StackTrace.Trim();
        }


        public static string GetMessage(Exception ex)
        {
            return string.Format("A general error occured!<div class='alert-detail-message'><div>Technical Message:</div><div>{0}</div></div>", ex.Message);
        }

        public static string GetMessage(FaultException<FcPermissionException> ex)
        {
            if (ex.Detail.IsCheckedOutByAnotherUser)
            {
                return "The workflow is checked out by another user";
            }

            if (ex.Detail.IsNotCheckedOut)
            {
                return "You can't edit a workflow, which is not checked out.";
            }

            return "There was a general problem with a permission access!";

        }


        public static string GetMessage(FaultException<FcWorkflowException> ex)
        {
            StringBuilder builder = new StringBuilder();

            WorkflowErrorType errorTypeEnum = (WorkflowErrorType)ex.Detail.ErrorType;

            switch (errorTypeEnum)
            {
                case WorkflowErrorType.NotWellformed:
                    builder.Append("The XML is not well formed!");
                    break;
                case WorkflowErrorType.WorkflowNotFound:
                    builder.Append("There was no workflow found!");
                    break;
                case WorkflowErrorType.RuleViolation:
                    builder.Append("There was a rule violation!");
                    break;
                case WorkflowErrorType.WorkflowNotCheckedOut:
                    builder.Append("There workflow was not checked out!");
                    break;
                default:
                    break;
            }
            builder.Append("<br>");

            if (ex.Detail.ErrorItems != null)
            {
                builder.Append("Following errors were found:<br>");
                builder.Append("<ul>");
                foreach (WorkflowErrorItem workflowErrorItem in ex.Detail.ErrorItems.ToList())
                {
                    builder.Append(string.Format("<li>In linenumber {0} at position {1} there is an error of: {2}</li>", workflowErrorItem.LineNumber, workflowErrorItem.LinePosition, workflowErrorItem.Message));
                }
                builder.Append("</ul>");
            }

            return builder.ToString();

        }

        public static string GetMessage(RelatedEntitiesException ex)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<div>The item couldn't be deleted, because it has following dependencies:</div><br>");


            if (ex.RelatedEntities != null)
            {
                foreach (DictionaryEntry pair in ex.RelatedEntities)
                {
                    stringBuilder.Append(string.Format("<div>{0} has {1} entries</div>", pair.Key, pair.Value));
                }
            }

            return stringBuilder.ToString();
        }

        
       
    }
}