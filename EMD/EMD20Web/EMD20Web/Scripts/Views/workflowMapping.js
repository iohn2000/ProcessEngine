workflowMapping = {};


workflowMapping.Entities = {
    SelectedMappingPrefix: null
}

workflowMapping.Events = {
    OnAvailableEntitiesSelect: function (e) {
        // use index + 1, because option label is the first index
        workflowMapping.Entities.SelectedMappingPrefix = this.dataItem(e.item).EntityPrefix;

        $("#Method").data("kendoDropDownList").dataSource.read();
        workflowMapping.Communication.GetHasMappingEntities(workflowMapping.Entities.SelectedMappingPrefix);
    },

    OnAvailableEntitiesDataBound: function (e) {
        workflowMapping.Entities.SelectedMappingPrefix = $("#PrefixUI").data("kendoDropDownList").value();
        workflowMapping.Communication.GetHasMappingEntities(workflowMapping.Entities.SelectedMappingPrefix);
    }
}

workflowMapping.Functions = {
    ReadMappingEntityParameter: function (e) {

        return {
            prefix: workflowMapping.Entities.SelectedMappingPrefix
        }
    },

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                workflowMapping.Communication.Delete(guid);
            },
            null
        );
    }
}

workflowMapping.Communication = {
    GetHasMappingEntities: function (prefix) {

        var data = { prefix: prefix };

        $.ajax({

            type: "POST",
            url: "HasMappingEntities",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {

                if (response.success) {

                    if (response.hasEntities) {

                        //    $('#FormGroupMappingEntities').show();
                        $('#FormGroupMappingEntities input').removeAttr('disabled');

                        $('#FormGroupMappingEntities').attr("style", "display:block");

                        //   $("#EntityMappingGuid").closest(".k-widget").show();
                        $("#EntityMappingGuid").data("kendoDropDownList").dataSource.read();

                    }
                    else {
                        $('#FormGroupMappingEntities').attr("style", "display:none");
                        $('#FormGroupMappingEntities input').attr('disabled', 'disabled');
                        // $("#EntityMappingGuid").closest(".k-widget").hide();
                        //  $('#FormGroupMappingEntities').hide();



                    }

                }
                else {
                    alertify.alert(res.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }

            },
            error: function (x, t, m) {
                console.log('Error in calling ServerMethod HasMappingEntities');
            },
            timeout: 15000
        });
    },
    Delete: function (guid, gridId) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/Workflow/DeleteWorkflowMapping",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh(gridId);
                    alertify.success(res.WorkflowMapping.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.WorkflowMapping.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.WorkflowMapping.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.WorkflowMapping.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
}