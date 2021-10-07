'use strict';
PostInvoiceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'factoryService', '$window'];
function PostInvoiceController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, factoryService, $window) {
    $rootScope.title = "Post Invoice";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.masterList = [];
    $scope.path = 'Accounts/PostInvoice/';

    $scope.searchByPostedGRN = "Id"; $scope.searchGRN = "";
    $scope.searchByPostedGRNList = [{ value: 'Id', name: "Id" }, { value: 'PartyName', name: "Vendor" }, { value: 'DocRefNo', name: "DocRef No" }];

    $scope.model = {
        Id: null, InvoiceDate: null, DocRefNo: null, PartyId: null, PartyPlantId: null, CurrencyId: null, ToCurrencyRate: null, Narration: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);
    $scope.voucher = {
        Id: null,
        CompanyId: null,
        EntityId: null,
        PlantId: null,
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
        TaxYearId: null,
        TaxYearPeriodId: null,
        IsExcludingTax: false,
        Amount: null,
        Narration: null,
        Remarks: null,
    };
    $scope.getDataList = function () {
        $http({
            method: 'POST',
            url: 'Accounts/PostInvoice/GetList',
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.masterList = response.data;
        });
    };
    $scope.getDataList();

    $scope.getDetailDataList = function () {
        $http({
            method: 'GET',
            url: 'Accounts/PostInvoice/GetPostInvoiceDetailData?masterId=' + $scope.modelNew.Id,
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.InventoryReceiveDetailList = response.data;
            $scope.GetSavedGRNListForPostInvoice();
        });
    };

    $scope.GetSavedGRNListForPostInvoice = function () {
        $http({
            method: 'GET',
            url: 'Accounts/PostInvoice/GetSavedGRNListForPostInvoice?masterId=' + $scope.modelNew.Id,
            data: { column: $scope.searchByPostedGRN, value: $scope.searchGRN },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.TempList = response.data;
            if ($scope.TempList.length > 0) {
                var uniqueInventoryReceiveId = removeDuplicates($scope.TempList, 'Id');
                var wcInventoryReceiveId = "";
                if (uniqueInventoryReceiveId.length > 0) {
                    wcInventoryReceiveId = "IN(";
                    wcInventoryReceiveId += Array.prototype.map.call(uniqueInventoryReceiveId, function (item) { return "'" + item.Id + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcInventoryReceiveId;
            }
            $scope.GetDetailData($scope.sqlInStatement);
        });
    };

    $scope.Get = function (obj) {
        $scope.model = obj.data;
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.voucher.Id = $scope.modelNew.Id;
        $scope.voucher.CurrencyId = $scope.modelNew.CurrencyId;
        $scope.voucher.DocRefNo = $scope.modelNew.DocRefNo;
        $scope.voucher.PostingDate = $scope.modelNew.InvoiceDate;
        $scope.voucher.DocDate = $scope.modelNew.InvoiceDate;
        $scope.voucher.PartyId = $scope.modelNew.PartyId;
        $scope.voucher.PartyPlantId = $scope.modelNew.PartyPlantId;
        $scope.voucher.Amount = $scope.modelNew.Amount;
        $scope.voucher.ToCurrencyRate = $scope.modelNew.ToCurrencyRate;
        $scope.voucher.CompanyCurrencyRate = $scope.modelNew.ToCurrencyRate;
        $scope.voucher.Currency = $scope.modelNew.Currency;
        $scope.GetSavedGRNListForPostInvoice();
        $scope.getPostableJVList($scope.modelNew.Id, $scope.modelNew.PartyId);
        $scope.paymentTerm();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.setTab2(2);
    };

    $scope.approvedGRNList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/PostInvoice/GetListForInvPayable',
        }).then(function successCallback(response) {
            $scope.approvedGRNList = response.data;
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#GRNpopUp')).modal('show');
    };

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GRNGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.approvedGRNList.length; i++) {
                $scope.approvedGRNList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GRNGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    function MakeData() {
        $scope.TempList = [];
        for (var i = 0; i < $scope.approvedGRNList.length; i++) {
            var getRow = $filter("filter")($scope.TempList, { "TempList": $scope.approvedGRNList[i].Id });
            if (getRow.length == 0) {
                if ($scope.approvedGRNList[i].Active == true) {
                    var ob = {};
                    ob.InventoryReceiveId = $scope.approvedGRNList[i].Id;
                    ob.PartyId = $scope.approvedGRNList[i].PartyId;

                    if (checkExistCustomer($scope.TempList, ob.PartyId)) {
                        if (checkExistList($scope.TempList, ob.InventoryReceiveId) === false) {

                            ob.Active = $scope.approvedGRNList[i].Active;
                            ob.MaterialMaster = $scope.approvedGRNList[i].MaterialMaster;
                            ob.Article = $scope.approvedGRNList[i].Article;

                            $scope.modelNew.PartyId = $scope.approvedGRNList[i].PartyId;
                            $scope.modelNew.PartyPlantId = $scope.approvedGRNList[i].PartyPlantId;
                            $scope.modelNew.CurrencyId = $scope.approvedGRNList[i].CurrencyId;
                            $scope.modelNew.PartyName = $scope.approvedGRNList[i].PartyName;

                            $scope.TempList.push($scope.approvedGRNList[i]);
                            ob = {};
                        }
                    }
                    else {
                        throw 'Select same Vendor.';
                    }
                }

            }
        }
        $scope.GetCurrencyExchangeRate();
    }

    $scope.GetCurrencyExchangeRate = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.modelNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.modelNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    function checkExistCustomer(list, customerId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== customerId) {
                return false;
            }
        }
        return true;
    }

    function checkExistList(list, InventoryReceiveId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === InventoryReceiveId) {
                return true;
            }
        }
        return false;
    }

    $scope.closeGRNPopUp = function () {
        try {
            //var row = $filter('filter')($scope.approvedGRNList, { 'Active': true });
            //if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            //    $scope.TempList = row;
            //}
            MakeData();
            if ($scope.TempList.length > 0) {
                var uniqueInventoryReceiveId = removeDuplicates($scope.TempList, 'Id');
                var wcInventoryReceiveId = "";
                if (uniqueInventoryReceiveId.length > 0) {
                    wcInventoryReceiveId = "IN(";
                    wcInventoryReceiveId += Array.prototype.map.call(uniqueInventoryReceiveId, function (item) { return "'" + item.Id + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcInventoryReceiveId;
            }
            if (!baseService.isUndefinedOrNull($scope.sqlInStatement)) {
                $scope.GetDetailData($scope.sqlInStatement);
                angular.element(document.querySelector('#GRNpopUp')).modal('hide');
            }

        } catch (e) {
            ShowResult(e, 'failure', 'GRNpopUp');
        }
    }

    $scope.InventoryReceiveDetailList = [];
    $scope.GetDetailData = function (inventoryReceiveIds) {
        $http({
            method: 'GET',
            url: 'Accounts/PostInvoice/GetGRNDetailListForPostInvoice?inventoryReceiveId=' + inventoryReceiveIds + '&masterId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.InventoryReceiveDetailList = response.data;
        });
    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.TempList = [];
    $scope.sqlInStatement = null;

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });

    $scope.companyCurrencyId = null;
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    }).then(function successCallback(response) {
        angular.forEach(response.data, function (item, i) {
            if (item.ParallelCurrencyType === 'CompanyCurrency') {
                $scope.companyCurrencyId = item.CurrencyId;
                $scope.modelNew.CompanyCurrencyId = item.CurrencyId;
            }
        });
    });

    $scope.calculateAmount = function (data) {
        try {
            if (data.GRNQty >= data.OtherQty + data.TransactionQty) {
                data.TransactionAmount = parseFloat(data.TransactionQty * data.TransactionRate).toFixed(2);
                data.Balance = data.GRNQty - (data.OtherQty + data.TransactionQty)	
                var gridObj = $("#GRNDetail").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            }
            else {
                throw "Invoice Qty can't greater than GRN Qty.";
            }
            
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplateDetail = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllDetail });
    };

    function CheckBoxSelectAllDetail(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GRNDetail").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.InventoryReceiveDetailList.length; i++) {
                $scope.InventoryReceiveDetailList[i].Activ = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GRNDetail").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.InventoryReceiveDetailLists = [];
    $scope.Action = 'Save';
    $scope.Save = function () {
        try {
            angular.copy($scope.modelNew, $scope.model);
            $scope.$broadcast('show-errors-check-validity');

            if (baseService.arrayLength($scope.InventoryReceiveDetailList)>0) {
                for (var i = 0; i < $scope.InventoryReceiveDetailList.length; i++) {
                    if ($scope.InventoryReceiveDetailList[i].GRNQty < $scope.InventoryReceiveDetailList[i].OtherQty + $scope.InventoryReceiveDetailList[i].TransactionQty) {
                        throw "Invoice Qty can't greater than GRN Qty.";
                    }
                    if ($scope.InventoryReceiveDetailList[i].Activ) {
                        $scope.InventoryReceiveDetailLists.push($scope.InventoryReceiveDetailList[i]);
                    }
                }
            }
            if (baseService.arrayLength($scope.InventoryReceiveDetailLists)==0) {
                throw "Please select GRN Detail data.";
            }
            else {
                for (var i = 0; i < $scope.InventoryReceiveDetailLists.length; i++) {
                    if ($scope.InventoryReceiveDetailLists[i].TransactionQty == 0 || baseService.isUndefinedOrNull($scope.InventoryReceiveDetailLists[i].TransactionQty)) {
                        throw "Please input Qty.";
                    }
                }
            }

            if ($scope.modelNewForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: 'Accounts/PostInvoice/Create',
                        data: {
                            'master': $scope.modelNew, 'dataList': $scope.InventoryReceiveDetailLists
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getDataList();
                            //$scope.paymentTerm();
                            //$scope.setTab2(2);
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.deleteUrl = 'Accounts/PostInvoice/delete?Id=';
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDataList();
                    
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.model = {
            Id: null, InvoiceDate: null, DocRefNo: null, PartyId: null, PartyPlantId: null, CurrencyId: null, ToCurrencyRate: null, Narration: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
        };
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.InventoryReceiveDetailList = [];
        $scope.TempList = [];
        $scope.Action = 'Save';
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
    $scope.postableJVList = [];
    $scope.getPostableJVList = function (id,partyId) {
        $http({
            method: "POST",
            url: "accounts/PostInvoice/GetPostableJVList?id=" + id + '&partyId=' + partyId
        }).then(function successCallback(response) {
            $scope.postableJVList = response.data;
        });
    };

    $scope.selectPaymentList = function () {
        $scope.checkedMultipleVendorpaymentList = [];
        $scope.MultiplepaymentDetailSelectedList = [];
        for (var i = 0; i < $scope.postableList.length; i++) {
            if ($scope.postableList[i].flag === true) {
                $scope.checkedMultipleVendorpaymentList.push($scope.postableList[i]);
                for (var j = 0; j < window.lst.length; j++) {
                    if (window.lst[j].PartyId == $scope.postableList[i].PartyId) {
                        $scope.MultiplepaymentDetailSelectedList.push(window.lst[j]);
                    }
                }
            }
        }
    }
    $scope.getCboVoucherTypePostInvoiceList = function () {
        cboService.getCboVoucherTypePostInvoiceList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
            }
        });
    };
    $scope.getCboVoucherTypePostInvoiceList();

    $scope.SavePost = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.formPost.$valid) {
                $http({
                    method: "POST",
                    url: "accounts/PostInvoice/Postdata",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.postableJVList,
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getDataList();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
        }
    };
    $scope.paymentTerm = function () {

        $scope.paymenttermUrl = "accounts/PaymentTerm/getvendorcbo";
        $http({
            method: "GET",
            url: $scope.paymenttermUrl
        }).then(function successCallback(response) {
            $scope.paymentTermList = response.data;
        });
    };
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
        if (!baseService.isUndefinedOrNull(date)) {
            date = new Date(date);
            date.setDate(date.getDate() + days);
            $scope.voucher.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
        }
    };
    $scope.getMatureDateNew = function (date) {
        if (!baseService.isUndefinedOrNull(date)) {
            $scope.voucher.MatureDate = $filter("date")(date, "dd-MMM-yyyy");
        }
    };
    $scope.onClickReportExcelPosting = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('accounts/PostInvoice/PostInvoiceVoucherReport?reportFormat=' + reportFormat + '&&voucherId=' + data.VoucherId, '_blank');
    };
    $scope.onClickReportPDFPosting = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('accounts/PostInvoice/PostInvoiceVoucherReport?reportFormat=' + reportFormat + '&&voucherId=' + data.VoucherId, '_blank');
    };

}