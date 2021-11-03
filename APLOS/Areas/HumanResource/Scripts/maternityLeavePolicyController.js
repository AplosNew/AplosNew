'use strict';
maternityLeavePolicyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function maternityLeavePolicyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Maternity Leave Policy';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeavePolicys = [];
    $scope.path = 'humanresource/MaternityLeavePolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.maternityLeavePolicy = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        DurationType: null,
        DurationValue: null,
        ChildNo: null,
        IsMonthly: false,
        IsBefore: false,
        IsAfter: false,
        MonthlyPercentage: null,
        BeforePercentage: null,
        AfterPercentage: null
    };
    $scope.maternityLeavePolicyNew = Object.assign({}, $scope.maternityLeavePolicy);
    function Policyvalidation() {
        if ($scope.maternityLeavePolicyNew.IsMonthly)
        {
            if (baseService.isUndefinedOrNull($scope.maternityLeavePolicyNew.MonthlyPercentage)) {
                throw 'Monthly Percentage  is required.';
            }
            if ($scope.maternityLeavePolicyNew.MonthlyPercentage > 100)
            {
                throw ' Monthly Percentage  can not be greater than 100.';
            }
        }
        if ($scope.maternityLeavePolicyNew.IsBefore) {
            if (baseService.isUndefinedOrNull($scope.maternityLeavePolicyNew.BeforePercentage)) {
                throw 'Before Percentage  is required.';
            }
            if ($scope.maternityLeavePolicyNew.BeforePercentage > 100) {
                throw 'Before Percentage  can not be greater than 100.';
            }
        }
        if ($scope.maternityLeavePolicyNew.IsAfter) {
            if (baseService.isUndefinedOrNull($scope.maternityLeavePolicyNew.AfterPercentage)) {
                throw 'After Percentage  is required.';
            }
            if ($scope.maternityLeavePolicyNew.AfterPercentage > 100) {
                throw 'After Percentage  can not be greater than 100.';
            }
        }
        var total = parseInt($scope.maternityLeavePolicyNew.MonthlyPercentage) + parseInt($scope.maternityLeavePolicyNew.BeforePercentage) + parseInt($scope.maternityLeavePolicyNew.AfterPercentage);

        if (total>100)
        {
            throw 'Total Percentage  can not be greater than 100.';
            
        }
    }
    
    $scope.companyList = [];
    $scope.plantList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.maternityLeavePolicyNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.searchByList = [
        {
            'name': 'Child No',
            'value': 'ChildNumber'
        },
        {
            'name': 'Duration Type',
            'value': 'DurationType'
        }
    ];

    $scope.getListData = function () {
        baseService.init("humanresource/maternityleavepolicy/getList?plantId=" + $scope.maternityLeavePolicyNew.PlantId, null, null, null, "ChildNumber", "ChildNumber");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.maternityLeavePolicys = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.maternityLeavePolicy = $scope.maternityLeavePolicys[$scope.index];
        $scope.maternityLeavePolicy.ChildNo = $scope.maternityLeavePolicy.ChildNo.toString();
        $scope.maternityLeavePolicyNew = Object.assign({}, $scope.maternityLeavePolicy);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    function validation() {
        try {
            if (baseService.isUndefinedOrNull($scope.maternityLeavePolicyNew.DurationValue)) {
                throw 'Duration value is required.';
            }
            var text = angular.element("#DurationType :selected").text();
            if (text === 'Day') {
                if ($scope.maternityLeavePolicyNew.DurationValue > 365) {
                    throw 'Day can not be greater than 365.';
                }
            }
            else if (text === 'Week') {
                if ($scope.maternityLeavePolicyNew.DurationValue > 54) {
                    throw 'Week can not be greater than 52.';
                }
            } else {
                if ($scope.maternityLeavePolicyNew.DurationValue > 12) {
                    throw 'Month can not be greater than 12.';
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            angular.copy($scope.maternityLeavePolicyNew, $scope.maternityLeavePolicy);
            $scope.$broadcast('show-errors-check-validity');
            validation();
            Policyvalidation();
            if ($scope.maternityLeavePolicyNewForm.$valid && $scope.maternityLeavePolicyNewForm1.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.maternityLeavePolicy,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.maternityLeavePolicys.push(response.data.MaternityLeavePolicy);
                            baseService.paginationAdd();
                            ClearFields();
                            $scope.getListData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.maternityLeavePolicy,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.maternityLeavePolicys[$scope.index] = $scope.maternityLeavePolicy;
                            }
                            ClearFields();
                            $scope.getListData();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.maternityLeavePolicyNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.maternityLeavePolicyNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.maternityLeavePolicys.splice($scope.index, 1);
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
        $scope.maternityLeavePolicy = { CompanyId: $scope.maternityLeavePolicyNew.CompanyId, PlantId: $scope.maternityLeavePolicyNew.PlantId };
        $scope.maternityLeavePolicyNew = { CompanyId: $scope.maternityLeavePolicyNew.CompanyId, PlantId: $scope.maternityLeavePolicyNew.PlantId };
    }
}