"use strict";
expenseBookingApprovalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function expenseBookingApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Expense Booking Approval";
    $scope.Action = "Save";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.budgetTransactionMasters = [];
    $scope.CurrencyList = [];
    $scope.path = "accounts/expenseBooking/";
    $scope.getListUrl = $scope.path + "GetExpenseBookingPendingList";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    baseService.init($scope.getListUrl, null, null, "DESC", "InvoiceDate DESC, InvoiceNumber", "InvoiceNumber");
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
            name: "Transaction Id#",
            value: "InvoiceNumber"
        },
        {
            "name": "Employee Code",
            "value": "EmployeeCode"
        },
        {
            "name": "Employee Name",
            "value": "EmployeeName"
        },
        {
            "name": "Approver Code",
            "value": "ApproverCode"
        }
            ,
        {
            "name": "Approver By",
            "value": "ApprovedBy"
        }];

    $scope.budgetList = [];
    cboService.getCboEmployeeBudgetList("", function (result) {
        $scope.budgetList = result;
    });

    $scope.activityList = [];
    $scope.getCboEmployeeBudgetActivityList = function (budgetId) {
        cboService.getCboEmployeeBudgetActivityList(" ", budgetId, function (result) {
            $scope.activityList = result;
        });
    };
    $scope.partyList = [];
    $scope.budgetTransactionMaster = {
        Id: null,
        EmployeeId: null,
        InvoiceNumber: null,
        InvoiceDate: new Date(),
        Active: true
    };

    $scope.budgetTransactionDetail = {
        Id: null,
        BudgetTransactionMasterId: null,
        BudgetId: null,
        PartyId: null,
        ActivityId: null,
        ActivityPhoneId: null,
        Amount: 0.00,
        ExpenseDate: new Date()
    };

    $scope.GetVoucherDetailrow = function (data, index) {
        $scope.indexdetails = index;
        data.ExpenseDate = $filter("dateFiltering")(data.ExpenseDate);
        $scope.budgetTransactionDetail = data;
        $scope.getCboBudgetByEmployeeActivity($scope.budgetTransactionDetail.ActivityId);
        $scope.getCboActivityPhoneByEmployeeActivity($scope.budgetTransactionDetail.ActivityId);
        $scope.CAction = "Update";
    };

    $scope.valuePassInDelModal = function (x, index) {
        $scope.id = x.Id;
        $scope.dindex = index;
        $scope.message_confirmation = "Are you sure want to delete this data....";
        angular.element(document.querySelector("#confirmgenericPopUp")).modal("show");
    };

    $scope.removeRow = function () {
        $scope.budgetTransactionDetailList.splice($scope.dindex, 1);
    };

    $scope.GetBudgetTransactionDetail = function (id) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/getbudgettransactiondetail?budgetTransactionMasterId=" + id
        }).then(function successCallback(response) {
            $scope.budgetTransactionDetailList = response.data.Rows;
        });
    };

    $scope.Get = function (data) {
        $scope.budgetTransactionMaster = data;
        $scope.GetBudgetTransactionDetail($scope.budgetTransactionMaster.Id);
        $scope.budgetTransactionMaster.AddedDate = $filter("dateFiltering")($scope.budgetTransactionMaster.AddedDate);
        $scope.budgetTransactionMaster.UpdatedDate = $filter("dateFiltering")($scope.budgetTransactionMaster.UpdatedDate);
        $scope.budgetTransactionMaster.InvoiceDate = $filter("dateFiltering")($scope.budgetTransactionMaster.InvoiceDate);
        $scope.Action = "Update";
    };
    $scope.validation = function () {

        if (new Date($scope.voucher.PostingDate) > new Date()) {
            ShowResult("Posting Date must be below or equal to current Date!", "failure");
            return true;
        }
        return false;
    };
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetTransactionMasterForm.$valid && !$scope.validation()) {
            try {
                if ($scope.budgetTransactionDetailList.length < 1) {
                    throw "Please add at least one TransactionDetail. ";
                }
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "budgetTransactionMaster": $scope.budgetTransactionMaster,
                            "expenseBookingDetailList": $scope.budgetTransactionDetailList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: $scope.updateUrl,
                        data: {
                            "budgetTransactionMaster": $scope.budgetTransactionMaster,
                            "expenseBookingDetailList": $scope.budgetTransactionDetailList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            if ($scope.index > -1) {
                                $scope.budgetTransactionMasters[$scope.index] = $scope.budgetTransactionMaster;
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            } catch (e) {
                throw ShowResult(e, "failure");
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetTransactionMaster.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.budgetTransactionMaster.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetTransactionMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.budgetTransactionMaster = {};
        $scope.budgetTransactionDetailList = [];
        $scope.budgetTransactionMaster.Active = true;
    }


    //ExpenseBookingApprovalList


    $scope.ExpenseBookingApprovalList = function () {

      
        try {
            var file_src = $scope.path + "ExpenseBookingApprovalList";
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
    };
}