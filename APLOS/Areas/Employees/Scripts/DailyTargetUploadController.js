'use strict';
DailyTargetUploadController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyTargetUploadController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Daily Target Upload';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.bloodGroups = [];
    $scope.path = 'employees/EmployeeInformation/';
    $scope.path2 = 'Productions/EmployeeOperations/';
    $scope.TargetDate= $filter("dateFiltering")(Date.now());
   
    $scope.POList = [];
    $scope.ShiftList = [];
    $scope.EntityList = [];
    $scope.ProcessList = [];

    $scope.getStartUp = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + 'GetEntity'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
    }
    $scope.getStartUp();
    $scope.getProcess = function () {
            $http({
                method: 'POST',
                url: $scope.path2 + 'GetProcess',
                data: { 'EId': $scope.EntityId }
            }).then(function succ(resp) {
                $scope.ProcessList = resp.data;
            });
        }

       
        $scope.GetShiftList = function () {
            $http.get('Productions/EmployeeOperations/GetShift?processId=' + $scope.ProcessId + '&entityId=' + $scope.EntityId)
                .then(function (response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ShiftList = response.data;
                        if (baseService.arrayLength(response.data) === 1) {
                            $scope.shiftId = $scope.ShiftList[0].Value;
                        }
                    }
                });
        }

        // Getting the POs
        $scope.getPo = function () {
            $http({
                method: 'POST',
                url: $scope.path2 + 'GetPOs',
                data: { 'entityId': $scope.EntityId },
            }).then(function succ(resp) {
                $scope.POList = resp.data;
            });
        }

    $scope.UploadedData = [];
    $scope.picdata = null;
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

    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetDailyTargetSampleFile?reportFormat=' + ReportFormat + '&entityId=' + $scope.EntityId
            + '&targetDate=' + $scope.TargetDate + '&processId=' + $scope.ProcessId + '&shiftId=' + $scope.shiftId;
    };

    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: $scope.path + 'ImportDailyTargetData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.UploadedData = [];
                        $scope.UploadedData = response.data;
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
    $scope.SaveUploadedData = function () {
        try {
            for (var i = 0; i < $scope.UploadedData.length; i++) {
               
                $scope.UploadedData[i].Id = null;
                $scope.UploadedData[i].TargetDate = $scope.TargetDate;

            }
            $http({
                method: 'POST',
                url: $scope.path + 'SaveDailyTargetUploadedData?targetDate=' + $scope.TargetDate + '&processId=' + $scope.ProcessId + '&shiftId=' + $scope.shiftId,
                data: {
                    'data': $scope.UploadedData
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.UploadedData = [];
                    $("#uploadImage").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };


}