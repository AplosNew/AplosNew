'use strict';
SalesReturnController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function SalesReturnController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Sales Return";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.CustomerList = [];
    $scope.PostingStockBeyondIssueDateList = [];
    $scope.PostingStockList = [];
    $scope.UnApprovedStockDetailBeyondIssueDateList = [];
    $scope.ApprovedStockBeyondIssueDateList = [];
    $scope.UnApprovedStockList = [];
    $scope.ApprovedStockList = [];
    $scope.IsSaveButtonDisable = false;
    $scope.partyType = "Customer";
    $scope.path1 = 'Products/PurchaseOrder/';
    $scope.path = 'Products/InventoryIssue/';
    $scope.getListUrl = $scope.path + 'GetDataByInventoryIssue';
    $scope.saveUrl = 'SalesManagements/Sales/SaveSalesReturn';
    $scope.updateUrl = 'Products/InventorySalesReturn/Update';
    $scope.deleteUrl = $scope.path + 'DeleteSalesDetail/';
    $scope.sreviceSaveUrl = $scope.path + 'SalesServiceChargesCreate/';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';

    $scope.currentDate = new Date(Date.now());
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.tab = 1;

    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'Id', name: "Sales No" }, { value: 'SalesDate', name: "Sales Date" }
        , { value: 'Tracenent', name: "Tracenent" }
        , { value: 'PartyName', name: "Party" }
        , { value: 'GateEntryNo', name: "Gate EntryNo" }, { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];

    $scope.SalesdataList = [];
    $scope.getData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'SalesManagements/Sales/GetSalesReturnList',
        }).then(function successCallback(response) {
            $scope.SalesdataList = response.data;
        });
    };
    $scope.getData();

    $scope.searchBySales = "Id"; $scope.searchSales = "";
    $scope.searchBySalesList = [{ value: 'Id', name: "Sales No" }, { value: 'SalesDate', name: "Sales Date" }
        , { value: 'PartyName', name: "Party" }
        , { value: 'DocRefNo', name: "DocRef No" }
        , { value: 'DocDate', name: "Doc Date" }];
    $scope.approvedSalesList = [];
    $scope.getPopUpData = function () {
        if ($scope.ReturnType == 'PackingSales') {
            $scope.SalesPopupUrl = 'SalesManagements/Sales/GetPackingSalesListForReturn'
        }
        else {
            $scope.SalesPopupUrl = 'SalesManagements/Sales/GetSalesListForReturn'
        }
        $http({
            method: 'POST',
            url: $scope.SalesPopupUrl,
            data: { column: $scope.searchBySales, value: $scope.searchSales },
        }).then(function successCallback(response) {
            $scope.approvedSalesList = response.data;
            for (var i = 0; i < $scope.approvedSalesList.length; i++) {
                response.data[i].SalesDate = new Date($scope.approvedSalesList[i].SalesDate);
            }
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#SalespopUp')).modal('show');
    };



    $scope.selectDoubleClick = function (data) {
        $scope.product = data.data;
        $scope.product.SalesId = data.data.Id;
        $scope.product.SalesId = data.data.Id;
        $scope.product.Id = null;
        $scope.product.SalesDate = data.data.SalesDateNew;
        $scope.product.InvoicingPartyPlantId = data.data.InvoicingPartyPlantId;
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.materialStockList = [];
        $scope.specificStockList = [];
        if ($scope.ReturnType == 'PackingSales') {
            getItemScanChildByPackingId($scope.product.SalesId, data.data.PackingId);
            $scope.getLocationCbo();
        }
        else {
            getIssueDetailList($scope.product.SalesId, data.data.PackingId,null);

        }

        $scope.productNew.TaxOption = 'Yes';
        $scope.productNew.TaxOptionMat = 'Yes';
        $scope.productNew.TaxOptionService = 'Yes';
        $scope.productNew.TaxOptionServiceModify = 'Yes';
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        $scope.Action = 'Save';
        $scope.IsSaveButtonDisable = false;
      
        $scope.closeSalesPopUp();
    };


    $scope.closeSalesPopUp = function () {
        $scope.valueData = '';

        angular.element(document.querySelector('#SalespopUp')).modal('hide');
    };

    $scope.GridInventorySalesdata = [];
    $scope.getdataInventorySales = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/InventoryIssue/GetDataByInventorySales?tabType=' + $scope.tabType,
        }).then(function successCallback(response) {
            $scope.GridInventorySalesdata = response.data;
        });

    };
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab = 1;
    $scope.tabType = 1;

    $scope.getdataInventorySales($scope.tabType);
    $scope.setTabFirst = function (newTab) {

        $scope.tab = newTab;
        $scope.tabType = '1';
        /* $scope.getdataInventorySales($scope.tabType);*/

    };
    $scope.isSetFirst = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTabSecond = function (newTab) {
        $scope.tabType = '2';
        $scope.tab = newTab;

        /*$scope.getdataInventorySales($scope.tabType);*/

    };
    $scope.isSetSecond = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabThird = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '3';
        /*  $scope.getdataInventorySales($scope.tabType);*/
    };
    $scope.isSetThird = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabFourth = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '4';
        $scope.getdataInventorySales($scope.tabType);

    };
    $scope.isSetFourth = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabFifth = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '5';

        $scope.getdataInventorySales($scope.tabType);

    };
    $scope.isSetFifth = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabSixth = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '6';
        $scope.getdataInventorySales($scope.tabType);

    };
    $scope.isSetSixth = function (tabNum) {
        return $scope.tab === tabNum;
    };




    //#endregion

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
        , SalesReturnDate: null
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
        , LocMasterId: null
    };
    $scope.IssueType = 'Revenue';
    $scope.productNew = Object.assign({}, $scope.product);
    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/InventoryIssue/inventorySalesReportPrint?grnId=" + data.Id;

    };


    //#region report
    $scope.InventorySalesReportExcels = function (id, reportFormat) {

        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }

        }
        else {

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }


        }

        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Summery=' + $scope.productNew.Summery, '_blank');
    };

    $scope.InventorySalesRepoReportPdf = function (id, reportFormat) {

        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
        }
        else {

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
        }


        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Summery=' + $scope.productNew.Summery, '_blank');

    };

    //#endregion

    //#region Material Tax
    $scope.materialtaxCategoryListResFinal = [];
    $scope.getTaxList = function (data, index) {
        $scope.total = 0;
        $scope.LoadTaxButtonClick();
        $scope.filterSalesMaterialId = data.SalesMaterialId;
        $scope.taxAbleAmnt = $scope.detailList[index].TotalAmount;
        if ($scope.ReturnType == 'PackingSales') {
            for (var i = 0; i < $scope.taxlist.length; i++) {
                $scope.taxlist[i].Amount = ($scope.detailList[index].Amount * $scope.taxlist[i].Percentage) / 100
            }
        }
        $scope.indexRow = index;
        $scope.index = index;
        $scope.HSNCode = $scope.taxlist[0].HSNCode;
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };

    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });

    }
    $scope.closeReceiveTaxPopUpwindow = function () {
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
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
    $scope.locationCbo = [];
    $scope.getLocationCbo = function () {
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetSalesReturnLocationCbo"
        }).then(function successCallback(response) {
            $scope.locationCbo = response.data;
        });
    };
    $scope.getLocationCbo();
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

    function checkLCExist(list, InvoiceId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceId === InvoiceId) {

                return true;
            }
        }
        return false;
    }
    //#endregion
    $scope.newList = [];
    $scope.newtaxList = [];
    $scope.tempitemScan = {
        Id: null, WorkDate: $filter("dateFiltering")(Date.now()), Time: $filter("dateFiltering")(Date.now()), ShiftId: null, LocMasterId: null, PurposeId: null, Grade: null
    };
    $scope.Validation = function () {
        if ($scope.detailList.length === 0) {
            ShowResult('Please select Atlest one material');
            $scope.IsSaveButtonDisable = false;
            return true;
        }
        $scope.newList = [];
        $scope.newtaxList = [];
        for (var i = 0; i < $scope.detailList.length; i++) {
            if ($scope.detailList[i].ReturnQty > 0) {
                $scope.newList.push($scope.detailList[i])
                angular.forEach($scope.taxlist, function (a) {
                    if (a.SalesMaterialId == $scope.detailList[i].SalesMaterialId) {
                        a.Amount = (($scope.detailList[i].Amount * a.Percentage) / 100).toFixed(2)
                        $scope.newtaxList.push(a);
                    }
                });
            }
        }
        if ($scope.productNew.SourceType == 'Packing') {
            for (var i = 0; i < $scope.newList.length; i++) {
                if ($scope.newList[i].VerifiedQty > 0 && $scope.newList[i].VerifiedQty != $scope.newList[i].ReturnQty) {
                    ShowResult('Return Qty and VerifiedQty Qty is not equal !!!');
                    $scope.IsSaveButtonDisable = false;
                    return true;
                }
            }
        }
        if ($scope.productNew.SourceType == 'Packing') {
            $scope.tempitemScan.LocMasterId = $scope.productNew.LocMasterId;
            $scope.tempitemScan.WorkDate = $scope.productNew.SalesReturnDate;
            $scope.tempitemScan.Time = $scope.productNew.SalesReturnDate;
            $scope.tempitemScan.PurposeId = $scope.locationCbo[0].PurposeId
            $scope.tempitemScan.ShiftId = $scope.tempitemScanList[0].ShiftId
            $scope.tempitemScan.Grade = $scope.tempitemScanList[0].Grade

            $scope.itemScanNewListForSales = [];
            $scope.itemScanNewListForSales = $scope.tempitemScanList.slice();
        }
        return false;
    }
    $scope.itemScanNewListForSales = [];
    $scope.Save = function () {
        $scope.IsSaveButtonDisable = true;
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.productNewForm.$valid && !$scope.Validation()) {
            if ($scope.Action === "Save" && $scope.productNew.Id == null) {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: {
                        'data': $scope.productNew
                        , 'detaildataList': $scope.newList
                        , 'taxList': $scope.newtaxList
                        , 'itemScanCildList': $scope.tempitemScanList
                        , 'ItemScandata': $scope.tempitemScan
                        , 'itemScanCildNewList': $scope.itemScanNewListForSales
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.detailList = [];
                        $scope.newList = [];
                        $scope.tempitemScanList = [];
                        $scope.taxlist = [];
                        $scope.productNew.Id = response.data.Id;
                        $scope.IsSaveButtonDisable = false;
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
        $scope.IsSaveButtonDisable = false;
    };



    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.IsSaveButtonDisable = false;
    }


    function getIssueDetailList(salesId, packingId,smIds) {
        if ($scope.productNew.SourceType == 'Packing') {
            $scope.returnDetailurl = 'SalesManagements/Sales/GetPackingSalesDetailDataBySales?salesId=' + salesId + '&packingid=' + packingId + '&smIds=' + smIds
        } else {
            $scope.returnDetailurl = 'SalesManagements/Sales/GetSalesDetailDataBySales?salesId=' + salesId
        }
        $http.get($scope.returnDetailurl)
            .then(function (response) {
                $scope.detailList = response.data;
                getInvTaxList();
                getSalesServiceList();
            });
    }

    $scope.chargesList = [];
    function getSalesServiceList() {
        $scope.SalesServiceurl = 'SalesManagements/Sales/GetSalesServiceChargeList?salesId=' + $scope.productNew.SalesId
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.SalesServiceurl
        }).then(function successCallback(response) {
            $scope.chargesList = response.data;
            getServiceTaxList();
        });
    }

    $scope.tempidList = [];
    $scope.sqlInStatement = null;
    function getItemScanChildByPackingId(salesId, packingId) {
        $scope.tempitemScanList = [];
        
        if ($scope.productNew.SourceType == 'Packing') {
            $scope.returnDetailurl = 'SalesManagements/Sales/GetItemScanChildDataByPackingId?salesId=' + salesId + '&packingid=' + packingId
        }
        $http.get($scope.returnDetailurl)
            .then(function (response) {
                $scope.tempitemScanList = response.data;

                if ($scope.tempitemScanList.length > 0) {
                    $scope.tempidList = [];
                    //for (var di = 0; di < $scope.tempitemScanList.length; di++) {
                    //    $scope.tempidList.push($scope.tempitemScanList[di]);
                    //}
                   // if ($scope.tempitemScanList.length > 0) {
                        var uniqueSalesMaterialId = removeDuplicates($scope.tempitemScanList, 'SalesMaterialId');
                        var wcSMId = "";
                        if (uniqueSalesMaterialId.length > 0) {
                            wcSMId = "IN(";
                            wcSMId += Array.prototype.map.call(uniqueSalesMaterialId, function (item) { return "'" + item.SalesMaterialId + "'"; }).join(",") + ")";
                        }
                        $scope.sqlInStatement = wcSMId;
                   // }
                }

                getIssueDetailList($scope.product.SalesId, packingId, $scope.sqlInStatement);
            });
    }
    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
    function getInvTaxList() {
        if ($scope.productNew.Id == null) {
            $scope.returnTaxurl = 'SalesManagements/Sales/GetMaterialSalesTaxDetail?salesId=' + $scope.productNew.SalesId
        } else {
            $scope.returnTaxurl = 'Products/InventorySalesReturn/GetTaxForUpdateSalesReturn?salesReturnId=' + $scope.productNew.Id + '&SalesId=' + $scope.productNew.SalesId
        }
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.returnTaxurl
        }).then(function successCallback(response) {
            $scope.taxlist = response.data;
            //taxProcess();
        });
    }

    function getServiceTaxList() {
        $scope.returnTaxurl = 'SalesManagements/Sales/GetSalesServiceTaxList?salesId=' + $scope.productNew.SalesId
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.returnTaxurl
        }).then(function successCallback(response) {
            $scope.Servicetaxlist = response.data;
        });
    }

    function taxProcess() {
        for (var i = 0; i < $scope.taxlist.length; i++) {
            $scope.taxlist[i].Amount = valueCheckInList($scope.detailList, $scope.taxlist[i].SalesMaterialId, $scope.taxlist[i].Percentage);
        }
    }

    function valueCheckInList(list, fildName, value) {
        for (var j = 0; j < list.length; j++) {
            if (list[j].SalesMaterialId === fildName) {
                (list[j].Amount * value) / 100
                return true;
            }
        }
        return false;
    }

    $scope.itemScanChildList = [];
    $scope.tempData = {};
    $scope.getItemScanChildPopUp = function (data) {
        $scope.tempData = {};
        $scope.tempData = data;
        $scope.tempData.VerifiedQty = 0;
        $scope.ItemScanChildurl = 'SalesManagements/Sales/GetItemScanChildData?salesId=' + $scope.tempData.SalesId + '&packingId=' + $scope.tempData.PackingId + '&soId=' + $scope.tempData.SalesOrderId
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.ItemScanChildurl
        }).then(function successCallback(response) {
            $scope.itemScanChildList = response.data;
        });
        angular.element(document.querySelector('#ISCpopUp')).modal('show');
    }
    $scope.tempitemScanList = [];
    $scope.closeItemScanChildPopUp = function () {
        for (var i = 0; i < $scope.itemScanChildList.length; i++) {
            if ($scope.itemScanChildList[i].Active) {
                $scope.tempitemScanList.push($scope.itemScanChildList[i])
                $scope.tempData.VerifiedQty = parseFloat($scope.tempData.VerifiedQty.toFixed(4))
                $scope.tempData.VerifiedQty = parseFloat(($scope.tempData.VerifiedQty + Math.round(($scope.itemScanChildList[i].ReturnNetWeight) * 100 + Number.EPSILON) / 100).toFixed(4))
            }
        }
        angular.element(document.querySelector('#ISCpopUp')).modal('hide');
    }

    $scope.returnAmountCalculation = function (data) {
        if (data.TransactionRate > data.TempTransactionRate) {
            data.TransactionRate = data.TempTransactionRate;
            ShowResult('Rate can not greater than ' + data.TempTransactionRate, "failure");
        }
        if (data.TransactionUoMId == data.BaseUOMId) {
            data.BaseRate = data.TransactionRate;
        }
        data.Amount = Math.round((data.ReturnQty * data.TransactionRate) * 100 + Number.EPSILON) / 100
        data.CurrentBalanceQty = data.BalanceQty - data.ReturnQty
        data.TaxAmount = 0;
        var TotalQty = $filter('sumByKey')($filter('filter')($scope.detailList), 'TransactionQty');
        var ratio = data.ReturnQty / TotalQty;
        for (var j = 0; j < $scope.taxlist.length; j++) {
            if ($scope.taxlist[j].SalesMaterialId == data.SalesMaterialId) {
                $scope.taxlist[j].Amount = Math.round((data.Amount * ($scope.taxlist[j].Percentage / 100)) * 100 + Number.EPSILON) / 100
                data.TaxAmount += Math.round((data.Amount * ($scope.taxlist[j].Percentage / 100)) * 100 + Number.EPSILON) / 100
            }
           

            if ($scope.chargesList.length > 0) {
                //var TotalServiceCharge = $filter('sumByKey')($filter('filter')($scope.chargesList), 'BalanceAmount');

                for (var k = 0; k < $scope.chargesList.length; k++) {
                    $scope.chargesList[k].ReturnAmount = Math.round(($scope.chargesList[k].BalanceAmount * ratio) * 100 + Number.EPSILON) / 100
                    $scope.chargesList[k].TotalTaxAmount = Math.round(($scope.chargesList[k].SalesServiceTaxAmount * ratio) * 100 + Number.EPSILON) / 100
                }
                if ($scope.Servicetaxlist.length > 0) {
                    for (var l = 0; l < $scope.Servicetaxlist.length; l++) {
                        $scope.Servicetaxlist[l].Amount = Math.round(($scope.Servicetaxlist[l].TaxAmount * ratio) * 100 + Number.EPSILON) / 100
                    }
                }
            }
        }

        angular.element(document.querySelector('#ISCpopUp')).modal('hide');
    }
    // #endregion Details

    $scope.BudgetActivityList = [];
    $scope.getBudgetActivityInIssueMaterial = function (materialGroupMasterId) {
        $http({
            method: "GET",
            url: 'Products/InventoryIssue/GetBudgetActivityInSalesMaterial?materialGroupMasterId=' + materialGroupMasterId
        }).then(function successCallback(response) {
            $scope.BudgetActivityList = response.data;
            $scope.detailModel.GLGeneralInfoId = $scope.BudgetActivityList[0].GLGeneralInfoId;
            $scope.detailModel.BudgetMasterId = $scope.BudgetActivityList[0].BudgetMasterId;
            $scope.detailModel.BudgetName = $scope.BudgetActivityList[0].BudgetName;
            $scope.getActivity($scope.BudgetActivityList[0]);
        });
    };

    $scope.LocalTaxInvoiceReport = function (data) {
        location.href = "Sales/SalesReturnReport?salesReturnId=" + data.Id;
    };
}