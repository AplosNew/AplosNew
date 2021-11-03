'use strict';
separatedsalaryStructureController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function separatedsalaryStructureController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'humanresource/payrollReports/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.paymentDate = null;
    $scope.languageId = null;
    $scope.paymentMode = null;
    var sqlInStatement = "";
    $scope.reportStatus = {
        status: "dayStatus"
    };

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.effectiveDate = null;
    $scope.FromDate = $filter('dateFiltering')(firstDay);
    $scope.ToDate = $filter('dateFiltering')(Date.now());

    $scope.hrStatus = {
        pstatus: 'Default'
    };
    $scope.withStructure = null;
    $scope.sheetType = false;
    $scope.empGrid = false;

    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;
    $scope.empGrid = false;
    $scope.localLanguageList = [];
    cboService.getLanguageIdCbo(function (result) {
        $scope.localLanguageList = result;
    });

    $scope.payGroupList = [];
    $scope.payGroupListSelected = [];

    cboService.getPayRollGroupCbo(function (result) {
        $scope.payGroupList = result;
    });

    $scope.getSalaryProcessIdList = function () {
        $scope.isCompletedMonth = 1;
        cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
            $scope.cboSalaryProcessIdList = result;
        });
    };
    //$scope.getSalaryProcessIdList();
    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();
    $scope.payGroupListSelected = [];
    $scope.EmployeeList = [];

    $scope.GetEmployeeInformation = function () {   
        var FromDate = new Date($scope.FromDate);
        var ToDate = new Date($scope.ToDate);
        var Difference_In_Time = ToDate.getTime() - FromDate.getTime();
        var Difference_In_Days = Difference_In_Time / (1000 * 3600 * 24);   
         if (baseService.isUndefinedOrNull($scope.FromDate)) {          
             ShowResult('From Date is required');
             throw "";
                        }
         if (baseService.isUndefinedOrNull($scope.ToDate)) {
             ShowResult('To Date is required');
             throw "";
                }
         if (new Date($scope.FromDate) > new Date($scope.ToDate)) {     
             ShowResult("From date must be below or equal to To Date", 'failure');
             throw "";
                    }
         if (new Date($scope.ToDate) < new Date($scope.FromDate)) {          
             ShowResult('To date must be above or equal to From Date');
             throw "";
                    }                  
        if (Difference_In_Days > 365) {
            ShowResult("Date Duration More Than One Year");
            throw "";
        }        
        else
        {
            var parameters = { 'effectiveDate': $scope.effectiveDate, 'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate,'salaryProcessId':'STRUCTURE', 'payRollGroup': $scope.payGroupListSelected };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'humanresource/PayrollReports/GetSeparatedEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.empGrid = true;
                    $scope.EmployeeList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });
        }       
    };

    $scope.GetEmployeeSalaryStructure = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                ShowResult('From Date is required');
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                ShowResult('To Date is required');
            }
            if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
                ShowResult('To date must be above or equal to From Date');
            }

            var FromDate = new Date($scope.FromDate);
            var ToDate = new Date($scope.ToDate);
            var Difference_In_Time = ToDate.getTime() - FromDate.getTime();
            var Difference_In_Days = Difference_In_Time / (1000 * 3600 * 24);
            if (Difference_In_Days > 365) {
                ShowResult("Date Duration More Than One Year");
            }

            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                     parameters = [];
                    parameters.push({ "Key": "EmployeeCode", "Value": getString(filteredRecords, "EmployeeCode") });                  
                }               
            }
            if (parameters.length === 0)
            {
                parameters.push({ "Key": "", "Value": ""});                  

            }
            $http({
                method: 'POST',
                url: 'humanresource/PayrollReports/GetSeparatedEmployeeStructure',
                data: {
                    'effectiveDate': $scope.effectiveDate ,
                    'payRollGroup': $scope.payGroupListSelected,
                    'parameters': parameters,
                    'FromDate': $scope.FromDate,
                    'ToDate': $scope.ToDate
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                   $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    };
  

}



