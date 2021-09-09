'use strict';
CreditLimitOpeningController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CreditLimitOpeningController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Credit Limit Opening';
    $scope.ModelList = [];
    $scope.path = 'humanresource/CreditLimitOpening/';
    $scope.saveUrl = $scope.path + 'create';

    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetData",
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;

        });
    }
    $scope.getData();

    $scope.Save = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.ModelList.length; i++) {
            if (
                $scope.ModelList[i].DailyLimit != $scope.ModelList[i].OriginalDayLimit
                || $scope.ModelList[i].MonthlyLimit != $scope.ModelList[i].OriginalMonthlyLimit
               ) {
                DataToBeSaved.push($scope.ModelList[i]);

            }
        }

        $http({
                method: 'POST',
                url: $scope.saveUrl,
            data: { 'data': DataToBeSaved },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
       
        }
    };       
   
}