'use strict';
BOQPurchaseOrderController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function BOQPurchaseOrderController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
    $rootScope.FormTitle = 'BOQ Purchase Order';
    $rootScope.title = 'BOQ Purchase Order';
    $scope.path = "Costings/BOQPurchaseOrder/";
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.pathPO = 'Products/PurchaseOrder/';
    $scope.getListUrl = $scope.pathPO + 'getlist';
    $scope.saveUrl = $scope.pathPO + 'create';
    $scope.saveUrlFG = $scope.pathPO + 'CreateFGMasterOrder';
    $scope.updateUrl = $scope.pathPO + 'edit';
    $scope.updateUrlFG = $scope.pathPO + 'FGMasterOrderedit';
    $scope.deleteUrl = $scope.pathPO + 'delete/';
    $scope.detailSaveUrl = $scope.pathPO + 'detailcreate';
    $scope.detailDeleteUrl = $scope.pathPO + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.pathPO + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.pathPO + 'servicechargesdelete?serviceId=';
    $scope.PurchaseOrderFileLocation = virtualPath.PurchaseOrder;
    $scope.Action = "Save";
    $scope.searchByBOM = [{ value: 'BOMUserName', name: 'BOM User Name' }, { value: 'Customer', name: 'Customer' }, { value: 'Vendor', name: 'Vendor' }, { value: 'Item', name: 'Costing Item' }];
    $scope.searchBOMFieldName = 'BOMUserName'; $scope.searchBOMText = ''; $scope.searchByBOMDate = { FromDate: new Date(), ToDate: new Date() };
    function FromDateTransform() {
        var d = new Date();
        d.setDate(d.getDate() - 7);
        $scope.searchByBOMDate.FromDate = d;
    }
    FromDateTransform();

    $scope.BOMItemList = [];
    $scope.GetBOMList = function (flag) {
        var Filter = { column: $scope.searchBOMFieldName, value: $scope.searchBOMText, Date: null };
        if (flag == 'DATESEARCH')
            Filter = { column: $scope.searchBOMFieldName, value: $scope.searchBOMText, Date: $scope.searchByBOMDate };

        $http({
            method: 'POST',
            url: $scope.path + "GetBOMList",
            data: Filter,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.DATA.length; i++)
                response.data.DATA.BOMCreationDate = new Date(response.data.DATA.BOMCreationDate);

            $scope.BOMItemList = response.data.DATA;
        });
    }
    $scope.baseCurrencyId = null;
    $scope.masterId = null;
    //-----------------PO Header Information--------------------
    $scope.productNewMain = {
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
    $scope.productNew = Object.assign({}, $scope.productNewMain)
    $scope.OrderSpecific = 'Yes';
    $scope.inventoryMaterialList = [];
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
        //factoryService.getCurrencyPrecision($scope.baseCurrencyId);
    });


    $scope.BOMItemListSelected = [];
    $scope.SelectdItemsForPO = function () {
        try {
            $scope.BOMItemListSelected = [];
            var SelectedItems = ej.DataManager($scope.BOMItemList).executeLocal(ej.Query().where("Checked", "equal", true));
            if (SelectedItems.length == 0)
                throw "Please select BOQ Item to create PO";

            $scope.BOMItemListSelected = SelectedItems;
            $rootScope.openPopupAngular('ListOfPOMaterialSelected');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.ApplyItemsForPO = function () {
        try {
            $scope.productNew = Object.assign({}, $scope.productNewMain);

            for (var i = 0; i < $scope.BOMItemListSelected.length; i++) {
                if (angular.isUndefinedOrNull($scope.BOMItemListSelected[i]["VendorId"]) == false) {

                    $scope.productNew["PartyId"] = $scope.BOMItemListSelected[i]["VendorId"];
                    $scope.productNew["PartyCode"] = $scope.BOMItemListSelected[i]["VendorCode"];
                    $scope.productNew["PartyName"] = $scope.BOMItemListSelected[i]["Vendor"];
                    break;
                }
            }
            $http({
                method: 'GET',
                url: $scope.path + 'GetPartyInformationById?VendorId=' + $scope.productNew["PartyId"]
            }).then(function successCallback(response) {

                if (response.data.DATA.length > 0) {
                    response.data = response.data.DATA[0];
                    $scope.closePartyPopUp(response);
                }

                $rootScope.closePopup('ListOfPOMaterialSelected');
                if (!$rootScope.isCollapsed) $rootScope.toggle();
                $scope.GetBOQItemList();
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //---------------PO ITEM Information---------------
    $scope.inventoryMaterialList = [];
    $scope.chargesList = [];
    $scope.ClearList = function (data) {
        $scope.inventoryMaterialList = [];
        $scope.OrderSpecific = data;
    };

    $scope.contractList = [];
    $scope.IsBaseOnDueDateEnable = false;
    $scope.partyType = 'Vendor';
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
    $scope.closePartyPopUp = function (x) {
        //
        //if ($scope.partyIndex !== -1) {
        var party = x.data;
        // var party = $scope.partyList[$scope.partyIndex];
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
        //}
        $scope.getToCurrencyRate();
    };
    $scope.PaymentModeList = [];
    $scope.PaymentModeByPaymentTerm = function () {
        //
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/PurchaseOrder/PaymentModeByPaymentTerm?Id=' + $scope.productNew.PaymentTermId
        }).then(function successCallback(response) {
            $scope.PaymentModeList = response.data;
            $scope.productNew.PaymentMode = response.data[0].PaymentMode;

        });
    }
    $scope.paymentTermList = [];
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {

        $scope.paymentTermList = response.data;
    });
    $scope.plantList = [];
    $scope.GetTerms = function (id) {
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetTerms?id=' + id
        }).then(function successCallback(response) {
            $scope.paymentTermList1 = response.data;
            $scope.productNew.DeliveryInstruction = $scope.paymentTermList1[0].DeliveryInstruction;
            $scope.productNew.SpecialInstruction = $scope.paymentTermList1[0].SpecialInstruction;
            //$scope.productNew.CheckedBy = $scope.paymentTermList1[0].CheckedBy;
        });
    }
    $scope.changePaymentTerm = function () {

        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.productNew.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }
            $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
        }
    };
    function getPartyPlantList() {


        //var aa = $scope.Id;
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
                    //$scope.partyPlantId = item.Value;
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


    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };

    $scope.OrderSpecific = $scope.productNew.OrderSpecific;
    $scope.SelectedContract = function (obj) {
        $scope.productNew.ContractId = obj.data.ContractId;
        $scope.productNew.CustomerName = obj.data.CustomerName;
        $scope.productNew.ContractNo = obj.data.ContractNo;
        $scope.productNew.LCRef = obj.data.LCRef;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.ClearFields = function () {
        $scope.productNew.ContractId = null;

    }
    $scope.Clearcontract = function () {
        $scope.productNew.CustomerName = "";
        $scope.productNew.ContractId = "";

    };


    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {

        if ($scope.inventoryMaterialList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.productNew.ChangeInvoicingStateId)) {
                if ($scope.productNew.PlantStateId === $scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.InvoicingStateId === $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.PlantStateId !== $scope.productNew.InvoicingStateId && $scope.productNew.PlantStateId != $scope.productNew.ChangeInvoicingStateId)
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

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });
    $scope.getToCurrencyRate = function () {

        if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
            $scope.productNew.ToCurrencyRate = 1;
            return;
        }
        $http.get('Products/PurchaseOrder/GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
            .then(function (response) {
                if (parseFloat(response.data) === 0)
                    $scope.productNew.ToCurrencyRate = 1;
                else
                    $scope.productNew.ToCurrencyRate = response.data;
            });
    };

    $scope.validation = function () {
        $scope.modelValidation('div_mm', 'detailModel', 'MaterialMasterName', 'Material Master');
        if ($scope.hasArticle) $scope.modelValidation('div_ar', 'detailModel', 'ArticleName');
        $scope.manualValidationAddRemove('div_qty', 'detailModel', 'TransactionQty');
        $scope.modelValidation('div_qty', 'detailModel', 'TransactionUoMId', 'UoM is required');
        if ($scope.detailModel.TransactionAmount === 0)
            throw manualValidation('div_tamnt', true, 'Total amount is required.');
        $scope.manualValidationAddRemove('div_tamnt', 'detailModel', 'TransactionAmount');
        if ($scope.detailModel.IsOriginApplicable)
            $scope.manualValidationAddRemove('div_country', 'detailModel', 'CountryId');

        var isSku = false;
        if ($scope.hasSku) {
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
            }
            if (isSku) throw ShowResult('Please insert SKU.', 'failure', 'detailPopUp');
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
    //manualDateValidation
    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };

    $scope.Save = function () {
        //
        try {
            $scope.dbval = $scope.StateData;
            $scope.UIval = $scope.productNew.InvoicingState;
            if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be approved by", 'failure');
                return false;
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be checked by", 'failure');
                return false;
            }
            else if ($scope.inventoryMaterialList.length === 0) {
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            }
            //else if ($scope.dbval.length === 0) {
            //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            //}
            //else if ($scope.dbval === $scope.UIval) {
            //    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            //}
            else if ($scope.productNew.OrderSpecific === 'Yes' && baseService.isUndefinedOrNull($scope.productNew.ContractId)) {
                //ShowResult('Please Select Contract');
                //return false;
            }
            else {
                ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

            }


            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            //$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
            $scope.modelValidation('div_PODate', 'productNew', 'PODate', 'PO Entry Date');
            //if ($scope.Action === 'Update')
            //    $scope.modelValidation('div_grnNo', 'productNew', 'Id');
            //$scope.modelValidation('div_grnDate', 'productNew', 'GRNDate');

            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                //if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate))
                //    return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
                //else
                //    manualValidation('div_entryDate', false);
                //if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate))
                //    return manualValidation('div_grnDate', true, "PO date can't be less than gate entry date");
                //else
                //    manualValidation('div_grnDate', false);
                if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate))
                    return manualValidation('div_PODate', true, "PO date can't be less than Doc entry date");
                else
                    manualValidation('div_PODate', false);

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {

                    if ($scope.GetListForMasterOrdernew.length > 0) {

                    }
                    else {
                        ShowResult("Please select material", 'failure');
                        return;
                    }

                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;

                            $scope.Action = "Update";


                            $http({
                                method: 'POST',
                                url: 'Products/PurchaseOrder/detailPOSaveForBOQ',
                                data: {
                                    entity: JSON.stringify($scope.GetListForMasterOrdernew)
                                    , taxCategoryList: $scope.taxCategoryList
                                    , PoId: $scope.productNew.Id
                                    , groupList: JSON.stringify($scope.groupList)
                                },
                                dataType: 'JSON'
                            }).then(function successCallback(response) {
                                if (response.data.Error === true)
                                    ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                                else {

                                    ShowResult(response.data.Message, 'success', 'ListOfPOMaterial');
                                    getInventoryMaterialList($scope.productNew.Id);
                                    angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');
                                    $scope.getalldata();
                                }
                            }), function errorCallBack(response) {
                                ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                            };



                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {

                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            //$scope.getDataList();
                            $scope.getalldata();

                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            throw e;
        }
    };


    $scope.Delete = function () {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.productNew.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDataList();
                        ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
        else
            ShowResult('First delete all line item.', 'failure');
    };

    $scope.Clear = function () {

        ClearFields();
        $scope.NotificationSettingStatus();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        return true;

    };
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.GetListForMasterOrder = [];
    $scope.groupList = [];
    $scope.GetListForMasterOrdernew = [];
    $scope.taxCategoryList = [];
    $scope.Action1 = 'Save';
    $scope.ActionPOBOQ = 'Save';

    $scope.GetBOQItemList = function () {
        //debugger;
        $scope.GetListForMasterOrder = [];
        $scope.groupList = [];
        $scope.GetListForMasterOrdernew = [];
        $scope.taxCategoryList = [];
        $scope.groupList = [];
        $scope.Action1 = 'Save';
        //$scope.uom();

        $scope.getalldataListForBOQList();
        $scope.ActionPOBOQ = 'Save';

    };

    $scope.GetListForMasterOrder = [];
    $scope.getalldataListForBOQList = function () {
        //tarek
        var gridObj = $("#GridReq").data("ejGrid");

        var _CostingItemIds = null;
        var _CostingBOQMasterIds = null;
        if ($scope.BOMItemListSelected.length > 0) {
            _CostingItemIds = getString($scope.BOMItemListSelected, 'CostingItemId');
            _CostingBOQMasterIds = getString($scope.BOMItemListSelected, 'BOQRef');
        }

        $scope.GetListForMasterOrder = [];
        $http({
            method: "POST",
            dataType: 'JSON',
            url: $scope.path + 'GetBOQItems',
            data: {
                CostingItemIds: _CostingItemIds,
                CostingBOQMasterIds: _CostingBOQMasterIds,
                ContractId: $scope.productNew.ContractId,
                VendorId: $scope.productNew.PartyCode,
                IsOwnVendor: $scope.IsOwnVendor,
                inveReveiveMasterId: $scope.productNew.Id
            }

        }).then(function successCallback(response) { //datagatefun
            $scope.GetListForMasterOrder = [];

            for (var i = 0; i < response.data.length; i++) {
                if (angular.isUndefinedOrNull(response.data[i].MaterialMasterId))
                    response.data[i].IncompleteMaterial = true;
            }

            $scope.GetListForMasterOrder = response.data;
            gridObj.clearFiltering();
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $scope.processgroupList111();
        });


        $scope.Action1 = 'Save';
        $scope.processgroupList1();
    };


    $scope.processgroupList111 = function () {

        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrder;
            $scope.GetListForMasterOrder = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.newlistitems[i].ThirdCharacteristicsValueId });
                //var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrder.push($scope.newlistitems[i]);
                }
            }
        }
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.RequisitionListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');
    };
    $scope.processgroupList1 = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrder;
            $scope.GetListForMasterOrder = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrder.push($scope.newlistitems[i]);
                }
            }
        }
        $scope.Action1 = 'Save';
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.detailPOSaveForBOQ = function () {

        try {
            $scope.check();
            $scope.GetListForMasterOrdernew = [];
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                if ($scope.GetListForMasterOrder[i].CheckedStatus === false && !(baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty) || $scope.GetListForMasterOrder[i].TransactionQty === 0)) {
                    //if ($scope.ActionPOBOQ === 'Update') {
                    //    ShowResult('Select the Material', 'failure', 'ListOfPOMaterial1');
                    //    return false;
                    //}

                    //else {
                    //    ShowResult('Select the Material', 'failure', 'ListOfPOMaterial');
                    //    return false;
                    //}
                }

                if ($scope.GetListForMasterOrder[i].CheckedStatus == true) {
                    var kkkkkk = 10;
                }
                if ((baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty) || $scope.GetListForMasterOrder[i].TransactionQty === 0) && $scope.GetListForMasterOrder[i].CheckedStatus === true) {

                    if ($scope.ActionPOBOQ === 'Update') {

                        ShowResult('Enter the Selected  Material Qty', 'failure', 'ListOfPOMaterial1');
                        return false;
                    }
                    else {
                        ShowResult('Enter the Selected  Material Qty', 'failure', 'ListOfPOMaterial');
                        return false;
                    }
                }
                if ($scope.GetListForMasterOrder[i].CheckedStatus === true && $scope.GetListForMasterOrder[i].RequiredQtyApproved === 'Yes' && $scope.GetListForMasterOrder[i].IncompleteMaterial === 'No') {
                    if ($scope.ActionPOBOQ === 'Save') {
                        //if ((parseFloat($scope.GetListForMasterOrder[i].TransactionQty) + parseFloat($scope.GetListForMasterOrder[i].OtherPOQty)) > parseFloat($scope.GetListForMasterOrder[i].RequiredQty)) {
                        //	ShowResult('Trasaction qty can not grater than required Qty', 'failure', 'ListOfPOMaterial');
                        //	return false;
                        //}
                        if (baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty)) {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                            return false;
                        }
                        else if ($scope.GetListForMasterOrder[i].TransactionQty === '0' || $scope.GetListForMasterOrder[i].TransactionQty === '0.00' || $scope.GetListForMasterOrder[i].TransactionQty === '0.0') {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                            return false;
                        }

                        else if ($scope.GetListForMasterOrder[i].RequiredQtyApproved === 'No') {
                            ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure', 'ListOfPOMaterial');
                            return false;
                        }
                        else if ($scope.GetListForMasterOrder[i].IncompleteMaterial === 'Yes') {
                            ShowResult('This is incomplete material.So you can not take this material', 'failure', 'ListOfPOMaterial');
                            return false;
                        }

                        else {
                            $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);

                        }
                    }
                    else {
                        if ((parseFloat($scope.GetListForMasterOrder[i].TransactionQty) + parseFloat($scope.GetListForMasterOrder[i].OtherPOQty)) > parseFloat($scope.GetListForMasterOrder[i].RequiredQty)) {
                            ShowResult('Trasaction qty can not grater than required Qty', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        else if (baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty)) {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        else if ($scope.GetListForMasterOrder[i].TransactionQty === '0' || $scope.GetListForMasterOrder[i].TransactionQty === '0.00' || $scope.GetListForMasterOrder[i].TransactionQty === '0.0') {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }

                        else if ($scope.GetListForMasterOrder[i].RequiredQtyApproved === 'No') {
                            ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        else if ($scope.GetListForMasterOrder[i].IncompleteMaterial === 'Yes') {
                            ShowResult('This is incomplete material.So you can not take this material', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        else {
                            $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);
                        }
                    }

                }

                for (var j = 0; j < $scope.GetListForMasterOrdernew.length; j++) {
                    if ($scope.GetListForMasterOrdernew[j].CheckedStatus === true) {
                        $scope.tempList.push($scope.GetListForMasterOrdernew[j]);
                    }
                }


            }
            $scope.UOMValidation();
            if ($scope.GetListForMasterOrdernew.length === 0) {
                if ($scope.ActionPOBOQ === 'Update') {

                    ShowResult('Please select atleast one material', 'failure', 'ListOfPOMaterial1');
                    return false;
                }
                else {
                    ShowResult('Please select atleast one material', 'failure', 'ListOfPOMaterial');
                    return false;
                }

            }

            $scope.groupList = [];
            $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);

            if ($scope.ActionPOBOQ === 'Save') {
                $scope.materialValidationForBOQItem();
                if ($scope.invalid && !$scope.UOMValidation()) {
                    if (angular.isUndefinedOrNull($scope.productNew.Id)) {
                        //if first time, cache the selected items and show
                        angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');
                    }
                    else {
                        //i have commented these lines
                        $http({
                            method: 'POST',
                            url: 'Products/PurchaseOrder/detailPOSaveForBOQ',
                            data: {
                                entity: JSON.stringify($scope.GetListForMasterOrdernew)
                                , taxCategoryList: $scope.taxCategoryList
                                , PoId: $scope.productNew.Id
                                , groupList: JSON.stringify($scope.groupList)
                            },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true)
                                ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                            else {
                                ShowResult(response.data.Message, 'success', 'ListOfPOMaterial');
                                getInventoryMaterialList($scope.productNew.Id);
                                angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');

                            }
                        }), function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                        };
                    }

                }
            }

            else {
                $http({
                    method: 'POST',
                    url: 'Products/PurchaseOrder/detailPOUpdateForBOQ',
                    data: {
                        //entity: $scope.GetListForMasterOrdernew
                        //, taxCategoryList: $scope.taxCategoryList
                        //, PoId: $scope.productNew.Id
                        //, groupList: $scope.groupList
                        entity: JSON.stringify($scope.GetListForMasterOrdernew)
                        , taxCategoryList: $scope.taxCategoryList
                        , PoId: $scope.productNew.Id
                        , groupList: JSON.stringify($scope.groupList)
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfPOMaterial');
                        getInventoryMaterialList($scope.productNew.Id);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                };

            }

        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.check = function () {
        var aa = 0;
        for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
            if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
                return true;
                aa++;

            }

        }
        if (aa === 0) {
            ShowResult('Please select atleast one material', 'failure', 'ListOfPOMaterial');
            return false;
        }

    }
    $scope.tempList = [];
    $scope.UOMValidation = function () {
        var getRow3
        $scope.invalid = false;
        for (var i = 0; i < $scope.tempList.length; i++) {
            getRow3 = $filter("filter")($scope.tempList, { "MaterialMasterId": $scope.tempList[i].MaterialMasterId, "ArticleId": $scope.tempList[i].ArticleId, "FirstCharacteristicsValueId": $scope.tempList[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.tempList[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.tempList[i].ThirdCharacteristicsValueId });

        }
        $scope.TransactionUoMId = '';
        for (var k = 0; k < getRow3.length; k++) {
            $scope.TransactionUoMId = getRow3[0].TransactionUoMId;
            if (getRow3[k].TransactionUoMId != $scope.TransactionUoMId) {
                ShowResult('Have you selected Same UOM?', 'failure', 'ListOfPOMaterial');
                return true;

            }

        }

        return false;
    }
    $scope.processgroupList = function (oldlist, newlist) {
        for (var i = 0; i < oldlist.length; i++) {
            var getRow = $filter("filter")(oldlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
            var ExistingRow = $filter("filter")(newlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
            if (ExistingRow.length === 0) {
                if (!baseService.isUndefinedOrNull(getRow[0].MaterialMasterId)) {

                    newlist.push(getRow[0]);
                }
            }

            var getRowWithoutMaterial = $filter("filter")(oldlist, { "MaterialDetail": oldlist[i].MaterialDetail, "RequisitionDetailId": oldlist[i].RequisitionDetailId });

            if (getRowWithoutMaterial.length === 1) {
                if (baseService.isUndefinedOrNull(getRowWithoutMaterial[0].MaterialMasterId)) {
                    newlist.push(getRowWithoutMaterial[0]);
                }
            }

        }
        return newlist;
    };
    $scope.materialValidationForBOQItem = function () {
        for (var i = 0; i < $scope.GetListForMasterOrdernew.length; i++) {
            var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.GetListForMasterOrdernew[i].MaterialMasterId, "ArticleId": $scope.GetListForMasterOrdernew[i].ArticleId, "FirstCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].ThirdCharacteristicsValueId });

            if (getRow3 == 0) {
                $scope.invalid = true;
            }
            else {
                ShowResult('Material Combination Already Exist', 'failure', 'ListOfPOMaterial');
                $scope.invalid = false;
            }
        }


    };
    $scope.materialValidationForBOQItemUOMCheck = function () {
        for (var i = 0; i < $scope.GetListForMasterOrdernew.length; i++) {
            var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.GetListForMasterOrdernew[i].MaterialMasterId, "ArticleId": $scope.GetListForMasterOrdernew[i].ArticleId, "FirstCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].ThirdCharacteristicsValueId });

            if (getRow3 == 0) {
                $scope.invalid = true;
            }
            else {
                ShowResult('Material Combination Already Exist', 'failure', 'ListOfPOMaterial');
                $scope.invalid = false;
            }
        }


    }


    $scope.DetailId = null;
    $scope.InvoicingPartyPlantId = null;
    function getInventoryMaterialList(inveReveiveId) {
        $scope.masterId = inveReveiveId;

        $scope.inventoryMaterialList = [];
        $http.get($scope.pathPO + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
            .then(function (response) {

                $scope.inventoryMaterialList = ej.DataManager(response.data.Rows).executeLocal(ej.Query().sortBy("UserName desc"));//response.data.Rows;
                //var dataManagerObj = ej.DataManager(response.data.Rows).executeLocal(ej.Query().sortBy("UserName ASC"));
                $scope.DetailId = $scope.inventoryMaterialList[0].InventoryReceiveDetailId;
                $scope.InvoicingPartyPlantId = $scope.inventoryMaterialList[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingPartyPlantId = $scope.inventoryMaterialList[0].InvoicingPartyPlantId;
                $scope.productNew.InvoicingStateId = $scope.inventoryMaterialList[0].InvoicingStateId;
                $scope.productNew.PlantStateId = $scope.inventoryMaterialList[0].PlantStateId;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetSalesTaxData();
            });

    }
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function getGrossAmount(list, key1, key2, key3, fieldName) {
        $scope[fieldName] = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            $scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
        }
    }
    $scope.TaxList = [];
    $scope.GetSalesTaxData = function (salesId) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: $scope.pathPO + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.TaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = gettaxlist(linepk);
                $scope.inventoryMaterialList[i].TaxList = list;
            }
        });
    };
    function gettaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.TaxList.length; i++) {
            if ($scope.TaxList[i].PODetailId === linepk) {
                result.push($scope.TaxList[i]);
            }
        }
        return result;
    }


    $scope.Griddata = [];
    $scope.POTypeStatus = 'Pending';
    $scope.POTypeStatus = '';
    $scope.tab1 = 1;
    $scope.setTabIndex = function (newTab) {

        $scope.POTypeStatus = 'Pending';
        $scope.getalldata();
        $scope.tab1 = newTab;

    };
    $scope.isSetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabCHRIndex = function (newTab) {
        //alert('tabCHR');

        //$scope.POTypeStatus = 'CheckedHoldRej';
        //$scope.getalldata();
        $scope.tab1 = newTab;

    };
    $scope.isSetCHRIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabCheckedIndex = function (newTab) {

        $scope.POTypeStatus = 'Checked';
        $scope.getalldata();
        $scope.tab1 = newTab;


    };
    $scope.isSetCheckedIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
    $scope.getalldata = function () {
        if ($scope.POTypeStatus === 'Pending') {
            $scope.POTypeStatus = 'Pending'
        }
        else {

        }

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetPOTypeList?POTypeStatus=' + $scope.POTypeStatus,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            for (var i = 0; i < $scope.Griddata.length; i++) {
                response.data[i].PODate = new Date($scope.Griddata[i].PODate);
            }
        });
    };
    $scope.getalldata();
    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.Currency = $("#currency option:selected").text();
        $scope.productNew = x.data;
        $scope.Id = $scope.productNew.Id;
        $scope.productNew.PODate = x.data.PODate1;
        //getPartyPlantList();
        $scope.GetTerms($scope.productNew.Id);
        getPartyPlantEditList($scope.productNew.InvoicingPartyPlantId, $scope.productNew.InvoicingByAddress, $scope.productNew.DeliveryPartyPlantId, $scope.productNew.DeliveryByAddress, $scope.productNew.DeliveryState, $scope.productNew.DeliveryGSTIN);
        // getPartyPlantEditList();
        getInventoryMaterialList($scope.productNew.Id);
        //getInventoryMaterialList(Id);
        //getServiceChargeList($scope.productNew.Id);
        //getServiceChargeList(Id);
        //$scope.getToCurrencyRate();

        if (!baseService.isUndefinedOrNull(x.data.ContractId)) {
            $scope.productNew.OrderSpecific = 'Yes';
        }
        else {
            $scope.productNew.OrderSpecific = 'No';
        }

        $scope.productNew["OrderSpecific"] = 'Yes';
        $scope.BOQItemDisabled = 'GridClick';
        if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.CheckedById;
        }
        $scope.ContractWiseData(x.data.ContractId);
        $scope.ImagedataLoad($scope.productNew.Id);
        $scope.GetCheckedByAndApprovedBy1();
        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {
            $scope.GetCheckedByAndApprovedBy1();
            $scope.productNew.CheckedBy = x.data.ApprovedById;
            $scope.productNew.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {
            $scope.GetCheckedByAndApprovedBy1();
            $scope.productNew.CheckedBy = x.data.CheckedById;
            $scope.productNew.labelCheckAndApproved = 'To be checked by';
        }

        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) $rootScope.toggle();


    };
    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/NotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            $scope.GetCheckedByAndApprovedBy1();
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be approved by';
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            //else {
            //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
            //}

        });
    };
    $scope.NotificationSettingStatus();

    $scope.GetCheckedByAndApprovedBy1 = function () {
        //debugger;

        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/PurchaseOrder/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }
    $scope.ContractWiseData = function (Id) {
        try {
            $http({
                method: "GET",
                dataType: 'JSON',
                url: 'Products/PurchaseOrder/ContractWiseData?ContractId=' + Id
            }).then(function successCallback(response) { //datagatefun
                $scope.productNew.ContractNo = response.data[0].ContractNo;
                $scope.productNew.LCRef = response.data[0].LCRef;
            });
        } catch (e) {

        }

    };
    $scope.Imagedata = [];
    $scope.ImagedataLoad = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/PurchaseOrder/PODocumentMapData?POID=' + $scope.productNew.Id,
        }).then(function successCallback(response) { //datagatefun
            $scope.Imagedata = response.data;

        });
    };
    $scope.receiveTaxList = [];
    $scope.detailPopUpEdit = function () {
        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            for (var t = 0; t < $scope.inventoryMaterialList[i].TaxList.length; t++) {
                if ($scope.inventoryMaterialList[i].TransactionRate === 0 || $scope.inventoryMaterialList[i].TransactionRate === '0.0' || $scope.inventoryMaterialList[i].TransactionRate === '') {
                    ShowResult('Enter Rate', 'failure');
                    return false;
                }
                else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].DeliveryDate)) {
                    ShowResult('Enter Delivery Date', 'failure');
                    return false;
                }
                // $scope.receiveTaxList.push($scope.inventoryMaterialList[i].TaxList[t]);
            }
        }
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/UpdateMaterial',
            data: {
                entity: $scope.inventoryMaterialList,
                receiveTaxList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.productNew.Id = response.data.entity.Id;
                //$scope.productNew.PartyName = $scope.product.PartyName;
                //$scope.Action = "Update";
                //getInventoryMaterialList($scope.detailModel.Id);

            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };




        //$scope.detailModel.MaterialStorageId = data.MaterialStorageId



        // data.TransactionQty=
        // $scope.clearCharNames();
        // angular.element(document.querySelector('#detailPopUpEdit')).modal('show');
    };
    $scope.dindex = -1;
    $scope.DelCharge = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === Id) {
                $scope.receiveTaxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };
    $scope.Del = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === Id) {
                $scope.receiveTaxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };
    $scope.calculateAmount = function (data) {

        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount === 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        if ($scope.productNew.IsNonCreditable == 1) {
            //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
            if (data.BaseTaxAmount === null) {
                data.BaseTaxAmount = '0.00';
            }
            data.BaseAmount = parseFloat(data.TrnAmount + data.BaseTaxAmount);
            $scope.detailPopUpEdit();
        }
        else {
            // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            data.BaseAmount = data.TrnAmount;
            $scope.detailPopUpEdit();
        }
    };
    $scope.calculateRate = function (data, event) {

        data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        if ($scope.productNew.IsNonCreditable == 1) {
            //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
            data.BaseAmount = data.TrnAmount + data.BaseTaxAmount;
        }
        else {
            // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            data.BaseAmount = data.TrnAmount;
        }

    };
    $scope.calculateAmountForServiceCharge = function (data) {
        data.TotalTaxAmount = 0;
        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === data.Id) {
                $scope.ChargeTaxList[i].TaxAmount = data.Amount * $scope.ChargeTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.ChargeTaxList[i].TaxAmount;
            }
        }
    };
    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');

        }

    }
    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
        }
    };

    $scope.PODetailsUpdatePOPUp = function (x) {
        $scope.ActionPOBOQ = "Update";
        getInventoryMaterialListForUpdate(x.InventoryReceiveDetailId, x.InventoryMaterialId, x.ArticleId, x.FirstCharacteristicsValueId, x.SecondCharacteristicsValueId, x.ThirdCharacteristicsValueId);
    };
    function getInventoryMaterialListForUpdate(inveReveiveId, MaterialMasterId, ArticleId, FirstCharacteristicsValueId, SecondCharacteristicsValueId, ThirdCharacteristicsValueId) {
        $scope.masterId = inveReveiveId;
        $scope.GetListForMasterOrder = [];
        $http.get($scope.path + 'GetCostingBOQItemsListForUpdate?VendorId=' + $scope.productNew.PartyCode + '&inveReveiveId=' + inveReveiveId + '&inveReveiveMasterId=' + $scope.productNew.Id + '&MaterialMasterId=' + MaterialMasterId + '&ArticleId=' + ArticleId + '&FirstCharacteristicsValueId=' + FirstCharacteristicsValueId + '&SecondCharacteristicsValueId=' + SecondCharacteristicsValueId + '&ThirdCharacteristicsValueId=' + ThirdCharacteristicsValueId)
            .then(function (response) {

                $scope.GetListForMasterOrder = response.data;
            });
        angular.element(document.querySelector('#ListOfPOMaterial1')).modal('show');

    }
}

