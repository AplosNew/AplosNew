'use strict';
SFGInventoryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SFGInventoryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'SFG Inventory';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SFGInventories = [];
    $scope.path = 'products/sfginventory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.SFGInventories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.SFGInventory = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        ProcessId: null,
        IsFirst: false,
        IsLast: false,
        IsCrossAllowed: false

    };

    $scope.SFGInventoryNew = Object.assign({}, $scope.SFGInventory);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.SFGInventoryNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
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
        },
        {
            'name': 'Owner Process',
            'value': 'OwnerProcess'
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.SFGInventory = $scope.SFGInventories[$scope.index];
        $scope.SFGInventoryNew = Object.assign({}, $scope.SFGInventory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.processList = [];
    cboService.getProcessCbo(function (result) {
        $scope.processList = result;
    });

    $scope.Save = function () {
        angular.copy($scope.SFGInventoryNew, $scope.SFGInventory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SFGInventoryNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.SFGInventory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.SFGInventories.push(response.data.SFGInventory);
                        $scope.SFGInventories = $filter('orderBy')($scope.SFGInventories, 'Sequence');
                        baseService.paginationAdd();
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.SFGInventory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.SFGInventories[$scope.index] = $scope.SFGInventory;
                            $scope.SFGInventories = $filter('orderBy')($scope.SFGInventories, 'Sequence');
                        }
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SFGInventoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.SFGInventoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SFGInventories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields()
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.SFGInventory = {};
        $scope.SFGInventoryNew = { Active: true };
        $scope.GetSequence();
    }
}