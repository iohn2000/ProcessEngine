workflow = {};

workflow.Configuration = {
    UseXonomy: false
}

workflow.Data = {
    XonomyDefinition: {
        validate: function (jsElement) {
            if (typeof (jsElement) === "string") jsElement = Xonomy.xml2js(jsElement);
            var valid = true;
            var elementSpec = this.elements[jsElement.name];
            if (elementSpec.validate) {
                elementSpec.validate(jsElement); //validate the element
            }
            for (var iAttribute = 0; iAttribute < jsElement.attributes.length; iAttribute++) {
                var jsAttribute = jsElement.attributes[iAttribute];
                var attributeSpec = elementSpec.attributes[jsAttribute.name];
                if (attributeSpec.validate) {
                    if (!attributeSpec.validate(jsAttribute)) valid = false; //validate the attribute
                }
            }
            for (var iChild = 0; iChild < jsElement.children.length; iChild++) {
                if (jsElement.children[iChild].type === "element") {
                    var jsChild = jsElement.children[iChild];
                    if (!this.validate(jsChild)) valid = false; //recurse to the child element
                }
            }
            return valid;
        },

        elements: {
            "variables": {
                menu: [
                          {
                              caption: "New <variable> TAG",
                              action: Xonomy.newElementChild,
                              actionParameter: "<variable name='' direction='' dataType=''></variable>",
                              hideIf: function (jsElement) { return false }
                          }
                ]

            },
            "variable": {
                collapsible: false,
                menu: [
                 {
                     caption: "Delete",
                     action: Xonomy.deleteElement,
                     actionParameter: null,
                     hideIf: function (jsElement) { return false }
                 }
                ],
                canDropTo: [],
                mustBeAfter: [],
                mustBeBefore: [],

                hasText: true,
                inlineMenu: [],

                attributes: {
                    "direction": {
                        asker: Xonomy.askPicklist,
                        askerParameter: [
                            { value: "input", caption: "input" },
                            { value: "output", caption: "output" },
                            { value: "both", caption: "both" }
                        ]
                    },
                    "dataType": {
                        asker: Xonomy.askPicklist,
                        askerParameter: [
                            { value: "stringType", caption: "stringType" },
                            { value: "intType", caption: "intType" },
                            { value: "dateType", caption: "dateType" },
                            { value: "boolType", caption: "boolType" }
                        ]
                    },
                    "name": {
                        asker: Xonomy.askString,
                        askerParameter: {},
                    }
                }

            },
            "Taskfields": {
                collapsible: false,
                menu: [
                           {
                               caption: "New <Field> TAG",
                               action: Xonomy.newElementChild,
                               actionParameter: "<Field type='' id='' name='' description=''></Field>",
                               hideIf: function (jsElement) { return false }
                           }
                ],
            },
            "Field": {
                collapsible: false,
                menu: [
                            {
                                caption: "New <option> TAG",
                                action: Xonomy.newElementChild,
                                actionParameter: "<option value=''></option>",
                                hideIf: function (jsElement) { return false }
                            },
                            {
                                caption: "Delete",
                                action: Xonomy.deleteElement,
                                actionParameter: null,
                                hideIf: function (jsElement) { return false }
                            }
                ],
                attributes: {
                    "type": {
                        asker: Xonomy.askPicklist,
                        askerParameter: [
                            { value: "dropdown", caption: "dropdown" },
                            { value: "multiselect", caption: "multiselect" },
                            { value: "radiobutton", caption: "radiobutton" },
                            { value: "textbox", caption: "textbox (no option TAGS!!!)" },
                            { value: "date", caption: "date (no option TAGS!!!)" },
                            { value: "datetime", caption: "datetime (no option TAGS!!!)" }
                        ],
                    },
                    "id": {
                        asker: Xonomy.askString,
                        askerParameter: {},
                        menu: []
                    },
                    "name": {
                        asker: Xonomy.askString,
                        askerParameter: {},
                        menu: []
                    },
                    "description": {
                        asker: Xonomy.askString,
                        askerParameter: {},
                        menu: []
                    }
                }
            },
            "option": {
                hasText: true,
                collapsible: false,
                inlineMenu: [],
                menu: [

                            {
                                caption: "Delete",
                                action: Xonomy.deleteElement,
                                actionParameter: null,
                                hideIf: function (jsElement) { return false }
                            }

                ],
                attributes: {
                    "value": {
                        asker: Xonomy.askString,
                        askerParameter: {},
                        menu: []

                    }
                }
            },

            "linkedTo": {
                attributes: {
                    "instance": {
                        asker: Xonomy.askString,
                        askerParameter: {},
                        menu: []

                    }
                }
            },
            "properties": {
                collapsed: function (jsElement) { return true; }
                //hideIf: function (jsElement) {
                //    return true;
                //}
            },
            "activities": {
                menu: [
                         {
                             caption: "Add Activity",
                             action: function (jsElement) { workflow.Events.OnButtonShowActivitiesWindowClick(this); }

                         }
                ]
            },
            "activity": {
                menu: [
                          {
                              caption: "New <transition> TAG",
                              action: Xonomy.newElementChild,
                              actionParameter: "<transition to=''></transition>",
                              hideIf: function (jsElement) { return false }
                          },
                           {
                               caption: "Delete",
                               action: Xonomy.deleteElement,
                               actionParameter: null,
                               hideIf: function (jsElement) { return false }
                           }
                ]

            },
            "condition": {
                asker: Xonomy.askString,
                askerParameter: {},
                hasText: true,
                inlineMenu: [],
                menu: [
                 {
                     caption: "Delete",
                     action: Xonomy.deleteElement,
                     actionParameter: null,
                     hideIf: function (jsElement) { return false }
                 }
                ]
            },
            "transition": {
                menu: [

                    {
                        caption: "New <condition> TAG",
                        action: Xonomy.newElementChild,
                        actionParameter: "<condition></condition>",
                        hideIf: function (jsElement) { return false }
                    },
                   {
                       caption: "Delete",
                       action: Xonomy.deleteElement,
                       actionParameter: null,
                       hideIf: function (jsElement) { return false }
                   }
                ],
                attributes: {
                    "to": {
                        asker: Xonomy.askString,
                        askerParameter: {},
                        menu: []

                    }
                }
            },
        }
    }
}

workflow.Events = {

    OnComboBoxActivitySelect: function (e) {
        workflow.Entities.SelectedActivity = this.dataItem(e.item.index());

        if (workflow.Entities.SelectedActivity.ActivityType === 1) {
            $('#area-workflow').css('visibility', 'visible');
        }
        else {
            $('#area-workflow').css('visibility', 'hidden');
            workflow.Entities.SelectedWorfklow = {};
        }
    },

    OnComboBoxWorkflowSelect: function(e){
        workflow.Entities.SelectedWorfklow = this.dataItem(e.item.index());
    },

    OnButtonAddSelectedActivityClick: function (sender) {
        var xml = workflow.Functions.GetXml();

        var idWorkflow = '';

        if (workflow.Entities.SelectedWorfklow) {
            idWorkflow = workflow.Entities.SelectedWorfklow.Value;
        }

        workflow.Communication.GetWorkflowXmlWithNewActivityId(xml, workflow.Entities.SelectedActivity.Id, idWorkflow, sender);
    },

    OnButtonShowActivitiesWindowClick: function (sender) {

        var wintitle = 'Add Activity';
        $("<div id='modal-window-activities'></div>").kendoWindow({
            title: wintitle,
            content: 'AddActivity',
            modal: true,
            deactivate: function () {
                this.destroy();
            },
            visible: true,
            //refresh: function () { winElement.find(".k-loading-mask").remove(); }
        }).data("kendoWindow").open().center().title(wintitle);
    },

    OnEditClick: function (e) {
        e.preventDefault();
        var tr = $(e.target).closest("tr");
        var data = this.dataItem(tr);
        showInWindow('/Workflow/Configure/' + data.IdWorkflow + '/true', 'Configure Workflow');
    },

    OnButtonShowWorkflowImageClick: function (e) {
        var xml = workflow.Functions.GetXml();

        workflow.Communication.GetWorkflowImage(xml);
    },

    OnButtonOpenWorkflowImageViewClick: function (e) {

        $('#button-showImage').attr('disabled', 'disabled');

        var currentWindow = kendoHelper.FindWindowName();
        var kendoWindow = $("#" + currentWindow).data("kendoWindow");
        kendoWindow.wrapper.css({
            top: 0, left: 0, top: 0, width: "48%", height: "90%"
        });



        showInWindow("WorkflowImage", "Workflow Graph", false, false);

        var currentWindow2 = kendoHelper.FindWindowName();
        var kendoWindow2 = $("#" + currentWindow2).data("kendoWindow");
        kendoWindow2.bind("close", function (e) {
            kendoWindow.wrapper.css({
                width: "90%"
            });
            kendoWindow.center();
            $('#button-showImage').removeAttr('disabled');
        });
    }
}

workflow.Entities = {
    IdWorkflow: -1,
    SelectedActivity: {},
    SelectedWorfklow: {},
    isCheckedOut: false,
    Xml: '',
    Username: ''
}

workflow.Functions = {
    RenderXml: function () {

        if (workflow.Helper.HasViewXmlEditor()) {

            if (helper.IsInternetExplorer() || !workflow.Configuration.UseXonomy) {
                var isDisabled = '';
                if (!workflow.Entities.isCheckedOut) {
                    isDisabled = 'disabled';
                }

                if (workflow.Configuration.UseXonomy) {
                    $('#workflowEditor').html('<div class="infobox">To edit the XML in a tree structure, please use Chrome or Firefox!</div><textarea id="workflowTextArea" ' + isDisabled + ' style="width: 100%; height: 600px">' + workflow.Entities.Xml + '</textarea>');
                }
                else {
                    $('#workflowEditor').html('<textarea id="workflowTextArea" ' + isDisabled + ' data-autoresize style="width: 100%; min-height: 400px">' + workflow.Entities.Xml + '</textarea>');
                }

                helper.Functions.AutoResize();
            }
            else {
                if (workflow.Entities.Xml.length > 0) {
                    var editor = document.getElementById('workflowEditor');
                    if (workflow.Entities.isCheckedOut) {
                        Xonomy.render(workflow.Entities.Xml, editor, workflow.Data.XonomyDefinition);
                    }
                    else {
                        Xonomy.render(workflow.Entities.Xml, editor);
                    }
                }
            }

        }
    },

    GetXml: function () {
        var xml = '';
        if (helper.IsInternetExplorer() || !workflow.Configuration.UseXonomy) {
            xml = $('#workflowTextArea').val();
        }
        else {
            xml = Xonomy.harvest();
        }
        return xml;
    },

    OnWorkflowDefinitionSaveSuccess: function (response) {
        kendoHelper.HideProgress();
        alertify.success(res.Workflow.Alert.Save_Success);
    },

    Delete: function (guid, entityName, name) {
        alertify.confirm(res.Confirmation.Title_Delete, res.GetRes(res.Confirmation.Detail_Delete_P_EntityName, [entityName, name]),
            function () {
                workflow.Communication.Delete(guid);
            },
            null
        );
    },

    RenderImage: function (base64String) {
        $("#imgWorkflow").attr('src', base64String);
    }
}


workflow.Communication = {
    GetWorkflowImage: function (xml) {
        kendoHelper.ShowProgress();
        var data = { workflowxml: xml };

        $.ajax({

            type: "POST",
            url: "GetWorkflowImage",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    workflow.Functions.RenderImage(response.base64image);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },

    GetWorkflowXmlWithNewActivityId: function (xml, idActivity, idWorkflow, sender) {
        kendoHelper.ShowProgress();
        var data = { xml: xml, idActivity: idActivity, idWorkflow : idWorkflow };

        $.ajax({

            type: "POST",
            url: "GetWorkflowXmlWithNewActivityId",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.CloseWindow(sender);
                    workflow.Entities.Xml = response.xml;
                    workflow.Functions.RenderXml();
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorMessage);
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },

    GetWorkflowXml: function () {
        kendoHelper.ShowProgress();
        var data = { idWorkflow: workflow.Entities.IdWorkflow };

        $.ajax({

            type: "POST",
            url: "GetWorkflowXml",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {

                    workflow.Entities.Xml = response.xml;
                    workflow.Functions.RenderXml();
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorMessage);
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },


    Checkout: function (id) {

        kendoHelper.ShowProgress();
        $.ajax({
            type: "POST",
            url: "Checkout",
            data: '{ "idWorkflow": "' + id + '" }',
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    workflow.Entities.isCheckedOut = true;
                    workflow.Helper.UpdateCheckoutState(true, response.username, response.version);
                    workflow.Functions.RenderXml();
                    alertify.success(res.Workflow.Alert.Checkout_Success);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },
    Checkin: function (id) {
        kendoHelper.ShowProgress();
        $.ajax({
            type: "POST",
            url: "Checkin",
            data: '{ "idWorkflow": "' + id + '" }',
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    workflow.Entities.isCheckedOut = false;
                    workflow.Helper.UpdateCheckoutState(false, response.username, response.version);
                    workflow.Entities.Xml = response.xml;
                    workflow.Functions.RenderXml();
                    alertify.success(res.Workflow.Alert.Checkin_Success);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },
    UndoCheckout: function (id) {
        kendoHelper.ShowProgress();
        $.ajax({
            type: "POST",
            url: "UndoCheckout",
            data: '{ "idWorkflow": "' + id + '" }',
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    workflow.Entities.isCheckedOut = false;
                    workflow.Helper.UpdateCheckoutState(false, response.username, response.version);
                    workflow.Entities.Xml = response.xml;
                    workflow.Functions.RenderXml();
                    workflow.Helper.UpdateFormFields(response.model);

                    alertify.success(res.Workflow.Alert.Checkout_Undo);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },
    Save: function (id) {
        var xml = workflow.Functions.GetXml();
        kendoHelper.ShowProgress();
        var data = { idWorkflow: id, xml: xml };
        $.ajax({
            type: "POST",
            url: "/Workflow/SaveWorkflow",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    workflow.Entities.Xml = response.xml;
                    alertify.success(res.Workflow.Alert.Save_Success);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorMessage);
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    },
    Delete: function (guid, gridId) {
        kendoHelper.ShowProgress();
        var data = { idWorkflow: guid };

        $.ajax({

            type: "POST",
            url: "/Workflow/DeleteWorkflow",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {
                kendoHelper.HideProgress();
                if (response.success) {
                    kendoHelper.Grid.Refresh(gridId);
                    alertify.success(res.Workflow.Alert.Delete_Success);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, response.errorModel.ErrorMessage);
                    exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(response.errorModel));
                }
            },
            error: function (x, t, m) {
                kendoHelper.HideProgress();
                if (t === "timeout") {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.Timeout);
                }
                else {
                    alertify.alert(res.Workflow.Alert.Title_Error, res.Alert.GeneralError);
                }
            },
            timeout: 15000
        });
    }
}

workflow.Helper = {
    UpdateCheckoutState: function (isCheckedOut, username, version) {
        $('#workflow-checkoutname').html(username);
        $('#workflow-version').html(version);

        var validFrom = $('#ValidFrom').data('kendoDateTimePicker');
        var validTo = $('#ValidTo').data('kendoDateTimePicker');

        if (isCheckedOut && username.toLowerCase() !== workflow.Entities.Username.toLowerCase()) {
            $('#button-checkout').attr('disabled');
            $('#button-checkin').attr('disabled', 'disabled');
            $('#button-undocheckout').attr('disabled', 'disabled');
            $('#button-activities').attr('disabled', 'disabled');
            $('#button-save').attr('disabled', 'disabled');

            $('form input').attr('disabled', 'disabled');
            $('form textarea').attr('disabled', 'disabled');
            if (validFrom) {
                validFrom.enable(false);
            }
            if (validTo) {
                validTo.enable(false);
            }
        }
        else if (isCheckedOut === true) {
            $('#button-checkout').attr('disabled', 'disabled');
            $('#button-checkin').removeAttr('disabled');
            $('#button-undocheckout').removeAttr('disabled');
            $('#button-activities').removeAttr('disabled');
            $('#button-save').removeAttr('disabled');

            $('form input').removeAttr('disabled');
            $('form textarea').removeAttr('disabled');
            if (validFrom) {
                validFrom.enable(true);
            }
            if (validTo) {
                validTo.enable(true);
            }


        }
        else {
            $('#button-checkout').removeAttr('disabled');
            $('#button-checkin').attr('disabled', 'disabled');
            $('#button-undocheckout').attr('disabled', 'disabled');
            $('#button-activities').attr('disabled', 'disabled');
            if (workflow.Entities.IdWorkflow && workflow.Entities.IdWorkflow.length > 0)
            {
                $('#button-save').attr('disabled', 'disabled');

                $('form input').attr('disabled', 'disabled');
                $('form textarea').attr('disabled', 'disabled');
                if (validFrom) {
                    validFrom.enable(false);
                }
                if (validTo) {
                    validTo.enable(false);
                }
            }
        }


        workflow.Functions.RenderXml();

        var kendoGrid = $('#Grid').data('kendoGrid');
        if (kendoGrid) {
            kendoGrid.dataSource.read();
            kendoGrid.refresh();
        }
    },

    UpdateFormFields: function (model) {
        var validFrom = $('#ValidFrom').data('kendoDateTimePicker');
        var validTo = $('#ValidTo').data('kendoDateTimePicker');

        $('#Name').val(model.Name);
        $('#Description').val(model.Name);

        if (validFrom) {

            var fromValue = null;
            if (model.ValidFrom) {
                fromValue = new Date(parseInt(model.ValidFrom.substr(6)));
            }

            validFrom.value(fromValue);
        }

        if (validFrom) {
            var toValue = null;
            if (model.ValidTo) {
                toValue = new Date(parseInt(model.ValidTo.substr(6)));
            }

            validTo.value(toValue);
        }
    },

    HasViewXmlEditor: function () {
        return $('#workflowEditor').length > 0 ? true : false;
    }
}

