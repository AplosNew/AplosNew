'use strict';
pendingMaintenanceScheduleController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function pendingMaintenanceScheduleController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "PendingMaintenanceSchedule";
    $scope.Action = 'Save';
    $scope.path = 'Machines/PendingMaintenanceSchedule/';
    $scope.savePlannedUrl = $scope.path + 'createPlanned';
    $scope.saveResponsibleUrl = $scope.path + 'createResponsible';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.status = {
        Id: null,
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        Responsible: null,
        WorkCenter: null,
        Status:'Pending',
        Asset: null
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.statusNew.ToDate)) {
                throw "To Date is required.";
            }
          
            $http({
                method: 'GET',
                url: 'Machines/PendingMaintenanceSchedule/LoadMaintenanceStatusDetailsList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&Status=' + $scope.statusNew.Status,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'AssetName', width: 20, headerText: "Asset/Machine", type: "string" },
                    { field: 'WorkCenter', width: 20, headerText: "Work Center", type: "string" },
                    { field: 'ResponsiblePersonBudgetCode', width: 20, headerText: "Responsible Person Budget Code", type: "string" },
                    { field: 'Entity', width: 20, headerText: "Entity", type: "string" }
                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "AssetId", "Value": getString(fl, "AssetId") });
        parameters.push({ "Key": "WorkCenterMasterId", "Value": getString(fl, "WorkCenterMasterId") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "ResponsiblePersoneBgtCodeId", "Value": getString(fl, "ResponsiblePersoneBgtCodeId") });
       

        $scope.parameters = parameters;
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }

    $scope.PendingMaintenanceScheduleList = [];
    $scope.View = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.statusNew.ToDate)) {
                throw "To Date is required.";
            }

           $scope.PendingMaintenanceScheduleList = [];
            $scope.filterComplete();

            $http({
                method: 'POST',
                url: $scope.path + "LoadPendingMaintenanceSchedule",
                data: { 'parameters': $scope.parameters, 'todate': $scope.statusNew.ToDate, 'fromDate': $scope.statusNew.FromDate, 'Status' : $scope.statusNew.Status},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.PendingMaintenanceScheduleList = response.data;
                   var gridObj = $("#GridPendingMaintenanceSchedule").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.rowDataBound = function rowDataBound(e) {

        //if (new Date(e.data.PlannedDate) < new Date($scope.statusNew.ToDate))
        //{
        //    e.row.css("background-color", '#FFA500');
        //}
        //else if (new Date(e.data.PlannedDate) > new Date($scope.statusNew.ToDate))
        //{
           
        //    e.row.css("background-color", '#FFFFFF');
        //}

        //else
        //{
        //    e.row.css("background-color", '#d1e5ff');

        //}

        if (e.data.OverDue > 0) {
            e.row.css("background-color", '#FFA500');
        }
        else if (e.data.OverDue === 0 && e.data.DueToday > 0) {

            e.row.css("background-color", '#d1e5ff');
        }

        else {
            e.row.css("background-color", '#FFFFFF');

        }


    }

    $scope.refreshTemplateResponsiblePerson = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllResponsiblePerson });
    };
    function CheckBoxSelectAllResponsiblePerson(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridResponsiblePopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ReponsiblePersonList.length; i++) {
                $scope.ReponsiblePersonList[i].IsActive = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].IsActive = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridResponsiblePopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.PlannedId = null;
    $scope.ReponsiblePersonList = [];
    $scope.GetReponsiblePersonPopUp = function (data) {
        $scope.NewObject = data.data;
        var PlannedId = data.data.PlannedId;
        $scope.PlannedId = PlannedId;
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadReponsiblePersonList?Id=' + $scope.PlannedId
        }).then(function successCallback(response) {
            $scope.ReponsiblePersonList = response.data;
            var gridObj = $("#GridResponsiblePopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
        }
        )
    }


    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.SaveResponsiblePerson = function () {
        try {

            $scope.SaveResponsibleList = [];
            for (var i = 0; i < $scope.ReponsiblePersonList.length; i++) {
                $scope.SaveResponsibleList.push($scope.ReponsiblePersonList[i]);
            }


            $http({
                method: 'POST',
                url: $scope.saveResponsibleUrl,
                data: {
                    "DataList": $scope.SaveResponsibleList,
                    "PId": $scope.PlannedId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.refreshTemplateMachineAsset = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMachineAsset });
    };
    function CheckBoxSelectAllMachineAsset(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPlannedMachineAsset").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MaintenanceStatusPlannedDetailsList.length; i++) {
                $scope.MaintenanceStatusPlannedDetailsList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

  
    $scope.MaintenanceStatusPlannedDetailsList = [];
    $scope.GetAssetPopUp = function (data) {
     
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenancePendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + data.data.PlannedId
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }
    $scope.GetAssetDetails = function (data) {

        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenancePendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + data.data.AssetId
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachineAssetPop')).modal('hide');
    }
    $scope.SavePlannedDetails = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.MaintenanceStatusPlannedDetailsList.length; i++) {
                if ($scope.MaintenanceStatusPlannedDetailsList[i].Flag == true) {
                    $scope.SaveList.push($scope.MaintenanceStatusPlannedDetailsList[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.savePlannedUrl,
                data: {
                    "DataList": $scope.SaveList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.GetAssetDetails();
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //#region MOI File 
    $scope.ItemId = null;
    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.model.Data))
                throw 'Please select/save the order first'
            $scope.ItemId = args.model.Data;
            args.data = args.model.Data;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "Machines/PendingMaintenanceSchedule/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ItemId))
            ShowResult('Please select/save the order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.FileDownload = function (data,test) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        if (test == 'id') {
            $scope.dwonloadUrl = virtualPath.MSAPath + '/' + data.Id + extention;
            test = null;
        }
        else {
            $scope.dwonloadUrl = virtualPath.MSAPath + '/' + data.PlannedId + extention;
            test = null;
        }
    };

    //$scope.FileDownloadPending = function (data) {
    //    $scope.dwonloadUrl = null;
    //    var str = data.FileName;
    //    var extention = str.substr(str.indexOf('.'));
    //    $scope.dwonloadUrl = virtualPath.MSAPath + '/' + data.PlannedId + extention;
    //};


    //#endregion
}

