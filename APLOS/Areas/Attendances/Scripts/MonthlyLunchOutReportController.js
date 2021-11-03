'use strict';
MonthlyLunchOutReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function MonthlyLunchOutReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Monthly Lunch Out';
    $scope.path = 'Attendances/MonthlyLunchOutReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
   
    $scope.OTFinalInformation = {
        YearNo: null,
        MonthNo: null
    };

    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];

    $scope.yearlist = [];
    $scope.GetCbo = function () {
        $http.get('Attendances/AttendanceProcessUI/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.yearlist = [];
                        $scope.yearlist = response.data;
                        
                        
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();
    var d = new Date();
    $scope.OTFinalInformation.YearNo = ''+d.getFullYear();
    var xx = d.getMonth() + 1;
    $scope.OTFinalInformation.MonthNo = '' + xx;


    

    $scope.GetReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.OTFinalInformation.YearNo)) {
                manualValidation('div_FromDate', true, "Year is required.");
                ShowResult("Year is required.", 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.OTFinalInformation.MonthNo)) {
                manualValidation('div_FromDate', true, "Month is required.");
                ShowResult("Month is required.", 'failure');
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Attendances/MonthlyLunchOutReport/MonthlyLunchOutRpt',
                    data: {
                        'Month': $scope.OTFinalInformation.MonthNo, 'Year': $scope.OTFinalInformation.YearNo
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}