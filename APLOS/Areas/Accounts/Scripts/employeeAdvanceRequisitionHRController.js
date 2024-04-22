'use strict';
employeeAdvanceRequisitionHRController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function employeeAdvanceRequisitionHRController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {

    $scope.path = 'accounts/Advance/';
    $scope.currencyList = [];
    $scope.Action = 'Save';
    //search
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    $scope.modelFilterByList = [
        { value: 'SystemId', name: 'SystemId ' },
        { value: 'RequisitionAddedDate', name: 'AddedDate' },
        { value: 'RequisitionRequiredDate', name: 'RequiredDate ' },
        { value: 'Amount', name: 'Amount ' },
        { value: 'Remarks', name: 'Remarks ' },
        { value: 'CheckedBy', name: 'Checked By' },
        { value: 'ApprovedBy', name: 'Approved By' },
        { value: 'AdvanceType', name: 'Advance Type' }

    ];
    $scope.EmpAdvanceReqList = [];
    $scope.create = function (args) {
        $("#checkBox").ejCheckBox({
            change: function (args) {
                var obj = $("#ddlPlantList").ejDropDownList("instance");
                if (args.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "ddlSelectAllCheckBox"
        });
    };



    $scope.searchCol = "SystemId";
    $scope.searchVal = "";
    $scope.getData = function () {

        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "HREmployeeAdvanceRequisitionGetList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.EmpAdvanceReqList = response.data;
        });
    };
    $scope.getData();

    $scope.CheckedDataList = [];
    $scope.getCheckedData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetHREmployeeCheckedDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.CheckedDataList = response.data;
        });
    };
    //$scope.getCheckedData();

    $scope.CheckedHoldDataList = [];
    $scope.getCheckedHoldData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetHREmployeeCheckedHoldDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.CheckedHoldDataList = response.data;
        });
    };
    // $scope.getCheckedHoldData();

    $scope.CheckedRejectDataList = [];
    $scope.getCheckedRejectData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetHREmployeeCheckedRejectDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.CheckedRejectDataList = response.data;
        });
    };
    //$scope.getCheckedRejectData();

    $scope.ApprovedDataList = [];
    $scope.getApprovedData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetHREmployeeApprovedDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedDataList = response.data;
        });
    };
    //$scope.getApprovedData();

    $scope.ApprovedHoldDataList = [];
    $scope.getApprovedHoldData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetHREmployeeApprovedHoldDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedHoldDataList = response.data;
        });
    };
    //$scope.getApprovedHoldData();

    $scope.ApprovedRejectDataList = [];
    $scope.getApprovedRejectData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetHREmployeeApprovedRejectDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedRejectDataList = response.data;
        });
    };
    //$scope.getApprovedRejectData();

    $scope.checkedByList = [];
    $scope.getCboCheckedByList = function () {
        cboService.getAuthorizationConfigCbo('EmployeeAdvanceCheckedBy', function (result) {
            $scope.checkedByList = result;
            //if ($scope.checkedByList.length == 1) {
            //    $scope.model.CheckedBy = $scope.checkedByList[0].Id;
            //}
        });
    };
    $scope.getCboCheckedByList();
    $scope.PostedDataList = [];
    $scope.getPostedData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetHREmployeePostedDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.PostedDataList = response.data;
        });
    };

    $scope.modelMain = {
        SystemId: "",
        EmpSystemId: null,
        EmployeeName: null,
        CurrencyId: null,
        RequisitionAddedDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        RequisitionRequiredDate: null,
        Amount: null,
        Remarks: null,
        CheckedBy: null,
        ApprovedBy: null,
        AdvanceType: "Salary",
        RepaymentStartDate: null,
        LifeOfYear: null,
        ProfitRate: null,
        NoOfInstallmentPerYear: null,
        TotalNoOfInstallment: null
    };
    $scope.model = Object.assign({}, $scope.modelMain);



    cboService.getCurrencyCboForPotal(null, function (result) {
        $scope.currencyList = result;

        $scope.model.CurrencyId = $scope.selectBaseCurrency();
    });

    //cboService.getCompanyGroupCurrencyCbo(null, function (result) {
    //    $scope.currencyList = result;
    //    $scope.detailModel.CurrencyId = $scope.selectBaseCurrency();
    //});



    $scope.Get = function (args) {
        $scope.model = Object.assign({}, $scope.modelMain);
        $scope.LoadData(args.data.SystemId);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };
    $scope.LoadData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + "Get?Id=" + Id
        }).then(function successCallback(response) {
            $scope.model = response.data.master[0];
            $scope.Action = 'Update';
        });
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField($scope.model.RequisitionRequiredDate, "Required Date");

            if (new Date($scope.model.RequisitionAddedDate) > new Date($scope.model.RequisitionRequiredDate)) {
                throw "Required Date cann't less than Entry Date.";
            }

            CheckField($scope.model.Amount, "Amount");
            CheckField($scope.model.CurrencyId, "Currency");
            CheckField($scope.model.CheckedBy, "Checked By");
            CheckField($scope.model.Remarks, "Remarks");
        } catch (e) {
            throw e;
        }
    }
    $scope.validation = function () {
        if ($scope.model.AdvanceType === "Salary" && $scope.loanRepaymentSchedulelist.length == 0) {
            ShowResult("Please Input installment!", "failure");
            return true;
        }
        return false;
    }

    $scope.Save = function () {
        try {
            ValidationMaster();

            var DropDownListcheckedBy = $("#ddlcheckedByList").data("ejDropDownList");
            var CheckedBy = DropDownListcheckedBy.getSelectedValue();
            $scope.model.CheckedBy = CheckedBy;
            if (!$scope.validation()) {
                $http({
                    method: 'POST',
                    data: {
                        "EmpAdvanceReqList": $scope.model,
                        "advanceSalarySchedulelist": $scope.loanRepaymentSchedulelist
                    },
                    url: $scope.path + "HREmployeeAdvanceRequisitionSave"
                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        ShowResult(response.data.Message, 'success');

                        $scope.Cancel();
                        $scope.getData();
                        $scope.Action = 'Save';
                    }
                    else {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "EmployeeAdvanceRequisitionDelete?id=" + $scope.model.SystemId
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.Cancel();
                    $scope.getData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Cancel = function () {
        $scope.Action = 'Save';

        $("#gridEmpAdvanceReqList").ejGrid("instance").refreshContent();
        $scope.modelMain = {
            SystemId: "",
            EmpSystemId: null,
            EmployeeName: null,
            CurrencyId: null,
            RequisitionAddedDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
            RequisitionRequiredDate: null,
            Amount: null,
            Remarks: null,
            CheckedBy: null,
            ApprovedBy: null,
            AdvanceType: "Salary",
            RepaymentStartDate: null,
            LifeOfYear: null,
            ProfitRate: null,
            NoOfInstallmentPerYear: null,
            TotalNoOfInstallment: null
        };
        $scope.model = Object.assign({}, $scope.modelMain);
        $scope.loanRepaymentSchedulelist = [];
        $scope.ClearAdvanceReqSchedule();
        $scope.setVisible($scope.model.AdvanceType);
    };

    $scope.ClearAdvanceReqSchedule = function () {
        $scope.model.RepaymentStartDate = null;
        $scope.model.LifeOfYear = null;
        $scope.model.ProfitRate = null;
        $scope.model.NoOfInstallmentPerYear = null;
        $scope.model.TotalNoOfInstallment = null;
        $scope.loanRepaymentSchedulelist = [];
        $scope.totalInstallment();
        $scope.LoadRepamentDetail();
        $scope.TotalPayments = 0;
        $scope.TotalInterestPaid = 0;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.onClickPdfPrint = function (args) {

        var gridObj = $("#gridEmpAdvanceReqList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SystemId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/Advance/GetEmployeeAdvanceReportPortal?reportFormat=' + reportFormat + '&employeeAdvanceRequisitionId=' + data.SystemId, '_blank');
    };
    $scope.PdfPrint = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "30",
            height: "20",

            click: $scope.onClickPdfPrint
        }
    }];

    $scope.onClickExcelPrint = function (args) {

        var gridObj = $("#gridEmpAdvanceReqList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.SystemId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/Advance/GetEmployeeAdvanceReportPortal?reportFormat=' + reportFormat + '&employeeAdvanceRequisitionId=' + data.SystemId, '_blank');
    };
    $scope.ExcelPrint = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "40",
            height: "20",

            click: $scope.onClickExcelPrint
        }
    }];

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            //$scope.advance.PartyType = $scope.partyType;
            $scope.model.EmpSystemId = employee.SystemId;
            $scope.model.EmployeeName = employee.EmployeeName;
            $scope.setVisible($scope.model.AdvanceType);
        }
        $scope.hideEmployeePopUp();
    };

    $scope.clearEmployeePopUp = function () {
        $scope.model.EmpSystemId = null;
        $scope.model.EmployeeName = null;
    };
    $scope.isVisibleInstallment = false;
    $scope.setVisible = function (AdvanceType) {
        if (AdvanceType === "Salary") {
            $scope.isVisibleInstallment = true;
        }
        else {
            $scope.isVisibleInstallment = false;
        }
    }

    $scope.totalInstallment = function () {
        $scope.model.TotalNoOfInstallment = $scope.model.NoOfInstallmentPerYear;
    };

    $scope.loanRepaymentSchedulelist = [];
    $scope.TotalPayments = 0;
    $scope.TotalInterestPaid = 0;
    $scope.LoadRepamentDetail = function () {

        if ($scope.model.ProfitRate === '' || $scope.model.ProfitRate == 'undefined' || $scope.model.ProfitRate === null) {
            $scope.model.ProfitRate = 0;
        }

        if ($scope.model.NoOfInstallmentPerYear < 12) {
            $scope.model.LifeOfYear = 1;
        }
        else {
            $scope.model.LifeOfYear = $scope.model.NoOfInstallmentPerYear / 12;
        }

        //$scope.model.Amount = $scope.advance.Amount;
        $scope.loanRepaymentSchedulelist = [];
        $("#loanDetails").children().remove();

        var numberOfInstallment = $scope.model.NoOfInstallmentPerYear;
        var actualAmount = parseFloat($scope.model.Amount);
        var actualAmountWithoutProfit = parseFloat($scope.model.Amount);
        var installmentPerYear = 12;

        var rate = parseFloat((parseFloat($scope.model.ProfitRate) / 100) / installmentPerYear);

        var repaymentStartDate = $scope.model.RepaymentStartDate;

        var installmentDate;
        var payment = 0.00;
        var profit = 0.00;
        var principal = 0.00;

        var totalPayment = 0.00;
        var totalProfit = 0.00;
        var totalPrincipal = 0.00;

        var i = 0;

        var idate;
        var periodHtml = "<div class='SearchResult'> <table><thead><tr><td style='width:220px;'>Installment date</td><td style='width:100px;'>Installment no.</td><td style='text-align:right; width:120px;'>Payment</td><td style='text-align:right; width:120px;'>Interest</td><td style='text-align:right; width:120px;'>Principal</td><td style='text-align:right; width:120px;'>Loan</td></tr></thead>";

        for (var i = 1; i <= numberOfInstallment; i++) {
            if (i === 1) {
                installmentDate = new Date(repaymentStartDate);
                idate = installmentDate;
            }
            if (i > 1) {
                installmentDate = new Date((new Date(idate)).setMonth((new Date(idate)).getMonth() + (12 / installmentPerYear)));
                idate = installmentDate;
            }
            if (rate === 0) {
                payment = actualAmountWithoutProfit / numberOfInstallment;
            }
            else {
                payment = PMT(rate, numberOfInstallment, installmentPerYear, parseFloat($scope.model.Amount));
            }
            var iRate = parseFloat($scope.model.ProfitRate) / 100;
            profit = (actualAmount * iRate) / installmentPerYear;

            principal = payment - profit;

            if (i === parseFloat(numberOfInstallment)) {
                actualAmount = parseFloat("0.00");
            }
            else {
                actualAmount = actualAmount - principal;
            }
            var schedule = new Object({
                InstallmentNo: i,
                InstallmentDate: new Date(idate),
                InstallmentAmount: payment.toFixed(2),
                ProfitAmount: profit.toFixed(2),
                PrincipalAmount: principal.toFixed(2),
                Balance: actualAmount.toFixed(2),
                ScheduleNo: 1
            });
            $scope.loanRepaymentSchedulelist.push(schedule);

            totalPayment = totalPayment + payment;
            totalProfit = totalProfit + profit;
            totalPrincipal = totalPrincipal + principal;

            $scope.TotalPayments = totalPayment.toFixed(2);
            $scope.TotalInterestPaid = totalProfit.toFixed(2);

            periodHtml += "<tr><td style ='width:220px;'>" + FormatDate(idate) + "</td><td style ='width:100px;'>" + i + "</td><td style='text-align:right; width:120px;'>" + payment.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + profit.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + principal.toFixed(2) + "</td><td style='text-align:right; width:120px;'>" + actualAmount.toFixed(2) + "</td></tr>";
        }

        $("#loanDetails").append(periodHtml);
        $scope.model.ProfitAmount = totalProfit.toFixed(2);
        return false;

    };

    function PMT(rate, numberOfInstallment, installmentPerYear, actualAmount) {
        var numberOfYear = numberOfInstallment / installmentPerYear;

        var a = 1 / rate;
        var b = 1 + rate;
        var c = Math.pow(b, numberOfInstallment);//
        var d = rate * c;
        var e = 1 / d;

        var pvFactor = a - e;
        var payment = actualAmount / pvFactor;
        return payment;
    }

    function FormatDate(input) {
        var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var dt = new Date(input);
        return [dt.getDate(), months[dt.getMonth()], dt.getFullYear()].join('-');
    }

}