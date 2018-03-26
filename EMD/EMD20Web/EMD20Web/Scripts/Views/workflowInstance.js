workflowInstance = {}

workflowInstance.Functions = {
    Start: function (idWorkflowInstance, entityName) {
        alertify.confirm(res.WorkflowInstance.Confirmation.Title_Rerun, res.GetRes(res.WorkflowInstance.Confirmation.Detail_Rerun, [entityName]),
            function () {
                workflowInstance.Communication.Start(idWorkflowInstance);
            },
            null
        );
    },


   

}

workflowInstance.Communication = {
    Start: function (idWorkflowInstance) {
        kendoHelper.ShowProgress();
        var data = { idWorkflowInstance: idWorkflowInstance };

        $.ajax({

            type: "POST",
            url: "/Workflow/StartWorkflowInstance",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.WorkflowInstance.Alert.Rerun_Success);
                }
                else {
                    alertify.alert(res.WorkflowInstance.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.WorkflowInstance.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.WorkflowInstance.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
}