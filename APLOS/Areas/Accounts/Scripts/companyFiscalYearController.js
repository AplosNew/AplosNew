'use strict';
CompanyFiscalYearController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyFiscalYearController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $rootScope.title = "Company Fiscal Year";
    $scope.fiscalYearList = [];
    $scope.companyList = [];
    $scope.path = 'accounts/CompanyFiscalYear/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.companyFiscalYears = result.Rows;
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
            'name': "FiscalYear",
            'value': "FiscalYearName"
        }
    ];

    $http({
        method: 'GET',
        url: 'accounts/fiscalyear/getcbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.companyFiscalYear = {
        Id: null,
        FiscalYearId: null,
        CompanyId: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd')
    };

    $scope.Save = function () {
        $scope.companyFiscalYear.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.companyFiscalYear.UpdatedDate = null;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.companyFiscalYearForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "accounts/CompanyFiscalYear/Create",
                    data: $scope.companyFiscalYear,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        ClearFields();
                    }
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: "accounts/CompanyFiscalYear/Edit",
                    data: $scope.companyFiscalYear,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.companyFiscalYears[$scope.index] = $scope.companyFiscalYear;
                            $scope.getData();
                        }
                        ClearFields();
                    }
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.companyFiscalYear.Id)) {
            $http({
                method: 'POST',
                url: "accounts/CompanyFiscalYear/Delete/" + $scope.companyFiscalYear.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.companyFiscalYears.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $http.get("accounts/CompanyFiscalYear/GetCompanyFiscalYear/" + id)
            .then(function (response) {
                $scope.companyFiscalYear = response.data;
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
        $scope.companyFiscalYear = {};
        $scope.Action = "Save";
        $scope.companyFiscalYear.Active = true;
    }
}