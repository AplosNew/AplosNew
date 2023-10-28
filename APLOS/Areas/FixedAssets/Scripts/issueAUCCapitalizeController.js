'use strict';
issueAUCCapitalizeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function issueAUCCapitalizeController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = "FixedAsset Capitalize";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.dataList = [];
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.getListUrl = $scope.path + 'GetIssueCapitalizeFixedAssetGL';
    $scope.saveUrl = $scope.path + 'InsertIssueFixedAssetCapitalizeJournal';
    $scope.saveinventoryAUCUrl = $scope.path + 'InsertIssueInventoryCapitalizeJournal';

    baseService.init($scope.getListUrl, null, null, 'DESC', 'Id', 'Id');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.dataList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
   // $scope.getData();

    cboService.getCboVoucherTypeFixedAssetCapitalizeJournalList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    $scope.model = {
        Id: null
        , MaterialStorage: null
        , GRNDate: null
        , VoucherTypeId: null
        , EmployeeId: null
        , EmployeeCode: null
        , PartyCode: null
        , PartyName: null
        , InvoicingBy: null
        , InvoicingByAddress: null
        , GateEntryNo: null
        , PaymentTermName: null
        , BaseOnDueDate: null
        , EmployeeName: null
        , PartyAccountGroupName: null
        , DeliveryBy: null
        , DeliveryByAddress: null
        , EntryDate: null
        , CurrencyCode: null
        , MatureDate: null
        , IsNonCreditable: null
        , ToCurrencyRate: 0
        , FAType: 'AssetCapatalized'
    };
    $scope.modelNew = Object.assign({}, $scope.model);


    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.searchByList = [
        {
            value: 'Id'
            , name: 'Id No'
        },
        {
            value: 'Voucher No'
            , name: 'VoucherNo'
        },
        {
            value: 'Voucher Date'
            , name: 'VoucherDate'
        }
    ];

    $scope.columnExcluedList = [];
    $scope.popUp = function () {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'DESC',
            sort: 'Id',
            searchBy: "Id",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'FixedAssets/FixedAssetRegister/GetIssueAUCList';
        $scope.popUpTitle = 'Issue AUC Data';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };


    $scope.popUpDataList = [];
    $scope.popUp = function () {
        $http({
            method: 'GET',
            url: 'FixedAssets/FixedAssetRegister/GetIssueAUCList'
        }).then(function successCallback(response) {
            $scope.popUpDataList = response.data;
            // angular.element(document.querySelector('#popUpId')).modal('show');
        });
    }
    $scope.popUp();

    $scope.inventoryMaterialList = [];
    $scope.selectDoubleClick = function () {
        var gridObj = $("#popUpData").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data;
        $scope.modelNew.PostingDate = new Date();
        $scope.modelNew.GRNDate = data.GRNDate;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        getCapitalizeJournal(data);
        //$scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.capitalizeJournalDataList = [];
    function getCapitalizeJournal(data) {
        $scope.capitalizeJournalDataList = [];

        if (data.PostDrBudgetMasterId) $scope.getActivity(data.PostDrBudgetMasterId, data.PostDrActivityId)
        $scope.dr = {};
        $scope.cr = {};
        $scope.dr.GLGeneralInfoId = data.PostDrGLGeneralInfoId;
        $scope.dr.AccountCode = data.DrAccountCode;
        $scope.dr.GLGeneralInfoName = data.DrGLGeneralInfoName;
        $scope.dr.BudgetMasterId = data.PostDrBudgetMasterId;
        $scope.dr.BudgetCode = data.DrBudgetCode;
        $scope.dr.BudgetName = data.DrBudgetName;
        $scope.dr.ActivityId = data.PostDrActivityId;
        $scope.dr.ActivityCode = data.DrActivityCode;
        $scope.dr.ActivityName = data.DrActivityName;
        $scope.dr.Amount = parseFloat(data.Amount.toFixed(2));
        $scope.dr.FixedAssetMasterId = data.FixedAssetMasterId;
        $scope.dr.InventoryIssueHistoryId = data.InventoryIssueHistoryId;
        $scope.dr.TrnType = 'Dr';

        $scope.cr.GLGeneralInfoId = data.PostCrGLGeneralInfoId;
        $scope.cr.AccountCode = data.CrAccountCode;
        $scope.cr.GLGeneralInfoName = data.CrGLGeneralInfoName;
        $scope.cr.BudgetMasterId = data.PostCrBudgetMasterId;
        $scope.cr.BudgetCode = data.CrBudgetCode;
        $scope.cr.BudgetName = data.CrBudgetName;
        $scope.cr.ActivityId = data.PostCrActivityId;
        $scope.cr.ActivityCode = data.CrActivityCode;
        $scope.cr.ActivityName = data.CrActivityName;
        $scope.cr.Amount = parseFloat(data.Amount.toFixed(2));
        $scope.cr.TrnType = 'Cr';
        $scope.capitalizeJournalDataList.push($scope.dr);
        $scope.capitalizeJournalDataList.push($scope.cr);
    }

    $scope.PostButton = false;
    $scope.Post = function () {
        $scope.PostButton = true;
        $scope.modelNew.ToCurrencyRate = $scope.selectFixedAssetDataList[0].ToCurrencyRate;
        $scope.modelNew.CurrencyId = $scope.selectFixedAssetDataList[0].CurrencyId;
        $scope.modelNew.Id = $scope.selectFixedAssetDataList[0].IssueNo;
        $scope.modelNew.postingDate = $filter("dateFiltering")($scope.selectFixedAssetDataList[0].IssueDate);
        if ($scope.modelNew.FAType == 'AssetNonCapitalized') {
            $scope.modelNew.ToCurrencyRate = 1;
            $scope.modelNew.CurrencyId = $scope.selectFixedAssetDataList[0].CurrencyId;
            $scope.saveUrl = $scope.saveinventoryAUCUrl;
        }
        else
            $scope.saveUrl = $scope.saveUrl;
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                issueId: $scope.modelNew.Id
                , postingDate: $scope.modelNew.postingDate
                , voucherTypeId: $scope.modelNew.VoucherTypeId
                , currencyId: $scope.modelNew.CurrencyId
                , ToCurrencyRate: $scope.modelNew.ToCurrencyRate
                , voucherDetailVMList: $scope.capitalizeJournalList
                , invIssueDetailList: $scope.selectFixedAssetDataList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
                $scope.popUp();
                $scope.issueInventoryAUC();
                $scope.PostedAUCData();
                //$scope.PostButton = false;
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.Clear = function () {
        $scope.model = {};
        $scope.modelNew = { PostingDate: new Date() };
        $scope.capitalizeJournalList = [];
        $scope.selectFixedAssetDataList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
        $scope.PostButton = false;
    };

    //#region GL, Budget & Activity

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    $scope.searchGLByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.glListParameters = {
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
    $scope.getGLPopUP = function (index) {
        $scope.index = index;
        $scope.getGLData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.companyConfig.COAId, pageno, $scope.glListParameters)
                .then(function (data) {
                    $scope.glList = data.Rows;
                    $scope.glListParameters.total_count = data.Total;
                    angular.element(document.querySelector('#gltListPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getGLData();
    };

    $scope.setGL = function (data) {
        $scope.inventoryMaterialList[$scope.index].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.inventoryMaterialList[$scope.index].GLName = data.GLGeneralInfoCode + '-' + data.GLGeneralInfoName;
        getBudgetList($scope.index);
        $scope.closeGltListPopUp();
    };

    $scope.refreshGL = function (index) {
        $scope.inventoryMaterialList[index].GLGeneralInfoId = null;
        $scope.inventoryMaterialList[index].GLName = null;
        $scope.inventoryMaterialList[index].BudgetMasterId = null;
        $scope.inventoryMaterialList[index].ActivityId = null;
        $scope.inventoryMaterialList[index].budgetList = null;
        $scope.inventoryMaterialList[index].activityList = null;
    };

    $scope.closeGltListPopUp = function () {
        $scope.index = -1;
        angular.element(document.querySelector('#gltListPopUp')).modal('hide');
    };


    $scope.columnExcluedList = [];

    $scope.issueInventoryAUCDataList = [];
    $scope.issueInventoryAUC = function () {
        $http({
            method: 'GET',
            url: 'FixedAssets/FixedAssetRegister/GetIssueInventoryAUCList'
        }).then(function successCallback(response) {
            $scope.issueInventoryAUCDataList = response.data;
            //angular.element(document.querySelector('#issueInventoryAUC')).modal('show');
        });
    }
    $scope.issueInventoryAUC();

    $scope.selectIssueInventoryAUCDoubleClick = function () {
        var gridObj = $("#issueInventoryAUC").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data;
        $scope.modelNew.PostingDate = new Date();
        $scope.modelNew.GRNDate = data.GRNDate;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        getCapitalizeJournal(data);
        //$scope.closeIssueInventoryAUCPopUp();
    };

    $scope.closeIssueInventoryAUCPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#issueInventoryAUC')).modal('hide');
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.capitalizetype = function (data) {
        $scope.modelNew.FAType = data;
    }
    $scope.searchissueAUCglByList = [
        {
            "name": "Fixed Asset",
            "value": "FixedAssetName"
        },
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        }
    ];

    $scope.issueAUCglListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetName",
        searchBy: "FixedAssetName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.issueAUCglList = [];
    $scope.GetIssueAUC = function (index, data) {
        $scope.indexMB = index;
        $scope.TempData = {};
        $scope.TempData.bMasterId = data.PostDrBudgetMasterId;
        $scope.TempData.AId = data.PostCrActivityId;
        $scope.TempData.Amount = data.TrnAmount;
        $scope.IssueAUCGLUrl = "Accounts/glitem/GetAssetMasterGLBudget";
        baseService.setCurrentPage('issueAUCglList');
        $scope.GetIssueAUCGLData = function (pageno) {

            baseService.paginationBase($scope.IssueAUCGLUrl, pageno, $scope.issueAUCglListParameters)
                .then(function (result) {
                    $scope.issueAUCglList = result.Rows;
                    $scope.issueAUCglListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetIssueAUCGLData();
    };

    $scope.closeIssueAUCglListPopUp = function () {
        angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("hide");
    };

    $scope.setissueAUCglSelected = function (data) {
        $scope.selectFixedAssetDataList[$scope.indexMB].PostDrGLGeneralInfoId = data.GLGeneralInfoId;
        $scope.selectFixedAssetDataList[$scope.indexMB].DrGLGeneralInfoName = data.GLGeneralInfoName;
        $scope.selectFixedAssetDataList[$scope.indexMB].PostDrBudgetMasterId = data.BudgetMasterId;
        $scope.selectFixedAssetDataList[$scope.indexMB].DrBudgetName = data.BudgetName;
        $scope.selectFixedAssetDataList[$scope.indexMB].AssetMasterName = data.FixedAssetName;
        //$scope.getActivity(data.BudgetMasterId, data.ActivityId);
        $scope.getActivity($scope.selectFixedAssetDataList[$scope.indexMB].PostDrBudgetMasterId);
        //$scope.getActivityListWithCallBack($scope.selectFixedAssetDataList[$scope.indexMB].PostDrBudgetMasterId);

        $scope.getActivityListWithCallBack($scope.selectFixedAssetDataList[$scope.indexMB].PostDrBudgetMasterId, function (result) { });
        $scope.closeIssueAUCglListPopUp();
        //$scope.issueJournalNewBudjetAdd($scope.inventoryIssueList[$scope.indexMB]);
    };

    //$scope.setissueAUCglSelected = function (data) {
    //    $scope.capitalizeJournalDataList[$scope.indexMB].GLGeneralInfoId = data.GLGeneralInfoId;
    //    $scope.capitalizeJournalDataList[$scope.indexMB].GLGeneralInfoCode = data.GLGeneralInfoCode;
    //    $scope.capitalizeJournalDataList[$scope.indexMB].GLGeneralInfoName = data.GLGeneralInfoName;
    //    $scope.capitalizeJournalDataList[$scope.indexMB].BudgetMasterId = data.BudgetMasterId;
    //    $scope.capitalizeJournalDataList[$scope.indexMB].BudgetName = data.BudgetName;
    //    $scope.getActivity(data.BudgetMasterId, data.ActivityId);
    //    $scope.closeIssueAUCglListPopUp();
    //    //$scope.issueJournalNewBudjetAdd($scope.inventoryIssueList[$scope.indexMB]);
    //};
    $scope.refreshGLMB = function (index) {
        //$scope.capitalizeJournalDataList[index].GLGeneralInfoId = null;
        //$scope.capitalizeJournalDataList[index].GLGeneralInfoName = null;
        //$scope.capitalizeJournalDataList[index].BudgetMasterId = null;
        //$scope.capitalizeJournalDataList[index].BudgetName = null;
        //$scope.capitalizeJournalDataList[index].ActivityId = null;
        //$scope.capitalizeJournalDataList[index].activityList = null;

        $scope.selectFixedAssetDataList[index].GLGeneralInfoId = null;
        $scope.selectFixedAssetDataList[index].GLGeneralInfoName = null;
        $scope.selectFixedAssetDataList[index].BudgetMasterId = null;
        $scope.selectFixedAssetDataList[index].BudgetName = null;
        $scope.selectFixedAssetDataList[index].ActivityId = null;
        $scope.selectFixedAssetDataList[index].activityList = null;
    };
    $scope.activityList = [];
    $scope.getActivity = function (budgetMasterId) {
        cboService.getBudgetMasterActivityCbo(budgetMasterId, function (result) {
            //$scope.activityList = result;
            $scope.selectFixedAssetDataList[$scope.indexMB].PostDrActivityId = result[0].ActivityId;
            $scope.selectFixedAssetDataList[$scope.indexMB].DrActivityName = result[0].ActivityName;

            angular.forEach(result, function (item) {
                var ob = {
                    ActivityId: item.ActivityId,
                    ActivityName: item.ActivityName,
                    BudgetMasterId: item.BudgetMasterId
                }
                $scope.activityList.push(ob);
            })

        });

    };

    $scope.issueId = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {
        if ($scope.issueId != e.data.IssueNo) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.issueId = e.data.IssueNo;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#c8c8c8');
        else
            e.row.css("background-color", '#fff6b7');

    }

    $scope.issueId = "";
    $scope.isAlternative = -1;
    $scope.dataBoundInventory = function rowDataBound(e) {
        if ($scope.issueId != e.data.IssueNo) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.issueId = e.data.IssueNo;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#c8c8c8');

        else
            e.row.css("background-color", '#fff6b7');
    }

    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function checkChange(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.popUpDataList, { 'InventoryIssueHistoryId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }
    }

    function checkChangeInventory(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.issueInventoryAUCDataList, { 'InventoryIssueHistoryId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }
    }

    //$scope.dataBound = function (args) {
    //    $("#popUpData .rowCheckbox").ejCheckBox({ "change": checkChange });
    //    // $("#headchk").ejCheckBox({ "change": headCheckChange});
    //}

    $scope.refreshTemplate = function (args) {
        if (args.rowIndex == 0) {
            $("#popUpData").ejCheckBox({ "change": checkChange });
        }

        var valobj = $($("#popUpData .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#popUpData .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#popUpData .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.popUpDataList, { 'InventoryIssueHistoryId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#popUpData .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#popUpData .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#popUpData .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChange });
    }

    $scope.refreshInventoryTemplate = function (args) {
        if (args.rowIndex == 0) {
            $("#issueInventoryAUC").ejCheckBox({ "change": checkChangeInventory });
        }

        var valobj = $($("#issueInventoryAUC .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#issueInventoryAUC .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#issueInventoryAUC .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.issueInventoryAUCDataList, { 'InventoryIssueHistoryId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#issueInventoryAUC .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#issueInventoryAUC .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#issueInventoryAUC .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeInventory });
    }

    //$scope.dataBoundInventory = function (args) {
    //    $("#issueInventoryAUC .rowCheckbox").ejCheckBox({ "change": checkChangeInventory });
    //}
    // #endregion

    $scope.getActivityListWithCallBack = function (budgetMasterId, callback) {
        if (!baseService.isUndefinedOrNull($scope.activityList) && $scope.activityList.length > 0) {
            // $scope.PostDrBudgetMasterId = budgetMasterId
            $scope.PostDrBudgetMasterId = budgetMasterId + '01';
            callback($scope.activityList);
        }
        else {
            $http.get('accounts/BudgetMaster/GetBudgetMasterActivityCbo?budgetMasterId=' + budgetMasterId)
                .then(function (response) {
                    callback(response.data);
                    angular.forEach(response.data, function (item, i) {
                        $scope.activityList.push(item);
                        $scope.PostDrActivityId = item.Value;
                    });
                });
        }
    };


    $scope.selectFixedAssetDataList = [];
    $scope.Done = function (obj) {
        $scope.selectFixedAssetDataList = [];

        var gridObj = $("#popUpData").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        angular.forEach($scope.popUpDataList, function (item) {
            if (item.IssueNo === data.IssueNo && item.IssueDate === data.IssueDate) {//&& item.GRNNo === data.GRNNo
                var ob = {
                    Id: null,
                    IssueNo: item.IssueNo,
                    InventoryIssueDetailId: item.InventoryIssueDetailId,
                    InventoryIssueHistoryId: item.InventoryIssueHistoryId,
                    InventoryReceiveDetailId: item.InventoryReceiveDetailId,
                    GRNNo: item.GRNNo,
                    GateEntryNo: item.GateEntryNo,
                    GRNDate: item.GRNDate,
                    Type: item.Type,
                    EmployeeId: item.EmployeeId,
                    EmployeeCode: item.EmployeeCode,
                    EmployeeName: item.EmployeeName,
                    IssueDate: item.IssueDate,
                    TransactionUoM: item.TransactionUoM,
                    CurrencyCode: item.CurrencyCode,
                    GRNCurrency: item.GRNCurrency,
                    CurrencyId: item.CurrencyId,
                    ToCurrencyRate: item.ToCurrencyRate,
                    PartyCode: item.PartyCode,
                    PartyName: item.PartyName,
                    EntryDate: item.EntryDate,
                    IssueType: item.IssueType,
                    IsAsset: item.IsAsset,
                    MaterialGroupMasterName: item.MaterialGroupMasterName,
                    InventoryMaterialId: item.InventoryMaterialId,
                    TransactionQty: item.TransactionQty,
                    BaseQty: item.BaseQty,
                    BaseCurrencyRate: item.BaseCurrencyRate,
                    Amount: item.GRNAmount,
                    GRNBooksAmount: item.Amount,
                    BaseUOMId: item.BaseUOMId,
                    TransactionUoMId: item.TransactionUoMId,
                    MaterialMasterId: item.MaterialMasterId,
                    UserName: item.UserName,
                    ArticleId: item.ArticleId,
                    StandardName: item.StandardName,
                    FirstCharacteristicsId: item.FirstCharacteristicsId,
                    FirstCharacteristics: item.FirstCharacteristics,
                    FirstCharacteristics: item.FirstCharacteristics,
                    FirstCharacteristicsValueId: item.FirstCharacteristicsValueId,
                    FirstCharacteristicsValue: item.FirstCharacteristicsValue,
                    SecondCharacteristicsId: item.SecondCharacteristicsId,
                    SecondCharacteristics: item.SecondCharacteristics,
                    SecondCharacteristicsValueId: item.SecondCharacteristicsValueId,
                    SecondCharacteristicsValue: item.SecondCharacteristicsValue,
                    ThirdCharacteristicsId: item.ThirdCharacteristicsId,
                    ThirdCharacteristics: item.ThirdCharacteristics,
                    ThirdCharacteristicsValueId: item.ThirdCharacteristicsValueId,
                    ThirdCharacteristicsValue: item.ThirdCharacteristicsValue,
                    PostCrGLGeneralInfoId: item.PostCrGLGeneralInfoId,
                    CrAccountCode: item.CrAccountCode,
                    CrGLGeneralInfoName: item.CrGLGeneralInfoName,
                    PostCrBudgetMasterId: item.PostCrBudgetMasterId,
                    CrBudgetCode: item.CrBudgetCode,
                    CrBudgetName: item.CrBudgetName,
                    PostCrActivityId: item.PostCrActivityId,
                    CrActivityCode: item.CrActivityCode,
                    CrActivityName: item.CrActivityName,
                    PostDrGLGeneralInfoId: item.PostDrGLGeneralInfoId,
                    DrAccountCode: item.DrAccountCode,
                    DrGLGeneralInfoName: item.DrGLGeneralInfoName,
                    PostDrBudgetMasterId: item.PostDrBudgetMasterId,
                    DrBudgetCode: item.DrBudgetCode,
                    DrBudgetName: item.DrBudgetName,
                    PostDrActivityId: item.PostDrActivityId,
                    DrActivityCode: item.DrActivityCode,
                    DrActivityName: item.DrActivityName,
                    FixedAssetMasterId: item.FixedAssetMasterId,
                    UnitOfMeasurement: item.UnitOfMeasurement,
                    AssetMasterName: item.AssetMasterName
                    //activityList: []
                };
                $scope.getActivityListWithCallBack(item.PostDrBudgetMasterId, function (result) {
                    ob.PostDrActivityId = item.PostDrActivityId;
                    $scope.selectFixedAssetDataList.push(ob);
                });
            }
        });

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.setTab2(1);
    }

    $scope.InventoryDone = function (obj) {
        $scope.selectFixedAssetDataList = [];

        var gridObj = $("#issueInventoryAUC").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        angular.forEach($scope.issueInventoryAUCDataList, function (item) {
            if (item.IssueNo === data.IssueNo) {
                var ob = {
                    Id: null,
                    IssueNo: item.IssueNo,
                    InventoryIssueDetailId: item.InventoryIssueDetailId,
                    InventoryIssueHistoryId: item.InventoryIssueHistoryId,
                    InventoryReceiveDetailId: item.InventoryReceiveDetailId,
                    GRNNo: item.GRNNo,
                    GateEntryNo: item.GateEntryNo,
                    GRNDate: item.GRNDate,
                    Type: item.Type,
                    EmployeeId: item.EmployeeId,
                    EmployeeCode: item.EmployeeCode,
                    EmployeeName: item.EmployeeName,
                    IssueDate: item.IssueDate,
                    TransactionUoM: item.TransactionUoM,
                    CurrencyCode: item.CurrencyCode,
                    PartyCode: item.PartyCode,
                    PartyName: item.PartyName,
                    EntryDate: item.EntryDate,
                    IssueType: item.IssueType,
                    IsAsset: item.IsAsset,
                    MaterialGroupMasterName: item.MaterialGroupMasterName,
                    InventoryMaterialId: item.InventoryMaterialId,
                    TransactionQty: item.TransactionQty,
                    BaseQty: item.BaseQty,
                    BaseCurrencyRate: item.BaseCurrencyRate,
                    Amount: item.Amount,
                    BaseUOMId: item.BaseUOMId,
                    TransactionUoMId: item.TransactionUoMId,
                    MaterialMasterId: item.MaterialMasterId,
                    UserName: item.UserName,
                    ArticleId: item.ArticleId,
                    StandardName: item.StandardName,
                    FirstCharacteristicsId: item.FirstCharacteristicsId,
                    FirstCharacteristics: item.FirstCharacteristics,
                    FirstCharacteristics: item.FirstCharacteristics,
                    FirstCharacteristicsValueId: item.FirstCharacteristicsValueId,
                    FirstCharacteristicsValue: item.FirstCharacteristicsValue,
                    SecondCharacteristicsId: item.SecondCharacteristicsId,
                    SecondCharacteristics: item.SecondCharacteristics,
                    SecondCharacteristicsValueId: item.SecondCharacteristicsValueId,
                    SecondCharacteristicsValue: item.SecondCharacteristicsValue,
                    ThirdCharacteristicsId: item.ThirdCharacteristicsId,
                    ThirdCharacteristics: item.ThirdCharacteristics,
                    ThirdCharacteristicsValueId: item.ThirdCharacteristicsValueId,
                    ThirdCharacteristicsValue: item.ThirdCharacteristicsValue,
                    PostCrGLGeneralInfoId: item.PostCrGLGeneralInfoId,
                    CrAccountCode: item.CrAccountCode,
                    CrGLGeneralInfoName: item.CrGLGeneralInfoName,
                    PostCrBudgetMasterId: item.PostCrBudgetMasterId,
                    CrBudgetCode: item.CrBudgetCode,
                    CrBudgetName: item.CrBudgetName,
                    PostCrActivityId: item.PostCrActivityId,
                    CrActivityCode: item.CrActivityCode,
                    CrActivityName: item.CrActivityName,
                    PostDrGLGeneralInfoId: item.PostDrGLGeneralInfoId,
                    DrAccountCode: item.DrAccountCode,
                    DrGLGeneralInfoName: item.DrGLGeneralInfoName,
                    PostDrBudgetMasterId: item.PostDrBudgetMasterId,
                    DrBudgetCode: item.DrBudgetCode,
                    DrBudgetName: item.DrBudgetName,
                    PostDrActivityId: item.PostDrActivityId,
                    DrActivityCode: item.DrActivityCode,
                    DrActivityName: item.DrActivityName,
                    FixedAssetMasterId: item.FixedAssetMasterId,
                    AssetMasterName: item.AssetMasterName,
                    UnitOfMeasurement: item.UnitOfMeasurement
                };
                $scope.IsAssetCapitalize = false;

                $scope.getActivityListWithCallBack(item.PostDrBudgetMasterId, function (result) {
                    ob.PostDrActivityId = item.PostDrActivityId;
                    $scope.selectFixedAssetDataList.push(ob);
                });
            }
        });

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.setTab2(1);
    }

    $scope.capitalizeJournalList = [];
    $scope.issueJournal = function () {
        $scope.capitalizeJournalList = [];
        $scope.invGL = {};
        $scope.invGLDr = {};
        for (var d = 0; d < $scope.selectFixedAssetDataList.length; d++) {
            var getRowDr = $filter("filter")($scope.capitalizeJournalList, { "BudgetMasterId": $scope.selectFixedAssetDataList[d].PostDrBudgetMasterId, "ActivityId": $scope.selectFixedAssetDataList[d].PostDrActivityId });
            if (getRowDr.length == 0 && $scope.selectFixedAssetDataList[d].PostDrBudgetMasterId != null) {
                $scope.invGLDr.GLGeneralInfoId = $scope.selectFixedAssetDataList[d].PostDrGLGeneralInfoId;
                $scope.invGLDr.GLGeneralInfoCode = $scope.selectFixedAssetDataList[d].DrGLGeneralInfoCode;
                $scope.invGLDr.GLGeneralInfoName = $scope.selectFixedAssetDataList[d].DrGLGeneralInfoName;
                $scope.invGLDr.BudgetMasterId = $scope.selectFixedAssetDataList[d].PostDrBudgetMasterId;
                $scope.invGLDr.BudgetName = $scope.selectFixedAssetDataList[d].DrBudgetName;
                $scope.invGLDr.ActivityId = $scope.selectFixedAssetDataList[d].PostDrActivityId;
                $scope.invGLDr.ActivityName = $scope.selectFixedAssetDataList[d].DrActivityName;
                $scope.invGLDr.TrnType = "Dr";
                $scope.invGLDr.Dr = Math.round(parseFloat($scope.selectFixedAssetDataList[d].Amount.toFixed(2)) * 100 + Number.EPSILON) / 100;
                $scope.invGLDr.Amount = Math.round(parseFloat($scope.selectFixedAssetDataList[d].Amount.toFixed(2)) * 100 + Number.EPSILON) / 100;
                $scope.invGLDr.Cr = 0;
                $scope.capitalizeJournalList.push($scope.invGLDr);
                $scope.invGLDr = {};
            }
            else if ($scope.selectFixedAssetDataList[d].PostDrBudgetMasterId != null) {
                for (var k = 0; k < $scope.capitalizeJournalList.length; k++) {
                    if (getRowDr[0].BudgetMasterId == $scope.capitalizeJournalList[k].BudgetMasterId
                        && getRowDr[0].ActivityId == $scope.capitalizeJournalList[k].ActivityId) {
                        var dr = parseFloat($scope.capitalizeJournalList[k].Dr.toFixed(2)) + parseFloat($scope.selectFixedAssetDataList[d].Amount.toFixed(2));
                        $scope.capitalizeJournalList[k].Dr = parseFloat(dr.toFixed(2));
                        $scope.capitalizeJournalList[k].Amount = parseFloat(dr.toFixed(2));
                    }
                }
            }
        }
        for (var i = 0; i < $scope.selectFixedAssetDataList.length; i++) {
            var getRow = $filter("filter")($scope.capitalizeJournalList, { "BudgetMasterId": $scope.selectFixedAssetDataList[i].PostCrBudgetMasterId, "ActivityId": $scope.selectFixedAssetDataList[i].PostCrActivityId });
            if (getRow.length == 0 && $scope.selectFixedAssetDataList[i].PostCrBudgetMasterId != null) {
                $scope.invGL.GLGeneralInfoId = $scope.selectFixedAssetDataList[i].PostCrGLGeneralInfoId;
                $scope.invGL.GLGeneralInfoCode = $scope.selectFixedAssetDataList[i].CrActivityCode;
                $scope.invGL.GLGeneralInfoName = $scope.selectFixedAssetDataList[i].CrGLGeneralInfoName;
                $scope.invGL.BudgetMasterId = $scope.selectFixedAssetDataList[i].PostCrBudgetMasterId;
                $scope.invGL.BudgetName = $scope.selectFixedAssetDataList[i].CrBudgetName;
                $scope.invGL.ActivityId = $scope.selectFixedAssetDataList[i].PostCrActivityId;
                $scope.invGL.ActivityName = $scope.selectFixedAssetDataList[i].CrActivityName;
                $scope.invGL.TrnType = "Cr";
                $scope.invGL.Cr = Math.round(parseFloat($scope.selectFixedAssetDataList[i].Amount.toFixed(2)) * 100 + Number.EPSILON) / 100;
                $scope.invGL.Amount = Math.round(parseFloat($scope.selectFixedAssetDataList[i].Amount.toFixed(2)) * 100 + Number.EPSILON) / 100;
                $scope.invGL.Dr = 0;
                $scope.capitalizeJournalList.push($scope.invGL);
                $scope.invGL = {};

            }
            else if ($scope.selectFixedAssetDataList[i].PostCrBudgetMasterId != null) {
                for (var j = 0; j < $scope.capitalizeJournalList.length; j++) {
                    if (getRow[0].BudgetMasterId == $scope.capitalizeJournalList[j].BudgetMasterId
                        && getRow[0].ActivityId == $scope.capitalizeJournalList[j].ActivityId) {
                        var cr = parseFloat($scope.capitalizeJournalList[j].Cr.toFixed(2)) + parseFloat($scope.selectFixedAssetDataList[i].Amount.toFixed(2));
                        $scope.capitalizeJournalList[j].Cr = parseFloat(cr.toFixed(2));
                        $scope.capitalizeJournalList[j].Amount = parseFloat(cr.toFixed(2));
                        cr = 0;
                    }
                }
            }
        }
    }



    $scope.PostedAUCList = [];
    $scope.PostedAUCData = function () {
        $http({
            method: 'GET',
            url: 'FixedAssets/FixedAssetRegister/GetPostedAUCList'
        }).then(function successCallback(response) {
            $scope.PostedAUCList = response.data;
            for (var i = 0; i < $scope.PostedAUCList.length; i++) {
                response.data[i].PostingDate = new Date($scope.PostedAUCList[i].PostingDate);
            }
        });
    }
    $scope.PostedAUCData();

    $scope.onClickReportDownloadWord = function (args) {
        debugger;
        var gridObj = $("#postedAUC").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('FixedAssets/FixedAssetRegister/GetIssueFixedAssetCapitalizeJournalReport?reportFormat=' + reportFormat + '&voucherId=' + data.Id + '&sourceType=' + data.SourceType , '_blank');
    };

    $scope.commandPDF = [{
        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadWord
        }
    }];

    $scope.onClickReportDownloadExcel = function (args) {
        debugger;
        var gridObj = $("#postedAUC").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('FixedAssets/FixedAssetRegister/GetIssueFixedAssetCapitalizeJournalReport?reportFormat=' + reportFormat + '&voucherId=' + data.Id + '&sourceType='+ data.SourceType, '_blank');

    };
    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadExcel
        }
    }];

    $scope.onClickReportDownloadIssue = function (args) {
        debugger;
        var gridObj = $("#postedAUC").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/AssetIssueReport?grnId=' + data.IssueNo);
    };
    $scope.commandIssue = [{
        type: "details", buttonOptions: {
            text: "Issue",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadIssue
        }
    }];
} 