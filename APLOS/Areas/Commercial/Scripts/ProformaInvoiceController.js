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
        , VersionList: null
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
        , MaterialGroupMasterId: null
        , Description: null
        , Quantity: 0
        , UoMId: null
        , UoM: null
        , DeliveryDate: null
        , Currency: null
        , Amount: 0
    };


    $scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.PIGridModel));


    $scope.SubmitH = function (data) {
        try {
            var newObj = Object.assign({}, $scope.PIGridModel);
            if (data != null) {
                newObj = {
                    Id: null
                    , MaterialGroupMasterId: null
                    , Description: null
                    , Quantity: 0
                    , UoMId: null
                    , UoM: null
                    , DeliveryDate: null
                    , Currency: null
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
            , MaterialGroupMasterId: null
            , Description: null
            , Quantity: 0
            , UoMId: null
            , UoM: null
            , DeliveryDate: null
            , Currency: null
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

    $scope.Get = function (args) {
        $scope.getHeader(args.Id);
        $scope.PIModel = Object.assign({}, args.data);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        // $scope.DataList.push(Object.assign({}, $scope.PImodelNew));
    };
    $scope.VersionList = [];
    $scope.getHeader = function (PIMasterId, VersionId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetAllData?PIMasterId=" + PIMasterId + '&VersionId=' + VersionId,
        }).then(function successCallback(response) {
            $scope.PImodelNew = response.data.PIMaster;
            $scope.VersionList = response.data.VarsionData;
        });
    }
    $scope.getHeader();

    $scope.selectedData = {};
    $scope.OnUOMChange = function (data) {
        $scope.selectedData = data;
        $scope.getUoM();
    }

    $scope.getUoM = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetUoMList?MaterialGroupMasterId=" + $scope.selectedData.MaterialGroupMasterId,
        }).then(function successCallback(response) {
            $scope.selectedData.MaterialGroupUOMList = response.data;
        });
    }
    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields(seq) {
        $scope.fabricRollMaster = {};
        $scope.fabricRollMasterNew = {};
        $scope.fabricRollMasterHeadList = [];
        $scope.popUpList = [];
        $scope.valueData = [];
    }


    $rootScope.title = 'Proforma Invoice';

    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetMaster",
        }).then(function successCallback(response) {
            $scope.MasterList = response.data;
            //for (var i = 0; i < response.data.length; i++) {
            //}
            //$scope.MasterList = $filter('dateFiltering')(response.data.AddedDate, 'dd-MMM-yyyy');
        });
    }
    $scope.getMaster();
    //EndFile Upload

    //Import File


    function GetShortList(list) {
        var list2 = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === null || list[i].EmployeeCode === '' || list[i].EmployeeCode === 'undefined') {

            }
            else {
                list2.push(list[i]);
            }
        }
        return list2;
    }
    $scope.buyerNew = {
        FileName: null
    }
    $scope.ImportData = function () {
        try {
            $scope.msg = "";
            //$scope.btnProcess = true;
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.buyerNewForm.$valid) {
                var RollData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.RollData)) {
                    $scope.buyerNew.FileName = $scope.RollData.name;
                }
                $http({
                    method: 'POST',
                    url: 'Materials/FabricRoll/ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        RollData.append("buyerNew", angular.toJson(data.buyerNew));
                        if (baseService.isUndefinedOrNull($scope.RollData) === false) {
                            RollData.append('file', data.file);
                        }
                        return RollData;
                    },
                    data: { 'buyerNew': $scope.buyerNew, 'file': $scope.RollData }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        //$scope.AttdnManualData = response.data;

                        $scope.A = [];
                        var x = GetShortList(response.data);
                        $scope.A = x;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    //End Import File
    //CustomerPOPup
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
        //if (baseService.isUndefinedOrNull($window.CompanyId)) {
        //    ShowResult('Select Company', 'failure');
        //    return false;
        //}
        //if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) {
        //    ShowResult('Select Plant', 'failure');
        //    return false;
        //}


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
        //cboService.getBuyerDivisionCboByBuyer($scope.PImodelNew.BuyerId, function (result) {
        //    $scope.divisionList = result;
        //    if ($scope.divisionList.length == 1) {
        //        $scope.PImodelNew.BuyerDivisionId = $scope.divisionList[0].Value;
        //    }

        //});
        //cboService.getBuyerDepartmentCboByBuyer($scope.PImodelNew.BuyerId, function (result) {
        //    $scope.departmentList = result;
        //    if ($scope.departmentList.length == 1) {
        //        $scope.PImodelNew.BuyerDepartmentId = $scope.departmentList[0].Value;
        //    }

        //});
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
}
