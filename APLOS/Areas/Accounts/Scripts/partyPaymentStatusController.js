'use strict';
partyPaymentStatusController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function partyPaymentStatusController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Financial Status Dashboard';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.Action = 'Save';
    $scope.MasterLCList = [];
    $scope.path = 'Accounts/AccountStatusDashboard/';
    $scope.exportgriddataUrlUpdate2 = 'GridReports/ExcelExportUpdate2';
    $scope.downloadgriddataUrl2 = 'GridReports/Download';
    var dt = new Date();
    $scope.reportParameters = {
        FromDate: $filter("dateFiltering")(new Date(dt.setDate(dt.getDate() - 10))), //$filter("dateFiltering")(Date.now()) - 10,
        ToDate: $filter("dateFiltering")(Date.now()),
        TransactionType: 'LoanTaken',
        ReportFormat: 'Excel',
        VoucherId: null,
        IsWithAdvance: false
    };

    $scope.report = {
        IsUpToLevel: 'Detail',
        IsBudgetLevel: false,
        IsActivityLevel: true,
        IsDetailLevel: false,
        ToDate: $filter('dateFiltering')(Date.now()),
        AssetsLiability: ''
    };

    $scope.material = {
        ReportFormat: 'Pdf',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        VendorFromDate: $filter('dateFiltering')(Date.now()),
        VendorToDate: $filter('dateFiltering')(Date.now()),
        CustomerFromDate: $filter('dateFiltering')(Date.now()),
        CustomerToDate: $filter('dateFiltering')(Date.now()),
        GRNandAccPType: 'GRNPosted',
        DateType: 'PostingDate',
        IsOrderSpecific: true,
        IsNonOrderSpecific: false
    };

    //$scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    //$scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.exportgriddataUrl = 'Accounts/AccountStatusDashboard/ExcelExportJson';
    $scope.downloadgriddataUrl = 'Accounts/AccountStatusDashboard/Download';
    $scope.Print = function () {
        //debugger;
        //// var gridObj = $("#DetailGrid").data("ejGrid");
        //var gridObj = $("#DetailGrid").ejGrid("instance");
        //var data = gridObj.model.dataSource;

        var filtered = $("#GridSelectedTrialBalance").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            filtered = $scope.TrialBalanceList;
        }
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'obj': JSON.stringify(filtered),
                'ReportHeader': $scope.report.AssetsLiability + '   ' + $scope.report.ToDate,
                //'toDate': $scope.report.ToDate
            }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                // ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

            }
            else {

                location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    }

    window.chartColors = {
        red: 'rgba(240, 52, 52, .6)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgba(46, 204, 113,.6)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)',
        lightBlue: 'rgb(160, 184, 222)',
        lightGreen: 'rgb(139, 245, 137)'
    };

    var x = document.getElementById("MainDiv");
    var y = document.getElementById("materialDIV");

    x.style.display = "block";
    y.style.display = "none";
    $scope.showFinanceDB = true;

    $scope.showMainDiv = function () {
        $scope.showFinanceDB = true;
        if (x.style.display === "none") {
            y.style.display = "none";
            x.style.display = "block";

        }
    }
    $scope.showSecondDiv = function () {
        $scope.showFinanceDB = false;
        if (y.style.display === "none") {
            x.style.display = "none";
            y.style.display = "block";
        }
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    //.............#regon Vendor Tab...................
    //get data for master gride for vendor Payable
    $scope.invalidDocDate = false;
    $scope.ToDatevalidation = function () {
        var msg = "";

        if (baseService.isUndefinedOrNull($scope.material.VendorToDate)) {
            $scope.invalidDocDate = true;
            msg = "Please select To Date!";
        }
        else if (new Date($scope.material.VendorToDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "ToDate must be below or equal to current Date!";
        }
        else if (new Date($scope.material.FromDate) > new Date($scope.material.VendorToDate)) {
            msg = "To Date must be greater or equal to FromDate!";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_ToDate", $scope.invalidDocDate, msg);
    }

    $scope.invalidFromDate = false;
    $scope.FromDateValidation = function () {
        var msg = "";
        if (baseService.isUndefinedOrNull($scope.material.VendorFromDate)) {
            $scope.invalidFromDate = true;
            msg = "Please select From Date!";
        }
        else if (new Date($scope.material.VendorFromDate) > new Date()) {
            $scope.invalidFromDate = true;
            msg = "FromDate must be below or equal to current Date!";
        }
        else $scope.invalidFromDate = false;
        return manualValidation("div_FromDate", $scope.invalidFromDate, msg);
    }

    $scope.GetInvoiceList = function () {
        if (!$scope.invalidFromDate && !$scope.invalidDocDate) {
            try {
                $http({
                    method: 'POST',
                    url: $scope.path + "GetPartyPaymentStatusInvoiceList",
                    data: {
                        fromDate: "",
                        toDate: $scope.material.VendorToDate
                    },
                    dataType: 'JSON'

                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        $scope.MasterLCList = response.data.DATA;
                        $scope.GetPPSAgingList();
                        $scope.GetPartyPaymentStatusPendingAdjustmentList();
                    }
                    else {
                        ShowResult(response.data.Message, 'failure');
                    }

                }),
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }

            }

            catch (e) {

            }
        }
    }
    //$scope.GetInvoiceList();

    $scope.GetInvoiceListDateRange = function () {
        $scope.FromDateValidation();
        $scope.ToDatevalidation();
        if (!$scope.invalidFromDate && !$scope.invalidDocDate) {
            try {
                $http({
                    method: 'POST',
                    url: $scope.path + "GetPartyPaymentStatusInvoiceList",
                    data: {
                        fromDate: $scope.material.VendorFromDate,
                        toDate: $scope.material.VendorToDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        $scope.MasterLCList = response.data.DATA;
                    }
                    else {
                        ShowResult(response.data.Message, 'failure');
                    }
                }),
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }

            }

            catch (e) {

            }
        }
    }

    $scope.InvoiceSummaryReport = function () {
        try {
            var NewMasterLCList = [];
            for (var i = 0; i < $scope.MasterLCList.length; i++) {
                if ($scope.MasterLCList[i].isSelected == true) {
                    NewMasterLCList.push($scope.MasterLCList[i]);
                }
            }
            if (NewMasterLCList.length == 0) {
                ShowResult('Please select at least one Party', 'failure');
            }

            $scope.downloadgriddataUrl = 'GridReports/Download';
            $http({
                method: 'POST',
                url: $scope.path + "PartyPaymentStatusReport",
                data: {
                    'MasterLCList': NewMasterLCList,
                    'fromDate': "",
                    'toDate': $scope.material.VendorToDate
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.InvoiceAgingReport = function () {

        try {

            var NewMasterLCList = [];
            for (var i = 0; i < $scope.MasterLCList.length; i++) {
                if ($scope.MasterLCList[i].isSelected == true) {

                    if (NewMasterLCList, $scope.MasterLCList[i].PartyId) {
                        NewMasterLCList.push($scope.MasterLCList[i].PartyId);
                    }
                }
            }
            if (NewMasterLCList.length == 0) {
                ShowResult('Please select at least one Party', 'failure');
            }
            else {
                $scope.fileName = "PayableAging.xlsx";
                $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
                $http({
                    method: 'POST',
                    url: $scope.path + "PartyPaymentStatusAgingReport",
                    data: {
                        'parameters': NewMasterLCList,
                        'fromDate': "",
                        'toDate': $scope.material.VendorToDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    }
                    else {
                        ShowResult(response.data.Message, 'failure');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };

                //var file_src = $scope.path + "PartyPaymentStatusAgingReport?MasterLCList=" + NewMasterLCList;
                //$rootScope.report(file_src);
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridMasterVendorPayable").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MasterLCList.length; i++) {
                $scope.MasterLCList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridMasterVendorPayable").data("ejGrid");
        gridObj.refreshContent();
    };

    //#regon...... Pia Chart and Table/donught Vendor Payable 
    $scope.ODueMoreThan30Pai = 0;
    $scope.ODueMoreThan30NoOfInvoice = 0;

    $scope.ODueMoreThan15Pai = 0;
    $scope.ODueMoreThan15NoOfInvoice = 0;

    $scope.ODueLessThan15Pai = 0;
    $scope.ODueLessThan15NoOfInvoice = 0;

    $scope.TodayBalancePai = 0;
    $scope.TodayBalanceNoOfInvoice = 0;

    $scope.OneToSevenBalancePai = 0;
    $scope.OneToSevenBalanceNoOfInvoice = 0;

    $scope.EightToThirtyBalancePai = 0;
    $scope.EightToThirtyBalanceNoOfInvoice = 0;

    $scope.ThirtyToSixtyBalancePai = 0;
    $scope.ThirtyToSixtyBalanceNoOfInvoice = 0;

    $scope.Onword60Pai = 0;
    $scope.Onword60NoOfInvoice = 0;

    $scope.GetPPSAgingList = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetPartyPaymentStatusAgingList",
                // data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.MasterLCListu1 = response.data;

                $scope.ODueMoreThan30Pai = $scope.MasterLCListu1[0]["ODueMoreThan30"];
                $scope.ODueMoreThan30NoOfInvoicePai = $scope.MasterLCListu1[0]["ODueMoreThan30NoOfInvoice"];

                $scope.ODueMoreThan15Pai = $scope.MasterLCListu1[0]["ODueMoreThan15"];
                $scope.ODueMoreThan15NoOfInvoicePai = $scope.MasterLCListu1[0]["ODueMoreThan15NoOfInvoice"];

                $scope.ODueLessThan15Pai = $scope.MasterLCListu1[0]["ODueLessThan15"];
                $scope.ODueLessThan15NoOfInvoicePai = $scope.MasterLCListu1[0]["ODueLessThan15NoOfInvoice"];

                $scope.TodayBalancePai = $scope.MasterLCListu1[0]["TodayBalance"];
                $scope.TodayBalanceNoOfInvoicePai = $scope.MasterLCListu1[0]["TodayBalanceNoOfInvoice"];

                $scope.OneToSevenBalancePai = $scope.MasterLCListu1[0]["OneToSevenBalance"];
                $scope.OneToSevenBalanceNoOfInvoicePai = $scope.MasterLCListu1[0]["OneToSevenBalanceNoOfInvoice"];

                $scope.EightToThirtyBalancePai = $scope.MasterLCListu1[0]["EightToThirtyBalance"];
                $scope.EightToThirtyBalanceNoOfInvoicePai = $scope.MasterLCListu1[0]["EightToThirtyBalanceNoOfInvoice"];

                $scope.ThirtyToSixtyBalancePai = $scope.MasterLCListu1[0]["ThirtyToSixtyBalance"];
                $scope.ThirtyToSixtyBalanceNoOfInvoicePai = $scope.MasterLCListu1[0]["ThirtyToSixtyBalanceNoOfInvoice"];

                $scope.Onword60Pai = $scope.MasterLCListu1[0]["Onword60"];
                $scope.Onword60NoOfInvoicePai = $scope.MasterLCListu1[0]["Onword60NoOfInvoice"];

                $scope.chartAttdnLabel = ['ODueMoreThan30', 'ODueMoreThan15', 'ODueLessThan15', 'TodayBalance', 'OneToSevenBalance', 'EightToThirtyBalance', 'ThirtyToSixtyBalance', 'Onword60'];

                $scope.totalAgingdonught = $scope.ODueMoreThan30Pai + $scope.ODueMoreThan15Pai + $scope.ODueLessThan15Pai + $scope.TodayBalancePai + $scope.OneToSevenBalancePai + $scope.EightToThirtyBalancePai + $scope.ThirtyToSixtyBalancePai + $scope.Onword60Pai;
                $scope.chartAttdnList = [$scope.ODueMoreThan30Pai, $scope.ODueMoreThan15Pai, $scope.ODueLessThan15Pai, $scope.TodayBalancePai, $scope.OneToSevenBalancePai, $scope.EightToThirtyBalancePai, $scope.ThirtyToSixtyBalancePai, $scope.Onword60Pai];
                createAttnPieChart();
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }


    var ATTNPieChart;
    $scope.totalAgingdonught = 0.00;
    $scope.chartAttdnLabel = ['ODueMoreThan30', 'ODueMoreThan15', 'ODueLessThan15', 'TodayBalance', 'OneToSevenBalance', 'EightToThirtyBalance', 'ThirtyToSixtyBalance', 'Onword60'];
    $scope.chartAttdnList = [];
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
                        'rgba(242, 38, 19, 1)',
                        'rgba(150, 40, 27, 1)',

                        'rgba(231, 76, 60,0.7)',
                        'rgba(82, 179, 217, 0.7)',
                        'rgba(253, 227, 167, 0.7)',
                        'rgba(65, 246, 188, 0.7)',
                        'rgba(196, 171, 93, 0.46)',
                        'rgba(196, 93, 119, 0.46)'
                    ],
                    borderColor: [
                        'rgba(46, 204, 113,0.7)',
                        'rgba(241, 196, 15, 0.7)',
                        'rgba(231, 76, 60,0.7)',
                        'rgba(82, 179, 217, 0.7)',
                        'rgba(253, 227, 167, 0.7)',
                        'rgba(65, 246, 188, 0.7)',
                        'rgba(196, 171, 93, 0.46)',
                        'rgba(196, 93, 119, 0.46)'

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

    //party aging due popUp  get Data
    $scope.AgingType = null;
    $scope.partyWiseAgingDueList = [];
    $scope.getPartyWiseAgingDueList = function (x, type) {
        $scope.AgingType = type

        $scope.partyWiseAgingDueList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyAgingDueList?overDueDetailAmount=" + x
        }).then(function successCallback(response) {
            $scope.partyWiseAgingDueList = response.data;
            if (type == 'ODueMoreThan30Pai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].ODueMoreThan30;
                }
            }
            else if (type == 'ODueMoreThan15Pai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].ODueMoreThan15;
                }
            }
            else if (type == 'ODueLessThan15Pai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].ODueLessThan15;
                }
            }
            else if (type == 'TodayBalancePai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].TodayBalance;
                }
            }
            else if (type == 'OneToSevenBalancePai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].OneToSevenBalance;
                }
            }
            else if (type == 'EightToThirtyBalancePai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].EightToThirtyBalance;
                }
            }
            else if (type == 'ThirtyToSixtyBalancePai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].ThirtyToSixtyBalance;
                }
            }
            else if (type == 'Onword60Pai') {
                for (var i = 0; i < $scope.partyWiseAgingDueList.length; i++) {
                    $scope.partyWiseAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].Onword60;
                }
            }

            //$scope.TotalDebitNoteAmount = $filter("sumByKey")($filter("filter")($scope.partyWiseAgingDueList), "Balance");

            //if ($scope.partyWiseAgingDueList.length > 0) {
            //    angular.element(document.querySelector("#partyWiseAgingDuePopUp")).modal("show");
            //}

        });
    };

    $scope.showPartyWiseDueAmountPopUp = function (type) {
        if (type == 'ODueMoreThan30Pai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-30';
            $scope.AgingDueType = 'Over Due More Than 30 List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off Over Due More Than 30'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Over Due More Than 30'

        }
        else if (type == 'ODueMoreThan15Pai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-30';
            $scope.AgingDueType = 'Over Due More Than 15 List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off Over Due More Than 15'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail More Then Fifteen'


        }
        else if (type == 'ODueLessThan15Pai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-15';
            $scope.AgingDueType = 'Over Due Less Than 15 List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off Over Due Less Than 15'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Less Then Fifteen'

        }
        else if (type == 'TodayBalancePai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0';
            $scope.AgingDueType = 'Today Balance List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off Today'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Today'

        }
        else if (type == 'OneToSevenBalancePai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=7';
            $scope.AgingDueType = 'One Two Seven Balance List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off One To Seven'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail One To Seven'

        }
        else if (type == 'EightToThirtyBalancePai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>7 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=30';
            $scope.AgingDueType = 'Eight To Thirty Balance List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off Eight To Thirty'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Eight To Thirty'

        }
        else if (type == 'ThirtyToSixtyBalancePai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>30 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=60';
            $scope.AgingDueType = 'Thirty To Sixty Balance List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off Thirty To Sixty'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Thirty To Sixty'

        }
        else if (type == 'Onword60Pai') {
            $scope.due = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>60';
            $scope.AgingDueType = 'On Ward Sixty Balance List'
            $scope.AgingDueTypeSetOff = 'List of Set-Off On Word Sixty'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail On Word Sixty'

        }

        $scope.getPartyWiseAgingDueList($scope.due, type);
        angular.element(document.querySelector("#partyWiseAgingDuePopUp")).modal("show");

    };

    $scope.closePartyAgingDueAmount = function () {
        angular.element(document.querySelector("#partyWiseAgingDuePopUp")).modal("hide");
    };

    //Aging ODMT30VoucherPrintPopUp get data
    $scope.partyWiseAgingDueVoucherList = [];
    //$scope.PartyId = null;
    $scope.getPartyWiseAgingDueVoucherList = function (p, due) {

        $scope.partyWiseAgingDueVoucherList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyAgingDueVoucherPrintList?partyId=" + p + '&setOffDetailAgingType=' + due
        }).then(function successCallback(response) {
            $scope.partyWiseAgingDueVoucherList = response.data;

            $scope.booksDebitNote = $scope.partyWiseAgingDueVoucherList[0].BooksDebitNoteAmount
            $scope.booksDiscount = $scope.partyWiseAgingDueVoucherList[0].BooksDiscountAmount
            $scope.booksTax = $scope.partyWiseAgingDueVoucherList[0].BooksTaxAmount
            $scope.booksPayment = $scope.partyWiseAgingDueVoucherList[0].BooksSetOff
            $scope.booksSetOffTotal = $scope.booksDebitNote + $scope.booksDiscount + $scope.booksTax + $scope.booksPayment

        });
    };

    $scope.showAgingMoreThirtyVoucherPrintPopUp = function (args) {
        var gridObj = $("#partyAgingOverDueMoreThanThirty").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.tempPartyId = data.PartyId;
        $scope.getPartyWiseAgingDueVoucherList($scope.tempPartyId, $scope.due)

        angular.element(document.querySelector("#vendorPayableODMT30VoucherPrintPopUp")).modal("show");
    };

    $scope.closePartyAgingDueVoucherAmount = function () {
        angular.element(document.querySelector("#vendorPayableODMT30VoucherPrintPopUp")).modal("hide");
    };

    $scope.printVoucherReport = function (obj) {
        var data = obj.data;
        if (data.SourceType == 'VendorInvoice')
            var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'InventoryPayable')
            var file_src = 'Accounts/InventoryPayable/PabyableJournal?reportFormat=' + 'Excel' + '&inventoryReceiveId=' + data.InventoryReceiveId + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false;

        if (data.SourceType == 'VendorPayment')
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        $rootScope.report(file_src);

    }

    //GetSetOffDetailPopUp
    $scope.PartyPaymentSetOffDetailPoPUpList = [];
    $scope.GetSetOffDetailPopUp = function () {
        $scope.tempPartyId
        $scope.due
        $http({
            method: 'POST',
            url: 'Accounts/AccountStatusDashboard/GetpartyPaymentSetOffDetailList',
            data: { 'setOffPaymentDetailAgingType': $scope.due, 'partyId': $scope.tempPartyId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                $scope.PartyPaymentSetOffDetailPoPUpList = response.data;
            }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('setOffDetailPopUp');
    }

    $scope.closeSetOffDetailPopUp = function () {
        //alert('hhh');
        angular.element(document.querySelector("#setOffDetailPopUp")).modal("hide");
    };

    //Voucher Report for SetOff Detail 
    $scope.printSetOffDetailVoucherReport = function (obj) {
        var data = obj.data;
        if (data.SourceType == 'VendorInvoice')
            var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'InventoryPayable')
            var file_src = 'Accounts/InventoryPayable/PabyableJournal?reportFormat=' + 'Excel' + '&inventoryReceiveId=' + data.InventoryReceiveId + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false;

        if (data.SourceType == 'VendorPayment')
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        if (data.SourceType == 'VendorAdvanceWriteOff')
            var file_src = 'Accounts/Advance/ReportVendorAdvanceWriteOff?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        $rootScope.report(file_src);
    }

    //NO Of Invoice detail PopUp
    $scope.PartyVendorPayableNoOfInvoiceDetailList = [];
    //$scope.PartyId = null;
    $scope.getPartyVendorPayableNoOfInvoiceDetailList = function (p, due) {
        $scope.partyWiseAgingDueVoucherList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyVendorPayableNoOfInvoiceDetailList?partyId=" + p + '&vendorPayableInvoiceDetailAgingType=' + due
        }).then(function successCallback(response) {
            $scope.PartyVendorPayableNoOfInvoiceDetailList = response.data;
            //$scope.debitNote = $scope.partyWiseAgingDueVoucherList[0].DebitNoteAmount
        });
    };

    $scope.showVendorPayableNoOfInvoiceDetailPopUp = function (args) {
        var gridObj = $("#partyAgingOverDueMoreThanThirty").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.tempPartyId = data.PartyId;
        $scope.tempPartyName = data.PartyName;
        $scope.getPartyVendorPayableNoOfInvoiceDetailList($scope.tempPartyId, $scope.due)

        angular.element(document.querySelector("#vendorPayableAgingNoOfInvoiceDetailPopUp")).modal("show");
    };

    $scope.closeVendorPayableAgingNoOfInvoiceDetailPopUp = function () {
        angular.element(document.querySelector("#vendorPayableAgingNoOfInvoiceDetailPopUp")).modal("hide");
    };

    $scope.printNoOfInvoiceDetailVoucherReport = function (obj) {
        var data = obj.data;
        if (data.SourceType == 'VendorInvoice')
            var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'InventoryPayable')
            var file_src = 'Accounts/InventoryPayable/PabyableJournal?reportFormat=' + 'Excel' + '&inventoryReceiveId=' + data.InventoryReceiveId + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false;

        if (data.SourceType == 'VendorPayment')
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        $rootScope.report(file_src);
    }

    $scope.agingNoOfInvoiceDetailSetoffDetailList = [];
    //$scope.PartyId = null;
    $scope.getAgingNoOfInvoiceSetOffDetilList = function (p, invoiceId, due) {
        $scope.agingNoOfInvoiceDetailSetoffDetailList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetAgingNoOfInvoiceSetOffDetilList?partyId=" + p + '&invoiceId=' + invoiceId + '&setOffDetailAgingType=' + due
        }).then(function successCallback(response) {
            $scope.agingNoOfInvoiceDetailSetoffDetailList = response.data;

            $scope.booksDebitNote = $scope.agingNoOfInvoiceDetailSetoffDetailList[0].BooksDebitNoteAmount
            $scope.booksDiscount = $scope.agingNoOfInvoiceDetailSetoffDetailList[0].BooksDiscountAmount
            $scope.booksTax = $scope.agingNoOfInvoiceDetailSetoffDetailList[0].BooksTaxAmount
            $scope.booksPayment = $scope.agingNoOfInvoiceDetailSetoffDetailList[0].BooksSetOff
            $scope.booksInvoiceSetOffTotal = $scope.booksDebitNote + $scope.booksDiscount + $scope.booksTax + $scope.booksPayment
        });
    };

    $scope.showAgingNoOfInvoiceDetailSetOffDetailPopUp = function (args) {
        var gridObj = $("#GridAgingNoOfInvoiceDetailSetOffDetail").data("ejGrid");
        $scope.tempInvoiceId = args.InvoiceId;
        $scope.getAgingNoOfInvoiceSetOffDetilList(args.PartyId, args.InvoiceId, $scope.due)
        angular.element(document.querySelector("#vendorPayableAgingNoOfInvoiceDetailSetOffDetailPopUp")).modal("show");
    };

    $scope.closeVendorPayableAgingNoOfInvoiceDetailSetOffDetailPopUp = function () {
        angular.element(document.querySelector("#vendorPayableAgingNoOfInvoiceDetailSetOffDetailPopUp")).modal("hide");
    };

    //Get Invoice Detail SetOffPaymentDetailPopUp
    $scope.AgingInvoiceSetOffPaymentDetailPoPUpList = [];
    $scope.GetInvoiceDetailSetOffPaymentDetailPopUp = function () {
        $scope.tempPartyId
        $scope.due
        $scope.tempInvoiceId
        $http({
            method: 'POST',
            url: 'Accounts/AccountStatusDashboard/GetInvoiceDetailSetOffPaymentDetailPopUp',
            data: { 'setOffPaymentDetailAgingType': $scope.due, 'partyId': $scope.tempPartyId, 'invoiceId': $scope.tempInvoiceId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                $scope.AgingInvoiceSetOffPaymentDetailPoPUpList = response.data;
            }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('InvoiceDetailPaymentDetailPopUp');
    }

    $scope.closeInvoiceDetailPaymentDetailPopUp = function () {
        //alert('hhh');
        angular.element(document.querySelector("#InvoiceDetailPaymentDetailPopUp")).modal("hide");
    };

    //Voucher Report for SetOff Detail 
    $scope.printInvoicePaymentDetailVoucherReport = function (obj) {
        var data = obj.data;
        if (data.SourceType == 'VendorInvoice')
            var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'InventoryPayable')
            var file_src = 'Accounts/InventoryPayable/PabyableJournal?reportFormat=' + 'Excel' + '&inventoryReceiveId=' + data.InventoryReceiveId + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false;

        if (data.SourceType == 'VendorPayment')
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        if (data.SourceType == 'VendorAdvanceWriteOff')
            var file_src = 'Accounts/Advance/ReportVendorAdvanceWriteOff?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        $rootScope.report(file_src);

    }
    //.............#endregion vendor payable tab

    //AutoMailReport
    $scope.AutoMailReport = function () {
        var file_src = 'Accounts/Invoice/GetAutoMailReport';
        $rootScope.report(file_src);
    }
    $scope.AutoMailVPaymentReport = function () {
        var file_src = 'Accounts/Invoice/GetAutoMailVPaymentReport';
        $rootScope.report(file_src);
    }

    //#regon...... Pending Adjustment Vendor Payable
    $scope.PayablePostedAmount = 0;
    $scope.PayableUnPostedAmount = 0;
    $scope.TotalPayablePostedUnPostedAmount = 0;

    $scope.VendorAdvancePostedAmount = 0;
    $scope.VendorAdvanceUnPostedAmount = 0;
    $scope.TotalVendorAdvancePostedUnPostedAmount = 0;

    $scope.VendorDebitNotePostedAmount = 0;
    $scope.VendorDebitNoteUnPostedAmount = 0;
    $scope.TotalVendorDebitNotePostedUnPostedAmount = 0;

    $scope.GetPartyPaymentStatusPendingAdjustmentList = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetPartyPaymentStatusPendingAdjustmentData",
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.VendorPendingAdjustmentList = response.data;

                $scope.PayablePostedAmount = $scope.VendorPendingAdjustmentList[0]["PayablePostedAmount"];
                $scope.PayableUnPostedAmount = $scope.VendorPendingAdjustmentList[0]["PayableUnPostedAmount"];
                $scope.TotalPayablePostedUnPostedAmount = $scope.VendorPendingAdjustmentList[0]["TotalPayablePostedUnPostedAmount"];

                $scope.VendorAdvancePostedAmount = $scope.VendorPendingAdjustmentList[0]["VendorAdvancePostedAmount"];
                $scope.VendorAdvanceUnPostedAmount = $scope.VendorPendingAdjustmentList[0]["VendorAdvanceUnPostedAmount"];
                $scope.TotalVendorAdvancePostedUnPostedAmount = $scope.VendorPendingAdjustmentList[0]["TotalVendorAdvancePostedUnPostedAmount"];

                $scope.VendorDebitNotePostedAmount = $scope.VendorPendingAdjustmentList[0]["VendorDebitNotePostedAmount"];
                $scope.VendorDebitNoteUnPostedAmount = $scope.VendorPendingAdjustmentList[0]["VendorDebitNoteUnPostedAmount"];
                $scope.TotalVendorDebitNotePostedUnPostedAmount = $scope.VendorPendingAdjustmentList[0]["TotalVendorDebitNotePostedUnPostedAmount"];


            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }

    //#endregon...... Pending Adjustment Vendor Payable

    //#regon...... Pending Adjustment Customer Receivable
    $scope.ReceivablePostedAmount = 0;
    $scope.ReceivableUnPostedAmount = 0;
    $scope.TotalReceivablePostedUnPostedAmount = 0;

    $scope.CustomerAdvancePostedAmount = 0;
    $scope.CustomerAdvanceUnPostedAmount = 0;
    $scope.TotalCustomerAdvancePostedUnPostedAmount = 0;

    $scope.CustomerCreditNotePostedAmount = 0;
    $scope.CustomerCreditNoteUnPostedAmount = 0;
    $scope.TotalCustomerCreditNotePostedUnPostedAmount = 0;

    $scope.GetPartyReceiveStatusPendingAdjustmentData = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetPartyReceiveStatusPendingAdjustmentData",
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.CustomerPendingAdjustmentList = response.data;

                $scope.ReceivablePostedAmount = $scope.CustomerPendingAdjustmentList[0]["ReceivablePostedAmount"];
                $scope.ReceivableUnPostedAmount = $scope.CustomerPendingAdjustmentList[0]["ReceivableUnPostedAmount"];
                $scope.TotalReceivablePostedUnPostedAmount = $scope.CustomerPendingAdjustmentList[0]["TotalReceivablePostedUnPostedAmount"];

                $scope.CustomerAdvancePostedAmount = $scope.CustomerPendingAdjustmentList[0]["CustomerAdvancePostedAmount"];
                $scope.CustomerAdvanceUnPostedAmount = $scope.CustomerPendingAdjustmentList[0]["CustomerAdvanceUnPostedAmount"];
                $scope.TotalCustomerAdvancePostedUnPostedAmount = $scope.CustomerPendingAdjustmentList[0]["TotalCustomerAdvancePostedUnPostedAmount"];

                $scope.CustomerCreditNotePostedAmount = $scope.CustomerPendingAdjustmentList[0]["CustomerCreditNotePostedAmount"];
                $scope.CustomerCreditNoteUnPostedAmount = $scope.CustomerPendingAdjustmentList[0]["CustomerCreditNoteUnPostedAmount"];
                $scope.TotalCustomerCreditNotePostedUnPostedAmount = $scope.CustomerPendingAdjustmentList[0]["TotalCustomerCreditNotePostedUnPostedAmount"];


            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }

    //#endregon...... Pending Adjustment Customer Receivable

    $scope.DateRangeWisePaymentList = [];
    $scope.GetDateRangeWisePaymentData = function () {
        try {
            $http({
                method: 'POST',
                url: 'Accounts/AccountStatusDashboard/GetDateRangeWisePaymentData',
                data: { 'fromDate': $scope.reportParameters.FromDate, 'toDate': $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.DateRangeWisePaymentList = response.data.DATA;
                $scope.PaymentBarChartDateRange();

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }


    $scope.DateRangeWisePaymentReport = function () {

        if (baseService.isUndefinedOrNull($scope.reportParameters.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.reportParameters.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.reportParameters.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.reportParameters.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            var url = "Accounts/AccountStatusDashboard/GetDateRangeWisePaymentReport?fromDate=" + $scope.reportParameters.FromDate + "&toDate=" + $scope.reportParameters.ToDate;
            $rootScope.report(url);

        }

    }

    $scope.PartyPaymentDetailPoPUpList = [];
    $scope.detailPaymentPopUpData = null;
    $scope.LoadPaymentDetailList = function (data) {
        $scope.tempBooksPayment = data.BooksPayment;

        $scope.detailPaymentPopUpData = data;
        $scope.tempid = null;
        if (data.Type == 'Vendor') {
            $scope.tempid = data.PartyId;
        }
        if (data.Type == 'Employee') {
            $scope.tempid = data.EmployeeId;
        }
        if (data.Type == 'GL') {
            $scope.tempid = data.ActivityId;
        }
        if (data.Type == 'Cash') {
            $scope.tempid = data.CashMasterId;
        }
        if (data.Type == 'Bank') {
            $scope.tempid = data.BankMasterId;
        }
        $http({
            method: 'POST',
            url: 'Accounts/AccountStatusDashboard/GetDateRangeWisePaymentPopUpData',
            data: { 'id': $scope.tempid, 'type': data.Type, 'fromDate': $scope.reportParameters.FromDate, 'toDate': $scope.reportParameters.ToDate },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.PartyPaymentDetailPoPUpList = response.data.DATA;
                    $scope.ParticularName = $scope.PartyPaymentDetailPoPUpList[0].ParticularName;
                    $scope.Type = $scope.PartyPaymentDetailPoPUpList[0].Type;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('PaymentDetailPopup');
    }


    $scope.DateRangeWiseDetailPaymentPoPUpReport = function () {

        $scope.tempid = null;
        if ($scope.detailPaymentPopUpData.Type == 'Vendor') {
            $scope.tempid = $scope.detailPaymentPopUpData.PartyId;
        }
        if ($scope.detailPaymentPopUpData.Type == 'Employee') {
            $scope.tempid = $scope.detailPaymentPopUpData.EmployeeId;
        }
        if ($scope.detailPaymentPopUpData.Type == 'GL') {
            $scope.tempid = $scope.detailPaymentPopUpData.ActivityId;
        }
        if ($scope.detailPaymentPopUpData.Type == 'Cash') {
            $scope.tempid = $scope.detailPaymentPopUpData.CashMasterId;
        }
        if ($scope.detailPaymentPopUpData.Type == 'Bank') {
            $scope.tempid = $scope.detailPaymentPopUpData.BankMasterId;
        }

        if (baseService.isUndefinedOrNull($scope.reportParameters.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.reportParameters.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.reportParameters.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.reportParameters.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            var url = "Accounts/AccountStatusDashboard/GetDateRangeWiseDetailPaymentPoPUpReport?fromDate=" + $scope.reportParameters.FromDate + "&toDate=" + $scope.reportParameters.ToDate + "&id=" + $scope.tempid + "&type=" + $scope.Type;
            //data: { 'id': $scope.tempid, 'type': data.Type, 'fromDate': $scope.reportParameters.FromDate, 'toDate': $scope.reportParameters.ToDate },

            $rootScope.report(url);

        }

    }

    $scope.ClosePaymentDetailPopUp = function () {
        $scope.hidePaymentDetailPopUp();
        $scope.detailPaymentPopUpData = null;
    };
    $scope.hidePaymentDetailPopUp = function () {
        angular.element(document.querySelector("#PaymentDetailPopup")).modal("hide");
    };

    //due work
    $scope.printPaymentDetailPopUpVoucherReport = function (obj) {
        var data = obj.data;
        if (data.SourceType == 'VendorAdvance')
            var file_src = 'Accounts/Advance/ReportVendorAdvance?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'VendorPayment')
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'EmployeePayment')
            var file_src = 'Employees/EmployeeReport/GetEmployeePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        if (data.SourceType == 'CashJournal')
            var file_src = 'Banks/CashReport/GetCashJournalReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'BankJournal')
            var file_src = 'Banks/BankReport/GetBankJournalReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        if (data.SourceType == 'CreditNoteSetOff')
            var file_src = 'Accounts/AdjustmentNote/CreditNoteSetOffReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        $rootScope.report(file_src);

    }

    $scope.PaymentBarChartDateRange = function () {
        $scope.DateRangeWisePaymentBarChartList = [];

        $http({
            method: 'POST',
            url: 'Accounts/AccountStatusDashboard/GetDateRangeWisePaymentDataBarChart',
            data: {
                'fromDate': $scope.reportParameters.FromDate, 'toDate': $scope.reportParameters.ToDate,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DateRangeWisePaymentBarChartList = response.data;
            createPaymentStackedBaChart(response.data);

        });
    };
    $scope.PaymentBarChartDateRange();

    var PaymentsbarChart;
    $scope.paymentList = [];
    var row =
    {
        postingDate: null,
        VendorAmount: 0,
        EmployeeAmount: 0,
        GLAmount: 0,
        TotalAmount: 0
    }
    function createPaymentStackedBaChart(list) {
        var postingDate = "";
        row =
        {
            PostingDate: null,
            VendorAmount: 0,
            EmployeeAmount: 0,
            GLAmount: 0,
            TotalAmount: 0

        }
        $scope.paymentList = [];
        $scope.postingDate = []
        $scope.Vendor = [];
        $scope.Employee = [];
        $scope.GL = [];
        angular.forEach(list, function (item, i) {
            if (item.PostingDate != postingDate) {
                row.TotalAmount = row.VendorAmount + row.EmployeeAmount + row.GLAmount;
                $scope.paymentList.push(row);

                if ($scope.paymentList[0]["PostingDate"] === null) {
                    $scope.paymentList.pop();
                }

                $scope.postingDate.push(item.PostingDate);
                $scope.Vendor.push(0);
                $scope.Employee.push(0);
                $scope.GL.push(0);
                row =
                {
                    PostingDate: null,
                    VendorAmount: 0,
                    EmployeeAmount: 0,
                    GLAmount: 0,
                    TotalAmount: 0

                }
                row.PostingDate = item.PostingDate;
                row.VendorAmount = 0;
                row.EmployeeAmount = 0;
                row.GLAmount = 0;
            }
            if (item.Type == "Vendor") {
                $scope.Vendor.pop();

                $scope.Vendor.push(item.BooksPayment);
                row.VendorAmount = item.BooksPayment;
            }

            if (item.Type == "Employee") {
                $scope.Employee.pop();

                $scope.Employee.push(item.BooksPayment);
                row.EmployeeAmount = item.BooksPayment;
            }
            if (item.Type == "GL") {
                $scope.GL.pop();
                $scope.GL.push(item.BooksPayment);
                row.GLAmount = item.BooksPayment;
            }

            postingDate = item.PostingDate;

            if (list.length == i) {
                $scope.paymentList.push(row);
            }
        });
        row.TotalAmount = row.VendorAmount + row.EmployeeAmount + row.GLAmount;

        $scope.paymentList.push(row);
        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("PaymentbarChart").getContext('2d');
        if (PaymentsbarChart !== undefined && typeof PaymentsbarChart === 'object' && typeof PaymentsbarChart.destroy === 'function') PaymentsbarChart.destroy();
        PaymentsbarChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.postingDate,
                datasets: [{
                    label: 'Vendor',
                    data: $scope.Vendor,
                    backgroundColor: window.chartColors.yellow,
                    borderColor: window.chartColors.yellow,
                    fill: true,
                    borderWidth: 2
                },
                {
                    label: 'Employee',
                    data: $scope.Employee,
                    backgroundColor: window.chartColors.blue,
                    borderColor: window.chartColors.blue,
                    fill: true,
                    borderWidth: 2
                },

                {
                    label: 'GL',
                    data: $scope.GL,
                    backgroundColor: window.chartColors.green,
                    borderColor: window.chartColors.green,
                    fill: true,
                    borderWidth: 2
                }]
            },
            options: {
                legend: {
                    display: true,
                    labels: {
                        border: 1
                    }
                },
                title: {
                    display: true,
                    text: 'Payment List',
                    position: 'bottom'
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                tooltips: {
                    mode: 'index',
                    intersect: true
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                // Convert the number to a string and splite the string every 3 charaters from the end
                                value = value.toString();
                                value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                return value;
                            },
                            stacked: true
                        }
                    }],
                    xAxes: [{
                        //stacked: true,
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        },
                        stacked: true

                    }]
                },
                elements: {
                    line: {
                        tension: 0
                    }
                }
            }
        });
    }
    $scope.actionCompleteSelected = function (args) {
        if (args.requestType === "filtering") {
            var gridObj = $("#GridSelectedMasterPayment").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            createPaymentStackedBaChart(filtereddata);
        }
    }


    //#regoin Fixed Assets
    $scope.FixedAssetsList = [];
    $scope.GetFixedAssetsList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetFixedAssetsList",
                // data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //for (var i = 0; i < response.data.DATA.length; i++) {
                    //}
                    $scope.FixedAssetsList = response.data.DATA;
                    $scope.GetFixedArticalList();
                    createFixedAssetsBarChart(response.data.DATA);
                    // createFixedAssetsQuantityBarChart(response.data.DATA);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

        }

        catch (e) {

        }
    }

    $scope.FixedArticalList = [];
    $scope.GetFixedArticalList = function (id) {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetFixedArticalList",
                data: { /*FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate*/
                    "materialMasterId": $scope.id
                },
                dataType: 'JSON'
                // , contentType: "application/json charset=utf-8"

            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    for (var i = 0; i < response.data.DATA.length; i++) {
                    }
                    $scope.FixedArticalList = response.data.DATA;
                    window.FixedArticalList = response.data.DATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }


    $scope.data1 = $scope.FixedArticalList;
    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["MaterialMasterId"];
        var data = ej.DataManager(window.FixedArticalList).executeLocal(ej.Query().where("MaterialMasterId", "equal", filteredData, true).take(1000));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data,
            allowSelection: true,
            columns: [
                { field: "Code", headerText: "Code", width: 50 },
                { field: "Article", headerText: "Artical", width: 80 },
                { field: "MachineAllowance", headerText: "Machine Allowance", width: 30 },
                { field: "RPM", headerText: "RPM", width: 30 },
                { field: "StitchCode", headerText: "Stitch Code", width: 30 },
                { field: "FACount", headerText: "Total Qty", width: 30 },

                { field: "FABaseAmount", headerText: "FA Base Amount", width: 30 },
                { field: "SubAssetAmount", headerText: "Sub Asset Amount", width: 30 },
                { field: "TotalBaseAmount", headerText: "Total Base Amount ", width: 30 },
                { field: "ADBaseAmount", headerText: "AD Base Amount", width: 30 },
                { field: "NetFixedAssetsAmount", headerText: "Net Amount", width: 30 },


                {
                    headerText: "Action",
                    commands: [
                        {
                            type: "Register",
                            buttonOptions: {
                                text: "Register",
                                width: "70",

                            }
                        }
                    ],
                    isUnbound: true,
                    textAlign: ej.TextAlign.Center,
                    width: 25
                }

            ],
            recordClick: GetFixedAssetsRegisterPopUp,
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    $scope.PartyPaymentFixedAssetsRegisterPopUpList = [];
    function GetFixedAssetsRegisterPopUp(args) {
        //alert('dd');
        this.preventClick = true;
        $scope.tempMaterialMasterId = args.data.MaterialMasterId;
        $scope.tempMaterialMasterArticleId = args.data.MaterialMasterArticleId;
        $http({
            method: 'POST',
            url: 'Accounts/AccountStatusDashboard/GetFixedAssetsRegisterPopUpList',
            data: { 'materialMasterId': $scope.tempMaterialMasterId, 'materialMasterArticleId': $scope.tempMaterialMasterArticleId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                $scope.PartyPaymentFixedAssetsRegisterPopUpList = response.data;
            }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('fixedAssetsRegisterPopUp');
    }
    $scope.closeFixedAssetsRegisterPopUp = function () {
        //alert('hhh');
        angular.element(document.querySelector("#fixedAssetsRegisterPopUp")).modal("hide");
    };

    $scope.TotalFARegisterPopUpAmount = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "PurchasePrice", dataMember: "PurchasePrice", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FABaseAmount", dataMember: "FABaseAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SubAssetAmount", dataMember: "SubAssetAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalAssetAmount", dataMember: "TotalAssetAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ADBaseAmount", dataMember: "ADBaseAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetFixedAssetsAmount", dataMember: "NetFixedAssetsAmount", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];


    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    }

    $scope.exportMaterialgriddataUrl = 'Accounts/AccountStatusDashboard/MaterialMasterReport2';
    //$scope.downloadgriddataUrl = 'Accounts/AccountStatusDashboard/Download';
    $scope.downloadgriddataUrlPath = 'AccountStatusDashboard/DownloadUsingFullPath';//DownloadUsingPath
    $scope.getMasterReport2 = function () {
        try {
            var filtered = $("#gridTab").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                filtered = $scope.FixedAssetsList;
            }
            //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
            var MaterialMasterId = getString(filtered, "MaterialMasterId");
            var MaterialTypeId = getString(filtered, "MaterialTypeId");
            var AssetMasterId = getString(filtered, "AssetMasterId");
            var MaterialGroup1Id = getString(filtered, "MaterialGroup1Id");
            var BaseUOMId = getString(filtered, "BaseUOMId");
            var IsAsset = getString(filtered, "IsAsset");
            var Machine = getString(filtered, "Machine");
            var Process = getString(filtered, "ProcessId");
            var SkillId = getString(filtered, "SkillId");
            var FACount = getString(filtered, "FACount");

            //$scope.fileName = $scope.report.AssetsLiability + ".xls";
            $scope.fileName = "MaterialMasterReport.xls";

            $http({
                method: 'POST',
                // url: 'Attendances/DailyAttendanceReport/DailyAttendanceStatusReport',
                url: $scope.exportMaterialgriddataUrl,
                data: {
                    'materialMasterId': MaterialMasterId
                    //"voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                    , 'materialTypeId': MaterialTypeId
                    , 'assetMasterId': AssetMasterId
                    , 'materialGroup1Id': MaterialGroup1Id
                    , 'baseUOMId': BaseUOMId
                    , 'isAsset': IsAsset
                    , 'isMachine': Machine
                    , 'process': Process
                    , 'skillId': SkillId
                    , 'fACount': FACount

                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"

            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.exportgriddataArticleUrl = 'Accounts/AccountStatusDashboard/MaterialMasterArticalReport';
    //$scope.downloadgriddataUrl = 'Accounts/AccountStatusDashboard/Download';
    $scope.downloadgriddataUrlPath = 'AccountStatusDashboard/DownloadUsingFullPath';//DownloadUsingPath
    $scope.getMaterialMasterArticalReport = function () {
        try {
            var filtered = $("#gridTab").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                filtered = $scope.FixedAssetsList;
            }
            //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
            var MaterialMasterId = getString(filtered, "MaterialMasterId");
            var MaterialTypeId = getString(filtered, "MaterialTypeId");
            var AssetMasterId = getString(filtered, "AssetMasterId");

            var MaterialGroup1Id = getString(filtered, "MaterialGroup1Id");
            var BaseUOMId = getString(filtered, "BaseUOMId");
            var IsAsset = getString(filtered, "IsAsset");
            var Machine = getString(filtered, "Machine");
            var Process = getString(filtered, "ProcessId");
            var SkillId = getString(filtered, "SkillId");
            var FACount = getString(filtered, "FACount");

            //$scope.fileName = $scope.report.AssetsLiability + ".xls";
            $scope.fileName = "MaterialMasterArticleReport.xls";

            $http({
                method: 'POST',
                // url: 'Attendances/DailyAttendanceReport/DailyAttendanceStatusReport',
                url: $scope.exportgriddataArticleUrl,
                data: {
                    'materialMasterId': MaterialMasterId
                    //"voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                    , 'materialTypeId': MaterialTypeId
                    , 'assetMasterId': AssetMasterId
                    , 'materialGroup1Id': MaterialGroup1Id
                    , 'baseUOMId': BaseUOMId
                    , 'isAsset': IsAsset
                    , 'isMachine': Machine
                    , 'process': Process
                    , 'skillId': SkillId
                    , 'fACount': FACount


                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"

            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.exportRegistergriddataUrl = 'Accounts/AccountStatusDashboard/GetFixedAssetRegisterReport';
    //$scope.downloadgriddataUrl = 'Accounts/AccountStatusDashboard/Download';
    $scope.downloadgriddataUrlPath = 'AccountStatusDashboard/DownloadUsingFullPath';//DownloadUsingPath
    $scope.getFixedAssetRegisterReport = function () {
        try {
            var filtered = $("#gridTab").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                filtered = $scope.FixedAssetsList;
            }
            //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
            var MaterialMasterId = getString(filtered, "MaterialMasterId");
            var MaterialTypeId = getString(filtered, "MaterialTypeId");
            var AssetMasterId = getString(filtered, "AssetMasterId");

            var MaterialGroup1Id = getString(filtered, "MaterialGroup1Id");
            var BaseUOMId = getString(filtered, "BaseUOMId");
            var IsAsset = getString(filtered, "IsAsset");
            var Machine = getString(filtered, "Machine");
            var Process = getString(filtered, "ProcessId");
            var SkillId = getString(filtered, "SkillId");
            var FACount = getString(filtered, "FACount");

            //$scope.fileName = $scope.report.AssetsLiability + ".xls";
            $scope.fileName = "FixedAssetRegisterReport.xls";

            $http({
                method: 'POST',
                // url: 'Attendances/DailyAttendanceReport/DailyAttendanceStatusReport',
                url: $scope.exportRegistergriddataUrl,
                data: {
                    'materialMasterId': MaterialMasterId
                    //"voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                    , 'materialTypeId': MaterialTypeId
                    , 'assetMasterId': AssetMasterId
                    , 'materialGroup1Id': MaterialGroup1Id
                    , 'baseUOMId': BaseUOMId
                    , 'isAsset': IsAsset
                    , 'isMachine': Machine
                    , 'process': Process
                    , 'skillId': SkillId
                    , 'fACount': FACount


                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"

            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //#endregion

    //Bar Chart for Fixed Assets
    var fixedAssetsBarChart;
    $scope.fixedAssetsBarList = [];
    function createFixedAssetsBarChart(fixedAssetslist) {
        $scope.fixedAssetsBarList = [];
        $scope.fixedAssetsBaseAmount = [];
        $scope.accDepBaseAmount = [];

        $scope.FixedAsset = '';
        var netFixedAssetsAmount = 0.00;
        var aDBaseAmount = 0.00;
        fixedAssetslist = fixedAssetslist.sort((a, b) => (a.AssetMaster > b.AssetMaster) ? 1 : -1)

        angular.forEach(fixedAssetslist, function (item, i) {

            if ($scope.FixedAsset != item.AssetMaster) {
                var filteredData = fixedAssetslist.filter(function (obj) {
                    return obj["AssetMaster"] === item.AssetMaster;
                });
                netFixedAssetsAmount = filteredData.reduce(function (sum, item1) {
                    return sum + item1.NetFixedAssetsAmount;
                }, 0);
                aDBaseAmount = filteredData.reduce(function (sum, item2) {
                    return sum + item2.ADBaseAmount;
                }, 0);

                $scope.fixedAssetsBarList.push(item.AssetMaster);
                $scope.fixedAssetsBaseAmount.push(netFixedAssetsAmount);
                netFixedAssetsAmount = 0.00;

                $scope.accDepBaseAmount.push(aDBaseAmount);
                aDBaseAmount = 0.00;
            }
            $scope.FixedAsset = item.AssetMaster;
            netFixedAssetsAmount += item.NetFixedAssetsAmount;
            aDBaseAmount += item.ADBaseAmount;

        });


        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("FixedAssetsStackedBarChart").getContext('2d');
        if (fixedAssetsBarChart !== undefined && typeof fixedAssetsBarChart === 'object' && typeof fixedAssetsBarChart.destroy === 'function') fixedAssetsBarChart.destroy();
        fixedAssetsBarChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.fixedAssetsBarList,
                datasets: [{
                    label: 'Net Amount',
                    data: $scope.fixedAssetsBaseAmount,
                    backgroundColor: window.chartColors.yellow,
                    borderColor: window.chartColors.yellow,
                    fill: true,
                    borderWidth: 2
                },
                {
                    label: 'Acc.Dep.Amount',
                    data: $scope.accDepBaseAmount,
                    backgroundColor: window.chartColors.blue,
                    borderColor: window.chartColors.blue,
                    fill: true,
                    borderWidth: 2
                }

                ]
            },
            options: {
                legend: {
                    display: true,
                    labels: {
                        border: 1
                    }
                    //onClick: (e) => e.stopPropagation()
                },
                title: {
                    display: true,
                    text: 'Fixed Assets List',
                    position: 'bottom'
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                tooltips: {
                    mode: 'index',
                    intersect: true
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                value = value.toString();
                                value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                return value;
                            },
                            stacked: true
                        }
                    }],
                    xAxes: [{
                        //stacked: true,
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        },
                        stacked: true

                    }]
                },
                elements: {
                    line: {
                        tension: 0
                    }
                }
            }
        });
    }

    $scope.actionCompleteFixedAssetsSelected = function (args) {
        if (args.requestType === "filtering") {
            var gridObj = $("#gridTab").ejGrid("instance");

            var filtereddata = gridObj.getFilteredRecords();

            if (filtereddata.length == 0) {
                filtereddata = $scope.FixedAssetsList;
            }
            var result = [];
            filtereddata.reduce(function (res, value) {
                if (!res[value.AssetMaster]) {
                    res[value.AssetMaster] = { AssetMaster: value.AssetMaster, NetFixedAssetsAmount: 0, ADBaseAmount: 0 };
                    result.push(res[value.AssetMaster])
                }
                res[value.AssetMaster].NetFixedAssetsAmount += value.NetFixedAssetsAmount;
                res[value.AssetMaster].ADBaseAmount += value.ADBaseAmount;
                return res;
            }, {});

            createFixedAssetsBarChart(result);
        }
    }

    //Cash Tab
    $scope.CashList = [];
    $scope.getCashListData = function () {
        try {
            $http({
                method: 'POST',
                url: 'Accounts/AccountStatusDashboard/GetMasterCashListData',
                data: { /*'fromDate': $scope.reportParameters.FromDate,*/ 'toDate': $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.CashList = response.data;
                createCashBarChart(response.data);

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }

    //Bank Tab
    $scope.BankMasterList = [];
    $scope.GetBankMasterListData = function () {
        try {
            $http({
                method: 'POST',
                url: 'Accounts/AccountStatusDashboard/GetBankMasterListData',
                data: { /*'fromDate': $scope.reportParameters.FromDate,*/ 'toDate': $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.BankMasterList = response.data;
                createBankBarChart(response.data);

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.GetBankMasterListData();

    //............#regon Cash Bar Chart................
    var CashbarChart;
    $scope.cashBarList = [];
    function createCashBarChart(CashBarChartlist) {

        $scope.cashBarList = [];
        $scope.booksCashBalance = [];

        angular.forEach(CashBarChartlist, function (item, i) {
            $scope.cashBarList.push(item.Cash);
            $scope.booksCashBalance.push(item.BooksCashBalance);
        });

        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("CashMasterbarChart").getContext('2d');
        if (CashbarChart !== undefined && typeof CashbarChart === 'object' && typeof CashbarChart.destroy === 'function') CashbarChart.destroy();
        CashbarChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.cashBarList,
                datasets: [
                    {
                        label: '',
                        data: $scope.booksCashBalance,
                        backgroundColor: window.chartColors.yellow,
                        borderColor: window.chartColors.yellow,
                        fill: true,
                        borderWidth: 2

                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: 'Cash List',
                    position: 'bottom'
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                tooltips: {
                    mode: 'index',
                    intersect: true
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                value = value.toString();
                                value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                return value;
                            },
                        }
                    }],
                    xAxes: [{
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        },
                    }]
                },
                elements: {
                    line: {
                        tension: 0
                    }
                }
            }
        });
    }
    //#endregon Cash bar chart

    //#regon Bank Bar Chart
    var BankbarChart;
    $scope.bankBarList = [];
    function createBankBarChart(BankBarChartlist) {
        $scope.bankBarList = [];
        $scope.booksBankBalance = [];

        angular.forEach(BankBarChartlist, function (item, i) {
            $scope.bankBarList.push(item.Bank);
            $scope.booksBankBalance.push(item.BooksBankBalance);
        });

        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("BankMasterbarChart").getContext('2d');
        if (BankbarChart !== undefined && typeof BankbarChart === 'object' && typeof BankbarChart.destroy === 'function') BankbarChart.destroy();
        BankbarChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.bankBarList,
                datasets: [{
                    label: '',
                    data: $scope.booksBankBalance,
                    backgroundColor: window.chartColors.blue,
                    borderColor: window.chartColors.blue,
                    fill: true,
                    borderWidth: 2
                }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: 'Bank List',
                    position: 'bottom'
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                tooltips: {
                    mode: 'index',
                    intersect: true
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                value = value.toString();
                                value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                return value;
                            },
                        }
                    }],
                    xAxes: [{
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        },
                    }]
                },
                elements: {
                    line: {
                        tension: 0
                    }
                }
            }
        });
    }
    //#endregon bank bar chart

    //...........#regon Loan.......
    $scope.LoanList = [];
    $scope.GetLoanListData = function () {
        try {
            $http({
                method: 'POST',
                url: 'Accounts/AccountStatusDashboard/GetLoanListData',
                data: { /*'fromDate': $scope.reportParameters.FromDate, 'toDate': $scope.reportParameters.ToDate*/
                    'transactionType': $scope.reportParameters.TransactionType
                },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.LoanList = response.data;
                createLoanTakenBarChart(response.data);
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }


    $scope.summaryRows = [{
        title: "Total Balance", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Balance", dataMember: "Balance", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.GetAllLoanRegisterReport = function (DataValue) {
        var data = DataValue.data;
        var url = 'accounts/AccountStatusDashboard/GetLoanRegisterLedgerReport?reportFormat=' + 'Excel' + '&transactionType=' + data.TransactionType + '&voucherId=' + data.VoucherId + '&financingId=' + data.FinancingId;
        $window.open(url, '_blank');
    };

    $scope.printLoanVoucherReport = function (obj) {
        var data = obj.data;
        var file_src = 'Accounts/Loan/LoanReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        $rootScope.report(file_src);
    }

    $scope.LoanTakenSetOffPoPUpList = [];
    $scope.GetLoanTakenSetOffPopUpData = function (FId) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetLoanTakenSetOffPopUpData?financingId=" + FId
        }).then(function successCallback(response) {
            $scope.LoanTakenSetOffPoPUpList = response.data;
            $scope.loanNo = $scope.LoanTakenSetOffPoPUpList[0].LoanNo
            $scope.partyName = $scope.LoanTakenSetOffPoPUpList[0].Party
            $scope.accountNumber = $scope.LoanTakenSetOffPoPUpList[0].AccountNumber
        });
        $rootScope.openPopupAngular('loanTakenSetOffPopUp');
        // angular.element(document.querySelector("#loanTakenSetOffPopUp")).modal("show");
    };

    $scope.showLoanTakenSetOffPopUp = function (args) {
        $scope.GetLoanTakenSetOffPopUpData(args.FinancingId)
    };

    $scope.closeloanTakenSetOffPopUp = function () {
        angular.element(document.querySelector("#loanTakenSetOffPopUp")).modal("hide");
    };

    $scope.printLoanTakenSetOffVoucherReport = function (obj) {
        var data = obj.data;
        var file_src = 'Accounts/Loan/LoanPaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        $rootScope.report(file_src);
    }

    $scope.LoanTakenInterestPoPUpList = [];
    $scope.GetLoanTakenInterestPopUpData = function (FId) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetLoanTakenInterestPopUpData?financingId=" + FId
        }).then(function successCallback(response) {
            $scope.LoanTakenInterestPoPUpList = response.data;
            $scope.loanNo = $scope.LoanTakenInterestPoPUpList[0].LoanNo
            $scope.partyName = $scope.LoanTakenInterestPoPUpList[0].Party
            $scope.accountNumber = $scope.LoanTakenInterestPoPUpList[0].AccountNumber
        });
        $rootScope.openPopupAngular('loanTakenInterestPopUp');

    };

    $scope.showLoanTakenInterestPopUp = function (args) {
        //var gridObj = $("#GridSelectedLoan").data("ejGrid");
        //var data = gridObj.getSelectedRecords()[0];
        $scope.tempFinancingId = args.FinancingId;
        $scope.GetLoanTakenInterestPopUpData($scope.tempFinancingId)
    };

    $scope.closeloanTakenInterestPopUp = function () {
        angular.element(document.querySelector("#loanTakenInterestPopUp")).modal("hide");
    };

    $scope.summaryRowsLoanTakenInterest = [{
        title: "Total Interest", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Interest", dataMember: "Interest", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.printLoanTakenInterestVoucherReport = function (obj) {
        var data = obj.data;
        var file_src = 'Accounts/Loan/LoanIntersetPayableReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId + '&sourceType=' + data.SourceType;
        $rootScope.report(file_src);
        // $window.open(url, '_blank');
    }

    $scope.LoanTakenChargesPayablePoPUpList = [];
    $scope.getLoanTakenChargesPayblePopUpData = function (FId) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetLoanTakenChargesPayablePopUpData?financingId=" + FId
        }).then(function successCallback(response) {
            $scope.LoanTakenChargesPayablePoPUpList = response.data;
            $scope.loanNo = $scope.LoanTakenChargesPayablePoPUpList[0].LoanNo
            $scope.partyName = $scope.LoanTakenChargesPayablePoPUpList[0].Party
            $scope.accountNumber = $scope.LoanTakenChargesPayablePoPUpList[0].AccountNumber

        });
        $rootScope.openPopupAngular('loanTakenChargesPayablePopUp');
    };

    $scope.showLoanTakenChargesPayablePopUp = function (args) {
        $scope.tempFinancingId = args.FinancingId;
        $scope.getLoanTakenChargesPayblePopUpData($scope.tempFinancingId)
    };

    $scope.closeloanTakenChargesPayablePopUp = function () {
        angular.element(document.querySelector("#loanTakenChargesPayablePopUp")).modal("hide");
    };

    $scope.summaryRowsLoanTakenChargesPayable = [{
        title: "Total Charges", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ChargesPayable", dataMember: "ChargesPayable", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.printLoanTakenChargesPayableVoucherReport = function (obj) {
        var data = obj.data;
        var file_src = 'Accounts/Loan/LoanIntersetPayableReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId + '&sourceType=' + data.SourceType;
        $rootScope.report(file_src);
        // $window.open(url, '_blank');
    }

    $scope.LoanTakenAdditionalLoanPayablePoPUpList = [];
    $scope.getLoanTakenAdditionalLoanPayblePopUpData = function (FId) {
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetLoanTakenAdditionalLoanPayablePopUpData?financingId=" + FId
        }).then(function successCallback(response) {
            $scope.LoanTakenAdditionalLoanPayablePoPUpList = response.data;
            $scope.partyName = $scope.LoanTakenAdditionalLoanPayablePoPUpList[0].Party
            $scope.accountNumber = $scope.LoanTakenAdditionalLoanPayablePoPUpList[0].AccountNumber
            $scope.loanNo = $scope.tempDocRefNo
        });
        $rootScope.openPopupAngular('loanTakenAdditionalLoanPayablePopUp');
    };

    $scope.showLoanTakenAdditionalLoanPayablePopUp = function (args) {
        //var gridObj = $("#GridSelectedLoan").data("ejGrid");
        //var data = gridObj.getSelectedRecords()[0];
        $scope.tempDocRefNo = args.DocRefNo;
        $scope.tempFinancingId = args.FinancingId;
        $scope.getLoanTakenAdditionalLoanPayblePopUpData($scope.tempFinancingId)
    };

    $scope.closeloanTakenAdditionalLoanPayablePopUp = function () {
        angular.element(document.querySelector("#loanTakenAdditionalLoanPayablePopUp")).modal("hide");
    };

    $scope.summaryRowsLoanTakenAdditionalLoanPayable = [{
        title: "Total Additional Loan", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AdditionalLoanPayable", dataMember: "AdditionalLoanPayable", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.printLoanTakenAdditionalLoanPayableVoucherReport = function (obj) {
        var data = obj.data;
        var url = 'Accounts/Loan/LoanIntersetPayableReport?reportFormat=' + 'Pdf' + '&voucherId=' + data.VoucherId + '&sourceType=' + data.SourceType;
        // $rootScope.report(file_src);
        $window.open(url, '_blank');
    }
    //.............#endregon Loan.............

    //#regon Loan Taken Bar chart

    var loanTakenBarChart;
    $scope.loanTakenBarList = [];
    function createLoanTakenBarChart(LoanTakenBarChartlist) {
        $scope.loanTakenBarList = [];
        $scope.loanTakenBalance = [];

        $scope.LoanTakenParticulars = '';
        var LoanTakenBalanceAmount = 0.00;
        LoanTakenBarChartlist = LoanTakenBarChartlist.sort((a, b) => (a.Particulars > b.Particulars) ? 1 : -1)
        angular.forEach(LoanTakenBarChartlist, function (item, i) {

            if ($scope.LoanTakenParticulars != item.Particulars) {
                var filteredData = LoanTakenBarChartlist.filter(function (obj) {
                    return obj["Particulars"] === item.Particulars;
                });
                LoanTakenBalanceAmount = filteredData.reduce(function (sum, item1) {
                    return sum + item1.Balance;
                }, 0);

                $scope.loanTakenBarList.push(item.Particulars);
                $scope.loanTakenBalance.push(LoanTakenBalanceAmount);
                LoanTakenBalanceAmount = 0.00;
            }
            $scope.LoanTakenParticulars = item.Particulars;
            LoanTakenBalanceAmount += item.Balance;
        });

        Chart.defaults.global.legend.display = false;
        var MPctx = document.getElementById("LoanTakenBarChart").getContext('2d');
        if (loanTakenBarChart !== undefined && typeof loanTakenBarChart === 'object' && typeof loanTakenBarChart.destroy === 'function') loanTakenBarChart.destroy();
        loanTakenBarChart = new Chart(MPctx, {
            type: 'bar',
            data: {
                labels: $scope.loanTakenBarList,
                datasets: [
                    {
                        label: '',
                        data: $scope.loanTakenBalance,
                        backgroundColor: window.chartColors.lightBlue,
                        borderColor: window.chartColors.lightBlue,
                        fill: true,
                        borderWidth: 2
                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: 'Loan Taken List',
                    position: 'bottom'
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                tooltips: {
                    mode: 'index',
                    intersect: true
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                            userCallback: function (value, index, values) {
                                value = value.toString();
                                value = value.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
                                return value;
                            },
                        }

                    }],
                    xAxes: [{
                        ticks: {
                            beginAtZero: true,
                            autoSkip: false,
                            maxRotation: 90,
                            minRotation: 90
                        },
                    }]
                },
                elements: {
                    line: {
                        tension: 0
                    }
                }
            }
        });
    }

    $scope.actionCompleteLoanSelected = function (args) {
        if (args.requestType === "filtering") {
            var gridObj = $("#GridSelectedLoan").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length == 0) {
                filtereddata = $scope.LoanList;
            }
            createLoanTakenBarChart(filtereddata);
        }
    }
    //#endregon loan taken

    //...............#regoin Customer Tab ..........................
    $scope.CustomerReceiptMasterList = [];
    $scope.GetCustomerReceiptMasterList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetFinancialDashboardCustomerReceiptMasterList",
                data: {
                    fromDate: "",
                    toDate: $scope.material.CustomerToDate
                },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.CustomerReceiptMasterList = response.data.DATA;
                    $scope.GetPartyReceiveStatusPendingAdjustmentData();
                    $scope.GetCustomerReceivablePaiChartList();
                }
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    //$scope.GetCustomerReceiptMasterList();

    $scope.crAgingsummaryRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksGross", dataMember: "BooksGross", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksSetOff", dataMember: "BooksSetOff", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];

    $scope.refreshTemplatecustomer = function (args) {
        $("#headchkcustomer").ejCheckBox({ "change": CheckBoxSelectAllCustomer });
    };

    function CheckBoxSelectAllCustomer(e) {

        var ChkOrUnchkCustomer = false;
        if (e.model.checkState === "check") {
            ChkOrUnchkCustomer = true;

        }

        var filtered = $("#GridMasterCustomerReceipt").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.CustomerReceiptMasterList.length; i++) {
                $scope.CustomerReceiptMasterList[i].isSelected = ChkOrUnchkCustomer;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchkCustomer;
            }


        }
        var gridObj = $("#GridMasterCustomerReceipt").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.CustomerSummaryReport = function () {
        try {
            var NewMasterCustomerList = [];
            for (var i = 0; i < $scope.CustomerReceiptMasterList.length; i++) {
                if ($scope.CustomerReceiptMasterList[i].isSelected == true) {
                    NewMasterCustomerList.push($scope.CustomerReceiptMasterList[i]);
                }
            }
            if (NewMasterCustomerList.length == 0) {
                ShowResult('Please select at least one Party', 'failure');

            } else {

                $scope.downloadgriddataUrl = 'GridReports/Download';
                $http({
                    method: 'POST',
                    url: $scope.path + "FinancialDashboardCustomerSummaryReport",
                    data: {
                        'masterCustomerSummaryList': NewMasterCustomerList,
                        'fromDate': "",
                        'toDate': $scope.material.CustomerToDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                    else {
                        ShowResult(response.data.Message, 'failure');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CustomerAgingReport = function () {

        try {
            //if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
            //    throw 'Please enter from date';

            //if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
            //    throw 'Please enter to date';

            var NewMasterCustomerAgingList = [];
            for (var i = 0; i < $scope.CustomerReceiptMasterList.length; i++) {
                if ($scope.CustomerReceiptMasterList[i].isSelected == true) {

                    if (NewMasterCustomerAgingList, $scope.CustomerReceiptMasterList[i].PartyId) {
                        NewMasterCustomerAgingList.push($scope.CustomerReceiptMasterList[i].PartyId);
                    }
                }
            }
            if (NewMasterCustomerAgingList.length == 0) {
                ShowResult('Please select at least one Party', 'failure');
            }
            else {
                var file_src = $scope.path + "FinancialDashboardCustomerReceiptAgingReport?masterCustomerReceiptAgingList=" + NewMasterCustomerAgingList + '&fromDate=' + "" + '&toDate=' + $scope.material.CustomerToDate;
                $rootScope.report(file_src);
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //Pai Chart Customer
    $scope.ODueMoreThan30CustomerPai = 0;
    $scope.ODueMoreThan30CustomerNoOfInvoice = 0;

    $scope.ODueMoreThan15CustomerPai = 0;
    $scope.ODueMoreThan15CustomerNoOfInvoice = 0;

    $scope.ODueLessThan15CustomerPai = 0;
    $scope.ODueLessThan15CustomerNoOfInvoice = 0;

    $scope.TodayBalanceCustomerPai = 0;
    $scope.TodayBalanceCustomerNoOfInvoice = 0;

    $scope.OneToSevenBalanceCustomerPai = 0;
    $scope.OneToSevenBalanceCustomerNoOfInvoice = 0;

    $scope.EightToThirtyBalanceCustomerPai = 0;
    $scope.EightToThirtyBalanceCustomerNoOfInvoice = 0;

    $scope.ThirtyToSixtyBalanceCustomerPai = 0;
    $scope.ThirtyToSixtyBalanceCustomerNoOfInvoice = 0;

    $scope.Onword60CustomerPai = 0;
    $scope.Onword60CustomerNoOfInvoice = 0;

    $scope.GetCustomerReceivablePaiChartList = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "GetFinancialDashboardCustomerReceivablePaiChartList",
                data: { /*FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate*/ },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.CustomerReceivablePiaChartList = response.data;

                $scope.ODueMoreThan30CustomerPai = $scope.CustomerReceivablePiaChartList[0]["ODueMoreThan30"];
                $scope.ODueMoreThan30CustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["ODueMoreThan30NoOfInvoice"];

                $scope.ODueMoreThan15CustomerPai = $scope.CustomerReceivablePiaChartList[0]["ODueMoreThan15"];
                $scope.ODueMoreThan15CustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["ODueMoreThan15NoOfInvoice"];

                $scope.ODueLessThan15CustomerPai = $scope.CustomerReceivablePiaChartList[0]["ODueLessThan15"];
                $scope.ODueLessThan15CustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["ODueLessThan15NoOfInvoice"];

                $scope.TodayBalanceCustomerPai = $scope.CustomerReceivablePiaChartList[0]["TodayBalance"];
                $scope.TodayBalanceCustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["TodayBalanceNoOfInvoice"];

                $scope.OneToSevenBalanceCustomerPai = $scope.CustomerReceivablePiaChartList[0]["OneToSevenBalance"];
                $scope.OneToSevenBalanceCustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["OneToSevenBalanceNoOfInvoice"];

                $scope.EightToThirtyBalanceCustomerPai = $scope.CustomerReceivablePiaChartList[0]["EightToThirtyBalance"];
                $scope.EightToThirtyBalanceCustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["EightToThirtyBalanceNoOfInvoice"];

                $scope.ThirtyToSixtyBalanceCustomerPai = $scope.CustomerReceivablePiaChartList[0]["ThirtyToSixtyBalance"];
                $scope.ThirtyToSixtyBalanceCustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["ThirtyToSixtyBalanceNoOfInvoice"];

                $scope.Onword60CustomerPai = $scope.CustomerReceivablePiaChartList[0]["Onword60"];
                $scope.Onword60CustomerNoOfInvoice = $scope.CustomerReceivablePiaChartList[0]["Onword60NoOfInvoice"];

                $scope.chartCustomerReceivablePiaChartLabel = ['ODueMoreThan30', 'ODueMoreThan15', 'ODueLessThan15', 'TodayBalance', 'OneToSevenBalance', 'EightToThirtyBalance', 'ThirtyToSixtyBalance', 'Onword60'];

                $scope.totalCustomerReceivableAgingPiaAndTable = $scope.ODueMoreThan30CustomerPai + $scope.ODueMoreThan15CustomerPai + $scope.ODueLessThan15CustomerPai + $scope.TodayBalanceCustomerPai
                    + $scope.OneToSevenBalanceCustomerPai + $scope.EightToThirtyBalanceCustomerPai + $scope.ThirtyToSixtyBalanceCustomerPai + $scope.Onword60CustomerPai;
                $scope.chartCustomerReceivablePiaChartList = [$scope.ODueMoreThan30CustomerPai, $scope.ODueMoreThan15CustomerPai, $scope.ODueLessThan15CustomerPai, $scope.TodayBalanceCustomerPai, $scope.OneToSevenBalanceCustomerPai, $scope.EightToThirtyBalanceCustomerPai, $scope.ThirtyToSixtyBalanceCustomerPai, $scope.Onword60CustomerPai];
                createCustomerReceivablePieChart();
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }


    var CUSRPieChart;
    $scope.totalCustomerReceivableAgingPiaAndTable = 0.00;
    $scope.chartCustomerReceivablePiaChartLabel = ['ODueMoreThan30', 'ODueMoreThan15', 'ODueLessThan15', 'TodayBalance', 'OneToSevenBalance', 'EightToThirtyBalance', 'ThirtyToSixtyBalance', 'Onword60'];
    $scope.chartCustomerReceivablePiaChartList = [];

    function createCustomerReceivablePieChart() {

        Chart.defaults.global.legend.display = false;
        var CUSRctx = document.getElementById("customerReceivablePieChart").getContext('2d');

        if (CUSRPieChart !== undefined && typeof CUSRPieChart === 'object' && typeof CUSRPieChart.destroy === 'function') CUSRPieChart.destroy();
        CUSRPieChart = new Chart(CUSRctx, {
            type: 'doughnut',
            data: {
                labels: $scope.chartCustomerReceivablePiaChartLabel,
                datasets: [{
                    label: '',
                    data: $scope.chartCustomerReceivablePiaChartList,
                    backgroundColor: [
                        'rgba(242, 38, 19, 1)',
                        'rgba(150, 40, 27, 1)',
                        'rgba(231, 76, 60,0.7)',
                        'rgba(82, 179, 217, 0.7)',
                        'rgba(253, 227, 167, 0.7)',
                        'rgba(65, 246, 188, 0.7)',
                        'rgba(196, 171, 93, 0.46)',
                        'rgba(196, 93, 119, 0.46)'
                    ],
                    borderColor: [
                        'rgba(46, 204, 113,0.7)',
                        'rgba(241, 196, 15, 0.7)',
                        'rgba(231, 76, 60,0.7)',
                        'rgba(82, 179, 217, 0.7)',
                        'rgba(253, 227, 167, 0.7)',
                        'rgba(65, 246, 188, 0.7)',
                        'rgba(196, 171, 93, 0.46)',
                        'rgba(196, 93, 119, 0.46)'

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
                            return $scope.chartCustomerReceivablePiaChartLabel[tooltipItem[0].index];
                        }
                    }
                }
            }
        });
    }

    //customer receivable Pia table aging popUp and get data
    $scope.CustomerReceivableAgingSlotType = null;
    $scope.CustomerReceivableAgingDueList = [];
    $scope.getCustomerReceivableAgingSlotPopUpDataList = function (crDueDays, crAgingType) {
        $scope.CustomerReceivableAgingSlotType = crAgingType

        $scope.CustomerReceivableAgingDueList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCustomerReceivableAgingDueList?overDueDaysSlot=" + crDueDays
        }).then(function successCallback(response) {
            $scope.CustomerReceivableAgingDueList = response.data;

            if (crAgingType == 'ODueMoreThan30Pai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.CustomerReceivableAgingDueList[i].ODueMoreThan30;
                }
            }
            else if (crAgingType == 'ODueMoreThan15Pai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.CustomerReceivableAgingDueList[i].ODueMoreThan15;
                }
            }
            else if (crAgingType == 'ODueLessThan15Pai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.partyWiseAgingDueList[i].ODueLessThan15;
                }
            }
            else if (crAgingType == 'TodayBalancePai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.CustomerReceivableAgingDueList[i].TodayBalance;
                }
            }
            else if (crAgingType == 'OneToSevenBalancePai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.CustomerReceivableAgingDueList[i].OneToSevenBalance;
                }
            }
            else if (crAgingType == 'EightToThirtyBalancePai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.CustomerReceivableAgingDueList[i].EightToThirtyBalance;
                }
            }
            else if (crAgingType == 'ThirtyToSixtyBalancePai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.CustomerReceivableAgingDueList[i].ThirtyToSixtyBalance;
                }
            }
            else if (crAgingType == 'Onword60Pai') {
                for (var i = 0; i < $scope.CustomerReceivableAgingDueList.length; i++) {
                    $scope.CustomerReceivableAgingDueList[i].Amount = $scope.CustomerReceivableAgingDueList[i].Onword60;
                }
            }

        });
    };

    $scope.showCustomerReceivableAgingSlotPopUp = function (agingSlotType) {
        if (agingSlotType == 'ODueMoreThan30Pai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-30';
            $scope.CRHeadingAgingSlotType = 'Over Due More Than 30 List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail Over Due More Than 30'
            //$scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Over Due More Than 30'

        }
        else if (agingSlotType == 'ODueMoreThan15Pai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-30';
            $scope.CRHeadingAgingSlotType = 'Over Due More Than 15 List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail Over Due More Than 15'
            //$scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail More Then Fifteen'


        }
        else if (agingSlotType == 'ODueLessThan15Pai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-15';
            $scope.CRHeadingAgingSlotType = 'Over Due Less Than 15 List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail Over Due Less Than 15'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Less Then Fifteen'

        }
        else if (agingSlotType == 'TodayBalancePai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0';
            $scope.CRHeadingAgingSlotType = 'Today Balance List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail Today'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Today'

        }
        else if (agingSlotType == 'OneToSevenBalancePai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=7';
            $scope.CRHeadingAgingSlotType = 'One Two Seven Balance List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail One To Seven'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail One To Seven'

        }
        else if (agingSlotType == 'EightToThirtyBalancePai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>7 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=30';
            $scope.CRHeadingAgingSlotType = 'Eight To Thirty Balance List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail Eight To Thirty'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Eight To Thirty'

        }
        else if (agingSlotType == 'ThirtyToSixtyBalancePai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>30 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=60';
            $scope.CRHeadingAgingSlotType = 'Thirty To Sixty Balance List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail Thirty To Sixty'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail Thirty To Sixty'

        }
        else if (agingSlotType == 'Onword60Pai') {
            $scope.customerReceivableDaysDue = 'DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>60';
            $scope.CRHeadingAgingSlotType = 'On Ward Sixty Balance List'
            $scope.CRHeadingSetOffDetailAgingSlotType = 'Set-Off Detail On Word Sixty'
            $scope.AgingDueTypeNoOfInvoiceDetail = 'List of Invoice Detail On Word Sixty'

        }

        $scope.getCustomerReceivableAgingSlotPopUpDataList($scope.customerReceivableDaysDue, agingSlotType);
        angular.element(document.querySelector("#customerReceivableAgingSlotPopUp")).modal("show");

    };

    $scope.closeCustomerReceivableAgingSlotPopUp = function () {
        angular.element(document.querySelector("#customerReceivableAgingSlotPopUp")).modal("hide");
    };

    //CR SetOff detail PopUp and get data
    $scope.customerReceivableSetOffDetailList = [];
    //$scope.PartyId = null;
    $scope.getCustomerReceivableSetOffDetailList = function (pid, crDue) {

        $scope.customerReceivableSetOffDetailList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCustomerReceivableSetOffDetailList?partyId=" + pid + '&crDueDaysSetOffDetail=' + crDue
        }).then(function successCallback(response) {
            $scope.customerReceivableSetOffDetailList = response.data;

            $scope.booksDebitNote = $scope.customerReceivableSetOffDetailList[0].BooksDebitNoteAmount
            $scope.booksDiscount = $scope.customerReceivableSetOffDetailList[0].BooksDiscountAmount
            $scope.booksTax = $scope.customerReceivableSetOffDetailList[0].BooksTaxAmount
            $scope.booksPayment = $scope.customerReceivableSetOffDetailList[0].BooksSetOff
            $scope.booksSetOffTotal = $scope.booksDebitNote + $scope.booksDiscount + $scope.booksTax + $scope.booksPayment

        });
    };

    $scope.showCRSetOffDetailPopUp = function (args) {
        var gridObj = $("#customerReceivableAgingDueSlot").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.tempCRSetOffPartyId = data.PartyId;
        $scope.getCustomerReceivableSetOffDetailList($scope.tempCRSetOffPartyId, $scope.customerReceivableDaysDue)

        angular.element(document.querySelector("#customerReceivableSetOffDetailPopUp")).modal("show");
    };

    $scope.closeCustomerReceivableSetOffDetailPopUp = function () {
        angular.element(document.querySelector("#customerReceivableSetOffDetailPopUp")).modal("hide");
    };

    //Payment Detail PopUp and get data
    $scope.CRPaymentDetailPoPUpList = [];
    $scope.GetCRPaymentDetailPopUp = function () {
        $scope.tempCRSetOffPartyId
        $scope.customerReceivableDaysDue
        $http({
            method: 'POST',
            url: 'Accounts/AccountStatusDashboard/GetCRPaymentDetailPopUpList',
            data: { 'crPaymentDetailDueDays': $scope.customerReceivableDaysDue, 'partyId': $scope.tempCRSetOffPartyId },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.CRPaymentDetailPoPUpList = response.data;
            }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('customerReceivablePaymentDetailPopUp');
        //$scope.tempCRSetOffPartyId = null;
    }


    $scope.closeCustomerReceivablePaymentDetailPopUp = function () {
        //alert('hhh');
        angular.element(document.querySelector("#customerReceivablePaymentDetailPopUp")).modal("hide");
    };

    //CR Aging Invoice detail PopUp
    $scope.CustomerReceivableAgingInvoiceDetailList = [];
    //$scope.PartyId = null;
    $scope.getCustomerReceivableInvoiceDetailList = function (idp, dueDays) {
        $scope.CustomerReceivableAgingInvoiceDetailList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCustomerReceivableAgingInvoiceDetailList?partyId=" + idp + '&crAgingInvoiceDetailDueDaya=' + dueDays
        }).then(function successCallback(response) {
            $scope.CustomerReceivableAgingInvoiceDetailList = response.data;
            //$scope.debitNote = $scope.partyWiseAgingDueVoucherList[0].DebitNoteAmount
        });
    };

    $scope.showCRInvoiceDetailPopUp = function (args) {
        var gridObj = $("#customerReceivableAgingDueSlot").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.tempCRIDPartyId = data.PartyId;
        $scope.tempCRIDPartyName = data.PartyName;
        $scope.getCustomerReceivableInvoiceDetailList($scope.tempCRIDPartyId, $scope.customerReceivableDaysDue)

        angular.element(document.querySelector("#crAgingInvoiceDetailPopUp")).modal("show");
    };

    $scope.closeCRAgingInvoiceDetailPopUp = function () {
        angular.element(document.querySelector("#crAgingInvoiceDetailPopUp")).modal("hide");
    };

    //Invoice Detail Voucher Print
    $scope.printCRInvoiceDetailVoucherReport = function (obj) {
        var data = obj.data;
        if (data.SourceType == 'CustomerInvoice')
            var file_src = 'Accounts/Invoice/GetCustomerInvoiceVoucherReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'CustomerReceipt')
            var file_src = 'Accounts/invoice/CustomerInvoiceReceiptReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        if (data.SourceType == 'CustomerBanksReceipt')
            var file_src = 'Accounts/invoice/CustomerInvoiceReceiptBanksReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        $rootScope.report(file_src);
    }

    //CR SetOff Detail PopUp and get data
    $scope.customerReceivableInvoiceSetOffDetailList = [];
    $scope.getCustomerReceivableInvoiceSetOffDetailList = function (setoffpid, crInvDue) {

        $scope.customerReceivableSetOffDetailList = [];
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCustomerReceivableInvoiceSetOffDetailList?partyId=" + setoffpid + '&crDueDaysInvoiceSetOffDetail=' + crInvDue
        }).then(function successCallback(response) {
            $scope.customerReceivableInvoiceSetOffDetailList = response.data;

            $scope.crInvSODBooksDebitNote = $scope.customerReceivableInvoiceSetOffDetailList[0].BooksDebitNoteAmount
            $scope.crInvSODBooksDiscount = $scope.customerReceivableInvoiceSetOffDetailList[0].BooksDiscountAmount
            $scope.crInvSODBooksTax = $scope.customerReceivableInvoiceSetOffDetailList[0].BooksTaxAmount
            $scope.crInvSODBooksPayment = $scope.customerReceivableInvoiceSetOffDetailList[0].BooksSetOff
            $scope.crInvSODBooksSetOffTotal = $scope.crInvSODBooksDebitNote + $scope.crInvSODBooksDiscount + $scope.crInvSODBooksTax + $scope.crInvSODBooksPayment

        });
    };

    $scope.showCRAgingInvoiceSetOffDetailPopUp = function (args) {
        var gridObj = $("#GridCRAgingInvoiceDetail").data("ejGrid");
        $scope.tempCRInvSetOffPartyId = args.PartyId;
        $scope.getCustomerReceivableInvoiceSetOffDetailList($scope.tempCRInvSetOffPartyId, $scope.customerReceivableDaysDue)

        angular.element(document.querySelector("#customerReceivableInvoiceSetOffDetailPopUp")).modal("show");
    };

    $scope.closeCustomerReceivableInvoiceSetOffDetailPopUp = function () {
        angular.element(document.querySelector("#customerReceivableInvoiceSetOffDetailPopUp")).modal("hide");
    };

    //CR Invoice Payment Detail
    $scope.CRInvoicePaymentDetailPoPUpList = [];
    $scope.GetCRInvoicePaymentDetailPopUp = function () {

        $http({
            method: 'POST',
            url: 'Accounts/AccountStatusDashboard/GetCRInvoicePaymentDetailPopUp',
            data: { 'crPaymentDetailDueDays': $scope.customerReceivableDaysDue, 'partyId': $scope.tempCRSetOffPartyId },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.CRInvoicePaymentDetailPoPUpList = response.data;
            }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('crInvoicePaymentDetailPopUp');
    }

    $scope.closeCRInvoicePaymentDetailPopUp = function () {
        //alert('hhh');
        angular.element(document.querySelector("#crInvoicePaymentDetailPopUp")).modal("hide");
    };
    //...............#endregoin Customer Tab .......................

    //........#regoin Trian Balance.................


    $scope.upToLevelList = [];
    $scope.getLevelType = function () {
        $http({
            method: "GET",
            url: "Enum/GetTrailBalanceLevelCbo/"
        }).then(function successCallback(response) {
            $scope.upToLevelList = response.data;
            $scope.report.IsUpToLevel = response.data[3].Value;
        });
    };
    $scope.getLevelType();



    $scope.LevelAssaign = function (level) {
        if (level == 'GL') {
            $scope.report.IsBudgetLevel = false;
            $scope.report.IsActivityLevel = false;
            $scope.report.IsDetailLevel = false;

        }
        if (level == 'Budget') {
            $scope.report.IsBudgetLevel = true;
            $scope.report.IsActivityLevel = false;
            $scope.report.IsDetailLevel = false;

        }
        if (level == 'Detail') {
            $scope.report.IsDetailLevel = true;
            $scope.report.IsBudgetLevel = false;
            $scope.report.IsActivityLevel = false;
        }

        else if (level == 'Activity') {
            $scope.report.IsBudgetLevel = false;
            $scope.report.IsDetailLevel = false;
            $scope.report.IsActivityLevel = true;

        }
    };

    $scope.TrialBalanceList = [];
    $scope.GetTrialBalanceData = function () {

        $scope.LevelAssaign($scope.report.IsUpToLevel);

        try {
            $http({
                method: 'POST',
                url: 'Accounts/AccountStatusDashboard/GetTrialBalanceData',
                //var url = 'Accounts/Voucher/TrialBalanceReport?reportFormat=' + $scope.report.ReportFormat + '&date=' + $scope.report.FromDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel + '&isDetailLevel=' + $scope.report.IsDetailLevel;
                data: { /*'fromDate': $scope.reportParameters.FromDate, 'toDate': $scope.reportParameters.ToDate
                    'transactionType': $scope.reportParameters.TransactionType*/
                    'toDate': $scope.report.ToDate,
                    'isBudgetLevel': $scope.report.IsBudgetLevel,
                    'isActivityLevel': $scope.report.IsActivityLevel,
                    'IsDetailLevel': $scope.report.IsDetailLevel

                },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.TrialBalanceList = response.data;
                console.log('Trial Balance List', $scope.TrialBalanceList);
                // createLoanTakenBarChart(response.data);
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    //$scope.GetTrialBalanceData();

    $scope.summaryRowsTrialBalance = [{
        title: "Total DR. & CR.", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DR", dataMember: "DR", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CR", dataMember: "CR", format: "{0:N2}" }],
        showCaptionSummary: true
    }];


    // $scope.LedgerActivityPoPUpList = [];
    //$scope.BankLedgerDetailLevelPoPUpList = [];

    $scope.getTrialBLDetailLevelBankMasterLedgerPopUpData = function (glId, budMId, actId, pId, ppId, bkmId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetLedgerActivityPoPUpListData?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&bankMasterId=' + bkmId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.BankLedgerDetailLevelPoPUpList = response.data;
            //$scope.partyName = $scope.LedgerActivityPoPUpList[0].Party
        });
        $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };

    //$scope.BankLedgerHeadingPoPUpList = [];
    $scope.getTrialBLBankMasterHeaderLedgerPopUpData = function (glId, budMId, actId, bkmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetBankMasterLedgerHeading?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&bankMasterId=' + bkmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.BankLedgerHeadingPoPUpList = response.data;
            $scope.bankName = $scope.BankLedgerHeadingPoPUpList[0].BankName
            $scope.accountNumber = $scope.BankLedgerHeadingPoPUpList[0].AccountNumber
            $scope.currencyCode = $scope.BankLedgerHeadingPoPUpList[0].CurrencyCode

            $scope.BankBranchName = $scope.BankLedgerHeadingPoPUpList[0].BankName
            $scope.AccountTitle = $scope.BankLedgerHeadingPoPUpList[0].AccountNumber
            $scope.gLGeneralInfoCode = $scope.BankLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.BankLedgerHeadingPoPUpList[0].GLGeneralInfoName
        });
        // $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };


    //$scope.CashLedgerDetailLevelPoPUpList = [];
    $scope.getTrialBLDetailLevelCashMasterLedgerPopUpData = function (glId, budMId, actId, pId, ppId, bkmId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/getLedgerActivityPoPUpListData?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&bankMasterId=' + bkmId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.CashLedgerDetailLevelPoPUpList = response.data;
        });
        $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    //$scope.CashLedgerHeadingPoPUpList = [];
    $scope.getTrialBLHeadingCashMasterLedgerPopUpData = function (glId, budMId, actId, pId, ppId, bkmId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCashMasterLedgerHeading?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&bankMasterId=' + bkmId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.CashLedgerHeadingPoPUpList = response.data;
            $scope.cashName = $scope.CashLedgerHeadingPoPUpList[0].CashName
            $scope.currencyCode = $scope.CashLedgerHeadingPoPUpList[0].CurrencyCode

            $scope.gLGeneralInfoCode = $scope.CashLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.CashLedgerHeadingPoPUpList[0].GLGeneralInfoName
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };


    $scope.getTrialBLDetailLevelGeneralLedgerPopUpData = function (glId, budMId, actId, pId, ppId, bkmId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/getLedgerActivityPoPUpListData?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&bankMasterId=' + bkmId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.LedgerActivityPoPUpList = response.data;

        });
        $rootScope.openPopupAngular('TrialBalanceDRPopUp');
        // angular.element(document.querySelector("#loanTakenSetOffPopUp")).modal("show");
    };

    //$scope.GeneralLedgerHeadingPoPUpList = [];
    $scope.getTrialBLHeadingGeneralLedgerPopUpData = function (glId, budMId, actId, pId, ppId, bkmId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetGeneralLedgerHeading?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&bankMasterId=' + bkmId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.GeneralLedgerHeadingPoPUpList = response.data;
            $scope.accountTypeName = $scope.GeneralLedgerHeadingPoPUpList[0].AccountTypeName
            $scope.accountGroupName = $scope.GeneralLedgerHeadingPoPUpList[0].AccountGroupName
            $scope.refNo = $scope.GeneralLedgerHeadingPoPUpList[0].RefNo
            $scope.gLGeneralInfoCode = $scope.GeneralLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.GeneralLedgerHeadingPoPUpList[0].GLGeneralInfoName
            $scope.budget = $scope.GeneralLedgerHeadingPoPUpList[0].Budget
            $scope.activity = $scope.GeneralLedgerHeadingPoPUpList[0].Activity
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    // $scope.PartyLedgerDetailLevelPoPUpList = [];
    $scope.getTrialBLDetailLevelPartyLedgerPopUpData = function (glId, budMId, actId, pId, ppId, bkmId, cmId, toDate) {
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/getLedgerActivityPoPUpListData?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&bankMasterId=' + bkmId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.PartyLedgerDetailLevelPoPUpList = response.data;
            //$scope.partyName = $scope.LedgerActivityPoPUpList[0].Party
        });
        $rootScope.openPopupAngular('TrialBLDetailLevelPartyLedgerPopUp');
    };

    // $scope.PartyLedgerHeadingPoPUpList = [];
    $scope.getTrialBLHeadingPartyLedgerPopUpData = function (glId, budMId, actId, pId, ppId, bkmId, cmId, toDate) {
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyLedgerHeading?gLInfoId=" + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&bankMasterId=' + bkmId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.PartyLedgerHeadingPoPUpList = response.data;
            $scope.partyCode = $scope.PartyLedgerHeadingPoPUpList[0].PartyCode
            $scope.partyName = $scope.PartyLedgerHeadingPoPUpList[0].PartyName
            $scope.partyPlantName = $scope.PartyLedgerHeadingPoPUpList[0].PartyName
            $scope.currencyCode = $scope.PartyLedgerHeadingPoPUpList[0].CurrencyCode
            $scope.partyAccountGroupName = $scope.PartyLedgerHeadingPoPUpList[0].PartyAccountGroupName
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    $scope.showTrialBalanceActivityPopUp = function (args) {
        $scope.toDate = $scope.reportParameters.ToDate

        if (args.BankMasterId != null) {
            $scope.getTrialBLDetailLevelBankMasterLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)
            $scope.getTrialBLBankMasterHeaderLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)
        }
        else if (args.CashMasterId != null) {
            $scope.getTrialBLDetailLevelCashMasterLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)
            $scope.getTrialBLHeadingCashMasterLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)

        }
        else if (args.PartyId != null) {
            $scope.getTrialBLDetailLevelPartyLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)
            $scope.getTrialBLHeadingPartyLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)
        }
        else {
            $scope.getTrialBLDetailLevelGeneralLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)
            $scope.getTrialBLHeadingGeneralLedgerPopUpData(args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, args.BankMasterId, args.CashMasterId, $scope.toDate)

        }
    };

    $scope.closeTrialBalanceDRPopUp = function () {
        angular.element(document.querySelector("#TrialBalanceDRPopUp")).modal("hide");
    };
    $scope.closeTrialBLBankMasterLedgerPopUp = function () {
        angular.element(document.querySelector("#TrialBLBankMasterLedgerPopUp")).modal("hide");
    };
    $scope.closeTrialBLCashMasterLedgerPopUp = function () {
        angular.element(document.querySelector("#TrialBLCashMasterLedgerPopUp")).modal("hide");
    };
    $scope.closeTrialBLDetailLevelPartyLedgerPopUp = function () {
        angular.element(document.querySelector("#TrialBLDetailLevelPartyLedgerPopUp")).modal("hide");
    };

    $scope.summaryRowsLedger = [{
        title: "Total DR. & CR.", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DrAmount", dataMember: "DrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CrAmount", dataMember: "CrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyDrAmount", dataMember: "CompanyCurrencyDrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyCrAmount", dataMember: "CompanyCurrencyCrAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];
    $scope.summaryRowsBankMasterLedger = [{
        title: "Total DR. & CR.", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DrAmount", dataMember: "DrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CrAmount", dataMember: "CrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyDrAmount", dataMember: "CompanyCurrencyDrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyCrAmount", dataMember: "CompanyCurrencyCrAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];
    $scope.summaryRowsCashMasterLedger = [{
        title: "Total DR. & CR.", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DrAmount", dataMember: "DrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CrAmount", dataMember: "CrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyDrAmount", dataMember: "CompanyCurrencyDrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyCrAmount", dataMember: "CompanyCurrencyCrAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];
    $scope.summaryRowsPartyLedger = [{
        title: "Total DR. & CR.", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DrAmount", dataMember: "DrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CrAmount", dataMember: "CrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyDrAmount", dataMember: "CompanyCurrencyDrAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CompanyCurrencyCrAmount", dataMember: "CompanyCurrencyCrAmount", format: "{0:N2}" }],
        showCaptionSummary: true,
    }];


    $scope.getvouchardetailjs = function (obj) {
        var reportformat = "pdf";
        var file_src = "";
        file_src = 'Accounts/VoucherReport/GetCommonVoucherReport?reportFormat=' + 'Pdf' + '&compnayGroupId=' + obj.data.CompanyGroupId + '&companyId=' + obj.data.CompanyId + '&plantId=' + obj.data.PlantId + '&sourceType=' + obj.data.SourceType + '&voucherId=' + obj.data.VoucherId + '&inventoryIssueId=' + obj.data.InventoryIssueId + '&inventoryReceiveId=' + obj.data.InventoryReceiveId + '&salesSourceType=' + obj.data.SalesSourceType + '&invoiceWriteOffGroupNo=' + obj.data.InvoiceWriteOffGroupNo + '&openingBalanceId=' + obj.data.OpeningBalanceId;
        $window.open(file_src, '_blank');
    };



    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    }

    $scope.GetTrialBLAccountGroupReport = function () {
        try {
            var filtered = $("#GridSelectedTrialBalance").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                filtered = $scope.TrialBalanceList;
            }
            //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
            var AccountGroupNames = getString(filtered, "AccountGroupName");

            //var AccountGroupNames = getString(filtered, "GL");
            //var AccountGroupNames = getString(filtered, "AccountGroupName");

            $scope.fileName = $scope.report.AssetsLiability + ".xls";

            $http({
                method: 'POST',
                // url: 'Attendances/DailyAttendanceReport/DailyAttendanceStatusReport',
                url: 'Accounts/AccountStatusDashboard/AccountGroupWiseReport',
                data: {
                    'allAccountGroupList': AccountGroupNames
                    //"voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                    , 'toDate': $scope.report.ToDate
                    , 'reportName': $scope.report.AssetsLiability
                    , 'isDetailLevel': $scope.report.IsDetailLevel
                    //,'isUpToLevel': $scope.report.IsUpToLevel
                    , 'isBudgetLevel': $scope.report.IsBudgetLevel
                    , 'isActivityLevel': $scope.report.IsActivityLevel
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"

            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };






    $scope.BankLedgerDetailLevelPoPUpList = [];
    $scope.getTrialBLAllLevelBankMasterLedgerPopUpData = function (particulars, glId, budMId, actId, bkmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetBankLedgerAllLevelPoPUpListData?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&bankMasterId=' + bkmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.BankLedgerDetailLevelPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.BankLedgerClosingBalance = Math.round(($scope.TotalDRAmount - $scope.TotalCRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'DR'
        });
        $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };

    $scope.BankLedgerHeadingPoPUpList = [];
    $scope.getTrialBLAllLevelBankMasterHeaderLedgerPopUpData = function (particulars, glId, budMId, actId, bkmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetAllLevelBankMasterLedgerHeading?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&bankMasterId=' + bkmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.BankLedgerHeadingPoPUpList = response.data;
            $scope.bankName = $scope.BankLedgerHeadingPoPUpList[0].BankName
            $scope.accountNumber = $scope.BankLedgerHeadingPoPUpList[0].AccountNumber
            $scope.currencyCode = $scope.BankLedgerHeadingPoPUpList[0].CurrencyCode

            $scope.BankBranchName = $scope.BankLedgerHeadingPoPUpList[0].BankName
            $scope.AccountTitle = $scope.BankLedgerHeadingPoPUpList[0].AccountNumber
            $scope.gLGeneralInfoCode = $scope.BankLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.BankLedgerHeadingPoPUpList[0].GLGeneralInfoName
        });
        // $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };

    $scope.CashLedgerHeadingPoPUpList = [];
    $scope.getTrialBLHeadingAllLevelCashMasterLedgerPopUpData = function (particulars, glId, budMId, actId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCashMasterLedgerHeading?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.CashLedgerHeadingPoPUpList = response.data;
            $scope.cashName = $scope.CashLedgerHeadingPoPUpList[0].CashName
            $scope.currencyCode = $scope.CashLedgerHeadingPoPUpList[0].CurrencyCode

            $scope.gLGeneralInfoCode = $scope.CashLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.CashLedgerHeadingPoPUpList[0].GLGeneralInfoName
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };
    $scope.CashLedgerDetailLevelPoPUpList = [];
    $scope.getTrialBLAllLevelCashMasterLedgerPopUpData = function (particulars, glId, budMId, actId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCashLedgerAllLevelPoPUpListData?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.CashLedgerDetailLevelPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.CashLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.CashLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.CashLedgerClosingBalance = Math.round(($scope.TotalDRAmount - $scope.TotalCRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'DR'
        });
        $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    $scope.PartyLedgerHeadingPoPUpList = [];
    $scope.getTrialBLAllLevelHeadingPartyLedgerPopUpData = function (particulars, glId, budMId, actId, pId, ppId, toDate) {
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyLedgerHeading?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.PartyLedgerHeadingPoPUpList = response.data;
            $scope.partyCode = $scope.PartyLedgerHeadingPoPUpList[0].PartyCode
            $scope.partyName = $scope.PartyLedgerHeadingPoPUpList[0].PartyName
            $scope.partyPlantName = $scope.PartyLedgerHeadingPoPUpList[0].PartyName
            $scope.currencyCode = $scope.PartyLedgerHeadingPoPUpList[0].CurrencyCode
            $scope.partyAccountGroupName = $scope.PartyLedgerHeadingPoPUpList[0].PartyAccountGroupName
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };
    $scope.PartyLedgerDetailLevelPoPUpList = [];
    $scope.getTrialBLAllLevelPartyLedgerPopUpData = function (particulars, glId, budMId, actId, pId, ppId, toDate) {
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyLedgerAllLevelPoPUpListData?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.PartyLedgerDetailLevelPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.PartyLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.PartyLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.PartyLedgerClosingBalance = Math.round(($scope.TotalDRAmount - $scope.TotalCRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'DR'
        });
        $rootScope.openPopupAngular('TrialBLDetailLevelPartyLedgerPopUp');
    };

    $scope.GeneralLedgerHeadingPoPUpList = [];
    $scope.getTrialBLHeadingAllLevelGeneralLedgerPopUpData = function (particulars, glId, budMId, actId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetGeneralLedgerAllLevelDRHeading?particulars=" + particulars + '&gLInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.GeneralLedgerHeadingPoPUpList = response.data;
            $scope.accountTypeName = $scope.GeneralLedgerHeadingPoPUpList[0].AccountTypeName
            $scope.accountGroupName = $scope.GeneralLedgerHeadingPoPUpList[0].AccountGroupName
            $scope.refNo = $scope.GeneralLedgerHeadingPoPUpList[0].RefNo
            $scope.gLGeneralInfoCode = $scope.GeneralLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.GeneralLedgerHeadingPoPUpList[0].GLGeneralInfoName
            $scope.budget = $scope.GeneralLedgerHeadingPoPUpList[0].Budget
            $scope.activity = $scope.GeneralLedgerHeadingPoPUpList[0].Activity
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    $scope.LedgerActivityPoPUpList = [];
    $scope.getTrialBLAllLevelGeneralLedgerPopUpData = function (particulars, glId, budMId, actId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/getLedgerAllLevelDRPoPUpListData?particulars=" + particulars + '&gLInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.LedgerActivityPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.LedgerActivityPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.LedgerActivityPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.GeneralLedgerClosingBalance = Math.round(($scope.TotalDRAmount - $scope.TotalCRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'DR'
        });
        $rootScope.openPopupAngular('TrialBalanceDRPopUp');
    };

    $scope.showTrialBalanceDRcumulativePopUp = function (args) {
        $scope.toDate = $scope.reportParameters.ToDate

        if (args.BankMasterId != null) {

            $scope.getTrialBLAllLevelBankMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.BankMasterId, $scope.toDate)
            $scope.getTrialBLAllLevelBankMasterHeaderLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.BankMasterId, $scope.toDate)
        }
        else if (args.CashMasterId != null) {

            $scope.getTrialBLAllLevelCashMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.CashMasterId, $scope.toDate)
            $scope.getTrialBLHeadingAllLevelCashMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.CashMasterId, $scope.toDate)

        }
        else if (args.PartyId != null) {

            $scope.getTrialBLAllLevelPartyLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, $scope.toDate)
            $scope.getTrialBLAllLevelHeadingPartyLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, $scope.toDate)
        }
        else {
            if (baseService.isUndefinedOrNull(args.BudgetMasterId)) {
                args.BudgetMasterId = null;
            }
            if (baseService.isUndefinedOrNull(args.ActivityId)) {
                args.ActivityId = null;
            }
            if (baseService.isUndefinedOrNull(args.Particulars)) {
                args.Particulars = null;
            }
            $scope.getTrialBLAllLevelGeneralLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, $scope.toDate)
            $scope.getTrialBLHeadingAllLevelGeneralLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, $scope.toDate)

        }
    };

    //..................CR Amount popUp------------------------


    $scope.getTrialBLAllLevelCRBankMasterLedgerPopUpData = function (particulars, glId, budMId, actId, bkmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetBankLedgerAllLevelPoPUpListData?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&bankMasterId=' + bkmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.BankLedgerDetailLevelPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.BankLedgerClosingBalance = Math.round(($scope.TotalCRAmount - $scope.TotalDRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'CR'
        });
        $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };


    $scope.getTrialBLAllLevelCRBankMasterHeaderLedgerPopUpData = function (particulars, glId, budMId, actId, bkmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetAllLevelBankMasterLedgerHeading?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&bankMasterId=' + bkmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.BankLedgerHeadingPoPUpList = response.data;
            $scope.bankName = $scope.BankLedgerHeadingPoPUpList[0].BankName
            $scope.accountNumber = $scope.BankLedgerHeadingPoPUpList[0].AccountNumber
            $scope.currencyCode = $scope.BankLedgerHeadingPoPUpList[0].CurrencyCode

            $scope.BankBranchName = $scope.BankLedgerHeadingPoPUpList[0].BankName
            $scope.AccountTitle = $scope.BankLedgerHeadingPoPUpList[0].AccountNumber
            $scope.gLGeneralInfoCode = $scope.BankLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.BankLedgerHeadingPoPUpList[0].GLGeneralInfoName
        });
        // $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };


    $scope.getTrialBLHeadingAllLevelCRCashMasterLedgerPopUpData = function (particulars, glId, budMId, actId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCashMasterLedgerHeading?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.CashLedgerHeadingPoPUpList = response.data;
            $scope.cashName = $scope.CashLedgerHeadingPoPUpList[0].CashName
            $scope.currencyCode = $scope.CashLedgerHeadingPoPUpList[0].CurrencyCode

            $scope.gLGeneralInfoCode = $scope.CashLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.CashLedgerHeadingPoPUpList[0].GLGeneralInfoName
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    $scope.getTrialBLAllLevelCRCashMasterLedgerPopUpData = function (particulars, glId, budMId, actId, cmId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetCashLedgerAllLevelPoPUpListData?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&cashMasterId=' + cmId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.CashLedgerDetailLevelPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.CashLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.CashLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.CashLedgerClosingBalance = Math.round(($scope.TotalCRAmount - $scope.TotalDRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'CR'
        });
        $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    $scope.PartyLedgerHeadingPoPUpList = [];
    $scope.getTrialBLAllLevelCRHeadingPartyLedgerPopUpData = function (particulars, glId, budMId, actId, pId, ppId, toDate) {
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyLedgerHeading?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.PartyLedgerHeadingPoPUpList = response.data;
            $scope.partyCode = $scope.PartyLedgerHeadingPoPUpList[0].PartyCode
            $scope.partyName = $scope.PartyLedgerHeadingPoPUpList[0].PartyName
            $scope.partyPlantName = $scope.PartyLedgerHeadingPoPUpList[0].PartyName
            $scope.currencyCode = $scope.PartyLedgerHeadingPoPUpList[0].CurrencyCode
            $scope.partyAccountGroupName = $scope.PartyLedgerHeadingPoPUpList[0].PartyAccountGroupName
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };
    $scope.PartyLedgerDetailLevelPoPUpList = [];
    $scope.getTrialBLAllLevelCRPartyLedgerPopUpData = function (particulars, glId, budMId, actId, pId, ppId, toDate) {
        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetPartyLedgerAllLevelPoPUpListData?particulars=" + particulars + '&glInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&partyId=' + pId + '&partyPlantId=' + ppId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.PartyLedgerDetailLevelPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.PartyLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.PartyLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.PartyLedgerClosingBalance = Math.round(($scope.TotalCRAmount - $scope.TotalDRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'CR'
        });
        $rootScope.openPopupAngular('TrialBLDetailLevelPartyLedgerPopUp');
    };

    $scope.GeneralLedgerHeadingPoPUpList = [];
    $scope.getTrialBLHeadingAllLevelCRGeneralLedgerPopUpData = function (particulars, glId, budMId, actId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/GetGeneralLedgerAllLevelDRHeading?particulars=" + particulars + '&gLInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.GeneralLedgerHeadingPoPUpList = response.data;
            $scope.accountTypeName = $scope.GeneralLedgerHeadingPoPUpList[0].AccountTypeName
            $scope.accountGroupName = $scope.GeneralLedgerHeadingPoPUpList[0].AccountGroupName
            $scope.refNo = $scope.GeneralLedgerHeadingPoPUpList[0].RefNo
            $scope.gLGeneralInfoCode = $scope.GeneralLedgerHeadingPoPUpList[0].GLGeneralInfoCode
            $scope.gLGeneralInfoName = $scope.GeneralLedgerHeadingPoPUpList[0].GLGeneralInfoName
            $scope.budget = $scope.GeneralLedgerHeadingPoPUpList[0].Budget
            $scope.activity = $scope.GeneralLedgerHeadingPoPUpList[0].Activity
        });
        // $rootScope.openPopupAngular('TrialBLCashMasterLedgerPopUp');
    };

    $scope.LedgerActivityPoPUpList = [];
    $scope.getTrialBLAllLevelCRGeneralLedgerPopUpData = function (particulars, glId, budMId, actId, toDate) {

        $http({
            method: "GET",
            url: "Accounts/AccountStatusDashboard/getLedgerAllLevelDRPoPUpListData?particulars=" + particulars + '&gLInfoId=' + glId + '&budgetMasterId=' + budMId + '&activityId=' + actId + '&toDate=' + toDate
        }).then(function successCallback(response) {
            $scope.LedgerActivityPoPUpList = response.data;
            $scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.LedgerActivityPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            $scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.LedgerActivityPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            $scope.GeneralLedgerClosingBalance = Math.round(($scope.TotalCRAmount - $scope.TotalDRAmount) * 100 + Number.EPSILON) / 100;
            $scope.DRBalanceType = 'CR'

        });
        $rootScope.openPopupAngular('TrialBalanceDRPopUp');
    };

    $scope.showTrialBalanceCRcumulativePopUp = function (args) {
        $scope.toDate = $scope.reportParameters.ToDate

        if (args.BankMasterId != null) {
            $scope.getTrialBLAllLevelCRBankMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.BankMasterId, $scope.toDate)
            $scope.getTrialBLAllLevelCRBankMasterHeaderLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.BankMasterId, $scope.toDate)
        }
        else if (args.CashMasterId != null) {

            $scope.getTrialBLAllLevelCRCashMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.CashMasterId, $scope.toDate)
            $scope.getTrialBLHeadingAllLevelCRCashMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.CashMasterId, $scope.toDate)

        }
        else if (args.PartyId != null) {

            $scope.getTrialBLAllLevelCRPartyLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, $scope.toDate)
            $scope.getTrialBLAllLevelCRHeadingPartyLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, $scope.toDate)
        }
        else {
            if (baseService.isUndefinedOrNull(args.BudgetMasterId)) {
                args.BudgetMasterId = null;
            }
            if (baseService.isUndefinedOrNull(args.ActivityId)) {
                args.ActivityId = null;
            }
            if (baseService.isUndefinedOrNull(args.Particulars)) {
                args.Particulars = null;
            }
            $scope.getTrialBLAllLevelCRGeneralLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, $scope.toDate)
            $scope.getTrialBLHeadingAllLevelCRGeneralLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, $scope.toDate)

        }
    };




    //-------------------#endregion  Trial Balance ----------------------------------------

    //...............#region CashInFlow Tab ..........................
    $scope.CashInFlowReceivableMasterList = [];
    $scope.GetCashInFlowReceivableMasterList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetCashInFlowReceivableMasterList",
                data: { /*FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate */ },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                for (var i = 0; i < response.data.DATA.length; i++) {
                    try {
                        if (angular.isUndefinedOrNull(response.data.DATA[i].PostingDate) == false)
                            // angular.isUndefinedOrNull(response.data.DATA[i].PostingDate)
                            response.data.DATA[i].PostingDate = new Date(response.data.DATA[i].PostingDate);

                        if (angular.isUndefinedOrNull(response.data.DATA[i].MatureDate) == false)
                            response.data.DATA[i].MatureDate = new Date(response.data.DATA[i].MatureDate);


                    } catch (e) {

                    }

                }
                $scope.CashInFlowReceivableMasterList = response.data.DATA;
                $scope.cashInFlowMaterialMOSBooksBalance = $filter("sumByKey")($filter("filter")($scope.CashInFlowReceivableMasterList, { FilteringSourceType: "FMasterOrderSales" }), "BooksBalance");
                $scope.cashInFlowMSBooksBalance = $filter("sumByKey")($filter("filter")($scope.CashInFlowReceivableMasterList, { FilteringSourceType: "MaterialSales" }), "BooksBalance");
                $scope.cashInFlowNSMOSBooksBalance = $filter("sumByKey")($filter("filter")($scope.CashInFlowReceivableMasterList, { FilteringSourceType: "NonShiftedMasterOrderSales" }), "BooksBalance");

                // $scope.partyCode = $scope.PartyLedgerHeadingPoPUpList[0].PartyCode

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    //$scope.GetCashInFlowReceivableMasterList();

    //$("#GridMasterCashInFlow").ejGrid("clearFiltering");

    $scope.summaryRowsCashInFlow = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksReceivableAmount", dataMember: "BooksReceivableAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksReceived", dataMember: "BooksReceived", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksBalance", dataMember: "BooksBalance", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];


    //...............#endregoin CashInFlow Tab .......................

    //...............#region Cash Out Flow

    //get data for master gride for Cash Out Flow 
    $scope.MasterCashOutFlowList = [];
    $scope.GetCashOutFlowMasterList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetCashOutFlowMasterList",
                data: { /*FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate*/ },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.DATA.length; i++) {
                    try {
                        if (angular.isUndefinedOrNull(response.data.DATA[i].PostingDate) == false)
                            // angular.isUndefinedOrNull(response.data.DATA[i].PostingDate)
                            response.data.DATA[i].PostingDate = new Date(response.data.DATA[i].PostingDate);

                        if (angular.isUndefinedOrNull(response.data.DATA[i].MatureDate) == false)
                            response.data.DATA[i].MatureDate = new Date(response.data.DATA[i].MatureDate);


                    } catch (e) {

                    }

                }
                $scope.MasterCashOutFlowList = response.data.DATA;

                $scope.cashOutflowInvPayBooksBalance = $filter("sumByKey")($filter("filter")($scope.MasterCashOutFlowList, { SourceType: "InventoryPayable" }), "BooksBalance");
                $scope.cashOutflowVenInvBooksBalance = $filter("sumByKey")($filter("filter")($scope.MasterCashOutFlowList, { SourceType: "VendorInvoice" }), "BooksBalance");
                //$scope.cashInFlowNSMOSBooksBalance = $filter("sumByKey")($filter("filter")($scope.MasterCashOutFlowList, { SourceType: "NonShiftedMasterOrderSales" }), "BooksBalance");


            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    //$scope.GetCashOutFlowMasterList();

    $scope.summaryRowsCashOutFlow = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksGross", dataMember: "BooksGross", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksSetOff", dataMember: "BooksSetOff", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksBalance", dataMember: "BooksBalance", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];

    //............#endregion Cash Out Flow..............................

    //..........#Material Management.................


    $scope.products = [];
    $scope.acceptancePostedList = [];
    $scope.getDataList = function () {
        if ($scope.material.GRNandAccPType == 'GRNPosted') {
            $scope.acceptancePostedList = [];
            $scope.products = [];
            $http({
                method: 'POST',
                url: 'Accounts/AccountStatusDashboard/GetGRNPostingList',
                data: { grnAndAccpType: $scope.material.GRNandAccPType, dateType: $scope.material.DateType, fromDate: $scope.material.FromDate, toDate: $scope.material.ToDate, isOrderSpecific: $scope.material.IsOrderSpecific, isNonOrderSpecific: $scope.material.IsNonOrderSpecific },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                $scope.products = response.data.DATA;
            });

        }
        else {
            $scope.acceptancePostedList = [];
            $scope.products = [];
            $http({
                method: 'POST',
                url: 'Accounts/AccountStatusDashboard/GetAcceptancePostingList',
                data: { grnAndAccpType: $scope.material.GRNandAccPType, dateType: $scope.material.DateType, fromDate: $scope.material.FromDate, toDate: $scope.material.ToDate, isOrderSpecific: $scope.material.IsOrderSpecific, isNonOrderSpecific: $scope.material.IsNonOrderSpecific },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                $scope.acceptancePostedList = response.data.DATA;
            });
        }
    }

    $scope.GRNPostedReport = function () {
        try {
            //var file_src = 'Accounts/Invoice/GetAutoMailReport';
            var file_src = $scope.path + 'GRNPostedReport?grnAndAccpType=' + $scope.material.GRNandAccPType + '&dateType=' + $scope.material.DateType + '&fromDate=' + $scope.material.FromDate + '&toDate=' + $scope.material.ToDate + '&isOrderSpecific=' + $scope.material.IsOrderSpecific + '&isNonOrderSpecific=' + $scope.material.IsNonOrderSpecific;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.AcceptancePostedReport = function () {
        try {
            var file_src = $scope.path + 'AcceptancePostedReport?grnAndAccpType=' + $scope.material.GRNandAccPType + '&dateType=' + $scope.material.DateType + '&fromDate=' + $scope.material.FromDate + '&toDate=' + $scope.material.ToDate + '&isOrderSpecific=' + $scope.material.IsOrderSpecific + '&isNonOrderSpecific=' + $scope.material.IsNonOrderSpecific;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    //.........#endregion Material Management...........................

    //...............#region Acceptance Liability Maturity...........................................

    $scope.AcceptanceLiabilityMaturityList = [];
    $scope.GetAcceptanceLiabilityMaturityData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetAcceptanceLiabilityMaturityList",
                data: { /*FromDate: $scope.reportParameters.FromDate,*/ ToDate: $scope.report.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.DATA.length; i++) {
                    try {
                        if (angular.isUndefinedOrNull(response.data.DATA[i].PostingDate) == false)
                            response.data.DATA[i].PostingDate = new Date(response.data.DATA[i].PostingDate);

                        if (angular.isUndefinedOrNull(response.data.DATA[i].ActualDueDate) == false)
                            response.data.DATA[i].ActualDueDate = new Date(response.data.DATA[i].ActualDueDate);


                        if (angular.isUndefinedOrNull(response.data.DATA[i].DueDateBaseON) == false)
                            response.data.DATA[i].DueDateBaseON = new Date(response.data.DATA[i].DueDateBaseON);

                    } catch (e) {

                    }
                }
                $scope.AcceptanceLiabilityMaturityList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {

        }
    }
    //$scope.GetAcceptanceLiabilityMaturityData();

    $scope.TotalAcceptanceLiabilityMaturity = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AcceptanceAmount", dataMember: "AcceptanceAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SetOff", dataMember: "SetOff", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Balance", dataMember: "Balance", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LCAmount", dataMember: "LCAmount", format: "{0:N2}" }

        ],
        showCaptionSummary: true
    }];


    $scope.TotalGRNWithoutInvoice = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalMaterialBooksCurrencyAmount", dataMember: "TotalMaterialBooksCurrencyAmount", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];

    $scope.TotalInvoiceWithoutGRN = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Gross", dataMember: "Gross", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DebitNoteAmount", dataMember: "DebitNoteAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TaxAmount", dataMember: "TaxAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SetOff", dataMember: "SetOff", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Balance", dataMember: "Balance", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksGross", dataMember: "BooksGross", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DebitNoteBooksAmount", dataMember: "DebitNoteBooksAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksTaxAmount", dataMember: "BooksTaxAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksSetOff", dataMember: "BooksSetOff", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksBalance", dataMember: "BooksBalance", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksWriteOffAmount", dataMember: "BooksWriteOffAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksInvoiceBalance", dataMember: "BooksInvoiceBalance", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];


    $scope.GetAcceptanceLiabilityMaturityReport = function () {
        try {
            //var file_src = $scope.path + 'MaterialMasterReport2?MaterialTypeId=' + $scope.materialMasterReportNew.MaterialTypeId + '&Article=' + $scope.materialMasterReportNew.WithArticle;;
            var file_src = $scope.path + 'GetAcceptanceLiabilityMaturityReport?toDate=' + $scope.report.ToDate;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    //............#endregion Acceptance Liability Maturity..............................

    //---------------#region Acceptance Liability-------------------------------

    $scope.AcceptanceLiabilityList = [];
    $scope.GetAcceptanceLiabilityData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetAcceptanceLiabilityList",
                data: { /*FromDate: $scope.reportParameters.FromDate,*/ ToDate: $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.DATA.length; i++) {
                    try {
                        if (angular.isUndefinedOrNull(response.data.DATA[i].PostingDate) == false)
                            response.data.DATA[i].PostingDate = new Date(response.data.DATA[i].PostingDate);

                        if (angular.isUndefinedOrNull(response.data.DATA[i].ActualDueDate) == false)
                            response.data.DATA[i].ActualDueDate = new Date(response.data.DATA[i].ActualDueDate);


                        if (angular.isUndefinedOrNull(response.data.DATA[i].DueDateBaseON) == false)
                            response.data.DATA[i].DueDateBaseON = new Date(response.data.DATA[i].DueDateBaseON);

                    } catch (e) {

                    }
                }
                $scope.AcceptanceLiabilityList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    //$scope.GetAcceptanceLiabilityData();

    $scope.TotalAcceptanceLiability = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AcceptanceAmount", dataMember: "AcceptanceAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SetOff", dataMember: "SetOff", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Balance", dataMember: "Balance", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksAcceptanceAmount", dataMember: "BooksAcceptanceAmount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksSetOff", dataMember: "BooksSetOff", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksBalance", dataMember: "BooksBalance", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" },
        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LCAmount", dataMember: "LCAmount", format: "{0:N2}" }

        ],
        showCaptionSummary: true
    }];

    $scope.GetAcceptanceLiabilitySummaryReport = function () {

        try {
            var file_src = $scope.path + 'GetAcceptanceLiabilitySummaryReport?toDate=' + $scope.reportParameters.ToDate /*+ '&isWithAdvance=' + $scope.reportParameters.IsWithAdvance*/;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetAcceptanceLiabilityReport = function () {
        try {
            //var file_src = $scope.path + 'MaterialMasterReport2?MaterialTypeId=' + $scope.materialMasterReportNew.MaterialTypeId + '&Article=' + $scope.materialMasterReportNew.WithArticle;;
            var file_src = $scope.path + 'GetAcceptanceLiabilityReport?toDate=' + $scope.reportParameters.ToDate;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    //------------------#endregion Acceptance liability-------------------------

    //------------------#region Others Liability ---------------
    $scope.OthersLiabilityList = [];
    $scope.GetOthersLiabilityData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetOthersLiabilityDataList",
                data: { ToDate: $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.OthersLiabilityList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

        }

        catch (e) {

        }
    }
    // $scope.GetOthersLiabilityData();

    $scope.TotalOthersLiabilityAmount = [{
        title: "Total", summaryColumns:
            [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Gross", dataMember: "Gross", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DebitNoteAmount", dataMember: "DebitNoteAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TaxAmount", dataMember: "TaxAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SetOff", dataMember: "SetOff", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Balance", dataMember: "Balance", format: "{0:N2}" },

            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksGross", dataMember: "BooksGross", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DebitNoteBooksAmount", dataMember: "DebitNoteBooksAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksTaxAmount", dataMember: "BooksTaxAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksSetOff", dataMember: "BooksSetOff", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksBalance", dataMember: "BooksBalance", format: "{0:N2}" }


            ],
        showCaptionSummary: true
    }];

    $scope.TotalBankAmount = [{
        title: "Total", summaryColumns: [
            {
                summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksBankBalance", dataMember: "BooksBankBalance", format: "{0:N2}"
            }],
        showCaptionSummary: true
    }];

    $scope.TotalCashAmount = [{
        title: "Total", summaryColumns: [
            {
                summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksCashBalance", dataMember: "BooksCashBalance", format: "{0:N2}"
            }],
        showCaptionSummary: true
    }];

    $scope.OthersLiabilitySummaryReport = function () {

        try {
            var file_src = $scope.path + 'OthersLiabilitySummaryReport?toDate=' + $scope.reportParameters.ToDate + '&isWithAdvance=' + $scope.reportParameters.IsWithAdvance;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.OthersLiabilityAgingDetailReport = function () {

        try {

            var file_src = $scope.path + "OthersLiabilityAgingDetailReport?toDate=" + $scope.reportParameters.ToDate /*+ '&isWithAdvance=' + $scope.reportParameters.IsWithAdvance*/;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //---------------#endregion others liability-------------------

    //**********************#GRN With out Invoice**************************
    $scope.GRNWithOutInvoiceList = [];
    $scope.GetGRNWithOutInvoiceData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetGRNWithOutInvoiceDataList",
                data: { /*FromDate: $scope.reportParameters.FromDate,*/ ToDate: $scope.report.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                $scope.GRNWithOutInvoiceList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }




    //********************#endregion GRN With Out Invoice***************************************

    //**********************#Invoice With out GRN  **************************
    $scope.InvoiceWithOutGRNList = [];
    $scope.GetInvoiceWithOutGRNData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetInvoiceWithOutGRNDataList",
                data: { /*FromDate: $scope.reportParameters.FromDate,*/ ToDate: $scope.report.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.InvoiceWithOutGRNList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }

    $scope.AssetWIPstatusList = [];
    $scope.GetAssetWIPstatusList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Accounts/VoucherReport/GetAssetWIPData',

        }).then(function successCallback(response) {
            $scope.AssetWIPstatusList = response.data.DATA
        });
    }
    //$scope.GetAssetWIPstatusList();


    $scope.TotalAssetWIPstatus = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionQty", dataMember: "TransactionQty", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TrnAmount", dataMember: "TrnAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BaseQty", dataMember: "BaseQty", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksAmount", dataMember: "BooksAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "IssueQty", dataMember: "IssueQty", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ADBaseAmount", dataMember: "ADBaseAmount", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetFixedAssetsAmount", dataMember: "NetFixedAssetsAmount", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FACount", dataMember: "FACount" }

        ],
        showCaptionSummary: true
    }];


    $scope.onGRNNoDownloadExcel = function (data) {
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.GRNNo;
    };

    $scope.onVoucherNoDownloadExcel = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.VoucherNo)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/InventoryPayable/PabyableJournal?' + '&reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.GRNNo + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false);
    };
    $scope.onInvoiceVoucherPrint = function (data) {
        var reportFormat = "Pdf";
        $window.open('Products/PurchaseDocumentsAcceptance/DocumentAcceptanceVoucher?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
    };
    $scope.onInvoiceVoucherPrintSourceTypeWise = function (data) {
        var reportFormat = "Pdf";
        if (data.SourceType == "VendorInvoice") {
            $window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
        }
        if (data.SourceType == "EmployeePayable") {
            $window.open('Employees/EmployeeReport/GetEmployeePayableExpenseReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
        }
        if (data.SourceType == "InventoryPayable") {
            $window.open('Accounts/InventoryPayable/PabyableJournal?' + '&reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.GRNNo + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false);
        }
        if (data.SourceType == "VendorPayment") {
            $window.open('Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
        }

    };

    $scope.invoiceSetOffDetailList = [];
    $scope.getInvoiceSetOffDetailByInvoice = function (Data) {
        $scope.invoiceSetOffDetailList = [];
        $http({
            method: "get",
            url: "accounts/invoice/GetInvoiceSetOffDetailByInvoiceId?invoiceId=" + Data.InvoiceId
        }).then(function successCallback(response) {
            $scope.invoiceSetOffDetailList = response.data;
            $rootScope.openPopupAngular('invoicSetOffByInvoiceOtherLiabilityPopUp');
        });
    };


    $scope.issueQtyList = [];
    $scope.onIssueQtyPopUp = function (Data) {
        $scope.SelectedLCRow = Data;

        $http({
            method: 'POST',
            url: 'Accounts/VoucherReport/GetIssueQtyList',
            data: { 'inventoryReceiveDetailId': Data.InventoryReceiveDetailId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {

                    $scope.issueQtyList = response.data.Data;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('IssueQtyPopup');
    }


    $scope.getAssetWIPstatusReportExcel = function () {
        var filtered = $("#GridAssetWIPstatus").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            filtered = $scope.AssetWIPstatusList;
        }
        $scope.fileName = 'AssetWIPStatus.xls';
        //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
        var materialMasterId = getString(filtered, "MaterialMasterId");
        var materialMasterArticleId = getString(filtered, "ArticleId");
        var voucherId = getString(filtered, "VoucherId");
        var grnNo = getString(filtered, "GRNNo");
        var glId = getString(filtered, "GlId");
        var activityId = getString(filtered, "ActivityId");
        try {

            $http({
                method: 'POST',
                url: 'Accounts/VoucherReport/AssetWIPstatusReportExcel',
                data: {

                    'MaterialMasterId': materialMasterId,
                    'materialMasterArticleId': materialMasterArticleId,
                    'VoucherId': voucherId,
                    'GRNNo': grnNo,
                    'GlId': glId,
                    'ActivityId': activityId
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            // ShowResult(e, 'failure');
            ShowResult(commonMessage.NetworkError, 'failure');
        }

    }


    $scope.PostedAUCData = function (issueno) {
        $window.open('Products/InventoryIssue/AssetIssueReport?grnId=' + issueno);
    }

    $scope.PostedcommandPDF = function (voucherNo) {
        var reportFormat = "Pdf";
        $window.open('FixedAssets/FixedAssetRegister/GetIssueFixedAssetCapitalizeJournalReport?reportFormat=' + reportFormat + '&voucherId=' + voucherNo, '_blank');
    }

    //**********************#endregion Asset WIP Status **************************

    $scope.InvoiceWithOutGRNList = [];



    $scope.NonRegisterAssetList = [];
    $scope.GetNonRegisterAssetData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: $scope.path + 'GetNonRegisterAssetData',

        }).then(function successCallback(response) {
            $scope.NonRegisterAssetList = response.data.DATA
        });
    }
    $scope.GetNonRegisterAssetData();


    $scope.TotalNonRegisterAsset = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionQty", dataMember: "TransactionQty", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TrnAmount", dataMember: "TrnAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BaseQty", dataMember: "BaseQty", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksAmount", dataMember: "BooksAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "IssueQty", dataMember: "IssueQty", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "IssueAmount", dataMember: "IssueAmount", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetFixedAssetsAmount", dataMember: "NetFixedAssetsAmount", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FACount", dataMember: "FACount" }

        ],
        showCaptionSummary: true
    }];

    $scope.onIssueNoDownloadExcel = function (data) {
        //location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.IssueNo;
        location.href = "Products/InventoryIssue/AssetIssueReport?grnId=" + data.IssueNo;

    };

    //$scope.onVoucherNoDownloadExcel = function (data) {
    //    var reportFormat = "Excel";
    //    if (baseService.isUndefinedOrNull(data.VoucherNo)) return ShowResult('No Id found', 'failure');
    //    $window.open('Accounts/InventoryPayable/PabyableJournal?' + '&reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.GRNNo + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false);
    //};
    $scope.onVoucherNoDownloadPDF = function (data) {
        var reportFormat = "Pdf";
        $window.open('FixedAssets/FixedAssetRegister/GetIssueFixedAssetCapitalizeJournalReport?reportFormat=' + reportFormat + '&voucherId=' + data.IssueVoucherId, '_blank');
    }

    $scope.getNonRegisterAssetReportExcel = function () {
        var filtered = $("#GridNonRegisterAsset").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            filtered = $scope.NonRegisterAssetList;
        }
        $scope.fileName = 'NonRegisterAsset.xls';
        //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
        var materialMasterId = getString(filtered, "MaterialMasterId");
        var materialMasterArticleId = getString(filtered, "ArticleId");
        var voucherId = getString(filtered, "VoucherId");
        var grnNo = getString(filtered, "GRNNo");
        var glId = getString(filtered, "GlId");
        var activityId = getString(filtered, "ActivityId");
        try {

            $http({
                method: 'POST',
                url: $scope.path + 'NonRegisterAssetReportExcel',
                data: {

                    'MaterialMasterId': materialMasterId,
                    'materialMasterArticleId': materialMasterArticleId,
                    'VoucherId': voucherId,
                    'GRNNo': grnNo,
                    'GlId': glId,
                    'ActivityId': activityId
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            // ShowResult(e, 'failure');
            ShowResult(commonMessage.NetworkError, 'failure');
        }

    }


    $scope.GRNWithoutInvoiceReportExcel = function () {
        var reportFormat = "Excel";
        try {
            //var url = $scope.path + 'EmployeeAdvanceDeductionReportExcelFormat?reportFormat=' + reportFormat + '&Year=' + $scope.year + '&Month=' + $scope.month + '&MonthName=' + $scope.monthname;

            var url = $scope.path + 'GRNWithoutInvoiceReportExcelFormat?reportFormat=' + reportFormat + '&ToDate=' + $scope.report.ToDate;
            $rootScope.report(url);
        }
        catch (e) {

        }
    }

    $scope.InvoiceWithoutGRNReportExcel = function () {
        var reportFormat = "Excel";
        try {
            var url = $scope.path + 'InvoiceWithoutGRNReportExcelFormat?reportFormat=' + reportFormat + '&ToDate=' + $scope.report.ToDate;
            $rootScope.report(url);
        }
        catch (e) {

        }
    }

    $scope.BankReportExcel = function () {
        var reportFormat = "Excel";
        try {
            var url = $scope.path + 'BankReportExcelFormat?reportFormat=' + reportFormat + '&toDate=' + $scope.reportParameters.ToDate;
            $rootScope.report(url);
        }
        catch (e) {

        }
    }

    $scope.CashReportExcel = function () {
        var reportFormat = "Excel";
        try {
            var url = $scope.path + 'CashReportExcelFormat?reportFormat=' + reportFormat + '&toDate=' + $scope.reportParameters.ToDate;
            $rootScope.report(url);
        }
        catch (e) {

        }
    }
    //**********************#endregion Invoice GRN With out **************************

    //**********************#startregion Current Fund Position **************************
    $scope.ModelList = [];
    $scope.getCurrentFundPositionData = function () {
        $scope.ModelList = [];
        $http.get('Banks/BankJournal/getCurrentFundPositionlist?PostingDate=' + $filter("dateFiltering")(Date.now()))
            .then(function (response) {
                $scope.ModelList = response.data;
            });
    };

    $scope.ReportCurrentFundPosition = function () {
        try {
            $scope.fileName = "Current Fund Position.xlsx";
            $http({
                method: 'POST',
                url: 'Banks/BankJournal/GetCurrentFundPositionReport',
                data: { 'PostingDate': $filter("dateFiltering")(Date.now()) },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    //**********************#endregion Current Fund Position **************************

    //**********************#startregion Receive Payment Status **************************
    $scope.refreshTemplateReceiptPaymentStatus = function (args) {
        $("#headchkReceiptPaymentStatus").ejCheckBox({ "change": CheckBoxSelectReceiptPaymentStatus });
    };

    function CheckBoxSelectReceiptPaymentStatus(e) {

        var ChkOrUnchkCustomer = false;
        if (e.model.checkState === "check") {
            ChkOrUnchkCustomer = true;
        }

        var filtered = $("#GridReceiptPaymentStatus").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ReceiptPaymentStatusList.length; i++) {
                $scope.ReceiptPaymentStatusList[i].isSelected = ChkOrUnchkCustomer;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchkCustomer;
            }


        }
        var gridObj = $("#GridReceiptPaymentStatus").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.ReceiptPaymentStatusList = [];
    $scope.GetReceiptPaymentStatusList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetReceiptPaymentStatusDataList",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.ReceiptPaymentStatusList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {

        }
    }
    // $scope.GetReceiptPaymentStatusList();

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.ReceiptPaymentStatusSummaryReport = function () {
        try {
            $scope.reportFormat = "Excel";
            $scope.fileName = 'ReceiptPaymentStatus.xls';
            //var ReportFileName = "Receipt Payment Status";
            var gridObj = $("#GridReceiptPaymentStatus").data("ejGrid");
            var data = gridObj.model.dataSource();

            var NewReceiptPaymentStatusList = [];
            for (var i = 0; i < $scope.ReceiptPaymentStatusList.length; i++) {
                if ($scope.ReceiptPaymentStatusList[i].isSelected == true) {
                    if (NewReceiptPaymentStatusList, $scope.ReceiptPaymentStatusList[i].CustomerCode) {
                        NewReceiptPaymentStatusList.push($scope.ReceiptPaymentStatusList[i].CustomerCode);
                    }
                }
            }
            //if (NewReceiptPaymentStatusList.length == 0) {
            //    ShowResult('Please select at least one Customer', 'failure');
            //}
            $http({
                method: 'POST',
                url: $scope.path + 'ReceiptPaymentStatusSummaryReport',
                data: { 'data': data }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //window.location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.PartyStatusList = [];
    $scope.GetPartyStatusData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetPartyStatusDataList",
                data: { 'ToDate': $scope.report.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.PartyStatusList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.PartyStatusReportExcel = function () {
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }

        var dataList = [];
        var g = $("#GridPartyStatus").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.PartyStatusList;
        }
        $scope.fileName = 'PartyStatusList';
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpdate2,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl2 + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };
    //**********************#endregion Receive Payment Status **************************

    //**********************#region Receipt From Customer**************************
    $scope.ReceiptFromCustomerList = [];
    $scope.GetReceiptFromCustomerData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetReceiptFromCustomerList",
                data: { 'fromDate': $scope.reportRFC.FromDate, 'toDate': $scope.reportRFC.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.ReceiptFromCustomerList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {
        }
    }

    $scope.ReceiptFromCustomerReportExcel = function () {
        if ($scope.reportRFC.FromDate === "" || $scope.reportRFC.FromDate === null || $scope.reportRFC.FromDate === undefined) {
            ShowResult('Select To FromDate', 'failure');
            return false;
        }
        if ($scope.reportRFC.ToDate === "" || $scope.reportRFC.ToDate === null || $scope.reportRFC.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }

        var dataList = [];
        var g = $("#GridRFC").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ReceiptFromCustomerList;
        }
        $scope.fileName = 'Receipt From Customer.xlsx';

        $http({
            method: 'POST',
            url: $scope.path + "GetReceiptFromCustomerReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    //**********************#endregion Receipt From Customer**************************

    //**********************#region Payment against invoice**************************
    $scope.PaymentAgainstInvoiceList = [];
    $scope.GetPaymentAgainstInvoiceData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetPaymentAgainstInvoiceList",
                data: { 'fromDate': $scope.reportPAI.FromDate, 'toDate': $scope.reportPAI.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.PaymentAgainstInvoiceList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {
        }
    }

    $scope.PaymentAgainstInvoiceReportExcel = function () {
        if ($scope.reportPAI.FromDate === "" || $scope.reportPAI.FromDate === null || $scope.reportPAI.FromDate === undefined) {
            ShowResult('Select To FromDate', 'failure');
            return false;
        }
        if ($scope.reportPAI.ToDate === "" || $scope.reportPAI.ToDate === null || $scope.reportPAI.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }

        var dataList = [];
        var g = $("#GridPAI").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.PaymentAgainstInvoiceList;
        }
        $scope.fileName = 'Payment Against Invoice Report.xlsx';

        $http({
            method: 'POST',
            url: $scope.path + "GetPaymentAgainstInvoiceReport",
            data: { 'data': dataList, 'reportFileName': $scope.fileName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    //**********************#endregion Payment against invoice**************************

    //**********************#region Employee**************************
    $scope.EmpToDate = $filter('dateFiltering')(Date.now());
    $scope.EmployeeDataList = [];
    $scope.GetEmployeeDataList = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetEmployeeList",
                data: { 'fromDate': "", 'toDate': $scope.EmpToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.EmployeeDataList = response.data.DATA;
                }
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {
        }
    }

    $scope.refreshTemplateEmployees = function (args) {
        $("#headchkemp").ejCheckBox({ "change": CheckBoxSelectEmployee });
    };

    function CheckBoxSelectEmployee(e) {
        var ChkOrUnchkEmp = false;
        if (e.model.checkState === "check") {
            ChkOrUnchkEmp = true;
        }

        var filtered = $("#Gridemp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeDataList.length; i++) {
                $scope.EmployeeDataList[i].isSelected = ChkOrUnchkEmp;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchkEmp;
            }
        }
        var gridObj = $("#Gridemp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.EmployeeSummaryReport = function () {
        try {
            var NewMasterEmpList = [];
            for (var i = 0; i < $scope.EmployeeDataList.length; i++) {
                if ($scope.EmployeeDataList[i].isSelected == true) {
                    NewMasterEmpList.push($scope.EmployeeDataList[i]);
                }
            }
            if (NewMasterEmpList.length == 0) {
                ShowResult('Please select at least one Employee', 'failure');
            }
            $scope.fileName = 'Employee Summary Report.xlsx';

            $http({
                method: 'POST',
                url: $scope.path + "GetEmployeeSummaryReport",
                data: { 'data': NewMasterEmpList, 'reportFileName': $scope.fileName },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.EmployeeDetailReport = function () {
        try {
            $scope.NewMasterEmpIds = [];
            for (var i = 0; i < $scope.EmployeeDataList.length; i++) {
                if ($scope.EmployeeDataList[i].isSelected == true) {
                    $scope.NewMasterEmpIds.push($scope.EmployeeDataList[i].EmployeeId);
                }
            }
            var empIds = getString($scope.NewMasterEmpIds);

            if ($scope.NewMasterEmpIds.length == 0) {
                ShowResult('Please select at least one Employee', 'failure');
            }
            $scope.fileName = 'Employee Details Report.xlsx';

            $http({
                method: 'POST',
                url: $scope.path + "GetEmployeeDetailsReport",
                data: { 'EmpIds': empIds, 'reportFileName': $scope.fileName },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    var getString = function (data) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i]) == false) {
                string += ",'" + data[i] + "'";
                collection.push(data[i]);
            }
        }
        return string;
    }
    //**********************#endregion Employee**************************

}


