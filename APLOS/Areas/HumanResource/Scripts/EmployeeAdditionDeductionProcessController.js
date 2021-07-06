'use strict';
EmployeeAdditionDeductionProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeAdditionDeductionProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.SelectedDate = null;
    $scope.Data = [];
    $scope.run = function () {

        $http({
            method: 'GET',
            url: 'HumanResource/EmployeeAdditionDeductionProcess/RunProcess',
            params: { 'date': $scope.SelectedDate },
        }).then(function succ(resp) {
            $scope.Data = resp.data;
            console.log(resp.data);
        })
    }

   

    $scope.printCurrentReport = function () {

        if ($scope.Data.length <=0 ) {
            ShowResult("Please First Process and if Processed then There are nothing new!! Please download the Saved Table Data!!");
            throw ("invalid");
        }

        $http({
            method: 'POST',
            url: 'HumanResource/EmployeeAdditionDeductionProcess/GetCurrentReport',
            data: { 'data': $scope.Data }
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

    $scope.printSavedReport = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/EmployeeAdditionDeductionProcess/GetSavedReport'
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
