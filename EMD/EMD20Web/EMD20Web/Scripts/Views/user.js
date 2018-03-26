user = {};

user.Entities = {
    IsNew: false,
    EmploymentGuid: null
}

user.Functions = {

    Delete: function (guid, entityName, username, gridId) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, username]),
            function () {
                //alert('delete ' + guid + ' called');

                user.Communication.DeleteUser(guid, gridId);
            },
            null
        );
    }
};

user.Events = {
    OnUserTypeChanged: function (e) {
        if (user.Entities.IsNew) {
            if (this.dataItem) {
                var usertype = this.dataItem(e.item);

                if (usertype.Type !== 70) {
                    $('#SubmitButton').attr('disabled', true);
                    $('#Username').attr('readonly', 'readonly');
                    user.Communication.RenderUserNameForEmployment(user.Entities.EmploymentGuid);
                }
                else {
                    $('#Username').removeAttr('readonly');
                }
            }
        }
    },    

    OnUserEditSuccess: function (xhr) {
        // kendoHelper.RefreshWindow(true);
        if (xhr.responseJSON.newUserName && xhr.responseJSON.newUserName.length > 0) {
            $('#personprofile-username').html(xhr.responseJSON.newUserName);
        }
        formValidation.OnSuccess(xhr, xhr.responseJSON.idUpdateGrid);
    }
};

user.Communication = {
    RenderUserNameForEmployment: function (emplGuid) {
        var data = { emplGuid: emplGuid };

        $.ajax({

            type: "POST",
            url: "/User/GetUserNameForEmployment",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    $('#Username').val(response.userName)
                    $('#SubmitButton').removeAttr('disabled');
                }

            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.User.Alert.GetUsername_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.User.Alert.GetUsername_Error, res.Alert.GeneralError);
                }
            },
            timeout: 10000
        });
    },
    DeleteUser: function (guid, gridId) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/User/DeleteUser",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh(gridId);
                    alertify.success(res.User.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.User.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.User.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.User.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },
    LogoutImpersonatedUser: function () {
        kendoHelper.ShowProgress();

        $.ajax({

            type: "POST",
            url: "/User/LogoutImpersonatedUser",
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    var url = window.location.href.toLowerCase();
                    var indexUsername = url.indexOf('user');
                    if (indexUsername >= 0) {
                        url = url.substring(0, indexUsername);
                    }

                    window.location = url;
                }
                else {
                    alertify.alert(res.User.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.User.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.User.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },
     DeleteCache: function () {
        kendoHelper.ShowProgress();

        $.ajax({

            type: "POST",
            url: "/User/DeleteCache",
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    var url = window.location.href.toLowerCase();
                 
                    window.location = url;
                }
                else {
                    alertify.alert(res.User.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.User.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.User.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};