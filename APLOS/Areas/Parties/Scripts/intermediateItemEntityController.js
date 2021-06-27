'use strict';
IntermediateItemEntityController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function IntermediateItemEntityController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.intermediateItemEntityList = [];
    $scope.path = 'Parties/IntermediateItemEntity/getlistWithEntity';
    $scope.selectedEntity = null;
    $scope.getIntermediateItemEntityMasterOnEntityChange = function (entityId) {
        $scope.selectedEntity = entityId;
        $http({
            method: 'GET',
            url: 'Parties/IntermediateItemEntity/GetListWithEntity?entityId=' + entityId
        }).then(function successCallback(response) {
            $scope.intermediateItemEntityList = response.data.Rows;
            if ($scope.intermediateItemEntityList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };
    $scope.entityList = [];
    cboService.getCboEntityByCompanyGroup(null, function (result) {
        $scope.entityList = result;
    });

    //IntermediateItemEntityList for modal
    $scope.ShowIntermediateItemList = function () {
        $scope.searchByList = [
            {
                'name': 'Code',
                'value': 'Code'
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
        if ($scope.selectedEntity == null) {
            return ShowResult('Please at first select entity......', 'failure');
        }
        baseService.init('Parties/IntermediateItem/getlist', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.entityId = $scope.selectedEntity;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.intermediateItemList = result.Rows;
                    angular.forEach($scope.intermediateItemEntityList, function (item) {
                        for (var i = 0; i < $scope.intermediateItemList.length; i++) {
                            if ($scope.intermediateItemList[i]['IntermediateItemId'] == item.IntermediateItemId) {
                                $scope.intermediateItemList.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#intermediateItemPopUp')).modal('show');
        $scope.getData();
    };
    //End IntermediateItemEntityList for modal
    //Passing Data For IntermediateItemEntity List
    $scope.IntermediateItemSelectdCloseListPopUp = function () {
        angular.forEach($scope.intermediateItemList, function (item) {
            if (item.Flag) {
                $scope.intermediateItemEntityList.push(
                    {
                        Id: null,
                        EntityId: $scope.selectedEntity,
                        IntermediateItemId: item.Id,
                        Code: item.Code,
                        UserName: item.UserName,
                        StandardName: item.StandardName,
                        Flag: item.Flag,
                        Archive: item.Archive,
                        Active: item.Active
                    }
                );
            }
        });
        angular.element(document.querySelector('#intermediateItemPopUp')).modal('hide');
        if ($scope.intermediateItemEntityList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.hasDuplicate = function (list) {
        for (var i = 0; i < list.length; i++) {
            for (var x = i + 1; x < list.length; x++) {
                if (list[i].IntermediateItemId == list[x].IntermediateItemId) {
                    throw list[i].UserName + " has duplicate row";
                }
            }
        }
    };
    $scope.Save = function () {
        try {
            $scope.hasDuplicate($scope.intermediateItemEntityList);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Parties/IntermediateItemEntity/create',
                    data: { 'intermediateItemEntity': $scope.intermediateItemEntityList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getIntermediateItemEntityMasterOnEntityChange($scope.selectedEntity);
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //Deleting Rows from IntermediateItemEntityList
    $scope.valuePassInDelModal = function (index, IntermediateItemId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.IntermediateItemId = IntermediateItemId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteIntermediateItemEntityList = function () {
        $scope.intermediateItemEntityList.splice($scope.index, 1);
        $scope.id = null;
        $scope.index = null;
        $scope.IntermediateItemId = null;
        if ($scope.intermediateItemEntityList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //
}