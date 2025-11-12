"use strict";
masterOrderSalesController.$inject = ["cboService", "commonMessage", '$window', "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService", "bankService"];
function masterOrderSalesController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $http, $filter, $controller, accountService, bankService) {
    $rootScope.title = "Master Order Sales";
    $scope.savebtndisable = false;
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.advanceTaxesList = [];
    $scope.voucherList = [];
    $scope.partyType = "Customer";
    $scope.isAdvance = false;
    $scope.salesMaterialList = [];
    $scope.deleteUrl = 'SalesManagements/Sales/delete/';
    $scope.postUrl = 'SalesManagements/Sales/MasterOrderSalesPost';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("baseMaterialAndArticleController", { $scope: $scope, $http: $http });

    baseService.init("SalesManagements/Sales/GetMasterOrderSalesDataList", null, null, "DESC", "AddedDate", "InvoiceNo");
    //baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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

    $scope.getcompanyState = function (addressMasterId) {
        $http.get('Addresses/AddressMaster/GetCompanyState?addressMasterId=' + addressMasterId)
            .then(function (response) {
                $scope.salesVM.PlantStateId = response.data.StateId;
            });
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        $scope.getcompanyState($scope.companyConfig.AddressMasterId);
    });

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    cboService.getCboSalesType(function (result) {
        $scope.salesTypeList = result;
    });

    cboService.getCboEntityByPlant(null, null, " ", function (result) {
        $scope.entityList = result;
    });

    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.taxCategoryList = result;
    });

    $scope.getCboSalesOrganisationByPlant = function (plantId) {
        cboService.getCboSalesOrganisationByPlant(plantId, function (result) {
            $scope.salesOrganisationList = result;
        });
    };

    $http({
        method: "GET",
        url: "accounts/PaymentTerm/getcustomercbo"
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

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
        SourceType: 'MasterOrderSales',
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
        TrancastionTypeId:null
    };
    $scope.salesVM.TaxOptionAddiTax = 'Yes';

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

    $scope.GetCustomerSalesData = function (partyId, salesOrganisationId) {
        $http({
            method: "GET",
            url: "Parties/CustomerSalesData/GetCustomerSalesData?partyId=" + partyId + "&salesOrganisationId=" + salesOrganisationId
        }).then(function (response) {
            var salesData = response.data;
            if (!baseService.isUndefinedOrNull(salesData.CurrencyId)) {
                $scope.salesVM.CurrencyId = salesData.CurrencyId;
            }
            if (salesData.IsChangeable && baseService.isUndefinedOrNull(salesData.PaymentTermId)) {
                ShowResult("PaymentTerm is not define in Customer Sales data.", "failure");
                return;
            }
            $scope.salesVM.PaymentTermId = salesData.PaymentTermId;
        });
    };

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
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
            for (var i = 0; i < $scope.selectedMasterOrderItemList.length; i++) {
                var linepk = $scope.selectedMasterOrderItemList[i].Id;
                var list = gettaxlist(linepk);
                $scope.selectedMasterOrderItemList[i].TaxList = list;

            }
            $scope.GetSalesServiceData($scope.salesVM.Id);
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

    $scope.selectedMasterOrderItemTempList = [];
    $scope.GetSalesMaterialData = function (salesId) {
        $scope.selectedMasterOrderItemList = [];
        $scope.salesMaterialList = [];
        $scope.selectedMasterOrderItemTempList = [];
        $scope.uoMList = [];
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetMasterOrderSalesMaterialData?salesId=" + salesId
        }).then(function (response) {
            $scope.salesMaterialList = response.data;

            $scope.selectedMasterOrderItemTempList = response.data;
            var obj = {};
            for (var i = 0; i < $scope.selectedMasterOrderItemTempList.length; i++) {
                obj.Id = $scope.selectedMasterOrderItemTempList[i].Id;
                obj.SalesId = $scope.selectedMasterOrderItemTempList[i].SalesId;
                obj.MaterialMasterId = $scope.selectedMasterOrderItemTempList[i].MaterialMasterId;
                obj.ArticleId = $scope.selectedMasterOrderItemTempList[i].ArticleId;
                obj.TaxAmount = $scope.selectedMasterOrderItemTempList[i].TaxAmount;
                obj.NetAmount = $scope.selectedMasterOrderItemTempList[i].NetAmount;
                obj.AddedBy = $scope.selectedMasterOrderItemTempList[i].AddedBy;
                obj.AddedDate = $scope.selectedMasterOrderItemTempList[i].AddedDate;
                obj.AddedFromIP = $scope.selectedMasterOrderItemTempList[i].AddedFromIP;
                obj.UpdatedBy = $scope.selectedMasterOrderItemTempList[i].UpdatedBy;
                obj.UpdatedDate = $scope.selectedMasterOrderItemTempList[i].UpdatedDate;
                obj.UpdatedFromIP = $scope.selectedMasterOrderItemTempList[i].UpdatedFromIP;
                obj.BaseUOMId = $scope.selectedMasterOrderItemTempList[i].BaseUOMId;
                obj.BaseRate = $scope.selectedMasterOrderItemTempList[i].BaseRate;

                obj.BaseUoMFactor = $scope.selectedMasterOrderItemTempList[i].BaseUoMFactor;
                obj.TransactionUoMId = $scope.selectedMasterOrderItemTempList[i].TransactionUoMId;
                obj.TransactionRate = $scope.selectedMasterOrderItemTempList[i].TransactionRate;

                obj.VoucherDatailId = $scope.selectedMasterOrderItemTempList[i].VoucherDatailId;
                obj.FirstCharacteristicsId = $scope.selectedMasterOrderItemTempList[i].FirstCharacteristicsId;
                obj.FirstCharacteristicsValueId = $scope.selectedMasterOrderItemTempList[i].FirstCharacteristicsValueId;
                obj.SecondCharacteristicsId = $scope.selectedMasterOrderItemTempList[i].SecondCharacteristicsId;
                obj.SecondCharacteristicsValueId = $scope.selectedMasterOrderItemTempList[i].SecondCharacteristicsValueId;
                obj.ThirdCharacteristicsId = $scope.selectedMasterOrderItemTempList[i].ThirdCharacteristicsId;
                obj.ThirdCharacteristicsValueId = $scope.selectedMasterOrderItemTempList[i].ThirdCharacteristicsValueId;
                obj.MaterialGroupMasterName = $scope.selectedMasterOrderItemTempList[i].MaterialGroupMasterName;
                obj.MaterialMasterName = $scope.selectedMasterOrderItemTempList[i].MaterialMasterName;
                obj.MaterialMasterArticleName = $scope.selectedMasterOrderItemTempList[i].MaterialMasterArticleName;
                obj.BaseUoM = $scope.selectedMasterOrderItemTempList[i].BaseUoM;
                obj.TransactionUoM = $scope.selectedMasterOrderItemTempList[i].TransactionUoM;
                obj.Currency = $scope.selectedMasterOrderItemTempList[i].Currency;
                obj.TaxList = $scope.selectedMasterOrderItemTempList[i].TaxList;
                obj.ValueFreeText = $scope.selectedMasterOrderItemTempList[i].ValueFreeText;
                obj.FreeText = $scope.selectedMasterOrderItemTempList[i].FreeText;
                obj.SecondCharacteristicsValue = $scope.selectedMasterOrderItemTempList[i].SecondCharacteristicsValue;
                obj.ThirdCharacteristicsValue = $scope.selectedMasterOrderItemTempList[i].ThirdCharacteristicsValue;
                obj.FirstCharacteristicsId = $scope.selectedMasterOrderItemTempList[i].FirstCharacteristicsId;
                obj.ThirdCharacteristicsValue = $scope.selectedMasterOrderItemTempList[i].ThirdCharacteristicsValue;
                obj.FCH = $scope.selectedMasterOrderItemTempList[i].FCH;
                obj.SKU1 = $scope.selectedMasterOrderItemTempList[i].SKU1;
                obj.SCH = $scope.selectedMasterOrderItemTempList[i].SCH;
                obj.SKU2 = $scope.selectedMasterOrderItemTempList[i].SKU2;
                obj.SecondCharacteristicsId = $scope.selectedMasterOrderItemTempList[i].SecondCharacteristicsId;
                obj.SecondCharacteristicsValueId = $scope.selectedMasterOrderItemTempList[i].SecondCharacteristicsValueId;
                obj.TCH = $scope.selectedMasterOrderItemTempList[i].TCH;
                obj.SKU3 = $scope.selectedMasterOrderItemTempList[i].SKU3;
                obj.ThirdCharacteristicsId = $scope.selectedMasterOrderItemTempList[i].ThirdCharacteristicsId;
                obj.ThirdCharacteristicsValueId = $scope.selectedMasterOrderItemTempList[i].ThirdCharacteristicsValueId;
                obj.MasterOrderId = $scope.selectedMasterOrderItemTempList[i].MasterOrderId;
                obj.MasterOrderItemId = $scope.selectedMasterOrderItemTempList[i].MasterOrderItemId;
                obj.SalesOrderId = $scope.selectedMasterOrderItemTempList[i].SalesOrderId;
                obj.SONo = $scope.selectedMasterOrderItemTempList[i].SONo;
                obj.BuyerReferenceNo = $scope.selectedMasterOrderItemTempList[i].BuyerReferenceNo;
                obj.PONumber = $scope.selectedMasterOrderItemTempList[i].PONumber;
                obj.DeliveryDate = $scope.selectedMasterOrderItemTempList[i].DeliveryDate;
                obj.DestinationName = $scope.selectedMasterOrderItemTempList[i].DestinationName;
                obj.SOType = $scope.selectedMasterOrderItemTempList[i].SOType;
                obj.Rate = $scope.selectedMasterOrderItemTempList[i].Rate;

                obj.BaseQty = $scope.selectedMasterOrderItemTempList[i].BaseQty;
                obj.BaseAmount = $scope.selectedMasterOrderItemTempList[i].BaseRate * $scope.selectedMasterOrderItemTempList[i].BaseQty;

                obj.SalesQty = $scope.selectedMasterOrderItemTempList[i].TransactionQty;
                obj.TransactionAmount = $scope.selectedMasterOrderItemTempList[i].Rate * $scope.selectedMasterOrderItemTempList[i].TransactionQty;
                obj.TempSalesQty = $scope.selectedMasterOrderItemTempList[i].TempSalesQty;

                obj.ExistSalesQty = $scope.selectedMasterOrderItemTempList[i].ExistSalesQty - obj.SalesQty;
                if (obj.ExistSalesQty < 0) {
                    obj.ExistSalesQty = $scope.selectedMasterOrderItemTempList[i].ExistSalesQty;
                }
                ///obj.ExistSalesQty = $scope.selectedMasterOrderItemTempList[i].ExistSalesQty;
                obj.TransactionQty = $scope.selectedMasterOrderItemTempList[i].Balance + $scope.selectedMasterOrderItemTempList[i].ExistSalesQty;

                obj.Balance = $scope.selectedMasterOrderItemTempList[i].Balance;

                obj.TaxAmount = $scope.selectedMasterOrderItemTempList[i].TaxAmount;

                obj.ServiceCharge = $scope.selectedMasterOrderItemTempList[i].ServiceCharge;
                obj.ServiceTax = $scope.selectedMasterOrderItemTempList[i].ServiceTax;

                $scope.selectedMasterOrderItemList.push(obj);
                obj = {};
            }

            for (var i = 0; i < $scope.salesMaterialList.length; i++) {
                $scope.getAllTransactionUoM($scope.salesMaterialList[i].MaterialMasterId);
            }

            $scope.GetSalesTaxData(salesId);
            $scope.GetAdvanceTaxInfo($scope.salesVM.Id);
            //$scope.TotalSumAfterTCS();
        });
    };

    $scope.CalculateBalance = function (data) {
        // $scope.indx = indx;
        data.TaxAmount = 0;
        data.Balance = data.TransactionQty.toFixed(2) - (data.ExistSalesQty + data.SalesQty);
        if (data.Balance >= 0) {
            if (!baseService.isUndefinedOrNull(data.Id)) {
                data.Balance = data.TransactionQty.toFixed(2) - (data.ExistSalesQty + data.SalesQty);
                data.TransactionAmount = parseFloat(data.Rate * data.SalesQty).toFixed(2);
            } else {
                data.Balance = data.TransactionQty.toFixed(2) - (data.ExistSalesQty + data.SalesQty);
                data.TransactionAmount = parseFloat(data.Rate * data.SalesQty).toFixed(2);
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
        } else {
            data.SalesQty = 0;
            ShowResult("Balance cann't less than 0.");
        }
    }
    $scope.indx = -1;


    $scope.GetSalesServiceData = function (salesId) {
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetSalesServiceData?salesId=" + salesId
        }).then(function (response) {
            $scope.chargesList = response.data;
            $scope.GetSalesServiceTaxData(salesId);
        });
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.salesVM.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
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
                }
            });
        });
    }
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

    $scope.Get = function (data) {
        $scope.salesVM = data;
        $scope.salesVM.BaseOnDueDate = $filter('dateFiltering')(new Date($scope.salesVM.BaseOnDueDate), 'dd-MM-yyyy');
        $scope.salesVM.EXPDate = $filter('dateFiltering')(new Date($scope.salesVM.EXPDate), 'dd-MM-yyyy');
        $scope.salesVM.AddedDate = $filter('dateFiltering')(new Date($scope.salesVM.AddedDate), 'dd-MM-yyyy');
        $scope.ModelNew.Amount = data.Amount;
        getPartyPlantEditList($scope.salesVM.InvoicingPartyPlantId, $scope.salesVM.InvoicingByAddress, $scope.salesVM.DeliveryPartyPlantId, $scope.salesVM.DeliveryByAddress, $scope.salesVM.DeliveryState, $scope.salesVM.DeliveryGSTIN);
        $scope.GetSalesMaterialData($scope.salesVM.Id);
        $scope.GetMasterOrderIdBySalesId($scope.salesVM.Id);
        $scope.getPostSalesData();

        $scope.getTaxCodeByTaxYearWithhold($scope.salesVM.InvoiceDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.salesVM.TaxOptionAddiTax = 'Yes';
    };

    $scope.GetMasterOrderIdBySalesId = function (salesId) {
        $scope.idList = [];
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetMasterOrderIdBySalesId?salesId=" + salesId
        }).then(function (response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.salesVM.ContractId = response.data[0].ContractId;
                $scope.salesVM.ContractNo = response.data[0].ContractNo;
                $scope.salesVM.MLCRef = response.data[0].LCRef;
                $scope.salesVM.BenificiaryBankId = response.data[0].BenificiaryBankId;
                $scope.ModelNew.Amount = response.data[0].Amount;
                if (baseService.isUndefinedOrNull($scope.ModelNew.Amount)) {
                    $scope.ModelNew.Amount = $scope.salesVM.Amount;
                }
                $scope.ModelNew.SalesId = $scope.salesVM.Id;
                $scope.ModelNew.InvoiceDate = $scope.salesVM.InvoiceDate;
                $scope.ModelNew.InvoiceNo = $scope.salesVM.InvoiceNo;
                $scope.ModelNew.ContractNo = $scope.salesVM.ContractNo;
                $scope.ModelNew.PartyName = $scope.salesVM.PartyName;

                //if (baseService.arrayLength($scope.bankMasterList) > 0) {
                //    for (var i = 0; i < $scope.bankMasterList.length; i++) {
                //        if ($scope.bankMasterList[i].Id === $scope.salesVM.BenificiaryBankId) {
                //            $scope.ModelNew.BankMasterId = $scope.bankMasterList[i].Id;
                //        }
                //    }
                //}


            }
            for (var di = 0; di < response.data.length; di++) {
                $scope.idList.push(response.data[di]);
            }

            if ($scope.idList.length > 0) {
                var uniqueMasterOrderId = removeDuplicates($scope.idList, 'MasterOrderId');
                var wcEmpCode = "";
                if (uniqueMasterOrderId.length > 0) {
                    wcEmpCode = "IN(";
                    wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.MasterOrderId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcEmpCode;
            }

            var uniqueMasterOrderItemId = removeDuplicates($scope.idList, 'MasterOrderItemId');
            var wcEmpItemCode = "";
            if (uniqueMasterOrderItemId.length > 0) {
                wcEmpItemCode = "IN(";
                wcEmpItemCode += Array.prototype.map.call(uniqueMasterOrderItemId, function (item) { return "'" + item.MasterOrderItemId + "'"; }).join(",") + ")";
            }
            $scope.sqlInItemStatement = wcEmpItemCode;
            $scope.GetMasterOrderDataByMasterOrderId($scope.sqlInStatement, $scope.sqlInItemStatement, salesId);
            getItemSOSKUList($scope.sqlInItemStatement);
        });
    };

    $scope.GetMasterOrderDataByMasterOrderId = function (MasterOrderId, MasterOrderItemId, salesId) {
        $scope.selectedMasterOrderList = [];
        $http({
            method: "GET",
            url: "SalesManagements/Sales/GetMasterOrderDataByMasterOrderId?masterOrderId=" + MasterOrderId + '&masterOrderItemId=' + MasterOrderItemId + '&salesId=' + salesId
        }).then(function (response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.selectedMasterOrderList = response.data;
            }
        });
    };

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

    $scope.getArticleList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.artData = [];
            baseService.setCurrentPage("artData");
            baseService.init("Productions/SalesOrderLinear/GetArticlListByMaterialStyle", null, null, null, "StandardName", "StandardName");
            $scope.loadArtData = function (pageno) {
                $rootScope.parameters.materialMasterId = id;
                baseService.pagination(pageno)
                    .then(function (result) {
                        $scope.artData = result;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            $scope.loadArtData();
            angular.element(document.querySelector("#articlePop")).modal("show");
        } catch (e) {
            ShowResult(e, "");
        }
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.voucherDetailList[$scope.voucherDetailList.length - 1].MaterialMasterArticleId = ob.Id;
            $scope.voucherDetailList[$scope.voucherDetailList.length - 1].MaterialMasterArticle = ob.StandardName;
            angular.element(document.querySelector("#articlePop")).modal("hide");
        } catch (e) {
            ShowResult(e, "", "articlePop");
        }
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

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.salesVM.InvoicingPartyPlantId)) {
                throw "Select Invoicing Party Plant for this Customer.";
            }

            if (baseService.isUndefinedOrNull($scope.salesVM.PaymentTermId) && $scope.BaseLineDate !== 'postingdate') {
                throw "Payment Term is required.";
            }
            if (baseService.isUndefinedOrNull($scope.salesVM.BaseOnDueDate) && $scope.BaseLineDate !== 'postingdate') {
                throw "Due Date BaseOn is required.";
            }

            $scope.BLDate = $scope.salesVM.BLDate;
            $scope.EXPDate = $scope.salesVM.EXPDate;
            $scope.BaseDate = $scope.salesVM.BaseOnDueDate;
            $scope.DData = $scope.salesVM.DocDate;
            $scope.InDate = $scope.salesVM.InvoiceDate;
            $scope.MDate = $scope.salesVM.MatureDate;
            $scope.PDate = $scope.salesVM.PostingDate;
            $scope.VDate = $scope.salesVM.VoucherDate;
            $scope.AddedDate = $scope.salesVM.AddedDate;


            if ($scope.salesVM.IsPark == 0) {
                throw "Posted data cann't save or update.";
            }
            //if (baseService.arrayLength($scope.selectedMasterOrderItemList) > 0) {
            //    for (var i = 0; i < $scope.selectedMasterOrderItemList.length; i++) {
            //        //if (parseFloat($scope.selectedMasterOrderItemList[i].TransactionQty).toFixed(2) < ($scope.selectedMasterOrderItemList[i].SalesQty + $scope.selectedMasterOrderItemList[i].ExistSalesQty + $scope.selectedMasterOrderItemList[i].Balance)) {
            //        if ($scope.selectedMasterOrderItemList[i].TransactionQty < ($scope.selectedMasterOrderItemList[i].SalesQty + $scope.selectedMasterOrderItemList[i].ExistSalesQty + $scope.selectedMasterOrderItemList[i].Balance)) {
            //            throw "Sales Qty can not be greater than Base Qty.";
            //        }
            //    }
            //}

            if (baseService.arrayLength($scope.selectedMasterOrderItemList) > 0) {
                for (var j = 0; j < $scope.selectedMasterOrderItemList.length; j++) {
                    $scope.selectedMasterOrderItemList[j].BaseRate = $scope.selectedMasterOrderItemList[j].Rate;
                    $scope.selectedMasterOrderItemList[j].TransactionRate = $scope.selectedMasterOrderItemList[j].Rate;
                    $scope.selectedMasterOrderItemList[j].Balance = $scope.selectedMasterOrderItemList[j].TransactionQty.toFixed(2) - ($scope.selectedMasterOrderItemList[j].ExistSalesQty + $scope.selectedMasterOrderItemList[j].SalesQty);

                    if ($scope.selectedMasterOrderItemList[j].Balance < 0) {
                        throw "Balance cann't less than 0.";
                    }
                }
            }

            $scope.$broadcast("show-errors-check-validity");
            if ($scope.form0.$valid) {
                $scope.savebtndisable = true;
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: "SalesManagements/Sales/MasterOrderSalesInsert",
                        data: {
                            "voucherVM": $scope.salesVM
                            , "salesMaterialVMList": $scope.selectedMasterOrderItemList
                            , "selectedMasterOrderList": $scope.selectedMasterOrderList
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
                            $scope.salesVM.AddedDate = $scope.AddedDate;

                            $scope.salesVM.BLDate = $scope.BLDate;
                            $scope.salesVM.EXPDate = $scope.EXPDate;

                            $scope.salesVM.BaseOnDueDate = $scope.BaseDate;
                            $scope.salesVM.DocDate = $scope.DData;
                            $scope.salesVM.InvoiceDate = $scope.InDate;
                            $scope.salesVM.MatureDate = $scope.MDate;
                            $scope.salesVM.PostingDate = $scope.PDate;
                            $scope.salesVM.VoucherDate = $scope.VDate;

                            getPartyPlantEditList($scope.salesVM.InvoicingPartyPlantId, $scope.salesVM.InvoicingByAddress, $scope.salesVM.DeliveryPartyPlantId, $scope.salesVM.DeliveryByAddress, $scope.salesVM.DeliveryState, $scope.salesVM.DeliveryGSTIN);
                            $scope.GetSalesMaterialData($scope.salesVM.Id);
                            $scope.GetMasterOrderIdBySalesId($scope.salesVM.Id);
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
                        url: "SalesManagements/Sales/MasterOrderSalesUpdate",
                        data: {
                            "voucherVM": $scope.salesVM
                            , "salesMaterialVMList": $scope.selectedMasterOrderItemList
                            , "selectedMasterOrderList": $scope.selectedMasterOrderList
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
                            $scope.salesVM.AddedDate = $scope.AddedDate;
                            $scope.salesVM.BLDate = $scope.BLDate;
                            $scope.salesVM.EXPDate = $scope.EXPDate;
                            $scope.salesVM.BaseOnDueDate = $scope.BaseDate;
                            $scope.salesVM.DocDate = $scope.DData;
                            $scope.salesVM.InvoiceDate = $scope.InDate;
                            $scope.salesVM.MatureDate = $scope.MDate;
                            $scope.salesVM.PostingDate = $scope.PDate;
                            $scope.salesVM.VoucherDate = $scope.VDate;

                            getPartyPlantEditList($scope.salesVM.InvoicingPartyPlantId, $scope.salesVM.InvoicingByAddress, $scope.salesVM.DeliveryPartyPlantId, $scope.salesVM.DeliveryByAddress, $scope.salesVM.DeliveryState, $scope.salesVM.DeliveryGSTIN);
                            $scope.GetSalesMaterialData($scope.salesVM.Id);
                            $scope.GetMasterOrderIdBySalesId($scope.salesVM.Id);
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
                url: $scope.deleteUrl + $scope.salesVM.Id,
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
            SourceType: 'MasterOrderSales',
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
        $scope.selectedMasterOrderList = [];
        $scope.selectedMasterOrderItemList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
    };

    $scope.businessProcesses = "";
    $scope.detailPopUp = function () {
        $scope.materialMaster.TransactionQty = '';
        $scope.materialMaster.TransactionAmount = '';
        angular.element(document.querySelector("#detailPopUp")).modal("show");
    };

    $scope.uoMList = [];
    $scope.setMaterialMasterData = function (materialMasterRow) {
        var getMaterial = $filter("filter")($scope.salesMaterialList, { "MaterialMasterId": materialMasterRow.Id });
        if (getMaterial.length === 0) {
            $scope.materialMaster = {};
            $scope.materialMaster.TaxList = [];
            $scope.materialMaster.MaterialMasterId = materialMasterRow.Id;
            $scope.materialMaster.MaterialMasterName = materialMasterRow.UserName;
            $scope.materialMaster.BaseUOMId = materialMasterRow.BaseUOMId;
            $scope.materialMaster.UOMName = materialMasterRow.BaseUoM;
            $scope.materialMaster.MaterialGroupMasterName = materialMasterRow.MaterialGroupMasterName;
            $scope.materialMaster.ProductMasterName = materialMasterRow.ProductMasterName;
            $scope.materialMaster.ArticleId = null;
            $scope.materialMaster.ArticleName = null;
            $scope.hasArticle = materialMasterRow.HasAttribute;
            $scope.hasSku = materialMasterRow.WithSKU;
            if (materialMasterRow.HasAttribute) $scope.getArticleSearchList(materialMasterRow.Id);
            if (materialMasterRow.WithSKU) $scope.getCharacteristicsList(materialMasterRow.Id);
            getTaxCategoryList(materialMasterRow.HSNCodeId);
            var mmId = [];
            mmId.push(materialMasterRow.Id);

            cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
                var getRow = $filter("filter")($scope.uoMList, { "MaterialMasterId": materialMasterRow.Id });
                if (getRow.length === 0) {
                    angular.forEach(result, function (item, i) {
                        $scope.uoMList.push(item);
                    });
                } else {
                    $scope.uoMList = result;
                }
                $scope.materialMaster.TransactionUoMId = materialMasterRow.SalesOrderUOMId;
            });
        }
        else {
            ShowResult("This Material is already added!", "failure");
        }
        angular.element(document.querySelector("#materialmastersearchpopup")).modal("hide");
    };

    function getTaxCategoryList(hsnCodeId, soId, transactionAmount, FirstCharacteristicsValueId, SecondCharacteristicsValueId) {
        $http({
            method: 'GET',
            url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
        }).then(function (response) {
            $scope.materialtaxCategoryList = response.data;
            for (var i = 0; i < $scope.materialtaxCategoryList.length; i++) {
                $scope.materialtaxCategoryList[i].Id = null;
            }

            for (var i = 0; i < $scope.selectedMasterOrderItemList.length; i++) {
                if ($scope.selectedMasterOrderItemList[i].SONo === soId && $scope.selectedMasterOrderItemList[i].FirstCharacteristicsValueId === FirstCharacteristicsValueId && $scope.selectedMasterOrderItemList[i].SecondCharacteristicsValueId === SecondCharacteristicsValueId) {
                    $scope.selectedMasterOrderItemList[i].TaxList = $scope.materialtaxCategoryList;

                    for (var j = 0; j < $scope.selectedMasterOrderItemList[i].TaxList.length; j++) {
                        $scope.calculateHSNTaxAmount($scope.selectedMasterOrderItemList[i].TaxList[j], transactionAmount);
                    }

                }
            }
        });
    }

    $scope.calculateHSNTaxAmount = function (data, transactionAmount) {
        $scope.taxAbleAmnt = transactionAmount;
        data.TotalAmount = $scope.taxAbleAmnt * data.Percentage / 100;
    };

    $scope.selectarticle = function (article) {
        try {
            $scope.materialMaster.ArticleId = article.Id;
            $scope.materialMaster.ArticleName = article.StandardName;
            angular.element(document.querySelector("#articleSearchPop")).modal("hide");
        } catch (e) {
            ShowResult(e, "", "articleSearchPop");
        }
    };

    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector("#searchcharactervaluepopup")).modal("hide");
    };

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
        var d = $scope.selectedMasterOrderItemList[$scope.currentMaterialRow];

        $scope.salesVM.TaxOption = 'Yes';
        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.taxAbleAmnt = data.TransactionAmount;

        $scope.receiveTaxList = [];


        if ($scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxList.length > 0) {
            $scope.HSNCode = $scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxList[0].HSNCode;
            if (baseService.isUndefinedOrNull($scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxList[0].HSNCode)) {
                $scope.HSNCode = $scope.selectedMasterOrderItemList[$scope.currentMaterialRow].HSNCode;
            }

            angular.copy($scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxList, $scope.receiveTaxList);


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
            var materialData = $scope.selectedMasterOrderItemList[$scope.currentMaterialRow];
            $scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxAmount = 0;
            for (var i = 0; i < $scope.receiveTaxList.length; i++) {
                var taxcat = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
                if (taxcat.length == 2) {
                    ShowResult('Same Tax Category already exsist', 'failure', 'receiveTaxPopUp');
                    angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
                }
                var TxA = parseFloat($scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxAmount) + parseFloat($scope.receiveTaxList[i].TotalAmount);
                $scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxAmount = parseFloat(TxA.toFixed(2));
            }
            $scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxList = $scope.receiveTaxList;
            var NAmount = parseFloat($scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TransactionAmount) + parseFloat($scope.selectedMasterOrderItemList[$scope.currentMaterialRow].TaxAmount);
            $scope.selectedMasterOrderItemList[$scope.currentMaterialRow].NetAmount = parseFloat(NAmount.toFixed(2));
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
        $scope.chargesList[$scope.currentServiceRow].TaxAmount = 0;
        // $scope.chargesList[$scope.currentServiceRow].Amount = 0;
        angular.forEach($scope.receiveTaxList, function (item) {
            $scope.chargesList[$scope.currentServiceRow].TaxAmount += item.Amount;
        });
        $scope.chargesList[$scope.currentServiceRow].NetAmount = $scope.chargesList[$scope.currentServiceRow].Amount + $scope.chargesList[$scope.currentServiceRow].TaxAmount;
        //$scope.chargesList[$scope.currentServiceRow].NetAmount = $scope.chargesList[$scope.currentServiceRow].Amount;


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

    $scope.Salesdb = {};
    $scope.confirmPost = function (data) {
        $scope.salesdb = data;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function (salesdb) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "sales": salesdb
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.product = {
        Id: null
        , GRNDate: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: $window.plantId
        , PartyId: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , CutOffDate: null
        , MaterialStorageId: null
        , CurrencyId: null
        , BaseCurrencyId: $scope.baseCurrencyId
        , ToCurrencyRate: 0

        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null

        , DocRefNo: null
        , DocDate: null
        , GateEntryNo: null
        , EntryDate: null
        , FixedAssetOrInventory: 'Inventory'
        , PODepended: false
        , AlongwithInvoice: true
        , InvoiceNo: null
        , InvoiceDate: null
        , IsNonCreditable: false
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: $scope.partyType
    };
    $scope.productNew = Object.assign({}, $scope.product);

    $scope.detailPopUp = function () {
        $scope.materialMaster.MaterialMasterId = null;
        $scope.materialMaster.MaterialMasterName = null;
        $scope.materialMaster.BaseUOMId = null;
        $scope.materialMaster.BaseUoM = null;
        $scope.materialMaster.OurStyleName = null;
        $scope.materialMaster.MaterialGroupMasterName = null;
        $scope.materialMaster.ProductMasterName = null;
        $scope.materialMaster.IsOurStyleRequired = null;
        $scope.materialMaster.IsProductMstRequired = null;
        $scope.materialMaster.TransactionUoMId = null;
        $scope.materialMaster.TransactionUoM = null;
        $scope.materialMaster.TransactionAmount = '';
        $scope.materialMaster.TransactionQty = '';
        $scope.materialMaster.ArticleId = null;
        $scope.materialMaster.ArticleName = null;
        $scope.materialMaster.TaxList = [];
        $scope.materialMaster.CurrencyName = angular.element("#currencyId :selected").text();
        $scope.materialMaster.CurrencyId = $scope.salesVM.CurrencyId;

        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    };

    $scope.closeDetaiPopUp = function () {
        $scope.materialMaster = {};
        // removeValidationMsg();
        angular.element(document.querySelector("#detailPopUp")).modal("hide");
    };

    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    $scope.materialMasterbyTypeList = [];
    $scope.searchMaterialMasterList = [
        {
            'Text': 'Material Type',
            'Value': 'MaterialTypeName'
        },
        {
            'Text': 'Material Group',
            'Value': 'MaterialGroupMasterName'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Material',
            'Value': 'UserName'
        },
        {
            'Text': 'Product',
            'Value': 'ProductMasterName'
        },
        {
            'Text': 'Id',
            'Value': 'Id'
        }
    ];
    $scope.getMaterialMasterbyTypePopUp = function () {
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        $scope.popUpUrl = 'Materials/MaterialMaster/GetMaterialListByMaterialType?materialType=' + JSON.stringify($scope.materialType);
        baseService.setCurrentPage('materialMasterbyTypeList');
        $scope.getMaterialMasterbyTypeData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialMasterbyTypeList = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getMaterialMasterbyTypeData();
    };
    $scope.closeMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('materialMasterbyTypePopup');
        angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('hide');
    };

    $scope.clearCharNames = function () {
        $scope.char1 = { show: false };
        $scope.char2 = { show: false };
        $scope.char3 = { show: false };
    };
    $scope.selectMaterialByType = function (ob) {
        $scope.materialMaster.MaterialMasterId = ob.Id;
        $scope.materialMaster.MaterialMasterName = ob.UserName;
        $scope.materialMaster.BaseUOMId = ob.BaseUOMId;
        $scope.materialMaster.BaseUoM = ob.BaseUoM;
        $scope.materialMaster.OurStyleName = ob.OurStyleName;
        $scope.materialMaster.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.materialMaster.ProductMasterName = ob.ProductMasterName;
        $scope.materialMaster.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.materialMaster.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.materialMaster.TransactionUoMId = ob.BaseUOMId;
        $scope.materialMaster.TransactionUoM = ob.BaseUoM;
        $scope.materialMaster.TransactionRate = '';
        $scope.materialMaster.TransactionQty = '';
        $scope.materialMaster.ArticleId = null;
        $scope.materialMaster.ArticleName = null;
        $scope.materialMaster.FirstCharacteristicsValueId = null;
        $scope.materialMaster.SecondCharacteristicsValueId = null;
        $scope.materialMaster.ThirdCharacteristicsValueId = null;
        $scope.materialMaster.IsOriginApplicable = ob.IsOriginApplicable;
        $scope.materialMaster.CountryId = null;

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        $scope.clearCharNames();
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);
        getTaxCategoryList(ob.HSNCodeId);
        var mmId = []; mmId.push(ob.Id);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
        });
        manualValidation('div_mm', false);
        manualValidation('div_country', false);
        $scope.closeMaterialMasterbyTypePopUp();
    };
    $scope.closeMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('materialMasterbyTypePopup');
        angular.element(document.querySelector('#materialMasterbyTypePopup')).modal('hide');
    };

    $scope.closeDetailPopUpEdit = function () {
        $scope.salesMaterialList[$scope.salesDetailIndex] = $scope.materialMaster;
        $scope.materialMaster = {};
        $scope.salesDetailIndex = null;
        angular.element(document.querySelector("#detailPopUpEdit")).modal("hide");
    };

    $scope.changeInvoicingParty = function () {
        $scope.dbval = $scope.StateData;
        if ($scope.selectedMasterOrderItemList.length == 0) {

        }
        else if ($scope.StateData == $scope.UIval) {

            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else {
            ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

        }
    }

    $scope.Validation = function () {
        if (baseService.isUndefinedOrNull($scope.materialMaster.MaterialMasterId)) {
            ShowResult('Please Add Material', 'failure', 'detailPopUp');
            $scope.invalid = true;
        }
        else if ($scope.materialMaster.TransactionQty == '' || $scope.materialMaster.TransactionQty == 0) {
            ShowResult('Please Input Qty', 'failure', 'detailPopUp');
            $scope.invalid = true;
        }
        else if ($scope.materialMaster.TransactionAmount == '' || $scope.materialMaster.TransactionAmount == 0) {
            ShowResult('Please Input Amount', 'failure', 'detailPopUp');
            $scope.invalid = true;
        }
        else {
            $scope.invalid = false;
            $scope.materialValidation();
        }

    }

    $scope.materialValidation = function () {
        /* If Attribute have then Article is mandatory. */
        if ($scope.hasArticle == true && $scope.materialMaster.ArticleId === null) {//hasArticle== Hasattribute
            ShowResult('Material has no Article !');
            $scope.invalid = true;
        }
        else {
            /*Without Article is SKU level may validate. */
            if ($scope.materialMaster.ArticleId)
                var getRow3 = $filter("filter")($scope.salesMaterialList, { "MaterialMasterId": $scope.materialMaster.MaterialMasterId, "ArticleId": $scope.materialMaster.ArticleId, "FirstCharacteristicsValueId": $scope.materialMaster.FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.materialMaster.SecondCharacteristicsValueId });
            else
                var getRow3 = $filter("filter")($scope.salesMaterialList, { "MaterialMasterId": $scope.materialMaster.MaterialMasterId, "FirstCharacteristicsValueId": $scope.materialMaster.FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.materialMaster.SecondCharacteristicsValueId });

            if (getRow3 == 0) {
                $scope.invalid = false;
            }
            else {
                ShowResult('This material  already exsist', 'failure', 'detailPopUp');
                $scope.invalid = true;
            }
        }

    }

    $scope.closeAndPushDetaiPopUp = function () {
        //removeValidationMsg();
        $scope.materialMaster.TaxList = $scope.materialtaxCategoryList;
        $scope.materialMaster.FirstCharacteristicsValueId = $scope.char1.CharacteristicsId;
        $scope.materialMaster.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
        $scope.materialMaster.FreeText = $scope.char1.FreeText;
        $scope.materialMaster.FirstCharacteristicsValue = $scope.char1.Name;
        $scope.materialMaster.SecondCharacteristicsValueId = $scope.char2.CharacteristicsId;
        $scope.materialMaster.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
        $scope.materialMaster.SecondCharacteristicsValue = $scope.char2.Name;
        $scope.materialMaster.TransactionUoMId = $scope.materialMaster.TransactionUoMId;
        $scope.materialMaster.BaseUoM = angular.element("#TransactionUoMId :selected").text();

        $scope.materialMaster.Id = null;
        $scope.Validation();

        if (!$scope.invalid) {
            $scope.salesMaterialList.push($scope.materialMaster);
            $scope.materialtaxCategoryList = [];
            $scope.materialMaster = {};
            angular.element(document.querySelector('#detailPopUp')).modal('hide');
        }
    };

    $scope.getArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            //$scope.popUpUrl = 'Materials/MaterialMasterArticle/GetMaterialArticle';
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {
                            angular.element(document.querySelector('#articleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };

    $scope.recorddoubleclick = function (args) {
        $scope.selectarticle(args.data);
    }

    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('articleSearchPop');
        angular.element(document.querySelector('#articleSearchPop')).modal('hide');
    };
    $scope.getCharacteristicsList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];
            $scope.characteristicsList = response.data.charData;
            if (baseService.arrayLength($scope.characteristicsList) > 0) {
                $scope.isSearch = $scope.characteristicsList[0].FreeText !== null ? true : false;
                $scope.char1 = {
                    CharacteristicsId: $scope.characteristicsList[0].Value
                    , CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                    , Name: $scope.characteristicsList[0].Text
                    , IsFreeField: $scope.characteristicsList[0].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[0].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[0].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[0].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[0].IsFreeField)

                    , FreeText: $scope.characteristicsList[0].FreeText
                    , show: true
                };
            }
            if (baseService.arrayLength($scope.characteristicsList) > 1) {
                $scope.isSearch = $scope.characteristicsList[1].FreeText !== null ? true : false;
                $scope.char2 = {
                    CharacteristicsId: $scope.characteristicsList[1].Value
                    , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[1].MaterialMasterId
                    , Name: $scope.characteristicsList[1].Text
                    , IsFreeField: $scope.characteristicsList[1].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[1].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[1].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[1].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[1].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[1].IsFreeField)
                    , FreeText: $scope.characteristicsList[1].FreeText
                    , show: true
                };
            }
            if (baseService.arrayLength($scope.characteristicsList) > 2) {
                $scope.isSearch = $scope.characteristicsList[2].FreeText !== null ? true : false;
                $scope.char3 = {
                    CharacteristicsId: $scope.characteristicsList[2].Value
                    , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[2].MaterialMasterId
                    , Name: $scope.characteristicsList[2].Text
                    , IsFreeField: $scope.characteristicsList[2].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[2].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[2].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[2].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[2].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[2].IsFreeField)
                    , FreeText: $scope.characteristicsList[2].FreeText
                    , show: true
                };
            }
        });
    };
    $scope.charValueSearchFor = null;
    $scope.charValueCharName = null;
    $scope.findCharValueSearchData = function (data, searchFor) {
        $scope.charValueSearchFor = searchFor;
        $scope.charValueCharName = data.Name;
        $scope.getCharData(data);
    };
    $scope.selectarticle = function (ob) {
        try {
            $scope.materialMaster.ArticleId = ob.Id;
            $scope.materialMaster.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.calculateTaxCategory = function () {
        $scope.materialMaster.TotalTaxAmount = 0;
        $scope.materialMaster.NetAmount = 0;
        $scope.materialMaster.TaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.materialMaster.TransactionQty) ? 0 : parseFloat($scope.materialMaster.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.materialMaster.TransactionAmount) ? 0 : parseFloat($scope.materialMaster.TransactionAmount);
        if (tQty > 0 && tAmount > 0)
            $scope.materialMaster.TransactionRate = tAmount / tQty;
        else
            $scope.materialMaster.TransactionRate = 0;
        for (var i = 0; i < baseService.arrayLength($scope.materialtaxCategoryList); i++) {
            $scope.materialtaxCategoryList[i].TotalAmount = ((parseFloat($scope.materialtaxCategoryList[i].Percentage) * $scope.materialMaster.TransactionAmount) / 100).toFixed(2);
            $scope.materialMaster.TaxAmount = (parseFloat($scope.materialMaster.TaxAmount) + parseFloat($scope.materialtaxCategoryList[i].TotalAmount)).toFixed(2);
        }
        $scope.materialMaster.NetAmount = parseFloat($scope.materialMaster.TransactionAmount) + parseFloat($scope.materialMaster.TotalTaxAmount);
        if (isNaN($scope.materialMaster.TotalTaxAmount)) $scope.materialMaster.TotalTaxAmount = 0;
    };

    $scope.removeMaterialRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.selectedMasterOrderItemList.splice(index, 1);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
            $scope.mateId = Id;
            $scope.mateIndex = index;
        }
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

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };

    $scope.closeInvoicingPartyPopUp = function () {
        //$scope.salesMaterialList
        if ($scope.selectedMasterOrderItemList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.salesVM.ChangeInvoicingStateId)) {
                if ($scope.salesVM.PlantStateId == $scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.PlantStateId != $scope.salesVM.InvoicingStateId && $scope.salesVM.PlantStateId != $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    //ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
                    ShowResult('First delete material.', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

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
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.salesVM.DeliveryState = null;
                $scope.salesVM.DeliveryGSTIN = null;
                return $scope.salesVM.DeliveryByAddress = null;
            }
        }
    };

    function getServiceTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: 'SalesManagements/Sales/GetTaxCategoryList?receiveId=' + $scope.salesVM.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.salesVM.InvoiceDate
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
        getServiceTaxCategoryList(hsnCodeId, HSNCode);
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

    //#region  GetSalesWordReport

    $scope.SalesReport = function (data) {
        location.href = "Sales/SalesReportService?grnId=" + data.Id;
    };


    //#endregion

    //#region  GetLocaltaxInvoiceReport

    $scope.LocalTaxInvoiceReport = function (data) {
        location.href = "Sales/LocalTaxInvoice?salesId=" + data.Id;
    };

    $scope.LocalTaxInvoiceWithoutSUIReport = function (data) {
        location.href = "Sales/LocalTaxInvoiceWithoutSKU?salesId=" + data.Id;
    };
    $scope.LocalTaxInvoiceWithProductDetailService = function (data) {
        location.href = "Sales/LocalTaxInvoiceWithProductDetailService?salesId=" + data.Id;
    };
    $scope.CommercialInvoiceReport = function (data) {
        location.href = "Sales/CommercialInvoice?salesId=" + data.Id;
    };


    //#endregion


    $scope.getMasterOrderItem = function () {

        angular.element(document.querySelector('#masterOrderItemPopUp')).modal('show');
    }

    $scope.ShowResultMasterOrderPopUp = function () {
        $scope.GetMasterOrderList();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
    }
    $scope.masterOrderList = [];
    $scope.GetMasterOrderList = function () {
        $scope.masterOrderList = [];
        $http({
            method: 'GET',
            url: "SalesManagements/Sales/GetMasterOrderPopUp"
        }).then(function (response) {
            $scope.masterOrderList = response.data;

            if (baseService.arrayLength($scope.selectedMasterOrderList) > 0) {
                for (var i = 0; i < $scope.selectedMasterOrderList.length; i++) {
                    for (var j = 0; j < $scope.masterOrderList.length; j++) {
                        if ($scope.selectedMasterOrderList[i].MasterOrderId === $scope.masterOrderList[j].MasterOrderId && $scope.selectedMasterOrderList[i].MasterOrderItemId === $scope.masterOrderList[j].MasterOrderItemId) {
                            $scope.masterOrderList[j].Active = true;
                        }
                    }
                }
            }
        });
    }

    $scope.hasError = false;

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridOperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.masterOrderList.length; i++) {
                $scope.masterOrderList[i].Active = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.selectedMasterOrderList = [];
    function MakeData() {
        try {
            for (var i = 0; i < $scope.masterOrderList.length; i++) {
                var getRow = $filter("filter")($scope.selectedMasterOrderList, { "selectedMasterOrderList": $scope.masterOrderList[i].MasterOrderId, "MasterOrderItemId": $scope.masterOrderList[i].MasterOrderItemId });
                if (getRow.length == 0) {
                    if ($scope.masterOrderList[i].Active == true) {
                        var ob = {};
                        ob.MasterOrderId = $scope.masterOrderList[i].MasterOrderId;
                        ob.MasterOrderItemId = $scope.masterOrderList[i].MasterOrderItemId;
                        ob.PartyId = $scope.masterOrderList[i].PartyId;

                        if (checkExistCustomer($scope.selectedMasterOrderList, ob.PartyId)) {
                            if (checkExistList($scope.selectedMasterOrderList, ob.MasterOrderId, ob.MasterOrderItemId) === false) {

                                ob.Active = $scope.masterOrderList[i].Active;
                                ob.MaterialMaster = $scope.masterOrderList[i].MaterialMaster;
                                ob.Article = $scope.masterOrderList[i].Article;
                                ob.OrderType = $scope.masterOrderList[i].OrderType;
                                ob.CustomerName = $scope.masterOrderList[i].CustomerName;
                                ob.MasterOrderNo = $scope.masterOrderList[i].MasterOrderId;
                                ob.TotalQty = $scope.masterOrderList[i].TotalQty;
                                ob.ItemQty = $scope.masterOrderList[i].ItemQty;
                                ob.Currency = $scope.masterOrderList[i].Currency;

                                $scope.salesVM.PartyId = $scope.masterOrderList[i].PartyId;
                                $scope.salesVM.CurrencyId = $scope.masterOrderList[i].CurrencyId;
                                $scope.salesVM.BaseCurrencyId = $scope.masterOrderList[i].BaseCurrencyId;
                                $scope.salesVM.EntityId = $scope.masterOrderList[i].EntityId;
                                $scope.salesVM.ContractId = $scope.masterOrderList[i].ContractId;
                                $scope.salesVM.ContractNo = $scope.masterOrderList[i].ContractNo;
                                $scope.salesVM.PartyName = $scope.masterOrderList[i].CustomerName;
                                $scope.salesVM.PaymentTermId = $scope.masterOrderList[i].PaymentTermId;
                                $scope.salesVM.IsPaymentTermChangeable = $scope.masterOrderList[i].IsPaymentTermChangeable;

                                $scope.selectedMasterOrderList.push(ob);
                                $scope.getPartyPlant();
                                $scope.hasError = false;
                            }
                        }
                        else {
                            $scope.hasError = true;
                            throw 'Select same Customer.';
                        }
                    }

                }

            }
            $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
            $scope.GetCurrencyExchangeRateList();
        } catch (e) {
            ShowResult(e, 'failure', 'masterOrderPopUp');
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

    function checkExistList(list, MasterOrderId, MasterOrderItemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MasterOrderId === MasterOrderId && list[i].MasterOrderItemId === MasterOrderItemId) {
                return true;
            }
        }
        return false;
    }

    $scope.dateMessage = "";
    $scope.checkInvoiceDate = function () {
        if (new Date($scope.salesVM.InvoiceDate) > new Date()) {
            $scope.dateMessage = "Doc date must be below or equal to current Date!";
            return false;
        }
        else {
            $scope.dateMessage = "";
            return true;
        }
    };


    $scope.getPartyPlant = function () {
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

    $scope.CloseMasterOrder = function () {
        try {
            MakeData();
            if ($scope.hasError !== true) {
                $scope.selectMasterOrderId();
                if (!baseService.isUndefinedOrNull($scope.sqlInStatement)) {
                    getItemSOSKUList($scope.sqlInItemStatement);
                }
                angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
            } else {
                throw 'Select same Customer.';
            }
        } catch (e) {
            ShowResult(e, 'failure', 'masterOrderPopUp');
        }
    }


    $scope.masterOrderItemList = [];
    function getItemSOSKUList(masterOrderId) {
        $scope.masterOrderItemList = [];
        $http({
            method: 'GET',
            url: "SalesManagements/Sales/GetItemSOSKUList?masterOrderId=" + masterOrderId
        }).then(function (response) {
            $scope.masterOrderItemList = response.data;


            if (baseService.arrayLength($scope.selectedMasterOrderItemList) > 0) {
                for (var i = 0; i < $scope.selectedMasterOrderItemList.length; i++) {
                    for (var j = 0; j < $scope.masterOrderItemList.length; j++) {
                        if ($scope.selectedMasterOrderItemList[i].SalesOrderId === $scope.masterOrderItemList[j].SONo &&
                            $scope.selectedMasterOrderItemList[i].MaterialMasterId === $scope.masterOrderItemList[j].MaterialMasterId &&
                            $scope.selectedMasterOrderItemList[i].ArticleId === $scope.masterOrderItemList[j].ArticleId &&
                            $scope.selectedMasterOrderItemList[i].FirstCharacteristicsValueId === $scope.masterOrderItemList[j].FirstCharacteristicsValueId &&
                            $scope.selectedMasterOrderItemList[i].SecondCharacteristicsValueId === $scope.masterOrderItemList[j].SecondCharacteristicsValueId) {
                            $scope.masterOrderItemList[j].Active = true;
                        }
                    }
                }
            }
        });
    }

    $scope.setSelected = function (data) {
        $scope.salesVM.MasterOrderId = data.Id;
        $scope.salesVM.MasterOrderNo = data.MasterOrderNo;
        $scope.salesVM.PartyName = data.CustomerName;
        $scope.salesVM.PartyId = data.PartyId;
        $scope.salesVM.OrderType = data.OrderType;
        $scope.salesVM.TotalQty = data.TotalQty;
        $scope.salesVM.EntityId = data.EntityId;
        $scope.salesVM.CurrencyId = data.CurrencyId;
        $scope.partyPlantList = [];
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
        $scope.GetCurrencyExchangeRateList();

        $scope.closeMasterOrderListPopUp();
    };

    $scope.selectDoubleClick = function ($event) {
        $scope.closeMasterOrderItemPopUp();
    };

    $scope.tempList = [];

    $scope.selectMasterOrderId = function () {
        try {
            $scope.tempList = [];
            for (var di = 0; di < $scope.selectedMasterOrderList.length; di++) {
                if ($scope.selectedMasterOrderList[di].Active) {
                    $scope.tempList.push($scope.selectedMasterOrderList[di]);
                }

            }
            if ($scope.tempList.length > 50) {
                ShowResult("Maximaum 50 order can take at a time", 'failure');
            }
            else {
                var uniqueMasterOrderId = removeDuplicates($scope.tempList, 'MasterOrderId');
                var wcEmpCode = "";
                if (uniqueMasterOrderId.length > 0) {
                    wcEmpCode = "IN(";
                    wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.MasterOrderId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcEmpCode;


                var uniqueMasterOrderItemId = removeDuplicates($scope.tempList, 'MasterOrderItemId');
                var wcEmpItemCode = "";
                if (uniqueMasterOrderItemId.length > 0) {
                    wcEmpItemCode = "IN(";
                    wcEmpItemCode += Array.prototype.map.call(uniqueMasterOrderItemId, function (item) { return "'" + item.MasterOrderItemId + "'"; }).join(",") + ")";
                }
                $scope.sqlInItemStatement = wcEmpItemCode;
            }


        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };
    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.CheckMaterialArticleSKU = function (mdata) {
        var getRowParty = $filter("filter")($scope.checkItemArticleSKUList, { "MaterialMasterId": mdata.MaterialMasterId, "MaterialMasterArticleId": mdata.ArticleId });
        if (getRowParty.length == 0) {
            $scope.mValid = false;
            ShowResult(mdata.MaterialMasterName + " have no article", "failure", 'masterOrderItemPopUp');
        }

        if (mdata.WithSKU == 'Yes') {
            if (getRowParty.length > 0) {
                if (getRowParty[0].IsSKU === 1 && mdata.FirstCharacteristicsValueId === null) {
                    ShowResult(mdata.MaterialMasterName + " have no SKU Value", "failure", 'masterOrderItemPopUp');
                    $scope.mValid = false;

                }
                if (getRowParty[0].IsSKU === 2 && mdata.FirstCharacteristicsValueId === null || getRowParty[0].IsSKU === 2 && mdata.SecondCharacteristicsValueId === null) {
                    if (mdata.FirstCharacteristicsValueId === null) {
                        ShowResult(mdata.MaterialMasterName + " have no SKU1 Value", "failure", 'masterOrderItemPopUp');
                    } else {
                        ShowResult(mdata.MaterialMasterName + " have no SKU2 Value", "failure", 'masterOrderItemPopUp');
                    }
                    $scope.mValid = false;

                }
                else
                    $scope.mValid = true;
            };
        } else {
            $scope.mValid = true;
        }
    };

    $scope.refreshTemplateMasterOrderOperation = function (args) {
        $("#headschk").ejCheckBox({ "change": CheckBoxSelectAllItemWise });
    };
    function CheckBoxSelectAllItemWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.masterOrderItemList.length; i++) {
                $scope.masterOrderItemList[i].Active = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.materialtaxCategoryList = [];
    $scope.selectedMasterOrderItemList = [];
    function MakeItemData() {
        for (var i = 0; i < $scope.masterOrderItemList.length; i++) {
            if (checkItemExist($scope.selectedMasterOrderItemList, $scope.masterOrderItemList[i].SONo, $scope.masterOrderItemList[i].MaterialMasterId, $scope.masterOrderItemList[i].ArticleId, $scope.masterOrderItemList[i].FirstCharacteristicsValueId, $scope.masterOrderItemList[i].SecondCharacteristicsValueId) === false) {
                if ($scope.masterOrderItemList[i].Active == true) {
                    $scope.CheckMaterialArticleSKU($scope.masterOrderItemList[i]);
                    if ($scope.mValid == true) {
                        var moi = {};

                        moi.PONumber = $scope.masterOrderItemList[i].PONumber;
                        moi.BuyerReferenceNo = $scope.masterOrderItemList[i].BuyerReferenceNo;
                        moi.PODate = $scope.masterOrderItemList[i].PODate;
                        moi.DeliveryDate = $scope.masterOrderItemList[i].DeliveryDate;
                        moi.DestinationName = $scope.masterOrderItemList[i].DestinationName;
                        moi.MaterialMasterName = $scope.masterOrderItemList[i].MaterialMasterName;
                        moi.ProductName = $scope.masterOrderItemList[i].ProductName;
                        moi.MaterialMasterArticleName = $scope.masterOrderItemList[i].MaterialMasterArticleName;

                        moi.MaterialMasterId = $scope.masterOrderItemList[i].MaterialMasterId;
                        moi.ArticleId = $scope.masterOrderItemList[i].ArticleId;
                        moi.FirstCharacteristicsId = $scope.masterOrderItemList[i].FirstCharacteristicsId;
                        moi.FirstCharacteristicsValueId = $scope.masterOrderItemList[i].FirstCharacteristicsValueId;
                        moi.SecondCharacteristicsId = $scope.masterOrderItemList[i].SecondCharacteristicsId;
                        moi.SecondCharacteristicsValueId = $scope.masterOrderItemList[i].SecondCharacteristicsValueId;
                        moi.MasterOrderItemId = $scope.masterOrderItemList[i].MasterOrderItemId;
                        moi.SalesOrderId = $scope.masterOrderItemList[i].SONo;
                        moi.SONo = $scope.masterOrderItemList[i].SONo;
                        moi.MasterOrderId = $scope.masterOrderItemList[i].MasterOrderId;

                        moi.SKU1 = $scope.masterOrderItemList[i].SKU1;
                        moi.SKU2 = $scope.masterOrderItemList[i].SKU2;
                        moi.SKU3 = $scope.masterOrderItemList[i].SKU3;
                        moi.SOType = $scope.masterOrderItemList[i].SOType;
                        moi.Rate = $scope.masterOrderItemList[i].Rate;
                        moi.BaseUOMId = $scope.masterOrderItemList[i].BaseUOMId;
                        moi.BaseRate = $scope.masterOrderItemList[i].Rate;
                        moi.TransactionRate = $scope.masterOrderItemList[i].Rate;
                        moi.TransactionUoMId = $scope.masterOrderItemList[i].BaseUOMId;

                        moi.BaseQty = $scope.masterOrderItemList[i].Qty;
                        moi.TransactionQty = $scope.masterOrderItemList[i].PlanQty;
                        moi.BaseAmount = $scope.masterOrderItemList[i].PlanQty * $scope.masterOrderItemList[i].Rate;
                        moi.TransactionAmount = 0;
                        moi.NetAmount = 0;
                        moi.TaxAmount = 0;

                        moi.TaxList = $scope.materialtaxCategoryList;
                        moi.HSNCodeId = $scope.masterOrderItemList[i].HSNCodeId;
                        $scope.salesVM.MasterOrderId = $scope.masterOrderItemList[i].MasterOrderId;
                        moi.ExistSalesQty = (baseService.isUndefinedOrNull($scope.masterOrderItemList[i].ExistSalesQty) == true ? 0 : $scope.masterOrderItemList[i].ExistSalesQty);
                        moi.Balance = $scope.masterOrderItemList[i].Balance;

                        $scope.selectedMasterOrderItemList.push(moi);

                        getTaxCategoryList($scope.masterOrderItemList[i].HSNCodeId, moi.SONo, moi.TransactionAmount, moi.FirstCharacteristicsValueId, moi.SecondCharacteristicsValueId);

                    }
                    else if ($scope.mValid == false) {
                        angular.element(document.querySelector('#masterOrderItemPopUp')).modal('show');
                        break;
                    }
                }
            }
            // }
            //else {
            //    ShowResult("Balance is not available.", "failure");
            //}
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

    $scope.ApplyMasterOrderItemPopUp = function () {
        MakeItemData();
        if ($scope.mValid == false) {
            angular.element(document.querySelector('#masterOrderItemPopUp')).modal('show');
        }
        else
            angular.element(document.querySelector('#masterOrderItemPopUp')).modal('hide');
    }

    $scope.closeMasterOrderItemPopUp = function () {
        angular.element(document.querySelector('#masterOrderItemPopUp')).modal('hide');
    }

    $scope.GetCheckItemArticleSKUData = function () {
        $http({
            method: "GET",
            url: "SalesManagements/Sales/CheckItemArticleSKUList"
        }).then(function (response) {
            $scope.checkItemArticleSKUList = response.data;
        });
    };
    $scope.GetCheckItemArticleSKUData();

    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];

        try {
            if (!baseService.isUndefinedOrNull($scope.salesVM.ContractId)) {
                $http({
                    method: 'GET',
                    url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.salesVM.ContractId
                }).then(function (response) {
                    $scope.masterOrderCustomerList = response.data;
                });
                angular.element(document.querySelector('#MOPopUp')).modal('show');
            } else {
                throw "Select contract.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N0}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amt", dataMember: "Amt", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.CloseMO = function () {
        angular.element(document.querySelector('#MOPopUp')).modal('hide');
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
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
        CargoNetWt: 0,
        CargoGrossWt: 0,
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
        if ($scope.flag === 'Transport' || $scope.flag === 'CNF') {
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
        $scope.searchByParty = "UserName"; $scope.searchParty = "";
        angular.element(document.querySelector('#vendorPopUp')).modal('hide');
    }

    $scope.closePartyPopUp = function (x) {

        var party = x.data;
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
    };


    //#endregion PostInvoice


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

            $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
            //$scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceCharge"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
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

            $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

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
        $scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceTax"))).toFixed(2);
        //$scope.TaxAmountVal = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceCharge"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
        $scope.advanceTax.TaxAmount = (($scope.TaxAmountVal * data) / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {

        $scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceTax"))).toFixed(2);
        $scope.advanceTax.ValueOfFixed = ((data / $scope.TaxAmountVal1) * 100).toFixed(4);
    }
    //$scope.TotalSumAfterTCSVal = "";
    $scope.TotalSumAfterTCS = function () {

        $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TransactionAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.selectedMasterOrderItemList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
    }

    //#endregion


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


}