orgunitrole = {};

orgunitrole.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                orgunitrole.Communication.Delete(guid);
            },
            null
        );
    }

};


orgunitrole.Events = {
    OnComboBoxEnterpriseSelect: function (e) {
        var dataItem = this.dataItem(e.item);
        var grid = $("#Grid").data("kendoGrid");
        grid.dataSource.view();

    },

    OnGriddataBound: function (e) {
        var dataSource = this.dataSource;
        this.element.find('tr.k-master-row').each(function () {
            var row = $(this);
            var data = dataSource.getByUid(row.data('uid'));
            // this example will work if ReportId is null or 0 (if the row has no details)
            if (!data.HasChildren) {
                row.find('.k-hierarchy-cell a').css({ visibility: 'hidden', opacity: 0.3, cursor: 'default' }).click(function (e) { e.stopImmediatePropagation(); return false; });
            }
        });
    }
}

orgunitrole.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/OrgunitRole/Delete",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.OrgunitRole.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.OrgunitRole.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.OrgunitRole.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.OrgunitRole.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};