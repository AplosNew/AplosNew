'use strict';
EmployeeMobileAppsAuthorizationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeMobileAppsAuthorizationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Employee Mobile Apps Authorization";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.empMobileAuths = [];
    $scope.path = 'Securities/employeemobileappsauthorization/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveDisabled = false;

    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });
    cboService.getCboUnitByCompanyGroup(null, function (result) {
        $scope.unitList = result;
    });
    cboService.getCboDivisionByCompanyGroup(null, function (result) {
        $scope.divisionIdList = result;
    });
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });
    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.sectionList = result;
    });
    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.subSectionList = result;
    });
    cboService.getCboDesignationGroupByCompanyGroup(null, function (result) {
        $scope.designationGroupList = result;
    });
    cboService.getCboDesignationByCompanyGroup(null, function (result) {
        $scope.designationList = result;
    });
    $scope.empMobileAuthSearchParameters = {
        Id: null,
        PlantId: null,
        EmployeeId: null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        DesignationGroupId: null,
        DesignationId: null
    };
    $scope.empMobileAuth = {
        Id: null,
        EmployeeId: null,
        EmployeeCode: null,
        EmployeeName: null,
        IsSalaryStructure: false,
        IsPaySlip: false,
        IsMonthlyAttendance: false,
        IsDailyAttendanceNotification: false,
        IsSalaryProcessConfirmationNotification: false,
        IsSalaryDisbursementNotification: false,
        IsIncrementNotification: false,
        IsPromotionNotification: false,
        IsLeaveNotification: false,
        PIN: null,
        DesignationName: null,
        DepartmentName: null
    };
    $scope.getData = function (flag) {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            $http({
                method: 'GET',
                url: $scope.getListUrl,
                params: {
                    'plantId': $scope.empMobileAuthSearchParameters.PlantId
                    , 'unitId': $scope.empMobileAuthSearchParameters.UnitId
                    , 'divisionId': $scope.empMobileAuthSearchParameters.DivisionId
                    , 'departmentId': $scope.empMobileAuthSearchParameters.DepartmentId
                    , 'sectionId': $scope.empMobileAuthSearchParameters.SectionId
                    , 'subSectionId': $scope.empMobileAuthSearchParameters.SubSectionId
                    , 'designationGroupId': $scope.empMobileAuthSearchParameters.DesignationGroupId
                    , 'designationId': $scope.empMobileAuthSearchParameters.DesignationId
                    , 'employeeId': JSON.stringify($scope.empIdList)
                    , 'flag': flag
                }
            }).then(function successCallback(response) {
                $scope.empMobileAuths = [];
                $scope.empMobileAuths = response.data;
            });
        }
    };
    $scope.setNewPin = function (index) {
        $scope.empMobileAuths[index].PIN = Math.floor(Math.random() * 900000) + 100000;
    };

    // #region checkAll

    $scope.CheckAll = function (event) {
        //console.log(event);
        var _isselected = event.target.checked;
        var _name = event.target.name;
        for (var i = 0; i < baseService.arrayLength($scope.empMobileAuths); i++) {
            $scope.empMobileAuths[i][_name] = _isselected;
        }
    };
    $scope.UnCheck = function (event) {
        var _isselected = event.target.checked;
        var _name = event.target.name;

        $scope[_name] = allTrue(_name);
    };
    function allTrue(name) {
        var flag = false;
        for (var i = 0; i < baseService.arrayLength($scope.empMobileAuths); i++) {
            if ($scope.empMobileAuths[i][name]) {
                flag = true;
            }
            else {
                flag = false;
                break;
            }
        }
        return flag;
    }
    // #endregion

    // #region employee
    $rootScope.tempList = [];
    $scope.getEmployeeListUrl = 'employees/EmployeeInformation/GetPlantEmployeeList';
    $scope.employeeList = [];
    $scope.searchEmployeeList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        }
    ];

    $scope.ShowEmployeeListPopUp = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            $rootScope.tempList = [];
            angular.forEach($scope.empIdList, function (a) {
                $rootScope.tempList.push(a);
            });
            baseService.setCurrentPage('employeeList');
            baseService.init($scope.getEmployeeListUrl, null, null, null, 'EmployeeCode, FirstName, MiddleName, LastName ', 'LastName');
            $rootScope.parameters.plantId = $scope.empMobileAuthSearchParameters.PlantId;
            $rootScope.parameters.employeeIds = baseService.getColumnValueList($scope.empMobileAuths, 'EmployeeId');
            $scope.getEmployeeData = function (pageno) {
                baseService.pagination(pageno)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        for (var t = 0; t < baseService.arrayLength($scope.employeeList); t++) {
                            $scope.employeeList[t].Flag = $rootScope.tempList.includes($scope.employeeList[t].EmployeeCode);
                        }
                        angular.element(document.querySelector('#employeePopUp')).modal('show');
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.getEmployeeData();
        }
    };
    $scope.ClearList = function () {
        $scope.empIdList = [];
    };

    $scope.pushTempList = function (data, event, list) {
        if (event.currentTarget.checked)
            $rootScope.tempList.push(data);
        else {
            $rootScope.tempList.splice($rootScope.tempList.indexOf(data), 1);
            list.splice(list.indexOf(data), 1);
        }
    };
    $scope.empIdList = [];

    $scope.SelectEmployeeByButton1 = function () {
        $scope.empIdList = [];
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!$scope.empIdList.includes(a))
                    $scope.empIdList.push(a);
            });
        }
        else $scope.empIdList = [];
        angular.forEach($scope.empIdList, function (a) {
            if (!$rootScope.tempList.includes(a))
                $scope.empIdList.splice($scope.empIdList.indexOf(a), 1);
        });
        angular.element(document.querySelector('#employeePopUp1')).modal('hide');
    };

    $scope.SelectEmployeeByButton = function () {
        $scope.empIdList = [];
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!$scope.empIdList.includes(a))
                    $scope.empIdList.push(a);
            });
        }
        else $scope.empIdList = [];
        angular.forEach($scope.empIdList, function (a) {
            if (!$rootScope.tempList.includes(a))
                $scope.empIdList.splice($scope.empIdList.indexOf(a), 1);
        });
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };


    $scope.CloseEmployeePopUp = function () {
        $scope.employeeId = '';
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    $scope.CloseEmployeePopUp = function () {
        $scope.employeeId = '';
        angular.element(document.querySelector('#employeePopUp1')).modal('hide');
    };
    function isRowSelected(ilst) {
        var flag = false;
        for (var i = 0; i < ilst.length; i++) {
            if (ilst[i].Flag) {
                return flag = true;
            }
        }
    }

    // #endregion

    // #region CRUD
    $scope.Save = function () {
        try {
            if (baseService.arrayLength($scope.empMobileAuths) === 0) throw 'Please select employee';
            for (var t = 0; t < baseService.arrayLength($scope.empMobileAuths); t++) {
                if (baseService.isUndefinedOrNull($scope.empMobileAuths[t].PIN))
                    $scope.empMobileAuths[t].PIN = 0;
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.empMobileAuths,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.saveDisabled = false;
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.saveDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
                $scope.saveDisabled = false;
            });
        } catch (e) {
            ShowResult(e, 'failure');
            $scope.saveDisabled = false;
        }
    };
    // #endregion

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.empMobileAuthSearchParameters = {};
        $scope.empMobileAuth = {};
        $rootScope.tempList = [];
        $scope.empMobileAuths = [];
        $scope.empIdList = [];
    }

    $scope.rowRemoveModal = function (id, index) {
        $scope.message = '';
        $scope.id = id;
        $scope.index = index;
        $scope.message = 'Are you sure want to remove this data....';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    return ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        $scope.empMobileAuths.splice($scope.index, 1);
        $scope.index = -1;
    };

    $scope.plantChange = function () {
        $scope.empMobileAuthSearchParameters = { PlantId: $scope.empMobileAuthSearchParameters.PlantId };
        $scope.empMobileAuth = {};
        $rootScope.tempList = [];
        $scope.empMobileAuths = [];
        $scope.empIdList = [];
    };
}