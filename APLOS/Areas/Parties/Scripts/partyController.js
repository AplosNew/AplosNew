'use strict';
PartyController.$inject = ["addressService", 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'toaster', 'cboService'];
function PartyController(addressService, commonMessage, $scope, $rootScope, baseService, $http, $filter, toaster, cboService) {
    $rootScope.title = 'Party';
    $scope.Action = 'Save';
    $scope.ContactAction = 'Add Row';
    $scope.maxRow = 2;
    $scope.index = -1;
    $scope.indexContact = -1;
    $scope.parties = [];
    $scope.bankListT = [];
    $scope.path = 'Parties/party/';
    $scope.getListUrl = $scope.path + 'getpartylist';
    $scope.getPartyContactListUrl = 'addresses/contactmasterparty/getlistbyparty/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveBankUrl = $scope.path + 'CreateBank';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteBankUrl = $scope.path + 'DeleteBank';

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

    $scope.PartyType = 'Vendor';
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
        },
        {
            'name': 'Party Group',
            'value': 'PartyGroup'
        },
        {
            'name': 'TAXID/No',
            'value': 'TINNO'
        }
        ,
        {
            'name': 'VAT Reg. No/PAN',
            'value': 'VATResistrationNo'
        }
    ];

    $scope.party = {
        Id: null,
        CompanyGroupId: null,
        PartyGroupId: null,
        AddressMasterId: null,
        VendorAccountGroupId: null,
        CustomerAccountGroupId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        RefNo: null,
        TransactionType: null,
        ExciseRange: null,
        CommissionRate: 0,
        IECNo: null,
        PINCode: null,
        CSTNO: null,
        TINNO: null,
        BINNO: null,
        LSTNO: null,
        VATResistrationNo: null,
        TradeLicenseNo: null,
        CertificationOfIncorporationNo: null,
        CertificationOfIncorporationDate: null,
        DebitLimit: 0,
        CreditLimit: 0,
        Description: null,
        Remarks: null,
        Active: false,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null,
        PartyGLType: null,
        Accounttype: null,
        PartyCategoryId: null,
        PartySubCategoryId: null,
        Latitude: null,
        Longitude: null,
        PartyNature: null,
        ResponsiblePersonId: null,
        ResponsiblePersonCode: null,
        ResponsiblePerson: null
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

    $scope.partyPartnerFunction = {
        Id: null,
        PartyId: null,
        PartnerDeterminationProcedureFunctionId: null,
        PartnerFunctionId: null,
        VendorId: null,
        CustomerId: null,
        UserName: null,
        IsDefaultValue: false,
        IsDefault: false,
        Active: true,
        PartyType: null
    };



    $scope.partyPlant = {
        Id: null,
        PartyId: null,
        LanguageId: null,
        Code: null,
        Sequence: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        VATResistrationNo: null,
        GSTIN: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.partyPlantNew = Object.assign({}, $scope.partyPlant);

    // #region  getPartyContact
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
    $scope.partyCategoryList = [];
    $scope.partySubCategoryList = [];
    // #endregion

    // #region GetSequence

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.party.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    // #endregion

    // #region CaseCading

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

    $scope.onStateChange = function (stateId) {
        addressService.getCboDistrictByState(stateId, function (result) {
            $scope.districtList = result;
        });
    };

    $scope.onDistictChange = function (districtId) {
        addressService.getCboCityByDistrict(districtId, function (result) {
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
        url: 'Parties/PartyCategory/GetPartyCategoryCbo'
    }).then(function (response) {
        $scope.partyCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Parties/PartySubCategory/GetPartySubCategoryCboList'
    }).then(function (response) {
        $scope.partySubCategoryList = response.data;
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

    cboService.getEnumCbo("enum/GetTaxApplicableCbo", function (result) {
        $scope.taxApplicableList = result;
    });
    // #endregion


    $scope.popUpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmpData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.party.ResponsiblePersonId = arg.data.SystemId;
        $scope.party.ResponsiblePerson = arg.data.EmployeeName;
        $scope.party.ResponsiblePersonCode = arg.data.EmployeeCode;
        $scope.closePopUp();
    }


    $scope.clearEmp = function () {
        $scope.party.ResponsiblePersonId = null;
        $scope.party.ResponsiblePerson = null;
        $scope.party.ResponsiblePersonCode = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }


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
        $scope.getVendorCompanyData($scope.party.Id);
        $scope.showContactMaster = true;
        $scope.getVendorAccountGroupDataPopupList($scope.party.VendorAccountGroupId);
        $scope.getCustomerAccountGroupDataPopupList($scope.party.CustomerAccountGroupId);
        $scope.GetPartyPlantByPartyId($scope.party.Id);
        $scope.GetCompanyPartyGL();
        $scope.GetPartyBank();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetAddressMaster = function (id) {
        $http.get('addresses/addressmaster/get?id=' + id)
            .then(function (response) {
                $scope.addressMaster = response.data;
                $scope.onContinentChange($scope.addressMaster.ContinentId);
                $scope.onCountryChange($scope.addressMaster.CountryId);
                $scope.onStateChange($scope.addressMaster.StateId);
                $scope.onDistictChange($scope.addressMaster.DistrictId);
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
            Archive: false,
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
                if ($scope.contactMaster !== {}) {
                    if ($scope.indexContact !== -1)
                        $scope.contactMasters[$scope.indexContact] = $scope.contactMaster;
                    else
                        $scope.contactMasters.push($scope.contactMaster);
                    $scope.indexContact = -1;
                    $scope.contactMaster = {};
                }
            }
            else if ($scope.ContactAction === 'Update Row') {
                if ($scope.contactMaster !== {}) {
                    if ($scope.indexContact !== -1)
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
            if ($scope.conid === $scope.contactMasters[i].Id) {
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

    $scope.companyPartyList = [];
    $scope.getVendorCompanyData = function (partyId) {
        $http({
            method: 'GET',
            url: 'parties/party/GetCompanyPartyList?partyId=' + partyId
        }).then(function (response) {
            $scope.companyPartyList = response.data;
        });
    };

    $scope.getVendorCompanyDataNew = function () {
        $http({
            method: 'GET',
            url: 'parties/party/GetCompanyPartyNewList'
        }).then(function (response) {
            $scope.companyPartyList = response.data;
        });
    };
    $scope.getVendorCompanyDataNew();
    // #endregion

    cboService.getCboAllCompanyTransactionList(function (result) {
        $scope.tranCurrencyList = result;
    });

    // #region Toaster

    $scope.popCode = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };
    // #endregion

    // #region VendorPartnerFunction

    $scope.partyPartnerFunctionList = [];
    $scope.partyPartnerFunctionListpopup = [];
    $scope.getVendorAccountGroupDataList = function (PartnerDeterminationProcedureId) {
        $http.get('Parties/partyaccountgroup/getaccountgrouptypelist?partnerDetPrcId=' + PartnerDeterminationProcedureId)
            .then(function (response) {
                $scope.partyPartnerFunctionList = response.data;

                for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
                    if (IsDefault($scope.partyPartnerFunctionList[i].IsModifiable, $scope.partyPartnerFunctionList[i].IsDefaultValue, $scope.partyPartnerFunctionList[i].IsMandatory)) {
                        if (baseService.isUndefinedOrNull($scope.party.Code)) {
                            $scope.partyPartnerFunctionList[i].VendorId = "Internal";
                            $scope.partyPartnerFunctionList[i].UserName = $scope.party.UserName;
                        }
                        else {
                            $scope.partyPartnerFunctionList[i].VendorId = $scope.party.Code;
                            $scope.partyPartnerFunctionList[i].UserName = $scope.party.UserName;
                        }
                    }
                }
                var list = angular.copy(response.data);
                $scope.partyPartnerFunctionListpopup = list;
            });
    };

    $scope.getVendorAccountGroupDataPopupList = function (PartnerDeterminationProcedureId) {
        $http.get('Parties/partyaccountgroup/getaccountgrouptypelist?partnerDetPrcId=' + PartnerDeterminationProcedureId)
            .then(function (response) {
                $scope.partyPartnerFunctionListpopup = [];
                $scope.partyPartnerFunctionListpopup = response.data;
            });
    };

    $scope.searchpartnerFunctionList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'PF Name',
            'value': 'PFName'
        },
        {
            'name': 'Account Type',
            'value': 'AccountType'
        }];
    $scope.partnerfunctionparameters = {
        limit: 20,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        search: ''
    };

    $scope.addVendorRow = function () {
        for (var i = 0; i < $scope.partyPartnerFunctionListpopup.length; i++) {
            $scope.partyPartnerFunctionListpopup[i].Flag = false;
        }
        angular.element(document.querySelector('#PartnerFunctionPopUp')).modal('show');
    };

    function GetName() {
        var listName = [];
        for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
            if ($scope.partyPartnerFunctionList[i].Archive == false) {
                var pFName = $scope.partyPartnerFunctionList[i].PFName;
                if (listName.indexOf(pFName) === -1) {
                    listName.push(pFName);
                }
            }
        }
        return listName;
    }

    function ValidationDuplicate() {
        var nameList = GetName();
        for (var i = 0; i < nameList.length; i++) {
            CountValue(nameList[i], $scope.partyPartnerFunctionList);
        }
    }

    function CountValue(value, list) {
        var Count = 0;
        var vendorId = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Archive == false) {
                if (list[i].PFName == value) {
                    if (Count == 0) {
                        vendorId.push(list[i].VendorId);
                        Count++;
                    }
                    else {
                        if (vendorId.indexOf(list[i].VendorId) != -1) {
                            throw "PFName: [" + list[i].PFName + "] and Vendor: [" + list[i].UserName + "] already exists!!!";
                        }
                        else {
                            vendorId.push(list[i].VendorId);
                        }
                    }
                }//value
            }//Archive
        }//for
    }

    function createguid(prefix) {
        var v = new Date().getTime();
        v += (parseInt(Math.random() * 100)).toString();
        if (undefined === prefix) {
            prefix = 'uid-';
        }
        v = prefix + v;
        return v;
    }

    $scope.IsDisabled = function (a) {
        if (a.IsModifiable) {
            return false;
        }
        else {
            if (a.IsDefaultValue == false) {
                if (a.IsMandatory) {
                    return false;
                }
                else {
                    return true;
                }
            }
            else {
                return true;//IsModifiable IsDefaultValue
            }
        }
    };

    function IsDefault(IsModifiable, IsDefaultValue, IsMandatory) {
        if (IsModifiable == false) {
            if (IsMandatory) {
                if (IsDefaultValue == true) {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                if (IsDefaultValue == true) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        else {
            if (IsDefaultValue == true) {
                return true;
            }
            else {
                return false;
            }
        }
    }

    function setVendorId(code) {
        if (baseService.isUndefinedOrNull(code)) {
            return "Internal";
        }
        else {
            return code;
        }
    }

    $scope.closePartnerFunctionPopUp = function () {
        for (var i = 0; i < $scope.partyPartnerFunctionListpopup.length; i++) {
            var a = $scope.partyPartnerFunctionListpopup[i];
            if (a.Flag) {
                $scope.partyPartnerFunctionList.push({
                    Id: createguid("v"),
                    PartyId: $scope.party.Id,
                    PDPFId: a.PDPFId,
                    PFName: a.PFName,
                    PartnerFunctionId: a.PartnerFunctionId,
                    VendorId: (IsDefault(a.IsModifiable, a.IsDefaultValue, a.IsMandatory) == true ? setVendorId($scope.party.Code) : a.VendorId),
                    CustomerId: null,
                    UserName: (IsDefault(a.IsModifiable, a.IsDefaultValue, a.IsMandatory) == true ? $scope.party.UserName : a.UserName),
                    AccountType: a.AccountType,
                    IsDefaultValue: a.IsDefaultValue,
                    IsMandatory: a.IsMandatory,
                    IsModifiable: a.IsModifiable,
                    IsDefault: false,
                    Active: true,
                    PartyType: 'Vendor'
                });
            }
        }

        angular.element(document.querySelector('#PartnerFunctionPopUp')).modal('hide');
    };

    function Validation() {
        //#region validation for vendorAccountGroup
        for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
            if ($scope.partyPartnerFunctionList[i].Archive == false) {
                if ($scope.partyPartnerFunctionList[i].IsModifiable == false) {
                    if ($scope.partyPartnerFunctionList[i].VendorId == null || $scope.partyPartnerFunctionList[i].VendorId == "") {
                        if ($scope.partyPartnerFunctionList[i].IsDefaultValue) {
                            if (baseService.isUndefinedOrNull($scope.party.Id)) {
                                throw "Vendor Is Mandatory!!!";
                            }
                        }
                        else {
                            if ($scope.partyPartnerFunctionList[i].IsMandatory) {
                                throw "Vendor Is Mandatory!!!";
                            }
                        }
                    }
                }
                else {
                    if ($scope.partyPartnerFunctionList[i].IsMandatory || $scope.partyPartnerFunctionList[i].IsModifiable) {
                        if ($scope.partyPartnerFunctionList[i].VendorId == null || $scope.partyPartnerFunctionList[i].VendorId == "") {
                            throw "Vendor Is Mandatory!!!";
                        }
                    }
                }
            }
            else {
                //
            }
        }
        //#endregion
        angular.forEach($scope.vendors, function (item) {
            if (item.Active && baseService.isUndefinedOrNull($scope.party.VendorAccountGroupId)) {
                throw "Vendor Account Group Is Mandatory!!!";
            }
        });
        angular.forEach($scope.customers, function (item) {
            if (item.Active && baseService.isUndefinedOrNull($scope.party.CustomerAccountGroupId)) {
                throw "Customer Account Group Is Mandatory!!!";
            }
        });
    }

    $scope.valuePassInVendorAccountModal = function (Id) {
        $scope.vendorAccountid = Id;
        $scope.message_confirmation = 'Are you sure to delete [ ' + Id + ' ]';
        angular.element(document.querySelector('#confirmVendorAccountdelete')).modal('show');
    };
    $scope.removeVendorAccountRow = function () {
        for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
            if ($scope.vendorAccountid == $scope.partyPartnerFunctionList[i].Id && $scope.partyPartnerFunctionList[i].Archive == false) {
                $scope.partyPartnerFunctionList[i].Archive = true;
                break;
            }
        }
    };
    // #endregion

    // #region CustomerPartnerFunction

    $scope.IsDisabledCust = function (b) {
        if (b.IsModifiable) {
            return false;
        }
        else {
            if (b.IsDefaultValue === false) {
                if (b.IsMandatory) {
                    return false;
                }
                else {
                    return true;
                }
            }
            else {
                return true;//IsModifiable IsDefaultValue
            }
        }
    };

    $scope.getCustomerAccountGroupDataList = function (PartnerDeterminationProcedureId) {
        $http.get('Parties/partyaccountgroup/getcustomeraccountgrouptypelist?partnerDetPrcId=' + PartnerDeterminationProcedureId)
            .then(function (response) {
                $scope.partyPartnerFunctionListpopup = [];
                $scope.partyPartnerFunctionListpopup = response.data;
                for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
                    if (IsDefault($scope.partyPartnerFunctionList[i].IsModifiable, $scope.partyPartnerFunctionList[i].IsDefaultValue, $scope.partyPartnerFunctionList[i].IsMandatory)) {
                        if (baseService.isUndefinedOrNull($scope.party.Code)) {
                            $scope.partyPartnerFunctionList[i].CustomerId = "Internal";
                            $scope.partyPartnerFunctionList[i].UserName = $scope.party.UserName;
                        }
                        else {
                            $scope.partyPartnerFunctionList[i].CustomerId = $scope.party.Code;
                            $scope.partyPartnerFunctionList[i].UserName = $scope.party.UserName;
                        }
                    }
                }
                var list = angular.copy(response.data);
                $scope.partyPartnerFunctionListpopup = list;
            });
    };

    $scope.customerAccountGroupDataListpopup = [];
    $scope.getCustomerAccountGroupDataPopupList = function (PartnerDeterminationProcedureId) {
        $http.get('Parties/partyaccountgroup/getcustomeraccountgrouptypelist?partnerDetPrcId=' + PartnerDeterminationProcedureId)
            .then(function (response) {
                $scope.partyPartnerFunctionListpopup = [];
                $scope.partyPartnerFunctionListListpopup = response.data;
            });
    };

    $scope.searchcustomerpartnerFunctionList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'PF Name',
            'value': 'PFName'
        },
        {
            'name': 'Account Type',
            'value': 'AccountType'
        }];
    $scope.customerpartnerfunctionparameters = {
        limit: 20,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        search: ''
    };
    $scope.addCustomerRow = function () {
        for (var i = 0; i < $scope.partyPartnerFunctionListpopup.length; i++) {
            $scope.partyPartnerFunctionListpopup[i].Flag = false;
        }
        angular.element(document.querySelector('#CustomerPartnerFunctionPopUp')).modal('show');
    };

    function GetCustName() {
        var CustlistName = [];
        for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
            if ($scope.partyPartnerFunctionList[i].Archive == false) {
                var pFName = $scope.partyPartnerFunctionList[i].PFName;
                if (CustlistName.indexOf(pFName) === -1) {
                    CustlistName.push(pFName);
                }
            }
        }
        return CustlistName;
    }

    function ValidationDuplicateCust() {
        var nameList = GetCustName();
        for (var i = 0; i < nameList.length; i++) {
            CountCustValue(nameList[i], $scope.partyPartnerFunctionList);
        }
    }
    function CountCustValue(value, list) {
        var Count = 0;
        var customerId = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Archive == false) {
                if (list[i].PFName == value) {
                    if (Count == 0) {
                        customerId.push(list[i].CustomerId);
                        Count++;
                    }
                    else {
                        if (customerId.indexOf(list[i].CustomerId) != -1) {
                            throw "PFName: [" + list[i].PFName + "] and Customer: [" + list[i].UserName + "] already exists!!!";
                        }
                        else {
                            customerId.push(list[i].VendorId);
                        }
                    }
                }//value
            }//Archive
        }//for
    }

    function setCustomerId(code) {
        if (baseService.isUndefinedOrNull(code)) {
            return "Internal";
        }
        else {
            return code;
        }
    }

    $scope.closeCustomerPartnerFunctionPopUp = function () {
        for (var i = 0; i < $scope.customerAccountGroupDataListpopup.length; i++) {
            var b = $scope.customerAccountGroupDataListpopup[i];

            if (b.Flag) {
                $scope.partyPartnerFunctionList.push({
                    Id: createguid("c"),
                    PartyId: $scope.party.Id,
                    PDPFId: b.PDPFId,
                    PFName: b.PFName,
                    PartnerFunctionId: b.PartnerFunctionId,
                    CustomerId: (IsDefault(b.IsModifiable, b.IsDefaultValue, b.IsModifiable) == true ? setCustomerId($scope.party.Code) : b.CustomerId),
                    UserName: (IsDefault(b.IsModifiable, b.IsDefaultValue, b.IsModifiable) == true ? $scope.party.UserName : b.UserName),
                    AccountType: b.AccountType,
                    IsDefaultValue: b.IsDefaultValue,
                    IsMandatory: b.IsMandatory,
                    IsModifiable: b.IsModifiable,
                    IsDefault: false,
                    Active: true,
                    PartyType: 'Customer'
                });
            }
        }
        angular.element(document.querySelector('#CustomerPartnerFunctionPopUp')).modal('hide');
    };

    function ValidationCust() {
        for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
            if ($scope.partyPartnerFunctionList[i].Archive == false) {
                if ($scope.partyPartnerFunctionList[i].IsModifiable == false) {
                    if ($scope.partyPartnerFunctionList[i].CustomerId == null || $scope.partyPartnerFunctionList[i].CustomerId == "") {
                        if ($scope.partyPartnerFunctionList[i].IsDefaultValue) {
                            if (baseService.isUndefinedOrNull($scope.party.Id)) {
                                throw "Customer Is Mandatory!!!";
                            }
                        }
                        else {
                            if ($scope.partyPartnerFunctionList[i].IsMandatory) {
                                throw "Customer Is Mandatory!!!";
                            }
                        }
                    }
                }
                else {
                    if ($scope.partyPartnerFunctionList[i].IsMandatory || $scope.partyPartnerFunctionList[i].IsModifiable) {
                        if ($scope.partyPartnerFunctionList[i].CustomerId == null || $scope.partyPartnerFunctionList[i].CustomerId == "") {
                            throw "Customer Is Mandatory!!!";
                        }
                    }
                }
            }
            else {
                //
            }
        }
    }

    $scope.valuePassInCustomerAccountModal = function (Id) {
        $scope.customerAccountid = Id;
        $scope.message_confirmation = 'Are you sure to delete [ ' + Id + ' ]';
        angular.element(document.querySelector('#confirmCustomerAccountdelete')).modal('show');
    };
    $scope.removeCustomerAccountRow = function () {
        for (var i = 0; i < $scope.partyPartnerFunctionList.length; i++) {
            if ($scope.customerAccountid == $scope.partyPartnerFunctionList[i].Id && $scope.partyPartnerFunctionList[i].Archive == false) {
                $scope.partyPartnerFunctionList[i].Archive = true;
                break;
            }
        }
    };
    // #endregion

    // #region ReturnToRequiredTab

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
        else if ($scope.partyForm5.$invalid) {
            $scope.setTab(4);
        }
    }
    function reDirectToRequiredPlantTab() {
        if ($scope.partyPlantForm1.$invalid) {
            $scope.setTab2(1);
        }
        else if ($scope.partyPlantForm2.$invalid) {
            $scope.setTab2(2);
        }
    }
    // #endregion

    // #region GLListPopUp

    $scope.glTypeList = [];
    cboService.getEnumCbo("enum/getpartygltypeenumcbo", function (result) {
        $scope.glTypeList = result;
    });

    $scope.searchModalList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget Code',
            'value': 'BudgetCode'
        },
        {
            'name': 'Budget Name',
            'value': 'BudgetName'
        },
        {
            'name': 'Activity Code',
            'value': 'ActivityCode'
        },
        {
            'name': 'Activity Name',
            'value': 'ActivityName'
        }
    ];
    $scope.glparameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName, BudgetName, ActivityName',
        searchBy: "GLGeneralInfoCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.companyPartyGLList = [];
    $scope.currentClickedCompanyPartyId = null;
    $scope.currentClickedPartyAccountGroupId = null;
    $scope.GetCompanyPartyGL = function () {
        $http({
            method: 'GET',
            url: 'parties/party/getcompanypartygllist?partyId=' + $scope.party.Id
        }).then(function (response) {
            $scope.companyPartyGLList = response.data;
        });
    };

    $scope.GetGLList = function (companyId, companyPartyId, partyAccountGroupId) {
        $scope.companyId = companyId;
        $scope.currentClickedCompanyPartyId = companyPartyId;
        $scope.currentClickedPartyAccountGroupId = partyAccountGroupId;
        angular.element(document.querySelector('#GLListPopUp')).modal('show');
    };

    $scope.vendorAccountType = null;
    $scope.customerAccountType = null;
    $scope.partyAccountGroupList = [];
    cboService.partyAccountGroupCbo(function (result) {
        $scope.partyAccountGroupList = result;
    });

    $scope.changePartyAccountGroupCbo = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            return $.grep($scope.partyAccountGroupList, function (item) {
                return item.Value === id;
            })[0].AccountType;
        }
        else return null;
    };

    $scope.companyPartyGL = {
        Id: null,
        PartyId: null,
        CompanyPartyId: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        PartyGLType: null,
        Remarks: null,
        Active: true,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityCode: null,
        ActivityName: null,
        AccountGroupName: null
    };

    $scope.GList = [];
    $scope.GlList = function () {
        try {
            $scope.partyAccountType = $scope.changePartyAccountGroupCbo($scope.currentClickedPartyAccountGroupId);
            $scope.GLUrl = 'accounts/glitem/';
            if ($scope.partyAccountType === 'Vendor') {
                if ($scope.party.PartyGLType === 'ReconciliationGL' || $scope.party.PartyGLType === 'AdditionalGL') {
                    $scope.GLUrl += 'getpartycreditglaccountcode?companyId=' + $scope.companyId;
                }
                else if ($scope.party.PartyGLType === 'DownPaymentGL' || $scope.party.PartyGLType === 'SuspenseGL') {
                    $scope.GLUrl += 'getvendordownpaymentgl?companyId=' + $scope.companyId;
                }
            }
            else if ($scope.partyAccountType === 'Customer') {
                if ($scope.party.PartyGLType === 'ReconciliationGL' || $scope.party.PartyGLType === 'AdditionalGL') {
                    $scope.GLUrl += 'getpartydebitglaccountcode?companyId=' + $scope.companyId;
                }
                else if ($scope.party.PartyGLType === 'DownPaymentGL' || $scope.party.PartyGLType === 'SuspenseGL') {
                    $scope.GLUrl += 'getcustomerdownpaymentgl?companyId=' + $scope.companyId;
                }
            }

            baseService.setCurrentPage('GList');
            $scope.getGLModalData = function (pageno) {
                baseService.paginationBase($scope.GLUrl, pageno, $scope.glparameters)
                    .then(function (data) {
                        $scope.GList = data.Rows;
                        $scope.glparameters.total_count = data.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#GLPopUp')).modal('show');
            $scope.getGLModalData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectGL = function (gl) {
        try {
            var companyPartyGL = $filter('filter')($scope.companyPartyGLList, { CompanyPartyId: $scope.currentClickedCompanyPartyId, PartyGLType: $scope.party.PartyGLType });
            if (!baseService.isUndefinedOrNull(companyPartyGL) && companyPartyGL.length > 0) {
                if (companyPartyGL[0].PartyGLType === 'ReconciliationGL') {
                    throw 'Reconciliation GL is already exists.';
                }
                else if (companyPartyGL[0].PartyGLType === 'DownPaymentGL') {
                    throw 'DownPayment GL is already exists.';
                }
                else if (companyPartyGL[0].PartyGLType === 'SuspenseGL') {
                    throw 'Suspense GL is already exists.';
                }
                else if ($filter('filter')(companyPartyGL, { PartyGLType: 'AdditionalGL', GLGeneralInfoId: gl.GLGeneralInfoId }).length > 0) {
                    throw 'Additional GL is already exists.';
                }
            }
            $scope.companyPartyGL.AccountGroupName = gl.AccountGroupName;
            $scope.companyPartyGL.GLGeneralInfoId = gl.GLGeneralInfoId;
            $scope.companyPartyGL.GLGeneralInfoCode = gl.GLGeneralInfoCode;
            $scope.companyPartyGL.GLGeneralInfoName = gl.GLGeneralInfoName;
            $scope.companyPartyGL.BudgetMasterId = gl.BudgetMasterId;
            $scope.companyPartyGL.BudgetCode = gl.BudgetCode;
            $scope.companyPartyGL.BudgetName = gl.BudgetName;
            $scope.companyPartyGL.ActivityId = gl.ActivityId;
            $scope.companyPartyGL.ActivityCode = gl.ActivityCode;
            $scope.companyPartyGL.ActivityName = gl.ActivityName;
            $scope.companyPartyGL.Active = true;
            $scope.companyPartyGL.PartyId = $scope.party.Id;
            $scope.companyPartyGL.PartyGLType = $scope.party.PartyGLType;
            $scope.companyPartyGL.CompanyPartyId = $scope.currentClickedCompanyPartyId;
            $scope.companyPartyGLList.push($scope.companyPartyGL);
            $scope.companyPartyGL = {};
            angular.element(document.querySelector('#GLPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'GLPopUp');
        }
    };

    $scope.Done = function () {
        angular.element(document.querySelector('#GLListPopUp')).modal('hide');
    };

    $scope.confirmDelete = function (data) {
        $scope.Name = data.GLGeneralInfoName;
        $scope.deleteId = data.Id;
        $scope.message_confirmation = "Are you sure to delete permanently [" + $scope.Name + "]?";
    };

    $scope.DeleteCompanyPartyGL = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            // $scope.companyPartyGLList.splice($scope.index, 1);
            var drc = $scope.companyPartyGLList.length;
            while (drc--) {
                if ($scope.companyPartyGLList[drc]['Id'] === $scope.deleteId) {
                    $scope.companyPartyGLList.splice(drc, 1);
                }
            }
        }
        else {
            $http({
                method: 'POST',
                url: 'parties/party/deletecompanypartygl',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    var drc = $scope.companyPartyGLList.length;
                    while (drc--) {
                        if ($scope.companyPartyGLList[drc]['Id'] === $scope.deleteId) {
                            $scope.companyPartyGLList.splice(drc, 1);
                        }
                    }
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });

            return true;
        }
    };

    $scope.Close = function () {
        angular.element(document.querySelector('#GLPopUp')).modal('hide');
        angular.element(document.querySelector('#BankSearchPopUp')).modal('hide');
    };

    // #endregion

    // #region Save
    $scope.IsCustomerCurrencyChanges = false;
    $scope.IsVendorCurrencyChanges = false;
    $scope.checkCustomerCurrencyChange = function () {
        if ($scope.Action === 'Update') {
            $scope.IsCustomerCurrencyChanges = true;
        }
    }
    $scope.checkVendorCurrencyChange = function () {
        if ($scope.Action === 'Update') {
            $scope.IsVendorCurrencyChanges = true;
        }
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        try {
            for (var i = 0; i < $scope.companyPartyList.length; i++) {
                if ($scope.companyPartyList[i].Active) {
                    if (baseService.isUndefinedOrNull($scope.companyPartyList[i].PartyAccountGroupId)) {
                        throw "Select Account Group.";
                    }
                    if (baseService.isUndefinedOrNull($scope.companyPartyList[i].CurrencyId)) {
                        throw "Select Currency.";
                    }
                    if (baseService.isUndefinedOrNull($scope.companyPartyList[i].PaymentTermId)) {
                        throw "Select PaymentTerm.";
                    }
                }
            }
            Validation();
            ValidationDuplicate();
            ValidationCust();
            ValidationDuplicateCust();
            if ($scope.partyForm1.$valid && $scope.partyForm2.$valid && $scope.partyForm3.$valid && $scope.partyForm4.$valid && $scope.partyForm5.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'party': $scope.party, 'addressMaster': $scope.addressMaster, 'contactMasters': $scope.contactMasters,
                            'companyPartyDataList': $scope.companyPartyList, 'partyPartnerFunction': $scope.partyPartnerFunctionList
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
                            $scope.party.Id = response.data.Party.Id;
                            $scope.popCode('success', 'Party Code Successfully Created : ' + $scope.party.Code);
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'party': $scope.party, 'addressMaster': $scope.addressMaster, 'contactMasters': $scope.contactMasters,
                            'companyPartyDataList': $scope.companyPartyList, 'companyPartyGLDataList': $scope.companyPartyGLList,
                            'partyPartnerFunction': $scope.partyPartnerFunctionList, 'isCustomerCurrencyChanges': $scope.IsCustomerCurrencyChanges, 'isVendorCurrencyChanges': $scope.IsVendorCurrencyChanges
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
                                $scope.party.Id = response.data.Party.Id;
                                $scope.parties = $filter('orderBy')($scope.parties, 'Sequence');
                                $scope.tempList = [];
                                $scope.IsCustomerCurrencyChanges = false;
                                $scope.IsVendorCurrencyChanges = false;
                            }
                            ClearFields(response.data.Sequence);
                            $scope.getData();
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
        $scope.partyPartnerFunctionList = [];
        $scope.customerAccountGroupDataList = [];
        $scope.vendorSaveCheck = {};
        $scope.customerSaveCheck = {};
        $scope.partyPlants = [];
        $scope.companyPartyList = [];
        $scope.getVendorCompanyDataNew();
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

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    // #endregion

    // #region Party Dynamic PopUp For PF

    $scope.part = [];
    $scope.popUpList = [];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: '',
        searchBy: '',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function (name, x, index) {
        $scope.setIndex = index;
        $scope.popUpUrl = '';
        $scope.popUpParameters.sort = '';
        $scope.popUpParameters.searchBy = '';
        if (name === 'vendor') {
            $scope.popUpTitle = 'Select Vendor';
            $scope.popUpUrl = 'Parties/party/getpartycodelist';
            $scope.popUpParameters.sort = 'Code';
            $scope.popUpParameters.searchBy = 'Code';
        }
        if (name === 'customer') {
            $scope.popUpTitle = 'Select Customer';
            $scope.popUpUrl = 'Parties/party/getpartycodelist';
            $scope.popUpParameters.sort = 'Code';
            $scope.popUpParameters.searchBy = 'Code';
        }
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    if (baseService.arrayLength($scope.popUpList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.fieldId = x;

        $scope.fieldName = name;
        angular.element(document.querySelector('#popUp')).modal('show');
        $scope.popUpData();
    };

    $scope.selectdblClick = function (data) {
        setPartyName(data);
        //$scope.party[$scope.fieldId] = data.Code;
        $scope.part[$scope.setIndex] = data;

        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    function setPartyName(ob) {
        if ($scope.fieldName === 'vendor') {
            $scope.vendorAccountGroupDataList[$scope.setIndex].VendorId = ob.Code;
            $scope.vendorAccountGroupDataList[$scope.setIndex].UserName = ob.UserName;
        }
        else if ($scope.fieldName === 'customer') {
            $scope.customerAccountGroupDataList[$scope.setIndex].CustomerId = ob.Code;
            $scope.customerAccountGroupDataList[$scope.setIndex].UserName = ob.UserName;
        }
    }
    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData == '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick(data);
        $scope.valueData = data;
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    // #endregion

    // #region PartyPlant

    $scope.GetPartyPlantSequence = function () {
        $http.get('parties/party/getpartypalntsequence')
            .then(function (response) {
                $scope.partyPlant.Sequence = response.data;
                $scope.partyPlantNew.Sequence = response.data;
            });
    };

    $scope.AddPlant = function () {
        $scope.partyPlant = {};
        $scope.partyPlantNew = {};
        $scope.addressMasterPlant = {};
        $scope.GetPartyPlantSequence();
        $scope.PartyPlantAction = 'Save';
        $scope.partyPlantNew.Active = true;
        angular.element(document.querySelector('#PlantPopUp')).modal('show');
    };

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.GetPartyPlantByPartyId = function (id) {
        $http.get('parties/party/GetPartyPlantByPartyId?partyId=' + id)
            .then(function (response) {
                $scope.partyPlants = response.data;
            });
    };

    $scope.PartyPlantData = function (data, index) {
        $scope.PartyPlantAction = 'Update';
        $scope.partyPlantNew = Object.assign({}, data);
        $scope.GetPartyPlantAddress($scope.partyPlantNew.AddressMasterId);
        angular.element(document.querySelector('#PlantPopUp')).modal('show');
    };

    $scope.GetPartyPlantAddress = function (addressMasterId) {
        $http.get('Addresses/AddressMaster/Get/' + addressMasterId)
            .then(function (response) {
                $scope.addressMasterPlant = response.data;
                $scope.onContinentChange($scope.addressMasterPlant.ContinentId);
                $scope.onCountryChange($scope.addressMasterPlant.CountryId);
                $scope.onStateChange($scope.addressMasterPlant.StateId);
                $scope.onDistictChange($scope.addressMasterPlant.DistrictId);
                $scope.onCityChange($scope.addressMasterPlant.CityId);
                $scope.GetPartyPlantContactData();
            });
    };

    $scope.partyPlants = [];
    $scope.PartyPlantAction = 'Save';

    $scope.SavePartyPlant = function () {
        $scope.partyPlantNew.PartyId = $scope.party.Id;
        angular.copy($scope.partyPlantNew, $scope.partyPlant);
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredPlantTab();
        if ($scope.partyPlantForm1.$valid && $scope.partyPlantForm2.$valid) {
            if ($scope.PartyPlantAction === 'Save' || $scope.PartyPlantAction === 'Update') {
                $http({
                    method: 'POST',
                    url: 'parties/party/CreatePartyPlant',
                    data: {
                        'entity': $scope.partyPlant
                        , 'addressMaster': $scope.addressMasterPlant
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.partyPlants.push(response.data.partyPlant);
                        $scope.GetPartyPlantByPartyId(response.data.PartyPlant.PartyId);
                        $scope.ClearPartyPlant();
                        $scope.GetPartyPlantSequence();
                        angular.element(document.querySelector('#PlantPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };

    $scope.confirmPartyPlantDelete = function (data) {
        $scope.PlantName = data.UserName;
        $scope.deleteId = data.Id;
        $scope.message_confirmation = "Are you sure to delete [" + $scope.PlantName + "]? ";
    };

    $scope.DeletePartyPlant = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            $scope.partyPlantList.splice($scope.index, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'parties/party/deletepartyplant',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPartyPlantByPartyId($scope.party.Id);
                    $scope.GetPartyPlantSequence();
                    $scope.addressMasterPlant = {};
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

    $scope.ClearPartyPlant = function () {
        $scope.partyPlant = {};
        $scope.partyPlantNew = {};
        $scope.GetPartyPlantSequence();
        $scope.partyPlantNew.Active = true;
        $scope.addressMasterPlant = {};
    };

    $scope.PartyPlantContactAction = 'Save';

    $scope.contactMasterPlantmodel = {
        Id: null,
        PartyPlantId: null,
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
        Active: true,
        Archive: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    }
    $scope.contactMasterPlant = Object.assign({}, $scope.contactMasterPlantmodel);

    $scope.SavePartyPlantContact = function () {
        $scope.contactMasterPlant.PartyPlantId = $scope.partyPlantNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.partyPlantForm3.$valid) {
            if ($scope.PartyPlantContactAction === 'Save' || $scope.PartyPlantContactAction === 'Update') {
                $http({
                    method: 'POST',
                    url: 'parties/party/CreatePartyPlantContact',
                    data: {
                        'entity': $scope.contactMasterPlant
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPartyPlantContactData();
                        $scope.ClearPartyPlantContact();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };

    $scope.GetPartyPlantContact = function (data) {
        $scope.PartyPlantContactAction = 'Update';
        $scope.contactMasterPlant = Object.assign({}, data);
    }

    $scope.GetPartyPlantContactData = function () {
        $http.get('parties/party/GetPartyPlantContactData?PartyPlantId=' + $scope.partyPlantNew.Id)
            .then(function (response) {
                $scope.partyPlantContacts = response.data;
            });
    };

    $scope.ClearPartyPlantContact = function () {
        $scope.PartyPlantContactAction = 'Save';
        $scope.contactMasterPlant = Object.assign({}, $scope.contactMasterPlantmodel);
    };

    // #endregion

    // #region PartyBank

    $scope.fileNew = {
        Id: null,
        CompanyPartyId: null,
        Bank: null,
        BankBranch: null,
        BankAcountNo: null,
        IFSCCode: null,
        MICRCode: null,
        Remarks: null,
        Active: true
    };
    $scope.partyBank = Object.assign({}, $scope.fileNew);

    $scope.GetPartyBankList = function (companyId, companyPartyId) {
        $scope.companyId = companyId;
        $scope.currentClickedCompanyPartyId = companyPartyId;
        $scope.partyBank.CompanyPartyId = $scope.currentClickedCompanyPartyId;
        angular.element(document.querySelector('#BankPopUp')).modal('show');
    };

    $scope.PartyBankList = [];
    $scope.GetPartyBank = function () {
        $http({
            method: 'GET',
            url: 'parties/partybank/getdetaillist?partyId=' + $scope.party.Id
        }).then(function (response) {
            $scope.PartyBankList = response.data;
        });
    };

    $scope.searchBankModalList = [
        {
            'name': 'Id',
            'value': 'BankMasterId'
        },
        {
            'name': 'Account Title',
            'value': 'AccountTitle'
        },
        {
            'name': 'Bank Code',
            'value': 'BankCode'
        },
        {
            'name': 'Bank Name',
            'value': 'BankName'
        },
        {
            'name': 'Bank Branch',
            'value': 'BankBranch'
        }
    ];

    $scope.bankparameters = {
        limit: 5,
        offset: 0,
        order: 'asc',
        sort: 'AccountTitle',
        searchBy: "AccountTitle",
        pageSize: 5,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.BankSearch = function () {
        try {
            $scope.Url = 'Parties/partybank/partybanklist?companyId=' + $scope.companyId

            baseService.setCurrentPage('bankList');
            $scope.BankModalList = function (pageno) {
                baseService.paginationBase($scope.Url, pageno, $scope.bankparameters)
                    .then(function (data) {
                        $scope.bankList = data.Rows;
                        $scope.bankparameters.total_count = data.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#BankSearchPopUp')).modal('show');
            $scope.BankModalList();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectBank = function (bn) {
        try {
            var partyBank = $filter('filter')($scope.PartyBankList, { CompanyPartyId: $scope.currentClickedCompanyPartyId });

            if ($filter('filter')(partyBank, { BankMasterId: bn.BankMasterId }).length > 0) {
                throw 'This Bank is exists.';
            }

            $scope.partyBank.BankMasterId = bn.BankMasterId;
            $scope.partyBank.AccountNumber = bn.AccountNumber;
            $scope.partyBank.BankCode = bn.BankCode;
            $scope.partyBank.AccountTitle = bn.AccountTitle;
            $scope.partyBank.BankName = bn.BankName;
            $scope.partyBank.BankBranchName = bn.BankBranchName;
            $scope.partyBank.BankBranchCode = bn.BankBranchCode;
            $scope.partyBank.Active = true;
            $scope.partyBank.PartyId = $scope.party.Id;
            $scope.partyBank.CompanyPartyId = $scope.currentClickedCompanyPartyId;
            $scope.PartyBankList.push($scope.partyBank);
            $scope.partyBank = {};
            angular.element(document.querySelector('#BankSearchPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'BankSearchPopUp');
        }
    };

    $scope.PartyBankAction = 'Save';
    $scope.SavePartyBank = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if (baseService.isUndefinedOrNull($scope.partyBank.Bank)) {
                throw "Bank is required.";
            }
            if (baseService.isUndefinedOrNull($scope.partyBank.BankBranch)) {
                throw "Bank Branch is required.";
            }
            if (baseService.isUndefinedOrNull($scope.partyBank.BankAccountNo)) {
                throw "Bank AccountNo is required.";
            }

            if ($scope.BankForm.$valid) {
                if ($scope.PartyBankAction == 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Parties/Party/CreatePartyBank',
                        data: { 'data': $scope.partyBank },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BankPopUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BankPopUp');
                            $scope.GetPartyBank();
                            $scope.PartyBankAction = 'Save';
                            $scope.ClearPartyBank();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BankPopUp');
                    }
                }
                else {
                    $http({
                        method: 'POST',
                        url: 'Parties/Party/EditPartyBank',
                        data: { 'data': $scope.partyBank },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'BankPopUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'BankPopUp');
                            $scope.GetPartyBank();
                            $scope.PartyBankAction = 'Save';
                            $scope.ClearPartyBank();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'BankPopUp');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'BankPopUp');
        }
    };
    $scope.ClosePartyBank = function () {
        angular.element(document.querySelector('#BankPopUp')).modal('hide');
        $scope.ClearPartyBank();
    }

    $scope.ClearPartyBank = function () {
        $scope.PartyBankAction = 'Save';
        $scope.fileNew = {
            Id: null,
            CompanyPartyId: $scope.currentClickedCompanyPartyId,
            Bank: null,
            BankBranch: null,
            BankAcountNo: null,
            IFSCCode: null,
            MICRCode: null,
            Remarks: null,
            Active: true
        };
        $scope.partyBank = Object.assign({}, $scope.fileNew);
    }

    $scope.CloseBank = function () {
        angular.element(document.querySelector('#BankPopUp')).modal('hide');
    };


    $scope.GetPartyBankById = function (obj) {
        $scope.PartyBankAction = 'Update';
        $scope.fileNew = {
            Id: null,
            CompanyPartyId: $scope.currentClickedCompanyPartyId,
            Bank: null,
            BankBranch: null,
            BankAcountNo: null,
            IFSCCode: null,
            MICRCode: null,
            Remarks: null,
            Active: true
        };
        $scope.fileNew = Object.assign({}, obj);
        $scope.partyBank = Object.assign({}, $scope.fileNew);

    };

    $scope.confirmPartyBankDelete = function (data) {
        $scope.Name = data.Bank;
        $scope.deleteId = data.Id;
        $scope.message_confirmation = "Are you sure to delete permanently [" + $scope.Name + "]?";
    };

    $scope.DeletePartyBank = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            var drc = $scope.PartyBankList.length;
            while (drc--) {
                if ($scope.PartyBankList[drc]['Id'] === $scope.deleteId) {
                    $scope.PartyBankList.splice(drc, 1);
                }
            }
        }
        else {
            $http({
                method: 'POST',
                url: 'parties/partybank/deletepartybank',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    var drc = $scope.PartyBankList.length;
                    while (drc--) {
                        if ($scope.PartyBankList[drc]['Id'] === $scope.deleteId) {
                            $scope.PartyBankList.splice(drc, 1);
                        }
                    }
                    $scope.ClearPartyBank();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });

            return true;
        }
    };

    // #endregion

    $scope.PartyAdditionalInfoDataList = [];
    $scope.GetPartyAdditionalInfoDataList = function () {

        $http({
            method: 'GET',
            url: 'Productions/PackingInvoice/GetPartyAdditionalInfoDataList?partyId=' + $scope.party.Id
        }).then(function successCallback(response) {
            $scope.PartyAdditionalInfoDataList = response.data;
        });
    }

    $scope.SaveAdditionalInfo = function () {
        try {
            $http({
                method: 'POST',
                url: 'Productions/PackingInvoice/CreatePartyAdditionalInfo',
                data: {
                    'data': $scope.PartyAdditionalInfoDataList
                    , 'salesId': $scope.salesVM.Id
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
}