'use strict';
employeeMyAppLeaveApplicationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function employeeMyAppLeaveApplicationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Leave Application';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.LeaveTransactionList = [];
    $scope.path = 'Employees/EmployeeMyAppLeaveApplication/';
    //$scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/'; 

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
        IsApproved: null,
        LvReason: null,
        AppliedDate: null,
        ApprovedBy: null,
        ApprovedDate: null,
        IsPostApplied: null,
        CompanyId: null,
        IsAdminApproved: null,
        IsCancel: null,
        Cancelby: null,
        CancelationDate: null,
        CancelationReason: null,
        AppliedBy: null,
        LeaveDayType: 'FullDay',
        LeaveStatus: null,
        ExpectedDelivaryDate: null,
        BabyNo: 0,
        FirstApprovingStatus: 0,
        FirstApprovingAuthority: null,
        FirstApprovingDate: null

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
    cboService.getCboLeaveType(function (result) {
        $scope.leaveTypelist = result;
    });


    $scope.leaveYearlist = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.leaveYearlist = result;
        $scope.YearNo = $filter("filter")($scope.leaveYearlist, { Text: new Date().getFullYear() })[0].Value;
        $scope.LeaveTypes();
    });

    $scope.employeeList = [];
    cboService.getEmployeeCbo(function (result) {
        $scope.employeeList = result;
    });

    $scope.YearNo = null;
    $scope.LeaveTypes = function () {
        $http.get('Employees/EmployeeMyAppLeaveApplication/GetLeaveBalance?calanderYearId=' + $scope.YearNo)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
            });
    };

    $scope.empList = [];
    cboService.getEmployeeCbo(function (result) {
        $scope.empList = result;
    });

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
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
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
    $scope.ResponsiblePerson = null;
    $scope.ResponsiblePersonList = [];

    $scope.getData = function (pageno) {
        if (baseService.isUndefinedOrNull($scope.YearNo)) {
            cboService.getCboLeaveYear(function (result) {
                $scope.leaveYearlist = result;
                $scope.YearNo = $filter("filter")($scope.leaveYearlist, { Text: new Date().getFullYear() })[0].Value;
            });
        }
        baseService.paginationBase('Employees/EmployeeMyAppLeaveApplication/GetList?yearNo=' + $scope.YearNo, pageno, $scope.leaveParameters)
            .then(function (data) {
                $scope.LeaveTransactionList = data.Rows;
                $scope.leaveParameters.total_count = data.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });

        $http.get('Employees/EmployeeMyAppLeaveApplication/GetResponsiblePersonBudgetCode')
            .then(function successCallback(response) {
                if (!baseService.isUndefinedOrNull(response.data.Message)) {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }
                else {
                    if (response.data.length > 0) {

                        $scope.ResponsiblePerson = "Reponsible Person: " + response.data[0]["EmployeeName"] + "\n Budget Code: " + response.data[0]["BudgetCode"];
                        $scope.leaveApplicationNew.FirstApprovingAuthority = response.data[0]["SystemId"];
                        $scope.ResponsiblePersonList = response.data;
                    }
                    else {
                        $scope.ResponsiblePerson = "Please assign responsible budget code to that employee's budget code";
                    }
                }
            }
            ), function errorCallBack(response) {
                showResult(response.data.Message, 'failure');
            };
    };
    $scope.getData();

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

    $scope.DisabledDates = [];

    $('.datepicker').datepicker({
        startDate: '-1d',
        endDate: '+31d',
        datesDisabled: $scope.DisabledDates,
        format: 'dd-M-yyyy',
        todayHighlight: true,
        autoclose: true,
        inline: true,
        changeMonth: true
    });

   
    $scope.Save = function () {
        try {
            ValidationLeave();
            angular.copy($scope.leaveApplicationNew, $scope.leaveApplication);
            $scope.$broadcast('show-errors-check-validity');
            $scope.leaveApplication.FirstApprovingStatus = 0;
            $scope.responsiblePersonId = null;
            $scope.responsiblePersonName = null;
            $scope.responsiplePersonEmail = null;


            if ($scope.leaveApplicationNewForm.$valid) {
               
                    if ($scope.ResponsiblePersonList.length > 0) {
                        $scope.responsiblePersonId = $scope.ResponsiblePersonList[0]["SystemId"];
                        $scope.responsiblePersonName = $scope.ResponsiblePersonList[0]["EmployeeName"];
                        $scope.responsiplePersonEmail = $scope.ResponsiblePersonList[0]["EmailId"];


                        if ($scope.Action === 'Save') {

                            $http({
                                method: 'POST',
                                url: $scope.saveUrl,

                                data: {
                                    'leaveApplication': $scope.leaveApplication
                                    , 'responsiblePersonId': $scope.responsiblePersonId
                                    , 'responsiblePersonName': $scope.responsiblePersonName
                                    , 'responsiplePersonEmail': $scope.responsiplePersonEmail
                                    , 'yearId': $scope.YearNo
                                },
                                dataType: 'JSON'
                            }).then(function successCallback(response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.data.Message, 'failure');
                                }
                                else {
                                    ShowResult(response.data.Message, 'success');
                                    $scope.LeaveTransactionList.push(response.data.LeaveApplication);
                                    $scope.getData();
                                    ClearFields();
                                }
                            }), function errorCallBack(response) {
                                ShowResult(response.data.Message, 'failure');
                            };
                        }
                        else if ($scope.Action === 'Update') {
                            $http({
                                method: 'POST',
                                url: $scope.updateUrl,
                                data: { 'leaveApplication':$scope.leaveApplication, 'yearId': $scope.YearNo },
                                dataType: 'JSON'
                            }).then(function successCallback(response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.data.Message, 'failure');
                                }
                                else {
                                    ShowResult(response.data.Message, 'success');
                                    if ($scope.index > -1) {
                                        $scope.LeaveTransactionList[$scope.index] = $scope.leaveApplication;
                                        $scope.getData();
                                        ClearFields();
                                    }
                                }
                            }, function errorCallBack(response) {
                                ShowResult(response.data.Message, 'failure');
                            });
                        }
                    
                }
                else {
                    ShowResult($scope.ResponsiblePerson, "failure");
                }
            }


        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.leaveApplicationNew.SystemID)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.leaveApplicationNew.SystemID,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LeaveTransactionList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.leaveApplication = {};
        $scope.leaveApplicationNew = {};
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';

    }
}