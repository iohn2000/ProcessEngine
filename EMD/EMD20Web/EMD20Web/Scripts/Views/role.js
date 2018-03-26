role = {};

role.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                role.Communication.Delete(guid);
            },
            null
        );
    }


};

role.Events = {

    OnBlurRoleId: function (htmlElement) {
        var inputText = $(htmlElement).val();


        console.log(inputText);
    },

    OnButtonNextRoleIdClick: function (htmlElement) {
        var inputText = $('#R_ID').val();

        role.Communication.GetNextRoleId(htmlElement, parseInt(inputText));
    }
};

role.Communication = {

    GetNextRoleId: function (htmlElement, roleId) {
        kendoHelper.ShowProgress($(htmlElement).attr('id'));
        var data = { requestRoleId: roleId };

        $.ajax({

            type: "POST",
            url: "/Role/GetNextRoleId",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress($(htmlElement).attr('id'));
                if (response.success) {
                    $('#R_ID').val(response.nextRoleId.toString());
                    alertify.success(response.message);
                }
                else {
                    alertify.alert(res.Role.Alert.Title_Error, response.errorModel.ErrorMessage);

                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress($(htmlElement).attr('id'));
                if (t === "timeout") {
                    alertify.alert(res.Role.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Role.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },

    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/Role/DeleteRole",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.Role.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.Role.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Role.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Role.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};