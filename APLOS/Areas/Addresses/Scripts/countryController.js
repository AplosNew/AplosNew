'use strict';
countryController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService',  '$http', '$filter'];
function countryController(addressService, cboService, commonMessage, $scope, $rootScope, baseService,  $http, $filter) {
    $rootScope.title = 'Country';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.countries = [];
    $scope.continentList = [];
    $scope.currencyList = [];
    $scope.path = 'addresses/country/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getcountrylist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.countries = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    addressService.getContinentCbo(function (result) {
        $scope.continentList = result;
    });

    $http({
        method: 'GET',
        url: 'currencies/currency/getcurrencycbo'
    }).then(function successCallback(response) {
        $scope.currencyList = response.data;
    });

    $scope.country = {
        Id: null
        , ContinentId: null
        , CurrencyId: null
        , Nationality: null
        , ICC: null
        , GMTMinute: null
        , GMTHour: null
        , Sequence: 0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , TINCaption: null
        , TINLength: 0
        , NIDCaption: null
        , NIDLength: 0
        , PhoneLength: 0
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.countryNew = angular.copy($scope.country);

    $scope.GetSequence = function () {
        $http.get('addresses/country/getautosequence')
            .then(function (response) {
                $scope.countryNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.language = {
        Id: null
        , LanguageId: null
        , LanguageName: null
        , Name: null
        , UtilityName: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.country = $scope.countries[$scope.index];
        $scope.countryNew = Object.assign({}, $scope.country);
        clearLanguage();
        $scope.languageDataList = [];
        $scope.languageData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.countryForm.$valid) {
            angular.copy($scope.countryNew, $scope.country);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'country': $scope.country
                        , 'localLanguages': $scope.languageDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.countries.push(response.data.Country);
                        $scope.countries = $filter('orderBy')($scope.countries, 'Sequence');
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
                    data: {
                        'country': $scope.country
                        , 'localLanguages': $scope.languageDataList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.countries[$scope.index] = $scope.country;
                            $scope.countries = $filter('orderBy')($scope.countries, 'Sequence');
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

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.countryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.countryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.countries.splice($scope.index, 1);
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

    $scope._languageIndex = -1;
    $scope.languageTbl = false;
    $scope.LanguageCaption = 'Add Row';
    $scope.languageDataList = [];
    $scope.AddMultipleLanguage = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.languageNew.LanguageId))
                throw 'Please select your language';
            if (baseService.isUndefinedOrNull($scope.languageNew.Name))
                throw 'Please insert locally translated name';
            var lng = document.getElementById("languageId").options[document.getElementById('languageId').selectedIndex].text;
            for (var i = 0; i < $scope.languageDataList.length; i++) {
                if (baseService.isAvailableInList($scope.languageDataList[i].LanguageId, $scope.languageNew.LanguageId, i, $scope._languageIndex))
                    throw 'This Language : [' + lng + '] has been already taken';
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

    $scope.languageDataParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'LanguageName'
        , searchBy: null
        , pageSize: 10
        , total_count: 0
        , search: 'LanguageName'
        , serverPagination: true
    };

    $scope.languageData = function () {
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetCountryLanguageList?countryId=' + $scope.countryNew.Id;
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
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.LanguageName + ' ]?';
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

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        clearLanguage();
        return true;
    };

    function clearLanguage() {
        $scope.language = {};
        $scope.languageNew = {};
        $scope._languageIndex = -1;
        $scope.LanguageCaption = 'Add Row';
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.country = {};
        $scope.countryNew = { Sequence: seq, Active: true };
        clearLanguage();
        $scope.languageDataList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}