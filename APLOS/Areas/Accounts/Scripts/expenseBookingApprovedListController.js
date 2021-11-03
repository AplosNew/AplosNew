"use strict";
expenseBookingApprovedListController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function expenseBookingApprovedListController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Expense Booking Approved List";
    $scope.Action = "Save";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.budgetTransactionMasters = [];
    $scope.CurrencyList = [];
    $scope.path = "accounts/expenseBooking/";
    $scope.getListUrl = $scope.path + "GetExpenseBookingApprovedList";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = "accounts/EmployeePayable/DeleteEmployeePayable";

    $scope.reportUrl = $scope.path + '/ReportEmployeePayable?voucherId=';
    baseService.init($scope.getListUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.budgetTransactionMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList =
        [{
            name: "Invoice Number",
            value: "InvoiceNumber"
        },
        {
            name: "EmployeeCode",
            value: "EmployeeCode"
        },
        {
            name: "EmployeeName",
            value: "EmployeeName"
        },
        {
            name: "Beneficiary",
            value: "BeneficiaryType"
        },
        {
            name: "Voucher No",
            value: "VoucherNo"
        },
        {
            name: "Posting Date",
            value: "PostingDate"
        },
        {
            name: "Invoice Date",
            value: "InvoiceDate"
        },
        {
            name: "ApprovedBy",
            value: "ApprovedBy"
        },
        {
            name: "Currency",
            value: "CurrencyCode"
        },
        {
            name: "Amount",
            value: "Amount"
        }];

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
    };

    $scope.delete = function (employeeBookingId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "employeeBookingId": employeeBookingId, "voucherId": voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
                $scope.employeeBookingId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.employeeBookingId = null;
    $scope.confirmDelete = function (employeeBookingId, voucherId) {
        $scope.employeeBookingId = employeeBookingId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
}