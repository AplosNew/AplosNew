'use strict';
BOQUploadController.$inject = ["addressService", 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function BOQUploadController(addressService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "BOQ Upload";
    $scope.Action = 'Save';
    $scope.path = 'OrderManagements/BOQUpload/';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.GetSampleFile = function () {
        try {
            var ReportFormat = 'Excel';
            location.href = 'OrderManagements/BOQUpload/GetSampleFile?reportFormat=' + ReportFormat;

        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.BOQData = [];
    $scope.ModelNew = {
        FileName: null
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
                    url: 'OrderManagements/BOQUpload/ImportData',
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
                        $scope.BOQData = response.data;
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

    $scope.save = function () {
        try {
            ValidationMaster();
            $http({
                method: 'POST',
                url: $scope.path + "Save",
                data: { 'BOQData': $scope.BOQData },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    function ValidationMaster() {
        try {
            for (var i = 0; i < $scope.BOQData.length; i++) {
                if (baseService.isUndefinedOrNull($scope.BOQData[i].MasterOrderItemId)) {
                    throw  "MasterOrderItemId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
                if (baseService.isUndefinedOrNull($scope.BOQData[i].MaterialMasterId)) {
                    throw  "MaterialMasterId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
                if (baseService.isUndefinedOrNull($scope.BOQData[i].ArticleId)) {
                    throw  "ArticleId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
                if (baseService.isUndefinedOrNull($scope.BOQData[i].CostingItemId)) {
                    throw  "CostingItemId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
                if (baseService.isUndefinedOrNull($scope.BOQData[i].UoMId)) {
                    throw  "UoMId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
                if (baseService.isUndefinedOrNull($scope.BOQData[i].ProcessId)) {
                    throw  "ProcessId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
                if (baseService.isUndefinedOrNull($scope.BOQData[i].ResponsiblePersonId)) {
                    throw  "ResponsiblePersonId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
                if (baseService.isUndefinedOrNull($scope.BOQData[i].VendorId)) {
                    throw  "VendorId for Id [" + $scope.BOQData[i].Id + "] is undefined";
                }
            }
        } catch (ex) {
            throw ex;
        }
    };

}