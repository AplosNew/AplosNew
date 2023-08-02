'use strict';
masterLCAmendmentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function masterLCAmendmentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "master LC Amendment";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/contract/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveMasterLCAmendmentUrl = $scope.path + 'SaveMasterLCAmendment';
    $scope.updateMasterLCAmendmentUrl = $scope.path + 'UpdateMasterLCAmendment';
    $scope.partyType = "Customer";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $scope.masterLCAmendment = {
        Id: null, Version: 0, CustomerId: null, ContractId: null, BenificiaryBankId: null, OpeningBankId: null, OpeningDescription: null, LeinBankId: null,
        LeinDescription: null, LCRef: null, LCDate: null, ExpiryDate: null, Amount: null, Type: null, Tenure: null, FinalDestinationId: null,
        PortOfLandingId: null, CurrencyId: null, IsClose: false, AmendmentDate: null
    };
    $scope.masterLCAmendmentNew = Object.assign({}, $scope.masterLCAmendment);

    $scope.flag = null;
    $scope.Get = function (obj, flag) {
        // if (obj.data.Version != 1) {
        $scope.flag = flag;
        //cboService.getCboTransactionCurrencyByCompany('', function (result) {
        //    $scope.currencyList = [];
        //    $scope.currencyList = result;
        //    $scope.purchaseLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        //    $scope.companyCurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;

        //});

        $scope.masterLCAmendment = obj.data;
        $scope.masterLCAmendment.LCDate = $filter('dateFiltering')($scope.masterLCAmendment.LCDate, 'dd-M-yyyy');

        if ($scope.flag == 'Update') {
            $scope.masterLCAmendment.AmendmentDate = $filter('dateFiltering')($scope.masterLCAmendment.AmendmentDate, 'dd-M-yyyy');

        } else {
            $scope.masterLCAmendment.AmendmentDate = null;
        }
        $scope.masterLCAmendmentNew = Object.assign({}, $scope.masterLCAmendment);

        //// $scope.ChangeBankMaster();
        //if ($scope.flag == 'Update') {
        //    $scope.GetPurchaseLCChargesDataByVersion();
        //}
        //GetAlldataPOWithLCMap($scope.purchaseLCNew.Id);

        if ($scope.masterLCAmendmentNew.Version > 1) {
            getAmandmentVersionCbo($scope.masterLCAmendmentNew.Id);
        }
        $scope.Version = $scope.masterLCAmendmentNew.Version;

        if ($scope.masterLCAmendmentNew.IsAccepptanceFirst) {
            $scope.masterLCAmendmentNew.IsAccepptanceFirst = 'true';
        } else {
            $scope.masterLCAmendmentNew.IsAccepptanceFirst = 'false';
        }
        $scope.GetSavedContract($scope.masterLCAmendment.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        //}
    }

    $scope.VersionList = [];
    function getAmandmentVersionCbo(MLCAId) {
        $http({
            method: 'GET',
            url: 'commercial/Contract/GetAmandmentVersionCbo?masterLCAId=' + MLCAId
        }).then(function (response) {
            $scope.VersionList = response.data;
            if (baseService.arrayLength($scope.VersionList) > 0) {
                for (var i = 0; i < $scope.VersionList.length; i++) {
                    $scope.VersionList[i].Text = parseInt($scope.VersionList[i].Text);
                    if ($scope.VersionList[i].Text === $scope.masterLCAmendmentNew.Version) {
                        $scope.masterLCAmendmentNew.Version = $scope.VersionList[i].Text;
                        $scope.version = $scope.masterLCAmendmentNew.Version;
                        $scope.masterLCAmendmentNew.Id = $scope.VersionList[i].Value;
                    }
                }
            }
        });
    }

    //$scope.Get = function (obj) {
    //    $scope.masterLCAmendment = obj.data;
    //    $scope.masterLCAmendmentNew = Object.assign({}, $scope.masterLCAmendment);
    //    $scope.GetSavedContract($scope.masterLCAmendment.Id);

    //    if ($scope.masterLCAmendmentNew.Version > 1) {
    //        $scope.version = $scope.masterLCAmendmentNew.PreVersion;
    //        $scope.getBackData();
    //    } else {
    //        $scope.version = $scope.masterLCAmendmentNew.Version;
    //    }

    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //}
    //$scope.Get = function (obj) {
    //    $scope.PurchaseLCUsedInAcceptance = false;
    //    $scope.purchaseLC = obj.data;
    //    $scope.purchaseLC.LCDate = $filter('dateFiltering')($scope.purchaseLC.LCDate, 'dd-M-yyyy');
    //    $scope.purchaseLC.AmendmentDate = $filter('dateFiltering')($scope.purchaseLC.AmendmentDate, 'dd-M-yyyy');
    //    $scope.purchaseLCNew = Object.assign({}, $scope.purchaseLC);
    //    $scope.AmendmentDate = $scope.purchaseLCNew.AmendmentDate;

    //    if ($scope.purchaseLCNew.Version > 1) {
    //        $scope.version = $scope.purchaseLCNew.PreVersion;
    //        $scope.getBackData();
    //    } else {
    //        $scope.version = $scope.purchaseLCNew.Version;
    //    }
    //    getPurchaseLCChargesBackData($scope.purchaseLCNew.Id, $scope.version);

    //    $scope.GetPurchaseLCUsedInAcceptance($scope.purchaseLCNew.Id);
    //    if ($scope.purchaseLCNew.IsAccepptanceFirst) {
    //        $scope.purchaseLCNew.IsAccepptanceFirst = "true";
    //    } else {
    //        $scope.purchaseLCNew.IsAccepptanceFirst = "false";
    //    }

    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //}


    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.masterLCAmendment.CustomerId = party.Id;
            $scope.masterLCAmendment.PartyCode = party.Code;
            $scope.masterLCAmendment.PartyName = party.UserName;
        }
        $scope.hidePartyPopUp();
    };
    $scope.shipmentModeList = [];
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

    $scope.savedcontractList = [];
    $scope.GetSavedContract = function (masterLCId) {
        $scope.savedcontractList = [];
        $http.get("Commercial/Contract/GetSavedContractList?masterLCId=" + masterLCId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.savedcontractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        $scope.contractList = [];
        $http.get("Commercial/Contract/GetContractListByCustomer?customerId=" + $scope.masterLCAmendment.CustomerId)
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

    $scope.SaveList = [];
    $scope.SaveContract = function () {
        $scope.SaveList = [];
        for (var i = 0; i < $scope.contractList.length; i++) {
            if ($scope.contractList[i].Active) {
                $scope.SaveList.push($scope.contractList[i]);
            }
        }

        if (baseService.arrayLength($scope.SaveList) > 0) {
            $http({
                method: 'POST',
                url: 'Commercial/Contract/CreateContractWithMasterLC',
                data: {
                    models: $scope.SaveList, masterLcId: $scope.masterLCAmendment.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedContract($scope.masterLCAmendment.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
            angular.element(document.querySelector('#ContractPopUp')).modal('hide');
        }
        else {
            ShowResult('Select Contract.', 'failure', 'ContractPopUp');
        }
    }

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }
        var filtered = $("#GridContract").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.contractList.length; i++) {
                $scope.contractList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridContract").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.masterLCAmendmentNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.masterLCAmendment.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.masterLCAmendment.Rate = $scope.currencyExchangeRate.ToCurrencyRate;
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
        $scope.masterLCAmendment.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        $scope.GetCurrencyExchangeRateList();
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

    $scope.masterLCList = [];
    $scope.getSavedData = function () {
        $scope.masterLCList = [];
        $http.get("Commercial/Contract/GetMasterLCDataList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();

    //Create Amandment version
    $scope.flag = 'Update';
    $scope.confirmToCreateNewVersion = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MasterLCAmendmentForm.$valid) {
            if ($scope.flag === 'Amendment') {
                if (!baseService.isUndefinedOrNull($scope.masterLCAmendment.Id)) {
                    $scope.message = "Are you sure to create Amendment?";
                    angular.element(document.querySelector("#confirmSavePopUp")).modal("show");
                }
                else {
                    $scope.SaveMasterLCAmendment();
                    angular.element(document.querySelector("#confirmSavePopUp")).modal("hide");
                }
            }
            else {
                $scope.SaveMasterLCAmendment();
            }
        }
    };

    $scope.SaveMasterLCAmendment = function () {
        try {

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.masterLCAmendment.Type === 'Usance') {
                if ($scope.masterLCAmendment.Tenure === 0 || $scope.masterLCAmendment.Tenure < 0) {
                    throw "Usance value must greater than 0.";
                }
            }
            else {
                $scope.masterLCAmendment.Tenure = 0;
            }
            if ($scope.flag === 'Amendment') {
                if (baseService.isUndefinedOrNull($scope.masterLCAmendment.AmendmentDate)) {
                    throw "Amendment Date is reqiured.";
                }
                $scope.masterLCAmendment.flag = $scope.flag;

                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveMasterLCAmendmentUrl,
                        data: { 'entity': $scope.masterLCAmendment },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.masterLCAmendment.Id = response.data.Id;
                            $scope.getSavedData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }

            else {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateMasterLCAmendmentUrl,
                        data: { 'entity': $scope.masterLCAmendment },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.masterLCAmendment.Id = response.data.Id;
                            $scope.getSavedData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
            $scope.Clear();
        }
        catch (e) {
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
        if ($scope.masterLCAmendmentNew.Type === 'AtSight') {
            $scope.masterLCAmendmentNew.Tenure = 0;
        }
    }


    $scope.deleteModal = function (obj) {

        var gridObj = $("#GridCon").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.Id = data.Id;
        $scope.message = "Are you sure to delete permanently?";
        angular.element(document.querySelector("#removerPopUp")).modal("show");
    }

    $scope.RemoveContract = function () {
        $http({
            method: 'POST',
            url: 'Commercial/Contract/RemoveContract?id=' + $scope.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.GetSavedContract($scope.masterLCAmendment.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    $scope.Clear = function () {
        ClearFields();
    }

    function ClearFields() {
        $scope.masterLCAmendment = {
            Id: null, Version: 0, CustomerId: null, ContractId: null, BenificiaryBankId: null, OpeningBankId: null, OpeningDescription: null, LeinBankId: null,
            LeinDescription: null, LCRef: null, LCDate: null, ExpiryDate: null, AmendmentDate: null, Amount: null, Type: null, Tenure: null, FinalDestinationId: null,
            PortOfLandingId: null, CurrencyId: null, IsClose: false
        };
        $scope.masterLCAmendmentNew = {};
        $scope.savedcontractList = [];
        $scope.contractList = [];
        $scope.Action = 'Save';

        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            $scope.masterLCAmendmentNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });
    }

}






