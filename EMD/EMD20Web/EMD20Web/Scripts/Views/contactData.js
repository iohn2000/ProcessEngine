contactData = {};

contactData.Entities = {
    AllNumbers: null
}

contactData.Entities = {
    GetContactData: function (idContactType, isFuture) {
        var returnValue = null;

        var index = contactData.Entities.GetContactDataIndex(idContactType, isFuture);
        if (index >= 0) {
            returnValue = contactData.Entities.AllNumbers[index];
        }

        return returnValue;
    },
    GetContactDataIndex: function (idContactType, isFuture) {
        for (var i = 0; i < contactData.Entities.AllNumbers.length; i++) {
            var current = contactData.Entities.AllNumbers[i];

            if (current && current.C_CT_ID == idContactType && current.IsFuture) {
                return i;
            }
        }
        return -1;
    },
    UpdateContactData: function (idContactType, isFuture, data) {

    }
}

contactData.Functions = {
    SetHiddenFields: function (contact) {
        var index = contactData.Entities.GetContactDataIndex(contact.C_CT_ID, contact.IsFuture);

        //currentContactType.NationalCode = $('#edit_nationalcode_text').val();
        //currentContactType.Prefix = $('#edit_nationalcode_text').val();
        //currentContactType.Number = $('#edit_number_text').val();
        //currentContactType.IsVisibleInPhoneBook = $('#edit_phonebook_visible').prop('checked');
        //currentContactType.IsVisibleInCallCenter = $('#edit_phonebook_callcenter').prop('checked');
        //var tempDate = $("#edit_number_active_from").data("kendoDatePicker").value();


        $('#AllNumbers_' + index + '__NationalCode').val(contact.NationalCode);
        $('#AllNumbers_' + index + '__Prefix').val(contact.Prefix);
        $('#AllNumbers_' + index + '__Number').val(contact.Number);
        $('#AllNumbers_' + index + '__IsVisibleInPhoneBook').val(contact.IsVisibleInPhoneBook.toString());
        $('#AllNumbers_' + index + '__IsVisibleInCallCenter').val(contact.IsVisibleInCallCenter.toString());
        if (contact.IsFuture) {
            var date = kendoHelper.ParseJsonDate(contact.ActiveFrom);
            var msDateString = kendo.toString(kendo.parseDate(date), 'dd.MM.yyyy HH:mm:ss');

            $('#AllNumbers_' + index + '__ActiveFrom').val(msDateString);
        }
    },

    UpdateDataItemInGrid: function (contact) {
        var grid = $("#Grid").data("kendoGrid");


        for (var i = 0; i < grid.dataSource._data.length; i++) {
            var dataItem = grid.dataSource._data[i];

            if (dataItem.C_CT_ID == contact.C_CT_ID && dataItem.IsFuture == contact.IsFuture) {
                dataItem.NationalCode = contact.NationalCode;
                dataItem.Prefix = contact.Prefix;
                dataItem.Number = contact.Number;
                dataItem.IsVisibleInPhoneBook = contact.IsVisibleInPhoneBook;
                dataItem.IsVisibleInCallCenter = contact.IsVisibleInCallCenter;
                dataItem.ActiveFrom = contact.ActiveFrom;
                // check the future date
                dataItem.ShowInGrid = contact.IsFuture;

                grid.refresh();

                break;
            }
        }

        var dataItem = new Object();
        dataItem.ActiveFrom = kendoHelper.ParseJsonDate(contact.ActiveFrom);
        dataItem.ActiveTo = kendoHelper.ParseJsonDate(contact.ActiveTo);
        dataItem.CT_Guid = contact.CT_Guid;
        dataItem.C_CT_ID = contact.C_CT_ID;
        dataItem.C_EP_ID = contact.C_EP_ID;
        dataItem.C_E_ID = contact.C_E_ID;
        dataItem.C_ID = contact.C_ID;
        dataItem.C_L_ID = contact.C_L_ID;
        dataItem.C_P_ID = contact.C_P_ID;
        dataItem.CssActionButtonVisible = contact.CssActionButtonVisible;
        dataItem.EP_Guid = contact.EP_Guid;
        dataItem.E_Guid = contact.E_Guid;
        dataItem.Guid = contact.Guid;
        dataItem.IsFuture = contact.IsFuture;
        dataItem.IsVisibleInCallCenter = contact.IsVisibleInCallCenter;
        dataItem.IsVisibleInPhoneBook = contact.IsVisibleInPhoneBook;
        dataItem.L_Guid = contact.L_Guid;
        dataItem.Name = contact.Name;
        dataItem.NationalCode = contact.NationalCode;
        dataItem.Number = contact.Number;
        dataItem.P_Guid = contact.P_Guid;
        dataItem.Prefix = contact.Prefix;
        dataItem.ShowActionButtons = contact.ShowActionButtons;
        dataItem.ShowInGrid = contact.true;
        dataItem.UserFullName = contact.UserFullName;
        dataItem.Username = contact.Username;



        grid.dataSource._data.push(dataItem);
        grid.refresh();
    }
}

contactData.Events = {

    OnFormValidationEditNumberSuccess: function (xhr) {
        formValidation.OnSuccess(xhr, 'GridContactModel');
        personprofile.Events.OnChangedContactDataSuccess(xhr, false);
    },

    OnEditContactData: function (e) {


        if (e.checked) {
            $('#FutureContact_NumberAsText').removeAttr('disabled');
        }
        else {
            $('#FutureContact_NumberAsText').attr('disabled', 'disabled');
        }

        var ms = new Date().getTime() + 86400000;
        var tomorrow = new Date(ms);
        var kendoDate = kendo.toString(kendo.parseDate(tomorrow), 'dd.MM.yyyy');
        $('#ActiveFromFuture').data("kendoDatePicker").value(kendoDate);

        $('#ActiveFromFuture').data("kendoDatePicker").enable(e.checked);

    },

    OnClickEditNumberOnly: function (e) {
        var wintitle = 'Edit';
        $("<div id='modal-window-edit-numberonly'></div>").kendoWindow({
            title: wintitle,
            content: '/ContactData/EditNumber',
            modal: true,
            deactivate: function () {
                this.destroy();
            },
            visible: true,
            //refresh: function () { winElement.find(".k-loading-mask").remove(); }
        }).data("kendoWindow").open().center().title(wintitle);
    },
    OnClickEditNumber: function (idContactType, isFuture) {
        var contact = contactData.Entities.GetContactData(idContactType, isFuture);

        if (contact) {


            //  $("#edit_number_active_from").data("kendoDatePicker").value(contact.ActiveFrom);

            var wintitle = 'Edit';
            $("<div id='modal-window-edit-number'></div>").kendoWindow({
                width: "350px",
                height: "600px",
                title: wintitle,
                content: '/ContactData/EditNumber',
                modal: true,
                deactivate: function () {
                    this.destroy();
                },
                refresh: function (e) {
                    $('#edit_number_save').attr('data-contact-C_CT_ID', idContactType);
                    $('#edit_nationalcode_text').val(contact.NationalCode);
                    $('#edit_prefix_text').val(contact.Prefix);
                    $('#edit_number_text').val(contact.Number);
                    $('#edit_phonebook_visible').prop('checked', contact.IsVisibleInPhoneBook);
                    $('#edit_phonebook_callcenter').prop('checked', contact.IsVisibleInCallCenter);


                    var datePicker = $("#edit_number_active_from").data("kendoDatePicker");
                    if (!datePicker) {
                        var initDate = $("#edit_number_active_from").kendoDatePicker();
                        datePicker = initDate.data("kendoDatePicker");
                    }

                    if (datePicker) {
                        var date = kendoHelper.ParseJsonDate(contact.ActiveFrom);
                        var kendoDate = kendo.toString(kendo.parseDate(date), 'dd.MM.yyyy');
                        $("#edit_number_active_from").data("kendoDatePicker").value(date);
                    }
                },
                visible: true,
                //refresh: function () { winElement.find(".k-loading-mask").remove(); }
            }).data("kendoWindow").open().center().title(wintitle);
        }
    },
    OnButtonEditNumberOkClick: function (e) {
        console.log(e);
        var idContactType = parseInt(e.attributes.getNamedItem("data-contact-c_ct_id").value);

        var tempDate = $("#edit_number_active_from").data("kendoDatePicker").value();
        var isFuture = false;
        if (tempDate > new Date()) {
            isFuture = true;
        }

        var currentContactData = contactData.Entities.GetContactData(idContactType, isFuture);

        currentContactData.NationalCode = $('#edit_nationalcode_text').val();
        currentContactData.Prefix = $('#edit_nationalcode_text').val();
        currentContactData.Number = $('#edit_number_text').val();
        currentContactData.IsVisibleInPhoneBook = $('#edit_phonebook_visible').prop('checked');
        currentContactData.IsVisibleInCallCenter = $('#edit_phonebook_callcenter').prop('checked');

        currentContactData.ActiveFrom = kendoHelper.GetJsonDate(tempDate);

        contactData.Functions.SetHiddenFields(currentContactData);
        contactData.Functions.UpdateDataItemInGrid(currentContactData);

    },
    OnButtonEditNumberOnlyOkClick: function (e) {
        console.log('Edit number finished');
    },
    OnDeleteClick: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                contactData.Communication.Delete(guid);
            },
            null
        );
    }

}

contactData.Communication = {
    Delete: function (guid) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/ContactData/Delete",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh();

                    var xhr = new Object();
                    xhr.responseJSON = response;

                    personprofile.Events.OnChangedContactDataSuccess(xhr, false);
                    kendoHelper.Grid.Refresh('GridContactModel');
                    alertify.success(res.ContactData.Callnumber.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.ContactData.Callnumber.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.ContactData.Callnumber.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.ContactData.Callnumber.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};