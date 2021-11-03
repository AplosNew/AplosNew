"use strict";
UpdatePlantController.$inject = ["addressService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function UpdatePlantController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Plant";
    $scope.Action = "Update";
    $scope.ContactAction = "Update Row";
    $scope.index = -1;
    $scope.indexContact = -1;
    $scope.plants = [];
    $scope.path = "Organizations/plant/";
    $scope.getListUrl = $scope.path + "GetPlantList";
    $scope.getPlantContactListUrl = "addresses/contactmasterplant/getlistbyplant/";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $scope.Clear();
        $rootScope.parameters.companyGroupId = $scope.plant.CompanyGroupId;
        $rootScope.parameters.companyId = $scope.plant.CompanyId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.plants = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.plant = {
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
        LanguageId: null,
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
        AddedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
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
        AddedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.language = {
        Id: null,
        LanguageId: null,
        LanguageName: null,
        Name: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);

    // #region GetPlantContact

    $scope.getPlantContact = function () {
        $scope.parameters = {
            limit: 20,
            offset: 0,
            order: "asc",
            sort: "[Type]",
            searchBy: "PlantId",
            search: $scope.plant.Id
        };
        baseService.paginationBase($scope.getPlantContactListUrl, 1, $scope.parameters)
            .then(function (result) {
                $scope.contactMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
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

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCboCompanyByCompanyGroup = function (companyGroupId) {
        cboService.getCboCompanyByCompanyGroup(companyGroupId, function (result) {
            $scope.companyList = result;
        });
    };
    // #endregion

    $scope.getSequence = function () {
        cboService.getSequence('Organizations/plant/getautosequence?companyId=' + $scope.plant.CompanyId, function (result) {
            $scope.plant.Sequence = result;
        });
    };
    $scope.getSequence();


    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    // #region Get

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plant = $scope.plants[$scope.index];
        $scope.getCboCompanyByCompanyGroup($scope.plant.CompanyGroupId);
        $scope.GetAddressMaster($scope.plant.AddressMasterId);
        $scope.getPlantContact();
        $scope.languageData();
        $scope.showContactMaster = true;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    // #endregion

    // #region GetAddressMaster

    $scope.GetAddressMaster = function (id) {
        $http.get("addresses/addressmaster/get/" + id)
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
        $scope.ContactAction = "Update Row";
    };

    $scope.contactMasters = [];

    $scope.addRow = function () {
        try {
            if ($scope.plant.Code === null || $scope.plant.Code === "") {
                throw "Plant User Name Can Not Be Blank !!!";
            }
            if ($scope.contactMaster.ContactPerson === null || $scope.contactMaster.ContactPerson === "") {
                throw "Please Enter Person Name  !!!";
            }
            if ($scope.ContactAction === "Update Row") {
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
            ShowResult(e, "failure");
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
        $scope.ContactAction = "Update Row";
    };
    // #endregion

    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.plantForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.plantForm2.$invalid) {
            $scope.setTab(2);
        }
    }
    // #endregion

    // #region Save

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        reDirectToRequiredTab();
        if ($scope.plantForm.$valid && $scope.plantForm1.$valid && $scope.plantForm2.$valid) {
            if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "Organizations/plant/edit",
                    data: { "plant": $scope.plant, "addressMaster": $scope.addressMaster, "contactMaster": $scope.contactMasters, "localLanguages": $scope.languageDataList },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.plants[$scope.index] = $scope.plant;
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getSequence();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
        return true;
    };

    // #endregion

    $scope.showContactMaster = true;

    //#region Local Language Part Start
    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = "Add Row";
    $scope.languageDataList = [];
    $scope.updateLanguage = function (languageId, languageName) {
        $scope.languageDataList[$scope._languageIndex].Name = languageName;
        $scope._languageIndex = -1;
        $scope.languageNew = {};
    };

    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNew.LanguageId)) {
                throw "Please select your language.";
            }
            if (baseService.isUndefinedOrNull($scope.languageNew.Name)) {
                throw "Please insert name.";
            }
            var isAvailable = false;
            var lng = document.getElementById("languageId").options[document.getElementById("languageId").selectedIndex]
                .text;
            for (var i = 0; i < $scope.languageDataList.length; i++) {
                isAvailable = listValidation($scope.languageDataList[i].LanguageId, $scope.languageNew.LanguageId, i);
                if (isAvailable) {
                    throw "This Language : [" + lng + "] has been already taken";
                }
            }
            angular.copy($scope.languageNew, $scope.language);
            if ($scope._languageIndex === -1) {
                $scope.languageDataList.push({
                    Id: null,
                    LanguageId: $scope.language.LanguageId,
                    LanguageName: lng,
                    Name: $scope.language.Name
                });
            } else {
                $scope.language.LanguageName = lng;
                $scope.languageDataList[$scope._languageIndex] = $scope.language;
            }
            if (!$scope.languageTbl) {
                $scope.languageTbl = true;
            }
            clearLanguage();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function listValidation(oldValue, newValue, index) {
        if ($scope._languageIndex === -1) {
            if (oldValue === newValue) {
                return true;
            }
        }
        else {
            if ($scope._languageIndex !== index) {
                if (oldValue === newValue) {
                    return true;
                }
            }
        }
        return false;
    }

    $scope.languageDataParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "LanguageName",
        searchBy: null,
        pageSize: 10,
        total_count: 0,
        search: "LanguageName",
        serverPagination: true
    };

    $scope.languageData = function () {
        $scope.languageDataUrl = "Setups/LocalLanguage/GetPlantLanguageList?plantId=" + $scope.plant.Id;
        $scope.getlanguageData = function (pageno) {
            baseService.paginationBase($scope.languageDataUrl, pageno, $scope.languageDataParameters)
                .then(function (result) {
                    $scope.languageDataList = result.Rows;
                    $scope.languageDataParameters.total_count = result.Total;
                    if ($scope.languageDataList.length > 0) {
                        $scope.languageTbl = true;
                    }
                },
                function () {
                    ShowResult(commonMessage.NetworkError, "failure", "languageDataId");
                }).finally(function () {
                });
        };
        $scope.getlanguageData();
    };

    $scope.languageEdit = function (data, index) {
        $scope.language = $scope.languageDataList[index];
        $scope.languageNew = Object.assign({}, $scope.language);
        $scope._languageIndex = index;
        $scope.LanguageCaption = "Update Row";
    };

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = "Update Row";
    }

     //#endregion Local Language Part End

    $scope.Clear = function () {
        ClearFields($scope.getSequence());
        clearLanguage();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Update";
        $scope.CompanyGroupId = $scope.plant.CompanyGroupId;
        $scope.CompanyId = $scope.plant.CompanyId;
        $scope.plant = {};
        $scope.plant.CompanyGroupId = $scope.CompanyGroupId;
        $scope.plant.CompanyId = $scope.CompanyId;
        $scope.addressMaster = {};
        $scope.contactMaster = {};
        $scope.contactMasters = [];
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
        $scope.plant.Sequence = seq;
        $scope.plant.Active = true;
        $scope.languageDataList = [];
        $scope.languageTbl = false;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}