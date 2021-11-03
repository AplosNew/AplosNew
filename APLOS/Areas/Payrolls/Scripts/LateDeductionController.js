'use strict';
LateDeductionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function LateDeductionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'humanresource/LateDeduction/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
  

   

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
    $scope.year = new Date().getFullYear().toString();
    $scope.dayList = [
        {
            Value: 1,
            Text: '1'
        },
        {
            Value: 2,
            Text: '2'
        },
        {
            Value: 3,
            Text: '3'
        },
        {
            Value: 4,
            Text: '4'
        },
        {
            Value: 5,
            Text: '5'
        },
        {
            Value: 6,
            Text: '6'
        },
        {
            Value: 7,
            Text: '7'
        },
        {
            Value: 8,
            Text: '8'
        },
        {
            Value: 9,
            Text: '9'
        },
        {
            Value: 10,
            Text: '10'
        }
       
    ];
    $scope.workingdayList = [
        {
            Value: 1,
            Text: '1'
        },
        {
            Value: 2,
            Text: '2'
        },
        {
            Value: 3,
            Text: '3'
        },
        {
            Value: 4,
            Text: '4'
        },
        {
            Value: 5,
            Text: '5'
        },
        {
            Value: 6,
            Text: '6'
        },
        {
            Value: 7,
            Text: '7'
        },
        {
            Value: 8,
            Text: '8'
        },
        {
            Value: 9,
            Text: '9'
        },
        {
            Value: 10,
            Text: '10'
        },
         {
            Value: 11,
            Text: '11'
        },
        {
            Value: 12,
            Text: '12'
        },
        {
            Value: 13,
            Text: '13'
        },
        {
            Value: 14,
            Text: '14'
        },
        {
            Value: 15,
            Text: '15'
        },
        {
            Value: 16,
            Text: '16'
        },
        {
            Value: 17,
            Text: '17'
        },
        {
            Value: 18,
            Text: '18'
        },
        {
            Value: 19,
            Text: '19'
        },
        {
            Value: 20,
            Text: '20'
        },
        {
            Value: 21,
            Text: '21'
        },
        {
            Value: 22,
            Text: '22'
        },
        {
            Value: 23,
            Text: '23'
        },
        {
            Value: 24,
            Text: '24'
        },
        {
            Value: 25,
            Text: '25'
        },
        {
            Value: 26,
            Text: '26'
        },
        {
            Value: 27,
            Text: '27'
        },
        {
            Value: 28,
            Text: '28'
        },
        {
            Value: 29,
            Text: '29'
        },
        {
            Value: 30,
            Text: '30'
        },
        {
            Value: 31,
            Text: '31'
        }
    ];
    $scope.yearList = [];

    //cboService.getCboLeaveYear(function (result) {
    //    $scope.yearList = result;
    //    $scope.year = new Date().getFullYear().toString();
       
    //});


    $scope.workingday = 30;
    $scope.day = 3;

   
    
    $scope.GetYearData = function () {
        var myDate = new Date();
        var yearNew = myDate.getFullYear();

        for (var i = 2017; i < yearNew + 2; i++) {
            var model = {};
            model.Text = i;
            model.Value = i;
            $scope.yearList.push(model);
        }
        $scope.year = new Date().getFullYear();
    };
    $scope.GetYearData();


    $scope.month = new Date().getMonth() + 1;

    $scope.GetData = function () {
        location.href = 'Payrolls/LateDeduction/GetData?YearNo=' + $scope.year + '&MonthNo=' + $scope.month + '&DayNo=' + $scope.day + '&Workingday=' + $scope.workingday;
        //location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileShift?reportFormat=' + ReportFormat + '&EmployeeIds=' + EmployeeIds;
    };
}



