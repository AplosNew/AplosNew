'use strict';
CurrentFundPositionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function CurrentFundPositionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Current Fund Position Report';
    $scope.ModelList = [];
    $scope.path = 'Banks/BankJournal/';
    $scope.downloadgriddataUrlPath = 'Banks/CheckManagement/DownloadUsingFullPath';
    baseService.init($scope.getListUrl);

    $scope.ModelList = [];
    $scope.getData = function () {
        $scope.ModelList = [];
        $http.get('Banks/BankJournal/getlist?PostingDate=' + $filter("dateFiltering")(Date.now()))
            .then(function (response) {
                $scope.ModelList = response.data;
            });
    };
    $scope.getData();

    $scope.Report = function () {
        try {
            $scope.fileName = "Current Fund Position.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetCurrentFundPositionReport",
                data: { 'PostingDate': $filter("dateFiltering")(Date.now())},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }






    ////The Filters 
    //$scope.filters = [];
    //$scope.MeetingAgendaloadfilters = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'getFilters',
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.filters = response.data;
    //        var columnList = [
    //            { field: 'Department', width: 20, headerText: "Department", type: "string" },
    //            { field: 'CreatedBy', width: 20, headerText: "Created By", type: "string" },
    //            { field: 'MeetingType', width: 20, headerText: "Meeting Type", type: "string" },
    //            { field: 'ItemTitle', width: 20, headerText: "Item Title", type: "string" },
    //            { field: 'Criticality', width: 20, headerText: "Criticality", type: "string" },
    //            { field: 'ActionApplicable', width: 20, headerText: "Action Applicable", type: "string" },
    //            { field: 'DecisionApplicable', width: 20, headerText: "Decision Applicable", type: "string" },
    //            { field: 'Status', width: 20, headerText: "Status", type: "string" },
    //            { field: 'ByWhom', width: 20, headerText: "By Whom", type: "string" },
    //            { field: 'TargetFromDate', width: 20, headerText: "Target From Date", type: "string" },
    //            { field: 'TargetToDate', width: 20, headerText: "Target To Date", type: "string" },
    //            { field: 'MeetingName', width: 20, headerText: "Meeting Name", type: "string" },
    //            { field: 'MeetingDate', width: 20, headerText: "Meeting Date", type: "string" },
    //            { field: 'ChairedBy', width: 20, headerText: "Chaired By", type: "string" },
    //            { field: 'OrganizedBy', width: 20, headerText: "Organized By", type: "string" },

    //        ];
    //        $("#filters").ejGrid({
    //            dataSource: $scope.filters,
    //            minWidth: 450, minHeight: 400,
    //            allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
    //            filterSettings: { filterType: "excel" },
    //            columns: columnList
    //        });

    //        var gridObj = $("#filters").data("ejGrid");
    //        gridObj.refreshContent(true);
    //        gridObj.refreshTemplate();
    //        $("#filters").children('.e-pager.e-js.e-pager').hide();
    //        $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
    //        $("#filters").children('.e-gridcontent').hide();
    //    });
    //}
    //$scope.MeetingAgendaloadfilters();

    //$scope.parameters = [];
    //$scope.filterComplete = function () {

    //    var g = $("#filters").data("ejGrid");
    //    var fl = g.getFilteredRecords();
    //    if (fl.length == 0) {
    //        fl = $scope.filters;
    //    }


    //    var parameters = [];
    //    parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
    //    parameters.push({ "Key": "ByWhomId", "Value": getString(fl, "ByWhomId") });
    //    parameters.push({ "Key": "MeetingTypeId", "Value": getString(fl, "MeetingTypeId") });
    //    parameters.push({ "Key": "Status", "Value": getString(fl, "Status") });
    //    parameters.push({ "Key": "MeetingId", "Value": getString(fl, "MeetingId") });
    //    //parameters.push({ "Key": "ItemType", "Value": getString(fl, "ItemType") });
    //    //parameters.push({ "Key": "Importance", "Value": getString(fl, "Importance") });
    //    //parameters.push({ "Key": "ActionApplicable", "Value": getString(fl, "ActionApplicable") });
    //    //parameters.push({ "Key": "DecisionApplicable", "Value": getString(fl, "DecisionApplicable") });
    //    //parameters.push({ "Key": "ResponsiblePerson", "Value": getString(fl, "ResponsiblePerson") });
    //    //parameters.push({ "Key": "TargetDate", "Value": getString(fl, "TargetDate") });
    //    //parameters.push({ "Key": "MeetingDate", "Value": getString(fl, "TargetDate") });
    //    //parameters.push({ "Key": "ExpectedPersonId", "Value": getString(fl, "AttendeeId") });
    //    //parameters.push({ "Key": "TalkingPointId", "Value": getString(fl, "TalkingPointId") });
    //    //parameters.push({ "Key": "SuggestionId", "Value": getString(fl, "SuggestionId") });
    //    //parameters.push({ "Key": "ActionToBeTakenId", "Value": getString(fl, "ActionToBeTakenId") });
    //    //parameters.push({ "Key": "DecisionId", "Value": getString(fl, "DecisionId") });

    //    $scope.parameters = parameters;

    //}

    //var getString = function (data, column) {
    //    var string = "''";
    //    var collection = [];

    //    for (var i = 0; i < data.length; i++) {
    //        if (collection.includes(data[i][column]) == false) {
    //            string += ",'" + data[i][column] + "'";
    //            collection.push(data[i][column]);
    //        }
    //    }
    //    return string;
    //}

    //$scope.Report = function () {
    //    try {

    //        $scope.filterComplete();
    //        $scope.fileName = "MeetingReport.xlsx";
    //        $http({
    //            method: 'POST',
    //            url: $scope.path + "GetMeetingReport",
    //            data: { 'parameters': $scope.parameters },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == false) {
    //                // $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
    //                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        };
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }

    //}

}