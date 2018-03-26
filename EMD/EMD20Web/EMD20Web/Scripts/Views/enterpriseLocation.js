enterpriseLocation = {};

enterpriseLocation.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                enterpriseLocation.Communication.Delete(guid);
            },
            null
        );
    }
};

enterpriseLocation.Events = {
    OnEnterpriseClick: function (enterpriseName) {
        var grid = $('#Grid').data("kendoGrid");
        grid.dataSource.filter({ field: "EnterpriseNameEnhanced", operator: "contains", value: enterpriseName });
    },
    OnLocationClick: function (locationName) {
        var grid = $('#Grid').data("kendoGrid");
        grid.dataSource.filter({ field: "LocationNameEnhanced", operator: "contains", value: locationName });
    }
}


enterpriseLocation.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/EnterpriseLocation/DeleteEnterpriseLocation",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.EnterpriseLocation.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.EnterpriseLocation.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.EnterpriseLocation.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.EnterpriseLocation.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};