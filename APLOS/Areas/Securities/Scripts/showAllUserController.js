'use strict';
showAllUserController.$inject = ['cboService', '$scope', '$rootScope', 'baseService', '$routeParams', '$http'];
function showAllUserController(cboService, $scope, $rootScope, baseService, $routeParams, $http) {
    $rootScope.title = 'All User';

    $scope.companyGroupList = [];
    $scope.model = {
        CompanyGroupId: null
    };

    $scope.searchBy = 'FullName';
    $scope.search = null;

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    //$scope.getData = function (pageno) {
    //    $scope.userList = [];
    //    $http.get('Securities/SystemAdmin/getalluserbycompanygrouplist?companyGroupId=' + $scope.model.CompanyGroupId)
    //        .then(function (response) {
    //            $scope.userList = response.data;
    //        });
    //};

    $rootScope.searchList = [
        {
            'name': 'User Id',
            'value': 'UserId'
        },
        {
            'name': 'Employee Id',
            'value': 'EmployeeId'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'DOB',
            'value': 'DateOfBirth'
        },
        {
            'name': 'Phone',
            'value': 'Phone'
        },
        {
            'name': 'Email',
            'value': 'Email'
        },
        {
            'name': 'Auth Token',
            'value': 'AuthToken'
        },
        {
            'name': 'Phone',
            'value': 'Phone'
        },
        {
            'name': 'Password',
            'value': 'Password'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.getListUrl = 'Securities/SystemAdmin/getalluserbycompanygrouplist/';
    baseService.init($scope.getListUrl, null, 10, null, 'FullName', 'FullName');
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.model.CompanyGroupId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.userList = result.Rows;
                for (var i = 0; i < $scope.userList.length; i++) {

                    $http({
                        method: "get"
                        , url: "Securities/controladmin/decrypttext?decrypttxt=" + encodeURIComponent($scope.userList[i].Password)
                        , dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.userList[i].Password = response.data;
                    });

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.encryptDecrypt = { Decrypt: null };
    $scope.index = -1;
    $scope.DecryptText = function (encrypt, index) {
        $scope.index = index;
        if (!baseService.isUndefinedOrNull(encrypt)) {
            $http({
                method: "get"
                , url: "Securities/controladmin/decrypttext?decrypttxt=" + encodeURIComponent(encrypt)
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.encryptDecrypt.Decrypt = response.data;
            });
            angular.element(document.querySelector('#Popup')).modal('show');
        }
        $scope.index = -1;
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#Popup')).modal('hide');
    };

}