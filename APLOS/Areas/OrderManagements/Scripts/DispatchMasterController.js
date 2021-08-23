'use strict';
DispatchMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function DispatchMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Dispatch Master";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.salesVM = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
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
        ProductionOrderId: null
    };

    $scope.searchBy = "PartyName"; $scope.search = "";
    $scope.searchByList = [{ value: 'PartyName', name: "Party" }, { value: 'PartyCode', name: "Party Code" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEmployeeDataList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employees = response.data;
        });
    }
   // $scope.getData();

    $scope.showPartyPopUpNew = function () {
        $scope.partyType = 'Customer';
        $scope.searchByParty = "UserName"; $scope.searchParty = "";
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
        angular.element(document.querySelector('#partyPopUpNew')).modal('show');
    };

    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUpNew')).modal('hide');
    }

    $scope.SetVendorData = function (obj) {
        $scope.salesVM.PartyId = obj.data.Id;
        $scope.salesVM.PartyName = obj.data.UserName;
        $scope.getPartyPlant();
        angular.element(document.querySelector('#partyPopUpNew')).modal('hide');
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

    $scope.selectedMasterOrderList = [];
    function MakeData() {
        $scope.selectedMasterOrderList = [];
        try {
            for (var i = 0; i < $scope.masterOrderList.length; i++) {
                var getRow = $filter("filter")($scope.selectedMasterOrderList, { "selectedMasterOrderList": $scope.masterOrderList[i].MasterOrderId, "MasterOrderItemId": $scope.masterOrderList[i].MasterOrderItemId });
                //var getRow = $filter("filter")($scope.selectedMasterOrderList, { "selectedMasterOrderList": $scope.masterOrderList[i].MasterOrderId });
                if (getRow.length == 0) {
                    if ($scope.masterOrderList[i].Active == true) {
                        var ob = {};
                        ob.Id = null;
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
            // $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
            //  $scope.GetCurrencyExchangeRateList();
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

    $scope.getMasterOrderItem = function () {

        angular.element(document.querySelector('#masterOrderItemPopUp')).modal('show');
    }

    $scope.selectedMasterOrderItemList = [];
    function MakeItemData() {
        //$scope.selectedMasterOrderItemList = [];
        for (var i = 0; i < $scope.masterOrderItemList.length; i++) {
            //if ($scope.masterOrderItemList[i].Balance != 0 && $scope.masterOrderItemList[i].Balance > 0) {
            if (checkItemExist($scope.selectedMasterOrderItemList, $scope.masterOrderItemList[i].SONo, $scope.masterOrderItemList[i].MaterialMasterId, $scope.masterOrderItemList[i].ArticleId, $scope.masterOrderItemList[i].FirstCharacteristicsValueId, $scope.masterOrderItemList[i].SecondCharacteristicsValueId) === false) {
                //if (checkItemExist($scope.selectedMasterOrderItemList, $scope.masterOrderItemList[i].SONo, $scope.masterOrderItemList[i].MasterOrderItemId) === false) {
                if ($scope.masterOrderItemList[i].Active == true) {
                    $scope.CheckMaterialArticleSKU($scope.masterOrderItemList[i]);
                    if ($scope.mValid == true) {
                        var moi = {};
                        moi.Id = null;
                        moi.PONumber = $scope.masterOrderItemList[i].PONumber;
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
                        moi.BaseAmount = $scope.masterOrderItemList[i].Qty * $scope.masterOrderItemList[i].Rate;
                        moi.TransactionAmount = 0;
                        moi.NetAmount = 0;
                        moi.TaxAmount = 0;

                        moi.TaxList = $scope.materialtaxCategoryList;
                        $scope.salesVM.InvoicingPartyPlantId = $scope.masterOrderItemList[i].InvoicingPartyPlantId;
                        moi.InvoicingPartyPlantId = $scope.masterOrderItemList[i].InvoicingPartyPlantId;
                        moi.HSNCodeId = $scope.masterOrderItemList[i].HSNCodeId;
                        $scope.salesVM.MasterOrderId = $scope.masterOrderItemList[i].MasterOrderId;
                        moi.ExistSalesQty = $scope.masterOrderItemList[i].ExistSalesQty;
                        moi.Balance = $scope.masterOrderItemList[i].Balance;

                        $scope.selectedMasterOrderItemList.push(moi);



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
                if (getRowParty[0].IsSKU === 2 && mdata.FirstCharacteristicsValueId === null || mdata.SecondCharacteristicsValueId === null) {
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

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };

    $scope.closeInvoicingPartyPopUp = function () {

        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };


    $scope.ProductionOrderList = [];
    $scope.ProdOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        $scope.ProductionOrderList = [];
        $http.get("OrderManagements/PackingConfirmation/GetProductionOrderDataList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionOrderList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };

    $scope.packingContenNew = {
        PackingForm: null
        , QtyPackingForm: null
        , ConPackingForm: null
        , BalancePackingForm: null
        , ColumnName: null
    }

    $scope.PackingContentDataListByPR = [];
    $scope.GetPackingContentDataByPRId = function (obj) {
        var modeldata = {};
        modeldata.ProductionOrderId = obj.data.POId;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');

        $http({
            method: 'GET',
            url: 'OrderManagements/PackingContent/GetPackingContentDataByPRIdWithTran?PRId=' + modeldata.ProductionOrderId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
               // console.log(response.data);
                $scope.packingContenNew.PackingForm = "No of " + response.data[0].PackingForm;
                $scope.packingContenNew.QtyPackingForm = "Qty/" + response.data[0].PackingForm;
                $scope.packingContenNew.ConPackingForm = "Confirmed " + response.data[0].PackingForm;
                $scope.packingContenNew.BalancePackingForm = "Balance " + response.data[0].PackingForm;
                $scope.packingContenNew.ColumnName = response.data[0].PackingForm;

                modeldata.Id = response.data[0].Id;
                modeldata.Description = response.data[0].Description;
                modeldata.NoOfQty = response.data[0].NoOfQty;
                modeldata.NoOfLine = response.data[0].NoOfLine;
                modeldata.Confirmed = response.data[0].Confirmed;
                modeldata.Balance = response.data[0].Balance;
                modeldata.NetWeight = response.data[0].NetWeight;
                modeldata.GrossWeight = response.data[0].GrossWeight;

                $scope.ProdOrderList.push(modeldata);
            }
        });
       
    };

    $scope.PackingContentDataList = [];
    $scope.getDetailData = function (obj) {
        $scope.PackingContentDataList = [];
        $http.get("OrderManagements/PackingContent/GetPackingContentDetailDataList?MasterId=" + obj.data.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PackingContentDataList = response.data;
                        angular.element(document.querySelector('#PackingContentPopUp')).modal('show');
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };


    $scope.ClosePackingContentPopUp = function () {
        angular.element(document.querySelector('#PackingContentPopUp')).modal('hide');
    }

    $scope.lineItemNo = [];
    $scope.getPackingChildData = function (obj) {
        $scope.lineItemNo = [];
        $http.get("OrderManagements/DispatchMaster/GetPackingChildDataList?MasterId=" + obj.data.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.lineItemNo = response.data;


                        //$scope.PackingContentDataList = [];
                        //$http.get("OrderManagements/PackingContent/GetPackingContentDetailDataList?MasterId=" + obj.data.Id)
                        //    .then(
                        //        function successCallback(response) {
                        //            if (baseService.arrayLength(response.data) > 0) {
                        //                $scope.PackingContentDataList = response.data;
                        //            }
                        //        },
                        //        function errorCallback(response) {
                        //            ShowResult(response, 'failure');
                        //        });

                       
                    }
                    angular.element(document.querySelector('#IPPopup')).modal('show');
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.CloseIPPopup = function () {
        angular.element(document.querySelector('#IPPopup')).modal('hide');
    }

    $scope.ShowMultiProductionPopup = function () {
        angular.element(document.querySelector('#MultiProductionPopup')).modal('show');
    }

    $scope.CloseMultiProductionPopup = function () {
        angular.element(document.querySelector('#MultiProductionPopup')).modal('hide');
    }

    $scope.ShowPackDetailPopupPopup = function (data) {
        $http({
            method: 'GET',
            url: 'OrderManagements/PackingContent/GetPackingContentDataByPRIdWithTran?PRId=' + data.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.PackingContentDataListByPR = response.data;
            $scope.packingContenNew.PackingForm = "No of " + response.data[0].PackingForm;
            $scope.packingContenNew.QtyPackingForm = "Qty/" + response.data[0].PackingForm;
            $scope.packingContenNew.ConPackingForm = "Confirmed " + response.data[0].PackingForm;
            $scope.packingContenNew.BalancePackingForm = "Balance " + response.data[0].PackingForm;
            $scope.packingContenNew.ColumnName = response.data[0].PackingForm;
        });
        angular.element(document.querySelector('#PackDetailPopup')).modal('show');
    }

    $scope.ClosePackDetailPopup = function () {
        angular.element(document.querySelector('#PackDetailPopup')).modal('hide');
    }

    

    


    // #region ContractItem

    $scope.selectedmasterOrderDataList = [];
    $scope.GetContractItemDataList = function () {

        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetContractItemDataList?contractId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.selectedmasterOrderDataList = response.data;
        });
    }

    $scope.SalesOrderDataList = [];
    $scope.GetSalesOrderDataList = function () {
        $scope.SalesOrderDataList = [];
        $http({
            method: 'GET',
            url: "OrderManagements/DispatchMaster/GetSOList?customerId=" + $scope.salesVM.PartyId
        }).then(function (response) {
            $scope.SalesOrderDataList = response.data;

            if (baseService.arrayLength($scope.selectedSalesOrderDataList) > 0) {
                for (var i = 0; i < $scope.selectedSalesOrderDataList.length; i++) {
                    for (var j = 0; j < $scope.SalesOrderDataList.length; j++) {
                        if ($scope.selectedSalesOrderDataList[i].SalesOrderId === $scope.SalesOrderDataList[j].SalesOrderId) {
                            $scope.SalesOrderDataList[j].Active = true;
                        }
                    }
                }
            }
            angular.element(document.querySelector('#SOPopUp')).modal('show');
        });
    }

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
            for (var i = 0; i < $scope.SalesOrderDataList.length; i++) {
                $scope.SalesOrderDataList[i].Active = ChkOrUnchk;
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
    $scope.hasError = false;

    $scope.selectedSalesOrderDataList = [];
    function MakeSOItemData() {
        $scope.selectedSalesOrderDataList = [];
        try {
            for (var i = 0; i < $scope.SalesOrderDataList.length; i++) {
                var getRow = $filter("filter")($scope.selectedSalesOrderDataList, { "selectedSalesOrderDataList": $scope.SalesOrderDataList[i].SalesOrderId});
                //var getRow = $filter("filter")($scope.selectedMasterOrderList, { "selectedMasterOrderList": $scope.masterOrderList[i].MasterOrderId });
                if (getRow.length == 0) {
                    if ($scope.SalesOrderDataList[i].Active == true) {
                        var ob = {};
                        ob.SalesOrderId = $scope.SalesOrderDataList[i].SalesOrderId;

                        if (checkExistSalesOrder($scope.selectedSalesOrderDataList, ob.SalesOrderId)) {
                            ob.Id = null;
                            ob.Active = $scope.SalesOrderDataList[i].Active;
                            ob.DeliveryDate = $scope.SalesOrderDataList[i].DeliveryDate;
                            ob.DestinationName = $scope.SalesOrderDataList[i].DestinationName;
                            ob.CommitmentDate = $scope.SalesOrderDataList[i].CommitmentDate;
                            ob.PONumber = $scope.SalesOrderDataList[i].PONumber;
                            ob.ShipmentModeName = $scope.SalesOrderDataList[i].ShipmentModeName;
                            ob.BuyerItem = $scope.SalesOrderDataList[i].BuyerItem;
                            ob.BuyerOrder = $scope.SalesOrderDataList[i].BuyerOrder;
                            ob.OwnOrder = $scope.SalesOrderDataList[i].OwnOrder;
                            ob.OwnItem = $scope.SalesOrderDataList[i].OwnItem;
                            ob.ProductionOrderId = $scope.SalesOrderDataList[i].ProductionOrderId;
                            
                            $scope.selectedSalesOrderDataList.push(ob);
                            $scope.hasError = false;
                        }
                        else {
                            $scope.hasError = true;
                            //throw 'Select same Customer.';
                        }
                    }

                }

            }

        } catch (e) {
            ShowResult(e, 'failure', 'SOPopUp');
        }
    }

    function checkExistSalesOrder(list, SalesOrderId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalesOrderId == SalesOrderId) {
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

    $scope.CloseSOPopUp = function () {
        MakeSOItemData();
        angular.element(document.querySelector('#SOPopUp')).modal('hide');
    };

    $scope.Action = "Save";
    $scope.Save = function () {
        try {
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.DispatchFrom.$valid) {
                if ($scope.Action === "Save" || $scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: "OrderManagements/DispatchMaster/Insert",
                        data: {
                            "data": $scope.salesVM
                            , "selectedSalesOrderList": $scope.selectedSalesOrderDataList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.GetAllConfirmedPackingContentData();


                            $scope.Action = "Update";
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                return true;
            }
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.AllConfirmedPackingContentDataList = [];
    $scope.GetAllConfirmedPackingContentData = function () {
        $scope.AllConfirmedPackingContentDataList = [];
        $http.get("OrderManagements/DispatchMaster/GetAllConfirmedPackingContentData")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.AllConfirmedPackingContentDataList = response.data;


                        $scope.packingContenNew.PackingForm = "No of " + response.data[0].PackingForm;
                        $scope.packingContenNew.QtyPackingForm = "Qty/" + response.data[0].PackingForm;
                        $scope.packingContenNew.ConPackingForm = "Confirmed " + response.data[0].PackingForm;
                        $scope.packingContenNew.BalancePackingForm = "Balance " + response.data[0].PackingForm;
                        $scope.packingContenNew.NetWeight = "Net Weight/"+response.data[0].PackingForm;
                        $scope.packingContenNew.GrossWeight = "Gross Weight/"+response.data[0].PackingForm;

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
       
    };


    $scope.masterDataList = [];
    $scope.getMasterData = function () {
        $scope.PackingContentDataList = [];
        $http.get("OrderManagements/DispatchMaster/GetList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterDataList = response.data;
                        
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getMasterData();

    $scope.selectedSalesOrderDataList = [];
    $scope.GetDispatchDetailSOList = function () {
        $scope.selectedSalesOrderDataList = [];
        $http.get("OrderManagements/DispatchMaster/GetDispatchDetailSOList?masterId=" + $scope.salesVM.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.selectedSalesOrderDataList = response.data;

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.Get = function (obj) {
        $scope.model = obj.data;
        $scope.salesVM = Object.assign({}, $scope.model);
        $scope.GetDispatchDetailSOList();

      
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    // #endregion ContractItem









}

