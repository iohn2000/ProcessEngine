﻿equipmentDefinitionPrice = {};

equipmentDefinitionPrice.Events = {
    OnEditEquipmentDefinitionPrice: function (e) {
        if (e.checked) {
            $('#FutureEquipmentDefinitionPrice_Price').removeAttr('disabled');
        }
        else {
            $('#FutureEquipmentDefinitionPrice_Price').attr('disabled', 'disabled');
        }

        var ms = new Date().getTime() + 86400000;
        var tomorrow = new Date(ms);
        var kendoDate = kendo.toString(kendo.parseDate(tomorrow), 'dd.MM.yyyy');
        $('#ActiveFromFuture').data("kendoDatePicker").value(kendoDate);

        $('#ActiveFromFuture').data("kendoDatePicker").enable(e.checked);
        $("#FutureEquipmentDefinitionPrice_BillingPeriod").data("kendoDropDownList").enable(e.checked);
        //$("#FutureEquipmentDefinitionPrice_Price").data("kendoNumericTextBox").enable(e.checked);
    }
};

//equipmentDefinitionPrice.Functions = {

//    Delete: function (guid, entityName, name) {
//        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
//            function () {
//                //alert('delete ' + guid + ' called');

//                equipmentDefinition.Communication.Delete(guid);
//            },
//            null
//        );
//    }
//};

//equipmentDefinitionPrice.Communication = {
//    Delete: function (guid) {
//        kendoHelper.ShowProgress();
//        var data = { guid: guid };

//        $.ajax({

//            type: "POST",
//            url: "/EquipmentDefinition/DeleteEquipmentDefinition",
//            data: JSON.stringify(data),
//            dataType: 'json',
//            contentType: 'application/json',
//            success: function (response) {
//                kendoHelper.HideProgress();
//                if (response.success) {
//                    kendoHelper.Grid.Refresh();
//                    alertify.success(res.EquipmentDefinition.Alert.Delete_Success);
//                }
//                else {
//                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, response.errorModel.ErrorMessage);
//                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
//                }
//            },
//            error: function (x, t, m) {
//                kendoHelper.HideProgress();
//                if (t === "timeout") {
//                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.Timeout);
//                }
//                else {
//                    alertify.alert(res.EquipmentDefinition.Alert.Title_Error, res.Alert.GeneralError);
//                }
//            },
//            timeout: 15000
//        });
//    }
//};