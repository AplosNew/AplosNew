'use strict';
goodsReceiveNoteController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function goodsReceiveNoteController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Goods Receive Note"; //Inventory Receive
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/GoodsReceiveNote/';
    $scope.getListUrl = $scope.path + 'getlist';
	$scope.getListUrl1 = $scope.path + 'GetListForGRNSaveData';
    $scope.getListUrl2 = $scope.path + 'GetListForGrnByPoReq'; 
    
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl1 = $scope.path + 'UpdareGRN';
    
    //$scope.saveUrl = $scope.path + 'InsertGRN';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.chargesList = [];
    $scope.chargesListPO = [];
    $scope.storageList = [];
    $scope.currencyList = [];
    $scope.detailModelSave = [];
    $scope.inventoryMaterialListPOnew = [];
    $scope.chargesListPOnew = [];

    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    //, CAST(GRNDate AS DATE)

    //#region notification setting

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
                $scope.labelCheckAndApproved = $scope.productNew.labelCheckAndApproved;
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be approved by';
                $scope.labelCheckAndApproved = $scope.productNew.labelCheckAndApproved;
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
                $scope.labelCheckAndApproved = $scope.productNew.labelCheckAndApproved;
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
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };

    //#region GRN Detail
    $scope.lst = [];
	$scope.GRNListDetails = function () {
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
	$scope.GRNListDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(105));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
			columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    //#endregion





    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, "DESC", "GRNDate", "PartyName");
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
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    
    //InventoryReceive Model
   
    $scope.product = {       
        Id: null
        , GRNDate:$filter("dateFiltering")(Date.now())
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
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: $scope.partyType
        , POId:null
        , IsApproved: 0
        , CheckedBy: null
        , CheckedByStatus: null
        , AuthorizedBy: null
		, AuthorizedByStatus: null
        , NoteForAccounts: null
        ,labelCheckAndApproved: null
        ,CheckedByStatusForNoti: null
        ,ApprovedByStatusForNoti: null
    };

    
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

    $scope.showPartyByGateEntryPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListByGateEntry?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
            }
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };



    $scope.productNew = Object.assign({}, $scope.product);

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

    $http.get('accounts/OpeningBalance/GetACCCutOffDate')
        .then(function (response) {
            if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                $scope.productNew.CutOffDate = response.data.CutOffDate;
                $('#cutOffDate').datepicker('setStartDate', new Date($scope.productNew.CutOffDate));
            }
            else
                ShowResult('Cut Off date not found!', 'failure');
        });

    

    $scope.Get = function (index) {
      
        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
       // $scope.productNew.GRNDate = data.GRNDate;
        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        $scope.productId = $scope.productNew.Id;
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Save = function () {
        //debugger;
		try {
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
            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
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
                $scope.product.POId = $scope.POId;
               
                if ($scope.Action === "Save") {
                    for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                        if ($scope.inventoryMaterialListPO[i].check == true) {
                            $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);
                           
                        }
                        //else if ($scope.inventoryMaterialList[i].check == true) {                           
                        //    $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i]);
                        //}
                        //else {
                        //    ShowResult('Please select Material', 'failure');
                        //    break;
                        //}
                    }
                    for (var i = 0; i < $scope.chargesListPO.length; i++) {
                        if ($scope.chargesListPO[i].check == true) {
                            $scope.chargesListPOnew.push($scope.chargesListPO[i]);
                            //$scope.chargesListPOnew.push($scope.chargesList[i]);
                        }
                        //else if ($scope.chargesList[i].check == true) {                           
                        //    $scope.chargesListPOnew.push($scope.chargesList[i]);
                        //}
                        else {
                           
                        }
                    }
                   
                    //debugger;
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data:
                        {
                            'entity': $scope.product,
                            'entityMatAndImat': $scope.inventoryMaterialListPOnew,
                            'receiveTaxList': $scope.POMaterialTaxList,
                            'chargesListPO': $scope.chargesListPOnew,
							'POServiceTaxList': $scope.POServiceTaxList,
                            'GRNType': 'GRNBYREQPO',
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
                            $scope.Clear();
                            $scope.getalldataMaster();
                            $scope.GRNListDetails();
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productId = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                          //  $scope.Action = "Update";
                            $scope.getDataList();
                           
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    for (var i3 = 0; i3< $scope.inventoryMaterialList.length; i3++) {
                        if ($scope.inventoryMaterialList[i3].check == true) {
                            //$scope.inventoryMaterialListPOnew = [];
                            $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i3]);
                        }
                        else {
                            
                        }
                    }
                    for (var i4 = 0; i4 < $scope.chargesList.length; i4++) {
                        if ($scope.chargesList[i4].check == true) {
                            //$scope.chargesListPOnew = [];
                            $scope.chargesListPOnew.push($scope.chargesList[i4]);
                        }

                        else {

                        }
                    }
                    $http({
                        method: 'POST',
                        //url: $scope.updateUrl,
                        url: $scope.updateUrl1,
                        //data: $scope.product,
                        data:
                        {
                            'entity': $scope.product,
                            'entityMatAndImat': $scope.inventoryMaterialListPOnew,
                            'receiveTaxList': $scope.MaterialTaxList,
                            'chargesListPO': $scope.chargesListPOnew,
							'POServiceTaxList': $scope.ServiceTaxList,
                            'GRNType': 'GRNBYREQPO',
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
                            $scope.Clear();
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
        //debugger;
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

    $scope.Clear = function () {
        //debugger;
        $scope.inventoryMaterialListPO =[];
        $scope.inventoryMaterialListPOnew = [];
        $scope.GriddataSelected = [];
        ClearFields();
        return true;
    };

    function ClearFields() {
       
        $scope.Action = "Save";
      // $scope.product = { POId: $scope.product.POId };
        $scope.IsBaseOnDueDateEnable = false;
        $scope.inventoryMaterialListPO = [];
        $scope.chargesListPO = [];
        $scope.inventoryMaterialList = [];
        $scope.chargesList = [];

        $scope.grossTotal = 0;
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
            //, POId: $scope.product.POId            
            ,GRNDate: $filter("dateFiltering")(Date.now())
           
        };
      // $scope.POId1 = '';
        $scope.NotificationSettingStatus();
        baseService.removeErrorClasses();
        //$scope.getToCurrencyRate();
    }
    $scope.changeAllInvoice = function () {
        $scope.productNew.InvoiceNo = null;
        $scope.productNew.InvoiceDate = null;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.productNew.PartyCode = party.Code;
            $scope.productNew.PartyName = party.UserName;
            $scope.productNew.PartyId = party.Id;
            $scope.productNew.PaymentTermId = party.PaymentTermId;
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
        }
        $scope.hidePartyPopUp();
    };

    function getPartyPlantList() {
        //debugger;
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
    function getPartyPlantListPO() {
        //debugger;
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.partyId).then(function (response) {
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
    $scope.detailModelSave = {
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
    };
    $scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
    $scope.detailPopUp = function () {
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
        };
        $scope.clearCharNames();
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

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        $scope.clearCharNames();
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);

        getTaxCategoryList(ob.HSNCodeId);
        var mmId = []; mmId.push(ob.Id);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
            //$scope.detailModel.BaseUOMId = $filter("filter")($scope.uoMList, { IsBaseUom: 1 })[0].Value;
        });
        manualValidation('div_mm', false);
        manualValidation('div_country', false);
        $scope.closeMaterialMasterbyTypePopUp();
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            $scope.detailModel.MinimumValue = ob.MinimumValue;
            $scope.detailModel.MaximumValue = ob.MaximumValue;
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

    $scope.detailSave = function () {
        try {
            $scope.validation();
            $scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
            $scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            $scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

            for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
                if ($scope.detailModel.MaterialMasterId === $scope.inventoryMaterialList[i].MaterialMasterId &&
                    $scope.detailModel.ArticleId === $scope.inventoryMaterialList[i].ArticleId &&
                    $scope.detailModel.FirstCharacteristicsId === $scope.inventoryMaterialList[i].FirstCharacteristicsId &&
                    $scope.detailModel.FirstCharacteristicsValueId === $scope.inventoryMaterialList[i].FirstCharacteristicsValueId &&
                    $scope.detailModel.SecondCharacteristicsId === $scope.inventoryMaterialList[i].SecondCharacteristicsId &&
                    $scope.detailModel.SecondCharacteristicsValueId === $scope.inventoryMaterialList[i].SecondCharacteristicsValueId &&
                    $scope.detailModel.ThirdCharacteristicsId === $scope.inventoryMaterialList[i].ThirdCharacteristicsId &&
                    $scope.detailModel.ThirdCharacteristicsValueId === $scope.inventoryMaterialList[i].ThirdCharacteristicsValueId) {
                    return ShowResult('This material already received');
                }
            }

            $http({
                method: 'POST',
                url: $scope.detailSaveUrl,
                data: {
                    entity: $scope.detailModel
                    , taxCategoryList: $scope.taxCategoryList
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
                    };
                    $scope.taxCategoryList = [];
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.clearCharNames();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'detailPopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.GrnRequisitionAllocationSave = function () {

        //debugger;

        try {

            $scope.GetListForMasterOrdernew = [];
            for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {

                if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
                    $scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);
                }

            }
            // if ($scope.invalid) {
            if ($scope.Action1 === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
                    data: {
                        entity: $scope.GetListForMasterOrdernew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfRequisition');
                        $scope.Action1 = "Update";
                        $scope.GetListForMasterOrder = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                };

            }
            else if ($scope.Action1 === "Update") {
                $http({
                    method: 'POST',
                    url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
                    data: {
                        entity: $scope.GetListForMasterOrdernew
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                    else {
                        ShowResult(response.data.Message, 'success', 'ListOfRequisition');

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
                };

            }
            //}
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
        $scope.masterId5 = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId + '&POID=' + $scope.POID)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                $scope.inventoryMaterialList = response.data.Rows;
                $scope.POIDs = $scope.inventoryMaterialList.POId;
                $scope.productNew.CheckedBy = $scope.inventoryMaterialList[0].CheckedBy;
                $scope.productNew.PODate = $scope.inventoryMaterialList[0].AddedDate;
                checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetMaterialTaxData();
            });
    }

   
    
    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function getTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
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
    $scope.getReceiveTaxList = function (data, flag, index, Id) {
        //$scope.taxAbleAmnt = data.TrnAmount;
        //$scope.percentageColumn = flag;
        //$http({
        //    method: 'GET',
        //    url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + data.InventoryReceiveDetailId
        //}).then(function (response) {
        //    $scope.receiveTaxList = response.data;
        //    angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        //});

        //debugger;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.receiveTaxList = [];
        if (data.MaterialTaxList.length > 0) {
            $scope.HSNCode = data.MaterialTaxList[0].HSNCode;
            $scope.receiveTaxList = data.MaterialTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

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
    $scope.closeReceiveTaxPopUp = function () {
        //debugger;
        $scope.detailModel = {};
        $scope.receiveTaxList = [];  

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            //if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
            //    $scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
            //    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
            //}
            //else {
            //    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
            //}
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
                //$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate);


            }
            else {
                //data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate);
            }

        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }


    $scope.closeReceiveTaxPopUpValue = function (x) {
		//debugger;  
		if ($scope.Action === 'Save') {
			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
				var row = $filter('filter')($scope.new, { 'PODetailsID': $scope.inventoryMaterialListPO[i].PODetailsID });
				if (row.length != 0) {
					if ($scope.inventoryMaterialListPO[i].PODetailsID === row[0].PODetailsID) {
						$scope.inventoryMaterialListPO[i].ShortageRate = row[0].ShortageRate;
						$scope.inventoryMaterialListPO[i].ShortageValue = row[0].ShortageValue;
						$scope.inventoryMaterialListPO[i].RejectionRate = row[0].RejectionRate;
						$scope.inventoryMaterialListPO[i].RejectionValue = row[0].RejectionValue;
						$scope.inventoryMaterialListPO[i].RejectionClamRate = row[0].RejectionClamRate;
					}
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}
				else {
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}

			}
			angular.element(document.querySelector('#ValueSet')).modal('hide');
		}
		else {
			//for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			//    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId === $scope.PODetailsID) {
			//        $scope.inventoryMaterialList[i].ShortageRate = $scope.inventoryMaterialList[i].ShortageRate;
			//        $scope.inventoryMaterialList[i].ShortageValue = $scope.inventoryMaterialList[i].ShortageValue;
			//        $scope.inventoryMaterialList[i].RejectionRate = $scope.inventoryMaterialList[i].RejectionRate;
			//        $scope.inventoryMaterialList[i].RejectionValue = $scope.inventoryMaterialList[i].RejectionValue;
			//        $scope.inventoryMaterialList[i].RejectionClamRate = $scope.inventoryMaterialList[i].RejectionClamRate;
			//    }


			//}
			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
				var row = $filter('filter')($scope.new1, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
				if (row.length != 0) {
					if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
						$scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
						$scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
						$scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
						$scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
						$scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
					}
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}
				else {
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}
				angular.element(document.querySelector('#ValueSet')).modal('hide');
			}
			angular.element(document.querySelector('#ValueSet')).modal('hide');
		}
        //angular.element(document.querySelector('#ValueSet')).modal('hide');
   //     if ($scope.Action === 'Save') {
   //         for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
   //             var row = $filter('filter')($scope.new, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
   //             if (row.length != 0) {
   //                 if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
   //                     $scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
   //                     $scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
   //                     $scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
   //                     $scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
   //                     $scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
			//		}
			//		//angular.element(document.querySelector('#ValueSet')).modal('hide');
   //             }
   //             else {
   //                 angular.element(document.querySelector('#ValueSet')).modal('hide');
   //             }
			//	angular.element(document.querySelector('#ValueSet')).modal('hide');
			//}
			//angular.element(document.querySelector('#ValueSet')).modal('hide');
   //     }
   //     else {
   //         //for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
   //         //    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId === $scope.PODetailsID) {
   //         //        $scope.inventoryMaterialList[i].ShortageRate = $scope.inventoryMaterialList[i].ShortageRate;
   //         //        $scope.inventoryMaterialList[i].ShortageValue = $scope.inventoryMaterialList[i].ShortageValue;
   //         //        $scope.inventoryMaterialList[i].RejectionRate = $scope.inventoryMaterialList[i].RejectionRate;
   //         //        $scope.inventoryMaterialList[i].RejectionValue = $scope.inventoryMaterialList[i].RejectionValue;
   //         //        $scope.inventoryMaterialList[i].RejectionClamRate = $scope.inventoryMaterialList[i].RejectionClamRate;
   //         //    }


   //         //}
   //         for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
   //             var row = $filter('filter')($scope.new1, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
   //             if (row.length != 0) {
   //                 if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
   //                     $scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
   //                     $scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
   //                     $scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
   //                     $scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
   //                     $scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
			//		}
					
   //             }
   //             else {
   //                 angular.element(document.querySelector('#ValueSet')).modal('hide');
   //             }
			//	angular.element(document.querySelector('#ValueSet')).modal('hide');
   //         }
   //     }        
   //     angular.element(document.querySelector('#ValueSet')).modal('hide');
    }


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
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
            return ShowResult('Without material charges not aplicable.');
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
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryList(hsnCodeId);
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

   
    //Command By shakawat If need open
    //$scope.getServiceTaxListPO = function (data, flag) {
    //    //debugger;
    //    $scope.taxAbleAmnt = data.Amount + data.TotalTaxAmount;
    //    $scope.percentageColumn = flag;
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'GetServiceTaxListPO?serviceId=' + data.Id
    //    }).then(function (response) {
    //        $scope.receiveTaxListPO = response.data;
    //        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    //    });
    //}
  
    function getServiceChargeList(inveReveiveId) {
        $scope.masterId12 = inveReveiveId;
        //debugger;
        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = [];
                $scope.chargesList = response.data;              
                $scope.getServiceTaxList();
                
            });
    }
    
    // #endregion Service

    $scope.inventoryReceiveReport = function (id, reportFormat) {
        if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId);
    };
    $scope.Griddata = [];
    $scope.getalldata = function () {
		//debugger;
		var PoType = 'POByReq';
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/GetListOfPO?PoType=' + PoType,
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
            //entrydata = copy(searchdata);
        });
    };


    $scope.getListPOByReqG = [];
    $scope.getListPOByReq = function () {
        //debugger;
        var PoType = 'POByReq';
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/GetListForREqPOGRN?PoType=' + PoType,
        }).then(function successCallback(response) {
            $scope.getListPOByReqG = response.data;
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
            //entrydata = copy(searchdata);
        });
    };





    // #region shakawat
    $scope.POPopUp = function () {
        $scope.getalldata();
      
        angular.element(document.querySelector('#POPopUp')).modal('show');
        
    };

    $scope.POPopUpGRNPOReqList = function () {
        $scope.getListPOByReq();
        angular.element(document.querySelector('#POPopUp')).modal('show');

    };
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp')).modal('hide');
    };
    $scope.GriddataSelected = [];
    $scope.recorddoubleclick = function ($event) {
       
        $scope.Griddatatemp = [];
        $scope.Griddatatemp1 = [];
        var partyId = null;
        $scope.tempList = [];
        for (var j = 0; j < $scope.getListPOByReqG.length; j++) {
            if ($scope.getListPOByReqG[j].Active === true) {
                $scope.tempList.push($scope.getListPOByReqG[j]);
            }
        }
        var flagTemp = false;
        if ($scope.tempList.length > 0) {
            for (var k = 0; k < $scope.tempList.length; k++) {
                if ($scope.tempList[k].PartyId != $scope.tempList[0].PartyId) {// && $scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId
                    flagTemp = true;
                    // angular.element(document.querySelector('#POPopUp')).modal('hide');
                    ShowResult('Please select Same vendor', 'POPopUp');
                    return;

                }

            }
        }


        if (flagTemp == false) {

            var gridObj = $("#Grid").data("ejGrid");
            var $event = gridObj.getSelectedRecords()[0];
            var x = $event;
            var Id = x.Id;
            $scope.productNew = x;
            $scope.productId = "";
            $scope.POId = x.Id;
            $scope.productNew.DocRefNo = '';
            $scope.productNew.DocDate = '';
            $scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
            //$scope.product.POId = $scope.POId;
            var id1 = "''";
            for (var i = 0; i < $scope.getListPOByReqG.length; i++) {
                if ($scope.getListPOByReqG[i].Active === true) {
                    id1 += ",'" + $scope.getListPOByReqG[i].Id + "'";
                }
            }

            getPartyPlantList();
            //getPartyPlantEditList();
            GetInventoryMaterialListByPO(id1);
            getServiceChargeListPO(id1);
            $scope.GriddataSelected = [];
            for (var x = 0; x < $scope.getListPOByReqG.length; x++) {

                if ($scope.getListPOByReqG[x].Active === true) {
                    $scope.GriddataSelected.push($scope.getListPOByReqG[x]);
                }
            }

            $scope.POPopUpClose();
            if (!$rootScope.isCollapsed) $rootScope.toggle();


        }





        $scope.productNew.labelCheckAndApproved = $scope.labelCheckAndApproved;

        //debugger;

       
    }

    $scope.GetSavedPOListNew = [];
    $scope.GetSavedPOList1 = function (Id) {
        //debugger;
        var PoType = 'PO';
        $scope.GriddataSelected = [];
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetSavedPOList1?GRNId=' + Id,
        }).then(function successCallback(response) {
            //$scope.GetSavedPOListNew = [];
            $scope.GetSavedPOListNew = response.data;
            for (var i = 0; i < $scope.GetSavedPOListNew.length; i++) {
             
                $scope.GriddataSelected.push($scope.GetSavedPOListNew[i]);
            }

        });
        
    };

    $scope.POPopUpGateEntry = function () {
        $scope.getalldataGateEntry();
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
    };
    $scope.POPopUpCloseGateEntry = function () {
        angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
    };

    $scope.GriddataGateEntry = [];
    $scope.getalldataGateEntry = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/GetListOfPOGateEntry?partyCode='+ $scope.productNew.PartyId,
        }).then(function successCallback(response) {
            $scope.GriddataGateEntry = response.data;
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
    



  

    // Load tax with Material Data
    $scope.getReceiveTaxListPO = function (data, flag, index, Id) {
        //debugger;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;        
        $scope.receiveTaxList = [];
        if (data.POMaterialTaxList.length > 0) {
            $scope.HSNCode = data.POMaterialTaxList[0].HSNCode;
            $scope.receiveTaxList = data.POMaterialTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        
    };

    $scope.getReceiveTaxListPOValueSet = function (data, flag, index, Id) {
        //debugger;
        //$scope.TransactionRate = '';
        //$scope.ShortageQty = '';
        //$scope.RejectionQty ='';
        //$scope.PODetailsID = '';
        if ($scope.Action === 'Save') {
            $scope.ShortageRate = '';
            $scope.ShortageValue = '';
            $scope.RejectionRate = '';
            $scope.RejectionValue = '';
            $scope.RejectionClamRate = '';
            $scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
            $scope.UserName = data.UserName;
            $scope.StandardName = data.StandardName;
            $scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
            $scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
            $scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

            $scope.TransactionRate = data.TransactionRate;
            $scope.ShortageQty = data.ShortageQty;
            $scope.RejectionQty = data.RejectionQty;

            $scope.PODetailsID = data.PODetailsID;
            $scope.ShortageRate = data.ShortageRate;
            $scope.ShortageValue = data.ShortageValue;
            $scope.RejectionRate = data.RejectionRate;
            $scope.RejectionValue = data.RejectionValue;
            $scope.RejectionClamRate = data.RejectionClamRate;

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }
        else {
            $scope.ShortageRate = '';
            $scope.ShortageValue = '';
            $scope.RejectionRate = '';
            $scope.RejectionValue = '';
            $scope.RejectionClamRate = '';
            $scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
            $scope.UserName = data.UserName;
            $scope.StandardName = data.StandardName;
            $scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
            $scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
            $scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

            $scope.TransactionRate = data.TransactionRate;
            $scope.ShortageQty = data.ShortageQty;
            $scope.RejectionQty = data.RejectionQty;

            $scope.PODetailsID = data.InventoryReceiveDetailId;
            $scope.ShortageRate = data.ShortageRate;
            $scope.ShortageValue = data.ShortageValue;
            $scope.RejectionRate = data.RejectionRate;
            $scope.RejectionValue = data.RejectionValue;
            $scope.RejectionClamRate = data.RejectionClamRate;

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }
        

    };
    $scope.getReceiveTaxListPOValueSet1 = function (data, flag, index, Id) {
        //debugger;
        //angular.element(document.querySelector('#ValueSet')).modal('show');
        if ($scope.Action === 'Save') {
            //$scope.ShortageRate = '';
            //$scope.ShortageValue = '';
            //$scope.RejectionRate = '';
            //$scope.RejectionValue = '';
            //$scope.RejectionClamRate = '';
            //for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            //    $scope.MaterialGroupMasterName = $scope.inventoryMaterialListPO[i].MaterialGroupMasterName;
            //    $scope.UserName = $scope.inventoryMaterialListPO[i].UserName;
            //    $scope.StandardName = $scope.inventoryMaterialListPO[i].StandardName;
            //    $scope.FirstCharacteristicsValue = $scope.inventoryMaterialListPO[i].FirstCharacteristicsValue;
            //    $scope.SecondCharacteristicsValue = $scope.inventoryMaterialListPO[i].SecondCharacteristicsValue;
            //    $scope.ThirdCharacteristicsValue = $scope.inventoryMaterialListPO[i].ThirdCharacteristicsValue;

            //    $scope.TransactionRate = $scope.inventoryMaterialListPO[i].TransactionRate;
            //    $scope.ShortageQty = $scope.inventoryMaterialListPO[i].ShortageQty;
            //    $scope.RejectionQty = $scope.inventoryMaterialListPO[i].RejectionQty;

            //    $scope.PODetailsID = $scope.inventoryMaterialListPO[i].PODetailsID;
            //    $scope.ShortageRate = $scope.inventoryMaterialListPO[i].ShortageRate;
            //    $scope.ShortageValue = $scope.inventoryMaterialListPO[i].ShortageValue;
            //    $scope.RejectionRate = $scope.inventoryMaterialListPO[i].RejectionRate;
            //    $scope.RejectionValue = $scope.inventoryMaterialListPO[i].RejectionValue;
            //    $scope.RejectionClamRate = $scope.inventoryMaterialListPO[i].RejectionClamRate;
            //}
            //GetInventoryMaterialListByPO($scope.POId);
            //$scope.ShortageRate = '110';

            //$scope.CalculateShortageValPop = function () {
            //    //debugger;
            //    for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            //        $scope.inventoryMaterialListPO[i].ShortageRate = 110;
            //        $scope.inventoryMaterialListPO[i].ShortageValue = ($scope.inventoryMaterialListPO[i].TransactionRate * $scope.inventoryMaterialListPO[i].ShortageRate) / 100;
            //        $scope.inventoryMaterialListPO[i].RejectionRate = 50;
            //        $scope.inventoryMaterialListPO[i].RejectionValue = ($scope.inventoryMaterialListPO[i].TransactionRate * $scope.inventoryMaterialListPO[i].RejectionRate) / 100;
            //        $scope.inventoryMaterialListPO[i].RejectionClamRate = (100 - $scope.inventoryMaterialListPO[i].RejectionRate);
            //    }
            //}
            $scope.new = [];
            //$scope.new = $scope.inventoryMaterialListPO;
            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                if ($scope.inventoryMaterialListPO[i].check === true) {
                    if ($scope.inventoryMaterialListPO[i].ShortageQty > 0 || $scope.inventoryMaterialListPO[i].RejectionQty > 0) {
                        $scope.new.push($scope.inventoryMaterialListPO[i]);
                    }
                }
            }

            //$scope.inventoryMaterialListPO = [];
            for (var i = 0; i < $scope.new.length; i++) {
                if ($scope.new[i].check == true) {
                    if ($scope.new[i].ShortageQty > 0 || $scope.new[i].RejectionQty > 0) {
                        $scope.new[i].ShortageRate = 110;
                        $scope.new[i].ShortageValue = (($scope.new[i].ShortageQty * $scope.new[i].ShortageRate) / 100) * $scope.new[i].TransactionRate;
                        $scope.new[i].RejectionRate = 50;
                        $scope.new[i].RejectionValue = (($scope.new[i].RejectionQty * $scope.new[i].RejectionRate) / 100) * $scope.new[i].TransactionRate;
						$scope.new[i].RejectionClamRate = (100 - $scope.new[i].RejectionRate);

                    }
                }
            }

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }
        else {
            //$scope.ShortageRate = '';
            //$scope.ShortageValue = '';
            //$scope.RejectionRate = '';
            //$scope.RejectionValue = '';
            //$scope.RejectionClamRate = '';
            //$scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
            //$scope.UserName = data.UserName;
            //$scope.StandardName = data.StandardName;
            //$scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
            //$scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
            //$scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

            //$scope.TransactionRate = data.TransactionRate;
            //$scope.ShortageQty = data.ShortageQty;
            //$scope.RejectionQty = data.RejectionQty;

            //$scope.PODetailsID = data.InventoryReceiveDetailId;
            //$scope.ShortageRate = data.ShortageRate;
            //$scope.ShortageValue = data.ShortageValue;
            //$scope.RejectionRate = data.RejectionRate;
            //$scope.RejectionValue = data.RejectionValue;
            //$scope.RejectionClamRate = data.RejectionClamRate;

            //$scope.inventoryMaterialListPO = $scope.inventoryMaterialList;
            //$scope.new = [];
            //$scope.new = $scope.inventoryMaterialListPO;
            //$scope.inventoryMaterialList = [];
            //for (var i = 0; i < $scope.new.length; i++) {
            //    if ($scope.new[i].check == true) {
            //        if ($scope.new[i].ShortageQty > 0 || $scope.new[i].RejectionQty >0) {
            //            $scope.new[i].ShortageRate = 110;
            //            $scope.new[i].ShortageValue = ($scope.new[i].TransactionRate * $scope.new[i].ShortageRate) / 100;
            //            $scope.new[i].RejectionRate = 50;
            //            $scope.new[i].RejectionValue = ($scope.new[i].TransactionRate * $scope.new[i].RejectionRate) / 100;
            //            $scope.new[i].RejectionClamRate = (100 - $scope.new[i].RejectionRate);
            //            $scope.inventoryMaterialList.push($scope.new[i]);
            //        }
                   
            //    }
            //}
            //angular.element(document.querySelector('#ValueSet')).modal('show');

            $scope.new1 = [];
           
            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                if ($scope.inventoryMaterialList[i].check === true) {
                    if ($scope.inventoryMaterialList[i].ShortageQty > 0 || $scope.inventoryMaterialList[i].RejectionQty > 0) {
                        $scope.new1.push($scope.inventoryMaterialList[i]);
                    }
                }
            }
           
            for (var i = 0; i < $scope.new1.length; i++) {
                if ($scope.new1[i].check == true) {
                    if ($scope.new1[i].ShortageQty > 0 || $scope.new1[i].RejectionQty > 0) {
                        $scope.new1[i].ShortageRate = 110;
                        $scope.new1[i].ShortageValue = (($scope.new1[i].ShortageQty * $scope.new1[i].ShortageRate) / 100) * $scope.new1[i].TransactionRate;
                        $scope.new1[i].RejectionRate = 50;
                        $scope.new1[i].RejectionValue = (($scope.new1[i].RejectionQty * $scope.new1[i].RejectionRate) / 100) * $scope.new1[i].TransactionRate;
                        $scope.new1[i].RejectionClamRate = (100 - $scope.new1[i].RejectionRate);
                    }
                }
            }

            angular.element(document.querySelector('#ValueSet')).modal('show');
        }


    };
    $scope.CalculateShortageVal = function (x) {
        //debugger;
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].ShortageValue = (($scope.inventoryMaterialListPO[i].ShortageQty * $scope.inventoryMaterialListPO[i].ShortageRate) / 100) * $scope.inventoryMaterialListPO[i].TransactionRate;
        }
       

    }
    $scope.CalculateRejectionVal = function () {
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].RejectionValue = (($scope.inventoryMaterialListPO[i].RejectionQty * $scope.inventoryMaterialListPO[i].RejectionRate) / 100) * $scope.inventoryMaterialListPO[i].TransactionRate;
            $scope.inventoryMaterialListPO[i].RejectionClamRate = (100-$scope.inventoryMaterialListPO[i].RejectionRate);

            
        }
    }
    function GetInventoryMaterialListByPO(inveReveiveId) {
        //debugger;
        $scope.masterId = inveReveiveId;
        $http.get($scope.path + 'GetInventoryMaterialListByPO?inveReveiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.inventoryMaterialListPO = [];
                $scope.inventoryMaterialListPO = response.data.Rows;
                $scope.POID = $scope.inventoryMaterialListPO.POID;
                $scope.PreBal = $scope.inventoryMaterialListPO.Balance;
                $scope.PODetailsID = $scope.inventoryMaterialListPO.InventoryReceiveDetailId;
                $scope.productNew.InvoicingByAddress = $scope.inventoryMaterialListPO[0].InvoicingByAddress;
                $scope.productNew.DeliveryByAddress = $scope.inventoryMaterialListPO[0].DeliveryByAddress;
                $scope.inventoryMaterialListPO.BaseAmount = '0';
                //$scope.POId1 = '';
                checkSameValueInColumnList($scope.inventoryMaterialListPO, 'TransactionUoM');
                getGrossAmount($scope.inventoryMaterialListPO, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
                $scope.GetPOMaterialTaxData();
                $scope.POPopUpClose();
            });
    }  
    $scope.GetPOMaterialTaxData = function () {
        //debugger;
        $scope.POMaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxListPO?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.POMaterialTaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
                var linepk = $scope.inventoryMaterialListPO[i].InventoryReceiveDetailId;
                var list = getPOMaterialtaxlist(linepk);
                $scope.inventoryMaterialListPO[i].POMaterialTaxList = list;
            }
        });
    };
    function getPOMaterialtaxlist(linepk) {
        //debugger;
        var result = [];
        for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
            if ($scope.POMaterialTaxList[i].PODetailId === linepk) {
                result.push($scope.POMaterialTaxList[i]);
            }
        }
        return result;
    }

    $scope.GetMaterialTaxData = function () {
        //debugger;
        $scope.MaterialTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId5
        }).then(function (response) {
            $scope.MaterialTaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = getMaterialtaxlist(linepk);
                $scope.inventoryMaterialList[i].MaterialTaxList = list;
            }
        });
    };
    function getMaterialtaxlist(linepk) {
        //debugger;
        var result4 = [];
        for (var i = 0; i < $scope.MaterialTaxList.length; i++) {
            if ($scope.MaterialTaxList[i].PODetailId === linepk) {
                result4.push($scope.MaterialTaxList[i]);
            }
        }
        return result4;
    }

    //Load Service Tax with service charge
    function getServiceChargeListPO(inveReveiveId) {
        $scope.masterId1 = inveReveiveId;
        $http.get($scope.path + 'GetServiceChargeListPO?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesListPO = [];
                $scope.chargesListPO = response.data;
                $scope.GetPOServiceTaxData();
            });
    }
    $scope.GetPOServiceTaxData = function () {
        //debugger;
        $scope.POServiceTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetServiceTaxListPO?serviceId=' + $scope.masterId1
        }).then(function (response) {
            $scope.POServiceTaxList = response.data;

            for (var i = 0; i < $scope.chargesListPO.length; i++) {
                var linepk = $scope.chargesListPO[i].Id;
                var list1 = getPOServicetaxlist(linepk);
                $scope.chargesListPO[i].POServiceTaxList = list1;
            }
        });
    };
    function getPOServicetaxlist(linepk1) {
        //debugger;
        var result1 = [];
        for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
            if ($scope.POServiceTaxList[i].InventoryServiceId === linepk1) {
                result1.push($scope.POServiceTaxList[i]);
            }
        }
        return result1;
    }
    function getServicetaxlist1(linepk111) {
        //debugger;
        var result11 = [];
        for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
            if ($scope.ServiceTaxList[i].InventoryServiceId === linepk111) {
                result11.push($scope.ServiceTaxList[i]);
            }
        }
        return result11;
    }
    $scope.getServiceTaxList = function () { //,data, flag)
        //$scope.taxAbleAmnt = data.Amount + data.TotalTaxAmount;
        //$scope.percentageColumn = flag;
    
        $http({
            method: 'GET',
            url: $scope.path + 'GetServiceTaxList?serviceId=' + $scope.masterId12//data.Id
        }).then(function (response) {
            $scope.ServiceTaxList = response.data;
            for (var i = 0; i < $scope.chargesList.length; i++) {
                var linepk1 = $scope.chargesList[i].Id;
                var list11 = getServicetaxlist1(linepk1);
                $scope.chargesList[i].ServiceTaxList = list11;
            }
           // angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });
    }



    $scope.getServiceTaxListPOPOP = function (data, flag, index, Id) {
        //debugger;
        $scope.taxAbleAmnt = data.Amount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.Amount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.ServiceTaxList = [];
        if (data.POServiceTaxList.length > 0) {
            $scope.HSNCode = data.POServiceTaxList[0].HSNCode;
            $scope.ServiceTaxList = data.POServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
            $scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
        //$http({
        //    method: 'GET',
        //    url: $scope.path + 'GetServiceTaxListPO?serviceId=' + data.Id
        //}).then(function (response) {
        //    $scope.receiveTaxList = response.data;
        //    angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
        //});
        

    }


    $scope.getServiceTaxListPOPOP1 = function (data, flag, index, Id) {
        //debugger;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.ServiceTaxList = [];
        if (data.ServiceTaxList.length > 0) {
            $scope.HSNCode = data.ServiceTaxList[0].HSNCode;
            $scope.ServiceTaxList = data.ServiceTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
            $scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
        //$http({
        //    method: 'GET',
        //    url: $scope.path + 'GetServiceTaxListPO?serviceId=' + data.Id
        //}).then(function (response) {
        //    $scope.receiveTaxList = response.data;
        //    angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
        //});

    }

    $scope.closeReceiveTaxPopUp1 = function () {
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            //if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
            //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
            $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //}
            //else {
            //    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //}
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate);

            }
            else {
                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate);
            }

        }
        angular.element(document.querySelector('#ServiceTaxPopUp')).modal('hide');
    }

    //$scope.enable = true;
    //$scope.selectAll = function () {
    //    // Loop through all the entities and set their isChecked property
    //    for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
    //        $scope.model.entities[i].isChecked = $scope.model.allItemsSelected;
    //    }
    //};



    $scope.index = -1;
    $scope.staus = true;
    $scope.enableid = true;
    $scope.Change = function (event, index,x) {
        //debugger;
        if (baseService.isUndefinedOrNull(x.TransactionQty)) {           
            ShowResult('Enter the current qty', 'failure');
        }
        else {
            if (event.currentTarget.checked) {
                $scope.index = index;
                //$scope.staus = false;
                x.enableid = false;
            
             if (x.POQty === (x.GRNRcvQty + x.TransactionQty)) {
                    x.POClosStatus = true;
                }
             else if (x.POQty > (x.GRNRcvQty + x.TransactionQty)) {
                 $scope.PODetailId = x.PODetailId;
                 $scope.message = 'Are you want to close this PO line item?';
                 angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');
                }
            }
            else {
                x.enableid = true;
                //$scope.index = index;
                x.POClosStatus = false;
                x.TransactionQty = "";
                x.Balance = x.POQty - x.GRNRcvQty;//parseFloat(x.POQty - x.GRNRcvQty).toFixed(2);
            }
        }
        
    }
    $scope.YesMessageForClosed = function ($event) {
        //debugger
        
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].check === true) {
                if ($scope.inventoryMaterialListPO[i].PODetailId === $scope.PODetailId) {
                    $scope.inventoryMaterialListPO[i].POClosStatus = true;
                }
            }
            else {
                $scope.inventoryMaterialListPO[i].POClosStatus = false;
            }
        }
    }
    $scope.NoMessageForClosed = function ($event) {
        //debugger
        
        for (var i = 0; i < $scope.inventoryMaterialListPO.length;  i++) {
            if ($scope.GetListForMasterOrder[i].check === true) {
                if ($scope.GetListForMasterOrder[i].PODetailId === $scope.PODetailId) {
                    $scope.inventoryMaterialListPO[i].POClosStatus = false;
                }
            }
            else {
                $scope.GetListForMasterOrder[i].WantToClose = false;
            }
        }
    }






    $scope.calculateRate = function (data, event) {
        //debugger;
        data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.POMaterialTaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        //if ($scope.productNew.IsNonCreditable==1) {
        //    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //    data.BaseAmount = data.TrnAmount+ data.BaseTaxAmount;
        //}
        //else {
        //    data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        //}

    };
    $scope.calculateAmount = function (data,index) {
        //debugger;
        data.check = false;
        data.POClosStatus = false;
        $scope.PreBal = data.Balance;
        
       // data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount == 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        data.BaseTaxAmount = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');      
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');

        angular.forEach(data.POMaterialTaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;
            data.BaseTaxAmount += item.TaxAmount;
           
        });
        
        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            $scope.inventoryMaterialListPO[i].Balance = '';
            if ($scope.inventoryMaterialListPO[i].POQty < ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty)) {
                $scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Current quantity can not grater than balance qty!', 'failure');
                $scope.inventoryMaterialListPO[i].TransactionQty = '';
            }
            else if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                //$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');

            }
            else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
                //$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
                ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
            }
            else {
                if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                    $scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
                    //$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    //$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                    //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                    $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

                }
                else {
                    //$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    //$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
                    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
                    //$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty+$scope.inventoryMaterialListPO[i].TransactionQty));
                    $scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                    //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
                    $scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
                }
                if ($scope.productNew.IsNonCreditable == 1) {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
                        //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
                        //$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
                        $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                    }

                }

                else {
                    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {

                        //data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
                        $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
                        $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
                    }
                }
            }
        }
        //angular.forEach($scope.inventoryMaterialListPO, function (item) {
        //    item.ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * data.TrnAmount;

        //});
      
        //$scope.detailModel.BaseUOMId = $filter("filter")($scope.chargesListPO, { IsBaseUom: 1 })[0].Value;
       
        // data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        
    };

    $scope.calculateAmount1 = function (data) {
        //debugger;
        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount == 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.MaterialTaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;
            data.BaseTaxAmount += item.TaxAmount;
        });
        // data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
        for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
            if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                $scope.inventoryMaterialList[i].TrnAmount = data.TrnAmount;
                $scope.inventoryMaterialList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                $scope.inventoryMaterialList[i].ShortageQty = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].RejectionQty);
            }
            else {
                $scope.inventoryMaterialList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
                $scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                $scope.inventoryMaterialList[i].ShortageQty = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
                //$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
                $scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].RejectionQty);
            }
            if ($scope.productNew.IsNonCreditable == 1) {
                if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
                    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
                    //$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
                    $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax);
                    $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate);

                }
            }
            else {
                if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId === data.InventoryReceiveDetailId) {
                    //data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
                    $scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
                    $scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate);
                }
            }
        }

       





    };
    // #endregion



    


    $scope.enableid1 = true;
    $scope.enableid3 = true;
    $scope.Change1 = function (event, index, x) {

        if (event.currentTarget.checked) {
            $scope.index = index;
            //$scope.staus = false;
            x.enableid1 = false;
            x.check == true;
        }


        else {
            x.enableid1 = true;
            x.check == false;
            //$scope.index = index;
        }
    }
    $scope.enableid2 = true;
    $scope.Change2 = function (event, index, x) {

        if (event.currentTarget.checked) {
            $scope.index = index;
            //$scope.staus = false;
            $scope.enableid2 = false;
            x.check == true;
        }


        else {
            $scope.enableid2 = true;
            //$scope.index = index;
            x.check == false;
        }
    }


    $scope.calculateAmountForServiceCharge = function (data) {
        //debugger;
        //data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        //if (data.TrnAmount == 'NaN')
        //    data.TrnAmount = 0;
        //data.TaxAmount = 0;
        data.TotalTaxAmount = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
       
        for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
            if ($scope.POServiceTaxList[i].InventoryServiceId == data.Id) {
                $scope.POServiceTaxList[i].TaxAmount = data.Amount * $scope.POServiceTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.POServiceTaxList[i].TaxAmount;
            }
        }
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');
        // data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;

        
        //for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
        //    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
        //        $scope.inventoryMaterialListPO[i].Amount = data.Amount;
        //        $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / data.Amount) * $scope.inventoryMaterialListPO[i].Amount;
        //        $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / data.Amount) * $scope.inventoryMaterialListPO[i].Amount;
        //    }
        //    else {
        //        $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / data.Amount) * $scope.inventoryMaterialListPO[i].TrnAmount;
        //        $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / data.Amount) * $scope.inventoryMaterialListPO[i].TrnAmount;
        //    }
        //    if ($scope.productNew.IsNonCreditable == 1) {
        //        //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //        $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + $scope.inventoryMaterialListPO[i].ServiceCharge + data.ServiceTax;

        //    }
        //    else {
        //        data.BaseAmount = parseFloat(data.TrnAmount) + data.ServiceCharge;
        //    }

        //}


        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            //if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
            //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
            $scope.inventoryMaterialListPO[i].ServiceCharge = (parseFloat(TotalServiceAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
            $scope.inventoryMaterialListPO[i].ServiceTax = (parseFloat(TotalServiceTaxAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
            //}
            //else {
            //    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //}
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
               
               // $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = $scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount;
                //$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);

                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);


            }
            else
            {

                $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) ).toFixed(2);
                $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount)  + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);


                //data.TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
                //data.TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate);
            }

        }

    };


    $scope.calculateAmountForServiceCharge1 = function (data) {
        ////debugger;
        ////data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        ////if (data.TrnAmount == 'NaN')
        ////    data.TrnAmount = 0;
        ////data.TaxAmount = 0;
        //data.TotalTaxAmount = 0;
        //for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
        //    if ($scope.ServiceTaxList[i].InventoryServiceId == data.Id) {
        //        $scope.ServiceTaxList[i].TaxAmount = data.Amount * $scope.ServiceTaxList[i].Percentage / 100;
        //        data.TotalTaxAmount += $scope.ServiceTaxList[i].TaxAmount;
        //    }
        //}
        //// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        ////data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;


		//debugger;
		
		data.TotalTaxAmount = 0;
		var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
		var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');

		for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
			if ($scope.ServiceTaxList[i].InventoryServiceId == data.Id) {
				$scope.ServiceTaxList[i].TaxAmount = data.Amount * $scope.ServiceTaxList[i].Percentage / 100;
				data.TotalTaxAmount += $scope.ServiceTaxList[i].TaxAmount;
			}
		}
		var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
		


		


		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			
			$scope.inventoryMaterialList[i].ServiceCharge = (parseFloat(TotalServiceAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
			$scope.inventoryMaterialList[i].ServiceTax = (parseFloat(TotalServiceTaxAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
			
			if ($scope.productNew.IsNonCreditable == 1) {
				

				$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)).toFixed(2);
				$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);


			}
			else {
				data.TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge).toFixed(2);
				data.TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate);
			}

		}
    };


    //#region  GRNReport

    $scope.GRNReport = function (data) {
        
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
    };

    //#endregion

    $scope.calculateMaterialTax = function (data, index) {
        //debugger;
        // data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        //data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        //if (data.TrnAmount == 'NaN')
        //    data.TrnAmount = 0;
        //data.TaxAmount = 0;
        //data.BaseTaxAmount = 0;
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');      
        var TotalMaterialTaxAmount = $filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount');   

        //angular.forEach(data.POMaterialTaxList, function (item) {
        //    item.TaxAmount = data.TrnAmount * item.Percentage / 100;
        //    data.BaseTaxAmount += item.TaxAmount;

        //});

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailId) {
                //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
                $scope.inventoryMaterialListPO[i].BaseTaxAmount = TotalMaterialTaxAmount;
                $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
                $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalMaterialTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            }
            else {
                $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
                $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalMaterialTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            }
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(4);

            }
            else {
                data.BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(4);
            }

        }
        //angular.forEach($scope.inventoryMaterialListPO, function (item) {
        //    item.ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * data.TrnAmount;

        //});

        //$scope.detailModel.BaseUOMId = $filter("filter")($scope.chargesListPO, { IsBaseUom: 1 })[0].Value;

        // data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;

    };

    $scope.calculateSerciceTax = function (data) {
        //debugger;       
        var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
        var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
        var ServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'TotalTaxAmount');

        for (var i = 0; i < $scope.chargesListPO.length; i++) {
            if ($scope.chargesListPO[i].Id == data.InventoryServiceId) {
                $scope.chargesListPO[i].TotalTaxAmount = ServiceTaxAmount;
            }
        }

        for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
            //if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
            //$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;  
            //$scope.inventoryMaterialListPO[i].ServiceTax = Math.round(Math$scope.chargesListPO[i].TotalTaxAmount);
            $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
            //}
            //else {
            //    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
            //}
            if ($scope.productNew.IsNonCreditable == 1) {
                //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
                $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(4);

            }
            else {
                data.BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(4);
            }

        }

    };
    $scope.onClickReportDownloadExcel = function (args) {
        //debugger;
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id )) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

    };
   
    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            BackgroundColor: "Black",
            Color: "White",
            width: "50",
            height: "20",
            contentType: "imageonly",
            prefixIcon: "e-icon e-dataexport",
           
            //prefixIcon: "e-icon e-edit" ,
            //prefixIcon: "e-icon e-delete",
            //prefixIcon: " e-icon e-save",
            //prefixIcon: " e-icon e-cancel",
            
            click: $scope.onClickReportDownloadExcel
        }
    }];
    $scope.onClickReportDownloadPdf = function (args) {
        //debugger;
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

    };
    $scope.commandPdf = [{
        type: "details", buttonOptions: {
            text: "Pdf",
            width: "50",
            height: "20",
            contentType: "imageonly",
            prefixIcon: "e-icon e-dataexport",
           
            //prefixIcon: "e-icon e-edit" ,
            //prefixIcon: "e-icon e-delete",
            //prefixIcon: " e-icon e-save",
            //prefixIcon: " e-icon e-cancel",

            click: $scope.onClickReportDownloadPdf
        }
    }];


   



    $scope.Get = function (index) {
		//debugger;
        $scope.index = index;
        $scope.product = $scope.products[$scope.index];
        $scope.productNew = Object.assign({}, $scope.product);
        // $scope.productNew.GRNDate = data.GRNDate;
        getPartyPlantList();
        getInventoryMaterialList($scope.productNew.Id);
        getServiceChargeList($scope.productNew.Id);
        $scope.productId = $scope.productNew.Id;
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };



    $scope.recorddoubleclickFromMasterGrid = function ($event) {
        ClearFields();
        var x = $event;
        var Id = x.data.Id;
        //debugger;
        //$scope.POId = x.data.POID;
        $scope.POId1 = x.data.POID;
        //$scope.index = index;
        $scope.POID = x.data.POID;
        $scope.product = $scope.products[$scope.index];
        //$scope.productNew = Object.assign({}, $scope.product);
        $scope.productNew = x.data;
        $scope.productNew.GRNDate = x.data.GRNDate1;
        $scope.productNew.CheckedBy = x.data.CheckedBy;
        getPartyPlantList();
        getInventoryMaterialList(Id);
        getServiceChargeList(Id);
        $scope.productId = Id;
        $scope.GetSavedPOList1(Id);
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
        //$scope.getToCurrencyRate();
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate')
                    $scope.IsBaseOnDueDateEnable = true;
                else
                    $scope.IsBaseOnDueDateEnable = false;
        }
     
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();



        //var x = $event;
        //var Id = x.data.Id;
        ////alert('Id'+Id);
        //$scope.productNew = x.data;
        //$scope.productId = "";
        //$scope.POId = x.data.Id;
        //$scope.product.POId = $scope.POId;
        //getPartyPlantList();
        ////getPartyPlantEditList();
        //GetInventoryMaterialListByPO(Id);
        //getServiceChargeListPO(Id);         //$scope.productNew.Id
        ////if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
        ////    var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
        ////    if (paymentTerm.BaseLineDate !== null)
        ////        if (paymentTerm.BaseLineDate === 'documentdate')
        ////            $scope.IsBaseOnDueDateEnable = true;
        ////        else
        ////            $scope.IsBaseOnDueDateEnable = false;
        ////}
        ////$scope.Action = 'Update';
        //if (!$rootScope.isCollapsed) $rootScope.toggle();






    }





    //$scope.PODetailsUpdatePOPUp = function (x) {
    //    //debugger;
    //    $scope.Action1 = 'Update'
    //    // $scope.GetListForMasterOrder = [];
    //    getInventoryMaterialListForUpdate(x);
    //    // $scope.GerRequisition();
    //    angular.element(document.querySelector('#ListOfRequisition')).modal('show');
    //};

    //function getInventoryMaterialListForUpdate(inveReveiveId) {
    //    $scope.masterId = inveReveiveId;
    //    //debugger;
    //    //$scope.inventoryMaterialList = [];
    //    $http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId)
    //        .then(function (response) {
    //            $scope.GetListForMasterOrder = response.data;

    //        });
    //}
    $scope.MasterOrderListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
    };  
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.RowColor != e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.RowColor = e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#D3D3D3');
        else
            e.row.css("background-color", '#ffffff');


    }
	$scope.PODetailsUpdatePOPUp = function (x, MaterialMasterId, InventoryReceiveDetailId) {
        //debugger;
        $scope.Action1 = 'Update'
        // $scope.GetListForMasterOrder = [];
		getInventoryMaterialListForUpdate(x, MaterialMasterId, InventoryReceiveDetailId);
        // $scope.GerRequisition();
        angular.element(document.querySelector('#ListOfRequisition')).modal('show');
    };
    $scope.GetListForMasterOrder = [];
	function getInventoryMaterialListForUpdate(inveReveiveId, MaterialMasterId, InventoryReceiveDetailId) {
        $scope.Action1 = 'Save';
        $scope.masterId = inveReveiveId;
        //debugger;
        //$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId + '&InventoryReceiveId=' + $scope.productNew.Id + '&MaterialMasterId=' + MaterialMasterId + '&InventoryReceiveDetailId=' + InventoryReceiveDetailId)
            .then(function (response) {
                $scope.GetListForMasterOrder = response.data;
                $scope.totalGRNVal = $scope.GetListForMasterOrder[0].GRNQty;
                $scope.RejectionQty = $scope.GetListForMasterOrder[0].RejectionQty;
            });

       
    }

    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function getTaxList(inveReveiveId) {
        //debugger;
        $http({
            method: 'GET',
            url: $scope.path + 'GetTaxCategoryListPO?receiveDetailId=' + inveReveiveId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
            //$scope.HSNCode = response.data[0]['HSNCode'];
            //angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });
    }
    function checkChangeemployee(e) {
        //debugger;
        //alert('dd');
        var val = e.model.value;
        var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
        // $scope.hsnCodeId = $event.data.hsnCodeId;

        //item level check
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check") {
                row[0].CheckedStatus = true;

            }
            else
                row[0].CheckedStatus = false;
        }
        //if ($scope.Action1 === 'Save') {
        //    getTaxCategoryList(row[0].HSNCodeId);
        //}
        //else {
        //    getTaxCategoryList(row[0].HSNCodeId);
        //    getTaxList(row[0].InventoryReceiveId)
        //   // getTaxCategoryList(row[0].InventoryReceiveId);
        //}



    }
    function headCheckChangeemployee(e) {
        var val = e.model.value;
        var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

        if (e.model.checkState == "check") {
            // alert('2');

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    $scope.GetListForMasterOrder[i].CheckedStatus = true;
                }
            }
            else {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
                            $scope.GetListForMasterOrder[i].CheckedStatus = true;
                    }

                }
            }

            var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    $scope.GetListForMasterOrder[i].CheckedStatus = false;
                }
            }
            else {
                for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
                            $scope.GetListForMasterOrder[i].CheckedStatus = false;
                    }

                }
            }
            var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#GridReq .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        //alert('fff');
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckedStatus == true)
                $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }

    



    //$scope.GetListForMasterOrder = [];
    //$scope.getalldataListForUpdatePoDetails = function () {
    //    $scope.GetListForMasterOrder = [];
    //    $http({
    //        method: "GET",
    //        dataType: 'JSON',
    //        //url: $scope.getSearchListUrl,
    //        url: 'Products/PurchaseOrder/GetInventoryMaterialListForPOUpdate',
    //    }).then(function successCallback(response) { //datagatefun
    //        $scope.GetListForMasterOrder = response.data;
    //        //entrydata = copy(searchdata);
    //    });
    //};

    $scope.tab1 = 1;
    $scope.setTabIndex = function (newTab) {
        $scope.tab1 = newTab;
        $scope.getalldata();
    };
    $scope.isSetIndex = function (tabNum) {
        return $scope.tab1 === tabNum;
    };

    $scope.setTabIndex1 = function (newTab) {
        $scope.tab1 = newTab;
        $scope.getalldataIndexApp();
    };
    $scope.isSetIndex1 = function (tabNum) {
        return $scope.tab1 === tabNum;
    };
// #endregion



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


   //#region ----- GRN-With-Req-PO-----ALL Print Buton For Approval  -------


    $scope.onClickReportAHRDownloadWord = function (args) {
        //debugger;
        var gridObj = $("#GriddataMasterAHR1").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandAHRWord = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportAHRDownloadWord
        }
    }];




  



    $scope.onClickReportPostedDownloadWord3 = function (args) {
        //debugger;
        var gridObj = $("#GriddataMasterAHR3").data("ejGrid");
        //getting corresponding record 
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandWordPosted = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportPostedDownloadWord3
        }
    }];

     //#endregion ----- GRN-With-Req-PO-----PrintButon






    //#region ----- GRN-With-Req-PO----- Index GridData  -------


    $scope.GRNWithReqPOCheckStatus = "ForChecked";
    $scope.GriddataMaster = [];
    $scope.getalldataMaster = function () {
        if ($scope.GRNWithReqPOCheckStatus === "ForChecked") {
            $scope.GRNWithReqPOCheckStatus = "ForChecked";
        }
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListForGRNSaveData?GRNWithReqPOCheckStatus=' + $scope.GRNWithReqPOCheckStatus,
        }).then(function successCallback(response) {
              // url: $scope.getListUrl1,
            $scope.GriddataMaster = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.getalldataMaster();



    $scope.GriddataMaster2 = [];
    $scope.getalldataMaster2 = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/GoodsReceiveNote/GetListForGrnByPoReq?GRNWithReqPOApprovedStatus=' + $scope.GRNWithReqPOApprovedStatus,
           // url: $scope.getListUrl2,
        }).then(function successCallback(response) {
            $scope.GriddataMaster2 = response.data;
           
        });
    };
   // $scope.getalldataMaster2();


    //#endregion ----- GRN-With-Req-PO----- Index GridData  -------

    //#region ----- GRN-With-Req-PO----- All Tab -------

  

    $scope.GRN = "";
    //$scope.tab = 1;
    $scope.tabGL = 1;
    //debugger;
    $scope.GRNWithReqPOCheckStatus = "ForChecked";
    $scope.setTabGRNList = function (newTab) {
        $scope.tabGL = newTab;
        $scope.GRNWithReqPOCheckStatus = "ForChecked";
       // $scope.GRN = 0;
        $scope.getalldataMaster();
    };
    $scope.isSetGRNList = function (tabNum) {
        return $scope.tabGL === tabNum;
        //$scope.GRN = 1;

    };




    $scope.setTabCheckedHoldReject = function (newTab) {
        $scope.tabGL = newTab;
        $scope.GRNWithReqPOCheckStatus = "CheckedHoldReject";
        $scope.getalldataMaster();
      
    };
    $scope.isSetCheckedHoldReject = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 2;

    };



    $scope.setTabNotApprovedChecked = function (newTab) {
        $scope.tabGL = newTab;
        $scope.GRNWithReqPOCheckStatus = "Checked";
        $scope.getalldataMaster();
     
    };
    $scope.isSetNotApprovedChecked = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 3;

    };



    $scope.GRNWithReqPOApprovedStatus = "ApprovedHoldReject";
    $scope.setTabApprovedHoldReject = function (newTab) {

        $scope.tabGL = newTab;
        $scope.GRNWithReqPOApprovedStatus = "ApprovedHoldReject";
        $scope.getalldataMaster2();
    };
    $scope.isSetApprovedHoldReject = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 4;
    };


    $scope.setTabApprovedNotPosted = function (newTab) {
        $scope.tabGL = newTab;
        $scope.GRNWithReqPOApprovedStatus = "Approved";
        $scope.getalldataMaster2();
    };
    $scope.isSetApprovedNotPosted = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 5;
    };



    $scope.setTabPosted = function (newTab) {
        $scope.tabGL = newTab;
        $scope.GRNWithReqPOApprovedStatus = "Posted";
        $scope.getalldataMaster2();
    };
    $scope.isSetPosted = function (tabNum) {
        return $scope.tabGL === tabNum;
        $scope.GRN = 6;
    };

    //#endregion ----- GRN-With-Req-PO----- All Tab -------


  //#region ----Inventory Receive GRN Print Option ------

   
    $scope.onClickReportANPDownloadWord1 = function (args) {
        //debugger;
        var gridObj = $("#GriddataMaster1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.commandANPWord1 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord1
        }
    }];
    

    $scope.onClickReportANPDownloadWord2 = function (args) {
        //debugger;
        var gridObj = $("#GriddataMasterHR").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord2 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord2
        }
    }];



    $scope.onClickReportANPDownloadWord3 = function (args) {
        //debugger;
        var gridObj = $("#GriddataMasterAC").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord3 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord3
        }
    }];




    $scope.onClickReportANPDownloadWord4 = function (args) {
        //debugger;
        var gridObj = $("#GriddataMasterAHR4").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };
    $scope.commandANPWord4 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord4
        }
    }];
   

    $scope.onClickReportANPDownloadWord5 = function (args) {
        //debugger;
        var gridObj = $("#GriddataMasterANP5").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord5 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord5
        }
    }];



    $scope.onClickReportANPDownloadWord6 = function (args) {
        //debugger;
        var gridObj = $("#GriddataMasterANP6").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

    };

    $scope.commandANPWord6 = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",


            click: $scope.onClickReportANPDownloadWord6
        }
    }];


    //#endregion ---Inventory Receive GRN Print Option---


}