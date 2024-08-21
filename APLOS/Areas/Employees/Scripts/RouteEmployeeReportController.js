'use strict';
RouteEmployeeReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function RouteEmployeeReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Route Employee Report';
    $scope.Action = 'Save';
    $scope.path = 'Employees/RouteEmployee/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    // Tab Change
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 ;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.tab3;
    $scope.setTab3 = function (newTab) {
        $scope.tab3 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab3 === tabNum;
    };


    //Route Emp Start

    $scope.ModelList = [];
    $scope.view = function () {
        $http({
            method: "Get",
            url: $scope.path + 'GetRouteEmployeesData',
            //data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        })
    }
    $scope.view();

    $scope.AssignReport = function () {
        $scope.fileName = 'Summary List';

        var dataList = [];
        var g = $("#GridRouteEmp").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.ModelAssignEmployeeList = [];
    $scope.AssignEmployeeView = function () {
        if (baseService.isUndefinedOrNull($scope.PlantId)) {
            $scope.PlantId = $window.plantId;
        }
        $http({
            method: "Get",
            url: $scope.path + 'getemployeeListRoute?PlantId=' + $scope.PlantId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelAssignEmployeeList = response.data;
        })
    }
    $scope.AssignEmployeeView();

    $scope.AssignEmployeeReport = function () {
        $scope.fileName = 'To Assign List';

        var dataList = [];
        var g = $("#GridEmp").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelAssignEmployeeList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };


    $scope.UnassignReport = function () {
        $scope.fileName = 'To Unassign List';
        var dataList = [];
        var g = $("#GridEUnassign").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelUnassignList;
        }
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };


    $scope.ModelUnassignList = [];
    $scope.UnassignView = function () {
        if (baseService.isUndefinedOrNull($scope.PlantId)) {
            $scope.PlantId = $window.plantId;
        }
        $http({
            method: "Get",
            url: $scope.path + 'viewUnassign?PlantId=' + $scope.PlantId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelUnassignList = response.data;
        })
    }
    $scope.UnassignView();

    $scope.ModelTransportSummaryList = [];
    $scope.viewTransportSummary = function () {
        $http({
            method: "Get",
            url: $scope.path + 'GetTransportSummaryData',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelTransportSummaryList = response.data;
        })
    }
    $scope.viewTransportSummary();

    $scope.TransportSummaryReport = function () {
        $scope.fileName = 'Transport Status Detail';

        var dataList = [];
        var g = $("#GridTranSummary").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelTransportSummaryList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.GetBusVerificationReport = function () {
        try {
            
            $scope.fileName = "BusVerification.xls";


            $scope.ReportFormat = 'Excel';
            // $scope.ReportFormat = 'Pdf';
            var url = 'Employees/RouteEmployee/GetBusVerificationReport?reportFormat=' + $scope.ReportFormat;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
  
}