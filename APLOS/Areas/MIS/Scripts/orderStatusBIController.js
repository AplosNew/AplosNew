'use strict';
orderStatusBIController.$inject = ['commonMessage', '$scope', '$rootScope', 'cboService', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function orderStatusBIController(commonMessage, $scope, $rootScope, cboService, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Order Status";

    $scope.comInfo = {};
    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
        $scope.comInfo.CompanyId = $scope.companyList[0].CompanyId;
        $scope.comInfo.CompanyName = $scope.companyList[0].CompanyName;
    });
    //$scope.srcLink = '<iframe title="Order Status" width="1140" height="541" src="https://app.powerbi.com/reportEmbed?reportId=a0f61718-d361-4bb8-a301-57d406e686a5&autoAuth=true&ctid=504c6c58-7e71-4be5-b5c3-dfe79f84ba5b" frameborder="0" allowFullScreen="true"></iframe>';
    //periodHtml += "<tr><td></td><td></td><td style='text-align:right;font-weight: bold'>" + totalPayment.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalProfit.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalPrincipal.toFixed(2) + "</td><td></tr></table></div>";
    //$("#loanDetails").append(periodHtml);

}