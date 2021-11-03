'use strict';
compliedShiftAssignmentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter', 'cboService', '$window'];
function compliedShiftAssignmentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, cboService, $window) {
    $rootScope.title = 'Shift Assignment';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.compliedShiftActualShiftTagList = [];
    $scope.path = 'humanresource/compliedshiftassignment/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'ShiftName', 'ShiftName');
    $scope.getData = function (pageno) {
        $rootScope.parameters.workDate = $scope.model.WorkDate;
        $rootScope.parameters.compliedShiftId = $scope.model.CompliedShiftId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.compliedShiftActualShiftTagList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.compliedShiftAssignment = {
        Id: null,
        CompliedShiftId: null,
        PlantId: $window.PlantId,
        EmpSystemID: null,
        SectionId: null,
        ActualShiftId: null,
        WorkDate: null,
        CompliedShiftGroupId: null
    };
    $scope.modelNew = Object.assign({}, $scope.compliedShiftAssignment);

    $scope.model = {
        WorkDate: null,
        CompliedShiftId: null
    };

    $scope.SectionList = [];
    cboService.getSectionCbo(function (result) {
        $scope.SectionList = result;
    });

    $scope.compliedShiftList = [];
    cboService.getCompliedShiftCbo(function (result) {
        $scope.compliedShiftList = result;
    });

    $scope.compliedShiftGroupingList = [];
    cboService.getCompliedShiftGroupingCbo(function (result) {
        $scope.compliedShiftGroupingList = result;
    });

    $scope.Get = function (index) {
        $scope.Action = 'Update';
        $scope.index = index;
        $scope.compliedShiftActualShiftTag = $scope.compliedShiftActualShiftTagList[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.compliedShiftActualShiftTag);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.modelNew.CompliedShiftId)) {
                throw "Select Shift.";
            }
            $scope.$broadcast('show-errors-check-validity');
            for (var i = 0; i < $scope.tempList.length; i++) {
                $scope.tempList[i].CompliedShiftId = $scope.modelNew.CompliedShiftId;
                $scope.tempList[i].SectionId = $scope.modelNew.SectionId;
                $scope.tempList[i].WorkDate = $scope.modelNew.WorkDate;
            }
            angular.copy($scope.modelNew, $scope.compliedShiftActualShiftTag);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.tempList,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.confirmDelete = function (Id, EmployeeCode, index) {
        $scope.index = index;
        $scope.deleteId = Id;
        $scope.message_confirmation = "Are you sure want to permanently delete?";
    };

    $scope.DeleteDetail = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            $scope.tempList.splice($scope.index, 1);
            $scope.index = -1;
        } else {
            $http({
                method: 'POST',
                url: 'humanresource/compliedshiftassignment/delete',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.getFixedShiftList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.compliedShiftActualShiftTag = {};
        $scope.modelNew = {};
        $scope.tempList = [];
    }

    // #region  Dynamic PopUp
    $scope.popUpList = [];
    $scope.popUp = function (name) {
        $scope.popUpParameters = {
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
        try {

            //if (baseService.isUndefinedOrNull($scope.modelNew.SectionId)) {
            //    throw "Select Section";
            //}
            if (name === 'EmployeeInformation') {
                if (baseService.isUndefinedOrNull($scope.modelNew.WorkDate)) {
                    throw "Select Work Date";
                }
            }

            //if (baseService.isUndefinedOrNull($scope.modelNew.CompliedShiftGroupId)) {
            //    throw "Select Shift Group";
            //}
            $scope.popUpUrl = '';
            $scope.popUpParameters.sort = '';
            $scope.popUpParameters.searchBy = '';

            if (name === 'EmployeeInformation') {
                $scope.popUpUrl = 'HumanResource/CompliedShiftAssignment/getallemployeelist?sectionId=' + $scope.modelNew.SectionId + '&workDate=' + $scope.modelNew.WorkDate + '&compliedShiftGruopId=' + $scope.modelNew.CompliedShiftGroupId;
            }
            if (name === 'Employee Information') {
                $scope.rosterShiftList = [];
                $scope.rosterShift();
                $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist';
            }
            $scope.popUpParameters.sort = 'EmployeeCode';
            $scope.popUpParameters.searchBy = 'EmployeeCode';
            if (name === 'EmployeeInformation' || name === 'Employee Information') {
                $scope.popUpTitle = 'Employee Information';
            }
            if (name === 'EmployeeInformation') {
                $scope.popUpParameters.offset = 0;
                $scope.popUpData = function (pageno) {
                    baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                        .then(function (result) {
                            $scope.popUpDataList = result.Rows;
                            if (baseService.arrayLength($scope.tempList) !== 0) {
                                for (var i = 0; i < $scope.popUpDataList.length; i++) {
                                    $scope.popUpDataList[i].Active = getActive($scope.tempList, $scope.popUpDataList[i].EmpSystemID);
                                }
                            }
                            $scope.popUpParameters.total_count = result.Total;
                            if (baseService.arrayLength($scope.popUpList) === 0) {
                                baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                            }
                        }, function () {
                            ShowResult(commonMessage.NetworkError, 'failure');
                        }).finally(function () {
                        });
                };
                $scope.fieldName = name;
                angular.element(document.querySelector('#popUp')).modal('show');
                $scope.popUpData();
            }

            if (name === 'Employee Information') {
                $scope.popUpData = function (pageno) {
                    baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                        .then(function (result) {
                            $scope.dataList = result.Rows;

                            $scope.popUpParameters.total_count = result.Total;
                            if (baseService.arrayLength($scope.popUpList) === 0) {
                                baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                            }
                        }, function () {
                            ShowResult(commonMessage.NetworkError, 'failure');
                        }).finally(function () {
                        });
                };
                $scope.fieldName = name;
                angular.element(document.querySelector('#EmppopUp')).modal('show');
                $scope.popUpData();
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.SelectEmployee = function (data) {
        setData(data);
        angular.element(document.querySelector('#EmppopUp')).modal('hide');
        $scope.popUpParameters.offset = 0;
    };
    function setData(ob) {
        if ($scope.fieldName === 'Employee Information') {
            $scope.modelNew.EmpSystemID = ob.SystemId;
            $scope.modelNew.EmployeeCode = ob.EmployeeCode;
            $scope.modelNew.EmployeeName = ob.EmployeeName;
            $scope.complied.EmpSystemId = $scope.modelNew.EmpSystemID;
            $scope.compliedRoster.EmpSystemId = $scope.modelNew.EmpSystemID;
            $scope.getRosterEmployeeList($scope.modelNew.EmpSystemID);
            $scope.rosterShift();
            $scope.popUpParameters.offset = 0;
        }
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
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.closePopUp = function () {
        if ($scope.fieldName === 'EmployeeInformation') {
            angular.element(document.querySelector('#popUp')).modal('hide');
        }
        if ($scope.fieldName === 'Employee Information') {
            angular.element(document.querySelector('#EmppopUp')).modal('hide');
        }
        $scope.popUpParameters.offset = 0;
    };

    $scope.clearEmployee = function () {
        $scope.modelNew.EmpSystemID = null;
        $scope.modelNew.EmployeeCode = null;
        $scope.modelNew.EmployeeName = null;
        $scope.FixedShiftList = [];
        $scope.rosterShiftList = [];
        $scope.rosterShift();
        $scope.roster = false;
        $scope.complied = {};
        $scope.compliedRoster = {};
    };
    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {

            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.EmpSystemID) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].EmpSystemID === data.EmpSystemID) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, EmpSystemID) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemID === EmpSystemID) {
                return true;
            }
        }
        return false;
    }

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            $scope.popUpDataList[i].Active = _isselected;
        }
        for (var j = 0; j < baseService.arrayLength($scope.popUpDataList); j++) {
            if (_isselected) {
                if (checkExistTempList($scope.tempList, $scope.popUpDataList[j].EmpSystemID) === false) {

                    $scope.tempList.push($scope.popUpDataList[j]);
                }
            }
            else {
                for (var k = 0; k < $scope.tempList.length; k++) {
                    if ($scope.tempList[k].EmpSystemID === $scope.popUpDataList[j].EmpSystemID) {
                        $scope.tempList.splice(k, 1);
                        break;
                    }
                }
            }
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemID === id) {
                return true;
            }
        }
        return false;
    }
    // #endregion

    $scope.complied = {
        PlantId: null,
        EmpSystemId: null,
        CompliedShiftId: null,
        WorkDate: null
    };

    $scope.compliedRoster = {
        Id: null,
        EmpSystemId: null,
        CompliedShiftRosterMasterID: null
    };

    $scope.SaveSingle = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.complied.CompliedShiftId)) {
                throw "Select Shift.";
            }
            if (baseService.isUndefinedOrNull($scope.complied.WorkDate)) {
                throw "Select EffectiveDate.";
            }
            $scope.$broadcast('show-errors-check-validity');
            if (!baseService.isUndefinedOrNull($scope.modelNew.EmpSystemID)) {
                $http({
                    method: 'POST',
                    url: 'humanresource/compliedshiftassignment/savesingle',
                    data: $scope.complied,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getFixedShiftList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.FixedShiftList = [];
    $scope.getFixedShiftList = function () {
        $http({
            method: 'GET',
            url: 'humanresource/compliedshiftassignment/GetEmployeeFixedShift?empId=' + $scope.modelNew.EmpSystemID + '&fromDate=' + $scope.modelNew.FromDate + '&toDate=' + $scope.modelNew.ToDate
        }).then(function successCallback(response) {
            $scope.FixedShiftList = response.data;

        });
    };

    $scope.rosterShiftList = [];
    $scope.roster = false;
    $scope.getRosterEmployeeList = function (empId) {
        cboService.getCboEmployeeRosterShift(empId, function (result) {
            if (result.length > 0) {
                $scope.compliedRoster.Id = result[0].Id;
                $scope.compliedRoster.CompliedShiftRosterMasterID = result[0].Value;
                $scope.roster = true;
            }
            else {
                $scope.roster = false;
                $scope.compliedRoster.Id = null;
            }
        });
    };

    $scope.rosterShift = function () {
        cboService.getCboCompliedRosterShift(function (result) {
            $scope.rosterShiftList = result;
        });
    };
    $scope.rosterShift();

    $scope.Uncheck = function () {
        if ($scope.roster === false) {
            $scope.compliedRoster.CompliedShiftRosterMasterID = null;
        } else {
            $scope.roster === true;
        }                  
    };

    $scope.SaveRoster = function () {
        try {
            $scope.compliedRoster.EmpSystemId = $scope.modelNew.EmpSystemID;
            if ($scope.roster  && !baseService.isUndefinedOrNull($scope.compliedRoster.CompliedShiftRosterMasterID)) {
                if (baseService.isUndefinedOrNull($scope.compliedRoster.EmpSystemId)) {
                    throw "Select Employee.";
                }
                if (baseService.isUndefinedOrNull($scope.compliedRoster.CompliedShiftRosterMasterID)) {
                    throw "Select Shift.";
                }
            }

            $scope.$broadcast('show-errors-check-validity');
            if (!baseService.isUndefinedOrNull($scope.modelNew.EmpSystemID)) {
                $http({
                    method: 'POST',
                    url: 'humanresource/compliedshiftassignment/saverostershift',
                    data: $scope.compliedRoster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getRosterEmployeeList($scope.modelNew.EmpSystemID);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };




    $scope.unassignParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'EmployeeCode',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: "",
        serverPagination: true
    };

    $scope.searchEmpList = [
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Plant',
            'value': 'PlantId'
        },
        {
            'name': 'Department',
            'value': 'DepartmentId'
        },
        {
            'name': 'Section',
            'value': 'SectionId'
        },
        {
            'name': 'SubSection',
            'value': 'SubSectionId'
        }
        ,
        {
            'name': 'Designation',
            'value': 'DesignationId'
        }
        ,
        {
            'name': 'Given Designation',
            'value': 'GivenDesignationId'
        }
    ];

    $scope.unassignEmployees = [];
    $scope.LoadDataList = function () {
        $scope.unassignParameters.offset = 0;
        $scope.GetAllData = function (pageno) {
            baseService.paginationBase('humanresource/compliedshiftassignment/getunassignemployee', pageno, $scope.unassignParameters)
                .then(function (data) {
                    $scope.unassignEmployees = data.Rows;
                    $scope.unassignParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetAllData();
    };
    $scope.LoadDataList();
}