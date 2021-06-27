'use strict';
CompliedShiftController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter', 'cboService', '$window'];
function CompliedShiftController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, cboService, $window) {
    $rootScope.title = 'Complied Shift';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'humanresource/CompliedShift/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl, null, null, null, 'ShiftName', 'ShiftName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.modelList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Shift Name',
            'value': 'ShiftName'
        },
        {
            'name': 'InTime',
            'value': 'InTime'
        },
        {
            'name': 'OutTime',
            'value': 'OutTime'
        },
        {
            'name': 'IsNight',
            'value': 'IsNight'
        }
    ];

    $scope.model = {
        Id: null
        , PlantId: $window.plantId
        , CompanyGroupId: $window.companyGroupId
        , Code: null
        , ShiftName: null
        , InTime: null
        , OutTime: null
        , IsNight: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.modelNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.Get = function (index) {
        $scope.Action = 'Update';
        $scope.index = index;
        $scope.model = $scope.modelList[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.model);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelForm.$valid) {
                if (!$scope.manualValidationAddRemove()) {
                    angular.copy($scope.modelNew, $scope.model);
                    if ($scope.Action === 'Save') {
                        $http({
                            method: 'POST'
                            , url: $scope.saveUrl
                            , data: $scope.model
                            , dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.getData();
                                ClearFields();
                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        }
                    }
                    else if ($scope.Action === 'Update') {
                        $http({
                            method: 'POST'
                            , url: $scope.updateUrl
                            , data: $scope.model
                            , dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.getData();
                                ClearFields();
                            }
                        }, function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        });
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.validateTimeEntry = function (data) {
        try {
            var isValid = /^([0-1]?[0-9]|2[0-3]):([0-5][0-9])(:[0-5][0-9])?$/.test(data);
            if (!isValid)
                return false;
            else return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.modelList.splice($scope.index, 1);
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
        $scope.model = { PlantId: $scope.modelNew.PlantId, CompanyGroupId: $scope.modelNew.CompanyGroupId };
        $scope.modelNew = { PlantId: $scope.modelNew.PlantId, CompanyGroupId: $scope.modelNew.CompanyGroupId };
    }


    $scope.manualValidationAddRemove = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.InTime))
            return manualValidation('div_inTime', true, 'In time is required.');
        else manualValidation('div_inTime', false);
        if (baseService.isUndefinedOrNull($scope.modelNew.OutTime))
            return manualValidation('div_inTime', true, 'Out time is required.');
        else manualValidation('div_inTime', false);
        if (!$scope.validateTimeEntry($scope.modelNew.InTime))
            return manualValidation('div_inTime', true, "Invalid Intime " + $scope.modelNew.InTime + " </b> (Check Input Range *00:00 - 23:59* and Format *HH : mm*)");
        else manualValidation('div_inTime', false);
        if (!$scope.validateTimeEntry($scope.modelNew.OutTime))
            return manualValidation('div_outTime', true, "Invalid Outtime " + $scope.modelNew.OutTime + " </b> (Check Input Range *00:00 - 23:59* and Format *HH : mm*)");
        else manualValidation('div_outTime', false);
    };
}