'use strict';
interTransactionTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function interTransactionTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Inter Transaction Type GL';
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.loanTypeGivenGLList = [];
    $scope.loanTypeGivenGLWithCombineList = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.saveUrl = $scope.path + 'SaveFinancingTypeGL';
    $scope.interTransactionTypeGL = {
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
        LiabilityActivityId: null
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
            if ($scope.interTransactionTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetInterTransactionTypeGLAllList?coaId=' + $scope.interTransactionTypeGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.interTransactionTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetInterTransactionTypeGLNotAssingList?coaId=' + $scope.interTransactionTypeGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.interTransactionTypeGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetInterTransactionTypeGLAssingList?coaId=' + $scope.interTransactionTypeGL.COAId;
        }
        $scope.interTransactionTypeGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'AssetUserName', 'AssetUserName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.interTransactionTypeGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.interTransactionTypeGLWithCombineList.length; i++) {
                        $scope.interTransactionTypeGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.interTransactionTypeGLWithCombineList[i].InvestmentTypeGivenId);
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
        if ($scope.interTransactionTypeGL.COAId === null || $scope.interTransactionTypeGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetCustomerReconeGLCOAWise?coaId=' + $scope.interTransactionTypeGL.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#assetTypeListPopUp')).modal('hide');
        }
    };

    $scope.setAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.AssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.interTransactionTypeGL.AssetGLId = x.GLGeneralInfoId;
        getAssetBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.AssetGLInof = null;
        $scope.interTransactionTypeGL.AssetGLId = null;
        $scope.assetBudgetList = [];
        $scope.assetActivityList = [];
        $scope.interTransactionTypeGL.AssetBudgetMasterId = null;
        $scope.interTransactionTypeGL.AssetActivityId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.interTransactionTypeGL.COAId, $scope.interTransactionTypeGL.AssetGLId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.interTransactionTypeGL.AssetBudgetMasterId, function (result) {
            $scope.assetActivityList = result;
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
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getLiabilityTypeList = function () {
        if ($scope.interTransactionTypeGL.COAId === null || $scope.interTransactionTypeGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorReconeGLCOAWise?coaId=' + $scope.interTransactionTypeGL.COAId;
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
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.LiabilityGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.interTransactionTypeGL.LiabilityGLId = x.GLGeneralInfoId;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.interTransactionTypeGL.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.interTransactionTypeGL.LiabilityBudgetMasterId = null;
        $scope.interTransactionTypeGL.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.interTransactionTypeGL.COAId, $scope.interTransactionTypeGL.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.interTransactionTypeGL.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
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
        for (var i = 0; i < $scope.interTransactionTypeGLWithCombineList.length; i++) {
            if ($scope.glUntagId != null) {
                if ($scope.interTransactionTypeGLWithCombineList[i].Id == $scope.glUntagId) {
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
        $scope.interTransactionTypeGLWithCombineList[i] = {
            AssetUserName: $scope.interTransactionTypeGLWithCombineList[i].AssetUserName,
            COAId: $scope.interTransactionTypeGLWithCombineList[i].COAId,
            COAName: $scope.interTransactionTypeGLWithCombineList[i].COAName,
            Code: $scope.interTransactionTypeGLWithCombineList[i].Code,
            FinancingTypeId: $scope.interTransactionTypeGLWithCombineList[i].FinancingTypeId
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
        $scope.interTransactionTypeGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.interTransactionTypeGL.AssetGLId != null) {
                    item.AssetGLId = $scope.interTransactionTypeGL.AssetGLId;
                }
                if ($scope.interTransactionTypeGL.AssetActivityId != null) {
                    item.AssetActivityId = $scope.interTransactionTypeGL.AssetActivityId;
                }
                if ($scope.interTransactionTypeGL.AssetBudgetMasterId != null) {
                    item.AssetBudgetMasterId = $scope.interTransactionTypeGL.AssetBudgetMasterId;
                }
                if ($scope.interTransactionTypeGL.RevenueGLId != null) {
                    item.RevenueGLId = $scope.interTransactionTypeGL.RevenueGLId;
                }
                if ($scope.interTransactionTypeGL.RevenueActivityId != null) {
                    item.RevenueActivityId = $scope.interTransactionTypeGL.RevenueActivityId;
                }
                if ($scope.interTransactionTypeGL.RevenueBudgetMasterId != null) {
                    item.RevenueBudgetMasterId = $scope.interTransactionTypeGL.RevenueBudgetMasterId;
                }
                if ($scope.interTransactionTypeGL.LiabilityGLId != null) {
                    item.LiabilityGLId = $scope.interTransactionTypeGL.LiabilityGLId;
                }
                if ($scope.interTransactionTypeGL.LiabilityActivityId != null) {
                    item.LiabilityActivityId = $scope.interTransactionTypeGL.LiabilityActivityId;
                }
                if ($scope.interTransactionTypeGL.LiabilityBudgetMasterId != null) {
                    item.LiabilityBudgetMasterId = $scope.interTransactionTypeGL.LiabilityBudgetMasterId;
                }
                if ($scope.interTransactionTypeGL.ExpensesGLId != null) {
                    item.ExpensesGLId = $scope.interTransactionTypeGL.ExpensesGLId;
                }
                if ($scope.interTransactionTypeGL.ExpensesActivityId != null) {
                    item.ExpensesActivityId = $scope.interTransactionTypeGL.ExpensesActivityId;
                }
                if ($scope.interTransactionTypeGL.ExpensesBudgetMasterId != null) {
                    item.ExpensesBudgetMasterId = $scope.interTransactionTypeGL.ExpensesBudgetMasterId;
                }
                item.COAId = $scope.interTransactionTypeGL.COAId;
                $scope.interTransactionTypeGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.interTransactionTypeGLListForSave.length < 1) {
            return ShowResult("Please select Investment Type Given!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.interTransactionTypeGL.AssetGLId) && baseService.isUndefinedOrNull($scope.interTransactionTypeGL.RevenueGLId)) {
            return ShowResult("Please select Asset and Revenue both side GL!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.interTransactionTypeGLForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'financingTypeGLList': $scope.interTransactionTypeGLListForSave
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
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
                $scope.getInvestmentTypeGivenWithCoa('all');
            }
        } else {
            $scope.getInvestmentTypeGivenWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.refreshLiabilityGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.interTransactionTypeGL = { COAId: $scope.interTransactionTypeGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.interTransactionTypeGLWithCombineList = [];
    }
}