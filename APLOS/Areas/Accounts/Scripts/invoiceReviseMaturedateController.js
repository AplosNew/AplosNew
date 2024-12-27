'use strict';
invoiceReviseMaturedateController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function invoiceReviseMaturedateController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'Accounts/Invoice/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.getinvoiceListUrl = $scope.path + 'GetInvoiceReviseMatureDateList';
    $scope.FormTitle = 'Update Invoice Revise MaturedDate';
    $scope.Action = 'Update';
    $scope.paymentMode = null;
    $scope.sheetType = false;
    $scope.cboSalaryProcessIdList = [];
    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;
    $scope.isActive = true;
    $scope.isSeperated = false;
    $scope.isMaternity = false;
    $scope.isManualFilter = false;
    $scope.empGrid = false;
    
    $scope.invoice = {
        Id: null,
        Remarks: null,
        PaymentMode: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        UpdatedReviseDate: $filter("dateFiltering")(Date.now())
    };

    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();
    $scope.payGroupListSelected = [];
    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmpNetPayment = [];

    function daysInMonth(month, year) {
        return new Date(year, month, 0).getDate();
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.ModelNew = { FileName: null };
 
 
    $scope.saveBtnDisable = false;
    $scope.UpdateInvoiceReviseDate = function () {
        try {
            $scope.InvoiceCheckedDataList = [];
            if ($scope.invoice.PartyType == null) {
                throw "Please Select PartyType.";
            }

            for (var i = 0; i < $scope.InvoiceDataList.length; i++) {
                if ($scope.InvoiceDataList[i].CheckBoxSelect == true) { $scope.InvoiceCheckedDataList.push($scope.InvoiceDataList[i]); }
            }

            $scope.$broadcast('show-errors-check-validity');
            $scope.saveBtnDisable = true;
            $http({
                method: 'POST',
                url: 'Accounts/Invoice/UpdateInvoiceReviseDate',
                data: { 'reviseDate': $scope.invoice.UpdatedReviseDate, 'invoiceList': $scope.InvoiceCheckedDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.saveBtnDisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.invoice.Remarks = null;

                    $scope.GetEmployeeInformation();

                    var gridObj = $("#empInfoGrid").data("ejGrid");
                    gridObj.refreshContent();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.InvoiceDataList = [];
    $scope.GetInvoiceData = function () {
        $scope.InvoiceDataList = [];
        if (angular.isUndefinedOrNull($scope.invoice.PartyType)) {
            ShowResult("Select PartyType", 'failure');
        }
        else if (baseService.isUndefinedOrNull($scope.invoice.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.invoice.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.invoice.FromDate) > new Date($scope.invoice.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.invoice.ToDate) < new Date($scope.invoice.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            if ($scope.invoice.FromDate != null && $scope.invoice.FromDate != null) { $scope.invoice.DateRange = true; } else { $scope.invoice.DateRange = false; }
            var parameters = { 'fromDate': $scope.invoice.FromDate, 'toDate': $scope.invoice.ToDate, 'partyType': $scope.invoice.PartyType, 'DateRange': $scope.invoice.DateRange };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Accounts/Invoice/GetInvoiceReviseMatureDateList',
                data: parameters
            }).then(function successCallback(response) {
                $scope.InvoiceDataList = response.data ;
                var gridObj = $("#InvoiceGrid").data("ejGrid");
                gridObj.windowonresize();
                gridObj.refreshContent(true);
            });
        }
    };

   
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

   
    $scope.refreshTemplateInvoiceList = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectInvoice });
    };

    function CheckBoxSelectInvoice(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#InvoiceGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.InvoiceDataList.length; i++) {
                $scope.InvoiceDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#InvoiceGrid").data("ejGrid");
        gridObj.refreshContent();
    };

}