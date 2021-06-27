'use strict';
LeaveDeleteSingleDayController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function LeaveDeleteSingleDayController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Leave Delete Single Day';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.LeaveTransactionList = [];
    $scope.path = 'Employees/LeaveDeleteSingleDay/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'GetApprovedLeave/';
    $scope.UpdateDataUrl = $scope.path + 'UpdateLeaveT/';

    $scope.leaveApplication = {
        SystemID: null,
        EmpSystemID: null,
        LTSystemID: null,
        ComAssignLvSystemID: null,
        OffDayMstSystemID: null,
        GroupID: null,
        PlantID: null,
        FromDate: null,
        ToDate: null,
        LeaveDays: null,
        IsApproved: false,
        LvReason: null,
        AppliedDate: null,
        ApprovedBy: null,
        ApprovedDate: null,
        IsPostApplied: null,
        CompanyId: null,
        IsAdminApproved: null,
        IsCancel: false,
        Cancelby: null,
        CancelationDate: null,
        CancelationReason: null,
        AppliedBy: null,
        LeaveDayType: 'FullDay',
        LeaveStatus: null,
        ExpectedDelivaryDate: null,
        BabyNo: 0,
        ExceptionLeave: null,
        ApprovalPerson: null
    };
    $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);

    
    $scope.leaveTypelist = [];
    $scope.leavetypecbo = function () {
        cboService.getEmpLeaveTypeCbo($scope.leaveApplicationNew.EmpSystemID, function (result) {
            $scope.leaveTypelist = result;
        });
    };

    $scope.empList = [];
    cboService.getEmployeeCbo(function (result) {
        $scope.empList = result;
    });

    $scope.leaveYearlist = [];
    $scope.getLeaveYear = function () {
        $http.get('Employees/LeaveDeleteSingleDay/LoadYearlyCalendar')
            .then(function (response) {
                $scope.leaveYearlist = response.data;
            });
    };
    $scope.getLeaveYear();
    $scope.YearNo = null;
    $scope.getLeaveBalance = function () {
        $http.get('Employees/LeaveDeleteSingleDay/GetEmpLeaveBalance?EmpsystemId=' + $scope.leaveApplicationNew.EmpSystemID + '&calanderYearId=' + $scope.YearNo)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
            });
    };

    $scope.Get = function (obj) {
        //$scope.index = index;
        $scope.leaveApplication = obj.data;
        $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);
        $scope.imageSrc = virtualPath.EmployeePic + $scope.leaveApplicationNew.EmpPicPath;
        $scope.leaveApplicationNew.FromDate = $filter('dateFiltering')($scope.leaveApplicationNew.FromDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.ToDate = $filter('dateFiltering')($scope.leaveApplicationNew.ToDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.DateAdded = $filter('dateFiltering')($scope.leaveApplicationNew.DateAdded, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.DateUpdated = $filter('dateFiltering')($scope.leaveApplicationNew.DateUpdated, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.AppliedDate = $filter('dateFiltering')($scope.leaveApplicationNew.AppliedDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.ApprovedDate = $filter('dateFiltering')($scope.leaveApplicationNew.ApprovedDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.CancelationDate = $filter('dateFiltering')($scope.leaveApplicationNew.CancelationDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.ExpectedDelivaryDate = $filter('dateFiltering')($scope.leaveApplicationNew.ExpectedDelivaryDate, 'dd-MM-yyyy');
        $scope.GetLeaveData();
        $scope.getLeaveBalance();
        $scope.getAllLeave($scope.leaveApplicationNew.EmpSystemID);
        if ($scope.leaveApplicationNew.LeaveDayType === 'FirstHalfDay') {
            $scope.lvTodate = true;
        }
        if ($scope.leaveApplicationNew.LeaveDayType === 'SecondHalfDay') {
            $scope.lvTodate = true;
        }
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = 'Update';

    };

    $scope.leaveParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'FromDate',
        searchBy: "FromDate",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LeaveTransactionList = [];
    $scope.getData = function (pageno) {
        if (baseService.isUndefinedOrNull($scope.YearNo)) {
            $scope.LeaveTransactionList = [];
            $scope.Clear();
        }
        else {
            $http.get('Employees/LeaveDeleteSingleDay/GetEmpLeaveListForSingleDelete?EmpsystemId=' + $scope.leaveApplicationNew.EmpSystemID + '&yearNo=' + $scope.YearNo)
                .then(function (data) {
                    $scope.LeaveTransactionList = data.data;
                    //$scope.leaveParameters.total_count = data.Total;

                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }

    };

    

    $scope.setData = function (obj) {
        $scope.Clear();
        var data = obj.data;
        $scope.leaveApplicationNew.EmployeeCode = data.EmployeeCode;
        $scope.leaveApplicationNew.EmpSystemID = data.SystemID;
        $scope.leaveApplicationNew.EmployeeName = data.EmployeeName;
        $scope.leaveApplicationNew.SectionId = data.SectionId;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.getData();
        $scope.leavetypecbo();
        $scope.getLeaveBalance();
    };

    
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.LeaveBalanceList = [];
        $scope.LeaveList = [];
        $scope.LeaveTransactionList = [];
        $scope.imageSrc = null;
    }
    $scope.LeaveList = [];
    
    $scope.GetLeaveData = function (EmpId) {
        $scope.FromDate = null;
        $scope.ToDate = null;
        $scope.LeaveDays = null;
        $http({
            method: 'POST',
            url: $scope.deleteUrl,
            data: { 'id': $scope.leaveApplicationNew.SystemID, 'EmpSystemid': $scope.leaveApplicationNew.EmpSystemID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.LeaveList = response.data;
            $scope.FromDate = response.data[0].FromDate;
            $scope.ToDate = response.data[0].ToDate;
            $scope.LeaveDays = response.data[0].LeaveDays;
        });
    };
    $scope.AllLeaveList = [];
    $scope.getAllLeave = function (EmpId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetAllLeave",
            data: { 'Emp': EmpId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AllLeaveList = response.data;
        });
    };


    $scope.confirmdelete = false;
    $scope.ConfirmRebate = function (obj) {
        var eDialog = $("#rebate").data("ejDialog");
        eDialog.open(obj);
        $scope.data = obj;
        $("#rebate_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmrebateClose = function () {
        var eDialog = $("#rebate").data("ejDialog");
        eDialog.close();
    };
    $scope.DeleteLeave = function () {
        if (!baseService.isUndefinedOrNull($scope.data, $scope.leaveApplicationNew.SystemID)) {
            $http({
                method: 'POST',
                url: $scope.path + "DeleteLeave",
                data: { 'ID': $scope.data.SystemID, 'Update': $scope.leaveApplicationNew.SystemID, 'EmpId': $scope.leaveApplication.EmpSystemID, 'workdate': $scope.data.WorkDate, 'FromDate': $scope.data.FromDate, 'ToDate': $scope.data.ToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetLeaveData();
                    $scope.getData();
                    $scope.getLeaveBalance();
                    $scope.getAllLeave();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Tab

}



