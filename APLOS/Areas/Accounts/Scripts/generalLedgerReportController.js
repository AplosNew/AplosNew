"use strict";
generalLedgerReportController.$inject = ["$scope", "$rootScope", "$filter", "accountService", "$window", "baseService"];
function generalLedgerReportController($scope, $rootScope, $filter, accountService, $window, baseService) {
    $rootScope.title = "General Ledger";
    $scope.report = {
        GLName: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        ReportFormat: "Pdf",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        Active:true
    };
    $(".searchableDDL").select2();

    $scope.glList = [];
    $scope.getCompanyGLCboList = function () {
        accountService.getCompanyGLCboList(function (result) {
            $scope.glList = result;
        });
    };
    $scope.getCompanyGLCboList();

    $scope.budgetList = [];
    $scope.getBudgetMasterCboList = function (glId) {
        accountService.getBudgetMasterCboList(glId, function (result) {
            $scope.budgetList = result;
        });
    };

    $scope.activityList = [];
    $scope.getBudgetMasterActivityCbo = function (budgetMasterId) {
        accountService.getBudgetMasterActivityCbo(budgetMasterId, function (result) {
            $scope.activityList = result;
        });
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.GLGeneralInfoId)) {
            manualValidation("div_GL", true, "GL is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            var url = "";
                url = "Accounts/Voucher/GetGeneralLedgerReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&glId=" + $scope.report.GLGeneralInfoId + '&active=' + $scope.report.Active;
            //if ($scope.report.Active) {

            //}
            //else {

            //url = "Accounts/Voucher/GetGeneralLedgerReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&glId=" + $scope.report.GLGeneralInfoId;
            //}
            if (!baseService.isUndefinedOrNull($scope.report.BudgetMasterId)) {
                url += "&budgetMasterId=" + $scope.report.BudgetMasterId;
            }
            if (!baseService.isUndefinedOrNull($scope.report.ActivityId)) {
                url += "&activityId=" + $scope.report.ActivityId;
            }
            $window.open(url, "_blank");
        }
    };
    //$scope.getReportWithDocRef = function () {
    //    if (baseService.isUndefinedOrNull($scope.report.GLGeneralInfoId)) {
    //        manualValidation("div_GL", true, "GL is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
    //        manualValidation("div_FromDate", true, "From Date is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
    //        manualValidation("div_ToDate", true, "To Date is required.");
    //    }
    //    else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
    //        manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
    //    }
    //    else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
    //        manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
    //    }
    //    else {
    //        var url = "Accounts/Voucher/GetGeneralLedgerReportWithDocRef?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&glId=" + $scope.report.GLGeneralInfoId  + '&active=' + $scope.report.Active;
    //        if (!baseService.isUndefinedOrNull($scope.report.BudgetMasterId)) {
    //            url += "&budgetMasterId=" + $scope.report.BudgetMasterId;
    //        }
    //        if (!baseService.isUndefinedOrNull($scope.report.ActivityId)) {
    //            url += "&activityId=" + $scope.report.ActivityId;
    //        }
    //        $window.open(url, "_blank");
    //    }
    //};


    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityList";
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.setSelected = function (data) {
        $scope.addRow(data);
        $scope.closeCOAICodeListPopUp();
    };

    $scope.addRow = function (data) {
        $scope.getCompanyGLCboList();
        $scope.getBudgetMasterCboList(data.GLGeneralInfoId);
        $scope.getBudgetMasterActivityCbo(data.BudgetMasterId);
        $scope.report.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.report.BudgetMasterId = data.BudgetMasterId;
        $scope.report.ActivityId = data.ActivityId;
    };
}