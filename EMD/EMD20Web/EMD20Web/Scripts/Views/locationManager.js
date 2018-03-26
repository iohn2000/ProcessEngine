locationManager = {};

locationManager.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                locationManager.Communication.Delete(guid);
            },
            null
        );
    }
};

locationManager.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/Location/DeleteLocation",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.Location.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.Location.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Location.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Location.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};