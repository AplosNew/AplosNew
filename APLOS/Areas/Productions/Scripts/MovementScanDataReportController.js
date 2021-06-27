'use strict';
MovementScanDataReportController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MovementScanDataReportController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Movement-Scan-Data Report';


    $scope.path = 'Productions/MovementScanDataReport/';

    $scope.selectedValues = {
        FromDate: null,
        ToDate: null,
        FromId: null,
        ToId: null,
        EntityId: null,
        PurposeId: null,
    };


    $scope.clearFliters = function () {
        $scope.selectedValues = {
            FromDate: null,
            ToDate: null,
            FromId: null,
            ToId: null,
            EntityId: null,
            PurposeId: null,
        };
    }

    $scope.EntityList = [];
    $scope.GetEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetEntity',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.EntityList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GetEntity();

    $scope.PurposeList = [];
    $scope.GetPurpose = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPurposeCategory',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.PurposeList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GetPurpose();

    $scope.FromList = []; $scope.ToList = [];
    $scope.GetFrom = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetFrom',
            data: { EntityId: $scope.selectedValues.EntityId, PurposeId: $scope.selectedValues.PurposeId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.FromList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }


    $scope.GetTo = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetTo',
            data: { EntityId: $scope.selectedValues.EntityId, PurposeId: $scope.selectedValues.PurposeId, FromId: $scope.selectedValues.FromId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.ToList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    /// --- Grid Show
    $scope.MainData = [];
    $scope.loadGrid = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MainData.length != 0) {
            $scope.destroy();
        }
        if ($scope.General.$valid) {

            var ColumnList = [
                { field: 'WorkDate', width: 150, headerText: "Date", type: "string" },
                { field: 'Shift', width: 150, headerText: "Shift", type: "string" },
                { field: 'Time', width: 100, headerText: "Time", type: "string", allowFiltering: false },
                { field: 'LotNo', width: 120, headerText: "LotNo", type: "string" },
                { field: 'RefNo', width: 120, headerText: "RefNo", type: "string" },
                { field: 'ProductCode', width: 100, headerText: "Product Code", type: "string" },
                { field: 'PO', width: 100, headerText: "PO", type: "string" },
                { field: 'Article', width: 200, headerText: "Article", type: "string" },
                { field: 'ArticleCode', width: 150, headerText: "Article Code", type: "string" },
                { field: 'Cones', width: 80, headerText: "Cones", type: "string" },
                { field: 'NetWeight', width: 100, headerText: "Net Weight", type: "string", allowFiltering: false },
                { field: 'GWeight', width: 100, headerText: "Gross Weight", type: "string", allowFiltering: false },
                { field: 'Shade', width: 130, headerText: "Shade", type: "string" },
                { field: 'Grade', width: 80, headerText: "Grade", type: "string" },
                { field: 'OrderStatus', width: 100, headerText: "OrderStatus", type: "string" },
                { field: 'Purpose', width: 100, headerText: "Purpose", type: "string" },
                { field: 'PackedBy', width: 150, headerText: "By-Whom", type: "string" },
                { field: 'FromLoc', width: 150, headerText: "From", type: "string" },
                { field: 'ToLoc', width: 150, headerText: "To", type: "string" },
            ];

            if ($scope.selectedValues.FromId == undefined)
                $scope.selectedValues.FromId = null;
            if ($scope.selectedValues.ToId == undefined)
                $scope.selectedValues.ToId = null;

            $http({
                method: 'GET',
                url: $scope.path + 'GetData?FromLoc=' + $scope.selectedValues.FromId + '&ToLoc=' + $scope.selectedValues.ToId + '&FromDate='
                    + $scope.selectedValues.FromDate + '&ToDate=' + $scope.selectedValues.ToDate + '&EntityId=' + $scope.selectedValues.EntityId + '&PurposeId=' + $scope.selectedValues.PurposeId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.MainData = response.data.DATA;

                    $scope.summaryRows = [{
                        title: "Total :-", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetWeight", dataMember: "NetWeight", format: "{0:N0}" },
                        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GWeight", dataMember: "GWeight", format: "{0:N0}" }
                        ],
                        showCaptionSummary: true
                    }];


                    $("#GridData").ejGrid({
                        dataSource: $scope.MainData,
                        minWidth: 450, minHeight: 400,
                        allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                        filterSettings: { filterType: "excel" },
                        summaryRows: $scope.summaryRows,
                        columns: ColumnList
                    });


                    var gridObj = $("#GridData").data("ejGrid");
                    gridObj.refreshContent(true);
                    gridObj.refreshTemplate();

                    var x = document.getElementById("but");
                    if ($scope.MainData.length != 0) {
                        x.style.display = "block";
                    }
                    else {
                        x.style.display = "none";
                    }
                }

            });


        }

    }


    //Grid Destroy
    $scope.destroy = function () {
        var grid = $("#GridData").data("ejGrid");
        grid.destroy();
    }


    $scope.filteringData = function () {

        var gridobj = $("#GridData").data("ejGrid");
        var filteredRecords = gridobj.getFilteredRecords();

        if (filteredRecords.length == 0) {
            filteredRecords = $scope.MainData;
        }

        var parameters = [];
        parameters.push({ "Key": "Shade", "Value": getString(filteredRecords, "Shade") });
        parameters.push({ "Key": "ShiftId", "Value": getString(filteredRecords, "ShiftId") });
        parameters.push({ "Key": "ProductCode", "Value": getString(filteredRecords, "ProductCode") });
        parameters.push({ "Key": "PO", "Value": getString(filteredRecords, "PO") });
        parameters.push({ "Key": "Cones", "Value": getString(filteredRecords, "Cones") });
        parameters.push({ "Key": "RefNo", "Value": getString(filteredRecords, "RefNo") });
        parameters.push({ "Key": "LotNo", "Value": getString(filteredRecords, "LotNo") });
        parameters.push({ "Key": "PackedBy", "Value": getString(filteredRecords, "PackedBy") });
        parameters.push({ "Key": "Grade", "Value": getString(filteredRecords, "Grade") });
        parameters.push({ "Key": "OrderStatusId", "Value": getString(filteredRecords, "OrderStatusId") });
        parameters.push({ "Key": "WorkDate", "Value": getString(filteredRecords, "WorkDate") });
        parameters.push({ "Key": "ArticleId", "Value": getString(filteredRecords, "ArticleId") });
        parameters.push({ "Key": "ArticleCode", "Value": getString(filteredRecords, "ArticleCode") });
        parameters.push({ "Key": "PurposeId", "Value": getString(filteredRecords, "PurposeId") });
        parameters.push({ "Key": "FromLoc", "Value": getString(filteredRecords, "FromLoc") });
        parameters.push({ "Key": "ToLoc", "Value": getString(filteredRecords, "ToLoc") });
        applyFilters(parameters);


    }

    function applyFilters(parameters) {

        $http({
            method: 'POST',
            url: $scope.path + 'GetPrintReport',
            data: {
                From: $scope.selectedValues.FromDate, To: $scope.selectedValues.ToDate,
                EntityId: $scope.selectedValues.EntityId,
                Shade: parameters[0].Value,
                ShiftId: parameters[1].Value, ProductCode: parameters[2].Value,
                PO: parameters[3].Value, Cones: parameters[4].Value, RefNo: parameters[5].Value,
                LotNo: parameters[6].Value, PackedBy: parameters[7].Value, Grade: parameters[8].Value,
                OrderStatusId: parameters[9].Value, Date: parameters[10].Value, Article: parameters[11].Value,
                ArticleCode: parameters[12].Value,
                PurposeId: parameters[13].Value, FromLoc: parameters[14].Value, ToLoc: parameters[15].Value
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


    $scope.downloadgriddataUrl = 'GridReports/Download';


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


    $scope.nullAll = function () {
        $scope.selectedValues.FromDate = null;

        $scope.selectedValues.ToDate = null;
        $scope.selectedValues.FromId = null;
        $scope.selectedValues.ToId = null;
        $scope.selectedValues.PurposeId = null;
        $scope.FromList = []; $scope.ToList = [];
    }

}