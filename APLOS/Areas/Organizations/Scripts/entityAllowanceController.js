'use strict';
entityAllowanceController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function entityAllowanceController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Entity Allowance';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.entityAllowances = [];
    $scope.path = 'Organizations/entity/';
    $scope.getListUrl = $scope.path + 'getAllowancelist';
    $scope.saveUrl = $scope.path + 'createAllowance';
    $scope.updateUrl = $scope.path + 'editAllowance';
    $scope.entityAllowance = {
        Id: null,
        CompanyGroupId: null,
        EntityId: null,
        DesignationGroupId: null,
        CurrencyId: null,
        CurrencyName: null,
        Allowance: null,
        EffectiveDate: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.entityAllowanceNew = Object.assign({}, $scope.entityAllowance);

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getEntityWithChange = function () {
        cboService.getCboDesignationGroupByCompanyGroup(null, function (result) {
            $scope.designationGroupList = result;
        });
    }

    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    $scope.searchByAllowanceList = [
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        },
        {
            'name': 'EffectiveDate',
            'value': 'EffectiveDate'
        },
        {
            'name': 'Allowance',
            'value': 'Allowance'
        }
    ];

    $scope.allowanceParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'CONVERT(DATETIME, EffectiveDate, 106)',
        searchBy: 'EffectiveDate',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.entityAllowanceList = [];
    $scope.getAllowanceData = function (pageno) {
        baseService.paginationBase('Organizations/entity/GetEffectiveDateList?entityId=' + $scope.entityAllowanceNew.EntityId + '&designationId=' + $scope.entityAllowanceNew.DesignationGroupId, pageno, $scope.allowanceParameters)
            .then(function (result) {
                $scope.entityAllowanceList = result.Rows;
                $scope.allowanceParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    //*********************** Entity PopUp Start *************************************
    $scope.entitySearchList = [];
    $scope.entityDataList = [];
    $scope.entitySearch = [];
    $scope.entityUrl = 'Organizations/entity/getlist?companyId=';
    $scope.entityParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.entityPopUp = function (companyId) {
        if (baseService.isUndefinedOrNull(companyId)) {
            $scope.entityDataList = [];
            ShowResult('Company selection required.', 'failure');
        }
        else {
            $scope.entityParameters.companyId = companyId;
            $scope.getEntityData = function (pageno) {
                baseService.paginationBase($scope.entityUrl + companyId, pageno, $scope.entityParameters)
                    .then(function (response) {
                        $scope.entityDataList = response.Rows;
                        $scope.entityParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.entitySearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.entityDataList, $scope.entitySearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#entityPopUp')).modal('show');
            $scope.getEntityData();
        }
    };
    $scope.closeEntityPopUp = function () {
        $scope.entityId = '';
        $scope.EntityName = '';
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };

    $scope.selectEntityPopUp = function (entity) {
        $scope.selectedEntityId = entity.Id;
        $scope.entityAllowanceNew.EntityId = $scope.selectedEntityId;
        $scope.entityAllowanceNew.EntityName = entity.UserName;
        $scope.getEntityMapData($scope.selectedEntityId);
        $scope.getEntityWithChange();
        // Nullify current selected position
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };

    $scope.getEntityMapData = function (id) {
        $scope.entityData = [];
        $scope.entitySearch = [];
        $http({
            method: 'GET',
            url: 'Organizations/entity/get?id=' + id
        }).then(function successCallback(response) {
            $scope.entityData = [];
            $scope.entityData.push(response.data);
            baseService.getDDLSearchColumn($scope.entityData, $scope.entitySearch);
        });
    };

    $scope.clearEntity = function () {
        $scope.selectedEntityId = null;
        $scope.entityAllowanceNew.DesignationGroupId = null;
        $scope.entityAllowanceNew.EntityId = null;
        $scope.entityAllowanceNew.EntityName = null;
        $scope.clearPosition();
        $scope.entityData = [];
        $scope.entitySearch = [];
    };
    //*********************** Entity PopUp End *************************************

    //$scope.effectiveDataList = [];
    ///*********************Geting Effective Date************/
    //$scope.getEffectiveDateWithEntity = function () {
    //    $scope.effectiveDataList = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Organizations/entity/GetEffectiveDateList?entityId=' + $scope.entityAllowanceNew.EntityId + '&designationId=' + $scope.entityAllowanceNew.DesignationGroupId
    //    }).then(function successCallback(response) {
    //        $scope.effectiveDataList = response.data;
    //    });
    //};

    //$scope.efectiveAction = "Add";
    //$scope.addEffectiveData = function () {
    //    $scope.entityAllowanceMultiple.CurrencyName = angular.element("#currencyId :selected").text();
    //    if ($scope.efectiveAction === "Add") {
    //        $scope.effectiveDataList.push($scope.entityAllowanceMultiple);
    //    } else {
    //        $scope.effectiveDataList[$scope.effectiveIndex] = $scope.entityAllowanceMultiple;
    //    }
    //    $scope.clearEffective();
    //}
    //$scope.clearEffective = function () {
    //    $scope.entityAllowanceMultiple = {};
    //    $scope.efectiveAction = 'Add';
    //};

    //$scope.getEffectiveData = function (data, index) {
    //    $scope.effectiveIndex = index;
    //    $scope.efectiveAction = 'Update';
    //    $scope.entityAllowanceMultiple.EffectiveDate = data.EffectiveDate;
    //    $scope.entityAllowanceMultiple.Allowance = data.Allowance;
    //    $scope.entityAllowanceMultiple.CurrencyId = data.CurrencyId;
    //};

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.entityAllowance = $scope.entityAllowanceList[$scope.index];
        $scope.entityAllowanceNew = Object.assign({}, $scope.entityAllowance);
        $scope.entityAllowanceNew.EffectiveDate = $filter('dateFiltering')($scope.entityAllowanceNew.EffectiveDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.entityAllowanceNew, $scope.entityAllowance);
        var date = new Date($scope.entityAllowance.EffectiveDate).getDate();
        if (date > 1) {
            return ShowResult('Selected date must be 1st day of month.', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.entityAllowanceForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.entityAllowance,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getAllowanceData();
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.entityAllowance,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getAllowanceData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.entityAllowanceNew.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/entity/deleteAllowance/' + $scope.entityAllowanceNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getAllowanceData();
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    //GL Untagging

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.entityAllowance = { CompanyId: $scope.entityAllowanceNew.CompanyId, EntityId: $scope.entityAllowanceNew.EntityId, EntityName: $scope.entityAllowanceNew.EntityName, DesignationGroupId: $scope.entityAllowanceNew.DesignationGroupId };
        $scope.entityAllowanceNew = { CompanyId: $scope.entityAllowanceNew.CompanyId, EntityId: $scope.entityAllowanceNew.EntityId, EntityName: $scope.entityAllowanceNew.EntityName, DesignationGroupId: $scope.entityAllowanceNew.DesignationGroupId };
        $scope.entityAllowanceNew.Sequence = seq;
        $scope.entityAllowanceNew.Active = true;
        $scope.entityAllowanceSaveList = [];
    }
}