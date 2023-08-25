'use strict';
masterLCController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function masterLCController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "MasterLC";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/contract/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveMasterLCUrl = $scope.path + 'SaveMasterLC';
    $scope.deleteUrl = $scope.path + 'DeleteMasterLC/';
    $scope.partyType = "Customer";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $scope.masterLC = {
        Id: null, Version: 0, CustomerId: null, ContractId: null, BenificiaryBankId: null, OpeningBankId: null, OpeningDescription: null, LeinBankId: null, LeinDescription: null, LCRef: null, LCDate: null, ExpiryDate: null, Amount: null, Type: null, Tenure: null, FinalDestinationId: null, PortOfLandingId: null, CurrencyId: null, IsClose: false, Remarks: null
    };
    $scope.masterLCNew = Object.assign({}, $scope.masterLC);

    $scope.Get = function (obj) {
        $scope.masterLC = obj.data;
        $scope.masterLCNew = Object.assign({}, $scope.masterLC);
        $scope.GetSavedContract($scope.masterLC.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.masterLC.CustomerId = party.Id;
            $scope.masterLC.PartyCode = party.Code;
            $scope.masterLC.PartyName = party.UserName;
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
        $http.get("Commercial/Contract/GetContractListByCustomer?customerId=" + $scope.masterLC.CustomerId)
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
                    models: $scope.SaveList, masterLcId: $scope.masterLC.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedContract($scope.masterLC.Id);
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
        if (!baseService.isUndefinedOrNull($scope.masterLCNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.masterLC.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.masterLC.Rate = $scope.currencyExchangeRate.ToCurrencyRate;
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
        $scope.masterLC.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
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

    $scope.SaveMasterLC = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.masterLC.Type === 'Usance') {
                if ($scope.masterLC.Tenure === 0 || $scope.masterLC.Tenure < 0) {
                    throw "Usance value must greater than 0.";
                }
            } else {
                $scope.masterLC.Tenure = 0;
            }
            if ($scope.MasterLCForm.$valid) {

                $http({
                    method: 'POST',
                    url: $scope.saveMasterLCUrl,
                    data: {
                        'entity': $scope.masterLC
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.masterLC.Id = response.data.Id;
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
        if ($scope.masterLCNew.Type === 'AtSight') {
            $scope.masterLCNew.Tenure = 0;
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.masterLCNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.masterLCNew.Id,
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

                $scope.GetSavedContract($scope.masterLC.Id);
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
        $scope.masterLC = {
            Id: null, CustomerId: null, ContractId: null, BenificiaryBankId: null, OpeningBankId: null, OpeningDescription: null, LeinBankId: null, LeinDescription: null, LCRef: null, LCDate: null, ExpiryDate: null, Amount: null, Type: null, Tenure: null, FinalDestinationId: null, PortOfLandingId: null, CurrencyId: null, IsClose: false, Remarks: null
        };
        $scope.masterLCNew = {};
        $scope.savedcontractList = [];
        $scope.contractList = [];
        $scope.Action = 'Save';

        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            $scope.masterLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });
    }

}






