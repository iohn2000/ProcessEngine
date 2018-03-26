
/*
 * Deactivate Autocomplete in all grids
 * see example at: http://dojo.telerik.com/OReXeQ (link from: http://www.telerik.com/forums/turning-off-autocomplete-on-column-filtering)
 * 
 */
//kendo.ui.FilterCell.fn.options.template = function (e) {
//    e.element.kendoAutoComplete({
//        serverFiltering: false,
//        valuePrimitive: true,
//        noDataTemplate: ''
//    });
//}



function showInWindow(uri, wintitle, isiframe, isModal) {


    if (isModal === undefined) {
        isModal = true;
    }

    wnd = window.parent.$("#window").data("kendoWindow");
    if (wnd != null)
    { wnd.destroy(); }
    else
    {
        wnd = $("#window").data("kendoWindow");
        if (wnd != null) { wnd.destroy(); }
    }

    isiframe = typeof isiframe !== 'undefined' ? isiframe : false;


    var name = 'window';
    var tempName = name;
    windowIndex = 0;

    while (true) {
        tempName = name + windowIndex;

        var exists = $('#' + tempName).length > 0;
        if (!exists) {
            name = tempName;
            break;
        }
        windowIndex++;
    }

  

    if (!isModal) {
        my_win = $("<div id='" + name + "'></div>").kendoWindow({
            width: "90%",
            height: "80%",
            title: wintitle,
            content: uri,
            modal: isModal,
            deactivate: function () {

                this.destroy();
            },
            appendTo: "#modwin",
            visible: true,
            actions: ["NewWindow", "Refresh", "Close"]
            //refresh: function () { winElement.find(".k-loading-mask").remove(); }
        }).data("kendoWindow").title(wintitle).wrapper.css({ top: 0, right: 0, left: "auto", width: "48%", height: "90%" }); //.addClass("kapschmodalwindow");
    }
    else {

        my_win = $("<div id='" + name + "'></div>").kendoWindow({
            width: "90%",
            height: "80%",
            title: wintitle,
            content: uri,
            modal: isModal,
            deactivate: function () {

                this.destroy();
            },
            iframe: isiframe,
            appendTo: "#modwin",
            visible: true,
            actions: ["HyperLink-Open", "Refresh", "Close"]
            //refresh: function () { winElement.find(".k-loading-mask").remove(); }
        }).data("kendoWindow").open().center().title(wintitle); //.addClass("kapschmodalwindow");
    }

    if (my_win !== undefined && my_win.wrapper !== undefined) {
        my_win.wrapper.addClass("kapschmodalwindow");

        my_win.wrapper.find(".k-i-hyperlink-open").click(function (e) {

            var uriBlank = uri.replace("/true", "");
            window.open(uriBlank, '_blank');

            //    $("#time-foo").html(returnTimeString());
        });

        if (uri.toLowerCase().indexOf('personprofile') > 0) {
            helper.Functions.Performance.DoShow = true;
            // reset the timer
            helper.Functions.Performance.SetStarted();
            my_win.wrapper.find(".k-icon.k-i-refresh").click(function (e) {
                helper.Functions.Performance.SetStarted();
            });



        }
    }

    wintitle = '<span>' + wintitle + '</span><span class="info-tooltip popup" style="display: none"><i class="material-icons" data-role="tooltip">info</i></span>';
    $('#' + name + '_wnd_title').html(wintitle);
}

function closeWindow(isiframe) {

    var name = kendoHelper.FindWindowName();


    isiframe = typeof isiframe !== 'undefined' ? isiframe : false;
    var wnd;
    if (isiframe) {
        wnd = window.parent.$("#" + name).data("kendoWindow");
    }
    else
        wnd = $("#" + name).data("kendoWindow");

    if (wnd) {
        wnd.destroy();
    } else {
        // error for finding kendoWindow
        // close window via click event
        console.log("kendoWindow can't be found - closing must be done manualy!");

        var windowInner = $('#' + name);
        if (windowInner) {
            $('#' + name).parent().find('a[aria-label="Close"] span').click()
        }
    }


}




function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)", "i"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

//function filterGrid(gridname, fieldname, operator, url) {
//    var filtervalue = getParameterByName(fieldname, url);
//    if (filtervalue != '' && filtervalue != null && filtervalue != 'undefined')
//    {
//        grid = $(gridname).data("kendoGrid");
//        grid.dataSource.filter({ field: fieldname, operator: operator, value: filtervalue });
//        var view = dataSource.view();
//    }
//}



kendoHelper = {

    /*
     *http://docs.telerik.com/kendo-ui/framework/globalization/numberformatting
     */
    IntegerFilter: function (args) {
        args.element.kendoNumericTextBox({
            format: "#",
            decimals: 0
        });
    },

    DisableAutocompleteFilter: function (e) {
        e.element.kendoAutoComplete({
            serverFiltering: false,
            valuePrimitive: true,
            noDataTemplate: ''
        });
    },

    KeyDownAutocompleteFilter: function (e) {
        e.element.kendoAutoComplete({
            serverFiltering: false,
            valuePrimitive: true,
            noDataTemplate: ''
        });

        try {
            //$(e.element).keypress(function (e) {
            //    console.log(e);
            //    if (e.keyCode != 13) {
            //        //$(e.target).trigger(jQuery.Event('keypress', { keycode: 13 }));
            //        var event = $.Event("keypress");
            //        event.keyCode = 13; // # Some key code value
            //        event.charCode = 13;
            //        $(e.target).trigger(event);
            //    }
            //});

            $(e.element).keydown(function (e) {
                console.log(e);
                if (e.keyCode != 13 && e.keyCode != 8 && e.keyCode != 46) {
                    //$(e.target).trigger(jQuery.Event('keypress', { keycode: 13 }));
                    var event = $.Event("keydown");
                    event.keyCode = 13; // # Some key code value
                    event.charCode = 13;
                    $(e.target).trigger(event);
                }
            });
        }
        catch (ex) {
            console.log(ex);
        }
    },

    FindWindowName: function () {
        var name = 'window';
        var tempName = name;
        windowIndex = 10;

        while (true) {
            tempName = name + windowIndex;

            var exists = $('#' + tempName).length > 0;
            if (exists) {
                name = tempName;
                break;
            }
            windowIndex--;

            if (windowIndex < -1) {
                break;
            }
        }

        return name;
    },

    CloseWindow: function (e) {
        if (e) {
            $(e).closest("[data-role=window]").data("kendoWindow").close();
        }
        else {
            var name = kendoHelper.FindWindowName();
            var window = $("#" + name).data("kendoWindow");
            if (window) {
                window.close();
            }
        }

    },

    /**
    * Show a progress bar over a specifig TAG ID
    * If the id is empty the default Layout ID is taken
    * @param {string} id id for the TAG 
    */
    ShowProgress: function (id) {
        if (!id) {
            id = kendoHelper.FindWindowName();
        }

        if (id) {
            kendo.ui.progress($('#' + id), true);
        }
    },

    /**
    * Hide a progress bar over a specifig TAG ID
    * If the id is empty the default Layout ID is taken
    * @param {string} id id for the TAG 
    */
    HideProgress: function (id) {
        if (!id) {
            id = kendoHelper.FindWindowName();
        }

        if (id) {
            kendo.ui.progress($('#' + id), false);
        }
    },

    GetWindowUrl: function () {
        var winName = kendoHelper.FindWindowName();
        wnd = window.parent.$("#" + winName).data("kendoWindow");

        var url = '';
        if (wnd != null)
        { url = wnd.options.content.url; }
        else
        { url = window.location.href; }

        return url;
    },

    RefreshWindow: function (refreshLower) {
        var name = kendoHelper.FindWindowName();
        if (name !== 'window') {
            if (refreshLower && refreshLower === true) {
                var number = name.replace('window', '');

                number = parseInt(number);
                number--;

                name = ('window' + number);
            }


            var window = $("#" + name).data("kendoWindow");
            if (window) {
                window.refresh();
            }
            else {
                kendoHelper.HideProgress();
                kendo.ui.progress($('body'), true);
                location.reload();
            }
        } else {
            location.reload();
        }
    },

    ParseJsonDate: function (jsonDateString) {
        return new Date(parseInt(jsonDateString.replace('/Date(', '')));
    },

    GetJsonDate: function (date) {
        return '/Date(' + date.getTime() + ')';
    }
}

kendoHelper.Grid = {
    OnError: function (e) {
        if (e.status === "timeout") {
            alertify.alert(res.Alert.Title_Error, res.Alert.Timeout);
        }
        else {
            alertify.alert(res.Alert.Title_Error, res.Alert.GeneralError);
        }
    },

    FilterGrid: function (gridname, fieldname, operator, url) {
        var filtervalue = getParameterByName(fieldname, url);
        if (filtervalue != '' && filtervalue != null && filtervalue != 'undefined') {
            grid = $(gridname).data("kendoGrid");
            grid.dataSource.filter({ field: fieldname, operator: operator, value: filtervalue });

        }
    },

    ClearFilter: function (gridname) {
        grid = $(gridname).data("kendoGrid");
        grid.dataSource.filter({});
    },

    Refresh: function (gridId) {
        var kendoGrid = null;
        if (gridId) {
            kendoGrid = $('#' + gridId + '').data('kendoGrid');
        }
        else {
            kendoGrid = $('#Grid').data('kendoGrid');
        }

        if (kendoGrid) {
            kendoGrid.dataSource.read();
        }
    }
}

//Generelle Kendo Validierung des Datums nach dem Format "dd.MM.yyyy", überschreibt die bestehende mvcdate-Methode
kendo.ui.validator.rules.mvcdate = function (input) {
    var datarole = $(input).data('role');
    return true;
    //console.log('datarole: ' + datarole + ' name: ' + input.attr("name"));
    if (datarole == 'datepicker') {
        if (input.is("[data-val-date]")) {
            return (input.val() === "" || kendo.parseDate(input.val(), "dd.MM.yyyy") !== null);
        }
        else {
            //console.log('No [data-val-date]');
            return true;
        }
    }
    else {
        //console.log('No datepicker');
        return true;
    }
}

function NumericFilter(control) {
    $(control).kendoNumericTextBox({ "format": "n0", "decimals": 0 });
}