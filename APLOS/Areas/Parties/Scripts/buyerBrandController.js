'use strict';
buyerBrandController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function buyerBrandController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Buyer Brand';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerBrands = [];
    $scope.path = 'Parties/buyerbrand/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (pageno) {
        $rootScope.parameters.buyerId = $scope.buyerBrandNew.BuyerId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.buyerBrands = result.Rows;
                $scope.buyerBrandNew = { BuyerId: $scope.buyerBrandNew.BuyerId, Active: $scope.buyerBrandNew.Active };
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.buyerList = [];
    cboService.getCboBuyer(function (result) {
        $scope.buyerList = result;
    });

    $scope.buyerBrand = {
        Id: null,
        BuyerId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Archive: false
    };

    $scope.buyerBrandNew = Object.assign({}, $scope.buyerBrand);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.path + 'getautosequence?buyerId=' + $scope.buyerBrandNew.BuyerId, function (data) {
            $scope.buyerBrandNew.Sequence = data;
        });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerBrand = $scope.buyerBrands[$scope.index];
        $scope.buyerBrandNew = Object.assign({}, $scope.buyerBrand);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.buyerBrandNew, $scope.buyerBrand);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerBrandNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.buyerBrand,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerBrands.push(response.data.BuyerBrand);
                        $scope.buyerBrands = $filter('orderBy')($scope.buyerBrands, 'Sequence');
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
                    data: $scope.buyerBrand,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.buyerBrands[$scope.index] = $scope.buyerBrand;
                            $scope.buyerBrands = $filter('orderBy')($scope.buyerBrands, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.buyerBrandNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'delete?id=' + $scope.buyerBrandNew.Id + '&buyerId=' + $scope.buyerBrandNew.BuyerId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.buyerBrands.splice($scope.index, 1);
                    //baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        $scope.buyerBrand = { BuyerId: $scope.buyerBrandNew.BuyerId };
        $scope.buyerBrandNew = { BuyerId: $scope.buyerBrandNew.BuyerId };
        $scope.buyerBrandNew.Active = true;
        $scope.buyerBrandNew.Sequence = seq;
    }
}