'use strict';
PurchaseLCWithPOController.$inject = ['accountService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function PurchaseLCWithPOController(accountService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "Purchase LC";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/PurchaseLCWithPO/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'CreateCharge';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $scope.purchaseLC = {
        Id: null,
        PlantId: $window.plantId,
        ContractId: null,
        VendorId: null,
        BenificiaryBank: null,
        OpeningBankMasterId: null,
        BenificiaryDescription: null,
        LeinDescription: null,
        LeinBankId: null,
        OrderSpecific: 'Yes',
        LCRef: null,
        LCDate: null,
        ExpiryDate: null,
        //AcceptanceDate: null,
        //MaturityDate: null,
        //PaymentDate: null,
        Amount: null,
        Type: null,
        Tenure: 0,
        FinalDestinationId: null,
        PortOfLandingId: null,
        PortOfLoading: null,
        IsClose: false,
        CurrencyId: null,
        Rate: 0,
        Version: 0,
        AmendmentDate: null,
        BankCurrency: null,
        LCANo: null,
        LIBOUR: null,
        InsuranceCoverNoteNo: null,
        InsuranceValue: null,
        InsuranceAttachment: null,
        PaymentBasedOn: null,
        IsAccepptanceFirst: 'true',
        Status: 'Active'
    };
    $scope.purchaseLCNew = Object.assign({}, $scope.purchaseLC);

    $scope.purchaseLCCharges = {
        Id: null,
        PurchaseLCId: null,
        OverHeadTypeGLId: null,
        Remarks: null,
        ChargesValue: 0,
        CurrencyId: null,
        Rate: 0,
        VendorId: null,
        Version: 0,
        BankAmount: 0
    };
    $scope.purchaseLCChargesNew = Object.assign({}, $scope.purchaseLCCharges);

    $scope.paymentBasedOnList = [];
    cboService.getEnumCbo("enum/GetPaymentBasedOnEnumCbo", function (result) {
        $scope.paymentBasedOnList = result;
    });

    $scope.show = function () {
        $scope.purchaseLCNew.OrderSpecific = 'Yes';

    };
    $scope.hide = function () {
        $scope.purchaseLCNew.OrderSpecific = 'No'
        $scope.purchaseLCNew.ContractId = null;
    };

    $scope.StatusList = [
        { Value: "Active", Text: "Active" },
        { Value: "Hold", Text: "Hold" },
        { Value: "Cancel", Text: "Cancel" },
        { Value: "Closed", Text: "Closed" }
    ];

    $scope.Get = function (obj) {
        $scope.PurchaseLCUsedInAcceptance = false;
        $scope.purchaseLC = obj.data;
        $scope.purchaseLC.LCDate = $filter('dateFiltering')($scope.purchaseLC.LCDate, 'dd-M-yyyy');
        $scope.purchaseLC.AmendmentDate = $filter('dateFiltering')($scope.purchaseLC.AmendmentDate, 'dd-M-yyyy');
        $scope.purchaseLCNew = Object.assign({}, $scope.purchaseLC);
        $scope.AmendmentDate = $scope.purchaseLCNew.AmendmentDate;
        if ($scope.purchaseLCNew.Version > 1) {
            $scope.version = $scope.purchaseLCNew.PreVersion;
            $scope.getBackData();
        } else {
            $scope.version = $scope.purchaseLCNew.Version;
        }
        getPurchaseLCChargesBackData($scope.purchaseLCNew.Id, $scope.version);
        GetAlldataPOWithLCMap($scope.purchaseLCNew.Id);
        //$scope.ChangeBankMaster();


        $scope.GetPurchaseLCUsedInAcceptance($scope.purchaseLCNew.Id);
        if ($scope.purchaseLCNew.IsAccepptanceFirst) {
            $scope.purchaseLCNew.IsAccepptanceFirst = 'true';
        } else {
            $scope.purchaseLCNew.IsAccepptanceFirst = 'false';
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };

    $scope.PurchaseLCUsedInAcceptance = false;
    $scope.isFirst = false;
    $scope.GetPurchaseLCUsedInAcceptance = function (purchaseLCId) {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLC/GetPurchaseLCUsedInAcceptance?purchaseLCId=' + purchaseLCId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.PurchaseLCUsedInAcceptance = true;
            } else {
                $scope.PurchaseLCUsedInAcceptance = false;
            }
        });
    };

    $scope.PlantCountryId = null;
    $scope.getPantCountry = function () {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLC/GetPlantCountry'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.PlantCountryId = response.data[0].PlantCountryId;
            }
            $scope.GetPortByPlantCountry($scope.PlantCountryId);
            accountService.getTaxCategoryMaterialLevelCbo($scope.PlantCountryId, function (result) {
                $scope.taxCategoryList = result;
            });
        });
    };
    $scope.getPantCountry();
    $scope.IsAccepptanceFirst = false;
    $scope.PartyCountryId = null;
    $scope.getVendorCountry = function () {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLC/GetVendorCountry?vendorId=' + $scope.purchaseLCNew.VendorId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.PartyCountryId = response.data[0].PartyCountryId;

                if ($scope.PlantCountryId == $scope.PartyCountryId) {
                    $scope.purchaseLCNew.IsAccepptanceFirst = 'false';
                    $scope.IsAccepptanceFirst = false;

                } else {
                    $scope.purchaseLCNew.IsAccepptanceFirst = 'true';
                    $scope.IsAccepptanceFirst = true;
                }

            }
        });
    };

    $scope.portList = [];
    $scope.GetPortByPlantCountry = function (CountryId) {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLCWithPO/GetPortByPlantCountry?CountryId=' + CountryId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.portList = response.data;
            }
        });
    };


    $scope.getBackData = function () {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLC/GetPurchaseLCBackData?purchaseLCId=' + $scope.purchaseLCNew.Id + '&Version=' + $scope.version
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.purchaseLCNew = response.data[0];
                $scope.purchaseLCNew.Id = $scope.purchaseLCNew.PurchaseLCId;
            }

        });
    };

    $scope.voucherId = null;
    function getPurchaseLCChargesBackData(purchaseLCId) {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLC/GetPurchaseLCChargesVersionData?purchaseLCId=' + purchaseLCId + '&Version=' + $scope.version
        }).then(function successCallback(response) {
            $scope.purchaseLCChargesList = response.data;
            if (!baseService.isUndefinedOrNull($scope.purchaseLCChargesList[0].VoucherId)) {
                $scope.voucherId = $scope.purchaseLCChargesList[0].VoucherId;
            }
        });
    }

    function GetAlldataPOWithLCMap(purchaseLCId) {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLCWithPO/GetAlldataPOWithLCMap?purchaseLCId=' + purchaseLCId
        }).then(function successCallback(response) {
            $scope.selectedPOList = response.data;
        });
    }

    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        $scope.contractList = [];
        $http.get("Commercial/Contract/getlist")
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

    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.SelectedContract = function (obj) {
        var data = obj.data;
        $scope.purchaseLCNew.ContractId = data.Id;
        $scope.purchaseLCNew.ContractNo = data.ContractNo;
        $scope.purchaseLCNew.CustomerName = data.CustomerName;
        //$scope.purchaseLCNew.LCRef = data.LCRef;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.closePartyPopUp = function (x) {

        var party = x.data;
        $scope.purchaseLCNew.VendorId = party.Id;
        $scope.purchaseLCNew.PartyCode = party.Code;
        $scope.purchaseLCNew.PartyName = party.UserName;
        $scope.purchaseLCNew.CurrencyId = party.CurrencyId;
        $scope.GetCurrencyExchangeRateList();
        $scope.getVendorCountry();

        //if ($scope.PantCountryId == $scope.PartyCountryId) {
        //    $scope.purchaseLCNew.IsAccepptanceFirst = 'false';
        //} else {
        //    $scope.purchaseLCNew.IsAccepptanceFirst = 'true';
        //}

        $scope.hidePartyPopUp();
    };

    $scope.clearVendor = function () {
        $scope.purchaseLCNew.VendorId = null;
        $scope.purchaseLCNew.PartyCode = null;
        $scope.purchaseLCNew.PartyName = null;
    }

    $scope.clearContract = function () {
        $scope.purchaseLCNew.ContractId = null;
        $scope.purchaseLCNew.ContractNo = null;
        $scope.purchaseLCNew.CustomerName = null;
    }

    $scope.companyCurrencyId = null;
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    }).then(function successCallback(response) {
        angular.forEach(response.data, function (item, i) {
            if (item.ParallelCurrencyType === 'CompanyCurrency') {
                $scope.companyCurrencyId = item.CurrencyId;
            }
        });
    });

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.purchaseLCNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.purchaseLCNew.Rate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };
    $scope.bankMasterList = [];
    bankService.getBankMasterCboListByPlant(function (result) {
        $scope.bankMasterList = result;

    });

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.purchaseLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        $scope.GetCurrencyExchangeRateList();
    });


    // #region PO     

    $scope.GriddataPOWithOutLC = [];
    $scope.getalldataPOWithOutLC = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Commercial/PurchaseLCWithPO/GetAlldataPOWithoutLCMap',
        }).then(function successCallback(response) {
            $scope.GriddataPOWithOutLC = response.data;
        });

        //var gridObj = $("#Grid456").data("ejGrid");
        //gridObj.refreshContent(true);
        //gridObj.refreshTemplate();
    };
    $scope.getalldataPOWithOutLC();

    $scope.selectedPOList = [];
    $scope.LcAmount = 0;
    $scope.MakeData = function () {
        try {
            $scope.LcAmount = 0;

            var i = $scope.GriddataPOWithOutLC.length;
            while (i--) {
                if ($scope.GriddataPOWithOutLC[i].check === true) {

                    var ob = {};

                    ob.ContractId = $scope.GriddataPOWithOutLC[i].ContractId;
                    ob.VendorId = $scope.GriddataPOWithOutLC[i].PartyId;
                    ob.CurrencyId = $scope.GriddataPOWithOutLC[i].CurrencyId;
                    ob.PaymentTermId = $scope.GriddataPOWithOutLC[i].PaymentTermId;
                    ob.Id = $scope.GriddataPOWithOutLC[i].Id;

                    //if (checkSameVendor($scope.selectedPOList, ob.VendorId, ob.CurrencyId, ob.ContractId)) {
                    if (checkSame($scope.selectedPOList, ob.VendorId, ob.CurrencyId, ob.PaymentTermId)) {
                        if (checkExistList($scope.selectedPOList, ob.Id) === false) {

                            $scope.purchaseLCNew.VendorId = $scope.GriddataPOWithOutLC[i].PartyId;
                            $scope.purchaseLCNew.PartyId = $scope.GriddataPOWithOutLC[i].PartyId;
                            $scope.purchaseLCNew.CurrencyId = $scope.GriddataPOWithOutLC[i].CurrencyId;
                            $scope.purchaseLCNew.PartyName = $scope.GriddataPOWithOutLC[i].StandardName;
                            $scope.purchaseLCNew.Currency = $scope.GriddataPOWithOutLC[i].Currency;
                            $scope.purchaseLCNew.ContractId = $scope.GriddataPOWithOutLC[i].ContractId;
                            $scope.purchaseLCNew.ContractNo = $scope.GriddataPOWithOutLC[i].ContractNo;
                            $scope.purchaseLCNew.CustomerName = $scope.GriddataPOWithOutLC[i].CustomerName;
                            $scope.purchaseLCNew.OrderSpecific = $scope.GriddataPOWithOutLC[i].OrderSpecifi;
                            $scope.isFirst = $scope.GriddataPOWithOutLC[i].IsFirst;
                            if ($scope.GriddataPOWithOutLC[i].IsFirst) {
                                $scope.purchaseLCNew.IsAccepptanceFirst = 'false';
                            }

                            $scope.GriddataPOWithOutLC[i].check = false;
                            $scope.selectedPOList.push($scope.GriddataPOWithOutLC[i]);
                            $scope.GriddataPOWithOutLC.splice(i, 1);
                            $scope.GetCurrencyExchangeRateList();
                        }
                    } else {
                        //throw "Please select same Vendor, Currency and Contract.";
                        throw "Please select same Vendor, Currency & Payment Term.";
                    }
                }
            }
            
            $scope.purchaseLCNew.Amount = Math.round(($filter('sumByKey')($filter('filter')($scope.selectedPOList), 'BalanceAmount') * 1000 + Number.EPSILON) / 1000);
        } catch (e) {
            ShowResult(e, 'failure');
        }
        var gridObj = $("#Grid45").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    };

    $scope.RemovePO = function () {
        $scope.LcAmount = 0;
        if (baseService.arrayLength($scope.selectedPOList) > 0) {
            var i = $scope.selectedPOList.length;
            while (i--) {
                if ($scope.selectedPOList[i].check === true) {
                    $scope.selectedPOList[i].check = false;
                    $scope.GriddataPOWithOutLC.push($scope.selectedPOList[i]);
                    $scope.selectedPOList.splice(i, 1);
                }
            }
        }
        $scope.purchaseLCNew.Amount = Math.round(($filter('sumByKey')($filter('filter')($scope.selectedPOList), 'Amount') * 1000 + Number.EPSILON) / 1000);
    }

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
        $scope.actionCompleteUnassign();
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid45").ejGrid("instance");
                var scrollerwidth = $("#Assigned").width();//Obtain the width of the container

                $("#Grid45").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 220 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.actionCompleteUnassign = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid456").ejGrid("instance");
                var scrollerwidth = $("#Unassign").width();//Obtain the width of the container

                $("#Grid456").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 220 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };


    $scope.summaryassignRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionAmount", dataMember: "TransactionAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];

    $scope.summaryUnassignRows = [{
        title: "Total", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionAmount", dataMember: "TransactionAmount", format: "{0:N2}" }],
        showCaptionSummary: true
    }];


    function checkSameVendor(list, vendorId, currencyId, ContractId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== vendorId || list[i].CurrencyId !== currencyId || list[i].ContractId !== ContractId) {
                return false;
            }
        }
        return true;
    }
    function checkSame(list, vendorId, currencyId, PaymentTermId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PartyId !== vendorId || list[i].CurrencyId !== currencyId || list[i].PaymentTermId !== PaymentTermId) {
                return false;
            }
        }
        return true;
    }

    function checkExistList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == Id) {
                return true;
            }
        }
        return false;
    }
    // #endregion

    $scope.purchaseLCList = [];
    $scope.getSavedData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/PurchaseLCWithPO/getlist")
            .then(
                function successCallback(response) {

                    //for (var i = 0; i < response.data.length; i++) {
                    //    response.data[i]["LCDate"] = new Date(response.data[i]["LCDate"]);
                    //    response.data[i]["AmendmentDate"] = new Date(response.data[i]["AmendmentDate"]);
                    //}
                    $scope.purchaseLCList = response.data;

                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();

    function Validation() {
        if ($scope.purchaseLCNew.Back === 1) {
            throw "Data update is not possible when it has another Version.";
        }

        if ($scope.purchaseLCNew.OrderSpecific === 'Yes') {
            if (baseService.isUndefinedOrNull($scope.purchaseLCNew.ContractId)) {
                throw "Contract is required.";
            }
        }
        if (baseService.isUndefinedOrNull($scope.purchaseLCNew.VendorId)) {
            throw "Vendor is required.";
        }
        //if (baseService.isUndefinedOrNull($scope.purchaseLCNew.Type)) {
        //    throw "LC Type is required.";
        //}
        if ($scope.purchaseLCNew.Type === 'Usance') {
            if ($scope.purchaseLCNew.Tenure === 0 || $scope.purchaseLCNew.Tenure < 0) {
                throw "Usance value must greater than 0.";
            }
        } else {
            $scope.purchaseLCNew.Tenure = 0;
        }

        //if ($scope.purchaseLCChargesList.length === 0) {
        //    throw "Add Opening Charges.";
        //}
        if (baseService.arrayLength($scope.purchaseLCChargesList) > 0) {
            for (var i = 0; i < $scope.purchaseLCChargesList.length; i++) {
                if ($scope.purchaseLCChargesList[i].ChargesValue < 0 || $scope.purchaseLCChargesList[i].ChargesValue === 0) {
                    throw "Charges Value must greater than 0 for " + $scope.purchaseLCChargesList[i].OverHeadType + ".";
                }
                if ($scope.purchaseLCChargesList[i].BankAmount < 0 || $scope.purchaseLCChargesList[i].BankAmount === 0) {
                    throw "Bank Amount must greater than 0 for " + $scope.purchaseLCChargesList[i].OverHeadType + ".";
                }
            }
        }

    }

    //confirm popup
    $scope.confirmToCreateNewVersion = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.PurchaseLCForm.$valid) {

            if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.Id)) {
                $scope.message_confirmation = "Is it an Amendment?";
                angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
            }
            else {
                $scope.Save();
                angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
            }
        }
    };

    $scope.Amendment = function () {
        $scope.message_Amendment = "Please go to Purchase LC Amendment screen.?";
        angular.element(document.querySelector("#confirmAmendmentPopUp")).modal("show");
    }

    $scope.CloseAmendmentPopUp = function () {
        angular.element(document.querySelector("#confirmAmendmentPopUp")).modal("hide");
    }

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
    };

    $scope.Save = function () {
        $scope.materialPoList = [];
        $scope.servcePoList = [];
        $scope.jwOutSourcePoList = [];
        try {
            Validation();

            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.purchaseLCNew.InsuranceAttachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.InsuranceAttachment)) {
                if ($scope.purchaseLCNew.InsuranceAttachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            var formData = new FormData();

            if (baseService.arrayLength($scope.selectedPOList) > 0) {
                for (var i = 0; i < $scope.selectedPOList.length; i++) {
                    if ($scope.selectedPOList[i].Flag == 'MaterialPO') {
                        $scope.materialPoList.push($scope.selectedPOList[i]);
                    }
                    else if ($scope.selectedPOList[i].Flag == 'ServicePO') {
                        $scope.servcePoList.push($scope.selectedPOList[i]);
                    }
                    else {
                        $scope.jwOutSourcePoList.push($scope.selectedPOList[i]);
                    }
                }
            }

            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("model", angular.toJson(data.model));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        formData.append("Charges", angular.toJson(data.Charges));
                        formData.append("POList", angular.toJson(data.POList));
                        formData.append("SPOList", angular.toJson(data.SPOList));
                        formData.append("JWPOList", angular.toJson(data.JWPOList));
                        return formData;
                    },
                    data: { 'model': $scope.purchaseLCNew, 'file': $scope.filedata, 'Charges': $scope.purchaseLCChargesList, 'POList': $scope.materialPoList, 'SPOList': $scope.servcePoList, 'JWPOList': $scope.jwOutSourcePoList }


                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.purchaseLCNew.Id = response.data.Id;
                        $scope.purchaseLCNew.Version = parseInt(response.data.Version);
                        $scope.getSavedData();
                        getPurchaseLCChargesData($scope.purchaseLCNew.Id);
                        GetAlldataPOWithLCMap($scope.purchaseLCNew.Id);
                        $scope.getalldataPOWithOutLC();
                        $scope.Action = 'Update';
                        angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.bankList = [];
    $scope.getBank = function () {
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetBankCbo"
        }).then(function (response) {
            $scope.bankList = response.data;
        });
    }
    $scope.getBank();

    $scope.changeType = function () {
        if ($scope.purchaseLCNew.Type === 'AtSight') {
            $scope.purchaseLCNew.Tenure = 0;
        }
    }

    $scope.ChangeBankMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.OpeningBankMasterId)) {
            $scope.purchaseLCNew.BankCurrency = $.grep($scope.bankMasterList, function (item) {
                return item.Id === $scope.purchaseLCNew.OpeningBankMasterId;
            })[0].CurrencyId;
            // $scope.purchaseLCNew.CurrencyId = $scope.purchaseLCNew.BankCurrency;
            $scope.GetCurrencyExchangeRateList();
        }
    }

    $scope.BankAmountFlag = false;
    $scope.ChargesIndex = -1;
    $scope.ChangeChargesBank = function (currencyId, index) {
        $scope.ChargesIndex = index;
        if (currencyId === $scope.purchaseLCNew.BankCurrency) {
            $scope.purchaseLCChargesList[$scope.ChargesIndex].BankAmount = $scope.purchaseLCChargesList[$scope.ChargesIndex].ChargesValue;
            $scope.purchaseLCChargesList[$scope.ChargesIndex].BankAmountFlag = true;
        } else {
            $scope.purchaseLCChargesList[$scope.ChargesIndex].BankAmountFlag = false;
            $scope.purchaseLCChargesList[$scope.ChargesIndex].BankAmount = 0;
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.purchaseLCNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.purchaseLCList.splice($scope.index, 1);
                    $scope.getSavedData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
    }

    function ClearFields() {
        $scope.voucherId = null;
        $scope.purchaseLC = {};
        $scope.purchaseLCNew = { OrderSpecific: 'Yes', Id: null, Tenure: 0, Version: 0, IsAccepptanceFirst: 'true', Status: 'Active' };
        $scope.purchaseLCChargesList = [];
        $scope.Action = 'Save';
        $scope.version = 0;
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            $scope.purchaseLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });
        $scope.selectedPOList = [];
        $scope.getalldataPOWithOutLC();
        $scope.IsAccepptanceFirst = false;
        $scope.PurchaseLCUsedInAcceptance = false;
        $scope.isFirst = false;
    }

    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        try {
            if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.ContractId)) {
                $http({
                    method: 'GET',
                    url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.purchaseLCNew.ContractId
                }).then(function (response) {
                    $scope.masterOrderCustomerList = response.data;
                });
                angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
            } else {
                throw "Select contract.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N0}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amt", dataMember: "Amt", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }

    // #region Charges

    $scope.GetCurrencyExchangeChargesRateList = function (currencyId, index) {
        if (!baseService.isUndefinedOrNull(currencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + currencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.purchaseLCChargesList[index].Rate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    function GetCompanyCurrencyExchangeRateCharges() {
        if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.BankCurrency)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.purchaseLCNew.BankCurrency
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.Rate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
    }

    $scope.LCChargesList = [];
    $scope.GetPurchaseLCCharges = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.purchaseLCNew.OpeningBankMasterId)) {
                throw 'Select Opening Bank.';
            }
            $scope.LCChargesList = [];
            $http.get("Commercial/PurchaseLC/GetOpenLCChargesGLData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.LCChargesList = response.data;
                            //if (baseService.arrayLength($scope.purchaseLCChargesList) > 0) {
                            //    for (var i = 0; i < $scope.purchaseLCChargesList.length; i++) {
                            //        for (var j = 0; j < $scope.LCChargesList.length; j++) {
                            //            if ($scope.LCChargesList[j].Id === $scope.purchaseLCChargesList[i].OverHeadTypeGLId) {
                            //                $scope.LCChargesList[j].Active = true;
                            //            }
                            //        }
                            //    }
                            //}
                            GetCompanyCurrencyExchangeRateCharges();
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#LCChargesPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseLCPopUp = function () {
        angular.element(document.querySelector('#LCChargesPopUp')).modal('hide');
    }

    $scope.purchaseLCChargesList = [];
    $scope.Rate = null;
    $scope.SelectedLC = function () {
        if (baseService.arrayLength($scope.LCChargesList) > 0) {
            angular.forEach($scope.LCChargesList, function (a) {
                if (a.Active) {
                    if (checkLCExist($scope.purchaseLCChargesList, a.Id) === false) {
                        $scope.purchaseLCChargesList.push({
                            Id: null
                            , OverHeadTypeGLId: a.Id
                            , PurchaseLCId: $scope.purchaseLCNew.Id
                            , OpeningBankMasterId: $scope.purchaseLCNew.OpeningBankMasterId
                            , OpeningBankMaster: $("#OB option:selected").text()
                            , GL: a.GL
                            , Budget: a.Budget
                            , Activity: a.Activity
                            , Remarks: null
                            , VoucherId: null
                            , ChargesValue: null
                            , Rate: $scope.Rate
                            , BankAmount: null
                            , Version: $scope.purchaseLCNew.Version
                            , OverHeadType: a.OverHeadType
                            , BankCurrency: $scope.purchaseLCNew.BankCurrency
                            , BankCurrencyId: $scope.purchaseLCNew.BankCurrency
                            , CurrencyId: $scope.purchaseLCNew.BankCurrency
                            , BankAmountFlag: $scope.BankAmountFlag
                            , Type: a.Type
                        });
                    }
                }

            });
        }
        else
            angular.forEach($scope.purchaseLCChargesList, function (a) {
                if (!baseService.valueCheckInList($scope.purchaseLCChargesList, 'Id', a.OverHeadTypeGLId))
                    $scope.purchaseLCChargesList.splice(a, 1);
            });
        $scope.CloseLCPopUp();
    };

    $scope.SaveCharges = function () {
        try {
            if (baseService.arrayLength($scope.purchaseLCChargesList) < 0) {
                throw "Add LCCharges.";
            }
            for (var i = 0; i < $scope.purchaseLCChargesList.length; i++) {
                if ($scope.purchaseLCChargesList[i].ChargesValue < 0 || $scope.purchaseLCChargesList[i].ChargesValue === 0) {
                    throw "Charges Value must greater than 0.";
                }
                if ($scope.purchaseLCChargesList[i].BankAmount < 0 || $scope.purchaseLCChargesList[i].BankAmount === 0) {
                    throw "Bank Amount must greater than 0.";
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveChargesUrl,
                data: {
                    'Charges': $scope.purchaseLCChargesList,
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getPurchaseLCChargesData($scope.purchaseLCNew.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function getPurchaseLCChargesData(purchaseLCId) {
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLC/getPurchaseLCChargesData?purchaseLCId=' + purchaseLCId
        }).then(function successCallback(response) {
            $scope.purchaseLCChargesList = response.data;
            //if (baseService.arrayLength($scope.purchaseLCChargesList) > 0) {
            //    for (var i = 0; i < $scope.purchaseLCChargesList.length; i++) {
            //        if ($scope.purchaseLCChargesList[i].CurrencyId !== $scope.purchaseLCNew.CurrencyId) {
            //            $scope.purchaseLCChargesList[i].BankAmountFlag = false;
            //        } else {
            //            $scope.purchaseLCChargesList[i].BankAmountFlag = true;
            //        }
            //    }
            //}
        });
    }

    function checkLCExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OverHeadTypeGLId === Id && list[i].VoucherId == null) {
                return true;
            }
        }
        return false;
    }

    $scope.removeRowModal = function (index, data) {
        try {
            if ($scope.purchaseLCNew.Back === 1) {
                throw "Data delete is not possible when it has another Version.";
            }
            $scope.LCChargesId = data.Id;
            $scope.bActivityIndex = index;
            if (baseService.isUndefinedOrNull($scope.LCChargesId))
                $scope.message = 'Are you sure want to delete this data....';
            else
                $scope.message = 'Are you sure want to delete permanently [ ' + data.OverHeadType + ' ]';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteCharges = function () {
        if (baseService.isUndefinedOrNull($scope.LCChargesId)) {
            $scope.purchaseLCChargesList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Commercial/PurchaseLC/DeleteCharges?id=' + $scope.LCChargesId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.purchaseLCChargesList.splice($scope.bActivityIndex, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion Charges

    // #region Tax



    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null,
            SpecialTaxId: null
        };
        $scope.taxList.push(data);

    };
    $scope.TaxAction = 'Save';

    function getTaxData() {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Commercial/PurchaseLC/GetPurchaseLCChargesTax?purchaseLCChargesId=' + $scope.ChargesId
        }).then(function successCall(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.taxList = response.data;
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.HSNCode = response.data[0]['HSNCode'];
                }
                $scope.TaxAction = 'Update';
            }
        })
    }

    $scope.changeAcceptanceCharges = function (data) {
        $scope.ChargesId = data.Id;
        $scope.chargestaxAbleAmnt = data.BankAmount;
        $scope.OpeningBankMasterId = data.OpeningBankMasterId;

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Commercial/PurchaseLC/GetPurchaseLCChargesTax?purchaseLCChargesId=' + $scope.ChargesId,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.taxList = response.data;
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.HSNCode = response.data[0]['HSNCode'];
                }
                $scope.TaxAction = 'Update';
            }
            else {
                if (baseService.isUndefinedOrNull(data.Id))
                    return getTaxCategoryList(hsnCodeId);
                var hsnCodeId = $.grep($scope.purchaseLCChargesList, function (item) { return item.Id === data.Id; })[0].HSNCodeId;
                getAcceptanceChargesTaxCategoryList(hsnCodeId);
            }
        });

        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };

    $scope.AccChargetaxCategoryList = [];
    function getAcceptanceChargesTaxCategoryList(hsnCodeId) {
        $scope.AccChargetaxCategoryList = [];
        $http({
            method: 'GET'
            , url: 'Commercial/PurchaseLC/GetTaxCategoryListByBankMaster?bankMasterId=' + $scope.OpeningBankMasterId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxList = response.data;
            $scope.TaxAction = 'Save';
        });
    }

    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;
        var getRow = $filter("filter")($scope.taxList, { "TaxCategoryId": id });
        if (getRow.length == 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'taxPopup');
        }
    };

    $scope.closeAccChargesTaxPopUp = function () {
        $scope.acceptanceChargesCheckedList[$scope.ChargesTaxIdex].TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.newChargesTaxList), 'TaxAmount');
        angular.element(document.querySelector('#AccepchargesTaxPopUp')).modal('hide');
    }

    $scope.calculateTaxAmount = function (data) {
        data.TaxAmount = Math.round($scope.chargestaxAbleAmnt * data.Percentage) / 100;
    };

    $scope.closeAccepServiceChargePopUp = function () {

        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };

    $scope.SaveChargesTax = function () {
        try {
            for (var i = 0; i < $scope.taxList.length; i++) {
                $scope.taxList[i].PurchaseLCId = $scope.purchaseLCNew.Id;
                $scope.taxList[i].PurchaseLCChargesId = $scope.ChargesId;
                if ($scope.taxList[i].TaxAmount <= 0) {
                    throw "Tax amount should greater than 0.";
                }
            }


            $http({
                method: 'POST',
                url: 'Commercial/PurchaseLC/CreateTax',
                data: {
                    'entities': $scope.taxList
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getTaxData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.dindex = -1;
    $scope.removeTax = function (id, index) {
        $scope.tempId = id;
        $scope.delindex = index;
        if (baseService.isUndefinedOrNull($scope.tempId))
            $scope.message = 'Are you sure want to delete?';
        else
            $scope.message = 'Are you sure want to delete?';
        angular.element(document.querySelector('#removPopUp')).modal('show');
    };

    $scope.removeTaxRow = function () {
        $scope.Del($scope.tempId, $scope.delindex);
        angular.element(document.querySelector('#removPopUp')).modal('hide');
    };


    $scope.Del = function (id, delindex) {
        $scope.dindex = delindex;
        for (var i = 0; i < $scope.taxList.length; i++) {
            if ($scope.taxList[i].Id === id) {
                $scope.taxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };
    function refresh() {
        var gridObj = $("#Grid45").data("ejGrid");
        gridObj.dataSource($scope.selectedPOList);
    }
    $scope.CalculateAmount = function (data) {
        data.data.BalanceAmount = Math.round(( data.data.TransactionAmount - data.data.LCAmount - data.data.Amount )* 100 + Number.EPSILON) / 100
        for (var i = 0; i < $scope.selectedPOList.length; i++) {
            if ($scope.selectedPOList[i].Id == data.data.Id) {
                $scope.selectedPOList[i].BalanceAmount = Math.round((data.data.TransactionAmount - data.data.LCAmount - data.data.Amount) * 100 + Number.EPSILON) / 100;
                refresh();
            }
        }
        if (data.data.BalanceAmount<0) {
            ShowResult('Amount can not greater than Balance Amount !!')
            $scope.purchaseLCNew.Amount = data.data.BalanceAmount;
        }
        else {
            $scope.purchaseLCNew.Amount = Math.round(($filter('sumByKey')($filter('filter')($scope.selectedPOList), 'Amount') * 100 + Number.EPSILON) / 100);
        }
    }
    // #endregion
}
