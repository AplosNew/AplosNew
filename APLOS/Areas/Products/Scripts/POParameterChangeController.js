'use strict';
POParameterChangeController.$inject = [  '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window', 'cboService',  '$controller'];
function POParameterChangeController(  $scope, $rootScope, baseService, $http, $filter, $window, cboService,  $controller) {
    $rootScope.title = "PO";
    $scope.Action = 'Save';
    $scope.path = 'Products/POParameterChange/';
    $scope.getListUrl = $scope.path + 'getlist';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.searchBy = "Id"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "PO No" }, { value: 'PartyName', name: "Vendor" }, { value: 'DocRefNo', name: "Vendor DocNo" }];
    $scope.partyType = 'Vendor';
    $scope.IsToleranceUpdate = false;
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
        , IsClosed: false
        , DeliveryInstruction: null
        , SpecialInstruction: null
        , CheckedBy: null
        , AuthorizedBy: null
        , CheckedByStatus: null
        , AuthorizedByStatus: null
        , ContractId: null
        , OrderSpecific: 'Yes'
        , PurchaseLCId: null
        , CustomerName: null
        , PaymentMode: null
        , ContractNo: null
        , LCRef: null
        , labelCheckAndApproved: null
        , CheckedByStatusForNoti: null
        , ApprovedByStatusForNoti: null
        , DiscountAmount: 0
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , FileName: null
        , UserFilename: null
        , SystemFileName: null
        , Description: null
        , Remarks: null
        , PODate: null
        , Tolerance: 0
    };
    $scope.productNew = Object.assign({}, $scope.product);

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $scope.OrderSpecific = $scope.productNew.OrderSpecific;
    $scope.POdataList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: "Products/POParameterChange/GetAllPOList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.POdataList = response.data;
        });
    };
    $scope.getData();

    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        var Id = x.data.Id;
        $scope.Currency = $("#currency option:selected").text();
        $scope.productNew = x.data;
        $scope.Id = $scope.productNew.Id;
        $scope.productNew.PODate = x.data.PODate1;

        getPartyPlantEditList($scope.productNew.InvoicingPartyPlantId, $scope.productNew.InvoicingByAddress, $scope.productNew.DeliveryPartyPlantId, $scope.productNew.DeliveryByAddress, $scope.productNew.DeliveryState, $scope.productNew.DeliveryGSTIN);

        if (!baseService.isUndefinedOrNull(x.data.ContractId)) {
            $scope.productNew.OrderSpecific = 'Yes';
        }
        else {
            $scope.productNew.OrderSpecific = 'No';
        }
        $scope.ContractWiseData(x.data.ContractId);
        getInventoryMaterialList($scope.Id);
        $scope.GetPOTaxUpdate($scope.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId = inveReveiveId;

        $scope.inventoryMaterialList = [];
        $http.get('Products/PurchaseOrder/GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialList = ej.DataManager(response.data.Rows).executeLocal(ej.Query().sortBy("UserName desc"));//response.data.Rows;
                var poboq = $filter("filter")($scope.inventoryMaterialList, { POType: 'POBOQ' });
                if (poboq.length > 0) {
                    $scope.IsToleranceUpdate = true;
                }
            });
       
    }

    $scope.POTaxUpdateList = [];
    $scope.GetPOTaxUpdate = function (poId) {
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetPOTaxListForUpdate?poId=' + poId
        }).then(function successCallback(response) {
            $scope.POTaxUpdateList = response.data;
        });
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.LCRef = null;
    $scope.GRNValue = 0;
    $scope.AcptValue = 0;

    function GetPOUsedData(masterId) {
        $scope.LCRef = null;
        $scope.GRNValue = 0;
        $scope.AcptValue = 0;

        $http.get('Products/POParameterChange/GetPOUsedData?masterId=' + masterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data.LC) > 0) {
                    $scope.LCRef = response.data.LC[0].LCRef;
                }
                if (baseService.arrayLength(response.data.GRN) > 0) {
                    $scope.GRNValue = response.data.GRN[0].TotalAmount;
                }
                if (baseService.arrayLength(response.data.Acpt) > 0) {
                    $scope.AcptValue = response.data.Acpt[0].TotalAmount;
                }
            });
    }

    $scope.ContractWiseData = function (Id) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/ContractWiseData?ContractId=' + Id
        }).then(function successCallback(response) {
            if (baseService.isUndefinedOrNull(response.data) > 0) {
                $scope.productNew.ContractNo = response.data[0].ContractNo;
                $scope.productNew.LCRef = response.data[0].LCRef;
            }
            GetPOUsedData($scope.Id);
        });
    };

    $scope.lst = [];
    $scope.POListDetails = function () {
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetInventoryMaterialListPoByReqDetail'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;
        });
    }
   // $scope.POListDetails();

    $scope.detailgrid = function detailGridData(e) {
        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("POmasterId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["MaterialGroupName", "MaterialName", "Article", "Sku1", "Sku2", "Sku3", "MaterialDetail", "TransactionQty", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("POId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid1").ejGrid({
            dataSource: dataImg,
            columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
            { field: "Description", headerText: "Description", width: 100 },
            { field: "Remarks", headerText: "Remarks", width: 100 }

            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        $scope.productNew.PaymentTermId = party.PaymentTermId;
        $scope.productNew.CurrencyId = party.CurrencyId;
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew.BaseOnDueDate = null;
        $scope.productNew.BaseNoOfDays = null;
        $scope.productNew.MatureDate = null;

        $scope.productNew.TaxApplicable = party.TaxApplicable;
        $scope.productNew.IsTaxApplicableChangeable = party.IsTaxApplicableChangeable;
        if (party.TaxApplicable === 'Mandatory')
            $scope.productNew.IsTaxApplicable = true;
        else
            $scope.productNew.IsTaxApplicable = false;

        if (!baseService.isUndefinedOrNull($scope.productNew.DocDate))
            $scope.changePaymentTerm();
        getPartyPlantList();
        $scope.hidePartyPopUp();
        $scope.PaymentModeByPaymentTerm();
    };

    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                //if (paymentTerm.BaseLineDate === 'documentdate') {
                $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
            $scope.IsBaseOnDueDateEnable = true;
            //}
            //else {
            //    $scope.productNew.BaseOnDueDate = null;
            //    $scope.IsBaseOnDueDateEnable = false;
            //}
            $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };

    $scope.PaymentModeList = [];
    $scope.PaymentModeByPaymentTerm = function () {
        if (baseService.arrayLength($scope.paymentTermList) > 0) {
            for (var i = 0; i < $scope.paymentTermList.length; i++) {
                if ($scope.paymentTermList[i].Value == $scope.productNew.PaymentTermId) {
                    $scope.productNew.PaymentMode = $scope.paymentTermList[i].PaymentMode;
                    break;
                }
            }
        }
    }

    function getPartyPlantList() {
        $scope.plantList = [];
        $http.get('Products/PurchaseOrder/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address2;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });

    }

    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = deliveryplant;
                    $scope.productNew.InvoicingByAddress = invoAddress;
                    $scope.productNew.DeliveryByAddress = deliAddress;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = deliState;
                    $scope.productNew.DeliveryGSTIN = deliGSTIN;
                }
            });
        });
    }

    $scope.Save = function () {
        try {
            $scope.temptaxlist = [];
           /* taxforUpdate(data);*/
            $scope.product = Object.assign({}, $scope.productNew);
            if ($scope.Action == "Update") {

                $http({
                    method: 'POST',
                    url: 'Products/POParameterChange/POUpdate',
                    data: {
                        'data': $scope.product,
                        'detaildataList': $scope.inventoryMaterialList,
                        'poTaxList': $scope.POTaxUpdateList,
                        'isToleranceUpdate': $scope.IsToleranceUpdate
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
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
    $scope.temptaxlist = [];
    function taxforUpdate(data) {
        for (var i = 0; i < $scope.POTaxUpdateList.length; i++) {
            if ($scope.POTaxUpdateList[i].InventoryReceiveDetailId == data.Id) {
                $scope.temptaxlist.push($scope.POTaxUpdateList[i]);
            }
        }
    }
    $scope.UpdateDetail = function (data) {
        $scope.temptaxlist = [];
        taxforUpdate(data);
        try {
                $http({
                    method: 'POST',
                    url: 'Products/POParameterChange/UpdateDetail',
                    data: {
                        'entity': data,
                        'poTaxList': $scope.temptaxlist,
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getInventoryMaterialList($scope.productNew.Id);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
           
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.Clear = function () {
        ClearFields();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        return true;

    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = {
            FixedAssetOrInventory: 'Inventory'
            , PODepended: false
            , AlongwithInvoice: true
            , IsNonCreditable: false
            , BaseCurrencyId: $scope.baseCurrencyId
            , ToCurrencyRate: 1
            , TaxApplicable: null
            , IsTaxApplicable: false
            , IsTaxApplicableChangeable: false
            , PartyType: $scope.partyType
            , PlantId: $window.plantId
        };

        $scope.productNew.OrderSpecific = 'Yes';
        $scope.productNew.DiscountAmount = '0';
        $scope.productNew.Tolerance = '0';
        $scope.LCRef = null;
        $scope.GRNValue = 0;
        $scope.AcptValue = 0;
        $scope.inventoryMaterialList = [];
    }

    $scope.valuePassInDelModal = function (data) {
        $scope.id = data.InventoryReceiveDetailId;
        $scope.detaildata = data;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };
    
    $scope.detailDelete = function () {
        try {
            if ($scope.detaildata.GRNAmount!=0) {
                throw "Data delete is not possible as this PO has GRN value.";
            }

            else if ($scope.detaildata.ACPTAmount!=0) {
                throw "Data delete is not possible as this PO has Acceptance value.";
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Products/POParameterChange/DetailDelete?receiveDetailId=' + $scope.id + '&OrderSpecific=' + $scope.productNew.OrderSpecific
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.id = null;
                        getInventoryMaterialList($scope.productNew.Id);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.calculateAmount = function (data) {

        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        data.BaseQty = (data.TransactionQty * data.BaseUoMFactor);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        if (data.TrnAmount === 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        data.BaseTaxAmount = 0;
        data.TotalTaxAmount = 0;

        for (var i = 0; i < $scope.POTaxUpdateList.length; i++) {
            if ($scope.POTaxUpdateList[i].InventoryReceiveDetailId == data.Id) {
                $scope.POTaxUpdateList[i].TaxAmount = (data.TrnAmount * $scope.POTaxUpdateList[i].Percentage) / 100;
                data.BaseTaxAmount += $scope.POTaxUpdateList[i].TaxAmount;
                data.TaxAmount += $scope.POTaxUpdateList[i].TaxAmount;
                data.TotalTaxAmount += $scope.POTaxUpdateList[i].TaxAmount;
            }
        }
        if ($scope.productNew.IsNonCreditable == 1) {
            if (data.BaseTaxAmount === null) {
                data.BaseTaxAmount = '0.00';
            }
            data.BaseAmount = parseFloat(data.TrnAmount) + data.BaseTaxAmount;
            data.TransactionAmount = parseFloat(data.TrnAmount) + data.BaseTaxAmount;
        }
        else {
            data.BaseAmount = data.TrnAmount;
            data.TransactionAmount = data.TrnAmount;
        }
        data.TotalAmount = parseFloat(data.TransactionAmount) + data.BaseTaxAmount;
        $scope.productNew.TransactionAmount = Math.round($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TotalAmount") * 1000 + Number.EPSILON) / 1000;
    };

    $scope.IsToleranceUpdateChange = function () {
        getInventoryMaterialList($scope.Id);
        $scope.GetPOTaxUpdate($scope.Id);
    }
}
