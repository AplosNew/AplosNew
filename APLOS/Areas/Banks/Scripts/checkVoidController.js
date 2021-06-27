"use strict";
checkVoidController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$window"];
function checkVoidController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = "Check Void";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.path = "accounts/voucher/";
    $scope.printpath = "Banks/CheckManagement/";
    $scope.getVoucherListUrl = $scope.path + "GetVoucherListForCheckVoidPrinting/";

    $scope.printNonCashCheck = {
        VoucherId: null,
        VoucherDetailId: null,
        VoucherNo: null,
        CurrencyCode: null,
        Amount: null,
        PartyId: null,
        BankMasterId: null,
        CheckLotId: null,
        CheckLotDetailId: null,

        BankName: null,
        BankBranch: null,
        BankAccount: null,
       // Date: null,
        PostingDate: null,
        CheckDate: null,
        BankGL: null,
        BankCurrencyId: null,
        BankCurrency: null,

        PartyName: null,
        VendorVATResistrationNo: null,
        VendorTradeLicenseNo: null,
        VendorGl: null,
        VendorCurrency: null,
        VendorTaxExemption: null,
        CheckTemplate: null
    };
    $scope.printNonCashCheckNew = Object.assign({}, $scope.printNonCashCheck);

    $scope.voucherList = [];
    $scope.voucherTitle = "Voucher";
    $scope.voucherIndex = -1;
    $scope.valueData = null;
    $scope.voucherParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getVoucher = function () {
        try {
            $scope.getVoucherData = function (pageno) {
                baseService.paginationBase($scope.getVoucherListUrl, pageno, $scope.voucherParameters)
                    .then(function (result) {
                        $scope.voucherDataList = result.Rows;
                        $scope.voucherParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.voucherList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.voucherList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            $scope.getVoucherData();
            angular.element(document.querySelector("#voucherListPopUp")).modal("show");
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.selectVoucherPopUp = function (index, id) {
        $scope.voucherIndex = index;
        $scope.selectedVoucher = id;
    };

    $scope.selectDoubleClick = function (data) {




        selectVoucherRow(data);
        $scope.closeVoucherPopUp();
    };

    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };

    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            ShowResult("Please at first select row", "failure");
        }
        $scope.selectDoubleClick($scope.valueData);
        $scope.closeVoucherPopUp();
    };

    $scope.closeVoucherPopUp = function () {
        $scope.valueData = "";
        angular.element(document.querySelector("#voucherListPopUp")).modal("hide");
        $scope.voucherIndex = -1;
    };

    function selectVoucherRow(data) {
        
        if (data.GLGeneralInfoId === null) {
            ShowResult("voucher GL not found!", "failure");
        }
        else if (data.CurrencyId === null) {
            ShowResult("voucher Transaction Currency not found!", "failure");
        }
        else  {
            $scope.printNonCashCheckNew.VoucherId = data.VoucherId;
            $scope.printNonCashCheckNew.VoucherDetailId = data.VoucherDetailId;
            $scope.printNonCashCheckNew.VoucherNo = data.VoucherNo;
            $scope.printNonCashCheckNew.CurrencyCode = data.CurrencyCode;
            $scope.printNonCashCheckNew.Amount = data.Amount;
            $scope.printNonCashCheckNew.PostingDate = data.PostingDate;
            $scope.printNonCashCheckNew.CheckDate = null;
            $scope.printNonCashCheckNew.PartyId = data.PartyId;
            $scope.printNonCashCheckNew.Party = data.Party;
            $scope.printNonCashCheckNew.BankMasterId = data.BankMasterId;
            $scope.printNonCashCheckNew.BankCurrencyId = data.CurrencyId;
            $scope.printNonCashCheckNew.BankName = data.Bank;
            $scope.printNonCashCheckNew.BankBranch = data.BankBranch;
            $scope.printNonCashCheckNew.BankGL = data.GLGeneralInfoId;
            $scope.printNonCashCheckNew.BankAccount = data.BankAccountTitle;
            $scope.printNonCashCheckNew.BankCurrency = data.CurrencyCode;
            $scope.printNonCashCheckNew.CheckTemplate = data.CheckTemplate;

            $scope.getCheckLot($scope.printNonCashCheckNew.BankMasterId);

            //$scope.getBank($scope.printNonCashCheckNew.BankMasterId);
        }
    }

    //$scope.getBank = function (bankMasterId) {
    //    try {
    //        $http.get("banks/bankmaster/GetBankMasterByMasterId?bankMasterId=" + bankMasterId)
    //      //  $http.get("banks/bankmaster/GetBankMasterByMasterId?bankMasterId=" + $scope.printNonCashCheckNew.BankMasterId)
                    
    //            .then(function (response) {
    //                if (response.data !== null) {
                       
    //                    $scope.printNonCashCheckNew.BankMasterId = response.data.BankMasterId;
                       
    //                    $scope.printNonCashCheckNew.BankGL = response.data.GLGeneralInfoId + ' - ' + response.data.GLGeneralInfoName;
    //                    $scope.printNonCashCheckNew.GLGeneralInfoId = response.data.GLGeneralInfoId;
                        

    //                    $scope.getCheckLot(bankMasterId);
    //                }
    //                else {
    //                    ShowResult("Bank not found!", "failure");
    //                }
    //            });
    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};

    $scope.getCheckLot = function (bankMasterId) {
        try {
            $scope.checkLotList = [];
            //$http.get("banks/CheckManagement/getcbo?bankMasterId=" + bankMasterId)
            //    .then(function (response) {
            //        if (response.data !== null) {
            //            $scope.checkLotList = response.data;
            //        }
            //        else {
            //            ShowResult("Active check not found!", "failure");
            //        }
            //    });

            $http({
                method: "GET",
                url: "banks/CheckManagement/getcbo?bankMasterId=" + bankMasterId
            }).then(function successCallback(response) {
                $scope.checkLotList = response.data;
            });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.checkNo = true;
    $scope.getCheckLotDetail = function (checkLotId) {
        try {
            if (baseService.isUndefinedOrNull(checkLotId)) {
                $scope.printNonCashCheckNew.CheckLotDetailId = null;
                $scope.checkNo = true;
                return;
            }
            var sequential = $.grep($scope.checkLotList, function (item) {
                return item.Value === checkLotId;
            })[0].isNonSequential;

            $scope.checkLotDetailList = [];

            $http({
                method: "GET",
                url: "Banks/CheckManagement/getdetailcbo?checkLotId=" + checkLotId + "&isSequential=" + sequential
            }).then(function successCallback(response) {
                $scope.checkLotDetailList = response.data;

                if ($scope.checkLotDetailList.length === 1) {
                            $scope.printNonCashCheckNew.CheckLotDetailId = $scope.checkLotDetailList[0].Value;
                            $scope.checkNo = true;
                }
                 else
                     $scope.checkNo = false;
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.reportPrintCheeckVoid = function () {
        try {
            location.href = 'Banks/CheckManagement/GetPrintCheckVoidReport?voucherDetailId=' + $scope.printNonCashCheckNew.VoucherDetailId + '&checkDate=' + $scope.printNonCashCheckNew.CheckDate + '&checkTamplate=' + $scope.printNonCashCheckNew.CheckTemplate ;
           // location.href = 'Banks/CheckManagement/GetPrintNonCashCheckReport?voucherDetailId=' + $scope.printNonCashCheckNew.VoucherDetailId + '&checkDate=' + $scope.printNonCashCheckNew.CheckDate;
        } catch (e) {
            ShowResult(e, "failure");
        }
    }





    //$scope.getReport = function () {
    //    if (baseService.isUndefinedOrNull($scope.printNonCashCheckNew.CheckDate)) {
    //        manualValidation("div_CheckDate", true, "Check Date is required.");
    //    }
    //    //else if (baseService.isUndefinedOrNull($scope.ModelNew.ToDate)) {
    //    //    manualValidation("div_ToDate", true, "To Date is required.");
    //    //}

    //    //else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
    //    //    manualValidation('div_FromDate', true, "From Date is required.");
    //    //}
    //    //else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
    //    //    manualValidation('div_ToDate', true, "To Date is required.");
    //    //}

    //    //else if (new Date($scope.ModelNew.FromDate) > new Date($scope.ModelNew.ToDate)) {
    //    //    manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
    //    //}
    //    else if (new Date($scope.printNonCashCheckNew.CheckDate) < new Date($scope.printNonCashCheckNew.PostingDate)) {
    //        manualValidation('div_CheckDate', true, "Check date must be above or equal to Posting Date.");
    //    }
    //    else {
    //        return false;
    //    }
    //};


    //$scope.Save = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    //$scope.dateValidation()
    //    if ($scope.SandwichLeaveOnHoliday.$valid && !$scope.getReport()) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrl,
    //            data: { 'sFromDate': $scope.ModelNew.FromDate, 'sTodate': $scope.ModelNew.ToDate },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //    }
    //};

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {

        var msg = "";
        if (new Date($scope.printNonCashCheckNew.CheckDate) < new Date($scope.printNonCashCheckNew.PostingDate)) {
            $scope.invalidDocDate = true;
            msg = "Check date must be above or equal to Posting Date!";
        }
        else if (baseService.isUndefinedOrNull($scope.printNonCashCheckNew.CheckDate)) {
            msg = "Check Date is required.";
            $scope.invalidDocDate = true;
        }
        else {
            $scope.invalidDocDate = false;
        }
        return manualValidation("div_CheckDate", $scope.invalidDocDate, msg);
    };

    $scope.Print = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkDocDate();
        // ($scope.printNonCashCheckNewForm.$valid && !$scope.invalidDocDate)
        if ($scope.printNonCashCheckNewForm.$valid && !$scope.invalidDocDate){
            $http({
                method: "POST",
                url: "Banks/CheckManagement/CheckVoidPrintReport",
                data: {
                    //"voucherVM": $scope.voucher,
                    //"voucherDetailVMList": $scope.voucherDetailList
                    "voucherDetailId": $scope.printNonCashCheckNew.VoucherDetailId
                    , "checkLotDetailId": $scope.printNonCashCheckNew.CheckLotDetailId
                    , "amount": $scope.printNonCashCheckNew.Amount
                    , "bankCurrencyId": $scope.printNonCashCheckNew.BankCurrencyId
                    , "checkDate": $scope.printNonCashCheckNew.CheckDate
                    , "checkTamplate": $scope.printNonCashCheckNew.CheckTemplate
                    //    //, "bankCurrency": $scope.printNonCashCheckNew.BankCurrency
                    //    //, "partyName": $scope.printNonCashCheckNew.PartyName
                    // , 'sFromDate': $scope.ModelNew.FromDate
                    // , 'sTodate': $scope.ModelNew.ToDate 

                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    // $scope.getData();
                    $scope.reportPrintCheeckVoid();
                    //  $scope.Clear();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        }
            
    }


    $scope.VoucherPrint = function () {
        try {
            openPrintPopUp();
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.printNonCashCheckNewForm.$valid) {
                angular.copy($scope.printNonCashCheckNew, $scope.printNonCashCheck);
                //$http({
                //    method: "POST",
                //    url: $scope.printpath + "xPrintNonCashCheck",
                //    data: {
                //        "voucherDetailId": $scope.printNonCashCheckNew.VoucherDetailId
                //        , "checkLotDetailId": $scope.printNonCashCheckNew.CheckLotDetailId
                //        , "amount": $scope.printNonCashCheckNew.Amount
                //        , "bankCurrencyId": $scope.printNonCashCheckNew.BankCurrencyId
                //        //, "bankCurrency": $scope.printNonCashCheckNew.BankCurrency
                //        //, "partyName": $scope.printNonCashCheckNew.PartyName
                //    },
                //    dataType: "JSON"
                //}).then(function successCallback(response) {
                //    if (response.data.Error == true) {
                //        ShowResult(response.data.Message, "failure");
                //    }
                //    else {
                //        ShowResult(response.data.Message, "success");
                //        var inWord=response.data.InWord;
                //    }
                //});
               // Accounts / Voucher / GetDashBoardJournalVoucherReport ? reportFormat = Pdf & voucherId=202018426
                try {
                    location.href = 'Banks/CheckManagement/GetPrintCheckVoidVoucherReport?reportFormat=' + 'Pdf' + '&voucherId=' + $scope.printNonCashCheckNew.VoucherId + '&voucherDetailId=' + $scope.printNonCashCheckNew.VoucherDetailId; //voucherDetailId     $scope.printNonCashCheckNew.VoucherDetailId
                } catch (e) {''
                    ShowResult(e, "failure");
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    function openPrintPopUp() {
        var window = $window.open("", "", "");
        window.document.write("<p>Amount : <b>" + $scope.printNonCashCheckNew.Amount + "</b></p>");
        window.document.write("<p>Party : <b>" + $scope.printNonCashCheckNew.PartyName + "</b></p>");
        window.document.close();
        window.focus();
        window.print();
        window.close();
    }

    $scope.Clear = function () {
        $scope.checkNo = true;
        $scope.printNonCashCheck = {};
        $scope.printNonCashCheckNew = { Amount: null };
    };
}