fiscalYearBaseController.$inject = ['$scope', '$http', '$filter', 'baseService', 'cboService', 'commonMessage'];
function fiscalYearBaseController($scope, $http, $filter, baseService, cboService, commonMessage) {
    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date)) {
            $http({
                method: 'get',
                url: 'accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=' + $filter('dateFiltering')(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, 'failure');
                            $scope.voucher.PostingDate = '';
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, 'failure');
                            $scope.voucher.PostingDate = '';
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                        }
                        else {
                            $scope.voucher.FiscalYearId = result.FiscalYearId;
                            $scope.voucher.FiscalYearName = result.FiscalYearName;
                            $scope.voucher.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.voucher.FiscalYearPeriodName = result.PeriodName;
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
    };
}