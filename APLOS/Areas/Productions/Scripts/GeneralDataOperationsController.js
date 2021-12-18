'use strict';
GeneralDataOperationsController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GeneralDataOperationsController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'General Data Operations';
    $rootScope.title1 = 'General Data Operations';
    $scope.Action = 'Save';
    var url = "productions/GeneralDataOperations/";
    $scope.path = "productions/GeneralDataOperations/";

    // The Tab Switching Code

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        try {
            window.open('productions/GeneralDataOperations/GetSampleReport?reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    $scope.currentList = [];
    $scope.getCurrentFileList = function () {

        $http({
            method: 'GET',
            url: url + 'getCurrentList'
        }).then(function success(response) {
            $scope.currentList = [];
            $scope.currentList = response.data;
           
        })
    }


    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
    });
    $scope.ExcelUploadData = [];
    //IMporting The Data From the Excel File

$scope.ModelNew = {
        FileName: null
    }


    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fileData.length == 0 ) {
                
                throw ("Please Select A File!!");
            }
           

            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

                $http({
                    method: 'POST',
                    url: url + 'ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        fileData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                            fileData.append('file', data.file);
                           
                        }
                        return fileData;
                    },
                    data: { 'modelNew': $scope.ModelNew,  'file': $scope.fileData }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }

                    else {
                        try {
                            $scope.ExcelUploadData = response.data;
                           
                        }

                        catch (e) {

                            ShowResult(e, "failure");
                        }

                    }
                }, function errorCallback(response) {

                });
                return true;

            
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    //Save the File Data
    $scope.saveFileList = function () {

        $http({
            method: 'POST',
            url: url + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData}
        }).then(function successCallback(response) {
        if (response.data.Error === true) {
            ShowResult(response.data.Message, "failure");
        }
        else {
            try {
                
                //$scope.getCurrentFileList();
                $scope.ExcelUploadData = [];
                ShowResult(response.data.Message, 'success')
            }
            catch (e) {

                ShowResult(e, "failure");
            }
        }
    });
    }

    // 2nd Tab for the Downloading of the report
    $scope.MastersList = [];
    $scope.dataView = [];
    $scope.ToDate = null;
    $scope.FromDate = null;
    $scope.GDMasterId = null;
    $scope.getMasters = function () {
        $http({
            method: 'POST',
            url: url + 'getMasters',
        }).then(function successCallback(response) {
            $scope.MastersList = [];
            $scope.MastersList = response.data;
        });
    }

    $scope.getMasters();

    $scope.getTheReport = function () {

        if (angular.isUndefinedOrNull($scope.GDMasterId)) {
            ShowResult("Please Select the User Name!", 'failure');
            throw ('Invalid Request!');
        }

        if (angular.isUndefinedOrNull($scope.ToDate) || angular.isUndefinedOrNull($scope.FromDate)) {
            ShowResult("Please Select the Dates!", 'failure');
            throw ('Invalid Request!');
        }

        $http({
            method: 'POST',
            url: url + 'getReport',
            data: { 'ToDate': $scope.ToDate , 'FromDate':$scope.FromDate , 'MasterId':$scope.GDMasterId }
        }).then(function successCallback(response) {
            $scope.dataView = [];
            $scope.dataView = response.data;
        });
    }

    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.downloadTheReport = function () {

        $http({
            method: 'POST',
            url: $scope.path + 'downloadTheReport',
            data: { 'ToDate': $scope.ToDate, 'FromDate': $scope.FromDate, 'MasterId': $scope.GDMasterId },
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