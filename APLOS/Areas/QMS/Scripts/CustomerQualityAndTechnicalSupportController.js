'use strict';
CustomerQualityAndTechnicalSupportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function CustomerQualityAndTechnicalSupportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Complaint Master';
    $scope.path = 'QMS/CustomerQualityAndTechnicalSupport/';
    $scope.partyType = 'Vendor';
    $scope.Action = 'Save';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
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
        PartyId: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        ResponsiblePersonCode: null,
        ByWhomId: null,
        ByWhomeCode: null,
        ByWhomeName:null

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

    //#region Responsible Person
   
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.Name = null;
    $scope.employeeList = [];
    $scope.showEmployeeListPopUp = function (name) {
        $scope.employeeList = [];
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.ModelNew.ResponsiblePersonId = data.SystemId;
        $scope.ModelNew.ResponsiblePerson  = data.EmployeeName;
        $scope.ModelNew.ResponsiblePersonCode = data.EmployeeCode;

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    
    //#endregion Responsible Person

    //#region ByWhom
    $scope.showByWhomListPopUp = function (name) {
        $scope.employeeList = [];
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#ByWhomePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectByWhomePopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.ModelNew.ByWhomId = data.SystemId;
        $scope.ModelNew.ByWhomeName = data.EmployeeName;
        $scope.ModelNew.ByWhomeCode = data.EmployeeCode;

        angular.element(document.querySelector('#ByWhomePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideByWhomePopUp = function () {
        angular.element(document.querySelector('#ByWhomePopUps')).modal('hide');
    };
    //#endregion ByWhom
}