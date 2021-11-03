'use strict';
entityTaskController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', 'cboService'];
function entityTaskController(commonMessage, $scope, $rootScope, baseService, $http, cboService, ) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.entityTaskList = [];
    $scope.path = 'TaskManagement/EntityTask/';
    $scope.getentityTaskListUrl = $scope.path + 'getlist?entityId=';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteGraphUrl = $scope.path + 'deleteGraph?entityId';
    $scope.getData = function (pageno) {
        $rootScope.tempList = [];
        $scope.entityTaskList = [];
        $http.get($scope.getentityTaskListUrl + $scope.entityTask.EntityId)
            .then(function (response) {
                $scope.entityTaskList = response.data.Rows;
            });
    };

    $scope.entityTask = {
        Id: null
        , CompanyId: null
        , EntityId: null
        , TaskMasterId: null
        , PlantId: null
        , EmpSystemId:null
    };

    // #region DDL

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.entityTask.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };


    $scope.entityList = [];
    $scope.getEntity = function () {
        $scope.entities = [];
        $scope.entityTaskList = [];

        $http({
            method: 'POST',
            url: "Processes/EntityProcessTag/GetEntity?plantId=" + $scope.entityTask.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };

    // #endregion

    // #region POP UP
    $scope.taskMasterList = [];
    $scope.processParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'UserDefineTask'
        , searchBy: "UserDefineTask"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.TaskMasterPopUp = function () {
        $scope.$broadcast('show-errors-check-validity');
        if (!$scope.modelForm.$valid) return;
        $rootScope.tempList = [];
        angular.forEach($scope.entityTaskList, function (a) {
            $rootScope.tempList.push({
                Id: a.TaskMasterId
                , Sequence: a.Sequence
                , Code: a.Code
                , StandardName: a.StandardName
                , UserName: a.UserDefineTask
            });
        });
        baseService.setCurrentPage('taskMasterList');
        $scope.getTaskData = function (pageno) {
            $scope.getProcessUrl = 'TaskManagement/EntityTask/GetTaskMasterData';
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.taskMasterList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.taskMasterList); t++) {
                        $scope.taskMasterList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.taskMasterList[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#TaskMasterPopUp')).modal('show');
        $scope.getTaskData();
    };
    $scope.CloseTaskMasterPopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#TaskMasterPopUp')).modal('hide');
    };
    $rootScope.searchProcessByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserDefineTask'
        }
    ];

    $scope.addSFG = function () {
        if (baseService.arrayLength($scope.taskMasterList) === 0)
            return ShowResult('Please select at least one row!', 'failure', 'TaskPopUp');
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.entityTaskList, 'TaskMasterId', a.Id)) {
                    $scope.entityTaskList.push({
                        Id: null
                        , EntityId: $scope.entityTask.EntityId
                        , TaskMasterId: a.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , StandardName: a.StandardName
                        , UserDefineTask: a.UserDefineTask
                    });
                }
            });
        }
        else
            $scope.entityTaskList = [];
        angular.forEach($scope.entityTaskList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.TaskMasterId))
                $scope.entityTaskList.splice(a, 1);
        });
        $scope.CloseTaskMasterPopUp();
    };

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanently delete [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope[$scope.listName][$scope.popUpIndex].Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope[$scope.listName][$scope.popUpIndex].Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    // #endregion

    $scope.Save = function () {
        try {

            if (baseService.arrayLength($scope.entityTaskList) === 0) {
                throw 'No data found.';
            }
            $http({
                method: 'POST'
                , url: 'taskmanagement/entitytask/create'
                , data: $scope.entityTaskList
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST'
            , url: $scope.deleteGraphUrl + $scope.entityTask.EntityId
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true)
                ShowResult(response.data.Message, "failure");
            else {
                ShowResult(response.data.Message, "success");
                $scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
    }


    $scope.showEmployeeInformationByRowModal = function (id, name, index) {
        $scope.empNme = name;
        $scope.empId = id;
        $scope.entityTempIndex = index;
        getEmployeeInformationByEntityData();

        if ($scope.empNme === 'TaskEmployeeName') {
            angular.element(document.querySelector('#Taskemployeepopup')).modal('show');
        } 

    };


    $scope.popUpEmpByEntityParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.sbEmployeeInformation = [];
    $scope.employeeinformationData = [];
    function getEmployeeInformationByEntityData() {
        $scope.popUpTitle = '';
        var popUpUrl = '';
        $scope.popUpTitle = 'Employee Profile';
        popUpUrl = 'employees/EmployeeInformation/GetEmployeeListByCompanyGroup';
        $scope.popUpEmpByEntityParameters.sort = 'EmployeeCode';
        $scope.popUpEmpByEntityParameters.searchBy = 'EmployeeCode';
        baseService.setCurrentPage('employeeinformationData');
        $scope.loadEIByEntityData = function (pageno) {
            baseService.paginationBase(popUpUrl, pageno, $scope.popUpEmpByEntityParameters)
                .then(function (result) {
                    $scope.employeeinformationData = result.Rows;
                    $scope.popUpEmpByEntityParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.sbEmployeeInformation) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbEmployeeInformation);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'Taskemployeepopup');
                }).finally(function () {
                });
        };
        $scope.loadEIByEntityData();
    }

    $scope.getTaskEmployee = function (data) {
        $scope.entityTaskList[$scope.entityTempIndex][$scope.empId] = data.SystemId;
        $scope.entityTaskList[$scope.entityTempIndex][$scope.empNme] = data.EmployeeName;
        angular.element(document.querySelector('#Taskemployeepopup')).modal('hide');
    };




    $scope.Clear = function () {
        $scope.tableShow = false;
        $scope.entityTask = {};
        $scope.entities = [];
        $scope.entityValue = [];
        $scope.entityTaskList = [];
    }
}