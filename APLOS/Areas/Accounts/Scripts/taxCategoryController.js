'use strict';
TaxCategoryController.$inject = ['addressService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TaxCategoryController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Tax Category';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.taxCodeCategories = [];
    $scope.path = 'accounts/taxcategory/';
    $scope.getListUrl = $scope.path + 'getlist/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.taxCategory = {
        Id: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , CountryId: null
        , TaxCircle: null
        , Description: null
        , Remarks: null
        , TaxCategoryType:null
        , Active: true
    };

    $scope.taxCategoryNew = Object.assign({}, $scope.taxCategory);
    baseService.init($scope.getListUrl);
    $scope.getList = function () {
        $scope.getData = function (pageno) {
            $rootScope.parameters.countryId = $scope.taxCategoryNew.CountryId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCodeCategories = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.taxCategoryNew.Sequence = data;
        });
    };

    $scope.GetSequence();
    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
    });

    cboService.getEnumCbo("enum/GetTaxCircleEnumCbo", function (result) {
        $scope.taxCircleList = result;
    });
    $scope.taxCategoryTypeList = [];
    cboService.getEnumCbo("enum/GetTaxCategoryTypeEnumCbo", function (result) {
        $scope.taxCategoryTypeList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.taxCategory = $scope.taxCodeCategories[$scope.index];
        $scope.taxCategoryNew = Object.assign({}, $scope.taxCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.taxCategoryNew, $scope.taxCategory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxCategoryNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.taxCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.taxCodeCategories.push(response.data.TaxCategory);
                        $scope.taxCodeCategories = $filter('orderBy')($scope.taxCodeCategories, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.taxCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.taxCodeCategories[$scope.index] = $scope.taxCategory;
                            $scope.taxCodeCategories = $filter('orderBy')($scope.taxCodeCategories, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.taxCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.taxCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.taxCodeCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.taxCategory = {};
        $scope.taxCategoryNew = { CountryId: $scope.taxCategoryNew.CountryId };
        $scope.taxCategoryNew.Sequence = seq;
        $scope.taxCategoryNew.Active = true;
    }
}