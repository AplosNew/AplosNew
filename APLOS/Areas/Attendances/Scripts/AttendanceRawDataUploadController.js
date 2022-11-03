'use strict';
AttendanceRawDataUploadController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader'];
function AttendanceRawDataUploadController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader) {
    $scope.path = 'Attendances/AttendanceRawDataUpload/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.title = 'Employee Profile';
    $scope.AttdnRawData = [];
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $scope.disableSaveBtn = false;
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
                    url: 'Attendances/AttendanceRawDataUpload/ImportData',
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
            for (var i = 0; i < $scope.AttdnRawData.length; i++) {

                if ($scope.AttdnRawData[i].Remarks !== '') {
                    throw "Please Upload valied data";
                }

            }

            //$http({
            //    method: 'POST',
            //    url: "Attendances/AttendanceRawDataUpload/SaveAttendanceRawData",
            //    //data: JSON.stringify($scope.AttdnRawData),
            //    transformRequest: angular.identity,
            //    headers: { 'Content-Type': undefined },
            //    data: { 'AttendanceRawData': $scope.AttdnRawData },
            //    //headers: {
            //    //    'Content-Type': 'application/json'
            //    //}
            //}).then(function successCallback(response) {
            //    if (response.data.Error === true) {
            //        ShowResult(response.data.Message, 'failure');
            //    }
            //    else {
            //        ShowResult(response.data.Message, 'success');
            //        $scope.AttdnRawData = [];
            //        $("#uploadImage").val(null);
            //        $scope.ShowSaveBtn = false;
            //    }
            //}), function errorCallBack(response) {
            //    ShowResult(response.data.Message, 'failure');
            //    };







            //$http({
            //    method: 'POST',
            //    url: 'Attendances/AttendanceRawDataUpload/SaveAttendanceRawData',
            //    headers: { 'Content-Type': undefined },
            //    transformRequest: function (data) {
            //        picData.append("modelNew", angular.toJson(data.modelNew));
            //        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
            //            picData.append('file', data.file);
            //        }
            //        return picData;
            //    },
            //    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
            //}).then(function successCallback(response) {
            //    if (response.data.Error === true) {
            //        ShowResult(response.data.Message, "failure");

            //    }
            //    else {
            //        $scope.AttdnRawData = [];
            //        $scope.AttdnRawData = response.data;
            //        $scope.ShowSaveBtn = true;
            //    }
            //}, function errorCallback(response) {

            //});
            $scope.disableSaveBtn = true;

            $.ajax({
                type: "POST",
                url: 'Attendances/AttendanceRawDataUpload/SaveAttendanceRawData',
                data: { 'AttendanceRawData': $scope.AttdnRawData },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                        $scope.ShowSaveBtn = false;
                        $scope.disableSaveBtn = false;

                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.AttdnRawData = [];
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                        $scope.disableSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    //$scope.GetSampleFile = function () {



    //    $http({
    //        method: 'GET',
    //        url: "Attendances/AttendanceRawDataUpload/GetSampleFile",
    //        headers: {
    //            'Content-Type': 'application/json'
    //        }
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //    }), function errorCallBack(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    };
    //};
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = 'Attendances/AttendanceRawDataUpload/GetSampleFile?reportFormat=' + ReportFormat;
    };


}





