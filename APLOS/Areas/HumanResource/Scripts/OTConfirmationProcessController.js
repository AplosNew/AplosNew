'use strict';
OTConfirmationProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', 'cboService', '$http', '$filter'];
function OTConfirmationProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, cboService, $http, $filter) {
    $rootScope.title = 'OT Confirmation Process';
    $scope.path = "humanresource/OTConfirmationProcess/";


    // The Tab Switching Code

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.dateShow = function () {
        if ($scope.month.length > 0 && $scope.year.length > 0 && angular.isUndefinedOrNull($scope.Week) == false) {
            $http({
                method: 'GET',
                url: $scope.path + 'GetWorkDateRange',
                params: { 'Week': $scope.Week, 'Month': $scope.month, 'Year': $scope.year },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.FromDate = response.data[0].FromDate;
                $scope.ToDate = response.data[0].ToDate;
            });
        }
    }

    //
    $scope.year = '';
    $scope.month = '';
    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    //$scope.year = new Date().getFullYear().toString();
    //$scope.month = new Date().getMonth().toString();
    //

    var but = document.getElementById('ChkBtn')
    but.style.display = "none";

    function ProcessChk() {
        if ($scope.DataList.length <= 0) {
            but.style.display = "none";
        }
        else {
            but.style.display = "block";
        }
    }

    $scope.parameters = [];
    $scope.filters = [];
    $scope.loadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;

            var gridObj = $("#WeekList").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#WeekList").children('.e-pager.e-js.e-pager').hide();
            $("#WeekList").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#WeekList").children('.e-gridcontent').hide();


            var gridObj = $("#WeekListO").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#WeekListO").children('.e-pager.e-js.e-pager').hide();
            $("#WeekListO").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#WeekListO").children('.e-gridcontent').hide();
        });
    }
    $scope.loadfilters();

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




    //$scope.blackoutDates = [new Date()];
    //$scope.Changes = function()
    //{
    //    console.log('changed');
    //}

    $scope.ToDate = null;
    $scope.FromDate = null;
    $scope.Week = null;

    $scope.ToDateO = null;
    $scope.FromDateO = null;
    $scope.WeekO = null;

    $scope.OTConfirmationValue = null;

    $scope.OTConfirmSelection = [
        { 'Id': '0', 'Value': 'To Confirm' },
        { 'Id': '1', 'Value': 'Confirmed' },
        { 'Id': '2', 'Value': 'All' },
    ];

    //Process Filter
    $scope.Process = null;
    $scope.ProcessValue = null;
    $scope.OTLimit = null;
    $scope.DSApp = null;

    $scope.SelectedOT = null;

    //Day Status

    $scope.DayStatus

    $scope.DayTypesList = [];
    function getDayTypes() {
        $http({
            method: 'GET',
            url: $scope.path + 'getDayTypes',
        }).then(function succ(resp) {
            $scope.DayTypesList = resp.data;
        });
    };

    getDayTypes();

    $scope.selectCurrDayStatus = function () {
        angular.element(document.querySelector('#CurrDayStatusModal')).modal('show');
    }

    $scope.doubleCurrDayStatus = function (e) {
        $scope.DayStatus = e.data.DayType;
        angular.element(document.querySelector('#CurrDayStatusModal')).modal('hide');
    }


    $scope.DataList = [];

    $scope.getData = function () {

        if (angular.isUndefinedOrNull($scope.Week)) {
            ShowResult("Please Select the Week!!", 'failure');
            throw ('Invaild Request');
        }



        var gridObj = $("#WeekList").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filters;
        }

        var parameters = [];
        parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });

        $http({
            method: 'POST',
            url: $scope.path + 'getGridData',
            data: {
                'Week': $scope.Week, 'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate
                /*,  'DayStatus': $scope.DayStatus*/, 'Parameters': parameters
            },
        }).then(function succ(resp) {
            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                $scope.DataList = [];
                $scope.DataList = resp.data;
                ProcessChk();
            }

        })
    }

    $scope.ProcessAll = function () {

        if (angular.isUndefinedOrNull($scope.SelectedOT)) {
            ShowResult('Please Select OT Type!', 'failure');
            throw ('Invalid Request');
        }

        var g = $("#GridProcess").data("ejGrid");
        var fl = g.getFilteredRecords();

        var ProcArr = [];

        if (fl.length > 0) {
            for (var i = 0; i <fl.length; i++) {
                ProcArr.push({
                    'EmpSystemID':fl[i].EmpSystemID, 'WorkDate':fl[i].WorkDate, 'PlanOT':fl[i].PlanOT, 'ProcessedOT':fl[i].ProcessedOT,
                    'DayLimit':fl[i].DayLimit, 'StandardOT':fl[i].StandardOT, 'AppliedOTLimit':fl[i].AppliedOTLimit,
                    'AllowedOTLimit':fl[i].AllowedOTLimit, 'AdditionalOT':fl[i].AdditionalOT, 'WeekLimit':fl[i].WeekLimit,
                    'TargetOT':fl[i].TargetOT, 'ApplicableWM':fl[i].ApplicableWM, 'IsOTComfirm':fl[i].IsOTComfirm, 'IsManualOutTime':fl[i].IsManualOutTime,
                    'MonthlyLimit':fl[i].MonthlyLimit, 'OutTime':fl[i].OutTime, 'PlantId':fl[i].PlantId, 'ProcessOutTime':fl[i].ProcessOutTime,
                    'RowId':fl[i].RowId
                });
            }
        }
        else {
            for (var i = 0; i < $scope.DataList.length; i++) {
                ProcArr.push({
                    'EmpSystemID': $scope.DataList[i].EmpSystemID, 'WorkDate': $scope.DataList[i].WorkDate, 'PlanOT': $scope.DataList[i].PlanOT, 'ProcessedOT': $scope.DataList[i].ProcessedOT,
                    'DayLimit': $scope.DataList[i].DayLimit, 'StandardOT': $scope.DataList[i].StandardOT, 'AppliedOTLimit': $scope.DataList[i].AppliedOTLimit,
                    'AllowedOTLimit': $scope.DataList[i].AllowedOTLimit, 'AdditionalOT': $scope.DataList[i].AdditionalOT, 'WeekLimit': $scope.DataList[i].WeekLimit,
                    'TargetOT': $scope.DataList[i].TargetOT, 'ApplicableWM': $scope.DataList[i].ApplicableWM, 'IsOTComfirm': $scope.DataList[i].IsOTComfirm, 'IsManualOutTime': $scope.DataList[i].IsManualOutTime,
                    'MonthlyLimit': $scope.DataList[i].MonthlyLimit, 'OutTime': $scope.DataList[i].OutTime, 'PlantId': $scope.DataList[i].PlantId, 'ProcessOutTime': $scope.DataList[i].ProcessOutTime,
                    'RowId': $scope.DataList[i].RowId
                });
            }
        }

        var Proc = JSON.stringify(ProcArr);

        $http({
            method: 'POST',
            url: $scope.path + 'ProcessData',
            data: { 'Data': Proc, 'OTWeek': $scope.Week, 'SelectedOT': $scope.SelectedOT },
        }).then(function succ(resp) {
            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }

        })
    }


    // Report Download Operations  
    $scope.ReportData = [];

    $scope.getReportData = function () {

        if (angular.isUndefinedOrNull($scope.WeekO)) {
            ShowResult("Please Select the Week!!", 'failure');
            throw ('Invaild Request');
        }

        if (angular.isUndefinedOrNull($scope.FromDateO) || angular.isUndefinedOrNull($scope.ToDateO)) {
            ShowResult("Please Select the From and To Date!!", 'failure');
            throw ('Invaild Request');
        }

        var gridObj = $("#WeekListO").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filters;
        }

        var parameters = [];
        parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });

        $http({
            method: 'POST',
            url: $scope.path + 'getReportData',
            data: {
                'Week': $scope.WeekO, 'FromDate': $scope.FromDateO, 'ToDate': $scope.ToDateO, 'OTConfirmationValue': $scope.OTConfirmationValue
                , 'Process': $scope.Process, 'ProcessValue': $scope.ProcessValue, 'DayStatus': $scope.DayStatus, 'DSApp': $scope.DSApp, 'Parameters': parameters
            },
        }).then(function succ(resp) {
            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                $scope.ReportData = [];
                $scope.ReportData = resp.data;
                ProcessChk();
            }

        })
    }


    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.Report = function () {

        if (angular.isUndefinedOrNull($scope.WeekO)) {
            ShowResult("Please Select the Week!!", 'failure');
            throw ('Invaild Request');
        }

        if (angular.isUndefinedOrNull($scope.FromDateO) || angular.isUndefinedOrNull($scope.ToDateO)) {
            ShowResult("Please Select the From and To Date!!", 'failure');
            throw ('Invaild Request');
        }

        var gridObj = $("#WeekListO").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (filteredRecords.length == 0) {
            filteredRecords = $scope.filters;
        }

        var parameters = [];
        parameters.push({ "Key": "PlantId", "Value": getString(filteredRecords, "PlantId") });
        parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });


        $http({
            method: 'POST',
            url: $scope.path + 'getOTReportDownload',
            data: {
                'Week': $scope.WeekO, 'FromDate': $scope.FromDateO, 'ToDate': $scope.ToDateO, 'OTConfirmationValue': $scope.OTConfirmationValue
                , 'Process': $scope.Process, 'ProcessValue': $scope.ProcessValue, 'DayStatus': $scope.DayStatus, 'DSApp': $scope.DSApp, 'Parameters': parameters
            },
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

}