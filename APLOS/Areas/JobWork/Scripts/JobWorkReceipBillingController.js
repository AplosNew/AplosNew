'use strict';
JobWorkReceiveBillingController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'factoryService'];
function JobWorkReceiveBillingController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, factoryService) {
    $rootScope.title = 'Receive Billing';
    $scope.Action = 'Save';
    $scope.GriddataMaster = [];
    $scope.masterList = [];
    $scope.ContractList = [];
    $scope.path = 'JobWork/JobWorkReceiveBilling/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "p.UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];

    var d = new Date();
    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()
    var _Time = hh + ":" + mm;

    $scope.ModelTemp = {
        Id: null,
        Type: null,
        InvoiceNo: null,
        InvoiceDate: null,
        JWTransformationPurchaseOrderId: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ReceiptVAModelTemp = {
        Id: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ByWhomId: null,
        DocumentReferenceNo: null,
        DocumentDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        InvoiceNo: null,
        InvoiceDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        GateEntryNoId: null,
        Remarks: null,
        EmployeeStatus: null,
        EmployeeCode: null,
        ResponsiblePerson: null,

    };
    $scope.ReceiptVA = Object.assign({}, $scope.ReceiptVAModelTemp);

    $scope.ReceiptTransformationModelTemp = {
        Id: null,
        GRNDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
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
        , BaseCurrencyId: null
        , ToCurrencyRate: 0
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , DocRefNo: null
        , DocDate: null

        , EntryDate: null
        , FixedAssetOrInventory: 'Inventory'
        , PODepended: false
        , AlongwithInvoice: true
        , InvoiceNo: null
        , InvoiceDate: null
        , IsNonCreditable: false
        , IsNonVendor: false
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: 'Vendor'
        , Reason: null
        , EmployeeId: null
        , NoteForAccounts: null
        , OrderSpecific: 'No'
        , IsFOC: false
        , ContractId: null
        , OrderSpecific: 'No'
        , PurchaseLCId: null
        , CustomerName: null
        , PaymentMode: null
        , ContractNo: null
        , LCRef: null
        , CheckedByStatusForNoti: null
        , ApprovedByStatusForNoti: null
        , labelCheckAndApproved: null
        , FromPlantId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes'
        , ByWhomId: null
        , GateEntryNo: null
        , EmployeeCode: null
        , ResponsiblePerson: null
        , ByWhomEmployeeId: null
        , TransformationContractId: null
    };
    $scope.ReceiptTransformation = Object.assign({}, $scope.ReceiptTransformationModelTemp);

    $scope.ShowExCurrency = true;
    $scope.CurrencyId = null;
    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;

    });

    $scope.GetCurrencyExchangeRateList = function (CurrencyId) {
        if (!baseService.isUndefinedOrNull(CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.ModelNew.BillingRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.ShowContractPopUp = function () {
        $scope.ModelNew.Type = "ValueAdded";
        $http({
            method: 'POST',
            url: $scope.path + "GetContractList",
            data: { column: $scope.searchBy, value: $scope.search, Type: $scope.ModelNew.Type },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ContractList = response.data;

            angular.element(document.querySelector("#ContractPopUp")).modal("show");
        });
    };

    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector("#ContractPopUp")).modal("hide");

    }

    $scope.SetContractData = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
    }

    $scope.GetData = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkReceiveBilling/GetJWReceiveBillingData'
        }).then(function successCallback(response) {
            $scope.masterList = response.data;

        });
    };
    $scope.GetData();

    $scope.GetDetailData = function (masterId) {
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkReceiveBilling/GetJWReceiveBillingDetailData?masterId=' + masterId + '&contractId=' + $scope.ModelNew.JWTransformationPurchaseOrderId + '&inventoryReceiveIds=' + $scope.sqlInStatement
        }).then(function successCallback(response) {
            $scope.JWPOList = response.data;
        });
    };

    $scope.Get = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        $scope.GetJWGRNDataChecking($scope.ModelNew.JWTransformationPurchaseOrderId);
        //$scope.GetDetailData($scope.ModelNew.Id);
        if ($scope.ModelNew.CurrencyId == $scope.CurrencyId) {
            $scope.ShowExCurrency = false;
        }

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SetOutSourcePO = function (args) {
        try {
            $scope.ModelNew = Object.assign({}, args.data);

            if ($scope.ModelNew.PaymentMode == "LC") {
                if (baseService.isUndefinedOrNull($scope.ModelNew.PurchaseLCId)) {
                    throw "LC not tagged with this Out Source PO";
                }
            }

            $scope.Transformation = Object.assign({}, args.data);
            $scope.ModelNew.JWTransformationPurchaseOrderId = $scope.ModelNew.JWTransformationPurchaseOrderId;
            $scope.TabTypeNew = $scope.Transformation.TabType;
            $scope.ReceiptTransformation.TransformationContractId = $scope.Transformation.Id;

            if ($scope.ModelNew.CurrencyId == $scope.CurrencyId) {
                $scope.ShowExCurrency = false;
            }
            $scope.GetJWGRNDataChecking($scope.ModelNew.JWTransformationPurchaseOrderId);
            $scope.GetCurrencyExchangeRateList($scope.ModelNew.CurrencyId);
            angular.element(document.querySelector("#ContractPopUp")).modal("hide");
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetJWGRNDataChecking = function (contractId) {
        $scope.GriddataMaster = [];
        $scope.lst = [];

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'JobWork/JobWorkReceiveBilling/GetInventoryReceiveByTransformationContractId?contractId=' + contractId,
        }).then(function successCallback(response) {
            $scope.GriddataMaster = response.data;

            if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
                if ($scope.GriddataMaster.length > 0) {
                    var uniqueInventoryReceiveId = removeDuplicates($scope.GriddataMaster, 'InventoryReceiveId');
                    var wcInventoryReceiveId = "";
                    if (uniqueInventoryReceiveId.length > 0) {
                        wcInventoryReceiveId = "IN(";
                        wcInventoryReceiveId += Array.prototype.map.call(uniqueInventoryReceiveId, function (item) { return "'" + item.InventoryReceiveId + "'"; }).join(",") + ")";
                    }
                    $scope.sqlInStatement = wcInventoryReceiveId;
                }
                $scope.GetDetailData($scope.ModelNew.Id);
            }

            $scope.GRNListDetails();
        });
    };

    $scope.lst = [];
    $scope.GRNListDetails = function () {
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/JWGRNDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;
        });
    }

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        var filteredData = e.data["InventoryReceiveId"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data,
            columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount", "CurrencyName", "MaterialBy"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    // #region checkbox all

    $scope.refreshJWPOTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GriddataMasterONE").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.GriddataMaster.length; i++) {
                $scope.GriddataMaster[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GriddataMasterONE").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.TempList = [];
    $scope.sqlInStatement = null;
    $scope.LoadGRNDetail = function () {
        try {
            var row = $filter('filter')($scope.GriddataMaster, { 'Active': true });
            if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                $scope.TempList = row;
            }

            if ($scope.TempList.length > 0) {
                var uniqueInventoryReceiveId = removeDuplicates($scope.TempList, 'InventoryReceiveId');
                var wcInventoryReceiveId = "";
                if (uniqueInventoryReceiveId.length > 0) {
                    wcInventoryReceiveId = "IN(";
                    wcInventoryReceiveId += Array.prototype.map.call(uniqueInventoryReceiveId, function (item) { return "'" + item.InventoryReceiveId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcInventoryReceiveId;
            }
            if (!baseService.isUndefinedOrNull($scope.sqlInStatement)) {
                $scope.GetGRNDetailData($scope.ModelNew.JWTransformationPurchaseOrderId, $scope.sqlInStatement);
            } else {
                throw "Please select GRN No.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.JWPOList = [];
    $scope.GetGRNDetailData = function (contractId, InventoryReceiveIds) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'JobWork/JobWorkReceiveBilling/GetInventoryReceiveDetailByOutSourcePO?contractId=' + contractId + '&inventoryReceiveIds=' + InventoryReceiveIds,
        }).then(function successCallback(response) {
            $scope.JWPOList = response.data;
        });
    }

    $scope.calculateBalance = function (data) {
        data.BalanceQty = data.ReceiveQty - data.BillingQty;
        data.Amount = parseFloat(data.BillingQty * data.MaterialTranRate).toFixed(2);
        var gridObj = $("#GridJWPO").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.GriddataMaster = [];
        $scope.lst = [];
        $scope.JWPOList = [];
        $scope.SelectedJWPOList = [];
        $scope.ReceiptVA = Object.assign({}, $scope.ReceiptVAModelTemp);
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ShowExCurrency = true;
    }

    $scope.Action = 'Save';

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMaster() {
        CheckField("OutSource POID", $scope.ModelNew.JWTransformationPurchaseOrderId);
        CheckField("Invoice No", $scope.ModelNew.InvoiceNo);
        CheckField("Invoice Date", $scope.ModelNew.InvoiceDate);
    }

    $scope.Save = function () {
        try {

            ValidationMaster();
            if (baseService.arrayLength($scope.GriddataMaster) < 0 || baseService.arrayLength($scope.GriddataMaster) == 0) {
                throw "Inventory Receive data is required";
            }
            if (baseService.arrayLength($scope.TempList) > 0) {
                for (var i = 0; i < $scope.TempList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.TempList[i].Status)) {
                        throw "GRN No: '" + $scope.TempList[i].InventoryReceiveId + "' is not posted.";
                    }
                }
            }

            if (baseService.arrayLength($scope.JWPOList) < 0 || baseService.arrayLength($scope.JWPOList) == 0) {
                throw "Billing detail is required";
            }
            if (baseService.arrayLength($scope.JWPOList) > 0) {
                for (var i = 0; i < $scope.JWPOList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.JWPOList[i].BillingQty)) {
                        throw "Billing Qty is required.";
                    }
                }
            }
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'JobWork/JobWorkReceiveBilling/Create',
                    data: {
                        'master': $scope.ModelNew, 'data': $scope.JWPOList
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


}