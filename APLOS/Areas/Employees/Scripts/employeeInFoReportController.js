'use strict';
employeeInFoReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function employeeInFoReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
  
    $scope.EmployeeInFoReport = {
        EmployeeCatagory: 'Active',
        ReportFormat: 'Excel',
        CheckBox: false,
        LONGABSENTEEISM: false,
        TBS: false,
        EmployeeCurrentStatus:null
    };


    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.GetEmployeeInFoReport = function (reportType) {
        try {

            $http({
                method: 'POST',
                url: 'employees/EmployeeInFoReport/XlsEmployeeInfo',
                data: {
                    'reportFormat': reportType,
                    'radioValue': $scope.EmployeeInFoReport.EmployeeCatagory ,
                    'IsCheck': $scope.EmployeeInFoReport.CheckBox ,
                    'LA': $scope.EmployeeInFoReport.LONGABSENTEEISM,
                    'TBS': $scope.EmployeeInFoReport.TBS
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if (reportType == "EXCEL") {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                    if (reportType == "PDF") {
                        $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);
                    }
                }
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
    
    $scope.GetEmployeeInFoReport_ = function () {
        var reportFormat = "Excel";
        try {
            var file_src = 'employees/EmployeeInFoReport/EmployeeInFoIndexReport?reportFormat=' + reportFormat + '&radioValue=' + $scope.EmployeeInFoReport.EmployeeCatagory + '&IsCheck=' + $scope.EmployeeInFoReport.CheckBox + '&LA=' + $scope.EmployeeInFoReport.LONGABSENTEEISM + '&TBS=' + $scope.EmployeeInFoReport.TBS 
            $rootScope.report(file_src);

        } catch (e) {

        }
    };

    
}