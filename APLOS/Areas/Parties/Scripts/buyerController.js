'use strict';
BuyerController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function BuyerController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Buyer";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyers = [];
    $scope.path = 'Parties/buyer/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.buyers = result.Rows;
                $scope.buyerNew = { Active: $scope.buyerNew.Active };
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.buyer = {
        Id: null,
        Sequence: null,
        BuyerCategoryId: null,
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
    $scope.buyerNew = Object.assign({}, $scope.buyer);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.buyerNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyer = $scope.buyers[$scope.index];
        $scope.buyerNew = Object.assign({}, $scope.buyer);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.buyerCategoryList = [];
    $http({
        method: 'GET',
        url: 'Parties/buyercategory/getcbo',
    }).then(function (response) {
        $scope.buyerCategoryList = response.data;
    });

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerNewForm.$valid) {
            angular.copy($scope.buyerNew, $scope.buyer);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.buyer,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyers.push(response.data.Buyer);
                        $scope.buyers = $filter('orderBy')($scope.buyers, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.buyer,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.buyers[$scope.index] = $scope.buyer;
                            $scope.buyers = $filter('orderBy')($scope.buyers, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.buyerNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.buyerNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.buyers.splice($scope.index, 1);
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
        $scope.Action = "Save";
        $scope.buyer = {};
        $scope.buyerNew = {};
        $scope.buyerNew.Sequence = seq;
        $scope.buyerNew.Active = true;
    }
}