'use strict';
CreditLimitOpeningController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CreditLimitOpeningController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Credit Limit Opening';
    $scope.ModelList = [];
    $scope.path = 'humanresource/CreditLimitOpening/';
    $scope.saveUrl = $scope.path + 'create';
    
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetData",
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields();           
        });
    }
    $scope.getData();

    //$scope.ModelTemp = {
    //    Id: null,       
    //    DailyLimit: null,
    //    MonthlyLimit: null,
    //    DesignationId: null      
    //};
    //$scope.ModelNew = Object.assign({}, $scope.ModelTemp);
     
    $scope.Save = function () {

        $http({
                method: 'POST',
                url: $scope.saveUrl,
            data: { 'data': $scope.ModelList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
       
        }
    };
        
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {       
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);       
    }
}