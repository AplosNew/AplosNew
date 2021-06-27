'use strict';
employeeInFoReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function employeeInFoReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
  
    $scope.EmployeeInFoReport = {
        EmployeeCatagory: 'Active',
        ReportFormat: 'Excel',
        CheckBox: false
    };
    $scope.GetdailyattendanceReport = function (reportType) {
        try {                 
                $http({
                    method: 'POST',
                    url: 'employees/EmployeeInFoReport/GetEmployeeInFo',
                    data: {
                        'radioValue': $scope.EmployeeInFoReport.EmployeeCatagory ,
                        'IsCheck': $scope.EmployeeInFoReport.CheckBox                  
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        if (reportType === 'EXCEL') {
                            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                        }
                    }
                });
           
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //$scope.GetEmployeeInFoReport = function (reportType) {
    //    try {
    //        $http({
    //            method: 'POST',
    //            url: 'employees/EmployeeInFoReport/GetEmployeeInFoReport',
    //            data: {
    //                'radioValue': $scope.EmployeeInFoReport.EmployeeCatagory,
    //                'IsCheck': $scope.EmployeeInFoReport.CheckBox
    //            }
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                if (reportType === 'EXCEL') {
    //                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

    //                }
    //            }
    //        });

    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};

    $scope.GetEmployeeInFoReport = function () {
        var reportFormat = "Excel";
        try {
            var file_src = 'employees/EmployeeInFoReport/EmployeeInFoIndexReport?reportFormat=' + reportFormat+'&radioValue=' + $scope.EmployeeInFoReport.EmployeeCatagory + '&IsCheck=' + $scope.EmployeeInFoReport.CheckBox
            $rootScope.report(file_src);

        } catch (e) {

        }
    };

    
}