'use strict';
AttendanceDashboardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function AttendanceDashboardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Attendance Dashboard';
    $scope.path = 'Attendances/AttendanceDashboard/';
    $scope.hrDate = $filter('dateFiltering')(Date.now(), 'dd-MMMM-yyyy');
    //var y = document.getElementById("MainDiv");
    //$scope.clickdde2 = function () {
    //    if (y.style.display === "none") {
    //        y.style.display = "block";
    //        x.style.display = "none";
    //        z.style.display = "none";
    //    }
    //};

    window.chartColors = {
        red: 'rgba(240, 52, 52, .6)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgba(46, 204, 113,.6)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)'
    };

    $scope.ColList = [];
    $scope.dynamicAttendanseList = [];

    var row = {
        ColumnName: null,
        OnRoleEmployee: null,
        totalPresentEmployee: null,
        totalLateEmployee: null,
        totalAbsentEmployee: null,
        totalLeaveEmployee: null,
        totalLongAbsentismEmployee: null,
        ShiftNotAssignedEmployee: null,
        totalAttdnNotProcessedToday: null,
        totalWeekoffEmployee: null
    };
    function setDynamicDashboardList(list) {
        var row = {
            ColumnName: null,
            OnRoleEmployee: null,
            totalPresentEmployee: null,
            totalLateEmployee: null,
            totalAbsentEmployee: null,
            totalLeaveEmployee: null,
            totalLongAbsentismEmployee: null,
            ShiftNotAssignedEmployee: null,
            totalAttdnNotProcessedToday: null,
            totalWeekoffEmployee: null
        };
        //$scope.dynamicAttendanseList = [];
        $scope.totalWeekoffEmployee = 0;
        $scope.totalOthersShiftNotAssignedEmployee = 0;
        $scope.ShiftNotAssignAsofToday = 0;
        $scope.totalAttdnNotProcessedToday = 0;
        $scope.totalShiftNotAssignAsofToday = 0;
        $scope.OnRoleEmployee = 0;
        $scope.present = 0;
        $scope.late = 0;
        $scope.absent = 0;
        $scope.leave = 0;
        $scope.others = 0;
        $scope.totalEarlyOutEmployee = 0;
        $scope.totalLounchOutEmployee = 0;
        $scope.totalLateInEmployee = 0;


        var columnName = "";

        for (var i = 0; i < list.length; i++) {

            if (baseService.isUndefinedOrNull($scope.hrDrpDownModel.PODirectIndirectStatus) || $scope.hrDrpDownModel.PODirectIndirectStatus == "") {

                $scope.dynamicAttendanseList = list.filter(function (ls) {
                    return ls.IsDirect == "General";
                });
            }
            else if ($scope.hrDrpDownModel.PODirectIndirectStatus === "Indirect") {
                $scope.dynamicAttendanseList = list.filter(function (ls) {
                    return ls.IsDirect == "Indirect";
                });

            }
            else if ($scope.hrDrpDownModel.PODirectIndirectStatus === "Direct") {
                $scope.dynamicAttendanseList = list.filter(function (ls) {
                    return ls.IsDirect == "Direct";
                });

            }
        }
    }
    $scope.hrDrpDownModel = {
        EmplyeeTypeOrCategoryId: null,
        PODirectIndirectStatus: null
    };
    $scope.dFunction = function () {
        $scope.groupWiseAttnList = [];
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/DefaultAttnStatus/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setDynamicDashboardList(response.data);
            $scope.dynamicAttendanseList2 = response.data;
            $scope.index = -1;
            $scope.stIndex = $scope.index - 1;
        });

        $scope.overAllStatusList = [];
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/HROverAllStatusDefault/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.overAllStatusList = response.data;
            angular.forEach($scope.overAllStatusList, function (item, i) {
                $scope.late3 = item.late3;
                $scope.absent3 = item.absent3;
                $scope.probationOverDue = item.probationOverDue;
                $scope.probationToday = item.probationToday;
                $scope.probationNext7Days = item.probationNext7Days;
                $scope.separatedToday = item.separatedToday;
                $scope.separatedNext7Days = item.separatedNext7Days;
                $scope.resignationApprovalPending = item.resignationApprovalPending;
                $scope.todayResignationApply = item.todayResignationApply;
                $scope.incrementToday = item.incrementToday;
                $scope.incrementOverDue = item.incrementOverDue;
                $scope.incrementNext7Days = item.incrementNext7Days;
                $scope.incrementNext30Days = item.incrementNext30Days;
            });
        });

        $http({
            method: 'POST',
            url: 'ManpowerBudgetDashboard/GetGroupWiseCompanyList/',
            params: {
                'date': $scope.hrDate,
                'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setMPList(response.data);
            $scope.MPOnRoleBudgetList = response.data;
            createMPChart();
        });//Manpower Budget

        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/JoiningStatusDaily/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            createJoiningAndSeparationLineChart(response.data);
        });//joiningAndSepartationStatus
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/AbsentismStatusDaily/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            createAbsentLineChart(response.data);
        });
        $scope.totalCAEmp = 0;
        $scope.totalCLEmp = 0;
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/ConsecutiveAbsentStats/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ConsecutiveAbsentStatsList = response.data;
            $scope.totalCAEmp = response.data.length;
        });
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/ConsecutiveLateStats/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ConsecutiveLateStatsList = response.data;
            $scope.totalCLEmp = response.data.length;
        });

        $http({
            method: 'GET',
            url: 'HRDashboard/HRLongAbsentismDefault/',
            params: {
                'hrDate': $scope.hrDate,
                'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.numLongAbsent = response.data.length;
            $scope.longAbsentList = response.data;
        });

    };
    $scope.dFunction();
    $scope.GetDrillDownAttnStatus = function (data) {
        var getRow = $filter("filter")($scope.ColList, { "ColumnName": "Company" });
        createColListWithCompany(getRow[0].Id);
    };
    function getDrillDownList(companyId) {

        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/OrgStructureListColList?CompanyId=' + companyId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    var row = {
                        Sequence: -2,
                        Id: null,
                        StandardName: null,
                        ColumnName: null,
                        RType: null,
                        Text: null,
                        Name: null,
                        date: ''
                    };
                    row.Sequence = i;
                    row.StandardName = response.data[i].StandardName;
                    row.ColumnName = response.data[i].ColumnName;
                    row.RType = response.data[i].RType;
                    row.Text = response.data[i].UId;
                    row.date = $scope.date;
                    $scope.ColList.push(row);
                }

            }

        });


        $scope.strColList = $scope.ColList;

    }
    $scope.headerNav = function (x) {
        if (x.Sequence !== -2) {
            $scope.setIndexHead(x);
            $scope.GetDrillDownAttnStatus(x.Id);
        }
        else {
            $scope.setIndexHead(x);
            $http({
                method: 'GET',
                url: 'Employees/HRDashboard/DefaultAttnStatus',
                params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                setDynamicDashboardList(response.data);
                $scope.dynamicAttendanseList2 = response.data;
                $scope.index = -1;
                $scope.stIndex = $scope.index - 1;
            });
            $scope.overAllStatusList = [];
            $http({
                method: 'GET',
                url: 'Employees/HRDashboard/HROverAllStatusDefault/',
                params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.overAllStatusList = response.data;
                angular.forEach($scope.overAllStatusList, function (item, i) {
                    $scope.late3 = item.late3;
                    $scope.absent3 = item.absent3;
                    $scope.probationOverDue = item.probationOverDue;
                    $scope.probationToday = item.probationToday;
                    $scope.probationNext7Days = item.probationNext7Days;
                    $scope.separatedToday = item.separatedToday;
                    $scope.separatedNext7Days = item.separatedNext7Days;
                    $scope.resignationApprovalPending = item.resignationApprovalPending;
                    $scope.todayResignationApply = item.todayResignationApply;
                    $scope.incrementToday = item.incrementToday;
                    $scope.incrementOverDue = item.incrementOverDue;
                    $scope.incrementNext7Days = item.incrementNext7Days;
                    $scope.incrementNext30Days = item.incrementNext30Days;


                });
            });
            $http({
                method: 'POST',
                url: 'ManpowerBudgetDashboard/GetGroupWiseCompanyList/',
                params: {
                    'date': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                setMPList(response.data);
                createMPChart();
            });
            $http({
                method: 'GET',
                url: 'Employees/HRDashboard/JoiningStatusDaily/',
                params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                createJoiningAndSeparationLineChart(response.data);
            });//joiningAndSepartationStatus

            $http({
                method: 'GET',
                url: 'Employees/HRDashboard/AbsentismStatusDaily/',
                params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                createAbsentLineChart(response.data);
            });
        }
    };
    $scope.GetModalOthersDetailJS = function () {
        var eDialog = $("#OthersSummaryStatus").data("ejDialog");
        eDialog.open();
    };
    $scope.GetModalHROnRoleStatusDetailList = function (pageno, data) {
        $scope.desigSummaryList = [];
        $scope.searchbyonRoleEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of OnRole Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Employees/HRDashboard/ModalOnRoleEmployeeList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.onRoleEmpList = response.data;

                if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyonRoleEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyonRoleEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyonRoleEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#onRoleEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    width: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchonRole,
                    templateRefresh: $scope.actionCompleteSearchonRole,
                    isResponsive: true

                });
                $scope.dataGrid = "#onRoleEmpGrid";
                //$scope.actionCompleteSearchPresent();

                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpOnRole").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActOnRole").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

            }
            var eDialog = $("#empOnRole").data("ejDialog");
            eDialog.open();
            //angular.element(document.querySelector('#empOnRole')).modal('show');
        });
    };
    function createAttendanceExtraInfoChart() {

        var AEBctx = document.getElementById("attendanceInfoExtraBarChart").getContext('2d');
        if (AEBarChart !== undefined && typeof AEBarChart === 'object' && typeof AEBarChart.destroy === 'function') AEBarChart.destroy();
        AEBarChart = new Chart(AEBctx, {
            type: 'bar',
            data: {
                labels: ['Late In', 'Lounch Out', 'Early Out'],
                datasets: [{
                    data: [$scope.totalLateInEmployee, $scope.totalLounchOutEmployee, $scope.totalEarlyOutEmployee],
                    backgroundColor: ['rgba(44, 130, 201, 1)', 'rgba(245, 229, 27, 1)', 'rgba(240, 59, 65, .6)'],
                    borderColor: ['rgba(44, 130, 201, 1)', 'rgba(245, 229, 27, 1)', 'rgba(240, 52, 65, .6)'],
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                legend: {
                    onClick: (e) => e.stopPropagation()
                },
                title: {
                    display: true,
                    text: ''
                },
                label: true,
                hover: { mode: null },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var value = data.datasets[0].data[tooltipItem.index];
                            value = value.toString();
                            value = value.split(/(?=(?:...)*$)/);
                            value = value.join(',');
                            return value;
                        }
                    }
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                value = value.toString();
                                value = value.split(/(?=(?:...)*$)/);
                                value = value.join(',');
                                return value;
                            }
                        }
                    }],
                    xAxes: [{
                        ticks: {
                        },
                        barThickness: 60
                    }]
                }
            }
        });
    }
}