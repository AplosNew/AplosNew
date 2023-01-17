'use strict';
VoucherGlUpdateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function VoucherGlUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Voucher GL Update';
    $scope.Action = 'Save';
    $scope.path = 'Accounts/VoucherGlUpdate/';
    $scope.url = "Accounts/VoucherGlUpdate";
    $scope.parkUrl = $scope.url + "/parkModeVoucher";
    $scope.saveUrl = $scope.path + 'UpdateVoucherGl';
    $scope.voucher = {
        Id: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        CompanyCurrencyRate: 1,
        Entity: null,
        CurrencyCode: null,
        VoucherType: null,
        SourceType: null,
        Capitalize: null
    };


    $scope.VoucherDataList = [];
    $scope.getVoucherData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "getVoucherDataList",
                data: { voucherNo: $scope.voucher.VoucherNo},
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.VoucherDataList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
   // $scope.getVoucherData();

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId,sourceType) {
        $scope.voucherId = voucherId;
        $scope.sourceType = sourceType;
        $scope.message_confirmation = "Are you sure to Park Mode?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.Clear = function () {
        $scope.Action = "Update";
        $scope.voucher = {};
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherDetailList = [];
    };
    $scope.Get = function (data) {
        if (data.Capitalize === "Yes") {
            ShowResult(data.VoucherNo + " Voucher Already Capitalized, update not allowed!", "failure");
            return;
        }
        $scope.voucher.Id = data.Id;
        $scope.voucher.PostingDate = data.PostingDate;
        $scope.voucher.DocDate = data.DocDate;
        $scope.voucher.DocRefNo = data.DocRefNo;
        $scope.voucher.Narration = data.Narration;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.EntityId = data.EntityId;
        $scope.voucher.Entity = data.Entity;
        $scope.voucher.CurrencyCode = data.CurrencyCode;
        $scope.voucher.VoucherType = data.VoucherType;
        $scope.voucher.SourceType = data.SourceType;
        $scope.voucher.Capitalize = data.Capitalize;
        $scope.GetCurrencyExchangeRateList();
        $scope.currencyDisable = true;
        $scope.Action = "Update";
        
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        $scope.getJournalVoucherDetailList($scope.voucher.Id);
    };
    $scope.getJournalVoucherDetailList = function (id) {
        $http({
            method: "get",
            url: "accounts/VoucherGlUpdate/Data?voucherId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };

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

    $scope.searchglByList = [
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];
    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoName",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.indexGL = "";
    $scope.popUpGL = function (index) {
        $scope.indexGL = index;
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetVendorInvoiceGLBudgetList", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    $scope.setSelected = function (data, index) {
        $scope.voucherDetailList[$scope.indexGL].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.voucherDetailList[$scope.indexGL].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.voucherDetailList[$scope.indexGL].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.voucherDetailList[$scope.indexGL].BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetailList[$scope.indexGL].BudgetName = data.BudgetName;
        $scope.voucherDetailList[$scope.indexGL].ActivityId = data.ActivityId;
        $scope.voucherDetailList[$scope.indexGL].ActivityName = data.ActivityName;
        $scope.closeCOAICodeListPopUp();
    };
    $scope.Save = function () {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    voucherDetailVMList: $scope.voucherDetailList
                },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
      

    };

    $scope.VendorInvoiceReport = function (reportFormat, Id, SourceType, InventoryIssueId, InventoryReceiveId) {
        if (SourceType == 'VendorInvoice') {
            $window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + Id, '_blank');
        }
        else if (SourceType == 'IssueJournal') {
            $window.open('Accounts/InventoryPayable/IssueJournalReport?reportFormat=' + reportFormat + '&inventoryIssueId=' + InventoryIssueId, '_blank');
        }
        else if (SourceType == 'InventoryPayable') {
            $window.open('Accounts/InventoryPayable/PabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + InventoryReceiveId + '&employeeId=null&isReversCharge=false&isFoc=false&otherVendorId=null', '_blank');
        }
        else if (SourceType == 'JournalVoucher') {
            $window.open('Accounts/Voucher/GetJournalVoucherReport?reportFormat=' + reportFormat + '&voucherId=' + Id, '_blank');
        }
        else
            $window.open('Employees/EmployeeReport/GetEmployeePayableExpenseReport?reportFormat=' + reportFormat + '&voucherId=' + Id, '_blank');

    }

};






