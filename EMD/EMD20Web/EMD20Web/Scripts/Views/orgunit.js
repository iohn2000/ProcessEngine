orgunit = {};

orgunit.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                orgunit.Communication.Delete(guid);
            },
            null
        );
    },

    ChangeGridToTreeOrFlat: function (e) {
        orgunit.Entities.ShowTree = !orgunit.Entities.ShowTree;

        var grid = $("#Grid").data("kendoGrid");

        if (orgunit.Entities.ShowTree) {
            grid.hideColumn("ParentName");
            grid.hideColumn("RootName");

            $('#btnShowHideTree').html(res.Orgunit.Button.ShowOrgunitFlatList);
        }
        else {
            grid.showColumn("ParentName");
            grid.showColumn("RootName");

            $('#btnShowHideTree').html(res.Orgunit.Button.ShowOrgunitTree);
        }

        //grid.dataSource.page(1);
        //grid.dataSource.read();

        grid.dataSource.query({ page: 1, pageSize: 15, sort: { field: "Name", dir: "asc" } });
    },

    CleanupOrgunits: function (e) {
        orgunit.Communication.CleanupOrgunits();
    }

};

orgunit.Entities = {
    SelectedOrgUnitRoot: null,
    IsSecurity: false,
    ShowTree: false
}

orgunit.Events = {
    OnRootOrgunitSelected: function (e) {
        orgunit.Entities.SelectedOrgUnitRoot = this.dataItem(e.item).Value;

        $("#Grid").data("kendoGrid").dataSource.read();
    }
}

orgunit.Grid = {
    DataOrgUnit: function (e) {
        return {
            parentGuid: orgunit.Entities.SelectedOrgUnitRoot,
            isSecurity: orgunit.Entities.IsSecurity,
            showTree: orgunit.Entities.ShowTree
        };
    }
}

orgunit.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/Orgunit/Delete",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.Orgunit.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.Orgunit.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Orgunit.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Orgunit.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },

    CleanupOrgunits: function () {
        kendoHelper.ShowProgress();

        $.ajax({

            type: "POST",
            url: "/Orgunit/CleanupOrgunits",
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.Orgunit.Alert.Cleanup_Success);
                }
                else {
                    alertify.alert(res.Orgunit.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Orgunit.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Orgunit.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 120000
        });
    }
};