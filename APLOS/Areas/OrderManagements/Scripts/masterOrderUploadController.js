'use strict';
masterOrderUploadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function masterOrderUploadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Master Order Upload";

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //$scope.pathBalanceSheetScheduling = 'accounts/BalanceSheetScheduling/';
    //  #region Skill Data Upload Download
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = 'OrderManagements/MasterOrder/GetMOSampleFile?reportFormat=' + ReportFormat;
    };
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
    $scope.moData = [];

    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/ImportMOData',
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
                        $scope.moData = [];
                        $scope.moData = response.data;
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

    $scope.SaveMOData = function () {
        try {
            $scope.ShowSaveBtn = true;
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/SaveMOData',
                data: { 'dataList': $scope.moData },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowSaveBtn = true;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.moData = [];
                    $("#uploadImage").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
            $scope.ShowSaveBtn = false;
        }
    };
    //  #endregion EmployeeOperationUploadedData Upload Download

    

    $scope.EmployeeOperationList = [];
    $scope.getSavedOperationData = function () {
            $http({
                method: 'GET',
                url: 'Employees/EmployeeInformation/GetAllEmployeeOperationData'
            }).then(function successCallback(response) {
                $scope.EmployeeOperationList = response.data;
            });
       
    }

    $scope.EmployeeOperationArchiveList = [];
    $scope.getSavedOperationArchiveData = function () {
        $http({
            method: 'GET',
            url: 'Employees/EmployeeInformation/GetAllEmployeeOperationArchiveData'
        }).then(function successCallback(response) {
            $scope.EmployeeOperationArchiveList = response.data;
        });

    }

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.XlsReport = function () {
        var dataList = [];
        var g = $("#GridEmpMultiop").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.EmployeeOperationList;
        }

        $scope.fileName = 'EmployeeSkillMaster';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {
                'data': dataList,
                'reportFileName': $scope.fileName,
            },
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };

    $scope.XlsAReport = function () {
        var dataList = [];
        var g = $("#GridEmpMultiopa").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.EmployeeOperationArchiveList;
        }

        $scope.fileName = 'EmployeeSkillMaster';
        $http({
            method: "POST",
            url: $scope.exportgriddataUrl,
            data: {
                'data': dataList,
                'reportFileName': $scope.fileName,
            },
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
}