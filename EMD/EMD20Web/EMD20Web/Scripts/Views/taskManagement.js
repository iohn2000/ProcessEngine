taskManagement = {};

taskManagement.Entities = {
    SelectedTaskGuids: [],
    ShowBulk: false
};

taskManagement.Data = {
    TaskParameters: function (e) {
        return {
            taskstatus: $("#TaskStatus").val()
        };

    }
}

taskManagement.Events = {
    HandleOnTaskFilterStatusChanged: function (e) {
        $("#Grid").data("kendoGrid").dataSource.read();
    },


    HandleOnTaskItemCheckBoxChanged: function (e) {
        var isChecked = e.checked;
        var selectedTaskItemGuid = $(e).attr('guid');
        var selectedTaskItemIsBulk = $(e).attr('isBulkTask');
        var selectedTaskItemType = $(e).attr('tasktype');


        // remove the item
        if (isChecked === false) {
            if (taskManagement.Entities.SelectedTaskGuids.hasOwnProperty(selectedTaskItemGuid) === true) {
                // taskManagement.Entities.SelectedTaskGuids.splice(index, 1);

                delete taskManagement.Entities.SelectedTaskGuids[selectedTaskItemGuid];
            }
        }
        else {
            if (taskManagement.Entities.SelectedTaskGuids.hasOwnProperty(selectedTaskItemGuid) === false) {
                // taskManagement.Entities.SelectedTaskGuids.push(selectedTaskItemGuid);

                taskManagement.Entities.SelectedTaskGuids[selectedTaskItemGuid] = { isBulkTask: selectedTaskItemIsBulk, type: selectedTaskItemType };
            }
        }

        taskManagement.Functions.UpdateUIForSelectedTasks();
    },

    OnGridDataBound: function (e) {
        for (var key in taskManagement.Entities.SelectedTaskGuids) {
            if (taskManagement.Entities.SelectedTaskGuids.hasOwnProperty(key))
                $('tr > td > input[type="checkbox"][guid="' + key + '"]').attr('checked', true);

        }

        taskManagement.Functions.SetColorsForSimilarItems(taskManagement.Entities.ShowBulk);
        //for (var i = 0; i < taskManagement.Entities.SelectedTaskGuids.length; i++) {
        //    $('tr > td > input[type="checkbox"][guid="' + taskManagement.Entities.SelectedTaskGuids[i] + '"]').attr('checked', true);
        //}
    },

    OnButtonForwardTasksClick: function (sender) {

        var dropdownlist = $("#ForwardToPerson").data("kendoDropDownList");
        var selectedEmplGuid = dropdownlist.value();

        taskManagement.Communication.ForwardTasks(Object.keys(taskManagement.Entities.SelectedTaskGuids), selectedEmplGuid, sender);
    },

    SelectSimilarBatchItems: function (sender) {

        if (Object.keys(taskManagement.Entities.SelectedTaskGuids).length > 0) {

            var firstBatchTask = taskManagement.Entities.SelectedTaskGuids[Object.keys(taskManagement.Entities.SelectedTaskGuids)[0]];

            $('input[type="checkbox"][tasktype="' + firstBatchTask.type + '"]').prop('checked', true);


            $('input[type="checkbox"][tasktype="' + firstBatchTask.type + '"]').each(function () {
                var selectedTaskItemGuid = $(this).attr('guid');
                if (taskManagement.Entities.SelectedTaskGuids.hasOwnProperty(selectedTaskItemGuid) === false) {
                    taskManagement.Entities.SelectedTaskGuids[selectedTaskItemGuid] = { isBulkTask: "true", type: firstBatchTask.type };
                }
            });
        }

    },

    OnButtonApproveTaskBatchClick: function (e) {
        if (!$(e).attr('disabled')) {
            var keys = '';

            var count = 0;
            var firstKey = null;
            for (var key in taskManagement.Entities.SelectedTaskGuids) {
                if (taskManagement.Entities.SelectedTaskGuids.hasOwnProperty(key)) {
                    keys += key + ',';
                }
                if (count === 0) {
                    firstKey = key;
                }
                count++;
            }
            keys = keys.substring(0, keys.length - 1);

            var hasMultipleEntriesString = 'false';
            if (count > 1) {
                hasMultipleEntriesString = 'true';
            }

            showInWindow('/TaskManagement/Edit/' + firstKey + '/' + hasMultipleEntriesString + '/true', 'Work with selected tasks in batch-mode');

            //showInWindow('/TaskManagement/Edit/' + keys + '/true', 'Work with selected tasks in batch-mode');
        }


    },

    OnButtonShowForwardTaskWindowClick: function (sender) {
        if (Object.keys(taskManagement.Entities.SelectedTaskGuids).length > 0) {

            var wintitle = 'Forward tasks ...';
            $("<div id='modal-window-activities'></div>").kendoWindow({
                title: wintitle,
                content: 'Forward',
                modal: true,
                deactivate: function () {
                    this.destroy();
                },
                visible: true,
                //refresh: function () { winElement.find(".k-loading-mask").remove(); }
            }).data("kendoWindow").open().center().title(wintitle);
        }
    },

    OnApproveTasksSuccess: function (xhr) {
        taskManagement.Entities.SelectedTaskGuids = [];

        formValidation.OnSuccess(xhr);
    },

    OnButtonSaveClick: function (e) {
        taskManagement.Communication.SaveTasks(Object.keys(taskManagement.Entities.SelectedTaskGuids));
    }


};

taskManagement.Functions = {

    StatusAutocompleteFilter: function (e) {
        e.element.kendoAutoComplete({
            dataSource: {
                data: ["open", "closed"]
            }
        });
    },

    SetColorsForSimilarItems: function (isBulkable) {
        if (!isBulkable || Object.keys(taskManagement.Entities.SelectedTaskGuids).length === 0) {
            // remove all colors

            $('i[linktype="workWithTask"]').removeClass('batchColored');
        } else {
            var firstBatchTask = taskManagement.Entities.SelectedTaskGuids[Object.keys(taskManagement.Entities.SelectedTaskGuids)[0]];

            $('i[linktype="workWithTask"][tasktype="' + firstBatchTask.type + '"]').addClass('batchColored');
        }
    },


    UpdateUIForSelectedTasks: function (e) {
        if (Object.keys(taskManagement.Entities.SelectedTaskGuids).length === 0) {
            $('#ButtonDoBulk').attr('disabled', 'disabled');
            $('#ButtonDoForward').attr('disabled', 'disabled');
            taskManagement.Functions.SetColorsForSimilarItems(false);
        }
        else {
            var showBulk = true;
            var taskType = null;
            for (var key in taskManagement.Entities.SelectedTaskGuids) {
                if (taskManagement.Entities.SelectedTaskGuids.hasOwnProperty(key)) {
                    var currentItem = taskManagement.Entities.SelectedTaskGuids[key];
                    if (taskType === null) {
                        if (currentItem.isBulkTask.toString() === "true") {
                            taskType = currentItem.type;
                        }
                        else {
                            showBulk = false;
                        }
                    }
                    else {
                        if (taskType !== currentItem.type || currentItem.isBulkTask.toString() === "false") {
                            showBulk = false;
                            break;
                        }
                    }

                }
            }

            taskManagement.Entities.ShowBulk = showBulk;
            if (showBulk) {
                $('#ButtonDoBulk').removeAttr('disabled');
                $('#ButtonSelectBatchTasks').removeAttr('disabled');

                taskManagement.Functions.SetColorsForSimilarItems(true);
            }
            else {
                $('#ButtonDoBulk').attr('disabled', 'disabled');
                $('#ButtonSelectBatchTasks').attr('disabled', 'disabled');

                taskManagement.Functions.SetColorsForSimilarItems(false);
            }

            $('#ButtonDoForward').removeAttr('disabled');
        }
    }
};

taskManagement.Communication = {
    ForwardTasks: function (taskList, empl_guid, sender) {

        var data = JSON.stringify({ taskListItemGuids: taskList, forwardToEmplGuid: empl_guid });

        $.ajax({
            url: '/TaskManagement/ForwardTasks',
            type: "POST",
            dataType: "json",
            contentType: "application/json",

            data: data,
            success: function (response) {

                if (response.success) {
                    alertify.success(response.message);
                    taskManagement.Entities.SelectedTaskGuids = [];
                    taskManagement.Functions.UpdateUIForSelectedTasks();
                    kendoHelper.Grid.Refresh();
                    kendoHelper.CloseWindow(sender);
                }
                else {
                    alertify.error(response.message);
                }
            },
            error: function (response) {
                if (response.message) {
                    alertify.error(response.message);
                }
                else {
                    alertify.error('A general error occured');
                }
            }

        })
    },

    SaveTasks: function (taskList) {
        var data = JSON.stringify({ taskListItemGuids: taskList });

        $.ajax({
            url: '/TaskManagement/SaveTasks',
            type: "POST",
            dataType: "json",
            contentType: "application/json",

            data: data,
            success: function (response) {

                if (response.success) {
                    alertify.success(response.message);
                    taskManagement.Entities.SelectedTaskGuids = [];
                    taskManagement.Functions.UpdateUIForSelectedTasks();
                    kendoHelper.Grid.Refresh();
                    kendoHelper.CloseWindow(sender);
                }
                else {
                    alertify.error(response.message);
                }
            },
            error: function (response) {
                if (response.message) {
                    alertify.error(response.message);
                }
                else {
                    alertify.error('A general error occured');
                }
            }

        })
    }
}