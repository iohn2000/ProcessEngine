formValidation = {
    OnBegin: function (xhr) {
        kendoHelper.ShowProgress();
    },
    OnSuccess: function (xhr, gridId) {
        kendoHelper.HideProgress();

        kendoHelper.CloseWindow();


        if (xhr.responseJSON) {
            if (xhr.responseJSON.message) {
                alertify.success(xhr.responseJSON.message);
            }
            else {
                alertify.success(res.Alert.Success_Default);
            }
        }
        else {
            alertify.error(res.Alert.FormValidation_Unknown_Error);

            var error = new exceptionManager.Entities.Error(-1, "The server couldn't be reached for last called method", '');
            exceptionManager.Functions.AddError(error);
        }

        // redirect to another URL
        //if (xhr.responseJSON.Url) {
        //    window.location.href = xhr.responseJSON.Url;
        //}
        kendoHelper.Grid.Refresh(gridId);


        $('.validation-summary-errors').html('');
        $('.input-validation-error').removeClass('input-validation-error');
        $('.field-validation-error').remove();
        $('#formresults').hide();
    },
    OnFailure: function (xhr) {
        console.log(xhr);

        kendoHelper.HideProgress();
        // $('#formcontent').html(xhr.responseText);
        var closeWindow = xhr.getResponseHeader('closeWindow');

        var kapschError = new exceptionManager.Entities.Error(500, xhr.statusText, xhr.getResponseHeader('stackTrace'));
        exceptionManager.Functions.AddError(exceptionManager.Entities.InitErrorWithModel(kapschError));


        if (closeWindow && closeWindow === 'true') {
            alertify.error(xhr.statusText);
            kendoHelper.CloseWindow();
        }
        else {
            alertify.error(res.Alert.FormValidation);
            if (xhr.statusText) {
                $('#formresults').html(xhr.statusText);
                $('#formresults').show();
            }

            $('.field-validation-error').prepend('<span class="k-icon k-warning"> </span>');
            $('.field-validation-error').addClass('k-widget');
            $('.field-validation-error').addClass('k-tooltip');
            $('.field-validation-error').addClass('k-tooltip-validation');
            $('.field-validation-error').addClass('k-invalid-msg');
        }
    }
};
