'use strict';
PIPackingListController.$inject = ['commonMessage', '$controller', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function PIPackingListController(commonMessage, $controller, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "PI Packing List";
    $scope.Action = 'Save';
    $scope.fabricRollMasters = [];
    $scope.selectedGRNList = [];
    $scope.path = 'Commercial/PIPackingList/';
    $scope.CostingPath = 'Costings/costingItem/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.Deletepath = $scope.path + 'DeletePI';
    $scope.saveUrl = $scope.path + 'create';
    $scope.savePIPackingListUrl = $scope.path + 'savePIPackingList';
    $scope.newVersionUrl = $scope.path + 'NewVersion';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("TaskScheduleController", { cboService: cboService, $scope: $scope, $http: $http });

    $controller("CurrencyExchangeController", { cboService: cboService, $scope: $scope, $http: $http, TableName: 'MasterOrderExchangeRates' });
    $scope.PIVersionModel = {
        Id: null,
        PIMasterId: null,
        VersionNo: null,
        VersionRefNo: null,
        VersionDate: null
    };

    $scope.PIPackingListMaterial = {
        Id: null
        , PIPackingListMasterId: null
        , PIMaterialId: null
        , PIQuantity: null
        , PIUoMId: null
    };
    $scope.PIPackingListMaterialTemp = Object.assign({}, $scope.PIPackingListMaterial);
    $scope.PIPackingListMaster = {
        Id: null
        , Description: null
        , Remarks: null
        , PImasterId: null
    };
    $scope.PIPackingListMasterTemp = Object.assign({}, $scope.PIPackingListMaster);

    $scope.SelectedPIVersion = null;
    $scope.VersionList = [];
    $scope.VersionList.push(Object.assign({}, $scope.PIVersionModel));


    $scope.PIHeaderModel = {
        Id: null
        , PINo: null
        , PIDate: null
        , RefNo: null
        , RevisionNo: null
        , BuyerId: null
        , Buyer: null
        , CustomerId: null
        , Customer: null
        , Currency: null
        , Description: null
        , Remarks: null
        , Quantity: 0
        , UoM: null
        , DeliveryDate: null
        , Amount: 0
        , CurrencyId: null
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , PartyCode: null
        , CustomerName: null
        , PartyId: null
        , PartyAccountGroupId: null
        , IsPaymentTermChangeable: null
        , PaymentTermId: null
        , SumAmount: 0
        , QTY:0
    };
    $scope.PImodelNew = Object.assign({}, $scope.PIHeaderModel);

    $scope.buyerList = [];
    cboService.getCboBuyer(function (data) {
        $scope.buyerList = data;
    });

    $scope.searchPIByList = [
        {
            name: 'Id',
            value: 'Id'
        },
        {
            name: 'PI No.',
            value: 'PINo'
        },
        {
            name: 'Ref No.',
            value: 'RefNo'
        },
        {
            name: 'PI Date',
            value: 'PIDate'
        },
        {
            name: 'Currency',
            value: 'CurrencyId'
        },
        {
            name: 'Buyer',
            value: 'BuyerId'
        },
        {
            name: 'Customer',
            value: 'CustomerId'
        },
        {
            name: 'Invoicing by Address',
            value: 'InvoicingByAddress'
        },
        {
            name: 'Delivery by Address',
            value: 'DeliveryByAddress'
        }
    ];
    $scope.PIGridModelBase = {
        Id: null
        , PIMasterId: null
        , PIVersionId: null
        , MaterialGroupMasterId: null
        , Description: null
        , Quantity: 0
        , Rate: 0
        , UoMId: null
        , UoM: null
        , DeliveryDate: null
        , CurrencyId: null
        , Amount: 0

    };
    $scope.PIGridModel = Object.assign({}, $scope.PIGridModelBase);

    $scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.PIGridModel));
    $scope.SumAmount = function (item) {
        item.Amount = parseFloat(item.Quantity) * parseFloat(item.Rate);
    }

    $scope.SubmitH = function (data) {
        try {
            var newObj = Object.assign({}, $scope.PIGridModel);
            if (data != null) {
                newObj = {
                    Id: null
                    , PIMasterId: null
                    , PIVersionId: null
                    , MaterialGroupMasterId: null
                    , Description: null
                    , Quantity: 0
                    , Rate: 0
                    , UoMId: null
                    , UoM: null
                    , DeliveryDate: null
                    , CurrencyId: null
                    , Amount: 0
                }
            }
            $scope.DataList.push(newObj);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
    }
    $scope.ClearGrid = function () {
        $scope.DataList = [];
        $scope.DataList.push(Object.assign({}, $scope.PIGridModelBase));
    }
    $scope.PIPopUpList = [];
    $scope.LoadPIPopUp = function () {
        $scope.PIPopUpList = [];
        try {
            $http({
                method: 'POST',
                url: $scope.path + "PIList",
                data: {},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PIPopUpList = [];
                $scope.PIPopUpList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.LoadPIPopUp();
    $scope.PIMasterId = '';
    $scope.PIPopUp = function () {
        angular.element(document.querySelector('#PIPOPopup')).modal('show');
    }
    $scope.ClosePIPopUp = function () {
        angular.element(document.querySelector('#PIPOPopup')).modal('hide');
    }
    $scope.GetPIPopUp = function (args) {
        $scope.SelectedPIVersion = args.data.PIVersionId;
        $scope.PIMasterId = args.data.Id;
        $http({
            method: 'GET',
            url: $scope.path + "GetAllData?PIMasterId=" + args.data.Id + '&VersionId=' + args.data.PIVersionId + '&PIPackingListMasterId=' + args.data.PIPackingListMasterId,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.PImodelNew = response.data.PIMaster[0];
                $scope.PIPackingListMasterTemp = response.data.PIPackingListMasterData[0];
                /*           $scope.PIVersionModel = response.data.VarsionData;*/
                $scope.DataList = response.data.ItemData;
                /*       $scope.PIVersionModel.VersionNo = $scope.PIVersionModel[0].Id;*/
                $scope.VersionList = $scope.PIVersionModel;
                $scope.PIVersionModel.VersionNo = args.data.LastVersion;

                $scope.ClosePIPopUp();
            }
        });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetPIPopUp2 = function (args) {
        $scope.SelectedPIVersion = args.data.PIVersionId;
        $scope.PIMasterId = args.data.Id;
        $http({
            method: 'GET',
            url: $scope.path + "GetAllData2?PIMasterId=" + args.data.Id + '&VersionId=' + args.data.PIVersionId,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.PImodelNew = response.data.PIMaster[0];
                // $scope.PIPackingListMasterTemp = response.data.PIPackingListMasterData[0];
                /*           $scope.PIVersionModel = response.data.VarsionData;*/
                $scope.DataList = response.data.ItemData;
                /*       $scope.PIVersionModel.VersionNo = $scope.PIVersionModel[0].Id;*/
                $scope.VersionList = $scope.PIVersionModel;
                $scope.PIVersionModel.VersionNo = args.data.LastVersion;

                $scope.ClosePIPopUp();
            }
        });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    //$scope.GetPIPopUp = function (args) {
    //    $scope.SelectedPIVersion = args.data.PIVersionId;
    //    $scope.PIMasterId = args.data.Id;
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "GetAllData?PIMasterId=" + args.data.Id + '&VersionId=' + args.data.PIVersionId + '&PIPackingListMasterId=' + args.data.PIPackingListMasterId,
    //    }).then(function successCallback(response) {
    //        if (!baseService.isUndefinedOrNull(response.data)) {
    //            $scope.PImodelNew = response.data.PIMaster[0];
    //            $scope.PIPackingListMasterTemp = response.data.PIPackingListMasterData[0];
    //            /*           $scope.PIVersionModel = response.data.VarsionData;*/
    //            $scope.DataList = response.data.ItemData;
    //            /*       $scope.PIVersionModel.VersionNo = $scope.PIVersionModel[0].Id;*/
    //            $scope.VersionList = $scope.PIVersionModel;
    //            $scope.PIVersionModel.VersionNo = args.data.LastVersion;

    //            $scope.ClosePIPopUp();
    //        }
    //    });
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    $scope.searchByPIPackingList = [
        {
            name: 'PI Packing No.',
            value: 'PIPackingListMasterId'
        },
        {
            name: 'Description',
            value: 'Description'
        },
        {
            name: 'Remarks',
            value: 'Remarks'
        },
        {
            name: 'PI Packing Date',
            value: 'AddedDate'
        }
    ];
    $scope.PIPackingSearchBy = "PIPackingListMasterId";
    $scope.PIPackingSearch = "";
    $scope.PIPackingGridList = [];
    $scope.LoadPIPackingList = function () {
        $scope.PIPackingGridList = [];
        try {
            $http({
                method: 'POST',
                url: $scope.path + "PIPackingList",
                data: { 'column': $scope.PIPackingSearchBy, 'value': $scope.PIPackingSearch },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PIPackingGridList = [];
                $scope.PIPackingGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.LoadPIPackingList();


    $scope.PIPackingMaterialGridList = [];
    $scope.AllocatedPIQty = 0;
    $scope.TotalPIQty = 0;
    $scope.PIMaterialId = '';
    $scope.POPopUpHeader = {};

    $scope.POQTYAllocation = function (args) {
        try {
            $scope.POPopUpHeader = args.data;
            if (baseService.isUndefinedOrNull($scope.PIPackingListMasterTemp.Description))
                throw "Please add Description.";
            if (baseService.isUndefinedOrNull($scope.PIPackingListMasterTemp.Remarks))
                throw "Please add Remarks.";
            $scope.AllocatedPIQty = args.data.AllocatedQty;
            $scope.TotalPIQty = args.data.Quantity;
            $scope.PIPackingListMaterialTemp = args.data;
            $scope.PopUpDataList(args.data.Id, args.data.MaterialGroupMasterId);
            angular.element(document.querySelector('#QTYAllocation')).modal('show');
        } catch (e) {
            ShowResult(e, 'info');
        }

    };
    $scope.PIPackingMaterialPopUpList = [];
    $scope.PopUpDataList = function (PIMaterialID, PIMaterialGroupID) {

        $http({
            method: 'GET',
            url: $scope.path + 'GetPopUp?PIMaterial=' + PIMaterialID + '&PIMaterialGroup=' + PIMaterialGroupID,
        }).then(function (response) {
            $scope.PIPackingMaterialPopUpList = response.data.data;
        });
        angular.element(document.querySelector('#QTYAllocation')).modal('show');
    }
    $scope.SumModel = {
        QTY: 0,
        Amount: 0
    };

    $scope.ASSSSDFG = function () {
        $scope.PImodelNew.SumAmount = 0;
        $scope.PImodelNew.QTY = 0;
        for (var i = 0; i < $scope.PIPackingMaterialPopUpList.length; i++) {
            if ($scope.PIPackingMaterialPopUpList[i].Active) {
                $scope.PImodelNew.SumAmount += $scope.PIPackingMaterialPopUpList[i].POAmount;
                $scope.PImodelNew.QTY += $scope.PIPackingMaterialPopUpList[i].POQty;
            }
        }
        $scope.PImodelNew.SumAmount = parseFloat($scope.PImodelNew.SumAmount).toFixed(2);
        $scope.PImodelNew.QTY = parseFloat($scope.PImodelNew.QTY).toFixed(2);
    }
    $scope.ClosePopUp = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#QTYAllocation')).modal('hide');
    };

    $scope.PIPOAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.PIPackingMaterialPopUpList.length; i++) {
            $scope.PIPackingMaterialPopUpList[i].Active = ChkOrUnchk;
        }

        var gridObj = $("#GridPIPOPOPUP").data("ejGrid");
        gridObj.refreshContent();
    };

    //$scope.Validation = function (x) {

    //    if (x.DistributeQTY > x.POQty) {
    //        x.DistributeQTY = x.POQty;
    //        throw "Distribution quantity is greater than PO quantity.";
    //    }
    //}


    $scope.SavePIPackingListData = function () {
        try {
            var sumQty = 0;
            var SaveList = [];
            for (var i = 0; i < $scope.PIPackingMaterialPopUpList.length; i++) {
                if ($scope.PIPackingMaterialPopUpList[i].Active) {
                    if ($scope.PIPackingMaterialPopUpList[i].DistributeQTY > $scope.PIPackingMaterialPopUpList[i].POQty) {
                        throw "Distribution quantity is greater than PO quantity.";
                    }
                    if (baseService.isUndefinedOrNull($scope.PIPackingMaterialPopUpList[i].DistributeQTY)) {
                        throw "Please enter quantity.";
                    }
                    SaveList.push($scope.PIPackingMaterialPopUpList[i]);
                    sumQty += $scope.PIPackingMaterialPopUpList[i].DistributeQTY;
                }
            }
            if (sumQty > $scope.AllocatedPIQty)
                throw "Packing quantity shlould less than Allocated quantity.";
            $http({
                method: 'POST',
                url: $scope.savePIPackingListUrl,
                data: { 'PIPackingListMasterData': $scope.PIPackingListMasterTemp, 'MaterialData': $scope.PIPackingListMaterialTemp, 'DataList': SaveList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.PIPackingListMasterTemp.Id = response.data.PIPackingListMasterId;
                    $scope.LoadPIPackingList();
                    /*                    $scope.getData();*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };







    $scope.GetAllVersionData = function () {
        //$scope.getHeader(args.data.Id, args.data.PIVersionId);
        $http({
            method: 'GET',
            url: $scope.path + "GetAllVersionData?PIMasterId=" + $scope.PImodelNew.Id,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.SelectedPIVersion = null;
                $scope.DataList = [];
                $scope.DataList.push(Object.assign({}, $scope.PIGridModel));

                $scope.VersionList = $scope.PIVersionModel;
            }

        });

    };

    $scope.selectedDataIndex = -1;
    $scope.OnUOMChange = function (data) {
        $scope.selectedDataIndex = data.model.ModelFieldsId;
        $scope.getUoM();
    }
    // $scope.OnUOMChange();

    $scope.getUoM = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetUoMList?MaterialGroupMasterId=" + $scope.DataList[$scope.selectedDataIndex].MaterialGroupMasterId
        }).then(function successCallback(response) {
            $scope.DataList[$scope.selectedDataIndex].MaterialGroupUOMList = response.data.UOMList;

        });
    }

    $scope.Clear = function () {
        $scope.PImodelNew = Object.assign({}, $scope.PIHeaderModel);
        $scope.DataList = [];
        $scope.DataList.push(Object.assign({}, $scope.PIGridModel));
        $scope.VersionList = [];
        // $scope.VersionList.push(Object.assign({}, $scope.PIVersionModel));

    };
    $scope.Clear();
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + $window.companyId + '&PlantId=' + $window.plantId;

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
    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.hidePartyPopUp();
        $scope.partyType = "Customer";
        $scope.searchParty = '';
    }
    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.PImodelNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.PImodelNew.PaymentTermId; })[0];
            $scope.PImodelNew.PaymentTermDays = paymentTerm.NoOfDay;
        }
    };

    $scope.paymentTermList = [];
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getcustomercbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.PImodelNew.InvoicingState = state;
                $scope.PImodelNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.PImodelNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.PImodelNew.DeliveryState = state;
                $scope.PImodelNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.PImodelNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.PImodelNew.InvoicingState = null;
                $scope.PImodelNew.InvoicingGSTIN = null;
                return $scope.PImodelNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.PImodelNew.DeliveryState = null;
                $scope.PImodelNew.DeliveryGSTIN = null;
                return $scope.PImodelNew.DeliveryByAddress = null;
            }
        }
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.PImodelNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.IsDefault) {
                    $scope.PImodelNew.InvoicingPartyPlantId = item.Value;
                    $scope.PImodelNew.DeliveryPartyPlantId = item.Value;
                    $scope.PImodelNew.InvoicingByAddress = item.Address1;
                    $scope.PImodelNew.DeliveryByAddress = item.Address1;
                    $scope.PImodelNew.InvoicingState = item.StateName;
                    $scope.PImodelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.PImodelNew.DeliveryState = item.StateName;
                    $scope.PImodelNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }
    $scope.departmentList = [];
    $scope.buyerChange = function () {
        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.PImodelNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
    };

    cboService.getCboWithBuyer(null, function (result) {
        $scope.testingStandardList = result;
    });

    $scope.SetCustomerData = function (obj) {

        var party = obj.data;
        $scope.PImodelNew.PartyCode = party.Code;
        $scope.PImodelNew.CustomerName = party.UserName;
        $scope.PImodelNew.PartyId = party.Id;
        $scope.PImodelNew.CurrencyId = party.CurrencyId;
        $scope.PImodelNew.PartyAccountGroupId = party.PartyAccountGroupId;
        $scope.PImodelNew.IsPaymentTermChangeable = '';
        $scope.PImodelNew.PaymentTermId = '';
        $scope.PImodelNew.PaymentTermId = party.PaymentTermId;
        $scope.PImodelNew.IsPaymentTermChangeable = party.IsPaymentTermChangeable;

        $scope.PImodelNew.Customer = party.UserName;
        $scope.PImodelNew.CustomerId = party.Id;

        $scope.changePaymentTerm($scope.PImodelNew.PaymentTermId);
        $scope.personList = [];
        getPartyPlantList();
        // GetDepartmentPersonCbo();
        $scope.hidePartyPopUp();
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }


    $scope.MaterialGroupList = [];
    $scope.GetMaterialGroupList = function () {
        $http({
            method: "GET",
            url: $scope.path + "GetMaterialGroupList",
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {

            }
            else {
                $scope.MaterialGroupList = response.data;
            }
        }, function errorCallback(response) {

        });
    }
    $scope.GetMaterialGroupList();


    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.PImodelNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });
    //$scope.Get = function (args) {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "PIPackingMaterialList?PIPackingMaterId=" + args.data.Id,
    //    }).then(function successCallback(response) {

    //        $scope.PIPackingMaterialGridList = response.data;

    //    });
    ////};
    //$scope.Get = function (args) {
    //    $scope.PIMasterId = args.data.Id;
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "GetAllData?PIMasterId=" + args.data.Id,
    //    }).then(function successCallback(response) {
    //        if (!baseService.isUndefinedOrNull(response.data)) {
    //            $scope.PImodelNew = response.data.PIMaster[0];
    //            //$scope.PIPackingListMasterTemp = response.data.PackingMasterData;
    //            $scope.PIVersionModel = response.data.VarsionData;
    //            $scope.DataList = response.data.ItemData;
    //            /*                $scope.PIVersionModel.Id = $scope.PIVersionModel[0].Id;*/
    //            $scope.VersionList = $scope.PIVersionModel;
    //            $scope.PIVersionModel.VersionNo = args.data.LastVersion;
    //            $scope.ClosePIPopUp();
    //        }
    //    });
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};
}
