'use strict';
UserAccessAppRoleController.$inject = ['cboService', 'baseService', '$rootScope', '$scope', '$routeParams', '$location', '$http', '$filter'];
function UserAccessAppRoleController(cboService, baseService, $rootScope, $scope, $routeParams, $location, $http, $filter) {
    $rootScope.title = "User Access App Role";
    $scope.saveUrl = 'Securities/UserAccessAppRole/Save'
    $scope.Action = 'Save';
    $scope.tableShow = false;

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        $scope.getRole();
        
    }

    $scope.ModelTemp = {
        Id: null,
        EmployeeId: null,
        User:null,
        UserId: null,
        RoleId: null,
        CompanyGroupId: null
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    

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
        $scope.ModelNew.Id = e.data.Id;
        $scope.ModelNew.EmployeeId = e.data.EmployeeId;
        $scope.ModelNew.User = e.data.FullName;
        $scope.ModelNew.UserId = e.data.UserId;
        
        $scope.GetUserAccessedIcon();
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
                $scope.Action = 'Save';
                //ClearFields(response.data.Sequence);
                $scope.GetUserAccessedIcon();
                $scope.getData();

            }
        }), function errorCallBack(response) { ShowResult(response.data.Message, 'failure'); }
    };

    $scope.AccessedIcon = [];
    $scope.GetUserAccessedIcon = function () {
        $http.get('Securities/UserAccessAppRole/GetUserAccessedIcon?employeeId=' + $scope.ModelNew.EmployeeId).
            then(function successCallback(response) {
                $scope.AccessedIcon = response.data;
            },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }


    $scope.Delete = function (x) {
        
            $http({
                method: 'POST',
                url: 'Securities/UserAccessAppRole/Delete',
                data: { 'id': x.data.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        
    };

    $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
    $scope.Remove = function (x) {
        
        if (!baseService.isUndefinedOrNull(x.data.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUp')).modal('show');
    }
    
}