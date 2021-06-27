'use strict';
biometricDeviceAsShortLeaveController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function biometricDeviceAsShortLeaveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Biometric Device As Short Leave';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.biometricDeviceAsShortLeaves = [];
    $scope.path = 'Attendances/biometricDeviceAsShortLeave/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Plant', 'Plant');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.biometricDeviceAsShortLeaves = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.biometricDeviceAsShortLeave = {
        SystemID: null,
        GroupID: null,
        PlantID: null,
        MachineID: null,
        MachineIP: null,
        IsActive: true,
        IsAdmin: false,
        AdminEnrollID: null,
        AdminPassword: null,
        AdminProxiCard: null,
        Description: null,
        Remarks: null,
        OneFlag: null,
        ZeroFlag: null,
        RegisTypeDec: false,
        RegisTypeHex: false,
        RegisCharacter: 0,
        DownLdEnrollID: false,
        DownLdTypeDec: false,
        DownLdTypeHex: false,
        DownLdTypeScan: false,
        DownLdCharacter: 0,
        IsDataClearAftDW: false
    };

    $scope.biometricDeviceAsShortLeaveNew = Object.assign({}, $scope.biometricDeviceAsShortLeave);

    $scope.searchByList = [
        {
            'name': 'Plant',
            'value': 'Plant'
        },
        {
            'name': 'Machine Id',
            'value': 'MachineID'
        },
        {
            'name': 'Machine IP',
            'value': 'MachineIP'
        }
    ];


    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.biometricDeviceAsShortLeave = $scope.biometricDeviceAsShortLeaves[$scope.index];
        $scope.biometricDeviceAsShortLeaveNew = Object.assign({}, $scope.biometricDeviceAsShortLeave);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SetChecked = function (event) {
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeDec = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeHex = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeScan = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdEnrollID = event.currentTarget.checked;
    }

    $scope.SetDecimalChecked = function (event) {
        $scope.biometricDeviceAsShortLeaveNew.DownLdEnrollID = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeHex = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeScan = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeDec = event.currentTarget.checked;
    }
    $scope.SetHexaDecimalChecked = function (event) {
        $scope.biometricDeviceAsShortLeaveNew.DownLdEnrollID = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeDec = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeScan = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeHex = event.currentTarget.checked;
    }
    $scope.SetScanChecked = function (event) {
        $scope.biometricDeviceAsShortLeaveNew.DownLdEnrollID = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeDec = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeHex = false;
        $scope.biometricDeviceAsShortLeaveNew.DownLdTypeScan = event.currentTarget.checked;
    }
    $scope.SetRegDecimalChecked = function (event) {
        $scope.biometricDeviceAsShortLeaveNew.RegisTypeHex = false;
        $scope.biometricDeviceAsShortLeaveNew.RegisTypeDec = event.currentTarget.checked;
    }
    $scope.SetRegHexDecimalChecked = function (event) {
        $scope.biometricDeviceAsShortLeaveNew.RegisTypeDec = false;
        $scope.biometricDeviceAsShortLeaveNew.RegisTypeHex = event.currentTarget.checked;
    }
    $scope.Save = function () {
        angular.copy($scope.biometricDeviceAsShortLeaveNew, $scope.biometricDeviceAsShortLeave);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.BiometricDeviceAsShortLeaveNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.biometricDeviceAsShortLeave,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.biometricDeviceAsShortLeaves.push(response.data.biometricDeviceAsShortLeave);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.biometricDeviceAsShortLeave,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.biometricDeviceAsShortLeaves[$scope.index] = $scope.biometricDeviceAsShortLeave;
                        }
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.BiometricDeviceAsShortLeaveNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.BiometricDeviceAsShortLeaveNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.BiometricDeviceAsShortLeaves.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.BiometricDeviceAsShortLeave = {};
        $scope.BiometricDeviceAsShortLeaveNew = {};
    }
}