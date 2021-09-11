'use strict';
TablesUploadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TablesUploadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Roster Pattern Creation';
    $rootScope.title1 = 'Roster Pattern Planning';
    $scope.Action = 'Save';
    var url = "humanresource/TablesUpload/";



    
    $scope.fileData = [];
    $scope.GetSample = function (e) {
        var reportFormat = "Excel";


        try {
            window.open('humanresource/TablesUpload/GetSampleReport?reportFormat=' + reportFormat+'&tab='+e, '_blank');

        } catch (e) {

        }
    }

    //$scope.currentList = [];
    //$scope.getCurrentFileList = function () {

    //    $http({
    //        method: 'GET',
    //        url: url + 'getCurrentList'
    //    }).then(function success(response) {
    //        $scope.currentList = [];
    //        $scope.currentList = response.data;
    //    })
    //}


    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
        $('#uploadFileTwo').val('');
        $('#uploadFileThree').val('');
    });

    $("#uploadFileTwo").change(function () {
        $scope.fileData = this.files[0];
        $('#uploadFile').val('');
        $('#uploadFileThree').val('');
    });

    $("#uploadFileThree").change(function () {
        $scope.fileData = this.files[0];
        $('#uploadFile').val('');
        $('#uploadFileTwo').val('');
    });

    $scope.ExcelUploadData = [];

    
    //IMporting The Data From the Excel File

$scope.ModelNew = {
        FileName: null
    }


    $scope.ImportData = function (e) {
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
                            fileData.append('Imp', e);
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
    $scope.saveFileList = function (e) {

        
        

        $http({
            method: 'POST',
            url: url + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData , 'tab': e}
        }).then(function successCallback(response) {
        if (response.data.Error === true) {
            ShowResult(response.data.Message, "failure");
        }
        else {
            try {
                if ($rootScope.isCollapsed == true) {
                    $rootScope.toggle();
                }
                //$scope.getCurrentFileList();
                ShowResult(response.data.Message, 'success')
            }
            catch (e) {

                ShowResult(e, "failure");
            }
        }
    });
    }

    //$scope.onFileSelect = function(event) {
    //    if (event.target.files.length > 0) {
    //        console.log(event.target.files[0].name);
    //    }
    //}
}