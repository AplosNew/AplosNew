'use strict';
bonusProvisionReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function bonusProvisionReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.withBonusValue = false;

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.BonusRegister = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        //EmployeeId: null,
        ReportFormat: 'Excel',
        //chkAdditionInfo: false
    };
    $scope.yearId = null;
    $scope.taxYearList = [];
    cboService.getTaxYearCbo(null, function (result) {
        $scope.taxYearList = result;
    });
    $scope.GetBonusRegister = function (reportType) {
        try {
       
            if (baseService.isUndefinedOrNull($scope.fromDate)) {
                //manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.toDate)) {
                //manualValidation('div_ToDate', true, "To Date is required."+);
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.fromDate) > new Date($scope.toDate)) {
                //manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.toDate) < new Date($scope.fromDate)) {
                //manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');
            }
            else if (new Date($scope.toDate) < new Date($scope.fromDate)) {
                //manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');
            }
            else if (new Date($scope.fromDate) < new Date($scope.originalfromDate)) {
                //manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("From Date Can not be less then fiscal year Start Date.", 'failure');
            }
            else if (new Date($scope.toDate) > new Date($scope.originaltoDate)) {
                //manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To Date Can not be Greater then fiscal year End Date.", 'failure');
            }
            else {
                $http({
                    method: 'POST',
                    url: 'humanresource/BonusRegisterReports/GetBonusReportProvisional',
                    data: {
                        'yearId': $scope.yearId,
                        'withBonusValue': $scope.withBonusValue,
                        'fromDate': $scope.fromDate,
                        'toDate': $scope.toDate

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        if (reportType === 'EXCEL') {
                            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                        }
                        if (reportType === 'PDF') {
                            $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);

                        }

                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.MonthList = [];
    $scope.MonthCbo = function (yearId) {

        $scope.ADGLUrl = 'humanresource/ProfessionalTaxReports/GetMonthCbo?yearId=' + yearId;

        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MonthList = response.data;
            $scope.MonthList = JSON.parse($scope.MonthList);
        });

    };
    $scope.fromDate = null;
    $scope.toDate = null;

    $scope.originalfromDate = null;
    $scope.originaltoDate = null;

    $scope.DateList = function (yearId) {

        $scope.ADGLUrl = 'humanresource/ProfessionalTaxReports/GetFromToDateCbo?yearId=' + yearId;

        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.fromDate = response.data[0]["StartDate"];
            $scope.toDate = response.data[0]["EndDate"];
            $scope.originalfromDate = response.data[0]["StartDate"];
            $scope.originaltoDate = response.data[0]["EndDate"];
        });

    };
}