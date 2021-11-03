'use strict';
materialSettingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function materialSettingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Material Setting";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.materialSettings = [];
    $scope.path = 'Materials/materialSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
   

    $scope.materialSetting = {
        Id: null,
        MaterialMasterTypeId: null,
        TypeValue: null
    };
    $scope.materialSettingNew = angular.copy($scope.materialSetting);

    $scope.materialSettings = [];
    $scope.getSavedData = function () {
        $scope.materialSettings = [];
        $http.get("Materials/materialSetting/GetList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.materialSettings = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();

    $scope.materialMasterTypeList = [];
    cboService.getMaterialMasterTypeCbo(function (result) {
        $scope.materialMasterTypeList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.materialSetting = $scope.materialSettings[$scope.index];
        $scope.materialSettingNew = angular.copy($scope.materialSetting);

        $scope.Action = 'Update';
      
    };

  

    $scope.materialTypeEnumList = [];
    cboService.getEnumCbo("enum/GetMaterialTypeEnumCbo", function (result) {
        $scope.materialTypeEnumList = result;
    });

    $scope.Save = function () {
        angular.copy($scope.materialSettingNew, $scope.materialSetting);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialSettingForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.materialSetting,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getSavedData();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.materialSetting,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getSavedData();
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.removeRowModal = function (data, index) {
        try {
            $scope.popUpIndex = index;
            $scope.materialSetting.Id = data.Id;
            $scope.message_confirmation = "Are you sure want to permanently delete [" + data.Type + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialSetting.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialSetting.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.materialSetting = {};
        $scope.materialSettingNew = {};
    }
};

