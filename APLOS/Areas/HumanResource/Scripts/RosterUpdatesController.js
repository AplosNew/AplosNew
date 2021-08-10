'use strict';
RosterUpdatesController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function RosterUpdatesController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Roster Pattern Creation';
    $rootScope.title1 = 'Roster Pattern Planning';
    $scope.Action = 'Save';
    var url = "humanresource/RosterUpdates/";

    //Get Plants List 
    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: url + 'getPlants'
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }

    $scope.getPlants();


    $scope.PlantId = null;
    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        if ($scope.PlantId == "" || $scope.PlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Invalid!!");
        }

        var plantName = "";
        for (var i = 0; i < $scope.PlantList.length; i++) {
            if ($scope.PlantList[i].Value == $scope.PlantId) {
                plantName = $scope.PlantList[i].Text;
            }
        }

        try {
            window.open('humanresource/RosterUpdates/GetSampleReport?plantId='+$scope.PlantId+'&name='+plantName+'&reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    $scope.currentList = [];
    $scope.getCurrentFileList = function () {

        if ($scope.PlantId == "" || $scope.PlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Invalid!!");
        }

        $http({
            method: 'GET',
            url: url + 'getCurrentList',
            params:{'plantId':$scope.PlantId}
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
            if ($scope.PlantId == "" || $scope.PlantId == undefined) {
                ShowResult("Please First Select a Plant!!", 'failure');
                throw ("Please First Select a Plant!!");
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
                            fileData.append('plantId', $scope.PlantId);
                        }
                        return fileData;
                    },
                    data: { 'modelNew': $scope.ModelNew,  'file': $scope.fileData , 'plantId':$scope.PlantId }
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

        if ($scope.PlantId == "" || $scope.PlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Please First Select a Plant!!");
        }

        

        $http({
            method: 'POST',
            url: url + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData, 'plantId': $scope.PlantId}
        }).then(function successCallback(response) {
        if (response.data.Error === true) {
            ShowResult(response.data.Message, "failure");
        }
        else {
            try {
                if ($rootScope.isCollapsed == true) {
                    $rootScope.toggle();
                }
                $scope.getCurrentFileList();
                ShowResult(response.data.Message, 'success')
            }
            catch (e) {

                ShowResult(e, "failure");
            }
        }
    }, function errorCallback(response) {

    });
    }
}