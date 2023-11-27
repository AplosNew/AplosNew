'use strict';
FurniturePolicyReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function FurniturePolicyReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Furniture Policy Report';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/FurniturePolicyReport/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

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
        var dataList = [];
        var g = $("#GridOTCompensation").data("ejGrid");
        dataList = g.getFilteredRecords();
         
        if (dataList == 0) {
            dataList = $scope.PolicyGridList;
        }
        $scope.fileName = "Furniture Policy Report.xlsx"

        $http({
            method: 'POST',
            url: $scope.path + "XlsFurnitureWiseReport",
            data: { 'data': dataList, reportFileName: $scope.fileName},
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
                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    
}