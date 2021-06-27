'use strict';
partyOpeningBalanceLedgerController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', 'cboService', '$controller', "$window"];
function partyOpeningBalanceLedgerController(commonMessage, $scope, $rootScope, baseService, $http, cboService, $controller, $window) {
    $rootScope.title = 'Party Opening Balance Ledger';
    $scope.parallelCurrencyList = [];
    $scope.partyType = 'Customer';
    $scope.isAdvance = false;
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.report = {
        GLGeneralInfoId: null,
        Code: null,
        Party: null,
        GLNameCode: null,
        MainPartyId: null,
        PartyId: null,
        VendorId: null,
        customerNameCode: null,
        Active: false,
        PartyPlantId: null,
        PartyPlantName: null,
        FiscalYear: null,
        FiscalYearId: null,
        ReportFormat: 'Pdf'
    };

    $scope.fiscalYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/FiscalYear/GetCbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.glList = [];
    $scope.searchglByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL',
            'value': 'GLItem'
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: 'GLItem',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function (index) {
        $scope.AlternativeCoaList = [];
        $scope.rowSelectedIndex = index;
        $scope.GLUrl = 'accounts/glitem/getvendorinvoicegllist';
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.glList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GLPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector('#GLPopUp')).modal('hide');
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#GLPopUp')).modal('hide');
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
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

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.customerNameCode = null;
        $scope.GLNameCode = null;
        $scope.FiscalYear = null;
    }

    $scope.ClearCode = function () {
        $scope.GLNameCode = null;
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.PartyId)) {
            manualValidation('div_Party', true, "Party is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.FiscalYearId)) {
            manualValidation('div_FY', true, "Fiscal Year is required.");
        }
        else {
            var url = 'Parties/PartyReport/GetPartyOpeningBalanceLedgerReport?reportFormat=' + $scope.report.ReportFormat + '&fiscalYearId=' + $scope.report.FiscalYearId + '&partyId=' + $scope.report.PartyId;
            if (!baseService.isUndefinedOrNull($scope.report.PartyPlantId)) {
                url += '&partyPlantId=' + $scope.report.PartyPlantId;
            }
            if (!baseService.isUndefinedOrNull($scope.report.GLGeneralInfoId)) {
                url += '&glId=' + $scope.report.GLGeneralInfoId;
            }
            $window.open(url, '_blank');
        }
    };

    $scope.closePartyPopUp = function myfunction() {
        if ($scope.partyIndex !== -1) {
            var data = $scope.partyList[$scope.partyIndex];
            if ($scope.partyType === 'Customer') {
                $scope.report.vendorId = null;
                $scope.report.PartyId = data.PartyId;
                $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
                $scope.getPartyPlantList($scope.report.PartyId);
            }
            else if ($scope.partyType === 'Vendor') {
                $scope.report.PartyId = null;
                $scope.report.PartyId = data.PartyId;
                $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
                $scope.getPartyPlantList($scope.report.PartyId);
            }
            else {
                $scope.report.PartyId = null;
                $scope.report.vendorId = null;
                $scope.report.MainPartyId = data.PartyId;
                $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
            }
        }
        $scope.hidePartyPopUp();
    };

    $scope.getPartyPlantList = function (partyId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                });
            });
    };

    $scope.popUp = function () {
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
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
        if (!baseService.isUndefinedOrNull($scope.partyType)) {
            $scope.title2 = '';
            $scope.popUp2Url = '';
            $scope.popUp2List = [];
            $scope.popUp2DataList = [];
            $scope.valueData = '';
            $scope.sort = '';
            $scope.searchBy = '';
            if ($scope.partyType === 'Customer') {
                $scope.title2 = 'Customer GL';
                $scope.sort = 'GLGeneralInfoCode';
                $scope.searchBy = 'GLGeneralInfoName';
                $scope.popUp2Url = 'Accounts/GLItem/GetPartyDebitGLAccountCode';
            }
            else if ($scope.partyType === 'Vendor') {
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
        if ($scope.partyType === 'Customer') {
            $scope.report.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.GLNameCode = data.GLGeneralInfoCode + ' - ' + data.GLGeneralInfoName;
        }
        else if ($scope.partyType === 'Vendor') {
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