'use strict';
purchaseLCAmendmentController.$inject = ['accountService','commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function purchaseLCAmendmentController(accountService,commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "Amendment";
    $scope.Action = 'Update';
    $scope.path = 'Commercial/PurchaseLCAmendment/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'CreateCharge';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.partyType = "Vendor";
    //  $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.purchaseLC = {
        Id: null,
        ContractId: null,
        VendorId: null,
        BenificiaryBankId: null,
        OpeningBankMasterId: null,
        BenificiaryDescription: null,
        LeinDescription: null,
        LeinBankId: null,
        OrderSpecific: 'Yes',
        LCRef: null,
        LCDate: null,
        ExpiryDate: null,
        Amount: null,
        Type: null,
        Tenure: 0,
        FinalDestination: null,
        PortOfLandingId: null,
        PortOfLoading: null,
        IsClose: false,
        CurrencyId: null,
        Rate: 0,
        Version: 0,
        flag: null,
        AmendmentDate: null,
        BankCurrency: null,
        LCANo: null,
        LIBOUR: null,
        InsuranceCoverNoteNo: null,
        InsuranceValue: 0,
        InsuranceAttachment: null,
        PaymentBasedOn: null,
        Remarks: null,
        IsAccepptanceFirst: 'true',
        AccepptanceFirst: 'true',
        GRNFirst: 'false',
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

    $scope.flag = null;
    $scope.Get = function (obj, flag) {
        $scope.flag = flag;
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            $scope.purchaseLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });

        $scope.purchaseLC = obj.data;
        $scope.purchaseLCNew = Object.assign({}, $scope.purchaseLC);

       // $scope.ChangeBankMaster();
        $scope.GetPurchaseLCChargesDataByVersion();
       
        if ($scope.purchaseLCNew.Version > 1) {
            getVersionCbo($scope.purchaseLCNew.Id);
        }
        $scope.version = $scope.purchaseLCNew.Version;

        if ($scope.purchaseLCNew.IsAccepptanceFirst) {
            $scope.purchaseLCNew.IsAccepptanceFirst = 'true';
        } else {
            $scope.purchaseLCNew.IsAccepptanceFirst = 'false';
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.ChangeBankMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.OpeningBankMasterId)) {
            $scope.purchaseLCNew.BankCurrency = $.grep($scope.bankMasterList, function (item) {
                return item.Id === $scope.purchaseLCNew.OpeningBankMasterId;
            })[0].CurrencyId;
            $scope.purchaseLCNew.CurrencyId = $scope.purchaseLCNew.BankCurrency;
            $scope.GetCurrencyExchangeRateList();
        }
    }

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

    $scope.bankMasterList = [];
    bankService.getBankMasterCboListByPlant(function (result) {
        $scope.bankMasterList = result;
    });

    $scope.currencyList = [];
    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.purchaseLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });

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

    //$scope.destinationList = [];
    //$scope.getDestination = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'OrderManagements/destination/GetCbo/'
    //    }).then(function successCallback(response) {
    //        $scope.destinationList = response.data;
    //    });
    //};
    //$scope.getDestination();

    $scope.purchaseLCList = [];
    $scope.getSavedData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/PurchaseLCAmendment/getlist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.purchaseLCList = response.data;
                     
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();
    $scope.flag = 'Edit';
    $scope.confirmToCreateNewVersion = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.PurchaseLCForm.$valid) {

            if ($scope.flag === 'Amendment') {
                if (!baseService.isUndefinedOrNull($scope.purchaseLCNew.Id)) {
                    $scope.message = "Are you sure to create Amendment?";
                    angular.element(document.querySelector("#confirmSavePopUp")).modal("show");
                }
                else {
                    $scope.Save();
                    angular.element(document.querySelector("#confirmSavePopUp")).modal("hide");
                }
            } else {
                $scope.Save();
            }
        }
    };

    $scope.VersionList = [];
    function getVersionCbo(purchaseLCId) {

        $http({
            method: 'GET',
            url: 'commercial/PurchaseLCAmendment/GetVersionCbo?purchaseLCId=' + purchaseLCId
        }).then(function (response) {
            $scope.VersionList = response.data;
            if (baseService.arrayLength($scope.VersionList) > 0) {
                for (var i = 0; i < $scope.VersionList.length; i++) {
                    $scope.VersionList[i].Text = parseInt($scope.VersionList[i].Text);
                    if ($scope.VersionList[i].Text === $scope.purchaseLCNew.Version) {
                        $scope.purchaseLCNew.Version = $scope.VersionList[i].Text;
                        $scope.version = $scope.purchaseLCNew.Version;
                        $scope.purchaseLCNew.Id = $scope.VersionList[i].Value;
                    }
                }
            }
        });
    }

    $scope.GetListByVersion = function () {

        $http({
            method: 'GET',
            url: 'commercial/PurchaseLCAmendment/GetListByVersion?purchaseLCId=' + $scope.purchaseLCNew.Id + '&Version=' + $scope.purchaseLCNew.Version
        }).then(function (response) {
            $scope.purchaseLCNew = response.data[0];

        });
    };

    $scope.voucherId = null;

    $scope.GetPurchaseLCChargesDataByVersion = function () {

        $scope.purchaseLCChargesList = [];
        $http({
            method: 'GET',
            url: 'commercial/PurchaseLCAmendment/GetPurchaseLCChargesDataByVersion?purchaseLCId=' + $scope.purchaseLCNew.Id + '&Version=' + $scope.purchaseLCNew.Version
        }).then(function (response) {
            $scope.purchaseLCChargesList = response.data;

            if (baseService.arrayLength($scope.purchaseLCChargesList) > 0) {
                $scope.voucherId = $scope.purchaseLCChargesList[0].VoucherId;
            }
        });
    };

    $scope.ValidateDate = function () {
        var fd = $filter('dateFiltering')($scope.purchaseLCNew.LCDate, 'dd-MM-yyyy');
        var td = $filter('dateFiltering')($scope.purchaseLCNew.AmendmentDate, 'dd-MM-yyyy');

        if (new Date(fd) > new Date(td)) {
            ShowResult('Amendment Date cann\'t be less than LC Opening Date.','failure');
        }
    };

    function Validation() {
        if ($scope.purchaseLCNew.Flag === 0) {
            throw "Previous Version data update is not possible.";
        }
        if ($scope.purchaseLCNew.Status ==='Close') {
            throw "This LC is closed.";
        }

        if ($scope.purchaseLCNew.OrderSpecific === 'Yes') {
            if (baseService.isUndefinedOrNull($scope.purchaseLCNew.ContractId)) {
                throw "Contract is required.";
            }
        }
        if (baseService.isUndefinedOrNull($scope.purchaseLCNew.VendorId)) {
            throw "Vendor is required.";
        }
        if ($scope.purchaseLCNew.Type === 'Usance') {
            if ($scope.purchaseLCNew.Tenure === 0 || $scope.purchaseLCNew.Tenure < 0) {
                throw "Usance value must greater than 0.";
            }
        } else {
            $scope.purchaseLCNew.Tenure = 0;
        }

        var fd = $filter('dateFiltering')($scope.purchaseLCNew.LCDate, 'dd-MM-yyyy');
        var td = $filter('dateFiltering')($scope.purchaseLCNew.AmendmentDate, 'dd-MM-yyyy');

        if (new Date(fd) > new Date(td)) {
            throw 'Amendment Date cann\'t be less than LC Opening Date.';
        }

    }

    $scope.paymentBasedOnList = [];
    cboService.getEnumCbo("enum/GetPaymentBasedOnEnumCbo", function (result) {
        $scope.paymentBasedOnList = result;
    });

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
    };


    $scope.Save = function () {
        try {
            Validation();

            if ($scope.flag === 'Amendment') {
                if (baseService.isUndefinedOrNull($scope.purchaseLCNew.AmendmentDate)) {
                    throw "Amendment Date is reqiured.";
                }
                if ($scope.purchaseLCChargesList.length === 0) {
                    throw "Add Amendment Charges.";
                }
                $scope.purchaseLCNew.flag = $scope.flag;
            }
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

            //if ($scope.Action === 'Save' || $scope.Action === 'Update') {
            //    $http({
            //        method: 'POST',
            //        url: $scope.saveUrl,
            //        data: {
            //            'model': $scope.purchaseLCNew,
            //            'Charges': $scope.purchaseLCChargesList,
            //            'flg': $scope.flg
            //        },
            //        dataType: 'JSON'
            //        , contentType: "application/json charset=utf-8"
            //    }).then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            ShowResult(response.data.Message, 'failure');
            //        }
            //        else {
            //            ShowResult(response.data.Message, 'success');
            //            $scope.purchaseLCNew.Id = response.data.Id;
            //            $scope.purchaseLCNew.Version = parseInt(response.data.Version);
            //            $scope.getSavedData();
            //            //getPurchaseLCChargesData($scope.purchaseLCNew.Id);
            //            $scope.Action = 'Update';
            //            getVersionCbo($scope.purchaseLCNew.Id);
            //            $scope.GetPurchaseLCChargesDataByVersion();
            //            angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
            //        }
            //    }), function errorCallBack(response) {
            //        ShowResult(response.data.Message, 'failure');
            //    };
            //}


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
                        return formData;
                    },
                    data: { 'model': $scope.purchaseLCNew, 'file': $scope.filedata, 'Charges': $scope.purchaseLCChargesList }


                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.purchaseLCNew.Id = response.data.Id;
                        $scope.purchaseLCNew.Version = parseInt(response.data.Version);
                        $scope.getSavedData();
                        $scope.Action = 'Update';
                        getVersionCbo($scope.purchaseLCNew.Id);
                        $scope.GetPurchaseLCChargesDataByVersion();
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

    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields() {
        $scope.purchaseLC = {};
        $scope.purchaseLCNew = { OrderSpecific: 'Yes', Id: null, Tenure: 0, Version: 0, IsAccepptanceFirst: 'true' ,Status: 'Active'};
        $scope.purchaseLCChargesList = [];
        $scope.VersionList = [];
        $scope.Action = 'Update';
        $scope.flag = 'Edit';
        $scope.version = 0;
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            $scope.purchaseLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });
    }

    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        $scope.masterOrderCustomerList = [];
        $http({
            method: 'GET',
            url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.purchaseLCNew.ContractId
        }).then(function (response) {
            $scope.masterOrderCustomerList = response.data;
        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    }

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalQty", dataMember: "TotalQty", format: "{0:N0}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amt", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
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

    // #region Charges

    $scope.LCChargesList = [];
    $scope.GetPurchaseLCCharges = function () {
        $scope.LCChargesList = [];
        $http.get("Commercial/PurchaseLCAmendment/GetAmendmentLCChargesGLData")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.LCChargesList = response.data;
                        if (baseService.arrayLength($scope.purchaseLCChargesList) > 0) {
                            for (var i = 0; i < $scope.purchaseLCChargesList.length; i++) {
                                for (var j = 0; j < $scope.LCChargesList.length; j++) {
                                    if ($scope.LCChargesList[j].Id === $scope.purchaseLCChargesList[i].OverHeadTypeGLId) {
                                        $scope.LCChargesList[j].Active = true;
                                    }
                                }
                            }
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#LCChargesPopUp')).modal('show');
    };

    $scope.CloseLCPopUp = function () {
        angular.element(document.querySelector('#LCChargesPopUp')).modal('hide');
    }

    $scope.purchaseLCChargesList = [];
    $scope.SelectedLC = function () {
        if (baseService.arrayLength($scope.LCChargesList) > 0) {
            angular.forEach($scope.LCChargesList, function (a) {
                if (checkLCExist($scope.purchaseLCChargesList, a.Id) === false) {
                    if (a.Active) {
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
                            , ChargesValue: null
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
                    throw "ChargesValue must greater than 0.";
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
                    //getPurchaseLCChargesData($scope.purchaseLCNew.Id);
                    $scope.GetPurchaseLCChargesDataByVersion();
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
            url: 'Commercial/PurchaseLCAmendment/getPurchaseLCChargesData?purchaseLCId=' + purchaseLCId
        }).then(function successCallback(response) {
            $scope.purchaseLCChargesList = response.data;
        });
    }

    function checkLCExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OverHeadTypeGLId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.removeRowModal = function (index, data) {
        $scope.LCChargesId = data.Id;
        $scope.bActivityIndex = index;
        if (baseService.isUndefinedOrNull($scope.LCChargesId))
            $scope.message = 'Are you sure want to delete this data....';
        else
            $scope.message = 'Are you sure want to delete permanently [ ' + data.OverHeadType + ' ]';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
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

    accountService.getTaxCategoryCbo(" ", function (result) {
        $scope.taxCategoryList = result;
    });

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


    // #endregion
}