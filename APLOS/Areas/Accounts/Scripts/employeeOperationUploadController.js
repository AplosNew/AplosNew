'use strict';
employeeOperationUploadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function employeeOperationUploadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Employee Operation Upload";

    $scope.pathBalanceSheetScheduling = 'accounts/BalanceSheetScheduling/';
    //  #region Skill Data Upload Download
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.pathBalanceSheetScheduling + 'GetOperationSampleFile?reportFormat=' + ReportFormat;
    };
    $scope.EmployeeOperationUploadedData = [];
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
    $scope.ShowSaveBtn = false;
    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: $scope.pathBalanceSheetScheduling + 'ImportEmployeeOperationData',
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
                        $scope.EmployeeOperationUploadedData = [];
                        $scope.EmployeeOperationUploadedData = response.data;
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
    $scope.SaveEmployeeOperationUploadedData = function () {

        try {
            $.ajax({
                type: "POST",
                url: $scope.pathBalanceSheetScheduling + 'SaveEmployeeOperationData',
                data: {
                    'operationDataList': $scope.EmployeeOperationUploadedData
                },
                dataType: "json",
                success: function (response) {
                    if (response.Error === true) {
                        $scope.ShowSaveBtn = true;
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.EmployeeOperationUploadedData = [];
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };
    //  #endregion EmployeeOperationUploadedData Upload Download
}