'use strict';
workStationDailyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', 'cboService', '$window'];
function workStationDailyController(commonMessage, $scope, $rootScope, baseService, $http, cboService, $window) {
    $rootScope.title = "Work Station Daily";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.workStationDailies = [];
    $scope.path = 'WorkCenters/workstationdaily/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'WorkCenter,Operation', 'WorkCenter');

    $scope.workStationDaily = {
        Id: null
        , EntityId: null
        , WorkCenterId: null
        , OperationId: null
        , EmployeeId: null
        , ArticleId: null
        , WorkStation: null
        , EntryDate: null
    };
    $scope.workStationDailyNew = Object.assign({}, $scope.workStationDaily);

    $scope.searchList = [
        {
            'name': 'Work Center',
            'value': 'WorkCenter'
        },
        {
            'name': 'Operation',
            'value': 'Operation'
        },
        {
            'name': 'Work Station',
            'value': 'WorkStation'
        }];
    $scope.getWorkStation = function (entityId, workcenterId) {
        $http({
            method: 'GET',
            url: 'WorkCenters/workstationdaily/getworkstation?entityId=' + $scope.workStationDailyNew.EntityId + '&workcenterId=' + $scope.workStationDailyNew.WorkCenterId,
        }).then(function (response) {
            $scope.workStationDailyNew.WorkStation = baseService.arrayLength(response.data) + 1;
        });
    };
    $scope.getData = function (pageno) {
        $rootScope.parameters.entityId = $scope.workStationDailyNew.EntityId;
        $rootScope.parameters.workCenterId = $scope.workStationDailyNew.WorkCenterId;
        $rootScope.parameters.entryDate = $scope.workStationDailyNew.EntryDate;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.workStationDailies = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    cboService.getCboProductionEntityByCompany($window.companyGroupId, $window.companyId, function (result) {
        $scope.entityList = result;
    });

    $scope.getCboWorkCenterMaster = function () {
        cboService.getCboWorkCenterMasterByEntity($scope.workStationDailyNew.EntityId, function (result) {
            $scope.workCenterList = result;
        });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getworkStationDaily = angular.copy($scope.workStationDailies[$scope.index]) // for not change in grid
        $scope.workStationDailyNew = $scope.getworkStationDaily;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    // #region Employee

    $scope.employeeUrl = 'WorkCenters/workcentermaster/GetEmployeeListByPlant';

    $scope.employeeFilterList = [
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
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

    $scope.employeeParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'EmployeeCode, FirstName, MiddleName, LastName '
        , searchBy: 'EmployeeCode'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.searchEmployeeByList = [];
        $scope.getEmployeeData = function (pageno) {
            $scope.employeeParameters.plantId = $window.PlantId;
            baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.workStationDailyNew.EmployeeId = employee.SystemId;
            $scope.workStationDailyNew.EmployeeName = employee.EmployeeName;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    // #endregion Employee

    $scope.searchbyoperationTypelist = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        }
    ];
    $scope.operationParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getOperationData = function () {
        try {
            baseService.setCurrentPage('operationData');
            if (baseService.isUndefinedOrNull($scope.workStationDailyNew.EntityId)) {
                throw "Please Select Entity !!!!";
            }
            if (baseService.isUndefinedOrNull($scope.workStationDailyNew.WorkCenterId)) {
                throw "Please Select Work Center !!!!";
            }
            $scope.loadOperationData = function (pageno) {
                baseService.paginationBase('WorkCenters/workstationdaily/getoperationlist?entityId=' + $scope.workStationDailyNew.EntityId + '&processId=' + $scope.workStationDailyNew.WorkCenterId, pageno, $scope.operationParameters)
                    .then(function (result) {
                        $scope.operationData = result.Rows;
                        $scope.operationParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', 'operationmodal');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#operationmodal')).modal('show');
            $scope.loadOperationData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.getoperationCode = function (ob) {
        $scope.workStationDailyNew.OperationId = ob.Id;
        $scope.workStationDailyNew.UserName = ob.UserName;
        $scope.clearMachineMaster();
        $scope.processId = ob.ProcessId;
        angular.element(document.querySelector('#operationmodal')).modal('hide');
    };
    $scope.clearOperationCode = function () {
        $scope.workStationDailyNew.OperationId = null;
        $scope.workStationDailyNew.UserName = null;
    };

    $scope.Save = function () {
        angular.copy($scope.workStationDailyNew, $scope.workStationDaily);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.workStationDailyNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.workStationDailyNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        baseService.paginationAdd();
                        ClearFields(response.data.WorkStation);
                        $scope.getWorkStation();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.workStationDailyNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.WorkStation);
                        $scope.getWorkStation();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.confirmDelete = function (Id) {
        $scope.deleteId = Id;
        $scope.message_confirmation = "Are you sure to delete permanently [" + Id + "] ";
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: $scope.deleteUrl,
            dataType: 'JSON',
            data: { 'Id': $scope.workStationDailyNew.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.workStationDailies.splice($scope.index, 1);
                baseService.paginationRemove();
                $scope.getData();
                ClearFields(response.data.WorkStation);
                $scope.getWorkStation();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.getWorkStation());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.workStationDaily = {};
        $scope.EntityId = $scope.workStationDailyNew.EntityId;
        $scope.WorkCenterId = $scope.workStationDailyNew.WorkCenterId;
        $scope.EntryDate = $scope.workStationDailyNew.EntryDate;
        $scope.workStationDailyNew = {};
        $scope.workStationDailyNew.EntityId = $scope.EntityId;
        $scope.workStationDailyNew.WorkCenterId = $scope.WorkCenterId;
        $scope.workStationDailyNew.EntryDate = $scope.EntryDate;
        $scope.workStationDailyNew.WorkStation = seq;
    }

    // #region Material Master

    $scope.materialList = [];
    $scope.materialParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'MaterialMasterName'
        , searchBy: "MaterialMasterName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.materialPopUp = function (index) {
        $scope.popUpIndex = index;
        $scope.materialDataList = [];
        $scope.materialUrl = 'Materials/MaterialMaster/GetCommonMachineListByProcess?processIds=[]';
        baseService.setCurrentPage('materialDataList');
        $scope.getMaterialData = function (pageno) {
            baseService.paginationBase($scope.materialUrl, pageno, $scope.materialParameters)
                .then(function (result) {
                    $scope.materialDataList = result.Rows;
                    $scope.materialParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.materialList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.materialList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'materialId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#materialId')).modal('show');
        $scope.getMaterialData();
    };
    $scope.closeMaterial = function () {
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#materialId')).modal('hide');
    };

    // #endregion MM

    // #region Article

    $scope.articleList = [];
    $scope.articleParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'StandardName'
        , searchBy: "StandardName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.articlePopUp = function (materialMasterId, materialIndex) {
        try {
            //var flag = false;
            //var prosessIds = $scope.materialDataList[materialIndex].ProsessIds;
            //if (!baseService.isUndefinedOrNull(prosessIds)) {
            //    var processAray = prosessIds.split(',');
            //    for (var i = 0; i < baseService.arrayLength(processAray); i++) {
            //        if (baseService.valueCheckInList($scope.sprocessList, 'ProcessId', processAray[i])) {
            //            flag = true;
            //            break;
            //        }
            //    }
            //}
            //if (!flag) throw 'operation process and machine process not match ';
            $scope.excluedList = ['SkillName', 'MachineAllowance'];
            $scope.articleDataList = [];
            $scope.articleUrl = 'Machines/Operation/GetArticleListByMaterialMaster?materialMasterId=' + materialMasterId;
            baseService.setCurrentPage('dataList');
            $scope.getarticleData = function (pageno) {
                baseService.paginationBase($scope.articleUrl, pageno, $scope.articleParameters)
                    .then(function (result) {
                        $scope.articleDataList = result.Rows;
                        $scope.articleParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.articleList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.articleList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', 'articleId');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#articleId')).modal('show');
            $scope.getarticleData();
        } catch (e) {
            ShowResult(e, '', 'materialId');
        }

    };
    $scope.selectArticle = function (data) {
        $scope.workStationDailyNew.ArticleId = data.Id;
        $scope.workStationDailyNew.ArticleName = data.StandardName;

        $scope.closeArticle();
        $scope.closeMaterial();
    };
    $scope.closeArticle = function () {
        angular.element(document.querySelector('#articleId')).modal('hide');
    };
    $scope.clearMachineMaster = function () {
        $scope.workStationDailyNew.ArticleId = null;
        $scope.workStationDailyNew.ArticleName = null;
    };

    // #endregion Article
}