'use strict';
UnitOfMeasurementController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function UnitOfMeasurementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.unitOfMeasurements = [];
    $scope.getListUrl = 'Setups/unitofmeasurement/getunitofmeasurementlist';

    $scope.getDataByUomD = function () {
        baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
        $scope.getData = function (pageno) {
            $rootScope.parameters.UOMDId = $scope.unitOfMeasurementNew.UOMDId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.unitOfMeasurements = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        $scope.GetSequence();
    }

    $scope.unitOfMeasurement = {
        Id: null,
        UOMDId: null,
        UOMDimension: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        IsComercialUnit: true,
        IsLengthUnit: false,
        IsWidthUnit: false,
        Active: true,
        Archive: false
    };

    $scope.unitOfMeasurementNew = Object.assign({}, $scope.unitOfMeasurement);

    $scope.uOMDIdList = [];
    $http({
        method: 'GET',
        url: 'Setups/uomdimension/getuomdimensioncbo/'
    }).then(function successCallback(response) {
        $scope.uOMDIdList = response.data;
    });

    $scope.onUOMDChange = function (item) {
        $http({
            method: 'GET',
            url: 'Setups/unitofmeasurement/getunitofmeasurementlist?UOMDId=' + item
        }).then(function successCallback(response) {
            $scope.unitOfMeasurements = response.data.Rows;
        });
    }

    $scope.GetSequence = function () {
        $http.get("Setups/unitofmeasurement/getautosequence?uomDId=" + $scope.unitOfMeasurement.UOMDId)
            .then(function (response) {
                $scope.unitOfMeasurementNew.Sequence = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.unitOfMeasurement = $scope.unitOfMeasurements[$scope.index];
        $scope.unitOfMeasurementNew = Object.assign({}, $scope.unitOfMeasurement);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.unitOfMeasurementNewForm.$valid) {
            angular.copy($scope.unitOfMeasurementNew, $scope.unitOfMeasurement);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Setups/unitofmeasurement/create",
                    data: $scope.unitOfMeasurement,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.unitOfMeasurements.push(response.data.UnitOfMeasurement);
                        $scope.unitOfMeasurements = $filter('orderBy')($scope.unitOfMeasurements, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: "Setups/unitofmeasurement/edit",
                    data: $scope.unitOfMeasurement,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.unitOfMeasurements[$scope.index] = $scope.unitOfMeasurement;
                            $scope.unitOfMeasurements = $filter('orderBy')($scope.unitOfMeasurements, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.unitOfMeasurementNew.Id)) {
            $http({
                method: 'POST',
                url: "Setups/unitofmeasurement/delete/" + $scope.unitOfMeasurementNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.unitOfMeasurements.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.unitOfMeasurement = {};
        $scope.unitOfMeasurementNew = { UOMDId: $scope.unitOfMeasurementNew.UOMDId };
        $scope.unitOfMeasurementNew.Sequence = seq;
        $scope.unitOfMeasurementNew.Active = true;
        $scope.unitOfMeasurementNew.IsComercialUnit = true;
    }
}