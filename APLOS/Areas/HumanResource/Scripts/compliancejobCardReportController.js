'use strict';
compliancejobCardReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function compliancejobCardReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Job Card';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.JobCardReport = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        EmployeeId: null,
        ReportFormat: 'Excel',
        chkAdditionInfo: false
    };


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeePopUpList.length; i++) {
                $scope.EmployeePopUpList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopUp").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {
        if (baseService.isUndefinedOrNull($scope.JobCardReport.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.JobCardReport.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else if (new Date($scope.JobCardReport.FromDate) > new Date($scope.JobCardReport.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.JobCardReport.ToDate) < new Date($scope.JobCardReport.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            $scope.searchbyonRoleEmpList = [];
            var parameters = { 'fromDate': $scope.JobCardReport.FromDate, 'toDate': $scope.JobCardReport.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/AttendanceManagement/GetEmployeeInformation',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeeList = response.data;
                    $scope.GetPopUpEmployee();
                }               
            });
        }
    };


    $scope.saveemployeedata = function () {
        var row = $filter('filter')($scope.EmployeePopUpList, { 'CheckBoxSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeList = row;
        }
        $scope.Back();
    }

    $scope.Back = function () {
        angular.element(document.querySelector('#JobCardPopUp')).modal('hide');
    }

   $scope.EmployeePopUpList = [];
    $scope.GetPopUpEmployee = function () {
        if (baseService.isUndefinedOrNull($scope.JobCardReport.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.JobCardReport.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else if (new Date($scope.JobCardReport.FromDate) > new Date($scope.JobCardReport.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.JobCardReport.ToDate) < new Date($scope.JobCardReport.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            var parameters = { 'fromDate': $scope.JobCardReport.FromDate, 'toDate': $scope.JobCardReport.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/AttendanceManagement/GetEmployeeInformation',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeePopUpList = response.data;
                }
            });
        }
    };
    
    $scope.showEmployeeFilterScreen = function () {
        try {
            var gridObj = $("#GridPopUp").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#JobCardPopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    
    $scope.jobcardreportFunc = function () {
        try {

            var gridObj = $("#Grid").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length == 0) {
                filtereddata = $scope.EmployeeList;
            }
            $scope.EmployeeListNew = [];
            for (var i = 0; i <filtereddata.length; i++) {
                if ($scope.EmployeeListNew, filtereddata[i].EmpSystemId) {
                    $scope.EmployeeListNew.push(filtereddata[i].EmpSystemId);
                    }            
            }      
            
            if (baseService.isUndefinedOrNull($scope.JobCardReport.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.JobCardReport.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.JobCardReport.FromDate) > new Date($scope.JobCardReport.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.JobCardReport.ToDate) < new Date($scope.JobCardReport.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                if ($scope.EmployeeListNew.length <= 0) {
                    throw "Select Employee.";
                }
                if ($scope.JobCardReport.ReportFormat === 'Excel') {
                    if ($scope.EmployeeListNew.length > 50) {
                        throw "Maximaum 50 'Job card' can be downloded at a time";
                    }
                    else {
                        var url = 'HumanResource/AttendanceManagement/GetComplianceJobCardReport?reportFormat=' + $scope.JobCardReport.ReportFormat + '&fromDate=' + $scope.JobCardReport.FromDate + '&toDate=' + $scope.JobCardReport.ToDate + '&employeeId=' + $scope.EmployeeListNew + '&chkAdditionInfo=' + $scope.JobCardReport.chkAdditionInfo;
                        $rootScope.report(url);
                        
                    }
                }
                if ($scope.JobCardReport.ReportFormat === 'Pdf') {

                    if ($scope.EmployeeListNew.length > 50) {
                        throw "Maximaum 50 'Job card' can be downloded at a time in PDF";
                    }
                    else {
                        var url = 'HumanResource/AttendanceManagement/GetComplianceJobCardReport?reportFormat=' + $scope.JobCardReport.ReportFormat + '&fromDate=' + $scope.JobCardReport.FromDate + '&toDate=' + $scope.JobCardReport.ToDate + '&employeeId=' + $scope.EmployeeListNew + '&chkAdditionInfo=' + $scope.JobCardReport.chkAdditionInfo;
                        $rootScope.report(url);                        
                    }
                }
                if ($scope.JobCardReport.ReportFormat === 'PdfView') {

                    if ($scope.EmployeeListNew.length > 50) {
                        throw "Maximaum 50 'Job card' can be downloded at a time in PDF";
                    }
                    else {
                        var url = 'HumanResource/AttendanceManagement/GetComplianceJobCardReport?reportFormat=' + $scope.JobCardReport.ReportFormat + '&fromDate=' + $scope.JobCardReport.FromDate + '&toDate=' + $scope.JobCardReport.ToDate + '&employeeId=' + $scope.EmployeeListNew + '&chkAdditionInfo=' + $scope.JobCardReport.chkAdditionInfo;
                        $rootScope.report(url);
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.jobcardreportFuncView = function () {
        try {

            var gridObj = $("#Grid").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length == 0) {
                filtereddata = $scope.EmployeeList;
            }
            $scope.EmployeeListNew = [];
            for (var i = 0; i < filtereddata.length; i++) {
                if ($scope.EmployeeListNew, filtereddata[i].EmpSystemId) {
                    $scope.EmployeeListNew.push(filtereddata[i].EmpSystemId);
                }
            }

            if (baseService.isUndefinedOrNull($scope.JobCardReport.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.JobCardReport.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.JobCardReport.FromDate) > new Date($scope.JobCardReport.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.JobCardReport.ToDate) < new Date($scope.JobCardReport.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                if ($scope.EmployeeListNew.length <= 0) {
                    throw "Select Employee.";
                } else {

                }
                $scope.JobCardReport.ReportFormat = 'PdfView';
                if ($scope.JobCardReport.ReportFormat === 'PdfView') {

                    if ($scope.EmployeeListNew.length > 50) {
                        throw "Maximaum 50 'Job card' can be downloded at a time in PDF";
                    }
                    else {
                        var url = 'HumanResource/AttendanceManagement/GetComplianceJobCardReport?reportFormat=' + $scope.JobCardReport.ReportFormat + '&fromDate=' + $scope.JobCardReport.FromDate + '&toDate=' + $scope.JobCardReport.ToDate + '&employeeId=' + $scope.EmployeeListNew + '&chkAdditionInfo=' + $scope.JobCardReport.chkAdditionInfo;
                        $rootScope.report(url);
                    }
                }
               
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}