"use strict";
CapitalizeAssetRegisterPostingController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function CapitalizeAssetRegisterPostingController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Capitalize Asset Register Posting";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";

    $scope.modelNew = {
        Id: null, FixedAssetItemId: null, CapitalizationDate: null, Qty: 0, GRNAmount: 0, IssueAmount: 0, ExpensesAmount: 0, Other: null, TotalAmount: 0, ApprovedById: null, IsApproved: false, Status: null, Type: null, VoucherRowId: null, Remark: null, InstallationYear: null, Lifetime: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };

    $scope.masterList = [];
    $scope.getMasterData = function () {
        $scope.purchaseLCList = [];
        $http.get("fixedassets/fixedassetregister/GetApprovedCapitalizeData")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#CapitalpopUp')).modal('show');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#CapitalpopUp')).modal('hide');
    }

    $scope.selectedmaterialMasterList = [];
    $scope.GetCapitalizationMasterDetail = function () {
        $scope.purchaseLCList = [];
        $http.get("fixedassets/fixedassetregister/GetCapitalizationMasterDetail?masterId=" + $scope.register.Id)
            .then(
                function successCallback(response) {
                    $scope.selectedmaterialMasterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
   
    $scope.SelectMaster = function (obj) {
        $scope.register = obj.data;
        $scope.register.InstallationYear = parseInt($scope.register.InstallationYear);
        $scope.register.Lifetime = parseInt($scope.register.Lifetime);
        $scope.GetCapitalizationMasterDetail();
        
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = 'Update';
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

}