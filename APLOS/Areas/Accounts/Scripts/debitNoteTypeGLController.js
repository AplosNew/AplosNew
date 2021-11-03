'use strict';
debitNoteTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function debitNoteTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Debit Note GL";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.debitNoteTypeGivenGLList = [];
    $scope.debitNoteTypeGivenGLWithCombineList = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.saveUrl = $scope.path + 'SaveFinancingTypeGL';
    $scope.debitNoteTypeGivenGL = {
        Id: null,
        CountryId: null,
        AssetGLId: null,
        AssetBudgetMasterId: null,
        AssetActivityId: null,
        RevenueGLId: null,
        RevenueBudgetMasterId: null,
        RevenueActivityId: null,
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
            if ($scope.debitNoteTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetdebitNoteTypeGLAllList?coaId=' + $scope.debitNoteTypeGivenGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.debitNoteTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetdebitNoteTypeGLNotAssingList?coaId=' + $scope.debitNoteTypeGivenGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.debitNoteTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetdebitNoteTypeGLAssingList?coaId=' + $scope.debitNoteTypeGivenGL.COAId;
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

    // #region ******AssetType GL******
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
        if ($scope.debitNoteTypeGivenGL.COAId === null || $scope.debitNoteTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWiseExceptRecon?coaId=' + $scope.debitNoteTypeGivenGL.COAId;
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
        $scope.debitNoteTypeGivenGL.AssetGLId = x.GLGeneralInfoId;
        getAssetBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.AssetGLInof = null;
        $scope.debitNoteTypeGivenGL.AssetGLId = null;
        $scope.assetBudgetList = [];
        $scope.assetActivityList = [];
        $scope.debitNoteTypeGivenGL.AssetBudgetMasterId = null;
        $scope.debitNoteTypeGivenGL.AssetActivityId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.debitNoteTypeGivenGL.COAId, $scope.debitNoteTypeGivenGL.AssetGLId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.debitNoteTypeGivenGL.AssetBudgetMasterId, function (result) {
            $scope.assetActivityList = result;
        });
    };
    // #endregion

    // #region ******RevenueType******
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
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getRevenueTypeList = function () {
        if ($scope.debitNoteTypeGivenGL.COAId === null || $scope.debitNoteTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }

        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.debitNoteTypeGivenGL.COAId;
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
        $scope.debitNoteTypeGivenGL.RevenueGLId = x.GLGeneralInfoId;
        getRevenueBudget();
    };

    $scope.refreshRevenueGL = function () {
        $scope.RevenueGLInof = null;
        $scope.debitNoteTypeGivenGL.RevenueGLId = null;
        $scope.revenueBudgetList = [];
        $scope.revenueActivityList = [];
        $scope.debitNoteTypeGivenGL.RevenueBudgetMasterId = null;
        $scope.debitNoteTypeGivenGL.RevenueActivityId = null;
    };

    $scope.revenueBudgetList = [];
    function getRevenueBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.debitNoteTypeGivenGL.COAId, $scope.debitNoteTypeGivenGL.RevenueGLId, function (result) {
            $scope.revenueBudgetList = result;
        });
    }

    $scope.revenueActivityList = [];
    $scope.getRevenueActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.debitNoteTypeGivenGL.RevenueBudgetMasterId, function (result) {
            $scope.revenueActivityList = result;
        });
    };

    // #region ******LiablityType GL******
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
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getLiabilityTypeList = function () {
        if ($scope.debitNoteTypeGivenGL.COAId === null || $scope.debitNoteTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.debitNoteTypeGivenGL.COAId;
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
        $scope.debitNoteTypeGivenGL.LiabilityGLId = x.GLGeneralInfoId;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.debitNoteTypeGivenGL.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.debitNoteTypeGivenGL.LiabilityBudgetMasterId = null;
        $scope.debitNoteTypeGivenGL.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.debitNoteTypeGivenGL.COAId, $scope.debitNoteTypeGivenGL.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.debitNoteTypeGivenGL.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
        });
    };
    // #endregion

    // #region ******ExpensesType GL******
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
        if ($scope.debitNoteTypeGivenGL.COAId === null || $scope.debitNoteTypeGivenGL.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.debitNoteTypeGivenGL.COAId;
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
        $scope.debitNoteTypeGivenGL.ExpensesGLId = x.GLGeneralInfoId;
        getExpensesBudget();
    };

    $scope.refreshExpensesGL = function () {
        $scope.ExpensesGLInof = null;
        $scope.debitNoteTypeGivenGL.GLGeneralInfoId = null;
        $scope.expensesBudgetList = [];
        $scope.expensesActivityList = [];
        $scope.debitNoteTypeGivenGL.ExpensesBudgetMasterId = null;
        $scope.debitNoteTypeGivenGL.ExpensesActivityId = null;
    };

    $scope.expensesBudgetList = [];
    function getExpensesBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.debitNoteTypeGivenGL.COAId, $scope.debitNoteTypeGivenGL.ExpensesGLId, function (result) {
            $scope.expensesBudgetList = result;
        });
    }

    $scope.expensesActivityList = [];
    $scope.getExpensesActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.debitNoteTypeGivenGL.ExpensesBudgetMasterId, function (result) {
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
                if ($scope.debitNoteTypeGivenGLWithCombineList[i].Id == $scope.glUntagId) {
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
            AssetUserName: $scope.debitNoteTypeGivenGLWithCombineList[i].AssetUserName,
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
                if (response.data.Error == true) {
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
                if ($scope.debitNoteTypeGivenGL.AssetGLId !== null) {
                    item.AssetGLId = $scope.debitNoteTypeGivenGL.AssetGLId;
                }
                if ($scope.debitNoteTypeGivenGL.AssetActivityId !== null) {
                    item.AssetActivityId = $scope.debitNoteTypeGivenGL.AssetActivityId;
                }
                if ($scope.debitNoteTypeGivenGL.AssetBudgetMasterId !== null) {
                    item.AssetBudgetMasterId = $scope.debitNoteTypeGivenGL.AssetBudgetMasterId;
                }
                if ($scope.debitNoteTypeGivenGL.RevenueGLId !== null) {
                    item.RevenueGLId = $scope.debitNoteTypeGivenGL.RevenueGLId;
                }
                if ($scope.debitNoteTypeGivenGL.RevenueActivityId !== null) {
                    item.RevenueActivityId = $scope.debitNoteTypeGivenGL.RevenueActivityId;
                }
                if ($scope.debitNoteTypeGivenGL.RevenueBudgetMasterId !== null) {
                    item.RevenueBudgetMasterId = $scope.debitNoteTypeGivenGL.RevenueBudgetMasterId;
                }
                if ($scope.debitNoteTypeGivenGL.LiabilityGLId !== null) {
                    item.LiabilityGLId = $scope.debitNoteTypeGivenGL.LiabilityGLId;
                }
                if ($scope.debitNoteTypeGivenGL.LiabilityActivityId !== null) {
                    item.LiabilityActivityId = $scope.debitNoteTypeGivenGL.LiabilityActivityId;
                }
                if ($scope.debitNoteTypeGivenGL.LiabilityBudgetMasterId !== null) {
                    item.LiabilityBudgetMasterId = $scope.debitNoteTypeGivenGL.LiabilityBudgetMasterId;
                }
                if ($scope.debitNoteTypeGivenGL.ExpensesGLId !== null) {
                    item.ExpensesGLId = $scope.debitNoteTypeGivenGL.ExpensesGLId;
                }
                if ($scope.debitNoteTypeGivenGL.ExpensesActivityId !== null) {
                    item.ExpensesActivityId = $scope.debitNoteTypeGivenGL.ExpensesActivityId;
                }
                if ($scope.debitNoteTypeGivenGL.ExpensesBudgetMasterId !== null) {
                    item.ExpensesBudgetMasterId = $scope.debitNoteTypeGivenGL.ExpensesBudgetMasterId;
                }
                item.COAId = $scope.debitNoteTypeGivenGL.COAId;
                $scope.debitNoteTypeGivenGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.debitNoteTypeGivenGLListForSave.length < 1) {
            return showresult("please select debit note type given!", 'failure');
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
        $scope.debitNoteTypeGivenGL = { COAId: $scope.debitNoteTypeGivenGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.debitNoteTypeGivenGLWithCombineList = [];
    }
}