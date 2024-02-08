'use strict';
loanLedgerReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$window'];
function loanLedgerReportController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = 'Loan Register';
    $scope.path = 'accounts/loan/';
    $scope.glvoucherXLUrl = $scope.path + 'LoanLedgerReport';
    $scope.partyledgerreportXLUrl = $scope.path + 'LoanLedgerReport';
    $scope.generalVoucherXLUrl = 'accounts/loan/LoanLedgerReport';
    $scope.partyType = 'Customer';
    $scope.isAdvance = false;
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.report = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        GLGeneralInfoId: '',
        GLNameCode: null,
        customerNameCode: null,
        Active: false,
        PartyId: null,
        VendorId: null,
        Code: null,
        Party: null,
        PartyPlantId: null,
        PartyPlantName: null,
        GSTIN: null,
        ReportFormat: 'Pdf',
        PartyType: 'Customer',
        TransactionType: 'LoanTaken'
    };

   
    $scope.loanDataList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/Loan/GetLoanRegisterList?transactionType=' + $scope.report.TransactionType
        }).then(function successCallback(response) {
            $scope.loanDataList = response.data;
            for (var i = 0; i < $scope.loanDataList.length; i++) {
                response.data[i].PostingDateNew = new Date($scope.loanDataList[i].PostingDateNew);
                response.data[i].DocDate = new Date($scope.loanDataList[i].DocDate);
            }
        });
    };
    $scope.showloanPopUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#loanPopUp')).modal('show');
    };
    $scope.closeloanPopUp = function () {
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };
   
    $scope.closeloanPopUpSelected = function (x) {
        var data = x.data;
        $scope.report.FinancingId = data.FinancingId;
        $scope.report.FinancingDetailId = data.FinancingDetailId;
        $scope.report.FinancingTypeId = data.FinancingTypeId;
        $scope.report.VoucherNo = data.VoucherNo;
        $scope.report.LoanNo = data.DocRefNo;
        $scope.report.Particulars = data.Particulars;
        $scope.report.CompanyId = data.CompanyId;
        $scope.report.PlantId = data.PlantId;
        $scope.report.PartyType = data.PartyType;
        $scope.report.LoanAmount = data.LoanAmount;
        $scope.report.LoanSetOff = data.LoanPayment;
        $scope.report.Balance = data.Balance;
        $scope.report.InterestAmount = data.InterestAmount;
        $scope.report.InterestWriteOff = data.InterestWriteOff;
        $scope.report.InterestBalance = data.InterestBalance;
        $scope.report.InterestCashPayment = data.InterestCashPayment;
        $scope.report.LoanDocRefNo = data.DocRefNo;
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.set();
        $scope.selectedCode = x.GLGeneralInfoCode;
    };

    $scope.cOAICodeListt = null;
    $scope.getAccountCodeby = function (keyEvent, accountcode) {
        if (keyEvent.which === 13)
            $http({
                method: 'GET',
                url: 'accounts/glitem/getglbyaccountcode?accountcode=' + accountcode
            }).then(function (result) {
                $scope.cOAICodeListt = result.data;
                $scope.report.COAICode = $scope.cOAICodeListt['0']['Value'];
                $scope.report.COAIText = $scope.cOAICodeListt['0']['Text'];
            }, function () {
            });
    };

    $scope.changePartyType = function () {
        $scope.partyType = $scope.report.PartyType;
        $scope.customerNameCode = null;
        $scope.GLNameCode = null;
    };

    $scope.ClearCode = function () {
        $scope.GLNameCode = null;
    };

    $scope.getReport = function () {
        var url = 'accounts/loan/GetLoanLedgerReport?reportFormat=' + $scope.report.ReportFormat + '&transactionType=' + $scope.report.TransactionType + '&voucherId=' + $scope.report.VoucherId + '&financingId=' + $scope.report.FinancingId;
        $window.open(url, '_blank');
    };

    $scope.getLoanRegisterReport = function () {
        var url = 'accounts/loan/GetLoanRegisterLedgerReport?reportFormat=' + $scope.report.ReportFormat + '&transactionType=' + $scope.report.TransactionType + '&voucherId=' + $scope.report.VoucherId + '&financingId=' + $scope.report.FinancingId;
        $window.open(url, '_blank');
    };
    //All Register Report
    $scope.GetAllRegisterReportExcel = function () {
        var url = 'accounts/loan/GetAllRegisterReportExcel?transactionType=' + $scope.report.TransactionType;
        $window.open(url, '_blank');
    };

    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    }; 

    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure');
        }
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.getPartyGl = function () {
        if (!baseService.isUndefinedOrNull($scope.report.PartyType)) {
            $scope.title2 = '';
            $scope.popUp2Url = '';
            $scope.popUp2List = [];
            $scope.popUp2DataList = [];
            $scope.valueData = '';
            $scope.sort = '';
            $scope.searchBy = '';
            if ($scope.report.PartyType === 'Customer') {
                $scope.title2 = 'Customer GL';
                $scope.sort = 'GLGeneralInfoCode';
                $scope.searchBy = 'GLGeneralInfoName';
                $scope.popUp2Url = 'Accounts/GLItem/GetPartyDebitGLAccountCode';
            }
            else if ($scope.report.PartyType === 'Vendor') {
                $scope.title2 = 'Vendor GL';
                $scope.sort = 'GLGeneralInfoCode';
                $scope.searchBy = 'GLGeneralInfoName';
                $scope.popUp2Url = 'Accounts/GLItem/GetPartyDebitGLAccountCode';
            }
            else {
                $scope.title2 = 'Party';
                $scope.sort = 'Party';
                $scope.searchBy = 'Party';
                $scope.popUp2Url = 'Parties/party/getpartytrngl?partyId=' + $scope.report.MainPartyId;
            }

            $scope.popUp2Parameters = {
                limit: 10,
                offset: 0,
                order: 'asc',
                sort: $scope.sort,
                searchBy: $scope.searchBy,
                pageSize: 10,
                total_count: 0,
                search: null,
                serverPagination: true
            };
            $scope.popUp2();
        }
    };

    $scope.popUp2 = function () {
        baseService.setCurrentPage('dataList');
        $scope.getPopUp2Data = function (pageno) {
            baseService.paginationBase($scope.popUp2Url, pageno, $scope.popUp2Parameters)
                .then(function (result) {
                    $scope.popUp2DataList = result.Rows;
                    $scope.popUp2Parameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUp2List) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUp2List);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUp2Id');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUp2Id')).modal('show');
        $scope.getPopUp2Data();
    };

    $scope.selectDoubleClick2 = function (data) {
        if ($scope.report.PartyType === 'Customer') {
            $scope.report.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.GLNameCode = data.GLGeneralInfoCode + ' - ' + data.GLGeneralInfoName;
        }
        else if ($scope.report.PartyType === 'Vendor') {
            $scope.report.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.GLNameCode = data.GLGeneralInfoCode + ' - ' + data.GLGeneralInfoName;
        }
        else {
            $scope.report.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.GLNameCode = data.GLGeneralInfoCode + ' - ' + data.GLGeneralInfoName;
        }
        $scope.closePopUp2();
    };

    $scope.selectSingleClick2 = function (data) {
        $scope.valueData2 = data;
    };

    $scope.selectByButton2 = function () {
        if (baseService.isUndefinedOrNull($scope.valueData2)) {
            return ShowResult('Please at first select row', 'failure');
        }
        $scope.selectDoubleClick2($scope.valueData2);
        $scope.closePopUp2();
    };

    $scope.closePopUp2 = function () {
        $scope.valueData2 = '';
        angular.element(document.querySelector('#popUp2Id')).modal('hide');
    };
}