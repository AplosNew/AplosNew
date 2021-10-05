'use strict';
employeeLeaveDeleteApplicationNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function employeeLeaveDeleteApplicationNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Leave Delete';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.LeaveTransactionList = [];
    $scope.path = 'HumanResource/LeaveApplicationNew/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'DeleteApprovedLeave/';

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
        ApprovalPerson:null
    };
    $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);

    $scope.searchByList = [
        {
            'name': 'Leave Name',
            'value': 'leaveTypeName'
        },
        {
            'name': 'Leave Day Type',
            'value': 'LeaveDayType'
        },
        {
            'name': 'From Date',
            'value': 'FromDate'
        },
        {
            'name': 'To Date',
            'value': 'ToDate'
        }
    ];

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
    //cboService.getCboLeaveYear(function (result) {
    //    $scope.leaveYearlist = result;
    //    $scope.YearNo = $filter("filter")($scope.leaveYearlist, { YearNo: new Date().getFullYear() })[0].Value;
    //    $scope.getLeaveBalance();
    //});
    $scope.getLeaveYear = function () {
        $http.get('HumanResource/LeaveApplicationNew/LoadYearlyCalendar')
            .then(function (response) {

                $scope.leaveYearlist = response.data;
                //$scope.YearNo = $filter("filter")($scope.leaveYearlist, { YearNo: new Date().getFullYear() })[0].YearNo;
                //$scope.getLeaveBalance();

            });

    };
    $scope.getLeaveYear();
    $scope.YearNo = null;
    $scope.getLeaveBalance = function () {
        $http.get('HumanResource/LeaveApplicationNew/GetEmpLeaveBalance?EmpsystemId=' + $scope.leaveApplicationNew.EmpSystemID + '&calanderYearId=' + $scope.YearNo)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
            });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.leaveApplication = $scope.LeaveTransactionList[$scope.index];
        $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);
        $scope.leaveApplicationNew.FromDate = $filter('dateFiltering')($scope.leaveApplicationNew.FromDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.ToDate = $filter('dateFiltering')($scope.leaveApplicationNew.ToDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.DateAdded = $filter('dateFiltering')($scope.leaveApplicationNew.DateAdded, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.DateUpdated = $filter('dateFiltering')($scope.leaveApplicationNew.DateUpdated, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.AppliedDate = $filter('dateFiltering')($scope.leaveApplicationNew.AppliedDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.ApprovedDate = $filter('dateFiltering')($scope.leaveApplicationNew.ApprovedDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.CancelationDate = $filter('dateFiltering')($scope.leaveApplicationNew.CancelationDate, 'dd-MM-yyyy');
        $scope.leaveApplicationNew.ExpectedDelivaryDate = $filter('dateFiltering')($scope.leaveApplicationNew.ExpectedDelivaryDate, 'dd-MM-yyyy');
        if ($scope.leaveApplicationNew.LeaveDayType === 'FirstHalfDay') {
            $scope.lvTodate = true;
        }
        if ($scope.leaveApplicationNew.LeaveDayType === 'SecondHalfDay') {
            $scope.lvTodate = true;
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

    $scope.getData = function (pageno) {
        baseService.paginationBase('HumanResource/LeaveApplicationNew/GetEmpLeaveListForDelete?EmpsystemId=' + $scope.leaveApplicationNew.EmpSystemID + '&yearNo=' + $scope.YearNo, pageno, $scope.leaveParameters)
            .then(function (data) {
                $scope.LeaveTransactionList = data.Rows;
                $scope.leaveParameters.total_count = data.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.employee = [];
    $scope.popUp = function () {
        try {
            $http({
                method: 'GET',
                url: 'humanresource/LeaveApplicationNew/getemployeelist'
            }).then(function successCallback(response) {
                $scope.employee = response.data;
            });
            angular.element(document.querySelector('#employeePopUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
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

    $scope.SectionList = [];
    cboService.getSectionCbo(function (result) {
        $scope.SectionList = result;
    });

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeParameters.offset = 0;
    };
    $scope.lvTodate = false;

    $scope.setDisable = function () {
        $scope.leaveApplicationNew.LeaveDayType = 'FirstHalfDay';
        if ($scope.leaveApplicationNew.LeaveDayType === 'FirstHalfDay') {
            $scope.lvTodate = true;
            $scope.leaveApplicationNew.ToDate = null;
        }
    };

    $scope.setDisableSec = function () {
        $scope.leaveApplicationNew.LeaveDayType = 'SecondHalfDay';
        if ($scope.leaveApplicationNew.LeaveDayType === 'SecondHalfDay') {
            $scope.lvTodate = true;
            $scope.leaveApplicationNew.ToDate = null;
        }
    };

    $scope.setEnable = function () {
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        if ($scope.leaveApplicationNew.LeaveDayType === 'FullDay') {
            $scope.lvTodate = false;
        }
    };

    function ValidationLeave() {
        var fd = $filter('dateFiltering')($scope.leaveApplicationNew.FromDate, 'dd-MM-yyyy');
        var td = $filter('dateFiltering')($scope.leaveApplicationNew.ToDate, 'dd-MM-yyyy');

        if ($scope.leaveApplicationNew.LeaveDayType === 'SecondHalfDay' || $scope.leaveApplicationNew.LeaveDayType === 'FirstHalfDay') {
            $scope.leaveApplicationNew.ToDate = $scope.leaveApplicationNew.FromDate;
        }

        if (new Date(fd) > new Date(td)) {
            throw 'From Date cann\'t be greater than To Date.';
        }
        if ($scope.leaveApplicationNew.LeaveDayType === 'FullDay' && baseService.isUndefinedOrNull($scope.leaveApplicationNew.ToDate)) {
            throw 'To Date is Required.';
        }
    }
  
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.leaveApplicationNew.SystemID)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl  ,
                data: { 'id': $scope.leaveApplicationNew.SystemID, 'EmpSystemid': $scope.leaveApplicationNew.EmpSystemID},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LeaveTransactionList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.getData();
                    $scope.getLeaveBalance();
                    $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
                    $scope.setEnable();
                    ClearDataField();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearDataField() {
        $scope.Action = 'Save';
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId, EmpSystemID: $scope.leaveApplication.EmpSystemID, EmployeeName: $scope.leaveApplication.EmployeeName, EmployeeCode: $scope.leaveApplication.EmployeeCode };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId, EmpSystemID: $scope.leaveApplicationNew.EmpSystemID, EmployeeName: $scope.leaveApplicationNew.EmployeeName,EmployeeCode: $scope.leaveApplication.EmployeeCode };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.setEnable();
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId};
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId};
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.LeaveBalanceList = [];
        $scope.LeaveTransactionList = [];
        $scope.imageSrc = virtualPath.EmployeePic + '';
    }
}
