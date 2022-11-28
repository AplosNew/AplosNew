'use strict';
UtilityTransactionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UtilityTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Utility Transaction';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/UtilityTransaction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'Update';

    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);

    $scope.ModelTemp = {
        Id: null,
        Date: null,
        UtilityMasterId: null,
        Quantity: 0,
        UoMId: null,
        UoM: null,
        Reading: 0,
        LastReading: 0,
        LastReadingDate: null,
        LastReadingTime:null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.utilityMasterList = [];
    $scope.GetUtilityMasterList = function () {
        $http({
            method: 'GET',
            url: 'Materials/UtilityTransaction/GetUtilityMasterList'
        }).then(function successCallback(response) {
            $scope.utilityMasterList = response.data;
        });
    }
    $scope.GetUtilityMasterList();

    $scope.UoMName = null;
    $scope.GetUoMAndReadingApplicable = function () {
        for (var i = 0; i < $scope.utilityMasterList.length; i++) {
            if ($scope.utilityMasterList[i].Value == $scope.ModelNew.UtilityMasterId) {
                $scope.UoMName = $scope.utilityMasterList[i].UoM;
            }
        }
    }

    //$scope.GetLastReadingList = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Materials/UtilityTransaction/GetEditReadingList?utilityMasterId=' + $scope.ModelNew.UtilityMasterId
    //    }).then(function successCallback(response) {
    //        $scope.LastReading = response.data[0].LastReading;
    //        $scope.LastReadingDate = response.data[0].LastReadingDate;
    //        $scope.LastReadingTime = response.data[0].LastReadingTime;
    //    });
    //}

    $scope.GetEditReadingList = function () {
        $scope.ModelNew.LastReading = 0;
        $scope.ModelNew.LastReadingDate = null;
        $scope.ModelNew.LastReadingTime = null;
        if (!baseService.isUndefinedOrNull($scope.ModelNew.UtilityMasterId)) {
            $http({
                method: 'GET',
                url: 'Materials/UtilityTransaction/GetEditReadingList?utilityMasterId=' + $scope.ModelNew.UtilityMasterId + '&utilityTransactionId=' + $scope.ModelNew.Id
            }).then(function successCallback(response) {
                $scope.ModelNew.LastReading = response.data[0].LastReading;
                $scope.ModelNew.LastReadingDate = response.data[0].LastReadingDate;
                $scope.ModelNew.LastReadingTime = response.data[0].LastReadingTime;
            });
        }
    }


    $scope.ModelNew.LastReading = 0;
    $scope.GetQuantity = function () {
            $scope.ModelNew.Quantity = $scope.ModelNew.Reading - $scope.LastReading;
    }


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetUoMAndReadingApplicable();
        //$scope.GetEditReadingList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            } else {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'data': $scope.ModelNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.Clear = function () {
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.UoMName = null;
        $scope.IsReadingApp = false;
        $scope.Action = 'Save';
    }
}