'use strict';
AttendanceManualDataUploadController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader'];
function AttendanceManualDataUploadController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader) {
    $scope.path = 'Attendances/AttendanceManualDataUpload/';
    $rootScope.title = 'Attendance Manual Data Upload';
    $scope.date = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.GetSampleFile = function () {
        try {
            if ($scope.date == null || $scope.date == "" || $scope.date == 'undefined') {
                throw "Select a Date first";
            }
            var ReportFormat = 'Excel';
            location.href = 'Attendances/AttendanceManualDataUpload/GetSampleFile?reportFormat=' + ReportFormat + '&date=' + $scope.date;

        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope, $scope.file)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };
    $scope.ModelNew = {
        FileName: null
    }

    function GetShortList(list) {
        var list2 = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === null || list[i].EmployeeCode === '' || list[i].EmployeeCode === 'undefined') {

            }
            else {
                list2.push(list[i]);
            }
        }
        return list2;
    }

    $scope.ImportData = function () {
        try {
            $scope.msg = "";
            //$scope.btnProcess = true;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }
                $http({
                    method: 'POST',
                    url: 'Attendances/AttendanceManualDataUpload/ImportData',
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
                        //$scope.AttdnManualData = response.data;

                        $scope.AttdnManualData = [];
                        var x = GetShortList(response.data);
                        $scope.AttdnManualData = x;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    $scope.AttdnManualData = [];
    $scope.save = function () {

        try {
            //for (var i = 0; i < $scope.AttdnManualData.length; i++) {

            //    if ($scope.AttdnManualData[i].Remarks !== '') {
            //        throw "Please Upload valied data";
            //    }
            //}
            $.ajax({
                type: "POST",
                url: 'Attendances/AttendanceManualDataUpload/SaveAttendanceManualData',
                data: { '_listUI': $scope.AttdnManualData, 'fromDate': $scope.date, 'toDate': $scope.date },
                dataType: "json",
                success: function (response) {
                    if (response.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.AttdnManualData = [];
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
}





