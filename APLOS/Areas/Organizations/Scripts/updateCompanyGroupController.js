'use strict';
UpdateCompanyGroupController.$inject = ["addressService", 'cboService', 'fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UpdateCompanyGroupController(addressService, cboService, fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Company Group";
    $scope.Action = 'Update';
    $scope.index = -1;
    $scope.companyGroups = [];
    $scope.path = 'Organizations/companygroup/';
    $scope.getListUrl = $scope.path + 'getcompanygrouplist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.companyGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.companyGroup = {
        Id: null,
        AddressMasterId: null,
        ContactMasterId: null,
        ConsolidateCurrencyId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        LegalName: null,
        Description: null,
        Remarks: null,
        PKPrefixField: null,
        Image: null,
        IsUserAccessFromEmployee: false,
        Active: true
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
        Active: true
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
        Active: true
    };

    $scope.language = {
        Id: null,
        LanguageId: null,
        LanguageName: null,
        Name: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);

    //=========================Dropdown==============================
    $scope.consolidateCurrencyList = [];
    $scope.ContinentList = [];
    $scope.CountryList = [];
    $scope.StateList = [];
    $scope.AreaList = [];
    $scope.CityList = [];

    $http({
        method: 'GET',
        url: 'currencies/currency/GetCurrencyCbo/'
    }).then(function successCallback(response) {
        $scope.consolidateCurrencyList = response.data;
    });

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
    //=========================Dropdown End==============================

    $scope.GetSequence = function () {
        $http.get('Organizations/companygroup/getautosequence')
            .then(function (response) {
                $scope.companyGroup.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.companyGroup = $scope.companyGroups[$scope.index];
        $scope.companyGroup.AddedDate = new Date();
        $scope.companyGroup.UpdatedDate = null;
        $scope.GetAddressMaster($scope.companyGroup.AddressMasterId);
        $scope.GetContactMaster($scope.companyGroup.ContactMasterId);
        $scope.languageData();
        $scope.imageSrc = virtualPath.LogoOrImage + $scope.companyGroup.Image;
        $scope.Action = 'Update';
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

    $scope.GetContactMaster = function (id) {
        $http.get('addresses/contactmaster/get/' + id)
            .then(function (response) {
                $scope.contactMaster = response.data;
            });
    };

    $scope.filedata = null;
    $("#uploadImage").change(function () {
        $scope.filedata = this.files[0];
    });

    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.companyGroupForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.companyGroupForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.companyGroupForm3.$invalid) {
            $scope.setTab(3);
        }
    }
    // #endregion

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        if ($scope.companyGroupForm.$valid) {
            var formData = new FormData();
            if ($scope.Action == 'Update') {
                $scope.addressMaster.UpdatedDate = null;
                $scope.contactMaster.UpdatedDate = null;
                $scope.companyGroup.UpdatedDate = null;
                $scope.addressMaster.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                $scope.contactMaster.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                $scope.companyGroup.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                $http({
                    method: 'POST',
                    url: 'Organizations/companygroup/edit',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("companyGroup", angular.toJson(data.companyGroup));
                        formData.append("addressMaster", JSON.stringify(data.addressMaster));
                        formData.append("contactMaster", JSON.stringify(data.contactMaster));
                        formData.append("localLanguages", JSON.stringify(data.localLanguages));
                        formData.append('file', data.file);
                        return formData;
                    },
                    data: {
                        'companyGroup': $scope.companyGroup, 'addressMaster': $scope.addressMaster,
                        'contactMaster': $scope.contactMaster, 'file': $scope.filedata
                        , 'localLanguages': $scope.languageDataList
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    //Local Language Part Start
    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.languageDataList = [];
    $scope.updateLanguage = function (languageId, languageName) {
        $scope.languageDataList[$scope._languageIndex].Name = languageName;
        $scope._languageIndex = -1;
        $scope.languageNew = {};
    };

    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        // LanguageId
        if ($scope._languageIndex == -1) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope._languageIndex != index) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }

    $scope.languageDataParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'LanguageName',
        searchBy: null,
        pageSize: 10,
        total_count: 0,
        search: 'LanguageName',
        serverPagination: true
    };
    $scope.languageData = function () {
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetCompanyGroupLanguageList?companyGroupId=' + $scope.companyGroup.Id;
        $scope.getlanguageData = function (pageno) {
            baseService.paginationBase($scope.languageDataUrl, pageno, $scope.languageDataParameters)
                .then(function (result) {
                    $scope.languageDataList = result.Rows;
                    $scope.languageDataParameters.total_count = result.Total;
                    if ($scope.languageDataList.length > 0) {
                        $scope.languageTbl = true;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'languageDataId');
                }).finally(function () {
                });
        };
        $scope.getlanguageData();
    }
    $scope.languageEdit = function (data, index) {
        $scope.language = data;
        $scope.languageNew = Object.assign({}, $scope.language);
        $scope._languageIndex = index;
        $scope.LanguageCaption = 'Update Row';
    }

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Update Row';
    }
    //Local Language Part End

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        clearLanguage();
        return true;
    };

    $scope.ClearImage = function () {
        $scope.message_confirmation = 'Are you sure to remove this logo?';
        angular.element(document.querySelector('#confirmDocDelete')).modal('show');
    };


    $scope.removeDoc = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
        $http({
            method: 'POST',
            url: 'Organizations/CompanyGroup/DeleteLogo?Id=' + $scope.companyGroup.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.imageSrc = null;
                document.getElementById("uploadImage").value = '';
                document.getElementById("uploadImageSrc").setAttribute('src', null);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Update';
        $scope.companyGroup = {};
        $scope.contactMaster = {};
        $scope.addressMaster = {};
        $scope.companyGroup.Sequence = seq;
        $scope.companyGroup.Active = true;
        $scope.imageSrc = null;
        $scope.addressMaster.Email = null;
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
        $scope.languageDataList = [];
        $scope.languageTbl = false;
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}