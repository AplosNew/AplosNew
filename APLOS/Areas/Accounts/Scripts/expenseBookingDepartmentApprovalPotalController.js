"use strict";
expenseBookingDepartmentApprovalPotalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller","$window"];
function expenseBookingDepartmentApprovalPotalController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = "Expense Booking Approval";
    $scope.Action = "Save";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.bookingApprovalList = [];
    $scope.approvalStatusList = [];
    $scope.CurrencyList = [];
    $scope.path = "accounts/expenseBooking/";
    $scope.saveUrl = $scope.path + "ExpenseBookingApprovalPotal";
    $scope.holdUrl = $scope.path + "ExpenseBookingHold";
    $scope.rejectUrl = $scope.path + "ExpenseBookingReject";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.HActionDisable = false;
    $scope.RActionDisable = false;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    cboService.getEnumCbo("enum/GetExpensesBookingApprovalStatusCbo", function (result) {
        $scope.approvalStatusList = result;
    });

   

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
        }];

    $scope.budgetCategoryList = [];
    cboService.getBudgetCategoryCbo(function (result) {
        $scope.budgetCategoryList = result;
    });

    $scope.budgetList = [];
    cboService.getCboEmployeeBudgetList("", function (result) {
        $scope.budgetList = result;
    });

    $scope.activityList = [];
    $scope.getCboEmployeeBudgetActivityList = function (budgetId) {
        cboService.getBudgetMasterActivityCbo(budgetId, function (result) {
            $scope.activityList = result;
            if ($scope.activityList.length === 1) {
                $scope.budgetTransactionDetail.ActivityId = $scope.activityList[0].ActivityId;
            }
        });
    };

    $scope.phoneList = [];
    $scope.getCboActivityPhoneByEmployeeActivity = function (budgetId, activityId) {
        cboService.getCboActivityPhoneByEmployeeActivity("", budgetId, activityId, function (result) {
            $scope.phoneList = result;
        });
    };

    $scope.getCboFALinkedList = function (activityId) {
        var activity = $.grep($scope.activityList, function (item) {
            return item.Id === activityId;
        })[0];
        $scope.FALinked = activity.FALinked;
        cboService.getEnumCbo("Accounts/BudgetMaster/GetFALinkedList?budgetMasterId=" + $scope.selectedBudgetMasterId + "&activityId=" + activityId + "&faLinked=" + activity.FALinked, function (result) {
            if ($scope.FALinked === "Master") {
                $scope.faRegisterList = [];
                $scope.faMasterList = result;
            }
            else if ($scope.FALinked === "Register") {
                $scope.faMasterList = [];
                $scope.faRegisterList = result;
            }
            else {
                $scope.faMasterList = [];
                $scope.faRegisterList = [];
                $scope.FALinked = null;
            }
        });
    };

    $scope.selectedBudgetId = null;
    $scope.selectedBudgetCodeName = null;
    $scope.selectedbudgetId = function (selected) {
        if (selected) {
            $scope.selectedBudgetId = selected.originalObject.BudgetId;
            $scope.selectedBudgetCodeName = selected.originalObject.BudgetCodeName;
            $scope.selectedBudgetMasterId = selected.originalObject.Id;
            cboService.getCboEmployeeBudgetActivityList(" ", selected.originalObject.Id, function (result) {
                $scope.activityList = result;
            });
        }
    };

    $scope.budgetTransactionMaster = {
        Id: null,
        EmployeeId: null,
        EntityId: null,
        PlantId: null,
        InvoiceNumber: null,
        InvoiceDate: $filter("dateFiltering")(Date.now()),
        ExpenseDate: $filter("dateFiltering")(Date.now()),
        Active: true,
        CurrencyId: null,
        Status: "ToBeApproved"
    };

    $scope.budgetTransactionDetail = {
        Id: null,
        ExpenseBookingId: null,
        PartyId: null,
        BudgetId: null,
        ActivityId: null,
        ActivityPhoneId: null,
        Amount: 0.00,
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        FixedAssetMasterId: null,
        FixedAssetRegisterId: null,
        BudgetCategory: null,
        BudgetSubCategory: null,
        BudgetName: null,
        BudgetGroup: null,
        GLGeneralInfoCode: null,
        GL: null
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.budgetTransactionMaster.EmployeeName = employee.EmployeeName;
            $scope.budgetTransactionMaster.EmployeeId = employee.SystemId;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboWithEmployee(null, null, function (result) {
                $scope.entityEmployeeList = result;
                if ($scope.entityEmployeeList.length > 0) {
                    $scope.budgetTransactionMaster.EntityId = $scope.entityEmployeeList[0].Value;
                }
            });
    });

    // #region Activity
    $scope.budgetTransactionDetailList = [];
    function checkNullValue() {
        try {
            if ($scope.budgetTransactionDetail.DocDate === null || $scope.budgetTransactionDetail.DocDate === "") {
                throw "Please input ExpenseDate.";
            } else if ($scope.budgetTransactionDetail.BudgetId === null || $scope.budgetTransactionDetail.BudgetId === "") {
                throw "Please input Budget.";
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.GetVoucherDetailrow = function (data, index) {
        $scope.indexdetails = index;
        data.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.budgetTransactionDetail = data;
        $scope.getCboEmployeeBudgetActivityList($scope.budgetTransactionDetail.ActivityId);
        $scope.getCboActivityPhoneByEmployeeActivity($scope.budgetTransactionDetail.ActivityId);
        $scope.CAction = "Approve";
    };

    $scope.GetBudgetTransactionDetail = function (id) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetExpensesBookingDetail?expenseBookingId=" + id
        }).then(function successCallback(response) {
            $scope.budgetTransactionDetailList = response.data;
        });
    };

    $scope.costCenterCboList = [];
    $scope.GetCboCostCenterIdByEntity = function (entityId) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetCboCostCenterIdByEntity?entityId=" + entityId
        }).then(function successCallback(response) {
            $scope.costCenterCboList = response.data;

        });
    };

    $scope.Get = function (data) {
        $scope.budgetTransactionMaster = data.data;
        $scope.GetCboCostCenterIdByEntity($scope.budgetTransactionMaster.EntityId);
        $scope.GetBudgetTransactionDetail($scope.budgetTransactionMaster.Id);
        $scope.budgetTransactionMaster.AddedDate = $filter("dateFiltering")($scope.budgetTransactionMaster.AddedDate);
        $scope.budgetTransactionMaster.UpdatedDate = $filter("dateFiltering")($scope.budgetTransactionMaster.UpdatedDate);
        $scope.budgetTransactionMaster.InvoiceDate = $filter("dateFiltering")($scope.budgetTransactionMaster.InvoiceDate);
        $scope.budgetTransactionMaster.EmployeeName = $scope.budgetTransactionMaster.EmployeeName;
        $scope.budgetTransactionMaster.EmployeeId = $scope.budgetTransactionMaster.EmployeeId;
        $scope.budgetTransactionMaster.ApprovalStatus = $scope.budgetTransactionMaster.ApprovalStatus;
        $scope.Action = "Approve";
        $scope.HAction = "Hold";
        $scope.RAction = "Reject";
        if ($scope.budgetTransactionMaster.ApprovalStatus === "Hold")
            $scope.HActionDisable = true;
        if ($scope.budgetTransactionMaster.ApprovalStatus === "Reject")
            $scope.RActionDisable = true;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetTransactionMasterForm.$valid) {
            try {
                if ($scope.budgetTransactionDetailList.length < 1) {
                    throw "Please add at least one TransactionDetail. ";
                }
                if ($scope.Action === "Approve") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: {
                            "expenseBooking": $scope.budgetTransactionMaster,
                            "expenseBookingDetails": $scope.budgetTransactionDetailList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.GetExBookingApproval();
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

    $scope.Hold = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetTransactionMasterForm.$valid) {
            try {
                if ($scope.budgetTransactionDetailList.length < 1) {
                    throw "Please add at least one TransactionDetail. ";
                }
                if ($scope.HAction === "Hold") {
                    $http({
                        method: "POST",
                        url: $scope.holdUrl,
                        data: {
                            "expenseBooking": $scope.budgetTransactionMaster,
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

    $scope.Reject = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.budgetTransactionMasterForm.$valid) {
            try {
                if ($scope.budgetTransactionDetailList.length < 1) {
                    throw "Please add at least one TransactionDetail. ";
                }
                if ($scope.RAction === "Reject") {
                    $http({
                        method: "POST",
                        url: $scope.rejectUrl,
                        data: {
                            "expenseBooking": $scope.budgetTransactionMaster,
                            "expenseBookingDetailList": $scope.budgetTransactionDetailList
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.GetExBookingApproval();
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

    $scope.budgetTransactionMaster.CurrencyId = $scope.selectBaseCurrency();

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.budgetTransactionMaster = {};
        $scope.budgetTransactionDetailList = [];
        $scope.budgetTransactionMaster.Active = true;
        $scope.HActionDisable = false;
        $scope.RActionDisable = false;
        $scope.budgetTransactionMaster.Status = $scope.approvalStatusList[0].Value;
    }

	$scope.GetExBookingApproval = function () {
		debugger;
        $scope.bookingApprovalList = [];
        $http({
            method: 'GET',
            url: $scope.path + "GetListForDepartmentApproval?approvalStatus=" + 'ToBeApproved'
        }).then(function successCallback(response) {
            $scope.bookingApprovalList = response.data;
        });
    }
    $scope.GetExBookingApproval();

    $scope.bookingApprovedList = [];
    $scope.GetExBookingApproved = function () {
        $scope.bookingApprovedList = [];
        $http({
            method: 'GET',
            url: $scope.path + "GetListForDepartmentApproval?approvalStatus=" + 'Approved'
        }).then(function successCallback(response) {
            $scope.bookingApprovedList = response.data;
        });
    }
    
    $scope.bookingApprovedHoldRejectList = [];
    $scope.GetExBookingApprovedHoldReject = function () {
        $scope.bookingApprovedList = [];
        $http({
            method: 'GET',
            url: $scope.path + "GetListForDepartmentApprovedHoldReject"
        }).then(function successCallback(response) {
            $scope.bookingApprovedHoldRejectList = response.data;
        });
    }
    $scope.tab = 1;
    $scope.setTabApprovalList = function (newTab) {
        $scope.tab = newTab;
		$scope.GetExBookingApproval();

    };
    $scope.isSetApprovalList = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabApproved = function (newTab) {
        $scope.tab = newTab;
        $scope.GetExBookingApproved();
    };
    $scope.isSetApproved = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTabApprovedHold = function (newTab) {
        $scope.tab = newTab;
        $scope.GetExBookingApprovedHoldReject();
    };
    $scope.isSetApprovedHold = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.dwonloadUrl = null;
    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
        $window.open($scope.dwonloadUrl, '_blank');
    };
    $scope.onClickPdfPrintChecked = function (args) {
        var gridObj = $("#BookingGridId2").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.PdfPrintChecked = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintChecked
        }
    }];

    $scope.onClickExcelPrintChecked = function (args) {
        var gridObj = $("#BookingGridId2").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrintChecked = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintChecked
        }
    }];
}