'use strict';
PurchaseDocumentAcceptancePostController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function PurchaseDocumentAcceptancePostController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Purchase Document Acceptance ";
    $scope.path = 'Products/PurchaseDocumentsAcceptance/';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl1 = $scope.path + 'GetListForMasterData';
    $scope.getListUrl2 = $scope.path + 'GetListForMasterData2';

    $scope.saveUrl = $scope.path + 'createGRNBYPO';
    $scope.updateUrl1 = $scope.path + 'UpdareGRNBYPO';

    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'deleteGRNBYPO/';
    $scope.deleteLineItemUrl = $scope.path + 'DeleteLineItem/';

    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.PostingAcceptanceId = '';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.chargesList = [];
    $scope.chargesListPO = [];
    $scope.storageList = [];
    $scope.currencyList = [];
    $scope.detailModelSave = [];
    $scope.inventoryMaterialListPOnew = [];
    $scope.chargesListPOnew = [];
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
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
    $scope.POPopUp = function () {
        $scope.getalldata();
        angular.element(document.querySelector('#POPopUp')).modal('show');

    };
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp')).modal('hide');
    };
    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        var Id = x.data.Id;
        //alert('Id'+Id);
        $scope.productNew = x.data;
        $scope.PurchaseLCNo = x.data.PurchaseLCNO;
        $scope.LCOpeningDate = x.data.LCOpeningDate;
        $scope.LCOpeningBank = x.data.LCOpeningBank;
        $scope.productId = "";
        $scope.PurchaseDocAcceptance.CurrencyName = x.data.CurrencyName;
        $scope.POId = x.data.Id;
        $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
        $scope.getPOList();
        $scope.POPopUpClose();
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

        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };
    $scope.chargesList = [];
    function getServiceChargeList(purchaseDocAcceptanceId) {
        $http.get('Products/PurchaseDocumentsAcceptance/GetServiceChargeList?purchaseDocAcceptanceId=' + purchaseDocAcceptanceId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
            });
    }

    $scope.PurchaseDocAcceptanceService = {
        Id: null
        , ChargeName: null
        , PurchaseDocAcceptanceId: null
        , AcceptanceServiceId: null
        , Amount: 0
        , TotalTaxAmount: 0
    };

    $scope.acceptanceChargesCheckedList = [];

    $scope.PurchaseDocAcceptance = {
        Id: null,
        AcceptanceNo: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        EntryDate: null,
        AcceptanceDate: null,
        POId: null,
        CheckedBy: null,
        CheckedByStatus: null,
        AuthorizedBy: null,
        AuthorizedByStatus: null,
        CurrencyName: null,
        Remarks: null,
        VoucherTypeId: null,
        ToCurrencyRate: null,
        DueDate: null,
        InvoiceDate: null,
        Tenure: null,
    };
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
        POId: null,
        PODetailId: null
    };
    $scope.PurchaseDocAcceptanceService = {
        Id: null,
        PurchaseDocAcceptanceId: null,
        AcceptanceServiceId: null,
        Amount: null,
        TotalTaxAmount: null,
    };
    $scope.Action = 'Post';

    $scope.rowDetails = [];

    $scope.voucherTypeList = [];
    cboService.getCboVoucherTypePuechaseDocumentAcceptanceList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.PurchaseDocAcceptance.VoucherTypeId = $scope.voucherTypeList[0].Value;
        $scope.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });


    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.AcceptanceDate) && !baseService.isUndefinedOrNull($scope.PurchaseDocAcceptance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.PurchaseDocAcceptance.AcceptanceDate + "&currencyId=" + $scope.PurchaseDocAcceptance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.PurchaseDocAcceptance.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.makerowDetails = function () {
        var DrRows = {}; var CrRows = {};
        $scope.rowDetails = [];
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            var getRowDr = $filter("filter")($scope.rowDetails, { "TrnType": "Dr", "ClearingAccountGLId": $scope.inventoryMaterialListPO[i].ClearingAccountGLId, "ClearingAccountBudgetMasterId": $scope.inventoryMaterialListPO[i].ClearingAccountBudgetMasterId, "ClearingAccountActivityId": $scope.inventoryMaterialListPO[i].ClearingAccountActivityId });
            var getRowCr = $filter("filter")($scope.rowDetails, { "TrnType": "Cr", "GLGeneralInfoId": $scope.inventoryMaterialListPO[i].GLGeneralInfoId, "BudgetMasterId": $scope.inventoryMaterialListPO[i].BudgetMasterId, "ActivityId": $scope.inventoryMaterialListPO[i].ActivityId });
            if (getRowDr.length == 0) {
                DrRows = {};
                DrRows.ClearingAccountGLId = $scope.inventoryMaterialListPO[i].ClearingAccountGLId;
                DrRows.ClearingAccountBudgetMasterId = $scope.inventoryMaterialListPO[i].ClearingAccountBudgetMasterId;
                DrRows.ClearingAccountActivityId = $scope.inventoryMaterialListPO[i].ClearingAccountActivityId;
                DrRows.TrnType = 'Dr';
                DrRows.GLGeneralInfoId = $scope.inventoryMaterialListPO[i].GLGeneralInfoId;
                DrRows.BudgetMasterId = $scope.inventoryMaterialListPO[i].BudgetMasterId;
                DrRows.ActivityId = $scope.inventoryMaterialListPO[i].ActivityId;
                DrRows.TrnAmount = $scope.inventoryMaterialListPO[i].TrnAmount;
                DrRows.TotalMaterialTranAmount = $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount;
                $scope.rowDetails.push(DrRows);
            }
            else {
                for (var j = 0; j < $scope.rowDetails.length; j++) {
                    if ($scope.inventoryMaterialListPO[i].ClearingAccountGLId == $scope.rowDetails[j].ClearingAccountGLId && $scope.inventoryMaterialListPO[i].ClearingAccountBudgetMasterId == $scope.rowDetails[j].ClearingAccountBudgetMasterId
                        && $scope.inventoryMaterialListPO[i].ClearingAccountActivityId == $scope.rowDetails[j].ClearingAccountActivityId && $scope.rowDetails[j].TrnType == 'Dr') {
                        $scope.rowDetails[j].TrnAmount += $scope.inventoryMaterialListPO[i].TrnAmount;
                        $scope.rowDetails[j].TotalMaterialTranAmount += $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount;
                    }
                }
            }
            if (getRowCr.length == 0) {
                CrRows = {};
                CrRows.ClearingAccountGLId = $scope.inventoryMaterialListPO[i].ClearingAccountGLId;
                CrRows.ClearingAccountBudgetMasterId = $scope.inventoryMaterialListPO[i].ClearingAccountBudgetMasterId;
                CrRows.ClearingAccountActivityId = $scope.inventoryMaterialListPO[i].ClearingAccountActivityId;
                CrRows.TrnType = 'Cr';
                CrRows.GLGeneralInfoId = $scope.inventoryMaterialListPO[i].GLGeneralInfoId;
                CrRows.BudgetMasterId = $scope.inventoryMaterialListPO[i].BudgetMasterId;
                CrRows.ActivityId = $scope.inventoryMaterialListPO[i].ActivityId;
                CrRows.TrnAmount = $scope.inventoryMaterialListPO[i].TrnAmount;
                CrRows.TotalMaterialTranAmount = $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount;
                $scope.rowDetails.push(CrRows);
            }
            else {
                for (var k = 0; k < $scope.rowDetails.length; k++) {
                    if ($scope.inventoryMaterialListPO[i].GLGeneralInfoId == $scope.rowDetails[k].GLGeneralInfoId && $scope.inventoryMaterialListPO[i].BudgetMasterId == $scope.rowDetails[k].BudgetMasterId
                        && $scope.inventoryMaterialListPO[i].ActivityId == $scope.rowDetails[k].ActivityId && $scope.rowDetails[k].TrnType == 'Cr') {
                        $scope.rowDetails[k].TrnAmount += $scope.inventoryMaterialListPO[i].TrnAmount;
                        $scope.rowDetails[k].TotalMaterialTranAmount += $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount;
                    }
                }
            }
        }
    }
    $scope.PurchaseDocAcceptanceList = [];
    $scope.merge = function () {
        for (var i = 0; i < $scope.acceptanceChargesCheckedList.length; i++) {
            var getRowDr = $filter("filter")($scope.PurchaseDocAcceptanceList, { "OpeningBankMasterId": $scope.acceptanceChargesCheckedList[i].OpeningBankMasterId });
            if (getRowDr.length == 0 && $scope.acceptanceChargesCheckedList[i].OpeningBankMasterId != null) {
                $scope.PurchaseDocAcceptanceList.push($scope.acceptanceChargesCheckedList[i]);
            }

        }
    }
    $scope.taxDetailVMList = [];
    $scope.Save1 = function () {
        $scope.PurchaseDocAcceptance.POId = $scope.POId;
        $scope.PurchaseDocAcceptance.PurchaseLCId = $scope.productNew.PurchaseLCNO;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.productNewForm.$valid) {
            if ($scope.Action === 'Post') {
                $scope.makerowDetails();
                try {
                    $http({
                        method: 'POST',
                        url: 'Products/PurchaseDocumentsAcceptance/DocumentAcceptancePost',
                        data: {
                            'voucherRows': $scope.PurchaseDocAcceptance
                            , 'docAcceptanceDetails': $scope.inventoryMaterialListPO
                            , 'rowDetails': $scope.rowDetails
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(' Save Successfully', 'success');
                            //$scope.PostingAcceptanceId= response.data.entity.Id;
                            $scope.gridAcceptancePostedList();
                            $scope.isSetAcceptenceList(2);
                            $scope.gridAcceptanceList();
                            $scope.Action = 'POST';
                            $scope.inventoryMaterialListPO = [];
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                } catch (e) {
                    ShowResult(e.Message, 'success');
                }
            }
            else if ($scope.Action === 'Charges Post') {
                $scope.merge();
                try {
                    $http({
                        method: 'POST',
                        url: 'Products/PurchaseDocumentsAcceptance/DocumentAcceptanceChargesPost',
                        data: {
                            'voucherRow': $scope.PurchaseDocAcceptance
                            , 'voucherRows': $scope.PurchaseDocAcceptanceList
                            , 'AcceptancechargesList': $scope.acceptanceChargesCheckedList
                            , 'taxDetailVMList': $scope.taxDetailVMList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.acceptanceChargesCheckedList = [];
                            $scope.gridAcceptanceChargesNonPostedList();
                            $scope.gridAcceptanceChargesPostedList();
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                } catch (e) {
                    ShowResult(e.Message, 'success');
                }
            }
        }
    };
    $scope.inventoryMaterialListPO = [];

    $scope.GridAcceptanceList = [];
    $scope.gridAcceptanceList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceNonPostedList',
        }).then(function successCallback(response) {
            $scope.GridAcceptanceList = response.data;
        });

    };
    $scope.gridAcceptanceList();


    $scope.GridAcceptancePostedList = [];
    $scope.gridAcceptancePostedList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptancePostedList',
        }).then(function successCallback(response) {
            $scope.GridAcceptancePostedList = response.data;
        });

    };
    $scope.gridAcceptancePostedList();


    $scope.GetAcceptanceChargesNonPostedList = [];
    $scope.gridAcceptanceChargesNonPostedList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceChargesNonPostedList',
        }).then(function successCallback(response) {
            $scope.GetAcceptanceChargesNonPostedList = response.data;
        });

    };
    $scope.gridAcceptanceChargesNonPostedList();

    $scope.GetAcceptanceChargesPostedList = [];
    $scope.gridAcceptanceChargesPostedList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceChargesPostedList',
        }).then(function successCallback(response) {
            $scope.GetAcceptanceChargesPostedList = response.data;
        });

    };
    $scope.gridAcceptanceChargesPostedList();

    $scope.AcceptancePOServiceList = [];
    $scope.getAcceptancePOService = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptancePOServiceNonPostedList',
        }).then(function successCallback(response) {
            $scope.AcceptancePOServiceList = response.data;
        });

    };
    $scope.getAcceptancePOService();


    $scope.GridAcceptanceListDetail = [];
    $scope.gridAcceptanceListDetail = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceDetailList',
        }).then(function successCallback(response) {
            $scope.GridAcceptanceListDetail = response.data;
        });
    };
    $scope.gridAcceptanceListDetail();


    $scope.GridAcceptanceServiceList = [];
    $scope.gridAcceptanceServiceList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceDetailList',
        }).then(function successCallback(response) {
            $scope.GridAcceptanceListDetail = response.data;
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
            // window.GetMaterialByIdList = response.data;
        });
    }
    $scope.GetMaterialById();

    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = function detailGridData(e) {
    //    //debugger;

    //    var filteredData = e.data["Id"];
    //    var data = ej.DataManager(window.GetMaterialByIdList).executeLocal(ej.Query().where("AcceptenceId", "equal", parseInt(filteredData), true).take(5));
    //    e.detailsElement.find("#detailGrid").ejGrid({

    //        dataSource: data,
    //        columns: ["MaterialMasterGroupName", "MaterialMasterName", "StandardName", "Article", "SKU1", "SKU2", "SKU3", "TransactionUoM", "Rate", "Amount"]
    //    });
    //    e.detailsElement.find(".tabcontrol").ejTab();
    //}

    $scope.GRN = "";
    $scope.tab = 1;
    $scope.setTabAcceptenceList = function (newTab) {
        $scope.tab = newTab;
        if ($scope.tab == 2) {
            $scope.gridAcceptancePostedList();
        }
    };
    $scope.isSetAcceptenceList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 1;

    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        //debugger;
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

    $scope.ClearFields = function () {
        $scope.PurchaseDocAcceptance.Id = null;

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
            $scope.POId = $scope.GetDataDoubleClickMaster[0].POId;
            $scope.productNew.PurchaseLCNO = $scope.GetDataDoubleClickMaster[0].PurchaseLCNO;
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
        });
    };

    $scope.GetDataDoubleClickDetails = [];
    $scope.getRecordDoubleClickDetail = function (Id) {
        //debugger;
        var PoType = 'PO';
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceDetailForPost?Id=' + Id,
        }).then(function successCallback(response) {
            $scope.GetDataDoubleClickDetails = response.data;
            $scope.inventoryMaterialListPO = $scope.GetDataDoubleClickDetails;
        });
    };

    $scope.getRecordDoubleClickDetailGRN = function (Id) {
        $scope.inventoryMaterialListPO = [];
        $http.get('Products/PurchaseDocumentsAcceptance/GetGRNAcceptanceDetailForPost?PurchaseDocAcceptanceId=' + Id)
            .then(function (response) {
                $scope.GetDataDoubleClickDetails = response.data;
                $scope.inventoryMaterialListPO = response.data;
            });
    };

    $scope.GetServiceDetails = [];
    $scope.GetService = function (Id) {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseDocumentsAcceptance/GetAcceptanceServiceListForPost?Id=' + Id,
        }).then(function successCallback(response) {
            $scope.GetServiceDetails = response.data;
            $scope.acceptanceChargesCheckedList = $scope.GetServiceDetails;
        });
    };
    $scope.getMatureDate = function (date, days) {
        if (!baseService.isUndefinedOrNull(date)) {
            date = new Date(date);
            date.setDate(date.getDate() + days);
            $scope.PurchaseDocAcceptance.DueDate = $filter("date")(date, "dd-MMM-yyyy");
        }
    };

    $scope.nonPostedAcceptance = function ($event) {
        var x = $event.data.Id;
        $scope.Id = $event.data.Id;
        $scope.productNew = $event.data;
        $scope.productNew.PurchaseLCNO = $event.data.PurchaseLCId;
        $scope.productNew.PaymentTermName = $event.data.PaymentTermName;
        $scope.LCOpeningBank = $event.data.LCOpeningBank;
        $scope.productNew.PODate = $event.data.PODate;
        $scope.productNew.ContractId = $event.data.ContractId;
        $scope.productNew.PartyName = $event.data.PartyName;
        $scope.productNew.LCExpiryDate = $event.data.LCExpiryDate;
        $scope.productNew.LCOpeningDate = $event.data.LCOpeningDate;
        $scope.productNew.CustomerName = $event.data.CustomerName;
        $scope.PurchaseDocAcceptance.Id = $event.data.Id;
        $scope.PurchaseDocAcceptance.AcceptanceDate = $event.data.AcceptanceDate;
        $scope.PurchaseDocAcceptance.AcceptanceNo = $event.data.AcceptanceNo;
        $scope.PurchaseDocAcceptance.Remarks = $event.data.Remarks;
        $scope.PurchaseDocAcceptance.InvoiceDate = $event.data.InvoiceDate;
        $scope.PurchaseDocAcceptance.Tenure = $event.data.Tenure;
        $scope.PurchaseDocAcceptance.CurrencyName = $event.data.CurrencyName;
        $scope.PurchaseDocAcceptance.CurrencyId = $event.data.CurrencyId;
        $scope.PurchaseDocAcceptance.ToCurrencyRate = $event.data.ToCurrencyRate;
        $scope.PurchaseDocAcceptance.IsNonCreditable = $event.data.IsNonCreditable;
        $scope.PurchaseLCNo = $event.data.PurchaseLCId;
        $scope.PurchaseDocAcceptance.PaymentType = $event.data.PaymentType;
        $scope.PurchaseDocAcceptance.AcceptanceAmount = $event.data.AcceptanceAmount;
        $scope.PurchaseDocAcceptance.Amount = $event.data.AcceptanceAmount;
        $scope.getMatureDate($scope.PurchaseDocAcceptance.AcceptanceDate, $scope.PurchaseDocAcceptance.Tenure)
        $scope.acceptanceChargesCheckedList = [];

        if ($event.data.AcceptanceFirst == 'Yes') {
            $scope.getRecordDoubleClickDetail(x);
        }
        else {
            $scope.getRecordDoubleClickDetailGRN(x);
        }
        getServiceChargeList($scope.Id);
        getSavedServicePODetailList($scope.Id);
        //$scope.GetCurrencyExchangeRateList();
        $scope.Action = 'Post';
        if (!$rootScope.isCollapsed) $rootScope.toggle();

    }

    $scope.nonPostedAcceptanceService = function ($event) {
        var x = $event.data.Id;
        $scope.Id = $event.data.Id;
        $scope.productNew = $event.data;
        $scope.productNew.PurchaseLCNO = $event.data.PurchaseLCId;
        $scope.productNew.PaymentTermName = $event.data.PaymentTermName;
        $scope.LCOpeningBank = $event.data.LCOpeningBank;
        $scope.productNew.PODate = $event.data.PODate;
        $scope.productNew.ContractId = $event.data.ContractId;
        $scope.productNew.PartyName = $event.data.PartyName;
        $scope.productNew.LCExpiryDate = $event.data.LCExpiryDate;
        $scope.productNew.PurchaseDocAcceptance = $event.data.LCOpeningDate;
        $scope.productNew.CustomerName = $event.data.CustomerName;
        $scope.PurchaseDocAcceptance.Id = $event.data.Id;
        $scope.PurchaseDocAcceptance.AcceptanceDate = $event.data.AcceptanceDate;
        $scope.PurchaseDocAcceptance.AcceptanceNo = $event.data.AcceptanceNo;
        $scope.PurchaseDocAcceptance.Remarks = $event.data.Remarks;
        $scope.PurchaseDocAcceptance.CurrencyId = $event.data.CurrencyId;
        $scope.PurchaseDocAcceptance.CurrencyName = $event.data.CurrencyName;
        $scope.PurchaseDocAcceptance.ToCurrencyRate = $event.data.ToCurrencyRate;
        $scope.PurchaseLCNo = $event.data.PurchaseLCId;
        $scope.inventoryMaterialListPO = [];
        $scope.GetService(x);
        //$scope.GetCurrencyExchangeRateList();
        $scope.Action = 'Charges Post';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        $scope.setTab(2);
    }

    $scope.recorddoubleclickPO = function ($event) {
        //debugger;


        var x = $event;
        var Id = x.data.Id;
    }
    function GetInventoryMaterialListByPO(inveReveiveId) {
        //debugger;
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListByOnlyPO?inveReveiveId=' + inveReveiveId)
            .then(function (response) {

                $scope.inventoryMaterialListPO = response.data.Rows;
                $scope.POPopUpClose();
            });
    }
    $scope.inventoryMaterialListPO1 = [];
    function GetInventoryMaterialListByPO1(inveReveiveId) {
        //debugger;
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListByOnlyPO?inveReveiveId=' + inveReveiveId)
            .then(function (response) {

                $scope.inventoryMaterialListPO1 = response.data.Rows;
            });
    }
    $scope.POmaterialDetailsPOPUP = function () {
        var gridObj = $("#seletedLSTGrid").data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        GetInventoryMaterialListByPO1($scope.podata.Id);
        angular.element(document.querySelector('#ListOfRequisition')).modal('show');
    }


    $scope.close = function () {
        angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
    }


    $scope.ItemSelectToSave = function () {
        //debugger;
        for (var i = 0; i < $scope.inventoryMaterialListPO1.length; i++) {
            if ($scope.inventoryMaterialListPO1[i].Active === true) {
                if ($scope.inventoryMaterialListPO.length > 0) {
                    for (var j = 0; j < $scope.inventoryMaterialListPO.length; j++) {
                        if ($scope.inventoryMaterialListPO1[i].POID === $scope.inventoryMaterialListPO[j].POID && $scope.inventoryMaterialListPO1[i].InventoryReceiveDetailId === $scope.inventoryMaterialListPO[j].InventoryReceiveDetailId) {
                            ShowResult('PO Material Already Added', 'failure', 'ListOfRequisition');
                            return false;
                        }
                        else {
                            $scope.inventoryMaterialListPO.push($scope.inventoryMaterialListPO1[i]);
                            ShowResult('PO Material Added Successfully', 'success');
                            return false;
                        }
                    }

                }
                else {
                    $scope.inventoryMaterialListPO.push($scope.inventoryMaterialListPO1[i]);
                    ShowResult('PO Material Added Successfully', 'success', 'ListOfRequisition');
                }
            }
        }
    }

    $scope.AllTabPrint = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        $window.open('Products/PurchaseDocumentsAcceptance/DocumentAcceptanceVoucher?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
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
    }


    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
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
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.ServicePODetailList = [];
    function getSavedServicePODetailList(acceptanceID) {
        $scope.SavedServicePODetailList = [];
        $http.get('Products/PurchaseDocumentsAcceptance/GetSavedServicePOList?acceptanceID=' + acceptanceID)
            .then(function (response) {
                $scope.ServicePODetailList = response.data;
            });
    }


    $scope.Clear = function () {

        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.PurchaseDocAcceptance.Id = '';
        $scope.PurchaseDocAcceptance.AcceptanceNo = '';
        $scope.PurchaseDocAcceptance.AcceptanceDate = '';
        $scope.PurchaseDocAcceptance.CurrencyName = '';
        $scope.PurchaseDocAcceptance.Remarks = '';
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
        };

        $scope.inventoryMaterialListPO = [];
        $scope.Action === 'Save';
        $scope.seletedLST = [];
        $scope.GridListPO = [];
    }

    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.purDocAcceptanceId = data.Id;
        $scope.voucherId = data.VoucherId;
  
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };



    $scope.delete = function (PDocAccId, vId) {
        $http({
            method: "POST",
            url: 'Products/PurchaseDocumentsAcceptance/DeletePurchaseDocAcceptance',
            data: {
                "pdocAccpId": PDocAccId, "voucherId": vId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
                $scope.purDocAcceptanceId = null;
                $scope.voucherId = null;
        
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };



}
