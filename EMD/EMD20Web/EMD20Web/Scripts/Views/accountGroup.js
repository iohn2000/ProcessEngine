accountGroup = {};

accountGroup.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                accountGroup.Communication.Delete(guid);
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

        accountGroup.Functions.CreateHiddenFields();
    },

    CreateHiddenFields: function (e) {
        // remove all items
        $('form input[itemtype="group"]').remove();

        // recreate hidden fields because the enum must be in order

        var count = 0;
        var dataItems = $('#AssignedEmployments').data("kendoGrid").dataSource.data();

        for (var i = 0; i < dataItems.length; i++) {
            var dataItem = dataItems[i];

            $('form').append('<input data-val="true" identifier="' + dataItem.Value + '" type="hidden" class="k-valid" itemtype="group" id="AssignedEmployments[' + i + '].Text" name="AssignedEmployments[' + i + '].Text" value="' + dataItem.Text + '">');
            $('form').append('<input data-val="true" identifier="' + dataItem.Value + '" type="hidden" class="k-valid" itemtype="group" id="AssignedEmployments[' + i + '].Value" name="AssignedEmployments[' + i + '].Value" value="' + dataItem.Value + '">');

        }


    }
};

accountGroup.Entity = {
    AssignedEmployees: null,
    SelectedEnterprise: null,
}

accountGroup.Data = {

    AvailableEmploymentParameters: function (e) {
        //var assignedEmployees = [];
        //var datasourcedata = $(AssignedEmployments).data("kendoGrid").dataSource.data();

        //for (var i = 0; i < datasourcedata.length; i++) {
        //    assignedEmployees.push(datasourcedata[i].Value);
        //}

        //return {
        //    guid_ente: $('#E_Guid').data('kendoDropDownList').value(),
        //    assignedGuids: assignedEmployees
        //};



        return {
            guid_ente: accountGroup.Entity.SelectedEnterprise,
            assignedGuids: accountGroup.Entity.AssignedEmployees
        };
    }
};


accountGroup.Events = {
    OnAssignedEmploymentsDataBound: function(e){
        //accountGroup.Functions.CreateHiddenFields();
        console.log('databound');
        accountGroup.Functions.CreateHiddenFields();
    },

    OnEnterpriseDropDownlistSelectionSelect: function (e) {
        // set the guid
        accountGroup.Entity.SelectedEnterprise = $('#E_Guid').val();

        var dataSourceGrid = $(AssignedEmployments).data("kendoGrid").dataSource
        var datasourcedata = $(AssignedEmployments).data("kendoGrid").dataSource.data();

        for (var i = 0; i < datasourcedata.length; i++) {
            dataSourceGrid.remove(datasourcedata[i]);
        }

        $('#AvailableEmployments').data('kendoGrid').dataSource.read();
    },
    OnEnterpriseDropDownlistDataBound: function (e) {


        $('#AvailableEmployments').data('kendoGrid').dataSource.read();
    },

    OnClickAddEmployment: function (e) {

        var ListViewFrom = "#AvailableEmployments";
        var ListViewTo = "#AssignedEmployments";


        accountGroup.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);
    },

    OnClickRemoveEmployment: function (e) {


        var ListViewFrom = "#AssignedEmployments";
        var ListViewTo = "#AvailableEmployments";



        accountGroup.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);

    }
}


accountGroup.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/AccountGroup/DeleteAccountGroup",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.AccountGroup.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.AccountGroup.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.AccountGroup.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.AccountGroup.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};