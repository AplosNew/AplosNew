"use strict";
vendorPaymentApprovalController.$inject = ["bankService", "accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller","$window"];
function vendorPaymentApprovalController(bankService, accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = "Payment";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherList = [];
    baseService.init("Accounts/Invoice/GetVendorPaymentParkedNonPostedList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.url = "Accounts/Invoice";
    $scope.ApproveUrl = $scope.url + "/ApproveVendorPayment";

    $scope.searchVendorInvoiceList = [
        {
            "Text": "Voucher No",
            "Value": "VoucherNo"
        },
        {
            "Text": "Vendor/Party",
            "Value": "PartyName"
        },
        {
            "Text": "Posting Date",
            "Value": "PostingDate"
        },
        {
            "Text": "Multiple Payment No",
            "Value": "MultiplePaymentNo"
        }
        ,
        {
            "Text": "Currency Code",
            "Value": "CurrencyCode"
        },
        {
            "Text": "Status",
            "Value": "Status"
        },
        {
            "Text": "Amount",
            "Value": "Amount"
        }
    ];

    $scope.parameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function (pageno) {
        baseService.pagination(pageno, $scope.parameters)
            .then(function (result) {
                $scope.paymentList = result.Rows;
                $scope.parameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.report = function (voucherId) {
        location.href = "accounts/invoice/VendorInvoicePaymentReport?voucherId=" + voucherId;
    };

    $scope.ApprovalStatusList = [];
    cboService.getEnumCbo("enum/GetApprovalStatusCbo", function (result) {
        $scope.ApprovalStatusList = result;
    });

    $scope.invoiceWriteOffId = null;
    $scope.confirmPost = function (data) {
        $scope.invoiceWriteOff = Object.assign({}, data);

        $scope.message_confirmation = "Are you sure to Save?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.Approve = function () {
        try {
            if ($scope.invoiceWriteOff.ApprovalStatus == "Rejected" || $scope.invoiceWriteOff.ApprovalStatus == "Hold") {
                if (baseService.isUndefinedOrNull($scope.invoiceWriteOff.ApproveRemark)) {
                    throw "Remark is required.";
                }
            }
            $http({
                method: "POST",
                url: $scope.ApproveUrl,
                data: {
                    "invoiceWriteOff": $scope.invoiceWriteOff
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

}