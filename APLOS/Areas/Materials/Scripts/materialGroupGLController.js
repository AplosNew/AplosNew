'use strict';
MaterialGroupGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MaterialGroupGLController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Material Group Account Determinate";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.fixedasseGroupList = [];
    $scope.fixedAssetSelectList = [];
    $scope.materialGroupGLList = [];
    $scope.selectMaterialGroupMasterWithCombineList = [];
    $scope.ReconAssetTypeGLList = [];
    $scope.AccDepreciationGLTypeList = [];
    $scope.DepreciationTypeGLList = [];
    $scope.AUCGLTypeList = [];
    if ($scope.selectMaterialGroupMasterWithCombineList.length > 0) {
        $scope.tableShow = true;
    } else {
        $scope.tableShow = false;
    }
    $scope.materialGroupGLRowList = [];
    $scope.path = 'Materials/MaterialGroupGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateMaterialGroupDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.materialGroupGL = {
        Id: null,
        COAId: null,
        MaterialGroupMasterId: null,
        AccumulatedDepreciationGLId: null,
        DepreciationGLId: null,
        AssetUnderConstructionGLId: null,
        DownPaymentBudgetMasterId: null,
        DownPaymentActivityId: null,
        ClearingAccountBudgetMasterId: null,
        InventoryInTransitBudgetMasterId: null,
        ClearingAccountActivityId: null,
        InventoryInTransitActivityId: null,
        InventoryGLId: null,
        InventoryBudgetMasterId: null,
        InventoryActivity: null,
        ExpenseGLId: null,
        ExpenseBudgetMasterId: null,
        ExpenseActivityId: null,
        DebitNoteGLId: null,
        DebitNoteBudgetMasterId: null,
        DebitNoteActivity: null,
        CreditNoteGLId: null,
        CreditNoteBudgetMasterId: null,
        CreditNoteActivity: null,
        ShortageGLId: null,
        ShortageBudgetMasterId: null,
        ShortageActivity: null,
        RejectionGLId: null,
        RejectionBudgetMasterId: null,
        RejectionActivity: null,
    };
    /*******************CBO***************/
    $http({
        method: 'GET',
        url: 'Materials/materialGroup1/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup1List = response.data;
    });
    $http({
        method: 'GET',
        url: 'Materials/materialgroup2/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup2List = response.data;
    });

    $http({
        method: 'GET',
        url: 'Materials/materialGroup3/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup3List = response.data;
    });

    $http({
        method: 'GET',
        url: 'Materials/materialGroup4/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup4List = response.data;
    });

    $http({
        method: 'GET',
        url: 'Materials/materialtype/getcbo'
    }).then(function successCallback(response) {
        $scope.materialTypeList = response.data;
    });

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.getDataWithCoaChange = function () {
        $http.get('Materials/MaterialGroupGL/getlistwithcombineCoa')
            .then(function (response) {
                $scope.selectMaterialGroupMasterWithCombineList = response.data.Rows;
            });
    };

    //$scope.getDataWithCoaChange();
    $scope.tempList = [];

    //$scope.selectMaterialGroupMasterWithCombineList = [];
    $scope.selectChValueId = function () {
        try {

            for (var di = 0; di < $scope.selectMaterialGroupMasterWithCombineList.length; di++)
            {
                if ($scope.selectMaterialGroupMasterWithCombineList[di].CheckBoxSelect)
                {
                    $scope.tempList.push($scope.selectMaterialGroupMasterWithCombineList[di]);
                }

            }
            //$scope.tempList = data;//.push(data);

            //if (event.currentTarget.checked) {
            //    if (checkExistTempListId($scope.tempList, data.MaterialGroupMasterId) === false) {
            //        $scope.tempList.push(data);
            //    }
            //}
            //else {
            //    for (var i = 0; i < $scope.tempList.length; i++) {
            //        if ($scope.tempList[i].MaterialGroupMasterId === data.MaterialGroupMasterId) {
            //            $scope.tempList.splice(i, 1);
            //        }
            //        // break;
            //    }
            //}
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };


    $scope.X_selectChValueId = function (event, MaterialGroupMasterId, data) {
        try {
            //$scope.tempList = data;//.push(data);

            if (event.currentTarget.checked) {
                if (checkExistTempListId($scope.tempList, data.MaterialGroupMasterId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].MaterialGroupMasterId === data.MaterialGroupMasterId) {
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
            if (list[i].MaterialGroupMasterId === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialGroupMasterId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.GetPartyAccountVDs = [];
    function GetPartyAccountVD(data) {
        $http.get('Materials/MaterialGroupGL/getpartyaccountvd')
            .then(function (response) {
                $scope.selectMaterialGroupMasterWithCombineList = data;
                $scope.GetPartyAccountVDs = response.data.Rows;
                for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
                    $scope.selectMaterialGroupMasterWithCombineList[i].Flag = getActive($scope.tempList, $scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroupMasterId); //$scope.tempList.includes($scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroupMasterId)
                }
                angular.forEach($scope.accountGroupSalesList, function (item, j) {
                    for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectMaterialGroupMasterWithCombineList[i].PartyAccountGroupId, $scope.selectMaterialGroupMasterWithCombineList[i].Id, item.PartyAccountGroupId, 'Sales');
                        $scope.selectMaterialGroupMasterWithCombineList[i]['S' + j + 'GL'] = ob.GL;
                        $scope.selectMaterialGroupMasterWithCombineList[i]['S' + j + 'Budget'] = ob.Budget;
                        $scope.selectMaterialGroupMasterWithCombineList[i]['S' + j + 'Activity'] = ob.Activity;
                    }
                });
                angular.forEach($scope.accountGroupCustomerList, function (item, j) {
                    for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectMaterialGroupMasterWithCombineList[i].PartyAccountGroupId, $scope.selectMaterialGroupMasterWithCombineList[i].Id, item.PartyAccountGroupId, 'Receivable');
                        $scope.selectMaterialGroupMasterWithCombineList[i]['C' + j + 'GL'] = ob.GL;
                        $scope.selectMaterialGroupMasterWithCombineList[i]['C' + j + 'Budget'] = ob.Budget;
                        $scope.selectMaterialGroupMasterWithCombineList[i]['C' + j + 'Activity'] = ob.Activity;
                    }
                });
                angular.forEach($scope.accountGroupVendorList, function (item, j) {
                    for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectMaterialGroupMasterWithCombineList[i].PartyAccountGroupId, $scope.selectMaterialGroupMasterWithCombineList[i].Id, item.PartyAccountGroupId, 'Payable');
                        $scope.selectMaterialGroupMasterWithCombineList[i]['V' + j + 'GL'] = ob.GL;
                        $scope.selectMaterialGroupMasterWithCombineList[i]['V' + j + 'Budget'] = ob.Budget;
                        $scope.selectMaterialGroupMasterWithCombineList[i]['V' + j + 'Activity'] = ob.Activity;
                    }
                });
            });
    }
    var AccountDYOb = {
        GL: null,
        Budget: null,
        Activity: null
    };
    function assignDomesticVendor(list, aId, adId, pid, gltype) {
        AccountDYOb = {
            GL: null,
            Budget: null,
            Activity: null
        };
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyAccountGroupId === pid && list[i].MaterialGroupGLId === adId && list[i].ClearingAccGLCode != null && list[i].GLType === gltype) {
                AccountDYOb.GL = list[i].ClearingAccGLCode + "-" + list[i].ClearingAccGLText;
                AccountDYOb.Budget = list[i].BudgetName;
                AccountDYOb.Activity = list[i].ActivityName;
                break;
                //vRT = list[i].ClearingAccGLCode + "-" + list[i].ClearingAccGLText + " [Budget:] " + list[i].BudgetName + " [Activity:]" + list[i].ActivityName;
            }
        }
        return AccountDYOb;
    }

    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };
    //$scope.getMaterialGroupMasterWithCoa = function (str) {
    //    $scope.selectMaterialGroupMasterWithCombineList = [];
    //    if ($scope.materialGroupGL.COAId === null) {
    //        return ShowResult("Select COA first", 'failure');
    //    }
    //    if ($scope.selectMaterialGroupMasterWithCombineList.length > 0) {
    //        $scope.tableShow = true;
    //    } else {
    //        $scope.tableShow = false;
    //    }
    //    if (str === 'all') {
    //        $scope.url = 'Materials/MaterialGroupGL/getlistwithcombine?coaId=' + $scope.materialGroupGL.COAId;
    //    }
    //    if (str === 'notassing') {
    //        $scope.btnActionAll = true;
    //        if ($scope.materialGroupGL.COAId === null) {
    //            return ShowResult("Select COA first", 'failure');
    //        }
    //        $scope.url = 'Materials/MaterialGroupGL/getlistwithcombinenotassing?coaId=' + $scope.materialGroupGL.COAId;
    //    }
    //    if (str === 'assing') {
    //        $scope.btnActionAll = true;
    //        if ($scope.materialGroupGL.COAId === null) {
    //            return ShowResult("Select COA first", 'failure');
    //        }
    //        $scope.url = 'Materials/MaterialGroupGL/getlistwithcombineassing?coaId=' + $scope.materialGroupGL.COAId;
    //    }
    //    baseService.setCurrentPage('selectMaterialGroupMasterWithCombineList');
    //    baseService.init($scope.url, null, null, null, 'MaterialGroupMasterName', 'MaterialGroupMasterName');
    //    $scope.getData = function (pageno) {
    //        baseService.pagination(pageno)
    //            .then(function (result) {
    //                //$scope.selectMaterialGroupMasterWithCombineList = result.Rows;
    //                if (result.Rows.length > 0) {
    //                    GetPartyAccountVD(result.Rows);
    //                }
    //                if (result.Rows.length > 0) {
    //                    $scope.tableShow = true;
    //                } else {
    //                    $scope.tableShow = false;
    //                }
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getData();
    //}
    $scope.getMaterialGroupMasterWithCoa = function (str) {
        try {       

            
            if ($scope.materialGroupGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            if ($scope.selectMaterialGroupMasterWithCombineList.length > 0) {
                $scope.tableShow = true;
            } else {
                $scope.tableShow = false;
            }
            if (str === 'all') {
                //$scope.url = 'Materials/MaterialGroupGL/getlistwithcombine?coaId=' + $scope.materialGroupGL.COAId;
                var parameters = { 'coaId': $scope.materialGroupGL.COAId };
                $scope.url = 'Materials/MaterialGroupGL/getlistwithcombine/';
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: $scope.url,
                    data: parameters
                }).then(function successCallback(response) {
                    $scope.selectMaterialGroupMasterWithCombineList = [];
                    if (response.data.Error === true) {
                        //$scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else{
                        $scope.selectMaterialGroupMasterWithCombineList = response.data;
                    } 
                    if ($scope.selectMaterialGroupMasterWithCombineList.length > 0) {
                        $scope.tableShow = true;
                    } else {
                        $scope.tableShow = false;
                    }

                }), function errorCallBack(response) {
               // $scope.ShowResultCustom(response.data.Message, 'failure');
            };

            }
            if (str === 'notassign') {
                $scope.btnActionAll = true;
                if ($scope.materialGroupGL.COAId === null) {
                    return ShowResult("Select COA first", 'failure');
                }
                var parametersnotassign = { 'coaId': $scope.materialGroupGL };
                $scope.url = 'Materials/MaterialGroupGL/getlistwithcombinenotassign/';
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: $scope.url,
                    data: parametersnotassign
                }).then(function successCallback(response) {
                    $scope.selectMaterialGroupMasterWithCombineList = [];

                    if (response.data.Error === true) {
                       // $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {

                        $scope.selectMaterialGroupMasterWithCombineList = response.data;

                    }
                    if ($scope.selectMaterialGroupMasterWithCombineList.length > 0) {
                        $scope.tableShow = true;
                    } else {
                        $scope.tableShow = false;
                    }
                }), function errorCallBack(response) {
                   // $scope.ShowResultCustom(response.data.Message, 'failure');
                };
            }
            if (str === 'assign') {
                $scope.btnActionAll = true;
                if ($scope.materialGroupGL.COAId === null) {
                    return ShowResult("Select COA first", 'failure');
                }
                var parametersassign = { 'coaId': $scope.materialGroupGL.COAId};

                $scope.url = 'Materials/MaterialGroupGL/getlistwithcombineassign/';
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: $scope.url,
                    data: parametersassign
                }).then(function successCallback(response) {
                    $scope.selectMaterialGroupMasterWithCombineList = [];

                    if (response.data.Error === true) {
                       // $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {

                        $scope.selectMaterialGroupMasterWithCombineList = response.data;

                    }
                    if ($scope.selectMaterialGroupMasterWithCombineList.length > 0) {
                        $scope.tableShow = true;
                    } else {
                        $scope.tableShow = false;
                    }
                }), function errorCallBack(response) {
                   // $scope.ShowResultCustom(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, "failure");
        }
    };
    // #region ******DownPayment GL******
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
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorDownpaymentGLCOAWise?coaId=' + $scope.materialGroupGL.COAId;
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
        
        $scope.DownPaymentGLInfo = x.GLGeneralInfoName;
        $scope.DownPaymentGLId = x.GLGeneralInfoId;
        getDownPaymentBudget();
    };
    $scope.refreshDownPaymentGL = function () {
        $scope.DownPaymentGLInfo = null;
        $scope.DownPaymentGLId = null;
    }
    $scope.downPaymentBudgetList = [];
    function getDownPaymentBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.DownPaymentGLId, function (result) {
            $scope.downPaymentBudgetList = result;
        });
    }
    $scope.downPaymentActivityList = [];
    $scope.getDownPaymentActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.DownPaymentBudgetMasterId, function (result) {
            $scope.downPaymentActivityList = result;
        });
    }
    // #endregion
    // #region ******Inventory GL******
    $scope.assetTypeGLList = [];
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
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetInventoryGlList = function () {
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWise?coaId=' + $scope.materialGroupGL.COAId;
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
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.InventoryGLInfo = x.GLGeneralInfoName;
        $scope.InventoryGLId = x.GLGeneralInfoId;
        getInventoryBudget();
    };
    $scope.refreshInventoryGL = function () {
        $scope.InventoryGLInfo = null;
        $scope.InventoryGLId = null;
    }
    $scope.inventoryBudgetList = [];
    function getInventoryBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.InventoryGLId, function (result) {
            $scope.inventoryBudgetList = result;
        });
    }
    $scope.inventoryActivityList = [];
    $scope.getInventoryActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.InventoryBudgetMasterId, function (result) {
            $scope.inventoryActivityList = result;
        });
    }
    // #endregion
    // #region ******ClearingAccount GL******/
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
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetClearingAccountGL?coaId=' + $scope.materialGroupGL.COAId;
        $scope.GetClearingAccountListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.clearingAccountListParameters)
                .then(function (data) {
                    $scope.clearingAccountGlList = data.Rows;
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
        getClearingAccountBudget();
    };
    $scope.refreshClearingAccountGL = function () {
        $scope.ClearingAccGLInfo = null;
        $scope.ClearingAccountGLId = null;
    }
    $scope.clearingAccountBudgetList = [];
    function getClearingAccountBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.ClearingAccountGLId, function (result) {
            $scope.clearingAccountBudgetList = result;
        });
    }
    $scope.clearingAccountActivityList = [];
    $scope.getClearingAccountActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.ClearingAccountBudgetMasterId, function (result) {
            $scope.clearingAccountActivityList = result;
        });
    }
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
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetExpenseGlList = function () {
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.materialGroupGL.COAId;
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
        $scope.AssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.ExpenseGLInfo = x.GLGeneralInfoName;
        $scope.ExpenseGLId = x.GLGeneralInfoId;
        getExpenseBudget();
    };
    $scope.refreshExpenseGL = function () {
        $scope.ExpenseGLInfo = null;
        $scope.ExpenseGLId = null;
    }
    $scope.expenseBudgetList = [];
    function getExpenseBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.ExpenseGLId, function (result) {
            $scope.expenseBudgetList = result;
        });
    }
    $scope.expenseActivityList = [];
    $scope.getExpenseActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.ExpenseBudgetMasterId, function (result) {
            $scope.expenseActivityList = result;
        });
    }
    // #endregion

    //region ***********Debit Note GL********************
    $scope.debitNoteGLList = [];
    $scope.searchDebitNoteByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.debitNoteListParameters = {
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
    $scope.GetDebitNoteGlList = function () {
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWise?coaId=' + $scope.materialGroupGL.COAId;
        $scope.getDebitNoteListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.debitNoteListParameters)
                .then(function (data) {
                    $scope.debitNoteGLList = data.Rows;
                    $scope.debitNoteListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#debitNoteListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getDebitNoteListData();
    };
    $scope.closeDebitNoteListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#debitNoteListPopUp')).modal('hide');
        }
    };
    $scope.setDebitNoteGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.DebitNoteGLInfo = x.GLGeneralInfoName;
        $scope.DebitNoteGLId = x.GLGeneralInfoId;
        getDebitNoteBudget();
    };
    $scope.refreshDebitNoteGL = function () {
        $scope.DebitNoteGLInfo = null;
        $scope.DebitNoteGLId = null;
    }
    $scope.debitNoteBudgetList = [];
    function getDebitNoteBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.DebitNoteGLId, function (result) {
            $scope.debitNoteBudgetList = result;
        });
    }
    $scope.debitNoteActivityList = [];
    $scope.getDebitNoteActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.DebitNoteBudgetMasterId, function (result) {
            $scope.debitNoteActivityList = result;
        });
    }
    // #endregion ***********Credit Note GL********************


    //region ***********Debit Note GL********************
    $scope.creditNoteGLList = [];
    $scope.searchCreditNoteByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.creditNoteListParameters = {
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
    $scope.GetCreditNoteGlList = function () {
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityCOAWise?coaId=' + $scope.materialGroupGL.COAId;
        $scope.getCreditNoteListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.creditNoteListParameters)
                .then(function (data) {
                    $scope.creditNoteGLList = data.Rows;
                    $scope.creditNoteListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#creditNoteListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getCreditNoteListData();
    };
    $scope.closeCreditNoteListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#creditNoteListPopUp')).modal('hide');
        }
    };
    $scope.setCreditNoteGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.CreditNoteGLInfo = x.GLGeneralInfoName;
        $scope.CreditNoteGLId = x.GLGeneralInfoId;
        getCreditNoteBudget();
    };
    $scope.refreshCreditNoteGL = function () {
        $scope.CreditNoteGLInfo = null;
        $scope.CreditNoteGLId = null;
    }
    $scope.creditNoteBudgetList = [];
    function getCreditNoteBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.CreditNoteGLId, function (result) {
            $scope.creditNoteBudgetList = result;
        });
    }
    $scope.creditNoteActivityList = [];
    $scope.getCreditNoteActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.CreditNoteBudgetMasterId, function (result) {
            $scope.creditNoteActivityList = result;
        });
    }
    // #endregion ***********Credit Note GL********************

    //region ***********Shortage GL********************
    $scope.shortageGLList = [];
    $scope.searchShortageByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.shortageListParameters = {
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
    $scope.GetShortageGlList = function () {
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWise?coaId=' + $scope.materialGroupGL.COAId;
        $scope.getShortageListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.shortageListParameters)
                .then(function (data) {
                    $scope.shortageGLList = data.Rows;
                    $scope.shortageListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#shortageListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getShortageListData();
    };
    $scope.closeShortageListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#shortageListPopUp')).modal('hide');
        }
    };
    $scope.setShortageGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.ShortageGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.ShortageGLInfo = x.GLGeneralInfoName;
        $scope.ShortageGLId = x.GLGeneralInfoId;
        getShortageBudget();
    };
    $scope.refreshShortageGL = function () {
        $scope.ShortageGLInfo = null;
        $scope.ShortageGLId = null;
    }
    $scope.shortageBudgetList = [];
    function getShortageBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.ShortageGLId, function (result) {
            $scope.shortageBudgetList = result;
        });
    }
    $scope.shortageActivityList = [];
    $scope.getShortageActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.ShortageBudgetMasterId, function (result) {
            $scope.shortageActivityList = result;
        });
    }
    // #endregion ***********Shortage GL********************


    //region ***********Rejection GL********************
    $scope.rejectionGLList = [];
    $scope.searchRejectionByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.rejectionListParameters = {
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
    $scope.GetRejectionGlList = function () {
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWise?coaId=' + $scope.materialGroupGL.COAId;
        $scope.getRejectionListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.rejectionListParameters)
                .then(function (data) {
                    $scope.rejectionGLList = data.Rows;
                    $scope.rejectionListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#rejectionListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getRejectionListData();
    };
    $scope.closeRejectionListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#rejectionListPopUp')).modal('hide');
        }
    };
    $scope.setRejectionGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.RejectionGLSelectedData = x;
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.RejectionGLInfo = x.GLGeneralInfoName;
        $scope.RejectionGLId = x.GLGeneralInfoId;
        getRejectionBudget();
    };
    $scope.refreshRejectionGL = function () {
        $scope.RejectionGLInfo = null;
        $scope.RejectionGLId = null;
    }
    $scope.rejectionBudgetList = [];
    function getRejectionBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.RejectionGLId, function (result) {
            $scope.rejectionBudgetList = result;
        });
    }
    $scope.rejectionActivityList = [];
    $scope.getRejectionActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.RejectionBudgetMasterId, function (result) {
            $scope.rejectionActivityList = result;
        });
    }
    // #endregion ***********Rejection GL********************



    // #region ***Account Group********/
    $scope.getChildHeaderAcGroup = function () {
        $scope.childHeaderAcG = [];
        $scope.childHeaderCustomerAcG = [];
        $scope.childHeaderSalesAcG = [];
        angular.forEach($scope.accountGroupVendorList, function (item, i) {
            $scope.childHeaderAcG.push('GL');
            $scope.childHeaderAcG.push('Budget');
            $scope.childHeaderAcG.push('Activity');
        });
        angular.forEach($scope.accountGroupCustomerList, function (item, i) {
            $scope.childHeaderCustomerAcG.push('GL');
            $scope.childHeaderCustomerAcG.push('Budget');
            $scope.childHeaderCustomerAcG.push('Activity');
        });
        angular.forEach($scope.accountGroupSalesList, function (item, i) {
            $scope.childHeaderSalesAcG.push('GL');
            $scope.childHeaderSalesAcG.push('Budget');
            $scope.childHeaderSalesAcG.push('Activity');
        });
    }
    $scope.buildValue = function () {
        $scope.NewListA = [];
        $scope.NewListCustomer = [];
        $scope.NewListSales = [];
        angular.forEach($scope.accountGroupVendorList, function (item, i) {
            $scope.NewListA.push('V' + i + "GL");
            $scope.NewListA.push('V' + i + "Budget");
            $scope.NewListA.push('V' + i + "Activity");
        })
        angular.forEach($scope.accountGroupCustomerList, function (item, i) {
            $scope.NewListCustomer.push('C' + i + "GL");
            $scope.NewListCustomer.push('C' + i + "Budget");
            $scope.NewListCustomer.push('C' + i + "Activity");
        })
        angular.forEach($scope.accountGroupSalesList, function (item, i) {
            $scope.NewListSales.push('S' + i + "GL");
            $scope.NewListSales.push('S' + i + "Budget");
            $scope.NewListSales.push('S' + i + "Activity");
        })
    }
    $scope.loadAccountGroup = function (pageno) {
        baseService.init('Parties/PartyAccountGroup/GetAllList', null, 12, null, 'UserName', 'UserName');
        $scope.accountGroupVendorList = [];
        $scope.accountGroupCustomerList = [];
        $scope.accountGroupSalesList = [];
        $http({
            method: 'GET',
            url: 'Organizations/Plant/GetCbo'
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
        });
        baseService.pagination(pageno)
            .then(function (result) {
                angular.forEach(result.Rows, function (item) {
                    if (item.AccountType === 'Vendor') {
                        $scope.accountGroupVendorList.push(
                            {
                                Id: null,
                                PartyAccountGroupId: item.Id,
                                AccountType: item.AccountType,
                                GLType: 'Payable',
                                Code: item.Code,
                                UserName: item.UserName,
                                MaterialGroupGLId: null,
                                GLGeneralInfoId: null,
                                VendorReconGLCode: null,
                                VendorRecontGLText: null,
                                BudgetMasterId: null,
                                ActivityId: null
                            }
                        );
                    } else if (item.AccountType === 'Customer') {
                        $scope.accountGroupCustomerList.push(
                            {
                                Id: null,
                                PartyAccountGroupId: item.Id,
                                AccountType: item.AccountType,
                                GLType: 'Receivable',
                                Code: item.Code,
                                UserName: item.UserName,
                                MaterialGroupGLId: null,
                                GLGeneralInfoId: null,
                                VendorReconGLCode: null,
                                VendorRecontGLText: null,
                                BudgetMasterId: null,
                                ActivityId: null
                            }
                        );
                        $scope.accountGroupSalesList.push(
                            {
                                Id: null,
                                PartyAccountGroupId: item.Id,
                                AccountType: item.AccountType,
                                GLType: 'Sales',
                                Code: item.Code,
                                UserName: item.UserName,
                                MaterialGroupGLId: null,
                                GLGeneralInfoId: null,
                                VendorReconGLCode: null,
                                VendorRecontGLText: null,
                                BudgetMasterId: null,
                                ActivityId: null
                            }
                        );
                    }
                });
                $scope.getChildHeaderAcGroup();
                $scope.buildValue();
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.loadAccountGroup();

    // #endregion
    // #region ******VendorRecon GL******
    $scope.searchReconypeByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.recontypeListParameters = {
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
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorReconeGLCOAWise?coaId=' + $scope.materialGroupGL.COAId;
        $scope.GetReconTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.recontypeListParameters)
                .then(function (data) {
                    $scope.ReconTypeGLList = data.Rows;
                    $scope.recontypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ReconTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetReconTypeListData();
    };
    $scope.closeReconTypeListPopUpSelected = function () {
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#ReconTypeListPopUp')).modal('hide');
        }
        $scope.accIndex = -1;
    };
    $scope.setReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.accountGroupVendorList[$scope.accIndex].GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.accountGroupVendorList[$scope.accIndex].VendorReconGLCode = x.GLGeneralInfoCode;
        $scope.accountGroupVendorList[$scope.accIndex].VendorRecontGLText = x.GLGeneralInfoName;
        getVendorReconBudget(x.GLGeneralInfoId, $scope.accIndex);
    };
    $scope.refreshAccGroup = function (index) {
        $scope.accountGroupVendorList[index].GLGeneralInfoId = null;
        $scope.accountGroupVendorList[index].VendorReconGLCode = null;
        $scope.accountGroupVendorList[index].VendorRecontGLText = null;
        $scope.accountGroupVendorList[index].VendorReconBudgetMasterId = null;
        $scope.accountGroupVendorList[index].VendorReconActivityId = null;
        $scope.accountGroupVendorList[index].VendorReconBudgetList = [];
        $scope.accountGroupVendorList[index].VendorReconActivityList = [];
    }

    function getVendorReconBudget(id, index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, id, function (result) {
            $scope.accountGroupVendorList[index].VendorReconBudgetList = result;
        });
    }

    $scope.clearingAccountActivityList = [];
    $scope.getVendorReconActivity = function (id, index) {
        cboService.getBudgetMasterActivityCbo(id, function (result) {
            $scope.accountGroupVendorList[index].VendorReconActivityList = result;
        });
    }
    // #endregion
    // #region ******CustomerRecon GL******
    $scope.searchCustomerReconTypeByList = [
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
    $scope.customerReconTypeListParameters = {
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
    $scope.customerReconGLSelectedList = [];
    $scope.GetCustomerReconGlList = function (index) {
        $scope.accReceivableIndex = index;
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetCustomerReconeGLCOAWise?coaId=' + $scope.materialGroupGL.COAId;
        $scope.GetCustomerReconTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.customerReconTypeListParameters)
                .then(function (data) {
                    $scope.CustomerReconTypeGLList = data.Rows;
                    $scope.customerReconTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CustomerReconTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCustomerReconTypeListData();
    };

    $scope.closeCustomerReconTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#CustomerReconTypeListPopUp')).modal('hide');
        }
    };

    $scope.setCustomerReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.accountGroupCustomerList[$scope.accReceivableIndex].GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.accountGroupCustomerList[$scope.accReceivableIndex].VendorReconGLCode = x.GLGeneralInfoCode;
        $scope.accountGroupCustomerList[$scope.accReceivableIndex].VendorRecontGLText = x.GLGeneralInfoName;
        getCustomerReconBudget(x.GLGeneralInfoId, $scope.accReceivableIndex);
    };

    $scope.refreshCustomerAccGroup = function (index) {
        $scope.accountGroupCustomerList[index].GLGeneralInfoId = null;
        $scope.accountGroupCustomerList[index].VendorReconGLCode = null;
        $scope.accountGroupCustomerList[index].VendorRecontGLText = null;
        $scope.accountGroupCustomerList[index].VendorReconBudgetMasterId = null;
        $scope.accountGroupCustomerList[index].VendorReconActivityId = null;
        $scope.accountGroupCustomerList[index].VendorReconBudgetList = [];
        $scope.accountGroupCustomerList[index].VendorReconActivityList = [];
    };

    function checkExistVendorRecon(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].GLGeneralInfoId === id) {
                return true;
            }
        }
        return false;
    }

    function getCustomerReconBudget(id, index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, id, function (result) {
            $scope.accountGroupCustomerList[index].VendorReconBudgetList = result;
        });
    }

    $scope.getCustomerReconActivity = function (id, index) {
        cboService.getBudgetMasterActivityCbo(id, function (result) {
            $scope.accountGroupCustomerList[index].VendorReconActivityList = result;
        });
    };

    $scope.searchRevenueTypeByList = [
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

    $scope.revenueTypeListParameters = {
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

    $scope.getRevenueTypeList = function (index) {
        $scope.accSalesIndex = index;
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.materialGroupGL.COAId;

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
        $scope.accountGroupSalesList[$scope.accSalesIndex].GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorReconGLCode = x.GLGeneralInfoCode;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorRecontGLText = x.GLGeneralInfoName;
        getRevenueBudget(x.GLGeneralInfoId, $scope.accSalesIndex);
    };

    $scope.refreshRevenueGL = function () {
        $scope.SalesGLInfo = null;
        $scope.SalesGLId = null;
    };

    $scope.salesBudgetList = [];
    function getRevenueBudget(id, index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, id, function (result) {
            $scope.accountGroupSalesList[index].VendorReconBudgetList = result;
        });
    }

    $scope.salesActivityList = [];
    $scope.getRevenueActivity = function (id, index) {
        cboService.getBudgetMasterActivityCbo(id, function (result) {
            $scope.accountGroupSalesList[index].VendorReconActivityList = result;
        });
    };

    $scope.closeMaterialGroupGLListPopUpSelected = function () {
        $scope.materialGroupGLRowList = [];
        angular.forEach($scope.selectMaterialGroupMasterWithCombineList, function (item) {
            if (item.Active) {
                if (checkIsAvailable($scope.materialGroupGLRowList, item.MaterialGroupMasterId) === false) {
                    $scope.materialGroupGLRowList.push(item);
                }
            }
        });
        if ($scope.materialGroupGLRowList.length > 0) {
            angular.element(document.querySelector('#itemsearchpopup')).modal('hide');
            $scope.selectedPayableTblShow = true;
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };

    function checkIsAvailable(list, MaterialGroupMasterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialGroupMasterId === MaterialGroupMasterId) {
                return true;
            }
        }
        return false;
    }
    $scope.addGlForSelectble = function () {
        $scope.materialGroupGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.CheckBoxSelect) {
                if (!baseService.isUndefinedOrNull($scope.DownPaymentGLId)) {
                    item.DownPaymentGLId = $scope.DownPaymentGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.DownPaymentBudgetMasterId)) {
                    item.DownPaymentBudgetMasterId = $scope.materialGroupGL.DownPaymentBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.DownPaymentActivityId)) {
                    item.DownPaymentActivityId = $scope.materialGroupGL.DownPaymentActivityId;
                }
                if (!baseService.isUndefinedOrNull($scope.ClearingAccountGLId)) {
                    item.ClearingAccountGLId = $scope.ClearingAccountGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.InventoryInTransitGLId)) {
                    item.InventoryInTransitGLId = $scope.InventoryInTransitGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.ClearingAccountBudgetMasterId)) {
                    item.ClearingAccountBudgetMasterId = $scope.materialGroupGL.ClearingAccountBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.InventoryInTransitBudgetMasterId)) {
                    item.InventoryInTransitBudgetMasterId = $scope.materialGroupGL.InventoryInTransitBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.ClearingAccountActivityId)) {
                    item.ClearingAccountActivityId = $scope.materialGroupGL.ClearingAccountActivityId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.InventoryInTransitActivityId)) {
                    item.InventoryInTransitActivityId = $scope.materialGroupGL.InventoryInTransitActivityId;
                }
                if (!baseService.isUndefinedOrNull($scope.InventoryGLId)) {
                    item.InventoryGLId = $scope.InventoryGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.InventoryBudgetMasterId)) {
                    item.InventoryBudgetMasterId = $scope.materialGroupGL.InventoryBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.InventoryActivityId)) {
                    item.InventoryActivityId = $scope.materialGroupGL.InventoryActivityId;
                }
                if (!baseService.isUndefinedOrNull($scope.ExpenseGLId)) {
                    item.ExpenseGLId = $scope.ExpenseGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.ExpenseBudgetMasterId)) {
                    item.ExpenseBudgetMasterId = $scope.materialGroupGL.ExpenseBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.ExpenseActivityId)) {
                    item.ExpenseActivityId = $scope.materialGroupGL.ExpenseActivityId;
                }

                if (!baseService.isUndefinedOrNull($scope.DebitNoteGLId)) {
                    item.DebitNoteGLId = $scope.DebitNoteGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.DebitNoteBudgetMasterId)) {
                    item.DebitNoteBudgetMasterId = $scope.materialGroupGL.DebitNoteBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.DebitNoteActivityId)) {
                    item.DebitNoteActivityId = $scope.materialGroupGL.DebitNoteActivityId;
                }

                if (!baseService.isUndefinedOrNull($scope.ShortageGLId)) {
                    item.ShortageGLId = $scope.ShortageGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.ShortageBudgetMasterId)) {
                    item.ShortageBudgetMasterId = $scope.materialGroupGL.ShortageBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.ShortageActivityId)) {
                    item.ShortageActivityId = $scope.materialGroupGL.ShortageActivityId;
                }

                if (!baseService.isUndefinedOrNull($scope.CreditNoteGLId)) {
                    item.CreditNoteGLId = $scope.CreditNoteGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.CreditNoteBudgetMasterId)) {
                    item.CreditNoteBudgetMasterId = $scope.materialGroupGL.CreditNoteBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.CreditNoteActivityId)) {
                    item.CreditNoteActivityId = $scope.materialGroupGL.CreditNoteActivityId;
                }

                if (!baseService.isUndefinedOrNull($scope.RejectionGLId)) {
                    item.RejectionGLId = $scope.RejectionGLId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.RejectionBudgetMasterId)) {
                    item.RejectionBudgetMasterId = $scope.materialGroupGL.RejectionBudgetMasterId;
                }
                if (!baseService.isUndefinedOrNull($scope.materialGroupGL.RejectionActivityId)) {
                    item.RejectionActivityId = $scope.materialGroupGL.RejectionActivityId;
                }

                item.COAId = $scope.materialGroupGL.COAId;
                $scope.materialGroupGLListForSave.push(item);
            }
        });
    };

    function setReconGLForSave() {
        $scope.accountGroupSaveList = [];
        angular.forEach($scope.accountGroupSalesList, function (item) {
            $scope.accountGroupSaveList.push(item);
        });
        angular.forEach($scope.accountGroupCustomerList, function (item) {
            $scope.accountGroupSaveList.push(item);
        });
        angular.forEach($scope.accountGroupVendorList, function (item) {
            $scope.accountGroupSaveList.push(item);
        });
    }

    function checkVendorReconGLIsAssinged(list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].GLGeneralInfoId !== null) {
                return false;
                break;
            }
        }
        return true;
    }

    $scope.Save = function () {
        $scope.selectChValueId();
        $scope.closeMaterialGroupGLListPopUpSelected();
        $scope.addGlForSelectble();
        setReconGLForSave();
        if (baseService.isUndefinedOrNull($scope.DownPaymentGLId) && baseService.isUndefinedOrNull($scope.ClearingAccountGLId) && baseService.isUndefinedOrNull($scope.InventoryGLId)
            && baseService.isUndefinedOrNull($scope.ExpenseGLId) && baseService.isUndefinedOrNull($scope.DebitNoteGLId) && baseService.isUndefinedOrNull($scope.CreditNoteGLId)
            && baseService.isUndefinedOrNull($scope.ShortageGLId) && baseService.isUndefinedOrNull($scope.RejectionGLId) && baseService.isUndefinedOrNull($scope.SalesGLId) && checkVendorReconGLIsAssinged($scope.accountGroupSaveList)) {
            return ShowResult("Please Select at least one GL!!", 'failure');
        }
        if ($scope.materialGroupGLListForSave.length < 1) {
            return ShowResult("No list found!!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialGroupGLNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'materialGroupGL': $scope.materialGroupGLListForSave,
                        'materialGroupVendorReconGL': $scope.accountGroupSaveList,
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
                        $scope.getMaterialGroupMasterWithCoa('all');
                        //$scope.setGlLebelHide();
                        $scope.setActiveBtn('all');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };
    $scope.getReport = function () {
        try {
            var file_src = $scope.path + 'GetReport';
            $rootScope.report(file_src);

        } catch (e) {

        }

    }

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure to untag GL permanently on [ ' + data.MaterialGroupMasterName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.selectMaterialGroupMasterWithCombineList[i].Id == $scope.glUntagId) {
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
        $scope.selectMaterialGroupMasterWithCombineList[i] = {
            Id: null,
            MaterialGroupMasterName: $scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroupMasterName,
            COAId: $scope.selectMaterialGroupMasterWithCombineList[i].COAId,
            COAName: $scope.selectMaterialGroupMasterWithCombineList[i].COAName,
            MaterialGroup1Name: $scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroup1Name,
            MaterialGroup2Name: $scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroup2Name,
            MaterialGroup3Name: $scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroup3Name,
            MaterialGroup4Name: $scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroup4Name,
            MaterialGroupMasterId: $scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroupMasterId
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
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.tempList.length; i++) {
                        if ($scope.tempList[i].Id === id) {
                            document.getElementById($scope.tempList[i].MaterialGroupMasterId).checked = false;
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }
                    unTagFromList(index);
                    $scope.glUntagIndex = -1;
                }
            }, function errorCallback(response) {
                 
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
                $scope.getMaterialGroupMasterWithCoa('all');
            } else if ($scope.btnSet === 'notassing') {
                getMaterialGroupMasterWithCoa('notassing');
            } else if ($scope.btnSet === 'assing') {
                getMaterialGroupMasterWithCoa('assing');
            }
        }
    };

    $scope.clearGlField = function () {
        $scope.AssetUnderConstructionGLId = null;
        $scope.DownPaymentGLInfo = null;
        $scope.DownPaymentGLGLId = null;
        $scope.ClearingAccGLInfo = null;
        $scope.ClearingAccountGLId = null;
        $scope.InventoryInTransitInfo = null;
        $scope.InventoryInTransitGLId = null;
        $scope.refreshInventoryGL();
        $scope.refreshInventoryInTransit();
        $scope.refreshExpenseGL();
        $scope.refreshRevenueGL();
        $scope.refreshDebitNoteGL();
        $scope.refreshCreditNoteGL();
        $scope.refreshRejectionGL();
        $scope.refreshShortageGL();
        angular.forEach($scope.accountGroupVendorList, function (item) {
            item.GLGeneralInfoId = null;
            item.VendorReconGLCode = null;
            item.VendorRecontGLText = null;
            item.BudgetMasterId = null;
            item.ActivityId = null;
            item.VendorReconBudgetList = [];
            item.VendorReconActivityList = [];
        });
        angular.forEach($scope.accountGroupCustomerList, function (item) {
            item.GLGeneralInfoId = null;
            item.VendorReconGLCode = null;
            item.VendorRecontGLText = null;
            item.BudgetMasterId = null;
            item.ActivityId = null;
            item.VendorReconBudgetList = [];
            item.VendorReconActivityList = [];
        });
        angular.forEach($scope.accountGroupSalesList, function (item) {
            item.GLGeneralInfoId = null;
            item.VendorReconGLCode = null;
            item.VendorRecontGLText = null;
            item.BudgetMasterId = null;
            item.ActivityId = null;
            item.VendorReconBudgetList = [];
            item.VendorReconActivityList = [];
        });
        var tempList = [];
    };

    $scope.refreshDrp = function () {
        $scope.materialGroup1Ids = [];
        $scope.materialGroup2Ids = [];
        $scope.materialGroup3Ids = [];
        $scope.materialGroup4Ids = [];
        $scope.materialTypeIds = [];
        $scope.tempList = [];
        $scope.getDataWithCoaChange();
    };

    $scope.Clear = function () {
        ClearFields();
    };



    function ClearFields() {
        $scope.Action = "Save";
        $scope.materialGroupGL = { COAId: $scope.materialGroupGL.COAId };
        $scope.materialGroup1Ids = [];
        $scope.materialGroup2Ids = [];
        $scope.materialGroup3Ids = [];
        $scope.materialGroup4Ids = [];
        $scope.materialTypeIds = [];
        $scope.tempList = [];
        $scope.getDataWithCoaChange();
        $scope.clearGlField();
        $scope.selectMaterialGroupMasterWithCombineList = [];
        $scope.vendorReconGLSelectedList = [];
        if ($scope.selectMaterialGroupMasterWithCombineList.length > 0) {
            $scope.tableShow = true;
        } else {
            $scope.tableShow = false;
        }
    }
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === "";
    };
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.selectMaterialGroupMasterWithCombineList, { 'MaterialGroupMasterId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
            
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState === "check") {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
                    $scope.selectMaterialGroupMasterWithCombineList[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroupMasterId === filtered[j].MaterialGroupMasterId)
                            $scope.selectMaterialGroupMasterWithCombineList[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.selectMaterialGroupMasterWithCombineList.length; i++) {
                    $scope.selectMaterialGroupMasterWithCombineList[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.selectMaterialGroupMasterWithCombineList[i].MaterialGroupMasterId === filtered[j].MaterialGroupMasterId)
                            $scope.selectMaterialGroupMasterWithCombineList[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Grid .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex === 0) {
          //  $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }
    }

    //#region Inventory In Transit
    $scope.InventoryInTransitList = [];
    $scope.InventoryInTransitInfo = null;
    $scope.InventoryInTransitGLId = null;
    $scope.clearingInventoryInTransitParameters = {
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
    $scope.searchInventoryInTransitList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.GetInventoryInTransitList = function () {
        if ($scope.materialGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetClearingAccountGL?coaId=' + $scope.materialGroupGL.COAId;
        $scope.GetInventoryInTransitListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.clearingInventoryInTransitParameters)
                .then(function (data) {
                    $scope.InventoryInTransitList = data.Rows;
                    $scope.clearingInventoryInTransitParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#InventoryInTransitGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetInventoryInTransitListData();
    };
    $scope.closeInventoryInTransitListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#InventoryInTransitGLListPopUp')).modal('hide');
        }
    };
    $scope.setInventoryInTransitSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.InventoryInTransitInfo = x.GLGeneralInfoName;
        $scope.InventoryInTransitGLId = x.GLGeneralInfoId;
        getInventoryInTransitBudget();
    };
    $scope.refreshInventoryInTransit = function () {
        $scope.InventoryInTransitInfo = null;
        $scope.InventoryInTransitGLId = null;
    }
    $scope.InventoryInTransitActivityList = [];
    $scope.getInventoryInTransitActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.materialGroupGL.InventoryInTransitBudgetMasterId, function (result) {
            $scope.InventoryInTransitActivityList = result;
        });
    }
    $scope.InventoryInTransitBudgetList = [];
    function getInventoryInTransitBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.materialGroupGL.COAId, $scope.InventoryInTransitGLId, function (result) {
            $scope.InventoryInTransitBudgetList = result;
        });
    }
    //#endregion

}