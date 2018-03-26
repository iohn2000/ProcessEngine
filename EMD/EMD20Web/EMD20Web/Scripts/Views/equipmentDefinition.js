equipmentDefinition = {};

equipmentDefinition.Entities = {
    EqdeGuid: null
}

equipmentDefinition.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                equipmentDefinition.Communication.Delete(guid);
            },
            null
        );
    },

    UpdatePrices: function () {
        equipmentDefinition.Communication.UpdatePrices();
    },

    UpdatePriceForClientReferenceId: function (idPriceSystem, idClientReference) {
        if (parseInt(idPriceSystem) > 0 && parseInt(idClientReference) > 0) {

            equipmentDefinition.Communication.GetPriceForClientReferenceId(idPriceSystem, idClientReference, function (data) {
                if (data !== null && data.priceInfo !== undefined) {
                    $('#buttonSetPrice').removeAttr('disabled');
                    $('#valueExternalPriceInfo').text(kendo.toString(data.priceInfo.Price, 'n2'));
                }
                else {
                    $('#ClientReferenceIDForPrice').val('');
                    $('#buttonSetPrice').attr('disabled', 'disabled');
                }
            });
        }
    },

    SetPriceFromExternal: function () {
        equipmentDefinition.Communication.UpdatePrice(equipmentDefinition.Entities.EqdeGuid, parseInt($('#ClientReferenceSystemForPrice').val()), $('#ClientReferenceIDForPrice').val(), function (data) {
            if (data.success) {
                // $('#DivPriceInformation').text(data.priceInfo.Price.toLocaleString('de-DE') + ' EUR / ' + data.priceInfo.BillingPeriodName);
                $('#DivPriceInformation').text(kendo.toString(data.priceInfo.Price, 'n2') + ' EUR / ' + data.priceInfo.BillingPeriodName);
                alertify.alert(res.EquipmentDefinition.Alert.UpdatePrice_Exernal_Success, res.EquipmentDefinition.Title_Default);
            }
        });
    }
};

equipmentDefinition.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/EquipmentDefinition/DeleteEquipmentDefinition",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.EquipmentDefinition.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },

    UpdatePrices: function () {
        kendoHelper.ShowProgress();

        $.ajax({

            type: "POST",
            url: "/EquipmentDefinition/UpdatePrices",
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();
                    alertify.success(res.EquipmentDefinition.Alert.UpdatePrices_Success);
                }
                else {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 120000
        });
    },

    UpdatePrice: function (eqdeGuid, enumClientReferenceSystemForPrice, idClientReference, callback) {
        kendoHelper.ShowProgress();
        var data = { eqdeGuid: eqdeGuid, enumClientReferenceSystemForPrice: enumClientReferenceSystemForPrice, idClientReference: idClientReference };
        $.ajax({

            type: "POST",
            url: "/EquipmentDefinition/UpdatePrice",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (!response.success) {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }

                callback(response);
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },

    GetPriceForClientReferenceId: function (enumClientReferenceSystemForPrice, idClientReference, callback) {
        kendoHelper.ShowProgress();
        var data = { enumClientReferenceSystemForPrice: enumClientReferenceSystemForPrice, idClientReference: idClientReference };
        $.ajax({

            type: "POST",
            url: "/EquipmentDefinition/GetPrice",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (!response.success) {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }

                callback(response);
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};

equipmentDefinition.Events = {
    OnChangedPriceInformationSuccess: function (xhr, callFormValidation) {
        console.debug("OnChangedPriceInformationSuccess");
        if (xhr.responseJSON.equipmentDefinitionPriceModel) {
            var eqdePriceModel = xhr.responseJSON.equipmentDefinitionPriceModel;
            $('#DivFuturePriceInformationWithActiveFromDate').html(eqdePriceModel.FutureEquipmentDefinitionPrice.FuturePriceInformation);
            $('#DivPriceInformation').html(eqdePriceModel.CurrentEquipmentDefinitionPrice.PriceInformation);

            if (callFormValidation === true) {
                formValidation.OnSuccess(xhr);
            }
        }
    },
    OnOwnerFilterChanged: function (e) {
        $("#Grid").data("kendoGrid").dataSource.read();
    },

    OnPriceClientReferenceSystemChanged: function (e) {
        var selectedItem = this.dataItem(e.item);
        if (selectedItem.Value !== '') {
            $('#areaClientReferenceId').show();
            $('#buttonEditPrice').hide();
        }
        else {
            $('#buttonEditPrice').show();
            $('#ClientReferenceIDForPrice').val('');
            $('#areaClientReferenceId').hide();
        }
    },

    OnPriceClientReferenceIdLeave: function (e) {
        var value = $(e).val();

        if (value.length > 0) {
            equipmentDefinition.Functions.UpdatePriceForClientReferenceId(parseInt($('#ClientReferenceSystemForPrice').val()), value);
        }
    },
};

equipmentDefinition.Data = {
    OwnerFilterParameters: function (e) {
        console.debug('empl:' + $("#FilterOwners").val());
        return {
            empl_guid: $("#FilterOwners").val()
        };
    }
};