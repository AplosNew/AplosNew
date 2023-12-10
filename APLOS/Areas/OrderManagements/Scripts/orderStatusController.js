'use strict';
OrderStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function OrderStatusController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = "Order Status";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.ModelList = [];
    $scope.path = 'OrderManagements/OrderStatus/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Sequence', name: "Sequence" }, { value: 'Id', name: "Order Status" }, { value: 'UserName', name: "User Name" }];

    //$scope.getData = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "GetList",
    //        data: { column: $scope.searchBy, value: $scope.search },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.ModelList = response.data;
    //    });
    //}
    //$scope.getData();

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Sequence', name: "Sequence" }, { value: 'Id', name: "Order Status" }, { value: 'UserName', name: "User Name" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null
        , Sequence: null
        , UserName: null
        , MasterPlanApplicable: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.ModelNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.OrderStatusEnumList = [];
    cboService.getEnumCbo("enum/GetOrderStatusEnumCbo", function (result) {
        $scope.OrderStatusEnumList = result;
    });

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            angular.copy($scope.ModelNew, $scope.ModelTemp);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: $scope.ModelTemp
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                        $scope.getData();
                        $scope.GetSequence();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: $scope.ModelTemp
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                        $scope.getData();
                        $scope.GetSequence();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
   

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.ModelNew.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.modelList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    $scope.GetSequence();
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
        $scope.model = {};
        $scope.ModelNew = { Sequence: seq };
    }
}