"use strict";
masterOrderSalesAdditionalController.$inject = ["cboService", "commonMessage", '$window', "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService", "bankService"];
function masterOrderSalesAdditionalController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $http, $filter, $controller, accountService, bankService) {
    $rootScope.title = "Master Order Sales";
    $scope.Action = "Save";
    $scope.invoiceList = [];
    $scope.postedSalesList = [];

    $scope.searchByPostedSales = "InvoiceNo"; $scope.searchSales = "";
    $scope.searchByPostedSalesList = [{ value: 'InvoiceNo', name: "Invoice No" }, { value: 'VoucherNo', name: "Voucher No" }, { value: 'PartyCode', name: "Party Code" }, { value: 'PartyName', name: "Party Name" }
        , { value: 'DocRefNo', name: "DocRef No" }
    ];
    $scope.FromDate = null; $scope.ToDate = null;
    $scope.MasterOrderSalesPostedList = [];
    $scope.getMasterOrderSalesPosted = function () {
        $http({
            method: 'POST'
            , url: 'SalesManagements/Sales/GetPostedMasterOrderSalesList'
            , data: { column: $scope.searchByPostedSales, value: $scope.searchSales, 'FromDate': $scope.FromDate, 'ToDate': $scope.ToDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.MasterOrderSalesPostedList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    // $scope.getMasterOrderSalesPosted();

    $scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.SalesId = null;
    $scope.ShowAdditionalPopup = function (obj) {
        //$scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        //$scope.modelNew = Object.assign({}, $scope.model);
        $scope.SalesAdditionalInfoDataList = [];
        $scope.SalesId = obj.data.Id;
        $scope.GetSalesAdditionalInfoData();
        angular.element(document.querySelector('#detailpopup')).modal('show');
    }

    $scope.EditData = function (data) {
        $scope.modelNew = Object.assign({}, data);
    }

    $scope.ClosePopUp = function () {
        $scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelNew = Object.assign({}, $scope.model);
        angular.element(document.querySelector('#detailpopup')).modal('hide');
    }

    $scope.monthList = [
        { 'Value': "1", 'Text': "Jan", 'Days': 31 },
        { 'Value': "2", 'Text': "Feb", 'Days': 28 },
        { 'Value': "3", 'Text': "Mar", 'Days': 31 },
        { 'Value': "4", 'Text': "Apr", 'Days': 30 },
        { 'Value': "5", 'Text': "May", 'Days': 31 },
        { 'Value': "6", 'Text': "Jun", 'Days': 30 },
        { 'Value': "7", 'Text': "Jul", 'Days': 31 },
        { 'Value': "8", 'Text': "Aug", 'Days': 31 },
        { 'Value': "9", 'Text': "Sep", 'Days': 30 },
        { 'Value': "10", 'Text': "Oct", 'Days': 31 },
        { 'Value': "11", 'Text': "Nov", 'Days': 30 },
        { 'Value': "12", 'Text': "Dec", 'Days': 31 }
    ];

    function validatedate(dateText) {

        if (dateText) {
            try {
                var errorMessage = "";
                var monthNO = 0;
                var daysPerMonth = 0;
                var splitComponents = dateText.split('-');
                if (splitComponents.length > 0) {
                    var day = parseInt(splitComponents[0]);
                    var month = splitComponents[1];
                    var year = parseInt(splitComponents[2]);

                    if (isNaN(day) || isNaN(year)) {
                        errorMessage = "Please enter the date in dd-MMM-yyyy format.";
                        throw errorMessage;
                        return false;
                    }

                    var monthName = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                    if (monthName.includes(month)) {
                        for (var i = 0; i < $scope.monthList.length; i++) {
                            if ($scope.monthList[i].Text == month) {
                                monthNO = $scope.monthList[i].Value;
                                daysPerMonth = $scope.monthList[i].Days;
                                break;
                            }
                        }
                    }
                    else {
                        throw "Invalid Month Name.";
                    }

                    if (day <= 0 || year <= 0) {
                        throw "The day and year need to be positive values greater than 0";
                    }

                    if (errorMessage == "") {
                        // assuming no leap year by default
                        //var daysPerMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
                        if (year % 4 == 0) {
                            // current year is a leap year
                            daysPerMonth = 29;
                        }

                        if (day > daysPerMonth) {
                            errorMessage = "Number of days are more than those allowed for the month";
                        }
                    }
                } else {
                    throw errorMessage = "Please enter the date in dd-MMM-yyyy format.";
                }

                if (errorMessage) {
                    throw errorMessage;
                    return false;
                }
            } catch (e) {
                throw e;
                return false;
            }
        }

        return true;
    }

    $scope.Action = "Save";
    $scope.Save = function () {
        try {
            for (var i = 0; i < $scope.SalesAdditionalInfoDataList.length; i++) {
                if ($scope.SalesAdditionalInfoDataList[i].Flag) {
                    if (baseService.isUndefinedOrNull($scope.SalesAdditionalInfoDataList[i].Value)) {
                        throw "Value is required for " + $scope.SalesAdditionalInfoDataList[i].UserName + ".";
                    }
                }

                if ($scope.SalesAdditionalInfoDataList[i].CharecterType == "DateTime") {
                    validatedate($scope.SalesAdditionalInfoDataList[i].Value);
                }


                if ($scope.SalesAdditionalInfoDataList[i].CharecterType == "Decimal") {
                    if (isNaN($scope.SalesAdditionalInfoDataList[i].Value)) {
                        throw "Number is required for " + $scope.SalesAdditionalInfoDataList[i].UserName + ".";
                    }
                }
            }

            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'SalesManagements/Sales/CreateSalesAdditionalInfo',
                    data: {
                        'data': $scope.SalesAdditionalInfoDataList,
                        'salesId': $scope.SalesId
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure", 'detailpopup');
        }
    };

    $scope.SalesAdditionalInfoDataList = [];
    $scope.GetSalesAdditionalInfoData = function () {
        $scope.SalesAdditionalInfoDataList = [];
        $http.get("SalesManagements/Sales/GetSalesAdditionalInfoData?salesId=" + $scope.SalesId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        for (var i = 0; i < response.data.length; i++) {
                            response.data[i].SalesId = $scope.SalesId;

                            if (response.data[i].CharecterType == "Text" || response.data[i].CharecterType == "DateTime") {
                                response.data[i].CharType = "text";
                            }
                            else {
                                response.data[i].CharType = "number";
                            }
                            if (response.data[i].CharecterType == "DateTime") {
                                response.data[i].datepic = 'datepicker';
                            }
                        }

                        $scope.SalesAdditionalInfoDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.Clear = function () {
        $scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.modelNew = Object.assign({}, $scope.model);
    }



    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.GetInvoiceReport = function () {
        var reportFormat = "Excel";
        var dataList = [];
        var g = $("#GridPost").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.MasterOrderSalesPostedList;
        }

        if (dataList.length > 0) {
            var wcId = "";
            if (dataList.length > 0) {
                wcId = "IN(";
                wcId += Array.prototype.map.call(dataList, function (item) { return "'" + item.InvoiceNo + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcId;
        }

        $scope.fileName = 'Invoice Report.xls';
        $scope.ReportFormat = 'Excel';
        //var url = 'SalesManagements/Sales/GetInvoiceReport?reportFormat=' + $scope.ReportFormat + '&Ids=' + $scope.sqlInStatement;
        //$rootScope.report(url);
        $http({
            method: "POST",
            url: 'SalesManagements/Sales/GetInvoiceReport',
            data: {
                'reportFormat': reportFormat,
                'Ids': $scope.sqlInStatement
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

}