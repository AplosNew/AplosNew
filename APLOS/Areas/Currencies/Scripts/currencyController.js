'use strict';
CurrencyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CurrencyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Currency";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.currencies = [];
    $scope.path = 'currencies/currency/';
    $scope.getListUrl = $scope.path + 'getcurrencylist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.currencies = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.currency = {
        Id: null
        , Sequence: 0
        , Code: null
        , Name: null
        , LargeUnit: null
        , UserName: null
        , SmallUnit: null
        , InWordFormate: null
        , Precision: 1
        , Remarks: null
        , Description: null
        , Active: true
    };

    function createNumberList() {
        $scope.numberList = [];
        for (var i = 0; i < 4; i++) {
            $scope.numberList.push({
                'Text': i,
                'Value': i
            });
        }
    };
    createNumberList();
    $rootScope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Name',
            'value': 'Name'
        },
        {
            'name': 'Large Unit',
            'value': 'LargeUnit'
        },
        {
            'name': 'In Word Format',
            'value': 'InWordFormat'
        },
        {
            'name': 'Active',
            'value': 'Active'
        }
    ];
    $rootScope.parameters.searchBy = 'Name';

    $scope.GetSequence = function () {
        $http.get('currencies/currency/getautosequence')
            .then(function (response) {
                $scope.currency.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.currency = $scope.currencies[$scope.index];
        $scope.currency.AddedDate = $filter('dateFilter')($scope.currency.AddedDate);
        $scope.currency.UpdatedDate = $filter('dateFilter')($scope.currency.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.currencyForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'currencies/currency/create',
                    data: $scope.currency,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.currencies.push(response.data.Currency);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'currencies/currency/edit',
                    data: $scope.currency,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.currencies[$scope.index] = $scope.currency;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.currency.Id)) {
            $http({
                method: 'POST',
                url: 'currencies/currency/delete/' + $scope.currency.Id,
                datatype: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.currencies.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.currency = { Sequence: seq, Precision: 1, Active: true };
        createNumberList();
    }
}