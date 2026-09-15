'use strict';
CartonUpdateController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function CartonUpdateController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Carton Update";
    $scope.Action = 'Save';

    $scope.CartonList = [];
    $scope.CartId = null;
    $scope.GetCartonList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CartId)) {
                throw "Carton No is required.";
            }
            else {
                $http({
                    method: 'Get',
                    url: 'OrderManagements/ProductionOrder/GetCartons?Id=' + $scope.CartId
                }).then(function successCallback(response) {
                    $scope.CartonList = response.data;
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Save= function () {
        try {
            if ($scope.CartonList.length > 0) {
                var tempList = [];

                for (var i = 0; i < $scope.CartonList.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.CartonList[i].ActualQty) || $scope.CartonList[i].ActualQty > 0) {
                        if (baseService.isUndefinedOrNull($scope.CartonList[i].Id)) {
                            $scope.CartonList[i].Id = -(Math.floor(Math.random() * 100) + 1);
                        }
                        tempList.push($scope.CartonList[i]);
                    }
                }
                $http({
                    method: 'POST',
                    url: "OrderManagements/ProductionOrder/SaveCartonQty",
                    data: { 'data': tempList[0] },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetCartonList();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



}

