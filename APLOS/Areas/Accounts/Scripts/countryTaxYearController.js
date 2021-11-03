'use strict';
CountryTaxYearController.$inject = ['addressService', 'cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CountryTaxYearController(addressService, cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $rootScope.title = "Company Tax Year";
    $scope.taxYearList = [];
    $scope.countryList = [];
    $scope.path = 'accounts/countryTaxYear/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.countryTaxYears = result.Rows;
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

    cboService.getTaxYearCbo(null, function (result) {
        $scope.taxYearList = result;
    });

    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $scope.countryTaxYear = {
        Id: null,
        TaxYearId: null,
        CountryId: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd')
    };

    $scope.Save = function () {
        $scope.countryTaxYear.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.countryTaxYear.UpdatedDate = null;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.companyTaxYearForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "accounts/countryTaxYear/Create",
                    data: $scope.countryTaxYear,
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
                    url: "accounts/countryTaxYear/Edit",
                    data: $scope.countryTaxYear,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.countryTaxYears[$scope.index] = $scope.countryTaxYear;
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
        if (!baseService.isUndefinedOrNull($scope.countryTaxYear.Id)) {
            $http({
                method: 'POST',
                url: "accounts/countryTaxYear/Delete/" + $scope.countryTaxYear.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.countryTaxYears.splice($scope.index, 1);
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
        $http.get("accounts/countryTaxYear/GetCountryTaxYear/" + id)
            .then(function (response) {
                $scope.countryTaxYear = response.data;
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
        $scope.countryTaxYear = {};
        $scope.Action = "Save";
        $scope.countryTaxYear.Active = true;
    }
}