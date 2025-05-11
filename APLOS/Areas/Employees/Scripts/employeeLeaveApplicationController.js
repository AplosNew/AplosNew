'use strict';
employeeLeaveApplicationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function employeeLeaveApplicationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Leave Application';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.LeaveTransactionList = [];
    $scope.path = 'Employees/LeaveApplication/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
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
        ApprovalPerson: null,
        FirstApprovingStatus: 1,
        FirstApprovingAuthority: null,
        FirstApprovingDate: null,
        PolicyName: null
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
    cboService.getCboLeaveYear(function (result) {
        $scope.leaveYearlist = result;
        $scope.YearNo = $filter("filter")($scope.leaveYearlist, { Text: new Date().getFullYear() })[0].Value;
        $scope.getLeaveBalance();
    });

    $scope.YearNo = null;
    $scope.getLeaveBalance = function () {
        $http.get('Employees/LeaveApplication/GetEmpLeaveBalance?EmpsystemId=' + $scope.leaveApplicationNew.EmpSystemID + '&calanderYearId=' + $scope.YearNo)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
                //console.log($scope.LeaveBalanceList);
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
        $scope.leaveApplicationNew.FirstApprovingDate = $filter('dateFiltering')($scope.leaveApplicationNew.FirstApprovingDate, 'dd-MM-yyyy');

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
        baseService.paginationBase('Employees/leaveApplication/GetEmpLeaveList?EmpsystemId=' + $scope.leaveApplicationNew.EmpSystemID + '&yearNo=' + $scope.YearNo, pageno, $scope.leaveParameters)
            .then(function (data) {
                $scope.LeaveTransactionList = data.Rows;
                $scope.leaveParameters.total_count = data.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.employeeList = [];
    $scope.popUp = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.leaveApplicationNew.SectionId)) {
                throw 'Select a section before employee selection.';
            }
            $scope.employeeList = [];
            $http({
                method: 'GET',
                url: 'employees/leaveApplication/getsectionemployeelist?sectionId=' + $scope.leaveApplicationNew.SectionId
            }).then(function successCallback(response) {
                $scope.employeeList = response.data;
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
        $scope.leaveApplicationNew.DOJ = data.DOJ;
        $scope.leaveApplicationNew.DOC = data.DOC;
        $scope.leaveApplicationNew.LegalDesignation = data.LegalDesignation;
        $scope.leaveApplicationNew.DesignationGroup = data.DesignationGroup;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        $scope.leaveApplicationNew.PolicyName = data.PolicyName;
        angular.element(document.querySelector('#employeePopUp')).modal('hide');

        const start = new Date(data.DOJ);
        const today = new Date();

        let years = today.getFullYear() - start.getFullYear();
        let months = today.getMonth() - start.getMonth();
        let days = today.getDate() - start.getDate();

        // Adjust months and years if needed
        if (days < 0) {
            months -= 1;
            const previousMonth = new Date(today.getFullYear(), today.getMonth(), 0);
            days += previousMonth.getDate();
        }

        if (months < 0) {
            years -= 1;
            months += 12;
        }

        $scope.ServicePeriod = "" + years + " years " + months + " months " + days + " days";


        $scope.getData();
        $scope.leavetypecbo();
        $scope.getLeaveBalance();
        $scope.LeavePolicyName();
    };


    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.LeavePolicyNames = null;
    $scope.LeavePolicyName = function () {
        $http({
            method: 'GET',
            url: 'Employees/LeaveApplication/getLeavePolicy'
        }).then(function successCallback(response) {
            $scope.LeavePolicyNames = response.data.data[0].PolicyName;
        });
    };

    $scope.ServicePeriod = "";
    $scope.GetEnterEmployeeOutInfo = function () {
        $scope.ServicePeriod = "";
        var parameters = {
            'SearchValue': $scope.leaveApplicationNew.EmployeeCode
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'employees/leaveApplication/GetEmpInfo',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                //$scope.Clear();
                var data = response.data;
                $scope.leaveApplicationNew.EmployeeCode = data[0].EmployeeCode;
                $scope.leaveApplicationNew.EmpSystemID = data[0].SystemID;
                $scope.leaveApplicationNew.EmployeeName = data[0].EmployeeName;
                $scope.leaveApplicationNew.SectionId = data[0].SectionId;
                $scope.leaveApplicationNew.DOJ = data[0].DOJ;
                $scope.leaveApplicationNew.DOC = data[0].DOC;
                $scope.leaveApplicationNew.LegalDesignation = data[0].LegalDesignation;
                $scope.leaveApplicationNew.DesignationGroup = data[0].DesignationGroup;
                $scope.leaveApplicationNew.PolicyName = data[0].PolicyName;
                $scope.imageSrc = virtualPath.EmployeePic + data[0].EmpPicPath;

                const start = new Date(data[0].DOJ);
                const today = new Date();

                let years = today.getFullYear() - start.getFullYear();
                let months = today.getMonth() - start.getMonth();
                let days = today.getDate() - start.getDate();

                // Adjust months and years if needed
                if (days < 0) {
                    months -= 1;
                    const previousMonth = new Date(today.getFullYear(), today.getMonth(), 0);
                    days += previousMonth.getDate();
                }

                if (months < 0) {
                    years -= 1;
                    months += 12;
                }

                $scope.ServicePeriod = "" + years + " years " + months + " months " + days + " days";




                $scope.getData();
                $scope.leavetypecbo();
                $scope.getLeaveBalance();
            }
            else {
                $scope.Clear();
                ShowResult("Please provide correct employee code", 'failure');

            }
        });
    };



    $scope.setEmpData = function (obj) {
        $scope.Clear();
        $scope.ServicePeriod = "";
        var data = obj.data;
        $scope.leaveApplicationNew.EmployeeCode = data.EmployeeCode;
        $scope.leaveApplicationNew.EmpSystemID = data.SystemID;
        $scope.leaveApplicationNew.EmployeeName = data.EmployeeName;
        $scope.leaveApplicationNew.SectionId = data.SectionId;
        $scope.leaveApplicationNew.DOJ = data.DOJ;
        $scope.leaveApplicationNew.DOC = data.DOC;
        $scope.leaveApplicationNew.PolicyName = data.PolicyName;
        $scope.leaveApplicationNew.DesignationGroup = data.DesignationGroup;
        $scope.leaveApplicationNew.LegalDesignation = data.LegalDesignation;

        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;

        const start = new Date(data.DOJ);
        const today = new Date();

        let years = today.getFullYear() - start.getFullYear();
        let months = today.getMonth() - start.getMonth();
        let days = today.getDate() - start.getDate();

        // Adjust months and years if needed
        if (days < 0) {
            months -= 1;
            const previousMonth = new Date(today.getFullYear(), today.getMonth(), 0);
            days += previousMonth.getDate();
        }

        if (months < 0) {
            years -= 1;
            months += 12;
        }

        $scope.ServicePeriod = "" + years + " years " + months + " months " + days + " days";


        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.getData();
        $scope.leavetypecbo();
        $scope.getLeaveBalance();
        //$scope.LeavePolicyName();
    };


    $scope.SectionList = [];
    cboService.getSectionCbo(function (result) {
        $scope.SectionList = result;
    });

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
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

        var dateOut1 = new Date($scope.leaveApplicationNew.FromDate); // it will work if date1 is in ISO format
        var dateOut2 = new Date($scope.leaveApplicationNew.ToDate);

        var timeDiff = Math.abs(dateOut2.getTime() - dateOut1.getTime());
        var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24) + 1);
        //alert(diffDays);

        for (var i = 0; i < $scope.LeaveBalanceList.length; i++) {
            if ($scope.LeaveBalanceList[i].LTSystemID === $scope.leaveApplicationNew.LTSystemID) {

                if ($scope.leaveApplicationNew.LeaveDayType != 'FullDay') {
                    diffDays = 0.5;
                }
                //-----------------------------------validation---------------
                // if
                // ($scope.LeaveBalanceList[i].Balance < diffDays && $scope.LeaveBalanceList[i].IsExceptionAllowed == false) {//99
                //    throw 'Leave Duration is greater then balance.';
                //}
            }
        }

    }

    $scope.Save = function () {
        try {
            ValidationLeave();
            angular.copy($scope.leaveApplicationNew, $scope.leaveApplication);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.leaveApplicationNewForm.$valid) {

                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'leaveApplication': $scope.leaveApplication, 'yearId': $scope.YearNo },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.LeaveTransactionList.push(response.data.LeaveApplication);
                            $scope.getData();
                            $scope.getLeaveBalance();
                            $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
                            $scope.setEnable();
                            ClearDataField();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
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
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId, EmpSystemID: $scope.leaveApplication.EmpSystemID, EmployeeName: $scope.leaveApplication.EmployeeName, EmployeeCode: $scope.leaveApplication.EmployeeCode, DOJ: $scope.leaveApplication.DOJ, DOC: $scope.leaveApplication.DOC };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId, EmpSystemID: $scope.leaveApplicationNew.EmpSystemID, EmployeeName: $scope.leaveApplicationNew.EmployeeName, EmployeeCode: $scope.leaveApplicationNew.EmployeeCode, DOJ: $scope.leaveApplicationNew.DOJ, DOC: $scope.leaveApplicationNew.DOC };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.setEnable();
    }

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.LeaveBalanceList = [];
        $scope.LeaveTransactionList = [];
        $scope.imageSrc = virtualPath.EmployeePic + '';
    }


    $scope.LeaveAppReportExcel = function () {
        var reportFormat = "Excel";
        try {
            var url = $scope.path + 'LeaveAppReportExcelFormat?reportFormat=' + reportFormat + '&employeeId=' + $scope.leaveApplicationNew.EmpSystemID;
            $rootScope.report(url);
        }
        catch (e) {

        }
    }
}
