ChartController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', '$timeout'];
function ChartController($scope, $http, $location, $rootScope, $window, $compile, baseService, $timeout) {
    $("#lineDash").hide();
    $("#cumulitive").hide();
    $scope.Colname = "";
    $scope.ColList = [];
    $scope.chartList = [];
    $scope.list = [];
    $scope.index = -1;
    $scope.chartLabel = [];
    $scope.stIndex = -2;//status Index

    var myChart;
    function getColList() {
        $http({
            method: 'GET',
            url: 'GetGroupWiseColumnJList?companyGroupId=' + $rootScope.CompanyGroupId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    var row = {
                        Id: null,
                        ColumnName: null,
                        AplosColumnName: null,
                        Sequence: -2,
                        Text: null
                    }
                    row.Id = response.data[i].AplosEmpFieldId;
                    row.AplosColumnName = response.data[i].AplosColumnName;
                    row.ColumnName = response.data[i].ColumnName;
                    row.Text = response.data[i].AplosColumnName;
                    row.Sequence = i;
                    $scope.ColList.push(row);
                }
            }
        });
    }

    function createColList() {
        if (baseService.arrayLength($scope.list) > 0) {
            var row = {
                Id: null,
                ColumnName: null,
                AplosColumnName: null,
                Sequence: -2,
                Text: null
            }

            row.Id = $scope.list[0].CompanyGroupId;
            row.AplosColumnName = "Group";
            row.Sequence = -2;
            row.Text = $scope.list[0].GroupName;
            $scope.ColList.push(row);
            var rowc = {
                Id: null,
                ColumnName: null,
                AplosColumnName: null,
                Sequence: -1,
                Text: null
            }

            rowc.Id = $scope.list[0].CompanyId;
            rowc.AplosColumnName = "Company";
            rowc.Sequence = -1;
            rowc.Text = $scope.list[0].CompanyName;
            $scope.ColList.push(rowc);
            getColList();
        }
    }

    function createChart(list) {
        //function setList(list) {
        $scope.chartList = [];
        $scope.chartLabel = [];
        var cmpTotalEmp = 0;
        var cmpNotLEmp = 0
        var cmpSubmitted = 0;
        var cmpNotSubmitted = 0;
        $scope.list = list;
        angular.forEach(list, function (item, i) {
            cmpTotalEmp += item.totalEmployee;
            cmpNotLEmp += item.NotLoggedIn;
            cmpSubmitted += item.Submitted;
            cmpNotSubmitted += item.NotSubmitted;
        });
        $scope.CmpTotalEmp = cmpTotalEmp;
        $scope.CmpNotLEmp = cmpNotLEmp;
        $scope.CmpSubmitted = cmpSubmitted;
        $scope.CmpNotSubmitted = cmpNotSubmitted;
        $scope.chartList.push($scope.CmpNotLEmp);

        $scope.chartList.push($scope.CmpSubmitted);
        $scope.chartList.push($scope.CmpNotSubmitted);
        $scope.chartLabel = ["Not Logged In", "Submitted", "Not Submitted"];
        //}

        var ctx = document.getElementById("myChart").getContext('2d');
        if (myChart != undefined && typeof myChart == 'object' && typeof myChart.destroy == 'function') myChart.destroy();
        myChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartLabel,
                datasets: [{
                    label: '',
                    data: $scope.chartList,
                    backgroundColor: [
                        'rgba(231, 76, 60,1.0)',
                        'rgba(46, 204, 113,1.0)',
                        'rgba(241, 196, 15, 1.0)'

                    ],
                    borderColor: [
                        'rgba(231, 76, 60,1.0)',
                        'rgba(46, 204, 113,1.0)',
                        'rgba(241, 196, 15, 1.0)'

                    ],
                    borderWidth: 1
                }]
            },
            options: {
                legend: {
                    position: 'bottom',
                    onClick: (e) => e.stopPropagation()
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
                            var precentage = (((currentValue / total) * 100) + 0.0).toFixed(2);
                            return precentage + "%";
                        }
                    }
                }
            }
        });

        //var myChart = new Chart(
    }

    $scope.getColName = function (seq) {
        if (seq = -1) {
            return "Company";
        }
        else {
            if (seq != -2) {
                return $scope.ColList[seq].AplosColumnName;
            }
        }
    }

    $scope.loadData = function () {
        $http({
            method: 'GET',
            url: 'GetGroupWiseCompanyList?companyGroupId=' + $rootScope.CompanyGroupId,
            //param: { 'companyGroupId': $rootScope.CompanyGroupId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            createChart(response.data);
            createColList();
        });
    }

    $scope.headerNav = function (x) {
        if (x.Sequence != -2) {
            $scope.setIndexHead(x);
            $scope.GetCompanyWisePList(x.Text, null);
        }
        else {
            $scope.setIndexHead(x);
            $http({
                method: 'GET',
                url: 'GetGroupWiseCompanyList?companyGroupId=' + $rootScope.CompanyGroupId,
                //data: { 'companyGroupId': $rootScope.CompanyGroupId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                //setList(response.data);

                createChart(response.data);
                $scope.index = -1;
                $scope.stIndex = $scope.index - 1
            });
        }
    }

    $scope.setIndex = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence == $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UserName;
            }
        }
    }

    $scope.setIndexHead = function (x) {
        $scope.index = x.Sequence;
    }

    $scope.GetCompanyWisePList = function (text, data) {
        if ($scope.index + 3 < $scope.ColList.length) {
            $http({
                method: 'POST',
                url: 'GetDetailsJList/',
                data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'cgid': $rootScope.CompanyGroupId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                createChart(response.data);
                $scope.index += 1;
                $scope.stIndex = $scope.index - 1;
                $scope.getColName($scope.index);
            });
        }
    }

    function getCol(seq) {
        for (var i = 0; i < baseService.arrayLength($scope.ColList); i++) {
            if ($scope.ColList[i].Sequence == seq) {
                return $scope.ColList[i].AplosColumnName;
            }
        }
    }

    $scope.loadData();

    $scope.NLEData = function (data) {
        $scope.NLEList = [];
        $scope.setModal(data);
        $http({
            method: 'POST',
            url: 'ModalNotLoggedInEmployeeList/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'cgid': $rootScope.CompanyGroupId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NLEList = response.data;
            angular.element(document.querySelector('#NLIEModal')).modal('show');
        });
    }

    //------------Modal function Submitted Employee----------------//
    $scope.SEData = function (data) {
        $scope.SEList = [];
        $scope.setModal(data);
        $http({
            method: 'POST',
            url: 'ModalSubmittedEmployeeList/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'cgid': $rootScope.CompanyGroupId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SEList = response.data;
            angular.element(document.querySelector('#SEModal')).modal('show');
        });
    }

    //------------Modal function for Not Submitted Employee----------------//
    $scope.NSEData = function (data) {
        $scope.NSEList = [];
        $scope.setModal(data);
        $http({
            method: 'POST',
            url: 'ModalNotSubmittedEmployeeList/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.index, 'cgid': $rootScope.CompanyGroupId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NSEList = response.data;
            angular.element(document.querySelector('#NSEModal')).modal('show');
        });
    }

    $scope.setModal = function (x) {
        for (var i = 0; i < $scope.ColList.length; i++) {
            if ($scope.ColList[i].Sequence == $scope.index) {
                $scope.ColList[i].Id = x.CompanyId;
                $scope.ColList[i].Text = x.UserName;
            }
        }
    }

    $scope.sNLEData = function () {
        $scope.sNLEList = [];
        $http({
            method: 'POST',
            url: 'sModalNotLoggedInEmployeeList/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'cgid': $rootScope.CompanyGroupId/*, 'click': click*/ },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.sNLEList = response.data;
            angular.element(document.querySelector('#stNLIEModal')).modal('show');
        });
    }

    $scope.sSEData = function () {
        $scope.sSEList = [];
        $http({
            method: 'POST',
            url: 'sModalSubmittedEmployeeList/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'cgid': $rootScope.CompanyGroupId/*, 'click': click*/ },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.sSEList = response.data;
            angular.element(document.querySelector('#stSEModal')).modal('show');
        });
    }

    $scope.sNSEData = function () {
        $scope.sNSEList = [];
        $http({
            method: 'POST',
            url: 'sModalNotSubmittedEmployeeList/',
            data: { 'ChartColumnList': $scope.ColList, 'seq': $scope.stIndex, 'cgid': $rootScope.CompanyGroupId/*, 'click': click*/ },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.sNSEList = response.data;
            angular.element(document.querySelector('#stNSEModal')).modal('show');
        });
    }

    //---------------Line DashBoard------------------------------------------------------//
    $scope.summaryLineChart = function () {
        $scope.date = [];
        $scope.tA = [];
        $scope.fdate = [];
        $scope.floggedIn = [];
        $scope.sdate = [];
        $scope.sloggedIn = [];
        $scope.dDate = [];
        $scope.tDoc = [];
        $scope.totalFirstLoggedIn = function () {
            var totalFL = 0;
            $scope.totalFlC = [];
            $http({
                method: 'POST',
                url: 'JFirstLoggedIn/',
                data: { 'cgid': $rootScope.CompanyGroupId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.fdate.push(item.FirstLoginTime);
                    totalFL += item.TFirstLogin;
                    $scope.totalFlC.push(totalFL);
                    $scope.floggedIn.push(item.TFirstLogin);
                });
                var fctx = document.getElementById("flineChart").getContext('2d');
                var flineChart = new Chart(fctx, {
                    type: 'line',
                    data: {
                        labels: $scope.fdate,
                        datasets: [{
                            label: 'Discrete - New Logged In',
                            data: $scope.floggedIn,
                            backgroundColor: 'rgba(75, 192, 192, 0.5)',
                            borderColor: 'rgba(75, 192, 192, 1)',
                            fill: false,
                            borderWidth: 2,
                        }
                        ]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                }
                            }]
                        }
                    }
                });
                var fcumuctx = document.getElementById("fcumuChart").getContext('2d');
                var fcumuChart = new Chart(fcumuctx, {
                    type: 'line',
                    data: {
                        labels: $scope.fdate,
                        datasets: [{
                            label: 'Cumulative - New Logged In',
                            data: $scope.totalFlC,
                            backgroundColor: 'rgba(75, 192, 192, 0.5)',
                            borderColor: 'rgba(75, 192, 192, 1)',
                            fill: false,
                            borderWidth: 2,
                        }
                        ]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                    onClick: (e) => e.stopPropagation()
                                }
                            }]
                        }
                    }
                });
            });
        }

        $scope.dayWiseSubmit = function () {
            var totalSE = 0;
            $scope.totalASE = [];
            $http({
                method: 'POST',
                url: 'JDayWiseSubmit/',
                data: { 'cgid': $rootScope.CompanyGroupId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.sdate.push(item.SubmitTime);
                    totalSE += item.totalSubmit;
                    $scope.totalASE.push(totalSE);
                    $scope.sloggedIn.push(item.totalSubmit);
                });
                var sctx = document.getElementById("slineChart").getContext('2d');
                var sChart = new Chart(sctx, {
                    type: 'line',
                    data: {
                        labels: $scope.sdate,
                        datasets: [{
                            label: 'Discrete - Submitted',
                            data: $scope.sloggedIn,
                            backgroundColor: 'rgba(46, 204, 113,0.5)',
                            borderColor: 'rgba(46, 204, 113,1.0)',
                            fill: false,
                            borderWidth: 2,
                        }]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                }
                            }]
                        }
                    }
                });

                var scumuctx = document.getElementById("scumuChart").getContext('2d');
                var scumuChart = new Chart(scumuctx, {
                    type: 'line',
                    data: {
                        labels: $scope.sdate,
                        datasets: [{
                            label: 'Cumulative - Submitted',
                            data: $scope.totalASE,
                            backgroundColor: 'rgba(46, 204, 113,0.5)',
                            borderColor: 'rgba(46, 204, 113,1.0)',
                            fill: false,
                            borderWidth: 2,
                        }]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                    onClick: (e) => e.stopPropagation()
                                }
                            }]
                        }
                    }
                });
            });
        }

        $scope.totalActivity = function () {
            var totalAct = 0;
            $scope.DtotalAct = [];
            $http({
                method: 'POST',
                url: 'JtotalActivity/',
                data: { 'cgid': $rootScope.CompanyGroupId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.date.push(item.AddedDateTime);
                    totalAct += item.totalActivity
                    $scope.DtotalAct.push(totalAct);
                    $scope.tA.push(item.totalActivity);
                });
                var alctx = document.getElementById("alineChart").getContext('2d');
                var alineChart = new Chart(alctx, {
                    type: 'line',
                    data: {
                        labels: $scope.date,
                        datasets: [{
                            label: 'Discrete - Activity',
                            data: $scope.tA,
                            backgroundColor: 'rgba(255, 99, 132, 0.5)',
                            borderColor: 'rgba(255,99,132,1)',
                            fill: false,
                            borderWidth: 2
                        }]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                    onClick: (e) => e.stopPropagation()
                                }
                            }]
                        }
                    }
                });

                var acumuctx = document.getElementById("acumuChart").getContext('2d');
                var acumuChart = new Chart(acumuctx, {
                    type: 'line',
                    data: {
                        labels: $scope.date,
                        datasets: [{
                            label: 'Cumulative - Activity',
                            data: $scope.DtotalAct,
                            backgroundColor: 'rgba(255, 99, 132, 0.5)',
                            borderColor: 'rgba(255,99,132,1)',
                            fill: false,
                            borderWidth: 2
                        }]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                    onClick: (e) => e.stopPropagation()
                                }
                            }]
                        }
                    }
                });
            });
        }

        $scope.totalDocument = function () {
            var totalDoc = 0;
            $scope.DtotalDocumnet = [];
            $http({
                method: 'POST',
                url: 'JtotalDocument/',
                data: { 'cgid': $rootScope.CompanyGroupId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.dDate.push(item.DAddedDate);
                    totalDoc += item.totalDocument;
                    $scope.DtotalDocumnet.push(totalDoc);
                    $scope.tDoc.push(item.totalDocument);
                });
                var dlctx = document.getElementById("dlineChart").getContext('2d');
                var dlineChart = new Chart(dlctx, {
                    type: 'line',
                    data: {
                        labels: $scope.dDate,
                        datasets: [{
                            label: 'Discrete - Document',
                            data: $scope.tDoc,
                            backgroundColor: 'rgba(26, 188, 156,0.5)',
                            borderColor: 'rgba(26, 188, 156,1.0)',
                            fill: false,
                            borderWidth: 2
                        }]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                    onClick: (e) => e.stopPropagation()
                                }
                            }]
                        }
                    }
                });

                var dcumuctx = document.getElementById("dcumuChart").getContext('2d');
                var dcumuChart = new Chart(dcumuctx, {
                    type: 'line',
                    data: {
                        labels: $scope.date,
                        datasets: [{
                            label: 'Cumulative - Document',
                            data: $scope.DtotalDocumnet,
                            backgroundColor: 'rgba(26, 188, 156,0.5)',
                            borderColor: 'rgba(26, 188, 156,1.0)',
                            fill: false,
                            borderWidth: 2
                        }]
                    },
                    options: {
                        legend: {
                            onClick: (e) => e.stopPropagation()
                        },
                        scales: {
                            yAxes: [{
                                ticks: {
                                    beginAtZero: true,
                                    onClick: (e) => e.stopPropagation()
                                }
                            }]
                        }
                    }
                });
            });
        }
    }

    $("#back").click(function () {
        $("#summaryPie").show(500);
        $("#navigMamun").show(500);
        $("#headerNav").show(500);
        $("#cumulitive").hide(500);
        $("#lineDash").hide(500);
    });

    $("#back2").click(function () {
        $("#summaryPie").hide(500);
        $("#navigMamun").hide(500);
        $("#headerNav").hide(500);
        $("#cumulitive").hide(500);
        $("#lineDash").show(500);
        $(this).hide();
        $("#forward").show();
        $("#back").show();
    });

    $("#forward").click(function () {
        $("#cumulitive").show(500);
        $("#summaryPie").hide();
        $("#navigMamun").hide();
        $("#headerNav").hide();
        $(this).hide();
        $("#back").hide();
        $("#navigMamun").hide(500);
        $("#lineDash").hide();
        $("#back2").show();
    });

    $("#Cbtn2_1").click(function () {
        $("#summaryPie").hide(500);
        $("#navigMamun").hide(500);
        $("#headerNav").hide(500);
        $("#cumulitive").hide(500);
        $("#lineDash").show(500);
        $("#back2").show();
        $("#forward").show();
    });

    $("#Cbtn3_1").click(function () {
        $("#cumulitive").show(500);
        $("#summaryPie").hide();
        $("#navigMamun").hide();
        $("#headerNav").hide();
        $("#back2").show();
        $("#lineDash").hide();
    });

    $("#Cbtn1_2").click(function () {
        $("#summaryPie").show(500);
        $("#navigMamun").show(500);
        $("#headerNav").show(500);
        $("#cumulitive").hide(500);
        $("#lineDash").hide(500);
    });

    $("#Cbtn3_2").click(function () {
        $("#summaryPie").hide(500);
        $("#navigMamun").hide(500);
        $("#headerNav").hide(500);
        $("#cumulitive").show(500);
        $("#lineDash").hide(500);
        $("#back2").show();
    });

    $("#Cbtn1_3").click(function () {
        $("#summaryPie").show(500);
        $("#navigMamun").show(500);
        $("#headerNav").show(500);
        $("#cumulitive").hide(500);
        $("#lineDash").hide(500);
    });

    $("#Cbtn2_3").click(function () {
        $("#summaryPie").hide(500);
        $("#navigMamun").hide(500);
        $("#headerNav").hide(500);
        $("#cumulitive").hide(500);
        $("#lineDash").show(500);
        $("#forward").show();
        $("#back").show();
    });

    $("#forward1").click(function () {
        $("#summaryPie").hide(500);
        $("#navigMamun").hide(500);
        $("#headerNav").hide(500);
        $("#lineDash").show(500);
        $("#cumulitive").hide(500);
    });
}