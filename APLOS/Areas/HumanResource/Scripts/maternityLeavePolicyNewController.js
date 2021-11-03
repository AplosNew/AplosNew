'use strict';
maternityLeavePolicyNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function maternityLeavePolicyNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Maternity Leave Policy New';
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
        ChildNo: "",
        IsMonthly: false,
        IsDistributed: true,
        IsBefore: false,
        IsAfter: false,
        BeforePercentage: null,
        AfterPercentage: null,
        MaternityStartDay: 0,
        MaternityEndDay: 0,
        MaternityLeaveStartDay: 0,
        MaternityLeaveEndDay: 0,
        CanAvailAfterDOJ: null,
        IsNoBenefit: false
    };

    $scope.maternityLeavePolicyNew = Object.assign({}, $scope.maternityLeavePolicy);

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


    $scope.MaternityLeave = [];
    $scope.getListData = function () {
        $scope.MaternityLeave = [];
        $http.get('humanresource/maternityleavepolicy/getList?plantId=' + $scope.maternityLeavePolicyNew.PlantId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.MaternityLeave = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.recorddoubleclick = function (obj) {
        //var data = {};
        //var gridObj = $("#Grid").data("ejGrid");
        //data = gridObj.getSelectedRecords()[0];
        $scope.maternityLeavePolicy = angular.copy(obj.data);
        $scope.maternityLeavePolicyNew = $scope.maternityLeavePolicy;
        $scope.maternityLeavePolicyNew.ChildNo = $scope.maternityLeavePolicyNew.ChildNo.toString();

        if ($scope.maternityLeavePolicyNew.IsMonthly) {
            $scope.maternityLeavePolicyNew.IsMonthly = true;
            $scope.salaryShow = false;
            $scope.maternityLeavePolicyNew.IsDistributed = false;
        }
        else {
            $scope.maternityLeavePolicyNew.IsDistributed = true;
            $scope.salaryShow = true;
        }
        if ($scope.maternityLeavePolicyNew.IsNoBenefit) {
            $scope.IsNoBenefit = true;
            $scope.maternityLeavePolicyNew.IsDistributed = false;
            $scope.maternityLeavePolicyNew.IsMonthly = false;

        } else {
            $scope.IsNoBenefit = false;
        }
        try {
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {

        }

    };


    function validation() {
        try {

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

            //if ($scope.maternityLeavePolicyNew.MaternityLeaveStartDay >= $scope.maternityLeavePolicyNew.MaternityStartDay) {
            //    throw 'Leave start day must be less then followUp start day.';
            //}
            //if ($scope.maternityLeavePolicyNew.MaternityLeaveEndDay >= $scope.maternityLeavePolicyNew.MaternityEndDay) {
            //    throw 'Leave End day must be less then followUp End day.';
            //}

            if ($scope.maternityLeavePolicyNew.IsMonthly === false) {
                if ($scope.maternityLeavePolicyNew.IsNoBenefit === false) {
                    if (!baseService.isUndefinedOrNull($scope.maternityLeavePolicyNew.BeforePercentage) && !baseService.isUndefinedOrNull($scope.maternityLeavePolicyNew.AfterPercentage)) {
                        var total = parseInt($scope.maternityLeavePolicyNew.BeforePercentage) + parseInt($scope.maternityLeavePolicyNew.AfterPercentage);

                        if (total > 100) {
                            throw 'Total Percentage  can not be greater than 100.';

                        }
                        if (total < 100) {
                            throw 'Total Percentage  can not be less than 100.';
                        }
                    }
                }
            }

        } catch (e) {
            throw e;
        }
    }

    $scope.setVisible = function () {
        if ($scope.maternityLeavePolicyNew.IsNoBenefit) {
            $scope.IsNoBenefit = true;

            $scope.maternityLeavePolicyNew.IsAfter = false;
            $scope.maternityLeavePolicyNew.IsBefore = false;
            $scope.maternityLeavePolicyNew.BeforePercentage = 0;
            $scope.maternityLeavePolicyNew.AfterPercentage = 0;
            $scope.maternityLeavePolicyNew.IsDistributed = false;
            $scope.maternityLeavePolicyNew.IsMonthly = false;

        } else {
            $scope.IsNoBenefit = false;
            $scope.ChangeToDistributed();
        }

    }

    $scope.IsNoBenefit = false;
    $scope.Save = function () {
        try {

            validation();
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.maternityLeavePolicyNewForm.$valid) {
                $scope.maternityLeavePolicy = {};
                angular.copy($scope.maternityLeavePolicyNew, $scope.maternityLeavePolicy);
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
                            //$scope.IsNoBenefit = false;
                            $scope.ChangeToDistributed();
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
                    $scope.getListData();
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

        var k = Object.assign({}, $scope.maternityLeavePolicyNew);
        $scope.maternityLeavePolicyNew = Object.assign({}, $scope.maternityLeavePolicy);

        $scope.maternityLeavePolicyNew = { CompanyId: k.CompanyId, PlantId: k.PlantId };

        $scope.ChangeToDistributed();
    }

    $scope.salaryShow = true;
    $scope.ChangeToRegular = function () {
        $scope.maternityLeavePolicyNew.IsMonthly = true;
        $scope.maternityLeavePolicyNew.IsBefore = false;
        $scope.maternityLeavePolicyNew.IsAfter = false;
        $scope.maternityLeavePolicyNew.BeforePercentage = 0;
        $scope.maternityLeavePolicyNew.AfterPercentage = 0;
        $scope.salaryShow = false;

    };

    $scope.ChangeToDistributed = function () {
        $scope.maternityLeavePolicyNew.IsDistributed = true;
        $scope.maternityLeavePolicyNew.IsMonthly = false;
        $scope.salaryShow = true;
    }
    $scope.ChangeToDistributed();
}