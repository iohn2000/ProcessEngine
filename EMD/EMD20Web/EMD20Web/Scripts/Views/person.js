person = {};

person.Functions = {

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                //alert('delete ' + guid + ' called');

                person.Communication.Delete(guid);
            },
            null
        );
    },

    SetFieldValue: function (str, field) {
        var inputName = '#' + field;
        $(inputName).val(str);
    },

    FillInReducedString: function(str, field) {
        person.Communication.ReduceString(str, field);
    }
};


person.Events = {

    OnShowPersonProfileClick: function (e) {
        e.preventDefault();
        var tr = $(e.target).closest("tr");
        var data = this.dataItem(tr);
        showInWindow('/PersonProfile/Profile/' + data.Guid + '/true', 'Person Profile');
    },

    OnAddEmploymentClick: function (e) {
        e.preventDefault();
        var tr = $(e.target).closest("tr");
        var data = this.dataItem(tr);
        showInWindow('/Onboarding/Create/' + data.Guid + '/true', 'Add Employment');
    },

    OnAddTestClick: function (e) {
        e.preventDefault();
        var tr = $(e.target).closest("tr");
        var data = this.dataItem(tr);
        showInWindow('/Test/Index/true', 'Test Title');
    },

    OnSuccess: function (xhr) {
        if (xhr.responseJSON.personModel) {
            if (xhr.responseJSON.personModel.IsVisibleInPhonebook === true) {
                $('#person_is_visible_in_phonebook input').attr('checked', 'checked');
            }
            else {
                $('#person_is_visible_in_phonebook input').removeAttr('checked');
            }

            if (xhr.responseJSON.personModel.IsPictureVisible === true) {
                $('#person_is_picture_visible input').attr('checked', 'checked');
            }
            else {
                $('#person_is_picture_visible input').removeAttr('checked');
            }

            if (xhr.responseJSON.personModel.IsPictureVisibleInAD === true) {
                $('#person_is_picture_visible_inAD input').attr('checked', 'checked');
            }
            else {
                $('#person_is_picture_visible_inAD input').removeAttr('checked');
            }

            $('#personinfo_sex').html(xhr.responseJSON.personModel.Sex);
            $('#personinfo_p_id').html(xhr.responseJSON.personModel.P_ID);
            $('#personinfo_degreeprefix').html(xhr.responseJSON.personModel.DegreePrefix);
            $('#personinfo_degreesuffix').html(xhr.responseJSON.personModel.DegreeSuffix);
            $('#person_complete_name').html(xhr.responseJSON.personModel.Display_FirstName + ' ' + xhr.responseJSON.personModel.Display_FamilyName + '(' + xhr.responseJSON.personModel.UserID + ')')
        }

        formValidation.OnSuccess(xhr);


    }
};

person.Communication = {

    Delete: function (guid, gridId) {
        kendoHelper.ShowProgress();
        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/Person/DeletePerson",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh(gridId);
                    alertify.success(res.Person.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.Person.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Person.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Person.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },

    //Because of asynchronous request we need to supply the function calle on success
    ReduceString: function (str, field) {
        var data = { toReduce: str, fieldName : field };

        $.ajax({

            type: "POST",
            url: "/Person/ReduceString",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {

                if (response.success) {
                    $('#' + response.fieldName).val(response.reduced);
                }
            }
        });
    }
};