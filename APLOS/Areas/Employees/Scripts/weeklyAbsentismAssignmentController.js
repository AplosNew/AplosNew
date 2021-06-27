'use strict';
weeklyAbsentismAssignmentController.$inject = ['cboService', 'commonMessage', "$window", '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function weeklyAbsentismAssignmentController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.weeklyAbsentismAssignmentList = [];
    $scope.weeklyAbsentismAssignmentSelectedList = [];
    $scope.path = 'employees/weeklyabsentismassignment/getlist';
    $scope.getListUrl = 'employees/weeklyabsentismassignment/getassignedlist';
    baseService.init($scope.getListUrl, null, null, null, 'EmployeeCode', 'EmployeeCode');
    $scope.getData = function (pageno) {
        $rootScope.parameters.yearId = $scope.absentisim.YearId;
        $rootScope.parameters.month = $scope.absentisim.Month;
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
            'value': 'WorkingDate'
        }
    ];

    $scope.weeklyAbsentismAssignmentOb = {
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
        Day: 3
    };


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.weekOffList = [];

    $scope.offdayList = [];
    $scope.RefreshBody = function () {
        $http.get('employees/weeklyabsentismassignment/getoffdaydata?yearId=' + $scope.absentisim.YearId + '&month=' + $scope.absentisim.Month)
            .then(
                function successCallback(response) {
                    $scope.offdayList = response.data;
                    for (var i = 0; i < $scope.offdayList.length; i++) {
                        $scope.offdayList[i].OffDayType = $scope.offdayList[i].OffDayType + i;
                    }
                    $scope.day1 = $filter('dateFiltering')($scope.offdayList[0].OffDayDate, 'dd-MM-yyyy');
                    $scope.day2 = $filter('dateFiltering')($scope.offdayList[1].OffDayDate, 'dd-MM-yyyy');
                    $scope.day3 = $filter('dateFiltering')($scope.offdayList[2].OffDayDate, 'dd-MM-yyyy');
                    $scope.day4 = $filter('dateFiltering')($scope.offdayList[3].OffDayDate, 'dd-MM-yyyy');
                    if (baseService.arrayLength($scope.offdayList) > 4) {
                        $scope.day5 = $filter('dateFiltering')($scope.offdayList[4].OffDayDate, 'dd-MM-yyyy');
                    } else {
                        $scope.day5 = null;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        $scope.getWeekEmployee();
    };

    var ob = {
        EmployeeCode: null
        , EmployeeName: null
        , EmpSystemID: null
        , WeekNo1: false
        , WeekNo2: false
        , WeekNo3: false
        , WeekNo4: false
        , WeekNo5: false
        , W0Status: false
        , W1Status: false
        , W2Status: false
        , W3Status: false
        , W4Status: false
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
        if (!baseService.isUndefinedOrNull($scope.absentisim.Month) && !baseService.isUndefinedOrNull($scope.absentisim.YearId)) {
            $http.get('employees/weeklyabsentismassignment/getassignedemployeelist?month=' + $scope.absentisim.Month + '&yearId=' + $scope.absentisim.YearId)
                .then(function (response) {
                    $scope.empList = response.data;
                });
        }
    };
    

    $scope.WeekEmployeeList = [];
    $scope.dayList = [];
    $scope.getWeekEmployee = function () {
        $scope.WeekEmployeeList = [];
        $scope.dayList = [];
        try {
            if (baseService.isUndefinedOrNull($scope.absentisim.Day)) {
                throw "Day is required.";
            }
            $http.get('employees/weeklyabsentismassignment/getemployeelist?yearId=' + $scope.absentisim.YearId + '&month=' + $scope.absentisim.Month + '&day=' + $scope.absentisim.Day)
                .then(
                    function successCallback(response) {
                        $scope.WeekEmployeeList = response.data;

                        for (var j = 0; j < $scope.WeekEmployeeList.length; j++) {

                            var ob = getObject($scope.dayList, 'EmpSystemID', $scope.WeekEmployeeList[j].EmpSystemID);
                            if (baseService.isUndefinedOrNull(ob.EmpSystemID)) {
                                var isNew = true;
                                var ob = {
                                    EmployeeCode: $scope.WeekEmployeeList[j].EmployeeCode,
                                    EmployeeName: $scope.WeekEmployeeList[j].EmployeeName,
                                    EmpSystemID: $scope.WeekEmployeeList[j].EmpSystemID,

                                    W0Status: false,
                                    W1Status: false,
                                    W2Status: false,
                                    W3Status: false,
                                    W4Status: false
                                };

                                if ($scope.WeekEmployeeList[j].WeekNo1 === 1) {
                                    var d1 = $scope.day1;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d1, empId);
                                    if (IsSavedemp) {
                                        ob.W0Status = true;
                                        ob.WeekNo1 = false;
                                    } else {
                                        ob.WeekNo1 = true;
                                    }

                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo2 === 1) {
                                    var d2 = $scope.day2;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d2, empId);
                                    if (IsSavedemp) {
                                        ob.W1Status = true;
                                        ob.WeekNo2 = false;
                                    } else {
                                        ob.WeekNo2 = true;
                                    }

                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo3 === 1) {
                                    var d3 = $scope.day3;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d3, empId);
                                    if (IsSavedemp) {
                                        ob.W2Status = true;
                                        ob.WeekNo3 = false;
                                    } else {
                                        ob.WeekNo3 = true;
                                    }

                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo4 === 1) {
                                    var d4 = $scope.day4;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d4, empId);
                                    if (IsSavedemp) {
                                        ob.W3Status = true;
                                        ob.WeekNo4 = false;
                                    } else {
                                        ob.WeekNo4 = true;
                                    }
                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo5 === 1) {
                                    var d5 = $scope.day5;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d5, empId);
                                    if (IsSavedemp) {
                                        ob.W4Status = true;
                                        ob.WeekNo5 = false;
                                    } else {
                                        ob.WeekNo5 = true;
                                    }
                                }
                            }
                            else {
                                isNew = false;
                                if ($scope.WeekEmployeeList[j].WeekNo1 === 1) {
                                    var d1 = $scope.day1;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d1, empId);
                                    if (IsSavedemp) {
                                        ob.W0Status = true;
                                        ob.WeekNo1 = false;
                                    } else {
                                        ob.WeekNo1 = true;
                                    }
                                    
                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo2 === 1) {
                                    var d2 = $scope.day2;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d2, empId);
                                    if (IsSavedemp) {
                                        ob.W1Status = true;
                                        ob.WeekNo2 = false;
                                    } else {
                                        ob.WeekNo2 = true;
                                    }
                                    
                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo3 === 1) {
                                    var d3 = $scope.day3;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d3, empId);
                                    if (IsSavedemp) {
                                        ob.W2Status = true;
                                        ob.WeekNo3 = false;
                                    } else {
                                        ob.WeekNo3 = true;
                                    }
                                    
                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo4 === 1) {
                                    var d4 = $scope.day4;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d4, empId);
                                    if (IsSavedemp) {
                                        ob.W3Status = true;
                                        ob.WeekNo4 = false;
                                    } else {
                                        ob.WeekNo4 = true;
                                    }
                                    

                                }
                                else if ($scope.WeekEmployeeList[j].WeekNo5 === 1) {
                                    var d5 = $scope.day5;
                                    var empId = $scope.WeekEmployeeList[j].EmpSystemID;
                                    var IsSavedemp = checkExist($scope.empList, d5, empId);
                                    if (IsSavedemp) {
                                        ob.W4Status = true;
                                        ob.WeekNo5 = false;
                                    } else {
                                        ob.WeekNo5 = true;
                                    }
                                    
                                }
                            }

                            //}//offday
                            if (isNew) {
                                $scope.dayList.push(ob);
                                isNew = false;
                                $scope.totalemployee = $scope.dayList.length;
                            }
                        }//emp
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    function checkExist(list, day, employeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemID === employeeId && list[i].WorkingDate === day) {
                return true;
                ////
                break;
            }
        }
        return false;
    }

    function getObject(list, field, value) {
        for (var i = 0; i < list.length; i++) {
            if (list[i][field] === value) {
                return list[i];
                ////
                break;
            }
        }
        return ob;
    }

    $scope.detailList = [];
    $scope.loadDetails = function (data, W, index) {
        $scope.index_popup = index;
        $scope.EmployeeCode = data.EmployeeCode;

        var _d = null;
        var d = null;

        for (var i = 0; i < $scope.offdayList.length; i++) {

            if ($scope.offdayList[i].OffDayType === W) {
                _d = getDate(W, $scope.offdayList);
                d = $filter('dateFiltering')(_d, 'dd-MM-yyyy');
                break;
            }//if
            if ($scope.offdayList[i].OffDayType === W) {
                _d = getDate(W, $scope.offdayList);
                d = $filter('dateFiltering')(_d, 'dd-MM-yyyy');
                break;
            }//if
            if ($scope.offdayList[i].OffDayType === W) {
                _d = getDate(W, $scope.offdayList);
                d = $filter('dateFiltering')(_d, 'dd-MM-yyyy');
                break;
            }//if
            if ($scope.offdayList[i].OffDayType === W) {
                _d = getDate(W, $scope.offdayList);
                d = $filter('dateFiltering')(_d, 'dd-MM-yyyy');
                break;
            }//if
            if ($scope.offdayList[i].OffDayType === W) {
                _d = getDate(W, $scope.offdayList);
                d = $filter('dateFiltering')(_d, 'dd-MM-yyyy');
                break;
            }//if
        }//for

        $http.get('employees/weeklyabsentismassignment/getemployeesdetailsdata?workDate=' + d + '&employeeCode=' + $scope.EmployeeCode)
            .then(function (response) {
                $scope.detailList = response.data;
            });
        angular.element(document.querySelector('#DetailPopUp')).modal('show');
    };

    $scope.close = function () {
        angular.element(document.querySelector('#DetailPopUp')).modal('hide');
    };

    function getDate(weekNo, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OffDayType === weekNo) {
                return list[i].OffDayDate;
            }
        }
        return null;
    }

    $scope.tempList = [];
    $scope.pushTempList = function (data, event, list) {
        if (event.currentTarget.checked)
            $scope.tempList.push(data);
        else {
            $scope.tempList.splice($scope.tempList.indexOf(data), 1);
            $scope.tempList.splice(list.indexOf(data), 1);
        }
    };

    $scope.saveList = [];

    function makeDataForSave() {
        try {
            for (var i = 0; i < $scope.tempList.length; i++) {

                if ($scope.tempList[i].W0Status === true) {
                    var d1 = getDate('W0', $scope.offdayList);
                    if (!baseService.isUndefinedOrNull(d1)) {
                        $scope.saveList.push({
                            EmpSystemID: $scope.tempList[i].EmpSystemID,
                            WorkingDate: $filter('dateFiltering')(d1, 'dd-MM-yyyy'),
                            CompanyGroupId: $window.companyGroupId,
                            PlantId: $window.plantId
                        });
                    }
                }//if
                if ($scope.tempList[i].W1Status === true) {
                    var d2 = getDate('W1', $scope.offdayList);
                    if (!baseService.isUndefinedOrNull(d2)) {
                        $scope.saveList.push({
                            EmpSystemID: $scope.tempList[i].EmpSystemID,
                            WorkingDate: $filter('dateFiltering')(d2, 'dd-MM-yyyy'),
                            CompanyGroupId: $window.companyGroupId,
                            PlantId: $window.plantId
                        });
                    }
                }//if
                if ($scope.tempList[i].W2Status === true) {
                    var d3 = getDate('W2', $scope.offdayList);
                    if (!baseService.isUndefinedOrNull(d3)) {
                        $scope.saveList.push({
                            EmpSystemID: $scope.tempList[i].EmpSystemID,
                            WorkingDate: $filter('dateFiltering')(d3, 'dd-MM-yyyy'),
                            CompanyGroupId: $window.companyGroupId,
                            PlantId: $window.plantId
                        });
                    }
                }//if
                if ($scope.tempList[i].W3Status === true) {
                    var d4 = getDate('W3', $scope.offdayList);
                    if (!baseService.isUndefinedOrNull(d4)) {
                        $scope.saveList.push({
                            EmpSystemID: $scope.tempList[i].EmpSystemID,
                            WorkingDate: $filter('dateFiltering')(d4, 'dd-MM-yyyy'),
                            CompanyGroupId: $window.companyGroupId,
                            PlantId: $window.plantId
                        });
                    }
                }//if
                if ($scope.tempList[i].W4Status === true) {
                    var d5 = getDate('W4', $scope.offdayList);
                    if (!baseService.isUndefinedOrNull(d5)) {
                        $scope.saveList.push({
                            EmpSystemID: $scope.tempList[i].EmpSystemID,
                            WorkingDate: $filter('dateFiltering')(d5, 'dd-MM-yyyy'),
                            CompanyGroupId: $window.companyGroupId,
                            PlantId: $window.plantId
                        });
                    }
                }//if
                if (baseService.arrayLength($scope.saveList) === 0) {
                    throw "Select day for Employee Code: " + $scope.tempList[i].EmployeeCode + ".";
                }

            }//for

        } catch (e) {
            throw e;
        }
    }



    $scope.CheckAll = function (event) {
        $scope.tempList = [];
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.dayList.length; i++) {
            $scope.dayList[i].Selected = _isselected;
        }

        for (var i = 0; i < baseService.arrayLength($scope.dayList); i++) {
            if (_isselected)
                $scope.tempList.push($scope.dayList[i]);
            else
                for (var j = 0; j < $scope.tempList.length; j++) {
                    if ($scope.tempList[j].EmpSystemID === $scope.tempList[i].EmpSystemID) {
                        $scope.tempList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    $scope.Save = function () {
        $scope.saveList = [];
        try {
            if ($scope.Action === 'Save') {
                makeDataForSave();
                if (baseService.arrayLength($scope.saveList) === 0) {
                    throw "Select employee and day.";
                }
                $http({
                    method: 'POST',
                    url: 'employees/weeklyabsentismassignment/create',
                    data: { 'weeklyAbsentismAssignments': $scope.saveList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.WeekEmployeeList = [];
                        $scope.dayList = [];
                        $scope.RefreshBody();
                        $scope.getData();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getColor = function (item) {
        if (item) {
            return 'filled';
        }
        else {
            return 'empty';
        }
    };
    $scope.getColor1 = function (item) {
        if (item) {
            return 'filled';
        }
        else {
            return 'empty';
        }
    };
    $scope.getColor2 = function (item) {
        if (item) {
            return 'filled';
        }
        else {
            return 'empty';
        }
    };
    $scope.getColor3 = function (item) {
        if (item) {
            return 'filled';
        }
        else {
            return 'empty';
        }

    };
    $scope.getColor4 = function (item) {
        if (item) {
            return 'filled';
        }
        else {
            return 'empty';
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
                url: 'employees/weeklyabsentismassignment/delete',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.index = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}