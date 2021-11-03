'use strict';
discountTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function discountTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Discount GL';
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.debitNoteTypeGivenGLList = [];
    $scope.debitNoteTypeGivenGLWithCombineList = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.saveUrl = $scope.path + 'SaveFinancingTypeGL';
    $scope.discountTypeGL = {
        Id: null,
        CountryId: null,
        COAId: null,
        ExpensesGLId: null,
        ExpensesBudgetMasterId: null,
        ExpensesActivityId: null,
        LiabilityGLId: null,
        LiabilityBudgetMasterId: null,
        LiabilityActivityId: null
    };

    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector('#itemsearchpopup')).modal('show');
    };

    $scope.debitNoteTypeGivenList = [];
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
                if ($scope.tempList[i].debitNoteTypeGivenId === data.debitNoteTypeGivenId) {
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
            if ($scope.discountTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetDiscountTypeGLTypeGLAllList?coaId=' + $scope.discountTypeGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.discountTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetRoundingGLNotAssingList?coaId=' + $scope.discountTypeGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.discountTypeGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetRoundingGLAssingList?coaId=' + $scope.discountTypeGL.COAId;
        }
        $scope.debitNoteTypeGivenGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'AssetUserName', 'AssetUserName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.debitNoteTypeGivenGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.debitNoteTypeGivenGLWithCombineList.length; i++) {
                        $scope.debitNoteTypeGivenGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.debitNoteTypeGivenGLWithCombineList[i].InvestmentTypeGivenId);
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
            'name': 'GL',
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
        if ($scope.discountTypeGL.COAId === null || $scope.discountTypeGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.discountTypeGL.COAId;
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
        $scope.discountTypeGL.ExpensesGLId = x.GLGeneralInfoId;
        getAssetBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.AssetGLInof = null;
        $scope.discountTypeGL.AssetGLId = null;
        $scope.assetBudgetList = [];
        $scope.assetActivityList = [];
        $scope.discountTypeGL.AssetBudgetMasterId = null;
        $scope.discountTypeGL.AssetActivityId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.discountTypeGL.COAId, $scope.discountTypeGL.AssetGLId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.discountTypeGL.AssetBudgetMasterId, function (result) {
            $scope.assetActivityList = result;
        });
    };

    $scope.searchRevenueTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.revenueTypeListParameters = {
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

    $scope.getRevenueTypeList = function () {
        if ($scope.discountTypeGL.COAId === null || $scope.discountTypeGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }

        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.discountTypeGL.COAId;
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
        $scope.discountTypeGL.RevenueGLId = x.GLGeneralInfoId;
        getRevenueBudget();
    };

    $scope.refreshRevenueGL = function () {
        $scope.RevenueGLInof = null;
        $scope.discountTypeGL.RevenueGLId = null;
        $scope.revenueBudgetList = [];
        $scope.revenueActivityList = [];
        $scope.discountTypeGL.RevenueBudgetMasterId = null;
        $scope.discountTypeGL.RevenueActivityId = null;
    };

    $scope.revenueBudgetList = [];
    function getRevenueBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.discountTypeGL.COAId, $scope.discountTypeGL.RevenueGLId, function (result) {
            $scope.revenueBudgetList = result;
        });
    }

    $scope.revenueActivityList = [];
    $scope.getRevenueActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.discountTypeGL.RevenueBudgetMasterId, function (result) {
            $scope.revenueActivityList = result;
        });
    };

    $scope.searchLiabilityTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
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
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getLiabilityTypeList = function () {
        if ($scope.discountTypeGL.COAId === null || $scope.discountTypeGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.discountTypeGL.COAId;
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
        $scope.discountTypeGL.LiabilityGLId = x.GLGeneralInfoId;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.discountTypeGL.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.discountTypeGL.LiabilityBudgetMasterId = null;
        $scope.discountTypeGL.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.discountTypeGL.COAId, $scope.discountTypeGL.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.discountTypeGL.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
        });
    };

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
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getExpensesTypeList = function () {
        if ($scope.discountTypeGL.COAId === null || $scope.discountTypeGL.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.discountTypeGL.COAId;
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
        $scope.discountTypeGL.ExpensesGLId = x.GLGeneralInfoId;
        getExpensesBudget();
    };

    $scope.refreshExpensesGL = function () {
        $scope.ExpensesGLInof = null;
        $scope.discountTypeGL.GLGeneralInfoId = null;
        $scope.expensesBudgetList = [];
        $scope.expensesActivityList = [];
        $scope.discountTypeGL.ExpensesBudgetMasterId = null;
        $scope.discountTypeGL.ExpensesActivityId = null;
    };

    $scope.expensesBudgetList = [];
    function getExpensesBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.discountTypeGL.COAId, $scope.discountTypeGL.ExpensesGLId, function (result) {
            $scope.expensesBudgetList = result;
        });
    }

    $scope.expensesActivityList = [];
    $scope.getExpensesActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.discountTypeGL.ExpensesBudgetMasterId, function (result) {
            $scope.expensesActivityList = result;
        });
    };

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.debitNoteTypeGivenName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };

    $scope.removeRow = function () {
        for (var i = 0; i < $scope.debitNoteTypeGivenGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.debitNoteTypeGivenGLWithCombineList[i].Id === $scope.glUntagId) {
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
        $scope.debitNoteTypeGivenGLWithCombineList[i] = {
            ExpensesUserName: $scope.debitNoteTypeGivenGLWithCombineList[i].ExpensesUserName,
            COAId: $scope.debitNoteTypeGivenGLWithCombineList[i].COAId,
            COAName: $scope.debitNoteTypeGivenGLWithCombineList[i].COAName,
            Code: $scope.debitNoteTypeGivenGLWithCombineList[i].Code,
            FinancingTypeId: $scope.debitNoteTypeGivenGLWithCombineList[i].FinancingTypeId
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
        $scope.debitNoteTypeGivenGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.discountTypeGL.RevenueGLId !== null) {
                    item.RevenueGLId = $scope.discountTypeGL.RevenueGLId;
                }
                if ($scope.discountTypeGL.RevenueActivityId !== null) {
                    item.RevenueActivityId = $scope.discountTypeGL.RevenueActivityId;
                }
                if ($scope.discountTypeGL.RevenueBudgetMasterId !== null) {
                    item.RevenueBudgetMasterId = $scope.discountTypeGL.RevenueBudgetMasterId;
                }
                if ($scope.discountTypeGL.LiabilityGLId !== null) {
                    item.LiabilityGLId = $scope.discountTypeGL.LiabilityGLId;
                }
                if ($scope.discountTypeGL.LiabilityActivityId !== null) {
                    item.LiabilityActivityId = $scope.discountTypeGL.LiabilityActivityId;
                }
                if ($scope.discountTypeGL.LiabilityBudgetMasterId !== null) {
                    item.LiabilityBudgetMasterId = $scope.discountTypeGL.LiabilityBudgetMasterId;
                }
                if ($scope.discountTypeGL.ExpensesGLId !== null) {
                    item.ExpensesGLId = $scope.discountTypeGL.ExpensesGLId;
                }
                if ($scope.discountTypeGL.ExpensesActivityId !== null) {
                    item.ExpensesActivityId = $scope.discountTypeGL.ExpensesActivityId;
                }
                if ($scope.discountTypeGL.ExpensesBudgetMasterId !== null) {
                    item.ExpensesBudgetMasterId = $scope.discountTypeGL.ExpensesBudgetMasterId;
                }
                item.COAId = $scope.discountTypeGL.COAId;
                $scope.debitNoteTypeGivenGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.debitNoteTypeGivenGLListForSave.length < 1) {
            return ShowResult("please select rounding type given!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.debitNoteTypeGivenGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'financingTypeGLList': $scope.debitNoteTypeGivenGLListForSave
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
                $scope.getdebitNoteTypeGivenWithCoa('all');
            }
        } else {
            $scope.getdebitNoteTypeGivenWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.refreshRevenueGL();
        $scope.refreshLiabilityGL();
        $scope.refreshExpensesGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.discountTypeGL = { COAId: $scope.discountTypeGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.debitNoteTypeGivenGLWithCombineList = [];
    }
}