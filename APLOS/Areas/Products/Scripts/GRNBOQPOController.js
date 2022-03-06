'use strict';
GRNBOQPOController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function GRNBOQPOController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "GRN BOQ PO"; //Inventory Receive
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/GoodsReceiveNote/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl1 = $scope.path + 'GetListForMasterData';
    $scope.getListUrl2 = $scope.path + 'GetListForMasterData2';

    $scope.saveUrl = $scope.path + 'createGRNBYPO';
    $scope.updateUrl1 = $scope.path + 'UpdateGRNBYPO';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'deleteGRNBYPO/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.updateUrlForSRValue = $scope.path + 'UpdateShortageRejectionValueMap';
    $scope.PurchaseOrderFileLocation = virtualPath.GRN;
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

    /*$controller('partyBaseController', { $scope: $scope, $http: $http });*/
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.showPartyPopUpNew = function () {

        if ($scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Products/GoodsReceiveNote/GetGRNBOQPartyListNew?partyType=' + $scope.partyType;
        }

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        //}
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };
    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
    };
    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    $scope.closeBOQPartyPopUp = function (x) {
        var party = x.data;
        $scope.SubmitPartyCode = party.Code;
        $scope.SubmitPartyName = party.UserName;
        $scope.SubmitPartyId = party.Id;
        $scope.SubmitPaymentTermId = party.PaymentTermId;
        $scope.SubmitCurrencyId = party.CurrencyId;
        getPartyPlantList();
        $scope.hideBOQPartyPopUp();
    };
}