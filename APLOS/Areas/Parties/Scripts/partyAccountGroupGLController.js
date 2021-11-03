'use strict';
partyAccountGroupGLController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function partyAccountGroupGLController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = "Party Account Group GL";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'parties/partyaccountgroupgl/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.partyAccountGroupGL = {
        Id: null,
        COAId: null,
        PartyAccountGroupId: null,
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        PartyGLType: null,
        Active: true
    };

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.partyAccountGroupGLList = [];
    $scope.GetPartyAccountGroupData = function () {
        $http({
            method: 'GET',
            url: 'Parties/partyaccountgroupgl/getlist?coaId=' + $scope.partyAccountGroupGL.COAId
        }).then(function (response) {
            $scope.partyAccountGroupGLList = response.data;
        });
    };

    $scope.glTypeList = [];
    cboService.getEnumCbo("enum/getpartygltypeenumcbo", function (result) {
        $scope.glTypeList = result;
    });

    $scope.partyAccountGroupList = [];
    cboService.partyAccountGroupCbo(function (result) {
        $scope.partyAccountGroupList = result;
    });

    $scope.changePartyAccountGroupCbo = function (id) {
        var accountType = $.grep($scope.partyAccountGroupList, function (item) {
            return item.Value === id;
        })[0].AccountType;
        $scope.partyAccountGroupGL.AccountType = accountType;
    };

    $scope.partyAccountGroupTypeList = [];
    $http({
        method: 'GET',
        url: 'Enum/getpartyaccountgrouptypelistcbo'
    }).then(function (response) {
        $scope.partyAccountGroupTypeList = response.data;
    });

    $scope.companyConfig = {
        IsVoucherFromBudget: true
    };

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

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "AccountGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.cOAICodeList = [];
    $scope.GetCOAICodeList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.partyAccountGroupGL.COAId)) {
                throw 'Please select COA.';
            }
            if ($scope.partyAccountGroupGL.AccountType === 'Vendor'
                && ($scope.partyAccountGroupGL.PartyGLType === 'ReconciliationGL' || $scope.partyAccountGroupGL.PartyGLType === 'AdditionalGL')) {
                $scope.GLUrl = 'accounts/glitem/GetVendorReconeGLPartyAccountGroup?coaId=' + $scope.partyAccountGroupGL.COAId;
            }
            else if ($scope.partyAccountGroupGL.AccountType === 'Customer'
                && ($scope.partyAccountGroupGL.PartyGLType === 'ReconciliationGL' || $scope.partyAccountGroupGL.PartyGLType === 'AdditionalGL')) {
                $scope.GLUrl = 'accounts/glitem/GetCustomerReconeGLPartyAccountGroup?coaId=' + $scope.partyAccountGroupGL.COAId;
            }
            else if ($scope.partyAccountGroupGL.AccountType === 'Vendor'
                && ($scope.partyAccountGroupGL.PartyGLType === 'DownPaymentGL' || $scope.partyAccountGroupGL.PartyGLType === 'SuspenseGL')) {
                $scope.GLUrl = 'accounts/glitem/getvendordownpaymentglcoawise?coaId=' + $scope.partyAccountGroupGL.COAId;
            }
            else if ($scope.partyAccountGroupGL.AccountType === 'Customer'
                && ($scope.partyAccountGroupGL.PartyGLType === 'DownPaymentGL' || $scope.partyAccountGroupGL.PartyGLType === 'SuspenseGL')) {
                $scope.GLUrl = 'accounts/glitem/getcustomerdownpaymentglcoawise?coaId=' + $scope.partyAccountGroupGL.COAId;
            }

            baseService.setCurrentPage('cOAICodeList');
            $scope.GetCOAICodeListData = function (pageno) {
                baseService.paginationBase($scope.GLUrl, pageno, $scope.glListParameters)
                    .then(function (data) {
                        $scope.cOAICodeList = data.Rows;
                        $scope.glListParameters.total_count = data.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#GLPopUp')).modal('show');
            $scope.modalShow = true;
            $scope.GetCOAICodeListData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
    };

    $scope.closeCOAICodeListPopUpSelected = function (gl) {
        try {
            var partyAccountGroupGL = $filter('filter')($scope.partyAccountGroupGLList, { PartyAccountGroupId: $scope.partyAccountGroupGL.PartyAccountGroupId, PartyGLType: $scope.partyAccountGroupGL.PartyGLType });
            if (!baseService.isUndefinedOrNull(partyAccountGroupGL) && partyAccountGroupGL.length > 0) {
                if (partyAccountGroupGL[0].PartyGLType === 'ReconciliationGL') {
                    throw 'Reconciliation GL is already exists.';
                }
                else if (partyAccountGroupGL[0].PartyGLType === 'DownPaymentGL') {
                    throw 'DownPayment GL is already exists.';
                }
                else if (partyAccountGroupGL[0].PartyGLType === 'SuspenseGL') {
                    throw 'Suspense GL is already exists.';
                }
                else if ($filter('filter')(partyAccountGroupGL, { PartyGLType: 'AdditionalGL', GLGeneralInfoId: gl.GLGeneralInfoId }).length > 0) {
                    throw 'Additional GL is already exists.';
                }
            }

            $scope.partyAccountGroupGL.GLGeneralInfoId = gl.GLGeneralInfoId;
            $scope.partyAccountGroupGL.GLGeneralInfoCode = gl.GLGeneralInfoCode;
            $scope.partyAccountGroupGL.GLGeneralInfoName = gl.GLGeneralInfoName;
            $scope.partyAccountGroupGL.BudgetMasterId = gl.BudgetMasterId;
            $scope.partyAccountGroupGL.BudgetCode = gl.BudgetCode;
            $scope.partyAccountGroupGL.BudgetName = gl.BudgetName;
            $scope.partyAccountGroupGL.ActivityId = gl.ActivityId;
            $scope.partyAccountGroupGL.ActivityCode = gl.ActivityCode;
            $scope.partyAccountGroupGL.ActivityName = gl.ActivityName;
            $scope.partyAccountGroupGL.Active = true;
            $scope.partyAccountGroupGL.PartyAccountGroupName = angular.element("#PartyAccountGroupId :selected").text();

            angular.element(document.querySelector('#GLPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'GLPopUp');
        }
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector('#GLPopUp')).modal('hide');
    };

    $scope.addRow = function () {
        $scope.partyAccountGroupGLList.push($scope.partyAccountGroupGL);
        $scope.partyAccountGroupGL = {
            COAId: $scope.partyAccountGroupGL.COAId
            , AccountType: $scope.partyAccountGroupGL.AccountType
            , PartyAccountGroupId: $scope.partyAccountGroupGL.PartyAccountGroupId
        };
    };

    $scope.Save = function () {
        try {
            if ($scope.partyAccountGroupForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: 'parties/partyaccountgroupgl/create',
                        data: {
                            'partyAccountGroupGlList': $scope.partyAccountGroupGLList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.partyAccountGroupGL.COAId = response.data.PartyAccountGroupGL[0].COAId;
                            $scope.GetPartyAccountGroupData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.confirmDelete = function (data) {
        $scope.Name = data.GLGeneralInfoCode + ' - ' + data.GLGeneralInfoName;
        $scope.deleteId = data.Id;
        $scope.message_confirmation = "Are you sure to delete [" + $scope.Name + "]?";
    };

    $scope.Delete = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            $scope.partyAccountGroupGLList.splice($scope.index, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'parties/partyaccountgroupgl/delete',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPartyAccountGroupData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };
}