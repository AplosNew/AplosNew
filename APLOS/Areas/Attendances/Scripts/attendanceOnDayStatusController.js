'use strict';
attendanceOnDayStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function attendanceOnDayStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    //var attdnDate = new Date();
    $scope.AttendanceOnDayStatusModel = {
        //FromDate: $filter('dateFiltering')(firstDay),
        FromDate: $filter('dateFiltering')(Date.now()),  
        ToDate: $filter('dateFiltering')(Date.now()),
        CanAvailUOM: null,
        ShiftDefinationDescription: null,
        ReportFormat: 'Excel',
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
            if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.CanAvailUOM)) {
                throw 'Place Select Day Status';
            }

            if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.AttendanceOnDayStatusModel.FromDate) > new Date($scope.AttendanceOnDayStatusModel.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.AttendanceOnDayStatusModel.ToDate) < new Date($scope.AttendanceOnDayStatusModel.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                $scope.searchbyonRoleEmpList = [];
                var parameters = { 'fromDate': $scope.AttendanceOnDayStatusModel.FromDate, 'toDate': $scope.AttendanceOnDayStatusModel.ToDate, 'DateStatus': $scope.AttendanceOnDayStatusModel.CanAvailUOM };
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: 'Attendances/AttendanceOnDayStatus/GetEmpInfo',
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
            if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.CanAvailUOM)) {
                throw 'Place Select Day Status';
            }

            if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.AttendanceOnDayStatusModel.FromDate) > new Date($scope.AttendanceOnDayStatusModel.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.AttendanceOnDayStatusModel.ToDate) < new Date($scope.AttendanceOnDayStatusModel.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else {
                var parameters = { 'fromDate': $scope.AttendanceOnDayStatusModel.FromDate, 'toDate': $scope.AttendanceOnDayStatusModel.ToDate, 'DateStatus': $scope.AttendanceOnDayStatusModel.CanAvailUOM };
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: 'Attendances/AttendanceOnDayStatus/GetEmpInfo',
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

    $scope.AttendanceOnDayStatusModelFunc = function (reportType) {
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

            if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.CanAvailUOM)) {
                throw 'Place Select Day Status';
            }
            if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.ShiftDefinationDescription)) {
                throw 'Place Select Description';
            }
            if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }

            else if (baseService.isUndefinedOrNull($scope.AttendanceOnDayStatusModel.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }

            else if (new Date($scope.AttendanceOnDayStatusModel.FromDate) > new Date($scope.AttendanceOnDayStatusModel.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }

            else if (new Date($scope.AttendanceOnDayStatusModel.ToDate) < new Date($scope.AttendanceOnDayStatusModel.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }

            else {
                if ($scope.EmployeeListNew.length <= 0) {
                    throw "Select Employee.";
                }
                else {
                    $http({
                        method: 'POST',
                        url: 'Attendances/AttendanceOnDayStatus/XlsAttendanceOnDayStatusReport',
                        data: {
                            'fromDate': $scope.AttendanceOnDayStatusModel.FromDate,
                            'toDate': $scope.AttendanceOnDayStatusModel.ToDate,
                            'DateStatus': $scope.AttendanceOnDayStatusModel.CanAvailUOM,
                            'Description': $scope.AttendanceOnDayStatusModel.ShiftDefinationDescription,
                            'employeeId': $scope.EmployeeListNew
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

}