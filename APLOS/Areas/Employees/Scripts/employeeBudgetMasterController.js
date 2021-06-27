"use strict";
employeeBudgetMasterController.$inject = ["commonMessage", "$rootScope", "$scope", "baseService", "$routeParams", "$http", "$filter", "$compile", "cboService", "$window", "$controller"];
function employeeBudgetMasterController(commonMessage, $rootScope, $scope, baseService, $routeParams, $http, $filter, $compile, cboService, $window, $controller) {
    $rootScope.title = "Budget Master";
    $scope.employeeUrl = "Accounts/BudgetMaster/GetAllEmployee";
    $scope.budgetUrl = "employees/EmployeeInformation/QueryBudgetMasterResponsiblePerson";
    $scope.positionAllowanceList = [];
    $scope.positionAllowance = {
        Id: null,
        PlantId: null,
        EmployeeId: null,
        EmployeeName: null,
        Active: true
    };

    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });

    if (!baseService.isUndefinedOrNull($routeParams.id)) {
        $scope.positionAllowance.EmployeeId = $routeParams.id;
        $scope.positionAllowance.EmployeeName = $routeParams.name;
        onChange();
        $routeParams.id = null;
        $routeParams.name = null;
    }

    $scope.clearEmployee = function () {
        $scope.positionAllowance = {};
        $scope.positionAllowance.Active = true;
        $scope.positionAllowanceList = [];
    };

    cboService.getEnumCbo("enum/GetCboResponsiblePersonMappingLevel", function (result) {
        $scope.mappingLevelList = result;
    });

    $scope.employeeList = [];
    $scope.employeeIndex = -1;
    $scope.selectedEmployee = null;
    $scope.searchEmployeeByList = [
        {
            "name": "Employee Code",
            "value": "EmployeeCode"
        },
        {
            "name": "First Name",
            "value": "FirstName"
        },
        {
            "name": "MiddleName",
            "value": "MiddleName"
        },
        {
            "name": "LastName",
            "value": "LastName"
        },
        {
            "name": "Employee Name",
            "value": "EmployeeName"
        }
    ];

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "EmployeeCode, FirstName, MiddleName, LastName ",
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showEmployeeListPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.positionAllowance.PlantId)) {
            ShowResult("Please select plant.", "failure");
            return;
        }
        $scope.getEmployeeData = function (pageno) {
            $scope.employeeParameters.PlantId = $scope.positionAllowance.PlantId;
            baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#employeePopUp")).modal("show");
        $scope.getEmployeeData();
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.positionAllowance.EmployeeName = employee.EmployeeCode + " - " + employee.EmployeeName;
            $scope.positionAllowance.EmployeeId = employee.SystemId;
            onChange();
        }
        $scope.hidePartyPopUp();
    };

    $scope.searchByList = [
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "Budget Category",
            "value": "BudgetCategory"
        },
        {
            "name": "Budget SubCategory",
            "value": "BudgetSubCategory"
        },
        {
            "name": "Budget",
            "value": "BudgetItem"
        },
        {
            "name": "Budget Group",
            "value": "BudgetGroup"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        }
    ];

    $scope.budgetParameters = {
        limit: 200,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode, GLGeneralInfoName, BudgetItem",
        searchBy: "BudgetItem",
        pageSize: 200,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function (pageno) {
        $scope.budgetParameters.EmployeeId = $scope.positionAllowance.EmployeeId;
        baseService.paginationBase($scope.budgetUrl, pageno, $scope.budgetParameters)
            .then(function (result) {
                $scope.positionAllowanceList = result.Rows;
                $scope.budgetParameters.total_count = result.Total;
                $scope.setBudgetMasterPopUpData();
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    function onChange() {
        $scope.getData();
        //cboService.getEnumCbo("employees/EmployeeInformation/QueryBudgetMasterResponsiblePerson?employeeId=" + $scope.positionAllowance.EmployeeId, function (result) {
        //    $scope.positionAllowanceList = result.Rows;
        //});
        $scope.getBudgetActivityData();
    }

    $scope.budgetMasterActivityList = [];
    $scope.getBudgetActivityData = function () {
        cboService.getEnumCbo("employees/EmployeeInformation/BudgetMasterActivityResponsiblePerson?employeeId=" + $scope.positionAllowance.EmployeeId, function (result) {
            $scope.budgetMasterActivityList = result;
        });
    };

    $scope.activityList = [];
    $scope.showActivityPopUp = function () {
        cboService.getEnumCbo("employees/EmployeeInformation/BudgetMasterActivityResponsiblePersonPopUp?budgetMasterId=" + $scope.budgetMasterId + "&&employeeId=" + $scope.positionAllowance.EmployeeId, function (result) {
            $scope.activityList = result.Rows;
        });
        angular.element(document.querySelector("#budgetActivityAddPopUp")).modal("show");
    };

    $scope.selectActivity = function () {
        angular.forEach($scope.activityList, function (item, i) {
            if (item.Active) {
                item.EmployeeId = $scope.positionAllowance.EmployeeId;
                $scope.budgetMasterActivityList.push(item);
            }
        });
        $scope.budgetActivityList = $filter("filter")($scope.budgetMasterActivityList, { BudgetMasterId: $scope.budgetMasterId });
        angular.element(document.querySelector("#budgetActivityAddPopUp")).modal("hide");
    };

    $scope.budgetMasterParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "GLGeneralInfoName",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showBudgetMasterPopUpList = function () {
        $scope.budgetMasterTempList = [];
        if (baseService.isUndefinedOrNull($scope.positionAllowance.EmployeeId)) {
            ShowResult("Please select employee.", "failure");
            return;
        }
        angular.element(document.querySelector("#budgetMasterPopUp")).modal("show");
        $scope.getBudgetMasterData();
    };

    $scope.getBudgetMasterData = function (pageno) {
        var url = "employees/EmployeeInformation/QueryBudgetMasterPopUp";
        $scope.budgetMasterParameters.EmployeeId = $scope.positionAllowance.EmployeeId;
        baseService.paginationBase(url, pageno, $scope.budgetMasterParameters)
            .then(function (result) {
                $scope.budgetMasterList = result.Rows;
                $scope.budgetMasterParameters.total_count = result.Total;
                angular.forEach($scope.budgetMasterList, function (item, i) {
                    var temp = $filter("filter")($scope.budgetMasterTempList, { "BudgetMasterId": item.BudgetMasterId });
                    if (null !== temp && temp.length > 0) {
                        if (temp[0].BudgetMasterId === item.BudgetMasterId) {
                            item.Active = true;
                        }
                    }
                });
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.closeBudgetMasterPopUp = function () {
        $scope.setBudgetMasterPopUpData();
        angular.element(document.querySelector("#budgetMasterPopUp")).modal("hide");
    };

    $scope.setBudgetMasterPopUpData = function () {
        if ($scope.budgetMasterTempList.length > 0) {
            angular.forEach($scope.budgetMasterTempList, function (item, i) {
                if (item.Active) {
                    item.EmployeeId = $scope.positionAllowance.EmployeeId;
                    item.MappingLevel = "Budget";
                    $scope.positionAllowanceList.push(item);
                }
            });
            $scope.budgetParameters.total_count += $scope.budgetMasterTempList.length;
        }
    };

    $scope.validation = function () {
        if ($scope.positionAllowanceList.length === 0) {
            ShowResult("Please add Budget Master !", "failure");
            return true;
        }
        for (var i = 0; i < $scope.positionAllowanceList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.positionAllowanceList[i].MappingLevel)) {
                ShowResult(" Please select Level of " + $scope.positionAllowanceList[i].BudgetItem, "failure");
                return true;
            }
        }
        return false;
    };

    $scope.saveBudgetMaster = function () {
        if (!$scope.validation()) {
            $http({
                method: "POST",
                url: "employees/EmployeeInformation/SaveBudgetMaster",
                data: {
                    "entityList": $scope.positionAllowanceList,
                    "activityList": $scope.budgetMasterActivityList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetMasterTempList = [];
                }
            });
        }
        return true;
    };

   
    $scope.Save = function () {
        if (!$scope.validation()) {
            $http({
                method: "POST",
                url: "employees/EmployeeInformation/SaveBudgetMaster",
                data: {
                    "entityList": JSON.stringify($scope.positionAllowanceList) ,
                    "activityList": JSON.stringify($scope.budgetMasterActivityList) ,
                },
                dataType: 'JSON'                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetMasterTempList = [];
                }
            });
        }
        return true;
    };

    $scope.removeRow = function (index) {
        $scope.activityIndex = index;
        $scope.activityConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#activityDeletePopUp")).modal("show");
    };

    $scope.confirmActivityRemoveRow = function () {
        $scope.positionAllowanceList.splice($scope.activityIndex, 1);
        $scope.activityIndex = null;
    };

    $scope.removeAtivityAddRow = function (index) {
        $scope.activityAddIndex = index;
        $scope.activityConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#activityDeleteAddPopUp")).modal("show");
    };

    $scope.confirmActivityAddRemoveRow = function () {
        $scope.budgetMasterActivityList.splice($scope.activityAddIndex, 1);
        $scope.budgetActivityList = $filter("filter")($scope.budgetMasterActivityList, { BudgetMasterId: $scope.budgetMasterId });
        $scope.activityAddIndex = null;
    };

    $scope.budgetActivityList = [];
    $scope.activityPopUpShow = function (data, index) {
        $scope.faIndex = index;
        $scope.budgetMasterId = data.BudgetMasterId;
        for (var i = 0; i < $scope.budgetMasterActivityList.length; i++) {
            if ($scope.budgetMasterActivityList[i].BudgetMasterId == $scope.budgetMasterId) {
                $scope.budgetActivityList.push($scope.budgetMasterActivityList[i]);
            }
        }
        //$scope.budgetActivityList = $filter("filter")($scope.budgetMasterActivityList, { BudgetMasterId: data.BudgetMasterId });
        angular.element(document.querySelector("#budgetActivityPopUp")).modal("show");
    };

    $scope.activityPopUpClose = function () {
        $scope.faIndex = -1;
        $scope.budgetActivityList = [];
        angular.element(document.querySelector("#budgetActivityPopUp")).modal("hide");
    };

    $scope.budgetMasterTempList = [];
    $scope.storeSelecteBudget = function (data) {
        $scope.budgetMasterTempList.push(data);
    };
}