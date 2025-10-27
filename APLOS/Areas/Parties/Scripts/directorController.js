'use strict';
directorController.$inject = ["addressService", 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'toaster'];
function directorController(addressService, commonMessage, $scope, $rootScope, baseService, $http, $filter, toaster) {
    $rootScope.title = 'Director';
    $scope.Action = 'Save';
    $scope.ContactAction = 'Add Row';
    $scope.maxRow = 2;
    $scope.index = -1;
    $scope.indexContact = -1;
    $scope.parties = [];
    $scope.path = 'Parties/party/';
    $scope.getListUrl = $scope.path + 'getdirectorlist';
    $scope.getPartyContactListUrl = 'addresses/contactmasterparty/getlistbyparty/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'insertdirector';
    $scope.updateUrl = $scope.path + 'editdirector';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', null);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.parties = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        }
    ];

    $scope.party = {
        Id: null,
        CompanyGroupId: null,
        PartyGroupId: null,
        AddressMasterId: null,
        GLIdDr: null,
        GLIdCr: null,
        DrBudgetId: null,
        DrActivityId: null,
        CrBudgetId: null,
        CrActivityId: null,
        VendorAccountGroup: null,
        CustomerAccountGroup: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        RefNo: null,
        TransactionType: null,
        ExciseRange: null,
        CommissionRate: 0,
        COACodeOldDr: null,
        COACodeOldCr: null,
        IECNo: null,
        PINCode: null,
        CSTNO: null,
        TINNO: null,
        BINNO: null,
        LSTNO: null,
        VATResistrationNo: null,
        TradeLicenseNo: null,
        CertificationOfIncorporationNo: null,
        DebitLimit: 0,
        CreditLimit: 0,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null,
        selectedAcc: null,
        GLItemDr: null,
        selectedAccCr: null,
        GLItemCr: null
    };

    $scope.contactMaster = {
        Id: null,
        ContactPerson: null,
        ContactPersonDesignation: null,
        Phone1: null,
        Phone2: null,
        Phone3: null,
        Fax: null,
        Email1: null,
        Email2: null,
        Email3: null,
        Website: null,
        Category: null,
        SubCategory: null,
        Type: null,
        ResponsiblePerson: null,
        Active: true,
        Archive: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.addressMaster = {
        Id: null,
        ContinentId: null,
        CountryId: null,
        StateId: null,
        CityId: null,
        AreaId: null,
        Thana: null,
        Circle: null,
        Ward: null,
        Village: null,
        Address1: null,
        Address2: null,
        Address3: null,
        Postcode: null,
        Phone: null,
        Email: null,
        Website: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    // #region getPartyContact
    $scope.getPartyContact = function () {
        $scope.parameters = {
            limit: 20,
            offset: 0,
            order: 'asc',
            sort: 'Type',
            searchBy: "PartyId",
            search: $scope.party.Id
        };
        baseService.paginationBase($scope.getPartyContactListUrl, 1, $scope.parameters)
            .then(function (result) {
                $scope.contactMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    // #endregion

    // #region AllDropDown
    $scope.ContinentList = [];
    $scope.CountryList = [];
    $scope.StateList = [];
    $scope.AreaList = [];
    $scope.CityList = [];
    $scope.partyGroupList = [];

    // #region GetSequence
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.party.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    // #endregion

    addressService.getContinentCbo(function (result) {
        $scope.ContinentList = result;
    });

    $scope.onContinentChange = function (continentId) {
        addressService.getCountryByContinentCbo(continentId, function (result) {
            $scope.CountryList = result;
        });
    };

    $scope.onCountryChange = function (countryId) {
        addressService.getCboStateByCountry(countryId, function (result) {
            $scope.StateList = result;
        });
    };

    $scope.onStateChange = function (countryId) {
        addressService.getCboCityByCountry(countryId, function (result) {
            $scope.CityList = result;
        });
    };

    $scope.onCityChange = function (cityId) {
        addressService.getCboAreaByCity(cityId, function (result) {
            $scope.AreaList = result;
        });
    };

    $http({
        method: 'GET',
        url: 'Parties/partygroup/getcbolist'
    }).then(function (response) {
        $scope.partyGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: 'accounts/paymentterm/getvendorcbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });

    $scope.customerpaymentTermList = [];
    $http({
        method: 'GET',
        url: 'accounts/paymentterm/getcustomercbo'
    }).then(function successCallback(response) {
        $scope.customerpaymentTermList = response.data;
    });

    // #endregion

    // #region Get
    $scope.Get = function (id, index) {
        $scope.Action = 'Update';
        $scope.index = index;
        $scope.getparty = angular.copy($scope.parties[$scope.index]); // for not change in grid
        $scope.party = $scope.getparty;
        $scope.party.AddedDate = $filter('dateFilter')($scope.party.AddedDate);
        $scope.party.UpdatedDate = $filter('dateFilter')($scope.party.UpdatedDate);
        $scope.GetAddressMaster($scope.party.AddressMasterId);
        $scope.getPartyContact();
        $scope.showContactMaster = true;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetAddressMaster = function (id) {
        $http.get('addresses/addressmaster/get/' + id)
            .then(function (response) {
                $scope.addressMaster = response.data;
                $scope.onContinentChange($scope.addressMaster.ContinentId);
                $scope.onCountryChange($scope.addressMaster.CountryId);
                $scope.onStateChange($scope.addressMaster.CountryId);
                $scope.onCityChange($scope.addressMaster.CityId);
            });
    };

    // #endregion

    // #region ContactMaster
    $scope.GetContact = function (id, index) {
        $scope.contactMaster = {
            Id: null,
            ContactPerson: null,
            ContactPersonDesignation: null,
            Phone1: null,
            Phone2: null,
            Phone3: null,
            Fax: null,
            Email1: null,
            Email2: null,
            Email3: null,
            Website: null,
            Category: null,
            SubCategory: null,
            Type: null,
            ResponsiblePerson: null,
            Active: true,
            Archive: false
        };
        $scope.indexContact = index;
        var obj = $scope.contactMasters[$scope.indexContact];
        for (var i in $scope.contactMaster) {
            $scope.contactMaster[i] = obj[i];
        }
        $scope.ContactAction = 'Update Row';
    };

    $scope.contactMasters = [];

    $scope.addRow = function () {
        try {
            if ($scope.party.UserName == null || $scope.party.UserName == '') {
                throw 'Party User Name Can Not Be Blank !!!';
            }
            if ($scope.contactMaster.ContactPerson == null || $scope.contactMaster.ContactPerson == '') {
                throw 'Please Enter Person Name  !!!';
            }
            if ($scope.ContactAction === 'Add Row') {
                if ($scope.contactMaster != {}) {
                    if ($scope.indexContact != -1)
                        $scope.contactMasters[$scope.indexContact] = $scope.contactMaster;
                    else
                        $scope.contactMasters.push($scope.contactMaster);
                    $scope.indexContact = -1;
                    $scope.contactMaster = {};
                }
            }
            else if ($scope.ContactAction == 'Update Row') {
                if ($scope.contactMaster != {}) {
                    if ($scope.indexContact != -1)
                        $scope.contactMasters[$scope.indexContact] = $scope.contactMaster;
                    else
                        $scope.contactMasters.push($scope.contactMaster);
                    $scope.indexContact = -1;
                    $scope.contactMaster = {};
                }
            }
            $scope.showContactMaster = true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.isArchive = function (archive) {
        if (archive) {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.valuePassInDelModal = function (id, ContactPerson) {
        $scope.conid = id;
        $scope.message_confirmation = 'Are you sure to delete [ ' + ContactPerson + ' ]';
        angular.element(document.querySelector('#confirmContactdelete')).modal('show');
    };

    $scope.removeContactMasterRow = function () {
        for (var i = 0; i < $scope.contactMasters.length; i++) {
            if ($scope.conid == $scope.contactMasters[i].Id) {
                $scope.contactMasters[i].Archive = true;
            }
        }
    };

    $scope.ClearContact = function () {
        $scope.indexContact = -1;
        $scope.contactMaster = {};
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
        $scope.ContactAction = 'Add Row';
    };
    // #endregion

    // #region Toaster
    $scope.popCode = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };

    function reDirectToRequiredTab() {
        if ($scope.partyForm2.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.partyForm3.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.partyForm4.$invalid) {
            $scope.setTab(3);
        }
    }
    // #endregion

    // #region Save
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        try {
            if ($scope.partyForm.$valid && $scope.partyForm2.$valid && $scope.partyForm3.$valid && $scope.partyForm4.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'party': $scope.party, 'addressMaster': $scope.addressMaster, 'contactMasters': $scope.contactMasters
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.party = response.data.Party;
                            $scope.parties.push($scope.party);
                            $scope.parties = $filter('orderBy')($scope.parties, 'Sequence');
                            baseService.paginationAdd();
                            $scope.party.Code = response.data.Party.Code;
                            $scope.popCode('success', 'Party Code Successfully Created : ' + $scope.party.Code);
                            ClearFields(response.data.Sequence);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'party': $scope.party, 'addressMaster': $scope.addressMaster, 'contactMasters': $scope.contactMasters
                        },
                        dataType: 'JSON'
                    }).then(function successCallBack(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.parties[$scope.index] = $scope.party;
                                $scope.parties = $filter('orderBy')($scope.parties, 'Sequence');
                                $scope.tempList = [];
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    // #endregion

    // #region Delete
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.party.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.party.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.parties.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };
    // #endregion

    $scope.showContactMaster = true;

    // #region Clear
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.party = {};
        $scope.party.Sequence = seq;
        $scope.party.CommissionRate = 0;
        $scope.party.DebitLimit = 0;
        $scope.party.CreditLimit = 0;
        $scope.addressMaster = {};
        $scope.contactMaster = {};
        $scope.contactMasters = [];
        $scope.party.Active = true;
        $scope.showContactMaster = false;
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
    }
    // #endregion

    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    $scope.DirectorGLList = [];
    $scope.searchDirectorByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.DirectorListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetDirectorGlList = function (data) {
        $scope.PartyId = data.Id;
        $scope.GLUrl1 = 'parties/party/GetDirectorGLList';
        $scope.getDirectorListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.DirectorListParameters)
                .then(function (data) {
                    $scope.DirectorGLList = data.Rows;
                    $scope.DirectorListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#DirectorListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getDirectorListData();
    };
    $scope.closeDirectorListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#DirectorListPopUp')).modal('hide');
        }
    };
    $scope.SelectItemGL = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.GLSelectedData = x;
        $scope.GLSelectedData.PartyId = $scope.PartyId;
        $scope.DirectorGLAction = 'Save';
        $scope.SaveDirectorGL();
    };
    $scope.SaveDirectorGL = function () {
        /*$scope.$broadcast('show-errors-check-validity');*/
        try {
            //if (baseService.isUndefinedOrNull($scope.GLSelectedData.Bank)) {
            //    throw "Bank is required.";
            //}
            //if (baseService.isUndefinedOrNull($scope.GLSelectedData.BankBranch)) {
            //    throw "Bank Branch is required.";
            //}
            //if (baseService.isUndefinedOrNull($scope.GLSelectedData.BankAccountNo)) {
            //    throw "Bank AccountNo is required.";
            //}

            if ($scope.DirectorGLAction == 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Parties/Party/CreateDirectorGL',
                        data: { 'data': $scope.GLSelectedData },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'DirectorListPopUp');

                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'DirectorListPopUp');
                            angular.element(document.querySelector('#DirectorListPopUp')).modal('hide');
                            $scope.getData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BankPopUp');
                    }
                }
                 
        } catch (e) {
            ShowResult(e, 'failure', 'BankPopUp');
        }
    };
}