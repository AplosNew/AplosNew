'use strict';
productionBookingPeriodController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function productionBookingPeriodController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Booking Period";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.ProductionBookingPeriods = [];
    $scope.path = 'Productions/productionBookingPeriod/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.ProductionBookingPeriods = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.productionBookingPeriod = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        StartTime:null,
        EndTime:null,
        PeriodType:'GeneralHour'
    };
    $scope.productionBookingPeriodNew = Object.assign({}, $scope.productionBookingPeriod);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.productionBookingPeriodNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.productionBookingPeriod = $scope.ProductionBookingPeriods[$scope.index];
        $scope.productionBookingPeriodNew = Object.assign({}, $scope.productionBookingPeriod);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.productionBookingPeriodNew, $scope.productionBookingPeriod);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.productionBookingPeriodNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.productionBookingPeriod,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ProductionBookingPeriods.push(response.data.ProductionBookingPeriod);
                        $scope.ProductionBookingPeriods = $filter('orderBy')($scope.ProductionBookingPeriods, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.productionBookingPeriod,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.ProductionBookingPeriods[$scope.index] = $scope.productionBookingPeriod;
                            $scope.ProductionBookingPeriods = $filter('orderBy')($scope.ProductionBookingPeriods, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.productionBookingPeriodNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.productionBookingPeriodNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ProductionBookingPeriods.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    }
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.productionBookingPeriod = {};
        $scope.productionBookingPeriodNew = {};
        $scope.productionBookingPeriodNew.Sequence = seq;
        $scope.productionBookingPeriodNew.Active = true;
        $scope.productionBookingPeriodNew.PeriodType = 'GeneralHour';
    }
}