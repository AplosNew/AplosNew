'use strict';
holidayAbsentismAssignmentController.$inject = ['cboService', 'commonMessage', "$window", '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function holidayAbsentismAssignmentController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Holiday Absentism Assignment';
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.holidayAbsentismAssignmentList = [];
    $scope.holidayAbsentismAssignmentSelectedList = [];
    $scope.path = 'employees/holidayabsentismassignment/getlist';
    $scope.getListUrl = 'employees/holidayabsentismassignment/getassignedlist';
    baseService.init($scope.getListUrl, null, null, null, 'EmployeeCode', 'EmployeeCode');
    $scope.getData = function (pageno) {
        $rootScope.parameters.workDate = $scope.absentisim.Holiday;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.assignedList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $rootScope.searchByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Work Date',
            'value': 'WorkDate'
        }
    ];

    $scope.holidayAbsentismAssignmentOb = {
        Id: null,
        CalendarWeekDayId: null,
        CompanyGroupId: $window.companyGroupId,
        PlantId: $window.plantId,
        FromDate: null,
        ToDate: null
    };

    $scope.absentisim = {
        YearId: null,
        Month: null,
        Day: 2,
        Holiday: null
    };


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.holidayList = [];
    $scope.LoadHoliday = function () {
        if (!baseService.isUndefinedOrNull($scope.absentisim.YearId) && !baseService.isUndefinedOrNull($scope.absentisim.Month)) {
                cboService.getHolidayCbo($scope.absentisim.YearId, $scope.absentisim.Month, function (result) {
                    $scope.holidayList = result;
                });
            }
    };

    $scope.employeeList = [];
    $scope.GetEmployeeList = function () {
        $scope.employeeList = [];
        try {
            if (baseService.isUndefinedOrNull($scope.absentisim.Day)) {
                throw "Select number of day.";
            }
            if (!baseService.isUndefinedOrNull($scope.absentisim.Holiday)) {
                $http.get('employees/holidayabsentismassignment/getemployeelist?workDate=' + $scope.absentisim.Holiday + '&day=' + $scope.absentisim.Day)
                    .then(function (response) {
                        $scope.employeeList = response.data;
                        $scope.totalemployee = response.data.length;
                    });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.EmpParameters = {
        limit: 10,
        offset: 0,
        order: '',
        sort: '',
        searchBy: "",
        pageSize: 10,
        total_count: 0,
        search: "",
        serverPagination: true
    };

    $scope.empList = [];
    $scope.GetAssignEmpList = function () {
        $scope.empList = [];
        if (!baseService.isUndefinedOrNull($scope.absentisim.Holiday)) {
            $http.get('employees/holidayabsentismassignment/getassignedemployeelist?workDate=' + $scope.absentisim.Holiday)
                .then(function (response) {
                    $scope.empList = response.data;

                    for (var i = 0; i < $scope.employeeList.length; i++) {
                        var IsSavedemp = getActive($scope.empList, $scope.employeeList[i].EmpSystemID);
                        if (IsSavedemp) {
                            $scope.employeeList[i].Selected = true;
                            $scope.employeeList[i].Active = true;
                        } else {
                            $scope.employeeList[i].Selected = false;
                            $scope.employeeList[i].Active = false;
                        }
                    }
                });
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemID === id ) {
                return true;
            }
        }
        return false;
    }

    $scope.detailList = [];
    $scope.loadDetails = function (data, index) {
        $scope.index_popup = index;
        $scope.EmployeeCode = data.EmployeeCode;
        $http.get('employees/holidayabsentismassignment/getemployeesdetailsdata?workDate=' + $scope.absentisim.Holiday + '&employeeCode=' + $scope.EmployeeCode)
            .then(function (response) {
                $scope.detailList = response.data;
            });
        angular.element(document.querySelector('#DetailPopUp')).modal('show');
    };

    $scope.close = function () {
        angular.element(document.querySelector('#DetailPopUp')).modal('hide');
    };

    $scope.pushTempList = function (data, event, list) {
        if (event.currentTarget.checked) {
            var ob = {};
            ob.Id = null;
            ob.CompanyGroupId = $window.companyGroupId;
            ob.PlantId = $window.plantId;
            ob.EmpSystemID = data.EmpSystemID;
            ob.WorkDate = $scope.absentisim.Holiday;
            $scope.empList.push(ob);
        }
        else {
            $scope.empList.splice($scope.empList.indexOf(data), 1);
            $scope.empList.splice(list.indexOf(data), 1);
        }
    };
    
    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.employeeList.length; i++) {
            $scope.employeeList[i].Selected = _isselected;
        }

        for (var i = 0; i < baseService.arrayLength($scope.employeeList); i++) {
            if (_isselected) {
                var ob = {};
                ob.Id = null;
                ob.CompanyGroupId = $window.companyGroupId;
                ob.PlantId = $window.plantId;
                ob.EmpSystemID = $scope.employeeList[i].EmpSystemID;
                ob.WorkDate = $scope.absentisim.Holiday;
               
                $scope.empList.push(ob);
            }
            else
                for (var j = 0; j < $scope.empList.length; j++) {
                    if ($scope.empList[j].EmpSystemID === $scope.employeeList[i].SystemId) {
                        $scope.empList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    $scope.Save = function () {
        try {
            if ($scope.Action === 'Save') {
                if (baseService.arrayLength($scope.empList) === 0) {
                    throw "Select employee.";
                }
                $http({
                    method: 'POST',
                    url: 'employees/holidayabsentismassignment/create',
                    data: { 'holidayAbsentismAssignments': $scope.empList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.GetEmployeeList();
                        $scope.GetAssignEmpList();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.confirmDelete = function (Id, EmployeeCode, index) {
        $scope.index = index;
        $scope.deleteId = Id;
        $scope.message_confirmation = "Are you sure want to permanently delete [" + EmployeeCode + "]? ";
    };

    $scope.DeleteDetail = function () {
        if (!baseService.isUndefinedOrNull($scope.deleteId)) {
            $http({
                method: 'POST',
                url: 'employees/holidayabsentismassignment/delete',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.GetEmployeeList();
                    $scope.GetAssignEmpList();
                    $scope.index = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}