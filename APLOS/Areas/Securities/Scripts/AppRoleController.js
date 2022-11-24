'use strict';
AppRoleController.$inject = ['cboService', 'baseService', '$rootScope', '$scope', '$routeParams', '$location', '$http', '$filter'];
function AppRoleController(cboService, baseService, $rootScope, $scope, $routeParams, $location, $http, $filter) {
    $rootScope.title = "App Role";
    $scope.tableShow = false;
    $scope.saveUrl = 'Securities/AppRole/Save'
    $scope.deleteUrl = 'Securities/AppRole/Delete'
    $scope.Action = 'Save';

    $scope.ModelTemp = {
        Id: null,
        Name:  null,
        Remarks: null,
        Active: true,
        CompanyGroupId: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
            {
                ShowResult(response.data.Message, 'failure');
            }
            else
            {
                ShowResult(response.data.Message, 'success');
                ClearFields(response.data.Sequence);
                $scope.getData();

            }
        }), function errorCallBack(response) { ShowResult(response.data.Message, 'failure'); }
    };
    $scope.ModelList = [];
    $scope.Getlist = function () {
        $http.get('Securities/AppRole/Getlist')
            .then(function successCallback(response) {
                $scope.ModelList = response.data;
            });
    }
    $scope.Getlist();


    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null,
            Name: null,
            Remarks: null,
            Active: true,
            CompanyGroupId: null
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: 'Securities/AppRole/Delete?id='+ $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
}