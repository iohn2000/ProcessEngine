var changeEmployment = {}


changeEmployment.Entities = {
    EmploymentGuid: null,
    EnterpriseGuid: null,
    EmploymentTypeGuid: null,

    SelectedChangeType: null,
    // filter
    SelectedEmploymentType: null,
    SelectedCostCenter: null,
    /**
    * The selected Enterprise is always the EnterpriseGuid (SOURCE)
    * In case of Enterprise change the selected enterprise is the enterprise choosen by the dropdownlist,
    * because the target costcenter/orgunit/sponsors/location are relying on the new enterprise
    **/
    SelectedEnterprise: null,
    SelectedLocation: null,

    SelectedEnterpriseDataItem: null,

    /**
    * enum matches C# enum
    **/
    EnumChangeValueType:
    {
        NewEmpl: { value: 0, name: 'NewEmpl' },
        Enterprise: { value: 1, name: 'Enterprise' },
        EmploymentType: { value: 2, name: 'EmploymentType' },
        OrgUnit: { value: 3, name: 'OrgUnit' },
        Costcenter: { value: 4, name: 'Costcenter' },
        Location: { value: 5, name: 'Location' },
        Pause: { value: 6, name: 'Pause' },
        EquipmentProc: { value: 7, name: 'EquipmentProc' }
    }
}

changeEmployment.Data = {
    CostCenterParameters: function (e) {
        return {
            ente_guid: changeEmployment.Entities.SelectedEnterprise
        };
    },
    OrgUnitParameters: function (e) {
        return {
            ente_guid: changeEmployment.Entities.SelectedEnterprise
        };
    },
    SponsorParamters: function (e) {
        return {
            ente_guid: changeEmployment.Entities.SelectedEnterprise
        };
    },
    LocationParameters: function (e) {
        return {
            ente_guid: changeEmployment.Entities.SelectedEnterprise
        };
    },
    TargetEnterpriseParameters: function (e) {
        return {
            ente_guid: changeEmployment.Entities.EnterpriseGuid
        };
    },
    EmploymentTypeParameters: function (e) {
        return {
            emty_guid: changeEmployment.Entities.EmploymentTypeGuid
        };
    }
}


changeEmployment.UIElements = {

}

changeEmployment.Functions = {
    DisableAllInputs: function (e) {

        $('#SelectedChangeType').data('kendoDropDownList').readonly(true);
        //   $('#TargetDate').data('kendoDatePicker').readonly(true);

        if ($('#GuidTargetEnte').data('kendoDropDownList') != null) {
            $('#GuidTargetEnte').data('kendoDropDownList').readonly(true);
        }

        if ($('#GuidEmploymentType').data('kendoDropDownList') != null) {
            $('#GuidEmploymentType').data('kendoDropDownList').readonly(true);
        }

        if ($('#EMailType').data('kendoDropDownList') != null) {
            $('#EMailType').data('kendoDropDownList').readonly(true);
        }


        if ($('#GuidCostcenter').data('kendoDropDownList') != null) {
            $('#GuidCostcenter').data('kendoDropDownList').readonly(true);
        }


        if ($('#GuidOrgUnit').data('kendoDropDownList') != null) {
            $('#GuidOrgUnit').data('kendoDropDownList').readonly(true);
        }


        $('#GuidSponsorEmployment').parent().children("button[type='button']:first").hide();

        if ($('#GuidLocation').data('kendoDropDownList') != null) {
            $('#GuidLocation').data('kendoDropDownList').readonly(true);
        }

        if ($('#Simcard').data('kendoDropDownList') != null) {
            $('#Simcard').data('kendoDropDownList').readonly(true);
        }

        if ($('#Datacard').data('kendoDropDownList') != null) {
            $('#Datacard').data('kendoDropDownList').readonly(true);
        }

        $('#TempApproveEquipmentMove').attr('disabled', true);
        $('#TempMoveAllRoles').attr('disabled', true);
    },

    DisableChangeChangeType: function (e) {
        $('#SelectedChangeType').data('kendoDropDownList').readonly(true);
        $("#SelectedChangeType").closest('.k-dropdown').addClass('disabled');
    },

    SetEnterpriseValidation: function () {
        if (changeEmployment.Entities.SelectedChangeType.ChangeType > 0) {
            $("#GuidTargetEnte").removeAttr('required');
            $("#GuidTargetEnte").removeAttr('data-val-required');
        }
        else {
            $("#GuidTargetEnte").attr('required', 'required');
            $("#GuidTargetEnte").attr('data-val-required', 'Required field');
        }
    },

    SetSponsorValidation: function () {
        if (changeEmployment.Entities.SelectedChangeType.ChangeType < 30) {
        var mustHavSponsor = $('#GuidEmploymentType').data('kendoDropDownList').dataItem().Data.MustHaveSponsor;
        if (mustHavSponsor == false) {
            $("#SponsorGuid").removeAttr('required');
            $("#SponsorGuid").removeAttr('data-val-required');
            $("#GuidSponsorEmployment").removeAttr('required');
            $("#GuidSponsorEmployment").removeAttr('data-val-required');
            $("#EMailType").removeAttr('required');
            $("#EMailType").removeAttr('data-val-required');
        }
        else {
            $("#SponsorGuid").attr('required');
            $("#SponsorGuid").attr('data-val-required');
            $("#GuidSponsorEmployment").attr('required');
            $("#GuidSponsorEmployment").attr('data-val-required');
            $("#EMailType").attr('required');
            $("#EMailType").attr('data-val-required');
        }
        }
    },

    GetSelectedChangeValueTypes: function () {
        var changeValueTypes = [];

        if ($('#GuidTargetEnte').data('kendoDropDownList') && $('#GuidTargetEnte').data('kendoDropDownList').selectedIndex != 0) {
            changeValueTypes.push(changeEmployment.Entities.EnumChangeValueType.Enterprise.value);
        }

        if ($('#GuidEmploymentType').data('kendoDropDownList') && $('#GuidEmploymentType').data('kendoDropDownList').selectedIndex != 0) {
            changeValueTypes.push(changeEmployment.Entities.EnumChangeValueType.EmploymentType.value);
        }

        if ($('#GuidOrgUnit').data('kendoDropDownList') && $('#GuidOrgUnit').data('kendoDropDownList').selectedIndex != 0) {
            changeValueTypes.push(changeEmployment.Entities.EnumChangeValueType.OrgUnit.value);
        }

        if ($('#GuidCostcenter').data('kendoDropDownList') && $('#GuidCostcenter').data('kendoDropDownList').selectedIndex != 0) {
            changeValueTypes.push(changeEmployment.Entities.EnumChangeValueType.Costcenter.value);
        }

        if ($('#GuidLocation').data('kendoDropDownList') && $('#GuidLocation').data('kendoDropDownList').selectedIndex != 0) {
            changeValueTypes.push(changeEmployment.Entities.EnumChangeValueType.Location.value);
        }

        if ($('#LeaveTo').data('kendoDropDownList') && $('#LeaveTo').data('kendoDropDownList').selectedIndex != 0) {
            changeValueTypes.push(changeEmployment.Entities.EnumChangeValueType.Pause.value);
        }

        return changeValueTypes;
    },

    ActivateNextStepIfChangeItemsAreFinished: function () {
        var changeTypes = changeEmployment.Functions.GetSelectedChangeValueTypes();

        changeEmployment.Communication.CheckInputValidationFinishedForChangeType(changeEmployment.Entities.SelectedChangeType.ChangeType, changeTypes, changeEmployment.Entities.SelectedEnterpriseDataItem != null ? changeEmployment.Entities.SelectedEnterpriseDataItem.Data.E_ID : null);

        //if (changeEmployment.Functions.CheckInputsFinished()) {
        //    switch (changeEmployment.Entities.SelectedChangeType.ChangeType) {
        //        case 5:
        //        case 2:
        //            $('#button_submit_change').removeAttr('disabled');
        //            break;
        //        case 0:
        //        case 1:
        //        case 3:
        //        case 4:
        //            changeEmployment.Functions.DisableAllInputs();
        //            $('#button_get_equipments').show();
        //            break;
        //    }
        //}
    },

    RenderFormFieldsForChangeTypeIfFinished: function (e) {
        if ($('#TargetDate').data('kendoDatePicker').value() != null && $('#SelectedChangeType').data('kendoDropDownList').selectedIndex != 0) {

            changeEmployment.Functions.DisableChangeChangeType();
            $('#TargetDate').data('kendoDatePicker').readonly(true);
            
            $('#area_change_employment_selection').load("/Change/GetChangeSelectionView",
                { changeType: changeEmployment.Entities.SelectedChangeType.ChangeType, empl_guid: changeEmployment.Entities.EmploymentGuid, guidEnterprise: changeEmployment.Entities.EnterpriseGuid });

            changeEmployment.Functions.SetEnterpriseValidation();
        }
    },

    ShowHideInputsForEmploymentType: function (employmentType) {
        /*
         * EnumEmploymentTypeCategory:
         * Internal = 1, External = 2, Other = 3
        */
        if (employmentType.Data.EnumEmploymentTypeCategory === 2 || employmentType.Data.EnumEmploymentTypeCategory === 3) {
            $('#group-emailtype').show();
            $('#GuidSponsorEmployment').next().children("input[type='text']:first").val('');
            $('#PersonalNumber').val('');
            $('#group-personalnumber').hide();
        }
        else {
            $('#GuidSponsorEmployment').next().children("input[type='text']:first").val('');
            $('#group-emailtype').hide();
            $('#group-personalnumber').show();
            // $('#PersonalNumber').val('');
        }

        if (employmentType.Data.MustHaveSponsor) {
            $('#group-sponsor').show();
            $("#SponsorGuid").attr('required', 'required');
            $("#SponsorGuid").attr('data-val-required', 'Required field');
        }
        else {
            $("#SponsorGuid").removeAttr('required');
            $("#SponsorGuid").removeAttr('data-val-required');
            $('#group-sponsor').hide();
        }
    }
}

changeEmployment.Events = {
    OnMoveAllRolesChanged: function (e) {
        if ($('#TempMoveAllRoles').prop('checked') === false) {
            $('#MoveAllRoles').attr('value', 'false');
        }
        else {
            $('#MoveAllRoles').attr('value', 'true');
        }
    },
    OnChangeOrgUnitApproveEquipmentMove: function (e) {
        if ($('#TempApproveEquipmentMove').prop('checked') === false) {
            $('#ApproveEquipmentMove').attr('value', 'false');
            $('#button_get_equipments').hide();
        }
        else {
            $('#ApproveEquipmentMove').attr('value', 'true');
        }


        //if ($('#ApproveEquipmentMove').prop('checked') === false) {
        //    $('#button_get_equipments').hide();
        //}

        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },

    OnChangeEnterprise: function (e) {

        changeEmployment.Entities.SelectedEnterprise = this.dataItem(e.item).Value;
        changeEmployment.Entities.SelectedEnterpriseDataItem = this.dataItem(e.item);

        var selectedEnterpriseItem = this.dataItem(e.item);

        // If the Enterprise was changed, refill all dependend Dropdownlists
        $("#GuidCostcenter").data("kendoDropDownList").dataSource.read();
        $("#GuidOrgUnit").data("kendoDropDownList").dataSource.read();
        //if ($('#group-sponsor').is(':visible')) {
        //    $("#GuidSponsorEmployment").data("kendoDropDownList").dataSource.read();
        //}
        $("#GuidLocation").data("kendoDropDownList").dataSource.read();

        if (selectedEnterpriseItem.Data.E_ID === 20) {
            $('#group-additionaldata').show();
        }
        else {
            $('#group-additionaldata').hide();
        }

        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },
    OnChangeTypeChanged: function (e) {
        changeEmployment.Entities.SelectedChangeType = null;
        changeEmployment.Entities.SelectedEmploymentType = null;
        changeEmployment.Entities.SelectedCostCenter = null;
        changeEmployment.Entities.SelectedEnterprise = null;
        changeEmployment.Entities.SelectedLocation = null;



        changeEmployment.Entities.SelectedChangeType = this.dataItem(e.item);

        if (changeEmployment.Entities.SelectedChangeType.ChangeType > 0) {
            changeEmployment.Entities.SelectedEnterprise = changeEmployment.Entities.EnterpriseGuid;
        }

        changeEmployment.Functions.RenderFormFieldsForChangeTypeIfFinished();

    },
    OnChangeTargetDate: function (e) {
        changeEmployment.Functions.RenderFormFieldsForChangeTypeIfFinished();
    },
    OnChangeCostCenter: function (e) {
        changeEmployment.Entities.SelectedCostCenter = this.dataItem(e.item).Value;

        changeEmployment.Communication.GetCostCenterResponsible(changeEmployment.Entities.SelectedCostCenter);

        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },
    OnChangeOrgUnit: function (e) {
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },

    OnChangeLeaveFromDate: function (e) {
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },

    OnChangeLeaveToDate: function (e) {
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },

    OnChangeDistributionGroup: function (e) {
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },
    OnChangeSponsor: function (e) {
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },

    OnDataBoundEmploymentType: function (e) {
        var employmentType = $('#GuidEmploymentType').data('kendoDropDownList').dataItem();
        changeEmployment.Entities.SelectedEmploymentType = employmentType.Value;

        changeEmployment.Functions.ShowHideInputsForEmploymentType(employmentType);
        
    },

    OnChangeEmploymentType: function (e) {

        var employmentType = this.dataItem(e.item);
        changeEmployment.Entities.SelectedEmploymentType = employmentType.Value;

        changeEmployment.Functions.ShowHideInputsForEmploymentType(employmentType);

        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },
    OnChangeSimCard: function (e) {
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },
    OnChangeDataCard: function (e) {
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },
    OnChangeLocation: function (e) {
        changeEmployment.Entities.SelectedLocation = this.dataItem(e.item).Value;
        changeEmployment.Functions.ActivateNextStepIfChangeItemsAreFinished();
    },

    OnButtonGetEquipmentsClick: function (e) {
        var validator = $("form").kendoValidator().data("kendoValidator");
        changeEmployment.Functions.SetSponsorValidation();
        changeEmployment.Functions.SetEnterpriseValidation();

        if (validator.validate())
        {
            $('#button_get_equipments').hide();
            $('#button_submit_change').removeAttr('disabled');

            changeEmployment.Functions.DisableAllInputs()

            // do checks for filled out values
            if (!changeEmployment.Entities.SelectedEmploymentType) {
                changeEmployment.Entities.SelectedEmploymentType = $('#GuidEmploymentType').val();
                console.log('EmploymentType not changed. Take source employmentType: ' + changeEmployment.Entities.SelectedEmploymentType);
            }
            if (!changeEmployment.Entities.SelectedCostCenter) {
                changeEmployment.Entities.SelectedCostCenter = $('#GuidCostcenter').val();
                console.log('Costcenter not changed. Take source costcenter: ' + changeEmployment.Entities.SelectedCostCenter);
            }
            if (!changeEmployment.Entities.SelectedLocation) {
                changeEmployment.Entities.SelectedLocation = $('#GuidLocation').val();
                console.log('Location not changed. Take source location: ' + changeEmployment.Entities.SelectedLocation);
            }

            kendoHelper.ShowProgress();
            $('#area_equipments_overview').load("/Change/GetEquipmentsView",
                {
                    changeType: changeEmployment.Entities.SelectedChangeType.ChangeType,
                    targetDateIso8601: $('#TargetDate').data('kendoDatePicker').value().toISOString(),
                    guidCurrentEnterprise: changeEmployment.Entities.EnterpriseGuid,
                    guidCurrentEmployment: changeEmployment.Entities.EmploymentGuid,
                    guidEmploymentType: changeEmployment.Entities.SelectedEmploymentType,
                    guidCostCenter: changeEmployment.Entities.SelectedCostCenter,
                    guidEnterprise: changeEmployment.Entities.SelectedEnterprise,
                    guidLocation: changeEmployment.Entities.SelectedLocation


                }, function () {
                    kendoHelper.HideProgress();
                });
        }
    },

    OnChangedEmploymentSuccess: function (xhr) {

        //if (xhr.responseJSON.enterpriseName) {
        //    var tabContent = $('.k-tabstrip-wrapper a.k-link:contains("' + xhr.responseJSON.enterpriseName + '")').closest('.k-content');
        //    $(tabContent).find('#employment-change-section input').attr('disabled', 'disabled');
        //}
        formValidation.OnSuccess(xhr);
        kendoHelper.RefreshWindow(true);
    }
}

changeEmployment.Communication = {
    GetCostCenterResponsible: function (guid) {

        var data = { guid: guid };

        $.ajax({

            type: "POST",
            url: "/Costcenter/GetCostCenterResponsibleName",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {

                if (response.success) {
                    $('#costcenter-responsible').val(response.name);
                }
            },
            timeout: 15000
        });
    },

    CheckInputValidationFinishedForChangeType: function (changeType, valueTypes, idEnterprise) {
        var data = { changeType: changeType, valueTypes: valueTypes, idEnterprise: idEnterprise };

        $.ajax({

            type: "POST",
            url: "/Change/CheckInputValidationFinishedForChangeType",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {

                if (response.success && response.isFinished) {
                    if (response.mustShowEquipments || $('#ApproveEquipmentMove').attr('value') === 'true') {
                        $('#button_get_equipments').show();
                        // changeEmployment.Functions.DisableAllInputs();
                        $('#button_submit_change').attr('disabled', 'disabled');
                    }
                    else {
                        $('#button_submit_change').removeAttr('disabled');
                    }




                }
                //if (changeEmployment.Functions.CheckInputsFinished()) {
                //    switch (changeEmployment.Entities.SelectedChangeType.ChangeType) {
                //        case 5:
                //        case 2:
                //            $('#button_submit_change').removeAttr('disabled');
                //            break;
                //        case 0:
                //        case 1:
                //        case 3:
                //        case 4:
                //            changeEmployment.Functions.DisableAllInputs();
                //            $('#button_get_equipments').show();
                //            break;
                //    }
                //}


            }
        });
    }
}