'use strict';
costCenterController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function costCenterController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Cost Center";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costCenters = [];
    $scope.path = 'Organizations/costCenter/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    $scope.searchByCostCenterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Category',
            'value': 'CostCenterCategoryName'
        },
        {
            'name': 'Sub Category',
            'value': 'CostCenterSubCategoryName'
        }
    ];
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.costCenters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.costCenter = {
        Id: null,
        CostCenterCategoryId: null,
        CostCenterSubCategoryId: null,
        Sequence: null,
        Code: null,
        UserName: null,
        StandardName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null,
        CostType: null,
        EmployeeId: null,
        EmployeeName:null,
        LineId: null,
        UnitId: null,
        DepartmentId:null
    };
    $scope.costCenterNew = Object.assign({}, $scope.costCenter);
    /****CBO***************/
    $scope.costCenterCategoryCboList = [];
    cboService.getCboCostCenterCategory(function (result) {
        $scope.costCenterCategoryCboList = result;
    });
    $scope.costCenterSubCategoryCboList = [];
    cboService.getCboCostCenterSubCategory(function (result) {
        $scope.costCenterSubCategoryCboList = result;
    });

    $scope.costCenterTypeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetCostCenterTypeCbo/'
    }).then(function successCallback(response) {
        $scope.costCenterTypeList = response.data;
        });

    //----
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.costCenterNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.costCenter = $scope.costCenters[$scope.index];
        $scope.costCenterNew = Object.assign({}, $scope.costCenter);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.costCenterNew.EmployeeName = employee.EmployeeName;
            $scope.costCenterNew.EmployeeId = employee.SystemId;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    cboService.getCboUnitByCompanyGroup(null, function (result) {
        $scope.unitList = result;
    });
    
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });

    //cboService.getCboEntityLineById(entityId, function (result) {
    //    $scope.lineList = result;
    //});

    $scope.Save = function () {
        angular.copy($scope.costCenterNew, $scope.costCenter);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.costCenterForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.costCenter,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.costCenters = $filter('orderBy')($scope.costCenters, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.costCenter,
                    dataType: 'JSO'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.costCenters[$scope.index] = $scope.costCenter;
                            $scope.costCenters = $filter('orderBy')($scope.costCenters, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.costCenterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.costCenterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.costCenters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.costCenter = {};
        $scope.costCenterNew = {};
        $scope.costCenterNew.Sequence = seq;
        $scope.costCenterNew.Active = true;
    }
}