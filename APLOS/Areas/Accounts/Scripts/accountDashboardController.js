// JavaScript source code
'use strict';
accountDashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function accountDashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $scope.chartList = [];
    $scope.list = [];
    $scope.index = -1;
    $scope.chartLabel = [];
    $scope.ColList = [];
    $scope.stIndex = -2;
    var AccPChart;
    var AccRChart;
    var partyData = [];

    /*Date calculation*/
    Date.prototype.addDays = function (days) {
        this.setDate(this.getDate() + parseInt(days));
        return this;
    };
    var currentDate = new Date();
    var cDate = $filter('date')(new Date(), 'MM-dd-yyyy');
    var currDate = new Date(cDate);

    // $scope.preRecruitmentEmployees[i].DOJ = $filter('dateFiltering')(new Date());

    var n7d = $filter('date')(currentDate.addDays(7), 'MM-dd-yyyy');
    var next7days = new Date(n7d);

    $scope.AccountDashboardDropDown = {
        PartyId: null,
        CurrencyId: null
    };

    //cboService.getCboParty(function (result) {
    //    $scope.PartyList = result;
    //});

    $scope.CurrencyParallel = function () {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
        }).then(function successCallback(response) {
            $scope.CurrencyParal = response.data;
            $scope.AccountDashboardDropDown.CurrencyId = response.data[0].CurrencyId;
        });
    };
    $scope.CurrencyParallel();

    $scope.GetaccountRecievableWithPartyCurrency = function () {
        var totalBalance = 0;
        $scope.overAllBalance = [];
        $http({
            method: 'GET',
            url: 'accounts/accountdashboard/GetOverAllReceivableWithPartyCurrency',
            params: {
                'partyId': $scope.AccountDashboardDropDown.PartyId,
                'currencyId': $scope.AccountDashboardDropDown.CurrencyId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            setList(response.data);
            createChart();
        });
        
        //$scope.overAllBalance = [];
        //$http({
        //    method: 'GET',
        //    url: 'accounts/accountdashboard/GetOverAllReceivableWithPartyCurrency',
        //    params: {
        //        'partyId': $scope.AccountDashboardDropDown.PartyId,
        //        'currencyId': $scope.AccountDashboardDropDown.CurrencyId
        //    },
        //    dataType: 'JSON'
        //}).then(function successCallback(response) {
        //    $scope.accountRecievablePartyList = response.data;
        //    setList(response.data);
        //    createChart();
        //});
    };
    $scope.GetaccountRecievableWithPartyCurrency();

    function setList(list) {
        $scope.date = new Date();
        $scope.chartLabel = [];
        var RecToday = 0;
        var RecN7Today = 0;
        var RecOtherToday = 0;
        var OverDue = 0;
        var RtotalBalance = 0;
        $scope.list = list;

        angular.forEach(list, function (item, i) {
            RtotalBalance += item.Balance;
            //var date = new Date(item.CMaturedDate);
            var rMdbdate = $filter('date')(item.CMaturedDate, 'MM-dd-yyyy');

            var RMdate = new Date(rMdbdate);

            if (RMdate === currDate) {
                RecToday += item.Balance;
            }

            else if (RMdate <= next7days && RMdate > currDate) {
                RecN7Today += item.Balance;
            }
            else if (RMdate > next7days) {
                RecOtherToday += item.Balance;
            }
            else if (RMdate < currDate) {
                OverDue += item.Balance;
            }
        });

        $scope.chartList = [];
        $scope.chartList.push(RecToday);
        $scope.chartList.push(RecN7Today);
        $scope.chartList.push(RecOtherToday);
        $scope.chartList.push(OverDue);
        $scope.RecToday = RecToday;
        $scope.RecN7Today = RecN7Today;
        $scope.RecOtherToday = RecOtherToday;
        $scope.OverDue = OverDue;
        $scope.RtotalBalance = RtotalBalance;
        $scope.chartLabel = ['Today', 'Next 7 days', 'Others', 'OverDue'];
    }

    function createChart() {
        Chart.defaults.doughnutLabels = Chart.helpers.clone(Chart.defaults.doughnut);

        var helpers = Chart.helpers;
        var defaults = Chart.defaults;

        Chart.controllers.doughnutLabels = Chart.controllers.doughnut.extend({
            updateElement: function (arc, index, reset) {
                var _this = this;
                var chart = _this.chart,
                    chartArea = chart.chartArea,
                    opts = chart.options,
                    animationOpts = opts.animation,
                    arcOpts = opts.elements.arc,
                    centerX = (chartArea.left + chartArea.right) / 2,
                    centerY = (chartArea.top + chartArea.bottom) / 2,
                    startAngle = opts.rotation, // non reset case handled later
                    endAngle = opts.rotation, // non reset case handled later
                    dataset = _this.getDataset(),
                    circumference = reset && animationOpts.animateRotate ? 0 : arc.hidden ? 0 : _this.calculateCircumference(dataset.data[index]) * (opts.circumference / (2.0 * Math.PI)),
                    innerRadius = reset && animationOpts.animateScale ? 0 : _this.innerRadius,
                    outerRadius = reset && animationOpts.animateScale ? 0 : _this.outerRadius,
                    custom = arc.custom || {},
                    valueAtIndexOrDefault = helpers.getValueAtIndexOrDefault;

                helpers.extend(arc, {
                    // Utility
                    _datasetIndex: _this.index,
                    _index: index,

                    // Desired view properties
                    _model: {
                        x: centerX + chart.offsetX,
                        y: centerY + chart.offsetY,
                        startAngle: startAngle,
                        endAngle: endAngle,
                        circumference: circumference,
                        outerRadius: outerRadius,
                        innerRadius: innerRadius,
                        label: valueAtIndexOrDefault(dataset.label, index, chart.data.labels[index])
                    },

                    draw: function () {
                        var ctx = this._chart.ctx,
                            vm = this._view,
                            sA = vm.startAngle,
                            eA = vm.endAngle,
                            opts = this._chart.config.options;

                        var labelPos = this.tooltipPosition();
                        var segmentLabel = vm.circumference / opts.circumference * 100;

                        ctx.beginPath();

                        ctx.arc(vm.x, vm.y, vm.outerRadius, sA, eA);
                        ctx.arc(vm.x, vm.y, vm.innerRadius, eA, sA, true);

                        ctx.closePath();
                        ctx.strokeStyle = vm.borderColor;
                        ctx.lineWidth = vm.borderWidth;

                        ctx.fillStyle = vm.backgroundColor;

                        ctx.fill();
                        ctx.lineJoin = 'bevel';

                        if (vm.borderWidth) {
                            ctx.stroke();
                        }

                        if (vm.circumference > 0.15) { // Trying to hide label when it doesn't fit in segment
                            ctx.beginPath();
                            ctx.font = helpers.fontString(opts.defaultFontSize, opts.defaultFontStyle, opts.defaultFontFamily);
                            ctx.fillStyle = "#fff";
                            ctx.textBaseline = "top";
                            ctx.textAlign = "center";

                            // Round percentage in a way that it always adds up to 100%
                            ctx.fillText(segmentLabel.toFixed(0) + "%", labelPos.x, labelPos.y);
                        }
                    }
                });

                var model = arc._model;
                model.backgroundColor = custom.backgroundColor ? custom.backgroundColor : valueAtIndexOrDefault(dataset.backgroundColor, index, arcOpts.backgroundColor);
                model.hoverBackgroundColor = custom.hoverBackgroundColor ? custom.hoverBackgroundColor : valueAtIndexOrDefault(dataset.hoverBackgroundColor, index, arcOpts.hoverBackgroundColor);
                model.borderWidth = custom.borderWidth ? custom.borderWidth : valueAtIndexOrDefault(dataset.borderWidth, index, arcOpts.borderWidth);
                model.borderColor = custom.borderColor ? custom.borderColor : valueAtIndexOrDefault(dataset.borderColor, index, arcOpts.borderColor);

                // Set correct angles if not resetting
                if (!reset || !animationOpts.animateRotate) {
                    if (index === 0) {
                        model.startAngle = opts.rotation;
                    } else {
                        model.startAngle = _this.getMeta().data[index - 1]._model.endAngle;
                    }

                    model.endAngle = model.startAngle + model.circumference;
                }

                arc.pivot();
            }
        });

        var config = {
            type: 'doughnutLabels',
            data: {
                datasets: [{
                    data: $scope.chartList,
                    backgroundColor: ['rgba(46, 204, 113,.6)', 'rgba(241, 196, 15,.6)', 'rgba(191, 204, 113,.6)', 'rgba(240, 52, 52, .6)']
                }],
                labels: $scope.chartLabel
            },
            options: {
                responsive: true,
                legend: {
                    onClick: (e) => e.stopPropagation(),
                    display: false,
                    position: 'bottom'
                },
                title: {
                    display: true,
                    text: 'Account Receivable Chart'
                },
                animation: {
                    animateScale: true,
                    animateRotate: true
                }
            }
        };

        var ARctx = document.getElementById("AcRChart").getContext("2d");
        if (AccRChart !== undefined && typeof AccRChart === 'object' && typeof AccRChart.destroy === 'function') AccRChart.destroy();

        AccRChart = new Chart(ARctx, config);
    }

    $scope.GetaccountPayableWithPartyCurrency = function () {
        var totalBalance = 0;
        $scope.overAllBalance = [];
        $http({
            method: 'GET',
            url: 'accounts/accountdashboard/GetOverAllPayableWithPartyCurrency',
            params: {
                'partyId': $scope.AccountDashboardDropDown.PartyId,
                'currencyId': $scope.AccountDashboardDropDown.CurrencyId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            psetList(response.data);
            pcreateChart();
        });
    };

    $scope.GetaccountPayableWithPartyCurrency();

    function psetList(list) {
        $scope.chartLabel = [];
        var pPayToday = 0;
        var pPayN7Today = 0;
        var pPayOtherToday = 0;
        var pPayOverDue = 0;
        var PtotalBalance = 0;
        $scope.list = list;

        angular.forEach(list, function (item, i) {
            PtotalBalance += item.Balance;
            var pMDBdate = $filter('date')(item.PMaturedDate, 'MM-dd-yyyy');
            var payDate = new Date(pMDBdate);

            if (payDate === currDate) {
                pPayToday += item.Balance;
            }

            else if (payDate <= next7days && payDate > currDate) {
                pPayN7Today += item.Balance;
            }
            else if (payDate > next7days) {
                pPayOtherToday += item.Balance;
            }
            else if (payDate < currDate) {
                pPayOverDue += item.Balance;
            }
        });

        $scope.chartList = [];
        $scope.chartList.push(pPayToday);
        $scope.chartList.push(pPayN7Today);
        $scope.chartList.push(pPayOtherToday);
        $scope.chartList.push(pPayOverDue);
        $scope.pPayToday = pPayToday;
        $scope.pPayN7Today = pPayN7Today;
        $scope.pPayOtherToday = pPayOtherToday;
        $scope.pPayOverDue = pPayOverDue;
        $scope.PtotalBalance = PtotalBalance;

        $scope.chartLabel = ['Today', 'Next 7 days', 'Others', 'OverDue'];
    }

    function pcreateChart() {
        Chart.defaults.global.legend.display = false;
        var AccPctx = document.getElementById("AccPChart").getContext('2d');
        if (AccPChart !== undefined && typeof AccPChart === 'object' && typeof AccPChart.destroy === 'function') AccPChart.destroy();
        AccPChart = new Chart(AccPctx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartLabel,
                datasets: [{
                    //label: 'Man Power budget',
                    data: $scope.chartList,
                    //backgroundColor: 'rgba(240, 52, 52, .6)',
                    backgroundColor: ['rgba(46, 204, 113,.6)', 'rgba(241, 196, 15,.6)', 'rgba(191, 204, 113,.6)', 'rgba(240, 52, 52, .6)'],
                    borderColor: ['rgba(46, 204, 113,.8)', 'rgba(241, 196, 15,.8)', 'rgba(191, 204, 113,.6)', 'rgba(240, 52, 52, .8)'],
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                tooltips: { enabled: true },
                legend: {
                    onClick: (e) => e.stopPropagation(),
                    display: false,
                    position: 'bottom'
                },
                label: true,
                hover: { mode: null }
            }
        });
    }

    $scope.GetROverDue = function () {
        $scope.ROverDueList = [];
        $http({
            method: 'POST',
            url: 'accounts/accountdashboard/GetOverDueReceivableModal',
            data: {
                'partyId': $scope.AccountDashboardDropDown.PartyId,
                'currencyId': $scope.AccountDashboardDropDown.CurrencyId,
                'matureDate': cDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ROverDueList = response.data;
            angular.element(document.querySelector('#OverDue')).modal('show');
        });
    };
}