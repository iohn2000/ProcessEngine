advancedsearch = {};

advancedsearch.Entities = {
    IsLoading: false,
    SelectedEntity: null,
    HistoricalAllowed: null
}

advancedsearch.Functions = {
    CheckForEntityGuid: function (searchstring) {
        if (searchstring.length == 37 && searchstring.indexOf("_") == 4) {
            if (!changeEmployment.Entities.SelectedEntity || changeEmployment.Entities.SelectedEntity !== "PREN") {
                advancedsearch.Communication.SetEntityPrefixFromGuid(searchstring);
            }

        }
        else {
            $("#SearchStringIsGuid").val("False");
        }
    }
    ,
    SetEntityDropdownVisibility: function (visible) {
        if (visible == true) {
            $('#DivStartDate').show();
            $('#DivEndDate').show();
        }
        else {
            $('#DivStartDate').hide();
            $('#DivEndDate').hide();
        }

    },

    OnDataBound: function (e) {

    },

    OnDataBinding: function (e) {
        if (advancedsearch.Entities.IsLoading) {
            kendoHelper.HideProgress('advancedSearch');
        }
    },

    getSearchInputData: function () {
        return {
            SearchString: $('#SearchString').val(),
            SelectedEntity: $('#SelectedEntity').val(),
            //StartDate: new Date($("#StartDate").data("kendoDatePicker").value()).toISOString().substring(0, 10),
            StartDate: new Date($("#StartDate").data("kendoDatePicker").value()).toISOString(),
            //EndDate: new Date($("#EndDate").data("kendoDatePicker").value()).toISOString().substring(0, 10),
            EndDate: new Date($("#EndDate").data("kendoDatePicker").value()).toISOString(),
            SearchStringIsGuid: $('#SearchStringIsGuid').val()
        }
    },

    SubmitForm: function (model) {
        if (validator.validate()) {
            kendoHelper.ShowProgress('advancedSearch');
            advancedsearch.Entities.IsLoading = true;
            var mysearchstring = $('#SearchString').val();
            var myentity = $('#SelectedEntity').val();
            //console.log('Startdate: ' + $("#StartDate").data("kendoDatePicker").value());
            //console.log('Enddate: ' + $("#EndDate").data("kendoDatePicker").value());

            //console.log('Startdate: ' + kendo.stringify($("#StartDate").data("kendoDatePicker").value()));
            //console.log('Enddate: ' + kendo.stringify($("#EndDate").data("kendoDatePicker").value()));

            grid = $("#Grid").data("kendoGrid");
            gridenterprise = $("#GridEnterprise").data("kendoGrid");
            gridlocation = $("#GridLocation").data("kendoGrid");
            gridaccount = $("#GridAccount").data("kendoGrid");
            gridUser = $("#GridUser").data("kendoGrid");
            gridProcessEntity = $("#GridProcessEntity").data("kendoGrid");

            if (myentity == "EMPL") {
                grid.dataSource.options.transport.read.data.SearchString = mysearchstring;
                grid.dataSource.query({ page: 1, pageSize: 20 });
                grid.dataSource.read();


                $("#Grid").show();
                $("#GridEnterprise").hide();
                $("#GridLocation").hide();
                $("#GridAccount").hide();
                $("#GridUser").hide();
                $("#GridProcessEntity").hide();
            }
            else if (myentity == "ENTE") {
                gridenterprise.dataSource.options.transport.read.data.SearchString = mysearchstring;
                gridenterprise.dataSource.query({ page: 1, pageSize: 20 });
                gridenterprise.dataSource.read();

                $("#GridEnterprise").show();
                $("#Grid").hide();
                $("#GridLocation").hide();
                $("#GridAccount").hide();
                $("#GridUser").hide();
                $("#GridProcessEntity").hide();
            }
            else if (myentity == "LOCA") {
                gridlocation.dataSource.options.transport.read.data.SearchString = mysearchstring;
                gridlocation.dataSource.query({ page: 1, pageSize: 20 });
                gridlocation.dataSource.read();

                $("#GridEnterprise").hide();
                $("#Grid").hide();
                $("#GridUser").hide();
                $("#GridProcessEntity").hide();
                $("#GridLocation").show();
                $("#GridAccount").hide();
            }
            else if (myentity == "ACCO") {
                gridaccount.dataSource.options.transport.read.data.SearchString = mysearchstring;
                gridaccount.dataSource.query({ page: 1, pageSize: 20 });
                gridaccount.dataSource.read();

                $("#GridEnterprise").hide();
                $("#Grid").hide();
                $("#GridLocation").hide();
                $("#GridUser").hide();
                $("#GridProcessEntity").hide();
                $("#GridAccount").show();
            }
            else if (myentity == "USER") {
                gridUser.dataSource.options.transport.read.data.SearchString = mysearchstring;
                gridUser.dataSource.query({ page: 1, pageSize: 20 });
                gridUser.dataSource.read();

                $("#GridEnterprise").hide();
                $("#Grid").hide();
                $("#GridLocation").hide();
                $("#GridAccount").hide();
                $("#GridProcessEntity").hide();
                $("#GridUser").show();
            }
            else if (myentity == "PREN") {
                gridProcessEntity.dataSource.options.transport.read.data.SearchString = mysearchstring;
                gridProcessEntity.dataSource.query({ page: 1, pageSize: 20 });
                gridProcessEntity.dataSource.read();

                $("#GridEnterprise").hide();
                $("#Grid").hide();
                $("#GridLocation").hide();
                $("#GridAccount").hide();
                $("#GridProcessEntity").show();
                $("#GridUser").hide();
            }
        }
        else {
            //console.log("Not able to validate");
        }
    },

    ChangeEntitySetItemData: function (item) {
        //console.log("ChangeEntitySetItemData item.Value: " + item.Value);

        if (item.Data) {
            changeEmployment.Entities.SelectedEntity = item.Value;
            advancedsearch.Entities.HistoricalAllowed = item.Data.historical;
            if (item.Data.showStartDateInfo) {
                $('#area-startDate-Text').html(item.Data.startDateInfoText);
                $('#area-startDate').show();

            }
            else {
                $('#area-startDate').hide();
            }
            if (item.Data.showEndDateInfo) {
                $('#area-endDate-Text').html(item.Data.endDateInfoText);
                $('#area-endDate').show();
            }
            else {
                $('#area-endDate').hide();
            }

            advancedsearch.Functions.SetEntityDropdownVisibility(advancedsearch.Entities.HistoricalAllowed);
        }
    }
}

advancedsearch.Events = {
    OnChangeEntity: function (e) {
        //console.log("OnChangeEntity this.dataItem(e.item).Value: " + this.dataItem(e.item).Value);
        advancedsearch.Functions.ChangeEntitySetItemData(this.dataItem(e.item));
        //changeEmployment.Entities.SelectedEntity = this.dataItem(e.item).Value;
        //advancedsearch.Entities.HistoricalAllowed = this.dataItem(e.item).Data.historical;
        //advancedsearch.Functions.SetEntityDropdownVisibility(advancedsearch.Entities.HistoricalAllowed);
    }
}

advancedsearch.Communication = {
    //Because of asynchronous request we need to supply the function calle on success
    SetEntityPrefixFromGuid: function (guid) {
        var data = { EntityGuid: guid };

        $.ajax({

            type: "POST",
            url: "/AdvancedSearch/GetEntityPrefixFromGuid",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json',
            success: function (response) {

                if (response.success) {
                    //console.log("response.entityprefix: " + response.entityprefix);
                    $("#SelectedEntity").data("kendoDropDownList").value(response.entityprefix);
                    //console.log("SelectedEntity - Value set to " + $("#SelectedEntity").data("kendoDropDownList").value());

                    advancedsearch.Functions.ChangeEntitySetItemData($("#SelectedEntity").data("kendoDropDownList").dataItem());
                    $("#SearchStringIsGuid").val("True");
                }
                else {
                    $("#SearchStringIsGuid").val("False");
                }
            }
        });
    }
}

