'use strict';
BulkIncrementSalaryStructureDataUploadController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader'];
function BulkIncrementSalaryStructureDataUploadController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader) {
    $scope.path = 'Payrolls/BulkIncrementSalaryStructureDataUpload/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.title = 'Salary Structure Data Upload';
    $scope.AttdnRawData = [];
    $scope.picdata = null;

    $scope.EffectiveDate  = null;
    $scope.NextDueDate = null;

    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.ModelNew = {
        Id: null,
        FileName: null

    };
    $scope.ImportData = function () {
        try {


            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }


                $http({
                    method: 'POST',
                    url: $scope.path+ 'ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.AttdnRawData = [];
                        $scope.AttdnRawData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    $scope.onrowdatabound = function (e) {
        if (e.data.Remarks !== '')
            e.row.css("background-color", "red");
    };



    $scope.save = function () {

        try {

                if(baseService.isUndefinedOrNull($scope.EffectiveDate)) {
                   throw "Please Enter Effective Date";
                }
                if(baseService.isUndefinedOrNull($scope.NextDueDate)) {
                   throw "Please Enter Next Due Date";
                }
            for (var i = 0; i < $scope.AttdnRawData.length; i++) {

                if ($scope.AttdnRawData[i].Remarks !== '') {
                    throw "Please Upload valied data";
                }
                $scope.AttdnRawData[i].EffectiveDate =$scope.EffectiveDate;
                $scope.AttdnRawData[i].NextDueDate =$scope.NextDueDate;
            }

       

            $.ajax({
                type: "POST",
                url: $scope.path+'SaveData',
                data: { 'data': $scope.AttdnRawData },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.AttdnRawData = [];
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path+'GetSampleFile?reportFormat=' + ReportFormat;
    };


}





