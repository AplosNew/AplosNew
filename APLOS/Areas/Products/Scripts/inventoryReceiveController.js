'use strict';
inventoryReceiveController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function inventoryReceiveController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Inventory Receive";
    $scope.Action = 'Save';
    $scope.Action1 = 'Update';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/InventoryReceive/';
    $scope.getListUrl = $scope.path + 'getList';
    $scope.getListUrlEmpGRN = $scope.path + 'GetListEmpGrn';

    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';


    $scope.saveUrlEMPGRN = $scope.path + 'createEMPGRN';
    $scope.updateUrlEMPGRN = $scope.path + 'editEMPGRN';
    $scope.deleteUrlEMPGRN = $scope.path + 'deleteEMPGRN/';



    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceUpdateUrl = $scope.path + 'ServiceChargesUpdate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.PurchaseOrderFileLocation = virtualPath.GRN;
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.updateUrl1 = $scope.path + 'UpdareGRN';
    $scope.updateUrlForSRValue = $scope.path + 'UpdateShortageRejectionValue';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    //, CAST(GRNDate AS DATE)
    //#region notification setting
    $scope.chargesList = [];
    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/NotificationSetting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            $scope.GetCheckedByAndApprovedBy1();
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be approved by';
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            //else {
            //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
            //}

        });
    }
    $scope.NotificationSettingStatus();
    $scope.GetCheckedByAndApprovedBy1 = function () {
        //debugger;

        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/InventoryReceive/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }




    //#endregion


    $scope.AllTabPrint = function (z) {

        try {
            //debugger;
            var x = "#" + z;
            var gridObj = $(x).data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];
            location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;
        }
        catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };
    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.productNew.EmployeeId = employee.SystemId;
            $scope.productNew.EmployeeCode = employee.EmployeeCode;
            $scope.productNew.EmployeeName = employee.EmployeeName;
            $scope.productNew.GateEntryNo = "";
        }
        $scope.hideEmployeePopUp();
    };
    $scope.uom = function () {
        //debugger;
        cboService.getUoMCbo(function (response) {
            $scope.uoMList = response;

        });
    }
    $scope.CheckedStatus = function () {
        ////debugger;
        if ($scope.productNew.IsNonVendor === true) {
            $scope.productNew.IsNonVendor = true;
            $scope.productNew.PartyName = "";
            $scope.productNew.PartyCode = "";
        }
        else {
            $scope.productNew.IsNonVendor = false;
        }
    }
    $scope.uom();
    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, "DESC", 'Id', 'PartyName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.products = [];
                    $scope.products = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    $scope.getDataList();
    $scope.productsEmpGRN = [];
    $scope.getDataListForEmpGRN = function () {
        baseService.init($scope.getListUrlEmpGRN, null, null, "DESC", 'Id', 'PartyName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.productsEmpGRN = [];
                    $scope.productsEmpGRN = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.TrancastionTypeCboList = [];
    function GetTrancastionTypeCboList() {
        $http({
            method: 'GET',
            url: 'Productions/SalesPurchaseTransactionType/GetPurchaseTypeCbo'
        }).then(function (response) {
            $scope.TrancastionTypeCboList = response.data;
        });
    }
    GetTrancastionTypeCboList();
    //$scope.getDataListForEmpGRN(); // Command by shakawat Hossain 8-20-2020

    //#region Start -Rejion---GetListGRN---GRN-without-PO UI






    $scope.GetListGRN = [];
    $scope.GetGRN = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetListGRN'
        }).then(function successCallback(response) {
            $scope.GetListGRN = response.data;
            for (var i = 0; i < $scope.GetListGRN.length; i++) {
                response.data[i].GRNDate = new Date($scope.GetListGRN[i].GRNDate);
            }
        });
    }



    $scope.GetListEmployeePurchase = [];
    $scope.GetEmployeePurchase = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetListEmployeePurchase'
        }).then(function successCallback(response) {
            $scope.GetListEmployeePurchase = response.data;
            for (var i = 0; i < $scope.GetListEmployeePurchase.length; i++) {
                response.data[i].GRNDate = new Date($scope.GetListEmployeePurchase[i].GRNDate);
            }
        });
    }



    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.partyUrl = "";
    $scope.showPartyByGateEntryPopUpNew = function () {
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor' || $scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataByGateEntryListNew?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataByGateEntryListNew';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataByGateEntryListNew';
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        //}
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };





    $scope.GetListNotApproveChecked = [];
    $scope.GetNotApproveChecked = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/NotApproveChecked'
        }).then(function successCallback(response) {
            $scope.GetListNotApproveChecked = response.data;
        });
    }
    $scope.GetNotApproveChecked();


    $scope.CheckedHoldReject = [];
    $scope.GRNCheckedHoldReject = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/CheckedHoldReject'
        }).then(function successCallback(response) {
            $scope.CheckedHoldReject = response.data;
        });
    }
    //$scope.GRNCheckedHoldReject();

    $scope.ApprovedHoldChecked = [];
    $scope.GRNApprovedHoldChecked = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/ApprovedHoldChecked'
        }).then(function successCallback(response) {
            $scope.ApprovedHoldChecked = response.data;
        });
    }
    $scope.GRNApprovedHoldChecked();






    $scope.GetListApprovedNotPost = [];
    $scope.GRNGetApprovedNotPost = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/ApprovedNotPost'
        }).then(function successCallback(response) {
            $scope.GetListApprovedNotPost = response.data;
        });
    }
    $scope.GRNGetApprovedNotPost();

    $scope.GetListPosted = [];
    $scope.GetPosted = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/Posted'
        }).then(function successCallback(response) {
            $scope.GetListPosted = response.data;
        });
    }
    $scope.GetPosted();

    //#endregion ---GetListGRN

    //#region Index --- GRid of-- Employee-GRN UI


    $scope.GetListEmployeeNotApproveChecked = [];
    $scope.GetEmployeeNotApproveChecked = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetListEmpNotApproveChecked'
        }).then(function successCallback(response) {
            $scope.GetListEmployeeNotApproveChecked = response.data;
        });
    }
    $scope.GetEmployeeNotApproveChecked();

    $scope.GetListEmpCheckedHoldReject = [];
    $scope.EmpGetListEmpCheckedHoldReject = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetListEmpCheckedHoldReject'
        }).then(function successCallback(response) {
            $scope.GetListEmpCheckedHoldReject = response.data;
        });
    }
    $scope.EmpGetListEmpCheckedHoldReject();




    $scope.GetListEmpApprovedHoldReject = [];
    $scope.EMPGetListEmpApprovedHoldReject = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetListEmpApprovedHoldReject'
        }).then(function successCallback(response) {
            $scope.GetListEmpApprovedHoldReject = response.data;
        });
    }
    $scope.EMPGetListEmpApprovedHoldReject();

    $scope.GetListEmployeeApprovedNotPost = [];
    $scope.GetEmployeeApprovedNotPost = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetListEmpApprovedNotPost'
        }).then(function successCallback(response) {
            $scope.GetListEmployeeApprovedNotPost = response.data;
        });
    }
    $scope.GetEmployeeApprovedNotPost();


    $scope.GetListEmployeePosted = [];
    $scope.EMPGetEmployeePosted = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetListEmpPosted'
        }).then(function successCallback(response) {
            $scope.GetListEmployeePosted = response.data;
        });
    }
    $scope.EMPGetEmployeePosted();

    //#endregion  GRid of-- Employee-GRN UI

    $scope.storageList = [];
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    $scope.currencyList = [];
    $scope.product = {
        Id: null
        , GRNDate: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: $window.plantId
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
        , BaseCurrencyId: $scope.baseCurrencyId
        , ToCurrencyRate: 0
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , DocRefNo: null
        , DocDate: null
        , GateEntryNo: null
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
        , PartyType: $scope.partyType
        , Reason: null
        , EmployeeId: null
        , NoteForAccounts: null
        , OrderSpecific: 'No'
        , IsFOC: false
        , ContractId: null
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
        , IsTradingPO: false
        , AlternativeQty: 0


    };
    $scope.productNew = Object.assign({}, $scope.product);
    $scope.productNew.TaxOptionAddiTax = 'Yes';
    $scope.productDocMap = {
        Id: null
        , CompanyGroupId: null
        , FileName: null
        , UserFilename: null
        , SystemFileName: null
        , Description: null
        , Remarks: null
    };
    $scope.advanceTax = {
        TaxCodeId: null,
        Text: null,
        TaxAmount: null,
        ValueOfFixed: null,
        CompanyCurrencyAmount: null,
        Type: null,
        TotalSumAfterTCSVal: null,
        TaxCategoryId: null

    };
    $scope.Load = function () {
        var bla = $('#GRNIdLoad').val();

        if (bla === 'GRN') {
            //debugger;
            $scope.GetGRN();
        }
        else {
            $scope.GetEmployeePurchase();

        }
    }
    $scope.Load();

    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
    }).then(function successCallback(response) {
        $scope.baseCurrencyId = response.data[0].Value;
        $scope.productNew.BaseCurrencyId = response.data[0].Value;
        factoryService.getCurrencyPrecision($scope.baseCurrencyId);
    });

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = result;
    });
    //$http.get('Products/InventoryReceive/GetACCCutOffDate')
    //	.then(function (response) {
    //		if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
    //			$scope.productNew.CutOffDate = response.data.CutOffDate;
    //			$('#cutOffDate').datepicker('setStartDate', new Date($scope.productNew.CutOffDate));
    //		}
    //		else
    //			ShowResult('Cut Off date not found!', 'failure');
    //	});

    $http.get('accounts/OpeningBalance/GetACCCutOffDate')
        .then(function (response) {
            if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                $scope.productNew.CutOffDate = response.data.CutOffDate;
                $('#cutOffDate').datepicker('setStartDate', new Date($scope.productNew.CutOffDate));
            }
            else
                ShowResult('Cut Off date not found!', 'failure');
        });

    $scope.searchByList = [
        {
            value: 'PartyCode'
            , name: 'Vendor Code'
        },
        {
            value: 'PartyName'
            , name: 'Vendor Name'
        },
        {
            value: 'PartyAccountGroupName'
            , name: 'Account Group'
        },
        {
            value: 'Id'
            , name: 'GRN No'
        },
        {
            value: 'GRNDate'
            , name: 'GRN Date'
        },
        {
            value: 'DocRefNo'
            , name: 'Vendor DocRefNo'
        },
        {
            value: 'InvoiceNo'
            , name: 'Invoice No'
        },
        {
            value: 'InvoiceDate'
            , name: 'Invoice Date'
        }
    ];

    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'UserName'
        },
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'State',
            'value': 'StateName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];
    $scope.Get = function ($event) {
        //debugger;
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        var x = $event;
        var Id = x.data.Id;
        //alert('Id'+Id);
        $scope.productNew = x.data;
        $scope.productNew.GRNDate = x.data.GRNDate1;
        $scope.productNew.Id = x.data.Id;
        $scope.index = Id;
        //$scope.product = $scope.products[$scope.index];
        //$scope.product = $scope.productsEmpGRN[$scope.index];
        //$scope.productNew = Object.assign({}, $scope.product);
        getPartyPlantList();
        if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = x.data.CheckedById;
        }

        $scope.GetCheckedByAndApprovedBy1();


        if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.ApprovedById;
            $scope.productNew.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

            $scope.productNew.CheckedBy = x.data.CheckedById;
            $scope.productNew.labelCheckAndApproved = 'To be checked by';
        }
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        getServiceOtherVendorChargeList($scope.productNew.Id);
        $scope.ContractWiseData(x.data.ContractId);
        //$scope.getToCurrencyRate();
        //if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
        //	var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
        //	if (paymentTerm.BaseLineDate !== null)
        //		if (paymentTerm.BaseLineDate === 'documentdate')
        //			$scope.IsBaseOnDueDateEnable = true;
        //		else
        //			$scope.IsBaseOnDueDateEnable = false;
        //}
        if (baseService.isUndefinedOrNull($scope.productNew.ContractId)) {
            $scope.productNew.OrderSpecific = 'No';
        }
        else {
            $scope.productNew.OrderSpecific = 'Yes';
        }
        $scope.getTaxCodeByTaxYearWithhold($scope.productNew.GRNDate);
        $scope.ImagedataLoad($scope.productNew.Id);
        $scope.TaxOptionAddiTax = 'Yes';
        //$scope.GetAdvanceTaxInfo($scope.productNew.Id);	
        //$scope.TotalSumAfterTCS();
        $scope.GetSalesTaxData();


        $scope.Action = 'Update';
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };


    $scope.Get1 = function (index) {
        $scope.index = index;
        //$scope.product = $scope.products[$scope.index];
        $scope.product = $scope.productsEmpGRN[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);

        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        getServiceOtherVendorChargeList($scope.productNew.Id);
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.GetSalesTaxData();
        $scope.Action = 'Update';
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.Save = function () {
        //debugger;
        try {
            $scope.productNew.PartyType = $scope.partyType;
            if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
                ShowResult("Enter Note for accounts", 'failure');
                return false;
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be approved by", 'failure');
                return false;
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be checked by", 'failure');
                return false;
            }

            else if ($scope.productNew.OrderSpecific === 'Yes' && baseService.isUndefinedOrNull($scope.productNew.ContractId)) {
                ShowResult("Please select Contract Information", 'failure');
                return false;
            }
            //if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            //if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            $scope.modelValidation('div_TT', 'productNew', 'TrancastionTypeId');
            $scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
            $scope.modelValidation('div_entryDate', 'productNew', 'EntryDate', 'Gate Entry Date');
            if ($scope.Action === 'Update')
                $scope.modelValidation('div_grnNo', 'productNew', 'Id');
            $scope.modelValidation('div_grnDate', 'productNew', 'GRNDate');

            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate))
                    return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
                else
                    manualValidation('div_entryDate', false);
                if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate))
                    return manualValidation('div_grnDate', true, "GRN date can't be less than gate entry date");
                else
                    manualValidation('div_grnDate', false);

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.Action = "Update";
                            $scope.GetGRN();
                            $scope.getDataList();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetGRN();
                            $scope.getDataList();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            throw e;
        }
    };
    $scope.SaveEMPGRN = function () {
        //debugger;
        try {
            if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be approved by", 'failure');
                return false;
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
                ShowResult("Please select to be checked by", 'failure');
                return false;
            }
            //if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            //if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            $scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
            $scope.modelValidation('div_entryDate', 'productNew', 'EntryDate', 'Gate Entry Date');
            if ($scope.Action === 'Update')
                $scope.modelValidation('div_grnNo', 'productNew', 'Id');
            $scope.modelValidation('div_grnDate', 'productNew', 'GRNDate');

            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate))
                    return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
                else
                    manualValidation('div_entryDate', false);
                if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate))
                    return manualValidation('div_grnDate', true, "GRN date can't be less than gate entry date");
                else
                    manualValidation('div_grnDate', false);

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrlEMPGRN,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.Action = "Update";
                            $scope.GetEmployeePurchase();
                            $scope.POListDetails();
                            $scope.GetGRN();
                            $scope.getDataList();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrlEMPGRN,
                        data: {
                            'entity': $scope.product,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetEmployeePurchase();
                            $scope.POListDetails();
                            $scope.GetGRN();
                            $scope.getDataList();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            throw e;
        }
    };
    $scope.Delete = function () {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl + $scope.productNew.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDataList();
                        ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
        else
            ShowResult('First delete all line item.', 'failure');
    };
    $scope.DeleteEMPGRN = function () {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
            if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrlEMPGRN + $scope.productNew.Id,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDataList();
                        ClearFields();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        }
        else
            ShowResult('First delete all line item.', 'failure');
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = {
            FixedAssetOrInventory: 'Inventory'
            , PODepended: false
            , AlongwithInvoice: true
            , IsNonCreditable: false
            , BaseCurrencyId: $scope.baseCurrencyId
            , ToCurrencyRate: 1
            , TaxApplicable: null
            , IsTaxApplicable: false
            , IsTaxApplicableChangeable: false
            , PartyType: $scope.partyType
            , PlantId: $window.plantId
            , OrderSpecific: 'No'
        };
        $scope.inventoryMaterialList = [];
        $scope.chargesList = [];
        $scope.grossTotal = 0;
        baseService.removeErrorClasses();
        $scope.productDocMap = { 
              UserFilename: null 
            , Description: null
            , Remarks: null
        };

        $scope.Imagedata = [];
    }
    $scope.changeAllInvoice = function () {
        $scope.productNew.InvoiceNo = null;
        $scope.productNew.InvoiceDate = null;
    };
    $scope.closePartyPopUp = function (x) {
        //if ($scope.partyIndex !== -1) {

        var party = x.data;// $scope.partyList[$scope.partyIndex];
        $scope.productNew.PartyCode = party.Code;
        $scope.productNew.PartyName = party.UserName;
        $scope.productNew.PartyId = party.Id;
        //$scope.productNew.PaymentTermId = party.PaymentTermId;
        $scope.productNew.CurrencyId = party.CurrencyId;
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew.BaseOnDueDate = null;
        $scope.productNew.BaseNoOfDays = null;
        $scope.productNew.MatureDate = null;
        $scope.productNew.TaxApplicable = party.TaxApplicable;
        $scope.productNew.IsTaxApplicableChangeable = party.IsTaxApplicableChangeable;
        if (party.TaxApplicable === 'Mandatory')
            $scope.productNew.IsTaxApplicable = true;
        else
            $scope.productNew.IsTaxApplicable = false;
        if (!baseService.isUndefinedOrNull($scope.productNew.DocDate))
            $scope.changePaymentTerm();
        getPartyPlantList();
        //}
        $scope.hidePartyPopUp();
    };
    function getPartyPlantList() {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address1;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }
    $scope.getToCurrencyRate = function () {
        if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
            $scope.productNew.ToCurrencyRate = 1;
            return;
        }
        $http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
            .then(function (response) {
                if (parseFloat(response.data) === 0)
                    $scope.productNew.ToCurrencyRate = 1;
                else
                    $scope.productNew.ToCurrencyRate = response.data;
            });
    };
    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.InvoicingGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = state;
                $scope.productNew.DeliveryGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = null;
                $scope.productNew.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = null;
                $scope.productNew.DeliveryGSTIN = null;
                return $scope.productNew.DeliveryByAddress = null;
            }
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Details

    $scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
    $scope.detailPopUp = function () {
        $scope.productNew.TaxOptionMat = 'Yes';
        $scope.detailModel = {
            Id: null
            , CountryId: null
            , InventoryReceiveId: $scope.productNew.Id
            , MaterialStorageId: $scope.productNew.MaterialStorageId
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , InventoryMaterialId: null
            , MaterialMasterId: null
            , MaterialMasterName: null
            , ArticleId: null
            , ArticleName: null
            , MaterialType: null
            , OurStyleName: null
            , Description: null
            , MaterialGroupMasterName: null
            , ProductMasterName: null
            , IsOurStyleRequired: false
            , IsProductMstRequired: false

            , FirstCharacteristicsId: null
            , FirstCharacteristicsValueId: null

            , SecondCharacteristicsId: null
            , SecondCharacteristicsValueId: null

            , ThirdCharacteristicsId: null
            , ThirdCharacteristicsValueId: null

            , TransactionQty: null
            , TransactionUoMId: null
            , TransactionRate: 0
            , TransactionAmount: 0
            , BaseQty: null
            , BaseUOMId: null
            , BaseUoM: null
            , BaseUoMFactor: null

            , TotalQty: null
            , TotalAmount: 0
            , TotalTaxAmount: 0
            , AvgRate: null
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
            , IsOriginApplicable: false
            , Description: null
            , TotalMaterialTranAmount: null
            , EnumType: null
            , TypeValueLot: false
            , TypeValueDiameter: false
            , TypeValueType: false
            , LotNumber: null
            , Diameter: null
            , Type: null
            , ShortageQty: null
            , RejectionQty: null
            , ApprovedQty: null
            , NetQty: null
            , IsAsset: null
            , LotNo: null
            , QualityStatus: null
            , GrossAmount: null
            , DiscountAmount: null
            , MasterOrderItemId: null
        };
        $scope.clearCharNames();
        getTaxCategoryList(null);
        angular.element(document.querySelector('#detailPopUp')).modal('show');

    };
    $scope.closeDetaiPopUp = function () {
        $scope.detailModel = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };

    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //$scope.setMaterialMasterData
    $scope.selectMaterialByType = function (ob) {
        //debugger;
        $scope.detailModel.MaterialMasterId = ob.Id;
        $scope.detailModel.MaterialMasterName = ob.UserName;
        $scope.detailModel.BaseUOMId = ob.BaseUOMId;
        $scope.detailModel.BaseUoM = ob.BaseUoM;
        $scope.detailModel.OurStyleName = ob.OurStyleName;
        $scope.detailModel.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.detailModel.ProductMasterName = ob.ProductMasterName;
        $scope.detailModel.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.detailModel.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.detailModel.TransactionUoMId = ob.BaseUOMId;
        $scope.detailModel.ArticleId = null;
        $scope.detailModel.ArticleName = null;
        $scope.detailModel.FirstCharacteristicsValueId = null;
        $scope.detailModel.SecondCharacteristicsValueId = null;
        $scope.detailModel.ThirdCharacteristicsValueId = null;
        $scope.detailModel.IsOriginApplicable = ob.IsOriginApplicable;
        $scope.detailModel.CountryId = null;
        $scope.detailModel.IsAsset = ob.IsAsset;
        $scope.MaterialHSNCodeId = ob.HSNCodeId;
        $scope.hasArticle = ob.HasAttribute;
        $scope.HasAttribute = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        $scope.clearCharNames();
        if (ob.HasAttribute)
            $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU)
            $scope.getCharacteristicsList(ob.Id);

        getTaxCategoryList(ob.HSNCodeId, ob.HSNCode);
        getBinMasterByMaterial(ob.Id);
        var mmId = []; mmId.push(ob.Id);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
            //$scope.detailModel.BaseUOMId = $filter("filter")($scope.uoMList, { IsBaseUom: 1 })[0].Value;
        });
        manualValidation('div_mm', false);
        manualValidation('div_country', false);
        $scope.LoadMaterialStatusLoad(ob.Id);
        $scope.closeMaterialMasterbyTypePopUp();

    };

    $scope.binMasterList = [];
    function getBinMasterByMaterial(materialMasterId) {
        $scope.binMasterList = [];
        $http({
            method: 'Post'
            , url: 'Materials/StorageBinAllocation/GetBinAllocationByMaterialId?materialMasterId=' + materialMasterId + '&materialStorageId=' + $scope.productNew.MaterialStorageId
        }).then(function (response) {
            $scope.binMasterList = response.data;
        });
    }
    $scope.getbinAllocationPopUp = function () {
        if ($scope.binMasterList.length == 1) {
            $scope.binMasterList[0].Qty = $scope.detailModel.TransactionQty;
        }
        angular.element(document.querySelector('#binAllocationPopUp')).modal('show');
    }
    $scope.binCheck = function () {

    }

    $scope.CloseBinAllocationPopUp = function () {
        if ($scope.binMasterList.length > 0) {
            $scope.TempbinMasterList = [];
            for (var i = 0; i < $scope.binMasterList.length; i++) {
                if ($scope.binMasterList.length == 1) {
                    $scope.binMasterList[i].Qty = $scope.detailModel.TransactionQty
                    angular.element(document.querySelector('#binAllocationPopUp')).modal('hide');
                }
                else {
                    $scope.TotalBinQty = Math.round($filter("sumByKey")($filter("filter")($scope.binMasterList), "Qty") * 1000 + Number.EPSILON) / 1000;
                    if ($scope.detailModel.TransactionQty > 0 && $scope.detailModel.TransactionQty < $scope.TotalBinQty) {
                        ShowResult("Allocation Qty can not greater than Transaction Qty", "failure", "binAllocationPopUp");
                    }
                    else if ($scope.detailModel.TransactionQty > 0 && $scope.detailModel.TransactionQty != $scope.TotalBinQty) {
                        ShowResult("Allocation Qty can not less than Transaction Qty", "failure", "binAllocationPopUp");
                    }
                    else {
                        angular.element(document.querySelector('#binAllocationPopUp')).modal('hide');
                    }
                }

            }
        }
    }
    $scope.LoadMaterialStatusLoad = function (ob) {
        ////debugger;
        $http({
            method: 'GET',
            url: 'accounts/OpeningBalance/LoadMaterialEnulType?Id=' + ob
        }).then(function successCallback(response) {
            $scope.LoadMaterialStatusLoadList = response.data;

            for (var i = 0; i < $scope.LoadMaterialStatusLoadList.length; i++) {
                if ($scope.LoadMaterialStatusLoadList[i].TypeValue === 'LotNo') {
                    $scope.detailModel.TypeValueLot = true;
                }
                else if ($scope.LoadMaterialStatusLoadList[i].TypeValue === 'Diameter') {
                    $scope.detailModel.TypeValueDiameter = true;
                }
                else if ($scope.LoadMaterialStatusLoadList[i].TypeValue === 'Type') {
                    $scope.detailModel.TypeValueType = true;
                }

            }

        });
    }
    $scope.selectarticle = function (ob) {
        //debugger;
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            $scope.detailModel.MinimumValue = ob.MinimumValue;
            $scope.detailModel.MaximumValue = ob.MaximumValue;
            getTaxCategoryList(ob.HSNCodeId, ob.HSNCode);
            manualValidation('div_ar', false);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };
    $scope.selectedBinAllocationList = [];
    $scope.detailSave = function () {
        //debugger;
        try {
            //$scope.validation();
            //if ($scope.chargesList.length > 0) {
            //	ShowResult('Delete Service First Then add items', 'failure', 'detailPopUp');
            //	return false;
            //}
            if (($scope.HasAttribute === true) && baseService.isUndefinedOrNull($scope.detailModel.ArticleId)) {
                ShowResult('Material Has Attribute.Please select Article', 'failure', 'detailPopUp');
                return false;
            }
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
                if ($scope.char1.CharacteristicsId.length > 0 && baseService.isUndefinedOrNull($scope.char1.FreeText)) {
                    ShowResult('Please select the Sku1', 'failure', 'detailPopUp');
                    return false;
                }
            }
            if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
                if ($scope.char2.CharacteristicsId.length > 0 && baseService.isUndefinedOrNull($scope.char2.FreeText)) {
                    ShowResult('Please select the Sku2', 'failure', 'detailPopUp');
                    return false;
                }

            }
            if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
                if ($scope.char3.CharacteristicsId.length > 0 && baseService.isUndefinedOrNull($scope.char3.FreeText)) {
                    ShowResult('Please select the Sku3', 'failure', 'detailPopUp');
                    return false;
                }
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) || isNaN($scope.detailModel.TransactionQty)) {
                ShowResult('Enter the  Transaction Qty', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.GrossAmount) || isNaN($scope.detailModel.GrossAmount)) {
                ShowResult('Enter the  Gross Amount', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.detailModel.DiscountAmount > $scope.detailModel.GrossAmount) {
                ShowResult('Discount Amount can not grater than Gross Amount', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.GrossAmount)) {
                ShowResult('Enter  the gross amount', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.DiscountAmount)) {
                ShowResult('Enter  the discount amount', 'failure', 'detailPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.QualityStatus)) {
                ShowResult('Select the quality status', 'failure', 'detailPopUp');
                return false;
            }
            //if ($scope.detailModel.TypeValue === true && baseService.isUndefinedOrNull($scope.detailModel.LotNumber)) {
            //	ShowResult('Enter the Lot Number.', '', 'detailPopUp');
            //	return false;
            //}
            if ($scope.productNew.IsFOC === true && baseService.isUndefinedOrNull($scope.detailModel.TransactionQty)) {
                ShowResult('Enter the Qty', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.productNew.IsFOC === true && $scope.detailModel.TransactionQty === 0) {
                ShowResult('Enter the Qty', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.productNew.IsFOC === true && ($scope.detailModel.TransactionAmount != 0 || $scope.detailModel.TransactionAmount != 0.00 || $scope.detailModel.TransactionAmount != 0.0)) {
                ShowResult('Enter the Total Amount Zero', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.productNew.IsFOC === true && $scope.detailModel.TransactionAmount > 0) {
                ShowResult('Enter the Total Amount Zero only', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.productNew.IsFOC != true && baseService.isUndefinedOrNull($scope.detailModel.TransactionQty)) {
                ShowResult('Enter the Qty', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.productNew.IsFOC != true && $scope.detailModel.TransactionQty === 0) {
                ShowResult('Enter the Qty', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.productNew.IsFOC != true && ($scope.detailModel.TransactionAmount === 0 || $scope.detailModel.TransactionAmount === 0.00 || $scope.detailModel.TransactionAmount === 0.0)) {
                ShowResult('Enter the Total Amount', 'failure', 'detailPopUp');
                return false;
            }

            if (baseService.isUndefinedOrNull($scope.detailModel.ShortageQty)) {
                $scope.detailModel.ShortageQty = 0;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.RejectionQty)) {
                $scope.detailModel.RejectionQty = 0;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.DiscountAmount)) {
                $scope.detailModel.DiscountAmount = 0;
            }
            if (isNaN($scope.detailModel.ShortageQty)) {
                $scope.detailModel.ShortageQty = 0;
            }
            if (isNaN($scope.detailModel.RejectionQty)) {
                $scope.detailModel.RejectionQty = 0;
            }
            if (isNaN($scope.detailModel.DiscountAmount)) {
                $scope.detailModel.DiscountAmount = 0;
            }
            if (baseService.isUndefinedOrNull($scope.detailModel.DiscountAmount) || isNaN($scope.detailModel.DiscountAmount)) {
                ShowResult('Enter the  total discount amount', 'failure', 'detailPopUp');
                return false;
            }

            $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
            $scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            $scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

            if ($scope.binMasterList.length) {
                $scope.selectedBinAllocationList = [];
                for (var b = 0; b < $scope.binMasterList.length; b++) {
                    if ($scope.binMasterList[b].Qty > 0) {
                        $scope.selectedBinAllocationList.push($scope.binMasterList[b]);
                    }
                }
            }

            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.detailModel.MaterialMasterId === $scope.inventoryMaterialList[i].MaterialMasterId &&
                    $scope.detailModel.ArticleId === $scope.inventoryMaterialList[i].ArticleId &&
                    $scope.detailModel.FirstCharacteristicsId === $scope.inventoryMaterialList[i].FirstCharacteristicsId &&
                    $scope.detailModel.FirstCharacteristicsValueId === $scope.inventoryMaterialList[i].FirstCharacteristicsValueId &&
                    $scope.detailModel.SecondCharacteristicsId === $scope.inventoryMaterialList[i].SecondCharacteristicsId &&
                    $scope.detailModel.SecondCharacteristicsValueId === $scope.inventoryMaterialList[i].SecondCharacteristicsValueId &&
                    $scope.detailModel.ThirdCharacteristicsId === $scope.inventoryMaterialList[i].ThirdCharacteristicsId &&
                    $scope.detailModel.ThirdCharacteristicsValueId === $scope.inventoryMaterialList[i].ThirdCharacteristicsValueId &&
                    $scope.detailModel.CountryId === $scope.inventoryMaterialList[i].CountryId &&
                    $scope.detailModel.LotNumber === $scope.inventoryMaterialList[i].LotNumber &&
                    $scope.detailModel.Diameter === $scope.inventoryMaterialList[i].Diameter &&
                    $scope.detailModel.Type === $scope.inventoryMaterialList[i].Type) {
                    return ShowResult('This material already received');
                }
            }

            $http({
                method: 'POST',
                url: $scope.detailSaveUrl,
                data: {
                    entity: $scope.detailModel
                    , taxCategoryList: $scope.taxCategoryList
                    , gRNBinAllocationMapList: $scope.selectedBinAllocationList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'detailPopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'detailPopUp');
                    $scope.detailModel.Id = null;
                    $scope.detailModel = {
                        InventoryReceiveId: $scope.productNew.Id
                        , MaterialStorageId: $scope.productNew.MaterialStorageId
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TotalAmount: 0
                        , TransactionAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                        , IsOriginApplicable: false
                        , ShortageQty: null
                        , RejectionQty: null
                    };
                    $scope.taxCategoryList = [];
                    $scope.selectedBinAllocationList = [];
                    $scope.TotalBinQty = 0;
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.clearCharNames();
                    $scope.uom();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'detailPopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };

    $scope.valuePassInDelModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.detailDelete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.detailDeleteUrl + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.validation = function () {
        $scope.modelValidation('div_mm', 'detailModel', 'MaterialMasterName', 'Material Master');
        if ($scope.hasArticle) $scope.modelValidation('div_ar', 'detailModel', 'ArticleName');
        $scope.manualValidationAddRemove('div_qty', 'detailModel', 'TransactionQty');
        $scope.modelValidation('div_qty', 'detailModel', 'TransactionUoMId', 'UoM is required');
        if ($scope.detailModel.TransactionAmount === 0)
            throw manualValidation('div_tamnt', true, 'Total amount is required.');
        $scope.manualValidationAddRemove('div_tamnt', 'detailModel', 'TransactionAmount');
        if ($scope.detailModel.IsOriginApplicable)
            $scope.manualValidationAddRemove('div_country', 'detailModel', 'CountryId');

        var isSku = false;
        if ($scope.hasSku) {
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
            }
            if (isSku) throw ShowResult('Please insert SKU.', 'failure', 'detailPopUp');
        }
    };
    $scope.manualValidationAddRemove = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    //manualDateValidation
    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };

    $scope.sumORnot = false;
    function getInventoryMaterialList(inveReveiveId) {
        //debugger;
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListwithoutpo?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                $scope.inventoryMaterialList = response.data.Rows;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetSalesTaxData();
                for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {

                    if ($scope.productNew.IsNonCreditable == 1) {
                        if ($scope.inventoryMaterialList[i].TaxAmount === null || $scope.inventoryMaterialList[i].TaxAmount === "" || $scope.inventoryMaterialList[i].TaxAmount === 0) {
                            $scope.inventoryMaterialList[i].TaxAmount = 0; //parseFloat(Math.round(num3 * 100) / 100).toFixed(2);
                            $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount + $scope.inventoryMaterialList[i].TaxAmount + $scope.inventoryMaterialList[i].ServiceCharge + $scope.inventoryMaterialList[i].ServiceTax).toFixed(2);
                            $scope.inventoryMaterialList[i].BaseAmount = parseFloat($scope.inventoryMaterialList[i].TotalMaterialTranAmount * $scope.productNew.ToCurrencyRate).toFixed(2);

                        }
                        else if ($scope.inventoryMaterialList[i].ServiceCharge === null || $scope.inventoryMaterialList[i].ServiceCharge === "" || $scope.inventoryMaterialList[i].ServiceCharge === 0) {
                            $scope.inventoryMaterialList[i].ServiceCharge = 0;
                            $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount + $scope.inventoryMaterialList[i].TaxAmount + $scope.inventoryMaterialList[i].ServiceCharge + $scope.inventoryMaterialList[i].ServiceTax).toFixed(2);
                            $scope.inventoryMaterialList[i].BaseAmount = parseFloat($scope.inventoryMaterialList[i].TotalMaterialTranAmount * $scope.productNew.ToCurrencyRate).toFixed(2);

                        }
                        else if ($scope.inventoryMaterialList[i].ServiceTax === null || $scope.inventoryMaterialList[i].ServiceTax === "" || $scope.inventoryMaterialList[i].ServiceTax === 0) {
                            $scope.inventoryMaterialList[i].ServiceTax = 0;
                            $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount + $scope.inventoryMaterialList[i].TaxAmount + $scope.inventoryMaterialList[i].ServiceCharge + $scope.inventoryMaterialList[i].ServiceTax).toFixed(2);
                            $scope.inventoryMaterialList[i].BaseAmount = parseFloat($scope.inventoryMaterialList[i].TotalMaterialTranAmount * $scope.productNew.ToCurrencyRate).toFixed(2);

                        }
                        else {
                            $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount + $scope.inventoryMaterialList[i].TaxAmount + $scope.inventoryMaterialList[i].ServiceCharge + $scope.inventoryMaterialList[i].ServiceTax).toFixed(2);
                            $scope.inventoryMaterialList[i].BaseAmount = parseFloat($scope.inventoryMaterialList[i].TotalMaterialTranAmount * $scope.productNew.ToCurrencyRate).toFixed(2);
                        }


                    }
                    else {
                        if ($scope.inventoryMaterialList[i].ServiceCharge === null || $scope.inventoryMaterialList[i].ServiceCharge === "" || $scope.inventoryMaterialList[i].ServiceCharge === 0) {
                            $scope.inventoryMaterialList[i].ServiceCharge = 0;
                            $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount + $scope.inventoryMaterialList[i].ServiceCharge).toFixed(2);
                            $scope.inventoryMaterialList[i].BaseAmount = parseFloat($scope.inventoryMaterialList[i].TotalMaterialTranAmount * $scope.productNew.ToCurrencyRate).toFixed(2);
                        }
                        else {
                            $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount + $scope.inventoryMaterialList[i].ServiceCharge).toFixed(2);
                            $scope.inventoryMaterialList[i].BaseAmount = parseFloat($scope.inventoryMaterialList[i].TotalMaterialTranAmount * $scope.productNew.ToCurrencyRate).toFixed(2);
                        }
                    }

                }
                $scope.GetAdvanceTaxInfo($scope.productNew.Id);
                $scope.TotalSumAfterTCS();
            });
    }
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function getTaxCategoryList(hsnCodeId, HSNCode) {
        $scope.taxCategoryList = [];
        //      if (baseService.isUndefinedOrNull(hsnCodeId)) {
        //          hsnCodeId = $scope.MaterialHSNCodeId;
        //      }
        //      else {
        //          hsnCodeId = hsnCodeId;
        //}
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&GRNDate=' + $scope.productNew.GRNDate
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            for (var i = 0; i < $scope.taxCategoryList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].hsnCodeId)) {
                    $scope.taxCategoryList[i].HSNCode = HSNCode;
                    $scope.taxCategoryList[i].HSNCodeId = hsnCodeId;
                    //$scope.taxCategoryList[i].Percentage = null;
                    //$scope.HSNCode = HSNCode;
                }
            }
        });
    }

    $scope.calculateTaxCategory = function () {
        $scope.detailModel.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) ? 0 : parseFloat($scope.detailModel.TransactionAmount);
        if (tQty > 0 && tAmount > 0)
            $scope.detailModel.TransactionRate = tAmount / tQty;
        else
            $scope.detailModel.TransactionRate = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;

    };
    $scope.sumTaxAmount = function () {
        $scope.detailModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };

    //   $scope.getReceiveTaxList = function (data, flag) {
    //       $scope.taxAbleAmnt = data.TrnAmount;
    //       $scope.percentageColumn = flag;
    //       $http({
    //           method: 'GET',
    //           url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + data.InventoryReceiveDetailId
    //       }).then(function (response) {
    //           $scope.receiveTaxList = response.data;
    //           angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    //       });
    //};
    $scope.calculateTaxAmount = function (data) {
        //debugger;
        //data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.GetSalesTaxData = function (salesId) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.TaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = gettaxlist(linepk);
                $scope.inventoryMaterialList[i].TaxList = list;
            }
        });
    };
    function gettaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.TaxList.length; i++) {
            if ($scope.TaxList[i].PODetailId === linepk) {
                result.push($scope.TaxList[i]);
            }
        }
        return result;
    }
    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });
    }
    $scope.getReceiveTaxList = function (data, flag, index, Id) {

        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            if ($scope.inventoryMaterialList[i].DataChangeFlag === 'True') {
                ShowResult("Please set shortage & rejection value then Update Your Changes");
                return false;
            }
        }
        $scope.productNew.TaxOption = 'Yes';
        $scope.LoadTaxButtonClick();

        //debugger;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;

        $scope.currentMaterialRow = index;
        //$scope.taxAbleAmnt = data.TransactionAmount;
        //$scope.taxAmnt = data.TaxAmount;
        $scope.receiveTaxList = [];
        if (data.TaxList.length > 0) {
            $scope.HSNCode = data.TaxList[0].HSNCode;
            $scope.receiveTaxList = data.TaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        //$http({
        //    method: 'GET',
        //    url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + data.InventoryReceiveDetailId
        //}).then(function (response) {
        // $scope.receiveTaxList = response.data;
        //$scope.HSNCode = $scope.receiveTaxList[0]['HSNCode'];
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        //});
        // $Scope.TAction = "OK";
    };



    $scope.getTotalReceiveTaxList = function (amount, flag) {
        $scope.taxAbleAmnt = amount;
        $scope.percentageColumn = flag;
        $http({
            method: 'GET',
            url: $scope.path + 'GetTotalReceiveTaxList?receiveId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.receiveTaxList = response.data;
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });
    };
    //$scope.closeReceiveTaxPopUp = function () {
    //    $scope.detailModel = {};
    //    $scope.receiveTaxList = [];
    //    angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    //}
    $scope.closeReceiveTaxPopUp = function () { //hossain
        //debugger;
        $scope.detailModel = {};
        //$scope.receiveTaxList = [];
        ////debugger;



        $scope.detailModel.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
        $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
                return false;
            }

            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            //if ($scope.receiveTaxList[i].TaxAmount == "0.00") {
            //    ShowResult("Tax Amount can't 0.", 'failure', 'receiveTaxPopUp');
            //    return false;
            //}
            //if ($scope.receiveTaxList[i].TaxAmount == "0") {
            //    ShowResult("Tax Amount can't 0.", 'failure', 'receiveTaxPopUp');
            //    return false;
            //}

        }

        //if ($scope.TAction === "OK") {
        $http({
            method: 'POST',
            //url: $scope.saveUrl,
            url: 'Products/InventoryReceive/InsertExtraTax',
            //data: $scope.receiveTaxList,
            data: {
                entity: $scope.detailModel
                , taxCategoryList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');
                //$scope.productNew.Id = response.data.entity.Id;
                // $scope.productNew.PartyName = $scope.product.PartyName;
                // $scope.Action = "Update";
                //$scope.getDataList();
                getInventoryMaterialList($scope.productNew.Id);
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
        };
        // }

        //angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

    }

    $scope.UpdateServiceTax = function () { //hossain
        //debugger;
        $scope.detailModel = {};
        //$scope.receiveTaxList = [];
        ////debugger;



        $scope.detailModel.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
        $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
                return false;
            }

            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            //if ($scope.receiveTaxList[i].TaxAmount == "0.00") {
            //    ShowResult("Tax Amount can't 0.", 'failure', 'receiveTaxPopUp');
            //    return false;
            //}
            //if ($scope.receiveTaxList[i].TaxAmount == "0") {
            //    ShowResult("Tax Amount can't 0.", 'failure', 'receiveTaxPopUp');
            //    return false;
            //}

        }

        //if ($scope.TAction === "OK") {
        $http({
            method: 'POST',
            //url: $scope.saveUrl,
            url: 'Products/InventoryReceive/InsertExtraTax',
            //data: $scope.receiveTaxList,
            data: {
                entity: $scope.detailModel
                , taxCategoryList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');
                //$scope.productNew.Id = response.data.entity.Id;
                // $scope.productNew.PartyName = $scope.product.PartyName;
                // $scope.Action = "Update";
                //$scope.getDataList();
                getInventoryMaterialList($scope.productNew.Id);
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
        };
        // }

        //angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

    }

    $scope.closeReceiveTaxPopUpwindow = function () {
        //debugger;
        getInventoryMaterialList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }
    $scope.closeReceiveTaxPopUpwindow1 = function () {
        //debugger;
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp1')).modal('hide');
    }

    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.receiveTaxList.push(data);

    };

    function removeValidationMsg() {
        CloseModalShowResult();
        $scope.clearCharNames();
        manualValidation('div_mm', false);
        manualValidation('div_ar', false);
        manualValidation('div_qty', false);
        manualValidation('div_qty', false);
        manualValidation('div_rate', false);
    }
    function getGrossAmount(list, key1, key2, key3, fieldName) {
        $scope[fieldName] = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            $scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
        }
    }

    // #endregion Details

    // #region Payment Term
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.productNew.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }
            $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };
    // #endregion Payment Term

    // #region Service
    $scope.serviceChargePopUp = function () {
        $scope.productNew.TaxOptionService = 'Yes';
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
            return ShowResult('Without material charges not aplicable.');
        $scope.taxCategoryList = null;
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.productNew.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: 0
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
        };
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceList = response.data;
        });
    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    $scope.changeService = function () {
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return getTaxCategoryList(hsnCodeId);//$scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCode;
        getTaxCategoryList(hsnCodeId, HSNCode);
    };



    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };







    $scope.serviceSave = function () {
        try {
            $scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');

            $http({
                method: 'POST',
                url: $scope.sreviceSaveUrl,
                data: {
                    entity: $scope.serviceModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                    $scope.serviceModel = {
                        Id: null
                        , ServiceMasterId: null
                        , InventoryReceiveId: $scope.productNew.Id
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TransactionAmount: 0
                        , BaseAmount: 0
                        , TotalTaxAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                    };
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };



    $scope.serviceUpdate = function () {
        try {
            $scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');

            $http({
                method: 'POST',
                url: $scope.sreviceUpdateUrl,
                data: {
                    entity: $scope.serviceModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                    $scope.serviceModel = {
                        Id: null
                        , ServiceMasterId: null
                        , InventoryReceiveId: $scope.productNew.Id
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TransactionAmount: 0
                        , BaseAmount: 0
                        , TotalTaxAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                    };
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };








    $scope.delModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };
    $scope.serviceDelete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.sreviceDeleteUrl + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.getServiceTaxList = function (data, flag) {
        //debugger;

        $scope.taxAbleAmnt = data.Amount + data.TotalTaxAmount;
        $scope.percentageColumn = flag;
        $scope.LoadTaxButtonClick();
        $http({
            method: 'GET',
            url: $scope.path + 'GetServiceTaxList?serviceId=' + data.Id
        }).then(function (response) {
            $scope.receiveTaxList = response.data;
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });

    }
    $scope.getServiceTaxList1 = function (data, flag) {
        //debugger;
        $scope.productNew.TaxOptionServiceModify = 'Yes';
        $scope.taxAbleAmnt = data.Amount;// + data.TotalTaxAmount;
        $scope.percentageColumn = flag;
        $scope.LoadTaxButtonClick();

        $http({
            method: 'GET',
            url: $scope.path + 'GetServiceTaxList?serviceId=' + data.Id
        }).then(function (response) {
            $scope.receiveTaxList = response.data;
            $scope.HSNCode = $scope.receiveTaxList[0].HSNCode;
            angular.element(document.querySelector('#receiveTaxPopUp1')).modal('show');
        });

    }

    function getServiceChargeList(inveReveiveId) {
        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;
            });
    }
    // #endregion Service

    $scope.inventoryReceiveReport = function (id, reportFormat) {
        if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryReceive/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId, '_blank');
    };


    $scope.getReceiveTaxListPOValueSet1 = function (data, flag, index, Id) {
        //debugger;
        //angular.element(document.querySelector('#ValueSet')).modal('show');
        if ($scope.Action1 === 'Update') {
            $scope.new = [];
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if ($scope.inventoryMaterialList[i].ShortRejFlag === true) {
                    $scope.new.push($scope.inventoryMaterialList[i]);

                }
                else {
                    //$scope.new = $scope.inventoryMaterialListPO;
                    for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                        //if ($scope.inventoryMaterialList[i].check === true) {
                        if ($scope.inventoryMaterialList[i].ShortageQty > 0 || $scope.inventoryMaterialList[i].RejectionQty > 0) {
                            $scope.new.push($scope.inventoryMaterialList[i]);
                        }
                        //}
                    }
                    //$scope.inventoryMaterialListPO = [];
                    for (var i = 0; i < $scope.new.length; i++) {
                        //if ($scope.new[i].ShortRejFlag === false) {
                        if ($scope.new[i].ShortageQty > 0 || $scope.new[i].RejectionQty > 0) {
                            $scope.new[i].ShortageRate = 110;
                            $scope.new[i].ShortageValue = (($scope.new[i].ShortageQty * $scope.new[i].ShortageRate) / 100) * $scope.new[i].TransactionRate;
                            $scope.new[i].RejectionRate = 50;
                            $scope.new[i].RejectionValue = (($scope.new[i].RejectionQty * $scope.new[i].RejectionRate) / 100) * $scope.new[i].TransactionRate;
                            $scope.new[i].RejectionClamRate = (100 - $scope.new[i].RejectionRate);
                        }

                    }
                }
            }


            angular.element(document.querySelector('#ValueSet')).modal('show');

        }



    };
    $scope.CalculateShortageVal = function (x) {
        //debugger;
        for (var i = 0; i < $scope.newList.length; i++) {
            $scope.newList[i].ShortageValue = (($scope.newList[i].ShortageQty * $scope.newList[i].ShortageRate) / 100) * $scope.newList[i].TransactionRate;
        }


    }
    $scope.CalculateRejectionVal = function () {
        for (var i = 0; i < $scope.newList.length; i++) {
            $scope.newList[i].RejectionValue = (($scope.newList[i].RejectionQty * $scope.newList[i].RejectionRate) / 100) * $scope.newList[i].TransactionRate;
            $scope.newList[i].RejectionClamRate = (100 - $scope.newList[i].RejectionRate);


        }
    }
    $scope.closeReceiveTaxPopUpValue = function (x) {

        angular.element(document.querySelector('#ValueSet')).modal('hide');
    }
    $scope.closeReceiveTaxPopUpValue1 = function () {
        angular.element(document.querySelector('#ValueSet')).modal('hide');
    }
    $scope.calculateAmount = function (data, index) {


        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {

            if ($scope.inventoryMaterialList[i].ShortageQty > $scope.inventoryMaterialList[i].TransactionQty) {
                //$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
            }
            else if ($scope.inventoryMaterialList[i].RejectionQty > $scope.inventoryMaterialList[i].TransactionQty) {
                //$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
            }
            else {
                if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                    //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
                    //$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    //$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    //$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
                    //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                    $scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);
                    $scope.inventoryMaterialList[i].DataChangeFlag = 'True'
                }
                else {
                    //$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    //$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    //$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty+$scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
                    //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                    $scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);
                    $scope.inventoryMaterialList[i].DataChangeFlag = 'True'
                }
                //if ($scope.productNew.IsNonCreditable == 1) {
                //	//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
                //	//$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
                //	$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
                //	$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);


                //}
                //else {
                //	//data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
                //	data.TotalMaterialTranAmount = parseFloat(data.TrnAmount).toFixed(2) + parseFloat(data.ServiceCharge).toFixed(2);
                //	data.TotalMaterialBaseAmount = ((parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                //}
            }
        }
        //angular.forEach($scope.inventoryMaterialListPO, function (item) {
        //    item.ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * data.TrnAmount;

        //});

        //$scope.detailModel.BaseUOMId = $filter("filter")($scope.chargesListPO, { IsBaseUom: 1 })[0].Value;

        // data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;

    };

    $scope.calculateAmountPOPUP = function () {
        if (baseService.isUndefinedOrNull($scope.detailModel.ShortageQty)) {
            $scope.detailModel.ShortageQty = 0;
        }
        if (baseService.isUndefinedOrNull($scope.detailModel.RejectionQty)) {
            $scope.detailModel.RejectionQty = 0;
        }
        if (baseService.isUndefinedOrNull($scope.detailModel.DiscountAmount)) {
            $scope.detailModel.DiscountAmount = 0;
        }
        if (isNaN($scope.detailModel.ShortageQty)) {
            $scope.detailModel.ShortageQty = 0;
        }
        if (isNaN($scope.detailModel.RejectionQty)) {
            $scope.detailModel.RejectionQty = 0;
        }
        if (isNaN($scope.detailModel.DiscountAmount)) {
            $scope.detailModel.DiscountAmount = 0;
        }
        if ($scope.detailModel.ShortageQty > $scope.detailModel.TransactionQty) {
            ShowResult('Shortage quantity can not grater than Transaction qty!', 'failure');
            return false;
        }
        if ($scope.detailModel.RejectionQty > $scope.detailModel.TransactionQty) {
            ShowResult('Rejection quantity can not grater than Transaction qty!', 'failure');
            return false;
        }

        $scope.detailModel.ApprovedQty = ($scope.detailModel.TransactionQty - ($scope.detailModel.ShortageQty + $scope.detailModel.RejectionQty));
        $scope.detailModel.NetQty = ($scope.detailModel.TransactionQty - $scope.detailModel.ShortageQty);
        //$scope.detailModel.TransactionAmount

    };

    $scope.UpdateGRNDetails = function () {
        //debugger;

        try {

            if ($scope.Action1 === "Update") {
                for (var i3 = 0; i3 < $scope.inventoryMaterialList.length; i3++) {
                    var shortREh = $scope.inventoryMaterialList[i3].ShortageQty + $scope.inventoryMaterialList[i3].RejectionQty;
                    if (shortREh > $scope.inventoryMaterialList[i3].TransactionQty) {
                        ShowResult('Shortage & Rejected Qty can not grater than GRN Qty ', 'failure');
                        break;
                    }
                    //$scope.detailModel.TotalMaterialTranAmount = $scope.inventoryMaterialList[i3].BaseAmount1;
                    if ($scope.inventoryMaterialList[i3].ShortageQty > 0) {
                        if ($scope.inventoryMaterialList[i3].ShortageValue == 0 || $scope.inventoryMaterialList[i3].ShortageValue == null) {
                            ShowResult('Please set Shortage Rate & Value ', 'failure');
                            break;
                        }
                    }
                    if ($scope.inventoryMaterialList[i3].RejectionQty > 0) {
                        if ($scope.inventoryMaterialList[i3].RejectionValue == 0 || $scope.inventoryMaterialList[i3].RejectionValue == null) {
                            ShowResult('Please set Rejection Rate & Value ', 'failure');
                            break;
                        }
                    }
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl1,
                        data:
                        {
                            //'entity': $scope.product,
                            'entityMatAndImat': $scope.newList,
                            'Id': $scope.productNew.Id

                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getDataList();
                            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                                $scope.inventoryMaterialList[i].DataChangeFlag = 'False'
                                $scope.inventoryMaterialList[i].ShortRejFlag = true
                            }

                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }

            }

        } catch (e) {
            throw e;
        }
    };

    $scope.POPopUpGateEntry = function () {
        $scope.getalldataGateEntry();
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
    };
    $scope.POPopUpCloseGateEntry = function () {
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
    };

    $scope.POPopUpGateEntryByEmployee = function () {
        //debugger;
        $scope.getalldataGateEntryByEmployee();
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
    };
    $scope.POPopUpCloseGateEntryByEmployee = function () {
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
    };



    $scope.GriddataGateEntry = [];
    $scope.getalldataGateEntry = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/GetListOfPOGateEntry?partyCode=' + $scope.productNew.PartyId,
        }).then(function successCallback(response) {
            $scope.GriddataGateEntry = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.gateList = [];
    $scope.getalldataGateEntryByEmployee = function () {

        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryReceive/GetListOfPOGateEntryEmployee?EmployeeId=' + $scope.productNew.EmployeeId,
        }).then(function successCallback(response) {
            $scope.gateList = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.recorddoubleclickGateEntry = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        //alert('Id'+Id);
        // $scope.productNew = x.data;
        //  $scope.productId = "";
        $scope.productNew.GateEntryNo = x.data.Id;
        $scope.productNew.EntryDate = x.data.EntryDate;

        $scope.POPopUpCloseGateEntry();
    }
    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryReceive/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();


    $scope.onClickReportDownloadWord = function (args) {
        //debugger;
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        //$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.GRNReport = function (data) {
        //debugger;

        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data;

        click: $scope.onClickReportDownloadWord
    };



    $scope.onClickGetEmployeePurchaseID = function (args) {

        var gridObj = $("#EmployeePurchaseID").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        //alert('jj' + data.Id);
        // $scope.valuePassInDelModal(data); 
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command1 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickGetEmployeePurchaseID
        }
    }];


    //#region All Tab Control in GRN With-out-PO
    $scope.GRN = "";
    $scope.tab = 1;
    $scope.setTabGRNList = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        $scope.GRN = 0;
        $scope.GetGRN();
    };
    $scope.isSetGRNList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };



    $scope.GRN = "";
    // $scope.tab = 2;
    $scope.setTabNotApproveCheck = function (newTab) {
        //debugger;
        $scope.tab = newTab;
        //  alert('Tab 2');
        //$scope.GRN = 0;
        //$scope.GetEmployeeNotApproveChecked();
        $scope.GetNotApproveChecked();
    };
    $scope.isSetNotApproveCheck = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };




    $scope.GRN = "";
    // $scope.tab = 3;
    $scope.setTabApproveNotPost = function (newTab) {
        $scope.tab = newTab;
        //  alert('Tab 3');
        // $scope.GRN = 0;
        $scope.GRNGetApprovedNotPost();
    };
    $scope.isSetApproveNotPost = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };


    $scope.GRN = "";
    // $scope.tab = 4;
    $scope.setTabPosted = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 4');
        $scope.GRN = 0;
        $scope.GetPosted();
    };
    $scope.isSetPosted = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };



    $scope.setTabCheckedHoldReject = function (newTab) {
        $scope.tab = newTab;
        $scope.GRNCheckedHoldReject();
    };
    $scope.isSetCheckedHoldReject = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };

    $scope.setTabApprovedHoldReject = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 4');
        //$scope.GRN = 0;
        $scope.GRNApprovedHoldChecked();
    };
    $scope.isSetApprovedHoldReject = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };

    //#endregion

    //#region All Tab Control in Employee-GRN
    $scope.GRN = "";
    $scope.tab = 1;
    $scope.setTabEmployeeGRNList = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        $scope.GRN = 0;
        $scope.GetEmployeePurchase();
    };
    $scope.isSetEmployeeGRNList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };

    $scope.setTabEmpNotApproveCheckedHoldReject = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        //$scope.GRN = 0;
        $scope.EmpGetListEmpCheckedHoldReject();

    };
    $scope.isSetEmpNotApproveCheckedHoldReject = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };

    $scope.setTabEmpApprovedNotPostListHoldReject = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        //$scope.GRN = 0;
        $scope.EMPGetListEmpApprovedHoldReject();
    };
    $scope.isSetEmpApprovedNotPostListHoldReject = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };





    $scope.GRN = "";
    // $scope.tab = 2;
    $scope.setTabEmpNotApproveCheckedList = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        //$scope.GRN = 0;
        $scope.GetEmployeeNotApproveChecked();
    };
    $scope.isSetEmpNotApproveCheckedList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };


    $scope.GRN = "";
    // $scope.tab = 3;
    $scope.setTabEmpApprovedNotPostList = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        //$scope.GRN = 0;
        $scope.GetEmployeeApprovedNotPost();
    };
    $scope.isSetEmpApprovedNotPostList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };


    $scope.GRN = "";
    // $scope.tab = 4;
    $scope.setTabEmployeePostedList = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        //$scope.GRN = 0;
        $scope.EMPGetEmployeePosted();
    };
    $scope.isSetEmployeePostedList = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.GRN = 0;
    };

    //#endregion Employee-GRN
    //#region All Print buton of Employee-GRn


    $scope.onClickGetEmployeePurchaseID = function (args) {

        var gridObj = $("#GridGRNID1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command11 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickGetEmployeePurchaseID
        }
    }];



    $scope.onClickGRNID1 = function (args) {


        var gridObj = $("#GridGRNID").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command1 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickGRNID1
        }
    }];
    $scope.onClickGRNID2 = function (args) {


        var gridObj = $("#GridGRNID2").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command2 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",
            click: $scope.onClickGRNID2
        }
    }];



    $scope.onClickGRNID3 = function (args) {

        var gridObj = $("#GridGRNID3").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command3 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickGRNID3
        }
    }];


    $scope.onClickGRNID4 = function (args) {


        var gridObj = $("#GridGRNID4").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command4 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickGRNID4
        }
    }];

    $scope.onClickGRNID5 = function (args) {
        //debugger;

        var gridObj = $("#GridGRNID5").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command5 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickGRNID5
        }
    }];

    $scope.onClickGRNID6 = function (args) {
        //debugger;

        var gridObj = $("#GridGRNID6").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.command6 = [{

        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickGRNID6
        }
    }];


    //#endregion


    //#region GRNApplrval icon Detail
    $scope.lst = [];
    $scope.POListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/GRNDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    $scope.POListDetails();
    $scope.GRNDocumentMapDataAll = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/GoodsReceiveNote/GRNDocumentMapDataAll'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.Img = response.data;

        });
    }
    $scope.GRNDocumentMapDataAll();

    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(200));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "CountryName", "TransactionQty", "TransactionUoM", "TransactionRate", "CurrencyName", "GrossAmount", "DiscountAmount", "TotalMaterialTranAmount", "MasterOrderNo"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
        //var filteredData1 = e.data["Id"];
        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("GRNId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid1").ejGrid({
            dataSource: dataImg,
            columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
            { field: "Description", headerText: "Description", width: 100 },
            { field: "Remarks", headerText: "Remarks", width: 100 },

            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion

    //#region Order Specific Info
    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        $scope.contractList = [];
        $http.get("Products/PurchaseOrder/GetLCContractList?isProcurementOnBom=" + false)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#ContractPopUp')).modal('show');
    };

    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;
        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    }
    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }
    $scope.Clearcontract = function () {
        $scope.productNew.CustomerName = "";
        $scope.productNew.ContractId = "";

    };
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;
        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    }
    $scope.GetShortageRejectionValue = function () {
        $scope.newList = [];
        $http({
            method: 'GET',
            url: "Products/InventoryReceive/GetShortageRejectionValue?InventoryReceiveId=" + $scope.productNew.Id
        }).then(function (response) {
            $scope.newList = response.data;
        });
        angular.element(document.querySelector('#ValueSet')).modal('show');
    }

    $scope.UpdateUrlForSRValue = function () {
        $http({
            method: 'POST',
            url: $scope.updateUrlForSRValue,
            data:
            {
                'InventoryReceiveId': $scope.productNew.Id,
                'UserSendData': $scope.newList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                angular.element(document.querySelector('#ValueSet')).modal('hide');

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.calculateTaxAmountForMat = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.detailModel.TransactionAmount * data.Percentage) / 100;
    };
    $scope.checkRowValidationMat = function (x) {
        debugger;
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) || $scope.detailModel.TransactionAmount === 0) {
                ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
            }
            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.detailModel.TransactionAmount).toFixed(4) * 100).toFixed(4);
            }

        }
    }

    $scope.calculateTaxAmountForService = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
    };
    $scope.checkRowValidationService = function (x) {
        debugger;
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            //if (baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) || $scope.detailModel.TransactionAmount === 0) {
            //	ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
            //}
            if ($scope.taxCategoryList[i].Id === x.Id) {
                $scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.TransactionAmount).toFixed(4) * 100);
            }

        }
    }

    $scope.calculateTaxAmountForServiceModify = function (data) {
        if (baseService.isUndefinedOrNull(data.Percentage)) {
            data.Percentage = 0;
        }
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.checkRowValidationServiceModify = function (x) {
        debugger;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }

        }
    }
    $scope.checkRowValidation = function (x) {
        debugger;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            //if (baseService.isUndefinedOrNull($scope.HSNCode)) {
            //if ($scope.receiveTaxList[i].Percentage === 0) {
            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100);
            }
            //}
            //}
        }
    }


    //#endregion
    //#region Document Upload
    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.UserFilename;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $scope.DocumentSave = function () {
        debugger;
        //$scope.$broadcast("show-errors-check-validity");

        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.productDocMap.UserFilename = fileName;
        $scope.productDocMap.POId = $scope.productNew.Id;
        if (baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            ShowResult('Select Attachment file');
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
            if ($scope.productDocMap.UserFilename.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        for (var i = 0; i < $scope.Imagedata.length; i++) {
            var getRow = $filter("filter")($scope.Imagedata, { "UserFilename": $scope.productDocMap.UserFilename });
            if (getRow.length === 1) {
                ShowResult('File Already added');
                return false;
            }
        }
        if (angular.isUndefinedOrNull($scope.productNew.Id))
            ShowResult('Please select/save the GRN first', 'Error');
        else {
            try {
                var formData = new FormData();
                $http({
                    method: "POST",
                    url: 'Products/InventoryReceive/GRNDocCreate',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("GRNDocumentMap", angular.toJson($scope.productDocMap));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: {
                        "GRNDocumentMap": $scope.productDocMap,
                        "file": $scope.filedata,
                        "POId": $scope.productNew.Id,
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.ImagedataLoad();
                        $scope.productDocMap.UserFilename = "";
                        $scope.productDocMap.Description = "";
                        $scope.productDocMap.Remarks = "";
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;

            } catch (e) {
                throw ShowResult(e, "failure");
            }
        }
        return true;
    };
    $scope.Imagedata = [];
    $scope.ImagedataLoad = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryReceive/GRNDocumentMapData?POID=' + $scope.productNew.Id,
        }).then(function successCallback(response) { //datagatefun
            $scope.Imagedata = response.data;

        });
    };
    $scope.removePopUpForDoc = function (Id) {
        $scope.DocId = Id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUpForDoc')).modal('show');
    };
    $scope.DeletePOIgame = function (Id) {

        if (!baseService.isUndefinedOrNull($scope.DocId)) {
            $http({
                method: 'POST',
                url: 'Products/InventoryReceive/GRNImageDelete?Id=' + $scope.DocId,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ImagedataLoad();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }


    };
    $scope.CalculateRateAndAmount = function () {
        if (baseService.isUndefinedOrNull($scope.detailModel.ShortageQty)) {
            $scope.detailModel.ShortageQty = 0;
        }
        else if (baseService.isUndefinedOrNull($scope.detailModel.RejectionQty)) {
            $scope.detailModel.RejectionQty = 0;
        }
        else if (baseService.isUndefinedOrNull($scope.detailModel.DiscountAmount)) {
            $scope.detailModel.DiscountAmount = 0;
        }
        if (isNaN($scope.detailModel.ShortageQty)) {
            $scope.detailModel.ShortageQty = 0;
        }
        else if (isNaN($scope.detailModel.RejectionQty)) {
            $scope.detailModel.RejectionQty = 0;
        }
        else if (isNaN($scope.detailModel.DiscountAmount)) {
            $scope.detailModel.DiscountAmount = 0;
        }
        else if ($scope.detailModel.GrossAmount === 0 && $scope.productNew.IsFOC != true) {
            ShowResult('Enter the Gross Amount Grather than 0', 'failure', 'detailPopUp');
            return false;
        }
        else if ($scope.detailModel.TransactionQty === 0) {
            ShowResult('Enter the TransactionQty Grather than 0', 'failure', 'detailPopUp');
            return false;
        }
        else {
            $scope.detailModel.TransactionRate = parseFloat(($scope.detailModel.GrossAmount - $scope.detailModel.DiscountAmount) / $scope.detailModel.TransactionQty).toFixed(4);
            $scope.detailModel.TransactionAmount = parseFloat($scope.detailModel.GrossAmount - $scope.detailModel.DiscountAmount).toFixed(2);
        }

    }
    //#endregion
    //#region Additional Code
    $scope.advanceTaxesList = [];
    $scope.additionalTax = function () {
        for (var i = 0; i < $scope.advanceTaxesList.length; i++) {
            if ($scope.advanceTaxesList[i].TaxCodeId === $scope.advanceTax.TaxCodeId) {
                ShowResult("Tax Already Added");
                return false;
            }

        }

        if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboListWithhold, function (item) { return item.Id === $scope.advanceTax.TaxCodeId; })[0].UserName;
            $scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) { return item.Id === $scope.advanceTax.TaxCodeId; })[0].TaxCategoryId;
            $scope.advanceTaxesList.push($scope.advanceTax);
            $scope.advanceTax = {};
            $scope.TotalSumAfterTCS();
        }

    };

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $scope.productNew.TaxOptionAddiTax = 'Yes';
        $http({
            method: "Get",
            url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    $scope.taxCodCboListWithhold = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };
    //$scope.getTaxCodeByTaxYearWithhold($filter("dateFiltering")(Date.now()));
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].TaxCategoryId;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100
            //$scope.advanceTax.TaxAmount = (parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) * $scope.advanceTax.ValueOfFixed / 100);

            $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
        }
        $scope.TotalSumAfterTCS();
    }

    $scope.SaveAdditinalTaxInGRNList = function () {
        $http({
            method: 'POST',
            url: 'Products/InventoryReceive/SaveAdditinalTaxInGRN',
            data:
            {
                'InventoryReceiveId': $scope.productNew.Id,
                'UserSendData': $scope.advanceTaxesList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.TotalSumAfterTCS();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }



    $scope.GetAdvanceTaxInfo = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/InventoryReceive/GetAdvanceTaxInfo?InventoryReceiveId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;


        });

    }
    $scope.removeTaxesRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.advanceTaxesList.splice(index, 1);

        }
        else {
            $scope.DeleteAdditinalTax(Id);
            $scope.GetAdvanceTaxInfo($scope.productNew.Id);
        }
    };
    $scope.DeleteAdditinalTax = function (Id) {
        $http({
            method: 'POST',
            url: 'Products/InventoryReceive/AdditionalTaxDelete?Id=' + Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.TaxOptionAdditax = function (data) {
        debugger;
        $scope.productNew.TaxOptionAddiTax = data;
    };

    $scope.calculateTaxAmountForAdditionalTax = function (data) {
        $scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))).toFixed(2);
        $scope.advanceTax.TaxAmount = (($scope.TaxAmountVal * data) / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {
        debugger;
        $scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))).toFixed(2);
        $scope.advanceTax.ValueOfFixed = ((data / $scope.TaxAmountVal1) * 100).toFixed(4);
    }
    //$scope.TotalSumAfterTCSVal = "";
    $scope.TotalSumAfterTCS = function () {
        $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
    }

    //#endregion
    $scope.OrderSpecific = $scope.productNew.OrderSpecific;
    $scope.SelectedContract = function (obj) {
        //debugger;
        //var data = obj.data.ContractId;
        $scope.productNew.ContractId = obj.data.ContractId;
        $scope.productNew.CustomerName = obj.data.CustomerName;
        $scope.productNew.ContractNo = obj.data.ContractNo;
        $scope.productNew.LCRef = obj.data.LCRef;
        //console.log($scope.productNew);
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }
    $scope.SelectedMasterOrderByContract = function (obj) {
        //debugger;		
        $scope.detailModel.MasterOrderId = obj.data.MasterOrderId;
        $scope.detailModel.MasterOrderItemId = obj.data.MasterOrderItemId;
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }
    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }

    $scope.ClearMasterOrder = function () {
        $scope.detailModel.MasterOrderId = "";
    };

    $scope.ContractWiseData = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/ContractWiseData?ContractId=' + Id
        }).then(function successCallback(response) { //datagatefun
            $scope.productNew.ContractNo = response.data[0].ContractNo;
            $scope.productNew.LCRef = response.data[0].LCRef;
            $scope.productNew.CustomerName = response.data[0].CustomerName;
        });
    };

    //#region BOQPart
    $scope.inventoryMaterialList = [];
    //#region all Tab Function of POBOQItem Index

    $scope.IsOwnVendor = 'OwnVendor';
    $scope.tab1 = 1;
    $scope.setOwnVendorTabIndex = function (newTab) {

        $scope.IsOwnVendor = 'OwnVendor';
        $scope.GetBOQItemList();

        $scope.tab1 = newTab;

    };
    $scope.isSetOwnVendorIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabOtherVendorIndex = function (newTab) {
        //alert('tabCHR');

        $scope.IsOwnVendor = 'OtherVendor';
        $scope.GetListForMasterOrderOtherVendor = [];
        $scope.GetListForMasterOrdernew = [];
        $scope.taxCategoryList = [];
        $scope.groupList = [];
        $scope.Action1 = 'Save';
        $scope.ActionPOBOQ = 'Save';

        $scope.getalldataListForOtherVendorBOQList();
        $scope.tab1 = newTab;

    };
    $scope.isSetOtherVendorIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    $scope.setTabParentIndex = function (newTab) {


        $scope.IsOwnVendor = 'Parent';
        $scope.getalldataListForParentBOQList();
        $scope.tab1 = newTab;

    };
    $scope.isSetParentIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };


    //#endregion

    $scope.GetBOQItemList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.productNew.ContractId)) {
                throw "Select  Contract";
            }
            $scope.GetListForMasterOrder = [];
            $scope.GetListForMasterOrderOtherVendor = [];
            $scope.groupList = [];
            $scope.GetListForMasterOrdernew = [];
            $scope.taxCategoryList = [];
            $scope.groupList = [];
            $scope.Action1 = 'Save';


            $scope.getalldataListForBOQList();
            $scope.ActionPOBOQ = 'Save';
        } catch (e) {
            ShowResult(e, 'Info');
        }

    };

    $scope.GetListForMasterOrder = [];
    $scope.getalldataListForBOQList = function () {

        var gridObj = $("#GridReq").data("ejGrid");
        gridObj.clearFiltering();

        $scope.GetListForMasterOrder = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetBOQItems?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyId + '&IsOwnVendor=' + $scope.IsOwnVendor + '&inveReveiveMasterId=' + $scope.productNew.Id + '&istradingPO=' + $scope.productNew.IsTradingPO,
        }).then(function successCallback(response) { //datagatefun			
            $scope.GetListForMasterOrder = [];
            $scope.GetListForMasterOrder = response.data;
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $scope.processgroupList111();
        });
        $scope.Action1 = 'Save';
        $scope.processgroupList1();
    };
    $scope.groupList = [];
    $scope.processgroupList1 = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrder;
            $scope.GetListForMasterOrder = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrder.push($scope.newlistitems[i]);
                }
            }
        }
        $scope.Action1 = 'Save';
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.groupList = [];
    $scope.processgroupList111 = function () {

        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrder;
            $scope.GetListForMasterOrder = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.newlistitems[i].ThirdCharacteristicsValueId });
                //var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrder.push($scope.newlistitems[i]);
                }
            }
        }
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk111").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };
    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                $scope.GetListForMasterOrder[i].CheckedStatus = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridReq").data("ejGrid");
        gridObj.refreshContent();
    };
    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionQty", dataMember: "TransactionQty", format: "{0:N4}" }]
        /*,showCaptionSummary: true*/
    }];
    $scope.RequisitionListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');
    };

    $scope.GetListForMasterOrderOtherVendor = [];
    $scope.getalldataListForOtherVendorBOQList = function () {
        var gridObj = $("#GridReqeee").data("ejGrid");
        gridObj.clearFiltering();
        $scope.GetListForMasterOrder = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetBOQItems?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&IsOwnVendor=' + $scope.IsOwnVendor + '&inveReveiveMasterId=' + $scope.productNew.Id + '&istradingPO=' + $scope.productNew.IsTradingPO,
        }).then(function successCallback(response) { //datagatefun			
            $scope.GetListForMasterOrderOtherVendor = [];
            $scope.GetListForMasterOrderOtherVendor = response.data;
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $scope.processgroupListOtherVendor();
        });
        $scope.Action1 = 'Save';
        $scope.processgroupListOV();
    };
    $scope.processgroupListOtherVendor = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrderOtherVendor;
            $scope.GetListForMasterOrderOtherVendor = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.newlistitems[i].ThirdCharacteristicsValueId });
                //var getRow = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.newlistitems[i].MaterialMasterId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrderOtherVendor.push($scope.newlistitems[i]);
                }
            }
        }
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.processgroupListOV = function () {
        if ($scope.inventoryMaterialList.length > 0) {
            $scope.newlistitems = [];
            $scope.newlistitems = $scope.GetListForMasterOrderOtherVendor;
            $scope.GetListForMasterOrderOtherVendor = [];
            for (var i = 0; i < $scope.newlistitems.length; i++) {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
                if (getRow.length == 0) {
                    $scope.GetListForMasterOrderOtherVendor.push($scope.newlistitems[i]);
                }
            }
        }
        $scope.Action1 = 'Save';
        angular.element(document.querySelector('#ListOfPOMaterial')).modal('show');

    }
    $scope.GetListForMasterOrderParent = [];
    $scope.getalldataListForParentBOQList = function () {
        var gridObj = $("#GridReq3").data("ejGrid");
        gridObj.clearFiltering();
        $scope.GetListForMasterOrderParent = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetBOQItems?ContractId=' + $scope.productNew.ContractId + '&VendorId=' + $scope.productNew.PartyCode + '&IsOwnVendor=' + $scope.IsOwnVendor + '&inveReveiveMasterId=' + $scope.productNew.Id + '&istradingPO=' + $scope.productNew.IsTradingPO,
        }).then(function successCallback(response) { //datagatefun			
            $scope.GetListForMasterOrderParent = [];
            $scope.GetListForMasterOrderParent = response.data;
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();

        });
        $scope.Action1 = 'Save';

    };
    $scope.refreshQtyTemplete = function (args) {
        var gridObj = $("#GridReq").data("ejGrid");
        /*		gridObj.refreshContent();*/
        gridObj.refreshTemplate(true);
    }
    $scope.ConvertedDataRowList = [];
    $scope.GetListForMasterOrderTemp = [];
    $scope.ConvertedDataRow = function (data) {
        var gridObj = $("#GridReq").data("ejGrid");
        var gridObjUpdate = $("#PODetailUpdate").data("ejGrid");
        //var x = $event;
        //var res = x.data;
        ;
        $http({
            method: 'POST',
            url: $scope.path + 'ConverttedBOQUOMData',
            data: {
                'data': data
            },
            dataType: 'JSON'
        }).then(function (response) {
            $scope.ConvertedDataRowList = response.data;
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                if ($scope.GetListForMasterOrder[i].BOQId === $scope.ConvertedDataRowList.data.BOQId) {
                    $scope.GetListForMasterOrder[i].RequiredQtyPO = $scope.ConvertedDataRowList.data.RequiredQtyPO;
                    $scope.GetListForMasterOrder[i].OtherPOQty = $scope.ConvertedDataRowList.data.OtherPOQty;
                    $scope.GetListForMasterOrder[i].TransactionQty = $scope.ConvertedDataRowList.data.TransactionQty;

                }
            }
            gridObj.refreshContent(true);
            gridObjUpdate.refreshContent(true);

            gridObj.refreshTemplate();
            gridObjUpdate.refreshTemplate();

        });

    };
    $scope.tempList = [];
    $scope.UOMValidation = function () {
        var getRow3
        $scope.invalid = false;
        for (var i = 0; i < $scope.tempList.length; i++) {
            getRow3 = $filter("filter")($scope.tempList, { "MaterialMasterId": $scope.tempList[i].MaterialMasterId, "ArticleId": $scope.tempList[i].ArticleId, "FirstCharacteristicsValueId": $scope.tempList[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.tempList[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.tempList[i].ThirdCharacteristicsValueId });

        }
        $scope.TransactionUoMId = '';
        for (var k = 0; k < getRow3.length; k++) {
            $scope.TransactionUoMId = getRow3[0].TransactionUoMId;
            if (getRow3[k].TransactionUoMId != $scope.TransactionUoMId) {
                if ($scope.ActionPOBOQ === 'Update') {

                    ShowResult('Have you selected Same UOM?', 'failure', 'ListOfPOMaterial1');
                    return true;
                }
                else {
                    ShowResult('Have you selected Same UOM?', 'failure', 'ListOfPOMaterial');
                    return true;
                }


            }

        }
        return false;
    }
    $scope.detailPOSaveForBOQ = function () {
        try {
            $scope.check();
            $scope.GetListForMasterOrdernew = [];
            $scope.tempList = [];
            if ($scope.ActionPOBOQ === 'Save') {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    if ((baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty) || $scope.GetListForMasterOrder[i].TransactionQty === 0) && $scope.GetListForMasterOrder[i].CheckedStatus === true) {
                        ShowResult('Enter the Selected  Material Qty', 'failure', 'ListOfPOMaterial');
                        return false;
                    }

                    if ($scope.GetListForMasterOrder[i].CheckedStatus === true && $scope.GetListForMasterOrder[i].RequiredQtyApproved === 'Yes' && $scope.GetListForMasterOrder[i].IncompleteMaterial === 'No') {

                        if ($scope.ActionPOBOQ === 'Save') {
                            if ((parseFloat($scope.GetListForMasterOrder[i].TransactionQty) + parseFloat($scope.GetListForMasterOrder[i].OtherPOQty)) > parseFloat($scope.GetListForMasterOrder[i].RequiredQtyPO)) {
                                ShowResult('Trasaction qty can not grater than booking Qty', 'failure', 'ListOfPOMaterial');
                                $scope.GetListForMasterOrder[i].TransactionQty = '';
                                return false;
                            }
                            if (baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].TransactionQty)) {
                                ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].TransactionQty < 0) {
                                ShowResult('Negative Qty  not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].TransactionQty === 0 || $scope.GetListForMasterOrder[i].TransactionQty === 0.00 || $scope.GetListForMasterOrder[i].TransactionQty === 0.0) {
                                ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial');
                                return false;
                            }

                            if ($scope.GetListForMasterOrder[i].RequiredQtyApproved === 'No') {
                                ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure', 'ListOfPOMaterial');
                                return false;
                            }
                            if ($scope.GetListForMasterOrder[i].IncompleteMaterial === 'Yes') {
                                ShowResult('This is incomplete material.So you can not take this material', 'failure', 'ListOfPOMaterial');
                                return false;
                            }

                            else {
                                $scope.GetListForMasterOrder[i].check = true;
                                $scope.GetListForMasterOrder[i].Id = null;
                                $scope.GetListForMasterOrder[i].NetQty = $scope.GetListForMasterOrder[i].TransactionQty;
                                $scope.GetListForMasterOrder[i].BaseQty = $scope.GetListForMasterOrder[i].TransactionQty;
                                $scope.GetListForMasterOrder[i].TrnAmount = $scope.GetListForMasterOrder[i].TransactionQty * $scope.GetListForMasterOrder[i].TransactionRate;
                                $scope.GetListForMasterOrder[i].MaterialTranAmount = $scope.GetListForMasterOrder[i].TransactionQty * $scope.GetListForMasterOrder[i].TransactionRate;
                                $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);

                            }
                        }
                    }

                }
            }
            else if ($scope.ActionPOBOQ === 'Update') {
                for (var i = 0; i < $scope.GetListForMasterOrderUpdate.length; i++) {
                    if ((baseService.isUndefinedOrNull($scope.GetListForMasterOrderUpdate[i].TransactionQty) || $scope.GetListForMasterOrderUpdate[i].TransactionQty === 0) && $scope.GetListForMasterOrderUpdate[i].CheckedStatus === true) {
                        ShowResult('Enter the Selected  Material Qty', 'failure', 'ListOfPOMaterial1');
                        return false;
                    }

                    if ($scope.GetListForMasterOrderUpdate[i].CheckedStatus === true && $scope.GetListForMasterOrderUpdate[i].RequiredQtyApproved === 'Yes' && $scope.GetListForMasterOrderUpdate[i].IncompleteMaterial === 'No') {
                        if ((parseFloat($scope.GetListForMasterOrderUpdate[i].TransactionQty) + parseFloat($scope.GetListForMasterOrderUpdate[i].OtherPOQty)) > parseFloat($scope.GetListForMasterOrderUpdate[i].RequiredQtyPO)) {
                            ShowResult('Trasaction qty can not grater than required Qty', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if (baseService.isUndefinedOrNull($scope.GetListForMasterOrderUpdate[i].TransactionQty)) {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if ($scope.GetListForMasterOrder[i].GetListForMasterOrderUpdate < 0) {
                            ShowResult('Negative Qty  not allowed', 'failure', 'ListOfPOMaterial');
                            return false;
                        }
                        if ($scope.GetListForMasterOrderUpdate[i].TransactionQty === '0' || $scope.GetListForMasterOrderUpdate[i].TransactionQty === '0.00' || $scope.GetListForMasterOrderUpdate[i].TransactionQty === '0.0') {
                            ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if ($scope.GetListForMasterOrderUpdate[i].RequiredQtyApproved === 'No') {
                            ShowResult('Required Qty not yet Approved.So you can not take this material', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        if ($scope.GetListForMasterOrderUpdate[i].IncompleteMaterial === 'Yes') {
                            ShowResult('This is incomplete material.So you can not take this material', 'failure', 'ListOfPOMaterial1');
                            return false;
                        }
                        else {
                            $scope.GetListForMasterOrder[i].check = true;
                            $scope.GetListForMasterOrder[i].Id = null;
                            $scope.GetListForMasterOrder[i].NetQty = $scope.GetListForMasterOrder[i].TransactionQty;
                            $scope.GetListForMasterOrder[i].BaseQty = $scope.GetListForMasterOrder[i].TransactionQty;
                            $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrderUpdate[i]);
                        }

                    }
                }
            }


            for (var j = 0; j < $scope.GetListForMasterOrder.length; j++) {
                if ($scope.GetListForMasterOrder[j].CheckedStatus === true) {
                    $scope.tempList.push($scope.GetListForMasterOrder[j]);
                }
            }

            if ($scope.GetListForMasterOrdernew.length === 0) {
                if ($scope.ActionPOBOQ === 'Update') {

                    ShowResult('Please select at least one material', 'failure', 'ListOfPOMaterial');
                    return false;
                }
                else {
                    ShowResult('Please select at least one material', 'failure', 'ListOfPOMaterial');
                    return false;
                }

            }

            $scope.UOMValidation();
            $scope.groupList = [];
            $scope.processgroupList($scope.GetListForMasterOrder, $scope.groupList);
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                $scope.GetListForMasterOrder[i].Tolerance = $scope.productNew.Tolerance;
                $scope.GetListForMasterOrder[i].MaterialStorageId = $scope.productNew.MaterialStorageId;
            }
            for (var i = 0; i < $scope.groupList.length; i++) {
                $scope.groupList[i].Tolerance = $scope.productNew.Tolerance;
            }

            if ($scope.ActionPOBOQ === 'Save') {
                $scope.materialValidationForBOQItem();
                if (!$scope.UOMValidation()) {//$scope.invalid && 

                    $http({
                        method: 'POST',
                        url: 'Products/InventoryReceive/CreateGRNBYBOQ',
                        data:
                        {
                            'entity': $scope.productNew,
                            'entityMatAndImat': JSON.stringify($scope.GetListForMasterOrdernew),
                            'receiveTaxList': $scope.taxCategoryList,
                            'chargesListPO': $scope.chargesListPOnew,
                            'POServiceTaxList': $scope.POServiceTaxList,
                            'GRNType': 'GRN',
                            'AcceptanceId': $scope.AcceptanceId,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"



                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                        else {
                            ShowResult(response.data.Message, 'success', 'ListOfPOMaterial');
                            getInventoryMaterialList($scope.productNew.Id);
                            angular.element(document.querySelector('#ListOfPOMaterial')).modal('hide');
                            $scope.GetGRN();

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial');
                    };

                }
            }

            else if ($scope.ActionPOBOQ === "Update") {
                $scope.materialValidationForBOQItem();
                if (!$scope.UOMValidation()) {
                    $http({
                        method: 'POST',
                        url: 'Products/InventoryReceive/CreateGRNBYBOQ',
                        data:
                        {
                            'entity': $scope.productNew,
                            'entityMatAndImat': JSON.stringify($scope.GetListForMasterOrdernew),
                            'receiveTaxList': $scope.taxCategoryList,
                            'chargesListPO': $scope.chargesListPOnew,
                            'POServiceTaxList': $scope.POServiceTaxList,
                            'GRNType': 'GRN',
                            'AcceptanceId': $scope.AcceptanceId,
                            'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                            'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true)
                            ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial1');
                        else {
                            ShowResult(response.data.Message, 'success', 'ListOfPOMaterial1');
                            getInventoryMaterialList($scope.productNew.Id);
                            $scope.GetGRN();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ListOfPOMaterial1');
                    };

                }
            }

        } catch (e) {
            ShowResult(e, 'fail');
        }
    };
    $scope.check = function () {
        var aa = 0;
        for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
            if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
                aa++;

            }
        }
        if (aa === 0) {
            ShowResult('Your selected Material is not Approved.Please see Approved Coulmn!', 'failure', 'ListOfPOMaterial');
            return false;
        }

    }
    $scope.groupList = [];
    $scope.processgroupList = function (oldlist, newlist) {
        for (var i = 0; i < oldlist.length; i++) {
            var getRow = $filter("filter")(oldlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
            var ExistingRow = $filter("filter")(newlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
            // getRow.TransactionQty = $filter('sumByKey')($filter('filter')(oldlist), 'TaxAmount');
            if (ExistingRow.length === 0) {
                if (!baseService.isUndefinedOrNull(getRow[0].MaterialMasterId)) {

                    newlist.push(getRow[0]);
                }
            }

            var getRowWithoutMaterial = $filter("filter")(oldlist, { "MaterialDetail": oldlist[i].MaterialDetail, "RequisitionDetailId": oldlist[i].RequisitionDetailId });

            if (getRowWithoutMaterial.length === 1) {
                if (baseService.isUndefinedOrNull(getRowWithoutMaterial[0].MaterialMasterId)) {
                    newlist.push(getRowWithoutMaterial[0]);
                }
            }

        }
        return newlist;
    };
    $scope.materialValidationForBOQItem = function () {
        for (var i = 0; i < $scope.GetListForMasterOrdernew.length; i++) {
            var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "InventoryMaterialId": $scope.GetListForMasterOrdernew[i].MaterialMasterId, "ArticleId": $scope.GetListForMasterOrdernew[i].ArticleId, "FirstCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].ThirdCharacteristicsValueId });

            if (getRow3 == 0) {
                $scope.invalid = true;
            }
            else {
                ShowResult('Material Combination Already Exist', 'failure', 'ListOfPOMaterial');
                $scope.invalid = false;
            }
        }


    };
    //#endregion
    $scope.searchByOtherParty = "UserName"; $scope.searchOtherParty = "";
    $scope.searchByOtherPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    $scope.otherpartyList = [];
    $scope.showOtherPartyPopUpNew = function () {

        if ($scope.partyType === 'Vendor') {
            $scope.OtherPartyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
        }
        $http({
            method: 'POST',
            url: $scope.OtherPartyUrl,
            data: { column: $scope.searchByOtherParty, value: $scope.searchOtherParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.otherpartyList = response.data;
        });
        angular.element(document.querySelector('#partyOtherPopUp')).modal('show');
    };
    $scope.closeOtherPartyPopUp = function (x) {
        //if ($scope.partyIndex !== -1) {

        var party = x.data;// $scope.partyList[$scope.partyIndex];
        $scope.productNew.OtherPartyCode = party.Code;
        $scope.productNew.OtherPartyName = party.UserName;
        $scope.productNew.OtherPartyId = party.Id;

        //$scope.productNew.TaxApplicable = party.TaxApplicable;
        //$scope.productNew.IsTaxApplicableChangeable = party.IsTaxApplicableChangeable;
        getOtherPartyPlantList();
        //}
        $scope.hideOtherPartyPopUp();
    };
    function getOtherPartyPlantList() {
        $scope.OtherPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.OtherPartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.OtherPlantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.OtherInvoicingPartyPlantId = item.Value;
                    $scope.productNew.OtherDeliveryPartyPlantId = item.Value;
                    $scope.productNew.OtherInvoicingByAddress = item.Address1;
                    $scope.productNew.OtherDeliveryByAddress = item.Address1;
                    $scope.productNew.OtherInvoicingState = item.StateName;
                    $scope.productNew.OtherInvoicingGSTIN = item.GSTIN;
                    $scope.productNew.OtherDeliveryState = item.StateName;
                    $scope.productNew.OtherDeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }

    $scope.hideOtherPartyPopUp = function () {
        angular.element(document.querySelector('#partyOtherPopUp')).modal('hide');
    };

    $scope.OtherinvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#OtherinvoicingPartyPopUp')).modal('show');
    };
    $scope.closeOtherInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#OtherinvoicingPartyPopUp')).modal('hide');
    };

    $scope.otherserviceCboList = [];
    $scope.OtherserviceChargePopUp = function () {
        $scope.productNew.TaxOptionService = 'Yes';
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
            return ShowResult('Without material charges not aplicable.');
        $scope.taxCategoryList = null;
        $scope.OtherserviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.productNew.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: 0
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
        };
        $scope.getOtherService();
        angular.element(document.querySelector('#otherserviceChargePopUp')).modal('show');
    };
    $scope.getOtherService = function () {
        $http.get('Setups/CompanyServiceMaster/GetCboList')
            .then(function (response) {
                $scope.otherserviceCboList = response.data;
            });
    }


    $scope.closeOtherServiceChargePopUp = function () {
        $scope.OtherserviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#otherserviceChargePopUp')).modal('hide');
    };

    $scope.changeOtherService = function () {
        if (baseService.isUndefinedOrNull($scope.OtherserviceModel.ServiceMasterId))
            return getTaxCategoryList(hsnCodeId);//$scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.otherserviceCboList, function (item) { return item.Value === $scope.OtherserviceModel.ServiceMasterId; })[0].HSNCodeId;
        var HSNCode = $.grep($scope.otherserviceCboList, function (item) { return item.Value === $scope.OtherserviceModel.ServiceMasterId; })[0].HSNCode;
        getTaxCategoryList(hsnCodeId, HSNCode);
    };

    $scope.calculateotherSvcTaxCategory = function () {
        $scope.OtherserviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.OtherserviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.OtherserviceModel.TotalTaxAmount = (parseFloat($scope.OtherserviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.OtherserviceModel.TotalTaxAmount = 0;
    };
    $scope.OthersreviceSaveUrl = $scope.path + 'OtherVendorServiceChargesCreate';

    $scope.showOtherVendorChargesAlart = function () {
        $scope.message = 'Are you sure want to Save Other Vendor Charges? Other vendor Charges amount will not allocate For New Material Item';
        angular.element(document.querySelector('#OtherVendorChargePopUp')).modal('show');
    };

    $scope.OtherserviceSave = function () {
        try {
            $scope.manualValidationAddRemove('div_svcOther', 'OtherserviceModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRateOther', 'OtherserviceModel', 'TransactionAmount', 'Amount');
            $scope.OtherserviceModel.OtherPartyId = $scope.productNew.OtherPartyId;
            $scope.OtherserviceModel.OtherPartyPlantId = $scope.productNew.OtherInvoicingPartyPlantId;

            $http({
                method: 'POST',
                url: $scope.OthersreviceSaveUrl,
                data: {
                    entity: $scope.OtherserviceModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'otherserviceChargePopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'otherserviceChargePopUp');
                    $scope.serviceModel = {
                        Id: null
                        , ServiceMasterId: null
                        , InventoryReceiveId: $scope.productNew.Id
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TransactionAmount: 0
                        , BaseAmount: 0
                        , TotalTaxAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                    };
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    getServiceOtherVendorChargeList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'otherserviceChargePopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };

    function getServiceOtherVendorChargeList(inveReveiveId) {
        $http.get($scope.path + 'GetServiceOtherVendorChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.otherserviceList = [];
                $scope.otherserviceList = response.data;
            });
    }

    $scope.CheckSpecialCharecter = function () {
        try {
            if (containsSpecialChars($scope.productNew.DocRefNo)) {
                $scope.productNew.DocRefNo = $scope.productNew.DocRefNo.substring(0, $scope.productNew.DocRefNo.length - 1);
                throw "No special characters allowed for Doc Ref No.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
     function containsSpecialChars(str) {
        const specialChars = /[@!#$%^&*()_+\=\[\]{};':"|,.<>\?`~]/;
        return specialChars.test(str);
    }

}