"use strict";
deleteAccCutOffDateBackDataController.$inject = ["cboService", "$scope", "$rootScope", "baseService", "$http", "$filter", '$window'];
function deleteAccCutOffDateBackDataController(cboService, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = "Delete Account CutOffDate Back Data";
    $scope.Action = "Save";
    $scope.index = -1;

    $scope.voucher = {
        Id: null,
        PartyId: null,
        PartyName: null,
        PartyType: 'Material',
        CurrencyId: null,
        CompanyCurrencyRate: 1,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        EmployeeTransactionTypeName: null,
        IsPark: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        FAType: null
    };
    $scope.companyGroupList = [];
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    $scope.companyList = [];
    $scope.getCompany = function () {
        cboService.getCboCompanyByCompanyGroup($scope.voucher.CompanyGroupId, function (result) {
            $scope.companyList = result;
        });
    };


    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.voucher.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };


    
    $scope.getCutOffDate = function () {
        $http({
            method: "GET",
            url: "accounts/OpeningBalance/GetCpanelACCCutOffDate?companyGroupId=" + $scope.voucher.CompanyGroupId + "&companyId=" + $scope.voucher.CompanyId
        }).then(function successCallback(response) {
            if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                $scope.voucher.PostingDate = $filter('dateFiltering')(response.data.CutOffDate);
                $scope.getPlantList();
            }
            else {
                ShowResult('Opening Balance Cut Off date not found!', 'failure');
            }
        });
    };
    $scope.voucherDetailList = [];
    $scope.getCutOffBackDateData = function () {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetCutOffBackDateData?companyGroupId=" + $scope.voucher.CompanyGroupId + "&companyId=" + $scope.voucher.CompanyId + "&plantId=" + $scope.voucher.PlantId + "&postingDate=" + $scope.voucher.PostingDate
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
            $scope.getEmployeePayableCutOffAfterPostingDateData();
            $scope.getVendorPayableCutOffAfterPostingDateData();
        });
    };
    $scope.employeePayableDetailList = [];
    $scope.getEmployeePayableCutOffAfterPostingDateData = function () {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetEmployeePayableCutOffAfterPostingDateData?companyGroupId=" + $scope.voucher.CompanyGroupId + "&companyId=" + $scope.voucher.CompanyId + "&plantId=" + $scope.voucher.PlantId + "&postingDate=" + $scope.voucher.PostingDate
        }).then(function successCallback(response) {
            $scope.employeePayableDetailList = response.data;
        });
    };
    $scope.vendorPayableDetailList = [];
    $scope.getVendorPayableCutOffAfterPostingDateData = function () {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetVendorPayableCutOffAfterPostingDateData?companyGroupId=" + $scope.voucher.CompanyGroupId + "&companyId=" + $scope.voucher.CompanyId + "&plantId=" + $scope.voucher.PlantId + "&postingDate=" + $scope.voucher.PostingDate
        }).then(function successCallback(response) {
            $scope.vendorPayableDetailList = response.data;
        });
    };
    $scope.getEntityCboByCostCenter = function (costCenterId) {
        $scope.voucherDetail.CostCenterName = $("#costCenterId option:selected").text();
        $scope.voucherDetail.CostCenterId = costCenterId;
        cboService.getCboEntityByCostCenter(costCenterId, function (result) {
            $scope.costCenterEntityList = result;
        });
    };

    $scope.SelectedCostCenterEntityItem = function (id) {
        $scope.voucherDetail.EntityName = $("#costcenterentityId option:selected").text();
        $scope.voucherDetail.EntityId = id;
    };

   
    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.PartyType= 'Material';
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.CompanyCurrencyRate = 1;
        $scope.voucher.CurrencyId = $scope.selectBaseCurrency();
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyDisable = false;
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
    };


  


    $scope.delete = {};
    $scope.confirmPost = function (index) {
        $scope.voucherId = voucherId;
        $scope.delete = $scope.voucherDetailList[index];
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.post = function (index) {
        $scope.delete = $scope.voucherDetailList[index];
        $scope.delete.PostingDate = $scope.voucher.PostingDate;
        $http({
            method: "POST",
            url: "accounts/OpeningBalance/PostDeleteAccCutOffDateBackData",
            data: {
                "voucherVM": $scope.delete,
            },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getCutOffBackDateData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
    $scope.deleteVendorPayable = function () {
        $http({
            method: "POST",
            url: "accounts/OpeningBalance/DeleteVendorPayableCutOffAfterPostingDateData",
            data: {
                "voucherDetailVM": $scope.vendorPayableDetailList,
            },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getCutOffBackDateData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
    $scope.deleteEmployeePayable = function () {
        $http({
            method: "POST",
            url: "accounts/OpeningBalance/DeleteEmployeePayableCutOffAfterPostingDateData",
            data: {
                "voucherDetailVM": $scope.employeePayableDetailList,
            },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getCutOffBackDateData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.redirectTab = function () {
        if ($scope.tabForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.tabForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.tabForm3.$invalid) {
            $scope.setTab(3);
        }
        else if ($scope.tabForm4.$invalid) {
            $scope.setTab(4);
        }
    };


        /* Mr. Taufiq u do you report from here*/

    $scope.PaybleVSPaymentReportExcel = function (id, reportFormat) {
        debugger;

        //if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
        //    ShowResult('Select From Date', 'failure');
        //    return false;
        //}
    
        var reportFormat = "Excel";
        $window.open('Accounts/OpeningBalance/PaybleVSpaymentReport?reportFormat=' + reportFormat+ "&companyId=" + $scope.voucher.CompanyId + "&plantId=" + $scope.voucher.PlantId + '&fromDate=' + $scope.voucher.PostingDate );//&fromDate=' + $scope.report.FromDate +'& toDate=' + $scope.report.ToDate + ' & Type=' + $scope.productNew.Type, '
      
    };
}