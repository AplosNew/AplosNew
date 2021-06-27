'use strict';
PartyGroupController.$inject = ["addressService", 'commonMessage', 'cboService', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function PartyGroupController(addressService, commonMessage, cboService, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Party Group';
    $scope.Action = 'Save';
    $scope.ContactAction = 'Add Row';
    $scope.index = -1;
    $scope.indexContact = -1;
    $scope.partyGroups = [];
    $scope.path = 'Parties/partygroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getPartyContactListUrl = 'addresses/contactmasterparty/getlistbyparty/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.partyGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.partyGroup = {
        Id: null,
        CompanyGroupId: null,
        AddressMasterId: null,
        PartyGroupCategoryId: null,
        PartyGroupSubCategoryId: null,
        PartyGroupClassId: null,
        PartyGroupPreferenceCategoryId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        RefNo: null,
        GroupType: null,
        TransactionType: null,
        ExciseRange: null,
        CommissionRate: 0,
        IECNo: null,
        PINCode: null,
        CSTNO: null,
        LSTNO: null,
        TINNO: null,
        VATResistrationNo: null,
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
        UpdatedFromIP: null
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
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
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
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.partyBrand = {
        Id: null,
        PartyGroupId: null,
        PartyId: null,
        BrandId: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null
    };

    // #region getPartyGroupContact
    $scope.getPartyGroupContact = function () {
        $scope.parameters = {
            limit: 20,
            offset: 0,
            order: 'asc',
            sort: 'Type',
            searchBy: "PartyGroupId",
            search: $scope.partyGroup.Id
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

    // #region partyBrands
    $scope.partyBrands = [];
    $scope.getpartyBrands = function (id) {
        $http({
            method: 'GET',
            url: 'Parties/partybrand/getlist?partyGroupId=' + id
        }).then(function (response) {
            $scope.partyBrands = response.data;
        });
    };
    $scope.getpartyBrands();
    // #endregion

    // #region AddressMaster
    $scope.ContinentList = [];
    $scope.CountryList = [];
    $scope.StateList = [];
    $scope.AreaList = [];
    $scope.CityList = [];

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
    // #endregion

    // #region GetSequence
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.partyGroup.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    // #endregion

    // #region Get
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.partyGroup = $scope.partyGroups[$scope.index];
        $scope.GetAddressMaster($scope.partyGroup.AddressMasterId);
        $scope.getPartyGroupContact();
        $scope.getpartyBrands($scope.partyGroup.Id);
        $scope.showContactMaster = true;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetAddressMaster = function (id) {
        $http.get('addresses/addressmaster/get/' + id)
            .then(function (response) {
                $scope.addressMaster = response.data;
                $scope.addressMaster.AddedDate = $filter('dateFilter')($scope.addressMaster.AddedDate);
                $scope.addressMaster.UpdatedDate = $filter('dateFilter')($scope.addressMaster.UpdatedDate);
                $scope.onContinentChange($scope.addressMaster.ContinentId);
                $scope.onCountryChange($scope.addressMaster.CountryId);
                $scope.onStateChange($scope.addressMaster.StateId);
                $scope.onDistictChange($scope.addressMaster.DistrictId);
                $scope.onCityChange($scope.addressMaster.CityId);
            });
    };
    // #endregion

    cboService.getCboServicePartyGroupCategory(function (result) {
        $scope.partyGroupCategoryList = result;
    });

    cboService.getCboServicePartyGroupSubCategory(function (result) {
        $scope.partyGroupSubCategoryList = result;
    });

    cboService.getCboServicePartyGroupClass(function (result) {
        $scope.partyGroupClassList = result;
    });

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
            if ($scope.partyGroup.UserName === null || $scope.partyGroup.UserName === '') {
                throw 'PartyGroup User Name Can Not Be Blank !!!';
            }
            if ($scope.contactMaster.ContactPerson === null || $scope.contactMaster.ContactPerson === '') {
                throw 'Please Enter Person Name  !!!';
            }
            if ($scope.ContactAction == 'Add Row') {
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
        $scope.ContactAction = 'Add Row';
    };
    // #endregion

    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.partyGroupForm1.$invalid) {
            $scope.setTab(1);
        }
        //else if ($scope.partyGroupForm2.$invalid) {
        //    $scope.setTab(2);
        //}
        else if ($scope.partyGroupForm3.$invalid) {
            $scope.setTab(4);
        }
    }
    // #endregion

    // #region Save
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        if ($scope.partyGroupForm.$valid && $scope.partyGroupForm1.$valid && $scope.partyGroupForm3.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'partyGroup': $scope.partyGroup, 'addressMaster': $scope.addressMaster, 'contactMaster': $scope.contactMasters, 'partyBrands': $scope.partyBrands },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.partyGroups.push(response.data.PartyGroup);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'partyGroup': $scope.partyGroup, 'addressMaster': $scope.addressMaster, 'contactMaster': $scope.contactMasters, 'partyBrands': $scope.partyBrands },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.partyGroups[$scope.index] = $scope.partyGroup;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };
    // #endregion

    // #region Delete
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.partyGroup.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.partyGroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.partyGroups.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
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
        $scope.partyGroup = {};
        $scope.addressMaster = {};
        $scope.contactMaster = {};
        $scope.contactMasters = [];
        $scope.partyGroup.Sequence = seq;
        $scope.partyGroup.Active = true;
        $scope.getpartyBrands();
        $scope.showContactMaster = false;
        $scope.partyGroup.CommissionRate = 0;
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
}