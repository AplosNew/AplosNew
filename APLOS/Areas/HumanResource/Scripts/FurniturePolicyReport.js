'use strict';
FurniturePolicyReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FurniturePolicyReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Furniture Policy Report';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/FurniturePolicyReport/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.ModelTemp = {
        Designation:null,
        EmployeeCategory: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.EmployeeCategoryList = [];
    $scope.getEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeCategory",
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
           
        })

    }
    $scope.getEmployeeCategory();

    $scope.DesignationList = [];
    $scope.getDesignation = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDesignation',
            data: { 'employeeCategoryId': $scope.ModelNew.EmployeeCategory, },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.DesignationList = response.data;

        })
    }

    $scope.PolicyGridList = [];
    $scope.FurnitureView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPolicyGrid',
            data: {
                'designationId': $scope.ModelNew.Designation,
            },
            dataType:'JSON',
        }).then(function successCallback(response) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.Designation)) {
                ShowResult('Designation is Required.', 'failure');
                throw "Invalid Request";
            }
            $scope.PolicyGridList = response.data;
        })
    }

    $scope.FurnitureWiseReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsFurnitureWiseReport",
            data: { 'designationId': $scope.ModelNew.Designation },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                if (baseService.isUndefinedOrNull($scope.ModelNew.Designation)) {
                    ShowResult('Designation is Required.', 'failure');
                    throw "Invalid Request";
                }
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    
}