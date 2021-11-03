'use strict';
PurchaseOrganisationController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PurchaseOrganisationController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Purchase Organisation';
    $scope.Action = 'Save';
    $scope.ContactAction = 'Add Row';
    $scope.index = -1;
    $scope.indexContact = -1;
    $scope.purchaseOrganisations = [];
    $scope.path = 'Organizations/purchaseorganisation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getPurchaseOrganisationContactListUrl = 'addresses/contactmasterpurchaseorganisation/getlistbypurchaseorganisation/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.purchaseOrganisations = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.purchaseOrganisation = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        AddressMasterId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        VATResistrationNo: null,
        Description: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
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
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
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
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.purchaseOrganisationMaster = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        PurchaseOrganisationId: null,
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

    $scope.purchaseOrganisationMasters = [];
    $scope.getpurchaseOrganisationMastersData = function (id) {
        $http({
            method: 'GET',
            url: 'Organizations/purchaseorganisationmaster/getlist?purchaseOrganisationId=' + id
        }).then(function successCallback(response) {
            $scope.purchaseOrganisationMasters = response.data;
        });
    };
    $scope.getpurchaseOrganisationMastersData();

    $scope.getPurchaseOrganisationContact = function () {
        $scope.parameters = {
            limit: 20,
            offset: 0,
            order: 'asc',
            sort: '[PurchaseOrganisationId]',
            searchBy: "PurchaseOrganisationId",
            search: $scope.purchaseOrganisation.Id
        };
        baseService.paginationBase($scope.getPurchaseOrganisationContactListUrl, 1, $scope.parameters)
            .then(function (result) {
                $scope.contactMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

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

    $scope.onCountryCityChange = function (countryId) {
        addressService.getCboCityByCountry(countryId, function (result) {
            $scope.CityList = result;
        });
    };

    $scope.onCityChange = function (cityId) {
        addressService.getCboAreaByCity(cityId, function (result) {
            $scope.AreaList = result;
        });
    };

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.GetSequence = function () {
        $http.get('Organizations/purchaseorganisation/getautosequence')
            .then(function (response) {
                $scope.purchaseOrganisation.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.purchaseOrganisation = $scope.purchaseOrganisations[$scope.index];
        $scope.GetAddressMaster($scope.purchaseOrganisation.AddressMasterId);
        $scope.getPurchaseOrganisationContact();
        $scope.getpurchaseOrganisationMastersData($scope.purchaseOrganisation.Id);
        $scope.showContactMaster = true;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetContact = function (id, index) {
        $scope.indexContact = index;
        $scope.contactMaster = $scope.contactMasters[$scope.indexContact];
    };

    $scope.GetAddressMaster = function (id) {
        $http.get('addresses/addressmaster/get/' + id)
            .then(function (response) {
                $scope.addressMaster = response.data;
                $scope.addressMaster.AddedDate = $filter('dateFilter')($scope.addressMaster.AddedDate);
                $scope.addressMaster.UpdatedDate = $filter('dateFilter')($scope.addressMaster.UpdatedDate);
                $scope.onContinentChange($scope.addressMaster.ContinentId);
                $scope.onCountryChange($scope.addressMaster.CountryId);
                $scope.onCityChange($scope.addressMaster.CityId);
            });
    };

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
            if ($scope.purchaseOrganisation.UserName == null || $scope.purchaseOrganisation.UserName == '') {
                throw 'Purchase Organisation User Name Can Not Be Blank !!!';
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

    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.purchaseOrganisationTab1Form.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.purchaseOrganisationTab2Form.$invalid) {
            $scope.setTab(2);
        }
    }
    // #endregion

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        if ($scope.purchaseOrganisationForm.$valid && $scope.purchaseOrganisationTab1Form.$valid && $scope.purchaseOrganisationTab2Form.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/purchaseorganisation/create',
                    data: {
                        'purchaseOrganisation': $scope.purchaseOrganisation,
                        'addressMaster': $scope.addressMaster,
                        'contactMasters': $scope.contactMasters,
                        'purchaseOrganisationMasters': $scope.purchaseOrganisationMasters
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.purchaseOrganisations.push(response.data.PurchaseOrganisation);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/purchaseorganisation/edit',
                    data: {
                        'purchaseOrganisation': $scope.purchaseOrganisation,
                        'addressMaster': $scope.addressMaster,
                        'contactMasters': $scope.contactMasters,
                        'purchaseOrganisationMasters': $scope.purchaseOrganisationMasters
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.purchaseOrganisations[$scope.index] = $scope.purchaseOrganisation;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.purchaseOrganisation.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/purchaseorganisation/delete/' + $scope.purchaseOrganisation.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.status.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.purchaseOrganisations.splice($scope.index, 1);
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

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.purchaseOrganisation = {};
        $scope.addressMaster = {};
        $scope.contactMaster = {};
        $scope.contactMasters = [];
        $scope.purchaseOrganisation.Sequence = seq;
        $scope.purchaseOrganisation.Active = true;
        $scope.addressMaster.Email = null;
        $scope.getpurchaseOrganisationMastersData();
        $scope.showContactMaster = false;
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}