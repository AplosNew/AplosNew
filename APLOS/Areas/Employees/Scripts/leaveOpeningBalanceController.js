'use strict';
LeaveOpeningBalanceController.$inject = ['cboService', 'commonMessage', "$window", '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LeaveOpeningBalanceController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.leaveOpeningBalanceList = [];
    $scope.leaveOpeningBalanceSelectedList = [];
    $scope.path = 'Employees/LeaveOpeningBalance/GetLeaveTypeList';
    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });
    $scope.leaveOpeningBalanceOb = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        YearlyCalendarId: null,
        CalendarName: null,
        PlantId: null,
        EmployeeId: null,
        EmployeeName: null,
        CurrentYearAvailedOpeningBalance: null,
        CurrentYearEarnedDaysOpeningBalance: null,
        CarryForwardOpeningBalance: null,
        LeaveTypeId: null,
        GivenDesignation: null,
        BudgetCode: null,
        EmployeeCode: null,
        Department: null,
        DOJ: null,
        PolicyName: null
    };
    $scope.yearlyCalendarList = [];
    cboService.getCboYearlyCaledar(function (result) {
        $scope.yearlyCalendarList = result;
    });

    $scope.getLeaveOpeningBalanceData = function () {
        $http.get("Employees/LeaveOpeningBalance/GetLeaveTypeList?employeeId=" + $scope.leaveOpeningBalanceOb.EmployeeId + "&calendarId=" + $scope.leaveOpeningBalanceOb.YearlyCalendarId)
            .then(
                function successCallback(response) {
                    $scope.leaveTypeList = [];
                    //$scope.leaveOpeningBalanceSelectedList = response.data.Rows;
                    if (response.data.Rows.length > 0) {
                        angular.forEach(response.data.Rows, function (item) {
                            item.EmployeeId = $scope.leaveOpeningBalanceOb.EmployeeId;
                            item.EmployeeName = $scope.leaveOpeningBalanceOb.EmployeeName;
                            item.YearlyCalendarId = $scope.leaveOpeningBalanceOb.YearlyCalendarId;
                            $scope.leaveTypeList.push(item);
                        });
                    }
                    else {
                        return ShowResult("No leave type found", 'failure', 'popUp');
                    }
                    angular.element(document.querySelector('#employeePopUp')).modal('hide');
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    //

    $scope.employeeList = [];
    $scope.popUp = function (name) {
        try { 
            if (baseService.isUndefinedOrNull($scope.leaveOpeningBalanceOb.YearlyCalendarId)) {
                throw 'Select calendar year !';
            }
            $scope.employeeParameters = {
                limit: 10,
                offset: 0,
                order: 'asc',
                sort: '',
                searchBy: '',
                pageSize: 10,
                total_count: 0,
                search: null,
                serverPagination: true
            };
            $scope.searchEmployeeByList = [
                {
                    name: 'Employee Code',
                    value: 'EmployeeCode'
                },
                {
                    name: 'Employee Name',
                    value: 'EmployeeName'
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                }
            ];

            $scope.popUpUrl = '';
            $scope.employeeParameters.sort = '';
            $scope.employeeParameters.searchBy = '';
            $scope.popUpTitle = 'Employee';
            $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist';
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';

            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', '#employeePopUp');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closeEmployeePopUp = function (data) {
        $scope.leaveOpeningBalanceOb.EmployeeId = data.SystemId;
        $scope.leaveOpeningBalanceOb.EmployeeName = data.EmployeeName;
        $scope.leaveOpeningBalanceOb.EmployeeCode = data.EmployeeCode;
        $scope.leaveOpeningBalanceOb.GivenDesignation = data.GivenDesignation;
        $scope.leaveOpeningBalanceOb.Department = data.Department;
        $scope.leaveOpeningBalanceOb.DOJ = data.DOJ;
        $scope.leaveOpeningBalanceOb.BudgetCode = data.BudgetCode;
        $scope.leaveOpeningBalanceOb.PolicyName = data.PolicyName;
        $scope.getLeaveOpeningBalanceData();
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    function addRow(ob) {
        $scope.leaveOpeningBalanceOb.EmployeeId = ob.SystemId;
        $scope.leaveOpeningBalanceOb.EmployeeName = ob.EmployeeName;
        $scope.leaveOpeningBalanceOb.CurrentYearAvailedOpeningBalance = ob.CurrentYearAvailedOpeningBalance;
        $scope.leaveOpeningBalanceOb.CurrentYearEarnedDaysOpeningBalance = ob.CurrentYearEarnedDaysOpeningBalance;
        $scope.leaveOpeningBalanceOb.LeaveTypeId = ob.LeaveTypeId;
        var obb = Object.assign({}, $scope.leaveOpeningBalanceOb);
        if (checkExisting(obb.EmployeeId) === false) {
            $scope.leaveOpeningBalanceSelectedList.push(obb);
        } else {
            return ShowResult("This employee already added", 'failure', 'employeePopUp');
        }
    }
    function checkExisting(id) {
        for (var i = 0; i < $scope.leaveOpeningBalanceSelectedList.length; i++) {
            var ob = $scope.leaveOpeningBalanceSelectedList[i];
            if (ob.EmployeeId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    $scope.clearProfileUpload = function () {
        $scope.approvalConfigurationNew.ProfileUploadRP = null;
        $scope.approvalConfigurationNew.ProfileUploadRPerson = null;
    };
    //Deleting Rows from LeaveOpeningBalanceList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempLeaveOpeningBalanceOb = data;
        $scope.glMappingIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempLeaveOpeningBalanceOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.EmployeeName + ' ]';
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
    };

    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempLeaveOpeningBalanceOb.Id) === true) {
            $scope.leaveOpeningBalanceSelectedList.splice($scope.glMappingIndex, 1);
        } else {
            $scope.removeFromDb($scope.tempLeaveOpeningBalanceOb.Id, $scope.glMappingIndex);
        }
        $scope.glMappingIndex = -1;
        $scope.$scope.tempLeaveOpeningBalanceOb.Id = null;
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Employees/LeaveOpeningBalance/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.leaveTypeList.splice($scope.glMappingIndex, 1);
                    $scope.glMappingIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    //Save
    function assaignLeave() {
        $scope.leaveTypeSavedList = [];
        angular.forEach($scope.leaveTypeList, function (item) {
            $scope.leaveTypeSavedList.push(item);
        });
    }
    $scope.Save = function () {
        try {
            assaignLeave();
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Employees/LeaveOpeningBalance/create',
                    data: { 'leaveOpeningBalance': $scope.leaveTypeSavedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.clear();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.clear = function () {
        $scope.leaveOpeningBalanceOb = { YearlyCalendarId: $scope.leaveOpeningBalanceOb.YearlyCalendarId };
        $scope.leaveOpeningBalanceOb.DOJ = null;
        $scope.leaveTypeList = [];
    };
}