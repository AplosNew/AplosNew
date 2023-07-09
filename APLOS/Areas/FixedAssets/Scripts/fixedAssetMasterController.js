'use strict';
fixedAssetMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function fixedAssetMasterController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = 'FixedAsset Master';
    $scope.Action = 'Save';
    $scope.ActionItem = 'Save';
    $scope.index = -1;
    $scope.FixedAssetMasters = [];
    $scope.glTagList = [];
    $scope.ReconAssetTypeGLList = [];
    $scope.AccDepreciationGLTypeList = [];
    $scope.DepreciationTypeGLList = [];
    $scope.AUCGLTypeList = [];
    $scope.path = 'fixedassets/fixedassetmaster/';
    $scope.fixedAssetMasterXLUrl = 'fixedassets/fixedassetmaster/fixedassetmasterreport';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveChildUrl = $scope.path + 'CreateChild'; 

    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Category',
            'value': 'FixedAssetCategory'
        },
        {
            'name': 'Sub Category',
            'value': 'FixedAssetSubCategory'
        },
        {
            'name': 'Asset Type',
            'value': 'AssetType'
        }
    ];

    $scope.fixedAssetMaster = {
        Id: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        CompanyGroupId: null,
        FixedAssetCategoryId: null,
        FixedAssetSubCategoryId: null,
        AssetType: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: new Date()
    };

    $scope.glTag = {
        Id: null,
        FixedAssetMasterId: null,
        FixedAssetGLId: null,
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
        LossOnDisposalAssetActivityId: null,
        COAId: null,
        AssetGLInfo: null,
        AccumulatedDepreciationGLInfo: null,
        DepreciationGLInfo: null,
        AUCGLCode: null,
        AUCGLText: null
    };

    $scope.fixedAssetTypes = [
        {
            Value: 'Building',
            Text: 'Building'
        },
        {
            Value: 'Machine',
            Text: 'Machine'
        },
        {
            Value: 'Equipment',
            Text: 'Equipment'
        },
        {
            Value: 'Land&SiteDevelopment',
            Text: 'Land&SiteDevelopment'
        },
        {
            Value: 'Plant',
            Text: 'Plant'
        },
        {
            Value: 'Vehicle',
            Text: 'Vehicle'
        },
        {
            Value: 'Other',
            Text: 'Other'
        }
    ];

    $scope.fixedAssetMasterItem = {
        Id: null,
        Code: null,
        FixedAssetMasterId: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        CapacityUoMId: null,
        CapacityValue: null,
        Active: true
    };
    $scope.ModelChildNew = Object.assign({}, $scope.fixedAssetMasterItem);

    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.FixedAssetMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.fixedAssetCategoryList = [];
    cboService.getFixedAssetCategoryList(function (result) {
        $scope.fixedAssetCategoryList = result;
    });

    $scope.fixedAssetClassList = [];
    cboService.getFixedAssetClassList(function (result) {
        $scope.fixedAssetClassList = result;
    });

    $scope.fixedAssetSubClassList = [];
    cboService.getFixedAssetSubClassList(function (result) {
        $scope.fixedAssetSubClassList = result;
    });

    $scope.fixedAssetSubCategoryList = [];
    cboService.getFixedAssetSubCategoryList(function (result) {
        $scope.fixedAssetSubCategoryList = result;
    });

    $scope.AssetAttributeList = [];
    cboService.getEnumCbo('enum/getassetattributeforcbo', function (result) {
        $scope.AssetAttributeList = result;
    });

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.loadAccountGroup = function (pageno) {
        try {
            $scope.accountGroupList = [];
            $http.get('Parties/PartyAccountGroup/GetList?accountType=' + 'Vendor')
                .then(function (result) {
                    angular.forEach(result.data.Rows, function (item) {
                        $scope.accountGroupList.push(
                            {
                                Id: null,
                                PartyAccountGroupId: item.Id,
                                Code: item.Code,
                                UserName: item.UserName,
                                FixedAssetGLId: null,
                                VendorReconGLId: null,
                                VendorReconGLCode: null,
                                VendorRecontGLText: null
                            }
                        );
                    });
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.loadAccountGroup();

    $scope.searchReconAssetTypeByList = [
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
        sort: 'AccountGroupName',
        searchBy: "AccountGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetReconAssetTypeList = function () {
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetBalanceSheetGLAssetRecon?coaId=' + $scope.glTag.COAId;
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
        $scope.glTag.AssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.FixedAssetGLId = x.GLGeneralInfoId;
    };
    $scope.refreshAssetGL = function () {
        $scope.glTag.AssetGLInfo = null;
        $scope.glTag.FixedAssetGLId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.FixedAssetGLId
        }).then(function successCallback(response) {
            $scope.assetBudgetList = response.data;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.AssetBudgetId,
        }).then(function successCallback(response) {
            $scope.assetActivityList = response.data;
        });
    };

    $scope.getGlTagList = function () {
        $http({
            method: 'GET',
            url: 'fixedassets/FixedAssetGL/GetDataByFixedAssetMasterId?fixedAssetMasterId=' + $scope.fixedAssetMaster.Id + '&coaId=' + $scope.glTag.COAId
        }).then(function successCallback(response) {
            $scope.fixedasseGroupList = response.data;
        });
    };

    $scope.searchAccDepreciationTypeByList = [
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
        sort: 'AccountGroupName',
        searchBy: "AccountGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetAccDepreciationGLTypeList = function () {
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl3 = 'accounts/glitem/getaccomultatedepriciationgl?coaId=' + $scope.glTag.COAId;
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
        $scope.glTag.AccumulatedDepreciationGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.AccumulatedDepreciationGLId = x.GLGeneralInfoId;
    };

    $scope.refreshAccDepreciationGL = function () {
        $scope.glTag.AccumulatedDepreciationGLInfo = null;
        $scope.glTag.AccumulatedDepreciationGLId = null;
    };

    $scope.accDepreciationBudgetList = [];
    function getAccDepreciationBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.AccumulatedDepreciationGLId,
        }).then(function successCallback(response) {
            $scope.accDepreciationBudgetList = response.data;
        });
    }

    $scope.accDepreciationActivityList = [];
    $scope.getAccDepreciationActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.AccDepreciationBudgetId
        }).then(function successCallback(response) {
            $scope.accDepreciationActivityList = response.data;
        });
    };

    $scope.searchDepreciationTypeByList = [
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
        searchBy: "AccountGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDepreciationTypeList = function () {
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/getdepriciationexpensesgl?coaId=' + $scope.glTag.COAId;
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
        $scope.glTag.DepreciationGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.DepreciationGLId = x.GLGeneralInfoId;
    };
    $scope.refreshDepreciationGL = function () {
        $scope.glTag.DepreciationGLInfo = null;
        $scope.glTag.DepreciationGLId = null;
    };

    $scope.depreciationBudgetList = [];
    function getDepreciationBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.DepreciationGLId
        }).then(function successCallback(response) {
            $scope.depreciationBudgetList = response.data;
        });
    }
    $scope.depreciationActivityList = [];
    $scope.getDepreciationActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.DepreciationBudgetId
        }).then(function successCallback(response) {
            $scope.depreciationActivityList = response.data;
        });
    };

    $scope.searchAUCGLTypeByList = [
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
        searchBy: "AccountGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetAUCGLTypeList = function () {
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl2 = 'accounts/glitem/getaucgl?coaId=' + $scope.glTag.COAId;
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
        $scope.glTag.AUCGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.AssetUnderConstructionGLId = x.GLGeneralInfoId;
    };

    $scope.refreshAUCGL = function () {
        $scope.glTag.AUCGLInfo = null;
        $scope.glTag.AssetUnderConstructionGLId = null;
    };

    $scope.aUCGLBudgetList = [];
    function getAUCGLBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.AssetUnderConstructionGLId
        }).then(function successCallback(response) {
            $scope.aUCGLBudgetList = response.data;
        });
    }

    $scope.aUCGLActivityList = [];
    $scope.getACGLActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.AUCGLBudgetId
        }).then(function successCallback(response) {
            $scope.aUCGLActivityList = response.data;
        });
    };

    $scope.searchDownPaymentByList = [
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
        sort: 'AccountGroupName',
        searchBy: "AccountGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDownPaymentGlList = function () {
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorDownpaymentGLCOAWise?coaId=' + $scope.glTag.COAId;
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
        $scope.setDownPaymentGLSelected = x;
        $scope.glTag.DownPaymentGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.DownPaymentGLId = x.GLGeneralInfoId;
    };
    $scope.refreshDownPaymentGL = function () {
        $scope.glTag.DownPaymentGLInfo = null;
        $scope.glTag.DownPaymentGLId = null;
    };

    $scope.downPaymentBudgetList = [];
    function getDownPaymentBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.DownPaymentGLId,
        }).then(function successCallback(response) {
            $scope.downPaymentBudgetList = response.data;
        });
    }

    $scope.downPaymentActivityList = [];
    $scope.getACGLActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.DownPaymentBudgetId,
        }).then(function successCallback(response) {
            $scope.downPaymentActivityList = response.data;
        });
    };

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
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetClearingAccountGL?coaId=' + $scope.glTag.COAId;
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
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.glTag.ClearingAccountGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.ClearingAccountGLId = x.GLGeneralInfoId;
    };
    $scope.refreshClearingAccountGL = function () {
        $scope.glTag.ClearingAccountGLInfo = null;
        $scope.glTag.ClearingAccountGLId = null;
    }
    $scope.clearingAccountBudgetList = [];
    function getclearingAccountBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.ClearingAccountGLId,
        }).then(function successCallback(response) {
            $scope.clearingAccountBudgetList = response.data;
        });
    }
    $scope.clearingAccountActivityList = [];
    $scope.getACGLActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.clearingAccountBudgetId,
        }).then(function successCallback(response) {
            $scope.clearingAccountActivityList = response.data;
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
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetRevenueGLCOAWise?coaId=' + $scope.glTag.COAId;
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
        //$scope.selectedCode = x.GLGeneralInfoCode;
        $scope.glTag.GainOnSaleOfAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.GainOnSaleOfAssetGLId = x.GLGeneralInfoId;
    };

    $scope.refreshGainOnSaleAssetGL = function () {
        $scope.glTag.GainOnSaleAssetGLInfo = null;
        $scope.glTag.GainOnSaleOfAssetGLId = null;
    };

    $scope.gainOnSaleAssetBudgetList = [];
    function getGainOnSaleAssetBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.GainOnSaleOfAssetGLId,
        }).then(function successCallback(response) {
            $scope.gainOnSaleAssetBudgetList = response.data;
        });
    }

    $scope.gainOnSaleAssetActivityList = [];
    $scope.getACGLActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.GainOnSaleAssetBudgetId,
        }).then(function successCallback(response) {
            $scope.gainOnSaleAssetActivityList = response.data;
        });
    };

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
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetDepriciationExpensesGL?coaId=' + $scope.glTag.COAId;
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
        $scope.glTag.LossOnSaleOfAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.LossOnSaleOfAssetGLId = x.GLGeneralInfoId;
    };

    $scope.refreshLossOnSaleAssetGL = function () {
        $scope.glTag.LossOnSaleOfAssetGLInfo = null;
        $scope.glTag.LossOnSaleOfAssetGLId = null;
    };

    $scope.lossOnSaleAssetBudgetList = [];
    function getLossOnSaleAssetBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.LossOnSaleOfAssetGLId
        }).then(function successCallback(response) {
            $scope.lossOnSaleAssetBudgetList = response.data;
        });
    }

    $scope.lossOnSaleAssetActivityList = [];
    $scope.getLossOnSaleAssetActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.LossOnSaleAssetBudgetId
        }).then(function successCallback(response) {
            $scope.lossOnSaleAssetActivityList = response.data;
        });
    };

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
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetDepriciationExpensesGL?coaId=' + $scope.glTag.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#LossOnDisposalAssetGLListPopUp')).modal('hide');
        }
    };
    $scope.setLossOnDisposalAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.LossOnDisposalAssetGLSelectedData = x;
        $scope.glTag.LossOnDisposalAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.LossOnDisposalAssetGLId = x.GLGeneralInfoId;
    };
    $scope.refreshLossOnDisposalAssetGL = function () {
        $scope.glTag.LossOnDisposalAssetGLInfo = null;
        $scope.glTag.LossOnDisposalAssetGLId = null;
    };

    $scope.lossOnDisposalAssetBudgetList = [];
    function getLossOnDisposalAssetBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.glTag.LossOnDisposalAssetGLId
        }).then(function successCallback(response) {
            $scope.lossOnDisposalAssetBudgetList = response.data;
        });
    }
    $scope.lossOnDisposalAssetActivityList = [];
    $scope.getACGLActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.glTag.LossOnDisposalAssetBudgetId
        }).then(function successCallback(response) {
            $scope.lossOnDisposalAssetActivityList = response.data;
        });
    };

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
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWise?coaId=' + $scope.glTag.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#LessValueAssetGLListPopUp')).modal('hide');
        }
    };
    $scope.setLessValueAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.LessValueAssetGLSelectedData = x;
        $scope.glTag.LessValueAssetGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLItem;
        $scope.glTag.LessValueAssetGLId = x.GLGeneralInfoId;
    };
    $scope.refreshLessValueAssetGL = function () {
        $scope.glTag.LessValueAssetGLInfo = null;
        $scope.glTag.LessValueAssetGLId = null;
    };

    $scope.lessValueAssetBudgetList = [];
    function getLessValueAssetBudget() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetMasterCboByCOAAndGLId?glId=' + $scope.LessValueAssetGLId,
        }).then(function successCallback(response) {
            $scope.lessValueAssetBudgetList = response.data;
        });
    }
    $scope.lessValueAssetActivityList = [];
    $scope.getACGLActivity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBudgetActivityCbo?budgetId=' + $scope.fixedAssetGL.LessValueAssetBudgetId,
        }).then(function successCallback(response) {
            $scope.lessValueAssetActivityList = response.data;
        });
    };

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
        if ($scope.glTag.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetVendorReconeGLCOAWise?coaId=' + $scope.glTag.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#VendorReconListPopUp')).modal('hide');
        }
        $scope.accIndex = -1;
    };
    $scope.setVendorReconGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.accountGroupList[$scope.accIndex].VendorReconGLId = x.GLGeneralInfoId;
        $scope.accountGroupList[$scope.accIndex].VendorReconGLCode = x.GLGeneralInfoCode;
        $scope.accountGroupList[$scope.accIndex].VendorRecontGLText = x.GLItem;
    };
    $scope.refreshAccGroup = function (index) {
        $scope.accountGroupList[index].VendorReconGLId = null;
        $scope.accountGroupList[index].VendorReconGLCode = null;
        $scope.accountGroupList[index].VendorRecontGLText = null;
    }
    function checkExistVendorRecon(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].VendorReconGLId === id) {
                return true;
            }
        }
        return false;
    }
    // #endregion
    $scope.assignedVendorReconList = [];
    function getVendorReconAssignGl(fixedAssetMasterId) {
        $http({
            method: 'GET',
            url: 'fixedassets/FixedAssetGL/GetVendorReconDataByFixedAssetMasterId?fixedAssetMasterId=' + fixedAssetMasterId + '&coaId=' + $scope.glTag.COAId
        }).then(function successCallback(response) {
            $scope.assignedVendorReconList = response.data.Rows;
            if ($scope.assignedVendorReconList.length > 0) {
                setVendorReconToAccountGroupList($scope.assignedVendorReconList);
            } else {
                $scope.loadAccountGroup();
            }
        });
    }
    $scope.getFAD = function (fixedAssetMasterId) {
        $scope.glTag = { COAId: $scope.glTag.COAId }
        if (fixedAssetMasterId != null) {
            $http.get('fixedassets/FixedAssetGL/GetDataByFixedAssetMasterId?fixedAssetMasterId=' + fixedAssetMasterId + '&coaId=' + $scope.glTag.COAId)
                .then(function (response) {
                    if (response.data.Rows.length > 0) {
                        $scope.glTag = response.data.Rows[0];
                    }
                    console.log('gsf', $scope.glTag)
                });
        }
        getVendorReconAssignGl(fixedAssetMasterId);
    };

    function setVendorReconToAccountGroupList(list) {
        for (var i = 0; i < list.length; i++) {
            for (var j = 0; j < $scope.accountGroupList.length; j++) {
                if (list[i].PartyAccountGroupId === $scope.accountGroupList[j].PartyAccountGroupId) {
                    $scope.accountGroupList[j].VendorReconGLCode = list[i].VendorReconGLCode;
                    $scope.accountGroupList[j].VendorRecontGLText = list[i].VendorRecontGLText;
                }
            }
        }
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.fixedAssetMaster = $scope.FixedAssetMasters[$scope.index];
        $scope.glTag = {};
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fixedAssetMasterNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'fixedAssetMaster': $scope.fixedAssetMaster },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'fixedAssetMaster': $scope.fixedAssetMaster, 'fixedAssetGL': $scope.glTag, 'fixedAssetVendorReconGL': $scope.accountGroupList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.FixedAssetMasters[$scope.index] = $scope.fixedAssetMaster;
                            ClearFields();
                            $scope.getData();
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.fixedAssetMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fixedAssetMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.FixedAssetMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.fixedAssetMaster = {};
        $scope.fixedAssetMaster.Active = true;
        $scope.glTag = {};
        $scope.refreshAccGroup(0);
        $scope.refreshAccGroup(1);
        $scope.refreshAccGroup(2);
    }

    $scope.fixedAssetMasterReport = function () {
        location.href = 'fixedassets/fixedassetmaster/fixedassetmasterreport';
    };

    $scope.FixedAssetMasterList = [];
    $scope.selectFixedAssetMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetFixedAssetMaster',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.FixedAssetMasterList = resp.data;
        });
        angular.element(document.querySelector('#FAMPop')).modal('show');
    }
    $scope.doubleFixedAssetMaster = function (e) {
        $scope.ModelChildNew.FixedAssetMasterId = e.data.Id;
        $scope.ModelChildNew.FixedAssetMaster = e.data.UserName;
        angular.element(document.querySelector('#FAMPop')).modal('hide');
    }

    $scope.closeFAMPopUp = function () {
        angular.element(document.querySelector('#FAMPop')).modal('hide');
    }

    $scope.SaveChild = function () { 
            $http({
                method: 'POST',
                url: $scope.saveChildUrl,
                data: { 'data': $scope.ModelChildNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFAMI(); 
                    $scope.getFAMIData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            } 
    };

    $scope.ClearFAMI = function () {
        $scope.ModelChildNew = Object.assign({}, $scope.fixedAssetMasterItem);
        $scope.ActionItem = 'Save';
    }

    $scope.searchByFAMIList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Fixed Asset Master Name',
            'value': 'FixedAssetMaster'
        },
        {
            'name': 'Capacity UoM',
            'value': 'CapacityUoM'
        },
        {
            'name': 'Capacity Value',
            'value': 'CapacityValue'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];

    $scope.FixedAssetMasterItemList = [];
    $scope.getFAMIListUrl = $scope.path + 'getFAMIlist';
    baseService.init($scope.getFAMIListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getFAMIData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.FixedAssetMasterItemList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getFAMIData();

    $scope.GetFAMI = function (id, index) {
        $scope.index = index;
        $scope.ModelChildNew = $scope.FixedAssetMasterItemList[$scope.index];
        $scope.ActionItem = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.message_confirmation = "Are you sure want to permanent delete ?";
    $scope.DeleteFAMI = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelChildNew.Id)) {
            $http.get('fixedassets/fixedassetmaster/DeleteFAMI?Id=' + $scope.ModelChildNew.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClearFAMI();
                        $scope.getFAMIData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

}