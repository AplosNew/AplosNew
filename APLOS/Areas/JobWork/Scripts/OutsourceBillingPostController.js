'use strict';
OutsourceBillingPostController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'factoryService'];
function OutsourceBillingPostController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, factoryService) {
    $rootScope.title = 'Billing Post';
    $scope.Action = 'Save';
    $scope.ContractList = [];
    $scope.masterList = [];
    $scope.IssueTypeList = [];
    $scope.IndividualReportList = [];
    $scope.GateEntryNoList = [];
    $scope.GateEntryList = [];
    $scope.voucherTypeList = [];
    $scope.TransformationTypeList = [];
    $scope.EntityList = [];
    $scope.MaterialLocationList = [];
    $scope.GriddataMaster = [];
    $scope.path = 'JobWork/OutSourceBillingPost/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'OutSourceBillingPost';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "p.UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];



    $scope.billingNonPostedList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'JobWork/OutSourceBillingPost/GetOutsourcingBillingNonPostData',
        }).then(function successCallback(response) {
            $scope.billingNonPostedList = response.data;
            for (var i = 0; i < $scope.billingNonPostedList.length; i++) {
                response.data[i].InvoiceDate = new Date($scope.billingNonPostedList[i].InvoiceDate);
            }
           
        });
    };
    $scope.popUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#OutSourceBillingpopUp')).modal('show');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#OutSourceBillingpopUp')).modal('hide');
    };

    $scope.outSourceBillingDetailList = [];
    $scope.GetDetailData = function (masterId) {
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkReceiveBilling/GetJWReceiveBillingDetailData?masterId=' + masterId
        }).then(function successCallback(response) {
            $scope.outSourceBillingDetailList = response.data;

        });
    }
    $scope.billingJV = [];

    $scope.GetOutsourcingBillingJV=function (billingId) {
        $http.get('JobWork/OutSourceBillingPost/GetOutsourcingBillingJV?billingId=' + billingId)
            .then(function (response) {
                $scope.billingJV = [];
                $scope.billingJV = response.data;
            });
    }
    $scope.model = {

        Id: null
        , InvoiceDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
        , DocDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
        , PostingDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PartyId: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , CutOffDate: null
        , MaterialStorageId: null
        , CurrencyId: null
        , BaseCurrencyId: null
        , ToCurrencyRate: 0
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , DocRefNo: null
        , DocDate: null

        , EntryDate: null
        , FixedAssetOrInventory: 'Inventory'
        , PODepended: false
        , AlongwithInvoice: true
        , InvoiceNo: null
        , InvoiceDate: null
        , IsNonCreditable: false
        , IsNonVendor: false
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: 'Vendor'
        , Reason: null
        , EmployeeId: null
        , NoteForAccounts: null
        , OrderSpecific: 'No'
        , IsFOC: false
        , ContractId: null
        , OrderSpecific: 'No'
        , PurchaseLCId: null
        , CustomerName: null
        , PaymentMode: null
        , ContractNo: null
        , LCRef: null
        , CheckedByStatusForNoti: null
        , ApprovedByStatusForNoti: null
        , labelCheckAndApproved: null
        , FromPlantId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes'
        , ByWhomId: null
        , GateEntryNo: null
        , EmployeeCode: null
        , ResponsiblePerson: null
        , ByWhomEmployeeId: null
        , TransformationContractId: null
        , VoucherTypeId:null
    };
    $scope.modelNew = Object.assign({}, $scope.model);


    $scope.Get = function (obj) {
        $scope.modelNew = Object.assign({}, obj.data);
        $scope.GetDetailData($scope.modelNew.Id);
        $scope.GetOutsourcingBillingJV($scope.modelNew.Id);
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
        angular.element(document.querySelector('#OutSourceBillingpopUp')).modal('hide');

    }

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.modelNew.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.modelNew.PostingDate) < new Date($scope.modelNew.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.modelNew.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.modelNew.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.modelNew.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    cboService.getCboVoucherTypeOutSourceBillingList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });
    $scope.Post = function () {
       // if (baseService.isUndefinedOrNull($scope.modelNew.EntityId)) return ShowResult('Please Select Entity', 'failure');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'outsourceBillingId': $scope.modelNew.Id
                ,'voucherVM': $scope.modelNew
                , 'voucherDetailVMList': $scope.billingJV
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDataList();
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
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
}