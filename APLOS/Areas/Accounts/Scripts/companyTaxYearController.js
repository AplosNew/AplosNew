'use strict';
CompanyTaxYearController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyTaxYearController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $rootScope.title = "Company Tax Year";
    $scope.taxYearList = [];
    $scope.path = 'accounts/CompanyTaxYear/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, 'DESC', 'StartDate', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.companyTaxYears = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [
        {
            'name': "Company Name",
            'value': "UserName"
        },
        {
            'name': "TaxYear",
            'value': "TaxYearName"
        }
    ];

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    cboService.getTaxYearCbo(null, function (result) {
        $scope.taxYearList = result;
    });

    $scope.companyTaxYear = {
        Id: null,
        TaxYearId: null,
        CompanyId: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null
    };

    $scope.Save = function () {
        $scope.companyTaxYear.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.companyTaxYear.UpdatedDate = null;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.companyTaxYearForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "accounts/CompanyTaxYear/Create",
                    data: $scope.companyTaxYear,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        baseService.paginationAdd();
                        ClearFields();
                    }
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: "accounts/CompanyTaxYear/Edit",
                    data: $scope.companyTaxYear,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.companyTaxYears[$scope.index] = $scope.companyTaxYear;
                            $scope.getData();
                        }
                        ClearFields();
                    }
                });
                return true;
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.companyTaxYear.Id)) {
            $http({
                method: 'POST',
                url: "accounts/CompanyTaxYear/Delete/" + $scope.companyTaxYear.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.companyTaxYears.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            },
                function (response) {
                    ShowResult(response.data.Message, 'failure');
                });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $http.get("accounts/companytaxyear/getcompanytaxyear/" + id)
            .then(function (response) {
                $scope.companyTaxYear = response.data;
            });
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.companyTaxYear = {};
        $scope.Action = "Save";
        $scope.companyTaxYear.Active = true;
    }
}