"use strict";
customerInterTransactionPendingController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function customerInterTransactionPendingController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $scope.url = 'accounts/Advance';
    $scope.getListUrl = $scope.url + '/GetAvilabeCustomerInterTransactionAdvanceList';
    $scope.sourceType = "CustomerAdvance";
    $scope.searchByList = [
        {
            "name": "#No",
            "value": "AdvanceNo"
        },
        {
            "name": "Party Code",
            "value": "PartyCode"
        },
        {
            "name": "Party Name",
            "value": "PartyName"
        },
        {
            "name": "Ordering Party",
            "value": "PartyPlantName"
        },
        {
            "name": "Posting Date",
            "value": "PostingDate"
        },
        {
            "name": "Doc Date",
            "value": "DocDate"
        },
        {
            "name": "Doc Ref",
            "value": "DocRefNo"
        },
        {
            "name": "Currency",
            "value": "Currency"
        }
    ];

    $rootScope.parameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'PostingDate',
        searchBy: "AdvanceNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function (pageno) {
        baseService.paginationBase($scope.getListUrl, pageno, $scope.parameters)
            .then(function (result) {
                $scope.voucherList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.getInterTransactionList = function (sourcetype) {
        if (sourcetype === 'CustomerAdvance') {
            $scope.getListUrl = $scope.url + '/GetAvilabeCustomerInterTransactionAdvanceList';
            $scope.getData();
        }
        else if (sourcetype === 'CustomerSuspense') {
            $scope.getListUrl = $scope.url + '/GetAvilabeCustomerInterTransactionSuspenseList';
            $scope.getData();
        }
    };
    $scope.getInterTransactionList('CustomerAdvance');
}