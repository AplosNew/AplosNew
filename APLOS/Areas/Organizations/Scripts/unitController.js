'use strict';
UnitController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UnitController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Unit';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.units = [];
    $scope.path = 'Organizations/unit/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.unit = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.unitNew = Object.assign({}, $scope.unit);

    $scope.language = {
        Id: null,
        LanguageId: null,
        LanguageName: null,
        Name: null
    };
    $scope.languageNew = Object.assign({}, $scope.language);

    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.units = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.GetSequence = function () {
        $http.get('Organizations/unit/getautosequence')
            .then(function (response) {
                $scope.unitNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.unit = $scope.units[$scope.index];
        $scope.unitNew = Object.assign({}, $scope.unit);
        $scope.unitNew.AddedDate = $filter('dateFilter')($scope.unitNew.AddedDate);
        $scope.unitNew.UpdatedDate = $filter('dateFilter')($scope.unitNew.UpdatedDate);
        clearLanguage();
        $scope.languageDataList = [];
        $scope.languageData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.unitNewForm.$valid) {
            angular.copy($scope.unitNew, $scope.unit);
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/unit/create',
                    //data: $scope.unit,
                    data: { 'unit': $scope.unitNew, 'localLanguages': $scope.languageDataList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.unit = response.data.Unit;
                        $scope.unit.CompanyName = $scope.companyName;
                        $scope.units.push($scope.unit);
                        $scope.units = $filter('orderBy')($scope.units, 'Sequence');
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
                    url: 'Organizations/unit/edit',
                    //data: $scope.unit,
                    data: { 'unit': $scope.unitNew, 'localLanguages': $scope.languageDataList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.unit.CompanyName = $scope.companyName;
                            $scope.units[$scope.index] = $scope.unit;
                            $scope.units = $filter('orderBy')($scope.units, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.unitNew.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/unit/delete/' + $scope.unitNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.units.splice($scope.index, 1);
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
    }
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
        $scope.languageDataUrl = 'Setups/LocalLanguage/GetUnitLanguageList?unitId=' + $scope.unitNew.Id;
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
    }

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

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.unit = {};
        $scope.unitNew = {};
        $scope.unitNew.Sequence = seq;
        $scope.unitNew.Active = true;
        clearLanguage();
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