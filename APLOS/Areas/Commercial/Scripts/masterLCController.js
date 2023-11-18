'use strict';
masterLCController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller', 'addressService'];
function masterLCController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller, addressService) {
    $rootScope.title = "MasterLC";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/contract/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveMasterLCUrl = $scope.path + 'SaveMasterLC';
    $scope.saveAddInfoLCUrl = $scope.path + 'SaveMasterLCAddInfo';
    $scope.deleteUrl = $scope.path + 'DeleteMasterLC/';
    $scope.partyType = "Customer";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $scope.masterLC = {
        Id: null, Version: 0, CustomerId: null, ContractId: null, BenificiaryBankId: null, OpeningBankId: null, OpeningDescription: null, LeinBankId: null, LeinDescription: null, LCRef: null, LCDate: null, ExpiryDate: null, Amount: null, Type: null, Tenure: null, FinalDestinationId: null, PortOfLandingId: null, CurrencyId: null, IsClose: false, Remarks: null
    };
    $scope.masterLCNew = Object.assign({}, $scope.masterLC);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Get = function (obj) {
        $scope.masterLC = obj.data;
        $scope.masterLCNew = Object.assign({}, $scope.masterLC);
        $scope.GetSavedContract($scope.masterLC.Id);
        $scope.GetMasterLCAddInfoData();
        $scope.GetMasterLCTermsAndConditionsList();
        $scope.Action = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }


    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + $window.companyId + '&PlantId=' + $window.plantId;

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
        $scope.masterLC.CustomerId = party.Id;
        $scope.masterLC.PartyCode = party.Code;
        $scope.masterLC.PartyName = party.UserName;

        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
        $scope.partyType = "Customer";
    }

    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.partyType = "Customer";
        $scope.searchParty = '';
    }

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
        $scope.masterLCAddInfoList = [];
        $scope.TermsAndConditionsList = [];
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = [];
            $scope.currencyList = result;
            $scope.masterLCNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
        });
    }

    $scope.AddModel = { Id: null, MasterLCId: null, Sequence: 0, Description: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
    $scope.addInfo = Object.assign({}, $scope.AddModel);

    $scope.GetSequence = function () {
        $scope.getSeqUrl = "Commercial/Contract/GetAutoSequence?masterLcId=" + $scope.masterLCNew.Id
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.addInfo.Sequence = data;
            $scope.AddModel.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.ActionAdd = 'Save';
    $scope.SaveMasterLCAddInfo = function () {
        try {
            $scope.addInfo.MasterLCId = $scope.masterLCNew.Id;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.addInfoForm.$valid) {

                $http({
                    method: 'POST',
                    url: $scope.saveAddInfoLCUrl,
                    data: {
                        'data': $scope.addInfo
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetMasterLCAddInfoData();
                        $scope.ClearAddInfo();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.masterLCAddInfoList = [];
    $scope.GetMasterLCAddInfoData = function () {
        $scope.masterLCAddInfoList = [];
        $http.get("Commercial/Contract/GetMasterLCAddInfoData?masterLcId=" + $scope.masterLCNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterLCAddInfoList = response.data;
                    }
                    $scope.GetSequence();
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.GetAddInfo = function (obj) {
        $scope.addInfo = Object.assign({}, obj.data);
        $scope.ActionAdd = 'Update';
    }

    $scope.ClearAddInfo = function () {
        $scope.AddModel = { Id: null, MasterLCId: null, Sequence: 0, Description: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };
        $scope.addInfo = Object.assign({}, $scope.AddModel);
        $scope.GetSequence();
        $scope.ActionAdd = 'Save';
    }


    $scope.GetMasterLCTermsAndConditionsList = function () {
        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetMasterLCTermsAndConditionsList?masTerLCId=' + $scope.masterLCNew.Id
        }).then(function successCallback(response) {
            $scope.TermsAndConditionsList = response.data;
        });
    }

    $scope.searchdata = [];
    $scope.GetTermsAndConditionsList = function () {
        $scope.searchdata = [];
        $http({
            method: 'GET',
            url: 'Commercial/Contract/GetTermsAndConditionsDataList'
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
        });
    }

    $scope.AddTermsAndConditions = function () {
        $scope.GetTermsAndConditionsList();
        $scope.ShowResultCustom();
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
    $scope.TermsAndConditionsList = [];
    function MakeData() {
        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Flag == true) {
                if (checkExists($scope.TermsAndConditionsList, $scope.searchdata[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.TermsAndConditionsId = $scope.searchdata[i].Id;
                    ob.MasterLCId = $scope.masterLCNew.Id;
                    ob.Sequence = $scope.searchdata[i].Sequence;
                    ob.Code = $scope.searchdata[i].Code;
                    ob.ShortName = $scope.searchdata[i].ShortName;
                    ob.StandardName = $scope.searchdata[i].StandardName;
                    ob.UserName = $scope.searchdata[i].UserName;
                    ob.OriginUserName = $scope.searchdata[i].UserName;
                    ob.Description = $scope.searchdata[i].Description;
                    ob.Remarks = null;

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
                url: 'Commercial/Contract/CreateMasterLCTNC',
                data: {
                    'data': $scope.TermsAndConditionsList
                    , 'masterLCId': $scope.masterLCNew.Id
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

    $scope.SaveRowData = function (obj) {
        try {
            if (obj.data.OriginUserName !== obj.data.UserName && baseService.isUndefinedOrNull(obj.data.Remarks)) {
                throw "Remarks is mandatory.";
            }
            $http({
                method: 'POST',
                url: 'Commercial/Contract/SaveTNCRowData',
                data: { 'data': obj.data },
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


    $scope.message_detailconfirmation = null;
    $scope.removeBoMDetail = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    }

    $scope.DeleteTNC = function () {
        $http({
            method: 'POST',
            url: 'Commercial/Contract/DeleteMasterLCTermsAndConditions?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetMasterLCTermsAndConditionsList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.ModelBank = {
        Id: null,
        BankName: null,
        AccountNo: null,
        BankCategory: null,
        UserName: null,
        SWIFTCode: null,
        CountryId: null,
        Remark: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    }
    $scope.BankModelNew = Object.assign({}, $scope.ModelBank);


    addressService.getCountryCbo(function (result) {
        $scope.companyList = result;
    });

    $scope.EditNB = function (obj) {
        $scope.NBAction = 'Update';
        $scope.BankModelNew = Object.assign({}, obj.data);
    }
    $scope.NBAction = 'Save';

    $scope.ClearNBank = function () {
        $scope.ModelBank = {
            Id: null,
            BankName: null,
            AccountNo: null,
            BankCategory: null,
            UserName: null,
            SWIFTCode: null,
            CountryId: null,
            Remark: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null
        }
        $scope.BankModelNew = Object.assign({}, $scope.ModelBank);
        $scope.NBAction = 'Save';
    }

    $scope.SaveNBank = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.BankModelNew.BankName)) {
                throw "Bank Name is required.";
            }
            if (baseService.isUndefinedOrNull($scope.BankModelNew.AccountNo)) {
                throw "AccountNo is required.";
            }
            if (baseService.isUndefinedOrNull($scope.BankModelNew.UserName)) {
                throw "User Name is required.";
            }
            if (baseService.isUndefinedOrNull($scope.BankModelNew.CountryId)) {
                throw "Country is required.";
            }

            $http({
                method: 'POST',
                url: 'Commercial/Contract/SaveNegotiatingBank',
                data: { 'data': $scope.BankModelNew },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetNegotiatingBankList();
                    $scope.ClearNBank();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.searchByNB = "UserName"; $scope.searchNB = "";

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

    $scope.NegotiatingBankDataList = [];
    $scope.searchByNBList = [{ value: 'BankName', name: "Bank Name" }, { value: 'UserName', name: "User Name" }, { value: 'AccountNo', name: "AccountNo" },{ value: 'Country', name: "Country" }];
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
        $scope.masterLC.OpeningBankId = obj.data.Id;
        $scope.masterLC.OpeningBank = obj.data.BankName;
        angular.element(document.querySelector('#NBPopUp')).modal('hide');
    }

    $scope.CloseNB = function () {
        angular.element(document.querySelector('#NBPopUp')).modal('hide');
    }




}






