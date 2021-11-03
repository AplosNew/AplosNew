'use strict';
buyerActivityController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function buyerActivityController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Buyer Activity';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerActivitys = [];
    $scope.path = 'Setups/OrderActivity/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateBuyerActivity';
    $scope.updateUrl = $scope.path + 'EditBuyerActivity';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.buyerActivity = {
        Id: null
        , CompanyGroupId: null
        , ActivityName: null
        , UserName: null
    };
    $scope.buyerActivityNew = Object.assign({}, $scope.buyerActivity);
    $scope.activityList = [];

    cboService.getEnumCbo("enum/GetBuyerActivityCbo", function (result) {
        $scope.activityList = result;
    });

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    $scope.searchByList = [
        {
            name: 'Id',
            value: 'Id'
        },
        {
            name: 'Buyer Activity',
            value: 'ActivityName'
        },
        {
            name: 'User Name',
            value: 'UserName'
        }
    ];

    $scope.getListData = function () {
        baseService.init($scope.getListUrl, null, null, null, "UserName", "UserName");
        $rootScope.parameters.companyGroupId = $scope.buyerActivityNew.CompanyGroupId;
        $rootScope.parameters.activityType = 'Buyer';
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerActivitys = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerActivity = $scope.buyerActivitys[$scope.index];
        $scope.buyerActivityNew = Object.assign({}, $scope.buyerActivity);
        $scope.Action = 'Update';
    };
    $scope.Save = function () {
        angular.copy($scope.buyerActivityNew, $scope.buyerActivity);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerActivityNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.buyerActivity,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerActivitys.push(response.data.BuyerActivity);
                        $scope.buyerActivitys = $filter('orderBy')($scope.buyerActivitys, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.buyerActivity,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1)
                            $scope.buyerActivitys[$scope.index] = $scope.buyerActivity;
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.buyerActivityNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.buyerActivityNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.buyerActivitys.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.buyerActivity = {};
        $scope.buyerActivityNew = { CompanyGroupId: $scope.buyerActivityNew.CompanyGroupId };
    }
}