'use strict';
TaxCodeGLController.$inject = ['addressService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TaxCodeGLController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'TaxCode Account Determinate';
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.SaveDisable = false;
    $scope.index = -1;
    $scope.taxCodeGLList = [];
    $scope.taxCodeGLWithCombineList = [];
    $scope.path = 'accounts/taxCodeGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateTaxCodeDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.taxCodeGL = {
        Id: null,
        CountryId: null,
        TaxCodeId: null,
        WithholdCreditableGLId: null,
        CreditableGLId: null,
        ExpensesGLId: null,
        WithholdCreditableBudgetMasterId: null,
        WithholdCreditableActivityId: null,
        CreditableGLBudgetMasterId: null,
        CreditableGLActivityId: null,
        ExpensesGLBudgetMasterId: null,
        ExpensesGLActivityId: null,
        COAId: null
    };
    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector('#itemsearchpopup')).modal('show');
    };

    $scope.taxCodeList = [];
    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $scope.tempList = [];
    $scope.expensesEnable = true;
    $scope.selectChValueId = function (data) {
        $scope.tempList = [];
        data.Flag = true;
        $scope.refreshAllGL();
        if (data.IsWithhold && data.IsCreditable) {
            $scope.withholdEnable = true;
            $scope.creditableEnable = true;
            $scope.expensesEnable = false;
            $scope.SaveDisable = false;
        } else if (data.IsMerge && data.IsWithhold) {
            $scope.withholdEnable = true;
            $scope.expensesEnable = false;
            $scope.creditableEnable = false;
            $scope.SaveDisable = false;
        }
        else if (data.IsMerge) {
            $scope.withholdEnable = false;
            $scope.expensesEnable = true;
            $scope.creditableEnable = false;
            $scope.SaveDisable = false;
        }
        else if (data.IsCreditable) {
            $scope.withholdEnable = false;
            $scope.expensesEnable = false;
            $scope.creditableEnable = true;
            $scope.SaveDisable = false;
        } else if (data.IsWithhold) {
            $scope.withholdEnable = true;
            $scope.expensesEnable = true;
            $scope.creditableEnable = false;
            $scope.SaveDisable = false;
        } else {
            $scope.withholdEnable = false;
            $scope.creditableEnable = false;
            $scope.expensesEnable = true;
            $scope.SaveDisable = false;
        }
        angular.forEach($scope.taxCodeGLWithCombineList, function (item) {
            if (data.TaxCodeId === item.TaxCodeId) {
                item.Flag = true;
            } else {
                item.Flag = false;
            }
        });
        $scope.tempList = data;
        if (data.IsMerge && data.IsWithhold === false && data.IsCreditable === false) {
            $scope.SaveDisable = true;
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    $scope.showAll = function (str) {
        if (str === 'all') {
            if ($scope.taxCodeGL.CountryId === null) {
                return ShowResult('Select Country first', 'failure');
            }
            if ($scope.taxCodeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/taxCodeGL/getlistwithcombine?countryId=' + $scope.taxCodeGL.CountryId + '&coaId=' + $scope.taxCodeGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.taxCodeGL.CountryId === null) {
                return ShowResult('Select Country first', 'failure');
            }
            if ($scope.taxCodeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }

            $scope.url = 'accounts/taxCodeGL/getlistwithcombinenotassing?countryId=' + $scope.taxCodeGL.CountryId + '&coaId=' + $scope.taxCodeGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.taxCodeGL.CountryId === null) {
                return ShowResult('Select Country first', 'failure');
            }
            if ($scope.taxCodeGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/taxCodeGL/getlistwithcombineassing?countryId=' + $scope.taxCodeGL.CountryId + '&coaId=' + $scope.taxCodeGL.COAId;
        }
        $scope.taxCodeGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'TaxCodeName', 'TaxCodeName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCodeGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCodeGLWithCombineList.length; i++) {
                        $scope.taxCodeGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCodeGLWithCombineList[i].TaxCodeId);
                        $scope.refreshAllGL();
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    function IdList() {
        $scope.taxCodeIdstr = createIdList(validListWithStr($scope.taxCodeClassNewList, $scope.fixedassetClassIds));
    }

    function createIdList(list) {
        var value = "''";
        for (var i = 0; i < list.length; i++) {
            if (value == "''") {
                value = "'" + list[i].Value + "'";
            } else {
                value += ",'" + list[i].Value + "'";
            }
        }
        return value;
    }

    $scope.refreshAllGL = function () {
        $scope.refreshLiabilityGL();
        $scope.refreshCreditableGL();
        $scope.revenueBudgetList = [];
        $scope.revenueActivityList = [];
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.expensesBudgetList = [];
        $scope.expensesActivityList = [];
    };

    $scope.searchLiabilityTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'WithHoldGLCode'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.liabilityTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'WithHoldGLCode',
        searchBy: 'GLItem',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getLiabilityTypeList = function () {
        if ($scope.taxCodeGL.COAId === null || $scope.taxCodeGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }

        $scope.GLUrl1 = 'accounts/glitem/GetWithHoldGLCOAWise?coaId=' + $scope.taxCodeGL.COAId;
        $scope.getLiabilityTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.liabilityTypeListParameters)
                .then(function (data) {
                    $scope.liabilityTypeGLList = data.Rows;
                    $scope.liabilityTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#liabilityTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getLiabilityTypeListData();
    };

    $scope.closeLiabilityTypeListPopUpSelected = function () {
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#liabilityTypeListPopUp')).modal('hide');
        }
    };

    $scope.setLiabilityGLSelected = function (x) {
        $scope.rowSelected = x.WithHoldGLCode;
        $scope.ExpensesGLSelectedData = x;
        $scope.LiabilityGLInof = x.WithHoldGLCode + ' - ' + x.WithHoldGLItem;
        $scope.LiabilityGLId = x.WithholdCreditableGL;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.taxCodeGL.WithholdCreditableBudgetMasterId = null;
        $scope.taxCodeGL.WithholdCreditableActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCodeGL.COAId, $scope.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCodeGL.WithholdCreditableBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
        });
    };

    $scope.searchCreditableTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.creditableTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.creditableGlList = [];
    $scope.getCreditableTypeList = function () {
        if ($scope.taxCodeGL.COAId === null || $scope.taxCodeGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetCreditableGLCOAWise?coaId=' + $scope.taxCodeGL.COAId;
        $scope.getCreditableTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.creditableTypeListParameters)
                .then(function (data) {
                    $scope.creditableGlList = data.Rows;
                    $scope.creditableTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CreditableListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getCreditableTypeListData();
    };

    $scope.closeCreditTypeListPopUpSelected = function () {
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#CreditableListPopUp')).modal('hide');
        }
    };

    $scope.setCreditGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.CreditableGLInof = x.CreditableGLCode + ' - ' + x.CreditableGLItem;
        $scope.CreditableGLId = x.CreditableGL;
        getCreditableBudget();
    };

    $scope.refreshCreditableGL = function () {
        $scope.CreditableGLInof = null;
        $scope.CreditableGLId = null;
        $scope.CreditableGLBudgetMasterId = null;
        $scope.CreditableGLActivityId = null;
    };

    $scope.creditableBudgetList = [];
    function getCreditableBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCodeGL.COAId, $scope.CreditableGLId, function (result) {
            $scope.creditableBudgetList = result;
        });
    }

    $scope.creditableActivityList = [];
    $scope.getCreditableActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCodeGL.CreditableGLBudgetMasterId, function (result) {
            $scope.creditableActivityList = result;
        });
    };

    // #endregion
    // #region ******Expense GL******
    $scope.expensesTypeGLList = [];
    $scope.searchExpensesTypeByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.expensesTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetExpenseGlList = function () {
        if ($scope.taxCodeGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWiseTaxRecon?coaId=' + $scope.taxCodeGL.COAId;
        $scope.getExpensesTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.expensesTypeListParameters)
                .then(function (data) {
                    $scope.expensesTypeGLList = data.Rows;
                    $scope.expensesTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#expensesTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getExpensesTypeListData();
    };
    $scope.closeExpensesTypeListPopUpSelected = function () {
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#expensesTypeListPopUp')).modal('hide');
        }
    };
    $scope.setExpensesGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.ExpenseGLInfo = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;
        $scope.ExpenseGLId = x.GLGeneralInfoId;
        getExpenseBudget();
    };
    $scope.refreshExpenseGL = function () {
        $scope.ExpenseGLInfo = null;
        $scope.ExpenseGLId = null;
    };

    $scope.expenseBudgetList = [];
    function getExpenseBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCodeGL.COAId, $scope.ExpenseGLId, function (result) {
            $scope.expenseBudgetList = result;
        });
    }
    $scope.expenseActivityList = [];
    $scope.getExpenseActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCodeGL.ExpensesGLBudgetMasterId, function (result) {
            $scope.expenseActivityList = result;
        });
    };

    // #endregion
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.addGlForSelectble = function () {
        $scope.taxCodeGLListForSave = [];
        angular.forEach($scope.taxCodeGLWithCombineList, function (item) {
            if (item.Flag) {
                item.WithholdCreditableGLId = null;
                item.WithholdCreditableBudgetMasterId = null;
                item.WithholdCreditableActivityId = null;
                item.CreditableGLId = null;
                item.CreditableGLBudgetMasterId = null;
                item.CreditableGLActivityId = null;
                item.ExpensesGLId = null;
                item.ExpensesGLBudgetMasterId = null;
                item.ExpensesGLActivityId = null;
                if ($scope.LiabilityGLId !== null) {
                    item.WithholdCreditableGLId = $scope.LiabilityGLId;
                }
                if ($scope.taxCodeGL.WithholdCreditableBudgetMasterId !== null) {
                    item.WithholdCreditableBudgetMasterId = $scope.taxCodeGL.WithholdCreditableBudgetMasterId;
                }
                if ($scope.taxCodeGL.WithholdCreditableActivityId !== null) {
                    item.WithholdCreditableActivityId = $scope.taxCodeGL.WithholdCreditableActivityId;
                }
                if ($scope.CreditableGLId !== null) {
                    item.CreditableGLId = $scope.CreditableGLId;
                }
                if ($scope.taxCodeGL.CreditableGLBudgetMasterId !== null) {
                    item.CreditableGLBudgetMasterId = $scope.taxCodeGL.CreditableGLBudgetMasterId;
                }
                if ($scope.taxCodeGL.CreditableGLActivityId !== null) {
                    item.CreditableGLActivityId = $scope.taxCodeGL.CreditableGLActivityId;
                }
                if ($scope.ExpenseGLId !== null) {
                    item.ExpensesGLId = $scope.ExpenseGLId;
                }
                if ($scope.taxCodeGL.ExpensesGLBudgetMasterId !== null) {
                    item.ExpensesGLBudgetMasterId = $scope.taxCodeGL.ExpensesGLBudgetMasterId;
                }
                if ($scope.taxCodeGL.ExpensesGLActivityId !== null) {
                    item.ExpensesGLActivityId = $scope.taxCodeGL.ExpensesGLActivityId;
                }
                item.COAId = $scope.taxCodeGL.COAId;
                $scope.taxCodeGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.taxCodeGLListForSave.length < 1) {
            return ShowResult("Please select Tax Code!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.ExpenseGLId) && baseService.isUndefinedOrNull($scope.LiabilityGLId) && baseService.isUndefinedOrNull($scope.CreditableGLId)) {
            return ShowResult("Please Select at least one GL!!", 'failure');
        }
        if (!baseService.isUndefinedOrNull($scope.LiabilityGLId) && (baseService.isUndefinedOrNull($scope.ExpensesGLId)
            && baseService.isUndefinedOrNull($scope.CreditableGLId)) && $scope.taxCodeGLListForSave.IsMerge === false) {
            return ShowResult("Please Select another gl with withhold!!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxCodeGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'taxCodeGL': $scope.taxCodeGLListForSave
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };

    $scope.btnSet = '';
    $scope.setActiveBtn = function (str) {
        $scope.btnSet = str;
    };

    $scope.getAllWithCoa = function () {
        if ($scope.btnSet != '') {
            if ($scope.btnSet === 'all') {
                $scope.getTaxCodeWithCoa('all');
            }
        } else {
            $scope.getTaxCodeWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshLiabilityGL();
        $scope.refreshExpenseGL();
        $scope.refreshCreditableGL();
        $scope.refreshRevenueGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.taxCodeGL = { COAId: $scope.taxCodeGL.COAId, CountryId: $scope.taxCodeGL.CountryId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.taxCodeGLWithCombineList = [];
        $scope.withholdEnable = false;
        $scope.creditableEnable = false;
        $scope.expensesEnable = false;
    }
}