'use strict';
function CompanyCoaController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Company Coa";
    $scope.index = -1;
    $scope.companies = [];
    $scope.path = 'Organizations/company/';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.getListUrl = $scope.path + 'getcompanycoalist';
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'Code');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.companies = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchByList = [
        {
            name: "Company Code",
            value: "Code"
        }, {
            name: "Company Name",
            value: "UserName"
        }, {
            name: "Company Coa",
            value: "COA"
        }, {
            name: "Alternative Coa",
            value: "AlternativeCOA"
        }
    ];
    $scope.company = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        LegalName: null,
        UserName: null,
        Image: null,
        AddressLine1: null,
        AddressLine2: null,
        AddressLine3: null,
        PostalCode: null,
        Phone1: null,
        Phone2: null,
        Phone3: null,
        Fax: null,
        Website: null,
        Email: null,
        ContactPersonName: null,
        ContactPersonDesignation: null,
        ContactPersonPhone: null,
        ContactPersonAddress: null,
        ContactPersonEmail: null,
        InchargeName: null,
        InchargePhone: null,
        InchargeAddress: null,
        InchargeEmail: null,
        TINNo: null,
        VATResistrationNo: null,
        BINNo: null,
        FYSDate: null,
        LPSDate: null,
        PPSDate: null,
        EstedDate: null,
        Remarks: null,
        PKPrefixField: null,
        WebDomain: null,
        ManagementGroup: null,
        OrganizationCategoryId: null,
        OrganizationClassId: null,
        IsBuyingSellingApplicable: false,
        IsProfitCenterApplicable: false,
        IsVoucherFromBudget: false,
        IsCostCenterApplicable: false,
        IsBudgetPeriod: false,
        BaseCurrencyId: null,
        CompanyGroupId: null,
        ContinentId: null,
        CountryId: null,
        CityId: null,
        StateId: null,
        AreaId: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        COAId: null,
        AlternativeCOAId: null
    };
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.COAList = [];
    $http({
        method: 'GET',
        url: 'accounts/coa/getcoacbo/'
    }).then(function successCallback(response) {
        $scope.COAList = response.data;
    });

    $scope.AlternateCOAList = [];
    $http({
        method: 'GET',
        url: 'accounts/alternativecoa/getalternativecoalistcbo/'
    }).then(function successCallback(response) {
        $scope.AlternateCOAList = response.data;
    });

    $scope.myupdate = false;
    $scope.Get = function (x, index) {
        $scope.index = index;
        $scope.company = $scope.companies[$scope.index];
        $scope.company.AddedDate = $filter('dateFilter')($scope.company.AddedDate);
        $scope.company.UpdatedDate = $filter('dateFilter')($scope.company.UpdatedDate);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.myupdate = true;
    };

    $scope.Update = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.companyCoaForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'Organizations/company/companycoaedit',
                    data: $scope.company,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.company = response.data.Company;
                        $scope.companies.push($scope.company);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'error');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Update';
        $scope.company = {};
    }
}
CompanyCoaController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];