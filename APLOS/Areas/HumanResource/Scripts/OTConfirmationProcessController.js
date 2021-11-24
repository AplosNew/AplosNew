'use strict';
OTConfirmationProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OTConfirmationProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
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



    var but = document.getElementById('ChkBtn')
    but.style.display = "none";

    function ProcessChk() {
        if ($scope.Data.length <= 0) {
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
        {'Id':'0', 'Value': 'To Confirm'},
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


    $scope.Data = [];

    $scope.getData = function () {

        if (angular.isUndefinedOrNull($scope.Week)) {
            ShowResult("Please Select the Week!!", 'failure');
            throw ('Invaild Request');
        }

        if (angular.isUndefinedOrNull($scope.FromDate) || angular.isUndefinedOrNull($scope.ToDate)) {
            ShowResult("Please Select the From and To Date!!", 'failure');
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
                /*,  'DayStatus': $scope.DayStatus*/, 'Parameters': parameters },
        }).then(function succ(resp) {
            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                $scope.Data = [];
                $scope.Data = resp.data;
                ProcessChk();
            }
            
        })
    }

    $scope.ProcessAll = function () {

        if (angular.isUndefinedOrNull($scope.SelectedOT)) {
            ShowResult('Please Select OT Type!', 'failure');
            throw ('Invalid Request');
        }

        var ProcArr = [];
        for (var i = 0; i < $scope.Data.length; i++) {
            ProcArr.push({
                'EmpSystemID': $scope.Data[i].EmpSystemID, 'WorkDate': $scope.Data[i].WorkDate, 'PlanOT': $scope.Data[i].PlanOT,
                'DayLimit': $scope.Data[i].DayLimit, 'StandardOT': $scope.Data[i].StandardOT, 'AppliedOTLimit': $scope.Data[i].AppliedOTLimit,
                'AllowedOTLimit': $scope.Data[i].AllowedOTLimit, 'AdditionalOT': $scope.Data[i].AdditionalOT, 'WeekLimit': $scope.Data[i].WeekLimit,
                'TargetOT': $scope.Data[i].TargetOT, 'ApplicableWM': $scope.Data[i].ApplicableWM, 'IsOTComfirm': $scope.Data[i].IsOTComfirm,
                'MonthlyLimit': $scope.Data[i].MonthlyLimit, 'OutTime': $scope.Data[i].OutTime, 'ManualOutTime': $scope.Data[i].ManualOutTime, 'RowId': $scope.Data[i].RowId
            });
        }

        var Proc = JSON.stringify(ProcArr);

        $http({
            method: 'POST',
            url: $scope.path + 'ProcessData',
            data: { 'Data': Proc, 'OTWeek': $scope.Week, 'SelectedOT' : $scope.SelectedOT},
        }).then(function succ(resp) {
            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                console.log(resp);
            }

        })
    }


    // Report Download Operations


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
                $scope.Data = [];
                $scope.Data = resp.data;
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