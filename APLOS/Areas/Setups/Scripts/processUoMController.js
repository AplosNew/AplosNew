'use strict';
processUoMController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function processUoMController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Process UoM";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.brands = [];
    $scope.path = 'Setups/ProcessUoM/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'ProcessName,BaseUoMName', 'ProcessName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.brands = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchList = [
        {
            'name': 'Process',
            'value': 'ProcessName'
        },
        {
            'name': 'BaseUoM',
            'value': 'BaseUoMName'
        },
        {
            'name': 'Capacity Name',
            'value': 'CapacityName'
        },
        {
            'name': 'First UoM',
            'value': 'CapacityFirstUoMName'
        },
        {
            'name': 'Second UoM',
            'value': 'CapacitySecondUoMName'
        }
    ];
    $scope.brand = {
        Id: null
        , CompanyGroupId: null
        , ProcessId: null
        , ProcessName: null
        , BaseUoMId: null
        , BaseUoMName: null
        , CapacityName: null
        , CapacityFirstUoMId: null
        , CapacityFirstUoMName: null
        , CapacitySecondUoMId: null
        , CapacitySecondUoMName: null
    };
    $scope.brandNew = Object.assign({}, $scope.brand);

    $scope.processList = [];
    cboService.getProcessCbo(function (result) {
        $scope.processList = result;
    });
    $scope.uomList = [];
    cboService.getUoMCbo(function (result) {
        $scope.uomList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.brands[$scope.index], $scope.brand);
        angular.copy($scope.brand, $scope.brandNew);
        GetAlternativeUoMList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed)
            $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            angular.copy($scope.brandNew, $scope.brand);
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'entity': $scope.brand,
                        'alternativeUoMList': $scope.alternativeUoMList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.copy(response.data.ProcessUoM, $scope.brand);
                        $scope.brand.ProcessName = angular.element("#processId :selected").text();
                        $scope.brand.BaseUoMName = angular.element("#baseUoMId :selected").text();
                        $scope.brand.CapacityFirstUoMName = angular.element("#capacityFirstUoMId :selected").text();
                        $scope.brand.CapacitySecondUoMName = angular.element("#capacitySecondUoMId :selected").text();
                        $scope.brands.push($scope.brand);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'entity': $scope.brand,
                        'alternativeUoMList': $scope.alternativeUoMList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.brand.ProcessName = angular.element("#processId :selected").text();
                            $scope.brand.BaseUoMName = angular.element("#baseUoMId :selected").text();
                            $scope.brand.CapacityFirstUoMName = angular.element("#capacityFirstUoMId :selected").text();
                            $scope.brand.CapacitySecondUoMName = angular.element("#capacitySecondUoMId :selected").text();
                            $scope.brands[$scope.index] = $scope.brand;
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.brandNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.brandNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.brands.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.brand = {};
        $scope.brandNew = {};
        $scope.alternativeUoMList = [];
    }

    // #region AlternativeUOM
    $scope.altUomIndex = -1;
    $scope.valueSetInAltUoM = function () {
        $scope.alternativeUoMNew.BaseUoMId = $scope.brandNew.BaseUoMId;
        $scope.alternativeUoMNew.BaseUoMName = angular.element("#baseUoMId :selected").text();
    }
    $scope.AltUomAction = 'Add Alternative UOM';
    $scope.alternativeUoMList = [];
    $scope.altUomIndex = -1;
    $scope.alternativeUoM = {
        Id: null,
        ProcessUoMId: null,
        AlternativeUoMId: null,
        AlternativeUoMName: null,
        AlternativeUoMFactor: 1,
        BaseUoMId: null,
        BaseUoMName: null,
        BaseUoMFactor: null
    };
    $scope.alternativeUoMNew = angular.copy($scope.alternativeUoM);
    function GetAlternativeUoMList() {
        $http({
            method: 'GET',
            url: 'Setups/ProcessUoM/GetAltUomList?masterId=' + $scope.brandNew.Id,
        }).then(function successCallback(response) {
            $scope.alternativeUoMList = response.data;
            $scope.alternativeUoMNew.BaseUoMName = angular.element("#baseUoMId :selected").text();
        });
    }
    $scope.GetMGMAlternativeUoM = function (id, index) {
        $scope.altUomIndex = index;
        $scope.alternativeUoM = $scope.alternativeUoMList[$scope.altUomIndex];
        $scope.alternativeUoMNew = angular.copy($scope.alternativeUoM);
        $scope.AltUomAction = 'Update Alternative UoM';
    }
    $scope.addAlternativeUoM = function () {
        try {
            if ($scope.brandNew.BaseUoMId == null)
                throw 'Please select base uom.';
            if ($scope.alternativeUoMNew.AlternativeUoMId == null)
                throw 'Please select alternative uom';
            if ($scope.brandNew.BaseUoMId == $scope.alternativeUoMNew.AlternativeUoMId)
                throw 'Base uom and alternative uom can not be same. Please select another alternative uom.';
            var isAvailable = false;
            $scope.alternativeUoMNew.AlternativeUoMName = angular.element("#altUOMId :selected").text();
            $scope.alternativeUoMNew.BaseUoMName = angular.element("#baseUoMId :selected").text();
            for (var i = 0; i < $scope.alternativeUoMList.length; i++) {
                isAvailable = baseService.isAvailableInList($scope.alternativeUoMList[i].AlternativeUoMId, $scope.alternativeUoMNew.AlternativeUoMId, i, $scope.altUomIndex);
                if (isAvailable)
                    throw 'This alternative uom : [' + $scope.alternativeUoMNew.AlternativeUoMName + '] has been already taken. Please select another alternative uom';
            }
            if ($scope.alternativeUoMNew.BaseUoMFactor > 0) {
                $scope.alternativeUoM = Object.assign({}, $scope.alternativeUoMNew);
                // isAvailable true == add new
                if (!isAvailable) {
                    if ($scope.altUomIndex == -1) {
                        $scope.alternativeUoM.BaseUoMId = $scope.brandNew.BaseUoMId;
                        $scope.alternativeUoMList.push($scope.alternativeUoM);
                        clearAltUOM($scope.alternativeUoMNew.BaseUoMId, $scope.alternativeUoMNew.BaseUoMName);
                    }
                    else {
                        $scope.alternativeUoMList[$scope.altUomIndex] = $scope.alternativeUoM;
                        clearAltUOM($scope.alternativeUoMNew.BaseUoMId, $scope.alternativeUoMNew.BaseUoMName);
                    }
                    $scope.AltUomAction = 'Add Alternative UOM';
                    $scope.altUomIndex = -1;
                }
            } else
                throw 'Please insert base uom factor';
        } catch (err) {
            ShowResult(err, 'failure');
        }
    }

    $scope.mauid = null;
    $scope.mauindex = -1;
    $scope.valuePassInDelModal = function (id, index, altUomName) {
        $scope.mauid = id;
        $scope.mauindex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + altUomName + ' ]';
        angular.element(document.querySelector('#mmaltuom')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.alternativeUoMList.length; i++) {
            if ($scope.alternativeUoMList[i].AlternativeUoMId == $scope.mauid) {
                $scope.alternativeUoMList.splice($scope.mauindex, 1);
                break;
            }
        }
        $scope.mauid = null;
        $scope.mauindex = -1;
    };
    function clearAltUOM(baseUoMId, baseUoM) {
        $scope.alternativeUoMNew = {
            Id: null,
            ProcessUoMId: $scope.brandNew.Id,
            AlternativeUoMId: null,
            AlternativeUoMName: null,
            AlternativeUoMFactor: 1,
            BaseUoMId: $scope.brandNew.BaseUoMId,
            BaseUoMName: baseUoM,
            BaseUoMFactor: null
        };
        $scope.alternativeUoM = {};
    };
    // #endregion
}