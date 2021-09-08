'use strict';
PurchaseDocumentAcceptanceController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function PurchaseDocumentAcceptanceController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Purchase Document Acceptance ";
    $scope.path = 'Products/PurchaseDocumentsAcceptance/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];

    $scope.deleteLineItemUrl = $scope.path + 'DeleteLineItem/';

    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.serviceList = [];
    $scope.receiveTaxList = [];
    $scope.accServiceTaxList = [];
    $scope.chargesListPO = [];
    $scope.storageList = [];
    $scope.currencyList = [];
    $scope.acceptanceChargesCheckedList = [];
    $scope.ChargesTaxList = [];
    $scope.newChargesTaxList = [];
    $scope.detailModelSave = [];
    $scope.inventoryMaterialListPOnew = [];
    $scope.chargesListPOnew = [];

    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        //$scope.productNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });

    $scope.AcceptancePaymentSourceList = [];
    cboService.getEnumCbo("enum/GetAcceptancePaymentSourceEnumCbo", function (result) {
        $scope.AcceptancePaymentSourceList = result;
    });

    // #region PurchaseDocumentAcceptance
    $scope.Griddata = [];
    $scope.getalldata = function () {
        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetPOWithLCList?PoType=' + PoType,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
        });
    };

    $scope.GridListPO = [];
    $scope.getPOList = function () {
        $scope.GridListPO = [];
        var PoType = 'PO';
        $scope.PurchaseLCNo = $scope.PurchaseLCNo;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetLCWisePOList?PoType=' + PoType + '&PurchaseLCNo=' + $scope.PurchaseLCNo,
        }).then(function successCallback(response) {
            $scope.GridListPO = response.data;
        });
    };

    $scope.GetGRNList = function () {
        var PoType = 'PO';
        $scope.TotalGRNAmount = 0;
        $scope.PurchaseLCId = $scope.PurchaseLCNo;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetGRNList?purchaseLCId=' + $scope.PurchaseLCId,
        }).then(function successCallback(response) {
            $scope.GridListPO = response.data;

            for (var i = 0; i < $scope.seletedLST.length; i++) {
                for (var j = 0; j < $scope.GridListPO.length; j++) {
                    if ($scope.seletedLST[i].Id == $scope.GridListPO[j].Id) {
                        $scope.GridListPO.splice(i, 1);
                    }
                    $scope.TotalGRNAmount += $scope.seletedLST[i].TotalMaterialTranAmount;
                }
            }
        });
    };

    $scope.POsqlInStatement = null;
    $scope.seletedLST = [];
    $scope.GetSavedGRNList = function () {
        var PoType = 'PO';
        $scope.TotalGRNAmount = 0;
        $scope.PurchaseLCId = $scope.PurchaseLCNo;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetSavedGRNList?purchaseLCId=' + $scope.PurchaseLCId,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.seletedLST = response.data;

                for (var i = 0; i < $scope.GridListPO.length; i++) {
                    for (var j = 0; j < $scope.seletedLST.length; j++) {
                        if ($scope.GridListPO[i].Id == $scope.seletedLST[j].Id) {
                            $scope.GridListPO.splice(i, 1);
                        }
                        $scope.TotalGRNAmount += $scope.seletedLST[j].TotalMaterialTranAmount;
                    }
                }

                if ($scope.seletedLST.length > 0) {
                    // for GRNId
                    var uniqueInventoryReceiveId = removeDuplicates($scope.seletedLST, 'Id');
                    var wcInventoryReceiveId = "";
                    if (uniqueInventoryReceiveId.length > 0) {
                        wcInventoryReceiveId = "IN(";
                        wcInventoryReceiveId += Array.prototype.map.call(uniqueInventoryReceiveId, function (item) { return "'" + item.Id + "'"; }).join(",") + ")";
                    }
                    $scope.sqlInStatement = wcInventoryReceiveId;
                    //// for POId
                    var uniquePOId = removeDuplicates($scope.seletedLST, 'POId');
                    var wcPOId = "";
                    if (uniquePOId.length > 0) {
                        wcPOId = "IN(";
                        wcPOId += Array.prototype.map.call(uniquePOId, function (item) { return "'" + item.POId + "'"; }).join(",") + ")";
                    }
                    $scope.POsqlInStatement = wcPOId;
                }
                $scope.GetGRNDetailData();
            }
        });
    };

    $scope.POPopUp = function () {
        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseDocumentsAcceptance/GetPOWithLCList?PoType=' + PoType,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
        });

        angular.element(document.querySelector('#POPopUp')).modal('show');
    };

    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp')).modal('hide');
    };

    $scope.IsAcceptanceFirst = false;

    function GetIsAccepptanceFirstData(masterId) {
        $scope.IsAccepptanceFirst = false;
        $http.get($scope.path + 'GetIsAccepptanceFirstData?masterId=' + masterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data.IsAccepptanceFirst) > 0) {
                    $scope.IsAccepptanceFirst = response.data.IsAccepptanceFirst[0].IsAccepptanceFirst;

                    if ($scope.IsAccepptanceFirst == false) {
                        ShowResult("GRN is first for this PurchaseLC.", 'failure');
                    }
                }

            });
    }


    $scope.recorddoubleclick = function ($event) {
        try {

            var x = $event;
            var Id = x.data.Id;
            //GetIsAccepptanceFirstData(x.data.PurchaseLCNO);
            $scope.productNew = x.data;

            $scope.Tenure = x.data.Tenure;
            $scope.PurchaseLCNo = x.data.PurchaseLCNO;
            $scope.productId = "";
            $scope.PurchaseDocAcceptance.PartyId = x.data.PartyId;
            $scope.PurchaseDocAcceptance.PartyPlantId = x.data.PartyPlantId;
            $scope.PurchaseDocAcceptance.PurchaseLCId = x.data.PurchaseLCNO;
            $scope.PurchaseDocAcceptance.CurrencyName = x.data.CurrencyName;
            $scope.PurchaseDocAcceptance.CurrencyId = x.data.CurrencyId;
            $scope.PurchaseDocAcceptance.LCOBCurrencyId = x.data.LCOBCurrencyId;
            $scope.PurchaseDocAcceptance.OBCurrencyCode = x.data.OBCurrencyCode;
            $scope.PurchaseDocAcceptance.BankCurrencyId = x.data.LCOBCurrencyId;
            $scope.PurchaseDocAcceptance.OpeningBankMasterId = x.data.OpeningBankMasterId;
            $scope.PurchaseDocAcceptance.CustomerName = x.data.CustomerName;
            $scope.PurchaseDocAcceptance.UDNo = x.data.UDNo;
            $scope.PurchaseDocAcceptance.MasterLCRef = x.data.MasterLCRef;
            $scope.productNew.AcceptanceFirst = x.data.AcceptanceFirst;
            if ($scope.productNew.AcceptanceFirst == 'No') {
                $scope.productNew.GRNFirst = 'Yes';
            }

            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
            if ($scope.productNew.AcceptanceFirst == 'Yes') {
                $scope.getPOList();
            }
            else {
                $scope.GetGRNList();
            }
            $scope.POPopUpClose();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.PurchaseDocAcceptanceService = {
        Id: null
        , ChargeName: null
        , PurchaseDocAcceptanceId: null
        , PurchaseDocAcceptanceChargesId: null
        , AcceptanceServiceId: null
        , Amount: null
        , TotalTaxAmount: 0
        , CurrencyId: null, OpeningBankMasterId: null, BankAmount: null, VoucherId: null, Rate: null, PartyId: null, PartyPlantId: null, ServiceMasterId: null
    };

    // #region AcceptanceChargesTax

    $scope.AcceptanceCharges = {
        Id: null, Amount: null, CurrencyId: null
    };


    $scope.MakeChargesObj = function () {
        $scope.productNew.TaxOptionCharge = 'Yes';
        var obj = Object.assign({}, $scope.PurchaseDocAcceptanceService);
        obj.AcceptanceServiceId = $scope.AcceptanceCharges.Id;
        obj.PurchaseDocAcceptanceId = $scope.PurchaseDocAcceptance.Id;
        obj.ChargeName = $("option:selected", $("#Service")).text();
        obj.CurrencyName = $scope.PurchaseDocAcceptance.OBCurrencyCode;
        obj.BankCurrencyId = $scope.PurchaseDocAcceptance.LCOBCurrencyId;
        obj.CurrencyId = $scope.AcceptanceCharges.CurrencyId;
        obj.OpeningBankMaster = $scope.productNew.LCOpeningBank;
        obj.OpeningBankMasterId = $scope.PurchaseDocAcceptance.OpeningBankMasterId;
        obj.Amount = $scope.AcceptanceCharges.Amount;


        obj.BankAmountFlag = $scope.BankAmountFlag;
        if ($scope.PurchaseDocAcceptance.LCOBCurrencyId === $scope.AcceptanceCharges.CurrencyId) {
            obj.BankAmount = $scope.AcceptanceCharges.Amount;
        }

        if (baseService.isUndefinedOrNull($scope.AcceptanceCharges.Id))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.AcceptanceChargesList, function (item) { return item.Id === $scope.AcceptanceCharges.Id; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.AcceptanceChargesList, function (item) { return item.Id === $scope.AcceptanceCharges.Id; })[0].HSNCode;

        getTaxCategoryList(hsnCodeId);

        $scope.acceptanceChargesCheckedList.push(obj);
    }

    $scope.addTax = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.AcceptanceCharges.Id)) {
                throw "Charge is required.";
            }
            if (baseService.isUndefinedOrNull($scope.AcceptanceCharges.Amount) || $scope.AcceptanceCharges.Amount === 0 || $scope.AcceptanceCharges.Amount < 0) {
                throw "Amount is required.";
            }
            if (baseService.isUndefinedOrNull($scope.AcceptanceCharges.CurrencyId)) {
                throw "Currency is required.";
            }

            var data = {
                TotalAmount: 0,
                Id: null,
                HSNCode: $scope.HSNCode,
                HSNCodeId: null,
                UserName: null,
                TaxCategoryId: null,
                SpecialTaxId: null
            };
            $scope.AccChargetaxCategoryList.push(data);

            $scope.changeAcceptanceCharges();

            if ($scope.AcceptanceCharges.Amount === '' || $scope.AcceptanceCharges.Amount === null || $scope.AcceptanceCharges.Amount === undefined) {
                ShowResult("Enter the Acceptance Charges Amount", 'failure', 'serviceChargePopUp');
                return false;
            }
            $scope.isExists = false;
            if (baseService.arrayLength($scope.acceptanceChargesCheckedList) == 0) {

                for (var i = 0; i < $scope.AccChargetaxCategoryList.length; i++) {
                    $scope.AccChargetaxCategoryList[i].AcceptanceServiceId = obj.AcceptanceServiceId;
                    $scope.ChargesTaxList.push($scope.AccChargetaxCategoryList[i]);
                }
                //$scope.AccChargetaxCategoryList = [];
                $scope.isExists = true;


            }
            else if (baseService.arrayLength($scope.acceptanceChargesCheckedList) > 0) {
                if ($scope.TaxAction === 'Save') {
                    for (var i = 0; i < baseService.arrayLength($scope.acceptanceChargesCheckedList); i++) {
                        if ($scope.acceptanceChargesCheckedList[i].AcceptanceServiceId === obj.AcceptanceServiceId) {
                            $scope.isExists = true;
                            ShowResult("Acceptance Charges already exists", 'failure', 'serviceChargePopUp');
                            return false;
                        }

                    }
                } else {
                    $scope.isExists = false;
                }
            }

            if ($scope.isExists == false) {
                $scope.acceptanceChargesCheckedList.push(obj);

            }


        } catch (e) {
            ShowResult(e, 'failure', 'serviceChargePopUp');
        }
    }

    $scope.closeChargeTaxPopUp = function () {
        try {
            if (baseService.arrayLength($scope.acceptanceChargesCheckedList) > 0) {
                for (var i = 0; i < $scope.acceptanceChargesCheckedList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.acceptanceChargesCheckedList[i].Amount)) {
                        $scope.acceptanceChargesCheckedList[i].Amount = $scope.AcceptanceCharges.Amount;
                        $scope.acceptanceChargesCheckedList[i].CurrencyId = $scope.AcceptanceCharges.CurrencyId;
                    }
                    $scope.acceptanceChargesCheckedList[i].TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.AccChargetaxCategoryList), 'TaxAmount');
                }
            }


            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/SaveServiceChargesAndChargesTax',
                data: {
                    'AcceptancechargesList': $scope.acceptanceChargesCheckedList, 'purchaseDocAcceptancechargesTax': $scope.AccChargetaxCategoryList, 'entity': $scope.PurchaseDocAcceptance
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                    $scope.GetService($scope.PurchaseDocAcceptance.Id);
                    $scope.GetAcceptanceChargesTaxList($scope.PurchaseDocAcceptance.Id);
                    angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            }
        } catch (e) {
            ShowResult(e.Message, 'success', 'serviceChargePopUp');
        }

    }

    $scope.SaveServiceChargesAndChargesTax = function () {
        try {
            if (baseService.arrayLength($scope.acceptanceChargesCheckedList) > 0) {
                for (var i = 0; i < $scope.acceptanceChargesCheckedList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.acceptanceChargesCheckedList[i].Amount)) {
                        $scope.acceptanceChargesCheckedList[i].Amount = $scope.AcceptanceCharges.Amount;
                        $scope.acceptanceChargesCheckedList[i].CurrencyId = $scope.AcceptanceCharges.CurrencyId;
                    }
                }
                $scope.acceptanceChargesCheckedList.TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.taxCategoryList), 'TaxAmount');
            }

            if (baseService.arrayLength($scope.taxCategoryList) > 0) {
                for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                    $scope.taxCategoryList[i].Id = null;
                    $scope.taxCategoryList[i].AcceptanceServiceId = $scope.AcceptanceCharges.Id;
                    $scope.ChargesTaxList.push($scope.taxCategoryList[i]);
                }
            }
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/SaveServiceChargesAndChargesTax',
                data: {
                    'AcceptancechargesList': $scope.acceptanceChargesCheckedList, 'purchaseDocAcceptancechargesTax': $scope.ChargesTaxList, 'entity': $scope.PurchaseDocAcceptance
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                    $scope.GetService($scope.PurchaseDocAcceptance.Id);
                    $scope.GetAcceptanceChargesTaxList($scope.PurchaseDocAcceptance.Id);
                    angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            }
        } catch (e) {
            ShowResult(e.Message, 'success', 'serviceChargePopUp');
        }

    }

    $scope.AcceptanceChargesList = [];
    $scope.serviceChargePopUp = function () {
        $scope.AcceptanceChargesList = [];
        $http({
            method: 'GET',
            url: "Products/PurchaseDocumentsAcceptance/GetAcceptanceCharges",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.AcceptanceChargesList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

        if (baseService.arrayLength($scope.currencyList) > 0) {
            for (var i = 0; i < $scope.currencyList.length; i++) {
                if ($scope.currencyList[i].Value === $scope.PurchaseDocAcceptance.BankCurrencyId) {
                    $scope.AcceptanceCharges.CurrencyId = $scope.currencyList[i].Value;
                }
            }
        }


        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };

    $scope.AccChargetaxCategoryList = [];

    $scope.TaxAction = 'Save';

    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;
        var getRow = $filter("filter")($scope.AccChargetaxCategoryList, { "TaxCategoryId": id });
        if (getRow.length == 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'taxPopup');
        }
    };

    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.newaccServiceTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
        }
    };
    $scope.calculateTaxAmount = function (data) {
        data.TaxAmount = Math.round($scope.AcceptanceCharges.Amount * data.Percentage) / 100;
    };

    $scope.closeServiceChargeTaxPopUpwindow = function () {
        getServiceChargeList($scope.PurchaseDocAcceptance.Id);
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }

    $scope.closeServiceChargeTaxPopUp = function () {
        try {


            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/SaveServiceAndServiceTax',
                //data: {
                //    'purchaseDocAcceptanceServiceTax': $scope.newaccServiceTaxList, 'PurchaseDocAcceptanceId': $scope.PurchaseDocAcceptance.Id, 'PurchaseDocAcceptanceServiceId':$scope.PurchaseDocAcceptanceServiceId
                //},
                data: {
                    'purchaseDocAcceptanceService': $scope.serviceList
                    , 'purchaseDocAcceptanceServiceTax': $scope.accServiceTaxList, 'PurchaseDocAcceptanceId': $scope.PurchaseDocAcceptance.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    getServiceChargeList($scope.PurchaseDocAcceptance.Id);
                    angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e.Message, 'success');
        }
    }

    $scope.flag = null;
    $scope.delindex = -1;
    $scope.removeTax = function (id, index, flg) {
        $scope.flag = flg;
        $scope.tempId = id;
        $scope.delindex = index;
        if (baseService.isUndefinedOrNull($scope.tempId))
            $scope.message = 'Are you sure want to delete?';
        else
            $scope.message = 'Are you sure want to delete?';
        angular.element(document.querySelector('#removPopUp')).modal('show');
    };


    $scope.removeTaxRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempId)) {
            if ($scope.flag == 'MT') {
                $scope.receiveTaxList.splice($scope.delindex, 1);
            }
            else if ($scope.flag == 'ST') {
                $scope.AccServicetaxCategoryList.splice($scope.delindex, 1);
            }
            else if ($scope.flag == 'CT') {
                $scope.AccChargetaxCategoryList.splice($scope.delindex, 1);
            }
            $scope.delindex = -1;
            angular.element(document.querySelector('#removPopUp')).modal('hide');
        }
        else {
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/DeleteTax?id=' + $scope.tempId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    if ($scope.flag == 'MT') {
                        $scope.receiveTaxList.splice($scope.delindex, 1);
                    }
                    else if ($scope.flag == 'ST') {
                        $scope.AccServicetaxCategoryList.splice($scope.delindex, 1);
                    }
                    else if ($scope.flag == 'CT') {
                        $scope.AccChargetaxCategoryList.splice($scope.delindex, 1);
                    }

                    $scope.delindex = -1;
                    angular.element(document.querySelector('#removPopUp')).modal('hide');
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion

    $scope.RemoveAcceptancePopUp = function (data, index) {
        $scope.acceptanceChargesId = data.Id;
        $scope.acceptanceIndex = index;
        $scope.message_confirmation = "Are you sure to delete permanently?";
        angular.element(document.querySelector("#RemoveAcceptancePopUp")).modal("show");
    }

    $scope.RemoveAcceptance = function () {
        if (baseService.isUndefinedOrNull($scope.acceptanceChargesId)) {
            $scope.acceptanceChargesCheckedList.splice($scope.acceptanceIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/DeleteCharge?id=' + $scope.acceptanceChargesId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.acceptanceChargesCheckedList.splice($scope.acceptanceIndex, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    $scope.PurchaseDocAcceptance = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        AcceptanceNo: null,
        EntryDate: null,
        AcceptanceDate: null,
        POId: null,
        CheckedBy: null,
        CheckedByStatus: null,
        AuthorizedBy: null,
        AuthorizedByStatus: null,
        Remarks: null,
        PurchaseLCId: null,
        AcceptancePaymentSource: null,
        DueDate: null,
        InvoiceDate: null,
        InvoiceNo: null,
        VoucherId: null,
        ServiceVoucherId: null,
        PartyId: null,
        PartyPlantId: null,
        IsNonCreditable: false,
        OBCurrencyCode: null,
        LCOBCurrencyId: null,
        AcceptanceAmount: 0,
        CurrentAcceptanceAmount: 0,
        CurrentQty: 0
    };
    $scope.TotalGRNAmount = 0;
    $scope.PurchaseDocAcceptanceDetail = {
        Id: null,
        PurchaseDocAcceptanceId: null,
        MaterialMasterId: null,
        ArticleId: null,
        FirstCharacteristicsId: null,
        FirstCharacteristicsValueId: null,
        SecondCharacteristicsId: null,
        SecondCharacteristicsValueId: null,
        ThirdCharacteristicsId: null,
        ThirdCharacteristicsValueId: null,
        TransactionQty: null,
        TransactionUoMId: null,
        MaterialTranRate: null,
        MaterialTranAmount: null,
        TaxAmount: null,
        TotalMaterialTranAmount: null,
        ChargesTranAmount: null,
        ChargesTaxTranAmount: null,
        POId: null,
        PODetailId: null
    };

    $scope.PurchaseDocAcceptanceService = {
        Id: null,
        PurchaseDocAcceptanceId: null,
        PurchaseDocAcceptanceChargesId: null,
        AcceptanceServiceId: null,
        Amount: null,
        TotalTaxAmount: null,
    };

    $scope.serviceModel = {
        Id: null
        , ServiceMasterId: null
        , PurchaseDocAcceptanceId: null
        , CurrencyName: null
        , CurrencyId: null
        , BaseCurrencyId: null
        , DocDate: null
        , TransactionAmount: 0
        , Amount: null
        , TotalTaxAmount: 0
        , ToCurrencyRate: null
        , IsNonCreditable: null
    };
    $scope.Action = 'Save';

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }
        var filtered = $("#GridAcceptance").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                $scope.inventoryMaterialListPO[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAcceptance").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.AcceptanceNo)) {
            ShowResult("Please input AcceptanceNo!", "failure");
            return true;
        }

        else if (baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.AcceptanceDate)) {
            ShowResult("Please input AcceptanceDate!", "failure");
            return true;
        }
        else if (baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.InvoiceNo)) {
            ShowResult("Please input InvoiceNo!", "failure");
            return true;
        }
        else if (baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.InvoiceDate)) {
            ShowResult("Please input InvoiceDate!", "failure");
            return true;
        }
        else if (new Date($scope.PurchaseDocAcceptance.InvoiceDate) > new Date($scope.PurchaseDocAcceptance.AcceptanceDate)) {
            ShowResult("Acceptance Date must be greater or equal to Invoice Date!", "failure");
            return true;
        }
        else if (baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.AcceptanceRate)) {
            ShowResult("Please input Exchange Rate!", "failure");
            return true;
        }
        else if (baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.AcceptancePaymentSource)) {
            ShowResult("Please input Acceptance Payment Source!", "failure");
            return true;
        }

        else
            return false;
    }


    $scope.Save1 = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.AcceptanceAmount)) {
                throw "Acceptance Amount is required.";
            }
            $scope.CalculateMaterialAmount();
            if ($scope.productNew.AcceptanceFirst == 'Yes') {
                $scope.inventoryMaterialListPOnew = [];

                var mlsddate = new Date($scope.PurchaseDocAcceptance.AcceptanceDate);
                //$scope.PurchaseDocAcceptance.POId = $scope.POId;
                $scope.PurchaseDocAcceptance.PurchaseLCId = $scope.productNew.PurchaseLCNO;
                $scope.PurchaseDocAcceptance.PartyId = $scope.productNew.PartyId;
                $scope.PurchaseDocAcceptance.PartyPlantId = $scope.productNew.PartyPlantId;

                $scope.$broadcast('show-errors-check-validity');
                if ($scope.productNewForm.$valid) {
                    if ($scope.PurchaseDocAcceptance.AcceptanceAmount > $scope.TotalPOAmount) {
                        throw "Acceptance Amount can't greater than Total PO Amount.";
                    }
                    if ($scope.Action === 'Save') {
                        var mlsd = mlsddate.setDate(mlsddate.getDate() + $scope.Tenure);
                        $scope.PurchaseDocAcceptance.DueDate = $filter('dateFiltering')(new Date(mlsd), 'dd-MM-yyyy');

                        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].TransactionQty)) {
                                $scope.inventoryMaterialListPO[i].TransactionQty = $scope.inventoryMaterialListPO[i].Qty;
                                $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);
                            } else {
                                $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);
                            }
                            //if ($scope.inventoryMaterialListPO[i].TransactionQty === null) {
                            //    ShowResult("Enter the Current Received", 'failure');
                            //    return false;
                            //}
                            //else if ($scope.inventoryMaterialListPO[i].TransactionQty === "") {
                            //    ShowResult("Enter the Current Received", 'failure');
                            //    return false;
                            //}
                            //else if ($scope.inventoryMaterialListPO[i].TransactionQty === 0) {
                            //    ShowResult("Enter the Current Received", 'failure');
                            //    return false;
                            //}


                        }
                        try {

                            $http({
                                method: 'POST',
                                url: 'Products/PurchaseDocumentsAcceptance/Create',
                                data: {
                                    'entity': $scope.PurchaseDocAcceptance
                                    , 'PurchaseDocAcceptanceDetail': $scope.inventoryMaterialListPOnew
                                    //, 'purchaseDocAcceptanceTax': $scope.acceptanceTaxList
                                    //, 'AcceptancechargesList': $scope.acceptanceChargesCheckedList
                                    //, 'purchaseDocAcceptancechargesTax': $scope.ChargesTaxList
                                    //, 'purchaseDocAcceptanceService': $scope.serviceList
                                    //, 'purchaseDocAcceptanceServiceTax': $scope.accServiceTaxList
                                },
                                dataType: 'JSON'
                            }).then(function successCallback(response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.data.Message, 'failure');
                                }
                                else {
                                    ShowResult(response.data.Message, 'success');
                                    $scope.PurchaseDocAcceptance.Id = response.data.entity.Id;
                                    $scope.gridAcceptanceList();
                                    // $scope.setTabAcceptenceList(1);
                                    $scope.Action = 'Update';
                                    $scope.seletedLST = [];
                                    $scope.GridListPO = [];

                                    $scope.getRecordDoubleClickDetail($scope.PurchaseDocAcceptance.Id);

                                }
                            }), function errorCallBack(response) {
                                ShowResult(response.data.Message, 'failure');
                            }
                        } catch (e) {
                            ShowResult(e.Message, 'success');
                        }


                    }
                    else if ($scope.Action === 'Update') {
                        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {

                            if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].TransactionQty)) {
                                $scope.inventoryMaterialListPO[i].TransactionQty = $scope.inventoryMaterialListPO[i].Qty;
                            }
                            if ($scope.inventoryMaterialListPO[i].Active == true) {
                                $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);

                            }

                        }

                        $scope.PurchaseDocAcceptance1 = {};
                        $scope.PurchaseDocAcceptance1 = $scope.PurchaseDocAcceptance;
                        //$scope.acceptanceTaxList = [];
                        //for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
                        //    $scope.acceptanceTaxList.push($scope.POMaterialTaxList[i]);
                        //}
                        try {
                            $http({
                                method: 'POST',
                                url: 'Products/PurchaseDocumentsAcceptance/Update',
                                data: {
                                    'entity': $scope.PurchaseDocAcceptance1
                                    , 'PurchaseDocAcceptanceDetail': $scope.inventoryMaterialListPOnew
                                    , 'PurchaseDocAcceptanceServiceDetail': $scope.ServicePODetailList
                                    , 'purchaseDocAcceptanceService': $scope.serviceList
                                    , 'purchaseDocAcceptanceServiceTax': $scope.accServiceTaxList
                                },
                                dataType: 'JSON'
                            }).then(function successCallback(response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.data.Message, 'failure');
                                }
                                else {
                                    ShowResult(response.data.Message, 'success');
                                    $scope.PurchaseDocAcceptance.Id = response.data.entity.Id;
                                    $scope.gridAcceptanceList();
                                    //$scope.setTabAcceptenceList(1);


                                    $scope.getRecordDoubleClickDetail($scope.PurchaseDocAcceptance.Id);

                                    $scope.Action = 'Update';
                                }
                            }), function errorCallBack(response) {
                                ShowResult(response.data.Message, 'failure');
                            }
                        } catch (e) {
                            ShowResult(e.Message, 'success');
                        }
                    }
                }
            }
            else {
                $scope.TotalAcptValue = 0;
                $scope.TotalAcptValue = $scope.PurchaseDocAcceptance.AcceptanceAmount + $scope.OtherTotalAcptValue;

                //for (var i = 0; i < $scope.seletedLST.length; i++) {
                //    if (new Date($scope.seletedLST[i].DocDate) > new Date($scope.PurchaseDocAcceptance.AcceptanceDate)) {
                //        throw "Acceptance Date can't less than GRN Date.";
                //    }
                //}

                if ($scope.TotalAcptValue > $scope.productNew.LCAmount) {
                    throw "Acceptance Amount can't greater than Total LC Amount.";
                }
                $scope.$broadcast('show-errors-check-validity');
                if ($scope.productNewForm.$valid) {

                    if ($scope.Action === 'Save') {
                        try {
                            $scope.PurchaseDocAcceptance.POId = $scope.POId;
                            $scope.PurchaseDocAcceptance.PurchaseLCId = $scope.productNew.PurchaseLCNO;
                            $scope.PurchaseDocAcceptance.PartyId = $scope.productNew.PartyId;
                            $scope.PurchaseDocAcceptance.PartyPlantId = $scope.productNew.PartyPlantId;
                            var mlsddate = new Date($scope.PurchaseDocAcceptance.AcceptanceDate);
                            var mlsd = mlsddate.setDate(mlsddate.getDate() + $scope.Tenure);
                            $scope.PurchaseDocAcceptance.DueDate = $filter('dateFiltering')(new Date(mlsd), 'dd-MM-yyyy');

                            $http({
                                method: 'POST',
                                url: 'Products/PurchaseDocumentsAcceptance/CreateAndUpdateGRNAcceptance',
                                data: {
                                    'entity': $scope.PurchaseDocAcceptance
                                    , 'PurchaseDocAcceptanceDetail': $scope.seletedLST
                                    , 'PurchaseDocAcceptanceDetails': $scope.inventoryMaterialListPO
                                    //, 'purchaseDocAcceptanceTax': $scope.acceptanceTaxList
                                    //, 'AcceptancechargesList': $scope.acceptanceChargesCheckedList
                                    //, 'purchaseDocAcceptancechargesTax': $scope.ChargesTaxList
                                    //, 'purchaseDocAcceptanceService': $scope.serviceList
                                    //, 'purchaseDocAcceptanceServiceTax': $scope.accServiceTaxList
                                },
                                dataType: 'JSON'
                            }).then(function successCallback(response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.data.Message, 'failure');
                                }
                                else {
                                    ShowResult(response.data.Message, 'success');
                                    $scope.PurchaseDocAcceptance.Id = response.data.entity.Id;
                                    $scope.gridAcceptanceList();
                                    $scope.Action = 'Update';
                                    $scope.seletedLST = [];
                                    $scope.GridListPO = [];
                                    $scope.Id = $scope.PurchaseDocAcceptance.Id;
                                    $scope.getRecordDoubleClickDetailGRN($scope.PurchaseDocAcceptance.Id);

                                }
                            }), function errorCallBack(response) {
                                ShowResult(response.data.Message, 'failure');
                            }
                        } catch (e) {
                            ShowResult(e.Message, 'success');
                        }
                    }
                    else {

                        $http({
                            method: 'POST',
                            url: 'Products/PurchaseDocumentsAcceptance/CreateAndUpdateGRNAcceptance',
                            data: {
                                'entity': $scope.PurchaseDocAcceptance
                                , 'PurchaseDocAcceptanceDetail': $scope.seletedLST
                                , 'PurchaseDocAcceptanceDetails': $scope.inventoryMaterialListPO
                            },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, 'failure');
                            }
                            else {
                                ShowResult(response.data.Message, 'success');
                                $scope.PurchaseDocAcceptance.Id = response.data.entity.Id;
                                $scope.gridAcceptanceList();
                                $scope.Action = 'Update';
                                $scope.seletedLST = [];
                                $scope.GridListPO = [];
                                $scope.Id = $scope.PurchaseDocAcceptance.Id;
                                $scope.getRecordDoubleClickDetailGRN($scope.PurchaseDocAcceptance.Id);
                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.calculateAmount = function (data, index) {
        try {
            if ($scope.productNew.AcceptanceFirst == 'Yes') {
                var gridObj = $("#GridAcceptance").data("ejGrid");
                data.GRNRcvQty = data.Otherqty + parseFloat(data.TransactionQty);

                data.Balance = data.POQty - data.GRNRcvQty;

                if (data.Balance >= 0) {
                    if (data.POQty >= (data.GRNRcvQty + data.Balance)) {

                        data.TrnAmount = data.TransactionRate * parseFloat(data.TransactionQty);

                        $scope.CalculateMaterialAmount();

                    } else {
                        throw 'Current quantity can not greater than balance quantity!';
                    }
                } else {
                    data.Balance = 0;
                    data.TransactionQty = 0;
                    throw 'Current quantity can not greater than balance quantity!';
                }
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            }
            else {
                var TranQty = Math.min(data.GRNRcvQty, data.POQty);

                //data.Balance = data.GRNRcvQty - (data.Otherqty + parseFloat(data.TransactionQty));
                data.Balance = TranQty - (data.Otherqty + parseFloat(data.TransactionQty));

                if (data.Balance >= 0) {
                    //if (data.GRNRcvQty >= (data.Otherqty + parseFloat(data.TransactionQty))) {
                    if (TranQty >= (data.Otherqty + parseFloat(data.TransactionQty))) {
                        data.TrnAmount = data.TransactionRate * parseFloat(data.TransactionQty);

                        $scope.CalculateMaterialAmount();

                    } else {
                        throw 'Current quantity can not greater than balance quantity!';
                    }
                } else {
                    data.Balance = 0;
                    data.TransactionQty = 0;
                    throw 'Current quantity can not greater than balance quantity!';
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.calculateRate = function (data, index) {

        if (data.TransactionQty === '' || data.TransactionQty === 0 || data.TransactionQty === null) {
            data.TransactionRate = (data.TrnAmount / data.GRNRcvQty).toFixed(2);
            if (data.TransactionRate === 'NaN')
                data.TransactionRate = 0;
        }
        else {
            $scope.PreBal = data.Balance;
            data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
            if (data.TrnAmount == 'NaN')
                data.TrnAmount = 0;
        }
    };

    $scope.CalculateMaterialAmount = function () {
        $scope.PurchaseDocAcceptance.AcceptanceAmount = 0;
        $scope.PurchaseDocAcceptance.CurrentQty = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.serviceList), 'Amount');
        var totalTaxAmount = $filter('sumByKey')($filter('filter')($scope.serviceList), 'TotalTaxAmount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');

        if ($scope.PurchaseDocAcceptance.IsNonCreditable) {
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                $scope.inventoryMaterialListPO[i].ChargesTranAmount = ((parseFloat(TotalServiceAmount) / parseFloat(TotalTrnAmount)) * $scope.inventoryMaterialListPO[i].TrnAmount);

                $scope.inventoryMaterialListPO[i].ChargesTaxTranAmount = ((parseFloat(totalTaxAmount) / parseFloat(TotalTrnAmount)) * $scope.inventoryMaterialListPO[i].TrnAmount);

                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TaxAmount) + $scope.inventoryMaterialListPO[i].ChargesTranAmount + $scope.inventoryMaterialListPO[i].ChargesTaxTranAmount + $scope.inventoryMaterialListPO[i].TrnAmount;
            }
        }
        else {
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = ((parseFloat(TotalServiceAmount) / parseFloat(TotalTrnAmount)) * $scope.inventoryMaterialListPO[i].TrnAmount) + $scope.inventoryMaterialListPO[i].TrnAmount;
                $scope.inventoryMaterialListPO[i].ChargesTranAmount = ((parseFloat(TotalServiceAmount) / parseFloat(TotalTrnAmount)) * $scope.inventoryMaterialListPO[i].TrnAmount);
                $scope.inventoryMaterialListPO[i].ChargesTaxTranAmount = ((parseFloat(totalTaxAmount) / parseFloat(TotalTrnAmount)) * $scope.inventoryMaterialListPO[i].TrnAmount);


            }
        }
        for (var k = 0; k < $scope.inventoryMaterialListPO.length; k++) {
            $scope.PurchaseDocAcceptance.AcceptanceAmount += $scope.inventoryMaterialListPO[k].TrnAmount;
            //$scope.PurchaseDocAcceptance.CurrentQty += $scope.inventoryMaterialListPO[k].TransactionQty;
        }
    };

    $scope.inventoryMaterialListPO = [];
    $scope.GridAcceptanceList = [];
    $scope.gridAcceptanceList = function () {
        $scope.GridAcceptanceList = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceList',
        }).then(function successCallback(response) {
            $scope.GridAcceptanceList = response.data;
            for (var i = 0; i < $scope.GridAcceptanceList.length; i++) {
                $scope.GridAcceptanceList[i].AcceptanceDate = new Date($scope.GridAcceptanceList[i].AcceptanceDate);
                $scope.GridAcceptanceList[i].InvoiceDate = new Date($scope.GridAcceptanceList[i].InvoiceDate);
            }

        });

    };
    $scope.gridAcceptanceList();

    $scope.GridAcceptanceListDetail = [];
    $scope.gridAcceptanceListDetail = function () {

        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceDetailList',
        }).then(function successCallback(response) {
            $scope.GridAcceptanceListDetail = response.data;
            //entrydata = copy(searchdata);
        });

    };
    $scope.gridAcceptanceListDetail();

    $scope.GridAcceptanceServiceList = [];
    $scope.gridAcceptanceServiceList = function () {

        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceDetailList',
        }).then(function successCallback(response) {
            $scope.GridAcceptanceListDetail = response.data;
            //entrydata = copy(searchdata);
        });

    };
    $scope.gridAcceptanceServiceList();

    $scope.GetMaterialByIdList = [];
    $scope.GetMaterialById = function () {

        $http({
            method: 'GET',
            url: 'Products/PurchaseDocumentsAcceptance/GetMaterialById'
        }).then(function successCallback(response) {
            $scope.GetMaterialByIdList = response.data;
            window.GetMaterialByIdList = response.data;

        });

    }
    $scope.GetMaterialById();

    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {


        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.GetMaterialByIdList).executeLocal(ej.Query().where("AcceptenceId", "equal", parseInt(filteredData), true).take(5));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["MaterialMasterGroupName", "MaterialMasterName", "StandardName", "Article", "SKU1", "SKU2", "SKU3", "TransactionUoM", "Rate", "Amount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    $scope.GRN = "";
    $scope.tab1 = 1;
    $scope.setTabAcceptenceList = function (newTab) {
        $scope.tab1 = newTab;
    };
    $scope.isSetAcceptenceList = function (tabNum) {
        return $scope.tab1 === tabNum;
        $scope.GRN = 1;

    };

    $scope.contractList = [];
    $scope.GetPopUpContract = function () {

        $scope.contractList = [];
        $http.get("Products/PurchaseOrder/GetLCContractList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#ContractPopUp')).modal('show');
    };

    $scope.SelectedContract = function (obj) {
        var data = obj.data.ContractId;
        $scope.productNew.ContractId = data;
        $scope.productNew.CustomerName = obj.data.CustomerName;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }
    $scope.GetDataDoubleClickMaster = [];
    $scope.getRecordDoubleClickMaster = function (id) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetRecordDoubleClickMaster?Id=' + id,
        }).then(function successCallback(response) {
            $scope.GetDataDoubleClickMaster = response.data;
            $scope.PurchaseDocAcceptance = $scope.GetDataDoubleClickMaster
            $scope.productNew.AcceptanceFirst = $scope.GetDataDoubleClickMaster.AcceptanceFirst;
            if ($scope.productNew.AcceptanceFirst == 'No') {
                $scope.productNew.GRNFirst = 'Yes';
            }
            $scope.POId = $scope.GetDataDoubleClickMaster[0].POId;
            $scope.productNew.PurchaseLCNO = $scope.GetDataDoubleClickMaster[0].PurchaseLCNO;
            $scope.productNew.PurchaseLCId = $scope.GetDataDoubleClickMaster[0].PurchaseLCId;
            $scope.productNew.LCRef = $scope.GetDataDoubleClickMaster[0].LCRef;
            $scope.productNew.PaymentTermName = $scope.GetDataDoubleClickMaster[0].PaymentTermName;
            $scope.productNew.LCOpeningBank = $scope.GetDataDoubleClickMaster[0].LCOpeningBank;
            $scope.productNew.PODate = $scope.GetDataDoubleClickMaster[0].PODate;
            $scope.productNew.ContractId = $scope.GetDataDoubleClickMaster[0].ContractId;
            $scope.productNew.PartyName = $scope.GetDataDoubleClickMaster[0].PartyName;
            $scope.productNew.LCExpiryDate = $scope.GetDataDoubleClickMaster[0].LCExpiryDate;
            $scope.productNew.LCOpeningDate = $scope.GetDataDoubleClickMaster[0].LCOpeningDate;
            $scope.productNew.CustomerName = $scope.GetDataDoubleClickMaster[0].CustomerName;
            $scope.PurchaseDocAcceptance.Id = $scope.GetDataDoubleClickMaster[0].Id;
            $scope.PurchaseDocAcceptance.AcceptanceDate = $scope.GetDataDoubleClickMaster[0].AcceptanceDate;
            $scope.PurchaseDocAcceptance.AcceptanceNo = $scope.GetDataDoubleClickMaster[0].AcceptanceNo;
            $scope.PurchaseDocAcceptance.Remarks = $scope.GetDataDoubleClickMaster[0].Remarks;
            $scope.PurchaseDocAcceptance.CurrencyName = $scope.GetDataDoubleClickMaster[0].CurrencyName;
            $scope.PurchaseDocAcceptance.CurrencyId = $scope.GetDataDoubleClickMaster[0].CurrencyId;

            $scope.PurchaseDocAcceptance.AcceptancePaymentSource = $scope.GetDataDoubleClickMaster[0].AcceptancePaymentSource;
            $scope.PurchaseDocAcceptance.IsNonCreditable = $scope.GetDataDoubleClickMaster[0].IsNonCreditable;
            $scope.PurchaseDocAcceptance.DueDate = $scope.GetDataDoubleClickMaster[0].DueDate;
            $scope.PurchaseDocAcceptance.InvoiceDate = $scope.GetDataDoubleClickMaster[0].InvoiceDate;
            $scope.PurchaseDocAcceptance.VoucherId = $scope.GetDataDoubleClickMaster[0].VoucherId;
            $scope.PurchaseDocAcceptance.ServiceVoucherId = $scope.GetDataDoubleClickMaster[0].ServiceVoucherId;
            $scope.PurchaseDocAcceptance.PartyId = $scope.GetDataDoubleClickMaster[0].PartyId;
            $scope.PurchaseDocAcceptance.PartyPlantId = $scope.GetDataDoubleClickMaster[0].PartyPlantId;
        });
    };

    $scope.GetDataDoubleClickDetails = [];
    $scope.getRecordDoubleClickDetail = function (Id) {
        $scope.TotalPOAmount = 0;
        $scope.seletedLST = [];
        $scope.getPOList();
        $scope.GetDataDoubleClickDetails = [];
        $scope.inventoryMaterialListPO = [];

        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseDocumentsAcceptance/GetRecordDoubleClickDetail?Id=' + Id,
        }).then(function successCallback(response) {
            $scope.GetDataDoubleClickDetails = response.data;
            $scope.inventoryMaterialListPO = $scope.GetDataDoubleClickDetails;
            //$scope.inventoryMaterialListPO.TaxList = [];

            if ($scope.GridListPO.length > 0) {
                for (var j = 0; j < $scope.inventoryMaterialListPO.length; j++) {
                    for (var i = 0; i < $scope.GridListPO.length; i++) {
                        if ($scope.inventoryMaterialListPO[j].POID === $scope.GridListPO[i].Id) {
                            $scope.seletedLST.push($scope.GridListPO[i]);
                            var i = $scope.GridListPO.length;
                            while (i--) {
                                if ($scope.inventoryMaterialListPO[j].POID === $scope.GridListPO[i].Id) {
                                    $scope.GridListPO.splice(i, 1);
                                }
                            }
                        }

                    }
                }
            }
            else {
                $scope.inventoryMaterialListPO = $scope.GetDataDoubleClickDetails;
            }
            getSavedServicePODetailList(Id);
            $scope.getMaterialTax(Id);
            getServiceChargeList(Id);
            $scope.CalculateMaterialAmount();

            //var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.serviceList), 'Amount');
            //var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');

            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                $scope.TotalPOAmount += $scope.inventoryMaterialListPO[i].TrnAmount;
            }

        });
    };
    $scope.SavedServicePODetailList = [];
    function getSavedServicePODetailList(acceptanceID) {
        $scope.SavedServicePODetailList = [];
        $http.get('Products/PurchaseDocumentsAcceptance/GetSavedServicePOList?acceptanceID=' + acceptanceID)
            .then(function (response) {
                $scope.SavedServicePODetailList = response.data;

                if ($scope.GridListPO.length > 0) {
                    for (var j = 0; j < $scope.SavedServicePODetailList.length; j++) {
                        for (var i = 0; i < $scope.GridListPO.length; i++) {
                            if ($scope.SavedServicePODetailList[j].ServicePOMasterId === $scope.GridListPO[i].Id) {
                                $scope.seletedLST.push($scope.GridListPO[i]);
                                var i = $scope.GridListPO.length;
                                while (i--) {
                                    if ($scope.SavedServicePODetailList[j].ServicePOMasterId === $scope.GridListPO[i].Id) {
                                        $scope.GridListPO.splice(i, 1);
                                    }
                                }
                            }

                        }
                    }
                }

                //GetServicePOAndAckTax(acceptanceID);
                getACKTaxList(acceptanceID);
            });
    }

    $scope.getRecordDoubleClickDetailGRN = function (Id) {
        $scope.TotalGRNAmount = 0;
        $scope.seletedLST = [];
        $scope.GetGRNList();
        $scope.GetSavedGRNList();

        $scope.GetDataDoubleClickDetails = [];
        $scope.inventoryMaterialListPO = [];

    };

    $scope.summaryassignGRNRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalMaterialTranAmount", dataMember: "TotalMaterialTranAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.summaryUnassignGRNRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalMaterialTranAmount", dataMember: "TotalMaterialTranAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.summaryassignPORows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionAmount", dataMember: "TransactionAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.summaryUnassignPORows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionAmount", dataMember: "TransactionAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.getMaterialTax = function (id) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: 'Products/PurchaseDocumentsAcceptance/GetPurchaseDocAcceptanceTax?Id=' + id
        }).then(function (response) {
            $scope.TaxList = response.data;
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                //var linepk = $scope.inventoryMaterialListPO[i].PODetailsID;
                var linepk = $scope.inventoryMaterialListPO[i].AcceptenceDetailId;

                var list = gettaxlist(linepk);
                $scope.inventoryMaterialListPO[i].TaxList = list;
            }
        });
    };

    function gettaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.TaxList.length; i++) {
            if ($scope.TaxList[i].PurchaseDocAcceptanceDetailId === linepk) {
                result.push($scope.TaxList[i]);
            }
        }
        return result;
    }

    function makeServicetaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.accServiceTaxList.length; i++) {
            if ($scope.accServiceTaxList[i].PurchaseDocAcceptanceId === linepk) {
                result.push($scope.accServiceTaxList[i]);
            }
        }
        return result;
    }

    $scope.getServiceTax = function (id) {
        $scope.accServiceTaxList = [];
        $http({
            method: 'GET',
            url: 'Products/PurchaseDocumentsAcceptance/GetPurchaseDocAcceptanceServiceTax?Id=' + id
        }).then(function successCallback(response) {
            $scope.accServiceTaxList = response.data;

            for (var i = 0; i < $scope.serviceList.length; i++) {
                var linepk = $scope.serviceList[i].PurchaseDocAcceptanceId;
                var list1 = makeServicetaxlist(linepk);
                $scope.serviceList[i].accServiceTaxList = list1;
            }

        });
    }

    $scope.GetMaterialById();
    $scope.GetServiceDetails = [];
    $scope.GetService = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceServiceList?Id=' + Id,
        }).then(function successCallback(response) {
            $scope.GetServiceDetails = response.data;
            $scope.acceptanceChargesCheckedList = $scope.GetServiceDetails;
            if (baseService.arrayLength($scope.acceptanceChargesCheckedList) > 0) {
                for (var i = 0; i < $scope.acceptanceChargesCheckedList.length; i++) {
                    if ($scope.acceptanceChargesCheckedList[i].CurrencyId === $scope.acceptanceChargesCheckedList[i].BankCurrencyId) {
                        $scope.acceptanceChargesCheckedList[i].BankAmountFlag = true;
                    }
                }
            }
        });
    };

    $scope.ChargesTaxList = [];
    $scope.GetAcceptanceChargesTaxList = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceChargesTaxList?Id=' + Id,
        }).then(function successCallback(response) {
            $scope.ChargesTaxList = response.data;

        });
    };

    $scope.recorddoubleclickFromMasterGrid = function ($event) {
        var x = $event.data.Id;
        $scope.Id = $event.data.Id;
        $scope.productNew = $event.data;
        $scope.productNew.PurchaseLCNO = $event.data.PurchaseLCId;
        $scope.productNew.PurchaseLCId = $event.data.PurchaseLCId;
        $scope.PurchaseDocAcceptance.PurchaseLCId = $event.data.PurchaseLCId;
        $scope.productNew.LCRef = $event.data.LCRef;
        $scope.productNew.PaymentTermName = $event.data.PaymentTermName;
        $scope.LCOpeningBank = $event.data.LCOpeningBank;
        $scope.productNew.PODate = $event.data.PODate;
        $scope.productNew.ContractId = $event.data.ContractId;
        $scope.productNew.ContractNo = $event.data.ContractNo;
        $scope.productNew.PartyName = $event.data.PartyName;
        $scope.productNew.LCExpiryDate = $filter("dateFiltering")($event.data.LCExpiryDate);
        $scope.productNew.LCOpeningDate = $filter("dateFiltering")($event.data.LCOpeningDate);
        $scope.productNew.CustomerName = $event.data.CustomerName;
        $scope.PurchaseDocAcceptance.Id = $event.data.Id;
        $scope.PurchaseDocAcceptance.AcceptanceDate = $filter("dateFiltering")($event.data.AcceptanceDate);
        $scope.PurchaseDocAcceptance.AcceptanceNo = $event.data.AcceptanceNo;
        $scope.PurchaseDocAcceptance.Remarks = $event.data.Remarks;
        $scope.PurchaseDocAcceptance.CurrencyName = $event.data.CurrencyName;
        $scope.PurchaseDocAcceptance.CurrencyId = $event.data.CurrencyId;
        $scope.PurchaseDocAcceptance.OBCurrencyCode = $event.data.OBCurrencyCode;
        $scope.PurchaseDocAcceptance.LCOBCurrencyId = $event.data.LCOBCurrencyId;
        $scope.PurchaseDocAcceptance.BankCurrencyId = $event.data.LCOBCurrencyId;

        $scope.PurchaseLCNo = $event.data.PurchaseLCId;
        $scope.productNew.CurrencyId = $event.data.CurrencyId;
        $scope.Tenure = $event.data.Tenure;
        $scope.PurchaseDocAcceptance.OpeningBankMasterId = $event.data.OpeningBankMasterId;
        $scope.PurchaseDocAcceptance.AcceptancePaymentSource = $event.data.AcceptancePaymentSource;
        $scope.PurchaseDocAcceptance.IsNonCreditable = $event.data.IsNonCreditable;
        $scope.PurchaseDocAcceptance.AcceptanceRate = $event.data.AcceptanceRate;
        $scope.PurchaseDocAcceptance.DueDate = $filter("dateFiltering")($event.data.DueDate);
        $scope.PurchaseDocAcceptance.InvoiceDate = $filter("dateFiltering")($event.data.InvoiceDate);
        $scope.PurchaseDocAcceptance.InvoiceNo = $event.data.InvoiceNo;
        $scope.PurchaseDocAcceptance.VoucherId = $event.data.VoucherId;
        $scope.PurchaseDocAcceptance.ServiceVoucherId = $event.data.ServiceVoucherId;
        $scope.PurchaseDocAcceptance.PlantId = $event.data.PlantId;
        $scope.PurchaseDocAcceptance.CompanyId = $event.data.CompanyId;
        $scope.PurchaseDocAcceptance.CompanyGroupId = $event.data.CompanyGroupId;
        $scope.PurchaseDocAcceptance.PartyId = $event.data.PartyId;
        $scope.PurchaseDocAcceptance.PartyPlantId = $event.data.PartyPlantId;
        $scope.PurchaseDocAcceptance.AcceptanceAmount = $event.data.AcceptanceAmount;

        $scope.productNew.AcceptanceFirst = $event.data.AcceptanceFirst;
        if ($scope.productNew.AcceptanceFirst == 'No') {
            $scope.productNew.GRNFirst = 'Yes';
            $scope.productNew.AcceptanceFirst = 'No';

            $scope.getRecordDoubleClickDetailGRN(x);
        }
        else {
            $scope.productNew.GRNFirst = 'No';
            $scope.productNew.AcceptanceFirst = 'Yes';
            $scope.getRecordDoubleClickDetail(x);
        }

        $scope.getLCdetailList($scope.productNew.PurchaseLCNO);
        // $scope.getRecordDoubleClickDetail(x);
        //$scope.getMaterialTax($scope.PurchaseDocAcceptance.Id);
        $scope.GetService(x);
        $scope.GetAcceptanceChargesTaxList(x);
        $scope.getServiceTax($scope.PurchaseDocAcceptance.Id);
        //getServiceChargeList($scope.Id);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.sqlInStatement = null;
    $scope.seletedLST = [];
    $scope.selectedData = function () {
        if ($scope.productNew.AcceptanceFirst == 'Yes') {

            if (baseService.arrayLength($scope.seletedLST) == 0) {
                $scope.seletedLST = [];
            }
            var i = $scope.GridListPO.length;
            while (i--) {
                if ($scope.GridListPO[i].Active === true) {
                    $scope.TotalPOAmount += $scope.GridListPO[i].TotalMaterialTranAmount;
                    $scope.seletedLST.push($scope.GridListPO[i]);
                    $scope.GridListPO[i].Active === false;
                    $scope.GridListPO.splice(i, 1);
                }
            }
        }
        else {
            if (baseService.arrayLength($scope.seletedLST) == 0) {
                $scope.seletedLST = [];
            }
            var i = $scope.GridListPO.length;
            while (i--) {
                if ($scope.GridListPO[i].Active === true) {
                    $scope.TotalGRNAmount += $scope.GridListPO[i].TotalMaterialTranAmount;
                    $scope.seletedLST.push($scope.GridListPO[i]);
                    $scope.GridListPO[i].Active === false;
                    $scope.GridListPO.splice(i, 1);
                }
            }

            if ($scope.seletedLST.length > 0) {
                var uniqueInventoryReceiveId = removeDuplicates($scope.seletedLST, 'Id');
                var wcInventoryReceiveId = "";
                if (uniqueInventoryReceiveId.length > 0) {
                    wcInventoryReceiveId = "IN (";
                    wcInventoryReceiveId += Array.prototype.map.call(uniqueInventoryReceiveId, function (item) { return "'" + item.Id + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcInventoryReceiveId;

                //// for POId
                var uniquePOId = removeDuplicates($scope.seletedLST, 'POId');
                var wcPOId = "";
                if (uniquePOId.length > 0) {
                    wcPOId = "IN (";
                    wcPOId += Array.prototype.map.call(uniquePOId, function (item) { return "'" + item.POId + "'"; }).join(",") + ")";
                }
                $scope.POsqlInStatement = wcPOId;
            }
            $scope.GetGRNDetailData();
        }
    }

    function checkExistPOId(list, POId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].POId == POId) {
                return false;
            }
        }
        return true;
    }

    $scope.selectedRoveData = function () {
        if ($scope.productNew.AcceptanceFirst == 'Yes') {
            if (baseService.arrayLength($scope.inventoryMaterialListPO) > 0) {
                for (var i = 0; i < $scope.seletedLST.length; i++) {
                    for (var j = 0; j < $scope.inventoryMaterialListPO.length; j++) {
                        if ($scope.inventoryMaterialListPO[j].POID === $scope.seletedLST[i].Id) {
                            ShowResult('First delete materials for this PO');
                            return false;
                        }
                        else if ($scope.seletedLST[i].Active === true) {
                            // $scope.GridListPO[i].Active = false;
                            //var x = "#seletedLSTGrid" + seletedLSTGrid;
                            var gridObj = $("#seletedLSTGrid").data("ejGrid");
                            $scope.data = gridObj.getSelectedRecords()[0];
                            // $scope.GridListPO.push($scope.seletedLST[i]);
                            //$scope.seletedLST.splice($scope.seletedLST[i], 1);
                            var i = $scope.seletedLST.length;
                            while (i--) {
                                if ($scope.seletedLST[i].Active === true) {
                                    //$scope.PurchaseDocAcceptance.TotalGRNAmount = $scope.PurchaseDocAcceptance.TotalGRNAmount - $scope.seletedLST[i].TotalMaterialTranAmount;
                                    $scope.GridListPO.push($scope.seletedLST[i]);
                                    $scope.seletedLST.splice(i, 1);
                                }
                            }
                            $scope.DeleteACPOmapTabledata($scope.data.Id);
                        }
                    }
                }
            }
            else {
                for (var i = 0; i < $scope.seletedLST.length; i++) {
                    if ($scope.seletedLST[i].Active === true) {
                        for (var j = 0; j < $scope.ServicePODetailList.length; j++) {
                            if ($scope.ServicePODetailList[j].ServicePOMasterId === $scope.seletedLST[i].Id) {
                                ShowResult('First delete items for this PO');
                                return false;
                            }
                            else if ($scope.seletedLST[i].Active === true) {
                                // $scope.GridListPO[i].Active = false;
                                //var x = "#seletedLSTGrid" + seletedLSTGrid;
                                var gridObj = $("#seletedLSTGrid").data("ejGrid");
                                $scope.data = gridObj.getSelectedRecords()[0];
                                // $scope.GridListPO.push($scope.seletedLST[i]);
                                //$scope.seletedLST.splice($scope.seletedLST[i], 1);
                                var i = $scope.seletedLST.length;
                                while (i--) {
                                    if ($scope.seletedLST[i].Active === true) {
                                        $scope.seletedLST[i].Active = false;
                                        $scope.GridListPO.push($scope.seletedLST[i]);
                                        $scope.seletedLST.splice(i, 1);
                                    }
                                }
                                //$scope.DeleteACPOmapTabledata($scope.data.Id);
                            }
                        }
                    }
                }
            }
        }
        else {
            for (var i = 0; i < $scope.seletedLST.length; i++) {
                if ($scope.seletedLST[i].Active === true) {
                    var gridObj = $("#seletedLSTGrid").data("ejGrid");
                    $scope.data = gridObj.getSelectedRecords()[0];
                    var i = $scope.seletedLST.length;
                    while (i--) {
                        if ($scope.seletedLST[i].Active === true) {

                            $scope.GridListPO.push($scope.seletedLST[i]);
                            $scope.seletedLST.splice(i, 1);
                        }
                    }
                    $scope.DeleteACPOmapTabledata($scope.data.Id);
                }
            }
        };
    }

    $scope.GetGRNDetailData = function () {
        $scope.inventoryMaterialListPO = [];
        $http.get('Products/PurchaseDocumentsAcceptance/GetGRNDetailData?inveReveiveId=' + $scope.sqlInStatement + '&PurchaseDocAcceptanceId=' + $scope.Id)
            .then(function (response) {
                $scope.inventoryMaterialListPO = response.data.Rows;

                if (!baseService.isUndefinedOrNull($scope.Id)) {
                    $scope.getMaterialTax($scope.Id);
                    getServiceChargeList($scope.Id);
                }

                $scope.getOtherAcptQtyValue()
            });
    };

    $scope.OtherTotalAcptValue = 0;
    $scope.TotalAcptValue = 0;
    $scope.OtherTotalQty = 0;
    $scope.getOtherAcptQtyValue = function () {
        $scope.OtherTotalAcptValue = 0;
        $scope.OtherTotalQty = 0;
        $http({
            method: "GET",
            url: 'Products/PurchaseDocumentsAcceptance/GetOtherAcptQtyValue?POId=' + $scope.POsqlInStatement + '&PurchaseDocAcceptanceId=' + $scope.Id
        }).then(function (response) {
            for (var i = 0; i < response.data.length; i++) {
                $scope.OtherTotalAcptValue += response.data[i].OtherTotalAcptValue;
                //$scope.OtherTotalQty += response.data[i].OtherTotalQty;
            }
            $scope.TotalAcptValue = $scope.PurchaseDocAcceptance.AcceptanceAmount + $scope.OtherTotalAcptValue;
        });
    };


    function checkExistsPOId(list, POId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].POId == POId) {
                return true;
            }
        }
        return false;
    }

    $scope.recorddoubleclickPO = function ($event) {
        var x = $event;
        var Id = x.data.Id;
    }

    $scope.POMaterialTaxList = [];
    $scope.POMaterialTaxListData = [];
    $scope.GetPOMaterialTaxData = function () {

        $scope.POMaterialTaxListData = [];
        $http({
            method: "GET",
            url: 'Products/GoodsReceiveNote/GetReceiveTaxListPO?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.POMaterialTaxListData = response.data;
            for (var i = 0; i < $scope.POMaterialTaxListData.length; i++) {
                var getrow = getPOMaterialtaxlist($scope.POMaterialTaxListData[i]);
                // var getrow = ($filter('filter')($scope.POMaterialTaxList, { 'PODetailId': $scope.POMaterialTaxListData[i].PODetailId, 'InventoryReceiveDetailId': $scope.POMaterialTaxListData[i].InventoryReceiveDetailId, 'TaxCategoryId': $scope.POMaterialTaxListData[i].TaxCategoryId }));
                if (getrow.length == 0) {
                    $scope.POMaterialTaxList.push($scope.POMaterialTaxListData[i]);
                }
            }
        });
    };

    function getPOMaterialtaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
            if ($scope.POMaterialTaxList[i].PODetailId === linepk.PODetailId && $scope.POMaterialTaxList[i].TaxCategoryId === linepk.TaxCategoryId) {
                result.push($scope.POMaterialTaxListData[i]);
            }
        }
        return result;
    }

    $scope.inventoryMaterialListPO = [];
    $scope.inventoryMaterialListPO1 = [];
    function GetInventoryMaterialListByPO1(inveReveiveId) {
        $scope.inventoryMaterialListPO1 = [];
        var gridObj = $("#GridAcceptance").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListByOnlyPO?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialListPO1 = response.data.Rows;

                if (baseService.arrayLength($scope.inventoryMaterialListPO1) > 0) {
                    for (var i = 0; i < $scope.inventoryMaterialListPO1.length; i++) {
                        for (var j = 0; j < $scope.inventoryMaterialListPO.length; j++) {
                            if ($scope.inventoryMaterialListPO1[i].POID == $scope.inventoryMaterialListPO[j].POID && $scope.inventoryMaterialListPO1[i].InventoryReceiveDetailId == $scope.inventoryMaterialListPO[j].InventoryReceiveDetailId) {
                                $scope.inventoryMaterialListPO1.splice(i, 1);
                            }
                        }
                    }
                }

            });
        $scope.GetPOMaterialTaxData();
    }

    $scope.POmaterialDetailsPOPUP = function (obj) {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                if ($scope.productNew.AcceptanceFirst == 'Yes') {
                    if (obj.data.Flag == "MaterialPO") {
                        GetInventoryMaterialListByPO1(obj.data.Id);
                        angular.element(document.querySelector('#ListOfRequisition')).modal('show');
                    }
                    else {
                        getServicePODetailList(obj.data.Id);
                        angular.element(document.querySelector('#ListOfServicePODetail')).modal('show');
                    }
                }

            }
            else {
                throw "First fillup all the required fields.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClosePOMaterial = function () {
        angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
    };

    $scope.ItemSelectToSave = function () {
        try {
            $scope.inventoryMaterialListPO = [];
            if (baseService.arrayLength($scope.inventoryMaterialListPO1) > 0) {
                angular.forEach($scope.inventoryMaterialListPO1, function (a) {

                    if (checkExist($scope.inventoryMaterialListPO, a.POID, a.InventoryReceiveDetailId) === false) {
                        if (a.Active) {
                            a.TrnAmount = a.Qty * a.TransactionRate;
                            a.Balance = 0;
                            if (a.POQty < (a.GRNRcvQty + parseFloat(a.Qty))) {
                                a.Balance = a.POQty - a.GRNRcvQty;
                                //ShowResult('Current quantity can not grater than balance qty!', 'failure');
                                //ShowResult(a.UserName + ' has no Balance Qty', 'failure', 'ListOfRequisition');
                                throw a.UserName + ' has no Balance Qty';
                            }
                            else {
                                a.TaxList = [];
                                $scope.inventoryMaterialListPO.push(a);
                            }
                        }
                    }
                });

                if (baseService.arrayLength($scope.inventoryMaterialListPO) > 0) {

                    $scope.Save1();
                    angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
                }
            }
            else
                // ShowResult('PO Material Already Added', 'failure', 'ListOfRequisition');
                throw 'PO Material Already Added';
        } catch (e) {
            ShowResult(e, 'failure', 'ListOfRequisition');
        }
    };

    function checkExist(list, poid, inventoryReceiveDetailId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].POID === poid && list[i].InventoryReceiveDetailId === inventoryReceiveDetailId) {
                return true;
            }
        }
        return false;
    }

    $scope.RemoveMaterialPOItem = function (data, index) {
        try {

            $scope.AcceptenceDetailId = data.AcceptenceDetailId;
            $scope.POID = data.POID;
            $scope.PODetailsID = data.PODetailsID;
            $scope.TransactionQty = data.TransactionQty;
            $scope.bIndex = index;

            $scope.message = 'Are you sure want to delete permanently [ ' + data.UserName + ' ]';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteLineItem = function () {
        //if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.serviceList) === 0) {

        if (!baseService.isUndefinedOrNull($scope.AcceptenceDetailId)) {
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/DeleteLineItem?id=' + $scope.AcceptenceDetailId + '&POID=' + $scope.POID + '&PODetailsID=' + $scope.PODetailsID + '&Qty=' + $scope.TransactionQty,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.getRecordDoubleClickDetail($scope.Id);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            $scope.inventoryMaterialListPO.splice($scope.bIndex, 1);
        }
        // ShowResult('First delete all line item.', 'failure');
    };

    $scope.AllTabPrint = function (z) {

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/PurchaseAcceptanceReport?PDACId=" + data.Id;
    };

    $scope.GridLCDetails = [];
    $scope.getLCdetailList = function (LCID) {

        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/LCDetails?LCID=' + LCID
        }).then(function successCallback(response) {

            if (baseService.arrayLength(response.data) > 0) {
                $scope.productNew = response.data[0];
                $scope.productNew.PurchaseLCId = response.data[0].PurchaseLCNO;
                if ($scope.productNew.AcceptanceFirst == 'No') {
                    $scope.productNew.GRNFirst = 'Yes';
                    $scope.productNew.AcceptanceFirst = 'No';


                } else {
                    $scope.productNew.GRNFirst = 'No';
                    $scope.productNew.AcceptanceFirst = 'Yes';

                }
            }
        });
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.index = -1;
    $scope.staus = true;
    $scope.enableid = true;
    $scope.Change = function (event, index, x) {

        if (event.currentTarget.checked) {
            $scope.index = index;
            x.enableid = false;
        }

        else {
            x.enableid = true;
        }
    };

    $scope.GridSavedPOList = [];
    $scope.SavedPOList = function (LCID) {
        $scope.GridListPO1 = [];
        $scope.seletedLST = [];
        if (baseService.isUndefinedOrNull(LCID)) {
            for (var l = 0; l < $scope.GridListPO.length; l++) {
                var Ids = $filter("filter")($scope.GridListPO, { "Id": $scope.GridListPO1[l].Id });
                if (Ids.length === 0) {
                    $scope.GridListPO.push($scope.GridListPO1[l]);
                }

            }

        }
        else {

            var PoType = 'PO';
            $http({
                method: "GET",
                dataType: 'JSON',
                url: 'Products/PurchaseDocumentsAcceptance/SavedPOList?AcceptanceID=' + LCID,
            }).then(function successCallback(response) {
                $scope.GridSavedPOList = response.data;

                if ($scope.inventoryMaterialListPO.length === 0) {
                    for (var l = 0; l < $scope.GridSavedPOList.length; l++) {
                        var Ids = $filter("filter")($scope.seletedLST, { "Id": $scope.GridSavedPOList[l].Id });
                        if (Ids.length === 0) {
                            $scope.seletedLST.push($scope.GridSavedPOList[l]);
                        }

                    }
                    for (var i = 0; i < $scope.GridListPO.length; i++) {
                        for (var j = 0; j < $scope.seletedLST.length; j++) {
                            if ($scope.GridListPO[i].Id === $scope.seletedLST[j].Id) {
                                var k = $scope.GridListPO.length;
                                while (k--) {
                                    if ($scope.GridListPO[k].Id === $scope.seletedLST[j].Id) {
                                        $scope.GridListPO.splice(k, 1);
                                    }
                                }
                            }

                        }

                    }

                }
                else {
                    if (baseService.arrayLength($scope.GridListPO) > 0) {
                        for (var i = 0; i < $scope.GridListPO.length; i++) {
                            for (var j = 0; j < $scope.GridSavedPOList.length; j++) {

                                if (baseService.arrayLength($scope.GridListPO > 0)) {
                                    if ($scope.GridListPO[i].Id === $scope.GridSavedPOList[j].Id) {
                                        $scope.seletedLST.push($scope.GridSavedPOList[j]);
                                        var k = $scope.GridListPO.length;
                                        while (k--) {
                                            if ($scope.GridListPO[k].Id === $scope.GridSavedPOList[j].Id) {
                                                $scope.GridListPO.splice(k, 1);
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                }


            });

        }



    };
    $scope.closeAccepServiceChargePopUp = function () {
        $scope.serviceModel = {};
        // $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };

    $scope.DeleteACPOmapTabledata = function (x) {

        if (!baseService.isUndefinedOrNull(x)) {
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/DeleteACPOmapTabledata?id=' + $scope.PurchaseDocAcceptance.Id + '&POID=' + $scope.data.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    var id = response.data.Id;
                    GetInventoryMaterialListByPO1(x.AcceptenceDetailId);
                    // ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }

    };

    $scope.BankAmountFlag = false;
    $scope.ChargesIndex = -1;
    $scope.ChangeChargesBank = function (currencyId, index) {
        $scope.ChargesIndex = index;
        if (currencyId === $scope.acceptanceChargesCheckedList[$scope.ChargesIndex].CurrencyId) {
            $scope.acceptanceChargesCheckedList[$scope.ChargesIndex].BankAmount = $scope.acceptanceChargesCheckedList[$scope.ChargesIndex].Amount;
        } else {
            $scope.acceptanceChargesCheckedList[$scope.ChargesIndex].BankAmount = 0;
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.PurchaseDocAcceptance = {
            Id: null,
            CompanyGroupId: null,
            CompanyId: null,
            PlantId: null,
            AcceptanceNo: null,
            EntryDate: null,
            AcceptanceDate: null,
            POId: null,
            CheckedBy: null,
            CheckedByStatus: null,
            AuthorizedBy: null,
            AuthorizedByStatus: null,
            Remarks: null,
            PurchaseLCId: null,
            AcceptancePaymentSource: null,
            DueDate: null,
            InvoiceDate: null,
            InvoiceNo: null,
            VoucherId: null,
            ServiceVoucherId: null,
            PartyId: null,
            PartyPlantId: null,
            IsNonCreditable: false,
            OBCurrencyCode: null,
            LCOBCurrencyId: null,
            AcceptanceAmount: 0
        };
        $scope.PurchaseDocAcceptance.Id = null;
        $scope.Id = "";
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
            , GRNDate: $filter("dateFiltering")(Date.now())
            , PurchaseLCNO: null
            , LCOpeningBank: null
            , PODate: null
            , LCOpeningDate: null
            , ContractId: null
            , PartyName: null
            , LCEntryDate: null
            , LCExpiryDate: null
            , LCRef: null
            , CurrencyId: null
            , ContractNo: null
            , ContractId: null
            , TaxOption: 'Yes'
            , TaxOptionMat: 'Yes'
            , TaxOptionService: 'Yes'
            , TaxOptionServiceModify: 'Yes'
            , TaxOptionAddiTax: 'Yes'
        };

        $scope.Action = 'Save';
        $scope.seletedLST = [];
        $scope.GridListPO = [];
        $scope.GridListPO1 = [];
        $scope.GridSavedPOList = [];
        $scope.GetDataDoubleClickDetails = [];
        $scope.inventoryMaterialListPO = [];
        $scope.receiveTaxList = [];
        $scope.getReceiveTaxListPO = [];
        $scope.taxCategoryList = [];
        $scope.serviceList = [];
        $scope.chargesListPO = [];
        $scope.storageList = [];
        // $scope.currencyList = [];
        $scope.accServiceTaxList = [];
        $scope.detailModelSave = [];
        $scope.inventoryMaterialListPOnew = [];
        $scope.chargesListPOnew = [];
        $scope.AcceptanceChargesList = [];
        $scope.acceptanceChargesCheckedList = [];
        $scope.GetServiceDetails = [];
        $scope.GetDataDoubleClickMaster = [];
        $scope.GetMaterialByIdList = [];
        $scope.GridAcceptanceServiceList = [];
        $scope.GridAcceptanceListDetail = [];
        $scope.GridLCDetails = [];
        $scope.inventoryMaterialListPO = [];
        $scope.acceptanceTaxList = [];
        $scope.gridAcceptanceList();
        //$scope.setTabAcceptenceList(1);
        $scope.TaxAction = 'Save';
        $scope.productNew.AcceptanceFirst = null;
        $scope.ServicePODetailList = [];
        $scope.SavedServicePODetailList = [];
        $scope.TotalGRNAmount = 0;
        $scope.TotalPOAmount = 0;
        $scope.CurrentQty = 0;
        $scope.TotalAcptValue = 0;
    }
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
        , GRNDate: $filter("dateFiltering")(Date.now())
        , PurchaseLCNO: null
        , LCOpeningBank: null
        , PODate: null
        , LCOpeningDate: null
        , ContractId: null
        , PartyName: null
        , LCEntryDate: null
        , LCExpiryDate: null
        , LCRef: null
        , CurrencyId: null
        , ContractNo: null
        , ContractId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionServicePOModify: 'Yes'
        , TaxOptionAddiTax: 'Yes'
    };
    // #endregion

    // #region Service

    $scope.AcceptanceServiceChargePopUp = function () {
        $scope.productNew.TaxOptionService = 'Yes';
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , PurchaseDocAcceptanceId: $scope.PurchaseDocAcceptance.Id
            , CurrencyName: $scope.PurchaseDocAcceptance.CurrencyName
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: 0
            , Amount: null
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable

        };
        angular.element(document.querySelector('#AcceptServiceChargePopUp')).modal('show');
    };
    $scope.serviceCboList = [];
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceCboList = response.data;
        });
    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.AccServicetaxCategoryList = [];
        angular.element(document.querySelector('#AcceptServiceChargePopUp')).modal('hide');
    };

    $scope.AccServicetaxCategoryList = [];
    $scope.addServiceTax = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId)) {
                throw "Service is required.";
            }
            if (baseService.isUndefinedOrNull($scope.serviceModel.Amount)) {
                throw "Amount is required.";
            }
            $scope.changeService();
            var data = {
                TotalAmount: 0,
                Id: null,
                HSNCode: $scope.HSNCode,
                HSNCodeId: null,
                UserName: null,
                TaxCategoryId: null,
                SpecialTaxId: null
            };
            $scope.AccServicetaxCategoryList.push(data);
        } catch (e) {
            ShowResult(e, 'failure', 'AcceptServiceChargePopUp');
        }

    };

    $scope.taxCatList = [];

    accountService.getTaxCategoryCbo(" ", function (result) {
        $scope.taxCategoryList = result;
    });
    $scope.changeService = function () {

        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceCboList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.serviceCboList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCode;
        getTaxCategoryList(hsnCodeId);
    };

    $scope.TaxOption = function (data) {
        $scope.productNew.TaxOption = data;
    };
    $scope.TaxOptionMat = function (data) {
        $scope.productNew.TaxOptionMat = data;

    };
    $scope.TaxOptionService = function (data) {
        $scope.productNew.TaxOptionService = data;

    };
    $scope.TaxOptionCharge = function (data) {
        $scope.productNew.TaxOptionCharge = data;

    };
    $scope.TaxOptionServiceModify = function (data) {
        $scope.productNew.TaxOptionServiceModify = data;

    };
    $scope.TaxOptionServicePOModify = function (data) {
        $scope.productNew.TaxOptionServicePOModify = data;

    };
    $scope.TaxOptionChargeModify = function (data) {
        $scope.productNew.TaxOptionChargeModify = data;

    };
    function getTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            //, url: 'Products/PurchaseDocumentsAcceptance/GetTaxCategoryList?receiveId=' + $scope.productNew.PartyId + '&hsnCodeId=' + hsnCodeId
            , url: 'Commercial/PurchaseLC/GetTaxCategoryListByBankMaster?bankMasterId=' + $scope.PurchaseDocAcceptance.OpeningBankMasterId + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.PurchaseDocAcceptance.AcceptanceDate
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }

    $scope.calculateServiceTaxAmount = function (data) {
        data.TaxAmount = Math.round($scope.serviceModel.Amount * data.Percentage) / 100;
    };

    $scope.calculateTaxAmountForService = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.serviceModel.Amount * data.Percentage) / 100;
    };
    $scope.checkRowValidationService = function (x) {

        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.Amount).toFixed(4) * 100);
            }

        }
    }

    $scope.serviceSave = function () {
        $scope.accServiceTaxList = [];
        if (baseService.arrayLength($scope.taxCategoryList) == 0) {
            ShowResult('Please add Tax', 'failure', 'AcceptServiceChargePopUp');
        }


        if ($scope.serviceModel.ServiceMasterId != null) {
            $scope.serviceModel.ServiceMasterName = $("#ServiceMasterId option:selected").text();
            $scope.serviceModel.CurrencyId = $scope.productNew.CurrencyId;
            $scope.serviceModel.BaseCurrencyId = $scope.baseCurrencyId;
            $scope.serviceModel.State = 'Acceptance';

            $scope.serviceModel.TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.taxCategoryList), 'TaxAmount');
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                $scope.taxCategoryList[i].Id = null;
                $scope.taxCategoryList[i].ServiceMasterId = $scope.serviceModel.ServiceMasterId;
                $scope.accServiceTaxList.push($scope.taxCategoryList[i]);
            }
            $scope.serviceList.push($scope.serviceModel);
            $scope.CalculateMaterialAmount();
            $scope.taxCategoryList = [];
            $scope.serviceModel = {
                Id: null
                , ServiceMasterId: null
                , PurchaseDocAcceptanceId: null
                , CurrencyName: null
                , CurrencyId: null
                , BaseCurrencyId: null
                , DocDate: null
                , TransactionAmount: 0
                , Amount: null
                , TotalTaxAmount: 0
                , ToCurrencyRate: null
                , IsNonCreditable: null
                , State: 'Acceptance'
            };
            $scope.SaveServiceAndServiceTax();
        }
        else
            ShowResult('Please Select Service', 'failure', 'AcceptServiceChargePopUp');
    };

    $scope.SaveServiceAndServiceTax = function () {
        try {
            if (baseService.arrayLength($scope.accServiceTaxList) > 0) {
                for (var i = 0; i < $scope.accServiceTaxList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.accServiceTaxList[i].TaxCategoryId)) {
                        throw "Tax Category is required.";
                    }
                }
            }
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/SaveServiceAndServiceTax',
                data: {
                    'purchaseDocAcceptanceService': $scope.serviceList
                    , 'purchaseDocAcceptanceServiceTax': $scope.accServiceTaxList, 'PurchaseDocAcceptanceId': $scope.PurchaseDocAcceptance.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'AcceptServiceChargePopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'AcceptServiceChargePopUp');
                    $scope.GetService($scope.PurchaseDocAcceptance.Id);
                    $scope.getServiceTax($scope.PurchaseDocAcceptance.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'AcceptServiceChargePopUp');
            }
        } catch (e) {
            ShowResult(e.Message, 'success', 'AcceptServiceChargePopUp');
        }

    }

    $scope.calculateTaxAmountForServiceModify = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.checkRowValidationServiceModify = function (x) {

        for (var i = 0; i < $scope.newaccServiceTaxList.length; i++) {

            if ($scope.newaccServiceTaxList[i].Id === x.Id) {
                $scope.newaccServiceTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }

        }
    }

    function getServiceChargeList(purchaseDocAcceptanceId) {
        $http.get('Products/PurchaseDocumentsAcceptance/GetServiceChargeList?purchaseDocAcceptanceId=' + purchaseDocAcceptanceId)
            .then(function (response) {
                $scope.serviceList = [];
                $scope.serviceList = response.data;
                $scope.getServiceTax(purchaseDocAcceptanceId);
            });
    }

    $scope.newaccServiceTaxList = [];
    $scope.getServiceTaxList1 = function (data, index) {
        $scope.PurchaseDocAcceptanceService = data;

        $scope.LoadTaxButtonClick();
        $scope.PurchaseDocAcceptanceServiceId = data.Id;
        $scope.newaccServiceTaxList = [];
        $scope.ServiceTaxIdex = index;
        $scope.taxAbleAmnt = data.Amount;
        for (var i = 0; i < $scope.accServiceTaxList.length; i++) {
            if ($scope.accServiceTaxList[i].ServiceMasterId == data.ServiceMasterId) {
                $scope.newaccServiceTaxList.push($scope.accServiceTaxList[i]);
            }
        }
        $scope.productNew.TaxOptionServiceModify = 'Yes';
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');

    };

    $scope.closeReceiveTaxPopUpwindow1 = function () {
        $scope.serviceList[$scope.ServiceTaxIdex].TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.newaccServiceTaxList), 'TaxAmount');
        $scope.CalculateMaterialAmount();
        angular.element(document.querySelector('#receiveTaxPopUp1')).modal('hide');
    }

    $scope.deleteModal = function (data, index) {
        $scope.serviceChargesId = data.Id;
        $scope.serviceIndex = index;
        $scope.message = "Are you sure to delete permanently?";
        angular.element(document.querySelector("#RemoveservicePopUp")).modal("show");
    }

    $scope.RemoveServiceCharges = function () {
        if (baseService.isUndefinedOrNull($scope.serviceChargesId)) {
            $scope.serviceList.splice($scope.serviceIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/DeleteServiceCharge?id=' + $scope.serviceChargesId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serviceList.splice($scope.serviceChargesId, 1);
                    getServiceChargeList($scope.Id);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    $scope.totalTax = 0;

    $scope.addServiceTax = function () {
        try {
            var data = {
                TotalAmount: 0,
                Id: null,
                HSNCode: $scope.HSNCode,
                HSNCodeId: null,
                UserName: null,
                TaxCategoryId: null,
                SpecialTaxId: null
            };

            $scope.newaccServiceTaxList.push(data);
            $scope.serviceModel = $scope.PurchaseDocAcceptanceService;
            $scope.changeService();

            if ($scope.AcceptanceCharges.Amount === '' || $scope.AcceptanceCharges.Amount === null || $scope.AcceptanceCharges.Amount === undefined) {
                ShowResult("Enter the Acceptance Charges Amount", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            $scope.isExists = false;
            if (baseService.arrayLength($scope.acceptanceChargesCheckedList) == 0) {

                for (var i = 0; i < $scope.AccChargetaxCategoryList.length; i++) {
                    $scope.AccChargetaxCategoryList[i].AcceptanceServiceId = obj.AcceptanceServiceId;
                    $scope.ChargesTaxList.push($scope.AccChargetaxCategoryList[i]);
                }
                //$scope.AccChargetaxCategoryList = [];
                $scope.isExists = true;


            }
            else if (baseService.arrayLength($scope.acceptanceChargesCheckedList) > 0) {
                if ($scope.TaxAction === 'Save') {
                    for (var i = 0; i < baseService.arrayLength($scope.acceptanceChargesCheckedList); i++) {
                        if ($scope.acceptanceChargesCheckedList[i].AcceptanceServiceId === obj.AcceptanceServiceId) {
                            $scope.isExists = true;
                            ShowResult("Acceptance Charges already exists", 'failure', 'ServiceChargeTaxPopUp');
                            return false;
                        }

                    }
                } else {
                    $scope.isExists = false;
                }
            }

            if ($scope.isExists == false) {
                $scope.acceptanceChargesCheckedList.push(obj);

            }


        } catch (e) {
            ShowResult(e, 'failure', 'ServiceChargeTaxPopUp');
        }
    }

    // #endregion

    accountService.getTaxCategoryCbo(" ", function (result) {
        $scope.taxCategoryList = result;
        $scope.taxCategoryListcbo = result;
    });

    $scope.taxCategoryListcbo = [];
    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryCbo(" ", function (result) {
            $scope.taxCategoryListcbo = result;
        });
    }

    $scope.getReceiveTaxListPO = function (data, index) {
        $scope.productNew.TaxOption = 'Yes';
        $scope.LoadTaxButtonClick();
        $scope.acceptanceTaxList = [];
        $scope.currentMaterialRow = index;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.taxAmnt = data.TaxAmount;
        $scope.PODetailId = data.PODetailsID;
        $scope.AcceptenceDetailId = data.AcceptenceDetailId;
        $scope.receiveTaxList = [];
        $scope.Currency = $("#currency option:selected").text();

        //if (data.TaxList.length > 0) {
        //    $scope.HSNCode = data.TaxList[0].HSNCode;
        //    $scope.receiveTaxList = data.TaxList;
        //}

        if (data.TaxList.length > 0) {
            $scope.HSNCode = data.TaxList[0].HSNCode;
            if (baseService.isUndefinedOrNull(data.TaxList[0].HSNCode)) {
                $scope.HSNCode = data.HSNCode;
            }
            $scope.receiveTaxList = data.TaxList;
        }
        $scope.total = 0;
        $scope.taxCategoryList = [];
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.taxCategoryList.push($scope.receiveTaxList[j]);
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }

        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };

    $scope.closeReceiveTaxPopUpwindow = function () {
        $scope.getMaterialTax($scope.PurchaseDocAcceptance.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.addNewTax = function () {
        var data = {
            TaxAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null,
            PODetailId: $scope.PODetailId,
            PurchaseDocAcceptanceDetailId: $scope.AcceptenceDetailId,
            PurchaseDocAcceptanceId: $scope.PurchaseDocAcceptance.Id
        };
        $scope.receiveTaxList.push(data);
    };

    $scope.calculateMOITaxAmount = function (data) {
        data.TaxAmount = $scope.taxAbleAmnt * data.Percentage / 100;
    };

    $scope.acceptanceTaxList = [];
    $scope.closeReceiveTaxPopUp = function () {
        $scope.inventoryMaterialListPO[$scope.currentMaterialRow].TaxAmount = 0;
        $scope.inventoryMaterialListPO[$scope.currentMaterialRow].TaxAmount = parseFloat($filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount'));
        if ($scope.PurchaseDocAcceptance.IsNonCreditable) {
            $scope.inventoryMaterialListPO[$scope.currentMaterialRow].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[$scope.currentMaterialRow].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[$scope.currentMaterialRow].ChargesTaxTranAmount)
                + parseFloat($scope.inventoryMaterialListPO[$scope.currentMaterialRow].ChargesTranAmount) + parseFloat($filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount'));
        }

        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getrow = ($filter('filter')($scope.acceptanceTaxList, { 'PODetailId': $scope.receiveTaxList[i].PODetailId, 'TaxCategoryId': $scope.receiveTaxList[i].TaxCategoryId }));
            if (getrow.length == 0) {
                $scope.acceptanceTaxList.push($scope.receiveTaxList[i]);
            }
            else {
                for (var j = 0; j < $scope.acceptanceTaxList.length; j++) {
                    if ($scope.acceptanceTaxList[j].PODetailId == $scope.receiveTaxList[i].PODetailId && $scope.receiveTaxList[i].TaxCategoryId == $scope.acceptanceTaxList[j].TaxCategoryId) {
                        $scope.acceptanceTaxList[j].TaxAmount = $scope.receiveTaxList[i].TaxAmount;
                    }
                }
            }
        }
        $scope.SaveMaterialTax();
    }

    $scope.SaveMaterialTax = function () {

        try {

            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/SaveMaterialTax',
                data: {
                    'purchaseDocAcceptanceTax': $scope.acceptanceTaxList, 'PurchaseDocAcceptanceId': $scope.PurchaseDocAcceptance.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    // $scope.PurchaseDocAcceptance.Id = response.data.entity.Id;

                    // $scope.Action = 'Update';

                    $scope.getMaterialTax($scope.PurchaseDocAcceptance.Id);
                    angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e.Message, 'success');
        }

    }

    $scope.PrePurchaseInvoicedata = [];
    $scope.getPrePurchaseInvoice = function (lcId) {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseDocumentsAcceptance/GetPrePurchaseInvoiceList?lcId=' + lcId,
        }).then(function successCallback(response) {
            $scope.PrePurchaseInvoicedata = response.data;
        });
    };

    $scope.PrePurchaseInvoicePopUp = function (lcId) {
        $scope.getPrePurchaseInvoice(lcId);

        angular.element(document.querySelector('#PrePurchaseInvoicePopUp')).modal('show');
    };

    $scope.PrePurchaseInvoicePopUpClose = function () {
        angular.element(document.querySelector('#PrePurchaseInvoicePopUp')).modal('hide');
    };

    $scope.prePurchaseInvoicedoubleclick = function ($event) {
        var x = $event;
        $scope.PurchaseDocAcceptance.PrePurchaseInvoiceId = x.data.Id;
        $scope.PurchaseDocAcceptance.InvoiceNo = x.data.InvoiceNo;
        $scope.PurchaseDocAcceptance.PrePurchaseInvoice = x.data.Id;
        $scope.PurchaseDocAcceptance.InvoiceDate = $filter("dateFiltering")(x.data.InvoiceDate);
        $scope.PrePurchaseInvoicePopUpClose();
    }

    $scope.clearPrePurchaseInvoicePopUp = function () {
        $scope.PurchaseDocAcceptance.PrePurchaseInvoiceId = null;
        $scope.PurchaseDocAcceptance.InvoiceNo = null;
        $scope.PurchaseDocAcceptance.PrePurchaseInvoice = null;
        $scope.PurchaseDocAcceptance.InvoiceDate = null;
    }

    $scope.changeAcceptanceCharges = function () {
        //accountService.getTaxCategoryCbo(" ", function (result) {
        //    $scope.taxCategoryList = result;
        //});
        if (baseService.isUndefinedOrNull($scope.AcceptanceCharges.Id))
            return getTaxCategoryList(hsnCodeId);//$scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.AcceptanceChargesList, function (item) { return item.Id === $scope.AcceptanceCharges.Id; })[0].HSNCodeId;
        getAcceptanceChargesTaxCategoryList(hsnCodeId);
    };

    $scope.taxCategoryList = [];
    function getAcceptanceChargesTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            //, url: 'Products/PurchaseDocumentsAcceptance/GetTaxCategoryList?receiveId=' + $scope.productNew.PartyId + '&hsnCodeId=' + hsnCodeId
            , url: 'Commercial/PurchaseLC/GetTaxCategoryListByBankMaster?bankMasterId=' + $scope.PurchaseDocAcceptance.OpeningBankMasterId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;

        });
    }

    $scope.getChargesTaxList = function (data, index) {
        $scope.productNew.TaxOptionChargeModify = 'Yes';
        $scope.TaxAction = 'Update';
        $scope.LoadTaxButtonClick();
        $scope.changeAcceptanceCharges();

        $scope.AcceptanceChargesList = [];
        $http({
            method: 'GET',
            url: "Products/PurchaseDocumentsAcceptance/GetAcceptanceCharges",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.AcceptanceChargesList = response.data;
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

        if (baseService.arrayLength($scope.currencyList) > 0) {
            for (var i = 0; i < $scope.currencyList.length; i++) {
                if ($scope.currencyList[i].Value === $scope.PurchaseDocAcceptance.CurrencyId) {
                    $scope.AcceptanceCharges.CurrencyId = $scope.currencyList[i].Value;
                }
            }
        }

        //$scope.newChargesTaxList = [];
        $scope.AccChargetaxCategoryList = [];
        $scope.ChargesTaxIdex = index;
        $scope.chargestaxAbleAmnt = data.Amount;
        for (var i = 0; i < $scope.ChargesTaxList.length; i++) {
            if ($scope.ChargesTaxList[i].AcceptanceServiceId == data.AcceptanceServiceId) {
                $scope.AccChargetaxCategoryList.push($scope.ChargesTaxList[i]);
            }
        }

        $scope.AcceptanceCharges.Amount = data.Amount;
        $scope.AcceptanceCharges.Id = data.AcceptanceServiceId;

        angular.element(document.querySelector('#AccepchargesTaxPopUp')).modal('show');

    };

    $scope.closeAccChargesTaxPopUp = function () {
        $scope.acceptanceChargesCheckedList[$scope.ChargesTaxIdex].TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.AccChargetaxCategoryList), 'TaxAmount');
        angular.element(document.querySelector('#AccepchargesTaxPopUp')).modal('hide');
    }

    $scope.calculateTaxAmountForChargesTaxModify = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.chargestaxAbleAmnt * data.Percentage) / 100;
    };

    $scope.checkRowValidationChargesTaxModify = function (x) {

        for (var i = 0; i < $scope.AccChargetaxCategoryList.length; i++) {

            if ($scope.AccChargetaxCategoryList[i].Id === x.Id) {
                $scope.AccChargetaxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.chargestaxAbleAmnt).toFixed(4) * 100);
            }

        }
    }

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
        $scope.actionCompleteUnassign();
    };

    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#seletedLSTGrid").ejGrid("instance");
                var scrollerwidth = $("#Assigned").width();//Obtain the width of the container

                $("#Grid3").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.actionCompleteUnassign = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridListLCPO").ejGrid("instance");
                var scrollerwidth = $("#Unassign").width();//Obtain the width of the container

                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        try {
            if (!baseService.isUndefinedOrNull($scope.productNew.ContractId)) {
                $http({
                    method: 'GET',
                    url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
                }).then(function (response) {
                    $scope.masterOrderCustomerList = response.data;
                });
                angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
            } else {
                throw "Select contract.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N0}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amt", dataMember: "Amt", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.PurchaseDocAcceptance.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.gridAcceptanceList();;
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.calculateAmountForServiceCharge = function (data) {
        data.TotalTaxAmount = 0;
        for (var i = 0; i < $scope.serviceList.length; i++) {
            if ($scope.serviceList[i].Amount > parseFloat($scope.serviceList[i].POAmount) + parseFloat($scope.serviceList[i].GRNServiceAmount)) {

                ShowResult('Amount can not grater than Service Amount');
                $scope.serviceList[i].Amount = 0;
                return false;
            }
        }
        for (var i = 0; i < $scope.accServiceTaxList.length; i++) {

            if ($scope.accServiceTaxList[i].PurchaseDocAcceptanceServiceId == data.Id) {
                $scope.accServiceTaxList[i].TaxAmount = data.Amount * $scope.accServiceTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.accServiceTaxList[i].TaxAmount;
            }
        }
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.accServiceTaxList), 'TaxAmount');

        $scope.CalculateMaterialAmount();

    };

    //#region Service PO 

    $scope.ServicePODetailList = [];
    function getServicePODetailList(inveReveiveId) {
        $scope.ServicePODetailList = [];
        $http.get('Products/PurchaseDocumentsAcceptance/GetServiceListByServicePO?servicepoid=' + inveReveiveId)
            .then(function (response) {
                $scope.ServicePODetailList = response.data;
                if (baseService.arrayLength($scope.ServicePODetailList) > 0) {
                    for (var i = 0; i < $scope.ServicePODetailList.length; i++) {
                        for (var j = 0; j < $scope.SavedServicePODetailList.length; j++) {
                            if ($scope.ServicePODetailList[i].ServicePOMasterId == $scope.SavedServicePODetailList[j].ServicePOMasterId && $scope.ServicePODetailList[i].ServicePODetailId == $scope.SavedServicePODetailList[j].ServicePODetailId) {
                                $scope.ServicePODetailList.splice(i, 1);
                            }
                        }
                    }
                }
                GetServicePOAndAckTax(inveReveiveId);
            });
    }

    $scope.ServicePOAndAckTax = [];
    function GetServicePOAndAckTax(inveReveiveId) {
        $scope.masterId1 = inveReveiveId;
        $http.get('Products/PurchaseOrder/getServicePOTaxForAckSave?POID=' + inveReveiveId)
            .then(function (response) {
                $scope.ServicePOAndAckTax = [];
                $scope.ServicePOAndAckTax = response.data;
                // console.log($scope.ServicePOAndAckTax);
            });
    }

    $scope.refreshTemplateServicePO = function (args) {
        $("#headServicePOchk").ejCheckBox({ "change": CheckBoxSelectAllServicePO });
    };

    function CheckBoxSelectAllServicePO(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }
        var filtered = $("#GridServicePODetail").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ServicePODetailList.length; i++) {
                $scope.ServicePODetailList[i].check = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].check = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridServicePODetail").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveServicePO = function () {
        try {

            var mlsddate = new Date($scope.PurchaseDocAcceptance.AcceptanceDate);
            //  $scope.PurchaseDocAcceptance.POId = $scope.POId;
            $scope.PurchaseDocAcceptance.PurchaseLCId = $scope.productNew.PurchaseLCNO;
            $scope.PurchaseDocAcceptance.PartyId = $scope.productNew.PartyId;
            $scope.PurchaseDocAcceptance.PartyPlantId = $scope.productNew.PartyPlantId;
            $scope.PurchaseDocAcceptance.DueDate = $scope.PurchaseDocAcceptance.InvoiceDate;

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {


                if ($scope.Action === "Save" || $scope.Action === "Update") {

                    $scope.ServicePODetailNewList = [];
                    for (var i = 0; i < $scope.ServicePODetailList.length; i++) {
                        if (isNaN($scope.ServicePODetailList[i].CurrentQty) || baseService.isUndefinedOrNull($scope.ServicePODetailList[i].CurrentQty))
                            $scope.ServicePODetailList[i].CurrentQty = 0;

                        if ($scope.ServicePODetailList[i].check == false && $scope.ServicePODetailList[i].CurrentQty > 0) {
                            ShowResult('Please check line item', 'failure');
                            return false;
                        }

                        if ($scope.ServicePODetailList[i].check == true && baseService.isUndefinedOrNull($scope.ServicePODetailList[i].CurrentQty)) {
                            ShowResult('Enter the qty for check line', 'failure');
                            return false;
                        }
                        if ($scope.ServicePODetailList[i].check == true && $scope.ServicePODetailList[i].CurrentQty === 0) {
                            ShowResult('Enter the qty for checked line', 'failure');
                            return false;
                        }
                        if ($scope.ServicePODetailList[i].check == true) {
                            if ($scope.Action === "Save") {
                                if ($scope.ServicePODetailList[i].Qty < Math.round(($scope.ServicePODetailList[i].CurrentQty + $scope.ServicePODetailList[i].OtherReceived) * 100 + Number.EPSILON) / 100) {
                                    ShowResult('Current Receive can not grater than balance', 'failure');
                                    $scope.ServicePODetailList[i].CurrentQty = '';
                                    return false;
                                }
                            }
                            if (baseService.isUndefinedOrNull($scope.ServicePODetailList[i].CurrentQty) || $scope.ServicePODetailList[i].CurrentQty === 0) {
                                ShowResult('Current Receive can not be Zero(0)', 'failure');
                                $scope.ServicePODetailList[i].CurrentQty = '';
                                return false;
                            }
                            $scope.ServicePODetailList[i].TransactionQty = $scope.ServicePODetailList[i].CurrentQty;
                            $scope.ServicePODetailList[i].MaterialTranRate = $scope.ServicePODetailList[i].Rate;
                            $scope.ServicePODetailList[i].TransactionRate = $scope.ServicePODetailList[i].Rate;
                            $scope.ServicePODetailList[i].MaterialTranAmount = $scope.ServicePODetailList[i].Amount;
                            $scope.ServicePODetailList[i].TotalMaterialTranAmount = $scope.ServicePODetailList[i].TotalAmount;
                            $scope.ServicePODetailNewList.push($scope.ServicePODetailList[i]);
                        }

                    }

                    $http({
                        method: 'POST',
                        url: 'Products/PurchaseDocumentsAcceptance/CreateServicePOAcceptance',
                        data:
                        {
                            'entity': $scope.PurchaseDocAcceptance,
                            'PurchaseDocAcceptanceDetail': $scope.ServicePODetailNewList
                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.PurchaseDocAcceptance.Id = response.data.entity.Id;
                            $scope.gridAcceptanceList();
                            //$scope.setTabAcceptenceList(1);
                            $scope.Action = 'Update';
                            $scope.seletedLST = [];
                            $scope.GridListPO = [];

                            $scope.getRecordDoubleClickDetail($scope.PurchaseDocAcceptance.Id);
                            angular.element(document.querySelector('#ListOfServicePODetail')).modal('hide');
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            throw e;
        }
    };

    $scope.calculateAckRcvAmount = function (data) {
        // if ($scope.Action === 'Save') {

        for (var i = 0; i < $scope.ServicePODetailList.length; i++) {
            if ($scope.ServicePODetailList[i].Qty < Math.round(($scope.ServicePODetailList[i].CurrentQty + $scope.ServicePODetailList[i].OtherReceived) * 100 + Number.EPSILON) / 100) {
                ShowResult('Current Receive can not grater than balance', 'failure');
                $scope.ServicePODetailList[i].CurrentQty = '';
                return false;
            }

            if ($scope.ServicePODetailList[i].ServiceMasterId === data.ServiceMasterId && $scope.ServicePODetailList[i].ServicePODetailId === data.ServicePODetailId) {
                $scope.ServicePODetailList[i].Amount = $scope.ServicePODetailList[i].CurrentQty * $scope.ServicePODetailList[i].Rate;
                if ($scope.ServicePOAndAckTax.length > 0) {
                    for (var i1 = 0; i1 < $scope.ServicePOAndAckTax.length; i1++) {
                        if ($scope.ServicePOAndAckTax[i1].ServicePoDetailId === data.ServicePODetailId) {
                            $scope.ServicePOAndAckTax[i1].TaxAmount = ($scope.ServicePODetailList[i].Amount * $scope.ServicePOAndAckTax[i1].Percentage) / 100;

                        }
                    }

                }
            }

            if ($scope.ServicePODetailList[i].ServicePODetailId === data.ServicePODetailId) {
                $scope.ServicePODetailList[i].TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.ServicePOAndAckTax, { "ServicePoDetailId": data.ServicePODetailId }), "TaxAmount");

                if (isNaN(data.CurrentQty)) data.CurrentQty = 0;
                if (isNaN(data.OtherReceived)) data.OtherReceived = 0;
                $scope.ServicePODetailList[i].Balance = (data.Qty - (data.OtherReceived + parseFloat(data.CurrentQty)));
                if (isNaN($scope.ServicePODetailList[i].Balance))
                    $scope.ServicePODetailList[i].Balance = 0;

            }

        }
        // }


        var gridObj = $("#GridServicePODetail").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }

    $scope.ClosePOService = function () {
        angular.element(document.querySelector('#ListOfServicePODetail')).modal('hide');
    };

    $scope.LoadServicePOTaxButtonClick = function () {
        accountService.getTaxCategoryCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });
    }

    $scope.RemoveServicePOItem = function (data, index) {
        try {

            $scope.LCChargesId = data.Id;
            $scope.bActivityIndex = index;

            $scope.message = 'Are you sure want to delete permanently [ ' + data.ServiceMasterName + ' ]';
            angular.element(document.querySelector('#RemoveServicePOItemPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteServicePOItem = function () {
        if (baseService.isUndefinedOrNull($scope.LCChargesId)) {
            $scope.SavedServicePODetailList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Products/PurchaseDocumentsAcceptance/DeleteServicePOItem?Id=' + $scope.LCChargesId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SavedServicePODetailList.splice($scope.bActivityIndex, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    function getACKTaxList(Id) {
        $http.get('Products/PurchaseDocumentsAcceptance/getServicePOAckTax?Id=' + Id)
            .then(function (response) {
                $scope.receiveTaxList1 = [];
                $scope.ServicePOAndAckTax = [];
                $scope.ServicePOTaxList1 = response.data;
                for (var i = 0; i < $scope.ServicePOTaxList1.length; i++) {
                    $scope.ServicePOAndAckTax.push($scope.ServicePOTaxList1[i]);
                }
            });
    }

    $scope.getServicePOTaxList = function (data, flag, index, Id) {
        if ($scope.Action === "Save") {
            $scope.productNew.TaxOptionServicePOModify = 'Yes';
            $scope.LoadServicePOTaxButtonClick();
            $scope.Currency = $("#currency option:selected").text();
            $scope.currentMaterialRow = index;
            $scope.currentInventoryReceiveDetailIdRow = Id;
            $scope.taxAbleAmnt = data.TotalAmount;
            $scope.percentageColumn = flag;
            $scope.currentMaterialRow = index;
            $scope.ServiceMasterName = data.ServiceMasterName;
            $scope.ServicePOTaxList = [];
            if ($scope.ServicePOAndAckTax.length > 0) {
                for (var i = 0; i < $scope.ServicePOAndAckTax.length; i++) {
                    if ($scope.ServicePOAndAckTax[i].ServicePoDetailId === data.ServicePODetailId) {
                        $scope.HSNCode = $scope.ServicePOAndAckTax[0].HSNCode;
                        $scope.ServicePOTaxList.push($scope.ServicePOAndAckTax[i]);

                    }
                }

            }
        }
        else {
            $scope.productNew.TaxOptionServicePOModify = 'Yes';
            $scope.LoadServicePOTaxButtonClick();
            $scope.Currency = $("#currency option:selected").text();
            $scope.currentMaterialRow = index;
            $scope.currentInventoryReceiveDetailIdRow = Id;
            $scope.taxAbleAmnt = data.TotalAmount;
            $scope.percentageColumn = flag;
            $scope.currentMaterialRow = index;
            $scope.ServiceMasterName = data.ServiceMasterName;
            if ($scope.ServicePOTaxList1.length > 0) {
                $scope.ServicePOTaxList = [];
                for (var i1 = 0; i1 < $scope.ServicePOTaxList1.length; i1++) {
                    if ($scope.ServicePOTaxList1[i1].ServiceAcknowledgementDetailId === data.ServicePODetailId) {
                        $scope.HSNCode = $scope.ServicePOTaxList1[0].HSNCodeId;
                        $scope.ServicePOTaxList.push($scope.ServicePOTaxList1[i1]);
                    }
                }
            }
        }
        angular.element(document.querySelector('#ServicePOTaxPopUp')).modal('show');
    };

    $scope.closegetServicePOTaxList = function () {
        angular.element(document.querySelector('#ServicePOTaxPopUp')).modal('hide');
    }

    $scope.calculateTaxAmountForServicePO = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.checkRowValidationServicePO = function (x) {

        for (var i = 0; i < $scope.ServicePOTaxList.length; i++) {
            if ($scope.ServicePOTaxList[i].Id === x.Id) {
                $scope.ServicePOTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }
        }
    }

    $scope.UpdateServicePOAckTax = function () {
        $http({
            method: 'POST',
            url: $scope.updateUrlForSerPOAckTaxValue,
            data:
            {
                'ServiceAcknowledgementMasterId': $scope.productId,
                'UserSendData': $scope.ServicePOTaxList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getServiceChargeList($scope.productId);
                angular.element(document.querySelector('#ServicePOTaxPopUp')).modal('hide');

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    // #endregion

};
