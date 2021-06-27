'use strict';
CompanyGroupController.$inject = ["addressService", 'cboService', 'fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyGroupController(addressService, cboService, fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Company Group";
    $scope.Action = 'Save';
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

    $scope.companyGroupNew = {
        Id: null
        , AddressMasterId: null
        , ContactMasterId: null
        , ConsolidateCurrencyId: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , LegalName: null
        , Description: null
        , Remarks: null
        , PKPrefixField: null
        , Image: null
        , IsUserAccessFromEmployee: false
        , Active: true
        , LanguageId: null
        , ApopURL: null
        ,MyAppURL:null
    };

    $scope.companyGroup = Object.assign({}, $scope.companyGroupNew);
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
        angular.copy($scope.companyGroups[$scope.index], $scope.companyGroupNew);
        angular.copy($scope.companyGroupNew, $scope.companyGroup);

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
            angular.copy($scope.companyGroup, $scope.companyGroupNew);
            var formData = new FormData();
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/companygroup/create',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("companyGroup", angular.toJson(data.companyGroup));
                        formData.append("addressMaster", JSON.stringify(data.addressMaster));
                        formData.append("contactMaster", JSON.stringify(data.contactMaster));
                        formData.append("localLanguages", JSON.stringify(data.localLanguages));
                        return formData;
                    },
                    data: {
                        'companyGroup': $scope.companyGroupNew, 'addressMaster': $scope.addressMaster,
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
                        //$scope.companyGroups.push(data.CompanyGroup);
                        //baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $scope.addressMaster.UpdatedDate = null;
                $scope.contactMaster.UpdatedDate = null;
                $scope.companyGroupNew.UpdatedDate = null;
                $scope.addressMaster.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                $scope.contactMaster.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                $scope.companyGroupNew.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
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
                        'companyGroup': $scope.companyGroupNew, 'addressMaster': $scope.addressMaster,
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

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.companyGroup.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/companygroup/delete/' + $scope.companyGroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.companyGroups.splice($scope.index, 1);
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

    //Local Language Part Start
    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = 'Add Row';
    $scope.languageDataList = [];
    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNew.LanguageId)) {
                throw 'Please select your language.';
            }
            if (baseService.isUndefinedOrNull($scope.languageNew.Name)) {
                throw 'Please insert name.';
            }
            var isAvailable = false;
            var lng = document.getElementById("languageId").options[document.getElementById('languageId').selectedIndex].text;
            for (var i = 0; i < $scope.languageDataList.length; i++) {
                isAvailable = listValidation($scope.languageDataList[i].LanguageId, $scope.languageNew.LanguageId, i);
                if (isAvailable) {
                    throw 'This Language : [' + lng + '] has been already taken';
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
            }
            else {
                $scope.language.LanguageName = lng;
                $scope.languageDataList[$scope._languageIndex] = $scope.language;
            }
            if (!$scope.languageTbl) {
                $scope.languageTbl = true;
            }
            clearLanguage();
        } catch (e) {
            ShowResult(e, 'failure');
        }
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
        $scope.language = $scope.languageDataList[index];
        $scope.languageNew = Object.assign({}, $scope.language);
        $scope._languageIndex = index;
        $scope.LanguageCaption = 'Update Row';
    };

    $scope.languageDelete = function (data, index) {
        $scope.message_confirmation = '';
        $scope._languageIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LanguageName + ' ]';
        angular.element(document.querySelector('#confirmlngPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        $scope.languageDataList.splice($scope._languageIndex, 1);
        if ($scope.languageDataList.length > 0)
            $scope.languageTbl = true;
        else
            $scope.languageTbl = false;
        $scope._languageIndex = -1;
    };

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Add Row';
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
        $scope.Action = 'Save';
        $scope.companyGroup = { Sequence: seq, Active: true };
        $scope.contactMaster = { Email1: null, Email2: null, Email3: null };
        $scope.addressMaster = { Email1: null };
        $scope.imageSrc = null;
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