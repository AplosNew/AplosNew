'use strict';
dayBooksReportController.$inject = ['cboService', '$scope', '$rootScope', '$filter', "baseService", "$http", "$window"];
function dayBooksReportController(cboService, $scope, $rootScope, $filter, baseService, $http, $window) {
    $rootScope.title = 'Day Books';
    $scope.entityList = {};
    $scope.report = {


        ReportFormat: 'Pdf',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        DateType: 'PostingDate'
    };
    $scope.entityId = null;
    cboService.getCboEntityPlantWise(null, null, null, function (result) {
        $scope.entityList = result;
        //$scope.entityList = addToObject($scope.entityList, "Text", "--Select--",0);
        //$scope.entityList = addToObject($scope.entityList, "Value", null,0);


    });

    $scope.getReport = function () {
        //var DropDownListEntity = $("#ddlEntityList").data("ejDropDownList");
        //$scope.entityId = DropDownListEntity.getSelectedValue();
        //if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
        //    manualValidation('div_FromDate', true, "Date is required.");
        //}     
        //if (baseService.isUndefinedOrNull($scope.entityId)) {
        //    manualValidation('div_Entity', true, "Select Entity.");
        //}
        //else {
        //    var url = 'Accounts/VoucherReport/GetDayBookReport?reportFormat=' + $scope.report.ReportFormat + '&fromdate=' + $scope.report.FromDate  + '&todate=' + $scope.report.ToDate;
        //$window.open(url, '_blank');

        //}

        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
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
        //else if ($scope.report.DateType) {

        //    var url = 'Accounts/VoucherReport/GetDayBookReport?reportFormat=' + $scope.report.ReportFormat + '&fromdate=' + $scope.report.FromDate + '&todate=' + $scope.report.ToDate;
        //    $window.open(url, '_blank');
        //}

        else {
                //var url = "Accounts/InvoiceTax/GetTaxPayableReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&taxCategoryId=" + $scope.report.TaxCategoryId;
                //$window.open(url, "_blank");

            var url = 'Accounts/VoucherReport/GetDayBookReport?reportFormat=' + $scope.report.ReportFormat + '&fromdate=' + $scope.report.FromDate + '&todate=' + $scope.report.ToDate + '&dateType=' + $scope.report.DateType;
                $window.open(url, '_blank');
             }



    };
   
}