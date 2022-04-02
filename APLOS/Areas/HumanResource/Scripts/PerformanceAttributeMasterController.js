'use strict';
PerformanceAttributeMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PerformanceAttributeMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Performance Attribute Master';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/PerformanceAttributeMaster/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteUrl = $scope.path + 'Delete/';
   
    $scope.ModelTemp = {
        Id: null,
        AttCategory: null,
        Category: null,
        Defination: null,
        Remarks: null,
    };

    
    //-------------------------------   SAVE

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    // --------------------------   DELETE
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
                    ClearFields(response.data.Sequence);
                    
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = {
            Id: null,
            AttCategory: null,
            Category: null,
            Defination: null,
            Remarks: null,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
   
}