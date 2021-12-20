'use strict';
hourlyOTNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function hourlyOTNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Hourly OT';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Leave/HourlyOTNew/';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.isActive = true;
    $scope.isSeperated = false;
     

    $scope.employeeInfo = {};
    $scope.SetData = function (obj) {
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID;

        $scope.employeeInfo.EmpPic = virtualPath.EmployeePic + emp.EmpPicPath;
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.employeeInfo.DOJ = emp.DOJ;
        $scope.employeeInfo.DOC = emp.DOC;
        $scope.employeeInfo.EmailId = emp.EmailId;
        $scope.employeeInfo.Code = emp.Code;
        $scope.employeeInfo.Section = emp.Section;
        $scope.employeeInfo.SubSection = emp.SubSection;
        $scope.employeeInfo.Department = emp.Department;
        $scope.employeeInfo.LegalDesignation = emp.LegalDesignation;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.OffDutyHoursModel = Object.assign({}, $scope.OffDutyHoursModelOriginal);
        $scope.GetShiftList = {};

        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

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
    
    $scope.GetHourlyOtReport = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.ManualOutTimeDateWise.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.ManualOutTimeDateWise.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.ManualOutTimeDateWise.FromDate) > new Date($scope.ManualOutTimeDateWise.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.ManualOutTimeDateWise.ToDate) < new Date($scope.ManualOutTimeDateWise.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');

            }
            else {

                var url = $scope.path + '/GetHourlyOT?reportFormat=Excel' + ' &FromDate=' + $scope.ManualOutTimeDateWise.FromDate + ' &ToDate=' + $scope.ManualOutTimeDateWise.ToDate;

                $rootScope.report(url);

            }


        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.monthList = [
        {
            Value: "Jan",
            Text: 'January'
        },
        {
            Value: "Feb",
            Text: 'February'
        },
        {
            Value: "Mar",
            Text: 'March'
        },
        {
            Value: "Apr",
            Text: 'April'
        },
        {
            Value: "May",
            Text: 'May'
        },
        {
            Value: "Jun",
            Text: 'June'
        },
        {
            Value: "Jul",
            Text: 'July'
        },
        {
            Value: "Aug",
            Text: 'August'
        },
        {
            Value: "Sep",
            Text: 'September'
        },
        {
            Value: "Oct",
            Text: 'October'
        },
        {
            Value: "Nov",
            Text: 'November'
        },
        {
            Value: "Dec",
            Text: 'December'
        }
    ];

    $scope.year  = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();


    $scope.GetHourlyOtMonthlyReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.year)) {
                manualValidation('div_FromDate', true, "Year is required.");
                ShowResult("Year No is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.month)) {
                manualValidation('div_ToDate', true, "Month is required.");
                ShowResult("Month No is required.", 'failure');
            }
                
            else {
                try
                {
                    var url = $scope.path+ '/GetHourlyOTMonthly?reportFormat=Excel' + ' &YearNo=' + $scope.year + ' &MonthNo=' + $scope.month + ' &isActive=' + $scope.isActive + ' &isSeperated=' + $scope.isSeperated;
                    $rootScope.report(url);
                    //ShowResult(response.data.Message, 'failure');                    
                }
                catch (e) {
                    ShowResult(e, 'failure');
                }
            }
            
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.IndividualDailyOT = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Excel',
        OTDuration: 0,
        OTfinal: 'OverStay',
        CheckBox: false
    };
    $scope.GetIndividualDailyOTReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.IndividualDailyOT.OTDuration)) {
                throw ("OT Duration is required.");
            }
            if (baseService.isUndefinedOrNull($scope.IndividualDailyOT.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("Year No is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.IndividualDailyOT.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("Month No is required.", 'failure');
            }
            else if (new Date($scope.IndividualDailyOT.FromDate) > new Date($scope.IndividualDailyOT.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.IndividualDailyOT.ToDate) < new Date($scope.IndividualDailyOT.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');

            }
            else {
                var url = 'Leave/HourlyOT/GetIndividualDailyOT?reportFormat=Excel' + ' &FromDate=' + $scope.IndividualDailyOT.FromDate + ' &ToDate=' + $scope.IndividualDailyOT.ToDate + ' &OTDuration=' + $scope.IndividualDailyOT.OTDuration + '&OTfinal=' + $scope.IndividualDailyOT.OTfinal + '&CheckBox=' + $scope.IndividualDailyOT.CheckBox;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };


}