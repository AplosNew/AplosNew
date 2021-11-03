'use strict';
LSDController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LSDController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "LSD";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.lsds = [];
    $scope.path = 'OrderManagements/lsd/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'BuyerName', 'BuyerName');
    $scope.getData = function (pageno) {
        $rootScope.parameters.buyerId = $scope.lsdNew.BuyerId;
        $scope.lsd = {
            Id: null,
            BuyerId: null,
            BuyerName: null,
            ShipModeId: null,
            ShipModeName: null,
            OrderLeadTime: null,
            ProductionLeadTime: null,
            FinishingLeadTime: null,
            ExFactoryLeadTime: null,
            MainRawMaterialInhouseLeadTime: null,
            OtherRawMaterialInhouseLeadTime: null,
            Weekend: null
        };
        $scope.lsdNew = Object.assign({}, $scope.lsd);
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.lsds = result.Rows;
                $scope.lsdNew.BuyerId = $rootScope.parameters.buyerId;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.lsd = {
        Id: null,
        BuyerId: null,
        BuyerName: null,
        ShipModeId: null,
        ShipModeName: null,
        OrderLeadTime: null,
        ProductionLeadTime: null,
        FinishingLeadTime: null,
        ExFactoryLeadTime: null,
        MainRawMaterialInhouseLeadTime: null,
        OtherRawMaterialInhouseLeadTime: null,
        Weekend: null
    };
    $scope.lsdNew = Object.assign({}, $scope.lsd);
    $scope.searchByList = [
        {
            'name': 'Buyer',
            'value': 'BuyerName'
        },
        {
            'name': 'ShipMode',
            'value': 'ShipModeName'
        }
    ];
    // #region DDL

    $scope.buyerList = [];
    $http({
        method: 'GET',
        url: 'Parties/buyer/getcbo'
    }).then(function (response) {
        $scope.buyerList = response.data;
    });

    $scope.shipModeList = [];
    $http({
        method: 'GET',
        url: 'OrderManagements/shipmode/getcbo'
    }).then(function (response) {
        $scope.shipModeList = response.data;
    });

    $scope.dayList = [
        {
            'Text': 'Saturday',
            'Value': 'Saturday'
        },
        {
            'Text': 'Sunday',
            'Value': 'Sunday'
        },
        {
            'Text': 'Monday',
            'Value': 'Monday'
        },
        {
            'Text': 'Tuesday',
            'Value': 'Tuesday'
        },
        {
            'Text': 'Wednesday',
            'Value': 'Wednesday'
        },
        {
            'Text': 'Thursday',
            'Value': 'Thursday'
        },
        {
            'Text': 'Friday',
            'Value': 'Friday'
        }
    ]

    // #endregion

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.lsd = $scope.lsds[$scope.index];
        $scope.lsdNew = Object.assign({}, $scope.lsd);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.lsdNewForm.$valid) {
            angular.copy($scope.lsdNew, $scope.lsd);
            $scope.shipModeName = document.getElementById("shipModeId").options[document.getElementById('shipModeId').selectedIndex].text;
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.lsd,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.lsd = response.data.LSD;
                        $scope.lsd.ShipModeName = $scope.shipModeName;
                        $scope.lsds.push($scope.lsd);
                        $scope.lsds = $filter('orderBy')($scope.lsds, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.lsd,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.lsd.ShipModeName = $scope.shipModeName;
                            $scope.lsds[$scope.index] = $scope.lsd;
                            $scope.lsds = $filter('orderBy')($scope.lsds, 'Sequence');
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.lsdNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.lsdNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.lsds.splice($scope.index, 1);
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
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.lsd = {};
        $scope.lsdNew = { BuyerId: $scope.lsdNew.BuyerId };
    }
}
