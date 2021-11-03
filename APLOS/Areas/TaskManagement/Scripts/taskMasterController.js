'use strict';
taskMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function taskMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Task Master';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.taskMasters = [];
    $scope.path = 'taskmanagement/taskMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.taskMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.RemoveLst = function ($index) {
        $scope.tempList.splice($index, 1);
    }
    $scope.taskMaster = {
        Id: null,
        TaskTypeId: null,
        TaskClassId: null,
        TaskCategoryId: null,
        TaskOrgCategoryId: null,
        TaskStatusId: null,
        TaskFrequencyId: null,
        Sequence: 0,
        ShortName: null,
        Description: null,
        TargetDate: null,
        ConfirmationDate: null,
        ConfidenceLevel: null,
        Active: true,
        TaskFor: 'Other',
        AssignBy: $window.employeeId,
        AssignByEmployeeName: $window.employeeName,
        AssignTo: $window.employeeId,
        AssignToEmployeeName: $window.employeeName
    };

    $scope.taskMasterNew = Object.assign({}, $scope.taskMaster);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.taskMasterNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.taskMaster = $scope.taskMasters[$scope.index];
        $scope.taskMasterNew = Object.assign({}, $scope.taskMaster);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.taskTypeList = [];
    $scope.taskClassList = [];
    $scope.taskCategoryList = [];
    $scope.taskOrgCategoryList = [];
    $scope.taskStatusList = [];
    $scope.taskFrequencyList = [];

    cboService.gettaskTypeCbo(function (result) {
        $scope.taskTypeList = result;
    });

    cboService.gettaskClassCbo(function (result) {
        $scope.taskClassList = result;
    });

    cboService.gettaskCategoryCbo(function (result) {
        $scope.taskCategoryList = result;
    });

    cboService.gettaskOrgCategoryCbo(function (result) {
        $scope.taskOrgCategoryList = result;
    });

    cboService.gettaskStatusCbo(function (result) {
        $scope.taskStatusList = result;
    });

    cboService.gettaskFrequencyCbo(function (result) {
        $scope.taskFrequencyList = result;
    });
    $scope.emplist = [];
    $scope.Save = function () {
        if (baseService.arrayLength($scope.tempList) > 0) {
            for (var i = 0; i < $scope.tempList.length; i++) {
                $scope.emplist.push(
                    {
                        Id: null,
                        EmployeeId: $scope.tempList[i].SystemId,
                        TaskMasterId: null
                    }
                )
            }
        }
        angular.copy($scope.taskMasterNew, $scope.taskMaster);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taskMasterNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'model': $scope.taskMaster, 'TaskNotificationList': $scope.emplist },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.taskMasters.push(response.data.TaskMaster);
                        baseService.paginationAdd();
                        $scope.Clear();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.taskMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.taskMasters[$scope.index] = $scope.taskMaster;
                        }
                        $scope.Clear();

                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.taskMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.taskMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.taskMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.popUp1 = function (name) {
        try {
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
            $scope.popUpUrl = 'employees/approvalconfiguration/GetEmployeeDataList';
            //$scope.popUpUrl = 'employees/approvedemployee/getlist';
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                        // getListForm($scope.employeeList);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', '#employeePopUp1');
                    }).finally(function () {
                    });
            };
            $scope.fieldName = name;
            angular.element(document.querySelector('#employeePopUp1')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.popUp = function () {
        try {
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
            $scope.popUpUrl = 'employees/approvalconfiguration/GetEmployeeDataList';
            //$scope.popUpUrl = 'employees/approvedemployee/getlist';
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
    }
    //#end region
    $scope.AssingToDefault = function () {

        if ($scope.taskMasterNew === 'Self') {
            $scope.taskMasterNew.AssignTo = $window.employeeId,
                $scope.taskMasterNew.AssignToEmployeeName = $window.employeeName;
        }

    }

    $scope.selectSingleemp = function (data) {

        if ($scope.fieldName === 'AssignBy') {

            $scope.taskMasterNew.AssignByEmployeeName = data.EmployeeName;
            $scope.taskMasterNew.AssignBy = data.SystemId;
        }
        else {
            $scope.taskMasterNew.AssignToEmployeeName = data.EmployeeName;
            $scope.taskMasterNew.AssignTo = data.SystemId;
        }
        angular.element(document.querySelector('#employeePopUp1')).modal('hide');
        $scope.fieldName = null;
    }

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
    $scope.pushTempList = function (data, event, list) {
        if (event.currentTarget.checked)
            $scope.tempList.push(data);
        else {
            $scope.tempList.splice($scope.tempList.indexOf(data), 1);
            list.splice(list.indexOf(data), 1);
        }
    }



    $scope.SelectEmployeeByButton1 = function () {
        $scope.empIdList1 = [];
        if (baseService.arrayLength($scope.tempList) > 0) {
            angular.forEach($scope.tempList, function (a) {
                if (!$scope.empIdList1.includes(a))
                    $scope.empIdList1.push(a);
            });
        }
        else $scope.empIdList1 = [];
        angular.forEach($scope.empIdList1, function (a) {
            if (!$scope.tempList.includes(a))
                $scope.empIdList1.splice($scope.empIdList1.indexOf(a), 1);
        });
        angular.element(document.querySelector('#employeePopUp1')).modal('hide');
    };
    $scope.empIdList = [];

    $scope.SelectEmployeeByButton = function () {
        $scope.empIdList = [];
        if (baseService.arrayLength($scope.tempList) > 0) {
            angular.forEach($scope.tempList, function (a) {
                if (!$scope.empIdList.includes(a))
                    $scope.empIdList.push(a);
            });
        }
        else $scope.empIdList = [];
        angular.forEach($scope.empIdList, function (a) {
            if (!$scope.tempList.includes(a))
                $scope.empIdList.splice($scope.empIdList.indexOf(a), 1);
        });
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };



    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.taskMaster = {};
        $scope.taskMasterNew = {
            AssignBy: $window.employeeId,
            AssignByEmployeeName: $window.employeeName,
            AssignTo: $window.employeeId,
            AssignToEmployeeName: $window.employeeName,
            TaskFor: $scope.taskMasterNew.TaskFor
        };
       


    }
}