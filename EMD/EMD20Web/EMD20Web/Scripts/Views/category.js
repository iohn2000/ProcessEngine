category = {};

category.Entities = {
    Guid: null,
    Name: null,
    Description: null
}

category.Functions = {

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

    },

    Delete: function (guid, entityName, name, showLinkedEntityWarning) {
        var warning = res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]);
        if (showLinkedEntityWarning == true)
        {
            warning += res.Confirmation.Linked_Entity_Warning;
        }
        

        alertify.confirm(res.Confirmation.Title_Delete, warning,
            function () {
                //alert('delete ' + guid + ' called');

                category.Communication.Delete(guid);
            },
            null
        );
    }
}


category.Events = {
    OnClickAddEquipment: function (e) {
        var ListViewFrom = "#AvailableEquipments";
        var ListViewTo = "#ConfiguredEquipments";

        category.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);
    },

    OnClickRemoveEquipment: function (e) {
        var ListViewFrom = "#ConfiguredEquipments";
        var ListViewTo = "#AvailableEquipments";

        category.Functions.MoveSelectedItems(ListViewFrom, ListViewTo);
    },

    OnChangedCategoryDataSuccess: function (xhr) {
        $("#Grid").data("kendoGrid").dataSource.read();
        $("#Grid").data("kendoGrid").refresh();
    }
}

category.Communication = {
    SaveConfiguredEquipments: function () {
        kendoHelper.ShowProgress();
        $.ajax({
            url: "Edit",
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ Guid: $("#Guid").val(), Name: $("#Name").val(), Description: $("#Description").val(), CategoryType: $("#CategoryType").val(), configuredEquipments: $("#ConfiguredEquipments").data("kendoGrid").dataItems() }),
            success: function (response) {

                kendoHelper.HideProgress();
                if (response.success) {

                    alertify.success(res.Equipment.Alert.Save_Success);
                    closeWindow();
                    category.Events.OnChangedCategoryDataSuccess(response);
                }
                else {
                    alertify.alert(res.Equipment.Alert.Title_Error, response.errorMessage);
                }
            },
            error: function (response, textStatus) {
                kendoHelper.HideProgress();
                if (textStatus === "timeout") {
                    alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    //alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.GeneralError);
                    alertify.alert(response.statusText);
                }
            }

        })
    },

    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({
            type: "POST",
            url: "/Category/Delete",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.Category.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.Category.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Category.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Category.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};

category.EquipmentDefinition = {};

category.EquipmentDefinition.Entities = {
    eqdeGuid: null
};

category.EquipmentDefinition.Events = {
    OnClickAddEquipment: function (e) {
        var ListViewFrom = "#AvailableCategories";
        var ListViewTo = "#ConfiguredCategories";

        category.EquipmentDefinition.Functions.MoveSelectedItems(ListViewFrom, ListViewTo, true);
    },

    OnClickRemoveEquipment: function (e) {
        var ListViewFrom = "#ConfiguredCategories";
        var ListViewTo = "#AvailableCategories";

        category.EquipmentDefinition.Functions.MoveSelectedItems(ListViewFrom, ListViewTo, false);
    }
};

category.EquipmentDefinition.Functions = {
        MoveSelectedItems: function (ListViewFrom, ListViewTo, CanAddCategory) {
            var listView = $(ListViewFrom).data("kendoGrid");
            var listViewSelected = $(ListViewTo).data("kendoGrid");

            var listViewDataSource = listView.dataSource;
            var listViewSelectedDataSource = listViewSelected.dataSource;
            var selectedItem = $(ListViewFrom).data('kendoGrid').select();

            var itemSelected = false;
            var y = $.map($(ListViewFrom).data('kendoGrid').select(), function (item) {
                dataItem = listViewDataSource.getByUid($(item).attr("data-uid"));
                listViewSelectedDataSource.add(dataItem.toJSON());
                listViewDataSource.remove(dataItem);
                itemSelected = true;
            });

            var categoryNameFromFilter = listViewDataSource.filter().filters[0].value;
            if (y.length == 0 && CanAddCategory && itemSelected==false)
            {
                $.ajax({
                    url: "/Category/Edit",
                    type: "POST",
                    dataType: "json",
                    contentType: "application/json",
                    data: JSON.stringify({ Name: categoryNameFromFilter, Description: '', CategoryType: 10 }),
                    success: function (response) {
                        console.debug(response);
                        kendoHelper.HideProgress();
                        if (response.success) {
                            alertify.success(res.Category.Alert.Create_Success);
                            listViewSelectedDataSource.add({
                                Text: categoryNameFromFilter,
                                Value: response.categoryGuid
                            });
                            //closeWindow();
                        }
                        else {
                            alertify.alert(res.Category.Alert.Title_Error, response.errorMessage);
                        }
                    },
                    error: function (response, textStatus) {
                        kendoHelper.HideProgress();
                        if (textStatus === "timeout") {
                            alertify.alert(res.Category.Alert.Title_Error, res.Alert.Timeout);
                        }
                        else {
                            alertify.alert(res.Category.Alert.Title_Error, response.statusText);
                        }
                    }

                })
            }

            

        }
};

category.EquipmentDefinition.Communication = {
    SaveConfiguredCategories: function () {
        kendoHelper.ShowProgress();
        $.ajax({
            url: "/EquipmentDefinition/DoManageCategories",
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            //data: JSON.stringify({ eqde_guid: equipmentDefinitionOwner.Entities.eqdeGuid, configuredCategories: $("#ConfiguredCategories").data("kendoGrid").dataItems() }),
            data: JSON.stringify({ eqde_guid: $("#EquipmentDefinitionGuid").val(), configuredCategories: $("#ConfiguredCategories").data("kendoGrid").dataItems() }),
            success: function (response, textStatus) {

                kendoHelper.HideProgress();
                if (response.success) {

                    alertify.success(res.Equipment.Alert.Save_Success);
                    closeWindow();
                }
                else {
                    alertify.alert(res.Equipment.Alert.Title_Error, response.errorMessage);
                }
            },
            error: function (response, textStatus, m) {
                kendoHelper.HideProgress();
                if (textStatus === "timeout") {
                    alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Equipment.Alert.Title_Error, res.Alert.GeneralError);
                }
            }

        })
    }
};