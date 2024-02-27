"use strict";
assetDisposePostController.$inject = ["accountService", "cboService","commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function assetDisposePostController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Capitalize Asset Dispose Post";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;

    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    $scope.voucherDetailList = [];
    $scope.searchBy = "DisposeNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'DisposeNo', name: "Dispose No" }, { value: 'EmployeeName', name: "Employee" }, { value: 'Status', name: "Status" }];

    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetFixedAssetDisposePostedList'
            , data: { column: $scope.searchBy, value: $scope.search }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.voucherList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getData();
    
    $scope.voucherDetailList = [];
    $scope.disposeSearchBy = "DisposeNo"; $scope.search = "";
    $scope.disposeSearchByList = [{ value: 'DisposeNo', name: "Dispose No" }, { value: 'EmployeeName', name: "Employee" }, { value: 'Status', name: "Status" }];

    $scope.fixedAssetDisposeList = [];
    $scope.getDisposeData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetCapitalizeAssetRegisterDisposePopUpList'
            , data: { column: $scope.disposeSearchBy, value: $scope.disposeSearch }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDisposeList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector('#DisposePopUp')).modal('show');
    };
   
    $scope.closeFixedAssetDisposePopUp = function () {
        angular.element(document.querySelector('#DisposePopUp')).modal('hide');
    }
    $scope.voucher = {
        Id: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        Remarks: null,
        IsPark: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        EmployeeId: null,
        Designation: null,
        DOJ: null,
        GivenDesignation: null,
        Department: null,
        LegalDesignation: null,
        CompanyCurrencyRate: 1,

        RepaymentStartDate: null,
        LifeOfYear: null,
        ProfitRate: null,
        NoOfInstallmentPerYear: null,
        ProfitAmount: null,
        TotalNoOfInstallment: null,
        PaymentTermId: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null
    };

    $scope.voucherDetail = {
        Id: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        COAICode: null,
        AccountTypeId: null,
        CurrencyId: null,
        DocRefNo: null,
        DrAmount: null,
        CrAmount: null,
        Narration: null,
        BankMasterId: null,
        CashMasterId: null,
        PartyId: null,
        PartyPlantId: null,
        TransactionTypeId: null,
        FAType: null,
        DrDisable: false,
        CrDisable: false,
        CashCurrencyId: null,
        BankCurrencyId: null,
        BankAmount: null
    };

    $scope.fixedAssetDisposeDetailList = [];
    $scope.getDataByDisposeId = function (x) {
        var data = x.data;
        $scope.voucher.DisposeNo = data.DisposeNo;
        $scope.voucher.EmployeeId = data.EmployeeId;
        $scope.voucher.CustomerName = data.CustomerName;
        $scope.voucher.PartyId = data.PartyId;
        $scope.voucher.PartyPlantId = data.PartyPlantId;
        $scope.voucher.EmployeeName = data.EmployeeName;
        $scope.voucher.Designation = data.Designation;
        $scope.voucher.Department = data.Department;
        $scope.voucher.Status = data.Status;
        $scope.voucher.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.voucher.Remarks = data.Remarks;
        $scope.voucher.DeliveryPartyPlantId = data.DeliveryPartyPlantId;
        $scope.voucher.InvoicingByAddress = data.InvoicingByAddress;
        $scope.voucher.DeliveryByAddress = data.DeliveryByAddress;
        if ($scope.voucher.Status =='Sales')
          $scope.paymentTerm();
        $scope.voucher.CurrencyId = data.trnCurrencyId;
        $scope.voucher.CompanyCurrencyRate = data.ToCurrencyRate;

        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetFixedAssetLostByDisposeIdList?id=' + data.Id
            , data: { column: $scope.disposeSearchBy, value: $scope.disposeSearch }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDisposeDetailList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
            };
        if ($scope.companyConfig.IsFixedAssetSalesBook && $scope.voucher.PartyId != null) {
            $scope.getFixedAssetSalesBookAsSalesJV1List(data.Id);
            $scope.getFixedAssetSalesBookAsSalesJV2List(data.Id);
        }
        else {

        $scope.getDisposeJV(data.Id);
        }
        angular.element(document.querySelector('#DisposePopUp')).modal('hide');
    };

    $scope.fixedAssetDisposeJVList = [];
    $scope.getDisposeJV = function (id) {
        $scope.fixedAssetDisposeJVList = [];
        if ($scope.voucher.Status == 'CompensateByEmployee') {
            $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetCapitalizeAssetLostJVList?fixedAssetDisposeId=' + id
        }
        else if ($scope.voucher.Status == 'Sales') {
            $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetFixedAssetSalesSingleJVList?fixedAssetDisposeId=' + id
        }
        else if ($scope.voucher.Status == 'Scrap') {
            $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetFixedAssetScrapSingleJVList?fixedAssetDisposeId=' + id
        }
        else if ($scope.voucher.Status == 'Theft') {
            $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetFixedAssetTheftSingleJVList?fixedAssetDisposeId=' + id
        }
        $http({
            method: 'Post'
            , url: $scope.jvurl
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDisposeJVList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };

    };

    $scope.getFixedAssetSalesBookAsSalesJV1List = function (id) {
        $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetFixedAssetSalesBookAsSalesJV1List?fixedAssetDisposeId=' + id
        $http({
            method: 'Post'
            , url: $scope.jvurl
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDisposeJVList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };

    };
    $scope.fixedAssetDisposeJV2List = [];
    $scope.getFixedAssetSalesBookAsSalesJV2List = function (id) {
        $scope.fixedAssetDisposeJV2List = [];
        $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetFixedAssetSalesBookAsSalesJV2List?fixedAssetDisposeId=' + id
        $http({
            method: 'Post'
            , url: $scope.jvurl
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDisposeJV2List = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };

    };

    $scope.Get = function (data) {
        $scope.voucher.Id = data.Id;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByPlant(null, null, "", function (result) {
            $scope.entityList = result;
        });
    });

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
        $scope.baseCurrencyId = $scope.selectBaseCurrency();
        $scope.voucher.CurrencyId = $scope.baseCurrencyId;
        $scope.GetCurrencyExchangeRateList();
    });

    $scope.getCboVoucherTypeFixedAssetDisposeJournalList = function () {
        cboService.getCboVoucherTypeFixedAssetDisposeJournalList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                //$scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeFixedAssetDisposeJournalList();
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };
    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };
    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.fixedAssetDisposeJVList = [];
        $scope.fixedAssetDisposeDetailList = [];
        $scope.loanRepaymentSchedulelist = [];
    };

    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'accounts/EmployeePayable/GetEmployeeListAllPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.voucher.EmployeeId = employee.SystemId;
            $scope.voucher.EmployeeName = employee.EmployeeCode + ' - ' + employee.EmployeeName;
            $scope.voucher.DOJ = employee.DOJ;
            $scope.voucher.Department = employee.Department;
            $scope.voucher.Designation = employee.Designation;
            $scope.voucher.GivenDesignation = employee.GivenDesignation;
            $scope.voucher.LegalDesignation = employee.LegalDesignation;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.Save = function () {
        //$scope.voucher.DocDate = $scope.voucher.PostingDate;
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
                $scope.SaveUrl = "fixedassets/FixedAssetRegister/CreateFixedAssetDisposePost"
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.SaveUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.fixedAssetDisposeJVList,
                        "farDisposeDetailList": $scope.fixedAssetDisposeDetailList,
                        "advanceSalarySchedulelist": $scope.loanRepaymentSchedulelist
                    },
                    dataType: "JSON"
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "accounts/OpeningBalance/UpdateOBAdvanceJournal",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    //$scope.post = function () {
    //    $http({
    //        method: "POST",
    //        url: "accounts/OpeningBalance/PostOBAdvanceJournal",
    //        data: {
    //            "voucherVM": $scope.voucher,
    //            "voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
    //        },
    //        dataType: 'JSON'
    //        , contentType: "application/json charset=utf-8"
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, "failure");
    //        }
    //        else {
    //            ShowResult(response.data.Message, "success");
    //            $scope.getData();
    //            $scope.clear();
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.status.Message, "failure");
    //    });
    //    return true;
    //};

    $scope.selectFARegisterPopUp = function (x) {
        var data = x.data;
        $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetail.BudgetName = data.BudgetName;
        $scope.voucherDetail.ActivityId = data.ActivityId;
        $scope.voucherDetail.ActivityName = data.ActivityName;
        $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoCode + '-' + data.GLGeneralInfoName;
        $scope.voucherDetail.FixedAssetMasterId = data.FixedAssetMasterId;
        $scope.voucherDetail.FixedAssetRegisterId = data.FixedAssetRegisterId;
        $scope.voucherDetail.ParticularName = data.FixedAssetMasterName;
        $scope.voucherDetail.FAType = $scope.voucher.FAType;
        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherDetail.Narration = $scope.voucher.Narration;
        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
        $scope.voucherDetail.BaseCurrency = $scope.voucher.BaseCurrency;
        

        $scope.voucherDetail.Price = data.Price;
        $scope.voucherDetail.SubAssetAmount = data.SubAssetAmount;
        $scope.voucherDetail.PurchasePrice = data.PurchasePrice;
        $scope.voucherDetail.ADBaseAmount = data.ADBaseAmount;
        $scope.voucherDetail.NetBookValue = data.NetBookValue;

        $scope.voucherDetail.FABaseAmount = data.FABaseAmount;
        $scope.voucherDetail.SubAssetBaseAmount = data.SubAssetBaseAmount;
        $scope.voucherDetail.PurchaseBaseAmount = data.PurchaseBaseAmount;
        $scope.voucherDetail.ADBaseAmount = data.ADBaseAmount;
        $scope.voucherDetail.NetBaseBookValue = data.NetBaseBookValue;
        $scope.voucherDetail.NegotiationValue = data.NegotiationValue;
        $scope.voucherDetail.isOB = data.IsOpeningBalance;

        $scope.voucherDetail.SerialNo = data.SerialNo;
        $scope.voucherDetail.AssetNo = data.AssetNo;
        $scope.voucherDetail.Id = data.Id;
        $scope.voucherDetail.PartyType = 'Fixed Asset';
        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
        $scope.voucherDetail = {};
        $scope.closeFARegisterPopUp();
    };
    $scope.showPopup = function () {
        angular.element(document.querySelector('#employeeSelectionPopUp')).modal('show');
    }
    $scope.hidePopup = function () {
        angular.element(document.querySelector('#employeeSelectionPopUp')).modal('hide');
    }




    $scope.totalInstallment = function () {
        //if ($scope.voucher.NoOfInstallmentPerYear < 12) {
        //    $scope.voucher.LifeOfYear = 1;
        //}
        //else {
        //    $scope.voucher.LifeOfYear = $scope.voucher.NoOfInstallmentPerYear / 12;
        //}

        //$scope.voucher.TotalNoOfInstallment = ($scope.voucher.LifeOfYear * $scope.voucher.NoOfInstallmentPerYear);
        $scope.voucher.TotalNoOfInstallment = $scope.voucher.NoOfInstallmentPerYear;
    };

    $scope.loanRepaymentSchedulelist = [];
    $scope.TotalPayments = 0;
    $scope.TotalInterestPaid = 0;
    $scope.LoadRepamentDetail = function () {

        if ($scope.voucher.ProfitRate === '' || $scope.voucher.ProfitRate == 'undefined' || $scope.voucher.ProfitRate === null) {
            $scope.voucher.ProfitRate = 0;
        }
        //if ($scope.voucher.IsSchedule) {
        if ($scope.voucher.NoOfInstallmentPerYear < 12) {
            $scope.voucher.LifeOfYear = 1;
        }
        else {
            $scope.voucher.LifeOfYear = $scope.voucher.NoOfInstallmentPerYear / 12;
        }

        $scope.voucher.Amount = $filter("sumByKey")($filter("filter")($scope.fixedAssetDisposeDetailList), "NegotiationValue");;

        $scope.loanRepaymentSchedulelist = [];
        $("#loanDetails").children().remove();
        //var numberOfInstallment = $scope.voucher.TotalNoOfInstallment;
        var numberOfInstallment = $scope.voucher.NoOfInstallmentPerYear;
        var actualAmount = parseFloat($scope.voucher.Amount);
        var actualAmountWithoutProfit = parseFloat($scope.voucher.Amount);
        var profitAmount = $scope.voucher.ProfitAmount;
        //var installmentPerYear = $scope.voucher.NoOfInstallmentPerYear;
        var installmentPerYear = 12;
        //if ($scope.voucher.NoOfInstallmentPerYear < 12) {           
        //    installmentPerYear = $scope.voucher.NoOfInstallmentPerYear;
        //}
        var rate = parseFloat((parseFloat($scope.voucher.ProfitRate) / 100) / installmentPerYear);
        //rate = parseFloat(rate.toFixed(2));
        //var rate = parseFloat((parseInt($scope.voucher.ProfitRate) / 100) / installmentPerYear);
        //console.log('rate', rate);
        //console.log('rated', $scope.voucher.ProfitRate);

        var disbursmentDate = $scope.voucher.DocDate;
        var repaymentStartDate = $scope.voucher.RepaymentStartDate;
        // var installmentDate = new Date(repaymentStartDate);
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
        //periodHtml += "<tr><td>" + FormatDate(disbursmentDate) + " (Disbursement date)" + "</td><td>" + " " + "</td><td style='text-align:right'>" + payment.toFixed(2) + "</td><td style='text-align:right'>" + profit.toFixed(2) + "</td><td style='text-align:right'>" + principal.toFixed(2) + "</td><td style='text-align:right'>" + actualAmount.toFixed(2) + "</td></tr>";
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
                payment = PMT(rate, numberOfInstallment, installmentPerYear, parseFloat($scope.voucher.Amount));
            }
            var iRate = parseFloat($scope.voucher.ProfitRate) / 100;
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
                InstallmentAmount: payment,
                ProfitAmount: profit,
                PrincipalAmount: principal,
                Balance: actualAmount,
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
        //periodHtml += "<tr><td></td><td></td><td style='text-align:right;font-weight: bold'>" + totalPayment.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalProfit.toFixed(2) + "</td><td style='text-align:right;font-weight: bold'>" + totalPrincipal.toFixed(2) + "</td><td></tr></table></div>";
        $("#loanDetails").append(periodHtml);
        $scope.voucher.ProfitAmount = totalProfit.toFixed(2);
        return false;
        //}
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

    //$scope.isVisibleInstallment = false;
    //$scope.setVisible = function (AdvanceType) {
    //    if (AdvanceType === null || AdvanceType === undefined || AdvanceType === '') {
    //        $scope.isVisibleInstallment = false;
    //        //return 
    //    }
    //    else {
    //        if (AdvanceType === 'Salary') {
    //            $scope.isVisibleInstallment = true;
    //        }
    //        else {
    //            $scope.isVisibleInstallment = false;
    //        }
    //    }
    //}


    //$scope.commandExcel = [{
    //    type: "details", buttonOptions: {
    //        text: "Excel",
    //        width: "50",
    //        height: "20",
    //        //contentType: "imageonly",
    //        //prefixIcon: "e-icon e-dataexport",

    //        //prefixIcon: "e-icon e-edit" ,
    //        //prefixIcon: "e-icon e-delete",
    //        //prefixIcon: " e-icon e-save",
    //        //prefixIcon: " e-icon e-cancel",

    //        click: $scope.onClickReportDownloadExcel
    //    }
    //}];

    $scope.onClickReportDownloadExcel = function (args) {
        //debugger;
        //var gridObj = $("#GridEdit").data("ejGrid");
        ////getting corresponding record 
        //var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        //$window.open($scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&voucherId=' + data.Id, '_blank');


        try {
            var file_src = $scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&disposedVoucherId=' + args.Id
            $rootScope.report(file_src/*, '_blank'*/);


            //var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' +'202015848'
            //$rootScope.report(file_src, '_blank');

            // $window.open($scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&disposedVoucherId=' + args.Id, '_blank');

            //$window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + args.Id, '_blank');

            // ReportVendorInvoice ? reportFormat = Pdf & voucherId=202015865

        } catch (e) {

        }

    };



    //$scope.commandPDF = [{
    //    type: "details", buttonOptions: {
    //        text: "PDF",
    //        width: "50",
    //        height: "20",
    //        //contentType: "imageonly",
    //        //prefixIcon: "e-icon e-dataexport",

    //        //prefixIcon: "e-icon e-edit" ,
    //        //prefixIcon: "e-icon e-delete",
    //        //prefixIcon: " e-icon e-save",
    //        //prefixIcon: " e-icon e-cancel",

    //        click: $scope.onClickReportDownloadWord
    //    }
    //}];

    $scope.onClickReportDownloadWord = function (args) {
        //debugger;
        //var gridObj = $("#GridEdit").data("ejGrid");
        ////getting corresponding record 
        //var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(args.Id)) return ShowResult('No Id found', 'failure');
        //$window.open($scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&voucherId=' + args.Id, '_blank');

        try {
            var file_src = $scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&disposedVoucherId=' + args.Id
            $rootScope.report(file_src/*, '_blank'*/);


            //var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' +'202015848'
            //$rootScope.report(file_src, '_blank');

            // $window.open($scope.path + 'FixedAssetsDisposePost?reportFormat=' + reportFormat + '&disposedVoucherId=' + args.Id, '_blank');

            //$window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + args.Id, '_blank');

            // ReportVendorInvoice ? reportFormat = Pdf & voucherId=202015865

        } catch (e) {

        }
    };

    $scope.paymentTerm = function () {
  
            $scope.paymenttermUrl = "accounts/PaymentTerm/getcustomercbo";
       
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
    };
    //

    $scope.changePaymentTerm = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) {
                return item.Value === id;
            })[0];
            $scope.voucher.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.voucher.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === "documentdate") {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.DocDate;
                    $scope.IsBaseOnDueDateEnable = true;
                } else if (paymentTerm.BaseLineDate === "postingdate") {
                    $scope.voucher.BaseOnDueDate = $scope.voucher.PostingDate;
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else if (paymentTerm.BaseLineDate === "voucherdate") {
                    $scope.voucher.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.IsBaseOnDueDateEnable = false;
                    $scope.voucher.BaseOnDueDate = $filter("dateFiltering")(Date.now());
                }
            $scope.getMatureDate($scope.voucher.BaseOnDueDate, $scope.voucher.BaseNoOfDays);
        }
    };

    $scope.getMatureDate = function (date, days) {
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.voucher.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
    };

}