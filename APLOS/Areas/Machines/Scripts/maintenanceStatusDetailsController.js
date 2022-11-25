'use strict';
maintenanceStatusDetailsController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function maintenanceStatusDetailsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "MaintenanceStatusDetails";
    $scope.Action = 'Save';
    $scope.path = 'Machines/MaintenanceStatusDetails/';
    $scope.savePlannedUrl = $scope.path + 'createPlanned';
    $scope.saveResponsibleUrl = $scope.path + 'createResponsible';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() + 7);
    /*var firstDay = new Date(y, m, 1);*/
   
    $scope.status = {
        Id: null,
        FromDate: null,
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        FromDateMD: null,
        ToDateMD: $filter('dateFiltering')(date, 'dd-MM-yyyy')
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.GetFromDateList = function () {
        $http({
            method: 'GET',
            url: 'Machines/MaintenanceStatusDetails/GetFromDateList'
        }).then(function successCallback(response) {
            $scope.statusNew.FromDate = response.data[0];
            $scope.statusNew.FromDateMD = response.data[0];
        });
    }
    $scope.GetFromDateList();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    function Validation() {
        try {
            CheckField("To Date", $scope.statusNew.ToDate);
        } catch (ex) {
            throw ex;
        }
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }

    $scope.MaintenanceStatusDetailsList = [];
    $scope.View = function () {
        try {
            Validation();
            $http({

                method: 'Get',
                url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusDetailsList?ToDate=' + $scope.statusNew.ToDateMD + '&FromDate=' + $scope.statusNew.FromDateMD
            }).then(function successCallback(response) {
                $scope.MaintenanceStatusDetailsList = response.data;
                var gridObj = $("#GridMaintenanceStatusDetails").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }
            )
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.MaintenanceStatusSummaryList = [];
    $scope.ViewSummary = function () {
        try {
            Validation();
            $http({

                method: 'Get',
                url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusSummaryList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate
            }).then(function successCallback(response) {
                $scope.MaintenanceStatusSummaryList = response.data;
                var gridObj = $("#GridMaintenanceStatusSummary").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }
            )
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

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
    $scope.Test = null;
    $scope.MachineId = null;
    $scope.MaintenanceId = null;
    $scope.EntityId = null;
    $scope.MaintenanceStatusPlannedDetailsList = [];
    $scope.GetAssetPopUp = function (data, sample) {
        $scope.Test = sample;
        if ($scope.Test != 0) {
            $scope.Test = 1;
        }
        $scope.MachineId = data.data.MachineId;
        $scope.MaintenanceId = data.data.Id;
        $scope.EntityId = data.data.EntityId;
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusPlannedList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.MaintenanceId + '&MachineId=' + $scope.MachineId + '&EntityId=' + $scope.EntityId + '&Value=' + $scope.Test
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }
    $scope.SD = null;
    $scope.GetDetails = function (args) {
        $scope.MachineAssetId = args.data.AssetId;
        $scope.MaintenanceId = args.data.Id;
        $scope.MachineId = args.data.MachineMasterId;
        $scope.SD = args.data.SD;
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusPlannedListGetDetails?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.MaintenanceId + '&MachineId=' + $scope.MachineId + '&AssetId=' + $scope.MachineAssetId
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }

    $scope.GetAssetPopUpDetails = function () {
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusPlannedListDetails?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.MaintenanceId + '&MachineId=' + $scope.MachineId 
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }

    $scope.GetAssetPopUpGetDetails = function () {
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadMaintenanceStatusPlannedListGetDetails?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.MaintenanceId + '&MachineId=' + $scope.MachineId + '&AssetId=' + $scope.MachineAssetId
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }

    $scope.PlannedId = null;
    $scope.ReponsiblePersonList = [];
    $scope.GetReponsiblePersonPopUp = function (data) {
        $scope.NewObject = data.data;
        var PlannedId = data.data.Id;
        $scope.PlannedId = PlannedId;
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceStatusDetails/LoadReponsiblePersonList?Id=' + $scope.PlannedId + '&MaintenanceId=' + data.data.MaintenanceSchedulingId
        }).then(function successCallback(response) {
            $scope.ReponsiblePersonList = response.data;
            var gridObj = $("#GridResponsiblePopUp").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
        }
        )
    }

    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachineAssetPop')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
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
                    if ($scope.SD == 'Status Details')
                    {
                        $scope.GetAssetPopUpGetDetails();
                        $scope.SD = null;
                    }
                    else
                    {
                        $scope.GetAssetPopUpDetails();
                        $scope.SD = null;
                    }
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

    $scope.SaveResponsiblePerson = function () {
        try {

            $scope.SaveResponsibleList = [];
            for (var i = 0; i < $scope.ReponsiblePersonList.length; i++) {
                if ($scope.ReponsiblePersonList[i].IsActive == true) {
                    $scope.SaveResponsibleList.push($scope.ReponsiblePersonList[i]);
                }
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

    //$scope.MaintenanceStatusSummaryReport = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'XlsMaintenanceStatusSummary?todate=' + $scope.statusNew.ToDate + '&fromDate=' + $scope.statusNew.FromDate,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {

    //            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    });

    //};

    $scope.MaintenanceStatusSummaryReport = function () {
        var dataList = [];
        var g = $("#GridMaintenanceStatusSummary").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.MaintenanceStatusSummaryList;
        }

        $scope.fileName = "Maintenance Status Summary";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    //$scope.MaintenanceStatusDetailsReport = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'XlsMaintenanceStatusDetails?todate=' + $scope.statusNew.ToDateMD + '&fromDate=' + $scope.statusNew.FromDateMD,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {

    //            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    });

    //};

    $scope.MaintenanceStatusDetailsReport = function () {
        var dataList = [];
        var g = $("#GridMaintenanceStatusDetails").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.MaintenanceStatusDetailsList;
        }

        $scope.fileName = "Maintenance Status Details";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.rowDataBound = function rowDataBound(e) {

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

    $scope.rowDataBoundDetails = function rowDataBoundDetails(e) {

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
}

