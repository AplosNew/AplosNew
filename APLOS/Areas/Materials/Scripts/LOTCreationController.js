'use strict';
LOTCreationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$controller'];
function LOTCreationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $controller) {
    $rootScope.title = "LOT Creation";
    $scope.Action = 'Save';
    $scope.partyType = 'Vendor';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.path = 'Materials/LOTCreation/';

    $scope.POList = [];
    $scope.GetPO = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPO",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.POList = response.data;

        });
    }
    $scope.GetPO();

    $scope.ArticleList = [];
    $scope.GetArticle = function (args) {
        $http({
            method: 'POST',
            url: $scope.path + "GetArticle",
            data: { 'poid': args.value },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;            
        });
    }

    $scope.ProductCodeList = []; 
    $scope.GetProductCode = function (articleid) {
        $http({
            method: 'POST',
            url: $scope.path + "GetProductCode",
            data: { 'articleid': articleid },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProductCodeList = response.data;
            
        });
    }

    $scope.ModelTemp = {
        PartyCode: null,
        PartyName: null,
        CustomerId: null,
        POId: null,
        ProductionOrderId: null
    }
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ClearPO = function () {
        ClearPOFields();
        return true;
    }
    function ClearPOFields() {
        $scope.ModelNew = {
            PartyCode: null,
            PartyName: null,
            CustomerId: null
        }
    }

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };


    $scope.productNew = Object.assign({}, $scope.product);
    $scope.partyList = [];


    // CLOSE PARTY POP UP
    $scope.closePartyPopUp = function (x) {
        var party = x.data;

        $scope.ModelNew.PartyCode = party.Code;
        $scope.ModelNew.PartyName = party.UserName;
        $scope.ModelNew.CustomerId = party.Id;
        $scope.hidePartyPopUp();
        $scope.LoadGrid();

    };

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        
        $scope.ProductionOrderList = [];
        $http.get('Materials/LOTCreation/GetProductionOrderDataList')
            .then(
                function successCallback(response) {
                    $scope.ProductionOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };

    $scope.SetPrOData = function ($event) {
        $scope.ModelNew.ProductionOrderId = $event.data.POId;
       
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
        
    }

    $scope.ProcessList = [];
    $scope.GetProcessSet = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProcessSet',
            dataType: 'JSON'
        }).then(function successCallback(res) {
            $scope.ProcessList = res.data;
        })
    }
    $scope.GetProcessSet();

    $scope.ChkdProcessSetList = [];
    $scope.save = function () {
        $scope.ChkdProcessSetList = [];
        for (var i = 0; i < $scopoe.ProcessList.length; i++) {
            if ($scopoe.ProcessList[i].isSelected) {
                $scope.ChkdProcessSetList.push($scopoe.ProcessList[i]);
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + 'Save',
            data: { 'datalist': $scope.ChkdProcessSetList},
            dataType: 'JSON'
        }).then(function successCallback(res) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                

            }
        })
    }
}