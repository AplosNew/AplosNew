'use strict';
QRCodeGenerationController.$inject = ['cboService', '$window', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QRCodeGenerationController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Commitment";
    $scope.path = 'OrderManagements/QRCodeGeneration/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.OperationList = [];
    $scope.EmployeeList = [];

    $scope.getAllData = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetOperationQRCode"
        }).then(function successCallback(response) {
            $scope.OperationList = response.data;
        });

        $http({
            method: 'POST',
            url: $scope.path + "GetEmployeeQRCode"
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }
    $scope.getAllData();


    $scope.OperationQRCode = function () {
        var FilterModel = null;
        var gridObj = $("#gridOperation").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (angular.isUndefinedOrNull(filteredRecords) == true || filteredRecords.length == 0) {
            filteredRecords = $scope.OperationList;
        }

        var FilterString = getString(filteredRecords, "Id");



        try {
            $http({
                method: 'POST',
                url: $scope.path + "OperationQRCode",
                data: { Filter: FilterString },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {

        }

    };

    $scope.EmployeeQRCode = function () {

        try {
            var FilterModel = null;
            var gridObj = $("#gridEmployee").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (angular.isUndefinedOrNull(filteredRecords) == true || filteredRecords.length == 0) {
                filteredRecords = $scope.EmployeeList;
            }

            var FilterString = getString(filteredRecords, "Id");
            

            $http({
                method: 'POST',
                url: $scope.path + "EmployeeQRCode",
                data: { Filter: FilterString },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };



        } catch (e) {

        }

    };

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
}