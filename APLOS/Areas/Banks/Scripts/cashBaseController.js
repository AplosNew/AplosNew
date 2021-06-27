cashBaseController.$inject = ["$scope", "$http", "baseService"];
function cashBaseController($scope, $http, baseService) {
    $scope.cashList = [];
    $scope.cashIndex = -1;
    $scope.cashSelected = null;
    $scope.cashSearchByList = [
        {
            "name": "Code",
            "value": "Code"
        },
        {
            "name": "User Name",
            "value": "UserName"
        },
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        }
    ];

    $scope.cashParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "Code",
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showCashPopUp = function (index, entityId) {
        $scope.getCashList = function (pageno) {
            baseService.paginationBase("banks/cashmaster/GetCashMasterVoucher?id=&entityId=" + entityId, pageno, $scope.cashParameters)
                .then(function (result) {
                    $scope.cashList = result.Rows;
                    $scope.cashParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getCashList();
        angular.element(document.querySelector("#cashPopUp")).modal("show");
    };

    $scope.showCashPaymentPopUp = function () {
        $scope.getCashList = function (pageno) {
            baseService.paginationBase("banks/cashmaster/GetCashMasterVoucherPayment", pageno, $scope.cashParameters)
                .then(function (result) {
                    $scope.cashList = result.Rows;
                    $scope.cashParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getCashList();
        angular.element(document.querySelector("#cashPopUp")).modal("show");
    };

    $scope.selectCashPopUp = function (index, id) {
        $scope.cashIndex = index;
    };

    $scope.clearJournalType = function () {
        $scope.voucher.OtherCashMasterId = null;
        $scope.voucher.OtherBankMasterId = null;
        $scope.voucherDetailList = [];
    };

    $scope.hideCashPopUp = function () {
        angular.element(document.querySelector("#cashPopUp")).modal("hide");
        $scope.cashIndex = -1;
        $scope.cashSelected = null;
    };
}