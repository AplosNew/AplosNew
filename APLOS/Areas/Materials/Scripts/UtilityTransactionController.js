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
    $scope.CalculatedValue = 0;

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
        LastReadingTime: null,
        MultiplyingFactor: null,
        Remarks: null,
        UoM:null,
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

    $scope.searchbyUtilityTransactionlist = [
        {
            'name': 'Utility Master',
            'value': 'UtilityMaster'
        },
        {
            'name': 'Utility Group',
            'value': 'UtilityGroup'
        },
        {
            'name': 'Utility SubGroup',
            'value': 'UtilitySubGroup'
        },
        {
            'name': 'Utility Category',
            'value': 'UtilityCategory'
        },
        {
            'name': 'Utility SubCategory',
            'value': 'UtilitySubCategory'
        },
        {
            'name': 'Item',
            'value': 'Item'
        }
    ];

    $scope.searchByUtility = "UtilityMaster"; $scope.searchUtility = "";
    
    $scope.valueData = '';
    $scope.utilityMasterList = [];
 
    $scope.getUtilityTransactionPopUpData = function () {
        $scope.UtilityUrl = 'Materials/UtilityTransaction/GetUtilityMasterList';
        $http({
            method: 'POST',
            url: $scope.UtilityUrl,
            data: { column: $scope.searchByUtility, value: $scope.searchUtility },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.utilityMasterList = response.data;
        });
        angular.element(document.querySelector('#UtilityMasterpopUpId')).modal('show');
    };


    $scope.closePopUp = function () {
        angular.element(document.querySelector('#UtilityMasterpopUpId')).modal('hide');
        $scope.searchUtility = '';
    }

    $scope.selectDoubleClick = function (obj) {
        $scope.ModelNew.UtilityMaster = obj.data.UtilityMaster;
        $scope.ModelNew.UtilityMasterId = obj.data.UtilityMasterId;
        $scope.ModelNew.MultiplyingFactor = obj.data.MultiplyingFactor;
        $scope.ModelNew.UoMId = obj.data.UoMId;
        $scope.ModelNew.UoM = obj.data.UoM;

        $scope.GetEditReadingList();
        $scope.GetUoMAndReadingApplicable();
        angular.element(document.querySelector('#UtilityMasterpopUpId')).modal('hide');
        $scope.searchUtility = '';
    }

     
    $scope.UoMName = null;
    $scope.GetUoMAndReadingApplicable = function () {
        for (var i = 0; i < $scope.utilityMasterList.length; i++) {
            if ($scope.utilityMasterList[i].Value == $scope.ModelNew.UtilityMasterId) {
                $scope.UoMName = $scope.utilityMasterList[i].UoM;
            }
        }
    }
     
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
                //$scope.ModelNew.MultiplyingFactor = response.data[0].MultiplyingFactor;
            });
        }
    }

    $scope.GetQuantity = function () {
        $scope.ModelNew.Quantity = $scope.ModelNew.Reading - $scope.ModelNew.LastReading;
    }

    $scope.GetCalculatedValue = function () {
        $http({
            method: 'GET',
            url: 'Materials/UtilityTransaction/GetCalculatedValue?utilityMasterId=' + $scope.ModelNew.UtilityMasterId
        }).then(function successCallback(response) {
            /*$scope.ModelNew.MultiplyingFactor = response.data[0].MultiplyingFactor;*/
            $scope.CalculatedValue = parseFloat($scope.ModelNew.Quantity * $scope.ModelNew.MultiplyingFactor).toFixed(4);
        });
    }


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetUoMAndReadingApplicable();
        $scope.GetCalculatedValue();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.Save = function () {
        //$scope.$broadcast('show-errors-check-validity');
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
        $scope.UtilityMaster = [];
        $scope.UoMName = null;
        $scope.CalculatedValue = 0;
        $scope.IsReadingApp = false;
        $scope.Action = 'Save';
    }
}