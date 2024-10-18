'use strict';
CompanyController.$inject = ["addressService", 'fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function CompanyController(addressService, fileReader, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Company';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.companies = [];
    $scope.path = 'Organizations/company/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno, id) {
        $scope.Clear();
        $rootScope.parameters.CompanyGroupId = $scope.company.CompanyGroupId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.companies = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
        $scope.Clear();
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.company = {
        Id: null,
        AddressMasterId: null,
        ContactMasterId: null,
        CompanyGroupId: null,
        EntityType: null,
        OrganizationCategoryId: null,
        OrganizationClassId: null,
        BaseCurrencyId: null,
        COAId: null,
        AlternativeCOAId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        LegalName: null,
        UserName: null,
        TINNo: null,
        VATResistrationNo: null,
        BINNo: null,
        FYSDate: null,
        LPSDate: null,
        PPSDate: null,
        EstedDate: null,
        PKPrefixField: null,
        IsVoucherFromBudget: false,
        Image: null,
        WebDomain: null,
        ManagementGroup: null,
        Description: null,
        Remarks: null,
        Active: true,
        IsCostCenterApplicable: false,
        IsProfitCenterApplicable: false,
        IsBudgetPeriod: false,
        TINRequiredForSalaryAbove: 0,
        IsTINRequiredForSalaryAbove: false,
        IsAccountModuleRunning: false,
        IsInventorySalesBook:true,
        IsInboundInvoiceServiceApplicable:false
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
        Name: null,
        UtilityName: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);

    $scope.ContinentList = [];
    $scope.CountryList = [];
    $scope.StateList = [];
    $scope.AreaList = [];
    $scope.CityList = [];
    $scope.companyGroupList = [];
    $scope.baseCurrencyList = [];
    $scope.organizationCategoryList = [];
    $scope.organizationClassList = [];
    $scope.ManagementGroupList = [];
    $scope.SelectedManagementGroup = null;
    //event fires when click on textbox
    $scope.SelectedManagementGroup = function (selected) {
        if (selected) {
            $scope.company.ManagementGroup = selected.originalObject.Name;
        }
    };
    $scope.inputChanged = function (str) {
        $scope.company.ManagementGroup = str;
    };

    //Gets data from the Database
    $http({
        method: 'GET',
        url: 'Organizations/managementgroup/getlist'
    }).then(function (response) {
        $scope.ManagementGroupList = response.data;
    }, function () {
    });

    $scope.onComGroupChange = function (item) {
        cboService.getCompanyGroupCurrencyCbo(item, function (result) {
            $scope.baseCurrencyList = result;
        });
    };

    $scope.entityTypeList = [];
    cboService.getCboEntityType(function (result) {
        $scope.entityTypeList = result;
    });

    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.ConsolidateCurrencyList = result;
    });

    $http({
        method: 'GET',
        url: 'Organizations/organizationcategory/getorganizationcatlist/'
    }).then(function successCallback(response) {
        $scope.organizationCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Organizations/organizationclass/getorganizationclslist'
    }).then(function successCallback(response) {
        $scope.organizationClassList = response.data;
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

    $scope.GetSequence = function () {
        $http.get('Organizations/company/getautosequence')
            .then(function (response) {
                $scope.company.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    $scope.datee = function (dateObject) {
        if (dateObject) {
            var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            $scope.d = new Date(dateObject);
            $scope.day = $scope.d.getDate();
            $scope.month = $scope.d.getMonth() + 1;
            $scope.year = $scope.d.getFullYear();
            if ($scope.day < 10) {
                $scope.day = "0" + $scope.day;
            }
            if ($scope.month < 10) {
                $scope.month = "0" + $scope.month;
            }
            $scope.date = $scope.day + "-" + months[$scope.month - 1] + "-" + $scope.year;
            return $scope.date;
        }
    };
    $scope.company.EstedDate = $scope.datee($scope.company.EstedDate);
    $scope.Get = function (id, index) {
        $scope.imageSrc = null;
        $scope.index = index;
        $scope.company = $scope.companies[$scope.index];
        $scope.company.AddedDate = $filter('dateFilter')($scope.company.AddedDate);
        $scope.company.UpdatedDate = $filter('dateFilter')($scope.company.UpdatedDate);
        $scope.imageSrc = virtualPath.LogoOrImage + $scope.company.Image;
        $scope.searchStr = $scope.company.ManagementGroup;
        $scope.company.EstedDate = $scope.datee($scope.company.EstedDate);
        $scope.company.LPSDate = $scope.datee($scope.company.LPSDate);
        $scope.GetAddressMaster($scope.company.AddressMasterId);
        $scope.GetContactMaster($scope.company.ContactMasterId);
        $scope.languageData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

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
        if ($scope.companyForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.companyForm2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.companyForm3.$invalid) {
            $scope.setTab(3);
        }
    }
    // #endregion

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            reDirectToRequiredTab();
            if ($scope.companyForm.$valid && $scope.companyForm1.$valid && $scope.companyForm2.$valid && $scope.companyForm3.$valid) {
                var formData = new FormData();
                if ($scope.company.IsTINRequiredForSalaryAbove && baseService.isUndefinedOrNull($scope.company.TINRequiredForSalaryAbove)) {
                    throw "TIN Required For Salary Above is required.";
                }
                if ($scope.company.IsTINRequiredForSalaryAbove && $scope.company.TINRequiredForSalaryAbove <= 0) {
                    throw "TIN Required For Salary Above can not be 0.";
                }
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Organizations/company/create',
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append("company", angular.toJson(data.company));
                            formData.append("addressMaster", JSON.stringify(data.addressMaster));
                            formData.append("contactMaster", JSON.stringify(data.contactMaster));
                            formData.append("localLanguages", JSON.stringify(data.localLanguages));
                            formData.append('file', data.file);
                            return formData;
                        },
                        data: {
                            'company': $scope.company, 'addressMaster': $scope.addressMaster,
                            'contactMaster': $scope.contactMaster, 'file': $scope.filedata
                            , 'localLanguages': $scope.languageDataList
                        }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.companies.push(response.data.Company);
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
                else if ($scope.Action === 'Update') {
                    $scope.addressMaster.UpdatedDate = null;
                    $scope.contactMaster.UpdatedDate = null;
                    $scope.company.UpdatedDate = null;
                    $scope.addressMaster.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                    $scope.contactMaster.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                    $scope.company.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
                    $http({
                        method: 'POST',
                        url: 'Organizations/company/edit',
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append("company", angular.toJson(data.company));
                            formData.append("addressMaster", JSON.stringify(data.addressMaster));
                            formData.append("contactMaster", JSON.stringify(data.contactMaster));
                            formData.append("localLanguages", JSON.stringify(data.localLanguages));
                            formData.append('file', data.file);
                            return formData;
                        },
                        data: {
                            'company': $scope.company, 'addressMaster': $scope.addressMaster
                            , 'contactMaster': $scope.contactMaster, 'file': $scope.filedata
                            , 'localLanguages': $scope.languageDataList
                        }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.companies[$scope.index] = response.data.Company;
                            }
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                }
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure")
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.company.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/company/delete/' + $scope.company.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.companies.splice($scope.index, 1);
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
                    Name: $scope.language.Name,
                    UtilityName: $scope.language.UtilityName
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
        if ($scope._languageIndex === -1) {
            if (oldValue === newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope._languageIndex !== index) {
                if (oldValue === newValue) {
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
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetCompanyLanguageList?companyId=' + $scope.company.Id;
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
    };

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
            url: 'Organizations/Company/DeleteLogo?Id=' + $scope.company.Id,
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
        $scope.CompanyGroupId = $scope.company.CompanyGroupId;
        $scope.company = {};
        $scope.company.CompanyGroupId = $scope.CompanyGroupId;
        $scope.contactMaster = {};
        $scope.addressMaster = {};
        $scope.addressMaster.Email = null;
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
        $scope.company.Sequence = seq;
        $scope.company.TINRequiredForSalaryAbove = 0;
        $scope.imageSrc = null;
        $scope.company.Active = true;
        $scope.imageSrc = null;
        $scope.languageDataList = [];
        $scope.languageTbl = false;
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
        $scope.company.IsInventorySalesBook = true;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}