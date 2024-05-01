"use strict";
InvoiceStatusController.$inject = ["cboService", "commonMessage", '$window', "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService", "bankService"];
function InvoiceStatusController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $http, $filter, $controller, accountService, bankService) {
    $rootScope.title = "Invoice Status";
    $scope.Action = "Save";
    $scope.invoiceList = [];
    $scope.postedSalesList = [];
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


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
            for (var i = 0; i < $scope.MasterOrderSalesPostedList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.MasterOrderSalesPostedList[i].InvoiceStatus))
                    $scope.MasterOrderSalesPostedList[i].InvoiceStatus = 'Active';
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    // $scope.getMasterOrderSalesPosted();

    //$scope.model = { Id: null, SalesId: null, PostCode: null, ShippingDate: null, ShippingBill: null, RodTepAmount: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    //$scope.modelNew = Object.assign({}, $scope.model);

    $scope.SalesId = null;
    $scope.ShowAdditionalPopup = function (obj) {
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.salesVM = Object.assign({}, obj.data);
        $scope.ModelInvoiceStatus = Object.assign({}, obj.data);
        if (baseService.isUndefinedOrNull($scope.ModelInvoiceStatus.InvoiceStatus)) {
            $scope.ModelInvoiceStatus.InvoiceStatus = 'Active';
        }
        $scope.SalesAdditionalInfoDataList = [];
        $scope.SalesId = obj.data.Id;
        $scope.ModelNew.SalesId = obj.data.Id;
        $scope.ModelNew.Amount = obj.data.Amount;
        $scope.GetSalesAdditionalInfoData();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.EditData = function (data) {
        $scope.modelNew = Object.assign({}, data);
    }

    $scope.ClosePopUp = function () {
        $scope.model = {
            Id: null,
            SalesId: null,
            PostCode: null,
            ShippingDate: null,
            ShippingBill: null, 
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
             UpdatedFromIP: null
        }
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
            ShowResult(e, "failure");
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
                    $scope.getPostSalesData();
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

    //#region PostInvoice
    $scope.ModelList = [];
    $scope.path = 'Commercial/PostSalesInvoice/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.PostSalesInvoicedeleteUrl = 'Commercial/PostSalesInvoice/delete/';
    $scope.Action = 'Save';
    // $scope.partyType = "Vendor";

    $scope.salesVM = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        EntityId: null,
        ItemDescription: null,
        PartyName: null,
        CurrencyId: null,
        PartyType: "Customer",
        InvoiceDate: $filter("dateFiltering")(Date.now()),
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: $filter("dateFiltering")(Date.now()),
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        Amount: 0,
        BankAmount: 0,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        PaymentTermId: null,
        Narration: null,
        CompanyCurrencyRate: 1,
        InvoicingPartyPlantId: null,
        DeliveryPartyPlantId: null,
        InvoicingByAddress: null,
        DeliveryByAddress: null,
        InvoicingState: null,
        InvoicingGSTIN: null,
        DeliveryState: null,
        DeliveryGSTIN: null,
        BLNumber: null,
        LCNumber: null,
        ComercialInvoiceNo: null,
        EXPFromNo: null,
        SourceType: 'MasterOrderSales',
        ContractId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes',
        BooksCurrencyTransactionAmount: null,
        BooksCurrencyTaxAmount: null,
        BooksCurrencyBaseRate: null,
        IsPark: 1,
        IsAdditionalInfoApplicable: true,
        IsIncentiveApplicable: false,
        InvoiceStatus:'Active'
    };

    $scope.getPostSalesData = function () {
        
        $http.get("Commercial/PostSalesInvoice/GetListBySalesId?SalesId=" + $scope.SalesId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ModelNew = Object.assign({}, response.data[0]);
                        $scope.ModelInvoiceStatus.InvoiceStatus = response.data[0].InvoiceStatus;
                        if (baseService.isUndefinedOrNull($scope.ModelInvoiceStatus.InvoiceStatus)) {
                            $scope.ModelInvoiceStatus.InvoiceStatus = 'Active';
                        }
                    }
                    $scope.ModelNew.SalesId = $scope.salesVM.Id;
                    $scope.ModelNew.InvoiceDate = $scope.salesVM.InvoiceDate;
                    $scope.ModelNew.InvoiceNo = $scope.salesVM.InvoiceNo;
                    $scope.ModelNew.ContractNo = $scope.salesVM.ContractNo;
                    $scope.ModelNew.PartyName = $scope.salesVM.PartyName;
                    $scope.ModelNew.Amount = $scope.salesVM.Amount;
                    $scope.getPartyPlant();
                    if (baseService.arrayLength($scope.bankMasterList) > 0 && !baseService.isUndefinedOrNull($scope.salesVM.BenificiaryBankId)) {
                        for (var i = 0; i < $scope.bankMasterList.length; i++) {
                            if ($scope.bankMasterList[i].Id === $scope.salesVM.BenificiaryBankId) {
                                $scope.ModelNew.BankMasterId = $scope.bankMasterList[i].Id;
                            }
                        }
                    }

                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }


    $scope.portList = [];
    cboService.getPortByPlantCbo(function (result) {
        $scope.portList = result;
    });


    $scope.deliveryPortList = [];
    cboService.getPortCbo(function (result) {
        $scope.deliveryPortList = result;
    });

    $scope.bankMasterList = [];
    bankService.getBankMasterCboListByPlant(function (result) {
        $scope.bankMasterList = result;

    });

    $scope.shipmentModeList = [];
    $scope.getShipmode = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/shipmode/GetCbo/'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.shipmentModeList = response.data;
            }
        });
    };
    $scope.getShipmode();

    $scope.dischargePortList = [];
    $scope.GetPortOfDischargeByDstination = function () {
        $http({
            method: 'GET',
            url: 'Commercial/PostSalesInvoice/GetPortByDestinationCbo?destinationId=' + $scope.ModelNew.FinalDestinationId
        }).then(function successCallback(response) {
            $scope.dischargePortList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.dischargePortList = response.data;
            }
        });
    };


    $scope.ModelTemp = {
        Id: null,
        SalesId: null,
        InvoiceDate: null,
        BankMasterId: null,
        ShipmentModeId: null,
        PortOfLoadingId: null,
        ExpFormNo: null,
        ExpDate: null,
        CargoNetWt: null,
        CargoGrossWt: null,
        Dimension: null,
        ExFactoryDocRef: null,
        ExFactoryDate: null,
        TransportAgentId: null,
        TransportDocRefNo: null,
        TransportDocDate: null,
        TransportVehicleNo: null,
        TransportDriverName: null,
        TransportDriverNo: null,
        PreCarriageBy: null,
        PlaceOfReceiptByPreCarriage: null,
        PreCarriageDocRef: null,
        PreCarriageDocDate: null,
        CNFAgentId: null,
        CNFContainerNo: null,
        CNFVesselTrackingNo: null,
        CNFVesselName: null,
        CNFVesselSalesDetails: null,
        CNFBLAWB: null,
        CNFBLAWBDate: null,
        ETA: null,
        FinalDestinationId: null,
        PortOfDischargeId: null,
        PortOfDelivaryId: null,
        BankDocRef: null,
        NegotiatingDate: null,

        ExportRefNo: null,
        VendorSelection: null,
        DocumentReceiveDate: null,
        AWBB2B: null,
        ActualPaymentReceived: null,
        ShippingBillNo: null,
        PortCode: null,
        DocumentSubmissionDate: null,
        DocAcceptanceDate: null,
        FinalShipmentStatus: null,
        ShippingBillDate: null,
        ShipmentDate: null,
        NegotiationType: null,
        PaymentReceivedDate: null,
        Remark: null,

        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.destinationList = [];
    $scope.getDestination = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/destination/GetCbo'
        }).then(function successCallback(response) {
            $scope.destinationList = response.data;
        });
    };
    $scope.getDestination();

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "" + fieldname + " is required.";
            }
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("Invoice No", $scope.ModelNew.InvoiceNo);
            CheckField("Customer", $scope.salesVM.PartyName);
            CheckField("Bank", $scope.ModelNew.BankMasterId);
            CheckField("ExFactory Date", $scope.ModelNew.ExFactoryDate);
            CheckField("Shipment Mode", $scope.ModelNew.ShipmentModeId);
            CheckField("Port of Loading", $scope.ModelNew.PortOfLoadingId);
            CheckField("Final Destination", $scope.ModelNew.FinalDestinationId);
            CheckField("Port Of Discharge", $scope.ModelNew.PortOfDischargeId);
            CheckField("Port Of Delivery", $scope.ModelNew.PortOfDelivaryId);
            CheckField("Transport Agent", $scope.ModelNew.TransportAgentId);
            CheckField("Transport Doc Ref No.", $scope.ModelNew.TransportDocRefNo);
            CheckField("Pre-CarriageBy", $scope.ModelNew.PreCarriageBy);
            CheckField("Place Of Receipt", $scope.ModelNew.PlaceOfReceiptByPreCarriage);
            CheckField("Pre-Carriage Doc Ref No.", $scope.ModelNew.PreCarriageDocRef);
            CheckField("Pre-Carriage DocDate", $scope.ModelNew.PreCarriageDocDate);
            CheckField("CNF Agent", $scope.ModelNew.CNFAgentId);
            CheckField("Container No", $scope.ModelNew.CNFContainerNo);
            CheckField("Vessel Tracking No", $scope.ModelNew.CNFVesselTrackingNo);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };

    $scope.closeInvoicingPartyPopUp = function () {
        //$scope.salesMaterialList
        if ($scope.selectedMasterOrderItemList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.salesVM.ChangeInvoicingStateId)) {
                if ($scope.salesVM.PlantStateId == $scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.InvoicingStateId == $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.salesVM.PlantStateId != $scope.salesVM.InvoicingStateId && $scope.salesVM.PlantStateId != $scope.salesVM.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    //ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
                    ShowResult('First delete material.', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
            if (flag === 'billTo') {
                $scope.salesVM.InvoicingState = state;
                $scope.salesVM.ChangeInvoicingStateId = stateId;
                $scope.salesVM.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.salesVM.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.salesVM.DeliveryState = state;
                $scope.salesVM.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.salesVM.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.salesVM.InvoicingState = null;
                $scope.salesVM.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.salesVM.DeliveryState = null;
                $scope.salesVM.DeliveryGSTIN = null;
                return $scope.salesVM.DeliveryByAddress = null;
            }
        }
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.salesVM.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = item.Value;
                    $scope.salesVM.InvoicingByAddress = item.Address1;
                    $scope.salesVM.DeliveryByAddress = item.Address1;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = item.StateName;
                    $scope.salesVM.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }
    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.salesVM.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = deliveryplant;
                    $scope.salesVM.InvoicingByAddress = invoAddress;
                    $scope.salesVM.DeliveryByAddress = deliAddress;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = deliState;
                    $scope.salesVM.DeliveryGSTIN = deliGSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });

        });
    }

    $scope.getPartyPlant = function () {
        $scope.getCboPartyPlantList($scope.salesVM.PartyId, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = item.Value;
                    $scope.salesVM.InvoicingByAddress = item.Address1;
                    $scope.salesVM.DeliveryByAddress = item.Address1;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = item.StateName;
                    $scope.salesVM.DeliveryGSTIN = item.GSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });
        });
    }

    $scope.SavePostSales = function () {
        try {
            $scope.ModelNew.SalesId = $scope.salesVM.Id;
            if (baseService.isUndefinedOrNull($scope.ModelNew.SalesId)) {
                throw "Select Invoice No.";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.ExpDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.ExpDate)) {
                    throw "Expected Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.ExpDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.ExFactoryDate)) {
                    throw "ExFactory Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.CNFBLAWBDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.CNFBLAWBDate)) {
                    throw "BL Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.NegotiatingDate)) {
                if (new Date($scope.ModelNew.CNFBLAWBDate) < new Date($scope.ModelNew.NegotiatingDate)) {
                    throw "Bank Doc Date should greater than BL Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.ETA)) {
                if (new Date($scope.ModelNew.CNFBLAWBDate) < new Date($scope.ModelNew.ETA)) {
                    throw "ETA Date should greater than BL Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.TransportDocDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.TransportDocDate)) {
                    throw "Transport Doc Date should greater than Invoice Date";
                }
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.PreCarriageDocDate)) {
                if (new Date($scope.ModelNew.InvoiceDate) < new Date($scope.ModelNew.PreCarriageDocDate)) {
                    throw "Pre-Carriage Doc Date should greater than Invoice Date";
                }
            }
            //ValidationMaster();
            
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'entity': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.Id;
                        // $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.DeletePostSales = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.PostSalesInvoicedeleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelList = [];
                    ClearPostSalesFields();
                    //$scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearPostSales = function () {
        ClearPostSalesFields();
        return true;
    };

    function ClearPostSalesFields() {
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.showVendorPopUp = function (flg) {
        $scope.flag = flg;
        $scope.GetVendorPopUpData();
        angular.element(document.querySelector('#vendorPopUp')).modal('show');
    };

    $scope.GetVendorPopUpData = function () {
        if ($scope.flag === 'Transport' || $scope.flag === 'CNF' || $scope.flag === 'Forwarder') {
            $scope.partyType = 'Vendor';
        }

        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
    };

    $scope.closevendorPopUpNew = function () {
        angular.element(document.querySelector('#vendorPopUp')).modal('hide');
        $scope.hidePartyPopUp();
        //$scope.partyType = "Customer";
    }

    $scope.SetVendorData = function (obj) {
        if ($scope.flag === 'CNF') {
            var party = obj.data;
            $scope.ModelNew.CNFAgentId = party.Id;
            $scope.ModelNew.CNFAgentCode = party.Code;
            $scope.ModelNew.CNFAgentName = party.UserName;
        }
        else if ($scope.flag === 'Transport') {
            var party = obj.data;
            $scope.ModelNew.TransportAgentId = party.Id;
            $scope.ModelNew.TransportAgentCode = party.Code;
            $scope.ModelNew.TransportAgentName = party.UserName;
        } else {
            var party = obj.data;
            $scope.ModelNew.TransporterCHAForwarderId = party.Id;
            $scope.ModelNew.TransporterCHAForwarder = party.UserName;
        }
        $scope.searchByParty = "UserName"; $scope.searchParty = "";
        angular.element(document.querySelector('#vendorPopUp')).modal('hide');
    }

    $scope.closePartyPopUp = function (x) {

        var party = x.data;
        $scope.salesVM.PartyName = party.UserName;
        $scope.salesVM.PartyId = party.Id;
        $scope.salesVM.PaymentTermId = party.PaymentTermId;
        $scope.salesVM.CurrencyId = party.CurrencyId;
        $scope.GetCurrencyExchangeRateList();
        $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
        $scope.partyPlantList = [];
        $scope.getCboPartyPlantList(party.Id, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.salesVM.InvoicingPartyPlantId = item.Value;
                    $scope.salesVM.DeliveryPartyPlantId = item.Value;
                    $scope.salesVM.InvoicingByAddress = item.Address1;
                    $scope.salesVM.DeliveryByAddress = item.Address1;
                    $scope.salesVM.InvoicingState = item.StateName;
                    $scope.salesVM.InvoicingGSTIN = item.GSTIN;
                    $scope.salesVM.DeliveryState = item.StateName;
                    $scope.salesVM.DeliveryGSTIN = item.GSTIN;
                    $scope.salesVM.InvoicingStateId = item.StateId;
                }
            });
        });
        $scope.partyType = "Customer";
        $scope.flag = null;
        $scope.hidePartyPopUp();
    };


    //#endregion PostInvoice
    $scope.ModelInvoiceTemp = {
        Id: null,
        InvoiceNo: null,
        InvoiceDate: null,
        Amount: null,
        InvoiceStatus: 'Active'
    };
    $scope.ModelInvoiceStatus = Object.assign({}, $scope.ModelTemp);

    console.log($scope.ModelInvoiceStatus);
    $scope.InvoiceStatusList = [
        { Value: 'Active', Text:'Active'},
        { Value: 'Closed', Text:'Closed'},
        { Value: 'Pending', Text:'Pending'}
    ]
    
  

    $scope.saveInvoiceStatusUrl = $scope.path + 'CreateInvoiceStatus';
    $scope.SaveInvoiceStatus = function (data) {
        try {  
            $http({
                method: 'POST',
                url: $scope.saveInvoiceStatusUrl,
                data: { 'data': data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success'); 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            } 
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
 
}