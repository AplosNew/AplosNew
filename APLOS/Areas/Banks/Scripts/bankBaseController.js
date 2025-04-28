bankBaseController.$inject = ["$scope", "$http", "baseService"];
function bankBaseController($scope, $http, baseService) {
    $scope.bankList = [];
    $scope.bankIndex = -1;
    $scope.bankSelected = null;
    $scope.bankSearchByList = [
        {
            "name": "Bank",
            "value": "BankName"
        },
        {
            "name": "Bank Branch",
            "value": "BankBranchName"
        },
        {
            "name": "Account Type",
            "value": "BankAccountTypeName"
        },
        {
            "name": "Account Number",
            "value": "AccountNumber"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];

    $scope.bankParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "BankName, BankBranchName, AccountTitle",
        searchBy: "AccountNumber",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    if ($scope.bankACType === "HouseBank") {
        $scope.bankSearchByList.push(
            {
                "name": "GL Code",
                "value": "GLGeneralInfoCode"
            },
            {
                "name": "GL Name",
                "value": "GLGeneralInfoName"
            },
            {
                "name": "Budget Code",
                "value": "BudgetCode"
            },
            {
                "name": "Budget Name",
                "value": "BudgetName"
            },
            {
                "name": "Activity Code",
                "value": "ActivityCode"
            },
            {
                "name": "Activity Name",
                "value": "ActivityName"
            }
        );
    }


    $scope.showBankPopUp = function (entityId) {
        if (entityId === undefined || entityId === "undefined") {
            entityId = null;
        }
        $scope.getBankList = function (pageno) {
            if ($scope.bankACType === "HouseBank") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=HouseBank&&entityId=" + entityId;
            }
            else if ($scope.bankACType === "Loan") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=Loan&&entityId=" + entityId;
            }
            else if ($scope.bankACType === "Investment") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=Investment&&entityId=" + entityId;
            }
            else if ($scope.bankACType === "Security") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=Security&&entityId=" + entityId;
            }
            else {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=HouseBank&&entityId=" + entityId;
            }
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#bankPopUp")).modal("show");
    };

    $scope.showALLBankPopUp = function () {
        $scope.getALLBankList = function (pageno) {
            $scope.url = "Banks/BankMaster/GetAllBankMasterLists";
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getALLBankList();
        angular.element(document.querySelector("#aLLBankPopUp")).modal("show");
    };

    $scope.showBankPaymentPopUp = function () {
        $scope.getBankList = function (pageno) {
            baseService.paginationBase("banks/bankmaster/GetHouseBankBankMasterList", pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#bankPopUp")).modal("show");
    };

    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
    };

    $scope.hideBankPopUp = function () {
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
    };
    $scope.hideALLBankPopUp = function () {
        angular.element(document.querySelector("#aLLBankPopUp")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
    };
}