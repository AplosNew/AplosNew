'use strict';
SFGMovementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SFGMovementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'SFG Movement';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SFGMovements = [];
    $scope.SFGInventoryList = [];
    $scope.processCboList = [];
    $scope.path = 'products/SFGMovement/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.SFGMovements = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.SFGMovement = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        FromProcessId: null,
        FromSFGInventoryId: null,
        ToProcessId: null,
        ToSFGInventoryId: null,

        From: true,
        To: true
    };

    $scope.SFGMovementNew = Object.assign({}, $scope.SFGMovement);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.SFGMovementNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    cboService.getSFGInventoryCbo(function (result) {
        $scope.SFGInventoryList = result;
    });

    cboService.getProcessCbo(function (result) {
        $scope.processCboList = result;
    });

    $scope.ShowFormProcess = true;
    $scope.ShowToProcess = true;

    $scope.ShowFormSFG = false;
    $scope.ShowToSFG = false;

    $scope.ChangeFromProcess = function () {
        $scope.SFGMovementNew.From = true;

        if ($scope.SFGMovementNew.From) {
            $scope.ShowFormProcess = true;
            $scope.ShowFormSFG = false;
            $scope.SFGMovementNew.FromSFGInventoryId = null;
        }
    };

    $scope.ChangeFromSFG = function () {
        $scope.SFGMovementNew.From = false;

        if (!$scope.SFGMovementNew.From) {
            $scope.ShowFormSFG = true;
            $scope.ShowFormProcess = false;
            $scope.SFGMovementNew.FromProcessId = null;
        }
    };

    $scope.ChangeToProcess = function () {
        $scope.SFGMovementNew.To = true;

        if ($scope.SFGMovementNew.To) {
            $scope.ShowToProcess = true;
            $scope.ShowToSFG = false;
            $scope.SFGMovementNew.ToSFGInventoryId = null;
        }
    };

    $scope.ChangeToSFG = function () {
        $scope.SFGMovementNew.To = false;

        if (!$scope.SFGMovementNew.To) {
            $scope.ShowToSFG = true;
            $scope.ShowToProcess = false;
            $scope.SFGMovementNew.ToProcessId = null;
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.SFGMovement = $scope.SFGMovements[$scope.index];
        $scope.SFGMovementNew = Object.assign({}, $scope.SFGMovement);

        if (!baseService.isUndefinedOrNull($scope.SFGMovementNew.FromProcessId)) {
            $scope.SFGMovementNew.From = true;
            $scope.ShowFormProcess = true;
            $scope.ShowFormSFG = false;
        }

        if (!baseService.isUndefinedOrNull($scope.SFGMovementNew.FromSFGInventoryId)) {
            $scope.SFGMovementNew.From = false;
            $scope.ShowFormSFG = true;
            $scope.ShowFormProcess = false;
        }

        if (!baseService.isUndefinedOrNull($scope.SFGMovementNew.ToProcessId)) {
            $scope.SFGMovementNew.To = true;
            $scope.ShowToProcess = true;
            $scope.ShowToSFG = false;
        }

        if (!baseService.isUndefinedOrNull($scope.SFGMovementNew.ToSFGInventoryId)) {
            $scope.SFGMovementNew.To = false;
            $scope.ShowToSFG = true;
            $scope.ShowToProcess = false;
        }

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.SFGMovementNew, $scope.SFGMovement);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SFGMovementNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.SFGMovement,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.SFGMovements.push(response.data.SFGMovement);
                        $scope.SFGMovements = $filter('orderBy')($scope.SFGMovements, 'Sequence');
                        baseService.paginationAdd();
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
                    data: $scope.SFGMovement,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.SFGMovements[$scope.index] = $scope.SFGMovement;
                            $scope.SFGMovements = $filter('orderBy')($scope.SFGMovements, 'Sequence');
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SFGMovementNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.SFGMovementNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SFGMovements.splice($scope.index, 1);
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
        $scope.SFGMovement = {};
        $scope.SFGMovementNew = { Active: true };
        $scope.GetSequence();

        $scope.ShowFormProcess = true;
        $scope.ShowToProcess = true;

        $scope.ShowFormSFG = false;
        $scope.ShowToSFG = false;

        $scope.SFGMovementNew.From = true;
        $scope.SFGMovementNew.To= true;
    }
}