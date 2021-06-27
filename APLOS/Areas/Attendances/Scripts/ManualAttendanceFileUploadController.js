'use strict';
ManualAttendanceFileUploadController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader'];
function ManualAttendanceFileUploadController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader) {
    $scope.path = 'Attendances/ManualAttendanceFileUpload/';
    $rootScope.title = 'Manual Attendance File Upload';
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.GetSampleFile = function () {
        try {
            var ReportFormat = 'Excel';
            location.href = 'Attendances/ManualAttendanceFileUpload/GetSampleFile?reportFormat=' + ReportFormat;

        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.ManualAttdnFile = {
        Id: null,
        FileId: null,
        FileName: null,
        FileStatus: null,
        PlantId: $window.plantId,
    }

    $scope.picdata = null;
    //$("#uploadImage").change(function () {
    //    $scope.picData = this.files[0];
    //});
    $scope.AttdnManualData = [];
    $scope.save = function () {
        try {
            if ($scope.picdata != null) {
                var picData = new FormData();
                //if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'Attendances/ManualAttendanceFileUpload/Create',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("ManualAttdnFile", angular.toJson(data.ManualAttdnFile));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'ManualAttdnFile': $scope.ManualAttdnFile,
                        'file': $scope.picdata
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getMaster();
                        document.getElementById("uploadImage").value = '';
                    }
                }, function errorCallback(response) {
                    $scope.savedisable = false;
                    $scope.showdiv = false;
                });
                return true;
                //}
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.MasterList = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetMaster",
        }).then(function successCallback(response) {
            $scope.MasterList = response.data;
            //for (var i = 0; i < response.data.length; i++) {
            //}
            //$scope.MasterList = $filter('dateFiltering')(response.data.AddedDate, 'dd-MMM-yyyy');
        });
    }
    $scope.getMaster();

    //#region Delete
    $scope.RemoveMaster = function (obj) {
        $scope.Id = obj.data.Id;
        $scope.FileId = obj.data.FileId;
        //$scope.FileStatus = obj.data.FileStatus;
        if (!baseService.isUndefinedOrNull($scope.Id, $scope.FileId))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
    }
    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteMaster?Id=' + $scope.Id + '&File=' + $scope.FileId,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMaster();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };
    //#endregion
}





