'use strict';
contractController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function contractController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "Contract";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/contract/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveMasterLCUrl = $scope.path + 'CreateMasterLC';
    $scope.updateContractUrl = $scope.path + 'UpdateContract';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.model = {
        Id: null,
        CompanyId: null,
        MasterOrderId: null,
        ContractNo: null,
        CustomerId: null,
        Descriotion: null,
        IsLC: false,
        CustomerName: null,
        Currency: null,
        TotalQty: 0,
        SOQty: 0,
        Amount: 0,
        UDNo: null,
        FileNo: null,
        IsPrint: false,
        IsMarketingCommisssionApplicable: false,
        MarketingCommisssionId: null,
        IsBusinessDevelopmentChargesApplicable: false,
        BusinessDevelopmentCharge: 'Percentage',
        MarketingCommisssionCharge: 'Percentage',
        BusinessDevelopmentChargeValue: null,
        MarketingCommisssionChargeValue: null,
        InvoicingPartyPlantId: null,
        DeliveryPartyPlantId: null,
        InvoicingByAddress: null,
        DeliveryByAddress: null,
        Remarks: null,
        PlantId: $window.plantId,
        BankId: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.lcMaster = {
        Id: null,
        ContractId: null,
        BenificiaryBankId: null,
        OpeningBankId: null,
        OpeningDescription: null,
        LeinBankId: null,
        LeinDescription: null,
        LCRef: null,
        LCDate: null,
        ExpiryDate: null,
        Amount: null,
        Type: null,
        Tenure: 0,
        FinalDestinationId: null,
        PortOfLandingId: null,
        CurrencyId: null,
        IsClose: null,
        BankId: null
    };
    $scope.lcMasterNew = Object.assign({}, $scope.lcMaster);

    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Commercial/contract/GetCompanyPartyDataListNew?partyType=' + $scope.partyType + '&CompanyId=' + $window.companyId + '&PlantId=' + $window.plantId;

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('show');
    };

    $scope.SetCustomerData = function (obj) {
        var party = obj.data;
        $scope.modelNew.CustomerName = party.UserName;
        $scope.modelNew.CustomerId = party.Id;

        getPartyPlantList();

        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
        $scope.partyType = "Customer";
    }

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.modelNew.CustomerId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.IsDefault) {
                    $scope.modelNew.InvoicingPartyPlantId = item.Value;
                    $scope.modelNew.DeliveryPartyPlantId = item.Value;
                    $scope.modelNew.InvoicingByAddress = item.Address1;
                    $scope.modelNew.DeliveryByAddress = item.Address1;
                    $scope.modelNew.InvoicingState = item.StateName;
                    $scope.modelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.modelNew.DeliveryState = item.StateName;
                    $scope.modelNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }

    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.partyType = "Customer";
        $scope.searchParty = '';
    }

    $scope.bankList = [];
    bankService.GetNegotiatingBankMasterCboListByPlant(function (result) {
        $scope.bankList = result;

    });

    $scope.bankMasterList = [];
    bankService.getBankMasterCboListByPlant(function (result) {
        $scope.bankMasterList = result;
    });

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.lcMasterNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });

    $scope.portList = [];
    cboService.getPortCbo(function (result) {
        $scope.portList = result;
    });

    $scope.destinationList = [];
    $scope.getDestination = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/destination/GetCbo/'
        }).then(function successCallback(response) {
            $scope.destinationList = response.data;
        });
    };
    $scope.getDestination();

    $scope.SalesOrderList = [];
    $scope.GetSalesOrderList = function () {
        $scope.SalesOrderList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetSalesOrderList?customerId=" + $scope.modelNew.CustomerId
        }).then(function (response) {
            $scope.SalesOrderList = response.data;
            angular.element(document.querySelector("#salesOrderPopUp")).modal("show");
        });
    };

    // #region checkbox all SO

    $scope.refreshTemplateSO = function (args) {
        $("#soheadchk").ejCheckBox({ "change": CheckBoxSelectAllSO });
    };

    function CheckBoxSelectAllSO(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSO").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SalesOrderList.length; i++) {
                $scope.SalesOrderList[i].Flags = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flags = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSO").data("ejGrid");
        gridObj.refreshContent();
    };

    function checkSamePaymentTerm(list, PaymentTermId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PaymentTermId !== PaymentTermId) {
                return false;
            }
        }
        return true;
    }

    $scope.CloseSOPopUp = function () {
        try {
            for (var i = 0; i < $scope.SalesOrderList.length; i++) {
                if ($scope.SalesOrderList[i].Flags == true) {
                    if (checkSamePaymentTerm($scope.SelectedSalesOrderList, $scope.SalesOrderList[i].PaymentTermId)) {
                        $scope.SelectedSalesOrderList.push($scope.SalesOrderList[i]);
                    }
                    else {
                        for (var j = 0; j < $scope.SelectedSalesOrderList.length; j++) {
                            if (baseService.isUndefinedOrNull($scope.SelectedSalesOrderList[j].ContractId)) {
                                $scope.SelectedSalesOrderList.splice(j, 1);
                            }
                        }

                        throw "Select same Payment Term.";
                    }

                }
            }

            for (var m = 0; m < $scope.SelectedSalesOrderList.length; m++) {

                tq += $scope.SelectedSalesOrderList[m].TotalQty;
                amt += $scope.SelectedSalesOrderList[m].Amount;
                qt += $scope.SelectedSalesOrderList[m].Qty;

            }

            $scope.modelNew.TotalQty = tq;
            $scope.modelNew.Amount = amt;
            $scope.modelNew.SOQty = qt;
            angular.element(document.querySelector('#salesOrderPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'salesOrderPopUp');
        }
    };


    // #endregion checkbox all



    $scope.partyId = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {
        if ($scope.partyId != e.data.PartyId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.partyId = e.data.PartyId;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#c8c8c8');
        else
            e.row.css("background-color", '#fff6b7');

    }

    $scope.GetshipmentMode = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/shipmode/GetCbo/'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.shipmentModeList = response.data;
            }
        });
    }
    $scope.GetshipmentMode();

    $scope.summaryRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N2}" }
        ]
        , showCaptionSummary: true

    }];

    $scope.message_detailconfirm = null;
    $scope.removeSO = function (obj) {

        $scope.New = obj.data;
        if (!baseService.isUndefinedOrNull($scope.New.SalesOrderId))
            $scope.message_detailconfirm = 'Are you sure want to remove permanently [ ' + $scope.New.SalesOrderId + ' ]';
        angular.element(document.querySelector('#confirmSOPopUp')).modal('show');
    }

    $scope.DeleteSO = function () {
        $http({
            method: 'POST',
            url: 'Commercial/Contract/DeleteSO?id=' + $scope.New.SalesOrderId
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.GetEditSalesOrderList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.searchBy = "ContractNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'ContractNo', name: "ContractNo" }, { value: 'CustomerName', name: "Customer" }];

    $scope.contractList = [];
    $scope.getSavedData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.contractList = response.data;
        });
        if (!baseService.isUndefinedOrNull($scope.modelNew.CustomerId) && !baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $scope.GetEditSalesOrderList();
        }

    }
    $scope.getSavedData();

    $scope.Commission = 0;
    $scope.GetCommission = function (x) {
        var com = (($scope.modelNew.Amount * x.Percentage) / 100).toFixed(2);
        x.Commission = parseFloat(com);
        $scope.Commission = x.Commission;

        for (var i = 0; i < $scope.fundUtilizationList.length; i++) {
            if ($scope.fundUtilizationList[i].FundUtilization === 'Purchase') {
                var pm = ((($scope.modelNew.Amount - $scope.Commission) * $scope.fundUtilizationList[i].Percentage) / 100).toFixed(2);
                $scope.fundUtilizationList[i].PurchaseMargin = parseFloat(pm);
            }
        }

    };

    $scope.GetPurchaseMargin = function (x) {
        if (x.FundUtilization === 'Purchase') {
            var pm = ((($scope.modelNew.Amount - $scope.Commission) * x.Percentage) / 100).toFixed(2);
            x.PurchaseMargin = parseFloat(pm);
        }
    };

    var tq = 0;
    var amt = 0;
    var qt = 0;

    $scope.selectedMasterOrderList = [];
    $scope.MakeData = function () {
        $scope.selectedMasterOrderList = [];
        var i = $scope.masterOrderList.length;
        while (i--) {
            if ($scope.masterOrderList[i].Active == true) {
                var ob = {};
                ob.Id = $scope.masterOrderList[i].MasterOrderId;
                ob.MasterOrderId = $scope.masterOrderList[i].MasterOrderId;
                ob.MasterOrderItemId = $scope.masterOrderList[i].MasterOrderItemId;
                ob.PartyId = $scope.masterOrderList[i].PartyId;
                ob.CustomerId = $scope.masterOrderList[i].PartyId;
                ob.CustomerName = $scope.masterOrderList[i].CustomerName;
                ob.MaterialMaster = $scope.masterOrderList[i].MaterialMaster;
                ob.Article = $scope.masterOrderList[i].Article;
                ob.Currency = $scope.masterOrderList[i].Currency;
                ob.CurrencyId = $scope.masterOrderList[i].CurrencyId;

                if (checkSameCustomer($scope.masterOrderCustomerList, ob.PartyId, ob.CurrencyId)) {
                    if (checkExistList($scope.masterOrderCustomerList, ob.MasterOrderId, ob.MasterOrderItemId) === false) {

                        $scope.modelNew.CustomerId = $scope.masterOrderList[i].PartyId;
                        $scope.modelNew.CustomerName = $scope.masterOrderList[i].CustomerName;
                        $scope.modelNew.Currency = $scope.masterOrderList[i].Currency;

                        $scope.masterOrderList[i].Active = false;

                        $scope.masterOrderCustomerList.push($scope.masterOrderList[i]);
                        $scope.masterOrderList.splice(i, 1);
                        $scope.getPartyPlant();
                    }
                } else {
                    ShowResult("Please select same Customer and Currency.", 'failure');
                }

            }
        }

        for (var i = 0; i < $scope.masterOrderCustomerList.length; i++) {
            tq += $scope.masterOrderCustomerList[i].TotalQty;
            amt += $scope.masterOrderCustomerList[i].Amount;
            qt += $scope.masterOrderCustomerList[i].Qty;
        }
        $scope.modelNew.TotalQty = tq;
        $scope.modelNew.Amount = amt;
        $scope.modelNew.SOQty = qt;

        for (var i = 0; i < $scope.buyerDeductionList.length; i++) {
            var com = (($scope.modelNew.Amount * $scope.buyerDeductionList[i].Percentage) / 100).toFixed(2);
            $scope.buyerDeductionList[i].Commission = parseFloat(com);
            $scope.Commission = $scope.buyerDeductionList[i].Commission;

            for (var i = 0; i < $scope.fundUtilizationList.length; i++) {
                if ($scope.fundUtilizationList[i].FundUtilization === 'Purchase') {
                    var pm = ((($scope.modelNew.Amount - $scope.Commission) * $scope.fundUtilizationList[i].Percentage) / 100).toFixed(2);
                    $scope.fundUtilizationList[i].PurchaseMargin = parseFloat(pm);
                }
            }
        }
    }

    $scope.RemovePO = function () {
        if (baseService.arrayLength($scope.masterOrderCustomerList) > 0) {
            var i = $scope.masterOrderCustomerList.length;
            while (i--) {
                if ($scope.masterOrderCustomerList[i].Active === true) {
                    $scope.masterOrderCustomerList[i].Active = false;
                    $scope.masterOrderList.push($scope.masterOrderCustomerList[i]);
                    $scope.masterOrderCustomerList.splice(i, 1);
                }
            }

            var tq = 0;
            var amt = 0;
            var qt = 0;
            for (var i = 0; i < $scope.masterOrderCustomerList.length; i++) {
                tq += $scope.masterOrderCustomerList[i].TotalQty;
                amt += $scope.masterOrderCustomerList[i].Amount;
                qt += $scope.masterOrderCustomerList[i].Qty;
            }
            $scope.modelNew.TotalQty = tq;
            $scope.modelNew.Amount = amt;
            $scope.modelNew.SOQty = qt;

        }

    }

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
        $scope.actionCompleteUnassign();
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid3").ejGrid("instance");
                var scrollerwidth = $("#Assigned").width();//Obtain the width of the container

                $("#Grid3").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 290 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.actionCompleteUnassign = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid").ejGrid("instance");
                var scrollerwidth = $("#Unassign").width();//Obtain the width of the container

                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 290 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    //$scope.summaryRows = [{
    //    title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N0}" }
    //        , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amt", dataMember: "Amt", format: "{0:N0}" }],
    //    showCaptionSummary: true

    //}];

    function checkSameCustomer(list, customerId, currencyId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== customerId || list[i].CurrencyId !== currencyId) {
                return false;
            }
        }
        return true;
    }

    function checkExistList(list, MasterOrderId, MasterOrderItemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MasterOrderId == MasterOrderId && list[i].MasterOrderItemId == MasterOrderItemId) {
                return true;
            }
        }
        return false;
    }

    $scope.masterOrderList = [];
    $scope.GetMasterOrderByCustomer = function () {
        $scope.masterOrderList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyCustomer?customerId=" + $scope.modelNew.CustomerId
        }).then(function (response) {
            $scope.masterOrderList = response.data;
            //$("#masterorderPoUp").ejDialog("setTitle", "Master Order");
            //var eDialog = $("#masterorderPoUp").data("ejDialog");
            //eDialog.open();
        });
    }

    $scope.SelectedSalesOrderList = [];
    $scope.GetEditSalesOrderList = function () {
        $scope.SelectedSalesOrderList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetEditSalesOrderList?customerId=" + $scope.modelNew.CustomerId + '&contractId=' + $scope.modelNew.Id
        }).then(function (response) {
            $scope.SelectedSalesOrderList = response.data;
            $scope.GetMasterLCData($scope.modelNew.Id);
        });
    }

    $scope.msg = null;

    $scope.selectContract = function (obj) {
        $scope.modelNew = obj.data;
        $scope.modelNew.ContractDate = $filter('dateFiltering')(obj.data.ContractDate, 'dd-M-yyyy');
        $scope.modelNew.Currency = null;
        $scope.GetEditSalesOrderList();


        if (!baseService.isUndefinedOrNull($scope.modelNew.MasterOrderId)) {
            $scope.msg = "As this contract saved from Master Order, so no change is possible from here.";
        } else {
            $scope.msg = null;
            $scope.modelNew.MasterOrderId = null;
        }
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = 'Update';
    };

    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function (contractId) {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + contractId
        }).then(function (response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.masterOrderCustomerList = response.data;
                $scope.modelNew.Currency = response.data[0].Currency;
                $scope.selectedMasterOrderList = $scope.masterOrderCustomerList;

                //for (var i = 0; i < $scope.masterOrderCustomerList.length; i++) {
                //    $scope.modelNew.TotalQty += $scope.masterOrderCustomerList[i].TotalQty;
                //    $scope.modelNew.Amount += $scope.masterOrderCustomerList[i].Amount;
                //    $scope.modelNew.SOQty += $scope.masterOrderCustomerList[i].Qty;
                //}

                var tq = 0;
                var amt = 0;
                var qt = 0;
                for (var i = 0; i < $scope.masterOrderCustomerList.length; i++) {
                    tq += $scope.masterOrderCustomerList[i].TotalQty;
                    amt += $scope.masterOrderCustomerList[i].Amount;
                    qt += $scope.masterOrderCustomerList[i].Qty;
                }
                $scope.modelNew.TotalQty = tq;
                $scope.modelNew.Amount = amt;
                $scope.modelNew.SOQty = qt;
            }
        });
    };

    $scope.getPercentageValue = function () {
        var negotiable = 0;
        for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
            if (!baseService.isUndefinedOrNull($scope.fundUtilizationList[j].Percentage) && $scope.fundUtilizationList[j].Text !== 'Negotiable') {
                negotiable += $scope.fundUtilizationList[j].Percentage;
            }
        }
        for (var j = 0; j < $scope.fundUtilizationList.length; j++) {
            if ($scope.fundUtilizationList[j].Text === 'Negotiable') {
                $scope.fundUtilizationList[j].Percentage = 100 - negotiable;
            }
        }
    }

    $scope.fundUtilizationList = [];
    $scope.GetContractFundData = function (contractId) {
        $scope.fundUtilizationList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetContractFundData?contractId=" + contractId
        }).then(function (response) {
            $scope.fundUtilizationList = response.data;
            getPartyPlantEditList($scope.modelNew.InvoicingPartyPlantId, $scope.modelNew.InvoicingByAddress, $scope.modelNew.DeliveryPartyPlantId, $scope.modelNew.DeliveryByAddress, $scope.modelNew.DeliveryState, $scope.modelNew.DeliveryGSTIN);
        });
    };


    // #region checkbox all for TermsAndConditions

    $scope.TermsAndConditionsList = [];

    $scope.GetContractTermsAndConditionsList = function () {

        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetContractTermsAndConditionsList?ContractId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.TermsAndConditionsList = response.data;
            $scope.GetContractFundData($scope.modelNew.Id);
        });
    }

    $scope.searchdata = [];
    $scope.GetTermsAndConditionsList = function () {
        $scope.searchdata = [];
        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetTermsAndConditionsList'
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
            for (var i = 0; i < $scope.TermsAndConditionsList.length; i++) {
                for (var j = 0; j < $scope.searchdata.length; j++) {
                    if ($scope.TermsAndConditionsList[i].TermsAndConditionsId == $scope.searchdata[j].TermsAndConditionsId) {
                        $scope.searchdata.splice(j, 1);
                    }
                }
            }
        });
    }

    $scope.AddTermsAndConditions = function () {

        $scope.GetTermsAndConditionsList();
        $scope.ShowResultCustom();
    }

    $scope.message_detailconfirmation = null;
    $scope.removeTNC = function (obj) {

        $scope.TNC = obj.data;
        if (!baseService.isUndefinedOrNull($scope.TNC.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.TNC.UserName + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    

    $scope.ShowResultCustom = function (message, type) {
        $("#TermsAndConditionsPoUp").ejDialog("setTitle", "Terms And Conditions");
        var eDialog = $("#TermsAndConditionsPoUp").data("ejDialog");
        eDialog.open();

        var gridObj = $("#GridTermsAndConditions").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering

    };


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridTermsAndConditions").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                $scope.searchdata[i].Flag = ChkOrUnchk;
            }

        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridTermsAndConditions").data("ejGrid");
        gridObj.refreshContent();

    };

    function MakeData() {

        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Flag == true) {
                if (checkExists($scope.TermsAndConditionsList, $scope.searchdata[i].TermsAndConditionsId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.TermsAndConditionsId = $scope.searchdata[i].TermsAndConditionsId;
                    ob.ContractId = $scope.modelNew.Id;
                    ob.Sequence = $scope.searchdata[i].Sequence;
                    ob.Code = $scope.searchdata[i].Code;
                    ob.ShortName = $scope.searchdata[i].ShortName;
                    ob.StandardName = $scope.searchdata[i].StandardName;
                    ob.UserName = $scope.searchdata[i].UserName;
                    ob.Description = $scope.searchdata[i].Description;

                    $scope.TermsAndConditionsList.push(ob);
                }
                //else {
                //    throw "This Terms & Conditions " + $scope.searchdata[i].UserName + " is already taken.";
                //}
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].TermsAndConditionsId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseTermsAndConditions = function () {
        try {
            MakeData();
            $scope.SaveTNC();
            var eDialog = $("#TermsAndConditionsPoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveTNC = function () {
        try {
            $http({
                method: 'POST',
                url: 'Commercial/Contract/CreateTNC',
                data: {
                    'data': $scope.TermsAndConditionsList
                    , 'contractId': $scope.modelNew.Id
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetContractTermsAndConditionsList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteTNC = function () {
        $http({
            method: 'POST',
            url: 'Commercial/Contract/DeleteContractTermsAndConditions?id=' + $scope.TNC.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetContractTermsAndConditionsList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion checkbox all

    $scope.message_detailLCconfirm = null;
    $scope.confirmToCreateLC = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.modelNew.ContractNo)) {
                throw "ContractNo is required.";
            }
            if (!$scope.modelNew.IsLC) {
                $scope.message_detailLCconfirm = "Please Confirm LC Applicable?";
                angular.element(document.querySelector("#confirmLCPopUp")).modal("show");
            }
            else {
                $scope.Save();
                angular.element(document.querySelector("#confirmLCPopUp")).modal("hide");
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.LCYes = function () {
        angular.element(document.querySelector("#confirmLCPopUp")).modal("hide");
    }

    $scope.Save = function () {
        try {
            var tq = 0;
            var amt = 0;
            var qt = 0;

            $scope.$broadcast('show-errors-check-validity');
            if (baseService.arrayLength($scope.SelectedSalesOrderList) === 0) {
                throw "Select Sales Order.";
            } else {
                for (var i = 0; i < $scope.SelectedSalesOrderList.length; i++) {
                    tq += $scope.SelectedSalesOrderList[i].TotalQty;
                    amt += $scope.SelectedSalesOrderList[i].Amount;
                    qt += $scope.SelectedSalesOrderList[i].Qty;
                }
            }
            $scope.modelNew.TotalQty = tq;
            $scope.modelNew.Amount = amt;
            $scope.modelNew.SOQty = qt;


            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'model': $scope.modelNew,
                        'selectedSalesOrderList': JSON.stringify($scope.SelectedSalesOrderList)
                        , 'funds': $scope.fundUtilizationList
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.modelNew.Id = response.data.Id;
                        $scope.contractList = [];
                        $scope.getSavedData();


                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetMasterLCData = function (contractId) {
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterLcData?contractId=" + contractId
        }).then(function (response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.lcMasterNew = response.data[0];
            } else {
                $scope.lcMasterNew = {
                    Id: null,
                    ContractId: null,
                    BenificiaryBankId: null,
                    OpeningBankId: null,
                    OpeningDescription: null,
                    LeinBankId: null,
                    LeinDescription: null,
                    LCRef: null,
                    LCDate: null,
                    ExpiryDate: null,
                    Amount: null,
                    Type: null,
                    Tenure: 0,
                    FinalDestinationId: null,
                    PortOfLandingId: null,
                    CurrencyId: null,
                    IsClose: false
                };
            }
            $scope.GetContractTermsAndConditionsList();
        });
    }

    //$scope.bankList = [];
    //$scope.getBank = function () {
    //    $http({
    //        method: 'GET',
    //        url: "Commercial/Contract/GetBankCbo"
    //    }).then(function (response) {
    //        $scope.bankList = response.data;
    //    });
    //}
    //$scope.getBank();

    $scope.NegotiatingBankList = [];
    $scope.GetNegotiatingBankList = function () {
        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetNegotiatingBankList'
        }).then(function successCallback(response) {
            $scope.NegotiatingBankList = response.data;
        });
    }
    $scope.GetNegotiatingBankList();

    $scope.SelectMasterLC = function (obj) {
        try {
            var gridObj = $("#GridLC").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];

            $scope.modelNew.MasterLCId = data.Id;
            $http({
                method: 'POST',
                url: $scope.updateContractUrl,
                data: {
                    'model': $scope.modelNew
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedData();
                    $scope.GetMasterLCData($scope.modelNew.Id);
                    $scope.setTab2(2);
                    angular.element(document.querySelector("#MasterLCPopUp")).modal("hide");
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ConfirmModal = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.modelNew.Id)) {
                $scope.setTab2(1);
                throw "Select Contract.";
            }
            if ($scope.modelNew.IsLC === false) {
                $scope.setTab2(1);
                throw "Check Is LC.";
            }
            $scope.message = "Do you want to tag with existing MasterLC?";
            angular.element(document.querySelector("#ConPopUp")).modal("show");
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.masterLCList = [];
    $scope.getMasterLCData = function () {
        $scope.masterLCList = [];
        $http.get("Commercial/Contract/GetMasterLCList?customerId=" + $scope.modelNew.CustomerId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector("#MasterLCPopUp")).modal("show");
    };

    $scope.ClosePopUp = function () {
        angular.element(document.querySelector("#MasterLCPopUp")).modal("hide");
    };

    $scope.NewMasterLC = function () {
        $scope.setTab2(2);
    }

    $scope.changeType = function () {
        if ($scope.lcMasterNew.Type === 'AtSight') {
            $scope.lcMasterNew.Tenure = 0;
        }
    }

    $scope.searchByNB = "UserName"; $scope.searchNB = "";

    $scope.NegotiatingBankDataList = [];
    $scope.searchByNBList = [{ value: 'BankName', name: "Bank Name" }, { value: 'UserName', name: "User Name" }, { value: 'AccountNo', name: "AccountNo" }, { value: 'Country', name: "Country" }];
    $scope.ShowNBPopUp = function () {
        $http({
            method: 'POST',
            url: 'Commercial/Contract/GetNegotiatingBankDataList',
            data: { column: $scope.searchByNB, value: $scope.searchNB },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NegotiatingBankDataList = response.data;
            angular.element(document.querySelector('#NBPopUp')).modal('show');
        });
    }

    $scope.SetNBData = function (obj) {
        $scope.lcMasterNew.OpeningBankId = obj.data.Id;
        $scope.lcMasterNew.OpeningBank = obj.data.BankName;
        angular.element(document.querySelector('#NBPopUp')).modal('hide');
    }

    $scope.CloseNB = function () {
        angular.element(document.querySelector('#NBPopUp')).modal('hide');
    }


    $scope.SaveMasterLC = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.lcMasterNew.Type === 'Usance') {
                if ($scope.lcMasterNew.Tenure === 0 || $scope.lcMasterNew.Tenure < 0) {
                    throw "Usance value must greater than 0.";
                }
            } else {
                $scope.lcMasterNew.Tenure = 0;
            }
            if ($scope.MasterLCForm.$valid) {
                $scope.lcMasterNew.ContractId = $scope.modelNew.Id;
                $scope.lcMasterNew.CustomerId = $scope.modelNew.CustomerId;
                $http({
                    method: 'POST',
                    url: $scope.saveMasterLCUrl,
                    data: {
                        'entity': $scope.lcMasterNew
                        , 'contract': $scope.modelNew

                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetMasterLCData($scope.modelNew.Id);
                        $scope.getSavedData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {

        $scope.model = {
            Id: null,
            CompanyId: null,
            MasterOrderId: null,
            ContractNo: null,
            CustomerId: null,
            Descriotion: null,
            IsLC: false,
            CustomerName: null,
            Currency: null,
            TotalQty: 0,
            SOQty: 0,
            Amount: 0,
            UDNo: null,
            IsPrint: false,
            IsMarketingCommisssionApplicable: false,
            MarketingCommisssionId: null,
            IsBusinessDevelopmentChargesApplicable: false,
            BusinessDevelopmentCharge: 'Percentage',
            MarketingCommisssionCharge: 'Percentage',
            BusinessDevelopmentChargeValue: null,
            MarketingCommisssionChargeValue: null,
            InvoicingPartyPlantId: null,
            DeliveryPartyPlantId: null,
            InvoicingByAddress: null,
            DeliveryByAddress: null
        };
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.msg = null;
        $scope.lcMaster = {
            Id: null,
            ContractId: null,
            BenificiaryBankId: null,
            OpeningBankId: null,
            OpeningDescription: null,
            LeinBankId: null,
            LeinDescription: null,
            LCRef: null,
            LCDate: null,
            ExpiryDate: null,
            Amount: null,
            Type: null,
            Tenure: 0,
            FinalDestinationId: null,
            PortOfLandingId: null,
            CurrencyId: null,
            IsClose: null
        };
        $scope.lcMasterNew = Object.assign({}, $scope.lcMaster);

        $scope.lcMasterNew.Id = null;
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            $scope.lcMasterNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });
        $scope.masterOrderCustomerList = [];
        $scope.fundUtilizationList = [];
        $scope.buyerDeductionList = [];
        $scope.TermsAndConditionsList = [];
        $scope.SalesOrderList = [];
        $scope.SelectedSalesOrderList = [];
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

    $scope.SetMarComValues = function () {
        if ($scope.modelNew.IsMarketingCommisssionApplicable === false) {
            $scope.modelNew.MarketingCommisssion = null;
            $scope.modelNew.MarketingCommisssionId = null;
            $scope.modelNew.MarketingCommisssionCharge = 'Percentage';
            $scope.modelNew.MarketingCommisssionValue = null;
        }
    }

    $scope.SetBusinessDevelopmentValues = function () {
        if ($scope.modelNew.IsBusinessDevelopmentChargesApplicable === false) {
            $scope.modelNew.BusinessDevelopmentCharge = 'Percentage';
            $scope.modelNew.BusinessDevelopmentChargeValue = null;
        }
    }


    // #region Contract report
    $scope.MasterOrderReport = function (obj) {
        var gridObj = $("#Grid2").data("ejGrid");
        var data = obj.data;
        $scope.modelNew.ContractNo = data.Id;

        try {
            var file_src = $scope.path + "MasterOrder?ContractId=" + $scope.modelNew.ContractNo + "&isMatrix=true";
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GetDetailsReport = function (obj) {
        var gridObj = $("#Grid2").data("ejGrid");
        var data = obj.data;
        $scope.modelNew.ContractNo = data.Id;

        try {
            var file_src = $scope.path + "GetContractDetailsReport?ContractId=" + $scope.modelNew.ContractNo + "&isMatrix=true";
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.ProformaInvoiceReport = function (obj) {
        var gridObj = $("#Grid2").data("ejGrid");
        try {
            var file_src = $scope.path + "ProformaInvoice?ContractId=" + obj.data.Id;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ProformaInvoiceReport1 = function (obj) {
        var gridObj = $("#Grid2").data("ejGrid");
        try {
            var file_src = $scope.path + "ProformaInvoice1?ContractId=" + obj.data.Id;
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    // #endregion Contract Details Report


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
                    $scope.getSavedData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.partyList = [];
    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.flag = null;
    //$scope.partyType = 'Vendor';
    //$scope.showPartyPopUp = function () {

    //    baseService.setCurrentPage('partyList');
    //    $scope.getPartyList = function (pageno) {
    //        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
    //            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
    //        }
    //        else if ($scope.partyType === 'Party') {
    //            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
    //        }
    //        else if ($scope.partyType === 'Director') {
    //            $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
    //        }
    //        else if ($scope.partyType === 'Other') {
    //            $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
    //        }
    //        baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
    //            .then(function (result) {
    //                $scope.partyList = result.Rows;
    //                $scope.partyParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#partyPopUp')).modal('show');
    //    $scope.getPartyList();
    //};

    //$scope.selectPartyPopUpRow = function (index, id) {
    //    $scope.partyIndex = index;
    //    $scope.selectedParty = id;
    //};

    //$scope.selectCustomerPopUp = function (index, id) {
    //    $scope.partyIndex = index;
    //    $scope.selectedCustomer = id;
    //};

    //$scope.hidePartyPopUp = function () {
    //    angular.element(document.querySelector('#partyPopUp')).modal('hide');
    //    $scope.partyIndex = -1;
    //    $scope.partySelected = null;
    //};

    //$scope.closePartyPopUp = function (x) {
    //    var party = x.data;

    //    $scope.modelNew.MarketingCommisssion = party.UserName;
    //    $scope.modelNew.MarketingCommisssionId = party.Id;
    //    $scope.modelNew.PaymentTermId = party.PaymentTermId;
    //    $scope.modelNew.CurrencyId = party.CurrencyId;


    //    $scope.hidePartyPopUp();
    //};

    $scope.getPartyPlant = function () {
        $scope.getCboPartyPlantList($scope.modelNew.CustomerId, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.modelNew.InvoicingPartyPlantId = item.Value;
                    $scope.modelNew.DeliveryPartyPlantId = item.Value;
                    $scope.modelNew.InvoicingByAddress = item.Address1;
                    $scope.modelNew.DeliveryByAddress = item.Address1;
                    $scope.modelNew.InvoicingState = item.StateName;
                    $scope.modelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.modelNew.DeliveryState = item.StateName;
                    $scope.modelNew.DeliveryGSTIN = item.GSTIN;
                    $scope.modelNew.InvoicingStateId = item.StateId;
                }
            });
        });
    }

    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.modelNew.CustomerId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    $scope.partyPlantId = item.Value;
                    $scope.modelNew.InvoicingPartyPlantId = item.Value;
                    $scope.modelNew.DeliveryPartyPlantId = deliveryplant;
                    $scope.modelNew.InvoicingByAddress = invoAddress;
                    $scope.modelNew.DeliveryByAddress = deliAddress;
                    $scope.modelNew.InvoicingState = item.StateName;
                    $scope.modelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.modelNew.DeliveryState = deliState;
                    $scope.modelNew.DeliveryGSTIN = deliGSTIN;
                    $scope.modelNew.InvoicingStateId = item.StateId;
                }
            });

        });

    }

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };

    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };

    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
            if (flag === 'billTo') {
                $scope.modelNew.InvoicingState = state;
                $scope.modelNew.ChangeInvoicingStateId = stateId;
                $scope.modelNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.modelNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.modelNew.DeliveryState = state;
                $scope.modelNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.modelNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.modelNew.InvoicingState = null;
                $scope.modelNew.InvoicingGSTIN = null;
                return $scope.modelNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.modelNew.DeliveryState = null;
                $scope.modelNew.DeliveryGSTIN = null;
                return $scope.modelNew.DeliveryByAddress = null;
            }
        }
    };

    // #region ContractItem

    $scope.selectedmasterOrderDataList = [];
    $scope.GetContractItemDataList = function () {

        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetContractItemDataList?contractId=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            $scope.selectedmasterOrderDataList = response.data;
        });
    }

    $scope.masterOrderDataList = [];
    $scope.GetMasterOrderDataList = function () {
        $scope.masterOrderDataList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderDataList"
        }).then(function (response) {
            $scope.masterOrderDataList = response.data;

            if (baseService.arrayLength($scope.selectedmasterOrderDataList) > 0) {
                for (var i = 0; i < $scope.selectedmasterOrderDataList.length; i++) {
                    for (var j = 0; j < $scope.masterOrderDataList.length; j++) {
                        if ($scope.selectedmasterOrderDataList[i].MasterOrderId === $scope.masterOrderDataList[j].MasterOrderId && $scope.selectedmasterOrderDataList[i].MasterOrderItemId === $scope.masterOrderDataList[j].MasterOrderItemId) {
                            $scope.masterOrderDataList[j].Active = true;
                        }
                    }
                }
            }
            angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
        });
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridOperation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.masterOrderDataList.length; i++) {
                $scope.masterOrderDataList[i].Active = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridOperation").data("ejGrid");
        gridObj.refreshContent();
    };
    $scope.hasError = false;

    $scope.selectedmasterOrderDataList = [];
    function MakeContractItemData() {
        $scope.selectedmasterOrderDataList = [];
        try {
            for (var i = 0; i < $scope.masterOrderDataList.length; i++) {
                var getRow = $filter("filter")($scope.selectedmasterOrderDataList, { "selectedmasterOrderDataList": $scope.masterOrderDataList[i].MasterOrderId, "MasterOrderItemId": $scope.masterOrderDataList[i].MasterOrderItemId });
                //var getRow = $filter("filter")($scope.selectedMasterOrderList, { "selectedMasterOrderList": $scope.masterOrderList[i].MasterOrderId });
                if (getRow.length == 0) {
                    if ($scope.masterOrderDataList[i].Active == true) {
                        var ob = {};
                        ob.MasterOrderId = $scope.masterOrderDataList[i].MasterOrderId;
                        ob.MasterOrderItemId = $scope.masterOrderDataList[i].MasterOrderItemId;
                        ob.PartyId = $scope.masterOrderDataList[i].PartyId;

                        if (checkExistCustomer($scope.selectedmasterOrderDataList, ob.PartyId)) {
                            if (checkExistList($scope.selectedmasterOrderDataList, ob.MasterOrderId, ob.MasterOrderItemId) === false) {
                                ob.Id = null;
                                ob.Active = $scope.masterOrderDataList[i].Active;
                                ob.MaterialMaster = $scope.masterOrderDataList[i].MaterialMaster;
                                ob.Article = $scope.masterOrderDataList[i].Article;
                                ob.OrderType = $scope.masterOrderDataList[i].OrderType;
                                ob.CustomerName = $scope.masterOrderDataList[i].CustomerName;
                                ob.MasterOrderNo = $scope.masterOrderDataList[i].MasterOrderId;
                                ob.MaterialMasterId = $scope.masterOrderDataList[i].MaterialMasterId;
                                ob.ArticleId = $scope.masterOrderDataList[i].ArticleId;
                                ob.MasterOrderItemId = $scope.masterOrderDataList[i].MasterOrderItemId;
                                ob.TotalQty = $scope.masterOrderDataList[i].TotalQty;
                                ob.Qty = $scope.masterOrderDataList[i].Qty;
                                ob.Rate = $scope.masterOrderDataList[i].Rate;
                                ob.Amount = $scope.masterOrderDataList[i].Amount;
                                ob.Currency = $scope.masterOrderDataList[i].Currency;
                                ob.BuyerItemRef = $scope.masterOrderDataList[i].BuyerItemRef;
                                ob.OwnItemRef = $scope.masterOrderDataList[i].OwnItemRef;


                                $scope.selectedmasterOrderDataList.push(ob);

                                $scope.hasError = false;
                            }
                        }
                        else {
                            $scope.hasError = true;
                            throw 'Select same Customer.';
                        }
                    }

                }

            }

        } catch (e) {
            ShowResult(e, 'failure', 'masterOrderPopUp');
        }
    }

    function checkExistCustomer(list, customerId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== customerId) {
                return false;
            }
        }
        return true;
    }

    function checkExistList(list, MasterOrderId, MasterOrderItemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MasterOrderId === MasterOrderId && list[i].MasterOrderItemId === MasterOrderItemId) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseMasterOrderPopUp = function () {
        MakeContractItemData();
        $scope.SaveContractItem();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    };

    $scope.SaveContractItem = function () {
        try {
            $http({
                method: 'POST',
                url: 'Commercial/Contract/CreateContractItem',
                data: {
                    'data': $scope.selectedmasterOrderDataList
                    , 'contractId': $scope.modelNew.Id
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetContractItemDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.removeRowModal = function (index, data) {
        try {
            $scope.LCChargesId = data.Id;
            $scope.bActivityIndex = index;
            if (baseService.isUndefinedOrNull($scope.LCChargesId))
                $scope.message = 'Are you sure want to delete this data....';
            else
                $scope.message = 'Are you sure want to delete permanently';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteMOI = function () {
        if (baseService.isUndefinedOrNull($scope.LCChargesId)) {
            $scope.selectedmasterOrderDataList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Commercial/Contract/DeleteContractItems?id=' + $scope.LCChargesId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.selectedmasterOrderDataList.splice($scope.bActivityIndex, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion ContractItem

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.GetReport = function (reportType) {
        try {

            $http({
                method: 'POST',
                url: $scope.path + 'GetContarctReport',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}

