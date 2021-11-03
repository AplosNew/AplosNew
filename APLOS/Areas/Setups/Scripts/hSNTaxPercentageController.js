'use strict';
HSNTaxPercentageController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http'];
function HSNTaxPercentageController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $rootScope.title = "HSNTaxPercentage";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.hSNTaxPercentages = [];
    $scope.path = 'Setups/hsntaxpercentage/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'HSNCode', 'HSNCode');
    $scope.getData = function (pageno) {
        $rootScope.parameters.CountryId = $scope.hSNTaxPercentage.CountryId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.hSNTaxPercentages = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.hSNTaxPercentage = {
        Id: null,
        CountryId: null,
        HSNCodeId: null,
        TaxCategoryId: null,
        Percentage: null,
        EffectiveDate: null
    };

    $scope.searchByList = [
        {
            'name': 'HSN Code',
            'value': 'HSNCode'
        },
        {
            'name': 'Tax Category',
            'value': 'TaxCategory'
        }
    ];

    $scope.hSNTaxPercentagesUpdate = [];
    $scope.Get = function (id, index, data) {
        $scope.index = index;
        $scope.gethSNTaxPercentage = angular.copy($scope.hSNTaxPercentages[$scope.index]); // for not change in grid
        $scope.hSNTaxPercentage = $scope.gethSNTaxPercentage;
        $scope.hSNTaxPercentagesUpdate = [];
        $scope.hSNTaxPercentagesUpdate.push(data);
        angular.element(document.querySelector('#HSNTaxPercentagePopUp')).modal('show');
    };

    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $scope.getTaxCategoryCbo = function () {
        cboService.getTaxCategoryCboByCountry($scope.hSNTaxPercentage.CountryId, function (result) {
            $scope.taxCategoryList = result;
        });
    };

    $scope.HSNParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'HSNCode',
        searchBy: "HSNCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getHSNData = function () {
        try {
            baseService.setCurrentPage('hsnList');
            $scope.getHSNCode = function (pageno) {
                baseService.paginationBase('Setups/hsntaxpercentage/gethnslist?countryId=' + $scope.hSNTaxPercentage.CountryId, pageno, $scope.HSNParameters)
                    .then(function (result) {
                        $scope.hsnList = result.Rows;
                        $scope.HSNParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.getHSNCode();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw ('[' + fieldName + '] is required...');
            }
        } catch (e) {
            throw e;
        }
    }
    //DepartmentList for modal

    $rootScope.tempList = [];
    $scope.hSNCodeSelectedList = [];
    $scope.hSNCodeList = [];
    $scope.searchByHSNCodeList = [
        {
            'name': 'Code',
            'value': 'HSNCode'
        },
        {
            'name': 'Description',
            'value': 'HSNDescription'
        },
        {
            'name': 'Country Code',
            'value': 'CountryHSNCode'
        },
        {
            'name': 'Country Description',
            'value': 'CountryHSNDescription'
        }
    ];
    $scope.hSNCodeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'HSNCode',
        searchBy: "HSNCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.hSNCodeSearchPopup = function () {
        $rootScope.tempList = [];
        if ($scope.hSNTaxPercentage.CountryId == null)
            return ShowResult('Please at first select country.', 'failure');
        angular.forEach($scope.hSNCodeSelectedList, function (item) {
            $rootScope.tempList.push({
                HSNId: item.HSNId,
                HSNCode: item.HSNCode,
                HSNDescription: item.HSNDescription,
                CountryHSNCode: item.CountryHSNCode,
                CountryHSNDescription: item.CountryHSNDescription,
            });
        });
        baseService.setCurrentPage('hSNCodeList');
        $scope.loadHSNCodeData = function (pageno) {
            baseService.paginationBase('Setups/hsntaxpercentage/gethnslist?countryId=' + $scope.hSNTaxPercentage.CountryId, pageno, $scope.hSNCodeListParameters)
                .then(function (result) {
                    $scope.hSNCodeList = result.Rows;
                    $scope.hSNCodeListParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.hSNCodeList); t++) {
                        $scope.hSNCodeList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'HSNId', $scope.hSNCodeList[t].HSNId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.loadHSNCodeData();
        angular.element(document.querySelector('#hSNCodeListPopUp')).modal('show');
    };

    $scope.hSNCodeCloseListPopUp = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (item) {
                if (!baseService.valueCheckInList($scope.hSNCodeSelectedList, 'HSNCodeId', item.HSNId)) {
                    if (item.Flag) {
                        $scope.hSNCodeSelectedList.push(
                            {
                                Id: null,
                                HSNCode: item.HSNCode,
                                HSNDescription: item.HSNDescription,
                                CountryHSNCode: item.CountryHSNCode,
                                CountryHSNDescription: item.CountryHSNDescription,
                                CountryId: $scope.hSNTaxPercentage.CountryId,
                                HSNCodeId: item.HSNId,
                                TaxCategoryId: null,
                                Percentage: null,
                                EffectiveDate: null,
                                Flag: item.Flag
                            }
                        );
                    }
                }
            });
        }
        angular.element(document.querySelector('#hSNCodeListPopUp')).modal('hide');
    };

    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempHSNOb = data;
        $scope.hsnIndex = index;
        $scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
    };

    $scope.removeRow = function () {
        $scope.hSNCodeSelectedList.splice($scope.hsnIndex, 1);
    };

    $scope.pushList = function (data, event, list) {
        if (event.currentTarget.checked)
            $rootScope.tempList.push(data);
        else {
            for (var a = 0; a < arrayLength($rootScope.tempList); a++) {
                if ($rootScope.tempList[a].HSNCodeId === data.HSNCodeId)
                    return $rootScope.tempList.splice(a, 1);
            }
            for (var b = 0; b < arrayLength(list); b++) {
                if (list[b].HSNCodeId === data.HSNCodeId)
                    return list.splice(b, 1);
            }
        }
    };

    //End DepartmentList for modal
    function Validation() {
        try {
            CheckField($scope.hSNTaxPercentage.CountryId, "Country");
            CheckField($scope.hSNTaxPercentage.TaxCategoryId, "Tax Category");
            CheckField($scope.hSNTaxPercentage.EffectiveDate, "Effective Date");
        } catch (e) {
            throw e;
        }
    }

    $scope.hSNCodeSelectedSavedList = [];
    function HSNCodeSelectedListfun() {
        $scope.hSNCodeSelectedSavedList = [];
        angular.forEach($scope.hSNCodeSelectedList,
            function (item) {
                $scope.hSNCodeSelectedSavedList.push(
                    {
                        Id: null,
                        CountryId: $scope.hSNTaxPercentage.CountryId,
                        HSNCodeId: item.HSNCodeId,
                        TaxCategoryId: $scope.hSNTaxPercentage.TaxCategoryId,
                        Percentage: item.Percentage,
                        EffectiveDate: $scope.hSNTaxPercentage.EffectiveDate,
                        Flag: item.Flag
                    }
                );
            });
    }

    function CheckPercentage() {
        angular.forEach($scope.hSNCodeSelectedSavedList,
            function (item) {
                if (item.Percentage == null || item.Percentage == '') {
                    throw "Percentage is required... ";
                }
            });
    }

    $scope.Save = function () {
        try {
            Validation();
            $scope.$broadcast('show-errors-check-validity');
            HSNCodeSelectedListfun();
            CheckPercentage();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'HSNTaxPercentage': $scope.hSNCodeSelectedSavedList, 'countryId': $scope.hSNTaxPercentage.CountryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.hSNCodeSelectedList = [];
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.hSNCodeSelectedList = [];
                    ClearFields();
                }
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
        return true;
    };

    $scope.Update = function () {
        try {
            Validation();
            $scope.$broadcast('show-errors-check-validity');
            HSNCodeSelectedListfun();
            CheckPercentage();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'HSNTaxPercentage': $scope.hSNTaxPercentagesUpdate, 'countryId': $scope.hSNTaxPercentage.CountryId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.hSNTaxPercentagesUpdate = [];
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.hSNTaxPercentagesUpdate = [];
                    ClearFields();
                }
            });
            angular.element(document.querySelector('#HSNTaxPercentagePopUp')).modal('hide');
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
        return true;
    };

    $scope.confirmDelete = function (Id) {
        $scope.deleteId = Id;
        $scope.message_confirmation = "Are you sure to delete permanently [" + Id + "] ";
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: $scope.deleteUrl,
            dataType: 'JSON',
            data: { 'Id': $scope.deleteId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        $scope.hSNTaxPercentagesUpdate = [];
        angular.element(document.querySelector('#HSNTaxPercentagePopUp')).modal('hide');
        ClearFields();
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.hSNTaxPercentage = { CountryId: $scope.hSNTaxPercentage.CountryId };
        $scope.hSNCodeSelectedList = [];
    }
}