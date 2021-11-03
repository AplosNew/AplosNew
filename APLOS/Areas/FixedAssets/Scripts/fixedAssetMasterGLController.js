'use strict';
fixedAssetMasterGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function fixedAssetMasterGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "FixedAsset Class";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.fixedasseGroupList = [];
    $scope.fixedAssetSelectList = [];
    $scope.fixedAssetGLList = [];
    $scope.selectFixedAssetMasterWithCombineList = [];
    $scope.ReconAssetTypeGLList = [];
    $scope.AccDepreciationGLTypeList = [];
    $scope.DepreciationTypeGLList = [];
    $scope.AUCGLTypeList = [];
    if ($scope.selectFixedAssetMasterWithCombineList.length > 0) {
        $scope.tableShow = true;
    } else {
        $scope.tableShow = false;
    }
    $scope.fixedAssetGLRowList = [];
    $scope.path = 'FixedAssets/fixedAssetMasterGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateFixedAssetDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.fixedAssetGL = {
        Id: null,
        COAId: null,
        AccumulatedDepreciationGLId: null,
        AccumulatedDepreciationBudgetMasterId: null,
        AccumulatedDepreciationActivityId: null,
        DepreciationGLId: null,
        DepreciationBudgetMasterId: null,
        DepreciationActivityId: null,
        AssetUnderConstructionGLId: null,
        AssetUnderConstructionBudgetMasterId: null,
        AssetUnderConstructionActivityId: null,
        DownPaymentGLId: null,
        DownPaymentBudgetMasterId: null,
        DownPaymentActivityId: null,
        ClearingAccountGLId: null,
        ClearingAccountBudgetMasterId: null,
        ClearingAccountActivityId: null,
        GainOnSaleOfAssetGLId: null,
        GainOnSaleOfAssetBudgetMasterId: null,
        GainOnSaleOfAssetActivityId: null,
        LossOnSaleOfAssetGLId: null,
        LossOnSaleOfAssetBudgetMasterId: null,
        LossOnSaleOfAssetActivityId: null,
        LossOnDisposalAssetGLId: null,
        LossOnDisposalAssetBudgetMasterId: null,
        LossOnDisposalAssetActivityId: null
    };

    $scope.fixedAssetCategoryList = [];
    cboService.getFixedAssetCategoryList(function (result) {
        $scope.fixedAssetCategoryList = result;
    });

    $scope.fixedAssetSubCategoryList = [];
    cboService.getFixedAssetSubCategoryList(function (result) {
        $scope.fixedAssetSubCategoryList = result;
    });

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.getDataWithCoaChange = function () {
        $http.get('FixedAssets/fixedAssetMasterGL/getlistwithcombineCoa')
            .then(function (response) {
                $scope.CompanyDepreciaitonRuleWithCombineListForDdl = response.data.Rows;
                getListForm($scope.CompanyDepreciaitonRuleWithCombineListForDdl);
            });
    };

    $scope.getDataWithCoaChange();

    function IdList() {
        $scope.fixedAssetMasterIdstr = createIdList(validListWithStr($scope.fixedAssetMasterNewList, $scope.fixedAssetMasterIds));
    }

    function createIdList(list) {
        var value = "''";
        for (var i = 0; i < list.length; i++) {
            if (value === "''") {
                value = "'" + list[i].Value + "'";
            } else {
                value += ",'" + list[i].Value + "'";
            }
        }
        return value;
    }
    function getListForm(list) {
        $scope.fixedAssetMasterNewList = createCbo(list, 'FixedAssetMasterId', 'FixedAssetMasterName');
    }
    function createCbo(dblist, value, text) {
        var list = [];
        for (var i = 0; i < dblist.length; i++) {
            if (!ddlFilter(list, dblist[i][value])) {
                list.push({
                    Text: dblist[i][text],
                    Value: dblist[i][value]
                });
            }
        }
        //Sorting with text A-Z
        return list.sort(function (a, b) {
            var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
            var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }

            // names must be equal
            return 0;
        });
    }
    function ddlFilter(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value === id)
                return true;
        }
        return false;
    }
    function newList(oldMainDDlList, values, name) {
        var list = [];
        for (var i = 0; i < oldMainDDlList.length; i++) {
            if (values.length > 0) {
                for (var ii = 0; ii < values.length; ii++) {
                    if (oldMainDDlList[i][name] === values[ii].Value) {
                        list.push({
                            Id: oldMainDDlList[i].Id,
                            FixedAssetMasterName: oldMainDDlList[i].FixedAssetMasterName,
                            FixedAssetMasterId: oldMainDDlList[i].FixedAssetMasterId
                        });
                    }
                }
            }
            else {
                list.push({
                    Id: oldMainDDlList[i].Id,
                    FixedAssetMasterName: oldMainDDlList[i].FixedAssetMasterName,
                    FixedAssetMasterId: oldMainDDlList[i].FixedAssetMasterId
                });
            }
        }
        return list;
    }
    function ddlFilterByDDL(newlist, value, text) {
        var list = [];
        for (var i = 0; i < newlist.length; i++) {
            if (!ddlFilter(list, newlist[i][value])) {
                list.push({
                    Value: newlist[i][value],
                    Text: newlist[i][text]
                });
            }
        }
        return list.sort(function (a, b) {
            var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
            var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }

            // names must be equal
            return 0;
        });
    }
    $scope.cboCratetor = function (val, name) {
        $scope.newList = [];
        $scope.newList = newList($scope.CompanyDepreciaitonRuleWithCombineListForDdl, val, name);
        if (name !== 'FixedAssetMasterId')
            $scope.assetItemNewList = ddlFilterByDDL($scope.newList, 'FixedAssetMasterId', 'FixedAssetMasterName');
        if (name !== 'FixedAssetMasterId')
            $scope.fixedAssetMasterNewList = ddlFilterByDDL($scope.newList, 'FixedAssetMasterId', 'FixedAssetMasterName');
    };

    $scope.multiSelectSettings = {
        scrollableHeight: 'auto',
        smartButtonMaxItems: 3,
        scrollable: true,
        showCheckAll: false,
        showUncheckAll: false,
        enableSearch: true,
        dynamicTitle: true
    };

    $scope.fixedAssetMasterIds = [];
    $scope.multi3events = {
        onItemSelect: function (item) {
            $scope.cboCratetor($scope.fixedAssetMasterIds, 'FixedAssetMasterId');
        }, onItemDeselect: function (item) {
            $scope.cboCratetor($scope.fixedAssetMasterIds, 'FixedAssetMasterId');
        }
    };

    $scope.fixedAssetMasterIds = [];
    $scope.multi4events = {
        onItemSelect: function (item) {
            $scope.cboCratetor($scope.fixedAssetMasterIds, 'FixedAssetMasterId');
        }, onItemDeselect: function (item) {
            $scope.cboCratetor($scope.fixedAssetMasterIds, 'FixedAssetMasterId');
        }
    };
    function validListWithStr(list, values) {
        var tempValues = [];
        for (var i = 0; i < values.length; i++) {
            for (var j = 0; j < list.length; j++) {
                if (values[i].Value === list[j].Value) {
                    tempValues.push(values[i]);
                }
            }
        }
        return tempValues;
    }

    //#endregion
    $scope.tempList = [];
    $scope.tempIdList = [];
    $scope.selectChValueId = function (event, FixedAssetMasterId, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListId($scope.tempList, data.FixedAssetMasterId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].FixedAssetMasterId === data.FixedAssetMasterId) {
                        $scope.tempList.splice(i, 1);
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempListId(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetMasterId === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetMasterId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.setGlLebelHide = function () {
        if ($scope.fixedAssetGL.COAId !== null) {
            $scope.btnActionAll = true;
        } else {
            $scope.btnActionAll = false;
        }
    };

    $scope.GetPartyAccountVDs = [];
    function GetPartyAccountVD(data) {
        $http.get('FixedAssets/fixedAssetMasterGL/getpartyaccountvd')
            .then(function (response) {
                $scope.selectFixedAssetMasterWithCombineList = data;
                $scope.GetPartyAccountVDs = response.data.Rows;
                for (var i = 0; i < $scope.selectFixedAssetMasterWithCombineList.length; i++) {
                    $scope.selectFixedAssetMasterWithCombineList[i].Flag = getActive($scope.tempList, $scope.selectFixedAssetMasterWithCombineList[i].FixedAssetMasterId); //$scope.tempList.includes($scope.selectFixedAssetMasterWithCombineList[i].FixedAssetMasterId)
                }
                angular.forEach($scope.accountGroupList, function (item, j) {
                    for (var i = 0; i < $scope.selectFixedAssetMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectFixedAssetMasterWithCombineList[i].PartyAccountGroupId, $scope.selectFixedAssetMasterWithCombineList[i].Id, item.PartyAccountGroupId);
                        $scope.selectFixedAssetMasterWithCombineList[i]['C' + j + 'GL'] = ob.GL;
                        $scope.selectFixedAssetMasterWithCombineList[i]['C' + j + 'Budget'] = ob.Budget;
                        $scope.selectFixedAssetMasterWithCombineList[i]['C' + j + 'Activity'] = ob.Activity;
                    }
                });
            });
    }

    var AccountDYOb = {
        GL: null,
        Budget: null,
        Activity: null
    };

    function assignDomesticVendor(list, aId, adId, pid) {
        AccountDYOb = {
            GL: null,
            Budget: null,
            Activity: null
        };

        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyAccountGroupId === pid && list[i].FixedAssetMasterGLId === adId && list[i].ClearingAccGLCode !== null) {
                AccountDYOb.GL = list[i].ClearingAccGLCode + "-" + list[i].ClearingAccGLText;
                AccountDYOb.Budget = list[i].BudgetName;
                AccountDYOb.Activity = list[i].ActivityName;
                break;
            }
        }
        return AccountDYOb;
    }

    $scope.getFixedAssetMasterWithCoa = function (str) {
        IdList();
        $scope.selectFixedAssetMasterWithCombineList = [];
        if ($scope.selectFixedAssetMasterWithCombineList.length > 0) {
            $scope.tableShow = true;
        } else {
            $scope.tableShow = false;
        }
        if (str === 'all') {
            if ($scope.fixedAssetGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'FixedAssets/fixedAssetMasterGL/getlistwithcombine?coaId=' + $scope.fixedAssetGL.COAId + '&fixedAssetMasterIds=' + $scope.fixedAssetMasterIdstr;
        }
        if (str === 'notassing') {
            $scope.btnActionAll = true;
            if ($scope.fixedAssetGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'FixedAssets/fixedAssetMasterGL/getlistwithcombinenotassing?coaId=' + $scope.fixedAssetGL.COAId + '&fixedAssetMasterIds=' + $scope.fixedAssetMasterIdstr;
        }
        if (str === 'assing') {
            $scope.btnActionAll = true;
            if ($scope.fixedAssetGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'FixedAssets/fixedAssetMasterGL/getlistwithcombineassing?coaId=' + $scope.fixedAssetGL.COAId + '&fixedAssetMasterIds=' + $scope.fixedAssetMasterIdstr;
        }
        baseService.setCurrentPage('selectFixedAssetMasterWithCombineList');
        baseService.init($scope.url, null, null, null, 'FixedAssetMasterName', 'FixedAssetMasterName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    if (result.Rows.length > 0) {
                        GetPartyAccountVD(result.Rows);
                    }
                    if (result.Rows.length > 0) {
                        $scope.tableShow = true;
                    } else {
                        $scope.tableShow = false;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.searchDepreciationTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.depreciationListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDepreciationTypeList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/getdepriciationexpensesgl?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetDepreciationTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.depreciationListParameters)
                .then(function (data) {
                    $scope.DepreciationTypeGLList = data.Rows;
                    $scope.depreciationListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#DepreciationTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetDepreciationTypeListData();
    };
    $scope.closeDepreciationTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#DepreciationTypeListPopUp')).modal('hide');
        }
    };
    $scope.setDepreciationGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.DeprectionGlData = x;
        $scope.DepreciationGLInfo = x.GLGeneralInfoName;
        $scope.DepreciationGLId = x.GLGeneralInfoId;
        getDepreciationBudget();
    };
    $scope.refreshDepreciationGL = function () {
        $scope.DepreciationGLInfo = null;
        $scope.DepreciationGLId = null;
    };

    $scope.depreciationBudgetList = [];
    function getDepreciationBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.DepreciationGLId, function (result) {
            $scope.depreciationBudgetList = result;
        });
    }

    $scope.depreciationActivityList = [];
    $scope.getDepreciationActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.DepreciationBudgetMasterId, function (result) {
            $scope.depreciationActivityList = result;
        });
    };

    $scope.searchAUCGLTypeByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.aUCGLTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetAUCGLTypeList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl2 = 'accounts/glitem/getaucgl?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetAUCGLTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl2, pageno, $scope.aUCGLTypeListParameters)
                .then(function (data) {
                    $scope.AUCGLTypeList = data.Rows;
                    $scope.aUCGLTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#AUCGLTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetAUCGLTypeListData();
    };
    $scope.closeAUCGLTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#AUCGLTypeListPopUp')).modal('hide');
        }
    };
    $scope.setAUCGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AUCGSelectedData = x;
        $scope.AUCGLInfo = x.GLGeneralInfoName;
        $scope.AssetUnderConstructionGLId = x.GLGeneralInfoId;
        getAUCGLBudget();
    };

    $scope.refreshAUCGL = function () {
        $scope.AUCGLInfo = null;
        $scope.AssetUnderConstructionGLId = null;
    };

    $scope.aUCGLBudgetList = [];
    function getAUCGLBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.AssetUnderConstructionGLId, function (result) {
            $scope.aUCGLBudgetList = result;
        });
    }

    $scope.aUCGLActivityList = [];
    $scope.getAUCGLActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.AssetUnderConstructionBudgetMasterId, function (result) {
            $scope.aUCGLActivityList = result;
        });
    };

    $scope.searchAccDepreciationTypeByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.accDepreciationTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetAccDepreciationGLTypeList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl3 = 'accounts/glitem/getaccomultatedepriciationgl?coaId=' + $scope.fixedAssetGL.COAId;
        baseService.setCurrentPage('AccDepreciationGLTypeList');
        $scope.GetAccDepreciationGLTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.accDepreciationTypeListParameters)
                .then(function (data) {
                    $scope.AccDepreciationGLTypeList = data.Rows;
                    $scope.accDepreciationTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#AccDepreciationTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetAccDepreciationGLTypeListData();
    };

    $scope.closeAccDepreciationTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#AccDepreciationTypeListPopUp')).modal('hide');
        }
    };

    $scope.setAccDepreciationGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AccDepreciationGlData = x;
        $scope.AccDepreciationGLInfo = x.GLGeneralInfoName;
        $scope.AccumulatedDepreciationGLId = x.GLGeneralInfoId;
        getAccDepreciationBudget();
    };
    $scope.refreshAccDepreciationGL = function () {
        $scope.AccDepreciationGLInfo = null;
        $scope.AccumulatedDepreciationGLId = null;
    };

    $scope.accDepreciationBudgetList = [];
    function getAccDepreciationBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.AccumulatedDepreciationGLId, function (result) {
            $scope.accDepreciationBudgetList = result;
        });
    }

    $scope.accDepreciationActivityList = [];
    $scope.getAccDepreciationActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.AccumulatedDepreciationBudgetMasterId, function (result) {
            $scope.accDepreciationActivityList = result;
        });
    };

    $scope.searchDownPaymentByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.downPaymentListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDownPaymentGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorDownpaymentGLCOAWise?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetDownPaymentListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.downPaymentListParameters)
                .then(function (data) {
                    $scope.DownPaymentGlList = data.Rows;
                    $scope.downPaymentListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#DownPaymentListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetDownPaymentListData();
    };
    $scope.closeDownPaymentListPopUpSelected = function () {
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#DownPaymentListPopUp')).modal('hide');
        }
    };
    $scope.setDownPaymentGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.DownPaymentGLInfo = x.GLGeneralInfoName;
        $scope.DownPaymentGLId = x.GLGeneralInfoId;
        getDownPaymentBudget();
    };
    $scope.refreshDownPaymentGL = function () {
        $scope.DownPaymentGLInfo = null;
        $scope.DownPaymentGLId = null;
    };

    $scope.downPaymentBudgetList = [];
    function getDownPaymentBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.DownPaymentGLId, function (result) {
            $scope.downPaymentBudgetList = result;
        });
    }
    $scope.downPaymentActivityList = [];
    $scope.getDownPaymentActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.DownPaymentBudgetMasterId, function (result) {
            $scope.downPaymentActivityList = result;
        });
    };

    $scope.searchClearingAccountByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.clearingAccountListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetClearingAccountGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetClearingAccountGL?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetClearingAccountListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.clearingAccountListParameters)
                .then(function (data) {
                    $scope.ClearingAccountGlList = data.Rows;
                    $scope.clearingAccountListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ClearingAccountListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetClearingAccountListData();
    };
    $scope.closeClearingAccountListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#ClearingAccountListPopUp')).modal('hide');
        }
    };
    $scope.setClearingAccountGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.ClearingAccGLInfo = x.GLGeneralInfoName;
        $scope.ClearingAccountGLId = x.GLGeneralInfoId;
        getclearingAccountBudget();
    };
    $scope.deleteDocumentSetPopup = function (ob, index) {
        try {
            $scope.message_confirmation = 'Are you sure to delete';
            angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
            $scope.documentSetIndex = index;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeFADeterminateSetYes = function () {
        $scope.fixedAssetGLRowList.splice($scope.documentSetIndex, 1);
        $scope.documentSetIndex = -1;
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
    };

    $scope.refreshClearingAccountGL = function () {
        $scope.ClearingAccGLInfo = null;
        $scope.ClearingAccountGLId = null;
    };

    $scope.clearingAccountBudgetList = [];
    function getclearingAccountBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.ClearingAccountGLId, function (result) {
            $scope.clearingAccountBudgetList = result;
        });
    }
    $scope.clearingAccountActivityList = [];
    $scope.getClearingAccActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.ClearingAccountBudgetMasterId, function (result) {
            $scope.clearingAccountActivityList = result;
        });
    };

    $scope.getChildHeaderAcGroup = function () {
        $scope.childHeaderAcG = [];
        angular.forEach($scope.accountGroupList, function (item, i) {
            $scope.childHeaderAcG.push('GL');
            $scope.childHeaderAcG.push('Budget');
            $scope.childHeaderAcG.push('Activity');
        });
    };

    $scope.NewListA = [];
    $scope.buildValue = function () {
        $scope.NewListA = [];
        angular.forEach($scope.accountGroupList, function (item, i) {
            $scope.NewListA.push('C' + i + "GL");
            $scope.NewListA.push('C' + i + "Budget");
            $scope.NewListA.push('C' + i + "Activity");
        });
    };

    $scope.loadAccountGroup = function (pageno) {
        baseService.init('Parties/PartyAccountGroup/GetList?accountType=' + 'Vendor', null, null, null, 'UserName', 'UserName');
        $scope.accountGroupList = [];
        baseService.pagination(pageno)
            .then(function (result) {
                angular.forEach(result.Rows, function (item) {
                    $scope.accountGroupList.push(
                        {
                            Id: null,
                            PartyAccountGroupId: item.Id,
                            Code: item.Code,
                            UserName: item.UserName,
                            FixedAssetMasterGLId: null,
                            VendorReconGLId: null,
                            VendorReconGLCode: null,
                            VendorRecontGLText: null,
                            VendorReconBudgetMasterId: null,
                            VendorReconActivityId: null
                        }
                    );
                });
                $scope.getChildHeaderAcGroup();
                $scope.buildValue();
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.loadAccountGroup();

    function getValue(partyAcId, fixedAssetMasterGlId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetPartyAccountWithAssignList?partyAcId=' + partyAcId + '&fixedAssetMasterGlId=' + fixedAssetMasterGlId,
        }).then(function successCallback(response) {
            return response.data[0].GL;
        });
    }

    $scope.searchVendorReconByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.vendorReconListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.vendorReconGLSelectedList = [];
    $scope.GetVendorReconGlList = function (index) {
        $scope.accIndex = index;
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorReconeGLCOAWise?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetVendorReconListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.vendorReconListParameters)
                .then(function (data) {
                    $scope.VendorReconGlList = data.Rows;
                    $scope.vendorReconListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#VendorReconListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetVendorReconListData();
    };

    $scope.closeVendorReconListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#VendorReconListPopUp')).modal('hide');
        }
        $scope.accIndex = -1;
    };

    $scope.setVendorReconGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.accountGroupList[$scope.accIndex].VendorReconGLId = x.GLGeneralInfoId;
        $scope.accountGroupList[$scope.accIndex].VendorReconGLCode = x.GLGeneralInfoCode;
        $scope.accountGroupList[$scope.accIndex].VendorRecontGLText = x.GLItem;
        $scope.accountGroupList[$scope.accIndex].VendorReconBudgetMasterId = null;
        $scope.accountGroupList[$scope.accIndex].VendorReconActivityId = null;
        getVendorReconBudget(x.GLGeneralInfoId, $scope.accIndex);
    };

    $scope.refreshAccGroup = function () {
        angular.forEach($scope.accountGroupList, function (item) {
            item.VendorReconGLId = null;
            item.VendorReconGLCode = null;
            item.VendorRecontGLText = null;
            item.VendorReconBudgetMasterId = null;
            item.VendorReconActivityId = null;
            item.VendorReconBudgetList = [];
            item.VendorReconActivityList = [];
        });
    };

    function checkExistVendorRecon(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].VendorReconGLId === id) {
                return true;
            }
        }
        return false;
    }

    function getVendorReconBudget(id, index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, id, function (result) {
            $scope.accountGroupList[index].VendorReconBudgetList = result;
        });
    }

    $scope.getVendorReconActivity = function (id, index, GLGeneralInfoId) {
        cboService.getBudgetMasterActivityCbo(id, function (result) {
            $scope.accountGroupList[index].VendorReconActivityList = result;
        });
    };

    $scope.searchGainOnSaleAssetByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.gainOnSaleAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GainOnSaleAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetGainOnSaleAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.gainOnSaleAssetListParameters)
                .then(function (data) {
                    $scope.GainOnSaleAssetList = data.Rows;
                    $scope.gainOnSaleAssetListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GainOnSaleAssetGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetGainOnSaleAssetTypeListData();
    };

    $scope.closeGainOnSaleAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#GainOnSaleAssetGLListPopUp')).modal('hide');
        }
    };

    $scope.setGainOnSaleAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.GainOnSaleAssetGLSelectedData = x;
        $scope.GainOnSaleAssetGLInfo = x.GLGeneralInfoName;
        $scope.GainOnSaleAssetGLId = x.GLGeneralInfoId;
        getGainOnSaleAssetBudget();
    };

    $scope.refreshGainOnSaleAssetGL = function () {
        $scope.GainOnSaleAssetGLInfo = null;
        $scope.GainOnSaleAssetGLId = null;
    };

    $scope.gainOnSaleAssetBudgetList = [];
    function getGainOnSaleAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.GainOnSaleAssetGLId, function (result) {
            $scope.gainOnSaleAssetBudgetList = result;
        });
    }

    $scope.gainOnSaleAssetActivityList = [];
    $scope.getGainOnSaleAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.GainOnSaleAssetBudgetMasterId, function (result) {
            $scope.gainOnSaleAssetActivityList = result;
        });
    };

    $scope.searchLossOnSaleAssetByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.lossOnSaleAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.LossOnSaleAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetDepriciationExpensesGL?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetLossOnSaleAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.lossOnSaleAssetListParameters)
                .then(function (data) {
                    $scope.LossOnSaleAssetList = data.Rows;
                    $scope.lossOnSaleAssetListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#LossOnSaleAssetGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetLossOnSaleAssetTypeListData();
    };

    $scope.closeLossOnSaleAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#LossOnSaleAssetGLListPopUp')).modal('hide');
        }
    };

    $scope.setLossOnSaleAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.LossOnSaleAssetGLSelectedData = x;
        $scope.LossOnSaleAssetGLInfo = x.GLGeneralInfoName;
        $scope.LossOnSaleAssetGLId = x.GLGeneralInfoId;
        getLossOnSaleAssetBudget();
    };

    $scope.refreshLossOnSaleAssetGL = function () {
        $scope.LossOnSaleAssetGLInfo = null;
        $scope.LossOnSaleAssetGLId = null;
    };

    $scope.lossOnSaleAssetBudgetList = [];
    function getLossOnSaleAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.LossOnSaleAssetGLId, function (result) {
            $scope.lossOnSaleAssetBudgetList = result;
        });
    }

    $scope.lossOnSaleAssetActivityList = [];
    $scope.getLossOnSaleAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.LossOnSaleAssetBudgetMasterId, function (result) {
            $scope.lossOnSaleAssetActivityList = result;
        });
    };

    $scope.searchLossOnDisposalAssetByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.lossOnDisposalAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LossOnDisposalAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetDepriciationExpensesGL?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetLossOnDisposalAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.lossOnDisposalAssetListParameters)
                .then(function (data) {
                    $scope.LossOnDisposalAssetList = data.Rows;
                    $scope.lossOnDisposalAssetListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#LossOnDisposalAssetGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetLossOnDisposalAssetTypeListData();
    };

    $scope.closeLossOnDisposalAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#LossOnDisposalAssetGLListPopUp')).modal('hide');
        }
    };

    $scope.setLossOnDisposalAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.LossOnDisposalAssetGLSelectedData = x;
        $scope.LossOnDisposalAssetGLInfo = x.GLGeneralInfoName;
        $scope.LossOnDisposalAssetGLId = x.GLGeneralInfoId;
        getLossOnDisposalAssetBudget();
    };
    $scope.refreshLossOnDisposalAssetGL = function () {
        $scope.LossOnDisposalAssetGLInfo = null;
        $scope.LossOnDisposalAssetGLId = null;
    };

    $scope.lossOnDisposalAssetBudgetList = [];
    function getLossOnDisposalAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.LossOnDisposalAssetGLId, function (result) {
            $scope.lossOnDisposalAssetBudgetList = result;
        });
    }

    $scope.lossOnDisposalAssetActivityList = [];
    $scope.getLossOnDisposalAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.LossOnDisposalAssetBudgetMasterId, function (result) {
            $scope.lossOnDisposalAssetActivityList = result;
        });
    };

    $scope.lessValueAssetList = [];
    $scope.searchLessValueAssetByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.lessValueAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LessValueAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWiseExceptRecon?coaId=' + $scope.fixedAssetGL.COAId;
        baseService.setCurrentPage('lessValueAssetList');
        $scope.GetLessValueAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.lessValueAssetListParameters)
                .then(function (data) {
                    $scope.lessValueAssetList = data.Rows;
                    $scope.lessValueAssetListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#LessValueAssetGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetLessValueAssetTypeListData();
    };
    $scope.closeLessValueAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#LessValueAssetGLListPopUp')).modal('hide');
        }
    };
    $scope.setLessValueAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.LessValueAssetGLSelectedData = x;
        $scope.LessValueAssetGLInfo = x.GLGeneralInfoName;
        $scope.LessValueAssetGLId = x.GLGeneralInfoId;
        getLessValueAssetBudget();
    };

    $scope.refreshLessValueAssetGL = function () {
        $scope.LessValueAssetGLInfo = null;
        $scope.LessValueAssetGLId = null;
    };

    $scope.lessValueAssetBudgetList = [];
    function getLessValueAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.fixedAssetGL.COAId, $scope.LessValueAssetGLId, function (result) {
            $scope.lessValueAssetBudgetList = result;
        });
    }

    $scope.lessValueAssetActivityList = [];
    $scope.getLessValueAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.fixedAssetGL.LessValueAssetBudgetMasterId, function (result) {
            $scope.lessValueAssetActivityList = result;
        });
    };

    $scope.closeFixedAssetGLListPopUpSelected = function () {
        $scope.fixedAssetGLRowList = [];
        angular.forEach($scope.selectFixedAssetMasterWithCombineList, function (item) {
            if (item.Active) {
                if (checkIsAvailable($scope.fixedAssetGLRowList, item.FixedAssetMasterId) === false) {
                    $scope.fixedAssetGLRowList.push(item);
                }
            }
        });

        if ($scope.fixedAssetGLRowList.length > 0) {
            angular.element(document.querySelector('#itemsearchpopup')).modal('hide');
            $scope.selectedPayableTblShow = true;
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    function checkIsAvailable(list, FixedAssetMasterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetMasterId === FixedAssetMasterId) {
                return true;
            }
        }
        return false;
    }

    $scope.addGlForSelectble = function () {
        $scope.fixedAssetGLListForSave = [];
        //$scope.tempList = [];
        //angular.forEach($scope.selectFixedAssetMasterWithCombineList, function (item) {
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.AccumulatedDepreciationGLId != null) {
                    item.AccumulatedDepreciationGLId = $scope.AccumulatedDepreciationGLId;
                }
                if ($scope.fixedAssetGL.AccumulatedDepreciationBudgetMasterId != null) {
                    item.AccumulatedDepreciationBudgetMasterId = $scope.fixedAssetGL.AccumulatedDepreciationBudgetMasterId;
                }
                if ($scope.fixedAssetGL.AccumulatedDepreciationActivityId != null) {
                    item.AccumulatedDepreciationActivityId = $scope.fixedAssetGL.AccumulatedDepreciationActivityId;
                }
                if ($scope.DepreciationGLId != null) {
                    item.DepreciationGLId = $scope.DepreciationGLId;
                }
                if ($scope.fixedAssetGL.DepreciationBudgetMasterId != null) {
                    item.DepreciationBudgetMasterId = $scope.fixedAssetGL.DepreciationBudgetMasterId;
                }
                if ($scope.fixedAssetGL.DepreciationActivityId != null) {
                    item.DepreciationActivityId = $scope.fixedAssetGL.DepreciationActivityId;
                }
                if ($scope.AssetUnderConstructionGLId != null) {
                    item.AssetUnderConstructionGLId = $scope.AssetUnderConstructionGLId;
                }
                if ($scope.fixedAssetGL.AssetUnderConstructionBudgetMasterId != null) {
                    item.AssetUnderConstructionBudgetMasterId = $scope.fixedAssetGL.AssetUnderConstructionBudgetMasterId;
                }
                if ($scope.fixedAssetGL.AssetUnderConstructionActivityId != null) {
                    item.AssetUnderConstructionActivityId = $scope.fixedAssetGL.AssetUnderConstructionActivityId;
                }
                if ($scope.DownPaymentGLId != null) {
                    item.DownPaymentGLId = $scope.DownPaymentGLId;
                }
                if ($scope.fixedAssetGL.DownPaymentBudgetMasterId != null) {
                    item.DownPaymentBudgetMasterId = $scope.fixedAssetGL.DownPaymentBudgetMasterId;
                }
                if ($scope.fixedAssetGL.DownPaymentActivityId != null) {
                    item.DownPaymentActivityId = $scope.fixedAssetGL.DownPaymentActivityId;
                }
                if ($scope.ClearingAccountGLId != null) {
                    item.ClearingAccountGLId = $scope.ClearingAccountGLId;
                }
                if ($scope.fixedAssetGL.ClearingAccountBudgetMasterId != null) {
                    item.ClearingAccountBudgetMasterId = $scope.fixedAssetGL.ClearingAccountBudgetMasterId;
                }
                if ($scope.fixedAssetGL.ClearingAccountActivityId != null) {
                    item.ClearingAccountActivityId = $scope.fixedAssetGL.ClearingAccountActivityId;
                }
                if ($scope.GainOnSaleAssetGLId != null) {
                    item.GainOnSaleOfAssetGLId = $scope.GainOnSaleAssetGLId;
                }
                if ($scope.fixedAssetGL.GainOnSaleAssetBudgetMasterId != null) {
                    item.GainOnSaleAssetBudgetMasterId = $scope.fixedAssetGL.GainOnSaleAssetBudgetMasterId;
                    item.GainOnSaleOfAssetBudgetMasterId = $scope.fixedAssetGL.GainOnSaleAssetBudgetMasterId;
                }
                if ($scope.fixedAssetGL.GainOnSaleAssetActivityId != null) {
                    item.GainOnSaleAssetActivityId = $scope.fixedAssetGL.GainOnSaleAssetActivityId;
                    item.GainOnSaleOfAssetActivityId = $scope.fixedAssetGL.GainOnSaleAssetActivityId;
                }
                if ($scope.LossOnSaleAssetGLId != null) {
                    item.LossOnSaleOfAssetGLId = $scope.LossOnSaleAssetGLId;
                }
                if ($scope.fixedAssetGL.LossOnSaleAssetBudgetMasterId != null) {
                    item.LossOnSaleAssetBudgetMasterId = $scope.fixedAssetGL.LossOnSaleAssetBudgetMasterId;
                    item.LossOnSaleOfAssetBudgetMasterId = $scope.fixedAssetGL.LossOnSaleAssetBudgetMasterId;
                }
                if ($scope.fixedAssetGL.LossOnSaleAssetActivityId != null) {
                    item.LossOnSaleAssetActivityId = $scope.fixedAssetGL.LossOnSaleAssetActivityId;
                    item.LossOnSaleOfAssetActivityId = $scope.fixedAssetGL.LossOnSaleAssetActivityId;
                }
                if ($scope.LossOnDisposalAssetGLId != null) {
                    item.LossOnDisposalAssetGLId = $scope.LossOnDisposalAssetGLId;
                    ///for combine db
                }
                if ($scope.fixedAssetGL.LossOnDisposalAssetBudgetMasterId != null) {
                    item.LossOnDisposalAssetBudgetMasterId = $scope.fixedAssetGL.LossOnDisposalAssetBudgetMasterId;
                }
                if ($scope.fixedAssetGL.LossOnDisposalAssetActivityId != null) {
                    item.LossOnDisposalAssetActivityId = $scope.fixedAssetGL.LossOnDisposalAssetActivityId;
                }
                if ($scope.LessValueAssetGLId != null) {
                    item.LessValueAssetGLId = $scope.LessValueAssetGLId;
                }
                if ($scope.fixedAssetGL.LessValueAssetBudgetMasterId != null) {
                    item.LessValueAssetBudgetMasterId = $scope.fixedAssetGL.LessValueAssetBudgetMasterId;
                }
                if ($scope.fixedAssetGL.LessValueAssetActivityId != null) {
                    item.LessValueAssetActivityId = $scope.fixedAssetGL.LessValueAssetActivityId;
                }
                item.COAId = $scope.fixedAssetGL.COAId;
                $scope.fixedAssetGLListForSave.push(item);
            }
        });
    }

    function checkVendorReconGLIsAssinged(list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].VendorReconGLId != null) {
                return false;
                break;
            }
        }
        return true;
    }

    $scope.Save = function () {
        $scope.closeFixedAssetGLListPopUpSelected();
        $scope.addGlForSelectble();
        if (baseService.isUndefinedOrNull($scope.AssetGLId) && baseService.isUndefinedOrNull($scope.AccumulatedDepreciationGLId) && baseService.isUndefinedOrNull($scope.DepreciationGLId) && baseService.isUndefinedOrNull($scope.AssetUnderConstructionGLId) && baseService.isUndefinedOrNull($scope.DownPaymentGLId) && baseService.isUndefinedOrNull($scope.ClearingAccountGLId) && baseService.isUndefinedOrNull($scope.GainOnSaleAssetGLId) && baseService.isUndefinedOrNull($scope.LossOnSaleAssetGLId) && baseService.isUndefinedOrNull($scope.LossOnDisposalAssetGLId) && checkVendorReconGLIsAssinged($scope.accountGroupList)) {
            return ShowResult("Please Select at least one GL!!", 'failure');
        }
        if ($scope.fixedAssetGLListForSave.length < 1) {
            return ShowResult("No list found!!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fixedAssetGLNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'fixedAssetMasterGL': $scope.fixedAssetGLListForSave,
                        'fixedAssetMasterVendorReconGL': $scope.accountGroupList
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
                        $scope.getFixedAssetMasterWithCoa('all');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure to untag GL permanently on [ ' + data.FixedAssetMasterName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.selectFixedAssetMasterWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.selectFixedAssetMasterWithCombineList[i].Id === $scope.glUntagId) {
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
        $scope.selectFixedAssetMasterWithCombineList[i] = {
            Id: null,
            FixedAssetMasterName: $scope.selectFixedAssetMasterWithCombineList[i].FixedAssetMasterName,
            COAId: $scope.selectFixedAssetMasterWithCombineList[i].COAId,
            COAName: $scope.selectFixedAssetMasterWithCombineList[i].COAName,
            FixedAssetMasterId: $scope.selectFixedAssetMasterWithCombineList[i].FixedAssetMasterId
        };
    }

    $scope.unTagGL = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/delete',
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
                            document.getElementById($scope.tempList[i].FixedAssetMasterId).checked = false;
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

    $scope.btnSet = '';
    $scope.setActiveBtn = function (str) {
        $scope.btnSet = str;
    };

    $scope.getAllWithCoa = function () {
        if ($scope.btnSet !== '') {
            if ($scope.btnSet === 'all') {
                $scope.getFixedAssetMasterWithCoa('all');
            } else if ($scope.btnSet === 'notassing') {
                getFixedAssetMasterWithCoa('notassing');
            } else if ($scope.btnSet === 'assing') {
                getFixedAssetMasterWithCoa('assing');
            }
        }
    };

    $scope.clearGlField = function () {
        $scope.ReconAssetGLInof = null;
        $scope.AssetGLId = null;
        $scope.AccDepreciationGLInfo = null;
        $scope.AccumulatedDepreciationGLId = null;
        $scope.DepreciationGLInfo = null;
        $scope.DepreciationGLId = null;
        $scope.AUCGLInfo = null;
        $scope.AssetUnderConstructionGLId = null;
        $scope.DownPaymentGLInfo = null;
        $scope.DownPaymentGLGLId = null;
        $scope.ClearingAccGLInfo = null;
        $scope.ClearingAccGLGLId = null;
        $scope.GainOnSaleAssetGLInfo = null;
        $scope.GainOnSaleAssetGLId = null;
        $scope.LossOnSaleAssetGLInfo = null;
        $scope.LossOnSaleAssetGLId = null;
        $scope.LossOnDisposalAssetGLInfo = null;
        $scope.LossOnDisposalAssetGLId = null;
        $scope.refreshLessValueAssetGL();
        $scope.refreshAccGroup();
        var tempList = [];
    };

    $scope.refreshDrp = function () {
        $scope.fixedAssetMasterIds = [];
        $scope.fixedAssetMasterIds = [];
        $scope.tempList = [];
        $scope.getDataWithCoaChange();
    };

    $scope.clearSearchDropDown = function () {
        $scope.fixedAssetMasterIds = [];
        $scope.fixedAssetMasterIds = [];
        getListForm($scope.CompanyDepreciaitonRuleWithCombineListForDdl);
    };

    $scope.Clear = function () {
        $scope.clearSearchDropDown();
        ClearFields();
    };

    $scope.getReport = function () {
        try {
            var file_src = $scope.path + 'GetReport';
            $rootScope.report(file_src);

        } catch (e) {

        }

    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.fixedAssetGL = { COAId: $scope.fixedAssetGL.COAId };
        $scope.tempList = [];
        $scope.clearGlField();
        $scope.getFixedAssetMasterWithCoa('all');
        $scope.vendorReconGLSelectedList = [];
        if ($scope.selectFixedAssetMasterWithCombineList.length > 0) {
            $scope.tableShow = true;
        } else {
            $scope.tableShow = false;
        }
    }
}