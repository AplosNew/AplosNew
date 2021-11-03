'use strict';
dailyTransactionReportController.$inject = ['cboService', '$scope', '$rootScope', '$filter', "baseService", "$http", "$window"];
function dailyTransactionReportController(cboService, $scope, $rootScope, $filter, baseService, $http, $window) {
    $rootScope.title = 'Daily Transaction';
    $scope.entityList = {};
    $scope.report = {


        ReportFormat: 'Pdf',
        FromDate: $filter('dateFiltering')(Date.now())
    };
    $scope.entityId = null;
    cboService.getCboEntityPlantWise(null, null, null, function (result) {
        $scope.entityList = result;
        //$scope.entityList = addToObject($scope.entityList, "Text", "--Select--",0);
        //$scope.entityList = addToObject($scope.entityList, "Value", null,0);


    });

    $scope.getReport = function () {
        var DropDownListEntity = $("#ddlEntityList").data("ejDropDownList");
        $scope.entityId = DropDownListEntity.getSelectedValue();
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_FromDate', true, "Date is required.");
        }     
        if (baseService.isUndefinedOrNull($scope.entityId)) {
            manualValidation('div_Entity', true, "Select Entity.");
        }
        else {
            var url = 'Accounts/Voucher/GetDailyTransactionReport?reportFormat=' + $scope.report.ReportFormat + '&date=' + $scope.report.FromDate + '&entityId=' + $scope.entityId;
            $window.open(url, '_blank');
        }
    };
   
}