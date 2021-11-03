'use strict';
entityCostCenterController.$inject = ['cboService', 'commonMessage', '$window', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function entityCostCenterController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.entityCostCenter = {
        Id: null,
        CompanyId: null,
        EntityId: null,
        CostCenterId: null,
        AddedDate: new Date(),
        UpdatedBy: null,
        UpdatedDate: new Date()
    };
    $scope.entityCostCenterNew = Object.assign({}, $scope.entityCostCenter);

    /**********CBO*************/
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $scope.entityList = [];
    $scope.getEntityList = function () {
        cboService.getCboEntityByCompanyWise(null, $scope.entityCostCenterNew.CompanyId, function (result) {
            $scope.entityList = result;
        });
    };

    $scope.costCenterList = [];
    $scope.getCostCenterList = function () {
        $http.get('Organizations/entitycostcenter/GetListWithCostCenter?entityId=' + $scope.entityCostCenterNew.EntityId + '&companyId=' + $scope.entityCostCenterNew.CompanyId)
            .then(function (result) {
                $scope.costCenterList = result.data.Rows;
                for (var i = 0; i < $scope.costCenterList.length; i++) {
                    if ($scope.costCenterList[i].Id !== null) {
                        $scope.costCenterList[i].Flag = true;
                    } else {
                        $scope.costCenterList[i].Flag = false;
                    }
                }
            });
    };

    /************/
    //Save
    function entityCostCenterSaved(list) {
        $scope.entityCostCenterSavedList = [];
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].Flag) {
                    list[i].EntityId = $scope.entityCostCenterNew.EntityId;
                    list[i].CompanyId = $scope.entityCostCenterNew.CompanyId;
                    $scope.entityCostCenterSavedList.push(list[i]);
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.Save = function () {
        try {
            if ($scope.entityCostCenterNew.CompanyId === null) {
                throw 'Company required!!';
            }
            if ($scope.entityCostCenterNew.EntityId === null) {
                throw 'Entity required!!';
            }
            if ($scope.costCenterList.length == 0) {
                throw "There is no cost center data against this entity..!";
            }
            entityCostCenterSaved($scope.costCenterList);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.entityCostCenterSavedList.length == 0) {
                throw "Select Cost Center..!";
            }

            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/entityCostCenter/create',
                    data: { 'entityCostCenter': $scope.entityCostCenterSavedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getCostCenterList();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}