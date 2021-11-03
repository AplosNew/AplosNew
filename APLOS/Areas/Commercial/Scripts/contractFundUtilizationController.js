'use strict';
contractFundUtilizationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function contractFundUtilizationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Fund';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Commercial/LCFundUtilization/';
    $scope.getFundUtilizationListUrl = $scope.path + 'GetFundUtilizationList';
    $scope.getBuyerDeductionListUrl = $scope.path + 'GetBuyerDeductionList';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.saveBuyerDeductionUrl = $scope.path + 'CreateBuyerDeduction';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "FundUtilization"; $scope.search = "";
    $scope.searchByList = [{ value: 'FundUtilization', name: "FundUtilization" }, { value: 'FundUtilizationText', name: "FundUtilization Text" }];

    $scope.ModelTemp = {
        FundUtilization: null, FundUtilizationText: null, Percentage: null, CurrencyId: null, UtilizationSourceType: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.currencyList = [];
    cboService.getCompanyGroupCurrencyCbo($window.companyGroupId, function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
    });

    $scope.fundUtilizationList = [];
    cboService.getEnumCbo("enum/GetFundUtilizationEnumCbo", function (result) {
        $scope.fundUtilizationList = result;

        $scope.getFundUtilizationData();
    });

    $scope.buyerDeductionList = [];
    cboService.getEnumCbo("enum/GetBuyerDeductionEnumCbo", function (result) {
        $scope.buyerDeductionList = result;

        $scope.getBuyerDeductionData();
    });

    $scope.getFundUtilizationData = function () {
        $http({
            method: 'POST',
            url: $scope.getFundUtilizationListUrl,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.ModelList = response.data;
                for (var i = 0; i < $scope.ModelList.length; i++) {
                    for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
                        if ($scope.ModelList[i].FundUtilization == $scope.fundUtilizationList[j].Text) {

                            $scope.fundUtilizationList[j].FundUtilizationText = $scope.ModelList[i].FundUtilizationText;
                            $scope.fundUtilizationList[j].Percentage = $scope.ModelList[i].Percentage;
                            $scope.fundUtilizationList[j].CurrencyId = $scope.ModelList[i].CurrencyId;
                        }
                    }
                }
            }
        });
    };

    $scope.dataList = [];
    $scope.getBuyerDeductionData = function () {
        $http({
            method: 'POST',
            url: $scope.getBuyerDeductionListUrl,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.dataList = response.data;
                for (var i = 0; i < $scope.dataList.length; i++) {
                    for (var j = 0; j < $scope.buyerDeductionList.length; j++) {
                        if ($scope.dataList[i].FundUtilization == $scope.buyerDeductionList[j].Text) {

                            $scope.buyerDeductionList[j].FundUtilizationText = $scope.dataList[i].FundUtilizationText;
                            $scope.buyerDeductionList[j].Percentage = $scope.dataList[i].Percentage;
                            $scope.buyerDeductionList[j].CurrencyId = $scope.dataList[i].CurrencyId;
                        }
                    }
                }
               
            }
        });
    };

    $scope.getPercentageValue = function () {
        var negotiable = 0;
        for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
            if (!baseService.isUndefinedOrNull($scope.fundUtilizationList[j].Percentage) && $scope.fundUtilizationList[j].Text !== 'Negotiable') {
                negotiable += $scope.fundUtilizationList[j].Percentage;
            }
        }
        for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
            if ($scope.fundUtilizationList[j].Text === 'Negotiable') {
                $scope.fundUtilizationList[j].Percentage = 100 - negotiable;
            }
        }
    }

    $scope.Save = function () {
        try {

            $scope.saveList = [];
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm1.$valid) {

                for (var i = 0; i < $scope.fundUtilizationList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.fundUtilizationList[i].FundUtilizationText)) {
                        throw "Fund Utilization text is required.";
                    }

                }

                for (var i = 0; i < $scope.fundUtilizationList.length; i++) {
                    $scope.fundUtilizationList[i].FundUtilization = $scope.fundUtilizationList[i].Text;
                    $scope.saveList.push($scope.fundUtilizationList[i]);
                }

                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.saveList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getFundUtilizationData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SaveBuyerDeduction = function () {
        try {

            $scope.saveBuyerList = [];
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm2.$valid) {

                for (var i = 0; i < $scope.buyerDeductionList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.buyerDeductionList[i].FundUtilizationText)) {
                        throw "Buyer Deduction text is required.";
                    }

                }

                for (var i = 0; i < $scope.buyerDeductionList.length; i++) {
                    $scope.buyerDeductionList[i].FundUtilization = $scope.buyerDeductionList[i].Text;
                    $scope.saveBuyerList.push($scope.buyerDeductionList[i]);
                }

                $http({
                    method: 'POST',
                    url: $scope.saveBuyerDeductionUrl,
                    data: { 'data': $scope.saveBuyerList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.getData();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

}