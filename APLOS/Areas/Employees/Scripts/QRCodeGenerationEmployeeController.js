'use strict';
QRCodeGenerationEmployeeController.$inject = ['cboService', '$window', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QRCodeGenerationEmployeeController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Employee QR Code";
    $scope.path = 'OrderManagements/QRCodeGenerationEmployee/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.EmployeeList = [];

    $scope.getAllData = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetEmployeeQRCode"
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }
    $scope.getAllData();


   
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