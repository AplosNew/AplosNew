'use strict';
welfareReturnController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function welfareReturnController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'Payrolls/welfarereturn/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.paymentDate = null;
    $scope.languageId = null;
    $scope.paymentMode = null;

    $scope.SalaryTopSheetCategory = 'PayrollGroup';

    $scope.month = null;
    $scope.year = null;
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;

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

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });
  


    $scope.GetSalaryTopRegistrar = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }
           
            $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&employeeCategoryId=' + $scope.employeeCategoryId + '&paymentDate=' + $scope.paymentDate + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&SalaryTopSheetCategory=' + $scope.SalaryTopSheetCategory ;
            location.href = 'humanresource/SalaryTopSheet/XlsSalaryTopSheet?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
   
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    
   
    $scope.SortingParametersList = [
        {
            parameter: "Department",
            type: "Sorting"
        },
        {
            parameter: "Section",
            type: "Sorting"
        },
        {
            parameter: "SubSection",
            type: "Sorting"
        },
        {
            parameter: "Designation",
            type: "Sorting"
        },
        {
            parameter: "EmployeeCategory",
            type: "Sorting"
        }
    ];
    

}