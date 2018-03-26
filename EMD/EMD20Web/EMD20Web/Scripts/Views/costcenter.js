costcenter = {};

costcenter.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                costcenter.Communication.Delete(guid);
            },
            null
        );
    },

    MoveSelectedItems: function (ListViewFrom, ListViewTo) {
        var listView = $(ListViewFrom).data("kendoGrid");
        var listViewSelected = $(ListViewTo).data("kendoGrid");

        var listViewDataSource = listView.dataSource;
        var listViewSelectedDataSource = listViewSelected.dataSource;

        var y = $.map($(ListViewFrom).data('kendoGrid').select(), function (item) {
            dataItem = listViewDataSource.getByUid($(item).attr("data-uid"));
            listViewSelectedDataSource.add(dataItem.toJSON());
            listViewDataSource.remove(dataItem);
        });

        costcenter.Functions.CreateHiddenFields();
    },

    CreateHiddenFields: function (e) {
        // remove all items
        $('form input[itemtype="group"]').remove();

        // recreate hidden fields because the enum must be in order

        var count = 0;
        var dataItems = $('#ConfiguredGroups').data("kendoGrid").dataSource.data();

        for (var i = 0; i < dataItems.length; i++) {
            var dataItem = dataItems[i];

            $('form').append('<input data-val="true" identifier="' + dataItem.Value + '" type="hidden" class="k-valid" itemtype="group" id="ConfiguredGroups[' + i + '].Text" name="ConfiguredGroups[' + i + '].Text" value="' + dataItem.Text + '">');
            $('form').append('<input data-val="true" identifier="' + dataItem.Value + '" type="hidden" class="k-valid" itemtype="group" id="ConfiguredGroups[' + i + '].Value" name="ConfiguredGroups[' + i + '].Value" value="' + dataItem.Value + '">');

        }


    },

    CleanupCostcenters: function (e) {
        costcenter.Communication.CleanupCostcenters();
    }
}

costcenter.Data = {
    AvailableGroupParameters: function (e) {
        return {
            guid_ente: $('#E_Guid').val()
        };
    }
}


costcenter.Events = {
    OnDropDownlistSelectionSelect: function (e) {

        var dataSourceGrid = $(ConfiguredGroups).data("kendoGrid").dataSource
        var datasourcedata = $(ConfiguredGroups).data("kendoGrid").dataSource.data();



        for (var i = 0; i < datasourcedata.length; i++) {


            dataSourceGrid.remove(datasourcedata[i]);
        }





        $('#AvailableGroups').data('kendoGrid').dataSource.read();
    },

    OnComboBoxEnterpriseSelect: function (e) {
        var dataItem = this.dataItem(e.item);
        var grid = $("#Grid").data("kendoGrid");
        grid.dataSource.read();

    },

    OnClickAddGroup: function (e) {

        var ListViewFrom = "#AvailableGroups";
        var ListViewTo = "#ConfiguredGroups";


        costcenter.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);
    },

    OnClickRemoveGroup: function (e) {


        var ListViewFrom = "#ConfiguredGroups";
        var ListViewTo = "#AvailableGroups";



        costcenter.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);

    }
}



costcenter.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/Costcenter/DeleteCostcenter",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.Costcenter.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.Costcenter.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Costcenter.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Costcenter.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 120000
        });
    },

    CleanupCostcenters: function () {
        kendoHelper.ShowProgress();

        $.ajax({

            type: "POST",
            url: "/Account/CleanupAccounts",
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.Costcenter.Alert.Cleanup_Success);
                }
                else {
                    alertify.alert(res.Costcenter.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Costcenter.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Costcenter.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 120000
        });
    }
};