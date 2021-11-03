'use strict';
dailyAllowanceController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function dailyAllowanceController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.DailyAllowance = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        AllowanceDailyId: null,
        ReportFormat: 'Excel',
        ReportGroup: null
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
        try {
            if (baseService.isUndefinedOrNull($scope.DailyAllowance.AllowanceDailyId)) {
                throw 'Place Select Allowance Type';
            }

            if (baseService.isUndefinedOrNull($scope.DailyAllowance.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.DailyAllowance.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.DailyAllowance.FromDate) > new Date($scope.DailyAllowance.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.DailyAllowance.ToDate) < new Date($scope.DailyAllowance.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                $scope.searchbyonRoleEmpList = [];
                var parameters = { 'fromDate': $scope.DailyAllowance.FromDate, 'toDate': $scope.DailyAllowance.ToDate, 'AllowanceDailyId': $scope.DailyAllowance.AllowanceDailyId };
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: 'Attendances/DailyAllowance/GetEmpInfo',
                    data: parameters
                }).then(function successCallback(response) {
                    if (response.data.length > 0) {
                        $scope.EmployeeList = response.data;
                        $scope.GetPopUpEmployee();
                    }
                    else {
                        ShowResult("No Data Found", 'failure');
                        $scope.empGrid = false;
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
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
        try {
            if (baseService.isUndefinedOrNull($scope.DailyAllowance.AllowanceDailyId)) {
                throw 'Place Select Allowance Type';
            }

            if (baseService.isUndefinedOrNull($scope.DailyAllowance.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.DailyAllowance.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.DailyAllowance.FromDate) > new Date($scope.DailyAllowance.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.DailyAllowance.ToDate) < new Date($scope.DailyAllowance.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                var parameters = { 'fromDate': $scope.DailyAllowance.FromDate, 'toDate': $scope.DailyAllowance.ToDate, 'AllowanceDailyId': $scope.DailyAllowance.AllowanceDailyId };
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: 'Attendances/DailyAllowance/GetEmpInfo',
                    data: parameters
                }).then(function successCallback(response) {
                    if (response.data.length > 0) {
                        $scope.EmployeePopUpList = response.data;
                    }
                    else {
                        ShowResult("No Data Found", 'failure');
                        $scope.empGrid = false;
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
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

    $scope.DailyAllowanceFunc = function (reportType) {
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

            if (baseService.isUndefinedOrNull($scope.DailyAllowance.AllowanceDailyId)) {
                throw 'Place Select Allowance Type';
            }

            if (baseService.isUndefinedOrNull($scope.DailyAllowance.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }

            else if (baseService.isUndefinedOrNull($scope.DailyAllowance.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }

            else if (new Date($scope.DailyAllowance.FromDate) > new Date($scope.DailyAllowance.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }

            else if (new Date($scope.DailyAllowance.ToDate) < new Date($scope.DailyAllowance.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }

            else {
                if ($scope.EmployeeListNew.length <= 0) {
                    throw "Select Employee.";
                }
                else {
                    $http({
                        method: 'POST',
                        url: 'Attendances/DailyAllowance/XlsAttendanceOnDayStatusReport',
                        data: {
                            'fromDate': $scope.DailyAllowance.FromDate,
                            'toDate': $scope.DailyAllowance.ToDate,
                            'AllowanceDailyId': $scope.DailyAllowance.AllowanceDailyId,
                            'employeeId': $scope.EmployeeListNew,
                            'ReportGroup': $scope.DailyAllowance.ReportGroup,
                        }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            if (reportType === 'Excel') {
                                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                            }
                        }
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.DailyAllowanceSummary = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Excel'
    };
    $scope.DailyAllowanceSummaryFunc = function (reportType) {
        try {

            if (baseService.isUndefinedOrNull($scope.DailyAllowanceSummary.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }

            else if (baseService.isUndefinedOrNull($scope.DailyAllowanceSummary.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }

            else if (new Date($scope.DailyAllowanceSummary.FromDate) > new Date($scope.DailyAllowanceSummary.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }

            else if (new Date($scope.DailyAllowanceSummary.ToDate) < new Date($scope.DailyAllowanceSummary.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Attendances/DailyAllowance/Getdailyattendance',
                    data: {
                        'fromDate': $scope.DailyAllowanceSummary.FromDate,
                        'toDate': $scope.DailyAllowanceSummary.ToDate

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        if (reportType === 'Excel') {
                            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                        }
                    }
                });
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.AllowanceTypeList = [];
    $scope.getAllowanceTypeList = function () {
        $http.get('Attendances/DailyAllowance/GetDailyAllowanceList')
            .then(function (response) {
                $scope.AllowanceTypeList = response.data;
            });
    };
    $scope.getAllowanceTypeList();
}