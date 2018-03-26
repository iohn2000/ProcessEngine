personprofile = {};

personprofile.Entities = {
    LastKendoToolTip: null,
}

personprofile.UIEvents = {
    OnTabStripShow: function (e) {

        console.log('onContentLoad');
        //console.log('onTabStripShow');
    },
    OnSelect: function (e) {
        $('#tabStripBackground').show();
        kendo.ui.progress($('#tabStripBackground'), true);
        //console.log('onSelect');
    },
    OnActivate: function (e) {
        $('#tabStripBackground').hide();
        kendo.ui.progress($('#tabStripBackground'), false);
        //console.log('onActivate');
    },
    OnContentLoad: function (e) {

    }
}

personprofile.data = {
    GetEmploymentGuid: function (e) {
        return {
            empl_guid: $("#Empl_Guid").val(),
            ownerPersGuid: $("#ActingUserPersGuid").val()
        };
    }
}

personprofile.Events = {

    OnEquipmentMouseover: function (element, obreGuid, isAdmin) {
        if (!element.hasAttribute('HasWFStatus')) {
            $(element).attr('HasWFStatus', true);

            var width = 450;
            var height = 340;

            if (!isAdmin) {
                width = 300;
                height: 120;
            }

            $(element).kendoTooltip({

                position: "right",
                autoHide: false,
                width: width,
                height: height,
                content: {
                    url: '/ProcessEntity/StatusInformation/' + obreGuid + '/' + isAdmin + '/true'
                },
                show: function () {
                    if (personprofile.Entities.LastKendoToolTip !== null) {
                        personprofile.Entities.LastKendoToolTip.hide();
                    }

                    personprofile.Entities.LastKendoToolTip = this;
                },
                hide: function () {
                    if (personprofile.Entities.LastKendoToolTip !== null) {
                        personprofile.Entities.LastKendoToolTip = null;
                    }
                }

            }).data("kendoTooltip").show();

        }
    },

    OnEmploymentMouseover: function (element, guid, isAdmin) {
        if (!element.hasAttribute('HasWFStatus')) {
            $(element).attr('HasWFStatus', true);

            var width = 450;
            var height = 340;

            if (!isAdmin) {
                width = 300;
                height: 100;
            }


            $(element).kendoTooltip({

                position: "right",
                autoHide: false,
                width: width,
                height: height,
                content: {
                    url: '/ProcessEntity/StatusInformation/' + guid + '/' + isAdmin + '/true'
                },
                show: function () {
                    if (personprofile.Entities.LastKendoToolTip !== null) {
                        personprofile.Entities.LastKendoToolTip.hide();
                    }

                    personprofile.Entities.LastKendoToolTip = this;
                },
                hide: function () {
                    if (personprofile.Entities.LastKendoToolTip !== null) {
                        personprofile.Entities.LastKendoToolTip = null;
                    }
                }
            }).data("kendoTooltip").show();


        }
    },

    OnSuccessProfilePackageAddPackageToEmployment: function (xhr) {
        // find all grid IDs an update each Grid
        $('div[id^="ProfilePackage"]').each(function (index) {
            var packagegrid = $(this).data('kendoGrid');
            packagegrid.dataSource.read();
        });

        formValidation.OnSuccess(xhr);
    },

    OnSuccessChangedEquipments: function (xhr) {
        // find all grid IDs an update each Grid
        $('div[id^="ProfileEqu"]').each(function (index) {
            var packagegrid = $(this).data('kendoGrid');
            packagegrid.dataSource.read();
        });

        formValidation.OnSuccess(xhr);
    },


    OnChangedContactDataSuccess: function (xhr, callFormValidation) {


        if (xhr.responseJSON.contactDataViewModel) {

            var personContent = $('.contact-container');

            var tabName = $('.k-tabstrip-wrapper a.k-link:contains("' + xhr.responseJSON.tabName + '")').parent().attr('aria-controls');
            var tabContent = $('#' + tabName);

            for (var i = 0; i < xhr.responseJSON.contactDataViewModel.length; i++) {
                var contactModel = xhr.responseJSON.contactDataViewModel[i];

                var selector = 'div[data-id="' + contactModel.C_CT_ID + '"][data-isFuture="' + contactModel.IsFuture + '"]';


                var text = contactModel.Text;
                if (contactModel.C_CT_ID === 7) {

                    if (contactModel.Text && contactModel.Text.length > 0) {
                        text = '<a target="_blank" href="/Home/Index?ATR=ZimmerNr&amp;TeleSuchString=' + contactModel.Number + '&amp;ELID=' + contactModel.ELID + '">' + contactModel.Text + '</a>';
                    }
                }

                $(tabContent).find(selector).html(text);
                if (xhr.responseJSON.isMain === true) {
                    $(personContent).find(selector).html(text);
                }
            }
        }

        if (callFormValidation === true) {
            formValidation.OnSuccess(xhr);
        }

    },

    OnChangedEmploymentDataSuccess: function (xhr) {
        if (xhr.responseJSON.employment) {
            var personellNumber = xhr.responseJSON.employment.PersNr;
            var tabContent = $('.k-tabstrip-wrapper a.k-link:contains("' + xhr.responseJSON.employment.EnterpriseName + '")').closest('.k-content');
            $(tabContent).find('#personellNumber').html(personellNumber);
        }

        // do refresh, after last window is closed
        if (xhr.responseJSON.doSiteReload) {
            kendoHelper.RefreshWindow(true);
        }

        formValidation.OnSuccess(xhr);
    }
}

personprofile.Equipment = {};

personprofile.Equipment.Functions = {

    Delete: function (empl_guid, obre_guid, entityName, name, gridname) {

        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                personprofile.Equipment.Communication.Delete(empl_guid, obre_guid, gridname);
            },
            null
        );
    }
};

personprofile.Equipment.Communication = {
    RenderProcessStatus: function (guidEntity, htmlElement) {
        var data = { guidEntity: guidEntity };

        $.ajax({

            type: "POST",
            url: "/ProcessEntity/GetEntityStatus",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    //$('#orgunit-linemanager').html(response.lineManager)
                    var text = $(htmlElement).attr('title');
                    text = text + ' ProcessInformation:<br> ' + response.processEntityHtml;
                    $(htmlElement).attr('title', text);
                }

            },
            timeout: 20000
        });
    },

    GetProcessStatus: function (guidEntity) {
        var data = { guidEntity: guidEntity };

        $.ajax({

            type: "POST",
            url: "/ProcessEntity/GetEntityStatus",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    return response.processEntityHtml;
                }
                else {
                    return 'status update failed';
                }

            },
            timeout: 20000
        });
    },

    Delete: function (empl_guid, obre_guid, gridname) {
        kendoHelper.ShowProgress();
        var data = { empl_guid: empl_guid, obre_guid: obre_guid };

        $.ajax({

            type: "POST",
            url: "/Employment/RemoveEquipmentInstanceFromEmployment",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh(gridname);
                    alertify.success(res.PersonProfileEquipment.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.PersonProfileEquipment.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.PersonProfileEquipment.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.PersonProfileEquipment.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};

personprofile.Package = {};

personprofile.Package.Functions = {

    Delete: function (empl_guid, pack_guid, entityName, name, gridname) {

        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                personprofile.Package.Communication.Delete(empl_guid, pack_guid, gridname);
            },
            null
        );
    }
};

personprofile.Package.Communication = {
    Delete: function (empl_guid, pack_guid, gridname) {
        kendoHelper.ShowProgress();
        var data = { empl_guid: empl_guid, pack_guid: pack_guid };

        $.ajax({

            type: "POST",
            url: "/Employment/RemovePackageFromEmployment",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh(gridname);
                    alertify.success(res.PersonProfilePackage.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.PersonProfilePackage.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.PersonProfilePackage.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.PersonProfilePackage.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
};


