equipmentMapping = {};

equipmentMapping.Entities = {
    PackageGuid: null
}

equipmentMapping.Events = {
    OnClickAddEquipment: function (e) {

        var ListViewFrom = "#AvailableEquipments";
        var ListViewTo = "#ConfiguredEquipments";


        equipmentMapping.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);
    },

    OnClickRemoveEquipment: function (e) {


        var ListViewFrom = "#ConfiguredEquipments";
        var ListViewTo = "#AvailableEquipments";



        equipmentMapping.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);

    }
}

equipmentMapping.Functions = {
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
    }


}

equipmentMapping.Communication = {
    SaveConfiguredMappings: function () {
        kendoHelper.ShowProgress();

        $.ajax({
            url: "DoManageEquipment",
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ pack_guid: equipmentMapping.Entities.PackageGuid, configuredEquipments: $("#ConfiguredEquipments").data("kendoGrid").dataItems() }),
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
            error: function (response) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.GeneralError);
                }
            }

        })
    }
}