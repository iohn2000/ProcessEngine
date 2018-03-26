equipmentDefinitionOwner = {};

equipmentDefinitionOwner.Entities = {
    eqdeGuid: null
}

equipmentDefinitionOwner.Events = {
    OnClickRemoveOwner: function (e) {
        var ListView = "#ConfiguredOwners";

        equipmentDefinitionOwner.Functions.DeleteSelectedItems(ListView);
    },

    onOwnerSelected: function (e) {
        console.debug(e);
        var listViewSelected = $("#ConfiguredOwners").data("kendoGrid");
        var listViewSelectedDataSource = listViewSelected.dataSource;

        var dataSelected = listViewSelectedDataSource.data(); // or this.view();
        var itemExistsInSelected = false;
        for (var i = 0; i < dataSelected.length; i++) {
            if (dataSelected[i]['Value'] == e['Value']) {
                itemExistsInSelected = true;
            }
        }
        if (itemExistsInSelected == false) {
            listViewSelectedDataSource.add(e);
        }
    }
}

equipmentDefinitionOwner.Functions = {
    MoveSelectedItems: function (ListViewFrom, ListViewTo, ListFromIsSource) {
        var listView = $(ListViewFrom).data("kendoGrid");
        var listViewSelected = $(ListViewTo).data("kendoGrid");
        var listViewDataSource = listView.dataSource;
        var listViewSelectedDataSource = listViewSelected.dataSource;

        var y = $.map($(ListViewFrom).data('kendoGrid').select(), function (item) {
            dataItem = listViewDataSource.getByUid($(item).attr("data-uid"));


            if (!ListFromIsSource) {
                listViewDataSource.remove(dataItem);
            }
            else {
                var dataSelected = listViewSelectedDataSource.data(); // or this.view();
                var itemExistsInSelected = false;
                for (var i = 0; i < dataSelected.length; i++) {
                    if (dataSelected[i]['Value'] == dataItem['Value']) {
                        itemExistsInSelected = true;
                    }
                }
                if (itemExistsInSelected == false) {
                    listViewSelectedDataSource.add(dataItem.toJSON());
                }
            }

        });
        

        
    },

    DeleteSelectedItems: function (ListView) {
        var listViewSelected = $(ListView).data("kendoGrid");
        var listViewSelectedDataSource = listViewSelected.dataSource;

        var rows = listViewSelected.select();

        rows.each(
            function () {
                listViewSelected.removeRow(this);
            }
        )
    }


}

equipmentDefinitionOwner.Communication = {
    SaveConfiguredOwners: function () {
        kendoHelper.ShowProgress();

        $.ajax({
            url: "/EquipmentDefinition/DoManageOwners",
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ eqde_guid: equipmentDefinitionOwner.Entities.eqdeGuid, configuredOwners: $("#ConfiguredOwners").data("kendoGrid").dataSource.data() }),
            success: function (response) {             
                kendoHelper.HideProgress();
                if (response.success) {
                    alertify.success(res.Equipment.Alert.Save_Success);
                    closeWindow();
                }
                else {
                    alertify.alert(res.Equipment.Alert.Title_Error, response.errorMessage);
                }
            },
            error: function (response, textStatus,m) {
                kendoHelper.HideProgress();
                if (textStatus === "timeout") {
                    alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    if (response.errorMessage != "undefined") {
                        alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.GeneralError);
                    }
                    else {
                        alertify.alert(res.Equipment.Alert.Title_Error, response.errorMessage);
                    }
                }
            }

        })
    }
}