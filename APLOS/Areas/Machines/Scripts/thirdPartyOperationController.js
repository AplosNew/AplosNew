'use strict';
ThirdPartyOperationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ThirdPartyOperationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Third Party";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.thirdParties = [];
    $scope.getListUrl = 'Machines/thirdpartyoperation/getlist';
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'Code');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.thirdParties = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.thirdParty = {
        Id: null,
        Code: null,
        TMU: null,
        IsMachine: null,
        Grouping: null,
        Description: null,
        Active: true
    };
    $scope.thirdPartyNew = Object.assign({}, $scope.thirdParty);

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Time Measurement Unit',
            'value': 'TMU'
        },
        {
            'name': 'Type',
            'value': 'Type'
        },
        {
            'name': 'Grouping',
            'value': 'Grouping'
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.thirdParty = $scope.thirdParties[$scope.index];
        $scope.thirdPartyNew = Object.assign({}, $scope.thirdParty);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.thirdPartyNewForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Machines/thirdpartyoperation/create',
                    data: $scope.thirdPartyNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: 'Machines/thirdpartyoperation/edit',
                    data: $scope.thirdPartyNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.getData();
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.thirdPartyNew.Id)) {
            $http({
                method: 'POST',
                url: 'Machines/thirdpartyoperation/delete/' + $scope.thirdPartyNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.thirdParties.splice($scope.index, 1);
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.thirdParty = {};
        $scope.thirdPartyNew = {};
        $scope.thirdPartyNew.Active = true;
    }
};

