using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.ProcessEngine.Webservice.Entities;
using Kapsch.IS.ProcessEngine.Webservice.FaultContracts;
using System;
using System.Collections.Generic;
using System.ServiceModel;


namespace Kapsch.IS.ProcessEngine.Webservice
{
    /// <summary>
    /// Summary of methods functions which are used frequently
    /// </summary>
    public static class Helper
    {
        public static WorkflowItem Map(WFEWorkflowDefinition workflowDefinition, int numberOfProcesses)
        {
            return new WorkflowItem()
            {
                Id = workflowDefinition.WFD_ID.ToString(),
                Definition = workflowDefinition.WFD_Definition,
                CheckedOutBy = workflowDefinition.WFD_CheckedOutBy,
                Description = workflowDefinition.WFD_Description,
                ActiveProcesses = numberOfProcesses,
                Name = workflowDefinition.WFD_Name,
                Version = workflowDefinition.WFD_Version.ToString(),
                Created = workflowDefinition.WFD_Created,
                ValidFrom = workflowDefinition.WFD_ValidFrom.HasValue ? workflowDefinition.WFD_ValidFrom.Value : DateTime.Now,
                ValidTo = workflowDefinition.WFD_ValidTo
            };
        }

        public static List<ActivityItem> Map(List<WFEActivityDefinition> activityDefinitions)
        {
            List<ActivityItem> activityItems = new List<ActivityItem>();

            foreach (WFEActivityDefinition activitiyDefinition in activityDefinitions)
            {
                activityItems.Add(new ActivityItem() { Id = activitiyDefinition.WFAD_ID, Name = activitiyDefinition.WFAD_Name, ActivityType = activitiyDefinition.WFAD_Type });
            }


            return activityItems;
        }


        public static FaultException<FcWorkflowException> Map(WorkflowException workflowException)
        {
            List<WorkflowErrorItem> workflowErrorItems = new List<WorkflowErrorItem>();

            if (workflowException.errorMessages != null)
            {
                foreach (WorkflowError workflowError in workflowException.errorMessages)
                {
                    workflowErrorItems.Add(new WorkflowErrorItem(workflowError.Message, workflowError.LineNumber, workflowError.LinePosition));
                }
            }


            FaultException<FcWorkflowException> fcWorkflowExeption = new FaultException<FcWorkflowException>(new FcWorkflowException()
            {
                ErrorType = (int)workflowException.ErrorType,
                LineNumberStart = workflowException.LineNumberStart,
                LineNumberEnd = workflowException.LineNumberEnd,
                ErrorItems = workflowErrorItems
            }
            , new FaultReason(workflowException.Message));


            return fcWorkflowExeption;
        }

        public static FaultException<FcActivityException> Map(ActivityException activityException)
        {
            FaultException<FcActivityException> fcActivityExeption = new FaultException<FcActivityException>(new FcActivityException()
            {
                ErrorType = (int)activityException.ErrorType,
                LineNumberStart = activityException.LineNumberStart,
                LineNumberEnd = activityException.LineNumberEnd
            }
            , new FaultReason(activityException.Message));


            return fcActivityExeption;
        }

        public static FaultException<FcVariableException> Map(VariableException activityException)
        {
            FaultException<FcVariableException> fcWorkflowExeption = new FaultException<FcVariableException>(new FcVariableException()
            {
                ErrorType = (int)activityException.ErrorType,
                LineNumberStart = activityException.LineNumberStart,
                LineNumberEnd = activityException.LineNumberEnd
            }
            , new FaultReason(activityException.Message));


            return fcWorkflowExeption;
        }

        public static FaultException<FcPermissionException> Map(PermissionException activityException)
        {
            FaultException<FcPermissionException> fcPermissionExeption = new FaultException<FcPermissionException>(new FcPermissionException()
            {
                IsCheckedOutByAnotherUser = activityException.IsCheckedOutByAnotherUser,
                IsNotCheckedOut = activityException.IsNotCheckedOut
            }
            , new FaultReason(activityException.Message));


            return fcPermissionExeption;
        }

        public static FaultException<FcDefaultException> Map(DefaultException activityException)
        {
            FaultException<FcDefaultException> fcPermissionExeption = new FaultException<FcDefaultException>(new FcDefaultException()
            {
                ErrorType = (int)activityException.ErrorType
            }
            , new FaultReason(activityException.Message));


            return fcPermissionExeption;
        }

        public static FaultException Map(Exception exception)
        {
            FaultException faultException = new FaultException(new FaultReason(exception.Message));

            return faultException;
        }
    }
}