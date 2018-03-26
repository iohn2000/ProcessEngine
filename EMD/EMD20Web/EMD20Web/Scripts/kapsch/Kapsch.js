function clearText(el) {
    if ($(el).val() == "Optional")
    { $(el).val(""); }
}

//for binding the data to the dropdown-boxes
//create a custom binder that always sets the value from the widget
kendo.data.binders.widget.nullableValue = kendo.data.Binder.extend({
    init: function (widget, bindings, options) {
        kendo.data.Binder.fn.init.call(this, widget.element[0], bindings, options);
        this.widget = widget;
        this._change = $.proxy(this.change, this);
        this.widget.bind("change", this._change);
    },
    refresh: function () {
        var value = this.bindings.nullableValue.get();
        this.widget.value(value);
    },
    change: function () {
        var value = this.widget.value();
        if (value === "") {
            value = null;
        }

        this.bindings.nullableValue.set(value);
    },
    destroy: function () {
        this.widget.unbind("change", this._change);
    }
});

function refreshGrid(gridname) {
    if (gridname == null) { gridname = 'Grid' }
    $('#' + gridname).data('kendoGrid').dataSource.read();
    $('#' + gridname).data('kendoGrid').refresh();
}


function insertAllTextToGridPager(gridname) {
    if (gridname == null) { gridname = 'Grid' }
    var grid = $("#" + gridname).data("kendoGrid");
    var dropdown = grid.pager.element
                        .find(".k-pager-sizes [data-role=dropdownlist]")
                        .data("kendoDropDownList");

    var item = {};
    item[dropdown.options.dataTextField] = "All";
    item[dropdown.options.dataValueField] = 100000;
    dropdown.dataSource.add(item);
    dropdown.value(100000);

    grid.dataSource.bind("change", function () {
        if (dropdown.value() == 100000) {
            dropdown.text("All");
        }
    });

}

function openModal(e, windowname) {
    var ModalErrorWindow = $(windowname).data("kendoWindow");
    if (ModalErrorWindow) {
        ModalErrorWindow.center().open();
    }

}

function openModal(windowname) {
    var ModalErrorWindow = $(windowname).data("kendoWindow");
    if (ModalErrorWindow) {
        ModalErrorWindow.center().open();
    }

}

//function grid_error_handler(e) {
//    if (e.errors) {
//        var message = "Errors:\n";
//        //if (e.status == "customerror") {
//        //    message += "customerror ";
//        //}
//        

//        openModal(e, "#modalErrorWindow");
//        var msg = $("#modalErrMsg");
//        msg.replaceWith(message);
//        //alert(message);
//    }
//}


function error_handler(e) {

    exceptionManager.Events.HandleError(e);

}

function sync_handler(e) {
    this.read();
}
