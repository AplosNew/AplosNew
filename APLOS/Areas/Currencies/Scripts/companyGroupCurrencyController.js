'use strict';
CompanyGroupCurrencyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function CompanyGroupCurrencyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $scope.tableShow = false;
    $scope.companyGroupList = [];

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.currency = {
        Id: null,
        CompanyGroupId: null,
        CompanyGroupCurrencyId: null,
        CurrencyId: null,
        CurrencyName: null,
        IsNeglateDecimal: false,
        Active: true
    };

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: 'currencies/companygroupcurrency/create',
            data: $scope.groupCurrencyList,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.getData = function () {
        $http.get('currencies/companygroupcurrency/getcurrencysearch?comanygroupid=' + $scope.currency.CompanyGroupId)
            .then(function (response) {
                $scope.groupCurrencyList = response.data.Rows;
                if ($scope.groupCurrencyList.length > 0)
                    $scope.tableShow = true;
                else
                    $scope.tableShow = false;
            });
    };

    // #region Currency Popup
    $scope.currencies = [];
    $scope.currencyList = [];
    $scope.groupCurrencyList = [];

    $scope.currencyParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Name",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.currencyPopUp = function () {
        $scope.getCurrencyData = function (pageno) {
            $scope.getCurrencyUrl = 'currencies/currency/searchcurrencylist?currencyids=' + isCurrencyIdExistInComG($scope.groupCurrencyList);
            baseService.paginationBase($scope.getCurrencyUrl, pageno, $scope.currencyParameters)
                .then(function (result) {
                    $scope.currencies = result.Rows;
                    $scope.currencyParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.currencyList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.currencyList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#currencyPopUp')).modal('show');
        $scope.getCurrencyData();
    };
    $scope.CloseCurrencyPopUp = function () {
        angular.element(document.querySelector('#currencyPopUp')).modal('hide');
    };
    function isCurrencyIdExistInComG(list) {
        $scope.currencyIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.currencyIds.push(list[i]['CurrencyId']);
                }
            }
        }
        return JSON.stringify($scope.currencyIds);
    }
    $scope.addCurrency = function () {
        if (!isRowSelected($scope.currencies)) {
            ShowResult('Please select at least one row...!', 'failure', 'currencyPopUp');
            return;
        }
        angular.forEach($scope.currencies, function (a) {
            if (a.Flag) {
                $scope.groupCurrencyList.push({
                    Id: null,
                    CompanyGroupId: $scope.currency.CompanyGroupId,
                    CurrencyId: a.Id,
                    Code: a.Code,
                    CurrencyName: a.Name,
                    LargeUnit: a.LargeUnit,
                    SmallUnit: a.SmallUnit,
                    InWordFormat: a.InWordFormat,
                    IsNeglateDecimal: false,
                    Active: true,
                    Archive: false
                });
            }
        });
        if (!$scope.tableShow)
            $scope.tableShow = true;
        $scope.CloseCurrencyPopUp();
    };
    function isRowSelected(ilst) {
        try {
            var flag = false;
            for (var i = 0; i < ilst.length; i++) {
                if (ilst[i].Flag) {
                    return flag = true;
                }
            }
        } catch (e) {
        }
    }
    // #endregion

    $scope.valuePassInDelModal = function (currencyId, name, index) {
        $scope.message_confirmation = '';
        $scope.index = index;
        $scope.currencyId = currencyId;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + name + ' ]';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.groupCurrencyList.length; i++) {
            if ($scope.groupCurrencyList[i].Id === null && $scope.groupCurrencyList[i].CurrencyId === $scope.currencyId) {
                $scope.groupCurrencyList.splice(i, 1);
            }
            else if ($scope.groupCurrencyList[i].Id !== null && $scope.groupCurrencyList[i].CurrencyId === $scope.currencyId)
                $scope.groupCurrencyList[i].Archive = true;
        }
        if ($scope.groupCurrencyList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
        $scope.currencyId = '';
        $scope.index = -1;
    };

    $scope.Clear = function () {
        $scope.currency.CompanyGroupId = null;
        $scope.groupCurrencyList = [];
    }
}