'use strict';
specialTaxController.$inject = ['addressService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function specialTaxController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Special Tax';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.specialTaxes = [];
    $scope.path = 'setups/specialtax/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
   
   //// $scope.getData();

    $scope.specialTax = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        CountryId: null,
        IsSpacifyToHSNCode: false
    };

    $scope.specialTaxNew = Object.assign({}, $scope.specialTax);
    baseService.init($scope.getListUrl);
    $scope.getList = function () {
        $scope.getData = function (pageno) {
            $rootScope.parameters.countryId = $scope.specialTaxNew.CountryId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.specialTaxes = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    //baseService.init($scope.getListUrl);
    //$rootScope.parameters.countryId = $scope.specialTaxNew.CountryId;
    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.specialTaxes = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.specialTaxNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.specialTax = $scope.specialTaxes[$scope.index];
        $scope.specialTaxNew = Object.assign({}, $scope.specialTax);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.specialTaxNew, $scope.specialTax);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.specialTaxNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.specialTax,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.specialTaxes.push(response.data.specialTax);
                        $scope.specialTaxes = $filter('orderBy')($scope.specialTaxes, 'Sequence');
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
                    data: $scope.specialTax,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.specialTaxes[$scope.index] = $scope.specialTax;
                            $scope.specialTaxes = $filter('orderBy')($scope.specialTaxes, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.specialTaxNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.specialTaxNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.specialTaxes.splice($scope.index, 1);
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
        $scope.specialTax = {};
        $scope.specialTaxNew = { CountryId: $scope.specialTaxNew.CountryId };
        $scope.specialTaxNew.Sequence = seq;
        $scope.specialTaxNew.Active = true;
    }
}