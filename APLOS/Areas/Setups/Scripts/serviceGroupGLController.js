'use strict';
ServiceGroupGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function ServiceGroupGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Material Group GL";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.fixedasseGroupList = [];
    $scope.fixedAssetSelectList = [];
    $scope.serviceGroupGLList = [];
    $scope.selectServiceGroupMasterWithCombineList = [];
    $scope.ReconAssetTypeGLList = [];
    $scope.AccDepreciationGLTypeList = [];
    $scope.DepreciationTypeGLList = [];
    $scope.AUCGLTypeList = [];
    if ($scope.selectServiceGroupMasterWithCombineList.length > 0) {
        $scope.tableShow = true;
    } else {
        $scope.tableShow = false;
    }
    $scope.serviceGroupGLRowList = [];
    $scope.path = 'Setups/ServiceGroupGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateServiceGroupDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.serviceGroupGL = {
        Id: null,
        COAId: null,
        ServiceGroupId: null,
        AccumulatedDepreciationGLId: null,
        DepreciationGLId: null,
        AssetUnderConstructionGLId: null,
        DownPaymentBudgetMasterId: null,
        DownPaymentActivityId: null,
        ClearingAccountBudgetMasterId: null,
        ClearingAccountActivityId: null,
        ServiceGLId: null,
        ServiceBudgetMasterId: null,
        ServiceActivity: null,
        ExpenseGLId: null,
        ExpenseBudgetMasterId: null,
        ExpenseActivityId: null
    };

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.getDataWithCoaChange = function () {
        $http.get('Setups/ServiceGroupGL/getlistwithcombineCoa')
            .then(function (response) {
                $scope.selectServiceGroupMasterWithCombineList = response.data.Rows;
            });
    };

    $scope.tempList = [];
    $scope.selectChValueId = function (event, ServiceGroupId, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListId($scope.tempList, data.ServiceGroupId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].ServiceGroupId === data.ServiceGroupId) {
                        $scope.tempList.splice(i, 1);
                    }
                    // break;
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempListId(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ServiceGroupId === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ServiceGroupId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.GetPartyAccountVDs = [];
    function GetPartyAccountVD(data) {
        $http.get('Setups/ServiceGroupGL/getpartyaccountvd')
            .then(function (response) {
                $scope.selectServiceGroupMasterWithCombineList = data;
                $scope.GetPartyAccountVDs = response.data.Rows;
                for (var i = 0; i < $scope.selectServiceGroupMasterWithCombineList.length; i++) {
                    $scope.selectServiceGroupMasterWithCombineList[i].Flag = getActive($scope.tempList, $scope.selectServiceGroupMasterWithCombineList[i].ServiceGroupId); //$scope.tempList.includes($scope.selectServiceGroupMasterWithCombineList[i].ServiceGroupId)
                }
                angular.forEach($scope.accountGroupSalesList, function (item, j) {
                    for (var i = 0; i < $scope.selectServiceGroupMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectServiceGroupMasterWithCombineList[i].PartyAccountGroupId, $scope.selectServiceGroupMasterWithCombineList[i].Id, item.PartyAccountGroupId, 'Sales');
                        $scope.selectServiceGroupMasterWithCombineList[i]['S' + j + 'GL'] = ob.GL;
                        $scope.selectServiceGroupMasterWithCombineList[i]['S' + j + 'Budget'] = ob.Budget;
                        $scope.selectServiceGroupMasterWithCombineList[i]['S' + j + 'Activity'] = ob.Activity;
                    }
                })
                angular.forEach($scope.accountGroupCustomerList, function (item, j) {
                    for (var i = 0; i < $scope.selectServiceGroupMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectServiceGroupMasterWithCombineList[i].PartyAccountGroupId, $scope.selectServiceGroupMasterWithCombineList[i].Id, item.PartyAccountGroupId, 'Receivable');
                        $scope.selectServiceGroupMasterWithCombineList[i]['C' + j + 'GL'] = ob.GL;
                        $scope.selectServiceGroupMasterWithCombineList[i]['C' + j + 'Budget'] = ob.Budget;
                        $scope.selectServiceGroupMasterWithCombineList[i]['C' + j + 'Activity'] = ob.Activity;
                    }
                });
                angular.forEach($scope.accountGroupVendorList, function (item, j) {
                    for (var i = 0; i < $scope.selectServiceGroupMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectServiceGroupMasterWithCombineList[i].PartyAccountGroupId, $scope.selectServiceGroupMasterWithCombineList[i].Id, item.PartyAccountGroupId, 'Payable');
                        $scope.selectServiceGroupMasterWithCombineList[i]['V' + j + 'GL'] = ob.GL;
                        $scope.selectServiceGroupMasterWithCombineList[i]['V' + j + 'Budget'] = ob.Budget;
                        $scope.selectServiceGroupMasterWithCombineList[i]['V' + j + 'Activity'] = ob.Activity;
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
            if (list[i].PartyAccountGroupId === pid && list[i].ServiceGroupGLId === adId && list[i].ClearingAccGLCode != null && list[i].GLType === gltype) {
                AccountDYOb.GL = list[i].ClearingAccGLCode + "-" + list[i].ClearingAccGLText;
                AccountDYOb.Budget = list[i].BudgetName;
                AccountDYOb.Activity = list[i].ActivityName;
                break;
                //vRT = list[i].ClearingAccGLCode + "-" + list[i].ClearingAccGLText + " [Budget:] " + list[i].BudgetName + " [Activity:]" + list[i].ActivityName;
            }
        }
        return AccountDYOb;
    }

    $scope.getServiceGroupMasterWithCoa = function (str) {
        $scope.selectServiceGroupMasterWithCombineList = [];
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        if ($scope.selectServiceGroupMasterWithCombineList.length > 0) {
            $scope.tableShow = true;
        } else {
            $scope.tableShow = false;
        }
        if (str === 'all') {
            $scope.url = 'Setups/ServiceGroupGL/getlistwithcombine?coaId=' + $scope.serviceGroupGL.COAId;
        }
        if (str === 'notassing') {
            $scope.btnActionAll = true;
            if ($scope.serviceGroupGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'Setups/ServiceGroupGL/getlistwithcombinenotassing?coaId=' + $scope.serviceGroupGL.COAId;
        }
        if (str === 'assing') {
            $scope.btnActionAll = true;
            if ($scope.serviceGroupGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'Setups/ServiceGroupGL/getlistwithcombineassing?coaId=' + $scope.serviceGroupGL.COAId;
        }
        baseService.setCurrentPage('selectServiceGroupMasterWithCombineList');
        baseService.init($scope.url, null, null, null, 'ServiceGroupName', 'ServiceGroupName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    //$scope.selectServiceGroupMasterWithCombineList = result.Rows;
                    if (result.Rows.length > 0) {
                        GetPartyAccountVD(result.Rows)
                    }
                    if (result.Rows.length > 0) {
                        $scope.tableShow = true;
                    } else {
                        $scope.tableShow = false;
                    }
                    console.log($scope.selectServiceGroupMasterWithCombineList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
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
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorDownpaymentGLCOAWise?coaId=' + $scope.serviceGroupGL.COAId;
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
        //$scope.selectedCode = x.GLGeneralInfoCode;
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
        cboService.getBudgetMasterCboByCOAAndGLId($scope.serviceGroupGL.COAId, $scope.DownPaymentGLId, function (result) {
            $scope.downPaymentBudgetList = result;
        });
    }
    $scope.downPaymentActivityList = [];
    $scope.getDownPaymentActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.serviceGroupGL.DownPaymentBudgetMasterId, function (result) {
            $scope.downPaymentActivityList = result;
        });
    };

    // #endregion
    // #region ******Service GL******
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

    $scope.GetServiceGlList = function () {
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.serviceGroupGL.COAId;
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
        $scope.ServiceGLInfo = x.GLGeneralInfoName;
        $scope.ServiceGLId = x.GLGeneralInfoId;
        getServiceBudget();
    };
    $scope.refreshServiceGL = function () {
        $scope.ServiceGLInfo = null;
        $scope.ServiceGLId = null;
    };

    $scope.serviceBudgetList = [];
    function getServiceBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.serviceGroupGL.COAId, $scope.ServiceGLId, function (result) {
            $scope.serviceBudgetList = result;
        });
    }
    $scope.serviceActivityList = [];
    $scope.getServiceActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.serviceGroupGL.ServiceBudgetMasterId, function (result) {
            $scope.serviceActivityList = result;
        });
    };

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
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetClearingAccountGL?coaId=' + $scope.serviceGroupGL.COAId;
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
        if ($scope.rowSelected != null) {
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
    };

    $scope.clearingAccountBudgetList = [];
    function getClearingAccountBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.serviceGroupGL.COAId, $scope.ClearingAccountGLId, function (result) {
            $scope.clearingAccountBudgetList = result;
        });
    }
    $scope.clearingAccountActivityList = [];
    $scope.getClearingAccountActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.serviceGroupGL.ClearingAccountBudgetMasterId, function (result) {
            $scope.clearingAccountActivityList = result;
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
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetExpenseGlList = function () {
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.serviceGroupGL.COAId;
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
        $scope.ExpenseGLInfo = x.GLGeneralInfoName;
        $scope.ExpenseGLId = x.GLGeneralInfoId;
        getExpenseBudget();
    };
    $scope.refreshExpenseGL = function () {
        $scope.ExpenseGLInfo = null;
        $scope.ExpenseGLId = null;
    };

    $scope.expenseBudgetList = [];
    function getExpenseBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.serviceGroupGL.COAId, $scope.ExpenseGLId, function (result) {
            $scope.expenseBudgetList = result;
        });
    }
    $scope.expenseActivityList = [];
    $scope.getExpenseActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.serviceGroupGL.ExpenseBudgetMasterId, function (result) {
            $scope.expenseActivityList = result;
        });
    }
    // #endregion

    // #region ******Sales******
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
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getRevenueTypeList = function (index) {
        $scope.accSalesIndex = index;
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.serviceGroupGL.COAId;

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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#revenueTypeListPopUp')).modal('hide');
        }
    };

    $scope.setRevenueGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.accountGroupSalesList[$scope.accSalesIndex].GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorReconGLCode = x.GLGeneralInfoCode;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorRecontGLText = x.GLItem;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorRecontGLInfo = x.GLGeneralInfoName;
        getRevenueBudget(x.GLGeneralInfoId, $scope.accSalesIndex);
    };

    $scope.refreshRevenueGL = function () {
        $scope.rowSelected = null;
        $scope.accountGroupSalesList[$scope.accSalesIndex].GLGeneralInfoId = null;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorReconGLCode = null;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorRecontGLText = null;
        $scope.accountGroupSalesList[$scope.accSalesIndex].VendorRecontGLInfo = null;
    };

    $scope.salesBudgetList = [];
    function getRevenueBudget(id, index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.serviceGroupGL.COAId, id, function (result) {
            $scope.accountGroupSalesList[index].VendorReconBudgetList = result;
        });
    }

    $scope.salesActivityList = [];
    $scope.getRevenueActivity = function (id, index) {
        cboService.getBudgetMasterActivityCbo(id, function (result) {
            $scope.accountGroupSalesList[index].VendorReconActivityList = result;
        });
    };

    // #endregion
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
    };

    $scope.buildValue = function () {
        $scope.NewListA = [];
        $scope.NewListCustomer = [];
        $scope.NewListSales = [];
        angular.forEach($scope.accountGroupVendorList, function (item, i) {
            $scope.NewListA.push('V' + i + "GL");
            $scope.NewListA.push('V' + i + "Budget");
            $scope.NewListA.push('V' + i + "Activity");
        });
        angular.forEach($scope.accountGroupCustomerList, function (item, i) {
            $scope.NewListCustomer.push('C' + i + "GL");
            $scope.NewListCustomer.push('C' + i + "Budget");
            $scope.NewListCustomer.push('C' + i + "Activity");
        });
        angular.forEach($scope.accountGroupSalesList, function (item, i) {
            $scope.NewListSales.push('S' + i + "GL");
            $scope.NewListSales.push('S' + i + "Budget");
            $scope.NewListSales.push('S' + i + "Activity");
        });
    };

    $scope.loadAccountGroup = function (pageno) {
        baseService.init('Parties/PartyAccountGroup/GetAllList', null, null, null, 'UserName', 'UserName');
        $scope.accountGroupVendorList = [];
        $scope.accountGroupCustomerList = [];
        $scope.accountGroupSalesList = [];
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
                                ServiceGroupGLId: null,
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
                                ServiceGroupGLId: null,
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
                                ServiceGroupGLId: null,
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
    $scope.recontypeListParameters = {
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
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorReconeGLCOAWise?coaId=' + $scope.serviceGroupGL.COAId;
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
        $scope.accountGroupVendorList[$scope.accIndex].VendorRecontGLText = x.GLItem;
        $scope.accountGroupVendorList[$scope.accIndex].VendorRecontGLACCInfo = x.GLGeneralInfoName;
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
        $scope.accountGroupVendorList[index].VendorRecontGLACCInfo = null;
    };



    function getVendorReconBudget(id, index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.serviceGroupGL.COAId, id, function (result) {
            $scope.accountGroupVendorList[index].VendorReconBudgetList = result;
        });
    }

    $scope.clearingAccountActivityList = [];
    $scope.getVendorReconActivity = function (id, index) {
        cboService.getBudgetMasterActivityCbo(id, function (result) {
            $scope.accountGroupVendorList[index].VendorReconActivityList = result;
        });
    };

    // #endregion
    // #region ******CustomerRecon GL******
    $scope.searchCustomerReconTypeByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.customerReconTypeListParameters = {
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
    $scope.customerReconGLSelectedList = [];
    $scope.GetCustomerReconGlList = function (index) {
        $scope.accReceivableIndex = index;
        if ($scope.serviceGroupGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetCustomerReconeGLCOAWise?coaId=' + $scope.serviceGroupGL.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#CustomerReconTypeListPopUp')).modal('hide');
        }
    };
    $scope.setCustomerReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.accountGroupCustomerList[$scope.accReceivableIndex].GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.accountGroupCustomerList[$scope.accReceivableIndex].VendorReconGLCode = x.GLGeneralInfoCode;
        $scope.accountGroupCustomerList[$scope.accReceivableIndex].VendorRecontGLText = x.GLItem;
        $scope.accountGroupCustomerList[$scope.accReceivableIndex].VendorRecontGLInfo = x.GLGeneralInfoName;
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
        $scope.accountGroupCustomerList[index].VendorRecontGLInfo = null;
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
        cboService.getBudgetMasterCboByCOAAndGLId($scope.serviceGroupGL.COAId, id, function (result) {
            $scope.accountGroupCustomerList[index].VendorReconBudgetList = result;
        });
    }

    $scope.getCustomerReconActivity = function (id, index) {
        cboService.getBudgetMasterActivityCbo(id, function (result) {
            $scope.accountGroupCustomerList[index].VendorReconActivityList = result;
        });
    };

    // #endregion
    $scope.closeServiceGroupGLListPopUpSelected = function () {
        $scope.serviceGroupGLRowList = [];
        angular.forEach($scope.selectServiceGroupMasterWithCombineList, function (item) {
            if (item.Active) {
                if (checkIsAvailable($scope.serviceGroupGLRowList, item.ServiceGroupId) === false) {
                    $scope.serviceGroupGLRowList.push(item);
                }
            }
        });
        if ($scope.serviceGroupGLRowList.length > 0) {
            angular.element(document.querySelector('#itemsearchpopup')).modal('hide');
            $scope.selectedPayableTblShow = true;
        } else {
            angular.element(document.querySelector('#cancelPopUp')).modal('show');
        }
    };
    function checkIsAvailable(list, ServiceGroupId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ServiceGroupId === ServiceGroupId) {
                return true;
            }
        }
        return false;
    }
    $scope.addGlForSelectble = function () {
        $scope.serviceGroupGLListForSave = [];
        //angular.forEach($scope.selectServiceGroupMasterWithCombineList, function (item) {
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.DownPaymentGLId != null) {
                    item.DownPaymentGLId = $scope.DownPaymentGLId;
                }
                if ($scope.serviceGroupGL.DownPaymentBudgetMasterId != null) {
                    item.DownPaymentBudgetMasterId = $scope.serviceGroupGL.DownPaymentBudgetMasterId;
                }
                if ($scope.serviceGroupGL.DownPaymentActivityId != null) {
                    item.DownPaymentActivityId = $scope.serviceGroupGL.DownPaymentActivityId;
                }
                if ($scope.ClearingAccountGLId !== null) {
                    item.ClearingAccountGLId = $scope.ClearingAccountGLId;
                }
                if ($scope.serviceGroupGL.ClearingAccountBudgetMasterId !== null) {
                    item.ClearingAccountBudgetMasterId = $scope.serviceGroupGL.ClearingAccountBudgetMasterId;
                }
                if ($scope.serviceGroupGL.ClearingAccountActivityId !== null) {
                    item.ClearingAccountActivityId = $scope.serviceGroupGL.ClearingAccountActivityId;
                }
                if ($scope.ServiceGLId !== null) {
                    item.ServiceGLId = $scope.ServiceGLId;
                }
                if ($scope.serviceGroupGL.ServiceBudgetMasterId !== null) {
                    item.ServiceBudgetMasterId = $scope.serviceGroupGL.ServiceBudgetMasterId;
                }
                if ($scope.serviceGroupGL.ServiceActivityId !== null) {
                    item.ServiceActivityId = $scope.serviceGroupGL.ServiceActivityId;
                }
                if ($scope.ExpenseGLId !== null) {
                    item.ExpenseGLId = $scope.ExpenseGLId;
                }
                if ($scope.serviceGroupGL.ExpenseBudgetMasterId !== null) {
                    item.ExpenseBudgetMasterId = $scope.serviceGroupGL.ExpenseBudgetMasterId;
                }
                if ($scope.serviceGroupGL.ExpenseActivityId !== null) {
                    item.ExpenseActivityId = $scope.serviceGroupGL.ExpenseActivityId;
                }
                item.COAId = $scope.serviceGroupGL.COAId;
                $scope.serviceGroupGLListForSave.push(item);
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
        //angular.copy($scope.fixedAssetClassNew, $scope.fixedAssetClass);
        $scope.closeServiceGroupGLListPopUpSelected();
        $scope.addGlForSelectble();
        setReconGLForSave();
        if (baseService.isUndefinedOrNull($scope.DownPaymentGLId) && baseService.isUndefinedOrNull($scope.ClearingAccountGLId) && baseService.isUndefinedOrNull($scope.ServiceGLId) && baseService.isUndefinedOrNull($scope.ExpenseGLId) && baseService.isUndefinedOrNull($scope.SalesGLId) && checkVendorReconGLIsAssinged($scope.accountGroupVendorList)) {
            return ShowResult("Please Select at least one GL!!", 'failure');
        }
        if ($scope.serviceGroupGLListForSave.length < 1) {
            return ShowResult("No list found!!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.serviceGroupGLNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'serviceGroupGL': $scope.serviceGroupGLListForSave,
                        'serviceGroupPartyAccountGroupGL': $scope.accountGroupSaveList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //baseService.paginationAdd();
                        ClearFields();
                        $scope.getServiceGroupMasterWithCoa('all');
                        $scope.clearGlField();
                        $scope.setGlLebelHide(); $scope.setActiveBtn('all');
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
        $scope.message_confirmation = 'Are you sure to untag GL permanently on [ ' + data.ServiceGroupName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.selectServiceGroupMasterWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.selectServiceGroupMasterWithCombineList[i].Id === $scope.glUntagId) {
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
        $scope.selectServiceGroupMasterWithCombineList[i] = {
            Id: null,
            ServiceGroupName: $scope.selectServiceGroupMasterWithCombineList[i].ServiceGroupName,
            ServiceTypeName: $scope.selectServiceGroupMasterWithCombineList[i].ServiceTypeName,
            COAId: $scope.selectServiceGroupMasterWithCombineList[i].COAId,
            COAName: $scope.selectServiceGroupMasterWithCombineList[i].COAName,
            ServiceGroupId: $scope.selectServiceGroupMasterWithCombineList[i].ServiceGroupId
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
                            document.getElementById($scope.tempList[i].ServiceGroupId).checked = false;
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }
                    unTagFromList(index);
                    $scope.glUntagIndex = -1;
                    //angular.element(document.querySelector('#confirmdocumentDeletePopUp')).modal('hide');
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
                $scope.getServiceGroupMasterWithCoa('all');
            } else if ($scope.btnSet === 'notassing') {
                getServiceGroupMasterWithCoa('notassing');
            } else if ($scope.btnSet === 'assing') {
                getServiceGroupMasterWithCoa('assing');
            }
        }
    };

    $scope.clearGlField = function () {
        $scope.AssetUnderConstructionGLId = null;
        $scope.DownPaymentGLInfo = null;
        $scope.DownPaymentGLGLId = null;
        $scope.ClearingAccGLInfo = null;
        $scope.ClearingAccGLGLId = null;
        $scope.refreshServiceGL();
        $scope.refreshExpenseGL();
        $scope.refreshRevenueGL();
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
        $scope.serviceGroup1Ids = [];
        $scope.serviceGroup2Ids = [];
        $scope.serviceGroup3Ids = [];
        $scope.serviceGroup4Ids = [];
        $scope.materialTypeIds = [];
        $scope.tempList = [];
        $scope.getDataWithCoaChange();
    };

    $scope.Clear = function () {
        ClearFields();
        $scope.clearGlField();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.serviceGroupGL = { COAId: $scope.serviceGroupGL.COAId };
        $scope.serviceGroup1Ids = [];
        $scope.serviceGroup2Ids = [];
        $scope.serviceGroup3Ids = [];
        $scope.serviceGroup4Ids = [];
        $scope.materialTypeIds = [];
        $scope.tempList = [];
       // $scope.getDataWithCoaChange();
       
        $scope.selectServiceGroupMasterWithCombineList = [];
        $scope.vendorReconGLSelectedList = [];
        if ($scope.selectServiceGroupMasterWithCombineList.length > 0) {
            $scope.tableShow = true;
        } else {
            $scope.tableShow = false;
        }
    }


    $scope.getServiceGroupGlReport = function () {
       
        try {
            var file_src = $scope.path + 'GetServiceGroupGlReport';
            $rootScope.report(file_src);

        } catch (e) {

        }
       


    }


}