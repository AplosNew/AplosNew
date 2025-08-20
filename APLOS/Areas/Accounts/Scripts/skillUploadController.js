'use strict';
skillUploadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function skillUploadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Skill Upload";

    $scope.pathBalanceSheetScheduling = 'accounts/BalanceSheetScheduling/';
    //  #region Skill Data Upload Download
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.pathBalanceSheetScheduling + 'GetSkillSampleFile?reportFormat=' + ReportFormat;
    };
    $scope.EmployeeSkillUploadedData = [];
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
                    url: $scope.pathBalanceSheetScheduling + 'ImportSkillData',
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
                        $scope.EmployeeSkillUploadedData = [];
                        $scope.EmployeeSkillUploadedData = response.data;
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
    $scope.SaveEmployeeSkillUploadedData = function () {
        try {
            $scope.EmployeeSkillUploadedDataList = [];
            for (var i = 0; i < $scope.EmployeeSkillUploadedData.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.EmployeeSkillUploadedData[i].SkillId) && !baseService.isUndefinedOrNull($scope.EmployeeSkillUploadedData[i].EmpSystemId)) {
                    $scope.EmployeeSkillUploadedDataList.push($scope.EmployeeSkillUploadedData[i]);
                }
            }


            $.ajax({
                type: "POST",
                url: $scope.pathBalanceSheetScheduling + 'SaveEmployeeSkillData',
                data: {
                    'skillDataList': $scope.EmployeeSkillUploadedDataList
                },
                dataType: "json",
                success: function (response) {
                    if (response.Error === true) {
                        $scope.ShowSaveBtn = true;
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.EmployeeSkillUploadedData = [];
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
    //  #endregion EmployeeSkillUploadedData Upload Download
}