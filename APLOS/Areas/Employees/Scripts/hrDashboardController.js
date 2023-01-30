'use strict';
hrDashboardController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$cookies', '$window'];
function hrDashboardController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $cookies, $window) {
    var x = document.getElementById("myDIV");
    var y = document.getElementById("MainDiv");
    var z = document.getElementById("secDiv");
    x.style.display = "none";
    y.style.display = "block";
    z.style.display = "none";
    $scope.numLongAbsent = 0;
    $scope.longAbsentList = null;
    $scope.dataGrid = null;
    $scope.currentColIndex = 0;
    $scope.totalEarlyOutEmployee = 0;
    $scope.totalLounchOutEmployee = 0;
    $scope.totalLateInEmployee = 0;
    var noOfdrilDownClick = 0;

    $scope.MPOnRoleBudgetList = [];

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.clickdde = function () {
        if (x.style.display === "none") {
            y.style.display = "none";
            x.style.display = "block";
            z.style.display = "none";
        }
    };
    $scope.clickdde2 = function () {
        if (y.style.display === "none") {
            y.style.display = "block";
            x.style.display = "none";
            z.style.display = "none";
        }
    };
    $scope.clickdde3 = function () {
        if (z.style.display === "none") {
            z.style.display = "block";
            x.style.display = "none";
            y.style.display = "none";
            $scope.SimulateVisual();
        }
    };
    $scope.strColList = [];

    $scope.rsPlantId = null;
    $scope.rspCompanyId = null;

    $scope.hrDrpDownModel = {
        EmplyeeTypeOrCategoryId: null,
        PODirectIndirectStatus: null
    };

    $scope.index = -1;
    $scope.ColList = [];
    $scope.dynamicAttendanseList = [];
    $scope.stIndex = -2;
    var ManPowerbarChart;
    var ATTNPieChart;
    var JSchart;
    var LSchart;
    var ASchart;
    var AEBarChart;
    $scope.hrDate = $filter('dateFiltering')(Date.now(), 'dd-MMMM-yyyy');

    $scope.late3 = 0;
    $scope.absent3 = 0;

    $scope.chartAttdnList = [];
    function makeItZero() {
        $scope.probationOverDue = 0;
        $scope.probationToday = 0;
        $scope.probationNext7Days = 0;
        $scope.separatedToday = 0;
        $scope.separatedNext7Days = 0;
        $scope.resignationApprovalPending = 0;
        $scope.todayResignationApply = 0;
        $scope.incrementToday = 0;
        $scope.incrementOverDue = 0;
        $scope.incrementNext7Days = 0;
        $scope.incrementNext30Days = 0;//item.incrementNext30Days;
    }

    $scope.leaveStatus = [];
    $scope.leaveDate = [];

    window.chartColors = {
        red: 'rgba(240, 52, 52, .6)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgba(46, 204, 113,.6)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)'
    };
    $scope.chartAttdnLabel = ['Present', 'Late', 'Absent', 'Leave', 'Others'];

    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.docEmployeeCategoryList = result;
    });

    function createAttnPieChart() {
        Chart.defaults.global.legend.display = false;
        var ATTNctx = document.getElementById("attnPieChart").getContext('2d');

        if (ATTNPieChart !== undefined && typeof ATTNPieChart === 'object' && typeof ATTNPieChart.destroy === 'function') ATTNPieChart.destroy();
        ATTNPieChart = new Chart(ATTNctx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartAttdnLabel,
                datasets: [{
                    label: '',
                    data: $scope.chartAttdnList,
                    backgroundColor: [
                        'rgba(46, 204, 113,0.7)',
                        'rgba(241, 196, 15, 0.7)',
                        'rgba(231, 76, 60,0.7)',
                        'rgba(82, 179, 217, 0.7)',
                        'rgba(253, 227, 167, 0.7)'
                    ],
                    borderColor: [
                        'rgba(46, 204, 113,1.0)',
                        'rgba(241, 196, 15, 1.0)',
                        'rgba(231, 76, 60,1.0)',
                        'rgba(82, 179, 217, 1.0)',
                        'rgba(253, 227, 167, 1.0)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                legend: {
                    display: false,
                    position: 'bottom'
                },
                title: {
                    display: true,
                    position: 'bottom'
                },
                hover: { mode: null },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var dataset = data.datasets[tooltipItem.datasetIndex];
                            var total = dataset.data.reduce(function (previousValue, currentValue, currentIndex, array) {
                                return previousValue + currentValue;
                            });
                            var currentValue = dataset.data[tooltipItem.index];
                            var precentage = ((currentValue / total * 100) + 0.0).toFixed(2);
                            return precentage + "%";
                        },
                        title: function (tooltipItem, data) {
                            return $scope.chartAttdnLabel[tooltipItem[0].index];
                        }
                    }
                }
            }
        });
    }

    $scope.GetGruopWiseConsecutiveAbsentLateStatus = function () {
        $scope.totalCAEmp = 0;
        $scope.totalCLEmp = 0;
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/ConsecutiveAbsentStats/',
            params: { 'hrDate': $scope.hrDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ConsecutiveAbsentStatsList = response.data;
            $scope.totalCAEmp = response.data.length;
        });
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/ConsecutiveLateStats/',
            params: { 'hrDate': $scope.hrDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ConsecutiveLateStatsList = response.data;
            $scope.totalCLEmp = response.data.length;
        });
    };
    $scope.GetGruopWiseConsecutiveAbsentLateStatus();

    $scope.DOJ = [];
    $scope.TEDOJ = [];
    $scope.DOS = [];
    $scope.TEDOS = [];
    $scope.JoiningStatusList = [];
    $scope.SeparationStatusList = [];

    function createLeaveStatusList(data) {

        angular.forEach(data, function (item, i) {
            $scope.leaveStatus.push(item.totalLeave);
            $scope.leaveDate.push(item.WorkDate);
            //});

            var LSctx = document.getElementById("lsLineChart").getContext('2d');
            if (LSchart !== undefined && typeof LSchart === 'object' && typeof LSchart.destroy === 'function') LSchart.destroy();
            LSchart = new Chart(LSctx, {
                type: 'line',
                data: {
                    labels: $scope.leaveDate,
                    datasets: [{
                        label: 'Leave taken',
                        data: $scope.leaveStatus,
                        backgroundColor: 'rgba(46, 204, 113,.6)',
                        borderColor: 'rgba(46, 204, 113,1)',
                        fill: false,
                        borderWidth: 2
                    }
                    ]
                },
                options: {
                    legend: {
                        onClick: (e) => e.stopPropagation()
                    },
                    title: {
                        display: true,
                        text: 'Future Leave trend',
                        position: 'bottom'
                    },

                    hover: { mode: null },
                    tooltips: {
                        mode: 'index',
                        intersect: false
                    },
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true
                            }
                        }],
                        xAxes: [{
                            ticks: {
                                beginAtZero: true,
                                autoSkip: false,
                                maxRotation: 75,
                                minRotation: 75
                            }
                        }]
                    },
                    elements: {
                        line: {
                        }
                    }
                }
            });
        });
    }


    function createJoiningAndSeparationLineChart(data) {
        $scope.JoiningStatusList = data;
        $scope.DOS = [];
        $scope.TEDOS = [];
        $scope.TEDOJ = [];

        angular.forEach($scope.JoiningStatusList, function (item, i) {
            $scope.DOS.push(item.DO);
            $scope.TEDOJ.push(item.TEDOJ);
            $scope.TEDOS.push(item.TEDOS);
        });
        var JSctx = document.getElementById("jsLineChart").getContext('2d');
        if (JSchart !== undefined && typeof JSchart === 'object' && typeof JSchart.destroy === 'function') JSchart.destroy();
        JSchart = new Chart(JSctx, {
            type: 'line',
            data: {
                labels: $scope.DOS,
                datasets: [{
                    label: 'Joined',
                    data: $scope.TEDOJ,
                    backgroundColor: 'rgba(46, 204, 113,.6)',
                    borderColor: 'rgba(46, 204, 113,1)',
                    fill: false,
                    borderWidth: 2
                },
                {
                    label: 'Separated',
                    data: $scope.TEDOS,
                    backgroundColor: 'rgba(240, 52, 52, 0.6)',
                    borderColor: 'rgba(240, 52, 52, 1)',
                    fill: false,
                    borderWidth: 2
                }
                ]
            },
            options: {
                legend: {
                    display: true
                },
                title: {
                    display: true,
                    text: 'Daily Joining/Separation Status',
                    position: 'bottom'
                },

                hover: { mode: null },
                tooltips: {
                    mode: 'index',
                    intersect: false
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                        }
                    }],
                    xAxes: [{
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 75,
                            minRotation: 7
                        }
                    }]
                },
                elements: {
                    line: {
                        //tension: 0
                    }
                }
            }
        });
    }


    function createAbsentLineChart(data) {
        $scope.AbsentStatusList = data;
        $scope.workDate = [];
        $scope.totalAbsent = [];
        angular.forEach($scope.AbsentStatusList, function (item, i) {
            $scope.workDate.push(item.WorkDate);
            $scope.totalAbsent.push(item.totalAbsent);

        });
        var ASctx = document.getElementById("absentLineChart").getContext('2d');
        if (ASchart !== undefined && typeof ASchart === 'object' && typeof ASchart.destroy === 'function') ASchart.destroy();
        ASchart = new Chart(ASctx, {
            type: 'line',
            data: {
                labels: $scope.workDate,
                datasets: [{
                    label: 'Absent',
                    data: $scope.totalAbsent,
                    backgroundColor: 'rgba(240, 52, 52, 0.6)',
                    borderColor: 'rgba(240, 52, 52, 1)',
                    fill: false,
                    borderWidth: 2
                }
                ]
            },
            options: {
                legend: {
                    display: true
                },
                title: {
                    display: true,
                    text: 'Absent Trend (Last 30 days)',
                    position: 'bottom'
                },

                hover: { mode: null },
                tooltips: {
                    mode: 'index',
                    intersect: false
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                        }
                    }],
                    xAxes: [{
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 75,
                            minRotation: 75
                        }
                    }]
                },
                elements: {
                    line: {
                        //tension: 0
                    }
                }
            }
        });
    }

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


    //-----------------------------*Dynamic Dashboard Start*-------------------------------//

    $scope.EmpCategory = function (EmplyeeTypeOrCategoryId) {
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
            method: 'POST',
            url: 'Employees/HRDashboard/LeaveStatus/',
            data: {
                'hrDate': $scope.hrDate,
                'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            createLeaveStatusList(response.data);
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

    $scope.PODirectIndirectStatusChange = function () {
        $scope.groupWiseAttnList = [];
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/DefaultAttnStatus/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'PODirectIndirectStatus': $scope.hrDrpDownModel.PODirectIndirectStatus },
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
            method: 'POST',
            url: 'Employees/HRDashboard/LeaveStatus/',
            data: {
                'hrDate': $scope.hrDate,
                'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            createLeaveStatusList(response.data);
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

    $scope.GetGruopWiseAttnStatus = function () {
        $scope.groupWiseAttnList = [];
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/DefaultAttnStatus/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setDynamicDashboardList(response.data);
            $scope.dynamicAttendanseList2 = response.data;
            createColList();
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
        });
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/AbsentismStatusDaily/',
            params: { 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            createAbsentLineChart(response.data);
        });
        $http({
            method: 'POST',
            url: 'Employees/HRDashboard/LeaveStatus/',
            data: { 'hrDate': $scope.hrDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            createLeaveStatusList(response.data);
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
    $scope.GetGruopWiseAttnStatus();


    $scope.totalCAEmp = 0;
    $scope.totalCLEmp = 0;


    $scope.GetDrillDownAttnStatus = function (data) {
        var getRow = $filter("filter")($scope.ColList, { "ColumnName": "Company" });
        createColListWithCompany(getRow[0].Id);
    };
    var companyId = "";
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


    function getDrillDownListWithCompany(companyId) {
        $http({
            method: 'GET',
            url: 'Employees/HRDashboard/OrgStructureListColList?CompanyId=' + companyId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (noOfdrilDownClick == 1) {
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
            }


            if ($scope.index + 3 < $scope.ColList.length) {
                $http({
                    method: 'POST',
                    url: 'Employees/HRDashboard/DrillDownAttnStatus/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    setDynamicDashboardList(response.data);
                    $scope.dynamicAttendanseList2 = response.data;
                    $scope.index += 1;
                    $scope.stIndex = $scope.index - 1;
                });

                $http({
                    method: 'POST',
                    url: 'Employees/HRDashboard/HROverAllStatusDynamic/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
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
                    url: 'ManpowerBudgetDashboard/GetDetailDrillDownTable/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'date': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.DDList = response.data;
                    setMPList(response.data);
                    $scope.MPOnRoleBudgetList = response.data;
                    createMPChart();
                });
                $http({
                    method: 'POST',
                    url: 'Employees/HRDashboard/ConsecutiveAbsentStatsDynamic/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.totalCAEmp = response.data.length;
                });
                $http({
                    method: 'POST',
                    url: 'Employees/HRDashboard/ConsecutiveLateStatsDynamic/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.totalCLEmp = response.data.length;
                });

                $http({
                    method: 'POST',
                    url: 'Employees/HRDashboard/DynamicJoiningOrSeparationStatusDaily/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    createJoiningAndSeparationLineChart(response.data);
                });
                $http({
                    method: 'POST',
                    url: 'Employees/HRDashboard/DynamicAbsentismStatusDaily/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    createAbsentLineChart(response.data);
                });
                $http({
                    method: 'POST',
                    url: 'Employees/HRDashboard/DynamicLeaveStatus/',
                    data: {
                        'ChartColumnList': $scope.ColList,
                        'seq': $scope.index,
                        'hrDate': $scope.hrDate,
                        'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    createLeaveStatusList(response.data);
                });
            }

        });


        $scope.strColList = $scope.ColList;

    }

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
    $scope.isDirectChange = function () {
        $scope.dynamicAttendanseList = $scope.dynamicAttendanseList2;
        setMPList($scope.MPOnRoleBudgetList); //= response.data;

        setDynamicDashboardList($scope.dynamicAttendanseList2);
    };

    // $scope.isDirectChange();


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




        angular.forEach($scope.dynamicAttendanseList, function (item, i) {
            $scope.present += item.totalPresentEmployee;
            $scope.late += item.totalLateEmployee;
            $scope.absent += item.totalAbsentEmployee;
            $scope.leave += item.totalLeaveEmployee;
            $scope.totalOthersShiftNotAssignedEmployee += item.ShiftNotAssignedEmployee;
            $scope.totalShiftNotAssignAsofToday += item.totalShiftNotAssignAsofToday;
            $scope.totalWeekoffEmployee += item.totalWeekoffEmployee;
            $scope.totalAttdnNotProcessedToday += item.totalAttdnNotProcessedToday;

            $scope.totalEarlyOutEmployee += item.totalEarlyOutEmployee;
            $scope.totalLounchOutEmployee += item.totalLounchOutEmployee;
            $scope.totalLateInEmployee += item.totalLateInEmployee;

            $scope.OnRoleEmployee += item.OnRoleEmployee;
        });
        $scope.othersSummary = $scope.totalOthersShiftNotAssignedEmployee + $scope.totalAttdnNotProcessedToday + $scope.totalWeekoffEmployee;

        $scope.chartAttdnList = [$scope.present, $scope.late, $scope.absent, $scope.leave, $scope.othersSummary];

        createAttnPieChart();
        createAttendanceExtraInfoChart();
    }

    function createColList() {
        noOfdrilDownClick = 0;
        //  $scope.ColList = [];
        if (baseService.arrayLength($scope.ExpenseList) >= 0) {
            var row = {
                Sequence: null,
                Id: null,
                StandardName: null,
                ColumnName: null,
                RType: null,
                Text: null,
                Name: null,
                date: ''
            };
            row.Sequence = -2;
            row.Id = $scope.dynamicAttendanseList[0].CompanyGroupId;
            row.StandardName = "Group";
            row.ColumnName = "Group";
            row.Text = $scope.dynamicAttendanseList[0].GroupName;
            row.Name = $scope.dynamicAttendanseList[0].GroupName;
            row.date = $scope.date;

            $scope.ColList.push(row);
            var rowc = {
                Sequence: null,
                Id: null,
                StandardName: null,
                ColumnName: null,
                RType: null,
                Text: null,
                Name: null,
                date: ''
            };
            rowc.Sequence = -1;
            rowc.Id = $scope.dynamicAttendanseList[0].CompanyId;
            rowc.StandardName = "Company";
            rowc.ColumnName = "Company";
            rowc.Text = $scope.dynamicAttendanseList[0].UserName;
            rowc.Name = $scope.dynamicAttendanseList[0].UserName;
            rowc.date = $scope.date;
            $scope.ColList.push(rowc);
            getDrillDownList();
        }
    }



    function createColListWithCompany(companyId) {
        noOfdrilDownClick++;
        if (noOfdrilDownClick == 1) {
            $scope.ColList = [];
            if (baseService.arrayLength($scope.ExpenseList) >= 0) {
                var row = {
                    Sequence: null,
                    Id: null,
                    StandardName: null,
                    ColumnName: null,
                    RType: null,
                    Text: null,
                    Name: null,
                    date: ''
                };
                row.Sequence = -2;
                row.Id = $scope.dynamicAttendanseList[0].CompanyGroupId;
                row.StandardName = "Group";
                row.ColumnName = "Group";
                row.Text = $scope.dynamicAttendanseList[0].GroupName;
                row.Name = $scope.dynamicAttendanseList[0].GroupName;
                row.date = $scope.date;

                $scope.ColList.push(row);
                var rowc = {
                    Sequence: null,
                    Id: null,
                    StandardName: null,
                    ColumnName: null,
                    RType: null,
                    Text: null,
                    Name: null,
                    date: ''
                };
                rowc.Sequence = -1;
                rowc.Id = $scope.dynamicAttendanseList[0].CompanyId;
                rowc.StandardName = "Company";
                rowc.ColumnName = "Company";
                rowc.Text = $scope.dynamicAttendanseList[0].UserName;
                rowc.Name = $scope.dynamicAttendanseList[0].UserName;
                rowc.date = $scope.date;
                $scope.ColList.push(rowc);
            }

        }
        getDrillDownListWithCompany(companyId);

    }

    $scope.setIndex = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
                $scope.ColList[i].Name = x.ColumnName;
            }
        }
    };
    function getCol(seq) {
        for (var i = 0; i < baseService.arrayLength($scope.ColList); i++) {
            if ($scope.ColList[i].Sequence === seq) {
                return $scope.ColList[i].ColumnName;
            }
        }
    }
    $scope.setIndexHead = function (x) {
        $scope.index = x.Sequence;
    };
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

    function setMPList(list) {

        for (var i = 0; i < list.length; i++) {

            if (baseService.isUndefinedOrNull($scope.hrDrpDownModel.PODirectIndirectStatus) || $scope.hrDrpDownModel.PODirectIndirectStatus == "") {

                list = list.filter(function (ls) {
                    return ls.IsDirect == "General";
                });
            }
            else if ($scope.hrDrpDownModel.PODirectIndirectStatus === "Indirect") {
                list = list.filter(function (ls) {
                    return ls.IsDirect == "Indirect";
                });

            }
            else if ($scope.hrDrpDownModel.PODirectIndirectStatus === "Direct") {
                list = list.filter(function (ls) {
                    return ls.IsDirect == "Direct";
                });

            }
        }


        $scope.date = new Date();
        $scope.chartAttdnLabelSal = [];
        var CurrentTotalEmp = 0;
        var proposedTotalEmp = 0;
        var Short = 0;
        var excess = 0;
        var unallocated = 0;

        var OnRoleSal = 0;
        var BudgetedSal = 0;

        $scope.list = list;
        angular.forEach(list, function (item, i) {
            CurrentTotalEmp += item.TotalManpower;
            proposedTotalEmp += item.ProposedManpowerBudget;
            Short += item.Short;
            excess += item.Excess;
            unallocated += item.Unallocated;
            OnRoleSal += item.OnRoleSalaryC;
            BudgetedSal += item.ProposedSalaryC;
        });
        $scope.currentTotalEmp = CurrentTotalEmp;
        $scope.proposedTotalEmp = proposedTotalEmp;
        $scope.Short = Short;
        $scope.excess = excess;
        $scope.unallocated = unallocated;

        $scope.chartMPList = [];
        $scope.chartMPList.push(proposedTotalEmp);
        $scope.chartMPList.push(CurrentTotalEmp);

        $scope.chartDataSalary = [];
        $scope.chartDataSalary.push(BudgetedSal);
        $scope.chartDataSalary.push(OnRoleSal);

        $scope.chartMPLabel = ['Budgeted', 'On Role'];
        $scope.chartAttdnLabelSal = ['Budgeted Salary', 'On Role Salary'];
    }
    function createMPChart() {
        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("ManPowerbarChart").getContext('2d');
        if (ManPowerbarChart !== undefined && typeof ManPowerbarChart === 'object' && typeof ManPowerbarChart.destroy === 'function') ManPowerbarChart.destroy();
        ManPowerbarChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.chartMPLabel,
                datasets: [{
                    data: $scope.chartMPList,
                    backgroundColor: ['rgba(46, 204, 113,.6)', 'rgba(240, 52, 52, .6)'],
                    borderColor: ['rgba(46, 204, 113,.8)', 'rgba(240, 52, 52, .8)'],
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
                    text: 'Manpower Forecast'
                },
                label: false,
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
    $scope.GetChartColList = function () {
        $scope.ADGLUrl = 'Employees/HRDashboard/GetchartColumnList';

        $http({
            method: 'POST',
            url: $scope.ADGLUrl,
            data: { 'ChartColumnList': $scope.ColList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
        });
    };
    $scope.GetChartColList();
    //-----------------------------*Dynamic Dashboard End*------------------------------//

    //----------------------------*Dynamic Modal Start*---------------------------------------//
    $scope.ModalIncrementDue = function (data) {
        $scope.incDueEmplist = [];
        $scope.searchbyincDueEmplist = [];
        $http({
            method: 'POST',
            url: 'Employees/HRDashboard/ListOfIncrementDue/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'condition': data, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.length > 0) {
                $scope.incDueEmplist = response.data;

                if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyincDueEmplist);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyincDueEmplist.length; i++) {
                    fieldList.push({ field: $scope.searchbyincDueEmplist[i].Value, visible: true, width: "180px" });
                }

                $('#incrementGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    width: '100%',
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchonIncrement,
                    templateRefresh: $scope.actionCompleteSearchonIncrement,
                    isResponsive: true

                });
                $scope.dataGrid = "#incrementGrid";
                //$scope.actionCompleteSearchPresent();
                var eDialog = $("#incrementDueModal").data("ejDialog");
                eDialog.open();
            }

        });
    };

    $scope.actionCompleteSearchonIncrement = function (args) {
        if (args.requestType == "refresh") {

            try {
                var gridObj = $("#incrementGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };

    $scope.actionCompleteSearchProbation = function () {

        try {
            var gridObj = $("#probationEmpGrid").ejGrid("instance");
            var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
            //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
            gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
            gridObj.windowonresize();
        } catch (e) {
            throw (e);
        }
    };
    $scope.ModalProbation = function (data) {
        angular.element(document.querySelector('#probationModal')).modal('show');

        $scope.probDueEmplist = [];
        $scope.searchbyDetaillist = [];
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'condition': data, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId };
        $scope.status = "List of Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Employees/HRDashboard/ListProbationOverDue/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.probDueEmplist = response.data;

                if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyDetaillist);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyDetaillist.length; i++) {
                    fieldList.push({ field: $scope.searchbyDetaillist[i].Value, visible: true, width: "180px" });
                }

                try {
                    var gridObj = $("#probationEmpGrid").data("ejGrid");

                    if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();

                } catch (e) {

                }
                $('#probationEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    width: '100%',
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchProbation,
                    templateRefresh: $scope.actionCompleteSearchProbation,
                    isResponsive: true
                });
                $scope.dataGrid = "#probationEmpGrid";
                $scope.actionCompleteSearchProbation();
            }
        });
    };
    $scope.ModalResignation = function (data) {
        $scope.rsgEmplist = [];
        $http({
            method: 'POST',
            url: 'Employees/HRDashboard/ListSeparationStatus/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'condition': data, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.rsgEmplist = response.data;
            angular.element(document.querySelector('#resigModal')).modal('show');
        });
    };

    $scope.ModalConsecutiveLate = function () {
        $scope.lateEmplist = [];
        $http({
            method: 'POST',
            url: 'Employees/HRDashboard/ModalConsecutiveLateStats/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.lateEmplist = response.data;
            angular.element(document.querySelector('#consecutivelateModal')).modal('show');
        });
    };

    $scope.ModalConsecutiveAbsent = function () {
        $scope.absentEmplist = [];
        $http({
            method: 'POST',
            url: 'Employees/HRDashboard/ModalConsecutiveAbsentStats/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.absentEmplist = response.data;
            angular.element(document.querySelector('#consecutiveAbsentModal')).modal('show');
        });
    };

    $scope.searchByListOnRole = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        }
    ];
    $scope.HROnRoleDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeName',
        searchBy: 'EmployeeCode',
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };
    $scope.desigSummaryList = [];


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
    $scope.actionCompleteSearchonRole = function (args) {
        if (args.requestType == "refresh") {

            try {
                var gridObj = $("#onRoleEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }

        }

    };
    function sort_by_key(array, key) {
        return array.sort(function (a, b) {
            var x = a[key]; var y = b[key];
            return ((x < y) ? -1 : ((x > y) ? 1 : 0));
        });
    }
    $scope.recorddoubleclick = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        $scope.zoneNew = gridObj.getSelectedRecords()[0];
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.actionCompleteSearchPresent = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#presentEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };

    $scope.GetModalHRDailyPresentStatusDetailList = function (pageno, data) {
        $scope.searchbyPresentEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Present Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyPresentStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyPresentEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyPresentEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyPresentEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyPresentEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#presentEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    width: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchPresent,
                    templateRefresh: $scope.actionCompleteSearchPresent,
                    isResponsive: true

                });
                $scope.dataGrid = "#presentEmpGrid";
                //$scope.actionCompleteSearchPresent();

                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrp").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActPresent").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

            }
            var eDialog = $("#empPrsntStatus").data("ejDialog");
            eDialog.open();
        });
    };
    function CreateSummaryList(list, idColumn, descColumn) {
        var SummaryList = [];

        var sortedData = sort_by_key(list, idColumn);

        var tempid = '';

        var count = 0;
        for (var id = 0; id < sortedData.length; id++) {
            if (sortedData[id][idColumn] != tempid) {
                if (SummaryList.length > 0)
                    SummaryList[SummaryList.length - 1].Total = count;

                SummaryList.push({ idColumn: sortedData[id][idColumn], descColumn: sortedData[id][descColumn], Total: 0 });

                count = 0;
            }
            count++;

            tempid = sortedData[id][idColumn];
        }
        if (SummaryList.length > 0)
            SummaryList[SummaryList.length - 1].Total = count;

        return SummaryList;
    }

    $scope.PrintAbsent = function () {
      
        var dataList = [];
        var g = $("#GridAbsent").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.DateWiseAbsentStatusList;
        }

        $scope.fileName = 'Employee';

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': dataList, 'reportFileName': $scope.fileName}
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };

    $scope.PrintLate = function () {

        var dataList = [];
        var g = $("#GridLate").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.DateWiseLateStatusList;
        }

        $scope.fileName = 'Employee';

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': dataList, 'reportFileName': $scope.fileName }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };

    $scope.PrintJoin = function () {

        var dataList = [];
        var g = $("#GridJoin").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.DateWiseJoiningStatusList;
        }

        $scope.fileName = 'Employee';

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': dataList, 'reportFileName': $scope.fileName }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };

    $scope.PrintSeparated = function () {

        var dataList = [];
        var g = $("#GridSeparated").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.DateWiseSeparationList;
        }

        $scope.fileName = 'Employee';

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': dataList, 'reportFileName': $scope.fileName }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };

    $scope.PrintIncGR = function () {
        var gridObj = $($scope.dataGrid).data("ejGrid");
        var data = gridObj.model.dataSource;
        data = ej.DataManager(data).executeLocal(ej.Query().select(["EmployeeName", "EmployeeCode", "Designation", "DOJ", "IncrementEffectiveDate", "IncDaysToGO", "IncrementNextDueDate", "OperationCode", "CellPhnNo"]));

        $scope.fileName="List of Employees"
        //data = ej.DataManager(data).executeLocal(ej.Query().select(["Department", "Designation", "EmployeeName", "EmployeeCode"]));
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data, 'reportFileName': $scope.fileName}
            // dataType: 'JSON'
            //, contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };
    $scope.PrintGRDes = function () {
        var gridObj = $($scope.dataGrid).data("ejGrid");
        var data = gridObj.model.dataSource;
        data = ej.DataManager(data).executeLocal(ej.Query().select(["EmployeeName", "EmployeeCode", "Shift", "Designation", "EmpCategory", "DOJ", "OperationActivityName", "OperationMasterName", "OperationCode", "CompanyName", "Plant", "Department", "Line", "CellPhnNo"]));

        var reportFileName = "List of Employees"
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data, 'reportFileName': reportFileName  }
            // dataType: 'JSON'
            //, contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');
            }
            else {
                window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };
    $scope.actionCompleteSearchAbsent = function (args) {
        if (args.requestType == "refresh") {

            try {
                var gridObj = $("#absentEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };
    $scope.AbsentList = [];
    $scope.GetModalHRDailyAbsentStatusDetailList = function (pageno, data) {
        $scope.searchbyAbsentEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Absent Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyAbsentStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            $scope.AbsentList = response.data;
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyAbsentEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyAbsentEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyAbsentEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyAbsentEmpList[i].Value, visible: true, width: "180px" });
                }
                $('#absentEmpGrid').ejGrid({
                    dataSource: $scope.AbsentList,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchAbsent,
                    templateRefresh: $scope.actionCompleteSearchAbsent,
                    isResponsive: true
                });
                $scope.dataGrid = "#absentEmpGrid";

                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpAbsent").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActAbsent").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationMasterId", "OperationMasterName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var eDialog = $("#empAbsentStatus").data("ejDialog");
                eDialog.open();
            }
        });
    };

    $scope.actionCompleteSearchLongAbsentism = function (args) {
        if (args.requestType == "refresh") {

            try {
                var gridObj = $("#longAbsentismEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };
    $scope.longAbsentismList = [];

    $scope.GetModalHRLongAbsentismDetailList = function (pageno, data) {
        $scope.searchbyAbsentEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Absent Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalLongAbsenteismStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            $scope.longAbsentismList = response.data;
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyAbsentEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyAbsentEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyAbsentEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyAbsentEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#longAbsentismEmpGrid').ejGrid({
                    dataSource: $scope.longAbsentismList,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLongAbsentism,
                    templateRefresh: $scope.actionCompleteSearchLongAbsentism,
                    isResponsive: true
                });
                $scope.dataGrid = "#longAbsentismEmpGrid";
                //$scope.actionCompleteSearchLongAbsentism();

                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLongAbsent").ejGrid({
                    dataSource: CreateSummaryList($scope.longAbsentismList, "DesignationId", "Designation"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var eDialog = $("#empLongAbsentism").data("ejDialog");
                eDialog.open();
            }

        });
    };

    $scope.modalClose = function (modal) {
        var eDialog = $(modal).data("ejDialog");
        eDialog.close();
    };

    $scope.GetModalHRDailyLateStatusList = function (pageno, data) {
        $scope.searchbyLateEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Late Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyLateStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLateEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLateEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLateEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLateEmpList[i].Value, visible: true, width: "180px" });
                }
                $('#lateEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLate,
                    templateRefresh: $scope.actionCompleteSearchLate,
                    isResponsive: true
                });

                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLate").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActLate").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                $scope.dataGrid = "#lateEmpGrid";
                var eDialog = $("#empLateStatus").data("ejDialog");
                eDialog.open();
            }

            //angular.element(document.querySelector('#empLateStatus')).modal('show');
        });
    };

    $scope.actionCompleteSearchLate = function (args) {
        if (args.requestType == "refresh") {

            try {
                var gridObj = $("#lateEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };

    $scope.GetModalHRDailyLeaveStatusDetailList = function (pageno, data) {
        $scope.searchbyLeaveEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Leave Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyLeaveStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLeaveEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLeaveEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLeaveEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLeaveEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#leaveEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLeave,
                    templateRefresh: $scope.actionCompleteSearchLeave,
                    isResponsive: true
                });
                //$scope.actionCompleteSearchLeave();
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLeave").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActLeave").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });

                var ColumnListLeaveCode = [{ field: 'descColumn', width: 150, headerText: "Leave Summary" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultLeaveCode").ejGrid({
                    dataSource: CreateSummaryList(response.data, "LeaveType", "LeaveType"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                $scope.dataGrid = "#leaveEmpGrid";
                var eDialog = $("#empLeaveStatus").data("ejDialog");
                eDialog.open();
            }

            // angular.element(document.querySelector('#empLeaveStatus')).modal('show');
        });
    };

    $scope.actionCompleteSearchLeave = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#leaveEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };

    $scope.GetModalHRDailyShiftNotAssignedStatusList = function (pageno, data) {
        $scope.searchbyHRDailyShiftNotAssignedEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Shift Not Assigned Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyShiftNotAssignedStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyHRDailyShiftNotAssignedEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyHRDailyShiftNotAssignedEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyHRDailyShiftNotAssignedEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyHRDailyShiftNotAssignedEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#dailyShiftNotAssignedGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteShiftNotAssigned,
                    templateRefresh: $scope.actionCompleteShiftNotAssigned,
                    isResponsive: true
                });
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigShftNotAssigned").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActShftNotAssigned").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                $scope.dataGrid = "#dailyShiftNotAssignedGrid";
                var eDialog = $("#HRDailyShiftNotAssignedStatus").data("ejDialog");
                eDialog.open();
            }
            //angular.element(document.querySelector('#HRDailyShiftNotAssignedStatus')).modal('show');
        });
    };
    $scope.actionCompleteShiftNotAssigned = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#dailyShiftNotAssignedGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };
    $scope.searchByListWeekOff = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Shift Name',
            'value': 'ShiftDefinationName'
        }
    ];



    $scope.GetModalWeekOffList = function (pageno, data) {
        $scope.searchbyWeekOffList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyOffDayStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyWeekOffList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyWeekOffList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyWeekOffList.length; i++) {
                    fieldList.push({ field: $scope.searchbyWeekOffList[i].Value, visible: true, width: "180px" });
                }

                $('#weekOffGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteWeekOff,
                    templateRefresh: $scope.actionCompleteWeekOff,
                    isResponsive: true
                });
                //$scope.actionCompleteSearchLeave();
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpWeekOff").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActWeekOff").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                $scope.dataGrid = "#weekOffGrid";
                var eDialog = $("#DailyOffDayStatusList").data("ejDialog");
                eDialog.open();
            }

            // angular.element(document.querySelector('#empLeaveStatus')).modal('show');
        });
    };
    $scope.actionCompleteWeekOff = function (args) {

        try {
            if (args.requestType == "refresh") {

                var gridObj = $("#weekOffGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            throw (e);
        }
    };


    $scope.XGetModalWeekOffList = function (pageno, data) {
        console.log("OffDay", $scope.ColList);
        $scope.OffDayEmpList = [];
        $scope.setModal(data);
        $scope.HRWeekOffParameters.tempData = data;

        $scope.status = "List of Week Off Employees";

        if (baseService.isUndefinedOrNull($scope.HRWeekOffParameters.searchBy) === false &&
            baseService.isUndefinedOrNull($scope.HRWeekOffParameters.search) === false &&
            undefined === pageno) {
            $scope.HRWeekOffParameters.offset = 0;
        } else if (pageno === 0) {
            $scope.HRWeekOffParameters.offset = 0;
            baseService.setCurrentPage('OffDayEmpList');
        }
        else if (!baseService.isUndefinedOrNull(pageno) && pageno > 0) {
            $scope.HRWeekOffParameters.offset = $scope.HRWeekOffParameters.limit * (pageno - 1);
        }
        var formData = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HRWeekOffParameters };

        baseService.paginationPost('Employees/HRDashboard/ModalHRDailyOffDayStatusList/', formData)
            .then(function (result) {
                $scope.OffDayEmpList = result.Rows;
                $scope.HRWeekOffParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        angular.element(document.querySelector('#DailyOffDayStatusList')).modal('show');
    };

    $scope.searchByListAttdnNotPrc = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Shift Name',
            'value': 'ShiftDefinationName'
        }
    ];

    $scope.HRDailyAttdnNotProcessedStatusParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'ShiftDefinationName,EmployeeName',
        searchBy: 'EmployeeCode',
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
        tempData: {}
    };

    //$scope.GetModalHRDailyAttdnNotProcessedStatusListPagn = function (pageno) {
    //    $scope.GetModalHRDailyAttdnNotProcessedStatusList(pageno, $scope.HRDailyAttdnNotProcessedStatusParameters.tempData);
    //};

    $scope.GetModalHRDailyAttdnNotProcessedStatusList = function (pageno, data) {
        $scope.searchbyAttdnNotPrcEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HRDailyAttdnNotProcessedStatusParameters };

        $scope.status = "List of Attendance not Processed Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyAttdnNotProcessedStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyAttdnNotPrcEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyAttdnNotPrcEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyAttdnNotPrcEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyAttdnNotPrcEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#attendanceNotProcessedGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteAttdnNotProcessed,
                    templateRefresh: $scope.actionCompleteAttdnNotProcessed,
                    isResponsive: true
                });
                //$scope.actionCompleteSearchLeave();
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation " }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpANP").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActANP").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                $scope.dataGrid = "#attendanceNotProcessedGrid";
                var eDialog = $("#empAttdnProcEmp").data("ejDialog");
                eDialog.open();
            }
        });
    };
    $scope.actionCompleteAttdnNotProcessed = function (args) {

        try {
            if (args.requestType == "refresh") {

                var gridObj = $("#attendanceNotProcessedGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            throw (e);
        }
    };




    $scope.GetOtherModalDetailJS = function (data) {
        $scope.setModal(data);
        $http({
            method: 'POST',
            url: 'Employees/HRDashboard/ModalOthersDetail/',
            data: { 'status': status, 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OthersList = response.data;
            var eDialog = $("#OthersDetailStatus").data("ejDialog");
            eDialog.open();

            //angular.element(document.querySelector('#OthersDetailStatus')).modal('show');
        });
    };


    $scope.GetModalHRDailyLateInStatusDetailList = function (pageno, data) {
        $scope.searchbyLeaveEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Late in Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyLateInStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLeaveEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLeaveEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLeaveEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLeaveEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#LateInEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLateIn,
                    templateRefresh: $scope.actionCompleteSearchLateIn,
                    isResponsive: true
                });
                //$scope.actionCompleteSearchLeave();
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLateIn").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActLateIn").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });


                $scope.dataGrid = "#LateInEmpGrid";
                var eDialog = $("#empLateInStatus").data("ejDialog");
                eDialog.open();
            }

            // angular.element(document.querySelector('#empLeaveStatus')).modal('show');
        });
    };

    $scope.actionCompleteSearchLateIn = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#leaveEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };



    $scope.GetModalHRDailyLateInStatusDetailList = function (pageno, data) {
        $scope.searchbyLeaveEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Late in Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyLateInStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLeaveEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLeaveEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLeaveEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLeaveEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#LateInEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLateIn,
                    templateRefresh: $scope.actionCompleteSearchLateIn,
                    isResponsive: true
                });
                //$scope.actionCompleteSearchLeave();
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLateIn").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActLateIn").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });


                $scope.dataGrid = "#LateInEmpGrid";
                var eDialog = $("#empLateInStatus").data("ejDialog");
                eDialog.open();
            }

            // angular.element(document.querySelector('#empLeaveStatus')).modal('show');
        });
    };

    $scope.actionCompleteSearchLateIn = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#LateInEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };


    $scope.GetModalHRDailyEarlyOutStatusDetailList = function (pageno, data) {
        $scope.searchbyLeaveEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Early Out Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyEarlyOutStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLeaveEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLeaveEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLeaveEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLeaveEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#EalryOutEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchEarlyOut,
                    templateRefresh: $scope.actionCompleteSearchEarlyOut,
                    isResponsive: true
                });
                //$scope.actionCompleteSearchLeave();
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpEarlyOut").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActEarlyOut").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });


                $scope.dataGrid = "#EalryOutEmpGrid";
                var eDialog = $("#empEarlyOutStatus").data("ejDialog");
                eDialog.open();
            }

            // angular.element(document.querySelector('#empLeaveStatus')).modal('show');
        });
    };

    $scope.actionCompleteSearchEarlyOut = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#EalryOutEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };



    $scope.GetModalHRDailyLunchOutStatusDetailList = function (pageno, data) {
        $scope.searchbyLeaveEmpList = [];
        $scope.setModal(data);
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.HROnRoleDetailParameters };
        $scope.status = "List of Lunch Out  Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Employees/HRDashboard/ModalHRDailyLunchOutStatusList/',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                if (baseService.arrayLength($scope.searchbyLeaveEmpList) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyLeaveEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyLeaveEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyLeaveEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#LunchOutEmpGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,
                    minWidth: 400,
                    height: 400,
                    actionComplete: $scope.actionCompleteSearchLunchOut,
                    templateRefresh: $scope.actionCompleteSearchLunchOut,
                    isResponsive: true
                });
                //$scope.actionCompleteSearchLeave();
                var ColumnList = [{ field: 'descColumn', width: 150, headerText: "Designation" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultDesigGrpLunchOut").ejGrid({
                    dataSource: CreateSummaryList(response.data, "DesignationId", "Designation"),
                    //minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: false,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnList,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });
                var ColumnListOpActivity = [{ field: 'descColumn', width: 150, headerText: "Operation Activity" }, { field: 'Total', width: 70, headerText: "Total" }];

                $("#GridResultOpActLunchOut").ejGrid({
                    dataSource: CreateSummaryList(response.data, "OperationActivityId", "OperationActivityName"),
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowSelection: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: ColumnListOpActivity,
                    showSummary: true,
                    summaryRows: [{ title: 'Total', summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Total", dataMember: "Total", format: "{0:N0}" }] }]
                });


                $scope.dataGrid = "#LunchOutEmpGrid";
                var eDialog = $("#empLuchOutStatus").data("ejDialog");
                eDialog.open();
            }

            // angular.element(document.querySelector('#empLeaveStatus')).modal('show');
        });
    };

    $scope.actionCompleteSearchLunchOut = function (args) {
        if (args.requestType == "refresh") {
            try {
                var gridObj = $("#LunchOutEmpGrid").ejGrid("instance");
                var scrollerwidth = $("#MainDiv").width();//Obtain the width of the container
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            } catch (e) {
                throw (e);
            }
        }
    };


    $scope.GetModalOthersDetailJS = function () {


        var eDialog = $("#OthersSummaryStatus").data("ejDialog");
        eDialog.open();

        //angular.element(document.querySelector('#OthersSummaryStatus')).modal('show');
    };
    //----------------------------*Dynamic Modal END*-----------------------------------------//
    $scope.setModal = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence === $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UId;
            }
        }
    };
    $scope.BudgetSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'Excess',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true
    };
    $scope.GetModalBudgetSummaryJSPagn = function (pageno) {
        $scope.GetModalBudgetSummaryJS(pageno);
    };
    $scope.GetModalBudgetSummaryJS = function (pageno) {

        $scope.GetModalBudgetSummaryJS = function (pageno) {

            $scope.searchbyOnRoleDetailEmpList = [];
            var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'date': $scope.Date, 'status': $scope.hrDrpDownModel.PODirectIndirectStatus, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.BudgetSummaryParameters };

            $scope.status = "Manpower Budget Details";
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'ManpowerBudgetDashboard/ModalBudgetSummary/',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.onRoleEmpList = response.data;

                    if (baseService.arrayLength($scope.searchbyOnRoleDetailEmpList) === 0) {
                        baseService.getDDLSearchColumn(response.data, $scope.searchbyOnRoleDetailEmpList);
                    }
                    var fieldList = [];
                    for (var i = 0; i < $scope.searchbyOnRoleDetailEmpList.length; i++) {
                        fieldList.push({ field: $scope.searchbyOnRoleDetailEmpList[i].Value, visible: true, width: "180px" });
                    }
                    var scrollerwidth = $("#ManpowerBudgetInfo").width();//Obtain the width of the container
                    $('#ManpowerBudgetInfo').ejGrid({
                        dataSource: response.data,
                        allowPaging: true,
                        allowFiltering: true,
                        allowKeyboardNavigation: true,
                        columns: fieldList,
                        filterSettings: { filterType: "excel" },
                        allowScrolling: true,
                        minWidth: 400,
                        isResponsive: true
                    });
                    $scope.dataGrid = "#ManpowerBudgetInfo";
                }

                angular.element(document.querySelector('#BudgetSummaryModal')).modal('show');
            });
        };


        //$scope.BudgetSummaryList = [];

        //if (baseService.isUndefinedOrNull($scope.BudgetSummaryParameters.searchBy) === false &&
        //    baseService.isUndefinedOrNull($scope.BudgetSummaryParameters.search) === false &&
        //    undefined === pageno) {
        //    $scope.BudgetSummaryParameters.offset = 0;
        //} else if (pageno === 0) {
        //    $scope.BudgetSummaryParameters.offset = 0;
        //    baseService.setCurrentPage('BudgetSummaryList');
        //}
        //else if (!baseService.isUndefinedOrNull(pageno) && pageno > 0) {
        //    $scope.BudgetSummaryParameters.offset = $scope.BudgetSummaryParameters.limit * (pageno - 1);
        //}
        //var formData = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'date': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.BudgetSummaryParameters };
        //baseService.paginationPost('ManpowerBudgetDashboard/ModalBudgetSummary/', formData)
        //    .then(function (result) {
        //        $scope.BudgetSummaryList = result.Rows;
        //        $scope.BudgetSummaryParameters.total_count = result.Total;

        //        $scope.propertyName = '';
        //        $scope.reverse = true;
        //        $scope.sortBy = function (propertyName) {
        //            $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
        //            $scope.propertyName = propertyName;
        //        };
        //        angular.element(document.querySelector('#BudgetSummaryModal')).modal('show');
        //    }, function () {
        //        ShowResult(commonMessage.NetworkError, 'failure');
        //    }).finally(function () {
        //    });
    };
    $scope.ExcessSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'Excess',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true
    };
    $scope.GetModalExcessSummaryJSPagn = function (pageno) {
        $scope.ExcessSummary(pageno);
    };
    $scope.ExcessSummary = function (pageno) {
        $scope.ExcessList = [];
        if (baseService.isUndefinedOrNull($scope.ExcessSummaryParameters.searchBy) === false &&
            baseService.isUndefinedOrNull($scope.ExcessSummaryParameters.search) === false &&
            undefined === pageno) {
            $scope.ExcessSummaryParameters.offset = 0;
        } else if (pageno === 0) {
            $scope.ExcessSummaryParameters.offset = 0;
            baseService.setCurrentPage('ExcessList');
        }
        else if (!baseService.isUndefinedOrNull(pageno) && pageno > 0) {
            $scope.ExcessSummaryParameters.offset = $scope.ExcessSummaryParameters.limit * (pageno - 1);
        }
        var formData = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'date': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.ExcessSummaryParameters };
        baseService.paginationPost('ManpowerBudgetDashboard/ModalExcessSummary/', formData)
            .then(function (result) {
                $scope.ExcessList = result.Rows;
                $scope.ExcessSummaryParameters.total_count = result.Total;

                $scope.propertyName = '';
                $scope.reverse = true;
                $scope.sortBy = function (propertyName) {
                    $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                    $scope.propertyName = propertyName;
                };
                angular.element(document.querySelector('#ExcessModalSummary')).modal('show');
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.ShortSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'short',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true,
    };
    $scope.GetModalShortSummaryJSPagn = function (pageno) {
        $scope.GetShortSummary(pageno);
    };
    $scope.GetShortSummary = function (pageno) {
        $scope.ShortSummaryList = [];
        if (baseService.isUndefinedOrNull($scope.ShortSummaryParameters.searchBy) === false &&
            baseService.isUndefinedOrNull($scope.ShortSummaryParameters.search) === false &&
            undefined === pageno) {
            $scope.ShortSummaryParameters.offset = 0;
        } else if (pageno === 0) {
            $scope.ShortSummaryParameters.offset = 0;
            baseService.setCurrentPage('ShortSummaryList');
        }
        else if (!baseService.isUndefinedOrNull(pageno) && pageno > 0) {
            $scope.ShortSummaryParameters.offset = $scope.ShortSummaryParameters.limit * (pageno - 1);
        }
        var formData = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'date': $scope.hrDate, 'parameters': $scope.ShortSummaryParameters };
        baseService.paginationPost('ManpowerBudgetDashboard/ModalShortSummary/', formData)
            .then(function (result) {
                $scope.ShortSummaryList = result.Rows;
                $scope.ShortSummaryParameters.total_count = result.Total;

                $scope.propertyName = '';
                $scope.reverse = true;
                $scope.sortBy = function (propertyName) {
                    $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                    $scope.propertyName = propertyName;
                };
                angular.element(document.querySelector('#ShortSummaryModal')).modal('show');
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.EmployeeSummaryParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'BudgetCode',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true
    };
    $scope.getModalEmployeeSummaryJSPagn = function (pageno) {
        $scope.getModalEmployeeSummaryJS(pageno);
    };
    $scope.getModalEmployeeSummaryJS = function (pageno) {
        var parameters = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId };

        $scope.searchbyonRoleEmpList = [];
        $scope.status = "List of OnRole Employees";
        $http({
            method: "POST",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'ManpowerBudgetDashboard/ModalEmployeeSummary/',
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
                    //scrollSettings: { width: 1200, height: 400 }
                    minWidth: 400,
                    isResponsive: true
                });
                $scope.dataGrid = "#onRoleEmpGrid";
            }
            angular.element(document.querySelector('#EmployeeModalSummary')).modal('show');
        });
    };

    $scope.getBudgetCodeWiseEmpList = function (data) {
        $scope.BudgetCWiseEmp = [];
        $scope.BEmpData = [];
        $scope.BEmpData = data;
        $scope.BEmpBudgetCode = [];
        $scope.BEmpBudgetCode = data.budgetCode;
        $scope.setModal(data);
        $http({
            method: 'POST',
            url: 'ManpowerBudgetDashboard/WPBudgetCodeWiseEmpList/',
            params: { 'ChartColumnList': $scope.ColList, 'budgetCode': data.MbId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetCWiseEmp = response.data;

            $scope.propertyName = '';
            $scope.reverse = true;
            $scope.sortBy = function (propertyName) {
                $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                $scope.propertyName = propertyName;
            };
            angular.element(document.querySelector('#BudgetWiseEmpList')).modal('show');
        });
    };

    $scope.LateParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'LateDays',
        searchBy: null,
        search: null,
        pageSize: 10,
        total_count: 0,
        serverPagination: true
    };

    $scope.GetDetailLateStatus = function (pageno) {
        $scope.ShortSummaryList = [];
        if (baseService.isUndefinedOrNull($scope.ShortSummaryParameters.searchBy) === false &&
            baseService.isUndefinedOrNull($scope.ShortSummaryParameters.search) === false &&
            undefined === pageno) {
            $scope.ShortSummaryParameters.offset = 0;
        } else if (pageno === 0) {
            $scope.ShortSummaryParameters.offset = 0;
            baseService.setCurrentPage('ShortSummaryList');
        }
        else if (!baseService.isUndefinedOrNull(pageno) && pageno > 0) {
            $scope.ShortSummaryParameters.offset = $scope.ShortSummaryParameters.limit * (pageno - 1);
        }
        var formData = { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'hrDate': $scope.hrDate, 'EmplyeeTypeOrCategoryId': $scope.hrDrpDownModel.EmplyeeTypeOrCategoryId, 'parameters': $scope.ShortSummaryParameters };
        baseService.paginationPost('ManpowerBudgetDashboard/ModalShortSummary/', formData)
            .then(function (result) {
                $scope.ShortSummaryList = result.Rows;
                $scope.ShortSummaryParameters.total_count = result.Total;

                $scope.propertyName = '';
                $scope.reverse = true;
                $scope.sortBy = function (propertyName) {
                    $scope.reverse = $scope.propertyName === propertyName ? !$scope.reverse : false;
                    $scope.propertyName = propertyName;
                };
                angular.element(document.querySelector('#ShortSummaryModal')).modal('show');
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    //----------------------2nd Part-----------------------------//
    $scope.secondPageDropDownModel = {
        status: null
    };
    var absentDiv = document.getElementById("absentDiv");
    absentDiv.style.display = "none";
    var conAbsentDiv = document.getElementById("conAbsentDiv");
    conAbsentDiv.style.display = "none";
    var lateDiv = document.getElementById("lateDiv");
    lateDiv.style.display = "none";
    //var conLateDiv = document.getElementById("conLateDiv");
    //conLateDiv.style.display = "none";
    var joinDiv = document.getElementById("joinDiv");
    joinDiv.style.display = "none";
    var separationDiv = document.getElementById("separtionDiv");
    separationDiv.style.display = "none";
    var rspnsblePersonDiv = document.getElementById("rspnsblePersonDiv");
    rspnsblePersonDiv.style.display = "none";

    var absentParam = document.getElementById("absentParam");
    absentParam.style.display = "none";

    var lateParam = document.getElementById("lateParam");
    lateParam.style.display = "none";

    var joiningParam = document.getElementById("joiningParam");
    joiningParam.style.display = "none";

 

    var separationParam = document.getElementById("separationParam");
    separationParam.style.display = "none";

    var responsibleParam = document.getElementById("responsibleParam");
    responsibleParam.style.display = "none";

    var conPresentDiv = document.getElementById("conPresentDiv");
    conPresentDiv.style.display = "none"; 
    var presentwithinParam = document.getElementById("presentwithinParam");
    presentwithinParam.style.display = "none";
     
    var conWorkingHoursDiv = document.getElementById("conWorkingHoursDiv");
    conWorkingHoursDiv.style.display = "none"; 
    var WorkingHoursParam = document.getElementById("WorkingHoursParam");
    WorkingHoursParam.style.display = "none";  
 
    $scope.divLoader = function () {
        if ($scope.secondPageDropDownModel.status === 'DateWiseAbsentList') {
            absentDiv.style.display = "block";
            conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "none";
            joinDiv.style.display = "none";
            separationDiv.style.display = "none";
            rspnsblePersonDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            //Parameter
            absentParam.style.display = "block";
            responsibleParam.style.display = "none";
            joiningParam.style.display = "none";
            separationParam.style.display = "none";
            conPresentDiv.style.display = "none";
            presentwithinParam.style.display = "none";
            lateParam.style.display = "none";

            $scope.secondDivTitle = "No. of days absent with-in date range";

        }
        else if ($scope.secondPageDropDownModel.status === 'DateWiseConsecutiveAbsentList') {
            conAbsentDiv.style.display = "block";
            absentDiv.style.display = "none";
            //conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "none";
            joinDiv.style.display = "none";
            separationDiv.style.display = "none";
            rspnsblePersonDiv.style.display = "none";
            conPresentDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            WorkingHoursParam.style.display = "none";
        }
        else if ($scope.secondPageDropDownModel.status === 'DateWiseLateList') {
            lateDiv.style.display = "block";

            absentDiv.style.display = "none";
            conAbsentDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            conLateDiv.style.display = "none";
            joinDiv.style.display = "none";
            separationDiv.style.display = "none";
            rspnsblePersonDiv.style.display = "none";

            //Param
            lateParam.style.display = "block";
            conPresentDiv.style.display = "none";

            absentParam.style.display = "none";
            responsibleParam.style.display = "none";
            joiningParam.style.display = "none";
            separationParam.style.display = "none";
            presentwithinParam.style.display = "none";
            WorkingHoursParam.style.display = "none";
            $scope.secondDivTitle = "No. of days late with-in date range";

        }
        else if ($scope.secondPageDropDownModel.status === 'DateWiseConsecutiveLateList') {
            conLateDiv.style.display = "block";
            conPresentDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            absentDiv.style.display = "none";
            conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "block";
            joinDiv.style.display = "none";
            separationDiv.style.display = "none";
            WorkingHoursParam.style.display = "none";

        }
        else if ($scope.secondPageDropDownModel.status === 'DateWiseJoiningList') {
            joinDiv.style.display = "block";
            absentDiv.style.display = "none";
            conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "none";
            //joinDiv.style.display = "none";
            separationDiv.style.display = "none";
            rspnsblePersonDiv.style.display = "none";
            conPresentDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            //Param
            joiningParam.style.display = "block";
            separationParam.style.display = "none";
            presentwithinParam.style.display = "none";
            lateParam.style.display = "none";
            absentParam.style.display = "none";
            responsibleParam.style.display = "none";
            WorkingHoursParam.style.display = "none";

        }
        else if ($scope.secondPageDropDownModel.status === 'DateWiseSeparationList') {
            separationDiv.style.display = "block";
            absentDiv.style.display = "none";
            conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "none";
            joinDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            rspnsblePersonDiv.style.display = "none";
            conPresentDiv.style.display = "none";
            presentwithinParam.style.display = "none";
            joiningParam.style.display = "none";
            separationParam.style.display = "block";
            lateParam.style.display = "none";
            absentParam.style.display = "none";
            responsibleParam.style.display = "none";
            WorkingHoursParam.style.display = "none";

        }
        else if ($scope.secondPageDropDownModel.status === 'ROWiseAttendance') {
            rspnsblePersonDiv.style.display = "block";
            absentDiv.style.display = "none";
            conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "none";
            joinDiv.style.display = "none";
            separationDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            conPresentDiv.style.display = "none";

            //Param

            responsibleParam.style.display = "block";
            presentwithinParam.style.display = "none";
            joiningParam.style.display = "none";
            separationParam.style.display = "none";

            lateParam.style.display = "none";
            absentParam.style.display = "none";
            WorkingHoursParam.style.display = "none";

            $scope.secondDivTitle = "Reporting Person Wise Attendance Status";
        }
        else if ($scope.secondPageDropDownModel.status === 'WorkedCont10Days') {
            conPresentDiv.style.display = "block";
            separationDiv.style.display = "none";
            absentDiv.style.display = "none";
            conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "none";
            joinDiv.style.display = "none";
            conWorkingHoursDiv.style.display = "none";
            rspnsblePersonDiv.style.display = "none";

            joiningParam.style.display = "none";
            separationParam.style.display = "none";
            presentwithinParam.style.display = "block";
            lateParam.style.display = "none";
            absentParam.style.display = "none";
            responsibleParam.style.display = "none";
            WorkingHoursParam.style.display = "none";
        }
        else if ($scope.secondPageDropDownModel.status === 'WorkedMoreThen10Hrs') {
             conWorkingHoursDiv.style.display = "block";
            separationDiv.style.display = "none";
            absentDiv.style.display = "none";
            conAbsentDiv.style.display = "none";
            lateDiv.style.display = "none";
            conLateDiv.style.display = "none";
            joinDiv.style.display = "none";
            conPresentDiv.style.display = "none";
            rspnsblePersonDiv.style.display = "none";

            joiningParam.style.display = "none";
            separationParam.style.display = "none";
            presentwithinParam.style.display = "none";
            lateParam.style.display = "none";
            absentParam.style.display = "none";
            responsibleParam.style.display = "none";
            responsibleParam.style.display = "none";
            WorkingHoursParam.style.display = "block";
        }
    };
    $rootScope.searchByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        }

    ];
    $scope.cboCompanyList = null;
    $scope.cboPlantList = null;
    $scope.companyId = null;

    $scope.lateCompanyId = null;
    $scope.roCompanyId = null;

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.cboCompanyList = result;
    });

    $scope.plantId = null;
    $scope.latePlantId = null;
    $scope.roPlantId = null;

    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.companyId, function (result) {
            $scope.cboPlantList = result;
        });
    };

    $scope.reportingPersonId = null;

    $scope.getReportingPerson = function () {
        cboService.getCboReportingPerson($scope.companyId, $scope.plantId, function (result) {
            $scope.reportingPersonList = result;
        });
    };



    $scope.hrJSFromDate = null;
    $scope.hrJSToDate = null;

    $scope.absentComparator = "<=";
    $scope.dateDiff = function dateDiff() {

        var toDate = new Date($scope.hrJSToDate);
        toDate.setDate(toDate.getDate());

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrJSFromDate);

        var diffDays = Math.ceil(Math.abs((formDate.getTime() - toDate.getTime()) / (oneDay)));
        $scope.dayCount = diffDays;
    };

    var absentTabele = document.getElementById("absentTable");
    absentTabele.style.display = "none";
    $scope.DateWiseAbsentStatusList = [];
    $scope.GetGruopWiseDateWiseAbsentStatus = function () {

        var toDate = new Date($scope.hrJSToDate);
        toDate.setDate(toDate.getDate());

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrJSFromDate);
        $scope.dataGrid = "#GridAbsent";
        if ($scope.companyId === "" || $scope.companyId === undefined || $scope.companyId === null) {
            throw ShowResult("Select Company", 'failure');
        }
        //else if ($scope.plantId === "" || $scope.plantId === undefined || $scope.plantId === null) {
        //    throw ShowResult("Select Plant", 'failure');
        //}
        if ($scope.hrJSFromDate === "" || $scope.hrJSFromDate === undefined) {
            throw ShowResult("Missing From Date", 'failure');
        }
        else if ($scope.hrJSToDate === "" || $scope.hrJSToDate === undefined) {
            throw ShowResult("Missing To Date", 'failure');
        }
        else if (formDate > toDate) {
            throw ShowResult("From Date Can not  be greater then To Date", 'failure');
        }


        else {
            if (absentTabele.style.display === "none") {
                absentTabele.style.display = "block";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/HRDashboard/DateWiseAbsentList',
                data: {
                    'hrFromDate': $scope.hrJSFromDate,
                    'hrToDate': $scope.hrJSToDate,
                    'dayCount': $scope.dayCount,
                    'comparator': $scope.absentComparator,
                    'companyId': $scope.companyId
                }
            }).then(function successCallback(response) {
                if (response.data.length > 0) {

                    $scope.DateWiseAbsentStatusList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });


        }
    };

    $scope.hrConAbsentJSFromDate = $filter('dateFiltering')(Date.now(), 'dd-MMMM-yyyy');
    $scope.hrConAbsentJSToDate = $filter('dateFiltering')(Date.now(), 'dd-MMMM-yyyy');

    //----------------------------Absent End--------------------------------------------------------//
    //----------------------------------Late---------------------------------------------------//
    $scope.dateWiseLateListParam = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'LateDays',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.lateComparator = "=";


    $scope.hrLateJSFromDate = null;
    $scope.hrLateJSToDate = null;
    $scope.dayCountlate = null;

    var lateTable = document.getElementById("lateTable");

    $scope.dateDifflate = function dateDifflate() {

        var toDate = new Date($scope.hrLateJSToDate);
        toDate.setDate(toDate.getDate());

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrLateJSFromDate);

        var diffDaysLate = Math.ceil(Math.abs((formDate.getTime() - toDate.getTime()) / (oneDay)));
        $scope.dayCountlate = diffDaysLate;
    };

    lateTable.style.display = "none";
    $scope.DateWiseLateStatusList = [];
    $scope.GetGruopWiseDateWiseLateStatus = function () {

        var toDate = new Date($scope.hrLateJSToDate);
        toDate.setDate(toDate.getDate());
        $scope.dataGrid = "#GridLate";

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrLateJSFromDate);

        if ($scope.companyId === "" || $scope.companyId === undefined || $scope.companyId === null) {
            throw ShowResult("Select Company", 'failure');
        }
        //else if ($scope.plantId === "" || $scope.plantId === undefined || $scope.plantId === null) {
        //    throw ShowResult("Select Plant", 'failure');
        //}
        else if ($scope.hrLateJSFromDate === "" || $scope.hrLateJSFromDate === undefined) {
            throw ShowResult("Missing From Date", 'failure');
        }
        else if ($scope.hrLateJSToDate === "" || $scope.hrLateJSToDate === undefined) {
            throw ShowResult("Missing To Date", 'failure');
        }
        else if (formDate > toDate) {
            throw ShowResult("From Date Can not  be greater then To Date", 'failure');
        }

        else {
            if (lateTable.style.display === "none") {
                lateTable.style.display = "block";
            }


            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/HRDashboard/DateWiseLatetListStatus',
                data: {
                    'hrFromDate': $scope.hrLateJSFromDate,
                    'hrToDate': $scope.hrLateJSToDate,
                    'dayCount': $scope.dayCountlate,
                    'comparator': $scope.lateComparator,
                    'companyId': $scope.companyId

                }
            }).then(function successCallback(response) {
                if (response.data.length > 0) {

                    $scope.DateWiseLateStatusList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });

        }
    };




    var presentTable = document.getElementById("conPresentTable");
    presentTable.style.display = "none";
    $scope.DateWisePresentStatusList = [];


    var today = new Date();
    var last30Days = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 30)
    $scope.hrPresentJSFromDate = $filter('dateFiltering')(last30Days, 'dd-MMMM-yyyy');
    $scope.hrPresentJSToDate = $filter('dateFiltering')(Date.now(), 'dd-MMMM-yyyy');
    $scope.dayCountPresent = null;
    $scope.dayCountPresent = 12;
    $scope.dateDiffPresent = function () {

        var toDate = new Date($scope.hrPresentJSToDate);
        toDate.setDate(toDate.getDate());

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrPresentJSFromDate);

        var diffDaysPresent = Math.ceil(Math.abs((formDate.getTime() - toDate.getTime()) / (oneDay)));
        $scope.dayCountPresent = diffDaysPresent;
    };
    $scope.presentComparator = ">=";
    $scope.GetGruopWiseDateWisePresentStatus = function () {

        var toDate = new Date($scope.hrPresentJSToDate);
        toDate.setDate(toDate.getDate());
        $scope.dataGrid = "#";

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrPresentJSFromDate);




        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrPresentJSFromDate);

        var diffDaysLate = Math.ceil(Math.abs((formDate.getTime() - toDate.getTime()) / (oneDay)));

        if ($scope.companyId === "" || $scope.companyId === undefined || $scope.companyId === null) {
            throw ShowResult("Select Company", 'failure');
        }
      
        else if ($scope.hrPresentJSFromDate === "" || $scope.hrPresentJSFromDate === undefined) {
            throw ShowResult("Missing From Date", 'failure');
        }
        else if ($scope.hrPresentJSToDate === "" || $scope.hrPresentJSToDate === undefined) {
            throw ShowResult("Missing To Date", 'failure');
        }
        else if (formDate > toDate) {
            throw ShowResult("From Date Can not  be greater then To Date", 'failure');
        }
        else if (diffDaysLate < $scope.dayCountPresent)
        {
            throw ShowResult("Given Dates are not valid", 'failure');

        }
        else {
            if (presentTable.style.display === "none") {
                presentTable.style.display = "block";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/HRDashboard/ConsecutivePresentStatusDynamic',
                data: {
                    'hrFromDate': $scope.hrPresentJSFromDate,
                    'hrToDate': $scope.hrPresentJSToDate,
                    'dayCount': $scope.dayCountPresent,
                    'presentComparator': $scope.presentComparator,
                    'CompanyId': $scope.companyId
                }
            }).then(function successCallback(response) {
               
                if (response.data.length > 0) {
                    $scope.DateWisePresentStatusList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });

        }
    };
    $scope.workingHours = 12;
    $scope.workingHoursComparator = ">=";
    $scope.workingHoursFromDate = null;
    $scope.workingHoursToDate = null;

    $scope.WorkingHoursList = [];
    $scope.GetGruopWiseDateWiseWorkingHours = function () {

        var toDate = new Date($scope.workingHoursToDate);
        toDate.setDate(toDate.getDate());
        $scope.dataGrid = "#GridWorkingHours";      
              
            if (presentTable.style.display === "none") {
                presentTable.style.display = "block";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/HRDashboard/GetEEmpJobCardInfoWithInDateTimes',
                data: {
                    'wrHrFromDate': $scope.workingHoursFromDate,
                    'wrHrToDate': $scope.workingHoursToDate,

                    'hours': $scope.workingHours,                   
                    'presentComparator': $scope.workingHoursComparator,
                    'companyId': $scope.companyId
                }
            }).then(function successCallback(response) {

                if (response.data.length > 0) {
                    $scope.WorkingHoursList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });

      
    };
    $scope.PrintPresent = function () {
        try {


            $http({
                method: 'POST',
                url: 'Employees/HRDashboard/PrintPresent',
                data: {
                    'hrFromDate': $scope.hrPresentJSFromDate,
                    'hrToDate': $scope.hrPresentJSToDate,
                    'dayCount': $scope.dayCountPresent,
                    'presentComparator': $scope.presentComparator,
                    'companyId': $scope.companyId
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
    $scope.presentDaysList = [];
     $scope.ModalPresentEmpWiseDate = function (data) {
        $scope.ADGLUrl = 'Employees/HRDashboard/ModalEmployeeWisePresentDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrPresentJSFromDate + "&hrToDate=" + $scope.hrPresentJSToDate + "&EmpSystemId=" + data.EmpSystemID + "&dayCount=" + $scope.dayCountPresent + "&comparator=" + $scope.presentComparator + "";
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.presentDaysList = response.data;
        });
         angular.element(document.querySelector('#presentDaysCountList')).modal('show');
    };
    $scope.ModalPresentEmpWiseDateList = function (data) {
        $scope.ADGLUrl = 'Employees/HRDashboard/ModalEmployeeWisePresentStatusDateWiseList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrLateJSFromDate + "&hrToDate=" + $scope.hrLateJSToDate + "&EmpSystemId=" + data.EmpSystemID + "&dayCount=" + $scope.dayCountPresent + "&comparator=" + $scope.presentComparator + "";
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.lateDayList = response.data;
        });
        angular.element(document.querySelector('#LateDateWiseList')).modal('show');
    };
    //------------------------Late End-------------------------------//
    $scope.dateWiseJoiningListParam = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'DOJ',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.hrJoiningJSFromDate = null;
    $scope.hrJoiningJSToDate = null;
    var joiningTable = document.getElementById("joinTable");
    joiningTable.style.display = "none";
    $scope.DateWiseJoiningStatusList = [];
    $scope.GetGruopWiseDateWiseJoinStatus = function () {

        var toDate = new Date($scope.hrJoiningJSToDate);
        toDate.setDate(toDate.getDate());
        $scope.dataGrid = "#GridJoin";


        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrJoiningJSFromDate);


        if ($scope.companyId === "" || $scope.companyId === undefined || $scope.companyId === null) {
            throw ShowResult("Select Company", 'failure');
        }
        else if ($scope.hrJoiningJSFromDate === "" || $scope.hrJoiningJSFromDate === undefined) {
            throw ShowResult("Missing From Date", 'failure');
        }
        else if ($scope.hrJoiningJSToDate === "" || $scope.hrJoiningJSToDate === undefined) {
            throw ShowResult("Missing To Date", 'failure');
        }
        else if (formDate > toDate) {
            throw ShowResult("From Date Can not  be greater then To Date", 'failure');
        }
        else {
            if (joiningTable.style.display === "none") {
                joiningTable.style.display = "block";

            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/HRDashboard/DateJoiningStatus',
                data: {
                    'hrFromDate': $scope.hrJoiningJSFromDate,
                    'hrToDate': $scope.hrJoiningJSToDate,
                    'companyId': $scope.companyId
                }
            }).then(function successCallback(response) {
                if (response.data.length > 0) {

                    $scope.DateWiseJoiningStatusList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });




        }
    };

    $scope.dateWiseSaparaionListParam = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'DOJ',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.hrSepartionJSFromDate = null;
    $scope.hrSeparationJSToDate = null;
    var separationTable = document.getElementById("separationTable");
    separationTable.style.display = "none";
    $scope.DateWiseSeparationList = [];
    $scope.GetGruopWiseDateWiseSeparationStatus = function () {

        var toDate = new Date($scope.hrSeparationJSToDate);
        toDate.setDate(toDate.getDate());

        $scope.dataGrid = "#GridSeparated";

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrSepartionJSFromDate);

        if ($scope.companyId === "" || $scope.companyId === undefined || $scope.companyId === null) {
            throw ShowResult("Select Company", 'failure');
        }
        else if ($scope.hrSepartionJSFromDate === "" || $scope.hrSepartionJSFromDate === undefined) {
            throw ShowResult("Missing From Date", 'failure');
        }
        else if ($scope.hrSeparationJSToDate === "" || $scope.hrSeparationJSToDate === undefined) {
            throw ShowResult("Missing To Date", 'failure');
        }
        else if (formDate > toDate) {
            throw ShowResult("From Date Can not  be greater then To Date", 'failure');
        }
        else {
            if (separationTable.style.display === "none") {
                separationTable.style.display = "block";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Employees/HRDashboard/DateSepartaionStatus',
                data: {
                    'hrFromDate': $scope.hrSepartionJSFromDate,
                    'hrToDate': $scope.hrSeparationJSToDate,
                    'companyId': $scope.companyId

                }
            }).then(function successCallback(response) {
                if (response.data.length > 0) {

                    $scope.DateWiseSeparationList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });
        }
    };







    //------------------------Responsible Person Wise Attendance Status-----------------------------------//   
    $scope.rPwiseAttdnParam = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'LateDays',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };


    $scope.hrRespPerJSFromDate = null;
    $scope.hrRespPerJSToDate = null;

    var responseDataDiv = document.getElementById("responseDataDiv");


    responseDataDiv.style.display = "none";
    $scope.DateWiseAbsentStatusList = [];

    $scope.GetResponsiblePersonWiseAttdnStatus = function () {

        var toDate = new Date($scope.hrRespPerJSToDate);
        toDate.setDate(toDate.getDate());

        var oneDay = 24 * 60 * 60 * 1000;

        var formDate = new Date($scope.hrRespPerJSFromDate);

        if ($scope.companyId === "" || $scope.companyId === undefined || $scope.companyId === null) {
            throw ShowResult("Select Company", 'failure');
        }
        else if ($scope.plantId === "" || $scope.plantId === undefined || $scope.plantId === null) {
            throw ShowResult("Select Plant", 'failure');
        }
        else if ($scope.hrRespPerJSFromDate === "" || $scope.hrRespPerJSFromDate === undefined) {
            throw ShowResult("Missing From Date", 'failure');
        }
        else if ($scope.hrRespPerJSToDate === "" || $scope.hrRespPerJSFromDate === undefined) {
            throw ShowResult("Missing To Date", 'failure');
        }
        else if (formDate > toDate) {
            throw ShowResult("From Date Can not  be greater then To Date", 'failure');
        }

        else {
            if (responseDataDiv.style.display === "none") {
                responseDataDiv.style.display = "block";
            }
            $scope.rpOthersSummary = null;
            $scope.rPpresent = null;
            $scope.rPlate = null;
            $scope.rPabsent = null;
            $scope.rPleave = null;
            $scope.rPtotalOthersShiftNotAssignedEmployee = null;
            $scope.rPtotalShiftNotAssignAsofToday = null;
            $scope.rPtotalWeekoffEmployee = null;
            $scope.rPtotalAttdnNotProcessedToday = null;
            $scope.rPWiseAttdnStatus = [];

            $scope.GLUrl = 'Employees/HRDashboard/ROPersonWiseAttnStatus?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrRespPerJSFromDate + "&hrToDate=" + $scope.hrRespPerJSToDate + "&reportingPersonId=" + $scope.reportingPersonId + "";

            $scope.GetRpWiseAttdnStatus = function () {
                $http({
                    method: 'GET',
                    url: $scope.GLUrl,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.rPWiseAttdnStatus = response.data;
                    angular.forEach($scope.rPWiseAttdnStatus, function (item, i) {
                        $scope.rpPresent = item.totalPresentEmployee;
                        $scope.rpLate = item.totalLateEmployee;
                        $scope.rpAbsent = item.totalAbsentEmployee;
                        $scope.rpLeave = item.totalLeaveEmployee;
                        $scope.rpTotalOthersShiftNotAssignedEmployee = item.ShiftNotAssignedEmployee;
                        $scope.rpTotalShiftNotAssignAsofToday = item.totalShiftNotAssignAsofToday;
                        $scope.rpTotalWeekoffEmployee = item.totalWeekoffEmployee;
                        $scope.rpTotalAttdnNotProcessedToday = item.totalAttdnNotProcessedToday;
                    });
                    $scope.rpOthersSummary = $scope.rpTotalShiftNotAssignAsofToday + $scope.rpTotalAttdnNotProcessedToday + $scope.rpTotalWeekoffEmployee;
                });
            };
            $scope.GetRpWiseAttdnStatus();
        }
    };
    $scope.GetModalrpOthersDetailJS = function () {
        $scope.oStatus = "Others";
        angular.element(document.querySelector('#rpOthersSummaryStatus')).modal('show');
    };

    var rpWisePresent = document.getElementById("rpWisePresent");
    rpWisePresent.style.display = "none";

    $scope.paramReportingPersonWisePresentEmployee = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: "EmployeeName",
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalORDivGetReportingPersonWisePresentEmployee = function () {
        rpWisePresent.style.display = "block";

        baseService.setCurrentPage('rpWisePresentList');
        $scope.rpWisePresentList = [];
        $scope.GetReportingPersonWisePresentEmployee = function (pageno) {
            $scope.paramReportingPersonWisePresentEmployee.companyId = $scope.companyId;
            $scope.paramReportingPersonWisePresentEmployee.plantId = $scope.plantId;
            $scope.paramReportingPersonWisePresentEmployee.hrFromDate = $scope.hrRespPerJSFromDate;
            $scope.paramReportingPersonWisePresentEmployee.hrToDate = $scope.hrRespPerJSToDate;
            $scope.paramReportingPersonWisePresentEmployee.reportingPersonId = $scope.reportingPersonId;

            baseService.paginationBase('Employees/HRDashboard/ROPersonWisePresentStatusList', pageno, $scope.paramReportingPersonWisePresentEmployee)
                .then(function (data) {
                    $scope.rpWisePresentList = data.Rows;
                    $scope.paramReportingPersonWisePresentEmployee.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetReportingPersonWisePresentEmployee();
        angular.element(document.querySelector('#rpWisePresent')).modal('show');
    };

    $scope.paramReportingPersonWiseAbsentEmployee = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: "EmployeeName",
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ModalORDivGetReportingPersonWiseAbsentEmployee = function () {


        baseService.setCurrentPage('rpWiseAbsentList');
        $scope.rpWiseAbsentList = [];
        $scope.GetReportingPersonWiseAbsentEmployee = function (pageno) {
            $scope.paramReportingPersonWiseAbsentEmployee.companyId = $scope.companyId;
            $scope.paramReportingPersonWiseAbsentEmployee.plantId = $scope.plantId;
            $scope.paramReportingPersonWiseAbsentEmployee.hrFromDate = $scope.hrRespPerJSFromDate;
            $scope.paramReportingPersonWiseAbsentEmployee.hrToDate = $scope.hrRespPerJSToDate;
            $scope.paramReportingPersonWiseAbsentEmployee.reportingPersonId = $scope.reportingPersonId;

            baseService.paginationBase('Employees/HRDashboard/ROPersonWiseAbsentStatusList', pageno, $scope.paramReportingPersonWiseAbsentEmployee)
                .then(function (data) {
                    $scope.rpWiseAbsentList = data.Rows;
                    $scope.paramReportingPersonWiseAbsentEmployee.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetReportingPersonWiseAbsentEmployee();
        angular.element(document.querySelector('#rpWiseAbsent')).modal('show');
    };

    $scope.paramReportingPersonWiseLateEmployee = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: "EmployeeName",
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ModalORDivGetReportingPersonWiseLateEmployee = function () {

        baseService.setCurrentPage('rpWiseLateList');
        $scope.rpWiseLateList = [];
        $scope.GetReportingPersonWiseLateEmployee = function (pageno) {
            $scope.paramReportingPersonWiseLateEmployee.companyId = $scope.companyId;
            $scope.paramReportingPersonWiseLateEmployee.plantId = $scope.plantId;
            $scope.paramReportingPersonWiseLateEmployee.hrFromDate = $scope.hrRespPerJSFromDate;
            $scope.paramReportingPersonWiseLateEmployee.hrToDate = $scope.hrRespPerJSToDate;
            $scope.paramReportingPersonWiseLateEmployee.reportingPersonId = $scope.reportingPersonId;

            baseService.paginationBase('Employees/HRDashboard/ROPersonWiseLatetStatusList', pageno, $scope.paramReportingPersonWiseLateEmployee)
                .then(function (data) {
                    $scope.rpWiseLateList = data.Rows;
                    $scope.paramReportingPersonWiseLateEmployee.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetReportingPersonWiseLateEmployee();
        angular.element(document.querySelector('#rpWiseLate')).modal('show');
    };

    $scope.paramReportingPersonWiseLeaveEmployee = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: "EmployeeName",
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ModalORDivGetReportingPersonWiseLeaveEmployee = function () {

        baseService.setCurrentPage('rpWiseLeaveList');

        $scope.rpWiseLateList = [];

        $scope.GetReportingPersonWiseLeaveEmployee = function (pageno) {
            $scope.paramReportingPersonWiseLeaveEmployee.companyId = $scope.companyId;
            $scope.paramReportingPersonWiseLeaveEmployee.plantId = $scope.plantId;
            $scope.paramReportingPersonWiseLeaveEmployee.hrFromDate = $scope.hrRespPerJSFromDate;
            $scope.paramReportingPersonWiseLeaveEmployee.hrToDate = $scope.hrRespPerJSToDate;
            $scope.paramReportingPersonWiseLeaveEmployee.reportingPersonId = $scope.reportingPersonId;

            baseService.paginationBase('Employees/HRDashboard/ROPersonWiseLeavetStatusList', pageno, $scope.paramReportingPersonWiseLeaveEmployee)
                .then(function (data) {
                    $scope.rpWiseLeaveList = data.Rows;
                    $scope.paramReportingPersonWiseLeaveEmployee.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetReportingPersonWiseLeaveEmployee();
        angular.element(document.querySelector('#rpWiseLeave')).modal('show');
    };

    $scope.paramReportingPersonWiseWeekOffHolidayEmployee = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: "EmployeeName",
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ModalORDivGetReportingPersonWiseWeekOffHolidayEmployee = function () {

        baseService.setCurrentPage('rpWiseWeekOffHolidayList');

        $scope.rpWiseWeekOffHolidayList = [];

        $scope.GetROPersonWiseWeekOffHolidayEmployee = function (pageno) {
            $scope.paramReportingPersonWiseWeekOffHolidayEmployee.companyId = $scope.companyId;
            $scope.paramReportingPersonWiseWeekOffHolidayEmployee.plantId = $scope.plantId;
            $scope.paramReportingPersonWiseWeekOffHolidayEmployee.hrFromDate = $scope.hrRespPerJSFromDate;
            $scope.paramReportingPersonWiseWeekOffHolidayEmployee.hrToDate = $scope.hrRespPerJSToDate;
            $scope.paramReportingPersonWiseWeekOffHolidayEmployee.reportingPersonId = $scope.reportingPersonId;

            baseService.paginationBase('Employees/HRDashboard/ROPersonWiseWeekOffHolidayStatusList', pageno, $scope.paramReportingPersonWiseWeekOffHolidayEmployee)
                .then(function (data) {
                    $scope.rpWiseWeekOffHolidayList = data.Rows;
                    $scope.paramReportingPersonWiseWeekOffHolidayEmployee.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetROPersonWiseWeekOffHolidayEmployee();
        angular.element(document.querySelector('#rpWiseWeekOffHoliday')).modal('show');
    };

    $scope.paramReportingPersonWiseShiftNotAssignEmployee = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: "EmployeeName",
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ModalORDivGetReportingPersonShiftNotAssignEmployee = function () {

        baseService.setCurrentPage('rpWiseShiftNotAssignedList');

        $scope.rpWiseShiftNotAssignedList = [];

        $scope.GetROPersonWiseShiftNotAssignEmployeeEmployee = function (pageno) {
            $scope.paramReportingPersonWiseShiftNotAssignEmployee.companyId = $scope.companyId;
            $scope.paramReportingPersonWiseShiftNotAssignEmployee.plantId = $scope.plantId;
            $scope.paramReportingPersonWiseShiftNotAssignEmployee.hrFromDate = $scope.hrRespPerJSFromDate;
            $scope.paramReportingPersonWiseShiftNotAssignEmployee.hrToDate = $scope.hrRespPerJSToDate;
            $scope.paramReportingPersonWiseShiftNotAssignEmployee.reportingPersonId = $scope.reportingPersonId;

            baseService.paginationBase('Employees/HRDashboard/ModalROPHRDailyShiftNotAssignedStatusList', pageno, $scope.paramReportingPersonWiseShiftNotAssignEmployee)
                .then(function (data) {
                    $scope.rpWiseShiftNotAssignedList = data.Rows;
                    $scope.paramReportingPersonWiseShiftNotAssignEmployee.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetROPersonWiseShiftNotAssignEmployeeEmployee();
        angular.element(document.querySelector('#rpWiseShiftNotAssign')).modal('show');
    };


    $scope.paramReportingPersonWiseAttdnNotProcessedEmployee = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: "EmployeeName",
        searchBy: "FullName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ModalORDivGetReportingPersonAttdnNotProcessedEmployee = function () {

        baseService.setCurrentPage('rpWiseAttdnNotProcessedList');

        $scope.rpWiseAttdnNotProcessedList = [];

        $scope.GetROPersonWiseAttdnNotProcessedEmployee = function (pageno) {
            $scope.paramReportingPersonWiseAttdnNotProcessedEmployee.companyId = $scope.companyId;
            $scope.paramReportingPersonWiseAttdnNotProcessedEmployee.plantId = $scope.plantId;
            $scope.paramReportingPersonWiseAttdnNotProcessedEmployee.hrFromDate = $scope.hrRespPerJSFromDate;
            $scope.paramReportingPersonWiseAttdnNotProcessedEmployee.hrToDate = $scope.hrRespPerJSToDate;
            $scope.paramReportingPersonWiseAttdnNotProcessedEmployee.reportingPersonId = $scope.reportingPersonId;

            baseService.paginationBase('Employees/HRDashboard/ModalROHRDailyAttdnNotProcessedStatusList', pageno, $scope.paramReportingPersonWiseAttdnNotProcessedEmployee)
                .then(function (data) {
                    $scope.rpWiseAttdnNotProcessedList = data.Rows;
                    $scope.paramReportingPersonWiseAttdnNotProcessedEmployee.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetROPersonWiseAttdnNotProcessedEmployee();
        angular.element(document.querySelector('#rpWiseAttdnNotProcessed')).modal('show');
    };

    $scope.ModalEmpWiseDate = function (data) {
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $scope.ADGLUrl = 'Employees/HRDashboard/ModalEmployeeWiseAbsentDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrJSFromDate + "&hrToDate=" + $scope.hrJSToDate + "&employeeCode=" + data.EmployeeCode + "&dayCount=" + $scope.dayCount + "&comparator=" + $scope.absentComparator + "";

        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.dayList = response.data;
        });
        angular.element(document.querySelector('#DateWiseList')).modal('show');
    };

    $scope.ModalLateEmpWiseDate = function (data) {
        $scope.ADGLUrl = 'Employees/HRDashboard/ModalEmployeeWiseLateDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrLateJSFromDate + "&hrToDate=" + $scope.hrLateJSToDate + "&employeeCode=" + data.EmployeeCode + "&dayCount=" + $scope.dayCount + "&comparator=" + $scope.absentComparator + "";
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.lateDayList = response.data;
        });
        angular.element(document.querySelector('#LateDateWiseList')).modal('show');
    };

    $scope.prsDayList = [];
    $scope.ModalPresentEmpWiseDateList = function (data) {
        $scope.ADGLUrl = 'Employees/HRDashboard/ModalEmployeeWisePresentStatusDateWiseList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + data.fromDate + "&hrToDate=" + data.toDate + "&EmpSystemId=" + data.EmpSystemID + "&dayCount=" + $scope.diffDaysPresent + "&comparator=" + $scope.presentComparator + "";
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.prsDayList = response.data;
        });
        angular.element(document.querySelector('#PresentDateWiseList')).modal('show');
    };



    //$scope.ModalPresentEmpWiseDate = function (data) {
    //    $scope.ADGLUrl = 'Employees/HRDashboard/ModalEmployeeWisePresentDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&hrFromDate=" + $scope.hrLateJSFromDate + "&hrToDate=" + $scope.hrPresentJSToDate + "&employeeCode=" + data.EmployeeCode + "&dayCount=" + $scope.dayCount + "&comparator=" + $scope.absentComparator + "";
    //    $scope.label = data.EmployeeName;
    //    $scope.empCode = data.EmployeeCode;
    //    $http({
    //        method: 'GET',
    //        url: $scope.ADGLUrl,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.lateDayList = response.data;
    //    });
    //    angular.element(document.querySelector('#LateDateWiseList')).modal('show');
    //};

    $scope.ModalConsecutiveAbsentDateList = function (data) {
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $scope.ADGLUrl = 'Employees/HRDashboard/ModalConsecutiveAbsentDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&empSystemID=" + data.EmpSystemID + "&hrDate=" + $scope.hrDate;

        $http({
            method: 'GET',
            url: $scope.ADGLUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.absentDayList = response.data;
        });
        angular.element(document.querySelector('#ConsecutiveAbsentDateList')).modal('show');
    };

    $scope.ModalConsecutiveLateDateList = function (data) {
        $scope.label = data.EmployeeName;
        $scope.empCode = data.EmployeeCode;
        $scope.Url = 'Employees/HRDashboard/ModalConsecutiveLateDateList?companyId=' + $scope.companyId + "&plantId=" + $scope.plantId + "&empSystemID=" + data.EmpSystemID + "&hrDate=" + $scope.hrDate;

        $http({
            method: 'GET',
            url: $scope.Url,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.latetDayList = response.data;
        });
        angular.element(document.querySelector('#ConsecutiveLateDateList')).modal('show');
    };
    $scope.GetModalHRLongAbsentDetailList = function () {
        angular.element(document.querySelector('#consecutive10DaysAbsentModal')).modal('show');
    };

    //------------------------Responsible Person Wise Attendance Status End-----------------------------------//

    //----------------------2nd Part End------------------------//


    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    //#region ----Lunch out Dashboard Part----
    $scope.appointments = [];

    //#region Month Select 

    $scope.LunchOut = {
        YearNo: null,
        MonthNo: null
    };
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

    $scope.yearlist = [];
    $scope.GetCbo = function () {
        $http.get('Attendances/AttendanceProcessUI/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.yearlist = [];
                        $scope.yearlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();
    var d = new Date();
    $scope.LunchOut.YearNo = '' + d.getFullYear();
    var xx = d.getMonth() + 1;
    $scope.LunchOut.MonthNo = '' + xx;
    //#endregion

    //#region Get Data

    $scope.IsColorON = true;

    $scope.SimulateVisual = function () {
        var _data = { 'Year': $scope.LunchOut.YearNo, 'Month': $scope.LunchOut.MonthNo };
        var _path = 'Attendances/LunchOutDashboard/GetAttendanceData';

        try {
            $http({
                method: 'POST',
                url: _path,
                data: _data
            }).then(function successCallback(res) {

                if (res.data.DATA.length > 0) {
                    $scope.resourcedata2 = {
                        dataSource: res.data.GROUPDATA,
                        text: "text", id: "id", groupId: "groupId", color: "color"
                    };
                    $scope.appointments = angular.copy(res.data.DATA);
                }
            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }



    $scope.ResetAttendanceGrid = function () {
        var gridObj = $("#GridEdit").ejGrid("instance");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

    $scope.rowDataBound = function rowDataBound(e) {
        try {
            if (e.column.field.endsWith(".DayStatus")) {
                if (e.text) {

                    var Col = e.column.field.replace(".DayStatus", "");
                    if ($scope.IsColorON == true) {
                        e.cell.bgColor = e.data[Col].Color;
                    }
                    else {

                        e.cell.bgColor = e.data[Col].LColor;
                    }

                }
            }
        } catch (e) {

        }
    }

    $scope.ShowDiv = false;
    $scope.GetValue = function (obj) {
        $scope.XX = obj.data.EmpSystemID;
        try {
            $scope.Date = obj.columnName;
            $scope.TDate = new Date($scope.LunchOut.YearNo, $scope.LunchOut.MonthNo, $scope.Date);
            $scope.Sdate = $scope.TDate.setMonth($scope.TDate.getMonth() - 1);
            $scope.QDate = new Date($scope.Sdate);
            var date = $scope.QDate, y = date.getFullYear(), m = date.getMonth();
            $scope.FinalDate = $filter('dateFiltering')(new Date(y, m, $scope.Date), 'dd-MM-yyyy');

            $scope.ShowDiv = true;
            var eDialog = $("#Base").data("ejDialog");
            eDialog.open();
            $scope.GetEmpData($scope.XX, $scope.FinalDate);
            $scope.GetRawData($scope.XX, $scope.FinalDate);
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    //#endregion
    $scope.tab = 40;
    //#region Job Card Donwload
    $scope.GetConsumption = function (obj) {
        $scope.ReportFormat = "Excel";
        $scope.FromDate = new Date($scope.LunchOut.YearNo, $scope.LunchOut.MonthNo, 1); //Get Selected Year,Month No, Date
        $scope.Xdate = $scope.FromDate.setMonth($scope.FromDate.getMonth() - 1);
        $scope.YDate = new Date($scope.Xdate);
        var date = $scope.YDate, y = date.getFullYear(), m = date.getMonth();
        $scope.EmpId = obj.data.EmpSystemID;
        $scope.firstDate = $filter('dateFiltering')(new Date(y, m, 1), 'dd-MM-yyyy');
        $scope.MonthLastDate = $filter('dateFiltering')(new Date(y, m + 1, 0), 'dd-MM-yyyy');

        var url = 'Attendances/ComplianceAttendanceSetting/GetComplianceJobCardReport?reportFormat=' + $scope.ReportFormat + '&fromDate=' + $scope.firstDate + '&toDate=' + $scope.MonthLastDate + '&employeeId=' + $scope.EmpId + '&chkAdditionInfo=' + true;
        $rootScope.report(url);
    }
    //#endregion

    //#region Get Data From Selected Employee.
    $scope.EmpModel = {
        SystemId: null,
        EmployeeCode: null,
        EmployeeName: null,
        WorkDate: null,
        DayStatus: null,
        ShiftName: true,
        ShiftInTime: null,
        ShiftOutTime: null,
        PunchInTime: null,
        PunchOutTime: null,
        LunchInTime: null,
        LunchOutTime: null,
        LateDuration: '',
        LeaveName: null,
        LeaveFrom: null,
        LeaveTo: null,
        LeaveDays: null,
        PreviousDate: null,
        PreviousDateInTime: null,
        PreviousDateOutTime: null,
        PreviousDayStatus: null,
        NextDate: null,
        NextDateInTime: null,
        NextDateOutTime: null,
        NextDayStatus: null,
        TodaysDate: null,

        ShiftLOutTime: null,
        ShiftLInTime: null,
        LoutTime: null,
        LIntime: null,
        LLatetime: null,
    };
    $scope.clr = null;
    $scope.clor = null;
    $scope.color = null;
    $scope.GetEmpData = function (EmpId, Date) {
        try {
            $http({
                method: 'GET',
                url: 'Attendances/LunchOutDashboard/GetEmployeeData?EmpId=' + EmpId + '&Date=' + Date,
            }).then(function successCallback(response) {
                //angular.copy(response.data[0], $scope.EmpModel);
                $scope.EmpModel = response.data[0];
                if ($scope.EmpModel.IsManualDayStatus == true) {
                    $scope.clr = "red";
                } else {
                    $scope.clr = "Black";
                }
                if ($scope.EmpModel.IsManualInTime == true) {
                    $scope.clor = "red";
                } else {
                    $scope.clor = "Black";
                }
                if ($scope.EmpModel.IsManualOutTime == true) {
                    $scope.color = "red";
                } else {
                    $scope.color = "Black";
                }
            });
        } catch (e) {

        }
    }
    $scope.EmpDataList = [];
    $scope.GetRawData = function (EmpId, Date) {
        try {
            $http({
                method: 'GET',
                url: 'Attendances/LunchOutDashboard/GetRawData?EmpId=' + EmpId + '&Date=' + Date,
            }).then(function successCallback(response) {
                //angular.copy(response.data[0], $scope.EmpModel);
                $scope.EmpDataList = response.data;
            });
        } catch (e) {

        }
    }

    

    //#endregion

    //#endregion
}