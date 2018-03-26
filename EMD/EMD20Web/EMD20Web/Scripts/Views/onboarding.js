var onboarding = {}

/*

Tabstrips
0 = Employment
1 = Contact Data
2 = Additional Data
3 = Equipments

*/


onboarding.Entities = {
    SelectedPackageEnterprise: null,
    SelectedPackageLocation: null,
    SelectedOrgunit: null,
    IsEquipmentsTabEnabled: false,
    IsKcc: false,
    Enterprise_ID: 0
}

onboarding.UIElements = {
    KendoDropDownEnterprisePackage: null,
    KendoDropDownLocationPackage: null
}

onboarding.Data = {
    EnterpriseParameters: function (e) {
        return {
            ente_guid: $("#Enterprise").val(),
            isOnboarding: true
        };

    },
    LocationPackagesParameters: function (e) {
        return {
            loca_guid: $("#Location").val(),
            isOnboarding: true
        };
    },
    OrgunitParameters: function (e) {
        return {
            ente_guid: $("#Enterprise").val(),
            text: $("#OrgUnit").data("kendoDropDownList").filterInput.val()
        };
    },
    SponsorParameters: function (e) {
        return {
            ente_guid: $("#Enterprise").val()
        };
    }
}

onboarding.Functions = {
    ShowHideInputsForEmploymentType: function (employmentType) {
        /*
         * EnumEmploymentTypeCategory:
         * Internal = 1, External = 2, Other = 3
        */
        if (employmentType.Data.EnumEmploymentTypeCategory === 2) {
            $("#DivEMailType").css("display", "normal");
            //$("#EMailType").attr("data-val", "true");
            $("#EMailType").attr('required', 'required');
            $("#EMailType").attr('data-val-required', 'Required field');
            $("#DivPersNr").hide();
        }
        else if (employmentType.Data.EnumEmploymentTypeCategory === 1) {
            $("#DivEMailType").css("display", "none");
            //$("#EMailType").removeAttr("data-val");
            $("#EMailType").removeAttr('required');
            $("#EMailType").removeAttr('data-val-required');
            $("#DivPersNr").show();
        }
        else {
            $("#DivEMailType").css("display", "none");
            //$("#EMailType").removeAttr("data-val");
            $("#EMailType").removeAttr('required');
            $("#EMailType").removeAttr('data-val-required');
            $("#DivPersNr").hide();
        }

        if (employmentType.Data.MustHaveSponsor) {
            $("#DivSponsor").show();
            $("#SponsorGuid").attr('required', 'required');
            $("#SponsorGuid").attr('data-val-required','Required field');
        }
        else {
            $("#SponsorGuid").removeAttr('required');
            $("#SponsorGuid").removeAttr('data-val-required');
            $("#DivSponsor").hide();
        }
    }
}

onboarding.Events = {
    OnEnterprisePackageChanged: function (e) {
        onboarding.Entities.SelectedPackageEnterprise = this.dataItem(e.item);

        onboarding.Communication.RenderEquipmentView(onboarding.Entities.SelectedPackageEnterprise ? onboarding.Entities.SelectedPackageEnterprise.Value : null, onboarding.Entities.SelectedPackageLocation ? onboarding.Entities.SelectedPackageLocation.Value : null);
    },
    OnLocationPackageChanged: function (e) {
        onboarding.Entities.SelectedPackageLocation = this.dataItem(e.item);

        onboarding.Communication.RenderEquipmentView(onboarding.Entities.SelectedPackageEnterprise ? onboarding.Entities.SelectedPackageEnterprise.Value : null, onboarding.Entities.SelectedPackageLocation ? onboarding.Entities.SelectedPackageLocation.Value : null);
    },
    OnLocationChanged: function (e) {
        $("#LocationPackages").data("kendoDropDownList").dataSource.read();
    },
    OnOrgunitChange: function (e) {
        if (this.dataItem) {
            onboarding.Entities.SelectedOrgunit = this.dataItem(e.item);

            onboarding.Communication.RenderLineManager(onboarding.Entities.SelectedOrgunit ? onboarding.Entities.SelectedOrgunit.Value : null);
        }
    },
    OnButtonNextClick: function (e) {
        var tabStrip = $("#tabstrip").data("kendoTabStrip");

        selectedTabStrip = tabStrip.select();
        var nextIndex = tabStrip.select().index() + 1;

        if (nextIndex > 3) {
            // there are only 3 available
            return;
        }

        if (nextIndex == 2 && onboarding.Entities.IsKcc === false) {
            nextIndex = 3;
        }

        tabStrip.select(nextIndex);
    },

    OnButtonPreviousClick: function (e) {
        var tabStrip = $("#tabstrip").data("kendoTabStrip");
        var previousIndex = tabStrip.select().index() - 1;

        if (previousIndex == 2 && onboarding.Entities.IsKcc === false) {
            previousIndex = 1;
        }

        tabStrip.select(previousIndex);
    },

    OnOnboardingSuccess: function (xhr) {
        formValidation.OnSuccess(xhr);
        kendoHelper.RefreshWindow(true);
   
    },

    OnSelectEmploymentType: function (e) {
        var employmentType = this.dataItem(e.item);

        onboarding.Functions.ShowHideInputsForEmploymentType(employmentType);




    }
}

onboarding.Communication = {
    RenderEquipmentView: function (package1, package2) {
        var tabStrip = $("#tabstrip").data("kendoTabStrip");
        if (!package1 && !package2) {
            tabStrip.disable(tabStrip.items()[3]);
            onboarding.Entities.IsEquipmentsTabEnabled = false;
        }
        else {
            tabStrip.enable(tabStrip.items()[3]);
            onboarding.Entities.IsEquipmentsTabEnabled = true;
        }

        $("#ContentEquipment").load("/Onboarding/GetEquipmentView",
         { package1: package1, package2: package2 });

    },

    RenderLineManager: function (orgunitGuid) {
        var data = { guidOrgunit: orgunitGuid };

        $.ajax({

            type: "POST",
            url: "/Employment/GetTeamLeader",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    $('#orgunit-linemanager').html(response.lineManager)
                }

            },
            timeout: 20000
        });
    }

}


function onEnterpriseSelect(e) {
    var tabStrip = $("#tabstrip").data("kendoTabStrip");
    var dataItem = this.dataItem(e.item);
    onboarding.Entities.Enterprise_ID = dataItem.Data.E_ID;
    onboarding.Entities.IsKcc = dataItem.Data.isKcc;
    if (onboarding.Entities.IsKcc == true) {
        $($("#tabstrip").data("kendoTabStrip").items()[2]).attr("style", "display:inline-block");

        //   $('#tabstrip-2 input').removeAttr('disabled');
        $('#tabstrip-3 input').removeAttr('disabled');

    }
    else {
        $($("#tabstrip").data("kendoTabStrip").items()[2]).attr("style", "display:none");


        //   $('#tabstrip-2 input').attr('disabled', 'disabled');
        $('#tabstrip-3 input').attr('disabled', 'disabled');
    }


    $("#EnterprisePackages").data("kendoDropDownList").dataSource.read();
    $("#Location").data("kendoDropDownList").dataSource.read();
}


/*
 * Possible index values 
 * 0 employment - always enabled
 * 1 contact data - always enabled
 * 2 addtional data - enabled if onboarding.Entities.IsKcc == true
 * 3 equipments - enabled if onboarding.Entities.IsEquipmentsTabEnabled == true
 */
function onSelect(e) {

    // the index of the html-id (from kendo) has different index (index +1)
    var isValid = $("#tabstrip-" + (selectedTabStrip.index() + 1)).kendoValidator().data("kendoValidator").validate();

    // prevent next step
    if (!isValid) {
        alertify.error(res.Alert.FormValidationTabError);
        e.preventDefault()
        return;
    }


    // the first index starts with 0
    var selectedIndex = $(e.item).index();
    selectedTabStrip = $(e.item).select();

    var hasNextItem = true

    if (selectedIndex == 2 && onboarding.Entities.IsKcc === true && onboarding.Entities.IsEquipmentsTabEnabled === false) {
        hasNextItem = false;
    }

    if (selectedIndex == 1 && onboarding.Entities.IsKcc === false && onboarding.Entities.IsEquipmentsTabEnabled === false) {
        hasNextItem = false;
    }

    if (selectedIndex >= 3) {
        hasNextItem = false;
    }


    if (selectedIndex == 0) {
        $("#Previous").attr('disabled', 'disabled');
    }
    else {
        $("#Previous").removeAttr('disabled');
    }

    if (hasNextItem) {
        $("#Next").removeAttr('disabled');
    }
    else {
        $("#Next").attr('disabled', 'disabled');
    }

    if (selectedIndex == 2) {
        toggleNoApprovalNeededReason();
    }
}




function toggleNoApprovalNeededReason() {
    if ($('#NoApprovalNeeded').is(":checked") == true) {
        $("#DivNoApprovalNeededReason").show();
        $("#NoApprovalNeededReason").attr('data-val-required', 'Required field');
    }
    else {
        $("#DivNoApprovalNeededReason").hide();
        $("#NoApprovalNeededReason").removeAttr('data-val-required');
    }
}

