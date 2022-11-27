'use strict';
UserAccessAppRoleController.$inject = ['cboService', 'baseService', '$rootScope', '$scope', '$routeParams', '$location', '$http', '$filter'];
function UserAccessAppRoleController(cboService, baseService, $rootScope, $scope, $routeParams, $location, $http, $filter) {
    $rootScope.title = "User Access App Role";
    $scope.saveUrl = 'Securities/UserAccessAppRole/Save'
    $scope.Action = 'Save';
    $scope.tableShow = false;


    $scope.ModelTemp = {
        Id: null,
        EmployeeId: null,
        User:null,
        UserId: null,
        RoleId: null,
        CompanyGroupId: null
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    /*$scope.userRoleNew = {
        Id: null,
        RoleId: null,
        UserId: null,
        EmployeeId: null,
        CompanyGroupId: null
    }
    $scope.ModelNew1 = Object.assign({}, $scope.userRoleNew);*/

    $scope.roleList = [];
    $scope.getRole = function () {
        $http.get('Securities/UserAccessAppRole/getRole').
            then(function successCallback(response) {
                $scope.roleList = response.data;
            },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.getRole();


    $scope.UserList = [];
    $scope.getUser = function () {
        $http.get('Securities/UserAccessAppRole/getUser').
            then(function successCallback(response) {
                $scope.UserList = response.data;
            },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    



    $scope.OpenContractorPopUp = function () {
        angular.element(document.querySelector('#UserDetailbyPopUp')).modal('show');
        $scope.getUser();
    }

    $scope.CloseContractorPopUp = function () {
        angular.element(document.querySelector('#UserDetailbyPopUp')).modal('hide');
    }

    $scope.DoubleClickedGetData = function (e) {
        $scope.ModelNew.EmployeeId = e.data.EmployeeId;
        $scope.ModelNew.User = e.data.FullName;
        $scope.ModelNew.UserId = e.data.UserId;
        $scope.CloseContractorPopUp();
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
                'userId': $scope.ModelNew.UserId,
            },
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
        }), function errorCallBack(response) { ShowResult(response.data.Message, 'failure'); }
    };


    
}