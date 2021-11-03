'use strict';
entityOperationSettingsController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$http", "$window"];
function entityOperationSettingsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $window) {
    $rootScope.title = "Entity Operation Settings";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Machines/entityoperationsettings/';
    $scope.getListUrl = $scope.path + 'GetList';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete?id=';
    $scope.entityList = [];

    $scope.model = {
        Id: null
        , PlantId: null
        , EntityId: null
    };
    $scope.modelNew = angular.copy($scope.model);

    cboService.getCboProductionEntityByPlant(null, $window.companyId, $window.plantId, function (result) {
        $scope.entityList = result;
    });

    $scope.getData = function (pageno) {
        if (baseService.isUndefinedOrNull($scope.modelNew.EntityId)) {
            ClearFields();
            return $scope.modelList = [];
        }
        $http.get($scope.getListUrl + '?entityId=' + $scope.modelNew.EntityId).then(function (response) {
            $scope.modelList = response.data;
            if (baseService.arrayLength($scope.modelList) === 0) $scope.Action = 'Save';
            else $scope.Action = 'Update';
        });
    };

    $scope.Save = function () {
        if (baseService.arrayLength($scope.modelList) === 0)
            return ShowResult('Please select operation', 'failure');
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelNewForm.$valid) {
            angular.copy($scope.modelNew, $scope.model);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.modelList,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.modelList,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.removeRowModal = function (data, index) {
        try {
            $scope.index = index;
            $scope.id = data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + data.OperationName + "] ";
            angular.element(document.querySelector('#confirmPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.modelList.splice($scope.index, 1);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            $scope.modelList.splice($scope.index, 1);
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = {};
        $scope.modelList = [];
    }

    // #region Operation
    $scope.searchByList = [
        {
            'name': 'Operation',
            'value': 'UserName'
        },
        {
            'name': 'Process',
            'value': 'Process'
        },
        {
            'name': 'Operation Type',
            'value': 'OperationTypeCode'
        },
        {
            'name': 'Operation Category',
            'value': 'OperationCategoryName'
        },
        {
            'name': 'Operation Activity',
            'value': 'OperationActivityName'
        }
    ];
    $scope.popUpDataList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
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
    $rootScope.tempList = [];
    $scope.popUp = function () {
        $rootScope.tempList = [];
        angular.forEach($scope.modelList, function (a) {
            $rootScope.tempList.push({
                Id: a.OperationId
                , OperationName: a.UserName
                , Process: a.Process
                , OperationTypeCode: a.OperationTypeCode
                , OperationCategoryName: a.OperationCategoryName
                , OperationActivityName: a.OperationActivityName
                , IsMachineRequired: a.IsMachineRequired
            });
        });
        baseService.setCurrentPage('popUpDataList');
        $scope.popUpUrl = 'machines/operation/getlist?ids=' + baseService.getColumnValueList($scope.modelList, 'OperationId');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.selectByButton = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.modelList, 'OperationId', a.Id)) {
                    $scope.modelList.push({
                        Id: null
                        , OperationId: a.Id
                        , OperationName: a.UserName
                        , Process: a.Process
                        , OperationTypeCode: a.OperationTypeCode
                        , OperationCategoryName: a.OperationCategoryName
                        , OperationActivityName: a.OperationActivityName
                        , IsMachineRequired: a.IsMachineRequired
                        , EntityId: $scope.modelNew.EntityId
                        , NoOfEmployee: 0
                    });
                }
            });
        }
        else
            $scope.modelList = [];
        angular.forEach($scope.modelList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.OperationId))
                $scope.modelList.splice(a, 1);
        });
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    // #endregion Operation
}
