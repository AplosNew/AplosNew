'use strict';
exchangeGainLossController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function exchangeGainLossController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Exchange Gain Loss';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.exchangeGainLosses = [];
    $scope.CompanyCurrencyGLList = [];
    $scope.CompanyGroupCurrencyGLList = [];
    $scope.HardCurrencyGLList = [];
    $scope.CompanyCurrencyLossGLList = [];
    $scope.exchangeGainList = [];
    $scope.exchangeLossList = [];
    $scope.CompanyGroupCurrencyLossGLList = [];
    $scope.HardCurrencyLossGLList = [];
    $scope.exchangeGainLossList = [];
    $scope.path = 'accounts/exchangegainloss/';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.COAId = null;
    $scope.SourceType = null;

    $scope.exchangeGain = {
        Id: null,
        ExchangeStatus: null,
        CompanyCurrencyGLId: null,
        CompanyGroupCurrencyGLId: null,
        HardCurrencyGLId: null,
        COAId: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null
    };

    $scope.exchangeLoss = {
        Id: null,
        ExchangeStatus: null,
        CompanyCurrencyGLId: null,
        CompanyGroupCurrencyGLId: null,
        HardCurrencyGLId: null,
        COAId: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null
    };

    $scope.CompanyCurrencyGLGainCode = null;
    $scope.CompanyCurrencyGLGainText = null;
    $scope.CompanyCurrencyBudgetGain = null;
    $scope.CompanyCurrencyActivityNameGain = null;
    $scope.CompanyCurrencyBudgetLoss = null;
    $scope.CompanyGroupCurrencyGLGainCode = null;
    $scope.CompanyGroupCurrencyGLGainText = null;
    $scope.HardCurrencyGLGainCode = null;
    $scope.HardCurrencyGLGainText = null;

    $scope.CompanyCurrencyGLLossCode = null;
    $scope.CompanyCurrencyGLLossText = null;
    $scope.CompanyCurrencyBudgetLoss = null;
    $scope.CompanyCurrencyActivityNameLoss = null;
    $scope.CompanyGroupCurrencyGLLossCode = null;
    $scope.CompanyGroupCurrencyGLLossText = null;
    $scope.HardCurrencyGLLossCode = null;
    $scope.HardCurrencyGLLossText = null;

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.OnChangeCompany = function () {
        $scope.getExchangeGain();
        $scope.getExchangeLoss();
    };

    $scope.getExchangeGain = function () {
        $http.get('accounts/ExchangeGainLoss/ExchangeGain?coaId=' + $scope.COAId + '&sourceType=' + $scope.SourceType)
            .then(
            function successCallback(response) {
                $scope.exchangeGainList = response.data.Rows[0];
                if ($scope.exchangeGainList !== null && $scope.exchangeGainList !== undefined) {
                    $scope.exchangeGain.Id = $scope.exchangeGainList.Id;
                    $scope.exchangeGain.CompanyCurrencyGLId = $scope.exchangeGainList.CompanyCurrencyGLId;
                    $scope.CompanyCurrencyGLGainText = $scope.exchangeGainList.CompanyCurrencyGLGainText;
                    $scope.CompanyCurrencyGLGainCode = $scope.exchangeGainList.CompanyCurrencyGLGainCode;
                    $scope.CompanyCurrencyBudgetGain = $scope.exchangeGainList.CompanyCurrencyBudgetGain;
                    $scope.CompanyCurrencyActivityNameGain = $scope.exchangeGainList.CompanyCurrencyActivityNameGain;
                    $scope.exchangeGain.CompanyGroupCurrencyGLId = $scope.exchangeGainList.CompanyGroupCurrencyGLId;
                    $scope.CompanyGroupCurrencyGLGainText = $scope.exchangeGainList.CompanyGroupCurrencyGLGainText;
                    $scope.CompanyGroupCurrencyGLGainCode = $scope.exchangeGainList.CompanyGroupCurrencyGLGainCode;
                    $scope.exchangeGain.HardCurrencyGLId = $scope.exchangeGainList.HardCurrencyGLId;
                    $scope.HardCurrencyGLGainText = $scope.exchangeGainList.HardCurrencyGLGainText;
                    $scope.HardCurrencyGLGainCode = $scope.exchangeGainList.HardCurrencyGLGainCode;
                    $scope.CompanyCurrencyGLGain = $scope.CompanyCurrencyGLGainCode + " " + $scope.CompanyCurrencyGLGainText;
                    $scope.CompanyGroupCurrencyGLGain = $scope.CompanyGroupCurrencyGLGainCode + " " + $scope.CompanyGroupCurrencyGLGainText;
                    $scope.HardCurrencyGLGain = $scope.HardCurrencyGLGainCode + " " + $scope.HardCurrencyGLGainText;
                }
                else {
                    $scope.exchangeGain.Id = null;
                    $scope.exchangeGain.CompanyCurrencyGLId = null;
                    $scope.CompanyCurrencyGLGainText = null;
                    $scope.CompanyCurrencyGLGainCode = null;
                    $scope.CompanyCurrencyBudgetGain = null;
                    $scope.CompanyCurrencyActivityNameGain = null;
                    $scope.exchangeGain.CompanyGroupCurrencyGLId = null;
                    $scope.CompanyGroupCurrencyGLGainText = null;
                    $scope.CompanyGroupCurrencyGLGainCode = null;
                    $scope.exchangeGain.HardCurrencyGLId = null;
                    $scope.HardCurrencyGLGainText = null;
                    $scope.HardCurrencyGLGainCode = null;
                    $scope.CompanyCurrencyGLGain = null;
                    $scope.CompanyGroupCurrencyGLGain = null;
                    $scope.HardCurrencyGLGain = null;
                }
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    $scope.getExchangeLoss = function () {
        $http.get('accounts/ExchangeGainLoss/ExchangeLoss?coaId=' + $scope.COAId + '&sourceType=' + $scope.SourceType)
            .then(
            function successCallback(response) {
                $scope.exchangeLossList = response.data.Rows[0];
                if ($scope.exchangeLossList !== null && $scope.exchangeLossList !== undefined) {
                    $scope.exchangeLoss.Id = $scope.exchangeLossList.Id;
                    $scope.CompanyCurrencyGLLossText = $scope.exchangeLossList.CompanyCurrencyGLLossText;
                    $scope.CompanyCurrencyGLLossCode = $scope.exchangeLossList.CompanyCurrencyGLLossCode;
                    $scope.exchangeLoss.CompanyCurrencyGLId = $scope.exchangeLossList.CompanyCurrencyGLId;
                    $scope.CompanyGroupCurrencyGLLossCode = $scope.exchangeLossList.CompanyGroupCurrencyGLLossCode;
                    $scope.exchangeLoss.CompanyGroupCurrencyGLId = $scope.exchangeLossList.CompanyGroupCurrencyGLId;
                    $scope.CompanyGroupCurrencyGLLossText = $scope.exchangeLossList.CompanyGroupCurrencyGLLossText;
                    $scope.exchangeLoss.HardCurrencyGLId = $scope.exchangeLossList.HardCurrencyGLId;
                    $scope.HardCurrencyGLLossCode = $scope.exchangeLossList.HardCurrencyGLLossCode;
                    $scope.HardCurrencyGLLossText = $scope.exchangeLossList.HardCurrencyGLLossText;
                    $scope.CompanyCurrencyGLLoss = $scope.CompanyCurrencyGLLossCode + " " + $scope.CompanyCurrencyGLLossText;
                    $scope.CompanyCurrencyBudgetLoss = $scope.exchangeLossList.CompanyCurrencyBudgetLoss;
                    $scope.CompanyCurrencyActivityNameLoss = $scope.exchangeLossList.CompanyCurrencyActivityNameLoss;
                    $scope.CompanyGroupCurrencyGLLoss = $scope.CompanyGroupCurrencyGLLossCode + " " + $scope.CompanyGroupCurrencyGLLossText;
                    $scope.HardCurrencyGLLoss = $scope.HardCurrencyGLLossCode + " " + $scope.HardCurrencyGLLossText;
                }
                else {
                    $scope.exchangeLoss.Id = null;
                    $scope.CompanyCurrencyGLLossText = null;
                    $scope.CompanyCurrencyGLLossCode = null;
                    $scope.exchangeLoss.CompanyCurrencyGLId = null;
                    $scope.CompanyGroupCurrencyGLLossCode = null;
                    $scope.exchangeLoss.CompanyGroupCurrencyGLId = null;
                    $scope.CompanyGroupCurrencyGLLossText = null;
                    $scope.exchangeLoss.HardCurrencyGLId = null;
                    $scope.HardCurrencyGLLossCode = null;
                    $scope.HardCurrencyGLLossText = null;
                    $scope.CompanyCurrencyGLLoss = null;
                    $scope.CompanyCurrencyBudgetLoss = null;
                    $scope.CompanyCurrencyActivityNameLoss = null;
                    $scope.CompanyGroupCurrencyGLLoss = null;
                    $scope.HardCurrencyGLLoss = null;
                }
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    $scope.searchCompanyCurrencyGLByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        },
        {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];

    $scope.companyCurrencyGLListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: 'AccountGroupName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCompanyCurrencyGLList = function () {
        if (baseService.isUndefinedOrNull($scope.COAId)) {
            return ShowResult('Please select COA', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetRevenueExpensesGLBudgetCOAWise?coaId=' + $scope.COAId;
        $scope.GetCompanyCurrencyGLListDatas = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.companyCurrencyGLListParameters)
                .then(function (data) {
                    $scope.CompanyCurrencyGLList = data.Rows;
                    $scope.companyCurrencyGLListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CompanyCurrencyGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCompanyCurrencyGLListDatas();
    };
    $scope.closeCompanyCurrencyGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#CompanyCurrencyGLListPopUp')).modal('hide');
        }
        else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };
    $scope.setComCurrGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.CompanyCurrencyGLGain = x.GLGeneralInfoCode + '-' + x.GLGeneralInfoName;
        $scope.exchangeGain.CompanyCurrencyGLId = x.GLGeneralInfoId;
        $scope.exchangeGain.CompanyCurrencyBudgetMasterId = x.BudgetMasterId;
        $scope.CompanyCurrencyBudgetGain = x.BudgetName;
        $scope.CompanyCurrencyActivityNameGain = x.ActivityName;
        $scope.exchangeGain.CompanyCurrencyActivityId = x.ActivityId;
    };

    $scope.searchCompanyGroupCurrencyGLByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        }, {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];
    $scope.companyGroupCurrencyGLListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: 'AccountGroupName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetCompanyGroupCurrencyGLList = function () {
        if (baseService.isUndefinedOrNull($scope.COAId)) {
            return ShowResult('Please select COA', 'failure');
        }
        $scope.GLUrl2 = 'accounts/glitem/GetRevenueExpensesGLBudgetCOAWise?coaId=' + $scope.COAId;
        $scope.GetCompanyGroupCurrencyGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl2, pageno, $scope.companyGroupCurrencyGLListParameters)
                .then(function (data) {
                    $scope.CompanyGroupCurrencyGLList = data.Rows;
                    $scope.companyGroupCurrencyGLListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CompanyGroupCurrencyGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCompanyGroupCurrencyGLListData();
    };
    $scope.closeCompanyGroupCurrencyGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#CompanyGroupCurrencyGLListPopUp')).modal('hide');
        }
        else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };
    $scope.setComGroupCurrGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.CompanyGroupCurrencyGLGainCode = x.GLGeneralInfoCode;
        $scope.CompanyGroupCurrencyGLGainText = x.GLGeneralInfoName;
        $scope.CompanyGroupCurrencyGLGain = x.GLGeneralInfoCode + '-' + x.GLGeneralInfoName;
        $scope.exchangeGain.CompanyGroupCurrencyGLId = x.GLGeneralInfoId;

        $scope.exchangeGain.CompanyGroupCurrencyBudgetMasterId = x.BudgetMasterId;
        $scope.exchangeGain.CompanyGroupCurrencyActivityId = x.ActivityId;
    };

    $scope.searchHardCurrencyGLByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        }, {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];
    $scope.hardCurrencyGLListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: 'AccountGroupName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetHardCurrencyGLList = function () {
        if (baseService.isUndefinedOrNull($scope.COAId)) {
            return ShowResult('Please select COA', 'failure');
        }
        $scope.GLUrl3 = 'accounts/glitem/GetRevenueExpensesGLBudgetCOAWise?coaId=' + $scope.COAId;
        $scope.GetHardCurrencyGLListDatas = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.hardCurrencyGLListParameters)
                .then(function (data) {
                    $scope.HardCurrencyGLList = data.Rows;
                    $scope.hardCurrencyGLListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#HardCurrencyGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetHardCurrencyGLListDatas();
    };
    $scope.closeHardCurrencyGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#HardCurrencyGLListPopUp')).modal('hide');
        }
        else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };
    $scope.setHardCurrGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.HardCurrencyGLGainCode = x.GLGeneralInfoCode;
        $scope.HardCurrencyGLGainText = x.GLGeneralInfoName;
        $scope.HardCurrencyGLGain = x.GLGeneralInfoName;
        $scope.exchangeGain.HardCurrencyGLId = x.GLGeneralInfoId;
        $scope.exchangeGain.HardCurrencyBudgetMasterId = x.BudgetMasterId;
        $scope.exchangeGain.HardCurrencyActivityId = x.ActivityId;
    };

    $scope.searchCompanyCurrencyGLLossByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        }, {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];

    $scope.companyCurrencyGLLossListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: 'AccountGroupName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCompanyCurrencyGLLossList = function () {
        if (baseService.isUndefinedOrNull($scope.COAId)) {
            return ShowResult('Please select COA', 'failure');
        }
        $scope.GLUrl4 = 'accounts/glitem/GetRevenueExpensesGLBudgetCOAWise?coaId=' + $scope.COAId;
        $scope.GetCompanyCurrencyLossGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl4, pageno, $scope.companyCurrencyGLLossListParameters)
                .then(function (data) {
                    $scope.CompanyCurrencyLossGLList = data.Rows;
                    $scope.companyCurrencyGLLossListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CompanyCurrencyGLLossListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCompanyCurrencyLossGLListData();
    };

    $scope.closeCompanyCurrencyGLLossListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#CompanyCurrencyGLLossListPopUp')).modal('hide');
        }
        else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    $scope.setComCurrLossGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.CompanyCurrencyGLLossCode = x.GLGeneralInfoCode;
        $scope.CompanyCurrencyGLLossText = x.GLGeneralInfoName;
        $scope.CompanyCurrencyGLLoss = x.GLGeneralInfoCode + '-' + x.GLGeneralInfoName;
        $scope.CompanyCurrencyBudgetLoss = x.BudgetName;
        $scope.CompanyCurrencyActivityNameLoss = x.ActivityName;
        $scope.exchangeLoss.CompanyCurrencyGLId = x.GLGeneralInfoId;
        $scope.exchangeLoss.CompanyCurrencyBudgetMasterId = x.BudgetMasterId;
        $scope.exchangeLoss.CompanyCurrencyActivityId = x.ActivityId;
    };

    $scope.searchCompanyGroupCurrencyLossGLByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        }, {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];

    $scope.companyGroupCurrencyLossGLListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: 'AccountGroupName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCompanyGroupCurrencyLossGLList = function () {
        $scope.GLUrl5 = 'accounts/glitem/GetRevenueExpensesGLBudgetCOAWise?coaId=' + $scope.COAId;
        $scope.GetCompanyGroupCurrencyLossGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl5, pageno, $scope.companyGroupCurrencyLossGLListParameters)
                .then(function (data) {
                    $scope.CompanyGroupCurrencyLossGLList = data.Rows;
                    $scope.companyGroupCurrencyLossGLListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CompanyGroupCurrencyGLLossListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCompanyGroupCurrencyLossGLListData();
    };

    $scope.closeCompanyGroupCurrencyGLLossListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#CompanyGroupCurrencyGLLossListPopUp')).modal('hide');
        }
        else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    $scope.setComGroupCurrGLLossSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.CompanyGroupCurrencyGLLossCode = x.GLGeneralInfoCode;
        $scope.CompanyGroupCurrencyGLLossText = x.GLGeneralInfoName;
        $scope.CompanyGroupCurrencyGLLoss = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.exchangeLoss.CompanyGroupCurrencyGLId = x.GLGeneralInfoId;
        $scope.exchangeLoss.CompanyGroupCurrencyBudgetMasterId = x.BudgetMasterId;
        $scope.exchangeLoss.CompanyGroupCurrencyActivityId = x.ActivityId;
    };

    $scope.searchHardCurrencyLossGLByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        }, {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];

    $scope.hardCurrencyLossGLListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: 'AccountGroupName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetHardCurrencyLossGLList = function () {
        if (baseService.isUndefinedOrNull($scope.COAId)) {
            return ShowResult('Please select COA', 'failure');
        }
        $scope.GLUrl6 = 'accounts/glitem/GetRevenueExpensesGLBudgetCOAWise?coaId=' + $scope.COAId;
        $scope.GetHardCurrencyLossGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl6, pageno, $scope.hardCurrencyLossGLListParameters)
                .then(function (data) {
                    $scope.HardCurrencyLossGLList = data.Rows;
                    $scope.hardCurrencyLossGLListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#HardCurrencyLossGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetHardCurrencyLossGLListData();
    };

    $scope.closeHardCurrencyGLLossListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#HardCurrencyLossGLListPopUp')).modal('hide');
        }
        else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    $scope.setHardCurrLossGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.HardCurrencyGLLossCode = x.GLGeneralInfoCode;
        $scope.HardCurrencyGLLossText = x.GLGeneralInfoName;
        $scope.HardCurrencyGLLoss = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.exchangeLoss.HardCurrencyGLId = x.GLGeneralInfoId;
        $scope.exchangeLoss.HardCurrencyBudgetMasterId = x.BudgetMasterId;
        $scope.exchangeLoss.HardCurrencyActivityId = x.ActivityId;
    };

    $scope.ExchangeGainLossList = function () {
        $scope.exchangeGain.ExchangeStatus = 'ExchangeGain';
        $scope.exchangeLoss.ExchangeStatus = 'ExchangeLoss';
        $scope.exchangeGain.COAId = $scope.COAId;
        $scope.exchangeLoss.COAId = $scope.COAId;
        $scope.exchangeGain.SourceType = $scope.SourceType;
        $scope.exchangeLoss.SourceType = $scope.SourceType;
        $scope.exchangeGainLossList.push($scope.exchangeGain);
        $scope.exchangeGainLossList.push($scope.exchangeLoss);
    };

    $scope.Save = function () {
        $scope.ExchangeGainLossList();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.exchangeGainLossForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'exchangeGainLoss': $scope.exchangeGainLossList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.exchangeGainLosses.push(response.data.exchangeGainLoss);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.exchangeGainLoss,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.chartOfAccountLevel6s[$scope.index] = $scope.exchangeGainLoss;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.exchangeGainLoss = {};
        $scope.exchangeGainLoss.Active = true;
        $scope.exchangeGain = {};
        $scope.exchangeLoss = {};
        $scope.exchangeGainList = [];
        $scope.exchangeLossList = [];
        $scope.exchangeGainLossList = [];
    }
}