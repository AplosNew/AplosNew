'use strict';
MaterialMasterGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function MaterialMasterGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
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
    $scope.path = 'Materials/materialMasterGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateFixedAssetDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.fixedAssetGL = {
        Id: null,
        COAId: null,
        AssetGLId: null,
        AssetBudgetId: null,
        AssetActivityId: null,
        AccumulatedDepreciationGLId: null,
        AccumulatedDepreciationBudgetId: null,
        AccumulatedDepreciationActivityId: null,
        DepreciationGLId: null,
        DepreciationBudgetId: null,
        DepreciationActivityId: null,
        AssetUnderConstructionGLId: null,
        AssetUnderConstructionBudgetId: null,
        AssetUnderConstructionActivityId: null,
        DownPaymentGLId: null,
        DownPaymentBudgetId: null,
        DownPaymentActivityId: null,
        ClearingAccountGLId: null,
        ClearingAccountBudgetId: null,
        ClearingAccountActivityId: null,
        GainOnSaleOfAssetGLId: null,
        GainOnSaleOfAssetBudgetId: null,
        GainOnSaleOfAssetActivityId: null,
        LossOnSaleOfAssetGLId: null,
        LossOnSaleOfAssetBudgetId: null,
        LossOnSaleOfAssetActivityId: null,
        LossOnDisposalAssetGLId: null,
        LossOnDisposalAssetBudgetId: null,
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

    //#region /************MultipleDropdown************/
    $scope.getDataWithCoaChange = function () {
        //$http.get('Materials/fixedAssetGL/getlistwithcombineCoa?coaId=' + $scope.fixedAssetGL.COAId)
        $http.get('Materials/materialMasterGL/getlistwithcombineCoa')
            .then(function (response) {
                $scope.CompanyDepreciaitonRuleWithCombineListForDdl = response.data.Rows;
                getListForm($scope.CompanyDepreciaitonRuleWithCombineListForDdl);
            });
    };

    $scope.getDataWithCoaChange();
    /****getfromLIst******/
    function IdList() {
        $scope.materialMasterIdstr = createIdList(validListWithStr($scope.assetItemNewList, $scope.materialMasterIds));
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
        $scope.assetItemNewList = createCbo(list, 'MaterialMasterId', 'MaterialMasterName');
        $scope.fixedAssetMasterNewList = createCbo(list, 'FixedAssetMasterId', 'FixedAssetMasterName');
    }
    function createCbo(dblist, value, text) {
        var list = [];
        for (var i = 0; i < dblist.length; i++) {
            if (!ddlFilter(list, dblist[i][value])) {
                list.push({
                    Text: dblist[i][text],
                    Value: dblist[i][value]
                })
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
                            MaterialMasterId: oldMainDDlList[i].MaterialMasterId,
                            MaterialMasterName: oldMainDDlList[i].MaterialMasterName,
                            FixedAssetMasterId: oldMainDDlList[i].FixedAssetMasterId
                        });
                    }
                }
            }
            else {
                list.push({
                    Id: oldMainDDlList[i].Id,
                    FixedAssetMasterName: oldMainDDlList[i].FixedAssetMasterName,
                    MaterialMasterId: oldMainDDlList[i].MaterialMasterId,
                    MaterialMasterName: oldMainDDlList[i].MaterialMasterName,
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
        if (name !== 'MaterialMasterId')
            $scope.assetItemNewList = ddlFilterByDDL($scope.newList, 'MaterialMasterId', 'MaterialMasterName');
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
        dynamicTitle: true,
    };
    $scope.materialMasterIds = [];
    $scope.multi3events = {
        onItemSelect: function (item) {
            //if ($scope.materialMasterIds.length > 0) {
            $scope.cboCratetor($scope.materialMasterIds, 'MaterialMasterId');
            //}
        }, onItemDeselect: function (item) {
            //if ($scope.materialMasterIds.length > 0) {
            $scope.cboCratetor($scope.materialMasterIds, 'MaterialMasterId');
            //}
        }
    };

    $scope.fixedAssetMasterIds = [];
    $scope.multi4events = {
        onItemSelect: function (item) {
            //if ($scope.fixedAssetMasterIds.length > 0) {
            $scope.cboCratetor($scope.fixedAssetMasterIds, 'FixedAssetMasterId');
            //}
        }, onItemDeselect: function (item) {
            //if ($scope.materialMasterIds.length > 0) {
            $scope.cboCratetor($scope.fixedAssetMasterIds, 'FixedAssetMasterId');
            //}
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
    $scope.selectChValueId = function (event, MaterialMasterId, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListId($scope.tempList, data.MaterialMasterId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].MaterialMasterId === data.MaterialMasterId) {
                        $scope.tempList.splice(i, 1);
                    }
                    // break;
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempListId(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialMasterId === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.setGlLebelHide = function () {
        if ($scope.fixedAssetGL.COAId != null) {
            $scope.btnActionAll = true;
        } else {
            $scope.btnActionAll = false;
        }
    };

    $scope.GetPartyAccountVDs = [];
    function GetPartyAccountVD(data) {
        $http.get('Materials/materialMasterGL/getpartyaccountvd')
            .then(function (response) {
                $scope.selectFixedAssetMasterWithCombineList = data;
                $scope.GetPartyAccountVDs = response.data.Rows;
                for (var i = 0; i < $scope.selectFixedAssetMasterWithCombineList.length; i++) {
                    $scope.selectFixedAssetMasterWithCombineList[i].Flag = getActive($scope.tempList, $scope.selectFixedAssetMasterWithCombineList[i].MaterialMasterId); //$scope.tempList.includes($scope.selectFixedAssetMasterWithCombineList[i].MaterialMasterId)
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
            if (list[i].PartyAccountGroupId === pid && list[i].MaterialMasterGLId === adId && list[i].ClearingAccGLCode != null) {
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
            $scope.url = 'Materials/materialMasterGL/getlistwithcombine?coaId=' + $scope.fixedAssetGL.COAId + '&materialMasterIds=' + $scope.materialMasterIdstr + '&fixedAssetMasterIds=' + $scope.fixedAssetMasterIdstr;
        }
        if (str === 'notassing') {
            $scope.btnActionAll = true;
            if ($scope.fixedAssetGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'Materials/materialMasterGL/getlistwithcombinenotassing?coaId=' + $scope.fixedAssetGL.COAId + '&materialMasterIds=' + $scope.materialMasterIdstr + '&fixedAssetMasterIds=' + $scope.fixedAssetMasterIdstr;
        }
        if (str === 'assing') {
            $scope.btnActionAll = true;
            if ($scope.fixedAssetGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'Materials/materialMasterGL/getlistwithcombineassing?coaId=' + $scope.fixedAssetGL.COAId + '&materialMasterIds=' + $scope.materialMasterIdstr + '&fixedAssetMasterIds=' + $scope.fixedAssetMasterIdstr;
        }
        baseService.setCurrentPage('selectFixedAssetMasterWithCombineList');
        baseService.init($scope.url, null, null, null, 'MaterialMasterName', 'MaterialMasterName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    //$scope.selectFixedAssetMasterWithCombineList = result.Rows;
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

    // #region ******ReconAssetType GL******
    $scope.searchReconAssetTypeByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.reconAssetTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetReconAssetTypeList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/GetBalanceSheetGLAssetRecon?coaId=' + $scope.fixedAssetGL.COAId;
        $scope.GetReconAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.reconAssetTypeListParameters)
                .then(function (data) {
                    $scope.ReconAssetTypeGLList = data.Rows;
                    $scope.reconAssetTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ReconAssetTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetReconAssetTypeListData();
    };
    $scope.closeReconAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#ReconAssetTypeListPopUp')).modal('hide');
        }
    };
    $scope.setAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.ReconAssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.AssetGLId = x.GLGeneralInfoId;
        getAssetBudget();
    };
    $scope.refreshAssetGL = function () {
        $scope.ReconAssetGLInof = null;
        $scope.AssetGLId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.AssetGLId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.AssetGLId, $scope.fixedAssetGL.AssetBudgetId, function (result) {
            $scope.assetActivityList = result;
        });
    };

    // #endregion

    // #region ******DepreciationType GL******
    $scope.searchDepreciationTypeByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.depreciationListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDepreciationTypeList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/getdepriciationexpensesgl?coaId=' + $scope.fixedAssetGL.COAId;
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
        $scope.DepreciationGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.DepreciationGLId = x.GLGeneralInfoId;
        getDepreciationBudget();
    };
    $scope.refreshDepreciationGL = function () {
        $scope.DepreciationGLInfo = null;
        $scope.DepreciationGLId = null;
    };

    $scope.depreciationBudgetList = [];
    function getDepreciationBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.DepreciationGLId, function (result) {
            $scope.depreciationBudgetList = result;
        });
    }

    $scope.depreciationActivityList = [];
    $scope.getDepreciationActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.DepreciationGLId, $scope.fixedAssetGL.DepreciationBudgetId, function (result) {
            $scope.depreciationActivityList = result;
        });
    };
    // #endregion

    // #region ******AUCGLType GL******
    $scope.searchAUCGLTypeByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.aUCGLTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetAUCGLTypeList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl2 = '/accounts/glitem/getaucgl?coaId=' + $scope.fixedAssetGL.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#AUCGLTypeListPopUp')).modal('hide');
        }
    };
    $scope.setAUCGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AUCGSelectedData = x;
        $scope.AUCGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.AssetUnderConstructionGLId = x.GLGeneralInfoId;
        getAUCGLBudget();
    };
    $scope.refreshAUCGL = function () {
        $scope.AUCGLInfo = null;
        $scope.AssetUnderConstructionGLId = null;
    };

    $scope.aUCGLBudgetList = [];
    function getAUCGLBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.AssetUnderConstructionGLId, function (result) {
            $scope.aUCGLBudgetList = result;
        });
    }
    $scope.aUCGLActivityList = [];
    $scope.getAUCGLActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.AssetUnderConstructionGLId, $scope.fixedAssetGL.AssetUnderConstructionBudgetId, function (result) {
            $scope.aUCGLActivityList = result;
        });
    };

    // #endregion

    // #region ******AccDepreciationType GL******
    $scope.searchAccDepreciationTypeByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.accDepreciationTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetAccDepreciationGLTypeList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl3 = '/accounts/glitem/getaccomultatedepriciationgl?coaId=' + $scope.fixedAssetGL.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#AccDepreciationTypeListPopUp')).modal('hide');
        }
    };
    $scope.setAccDepreciationGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AccDepreciationGlData = x;
        $scope.AccDepreciationGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.AccumulatedDepreciationGLId = x.GLGeneralInfoId;
        getAccDepreciationBudget();
    };
    $scope.refreshAccDepreciationGL = function () {
        $scope.AccDepreciationGLInfo = null;
        $scope.AccumulatedDepreciationGLId = null;
    };

    $scope.accDepreciationBudgetList = [];
    function getAccDepreciationBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.AccumulatedDepreciationGLId, function (result) {
            $scope.accDepreciationBudgetList = result;
        });
    }
    $scope.accDepreciationActivityList = [];
    $scope.getAccDepreciationActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.AccumulatedDepreciationGLId, $scope.fixedAssetGL.AccumulatedDepreciationBudgetId, function (result) {
            $scope.accDepreciationActivityList = result;
        });
    };
    // #endregion

    // #region ******DownPayment GL******
    $scope.searchDownPaymentByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.downPaymentListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDownPaymentGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/GetVendorDownpaymentGLCOAWise?coaId=' + $scope.fixedAssetGL.COAId;
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
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#DownPaymentListPopUp')).modal('hide');
        }
    };
    $scope.setDownPaymentGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.DownPaymentGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.DownPaymentGLId = x.GLGeneralInfoId;
        getDownPaymentBudget();
    };
    $scope.refreshDownPaymentGL = function () {
        $scope.DownPaymentGLInfo = null;
        $scope.DownPaymentGLId = null;
    };
    $scope.downPaymentBudgetList = [];
    function getDownPaymentBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.DownPaymentGLId, function (result) {
            $scope.downPaymentBudgetList = result;
        });
    }
    $scope.downPaymentActivityList = [];
    $scope.getDownPaymentActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.DownPaymentGLId, $scope.fixedAssetGL.DownPaymentBudgetId, function (result) {
            $scope.downPaymentActivityList = result;
        });
    };
    // #endregion

    // #region ******ClearingAccount GL******/
    $scope.searchClearingAccountByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.clearingAccountListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetClearingAccountGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/GetClearingAccountGL?coaId=' + $scope.fixedAssetGL.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#ClearingAccountListPopUp')).modal('hide');
        }
    };
    $scope.setClearingAccountGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.ClearingAccGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.ClearingAccountGLId = x.GLGeneralInfoId;
        getclearingAccountBudget();
    };
    $scope.deleteDocumentSetPopup = function (ob, index) {
        try {
            $scope.message_confirmation = 'Are you sure to delete ';
            angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
            $scope.documentSetIndex = index;
        } catch (e) {
            ShowResult(e, 'Error');
        }
        //$rootScope.passValue(_id, $scope.masterindex);
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
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.ClearingAccountGLId, function (result) {
            $scope.clearingAccountBudgetList = result;
        });
    }

    $scope.clearingAccountActivityList = [];
    $scope.getClearingAccActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.ClearingAccountGLId, $scope.fixedAssetGL.ClearingAccountBudgetId, function (result) {
            $scope.clearingAccountActivityList = result;
        });
    };
    // #endregion

    // #region ***AccountGroupName********/
    //baseService.init('fixedassets/FixedAssetGL/GetListAccountGroupVendor', null, null, null, 'UserName', 'UserName');
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
                            MaterialMasterGLId: null,
                            VendorReconGLId: null,
                            VendorReconGLCode: null,
                            VendorRecontGLText: null,
                            VendorReconBudgetId: null,
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

    function getValue(partyAcId, materialMasterGlId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetPartyAccountWithAssignList?partyAcId=' + partyAcId + '&materialMasterGlId=' + materialMasterGlId
        }).then(function successCallback(response) {
            return response.data[0].GL;
        });
    }

    $scope.searchVendorReconByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.vendorReconListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
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
        $scope.GLUrl1 = '/accounts/glitem/GetVendorReconeGLCOAWise?coaId=' + $scope.fixedAssetGL.COAId;
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
        $scope.accountGroupList[$scope.accIndex].VendorReconBudgetId = null;
        $scope.accountGroupList[$scope.accIndex].VendorReconActivityId = null;
        getVendorReconBudget(x.GLGeneralInfoId, $scope.accIndex);
    };
    $scope.refreshAccGroup = function () {
        angular.forEach($scope.accountGroupList, function (item) {
            item.VendorReconGLId = null;
            item.VendorReconGLCode = null;
            item.VendorRecontGLText = null;
            item.VendorReconBudgetId = null;
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
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, id, function (result) {
            $scope.accountGroupList[index].VendorReconBudgetList = result;
        });
    }
    $scope.getVendorReconActivity = function (id, index, GLGeneralInfoId) {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, GLGeneralInfoId, id, function (result) {
            $scope.accountGroupList[index].VendorReconActivityList = result;
        });
    };

    // #endregion
    // #region ******GainOnSaleAssetType GL******
    $scope.searchGainOnSaleAssetByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.gainOnSaleAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GainOnSaleAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.fixedAssetGL.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#GainOnSaleAssetGLListPopUp')).modal('hide');
        }
    };
    $scope.setGainOnSaleAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.GainOnSaleAssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.GainOnSaleAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.GainOnSaleAssetGLId = x.GLGeneralInfoId;
        getGainOnSaleAssetBudget();
    };
    $scope.refreshGainOnSaleAssetGL = function () {
        $scope.GainOnSaleAssetGLInfo = null;
        $scope.GainOnSaleAssetGLId = null;
    };
    $scope.gainOnSaleAssetBudgetList = [];
    function getGainOnSaleAssetBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.GainOnSaleAssetGLId, function (result) {
            $scope.gainOnSaleAssetBudgetList = result;
        });
    }
    $scope.gainOnSaleAssetActivityList = [];
    $scope.getGainOnSaleAssetActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.GainOnSaleAssetGLId, $scope.fixedAssetGL.GainOnSaleAssetBudgetId, function (result) {
            $scope.gainOnSaleAssetActivityList = result;
        });
    };
    // #endregion
    // #region ******LossOnSaleAssetType GL******
    $scope.searchLossOnSaleAssetByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.lossOnSaleAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LossOnSaleAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/GetDepriciationExpensesGL?coaId=' + $scope.fixedAssetGL.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#LossOnSaleAssetGLListPopUp')).modal('hide');
        }
    };
    $scope.setLossOnSaleAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.LossOnSaleAssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.LossOnSaleAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.LossOnSaleAssetGLId = x.GLGeneralInfoId;
        getLossOnSaleAssetBudget();
    };

    $scope.refreshLossOnSaleAssetGL = function () {
        $scope.LossOnSaleAssetGLInfo = null;
        $scope.LossOnSaleAssetGLId = null;
    };

    $scope.lossOnSaleAssetBudgetList = [];
    function getLossOnSaleAssetBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.LossOnSaleAssetGLId, function (result) {
            $scope.lossOnSaleAssetBudgetList = result;
        });
    }
    $scope.lossOnSaleAssetActivityList = [];
    $scope.getLossOnSaleAssetActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.LossOnSaleAssetGLId, $scope.fixedAssetGL.LossOnSaleAssetBudgetId, function (result) {
            $scope.lossOnSaleAssetActivityList = result;
        });
    };

    // #endregion
    // #region ******LossOnDisposalAssetType GL******
    $scope.searchLossOnDisposalAssetByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.lossOnDisposalAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LossOnDisposalAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/GetDepriciationExpensesGL?coaId=' + $scope.fixedAssetGL.COAId;
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
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.LossOnDisposalAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.LossOnDisposalAssetGLId = x.GLGeneralInfoId;
        getLossOnDisposalAssetBudget();
    };
    $scope.refreshLossOnDisposalAssetGL = function () {
        $scope.LossOnDisposalAssetGLInfo = null;
        $scope.LossOnDisposalAssetGLId = null;
    };

    $scope.lossOnDisposalAssetBudgetList = [];
    function getLossOnDisposalAssetBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.LossOnDisposalAssetGLId, function (result) {
            $scope.lossOnDisposalAssetBudgetList = result;
        });
    }
    $scope.lossOnDisposalAssetActivityList = [];
    $scope.getLossOnDisposalAssetActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.LossOnDisposalAssetGLId, $scope.fixedAssetGL.LossOnDisposalAssetBudgetId, function (result) {
            $scope.lossOnDisposalAssetActivityList = result;
        });
    };

    // #endregion
    // #region ******LessValueAssetType GL******
    $scope.lessValueAssetList = [];
    $scope.searchLessValueAssetByList = [
        {
            'name': 'COA',
            'value': 'COAName'
        },
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
    $scope.lessValueAssetListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LessValueAssetGlList = function () {
        if ($scope.fixedAssetGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = '/accounts/glitem/GetAssetCOAWiseExceptRecon?coaId=' + $scope.fixedAssetGL.COAId;
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
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.LessValueAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.LessValueAssetGLId = x.GLGeneralInfoId;
        getLessValueAssetBudget();
    };
    $scope.refreshLessValueAssetGL = function () {
        $scope.LessValueAssetGLInfo = null;
        $scope.LessValueAssetGLId = null;
    }
    $scope.lessValueAssetBudgetList = [];
    function getLessValueAssetBudget() {
        cboService.getCboBudgetForSetup($scope.fixedAssetGL.COAId, $scope.LessValueAssetGLId, function (result) {
            $scope.lessValueAssetBudgetList = result;
        });
    }
    $scope.lessValueAssetActivityList = [];
    $scope.getLessValueAssetActivity = function () {
        cboService.getCboActivityForSetup($scope.fixedAssetGL.COAId, $scope.LessValueAssetGLId, $scope.fixedAssetGL.LessValueAssetBudgetId, function (result) {
            $scope.lessValueAssetActivityList = result;
        });
    };

    // #endregion
    $scope.closeFixedAssetGLListPopUpSelected = function () {
        $scope.fixedAssetGLRowList = [];
        angular.forEach($scope.selectFixedAssetMasterWithCombineList, function (item) {
            if (item.Active) {
                if (checkIsAvailable($scope.fixedAssetGLRowList, item.MaterialMasterId) == false) {
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
    function checkIsAvailable(list, MaterialMasterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialMasterId === MaterialMasterId) {
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
                if ($scope.AssetGLId !== null) {
                    item.AssetGLId = $scope.AssetGLId;
                }
                if ($scope.fixedAssetGL.AssetBudgetId !== null) {
                    item.AssetBudgetId = $scope.fixedAssetGL.AssetBudgetId;
                }
                if ($scope.fixedAssetGL.AssetActivityId !== null) {
                    item.AssetActivityId = $scope.fixedAssetGL.AssetActivityId;
                }
                if ($scope.AccumulatedDepreciationGLId !== null) {
                    item.AccumulatedDepreciationGLId = $scope.AccumulatedDepreciationGLId;
                }
                if ($scope.fixedAssetGL.AccumulatedDepreciationBudgetId !== null) {
                    item.AccumulatedDepreciationBudgetId = $scope.fixedAssetGL.AccumulatedDepreciationBudgetId;
                }
                if ($scope.fixedAssetGL.AccumulatedDepreciationActivityId !== null) {
                    item.AccumulatedDepreciationActivityId = $scope.fixedAssetGL.AccumulatedDepreciationActivityId;
                }
                if ($scope.DepreciationGLId !== null) {
                    item.DepreciationGLId = $scope.DepreciationGLId;
                }
                if ($scope.fixedAssetGL.DepreciationBudgetId !== null) {
                    item.DepreciationBudgetId = $scope.fixedAssetGL.DepreciationBudgetId;
                }
                if ($scope.fixedAssetGL.DepreciationActivityId !== null) {
                    item.DepreciationActivityId = $scope.fixedAssetGL.DepreciationActivityId;
                }
                if ($scope.AssetUnderConstructionGLId !== null) {
                    item.AssetUnderConstructionGLId = $scope.AssetUnderConstructionGLId;
                }
                if ($scope.fixedAssetGL.AssetUnderConstructionBudgetId !== null) {
                    item.AssetUnderConstructionBudgetId = $scope.fixedAssetGL.AssetUnderConstructionBudgetId;
                }
                if ($scope.fixedAssetGL.AssetUnderConstructionActivityId != null) {
                    item.AssetUnderConstructionActivityId = $scope.fixedAssetGL.AssetUnderConstructionActivityId;
                }
                if ($scope.DownPaymentGLId !== null) {
                    item.DownPaymentGLId = $scope.DownPaymentGLId;
                }
                if ($scope.fixedAssetGL.DownPaymentBudgetId !== null) {
                    item.DownPaymentBudgetId = $scope.fixedAssetGL.DownPaymentBudgetId;
                }
                if ($scope.fixedAssetGL.DownPaymentActivityId !== null) {
                    item.DownPaymentActivityId = $scope.fixedAssetGL.DownPaymentActivityId;
                }
                if ($scope.ClearingAccountGLId !== null) {
                    item.ClearingAccountGLId = $scope.ClearingAccountGLId;
                }
                if ($scope.fixedAssetGL.ClearingAccountBudgetId !== null) {
                    item.ClearingAccountBudgetId = $scope.fixedAssetGL.ClearingAccountBudgetId;
                }
                if ($scope.fixedAssetGL.ClearingAccountActivityId !== null) {
                    item.ClearingAccountActivityId = $scope.fixedAssetGL.ClearingAccountActivityId;
                }
                if ($scope.GainOnSaleAssetGLId !== null) {
                    item.GainOnSaleOfAssetGLId = $scope.GainOnSaleAssetGLId;
                }
                if ($scope.fixedAssetGL.GainOnSaleAssetBudgetId !== null) {
                    item.GainOnSaleAssetBudgetId = $scope.fixedAssetGL.GainOnSaleAssetBudgetId;
                    item.GainOnSaleOfAssetBudgetId = $scope.fixedAssetGL.GainOnSaleAssetBudgetId;
                }
                if ($scope.fixedAssetGL.GainOnSaleAssetActivityId !== null) {
                    item.GainOnSaleAssetActivityId = $scope.fixedAssetGL.GainOnSaleAssetActivityId;
                    item.GainOnSaleOfAssetActivityId = $scope.fixedAssetGL.GainOnSaleAssetActivityId;
                }
                if ($scope.LossOnSaleAssetGLId !== null) {
                    item.LossOnSaleOfAssetGLId = $scope.LossOnSaleAssetGLId;
                }
                if ($scope.fixedAssetGL.LossOnSaleAssetBudgetId !== null) {
                    item.LossOnSaleAssetBudgetId = $scope.fixedAssetGL.LossOnSaleAssetBudgetId;
                    item.LossOnSaleOfAssetBudgetId = $scope.fixedAssetGL.LossOnSaleAssetBudgetId;
                }
                if ($scope.fixedAssetGL.LossOnSaleAssetActivityId !== null) {
                    item.LossOnSaleAssetActivityId = $scope.fixedAssetGL.LossOnSaleAssetActivityId;
                    item.LossOnSaleOfAssetActivityId = $scope.fixedAssetGL.LossOnSaleAssetActivityId;
                }
                if ($scope.LossOnDisposalAssetGLId !== null) {
                    item.LossOnDisposalAssetGLId = $scope.LossOnDisposalAssetGLId;
                    ///for combine db
                }
                if ($scope.fixedAssetGL.LossOnDisposalAssetBudgetId !== null) {
                    item.LossOnDisposalAssetBudgetId = $scope.fixedAssetGL.LossOnDisposalAssetBudgetId;
                }
                if ($scope.fixedAssetGL.LossOnDisposalAssetActivityId !== null) {
                    item.LossOnDisposalAssetActivityId = $scope.fixedAssetGL.LossOnDisposalAssetActivityId;
                }
                if ($scope.LessValueAssetGLId != null) {
                    item.LessValueAssetGLId = $scope.LessValueAssetGLId;
                }
                if ($scope.fixedAssetGL.LessValueAssetBudgetId !== null) {
                    item.LessValueAssetBudgetId = $scope.fixedAssetGL.LessValueAssetBudgetId;
                }
                if ($scope.fixedAssetGL.LessValueAssetActivityId !== null) {
                    item.LessValueAssetActivityId = $scope.fixedAssetGL.LessValueAssetActivityId;
                }
                item.COAId = $scope.fixedAssetGL.COAId;
                $scope.fixedAssetGLListForSave.push(item);
            }
        });
    };

    function checkVendorReconGLIsAssinged(list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].VendorReconGLId !== null) {
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
                        'materialMasterGL': $scope.fixedAssetGLListForSave,
                        'materialMasterVendorReconGL': $scope.accountGroupList
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
        $scope.message_confirmation = 'Are you sure to untag GL permanently on [ ' + data.MaterialMasterName + ' ]?';
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
            MaterialMasterName: $scope.selectFixedAssetMasterWithCombineList[i].MaterialMasterName,
            COAId: $scope.selectFixedAssetMasterWithCombineList[i].COAId,
            COAName: $scope.selectFixedAssetMasterWithCombineList[i].COAName,
            MaterialMasterId: $scope.selectFixedAssetMasterWithCombineList[i].MaterialMasterId
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
                            document.getElementById($scope.tempList[i].MaterialMasterId).checked = false;
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
        if ($scope.btnSet != '') {
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
        $scope.refreshAccGroup();
        var tempList = [];
    };

    $scope.refreshDrp = function () {
        $scope.materialMasterIds = [];
        $scope.fixedAssetMasterIds = [];
        $scope.tempList = [];
        $scope.getDataWithCoaChange();
    };

    $scope.clearSearchDropDown = function () {
        $scope.materialMasterIds = [];
        $scope.fixedAssetMasterIds = [];
        getListForm($scope.CompanyDepreciaitonRuleWithCombineListForDdl);
    };

    $scope.Clear = function () {
        $scope.clearSearchDropDown();
        ClearFields();
    };

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