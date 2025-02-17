'use strict';
InvoiceTaggedWithLCController.$inject = ['accountService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function InvoiceTaggedWithLCController(accountService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $rootScope.title = "Invoice Tagged With LC";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/InvoiceTaggedWithLC/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'CreateCharge';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    //#region Page Loading ...

    $scope.AutoLoanAvailableDataList = [];
    $scope.fromDateTitle = "As On Mature Date";
    $scope.toDateShow = false;
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.AutoLoan = {
        Id: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        DateRange: "false",
    };
    $scope.AutoLoanNew = Object.assign({}, $scope.AutoLoan);
    $scope.viewChange = function () {
        if ($scope.AutoLoanNew.DateRange === "true") {
            $scope.fromDateTitle = "From Date";
            $scope.toDateShow = true;
            $scope.AutoLoanNew.FromDate = $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1));
            $scope.AutoLoanNew.ToDate = $filter('dateFiltering')(Date.now());
        }
        else {
            $scope.fromDateTitle = "As On Mature Date";
            $scope.toDateShow = false;
            $scope.AutoLoanNew.FromDate = $filter('dateFiltering')(Date.now());
            $scope.AutoLoanNew.ToDate = $filter('dateFiltering')(Date.now())
        }
    };

    $scope.getAutoLoanAvailableList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetVendorAvailableInvoiceList?FromDate=" + $scope.AutoLoanNew.FromDate + '&ToDate=' + $scope.AutoLoanNew.ToDate + '&DateRange=' + $scope.AutoLoanNew.DateRange + '&PartyId=' + $scope.AutoLoanNew.PartyId,
        }).then(function successCallback(response) {
            $scope.AutoLoanAvailableDataList = response.data;
        });
    }
    $scope.ChangeValue = function () {
        $scope.LcModel.LoanAmount = null;
        $scope.LcModel.LoanNo = null;
        $scope.LcModel.LoanDate = $filter("dateFiltering")(Date.now());
    };
    //$scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        if (baseService.isUndefinedOrNull($scope.LcModel.LoanDate)) {
            ShowResult("Loan Date is required.! ", "failure");
            return true;
        }
        else if (new Date($scope.LcModel.LoanDate) > new Date()) {
            ShowResult("Loan date must be below or equal to current Date! " , "failure");
            return true;
        }
        else if ($scope.selectedInvoiceList.length > 0) {
            for (var i = 0; i < $scope.selectedInvoiceList.length; i++) {
                if (new Date($scope.selectedInvoiceList[i].PostingDate) > new Date($scope.LcModel.LoanDate)) {
                    $scope.LcModel.LoanDate = $filter("dateFiltering")(Date.now());
                    ShowResult("Loan date must be below or equal to payable of " + $scope.selectedInvoiceList[i].VoucherNo, "failure");
                    return true;
                }
            }
        }
        
    };
    //#endregion

    //#region Clear
    $scope.Clear = function () {
        $scope.AutoLoan = {
            Id: null,
            PartyId: null,
            PartyCode: null,
            PartyName: null,
            FromDate: $filter('dateFiltering')(Date.now()),
            ToDate: $filter('dateFiltering')(Date.now()),
            DateRange: "false",
        };
        $scope.selectedInvoiceList = [];
        $scope.AutoLoanAvailableDataList = [];
        $scope.fromDateTitle = "As On Date";
        $scope.LcModel = { LoanAmount: 0, IsLoan: 'true', LoanDate: $filter("dateFiltering")(Date.now()) };
    }
    //#endregion

    //#region Pop Up
    $scope.purchaseLCList = [];
    $scope.getpurchaseLCListData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/InvoiceTaggedWithLC/getpurchaseLCList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        
                        $scope.purchaseLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
   // $scope.getpurchaseLCListData();

    $scope.getSavedData = function (index) {
        $scope.lcIndex = index;
        $scope.getpurchaseLCListData();
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("show");
    }


    $scope.purchaseLCBalanceList = [];
    $scope.getLCBalanceAmountDetailByLCIdData = function (purchaseLCId) {
        $scope.purchaseLCBalanceList = [];
        $http.get("Commercial/InvoiceTaggedWithLC/GetLCSetOffDetailByInvoiceList?purchaseLCId=" + purchaseLCId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.purchaseLCBalanceList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    

    $scope.getLCBalanceAmountDetailData = function (index,x) {
        $scope.lcIndex = index;
        $scope.getLCBalanceAmountDetailByLCIdData(x.PurchaseLcId);
        angular.element(document.querySelector("#LCSetOffDetailPopUp")).modal("show");
    }

    $scope.LcModel = { LoanAmount: 0, IsLoan: 'true'};
    $scope.SetDetails = function (args) {
        var tempValue = $scope.LcModel.IsLoan;
        $scope.LcModel = Object.assign({}, args.data);
        $scope.LcModel.IsLoan = tempValue;
        if ($scope.LcModel.IsLoan === false) {
            $scope.LcModel.LoanAmount = 0;
        }
        else {
            $scope.Calculate();
            $scope.selectedInvoiceList[$scope.lcIndex].PurchaseLcId = $scope.LcModel.Id;
            $scope.selectedInvoiceList[$scope.lcIndex].LCRef = $scope.LcModel.LCRef;
            $scope.selectedInvoiceList[$scope.lcIndex].LCDate = $scope.LcModel.LCDate;
            $scope.selectedInvoiceList[$scope.lcIndex].OpeningBank = $scope.LcModel.OpeningBank;
            $scope.selectedInvoiceList[$scope.lcIndex].OpeningBankMasterId = $scope.LcModel.OpeningBankMasterId;
            $scope.selectedInvoiceList[$scope.lcIndex].BalanceLCAmount = $scope.LcModel.BalanceLCAmount;
        }
       
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("hide");
    }

    //#endregion

    //#region Save
    $scope.validation = function () {
        $scope.checkPostingDate();
        var tempBankMasterId = $scope.selectedInvoiceList[0].OpeningBankMasterId
        if (tempBankMasterId != null) {
            for (var i = 0; i < $scope.selectedInvoiceList.length; i++) {
                if ($scope.selectedInvoiceList[i].OpeningBankMasterId != tempBankMasterId) {
                    ShowResult("Opening Bank should same!", "failure");
                    return true;
                }
                if ($scope.selectedInvoiceList[i].PurchaseLcId != null) {
                    $scope.TotalAmountAgainstLC = Math.round($filter("sumByKey")($filter("filter")($scope.selectedInvoiceList, { PurchaseLcId: $scope.selectedInvoiceList[i].PurchaseLcId }), "Amount") * 1000 + Number.EPSILON) / 1000;

                    if ($scope.selectedInvoiceList[i].BalanceLCAmount < $scope.TotalAmountAgainstLC) {
                        ShowResult("Setoff Amount Can't Exceed LC Amount where LCRef No " + $scope.selectedInvoiceList[i].LCRef, "failure");
                        return true;
                    }
                }
            }
        }
        return false;
    };
    $scope.Save = function () {
        if (!$scope.validation()) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DataList': $scope.selectedInvoiceList, 'LcData': $scope.LcModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                    $scope.getAutoLoanAvailableList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    //#endregion

    //#region Get SaveDataList

    $scope.SaveDataList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetSaveData",
        }).then(function successCallback(response) {
            $scope.SaveDataList = response.data;
        });
    }
    $scope.getData();

    //#endregion

    //#region Calculation
    $scope.selectedInvoiceList = [];
    $scope.Calculate = function () {
        $scope.LcModel.LoanAmount = 0;
        angular.forEach($scope.AutoLoanAvailableDataList, function (data, i) {
            if (data.isSelected === true) {
                $scope.LcModel.LoanAmount += parseFloat(data.Balance);
            }
        });
        $scope.LcModel.LoanAmount = Math.round(($scope.LcModel.LoanAmount) * 100 + Number.EPSILON) / 100 ;
    }

    
    $scope.AddSelectedInvoice = function () {
        $scope.selectedInvoiceList = [];
        angular.forEach($scope.AutoLoanAvailableDataList, function (data, i) {
            if (data.isSelected === true) {
                $scope.selectedInvoiceList.push(data);
            }
        });
        $scope.LcModel.LoanAmount = Math.round(($scope.LcModel.LoanAmount) * 100 + Number.EPSILON) / 100;
    }
    $scope.checkLoanAmountByBalanceAmount = function (data) {
        if (data.Amount > data.Balance ) {
            data.Amount = data.Balance;
            ShowResult("Loan Amount can't be greater than Balance Amount!!", 'failure');
        }
    }

    $scope.copyLCNo = function () {
        for (var i = 0; i < $scope.selectedInvoiceList.length; i++) {
            $scope.selectedInvoiceList[i].LCRef = $scope.LcModel.LCRef;
            $scope.selectedInvoiceList[i].LCDate = $scope.LcModel.LCDate;
            $scope.selectedInvoiceList[i].OpeningBank = $scope.LcModel.OpeningBank;
            $scope.selectedInvoiceList[i].PurchaseLcId = $scope.LcModel.Id;
            $scope.selectedInvoiceList[i].OpeningBankMasterId = $scope.LcModel.OpeningBankMasterId;
            $scope.selectedInvoiceList[$scope.lcIndex].BalanceLCAmount = $scope.LcModel.BalanceLCAmount;
        }
    }



    $scope.InvoiceTaggedWithLCReportExcel = function () {
        var reportFormat = "Excel";
        try {
            //var url = 'IE/bulletintemplate/GetBulletinTamplateIndexReport?reportFormat=' + reportFormat;
            var url = $scope.path + 'InvoiceTaggedWithLCReportExcelFormat?reportFormat=' + reportFormat + '&FromDate=' + $scope.AutoLoanNew.FromDate + '&ToDate=' + $scope.AutoLoanNew.ToDate + '&DateRange=' + $scope.AutoLoanNew.DateRange;

            $rootScope.report(url);
        } catch (e) {

        }
    };
    $scope.removeSelectedInvoiceRow = function (index, data) {
        $scope.selectedInvoiceList.splice(index, 1);

        $scope.LcModel.LoanAmount = Math.round(($filter("sumByKey")($filter("filter")($scope.selectedInvoiceList), "Balance")) * 100 + Number.EPSILON) / 100;
    };
    //#endregion
    $scope.UntagInvoiceWithLC = function () {
        $http({
            method: "POST",
            url: 'Commercial/InvoiceTaggedWithLC/UntagInvoiceWithLC',
            data: {
                "untageId": $scope.UntageId, "voucherId": $scope.VoucherId, "VoucherNo": $scope.VoucherNo
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.getpurchaseLCListData();
                $scope.untageId = null;
                $scope.VoucherId = null;
                $scope.VoucherNo = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.onClickUnTaggLCPopUp = function (x) {
        var data = x;
        $scope.UntageId = data.Id;
        $scope.VoucherId = data.VoucherId;
        $scope.VoucherNo = data.VoucherNo;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmUntagPopUp')).modal('show');
    };

    $scope.closePartyPopUp_Invoice = function (x) {
        var party = x.data;
        $scope.AutoLoanNew.PartyId = party.Id;
        $scope.AutoLoanNew.PartyCode = party.Code;
        $scope.AutoLoanNew.PartyName = party.UserName;
        $scope.hidePartyPopUp_Invoice();
    };

}






