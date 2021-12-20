'use strict';
ProformaInvoiceController.$inject = ['commonMessage', '$controller', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function ProformaInvoiceController(commonMessage, $controller, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Proforma Invoice";
    $scope.Action = 'Save';
    $scope.fabricRollMasters = [];
    $scope.selectedGRNList = [];
    $scope.path = 'Commercial/ProformaInvoice/';
    $scope.CostingPath = 'Costings/costingItem/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.newVersionUrl = $scope.path + 'NewVersion';
    $scope.deleteUrl = $scope.path + 'delete/';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("TaskScheduleController", { cboService: cboService, $scope: $scope, $http: $http });

    // $scope.ExchangeRateTableName = 'MasterOrderExchangeRates';//very important to provide the table where the exchange rates will be saved
    $controller("CurrencyExchangeController", { cboService: cboService, $scope: $scope, $http: $http, TableName: 'MasterOrderExchangeRates' });
    $scope.PIHeaderModel = {
        Id: null
        , PINo: null
        , PIDate: null
        , RefNo: null
        , RevisionNo: null
        , BuyerId: null
        , CustomerId: null
        , Customer: null
        , Currency: null
        , Description: null
        , Quantity: 0
        , UoM: null
        , DeliveryDate: null
        , Amount: 0
       
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryState: null
        , DeliveryGSTIN: null
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

    $scope.PIGridModel = {
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
    $scope.PIVersionModel = {
        Id: null,
        PIMasterId: null,
        VersionNo: null,
        VersionRefNo: null,
        VersionDate: null
    };

    $scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.PIGridModel));


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
        $scope.PIGridModel = {
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

    }

    $scope.PISearchBy = "Id";
    $scope.PISearch = "";
    $scope.PIGridList = [];
    $scope.LoadPISearchList = function () {
        $scope.PIGridList = [];
        try {
            $http({
                method: 'POST',
                url: $scope.path + "PIList",
                data: { 'column': $scope.PISearchBy, 'value': $scope.PISearch },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PIGridList = [];
                $scope.PIGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.LoadPISearchList();
    $scope.VersionList = [];
    $scope.Get = function (args) {
        //$scope.getHeader(args.data.Id, args.data.PIVersionId);
        $http({
            method: 'GET',
            url: $scope.path + "GetAllData?PIMasterId=" + args.data.Id + '&VersionId=' + args.data.PIVersionId,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.PImodelNew = response.data.PIMaster[0];
                $scope.PIVersionModel = response.data.VarsionData;
                $scope.DataList = response.data.ItemData;
                $scope.PIVersionModel.Id = $scope.PIVersionModel[0].Id;
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
                $scope.VersionList = $scope.PIVersionModel;
            }

        });

    };

    //$scope.getHeader = function (PIMasterId, VersionId) {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "GetAllData?PIMasterId=" + PIMasterId + '&VersionId=' + VersionId,
    //    }).then(function successCallback(response) {
    //        $scope.PImodelNew = response.data.PIMaster[0];
    //        $scope.VersionList = response.data.VarsionData;
    //        $scope.PIGridModel = response.data.ItemData;
    //    });
    //}
    // $scope.getHeader();

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
        ClearFields();
       $scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.PIGridModel));
      
        return true;
    };
    function ClearFields() {
        $scope.PImodelNew = Object.assign({}, $scope.PIHeaderModel);

    }

    $rootScope.title = 'Proforma Invoice';
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
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.PImodelNew.BuyerId).then(function (response) {
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
        $scope.PImodelNew.Customer = party.UserName;
        $scope.PImodelNew.CustomerId = party.Id;

        //getPartyPlantList();
        //GetDepartmentPersonCbo();
        /* $scope.hidePartyPopUp();*/
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }
    $scope.MaterialGroupList = [];
    $scope.GetMaterialGroupList = function () {
        $http({
            method: "GET",
            url: $scope.CostingPath + "GetMaterialGroupList",
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

    $scope.Save = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'HeaderData': $scope.PImodelNew, 'MaterialData': $scope.DataList, 'VersionData': $scope.PIVersionModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPISearchList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure')
        }

    };

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.PIGridModel.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });
    //$scope.NewVersion = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "GetNewVersion?PIMasterId=" + $scope.PImodelNew + "&PIVersionId=" + $scope.PIVersionModel.Id + "&PIMaterialId=" + $scope.PIGridModel.Id,
    //    }).then(function successCallback(response) {
         
    //    });
    //}


    $scope.NewVersion = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.newVersionUrl,
                data: { 'PIMasterId': $scope.PImodelNew.Id, 'PIVersionId': $scope.PIVersionModel.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   /* $scope.LoadPISearchList();*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure')
        }

    };
}
