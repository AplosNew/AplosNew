'use strict';
multipleVendorPaymentController.$inject = ['cboService', 'commonMessage', '$scope', '$window', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function multipleVendorPaymentController(cboService, commonMessage, $scope, $window, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Multiple Vendor Payment';
    $scope.Action = 'Submit';
    $scope.index = -1;
    $scope.voucherList = [];
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.currencyExchangeRate = [];
    $scope.partyFromTo = 'From';
    $scope.bankFromTo = 'To';
    $scope.isWriteOff = true;
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.isBankAmount = false;
    $scope.downloadgriddataUrlPath = 'accounts/invoice/DownloadUsingFullPath';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    $scope.multiplePaymentDataList = [];
    $scope.multipleVendorpaymentList = [];
    $scope.multipleVendorpaymentDetailList = [];
    $scope.MultiplepaymentDetailSelectedList = [];

    $scope.getData = function () {
        $http({
            method: "GET",
            url: "accounts/invoice/GetMultiplePaymentData"
        }).then(function successCallback(response) {
            $scope.multiplePaymentDataList = response.data;
        });
    };
    $scope.getData();
    $scope.getParkData = function (id) {
        $http({
            method: "GET",
            url: "accounts/invoice/GetMultiplePaymentParkList?id="+id
        }).then(function successCallback(response) {
            $scope.multipleVendorpaymentList = response.data;
            //for (var i = 0; i < $scope.multipleVendorpaymentList.length; i++) {
            //    response.data[i].DueUpToDate = new Date($scope.multipleVendorpaymentList[i].DueUpToDate);
            //    response.data[i].TentativeDate = new Date($scope.multipleVendorpaymentList[i].TentativeDate);
            //}
            $scope.getDetailData(id);
        });
    };
  
    $scope.lst = [];
    $scope.getDetailData = function (id) {
        //debugger
        $http({
            method: 'GET',
            url: "accounts/invoice/GetMultipleVendorAvailableDetailList?multiplePaymentId="+id
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;

        });
    }
    
    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["PartyId"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("PartyId", "equal", filteredData, true).take(1000));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data,
            columns: [{ field: "VoucherNo", headerText: "VoucherNo", width: 50 },
            { field: "PartyName", headerText: "PartyName", width: 150 },
            { field: "PostingDate", headerText: "PostingDate", width: 50 },
            { field: "DocDate", headerText: "DocDate", width: 150 },
            { field: "DocRefNo", headerText: "DocRefNo", width: 150 },
            { field: "CurrencyCode", headerText: "CurrencyCode", width: 50 },
            { field: "Amount", headerText: "Amount", width: 50 },
            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    
    $scope.selectPaymentList = function () {
        $scope.checkedMultipleVendorpaymentList = [];
        $scope.MultiplepaymentDetailSelectedList = [];
        for (var i = 0; i < $scope.multipleVendorpaymentList.length; i++) {
            if ($scope.multipleVendorpaymentList[i].flag === true) {
                $scope.checkedMultipleVendorpaymentList.push($scope.multipleVendorpaymentList[i]);
                for (var j = 0; j < window.lst.length; j++) {
                    if (window.lst[j].PartyId == $scope.multipleVendorpaymentList[i].PartyId) {
                        $scope.MultiplepaymentDetailSelectedList.push(window.lst[j]);
                    }
                }
            }
        }
    }


    $scope.Get = function () {
        var gridObj = $("#MultiplePaymentGrid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.masterId = data.Id;
        $scope.getParkData(data.Id);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.setTab2(2);
    }
    

    $scope.multiplePayment = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EntityId: null,
        PlantId: null,
        CurrencyId: null,
        SourceType: "Vendor",
        ApprovalStatus: "Pending",
        ApprovedBy: null,
        ApprovedDate: $filter('dateFiltering')(Date.now()),
        DueUpToDate: $filter('dateFiltering')(Date.now()),
        TentativeDate: $filter('dateFiltering')(Date.now()),
        BankMasterId: null,
        IsFifo: false,
        ApprovedBy:null
    };
    $scope.multiplePaymentDetail = [];

    $scope.tempList = [];
    $scope.paymentSelectedList = [];
    $scope.multiplevendorInvoiceSearchList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Vendor Code",
            "value": "PartyCode"
        },
        {
            "name": "Vendor Name",
            "value": "PartyName"
        },
        {
            "name": "Entity",
            "value": "EntityName"
        },
        {
            "name": "Posting Date",
            "value": "PostingDate"
        },
        {
            "name": "Doc Date",
            "value": "DocDate"
        },
        {
            "name": "Doc Ref",
            "value": "DocRefNo"
        }
    ];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'PostingDate',
        searchBy: 'VoucherNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    function avoidCheckList(id) {
        for (var i = 0; i < $scope.paymentSelectedList.length; i++) {
            if ($scope.paymentSelectedList[i].InvoiceDetailId === id) {
                return true;
                break;
            }
        }
        return false;
    }

    
    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
     
    $scope.sqlInStatement = null;
    $scope.getPopupVendorPayableList = function () {
        if ($scope.partyDataListNew.length > 0) {
            var uniquePartyId = removeDuplicates($scope.partyDataListNew, 'PartyId');
            var wcEmpCode = "";
            if (uniquePartyId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniquePartyId, function (item) { return "'" + item.PartyId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }

        $scope.tempList = [];
        $scope.customerreceivableGLData = function (pageno) { 
            $scope.customerReceivableGLUrl1 = 'accounts/Invoice/GetMultipleVendorAvailableInvoiceList?doctate=' + $filter("date")($scope.multiplePayment.DueUpToDate, "dd-MMM-yyyy") + '&docType=' + $scope.multiplePayment.DateType + '&entityId=' + $scope.multiplePayment.EntityId + '&partyId=' + $scope.sqlInStatement;
            baseService.paginationBase($scope.customerReceivableGLUrl1, pageno, $scope.popUpParameters)
                .then(function (result) {
                    try {
                        $scope.paymentList = [];
                        angular.forEach(result.Rows, function (item) {
                            if (avoidCheckList(item.InvoiceDetailId) === false) {
                                $scope.paymentList.push(item);
                            }
                        })
                        $scope.popUpParameters.total_count = result.Total;
                        for (var i = 0; i < $scope.paymentList.length; i++) {
                            $scope.paymentList[i].Active = getActive($scope.tempList, $scope.paymentList[i].InvoiceDetailId);
                        }
                    } catch (e) {
                        ShowResult(e, 'Error');
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#VendorPayableListPopUP')).modal('show');
        $scope.customerreceivableGLData();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#VendorPayableListPopUP')).modal('hide');
    };
    $scope.changeDateType = function (type) {
        $scope.multiplePayment.DateType = type;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceDetailId === id) {
                return true;
            }
        }

        return false;
    }
    $scope.pushInTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.InvoiceDetailId) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].InvoiceDetailId === data.InvoiceDetailId) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.tempList); t++) {
                    if ($scope.tempList[t].InvoiceDetailId === data.InvoiceDetailId) {
                        $scope.tempList.splice(t, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempList(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].InvoiceDetailId === id) {
                return true;
            }
        }
        return false;
    }

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.closePopUp = function () {
        if (baseService.arrayLength($scope.tempList) > 0) {
            angular.forEach($scope.tempList, function (item) {
                $scope.paymentSelectedList.push({
                    InvoiceDetailId: item.InvoiceDetailId
                    , InvoiceId: item.InvoiceId
                    , VoucherNo: item.VoucherNo
                    , PostingDate: item.PostingDate
                    , DocDate: item.DocDate
                    , DocRefNo: item.DocRefNo
                    , CurrencyCode: item.CurrencyCode
                    , Receivable: item.Receivable
                    , Received: item.Received
                    , Balance: item.Balance
                    , Amount: item.Amount
                    , PartyName: item.PartyName
                    , PartyId: item.PartyId
                    , PartyPlantId: item.PartyPlantId
                    , BaseOnDueDate: item.BaseOnDueDate
                });
            });
        }
        angular.element(document.querySelector('#VendorPayableListPopUP')).modal('hide');
    };

    $scope.removeRow = function (index, data) {
        var row = $scope.paymentSelectedList[index];
        var drc = $scope.tempList.length;
        while (drc--) {
            if ($scope.tempList[drc]['InvoiceDetailId'] === row.InvoiceDetailId) {
                $scope.tempList.splice(drc, 1);
            }
        }
        $scope.paymentSelectedList.splice(index, 1);
    }

    $scope.pushMultiplePaymentDetail = function () {
        for (var i = 0; i < $scope.paymentList.length; i++) {
            if ($scope.paymentList[i].Active === true) {
                var getRow = $filter('filter')($scope.multiplePaymentDetail, { 'InvoiceDetail': $scope.paymentList[i].InvoiceDetail });
                if (getRow.length === 0) {
                    $scope.multiplePaymentDetail.push($scope.paymentList[i]);
                }
            }
        }
    }
    $scope.Clear = function () {
        var voucherTypeId = $scope.voucher.VoucherId;
        $scope.Action = 'Submit';
        $scope.tempList = [];
        $scope.paymentSelectedList = [];
    };
   
    $scope.validation = function () {
          if ($scope.paymentSelectedList.length > 0) {
              for (var i = 0; i < $scope.paymentSelectedList.length; i++) {
                  if ($scope.paymentSelectedList[i].Amount == 0 || baseService.isUndefinedOrNull($scope.paymentSelectedList[i].Amount)) {
                      ShowResult("Please input Amount where voucherNo is " + $scope.paymentSelectedList[i].VoucherNo, "failure");
                      return true;
                  }
                $scope.partyPlantCheck($scope.paymentSelectedList[i].PartyId, $scope.paymentSelectedList[i].PartyPlantId)

            }
        }
        if (baseService.isUndefinedOrNull($scope.multiplePayment.BankMasterId)) {
            ShowResult("Please select Bank", "failure");
            return true;
        }
        return false;
    };
    $scope.partyPlantCheck = function (partyId,partyPlantId) {
        for (var j = 0; j < $scope.paymentSelectedList.length; j++) {
            if ($scope.paymentSelectedList[j].PartyId == partyId) {
                if ($scope.paymentSelectedList[j].PartyPlantId == partyPlantId) {
                    return false;
                }
                else {
                    ShowResult("Diffrent PartyPlant are not allowed", "failure");
                    return true;
                }
            }
        }
    }
   
    $scope.Save = function () {
        // $scope.pushMultiplePaymentDetail();
        if ($scope.form1.$valid && !$scope.validation() && $scope.paymentSelectedList.length > 0) {
            if ($scope.Action === 'Submit') {
                $http({
                    method: 'POST',
                    url: 'accounts/Invoice/InsertMultipleVendorPayment',
                    data: {
                        'multiplePayment': $scope.multiplePayment,
                        'multiplePaymentDetailList': $scope.paymentSelectedList,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getParkData(response.data.Message);
                        $scope.Clear();
                        $scope.setTab2(2);
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            return true;
        }
    };
    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.multiplePayment.AccountTitle = bank.AccountTitle;
                $scope.multiplePayment.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.multiplePayment.BankMasterId = bank.BankMasterId;
                $scope.multiplePayment.BankCurrencyId = bank.CurrencyId;
                $scope.multiplePayment.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.multiplePayment.GLGeneralInfoName = bank.GLGeneralInfoName;
                $scope.multiplePayment.BudgetMasterId = bank.BudgetMasterId;
                $scope.multiplePayment.BudgetName = bank.BudgetName;
                $scope.multiplePayment.ActivityId = bank.ActivityId;
                $scope.multiplePayment.ActivityName = bank.ActivityName;
                $scope.checkBankAmount();
            }
        }
        $scope.hideBankPopUp();
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.multiplePayment.BankCurrencyId)) {
            if ($scope.multiplePayment.BankCurrencyId !== $scope.companyCurrencyId) {
                $scope.isBankAmount = true;
                $scope.multiplePayment.BankAmount = 0;
            }
            else {
                $scope.isBankAmount = false;
                $scope.multiplePayment.BankAmount = 0;
            }
        }
    };
    $scope.copyPayableBalanceAmount = function () {
        for (var i = 0; i < $scope.paymentSelectedList.length; i++) {
            $scope.paymentSelectedList[i].Amount = $scope.paymentSelectedList[i].Balance;
        }
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.voucher = {
        Id: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        PaymentTermId: null,
        SourceType: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        IsSplit: false,
        Amount: 0,
        Narration: null,
        Remarks: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        SourceFrom: null,
        SourceTo: null,
        PaymentSource: "Bank",

        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,

        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,

        InvoiceAmount: 0,
        ExGainLossAmount: 0,
        NetInvoiceAmount: 0,
        InvoiceGroupAmount: 0,
        ExGainLossGroupAmount: 0,
        CompanyCurrencyRate: 1,
        RoundingType: null,
        ExchangeAmount: null,
        ExchangeType: null,
        DiscountAmount: null
    };
    $scope.GetCurrencyParallel = function () {
        $http({
            method: "GET",
            url: "currencies/CompanyParallelCurrency/CurrencyParallel"
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
            if ($scope.CurrencyParallel.length === 0) {
                $scope.pop("error", "Company Parallel Currency is not set!");
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
        });
    };
    $scope.GetCurrencyParallel();

    $scope.tranCurrencyList = [];
    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.VoucherDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.VoucherDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                $scope.exchangeGainLossCal($scope.voucher.CompanyCurrencyRate);
                //$scope.rateChangeBankCharge($scope.voucher.CompanyCurrencyRate);
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
    };
   
    $scope.Post = function () {
        // $scope.pushMultiplePaymentDetail();
        if ($scope.formPost.$valid && $scope.checkedMultipleVendorpaymentList.length > 0) {
                $http({
                    method: 'POST',
                    url: 'accounts/Invoice/PostMultipleVendorPayment',
                    data: {
                        'voucherVM': $scope.voucher,
                        'multiplePaymentlist': $scope.checkedMultipleVendorpaymentList,
                        'multiplePaymentDetailList': $scope.MultiplepaymentDetailSelectedList,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.postClear();
                        
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
        }
    };
    $scope.postClear = function () {
        $scope.voucher = {};
        $scope.voucher.VoucherDate = $filter("dateFiltering")(Date.now());
        $scope.voucher.PaymentSource= "Bank";
        $scope.checkedMultipleVendorpaymentList = [];
        $scope.MultiplepaymentDetailSelectedList = [];
    };

    $scope.exchangeGainLossCal = function (rate) {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].CompanyCurrencyRate < rate) {
                $scope.voucherDetailList[i].ExchangeAmount = $scope.voucherDetailList[i].Amount * (rate - $scope.voucherDetailList[i].CompanyCurrencyRate);
                $scope.voucherDetailList[i].ExchangeType = "ExchangeLoss";
            }
            else if ($scope.voucherDetailList[i].CompanyCurrencyRate > rate) {
                $scope.voucherDetailList[i].ExchangeAmount = $scope.voucherDetailList[i].Amount * ($scope.voucherDetailList[i].CompanyCurrencyRate - rate);
                $scope.voucherDetailList[i].ExchangeType = "ExchangeGain";
            }
            else {
                $scope.voucherDetailList[i].ExchangeAmount = 0;
                $scope.voucherDetailList[i].ExchangeType = null;
            }
        }
    };
    $scope.getCboVoucherTypePaymentList = function () {
        cboService.getCboVoucherTypePaymentList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                //$scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                //$scope.voucher.DocDate = $scope.voucher.PostingDate;
               // $scope.getTaxCodeByTaxYear($scope.voucher.PostingDate);

            }
        });
    };
    $scope.getCboVoucherTypePaymentList();

    $scope.delete = function () {
        if ($scope.checkedMultipleVendorpaymentList.length > 0) {
            $http({
                method: 'POST',
                url: 'accounts/Invoice/DeleteMultipleVendorRow',
                data: {
                    'multiplePaymentlist': $scope.checkedMultipleVendorpaymentList,
                    'multiplePaymentDetailList': $scope.MultiplepaymentDetailSelectedList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getParkData($scope.masterId);
                    $scope.getData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

    $scope.confirmDelete = function () {
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.Report = function () {
        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('accounts/Invoice/GetMultiVendorPaymentReport?reportFormat=' + reportFormat + '&mpdId=' + $scope.masterId);
    };


    //$scope.reportFormat = "Excel";
    //$scope.Report = function () {
    //    try {
    //        $scope.fileName = "Multi Vendor Payment.xlsx";
    //        $http({
    //            method: 'POST',
    //            url:'accounts/Invoice/GetMultiVendorPaymentReport',
    //            data: { 'reportFormat': $scope.reportFormat, 'mpdId': $scope.masterId },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == false) {
    //                //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
    //                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        };
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }

    //}


    //Vendor Section Start

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
     
    $scope.partyList = [];
    $scope.getPopupVendorList = function () {
        if ($scope.partyType === 'Vendor') {
            //$scope.partyUrl = 'accounts/Invoice/GetMultipleVendorList?partyType=' + $scope.partyType;
            $scope.partyUrl = 'accounts/Invoice/GetMultipleVendorList?docdate=' + $filter("date")($scope.multiplePayment.DueUpToDate, "dd-MMM-yyyy") + '&docType=' + $scope.multiplePayment.DateType + '&entityId=' + $scope.multiplePayment.EntityId;

        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };

    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
    };

    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
     
    $scope.partyDataListNew = [];
    $scope.ViewParty = function () {
        try { 
            for (var i = 0; i < $scope.partyList.length; i++) {
                if ($scope.partyList[i].CheckBoxSelect == true) {
                    if (checkDoublePartyInformation($scope.partyDataListNew, $scope.partyList[i].PartyId) === false) {
                        $scope.partyDataListNew.push($scope.partyList[i]);
                    }
                }
            }
            angular.element(document.querySelector('#partyPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoublePartyInformation(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId === Id) {
                return true;
            }
        }
        return false;
    }

    //Vendor Section End

    $scope.approvedByList = [];
    $scope.GetapprovedByListCboList = function () {
        $http({
            method: 'GET',
            url: 'Accounts/Invoice/GetMultipleVendorPaymentApproveByCboList'
        }).then(function successCallback(response) {
            $scope.approvedByList = response.data;
        });
    }
    $scope.GetapprovedByListCboList();

}