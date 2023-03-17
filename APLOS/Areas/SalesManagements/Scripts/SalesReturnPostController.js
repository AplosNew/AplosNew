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
    $scope.path = 'Products/InventoryIssue/';
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
    };
    $scope.IssueType = 'Revenue';
    $scope.productNew = Object.assign({}, $scope.product);
    cboService.getCboVoucherTypeSalesReturnList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
            $scope.voucher.DocDate = $scope.voucher.PostingDate;
        }
    });

    $scope.SalesdataList = [];
    $scope.getData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/InventorySalesReturn/GetList',
        }).then(function successCallback(response) {
            $scope.SalesdataList = response.data;
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
        $scope.product.Id = null;
        $scope.product.SalesReturnDate = data.data.SalesReturnDate;
        $scope.product.PostingDate = $filter("dateFiltering")(data.data.SalesReturnDate);
        $scope.product.InvoicingPartyPlantId = data.data.InvoicingPartyPlantId;
        $scope.productNew = Object.assign({}, $scope.product);
        getSalesReturnDetailList();
        getSalesReturnJV($scope.product.SalesReturnId, data.data.CustomerId);
        $scope.Action = 'Save';
        $scope.closeSalesReturnPopUp();
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
    function getSalesReturnJV(salesReturnId, customerId) {
        $http.get('SalesManagements/Sales/GetSalesReturnJournal?salesReturnId=' + salesReturnId + '&customerId=' + customerId)
            .then(function (response) {
                $scope.salesReceiveDetailList = [];
                $scope.salesReturnJVList = [];
                $scope.newList = [];
                $scope.salesReturnJVList = response.data;
                reArrangeReturnJournalList($scope.salesReturnJVList, $scope.newList, $scope.salesReceiveDetailList);
            });
    }
    function reArrangeReturnJournalList(list, newList, newInvRecDetailList) {
        //var svcList = ($filter('filter')(list, { OtherName: 'Svc' }, true));
        //for (var t = 0; t < baseService.arrayLength(svcList); t++) {
        //    var row = svcList[t];
        //    if (row.OtherName === 'Svc' && row.TrnType === 'Dr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Svc', TrnType: 'Dr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
        //        row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, list, 'Dr');
        //    }
        //    else if (row.OtherName === 'Svc' && row.TrnType === 'Cr') {
        //        var taxList = ($filter('filter')(list, { OtherName: 'Svc', TrnType: 'Cr', GLGeneralInfoId: row.GLGeneralInfoId, BudgetMasterId: row.BudgetMasterId, ActivityId: row.ActivityId }, true));
        //        row.Amount = parseFloat(row.Amount) / parseFloat(baseService.arrayLength(taxList));
        //        assignSvcInTax(row, list, 'Cr');
        //    }
        //}
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
                        newList[a].Dr += row.Dr;
                        newList[a].Amount += row.Dr;
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
            else if (row.OtherName !== 'Svc' && row.OtherName === 'Vendor' && $scope.AcceptanceId === null && $scope.PurchaseLCId == null) {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
            }
            else if (row.OtherName !== 'Svc' && row.OtherName === 'LCBase' && $scope.PurchaseLCId != null) {
                newList.push(list[i]);
                $scope.TotalPayableAmount += list[i].Amount;
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
    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });
    $scope.getToCurrencyRate = function () {
        //debugger;
        if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
            $scope.productNew.ToCurrencyRate = 1;
            return;
        }
        $http.get($scope.path1 + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
            .then(function (response) {
                if (parseFloat(response.data) === 0)
                    $scope.productNew.ToCurrencyRate = 1;
                else
                    $scope.productNew.ToCurrencyRate = response.data;
            });
    };
    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.EntityList = result;
    });
   
   
    function getTaxCategoryList(hsnCodeId) {
       
        $scope.materialtaxCategoryList = [];
        $http({
            method: 'GET',
            url: $scope.path1 + 'GetTaxCategoryListForSalesMaterial?partyPlantId=' + $scope.productNew.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&InventorySalesDate=' + $scope.productNew.SalesDate
        }).then(function (response) {

            $scope.materialtaxCategoryList = response.data;
            // $scope.sevtaxCategoryList = response.data;
        });
    }

    $scope.lst = [];
    $scope.SalesDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/InventoryIssue/MaterialSalesDetails'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    $scope.SalesDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM", "SalesRate", "CurrencyName", "TotalAmount", "Comments"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    //#endregion

    $scope.Save = function () {
        //debugger;
        // $scope.SavePOPUpConfirm();
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
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    'voucherVM': $scope.productNew
                    , 'voucherDetailVMList': $scope.newList
                    , 'invoiceTaxVMList': null
                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.Action = 'Update';
                    $scope.productNew.Id = response.data.inventoryIssue.Id;
                    $scope.getdataInventorySales();
                    $scope.SalesDetails();
                    $scope.getData();
                    $scope.Clear();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else if ($scope.Action === "Update") {
            $http({
                method: 'POST'
                , url: $scope.updateUrl
                , data: {
                    inventoryIssue: $scope.productNew
                    , entities: $scope.detailList
                    , 'salesReturnTaxList': $scope.materialtaxCategoryListSavedData
                    , 'salesServiceVMList': $scope.chargesList
                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.Action = 'Update';
                    $scope.productNew.Id = response.data.inventoryIssue.Id;
                    $scope.getdataInventorySales();
                    $scope.SalesDetails();
                    $scope.getData();
                    $scope.Clear();
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
        //$scope.productNew.InvoicingPartyPlantId=$scope.productNew.InvoicingPartyPlantId;
        $scope.detailModel = {};
        $scope.clearCharNames();
        $scope.detailList = [];
        $scope.specificStockList = [];
        $scope.IssueType = 'Revenue';
    }

   
    $scope.closeDetaiPopUp = function () {
        //debugger;
        $scope.CostCenterIdTemp = $scope.detailModel.CostCenterId;
        $scope.detailModel = {};
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };


}