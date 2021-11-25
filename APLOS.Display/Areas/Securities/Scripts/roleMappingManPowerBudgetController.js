'use strict';
RoleMappingManPowerBudgetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function RoleMappingManPowerBudgetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.addRole = false;
    $scope.roleMappingList = [];
    $scope.roleMappingSearch = function () {
        $http({
            method: 'GET',
            url: 'Securities/rolemapping/getlistbymanpowerbudget?manPowerBudgetId=' + $scope.roleMapping.ManPowerBudgetMasterId
        }).then(function successCallback(response) {
            $scope.addRole = true;
            $scope.roleMappingList = response.data;
            if ($scope.roleMappingList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };

    $scope.roleMapping = {
        Id: null,
        CompanyId: null,
        RoleId: null,
        PositionStructureId: null,
        ManPowerBudgetMasterId: null,
        Active: true
    };

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.positionList = [];
    $scope.onCompanyChange = function () {
        $http({
            method: 'GET',
            url: 'Organizations/manpowerbudget/getcbolist?companyId=' + $scope.roleMapping.CompanyId
        }).then(function successCallback(response) {
            $scope.positionList = response.data;
        });
    };

    $scope.getPosition = function () {
        $scope.positions = [];
        $http({
            method: 'GET',
            url: 'Organizations/ManpowerBudget/getmanpowerbudget?manPowerBudgetMasterId=' + $scope.roleMapping.ManPowerBudgetMasterId
        }).then(function successCallback(response) {
            if (baseService.arrayLength($scope.positions) == 0) {
                var localValue = [];
                localValue = response.data;
                baseService.getDDLSearchColumn(localValue, $scope.positions);
                $scope.positionValue = localValue;
            }
        });
    };
    $scope.popUpList = [];
    $scope.popUpParameters = {
        limit: 20,
        offset: 0,
        order: 'asc',
        sort: 'RoleName',
        searchBy: "RoleName",
        pageSize: 20,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        if (baseService.isUndefinedOrNull($scope.roleMapping.CompanyId) || baseService.isUndefinedOrNull($scope.roleMapping.ManPowerBudgetMasterId))
            return ShowResult('Please select all drop down', 'failure');

        $scope.popUpUrl = 'Securities/rolemapping/getrolelistbymanpowerbudget?roleId=' + isRoleIdExistInRoleMapping($scope.roleMappingList);
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    if (result.Rows.length > 0) {
                        $scope.popUpDataList = result.Rows;
                        $scope.popUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.popUpList) == 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                            //$scope.popUpList = localValue;
                        }
                        $scope.roleData = true;
                        $scope.roleDataNotFound = false;
                    }
                    else {
                        $scope.roleData = false;
                        $scope.roleDataNotFound = true;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    function isRoleIdExistInRoleMapping(list) {
        $scope.roleIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] == false) {
                    $scope.roleIds.push(list[i]['RoleId']);
                }
            }
        }
        return JSON.stringify($scope.roleIds);
    }
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    $scope.addInGrid = function () {
        if (!isRowSelected($scope.popUpDataList)) {
            ShowResult('Please select at least one row', 'failure', 'popUpId');
            return;
        }
        angular.forEach($scope.popUpDataList, function (a) {
            if (a.Flag) {
                $scope.roleMappingList.push({
                    Id: null,
                    ManPowerBudgetMasterId: $scope.roleMapping.ManPowerBudgetMasterId,
                    RoleId: a.Id,
                    RoleName: a.RoleName,
                    Remarks: a.Remarks,
                    Archive: false
                });
            }
        });
        if (!$scope.tableShow)
            $scope.tableShow = true;
        $scope.closePopUp();
    };
    function isRowSelected(ilst) {
        try {
            var flag = false;
            for (var i = 0; i < ilst.length; i++) {
                if (ilst[i].Flag) {
                    return flag = true;
                }
            }
        } catch (e) {
        }
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.roleMapping.$valid) {
            $http({
                method: 'POST',
                url: 'Securities/rolemapping/createmanpowerbudget',
                data: { 'roleMappingPositionStructure': $scope.roleMappingList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.roleMappingSearch();
                }
            });
        }
    };

    $scope.valuePassInDelModal = function (id, roleId, name, index) {
        $scope.id = id;
        $scope.index = index;
        $scope.roleId = roleId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + name + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.roleMappingList.length; i++) {
            if ($scope.roleMappingList[i].Id == null && $scope.roleMappingList[i].RoleId == $scope.roleId) {
                $scope.roleMappingList.splice(i, 1);
            }
            else if ($scope.roleMappingList[i].Id != null && $scope.roleMappingList[i].RoleId == $scope.roleId)
                $scope.roleMappingList[i].Archive = true;
        }
        if ($scope.roleMappingList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
        $scope.id = null;
        $scope.index = -1;
        $scope.positionStructureId = null;
    };
}