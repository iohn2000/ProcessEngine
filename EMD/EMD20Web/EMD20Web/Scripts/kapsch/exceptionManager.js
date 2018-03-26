var exceptionManager = {


}

exceptionManager.Statics = {
    UserName: '',
    SessionStorageName: 'kapschExceptionInfos'
}

exceptionManager.Entities = {
    Error: function (errorCode, errorMessage, stackTrace) {
        this.TimeStamp = new Date();
        this.Url = window.location.pathname;
        this.User = exceptionManager.Statics.UserName; // $('.credentialbox').html().trim();
        this.ErrorCode = errorCode;
        this.ErrorMessage = errorMessage;
        this.StackTrace = stackTrace;
    },

    InitErrorWithModel: function (errorModel) {
        var temp = new exceptionManager.Entities.Error(errorModel.KapschErrorNumber, errorModel.ErrorMessage, errorModel.StackTrace);


        return temp;

    }
}

exceptionManager.Events = {
    HandleError: function (e) {
        if (e.errors) {

            if (e.errors.ErrorMessage) {
                var message = '<div style="font-weight:bold">An unhandled error happened in requesting your data source:</div><div>' + e.errors.ErrorMessage + '</div>';
                message += '<div style="font-weight: bold; margin-top: 10px">See details on the page <a href="/Error/ErrorList">"ErrorList"</a>!</div>';

                alertify.alert(res.Alert.Title_Error, message);

                exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(e.errors));

            }
        }
    },

    OnHandleDownloadClick: function (e) {
        var text = JSON.stringify(exceptionManager.Functions.GetErrors());

        text = helper.Functions.ConvertToCSV(text);

        var textFile = new Blob([text], {
            type: 'text/plain'
        });
        helper.Functions.InvokeSaveAsDialog(textFile, 'EDP20_Error_' + exceptionManager.Statics.UserName + '_' + new Date().toJSON().slice(0, 16) + '.log');
    }
}

exceptionManager.Functions = {
    GetErrors: function () {
        var jsonObject = sessionStorage.getItem(exceptionManager.Statics.SessionStorageName);
        if (jsonObject == null) {
            return null;
        }

        return JSON.parse(jsonObject);
    },

    AddError: function (error) {
        var errors = this.GetErrors();

        if (errors == null) {
            errors = [];
        }

        errors.push(error);

        sessionStorage.setItem(exceptionManager.Statics.SessionStorageName, JSON.stringify(errors));
    },

    OpenErrorListView: function (e) {
        window.location = '/Error/ErrorList';
    },

    ShowErrorInWindow: function (e) {

        var dataItem = this.dataItem($(e.currentTarget).closest("tr"));

        $("<div id='modalErroWindow'><div>" + dataItem.ErrorMessage + "</div><div style='margin-top: 20px;'>" + dataItem.StackTrace + "</div></div>").kendoWindow({
            width: "90%",
            height: "80%",
            title: 'Technical Error Information ' + kendo.toString(kendo.parseDate(new Date(dataItem.TimeStamp), 'yyyy-MM-dd'), 'dd.MM.yyyy HH:mm:ss'),
            //   content: dataItem.StackTrace,
            modal: true,
            deactivate: function () {

                this.destroy();
            },


            visible: true,
            actions: ["Close"]

        }).data("kendoWindow").open().center();
    }
}
