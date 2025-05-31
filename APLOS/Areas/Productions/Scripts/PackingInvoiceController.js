'use strict';
PackingInvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'accountService', 'bankService', '$window'];
function PackingInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, accountService, bankService, $window) {
    $rootScope.title = 'Packing Invoice';
    $scope.path = 'Productions/PackingInvoice/';
    $scope.searchBy = "Customer"; $scope.search = "";
    $scope.searchByList = [{ value: 'PO', name: "PO" }, { value: 'Customer', name: "Customer" }, { value: 'Productcode', name: "Product Code" }];
    $scope.Action = 'Save';
    $scope.partyType = "Customer";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    //baseService.init("Productions/PackingInvoice/GetList", null, null, "DESC", "InvoiceNo", "InvoiceNo");
    baseService.init("Productions/PackingInvoice/GetList", null, null, "DESC", "AddedDate", "InvoiceNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.invoiceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchInvoiceList = [
        {
            "name": "InvoiceNo",
            "value": "InvoiceNo"
        },
        {
            "name": "Invoice Date",
            "value": "InvoiceDate"
        },
        {
            "name": "Customer Name",
            "value": "PartyName"
        },
        {
            "name": "Customer Code",
            "value": "PartyCode"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        },
        {
            "name": "Status",
            "value": "RowState"
        }
    ];

    $scope.TaxOption = function (data) {
        $scope.salesVM.TaxOption = data;
    };
    $scope.TaxOptionMat = function (data) {
        $scope.salesVM.TaxOptionMat = data;

    };
    $scope.TaxOptionService = function (data) {
        $scope.salesVM.TaxOptionService = data;

    };
    $scope.TaxOptionServiceModify = function (data) {
        $scope.salesVM.TaxOptionServiceModify = data;

    };

    $scope.salesVM = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        CurrencyId: null,
        EntityId: null,
        ItemDescription: null,
        PartyType: "Customer",
        InvoiceDate: $filter("dateFiltering")(Date.now()),
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        Amount: 0,
        BankAmount: 0,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        CompanyCurrencyRate: 1,
        InvoicingPartyPlantId: null,
        DeliveryPartyPlantId: null,
        InvoicingByAddress: null,
        DeliveryByAddress: null,
        InvoicingState: null,
        InvoicingGSTIN: null,
        DeliveryState: null,
        DeliveryGSTIN: null,
        BLNumber: null,
        LCNumber: null,
        ComercialInvoiceNo: null,
        EXPFromNo: null,
        SourceType: 'Packing',
        ContractId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes',
        BooksCurrencyTransactionAmount: null,
        BooksCurrencyTaxAmount: null,
        BooksCurrencyBaseRate: null,
        IsPark: 1,
        IsAdditionalInfoApplicable: true,
        IsIncentiveApplicable: false,
        InvoiceStatus: 'Active',
        PaymentToReceiveBankId: null,
        Incoterms: null,
        IncotermsValue: 0,
        AdditionalFrieghtValue: 0,
        AdditionalFrieght: null,
        TrancastionTypeId: null
    };


    $scope.TrancastionTypeCboList = [];
    function GetTrancastionTypeCboList() {
        $http({
            method: 'GET',
            url: 'Productions/SalesPurchaseTransactionType/GetSalesTypeCbo'
        }).then(function (response) {
            $scope.TrancastionTypeCboList = response.data;
        });
    }
    GetTrancastionTypeCboList();

    $scope.AdditionalFreightList = [
        { Value: "PercentageOfValue", Text: "Percentage Of Value" },
        { Value: "Rate/Unit", Text: "Rate/Unit" },
        { Value: "Fixed", Text: "Fixed" }
    ];

    $scope.IncotermsList = [
        { Value: "CIF", Text: "CIF" },
        { Value: "CFR", Text: "CFR" },
        { Value: "CPT", Text: "CPT" }
    ];

    $scope.paymentTermList = [];
    $scope.PackingList = [];
    $scope.GetPackingListPopUp = function () {
        $scope.PackingList = [];
        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetPackingData"
        }).then(function (response) {
            $scope.PackingList = response.data;
        });
        angular.element(document.querySelector('#PackingListPopUp')).modal('show');
    }

    $scope.ClosePackingList = function () {
        try {
            MakeData();

            if ($scope.selectedPackingList.length > 0) {
                var uniquePackingId = removeDuplicates($scope.selectedPackingList, 'PackingId');
                var wcPackingId = "";
                if (uniquePackingId.length > 0) {
                    wcPackingId = "IN(";
                    wcPackingId += Array.prototype.map.call(uniquePackingId, function (item) { return "'" + item.PackingId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcPackingId;
            }


            $scope.GetPackingSOData($scope.sqlInStatement);
            angular.element(document.querySelector('#PackingListPopUp')).modal('hide');
            //angular.element(document.querySelector('#PackingListPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    function checkExistPTList(list, PTId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value === PTId) {
                return true;
            }
        }
        return false;
    }

    $scope.selectedPackingList = [];
    function MakeData() {
        $scope.paymentTermList = [];
        try {
            for (var i = 0; i < $scope.PackingList.length; i++) {
                var getRow = $filter("filter")($scope.selectedPackingList, { "selectedPackingList": $scope.PackingList[i].PackingId });
                if (getRow.length == 0) {
                    if ($scope.PackingList[i].Active == true) {
                        var ob = {};
                        var ObjPt = { Value: null, Text: null, BaseLineDate: null, NoOfDay: 0, PaymentTermCode: null };
                        ob.PackingId = $scope.PackingList[i].PackingId;
                        ob.PartyId = $scope.PackingList[i].CustomerId;
                        ob.EntityId = $scope.PackingList[i].EntityId;
                        $scope.salesVM.PartyId = $scope.PackingList[i].CustomerId;
                        if (checkExistCustomer($scope.selectedPackingList, ob.PartyId)) {
                            if (checkExistList($scope.selectedPackingList, ob.PackingId) === false) {

                                ob.PackingId = $scope.PackingList[i].PackingId;
                                $scope.salesVM.PackingId = $scope.PackingList[i].PackingId;
                                ob.Entity = $scope.PackingList[i].Entity;
                                $scope.salesVM.EntityId = $scope.PackingList[i].EntityId;
                                $scope.salesVM.CurrencyId = $scope.PackingList[i].CurrencyId;
                                ob.Customer = $scope.PackingList[i].Customer;
                                ob.CustomerId = $scope.PackingList[i].CustomerId;
                                $scope.salesVM.PartyName = $scope.PackingList[i].Customer;
                                $scope.salesVM.PartyId = $scope.PackingList[i].CustomerId;

                                ob.StorageLoc = $scope.PackingList[i].StorageLoc;
                                ob.ByWhom = $scope.PackingList[i].ByWhom;
                                ob.DRespPerson = $scope.PackingList[i].DRespPerson;
                                ob.AddedDate = $scope.PackingList[i].AddedDate;
                                ob.InActiveDate = $scope.PackingList[i].InActiveDate;
                                ObjPt.Value = $scope.PackingList[i].PaymentTermId;
                                ObjPt.Text = $scope.PackingList[i].PaymentTermName;
                                ObjPt.PaymentTermCode = $scope.PackingList[i].PaymentTermCode;
                                ObjPt.NoOfDay = $scope.PackingList[i].NoOfDay;
                                ObjPt.BaseLineDate = $scope.PackingList[i].BaseLineDate;

                                if (checkExistPTList($scope.paymentTermList, ObjPt.Value) === false) {
                                    $scope.paymentTermList.push(ObjPt);
                                }

                                ObjPt = { Value: null, Text: null, BaseLineDate: null, NoOfDay: 0, PaymentTermCode: null };

                                $scope.getPartyPlant();
                                $scope.selectedPackingList.push(ob);

                            }
                        }
                        else {
                            throw 'Select same Entity and Customer.';
                        }
                    }
                }
            }
            if (baseService.arrayLength($scope.paymentTermList) == 1) {
                $scope.salesVM.PaymentTermId = $scope.paymentTermList[0].Value;
                $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
            }

            $scope.GetCurrencyExchangeRateList();
        } catch (e) {
            ShowResult(e, 'failure', 'PackingListPopUp');
        }
    }

    function checkExistCustomer(list, customerId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== customerId) {
                return false;
            }
        }
        return true;
    }

    function checkExistList(list, PackingId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PackingId === PackingId) {
                return true;
            }
        }
        return false;
    }

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, " ", function (result) {
        $scope.entityList = result;
    });
    $scope.salesOrderList = [];
    $scope.GetPackingSOData = function () {
        $scope.salesOrderList = [];
        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetPackingSOData?PackingId=" + $scope.sqlInStatement
        }).then(function (response) {
            $scope.salesOrderList = response.data;
            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                $scope.salesVM.InvoicingPartyPlantId = $scope.salesOrderList[i].InvoicingPartyPlantId;
                $scope.salesVM.DeliveryPartyPlantId = $scope.salesOrderList[i].DeliveryPartyPlantId;
                getTaxCategoryList($scope.salesOrderList[i].HSNCodeId, $scope.salesOrderList[i].SONo, $scope.salesOrderList[i].TransactionAmount);
            }
        });
    }

    $scope.salesOrderNewList = [];
    $scope.GetPackingSODatum = function () {
        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetPackingSOData?PackingId=" + $scope.sqlInStatement
        }).then(function (response) {
            $scope.salesOrderNewList = response.data;
            angular.element(document.querySelector('#salesOrderItemPopUp')).modal('show');
        });
    }

    $scope.ApplyOrderItemPopUp = function () {
        MakeItemData();
        angular.element(document.querySelector('#salesOrderItemPopUp')).modal('hide');
    }

    function MakeItemData() {

        for (var i = 0; i < $scope.salesOrderNewList.length; i++) {

            if (checkItemExist($scope.salesOrderList, $scope.salesOrderNewList[i].SONo, $scope.salesOrderNewList[i].MaterialMasterId, $scope.salesOrderNewList[i].ArticleId, $scope.salesOrderNewList[i].FirstCharacteristicsValueId, $scope.salesOrderNewList[i].SecondCharacteristicsValueId) === false) {

                if ($scope.salesOrderNewList[i].Active == true) {
                    var ob = {};
                    ob.Id = null;
                    ob.SalesId = $scope.salesVM.Id;
                    ob.MasterOrderId = $scope.salesOrderNewList[i].MasterOrderId;
                    ob.MasterOrderItemId = $scope.salesOrderNewList[i].MasterOrderItemId;
                    ob.MaterialMasterArticleName = $scope.salesOrderNewList[i].MaterialMasterArticleName;
                    ob.MaterialMasterName = $scope.salesOrderNewList[i].MaterialMasterName;
                    ob.MaterialMasterId = $scope.salesOrderNewList[i].MaterialMasterId;
                    ob.ArticleId = $scope.salesOrderNewList[i].ArticleId;
                    ob.SONo = $scope.salesOrderNewList[i].SONo;
                    ob.SalesOrderId = $scope.salesOrderNewList[i].SalesOrderId;
                    ob.PackingId = $scope.salesOrderNewList[i].PackingId;
                    ob.PONumber = $scope.salesOrderNewList[i].PONumber;
                    ob.DeliveryDate = $scope.salesOrderNewList[i].DeliveryDate;
                    ob.DestinationName = $scope.salesOrderNewList[i].DestinationName;
                    ob.GoodsDescription = $scope.salesOrderNewList[i].GoodsDescription;
                    ob.SKU1 = $scope.salesOrderNewList[i].SKU1;
                    ob.SKU2 = $scope.salesOrderNewList[i].SKU2;
                    ob.Rate = $scope.salesOrderNewList[i].Rate;
                    ob.TransactionQty = $scope.salesOrderNewList[i].TransactionQty;
                    ob.TransactionAmount = $scope.salesOrderNewList[i].TransactionAmount;
                    ob.TaxAmount = $scope.salesOrderNewList[i].TaxAmount;
                    ob.BaseUOMId = $scope.salesOrderNewList[i].BaseUOMId;
                    ob.BaseQty = $scope.salesOrderNewList[i].BaseQty;
                    ob.BaseRate = $scope.salesOrderNewList[i].BaseRate;
                    ob.CommitmentDate = $scope.salesOrderNewList[i].CommitmentDate;
                    ob.CustomerPOId = $scope.salesOrderNewList[i].CustomerPOId;
                    ob.DestinationId = $scope.salesOrderNewList[i].DestinationId;
                    ob.Discount = $scope.salesOrderNewList[i].Discount;
                    ob.FirstCharacteristicsId = $scope.salesOrderNewList[i].FirstCharacteristicsId;
                    ob.FirstCharacteristicsValueId = $scope.salesOrderNewList[i].FirstCharacteristicsValueId;
                    ob.HSNCodeId = $scope.salesOrderNewList[i].HSNCodeId;
                    ob.HSNCode = $scope.salesOrderNewList[i].HSNCode;
                    ob.InvoicingPartyPlantId = $scope.salesOrderNewList[i].InvoicingPartyPlantId;
                    ob.DeliveryPartyPlantId = $scope.salesOrderNewList[i].DeliveryPartyPlantId;
                    ob.IsFirstEntry = $scope.salesOrderNewList[i].IsFirstEntry;
                    ob.LSD = $scope.salesOrderNewList[i].LSD;
                    ob.MainRawMaterialInhouseDate = $scope.salesOrderNewList[i].MainRawMaterialInhouseDate;
                    ob.OrderCategoryId = $scope.salesOrderNewList[i].OrderCategoryId;
                    ob.OrderStatusId = $scope.salesOrderNewList[i].OrderStatusId;
                    ob.OtherRawMaterialInhouseDate = $scope.salesOrderNewList[i].OtherRawMaterialInhouseDate;
                    ob.PODate = $scope.salesOrderNewList[i].PODate;
                    ob.PlanQty = $scope.salesOrderNewList[i].PlanQty;
                    ob.ProductName = $scope.salesOrderNewList[i].ProductName;
                    ob.Qty = $scope.salesOrderNewList[i].Qty;
                    ob.ResponsiblePersonId = $scope.salesOrderNewList[i].ResponsiblePersonId;
                    ob.ResponsiblePersonName = $scope.salesOrderNewList[i].ResponsiblePersonName;
                    ob.SKUQty = $scope.salesOrderNewList[i].SKUQty;
                    ob.SalesQty = $scope.salesOrderNewList[i].SalesQty;
                    ob.SecondCharacteristicsId = $scope.salesOrderNewList[i].SecondCharacteristicsId;
                    ob.SecondCharacteristicsValueId = $scope.salesOrderNewList[i].SecondCharacteristicsValueId;
                    ob.ShipmentModeId = $scope.salesOrderNewList[i].ShipmentModeId;
                    ob.TransactionAmount = $scope.salesOrderNewList[i].TransactionAmount;
                    ob.TransactionQty = $scope.salesOrderNewList[i].TransactionQty;
                    ob.TransactionRate = $scope.salesOrderNewList[i].TransactionRate;
                    ob.UpCharge = $scope.salesOrderNewList[i].UpCharge;

                    $scope.salesOrderList.push(ob);
                    ob = {};
                    getTaxCategoryList($scope.salesOrderNewList[i].HSNCodeId, $scope.salesOrderNewList[i].SONo, $scope.salesOrderNewList[i].TransactionAmount);
                }
            }
        }
    }

    function checkItemExist(list, SONo, materialMasterId, articleId, FirstCharacteristicsValueId, SecondCharacteristicsValueId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalesOrderId === SONo && list[i].MaterialMasterId === materialMasterId && list[i].ArticleId === articleId && list[i].FirstCharacteristicsValueId === FirstCharacteristicsValueId && list[i].SecondCharacteristicsValueId === SecondCharacteristicsValueId) {
                return true;
            }
        }
        return false;
    }

    $scope.closeMasterOrderItemPopUp = function () {
        angular.element(document.querySelector('#salesOrderItemPopUp')).modal('hide');
    }

    $scope.CalculateTransactionAmount = function (data) {

        data.TaxAmount = 0;
        if (!baseService.isUndefinedOrNull(data.Id)) {

            data.TransactionAmount = parseFloat(data.Rate * data.Qty).toFixed(2);
        } else {

            data.TransactionAmount = parseFloat(data.Rate * data.TransactionQty).toFixed(2);
        }

        if (baseService.arrayLength(data.TaxList) > 0) {
            angular.forEach(data.TaxList, function (item) {
                item.TotalAmount = parseFloat((data.TransactionAmount * item.Percentage / 100).toFixed(2));
                data.TaxAmount += item.TotalAmount;
            });
            data.NetAmount = parseFloat(data.TransactionAmount) + parseFloat(data.TaxAmount);
        } else {
            data.NetAmount = parseFloat(data.TransactionAmount).toFixed(2);
        }
    }

    function getTaxCategoryList(hsnCodeId, soId, transactionAmount) {
        $http({
            method: 'GET',
            //url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
            url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.DeliveryPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
        }).then(function (response) {
            $scope.materialtaxCategoryList = response.data;

            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                if ($scope.salesOrderList[i].SONo === soId && baseService.isUndefinedOrNull($scope.salesOrderList[i].Id)) {
                    $scope.salesOrderList[i].TaxList = $scope.materialtaxCategoryList;
                    for (var j = 0; j < $scope.salesOrderList[i].TaxList.length; j++) {
                        $scope.calculateHSNTaxAmount($scope.salesOrderList[i].TaxList[j], transactionAmount);
                    }
                    $scope.CalculateTransactionAmount($scope.salesOrderList[i]);
                }
            }
        });
    }

    $scope.calculateHSNTaxAmount = function (data, transactionAmount) {
        $scope.taxAbleAmnt = transactionAmount;
        data.TotalAmount = $scope.taxAbleAmnt * data.Percentage / 100;
    };

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.salesVM.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.salesVM.PaymentTermId; })[0];
            $scope.salesVM.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.salesVM.BaseNoOfDays = paymentTerm.NoOfDay;
            $scope.BaseLineDate = paymentTerm.BaseLineDate;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.salesVM.BaseOnDueDate = $filter('dateFiltering')($scope.salesVM.InvoiceDate);
                    $scope.IsBaseOnDueDateEnable = false;
                }
                else if (paymentTerm.BaseLineDate === 'postingdate') {
                    $scope.salesVM.BaseOnDueDate = $filter('dateFiltering')($scope.salesVM.InvoiceDate);
                    $scope.salesVM.BaseOnDueDate = null;
                    $scope.salesVM.BaseNoOfDays = null;
                    $scope.salesVM.MatureDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }

                else {
                    $scope.salesVM.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = true;
                }

            $scope.getMatureDate($scope.salesVM.BaseOnDueDate, $scope.salesVM.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.salesVM.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.salesVM.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };

    $scope.dateMessage = "";
    $scope.checkDocDate = function () {
        if (new Date($scope.salesVM.DocDate) > new Date()) {
            $scope.dateMessage = "Doc date must be below or equal to current Date!";
            return false;
        }
        else {
            $scope.dateMessage = "";
            return true;
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };

    $scope.closeInvoicingPartyPopUp = function () {
        if ($scope.salesMaterialList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.salesVM.ChangeInvoicingStateId)) {
                if ($scope.salesVM.PlantStateId == $scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.PlantStateId != $scope.salesVM.InvoicingStateId && $scope.salesVM.PlantStateId != $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

    $scope.getPartyPlant = function () {
        $scope.salesVM.InvoicingPartyPlantId = null;
        $scope.salesVM.DeliveryPartyPlantId = null;
        $scope.salesVM.InvoicingByAddress = null;
        $scope.salesVM.DeliveryByAddress = null;
        $scope.salesVM.InvoicingState = null;
        $scope.salesVM.InvoicingGSTIN = null;
        $scope.salesVM.DeliveryState = null;
        $scope.salesVM.DeliveryGSTIN = null;
        $scope.salesVM.InvoicingStateId = null;


        $scope.getCboPartyPlantList($scope.salesVM.PartyId, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = item.Value;
                    $scope.salesVM.InvoicingByAddress = item.Address1;
                    $scope.salesVM.DeliveryByAddress = item.Address1;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = item.StateName;
                    $scope.salesVM.DeliveryGSTIN = item.GSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });
        });
    }

    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
            if (flag === 'billTo') {
                $scope.salesVM.InvoicingState = state;
                $scope.salesVM.ChangeInvoicingStateId = stateId;
                $scope.salesVM.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.salesVM.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.salesVM.DeliveryState = state;
                $scope.salesVM.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.salesVM.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.salesVM.InvoicingState = null;
                $scope.salesVM.InvoicingGSTIN = null;
                return $scope.salesVM.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.salesVM.DeliveryState = null;
                $scope.salesVM.DeliveryGSTIN = null;
                return $scope.salesVM.DeliveryByAddress = null;
            }
        }
    };

    $scope.closeInvoicingPartyPopUp = function () {
        //if ($scope.salesMaterialList.length || $scope.chargesList.length) {
        //    if (!baseService.isUndefinedOrNull($scope.salesVM.ChangeInvoicingStateId)) {
        //        if ($scope.salesVM.PlantStateId == $scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
        //            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //        else if ($scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
        //            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //        else if ($scope.salesVM.PlantStateId != $scope.salesVM.InvoicingStateId && $scope.salesVM.PlantStateId != $scope.salesVM.ChangeInvoicingStateId)
        //            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //        else
        //            ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
        //    }
        //    else
        //        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //}
        //else
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.GetCurrencyExchangeRateList = function () {
        if ($scope.salesVM.CurrencyId !== null && undefined !== $scope.salesVM.CurrencyId) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.salesVM.PostingDate + "&currencyId=" + $scope.salesVM.CurrencyId
            }).then(function (response) {
                $scope.currencyExchangeRate = response.data;
                $scope.salesVM.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
    };

    $scope.refreshPackingTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPackingWise });
    };

    function CheckBoxSelectAllPackingWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPacking").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PackingList.length; i++) {
                $scope.PackingList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPacking").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.savebtndisable = false;
        $scope.salesVM.RowState = 'Parked';
        $scope.salesVM = {
            Id: null,
            CompanyGroupId: null,
            CompanyId: null,
            PartyId: null,
            EntityId: null,
            ItemDescription: null,
            PartyName: null,
            CurrencyId: null,
            PartyType: "Customer",
            InvoiceDate: $filter("dateFiltering")(Date.now()),
            VoucherDate: $filter("dateFiltering")(Date.now()),
            PostingDate: $filter("dateFiltering")(Date.now()),
            DocDate: $filter("dateFiltering")(Date.now()),
            DocRefNo: null,
            Amount: 0,
            BankAmount: 0,
            BaseOnDueDate: null,
            BaseNoOfDays: null,
            PaymentTermId: null,
            Narration: null,
            CompanyCurrencyRate: 1,
            InvoicingPartyPlantId: null,
            DeliveryPartyPlantId: null,
            InvoicingByAddress: null,
            DeliveryByAddress: null,
            InvoicingState: null,
            InvoicingGSTIN: null,
            DeliveryState: null,
            DeliveryGSTIN: null,
            BLNumber: null,
            LCNumber: null,
            ComercialInvoiceNo: null,
            EXPFromNo: null,
            SourceType: 'Packing',
            ContractId: null
            , TaxOption: 'Yes'
            , TaxOptionMat: 'Yes'
            , TaxOptionService: 'Yes'
            , TaxOptionServiceModify: 'Yes'
            , TaxOptionAddiTax: 'Yes',
            BooksCurrencyTransactionAmount: null,
            BooksCurrencyTaxAmount: null,
            BooksCurrencyBaseRate: null,
            IsPark: 1,
            IsAdditionalInfoApplicable: true,
            IsIncentiveApplicable: false,
            InvoiceStatus: 'Active',
            PaymentToReceiveBankId: null,
            Incoterms: null,
            IncotermsValue: 0,
            AdditionalFrieghtValue: 0,
            AdditionalFrieght: null,
            TrancastionTypeId: null
        };

        $scope.materialMaster = {
            MaterialMasterId: null,
            MaterialMasterName: null,
            BaseUOMId: null,
            BaseUoM: null,
            OurStyleName: null,
            MaterialGroupMasterName: null,
            ProductMasterName: null,
            IsOurStyleRequired: null,
            IsProductMstRequired: null,
            TransactionUoMId: null,
            ArticleId: null,
            ArticleName: null,
            CountryId: null
        };
        $scope.currencyExchangeRate = [];
        $scope.salesMaterialList = [];
        $scope.chargesList = [];
        $scope.receiveTaxList = [];
        $scope.uoMList = [];
        $scope.selectedPackingList = [];
        $scope.salesOrderList = [];
        $scope.SalesAdditionalInfoList = [];
        $scope.taxCategoryList = [];
    }

    $scope.serviceChargeTaxPopUp = function () {
        angular.element(document.querySelector("#serviceChargeTaxPopUp")).modal("show");
    };

    $http.get("Setups/CompanyServiceMaster/GetCboList")
        .then(function (response) {
            $scope.serviceList = response.data;
        });

    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector("#serviceChargeTaxPopUp")).modal("hide");
    };

    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
        }
    };

    $scope.calculateAmount = function (data) {
        data.TransactionAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TransactionAmount == 'NaN')
            data.TransactionAmount = 0;
        data.TaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TotalAmount = data.TransactionAmount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = parseFloat(data.TransactionAmount) + parseFloat(data.TaxAmount);
    };

    $scope.calculateRate = function (data) {
        data.TransactionRate = (data.TransactionAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate == 'NaN')
            data.TransactionRate = 0;
        data.TaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TotalAmount = data.TransactionAmount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = parseFloat(data.TransactionAmount) + parseFloat(data.TaxAmount);
    };

    $scope.calculateServiceAmount = function (data) {
        if (data.Amount == 'NaN')
            data.Amount = 0;
        data.TaxAmount = 0;
        angular.forEach(data.ServiceTaxList, function (item) {
            item.TotalAmount = data.Amount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = parseFloat(data.Amount) + parseFloat(data.TaxAmount);
    };

    $scope.updateMaterialTax = function () {
        var data = $scope.salesMaterialList[$scope.currentMaterialRow];
    };

    $scope.chargesList = [];
    $scope.addCharge = function () {
        var data = {
            Amount: 0
        };
        $scope.chargesList.push(data);
    };

    $scope.getMaterialTaxList = function (data, flag, index) {
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        var d = $scope.salesOrderList[$scope.currentMaterialRow];

        $scope.salesVM.TaxOption = 'Yes';
        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.taxAbleAmnt = data.TransactionAmount;

        $scope.receiveTaxList = [];


        if ($scope.salesOrderList[$scope.currentMaterialRow].TaxList.length > 0) {
            $scope.HSNCode = $scope.salesOrderList[$scope.currentMaterialRow].TaxList[0].HSNCode;
            if (baseService.isUndefinedOrNull($scope.salesOrderList[$scope.currentMaterialRow].TaxList[0].HSNCode)) {
                $scope.HSNCode = $scope.salesOrderList[$scope.currentMaterialRow].HSNCode;
            }

            angular.copy($scope.salesOrderList[$scope.currentMaterialRow].TaxList, $scope.receiveTaxList);


        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };

    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });
    }

    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.receiveTaxList.push(data);
    };

    $scope.addServiceTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.ServicetaxPopList.push(data);
    };

    $scope.calculateTaxAmount = function (data) {

        data.TotalAmount = parseFloat($scope.taxAbleAmnt * data.Percentage / 100).toFixed(2);
    };
    $scope.checkRowValidation = function (x) {
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TotalAmount / $scope.taxAbleAmnt).toFixed(2) * 100);
            }

        }
    }

    $scope.closeReceiveTaxPopUp = function () {
        try {
            var materialData = $scope.salesOrderList[$scope.currentMaterialRow];
            $scope.salesOrderList[$scope.currentMaterialRow].TaxAmount = 0;
            for (var i = 0; i < $scope.receiveTaxList.length; i++) {
                var taxcat = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
                if (taxcat.length == 2) {
                    ShowResult('Same Tax Category already exsist', 'failure', 'receiveTaxPopUp');
                    angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
                }
                var TxA = parseFloat($scope.salesOrderList[$scope.currentMaterialRow].TaxAmount) + parseFloat($scope.receiveTaxList[i].TotalAmount);
                $scope.salesOrderList[$scope.currentMaterialRow].TaxAmount = parseFloat(TxA.toFixed(2));
            }
            $scope.salesOrderList[$scope.currentMaterialRow].TaxList = $scope.receiveTaxList;
            var NAmount = parseFloat($scope.salesOrderList[$scope.currentMaterialRow].TransactionAmount) + parseFloat($scope.salesOrderList[$scope.currentMaterialRow].TaxAmount);
            $scope.salesOrderList[$scope.currentMaterialRow].NetAmount = parseFloat(NAmount.toFixed(2));
            $scope.materialMaster = {};
            $scope.receiveTaxList = [];
            $scope.isService = false;
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'receiveTaxPopUp');
        }
    };

    $scope.closeReceiveTaxPopUpwindow = function () {
        // getInventoryMaterialList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.closeServiceTaxPopUp = function () {
        var salesData = $scope.chargesList[$scope.currentServiceRow];
        //$scope.chargesList[$scope.currentServiceRow].TaxAmount = 0;
        $scope.chargesList[$scope.currentServiceRow].Amount = 0;
        angular.forEach($scope.receiveTaxList, function (item) {
            $scope.chargesList[$scope.currentServiceRow].TaxAmount += item.Amount;
        });
        //$scope.chargesList[$scope.currentServiceRow].NetAmount = $scope.chargesList[$scope.currentServiceRow].Amount + $scope.chargesList[$scope.currentServiceRow].TaxAmount;
        $scope.chargesList[$scope.currentServiceRow].NetAmount = $scope.chargesList[$scope.currentServiceRow].Amount;

        //  $scope.materialMaster = {};
        //  $scope.ServicetaxPopList = [];
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    };

    $scope.getServiceTax = function (index) {
        $scope.currentServiceIndex = index;
        var data = $scope.chargesList[$scope.currentServiceIndex];
        var TaxList = [];
        var hsnCodeId = $filter("filter")($scope.serviceList, { HSNCodeId: data.ServiceMasterId })[0].HSNCodeId;
        $http({
            method: 'GET',
            url: 'Accounts/TaxCategory/GetTaxCategoryList?partyPlantId=' + $scope.salesVM.PartyPlantId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            TaxList = response.data;
            $scope.chargesList[$scope.currentServiceIndex].ServiceTaxList = TaxList;
        });
    };

    $scope.getServiceTaxList = function (data, flag, ServiceId, index) {
        $scope.isService = true;

        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.ServiceId = ServiceId;
        if (!$scope.isService) {
            $scope.taxAbleAmnt = data.TransactionAmount;
        }
        else {
            $scope.taxAbleAmnt = data.Amount;
        }
        $scope.percentageColumn = flag;
        $scope.currentServiceRow = index;
        $scope.receiveTaxList = [];
        if (data.ServiceTaxList.length > 0) {
            $scope.HSNCode = data.ServiceTaxList[0].HSNCode;
            $scope.receiveTaxList = data.ServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        $scope.salesVM.TaxOptionServiceModify = 'Yes';
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
    }

    $scope.closeServiceChargeTaxPopUpwindowFG = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }

    $scope.calculateTaxAmountForService = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        // data.Amount = Math.round($scope.serviceModel.Amount * data.Percentage) / 100;
        data.Amount = parseFloat(($scope.serviceModel.Amount * data.Percentage) / 100).toFixed(2);
        $scope.calculateSvcTaxCategory();
    };

    $scope.checkRowValidationService = function (x) {
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {

            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.Amount).toFixed(2) * 100);
            }
        }
    }

    $scope.calculateTaxAmountForServiceModify = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.Amount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.checkRowValidationServiceModify = function (x) {
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(2) * 100);
            }
        }
    }

    $scope.closeServiceChargeTaxPopUpwindow = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }

    $scope.closeServiceChargeTaxPopUpwindowFG = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }

    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TaxAmount = 0;
        $scope.serviceModel.NetAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].Amount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.Amount) / 100).toFixed(2);
            $scope.serviceModel.TaxAmount = (parseFloat($scope.serviceModel.TaxAmount) + parseFloat($scope.taxCategoryList[i].Amount)).toFixed(2);
        }
        if (isNaN($scope.serviceModel.TaxAmount)) $scope.serviceModel.TaxAmount = 0;
        //$scope.serviceModel.NetAmount = parseFloat($scope.serviceModel.TaxAmount) + $scope.serviceModel.Amount;
        $scope.serviceModel.NetAmount = $scope.serviceModel.Amount;
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TaxAmount = (parseFloat($scope.serviceModel.TaxAmount) + parseFloat($scope.taxCategoryList[i].Amount)).toFixed(2);
        }
    };

    function getServiceTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            //, url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
            , url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.DeliveryPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].hsnCodeId)) {
                    $scope.taxCategoryList[i].HSNCode = HSNCode;
                    $scope.taxCategoryList[i].HSNCodeId = hsnCodeId;
                }
            }
        });
    }

    function getInvoicingPartyPlantIdServiceTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
            //, url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.DeliveryPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].hsnCodeId)) {
                    $scope.taxCategoryList[i].HSNCode = HSNCode;
                    $scope.taxCategoryList[i].HSNCodeId = hsnCodeId;
                }
            }
        });
    }

    $scope.changeService = function (id) {
        $scope.serviceModel.ServiceMasterId = id;
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCode;
        $scope.serviceModel.ChargeName = angular.element("#charge :selected").text();
        getInvoicingPartyPlantIdServiceTaxCategoryList(hsnCodeId, HSNCode);
    };

    $scope.serviceChargePopUp = function () {
        //if (baseService.arrayLength($scope.salesMaterialList) === 0)
        //    return ShowResult('Without material charges not aplicable.');

        $scope.salesVM.TaxOptionService = 'Yes';
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , SalesId: $scope.salesVM.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , ChargeName: null
            , CurrencyId: $scope.salesVM.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.salesVM.DocDate
            , Amount: 0
            , TaxAmount: 0
            , NetAmount: 0
            , ServiceTaxList: null
        };
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };
    $scope.chargeValidation = function () {
        var getRowCharge = $filter("filter")($scope.chargesList, { "ServiceMasterId": $scope.serviceModel.ServiceMasterId });
        if (getRowCharge == 0) {
            $scope.invalidcharges = false;
        }
        else {
            ShowResult('This Charge  already exsist', 'failure', 'serviceChargePopUp');
            $scope.invalidcharges = true;
        }
    }

    $scope.closeServiceChargeAddPopUp = function () {
        //$scope.serviceModel.TaxAmount = $filter("sumByKey")($filter("filter")($scope.taxCategoryList), "TotalAmount");
        $scope.serviceModel.TaxAmount = $filter("sumByKey")($filter("filter")($scope.taxCategoryList), "Amount");
        $scope.serviceModel.ServiceTaxList = $scope.taxCategoryList;
        $scope.chargeValidation();
        if (!$scope.invalidcharges) {
            $scope.chargesList.push($scope.serviceModel);
            angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
        }
    }

    $scope.closeServiceChargePopUp = function () {
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');

    }

    $scope.calculateServicePopupAmount = function (data) {
        if (data.Amount == 'NaN')
            data.Amount = 0;
        data.TaxAmount = 0;
        angular.forEach($scope.taxCategoryList, function (item) {
            item.TotalAmount = data.Amount * item.Percentage / 100;
            data.TaxAmount += item.TotalAmount;
        });
        data.NetAmount = data.TaxAmount + data.Amount;
    };

    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.salesVM.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = deliveryplant;
                    $scope.salesVM.InvoicingByAddress = invoAddress;
                    $scope.salesVM.DeliveryByAddress = deliAddress;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = deliState;
                    $scope.salesVM.DeliveryGSTIN = deliGSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });

        });
    }

    $scope.selectedMasterOrderItemTempList = [];
    $scope.GetSalesMaterialData = function (salesId) {
        var ObjPt = { Value: null, Text: null, BaseLineDate: null, NoOfDay: 0, PaymentTermCode: null };
        $scope.salesOrderList = [];
        $scope.salesMaterialList = [];
        $scope.paymentTermList = [];
        $scope.selectedMasterOrderItemTempList = [];
        $scope.uoMList = [];
        $http({
            method: "GET",
            url: "Productions/PackingInvoice/GetMasterOrderSalesMaterialData?salesId=" + salesId
        }).then(function (response) {
            $scope.salesMaterialList = response.data;
            for (var i = 0; i < $scope.salesMaterialList.length; i++) {
                ObjPt.Value = $scope.salesMaterialList[i].PaymentTermId;
                ObjPt.Text = $scope.salesMaterialList[i].PaymentTermName;
                ObjPt.PaymentTermCode = $scope.salesMaterialList[i].PaymentTermCode;
                ObjPt.NoOfDay = $scope.salesMaterialList[i].NoOfDay;
                ObjPt.BaseLineDate = $scope.salesMaterialList[i].BaseLineDate;
                $scope.paymentTermList.push(ObjPt);
                ObjPt = { Value: null, Text: null, BaseLineDate: null, NoOfDay: 0, PaymentTermCode: null };
            }
            if (baseService.isUndefinedOrNull($scope.salesVM.PaymentTermId)) {
                $scope.salesVM.PaymentTermId = $scope.PTermId;
            }
            for (var p = 0; p < $scope.paymentTermList.length; p++) {
                if ($scope.paymentTermList[p].Value == $scope.salesVM.PaymentTermId) {
                    $scope.salesVM.PaymentTermId = $scope.paymentTermList[p].Value; break;
                }
            }

            $scope.salesOrderList = response.data;

            for (var i = 0; i < $scope.salesMaterialList.length; i++) {
                $scope.getAllTransactionUoM($scope.salesMaterialList[i].MaterialMasterId);
            }

            $scope.GetSalesTaxData(salesId);
            $scope.GetAdvanceTaxInfo($scope.salesVM.Id);
            //$scope.TotalSumAfterTCS();
        });
    };

    function gettaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.TaxList.length; i++) {
            if ($scope.TaxList[i].SalesMaterialId === linepk) {
                result.push($scope.TaxList[i]);
            }
        }
        return result;
    }

    function gettaxServicelist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].SalesServiceId === linepk) {
                result.push($scope.ServiceTaxList[i]);
            }
        }
        return result;
    }

    $scope.GetSalesTaxData = function (salesId) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetSalesTaxData?salesId=" + salesId
        }).then(function (response) {
            $scope.TaxList = response.data;
            for (var i = 0; i < $scope.salesOrderList.length; i++) {
                var linepk = $scope.salesOrderList[i].Id;
                var list = gettaxlist(linepk);
                $scope.salesOrderList[i].TaxList = list;

            }
            $scope.GetSalesServiceData($scope.salesVM.Id);
        });
    };

    $scope.GetSalesServiceData = function (salesId) {
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetSalesServiceData?salesId=" + salesId
        }).then(function (response) {
            $scope.chargesList = response.data;
            $scope.GetSalesServiceTaxData(salesId);
        });
    };

    $scope.GetSalesServiceTaxData = function (salesId) {
        $scope.ServiceTaxList = [];
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetSalesServiceTaxData?salesId=" + salesId
        }).then(function (response) {
            $scope.ServiceTaxList = response.data;
            for (var i = 0; i < $scope.chargesList.length; i++) {
                var linepk = $scope.chargesList[i].Id;
                var list = gettaxServicelist(linepk);
                $scope.chargesList[i].ServiceTaxList = list;
            }
        });
    };

    $scope.getAllTransactionUoM = function (materialMasterId) {
        var mmId = [];
        mmId.push(materialMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            var getRow = $filter("filter")($scope.uoMList, { "MaterialMasterId": materialMasterId });
            if (getRow.length === 0) {
                angular.forEach(result, function (item, i) {
                    $scope.uoMList.push(item);
                });
            } else {
                $scope.uoMList = result;
            }
        });
    };

    $scope.GetPackingBySalesId = function (salesId) {
        $scope.selectedMasterOrderList = [];
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetMasterOrderDataByMasterOrderId?masterOrderId=" + MasterOrderId + '&masterOrderItemId=' + MasterOrderItemId + '&salesId=' + salesId
        }).then(function (response) {
            $scope.selectedMasterOrderList = response.data;
        });
    };

    $scope.Get = function (data) {
        $scope.salesVM = data;
        $scope.salesVM.BaseOnDueDate = $filter('dateFiltering')(new Date($scope.salesVM.BaseOnDueDate), 'dd-MM-yyyy');
        $scope.salesVM.EXPDate = $filter('dateFiltering')(new Date($scope.salesVM.EXPDate), 'dd-MM-yyyy');
        $scope.salesVM.AddedDate = $filter('dateFiltering')(new Date($scope.salesVM.AddedDate), 'dd-MM-yyyy');
        $scope.salesVM.EXPDate = $filter('dateFiltering')(new Date($scope.salesVM.EXPDate), 'dd-MM-yyyy');
        $scope.ModelNew.Amount = data.Amount;
        getPartyPlantEditList($scope.salesVM.InvoicingPartyPlantId, $scope.salesVM.InvoicingByAddress, $scope.salesVM.DeliveryPartyPlantId, $scope.salesVM.DeliveryByAddress, $scope.salesVM.DeliveryState, $scope.salesVM.DeliveryGSTIN);

        $scope.GetSalesPackingData($scope.salesVM.Id);
        $scope.getPostSalesData();

        $scope.getTaxCodeByTaxYearWithhold($scope.salesVM.InvoiceDate);
        $scope.GetSalesAdditionalInfoList();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.salesVM.TaxOptionAddiTax = 'Yes';

        
    };

    $scope.GetSalesPackingData = function (salesId) {
        $scope.selectedPackingList = [];
        $scope.paymentTermList = [];

        $http({
            method: 'GET',
            url: "Productions/PackingInvoice/GetSalesPackingData?salesId=" + salesId
        }).then(function (response) {
            $scope.selectedPackingList = response.data;

            if ($scope.selectedPackingList.length > 0) {
                var uniquePackingId = removeDuplicates($scope.selectedPackingList, 'PackingId');
                var wcPackingId = "";
                if (uniquePackingId.length > 0) {
                    wcPackingId = "IN(";
                    wcPackingId += Array.prototype.map.call(uniquePackingId, function (item) { return "'" + item.PackingId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcPackingId;
            }
            $scope.GetSalesMaterialData($scope.salesVM.Id);
        });
    }


    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.salesVM.PaymentTermId) && $scope.BaseLineDate !== 'postingdate') {
                throw "Payment Term is required.";
            }
            if (baseService.isUndefinedOrNull($scope.salesVM.BaseOnDueDate) && $scope.BaseLineDate !== 'postingdate') {
                throw "Due Date BaseOn is required.";
            }
            $scope.PTermId = $scope.salesVM.PaymentTermId;
            $scope.BLDate = $scope.salesVM.BLDate;
            $scope.EXPDate = $scope.salesVM.EXPDate;
            $scope.BaseDate = $scope.salesVM.BaseOnDueDate;
            $scope.DData = $scope.salesVM.DocDate;
            $scope.InDate = $scope.salesVM.InvoiceDate;
            $scope.MDate = $scope.salesVM.MatureDate;
            $scope.PDate = $scope.salesVM.PostingDate;
            $scope.VDate = $scope.salesVM.VoucherDate;


            if ($scope.salesVM.IsPark == 0) {
                throw "Posted data cann't save or update.";
            }

            if ($scope.salesVM.InvoiceDate > $scope.salesVM.VoucherDate) {
                throw "Invoice Date can not greater than Entry Date!!.";
            }

            $scope.$broadcast("show-errors-check-validity");
            if ($scope.form0.$valid) {
                $scope.savebtndisable = true;
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: "Productions/PackingInvoice/Create",
                        data: {
                            "voucherVM": $scope.salesVM
                            , "salesMaterialVMList": $scope.salesOrderList
                            , "selectedPackingList": $scope.selectedPackingList
                            , "salesServiceVMList": $scope.chargesList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                            $scope.savebtndisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.savebtndisable = false;
                            $scope.getData();

                            $scope.salesVM = response.data.Data;

                            $scope.salesVM.BLDate = $scope.BLDate;
                            $scope.salesVM.EXPDate = $scope.EXPDate;

                            $scope.salesVM.BaseOnDueDate = $scope.BaseDate;
                            $scope.salesVM.DocDate = $scope.DData;
                            $scope.salesVM.InvoiceDate = $scope.InDate;
                            $scope.salesVM.MatureDate = $scope.MDate;
                            $scope.salesVM.PostingDate = $scope.PDate;
                            $scope.salesVM.VoucherDate = $scope.VDate;
                            $scope.salesVM.PaymentTermId = $scope.PTermId;
                            getPartyPlantEditList($scope.salesVM.InvoicingPartyPlantId, $scope.salesVM.InvoicingByAddress, $scope.salesVM.DeliveryPartyPlantId, $scope.salesVM.DeliveryByAddress, $scope.salesVM.DeliveryState, $scope.salesVM.DeliveryGSTIN);
                            //$scope.GetSalesMaterialData($scope.salesVM.Id);
                            $scope.GetSalesPackingData($scope.salesVM.Id);
                            $scope.getPostSalesData();

                            $scope.getTaxCodeByTaxYearWithhold($scope.salesVM.InvoiceDate);

                            $scope.Action = "Update";
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                else if ($scope.Action === "Update") {
                    $scope.savebtndisable = true;
                    $http({
                        method: "POST",
                        url: "Productions/PackingInvoice/Edit",
                        data: {
                            "voucherVM": $scope.salesVM
                            , "salesMaterialVMList": $scope.salesOrderList
                            , "selectedPackingList": $scope.selectedPackingList
                            , "salesServiceVMList": $scope.chargesList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                            $scope.savebtndisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.savebtndisable = false;
                            $scope.getData();
                            $scope.salesVM = response.data.Data;

                            $scope.salesVM.BLDate = $scope.BLDate;
                            $scope.salesVM.EXPDate = $scope.EXPDate;
                            $scope.salesVM.BaseOnDueDate = $scope.BaseDate;
                            $scope.salesVM.DocDate = $scope.DData;
                            $scope.salesVM.InvoiceDate = $scope.InDate;
                            $scope.salesVM.MatureDate = $scope.MDate;
                            $scope.salesVM.PostingDate = $scope.PDate;
                            $scope.salesVM.VoucherDate = $scope.VDate;
                            $scope.salesVM.PaymentTermId = $scope.PTermId;
                            getPartyPlantEditList($scope.salesVM.InvoicingPartyPlantId, $scope.salesVM.InvoicingByAddress, $scope.salesVM.DeliveryPartyPlantId, $scope.salesVM.DeliveryByAddress, $scope.salesVM.DeliveryState, $scope.salesVM.DeliveryGSTIN);
                            //$scope.GetSalesMaterialData($scope.salesVM.Id);
                            $scope.GetSalesPackingData($scope.salesVM.Id);
                            $scope.getPostSalesData();

                            $scope.getTaxCodeByTaxYearWithhold($scope.salesVM.InvoiceDate);

                            $scope.Action = "Update";
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                }
                return true;
            }
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.salesVM.Id)) {
            $http({
                method: 'POST',
                url: 'Productions/PackingInvoice/Delete?Id=' + $scope.salesVM.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.removeMaterialRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.salesOrderList.splice(index, 1);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
            $scope.mateId = Id;
            $scope.mateIndex = index;
        }
    };

    $scope.cancelMaterialRow = function (Id, index) {
        angular.element(document.querySelector('#cancelPopUp')).modal('show');
        $scope.mateId = Id;
        $scope.mateIndex = index;
    };
    $scope.CancelRemark = null;
    $scope.closeCancelMaterialRow = function () {
        angular.element(document.querySelector('#cancelPopUp')).modal('hide');
        $scope.mateId = null;
        $scope.mateIndex = null;
    };
    $scope.removeServiceRow = function (Id, index) {
        if (Id === null) {
            $(this).remove();
            $scope.chargesList.splice(index);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#removeServicePopUp')).modal('show');
            $scope.serId = Id;
            $scope.serIndex = index;
        }
    };

    $scope.serviceDelete = function () {
        try {
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/DeleteSalesService?Id=' + $scope.serId,
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serId = null;
                    $scope.chargesList.splice($scope.serIndex, 1);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.detailDelete = function () {
        try {

            if (!baseService.isUndefinedOrNull($scope.mateId)) {
                $http({
                    method: 'POST',
                    url: 'SalesManagements/Sales/DeleteSalesMaterial?Id=' + $scope.mateId,
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.mateId = null;
                        $scope.salesMaterialList.splice($scope.mateIndex, 1);
                        $scope.GetSalesMaterialData($scope.salesVM.Id);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            } else {
                $scope.salesMaterialList.splice($scope.mateIndex, 1);
            }

        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.cancelMaterialRow = function () {
        try {

            if (!baseService.isUndefinedOrNull($scope.mateId)) {
                $http({
                    method: 'POST',
                    url: 'SalesManagements/Sales/CancelSalesMaterial?Id=' + $scope.mateId + '&remark=' + $scope.CancelRemark,
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.mateId = null;
                        $scope.GetSalesMaterialData($scope.salesVM.Id);
                        $scope.closeCancelMaterialRow();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }

        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.taxDel = function (Id, index) {
        if (Id === null) {
            $(this).remove();
            $scope.receiveTaxList.splice(index);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#confirmTaxCodeDelPopUp')).modal('show');
            $scope.metTaxId = Id;
            $scope.smetTaxIndex = index;
        }
    };

    $scope.removeTaxCodeRow = function () {
        try {
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/DeleteTaxRow?Id=' + $scope.metTaxId,
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.metTaxId = null;
                    $scope.receiveTaxList.splice($scope.smetTaxIndex, 1);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.taxSerDel = function (Id, index) {
        if (Id === null) {
            $(this).remove();
            $scope.ServicetaxPopList.splice(index);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#confirmTaxServiceDelPopUp')).modal('show');
            $scope.serTaxId = Id;
            $scope.serTaxIndex = index;
        }
    };

    $scope.removeServiceTaxRow = function () {
        try {
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/DeleteServiceTaxRow?Id=' + $scope.serTaxId,
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serTaxId = null;
                    $scope.ServicetaxPopList.splice($scope.serTaxIndex, 1);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };


    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        //$scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + $window.companyId + '&PlantId=' + $window.plantId;
        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('show');
    };

    $scope.SetCustomerData = function (obj) {
        var party = obj.data;
        $scope.salesVM.PartyName = party.UserName;
        $scope.salesVM.PartyId = party.Id;
        $scope.salesVM.PaymentTermId = party.PaymentTermId;
        $scope.salesVM.CurrencyId = party.CurrencyId;
        $scope.GetCurrencyExchangeRateList();
        $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
        $scope.partyPlantList = [];
        $scope.getCboPartyPlantList(party.Id, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = item.Value;
                    $scope.salesVM.InvoicingByAddress = item.Address1;
                    $scope.salesVM.DeliveryByAddress = item.Address1;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = item.StateName;
                    $scope.salesVM.DeliveryGSTIN = item.GSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });
        });
        $scope.partyType = "Customer";
        $scope.flag = null;
        $scope.hidePartyPopUp();
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }

    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.hidePartyPopUp();
        $scope.partyType = "Customer";
        $scope.searchParty = '';
    }

    //#region Additional TAX Code
    $scope.advanceTax = { TotalSumAfterTCSVal: 0 };
    $scope.advanceTaxesList = [];
    $scope.additionalTax = function () {
        for (var i = 0; i < $scope.advanceTaxesList.length; i++) {
            if ($scope.advanceTaxesList[i].TaxCodeId === $scope.advanceTax.TaxCodeId) {
                ShowResult("Tax Already Added");
                return false;
            }

        }

        if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboListWithhold, function (item) {
                return item.Id === $scope.advanceTax.TaxCodeId;
            })[0].UserName;

            $scope.advanceTaxesList.push($scope.advanceTax);
            $scope.advanceTax = {};
            $scope.TotalSumAfterTCS();
        }

    };

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $scope.salesVM.TaxOptionAddiTax = 'Yes';
        $http({
            method: "Get",
            url: "accounts/TaxCode/GetAdditionalTaxOutputCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    $scope.taxCodCboListWithhold = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };
    //$scope.getTaxCodeByTaxYearWithhold($filter("dateFiltering")(Date.now()));
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        $scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].TaxCategoryId;

        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100

            $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
            //$scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
        }
        else {
            $scope.advanceTax.TaxAmount = $scope.advanceTax.ValueOfFixed;
        }
        $scope.TotalSumAfterTCS();
    }

    $scope.SaveAdditinalTax = function () {
        try {
            if ($scope.salesVM.IsPark == 0) {
                throw "Posted data cann't save";
            }
            if (baseService.arrayLength($scope.advanceTaxesList) == 0) {
                throw "Add row for Additional Tax.";
            }
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/SaveAdditinalTax',
                data:
                {
                    'salesId': $scope.salesVM.Id,
                    'BooksCurrencyBaseRate': $scope.salesVM.CompanyCurrencyRate,
                    'UserSendData': $scope.advanceTaxesList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TotalSumAfterTCS();

                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetAdvanceTaxInfo = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'SalesManagements/Sales/GetAdvanceTaxInfo?SalesId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;

            $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

        });

    }
    $scope.removeTaxesRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.advanceTaxesList.splice(index, 1);

        }
        else {
            $scope.DeleteAdditinalTax(Id);
            $scope.GetAdvanceTaxInfo($scope.salesVM.Id);
        }
    };
    $scope.DeleteAdditinalTax = function (Id) {
        $http({
            method: 'POST',
            url: 'SalesManagements/Sales/AdditionalTaxDelete?Id=' + Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.TaxOptionAdditax = function (data) {
        $scope.salesVM.TaxOptionAddiTax = data;
    };

    $scope.calculateTaxAmountForAdditionalTax = function (data) {
        $scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax"))).toFixed(2);
        //$scope.TaxAmountVal = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
        $scope.advanceTax.TaxAmount = (($scope.TaxAmountVal * data) / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {

        $scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax"))).toFixed(2);
        $scope.advanceTax.ValueOfFixed = ((data / $scope.TaxAmountVal1) * 100).toFixed(4);
    }
    //$scope.TotalSumAfterTCSVal = "";
    $scope.TotalSumAfterTCS = function () {

        $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.salesOrderList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
    }

    //#endregion

    //#region PostInvoice

    $scope.ModelList = [];
    $scope.path = 'Commercial/PostSalesInvoice/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.PostSalesInvoicedeleteUrl = 'Commercial/PostSalesInvoice/delete/';
    $scope.Action = 'Save';
    // $scope.partyType = "Vendor";

    $scope.getPostSalesData = function () {
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

        $http.get("Commercial/PostSalesInvoice/GetListBySalesId?SalesId=" + $scope.salesVM.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ModelNew = Object.assign({}, response.data[0]);
                    }
                    $scope.ModelNew.SalesId = $scope.salesVM.Id;
                    $scope.ModelNew.InvoiceDate = $scope.salesVM.InvoiceDate;
                    $scope.ModelNew.InvoiceNo = $scope.salesVM.InvoiceNo;
                    $scope.ModelNew.ContractNo = $scope.salesVM.ContractNo;
                    $scope.ModelNew.PartyName = $scope.salesVM.PartyName;
                    $scope.ModelNew.Amount = $scope.salesVM.Amount;

                    //if (baseService.arrayLength($scope.bankMasterList) > 0 && !baseService.isUndefinedOrNull($scope.salesVM.BenificiaryBankId)) {
                    //    for (var i = 0; i < $scope.bankMasterList.length; i++) {
                    //        if ($scope.bankMasterList[i].Id === $scope.salesVM.BenificiaryBankId) {
                    //            $scope.ModelNew.BankMasterId = $scope.bankMasterList[i].Id;
                    //        }
                    //    }
                    //}

                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }


    $scope.portList = [];
    cboService.getPortByPlantCbo(function (result) {
        $scope.portList = result;
    });


    $scope.deliveryPortList = [];
    cboService.getPortCbo(function (result) {
        $scope.deliveryPortList = result;
    });

    $scope.bankMasterList = [];
    bankService.GetNegotiatingBankMasterCboListByPlant(function (result) {
        $scope.bankMasterList = result;

    });

    $scope.PaymentToReceiveBankList = [];
    bankService.getBankMasterCboListByPlant(function (result) {
        $scope.PaymentToReceiveBankList = result;

    });

    $scope.shipmentModeList = [];
    $scope.getShipmode = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/shipmode/GetCbo/'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.shipmentModeList = response.data;
            }
        });
    };
    $scope.getShipmode();



    $scope.dischargePortList = [];
    $scope.GetPortOfDischargeByDstination = function () {
        $http({
            method: 'GET',
            url: 'Commercial/PostSalesInvoice/GetPortByDestinationCbo?destinationId=' + $scope.ModelNew.FinalDestinationId
        }).then(function successCallback(response) {
            $scope.dischargePortList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.dischargePortList = response.data;
            }
        });
    };


    $scope.ModelTemp = {
        Id: null,
        SalesId: null,
        InvoiceDate: null,
        BankMasterId: null,
        ShipmentModeId: null,
        PortOfLoadingId: null,
        ExpFormNo: null,
        ExpDate: null,
        CargoNetWt: null,
        CargoGrossWt: null,
        Dimension: null,
        ExFactoryDocRef: null,
        ExFactoryDate: null,
        TransportAgentId: null,
        TransportDocRefNo: null,
        TransportDocDate: null,
        TransportVehicleNo: null,
        TransportDriverName: null,
        TransportDriverNo: null,
        PreCarriageBy: null,
        PlaceOfReceiptByPreCarriage: null,
        PreCarriageDocRef: null,
        PreCarriageDocDate: null,
        CNFAgentId: null,
        CNFContainerNo: null,
        CNFVesselTrackingNo: null,
        CNFVesselName: null,
        CNFVesselSalesDetails: null,
        CNFBLAWB: null,
        CNFBLAWBDate: null,
        ETA: null,
        FinalDestinationId: null,
        PortOfDischargeId: null,
        PortOfDelivaryId: null,
        BankDocRef: null,
        BankDocDate: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.destinationList = [];
    $scope.getDestination = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/destination/GetCbo'
        }).then(function successCallback(response) {
            $scope.destinationList = response.data;
        });
    };
    $scope.getDestination();

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "" + fieldname + " is required.";
            }
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("Invoice No", $scope.ModelNew.InvoiceNo);
            CheckField("Customer", $scope.salesVM.PartyName);
            CheckField("Bank", $scope.ModelNew.BankMasterId);
            CheckField("ExFactory Date", $scope.ModelNew.ExFactoryDate);
            CheckField("Shipment Mode", $scope.ModelNew.ShipmentModeId);
            CheckField("Port of Loading", $scope.ModelNew.PortOfLoadingId);
            CheckField("Final Destination", $scope.ModelNew.FinalDestinationId);
            CheckField("Port Of Discharge", $scope.ModelNew.PortOfDischargeId);
            CheckField("Port Of Delivery", $scope.ModelNew.PortOfDelivaryId);
            CheckField("Transport Agent", $scope.ModelNew.TransportAgentId);
            CheckField("Transport Doc Ref No.", $scope.ModelNew.TransportDocRefNo);
            CheckField("Pre-CarriageBy", $scope.ModelNew.PreCarriageBy);
            CheckField("Place Of Receipt", $scope.ModelNew.PlaceOfReceiptByPreCarriage);
            CheckField("Pre-Carriage Doc Ref No.", $scope.ModelNew.PreCarriageDocRef);
            CheckField("Pre-Carriage DocDate", $scope.ModelNew.PreCarriageDocDate);
            CheckField("CNF Agent", $scope.ModelNew.CNFAgentId);
            CheckField("Container No", $scope.ModelNew.CNFContainerNo);
            CheckField("Vessel Tracking No", $scope.ModelNew.CNFVesselTrackingNo);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.SavePostSales = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.ExpDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.ExpDate)) {
                    throw "Expected Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.ExpDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.ExFactoryDate)) {
                    throw "ExFactory Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.CNFBLAWBDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.CNFBLAWBDate)) {
                    throw "BL Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.BankDocDate)) {
                if (new Date($scope.ModelNew.CNFBLAWBDate) < new Date($scope.ModelNew.BankDocDate)) {
                    throw "Bank Doc Date should greater than BL Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.ETA)) {
                if (new Date($scope.ModelNew.CNFBLAWBDate) < new Date($scope.ModelNew.ETA)) {
                    throw "ETA Date should greater than BL Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.TransportDocDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.TransportDocDate)) {
                    throw "Transport Doc Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.PreCarriageDocDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.PreCarriageDocDate)) {
                    throw "Pre-Carriage Doc Date should greater than Invoice Date";
                }
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.TransportDriverNo)) {
                ShowResult("Transport Driver Number should not be blank");
                throw "Transport Driver Number should not be blank";
            }
            //ValidationMaster();
            $scope.ModelNew.SalesId = $scope.salesVM.Id;
            //$scope.$broadcast('show-errors-check-validity');
            //if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'entity': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Id;
                    // $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

            //}
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.DeletePostSales = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.PostSalesInvoicedeleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelList = [];
                    ClearPostSalesFields();
                    //$scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearPostSales = function () {
        ClearPostSalesFields();
        return true;
    };

    function ClearPostSalesFields() {
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.showVendorPopUp = function (flg) {
        $scope.flag = flg;
        $scope.GetVendorPopUpData();
        angular.element(document.querySelector('#vendorPopUp')).modal('show');
    };

    $scope.GetVendorPopUpData = function () {
        if ($scope.flag === 'Transport' || $scope.flag === 'CNF' || $scope.flag === 'Forwarder') {
            $scope.partyType = 'Vendor';
        }

        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
    };


    $scope.closevendorPopUpNew = function () {
        angular.element(document.querySelector('#vendorPopUp')).modal('hide');
        $scope.hidePartyPopUp();
        //$scope.partyType = "Customer";
    }

    $scope.SetVendorData = function (obj) {
        if ($scope.flag === 'CNF') {
            var party = obj.data;
            $scope.ModelNew.CNFAgentId = party.Id;
            $scope.ModelNew.CNFAgentCode = party.Code;
            $scope.ModelNew.CNFAgentName = party.UserName;
        }
        else if ($scope.flag === 'Transport') {
            var party = obj.data;
            $scope.ModelNew.TransportAgentId = party.Id;
            $scope.ModelNew.TransportAgentCode = party.Code;
            $scope.ModelNew.TransportAgentName = party.UserName;
        }
        else {
            var party = obj.data;
            $scope.ModelNew.TransporterCHAForwarderId = party.Id;
            $scope.ModelNew.TransporterCHAForwarder = party.UserName;
        }
        $scope.searchByParty = "UserName"; $scope.searchParty = "";
        angular.element(document.querySelector('#vendorPopUp')).modal('hide');
    }



    //#endregion PostInvoice

    //#region  GetInvoiceReport

    $scope.GetLotWiseTaxInvoice = function (data) {
        location.href = "SalesManagements/Sales/GetLotWiseTaxInvoice?salesId=" + data.Id;
    };

    $scope.LocalTaxInvoiceReport = function (data) {
        location.href = "SalesManagements/Sales/LocalTaxInvoice?salesId=" + data.Id;
    };
    $scope.LocalTaxInvoiceWithoutSUIReport = function (data) {
        location.href = "Sales/LocalTaxInvoiceWithoutSKU?salesId=" + data.Id;
    };
    $scope.LocalTaxInvoiceWithProductDetailService = function (data) {
        location.href = "Sales/LocalTaxInvoiceWithProductDetailService?salesId=" + data.Id;
    };
    $scope.CommercialInvoiceReport = function (data) {
        location.href = "SalesManagements/Sales/CommercialInvoice?salesId=" + data.Id;
        //  $scope.CommercialInvoicePackingListReport(data);
    };
    $scope.LRDraft = function (data) {
        location.href = "SalesManagements/Sales/LRDraft?salesId=" + data.Id;
        //  $scope.CommercialInvoicePackingListReport(data);
    };
    $scope.BillofExchange = function (data) {
        location.href = "SalesManagements/Sales/BillofExchange?salesId=" + data.Id;
        //  $scope.CommercialInvoicePackingListReport(data);
    };
    $scope.CertificateofOrigin = function (data) {
        location.href = "SalesManagements/Sales/CertificateofOrigin?salesId=" + data.Id;
        //  $scope.CommercialInvoicePackingListReport(data);
    };

    $scope.BeneficiaryCertificate = function (data) {
        location.href = "SalesManagements/Sales/BeneficiaryCertificate?salesId=" + data.Id;
        //  $scope.CommercialInvoicePackingListReport(data);
    };

    $scope.BankLatter = function (data) {
        try {
            if (baseService.isUndefinedOrNull(data.BankName))
                throw "Bank Is Not Selected";
            location.href = "SalesManagements/Sales/BankLatter?salesId=" + data.Id + '&BankName=' + data.BankId;

        }
        catch (e) {
            ShowResult(e, 'failure');
        }

        //  $scope.CommercialInvoicePackingListReport(data);
    };
    $scope.InsuranceCoverLetter = function (data) {
        location.href = "SalesManagements/Sales/InsuranceCoverLetter?salesId=" + data.Id;
        //  $scope.CommercialInvoicePackingListReport(data);
    };
    $scope.ANNEXUREReport = function (data) {
        location.href = "SalesManagements/Sales/ANNEXUREReport?salesId=" + data.Id;
        //  $scope.CommercialInvoicePackingListReport(data);
    };
    $scope.CommercialInvoicePackingListReport = function (data) {
        location.href = "SalesManagements/Sales/CommercialInvoicePackingList?salesId=" + data.Id;
    };

    //$scope.SendMailToParty = function (data) {
    //    location.href = "SalesManagements/Sales/CommercialInvoice?salesId=" + data.Id;
    //};

    $scope.SendMailToParty = function (args) {
        try {
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/SendMailInvoiceReport',
                params: {
                    'salesId': args.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //#endregion


    // #region checkbox all for AdditionalInfo

    $scope.SalesAdditionalInfoList = [];

    $scope.GetSalesAdditionalInfoList = function () {

        $http({
            method: 'GET',
            url: 'Productions/PackingInvoice/GetAdditionalInfoList?SalesId=' + $scope.salesVM.Id
        }).then(function successCallback(response) {
            $scope.SalesAdditionalInfoList = response.data;
            $scope.GetSalesAdditionalInfoData();
        });
    }

    $scope.searchdata = [];
    $scope.GetAdditionalInfoList = function () {
        $scope.searchdata = [];
        $http({
            method: 'GET',
            url: 'Commercial/CommercialAdditionalInfo/GetCommercialAdditionalInfo'
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
        });
    }

    $scope.AddAdditional = function () {
        $scope.GetAdditionalInfoList();
        $scope.ShowResultCustom();
    }

    $scope.ShowResultCustom = function (message, type) {
        $("#AdditionalInfoPoUp").ejDialog("setTitle", "Terms And Conditions");
        var eDialog = $("#AdditionalInfoPoUp").data("ejDialog");
        eDialog.open();

        var gridObj = $("#GridAdditionalInfo").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering

    };


    $scope.refreshTemplateAdditionalInfo = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridAdditionalInfo").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                $scope.searchdata[i].Flag = ChkOrUnchk;
            }

        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAdditionalInfo").data("ejGrid");
        gridObj.refreshContent();

    };

    function MakeAdditionalInfoData() {

        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Flag == true) {
                if (checkExists($scope.SalesAdditionalInfoList, $scope.searchdata[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.AdditionalInfoId = $scope.searchdata[i].Id;
                    ob.SalesId = $scope.salesVM.Id;
                    ob.Sequence = $scope.searchdata[i].Sequence;
                    ob.Code = $scope.searchdata[i].Code;
                    ob.ShortName = $scope.searchdata[i].ShortName;
                    ob.StandardName = $scope.searchdata[i].StandardName;
                    ob.UserName = $scope.searchdata[i].UserName;
                    ob.Description = $scope.searchdata[i].Description;

                    $scope.SalesAdditionalInfoList.push(ob);
                }

            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].AdditionalInfoId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseAdditionalInfo = function () {
        try {
            MakeAdditionalInfoData();
            $scope.SaveAdditionalInfo();
            var eDialog = $("#AdditionalInfoPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveAdditionalInfo = function () {
        try {
            $http({
                method: 'POST',
                url: 'Productions/PackingInvoice/CreateAdditionalInfo',
                data: {
                    'data': $scope.SalesAdditionalInfoList
                    , 'salesId': $scope.salesVM.Id
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetContractTermsAndConditionsList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.message_detailconfirmation = null;
    $scope.removeBoMDetail = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    }

    $scope.DeleteAddInfo = function () {
        $http({
            method: 'POST',
            url: 'Productions/PackingInvoice/DeleteCommercialInvoiceAdditionalInfo?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSalesAdditionalInfoList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion checkbox all

    //#region Sales File upload

    $scope.onBeginPBUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull($scope.ModelNew.Id))
                throw 'Please select/save Post Sales Invoice first'

            args.data = $scope.ModelNew.Id;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "Commercial/PostSalesInvoice/SavePostSaleFile";
    $scope.fileselect = function (e) {

    }
    $scope.errorPBPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ModelNew.Id))
            ShowResult('Please select/save the production order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }
    $scope.getFileList = function () {
        $http({
            method: 'POST', url: 'Commercial/PostSalesInvoice/GetFileInfo', dataType: 'JSON',
            data: { Id: $scope.ModelNew.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                var str = response.data[0].FileName;
                $scope.ModelNew.FileName = response.data[0].FileName;
                var extention = str.substr(str.indexOf('.'));
                $scope.FileName = virtualPath.PostSalesInvoiceDoc + '/' + $scope.ModelNew.Id + extention;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    $scope.FileNam = null;
    $scope.tempdata = {};
    $scope.DocDownload = function (data) {
        $scope.tempdata = data;
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        $scope.FileNam = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.PostSalesInvoiceDoc + '/' + data.Id + extention;
        angular.element(document.querySelector('#DocShowPopUp')).modal('show');
    };

    $scope.DownloadImageFile = function () {
        var str = $scope.tempdata.FileName;
        $scope.FileNam = $scope.tempdata.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.PostSalesInvoiceDoc + '/' + $scope.tempdata.Id + extention;
    };
    //#endregion Production Bulletin Picture upload



    $scope.monthList = [
        { 'Value': "1", 'Text': "Jan", 'Days': 31 },
        { 'Value': "2", 'Text': "Feb", 'Days': 28 },
        { 'Value': "3", 'Text': "Mar", 'Days': 31 },
        { 'Value': "4", 'Text': "Apr", 'Days': 30 },
        { 'Value': "5", 'Text': "May", 'Days': 31 },
        { 'Value': "6", 'Text': "Jun", 'Days': 30 },
        { 'Value': "7", 'Text': "Jul", 'Days': 31 },
        { 'Value': "8", 'Text': "Aug", 'Days': 31 },
        { 'Value': "9", 'Text': "Sep", 'Days': 30 },
        { 'Value': "10", 'Text': "Oct", 'Days': 31 },
        { 'Value': "11", 'Text': "Nov", 'Days': 30 },
        { 'Value': "12", 'Text': "Dec", 'Days': 31 }
    ];

    function validatedate(dateText) {

        if (dateText) {
            try {
                var errorMessage = "";
                var monthNO = 0;
                var daysPerMonth = 0;
                var splitComponents = dateText.split('-');
                if (splitComponents.length > 0) {
                    var day = parseInt(splitComponents[0]);
                    var month = splitComponents[1];
                    var year = parseInt(splitComponents[2]);

                    if (isNaN(day) || isNaN(year)) {
                        errorMessage = "Please enter the date in dd-MMM-yyyy format.";
                        throw errorMessage;
                        return false;
                    }

                    var monthName = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                    if (monthName.includes(month)) {
                        for (var i = 0; i < $scope.monthList.length; i++) {
                            if ($scope.monthList[i].Text == month) {
                                monthNO = $scope.monthList[i].Value;
                                daysPerMonth = $scope.monthList[i].Days;
                                break;
                            }
                        }
                    }
                    else {
                        throw "Invalid Month Name.";
                    }

                    if (day <= 0 || year <= 0) {
                        throw "The day and year need to be positive values greater than 0";
                    }

                    if (errorMessage == "") {
                        // assuming no leap year by default
                        //var daysPerMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
                        if (year % 4 == 0) {
                            // current year is a leap year
                            daysPerMonth = 29;
                        }

                        if (day > daysPerMonth) {
                            errorMessage = "Number of days are more than those allowed for the month";
                        }
                    }
                } else {
                    throw errorMessage = "Please enter the date in dd-MMM-yyyy format.";
                }

                if (errorMessage) {
                    throw errorMessage;
                    return false;
                }
            } catch (e) {
                throw e;
                return false;
            }
        }

        return true;
    }


    $scope.SaveAddInfo = function () {
        try {
            for (var i = 0; i < $scope.SalesAdditionalInfoDataList.length; i++) {
                if ($scope.SalesAdditionalInfoDataList[i].Flag) {
                    if (baseService.isUndefinedOrNull($scope.SalesAdditionalInfoDataList[i].Value)) {
                        throw "Value is required for " + $scope.SalesAdditionalInfoDataList[i].UserName + ".";
                    }
                }

                if ($scope.SalesAdditionalInfoDataList[i].CharecterType == "DateTime") {
                    validatedate($scope.SalesAdditionalInfoDataList[i].Value);
                }


                if ($scope.SalesAdditionalInfoDataList[i].CharecterType == "Decimal") {
                    if (isNaN($scope.SalesAdditionalInfoDataList[i].Value)) {
                        throw "Number is required for " + $scope.SalesAdditionalInfoDataList[i].UserName + ".";
                    }
                }
            }


            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/CreateSalesAdditionalInfo',
                data: {
                    'data': $scope.SalesAdditionalInfoDataList,
                    'salesId': $scope.salesVM.Id
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSalesAdditionalInfoData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SalesAdditionalInfoDataList = [];
    $scope.GetSalesAdditionalInfoData = function () {
        $scope.SalesAdditionalInfoDataList = [];
        $http.get("SalesManagements/Sales/GetSalesAdditionalInfoData?salesId=" + $scope.salesVM.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        for (var i = 0; i < response.data.length; i++) {
                            response.data[i].SalesId = $scope.SalesId;

                            if (response.data[i].CharecterType == "Text" || response.data[i].CharecterType == "DateTime") {
                                response.data[i].CharType = "text";
                            }
                            else {
                                response.data[i].CharType = "number";
                            }
                            if (response.data[i].CharecterType == "DateTime") {
                                response.data[i].datepic = 'datepicker';
                            }
                        }

                        $scope.SalesAdditionalInfoDataList = response.data;
                        $scope.PTData();
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.PTData = function () {
        $scope.paymentTermList = [];
        $http({
            method: "GET",
            url: "accounts/PaymentTerm/getcustomercbo"
        }).then(function successCallback(response) {
            var ObjPt = {};
            for (var i = 0; i < response.data.length; i++) {
                if (response.data[i].Value == $scope.salesVM.PaymentTermId) {
                    ObjPt.Value = response.data[i].Value;
                    ObjPt.Text = response.data[i].Text;
                    $scope.paymentTermList.push(ObjPt);
                    ObjPt = {};
                    break;
                }
            }
        });
    }

    $scope.PISVM = {//Production Inventory Sales 
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        CurrencyId: null,
        EntityId: null,
        ItemDescription: null,
        PartyType: "Customer",
        InvoiceDate: $filter("dateFiltering")(Date.now()),
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        Amount: 0,
        BankAmount: 0,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        CompanyCurrencyRate: 1,
        InvoicingPartyPlantId: null,
        DeliveryPartyPlantId: null,
        InvoicingByAddress: null,
        DeliveryByAddress: null,
        InvoicingState: null,
        InvoicingGSTIN: null,
        DeliveryState: null,
        DeliveryGSTIN: null,
        BLNumber: null,
        LCNumber: null,
        ComercialInvoiceNo: null,
        EXPFromNo: null,
        SourceType: 'Packing',
        ContractId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes',
        BooksCurrencyTransactionAmount: null,
        BooksCurrencyTaxAmount: null,
        BooksCurrencyBaseRate: null,
        IsPark: 1,
        IsAdditionalInfoApplicable: true,
        IsIncentiveApplicable: false,
        InvoiceStatus: 'Active',
        PaymentToReceiveBankId: null
    };

    $scope.NewEntityList = [];
    $scope.GetEntityPlantWise = function () {
        var PLT = $window.plantId
        $http({
            method: 'GET',
            url: 'Outsourcing/OSTransformationPO/GetAllEntity?PlantId=' + PLT
        }).then(function successCallback(response) {
            $scope.NewEntityList = response.data;

        });
    }


    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.PISNew.ProcessId = $scope.processList[0].Value;
            }
        });
    };

    $scope.ProductionOrderList = [];
    $scope.PRSearchColumn = null;
    $scope.PRSearchValue = null;
    $scope.GetProductionOrderPopUp = function () {
        if (!baseService.isUndefinedOrNull($scope.PISNew.EntityId)) {
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.PISNew.EntityId, 'processid': $scope.PISNew.ProcessId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: 'Outsourcing/OSTransformationPO/GetProductionOredrList'
            }).then(function successCallback(response) {
                $scope.ProductionOrderList = response.data;
                angular.element(document.querySelector('#POItemPopup')).modal('show');
            });
        }
    };
    $scope.selectedProductionOrder = [];
    $scope.SetPrOData = function () {
        var gridObj = $("#GridPO").data("ejGrid");
        $scope.selectedProductionOrder.push(gridObj.getSelectedRecords()[0]);
        $scope.GetProductionOrderSOList(gridObj.getSelectedRecords()[0].POId);
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }

    $scope.removeSelectedPO = function (x, index) {
        $scope.selectedProductionOrder.splice(index, 1);
    }

    $scope.ProductionOrderSOList = [];
    $scope.GetProductionOrderSOList = function (productionOrderId) {
        $scope.tempPOSOList = [];
        $http({
            method: 'POST',
            data: { 'productionOrderId': productionOrderId },
            url: 'Productions/PackingInvoice/GetProductionOrderSOList'
        }).then(function successCallback(response) {
            $scope.tempPOSOList = response.data
            for (var i = 0; i < $scope.tempPOSOList.length; i++) {
                $scope.ProductionOrderSOList.push($scope.tempPOSOList[i]);
            }
        });
    };

    $scope.calculateAmounts = function (data) {
        if (data.Balance < data.TransactionQty) {
            data.TransactionQty = '';
            ShowResult("Receive Qty can not greater than Balance Qty", 'failure', 'GridPOSO');
        }
        var gridObj = $("#GridPOSO").data("ejGrid");
        data.Amount = parseFloat(data.TransactionQty * data.TransactionRate).toFixed(2);
        gridObj.refreshContent();
    }
}