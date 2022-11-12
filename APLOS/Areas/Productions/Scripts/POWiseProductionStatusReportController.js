'use strict';
POWiseProductionStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function POWiseProductionStatusReportController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $scope.title = 'PO Wise Production Status Report';
    $scope.path = 'Productions/POWiseProductionStatusReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';

    $scope.ToDate = null;
    $scope.FromDate = null;
    $scope.model = { State: 'EmployeeWise', Status: 'All', Task: 'WithTask' };

    $scope.ChangeState = function () {
        $scope.TaskManagementDataList = [];
    }

    $scope.Today = new Date();
    $scope.PreviousMonth = new Date().setDate(new Date().getDate() - 31);
    $scope.NextMonth = new Date().setDate(new Date().getDate()-1);
    $scope.FromDate = $filter("dateFiltering")($scope.PreviousMonth);
    $scope.ToDate = $filter("dateFiltering")($scope.NextMonth);

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            $http({
                method: 'GET',
                url: 'Productions/POWiseProductionStatusReportController/getFiltersData?fromDate=' + $scope.FromDate + '&todate=' + $scope.ToDate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                    { field: 'CustomerRef', width: 20, headerText: "Customer Ref", type: "string" },
                    { field: 'OwnRef', width: 20, headerText: "Own Ref", type: "string" },
                    { field: 'ProductCode', width: 20, headerText: "Product Code", type: "string" },
                    { field: 'PONo', width: 20, headerText: "PO No", type: "string" },
                    { field: 'LotNo', width: 20, headerText: "Lot No", type: "string" },
                    { field: 'OrderStatus', width: 20, headerText: "Order Status", type: "string" },
                    { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                    { field: 'ResponsiblePerson ', width: 20, headerText: "Responsible Person", type: "string" }

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
        parameters.push({ "Key": "DesignationGroupId", "Value": getString(fl, "DesignationGroupId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "UserReportGroup", "Value": getString(fl, "UserReportGroup") });
        parameters.push({ "Key": "TaskCreatedBy", "Value": getString(fl, "TaskCreatedBy") });

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
    $scope.POWiseProductionStatusDataList = [];

    $scope.GetPOWiseProductionStatusData = function () {
        try {
            //if (baseService.isUndefinedOrNull($scope.FromDate)) {
            //    throw "From Date is required.";
            //}
            //else if (baseService.isUndefinedOrNull($scope.ToDate)) {
            //    throw "To Date is required.";
            //}
            //else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
            //    throw "From date must be below or equal to To Date";
            //}
            //else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
            //    throw "To date must be above or equal to From Date.";
            //}

            //$scope.POWiseProductionStatusDataList = [];
            //$scope.filterComplete();

            $http({
                method: 'POST',
                url: 'Productions/POWiseProductionStatusReport/GetPOWiseProductionStatusData',
                //data: { 'parameters': $scope.parameters},
                //data: { 'parameters': $scope.parameters, 'fromDate': $scope.FromDate, 'todate': $scope.ToDate, 'model': $scope.model },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.POWiseProductionStatusDataList = response.data;
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ids = "";
    function filteredData() {
        $scope.ids = "";
        var dataList = [];
        var g = $("#GridEmp").data("ejGrid");
        dataList = g.getFilteredRecords();
        
        if (baseService.arrayLength(dataList) > 0) {
            for (var i = 0; i < dataList.length; i++) {
                if ($scope.ids == "") {
                    $scope.ids = "'','" + dataList[i].SystemId + "'";
                }
                else {
                    $scope.ids += ",'" + dataList[i].SystemId + "'";
                }
            }
        }

    }

    $scope.GetTaskManagementReport = function () {
        try {
            //if (baseService.isUndefinedOrNull($scope.FromDate)) {
            //    throw "From Date is required.";
            //}
            //else if (baseService.isUndefinedOrNull($scope.ToDate)) {
            //    throw "To Date is required.";
            //}
            //else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
            //    throw "From date must be below or equal to To Date";
            //}
            //else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
            //    throw "To date must be above or equal to From Date.";
            //}

            var dataList = [];
            //var g = $("#GridPrint").data("ejGrid");
            //dataList = g.getFilteredRecords();
            if (dataList.length == 0) {
                dataList = $scope.POWiseProductionStatusDataList;
            }


            //$scope.filterComplete();
            $scope.fileName = "POWiseProductionStatusReport.xlsx";
         
                //filteredData();
                $http({
                    method: 'POST',
                    url: $scope.exportgriddataUrl,
                    data: {
                        'reportFileName': $scope.fileName,
                        'data': dataList},
                    //data: { 'parameters': $scope.parameters, 'fromDate': $scope.FromDate, 'todate': $scope.ToDate, 'model': $scope.model, 'EmpIds': $scope.ids },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                        //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
           
            
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

  
    $scope.ProductionData = function () {

        try {
            var file_src = 'Productions/POWiseProductionStatusReport/ProductionDataXls';
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

}