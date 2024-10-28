'use strict';
SalesReturnPostController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function SalesReturnPostController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Sales Return Post";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.CustomerList = [];
    $scope.PostingStockBeyondIssueDateList = [];
    $scope.PostingStockList = [];
    $scope.UnApprovedStockDetailBeyondIssueDateList = [];
    $scope.ApprovedStockBeyondIssueDateList = [];
    $scope.detailList = [];
    $scope.partyType = "Customer";
    $scope.path1 = 'Products/PurchaseOrder/';
    $scope.path = 'SalesManagements/Sales/';
    $scope.getListUrl = $scope.path + 'GetDataByInventoryIssue';
    $scope.saveUrl = 'SalesManagements/Sales/InsertSalesReturnCreditNote';
    $scope.updateUrl = 'Products/InventorySalesReturn/Update';
    $scope.deleteUrl = $scope.path + 'DeleteSalesDetail/';
    $scope.sreviceSaveUrl = $scope.path + 'SalesServiceChargesCreate/';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';

    $scope.currentDate = new Date(Date.now());
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.tab = 1;

   

    $scope.product = {
        Id: null
        , ComapnyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PlantName: null
        , EntityId: null
        , EntityName: null
        , MaterialStorageId: null
        , SalesDate: $filter("dateFiltering")(Date.now())
        , PostingDate: null
        , Remarks: null
        , EmployeeId: null
        , EmployeeName: null
        , IssueType: 'Revenue'
        , IssueRequestMasterId: null
        , SlipAssetIssueTypeStatus: 'Asset'
        , OrderRefNo: null
        , PartyId: null
        , PartyName: null
        , CheckedBy: null
        , CheckedByStatus: null
        , ApprovedBy: null
        , ApprovedByStatus: null
        , CustomerId: null
        , ChangeInvoicingStateId: null
        , PlantStateId: null
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , InvoicingStateId: null
        , ToCurrencyRate: null
        , DocRefNo: null
        , DocDate: null//$filter("dateFiltering")(Date.now())
        , NoteForAccounts: null
        , CurrencyId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes'
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , IsPaymentTermChangeable: null
        , Summery: null
        , Details: null
        , TaxApplicable:false
    };
    $scope.IssueType = 'Revenue';
    $scope.productNew = Object.assign({}, $scope.product);
    $scope.voucherTypeList = [];
    $scope.getvocherTypeSalesReturn = function () {
        cboService.getCboVoucherTypeSalesReturnList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.productNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
            }
        });
    }
   

    $scope.searchByPosted = "Id"; $scope.searchPosted = "";
    $scope.searchByList = [{ value: 'Id', name: "Sales Return No" }
        , { value: 'SalesId', name: "Sales No" }
        , { value: 'VoucherNo', name: "VoucherNo" }
        , { value: 'PartyName', name: "Party" }
        , { value: 'DocRefNo', name: "DocRef No" }
        , { value: '[Park/Post]', name: "[Park/Post]" }
        , { value: 'PostingDate', name: "Posting Date" }
        ];

    $scope.SalesReturnPostedList = [];
    $scope.getData = function () {
        $http({
            method: "POST",
            url: 'SalesManagements/Sales/GetSalesReturnPostedList',
            data: { column: $scope.searchByPosted, value: $scope.searchPosted},
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.SalesReturnPostedList = response.data;
            var rowdata = $filter("filter")($scope.SalesReturnPostedList, { "Id": $scope.tempSalesReturnId });
            if (!baseService.isUndefinedOrNull(rowdata[0].AdditionalTaxId)) {
                $scope.onClickadditionalTaxPop(rowdata[0]);
            }
            else { $scope.tempSalesReturnId = null;}
        });
    };
    $scope.getData();

  

    $scope.searchBySalesReturn = "Id"; $scope.searchSalesReturn = "";
    $scope.searchBySalesReturnList = [{ value: 'Id', name: "Sales Return No" }
        , { value: 'SalesId', name: "Sales No" }
        , { value: 'SalesReturnDate', name: "Sales Return Date" }
        , { value: 'PartyName', name: "Party" }
        , { value: 'DocRefNo', name: "DocRef No" }
    ];
    $scope.approvedSalesList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'POST',
            url: 'SalesManagements/Sales/GetSalesReturnPopUpData',
            data: { column: $scope.searchBySalesReturn, value: $scope.searchSalesReturn },
        }).then(function successCallback(response) {
            $scope.approvedSalesList = response.data;
            for (var i = 0; i < $scope.approvedSalesList.length; i++) {
                response.data[i].SalesDate = new Date($scope.approvedSalesList[i].SalesDate);
            }
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#SalesReturnPopUp')).modal('show');
    };


    $scope.selectDoubleClick = function (data) {
        $scope.product = data.data;
        $scope.product.SalesReturnId = data.data.Id;
        $scope.product.SalesId = data.data.SalesId;
        $scope.product.PartyId = data.data.CustomerId;
        $scope.product.Id = null;
        $scope.product.SalesReturnDate = data.data.SalesReturnDate;
        $scope.product.ToCurrencyRate = data.data.ToCurrencyRate;
        $scope.product.CompanyCurrencyRate = data.data.ToCurrencyRate;
        $scope.product.CurrencyId = data.data.CurrencyId;
        $scope.product.TaxApplicable = data.data.TaxApplicable;
        $scope.product.PostingDate = $filter("dateFiltering")(data.data.SalesReturnDate);
        $scope.product.InvoicingPartyPlantId = data.data.InvoicingPartyPlantId;
        $scope.product.IsCreditNote = false;
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.getvocherTypeSalesReturn();
        getSalesReturnDetailList();
        getSalesReturnJV($scope.product.SalesReturnId, data.data.CustomerId, $scope.product.TaxApplicable, $scope.product.IsCreditNote);
        getSalesReturnDetailGLData($scope.product.SalesReturnId);
        $scope.Action = 'Save';
        $scope.closeSalesReturnPopUp();
    };
    $scope.getSalesReturnIsCreditNoteJV = function () {
        getSalesReturnJV($scope.productNew.SalesReturnId, $scope.productNew.PartyId, $scope.productNew.TaxApplicable, $scope.productNew.IsCreditNote);
    };


    $scope.closeSalesReturnPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#SalesReturnPopUp')).modal('hide');
    };

   

    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
        //factoryService.getCurrencyPrecision($scope.baseCurrencyId);
    });
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.GridInventorySalesdata = [];
    $scope.getdataInventorySales = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetDataByInventorySales?tabType=' + $scope.tabType,
        }).then(function successCallback(response) {
            $scope.GridInventorySalesdata = response.data;
            //entrydata = copy(searchdata);
        });

    };
    function getSalesReturnDetailList() {
        $scope.returnDetailurl = 'SalesManagements/Sales/GetSalesReturnDetailDataBySalesReturn?salesReturnId=' + $scope.productNew.SalesReturnId
        $http.get($scope.returnDetailurl)
            .then(function (response) {
                $scope.detailList = response.data;
            });
    }

    $scope.salesReceiveDetailList = [];
    $scope.salesReturnJVList = [];
    $scope.newList = [];
    function getSalesReturnJV(salesReturnId, customerId, taxApplicable, isCreditNote) {
        $http.get('SalesManagements/Sales/GetSalesReturnJournal?salesReturnId=' + salesReturnId + '&customerId=' + customerId + '&taxApplicable=' + taxApplicable + '&isCreditNote=' + isCreditNote)
            .then(function (response) {
                $scope.salesReceiveDetailList = [];
                $scope.salesReturnJVList = [];
                $scope.newList = [];
                $scope.salesReturnJVList = response.data;
                reArrangeReturnJournalList($scope.salesReturnJVList, $scope.newList, $scope.salesReceiveDetailList);
            });
    }
    $scope.salesReturnDetailGLList = [];
    function getSalesReturnDetailGLData(salesReturnId, customerId) {
        $http.get('SalesManagements/Sales/GetSalesReturnDetailGLUpdateData?salesReturnId=' + salesReturnId )
            .then(function (response) {
                $scope.salesReturnDetailGLList = [];
                $scope.salesReturnDetailGLList = response.data;
            });
    }

    function reArrangeReturnJournalList(list, newList, newInvRecDetailList) {
        
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var row = list[i];
            if (row.OtherName === 'Tax' && row.TrnType === 'Dr' && row.Dr > 0) {
                var flag = false;
                for (var t = 0; t < baseService.arrayLength(newList); t++) {
                    if (row.OtherName === newList[t].OtherName && row.TrnType === newList[t].TrnType && row.GLGeneralInfoId === newList[t].GLGeneralInfoId && row.BudgetMasterId === newList[t].BudgetMasterId
                        && row.ActivityId === newList[t].ActivityId) {
                        newList[t].Dr += row.Dr;
                        newList[t].Amount += row.Dr;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    newList.push(list[i]);
            }
            if (row.OtherName === 'TCS' && row.TrnType === 'Dr' && row.Dr > 0) {
                var flag = false;
                for (var t = 0; t < baseService.arrayLength(newList); t++) {
                    if (row.OtherName === newList[t].OtherName && row.TrnType === newList[t].TrnType && row.GLGeneralInfoId === newList[t].GLGeneralInfoId && row.BudgetMasterId === newList[t].BudgetMasterId
                        && row.ActivityId === newList[t].ActivityId) {
                        newList[t].Dr += row.Dr;
                        newList[t].Amount += row.Dr;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'Tax' && row.TrnType === 'Cr' && row.Cr > 0) {
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId
                        && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Cr += row.Cr;
                        newList[a].Amount += row.Cr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }

            else if (row.OtherName === 'Material' && row.TrnType === 'Dr') {
                newInvRecDetailList.push(list[i]);
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId && row.ActivityId === newList[a].ActivityId) {
                        var dr = parseFloat(newList[a].Dr.toFixed(4)) + parseFloat(row.Dr.toFixed(4));
                        newList[a].Dr = parseFloat(dr.toFixed(4));
                        newList[a].Amount = parseFloat(dr.toFixed(4));
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
            else if (row.OtherName === 'Return' && row.TrnType === 'Cr' && row.Cr > 0) {
                newInvRecDetailList.push(list[i]);
                var has = false;
                for (var a = 0; a < baseService.arrayLength(newList); a++) {
                    if (row.OtherName === newList[a].OtherName && row.TrnType === newList[a].TrnType && row.GLGeneralInfoId === newList[a].GLGeneralInfoId && row.BudgetMasterId === newList[a].BudgetMasterId && row.ActivityId === newList[a].ActivityId) {
                        newList[a].Cr += row.Cr;
                        newList[a].Amount += row.Cr;
                        has = true;
                        break;
                    }
                }
                if (!has)
                    newList.push(list[i]);
            }
        }
    }
    function getInvTaxList() {

        $scope.returnTaxurl = 'SalesManagements/Sales/GetSalesReturnTaxDetail?salesReturnId=' + $scope.productNew.SalesReturnId

        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.returnTaxurl
        }).then(function successCallback(response) {
            $scope.materialtaxCategoryListSavedData = response.data;
        });
    }


    $scope.tab = 1;
    $scope.tabType = 1;

    $scope.getdataInventorySales($scope.tabType);
    $scope.setTabFirst = function (newTab) {

        $scope.tab = newTab;
        $scope.tabType = '1';
        $scope.getdataInventorySales($scope.tabType);

    };
    $scope.isSetFirst = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTabSecond = function (newTab) {
        //debugger;
        $scope.tabType = '2';
        $scope.tab = newTab;

        $scope.getdataInventorySales($scope.tabType);

    };
    $scope.isSetSecond = function (tabNum) {
        return $scope.tab === tabNum;
    };

  

    
    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/InventoryIssue/inventorySalesReportPrint?grnId=" + data.Id;

    };
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });

    
    $scope.invoicingPartyPopUp = function () {
        //debugger;
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
   

    //#endregion
   
    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.EntityList = result;
    });
   


    $scope.tempSalesReturnId = null;
    $scope.Save = function () {
        $scope.tempSalesReturnId = null;
        if ($scope.detailList.length === 0) {
            ShowResult('Please select Atlest one material');
            return false;
        }

        if (baseService.arrayLength($scope.detailList) > 0) {
            for (var i = 0; i < $scope.detailList.length; i++) {
                $scope.detailList[i].TransactionQty = $scope.detailList[i].ReturnQty;
            }
        }
        
        if ($scope.Action === "Save") {
            $scope.tempSalesReturnId=$scope.productNew.SalesReturnId;
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    'voucherVM': $scope.productNew
                    , 'voucherDetailVMList': $scope.newList
                    , 'salesReturnDetailList': $scope.salesReturnDetailGLList
                    , 'tdsTaxList': $scope.TDSList
                    , 'isCreditNote': $scope.productNew.IsCreditNote
                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        
    };


    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    $scope.manualValidationAddRemove = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.productNew = { FixedAssetOrInventory: 'Inventory', PODepended: false, AlongwithInvoice: false, IssueType: 'Revenue', InvoicingPartyPlantId: $scope.productNew.InvoicingPartyPlantId };
        $scope.detailModel = {};
        $scope.TDS = {};
        $scope.clearCharNames();
        $scope.detailList = [];
        $scope.specificStockList = [];
        $scope.TDSList = [];
        $scope.IssueType = 'Revenue';
    }

   
    $scope.closeDetaiPopUp = function () {
        //debugger;
        $scope.CostCenterIdTemp = $scope.detailModel.CostCenterId;
        $scope.detailModel = {};
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };

    $scope.onClickReportDownloadWord = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'GetSalesReturnCreditNoteReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId + '&sourceType=' + data.SourceType);

    };

    $scope.onClickReportDownloadExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'GetSalesReturnCreditNoteReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId + '&sourceType=' + data.SourceType);
    };

    $scope.LocalTaxInvoiceReport = function (data) {
        location.href = "Sales/SalesReturnReport?salesReturnId=" + data.Id;
    };


    $scope.TDSCboList = [];
    $scope.TDSlistMessage = "";
    $scope.getTDS = function (date) {
        $http({
            method: "get",
            url: "accounts/TaxCode/GetTDSCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.TDSlistMessage = response.data.Message;
                }
                else {
                    $scope.TDSCboList = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };

    $scope.getTDS($filter("dateFiltering")(Date.now()));
    $scope.TDS = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null
    };
    $scope.selectTDS = function () {
        $scope.TDS.ValueOfFixed = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.TDS.Type = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].Type;
        $scope.TDS.TaxCategoryId = $.grep($scope.TDSCboList, function (item) {
            return item.Id === $scope.TDS.TaxCodeId;
        })[0].TaxCategoryId;
        if ($scope.TDS.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.TDS.ValueOfFixed)) {
            $scope.TDS.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "ReturnAmount") * $scope.TDS.ValueOfFixed / 100).toFixed(4);
        }
    }
    $scope.TDSList = [];
    $scope.addTDS = function () {
        if (manualValidation("td_TDS_TaxCode", baseService.isUndefinedOrNull($scope.TDS.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TDS_TaxCodeAmount", baseService.isUndefinedOrNull($scope.TDS.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TDS_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.TDS.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.TDS.TaxName = $.grep($scope.TDSCboList, function (item) {
                return item.Id === $scope.TDS.TaxCodeId;
            })[0].UserName;

            $scope.TDSList.push($scope.TDS);
            $scope.TDS = {};
        }
        $scope.calBaseAmount();
    };
    $scope.removeTDSRow = function (index) {
        $scope.TDSList.splice(index, 1);
    };



    $scope.copyTaxesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.advanceTax.CompanyCurrencyAmount = $scope.advanceTax.TaxAmount;
        }
        else {
            $scope.advanceTax.CompanyCurrencyAmount = ($scope.advanceTax.TaxAmount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    $scope.removeTaxesRow = function (index) {
        $scope.advanceTaxesList.splice(index, 1);
    };

    $scope.voucherTypeListnew = [];
    $scope.additionalTaxVoucherTypeId = null;
    $scope.getPaymentVoucherType = function () {
        cboService.getCboVoucherTypePaymentList(function (result) {
            $scope.voucherTypeListnew = result;
            if (baseService.arrayLength($scope.voucherTypeListnew) === 1)
                $scope.additionalTaxVoucherTypeId = $scope.voucherTypeListnew[0].Value;
        });
    }

    $scope.additionalTaxPostUrl = 'Accounts/InvoicePost/InsertCreditNoteAdditionalTaxPost';
    $scope.additionalTaxDetailList = [];
    $scope.onClickadditionalTaxPop = function (x) {
        $scope.additionalTaxData = {};
        var data = x;
        data.VoucherTypeId = null;
        data.VoucherTypeId = $scope.additionalTaxVoucherTypeId;
        data.VoucherDate = new Date();
        $scope.additionalTaxData = data;
        $http({
            method: 'POST',
            url: 'SalesManagements/Sales/GetCreditNoteAdditionalTaxDetail?additionalTaxId=' + data.AdditionalTaxId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.additionalTaxDetailList = response.data;
        });
        $scope.getPaymentVoucherType();
        angular.element(document.querySelector('#additionalTaxPopUp')).modal('show');
    };

    $scope.postAdditionalTax = function () {
        if ($scope.additionalTaxVoucherTypeId == null)
            ShowResult('Please select VoucherType', 'failure', 'additionalTaxPopUp');

        $scope.additionalTaxData.VoucherTypeId = $scope.additionalTaxVoucherTypeId;
        if ($scope.additionalTaxData != null && $scope.additionalTaxVoucherTypeId != null) {
            $http({
                method: 'POST',
                url: $scope.additionalTaxPostUrl,
                data: {
                    "additionalTaxId": $scope.additionalTaxData.AdditionalTaxId
                    , "voucherVM": $scope.additionalTaxData
                },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDataList();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
            angular.element(document.querySelector('#additionalTaxPopUp')).modal('hide');
        }

    }
    $scope.closeAdditionalTax = function () {
        $scope.additionalTaxData = {};
        angular.element(document.querySelector('#additionalTaxPopUp')).modal('hide');

    }
    $scope.additionalTaxPrint = function () {
        try {
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + $scope.additionalTaxData.TDSTaxVoucherId
            $rootScope.report(file_src);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}