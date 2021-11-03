'use strict';
loanTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function loanTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Loan Type GL';
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.loanTypeGivenGLList = [];
    $scope.loanTypeGivenGLWithCombineList = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.saveUrl = $scope.path + 'SaveFinancingTypeGL';
    $scope.investmentTypeGivenGL = {
        Id: null,
        CountryId: null,
        InvestmentTypeGivenId: null,
        AssetGLId: null,
        AssetBudgetMasterId: null,
        AssetActivityId: null,
        RevenueGLId: null,
        RevenueBudgetMasterId: null,
        RevenueActivityId: null,
        COAId: null,
        InvestmentTypeTakenId: null,
        ExpensesGLId: null,
        ExpensesBudgetMasterId: null,
        ExpensesActivityId: null,
        LiabilityGLId: null,
        LiabilityBudgetMasterId: null,
        LiabilityActivityId: null,
        ExpensesPayableBudgetMasterId: null,
        ExpensesPayableActivityId: null,
        ExpensesPayableGLId: null,
        ChargesPayableBudgetMasterId: null,
        ChargesPayableActivityId: null,
        ChargesPayableGLId: null
    };

    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector('#itemsearchpopup')).modal('show');
    };

    $scope.investmentTypeGivenList = [];
    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        if (event.currentTarget.checked) {
            $scope.tempList.push(data);
        }
        else {
            for (var i = 0; i < $scope.tempList.length; i++) {
                if ($scope.tempList[i].InvestmentTypeGivenId === data.InvestmentTypeGivenId) {
                    $scope.tempList.splice(i, 1);
                }
                break;
            }
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
            if ($scope.investmentTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetLoanTypeGLAllList?coaId=' + $scope.investmentTypeGivenGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.investmentTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetLoanTypeGLNotAssingList?coaId=' + $scope.investmentTypeGivenGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.investmentTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetLoanTypeGLAssingList?coaId=' + $scope.investmentTypeGivenGL.COAId;
        }
        $scope.investmentTypeGivenGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'AssetUserName', 'AssetUserName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.investmentTypeGivenGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.investmentTypeGivenGLWithCombineList.length; i++) {
                        $scope.investmentTypeGivenGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.investmentTypeGivenGLWithCombineList[i].InvestmentTypeGivenId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.searchAssetTypeByList = [
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
        }
    ];

    $scope.assetTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getAssetTypeList = function () {
        if ($scope.investmentTypeGivenGL.COAId === null || $scope.investmentTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWiseExceptRecon?coaId=' + $scope.investmentTypeGivenGL.COAId;
        $scope.getAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.assetTypeListParameters)
                .then(function (data) {
                    $scope.assetTypeGLList = data.Rows;
                    $scope.assetTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#assetTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getAssetTypeListData();
    };

    $scope.closeAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#assetTypeListPopUp')).modal('hide');
        }
    };

    $scope.setAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.AssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.investmentTypeGivenGL.AssetGLId = x.GLGeneralInfoId;
        getAssetBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.AssetGLInof = null;
        $scope.investmentTypeGivenGL.AssetGLId = null;
        $scope.assetBudgetList = [];
        $scope.assetActivityList = [];
        $scope.investmentTypeGivenGL.AssetBudgetMasterId = null;
        $scope.investmentTypeGivenGL.AssetActivityId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.investmentTypeGivenGL.COAId, $scope.investmentTypeGivenGL.AssetGLId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.investmentTypeGivenGL.AssetBudgetMasterId, function (result) {
            $scope.assetActivityList = result;
        });
    };

    $scope.searchRevenueTypeByList = [
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
        }
    ];

    $scope.revenueTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getRevenueTypeList = function () {
        if ($scope.investmentTypeGivenGL.COAId === null || $scope.investmentTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }

        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.investmentTypeGivenGL.COAId;
        $scope.getRevenueTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.revenueTypeListParameters)
                .then(function (data) {
                    $scope.revenueTypeGLList = data.Rows;
                    $scope.revenueTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#revenueTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getRevenueTypeListData();
    };

    $scope.closeRevenueTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#revenueTypeListPopUp')).modal('hide');
        }
    };

    $scope.setRevenueGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.RevenueGLSelectedData = x;
        $scope.RevenueGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.investmentTypeGivenGL.RevenueGLId = x.GLGeneralInfoId;
        getRevenueBudget();
    };

    $scope.refreshRevenueGL = function () {
        $scope.RevenueGLInof = null;
        $scope.investmentTypeGivenGL.RevenueGLId = null;
        $scope.revenueBudgetList = [];
        $scope.revenueActivityList = [];
        $scope.investmentTypeGivenGL.RevenueBudgetMasterId = null;
        $scope.investmentTypeGivenGL.RevenueActivityId = null;
    };

    $scope.revenueBudgetList = [];
    function getRevenueBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.investmentTypeGivenGL.COAId, $scope.investmentTypeGivenGL.RevenueGLId, function (result) {
            $scope.revenueBudgetList = result;
        });
    }

    $scope.revenueActivityList = [];
    $scope.getRevenueActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.investmentTypeGivenGL.RevenueBudgetMasterId, function (result) {
            $scope.revenueActivityList = result;
        });
    };

    $scope.searchLiabilityTypeByList = [
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
        }
    ];

    $scope.liabilityTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getLiabilityTypeList = function () {
        if ($scope.investmentTypeGivenGL.COAId === null || $scope.investmentTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityCOAWise?coaId=' + $scope.investmentTypeGivenGL.COAId;
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
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#liabilityTypeListPopUp')).modal('hide');
        }
    };

    $scope.setLiabilityGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.LiabilityGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.investmentTypeGivenGL.LiabilityGLId = x.GLGeneralInfoId;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.investmentTypeGivenGL.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.investmentTypeGivenGL.LiabilityBudgetMasterId = null;
        $scope.investmentTypeGivenGL.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.investmentTypeGivenGL.COAId, $scope.investmentTypeGivenGL.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.investmentTypeGivenGL.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
        });
    };

    $scope.searchExpensesTypeByList = [
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
        }
    ];

    $scope.expensesTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getExpensesTypeList = function () {
        if ($scope.investmentTypeGivenGL.COAId === null || $scope.investmentTypeGivenGL.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.investmentTypeGivenGL.COAId;
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
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#expensesTypeListPopUp')).modal('hide');
        }
    };

    $scope.setExpensesGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.ExpensesGLSelectedData = x;
        $scope.ExpensesGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.investmentTypeGivenGL.ExpensesGLId = x.GLGeneralInfoId;
        getExpensesBudget();
    };
    $scope.refreshExpensesGL = function () {
        $scope.ExpensesGLInof = null;
        $scope.investmentTypeGivenGL.GLGeneralInfoId = null;
        $scope.expensesBudgetList = [];
        $scope.expensesActivityList = [];
        $scope.investmentTypeGivenGL.ExpensesBudgetMasterId = null;
        $scope.investmentTypeGivenGL.ExpensesActivityId = null;
    };

    $scope.expensesBudgetList = [];
    function getExpensesBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.investmentTypeGivenGL.COAId, $scope.investmentTypeGivenGL.ExpensesGLId, function (result) {
            $scope.expensesBudgetList = result;
        });
    };

    $scope.expensesActivityList = [];
    $scope.getExpensesActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.investmentTypeGivenGL.ExpensesBudgetMasterId, function (result) {
            $scope.expensesActivityList = result;
        });
    };

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.InvestmentTypeGivenName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };

    $scope.removeRow = function () {
        for (var i = 0; i < $scope.investmentTypeGivenGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.investmentTypeGivenGLWithCombineList[i].Id === $scope.glUntagId) {
                    $scope.unTagGL($scope.glUntagId, i);
                    break;
                }
            } else {
                unTagFromList($scope.glUntagIndex);
                $scope.glUntagIndex = -1;
                break;
            }
        }
        $scope.mauid = null;
        $scope.mauindex = -1;
    };
    function unTagFromList(i) {
        $scope.investmentTypeGivenGLWithCombineList[i] = {
            AssetUserName: $scope.investmentTypeGivenGLWithCombineList[i].AssetUserName,
            COAId: $scope.investmentTypeGivenGLWithCombineList[i].COAId,
            COAName: $scope.investmentTypeGivenGLWithCombineList[i].COAName,
            Code: $scope.investmentTypeGivenGLWithCombineList[i].Code,
            FinancingTypeId: $scope.investmentTypeGivenGLWithCombineList[i].FinancingTypeId
        };
    }
    $scope.unTagGL = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/DeleteFinancingTypeGL',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.tempList.length; i++) {
                        if ($scope.tempList[i].Id === id) {
                            document.getElementById($scope.tempList[i].FinancingTypeId).checked = false;
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }
                    unTagFromList(index);
                    $scope.glUntagIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.addGlForSelectble = function () {
        $scope.investmentTypeGivenGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.investmentTypeGivenGL.AssetGLId !== null) {
                    item.AssetGLId = $scope.investmentTypeGivenGL.AssetGLId;
                }
                if ($scope.investmentTypeGivenGL.AssetActivityId !== null) {
                    item.AssetActivityId = $scope.investmentTypeGivenGL.AssetActivityId;
                }
                if ($scope.investmentTypeGivenGL.AssetBudgetMasterId !== null) {
                    item.AssetBudgetMasterId = $scope.investmentTypeGivenGL.AssetBudgetMasterId;
                }
                if ($scope.investmentTypeGivenGL.RevenueGLId !== null) {
                    item.RevenueGLId = $scope.investmentTypeGivenGL.RevenueGLId;
                }
                if ($scope.investmentTypeGivenGL.RevenueActivityId !== null) {
                    item.RevenueActivityId = $scope.investmentTypeGivenGL.RevenueActivityId;
                }
                if ($scope.investmentTypeGivenGL.RevenueBudgetMasterId !== null) {
                    item.RevenueBudgetMasterId = $scope.investmentTypeGivenGL.RevenueBudgetMasterId;
                }
                if ($scope.investmentTypeGivenGL.LiabilityGLId !== null) {
                    item.LiabilityGLId = $scope.investmentTypeGivenGL.LiabilityGLId;
                }
                if ($scope.investmentTypeGivenGL.LiabilityActivityId !== null) {
                    item.LiabilityActivityId = $scope.investmentTypeGivenGL.LiabilityActivityId;
                }
                if ($scope.investmentTypeGivenGL.LiabilityBudgetMasterId !== null) {
                    item.LiabilityBudgetMasterId = $scope.investmentTypeGivenGL.LiabilityBudgetMasterId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesGLId !== null) {
                    item.ExpensesGLId = $scope.investmentTypeGivenGL.ExpensesGLId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesActivityId !== null) {
                    item.ExpensesActivityId = $scope.investmentTypeGivenGL.ExpensesActivityId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesBudgetMasterId !== null) {
                    item.ExpensesBudgetMasterId = $scope.investmentTypeGivenGL.ExpensesBudgetMasterId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesPayableGLId !== null) {
                    item.ExpensesPayableGLId = $scope.investmentTypeGivenGL.ExpensesPayableGLId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesPayableActivityId !== null) {
                    item.ExpensesPayableActivityId = $scope.investmentTypeGivenGL.ExpensesPayableActivityId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesPayableBudgetMasterId !== null) {
                    item.ExpensesPayableBudgetMasterId = $scope.investmentTypeGivenGL.ExpensesPayableBudgetMasterId;
                }
                if ($scope.investmentTypeGivenGL.ChargesPayableGLId !== null) {
                    item.ChargesPayableGLId = $scope.investmentTypeGivenGL.ChargesPayableGLId;
                }
                if ($scope.investmentTypeGivenGL.ChargesPayableActivityId !== null) {
                    item.ChargesPayableActivityId = $scope.investmentTypeGivenGL.ChargesPayableActivityId;
                }
                if ($scope.investmentTypeGivenGL.ChargesPayableBudgetMasterId !== null) {
                    item.ChargesPayableBudgetMasterId = $scope.investmentTypeGivenGL.ChargesPayableBudgetMasterId;
                }
                item.COAId = $scope.investmentTypeGivenGL.COAId;
                $scope.investmentTypeGivenGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.investmentTypeGivenGLListForSave.length < 1) {
            return ShowResult("Please select Investment Type Given!", 'failure');
        }
        //if (baseService.isUndefinedOrNull($scope.investmentTypeGivenGL.AssetGLId) && baseService.isUndefinedOrNull($scope.investmentTypeGivenGL.RevenueGLId)) {
        //    return ShowResult("Please select Asset and Revenue both side GL!", 'failure');
        //}
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.investmentTypeGivenGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'financingTypeGLList': $scope.investmentTypeGivenGLListForSave
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
        if ($scope.btnSet !== '') {
            if ($scope.btnSet === 'all') {
                $scope.getInvestmentTypeGivenWithCoa('all');
            }
        } else {
            $scope.getInvestmentTypeGivenWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.refreshRevenueGL();
        $scope.refreshLiabilityGL(); 
        $scope.refreshExpensesGL();
        $scope.refreshExpensesPayableGL();
        $scope.refreshChargesPayableGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.investmentTypeGivenGL = { COAId: $scope.investmentTypeGivenGL.COAId }
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.investmentTypeGivenGLWithCombineList = [];
    }

    // #region ExpensesPayable

 
    $scope.getExpensesPayableTypeList = function () {
        if ($scope.investmentTypeGivenGL.COAId === null || $scope.investmentTypeGivenGL.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityCOAWise?coaId=' + $scope.investmentTypeGivenGL.COAId;
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
        angular.element(document.querySelector('#expensesPayableTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getExpensesTypeListData();
    };
    $scope.closeExpensesPayableTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#expensesPayableTypeListPopUp')).modal('hide');
        }
    };

    $scope.setExpensesPayableGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.ExpensesGLSelectedData = x;
        $scope.ExpensesPayableGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.investmentTypeGivenGL.ExpensesPayableGLId = x.GLGeneralInfoId;
        getExpensesPayableBudget();
    };
    $scope.refreshExpensesPayableGL = function () {
        $scope.ExpensesPayableGLInof = null;
        $scope.investmentTypeGivenGL.GLGeneralInfoId = null;
        $scope.expensesPayableBudgetList = [];
        $scope.expensesPayableActivityList = [];
        $scope.investmentTypeGivenGL.ExpensesPayableBudgetMasterId = null;
        $scope.investmentTypeGivenGL.ExpensesPayableActivityId = null;
    };

    $scope.expensesBudgetList = [];
    function getExpensesPayableBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.investmentTypeGivenGL.COAId, $scope.investmentTypeGivenGL.ExpensesPayableGLId, function (result) {
            $scope.expensesPayableBudgetList = result;
        });
    };

    $scope.expensesActivityList = [];
    $scope.getExpensesPayableActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.investmentTypeGivenGL.ExpensesPayableBudgetMasterId, function (result) {
            $scope.expensesPayableActivityList = result;
        });
    };


    $scope.chargesPayableTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    // #endregion
    $scope.chargesPayableTypeGLList = [];
    $scope.getChargesPayableTypeList = function () {
        if ($scope.investmentTypeGivenGL.COAId === null || $scope.investmentTypeGivenGL.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityCOAWise?coaId=' + $scope.investmentTypeGivenGL.COAId;
        $scope.getChargesPayableListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.chargesPayableTypeListParameters)
                .then(function (data) {
                    $scope.chargesPayableTypeGLList = data.Rows;
                    $scope.chargesPayableTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#chargesPayableTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getChargesPayableListData();
    };
    $scope.closeChargesPayableTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#chargesPayableTypeListPopUp')).modal('hide');
        }
    };

    $scope.setChargesPayableGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.ChargesPayableGLSelectedData = x;
        $scope.ChargesPayableGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.investmentTypeGivenGL.ChargesPayableGLId = x.GLGeneralInfoId;
        getChargesPayableBudget();
    };
    $scope.refreshChargesPayableGL = function () {
        $scope.ChargesPayableGLInof = null;
        $scope.investmentTypeGivenGL.GLGeneralInfoId = null;
        $scope.chargesPayableBudgetList = [];
        $scope.chargesPayableActivityList = [];
        $scope.investmentTypeGivenGL.ChargesPayableBudgetMasterId = null;
        $scope.investmentTypeGivenGL.ChargesPayableActivityId = null;
    };

    $scope.chargesPayableBudgetList = [];
    function getChargesPayableBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.investmentTypeGivenGL.COAId, $scope.investmentTypeGivenGL.ChargesPayableGLId, function (result) {
            $scope.chargesPayableBudgetList = result;
        });
    };

    $scope.chargesPayableActivityList = [];
    $scope.getChargesPayableActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.investmentTypeGivenGL.ChargesPayableBudgetMasterId, function (result) {
            $scope.chargesPayableActivityList = result;
        });
    };



}