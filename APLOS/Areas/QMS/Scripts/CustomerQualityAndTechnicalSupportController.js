'use strict';
CustomerQualityAndTechnicalSupportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function CustomerQualityAndTechnicalSupportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Complaint Master';
    $scope.path = 'QMS/CustomerQualityAndTechnicalSupport/';
    $scope.partyType = 'Vendor';
    $scope.Action = 'Save';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelTemp = {
        Id: null,
        SalesId: null,
        ArticleId: null,
        PartyName: null,
        PartyCode: null,
        PartyId: null
    }
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp)

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
        $scope.ModelNew.PartyId = party.Id;

        $scope.hidePartyPopUp();
        $scope.GetArticle();
    };

    $scope.ArticleList = [];
    $scope.GetArticle = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetArticle",
            data: {
                'salesId': $scope.ModelNew.PartyId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
        })
    }

    $scope.InvoicenumberList = [];
    $scope.GetInvoiceNumber = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInvoiceNumber",
            data: {
                'articleId': $scope.ModelNew.ArticleId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InvoicenumberList = response.data;
        })
    }
}