'use strict';
manpowerBudgetDesignationReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function manpowerBudgetDesignationReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.path = 'HumanResource/ManpowerBudgetDesignationReport/';
    $scope.companyList = [];
    $scope.companyId = window.companyId;
    $scope.cboPlantList = [];
    $scope.plantId = null;
    $scope.report = {
      
       // FromDate: $filter("dateFiltering")(firstDay),
        ToDate: $filter("dateFiltering")(Date.now())
    };

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    //$scope.getPlant = function (args) {
    $scope.companyId = $window.companyId;
        cboService.getCboPlantByCompany($scope.companyId, function (result) {
            $scope.cboPlantList = result;
        });
   // };

    $scope.GetManpowerBudgtDesignationReport = function () {
        try {
            var DropDownJobLocationListObj = $("#ddlPlantList").data("ejDropDownList");
            var plantListsel = "'" + DropDownJobLocationListObj.getSelectedValue().split(",").join("','") + "'";

            var url = $scope.path + 'GetManpowerBudgtDesignationReport?companyId=' + $scope.companyId + '&plantIds=' + plantListsel;
            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetBudgetedDesignationDetail = function () {
        try {
            var DropDownJobLocationListObj = $("#ddlPlantList").data("ejDropDownList");
            var plantListsel = "'" + DropDownJobLocationListObj.getSelectedValue().split(",").join("','") + "'";

            var url = $scope.path + 'GetBudgetedDesignationDetail?plantIds=' + plantListsel + '&date=' + $scope.report.ToDate;
            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
}