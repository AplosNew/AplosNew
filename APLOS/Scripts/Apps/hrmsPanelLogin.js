'use strict';
HRMSPanelLogin.$inject = ['$scope', '$rootScope', '$routeParams', '$http', '$filter', '$window', 'cboService'];
function HRMSPanelLogin($scope, $rootScope, $routeParams, $http, $filter, $window, cboService) {
    $rootScope.title = 'HRMS::Login';
    $scope.authenticationToken = angular.element(document.querySelector('#authToken')).val();
    $scope.companyGroupId = angular.element(document.querySelector('#groupId')).val();
    $scope.companyGroupName = null;
    $scope.servicepanel = 'uPanel';
    $scope.moduleId = $routeParams.moduleId;
    $scope.timezoneoffset = new Date().getTimezoneOffset();
    $scope.userId = null;
    $scope.password = null;
    $scope.remember = null;
    $scope.companyId = null;
    $scope.companyName = null;
    $scope.plantId = null;
    $scope.plantName = null;
    // Default/alt company group logo.
    $scope.companyGroupLogo = 'images/group-alt.png';
    $scope.errorText = null;
    $scope.errorHide = true;
    $http.get('Organizations/companygroup/getnameandlogo/' + $scope.companyGroupId)
        .then(function (response) {
            if (response.data[0] !== null) {
                $scope.companyGroupName = response.data[0];
            }
            else {
                $scope.companyGroupName = 'Company group name not found!';
            }
            if (response.data[1] !== null) {
                $scope.companyGroupLogo = virtualPath.LogoOrImage + response.data[1];
            }
        });

    $scope.companyList = [];
    $scope.plantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: 'Organizations/plant/getcbobycompany?companyId=' + $scope.companyId
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
            if ($scope.plantList.length == 1)
                $scope.plantId = $scope.plantList[0].Value;
            else $scope.plantId = null;
        });
    };

    cboService.getCompanyGroupCompanyCbo($scope.companyGroupId, function (data) {
        $scope.companyList = data;
        //$scope.companyId = $scope.companyList[0].Value;
    });

    $scope.getCompanyName = function (id) {
        $scope.companyName = $.grep($scope.companyList, function (item) {
            return item.Value === id;
        })[0].Text;
    };

    $scope.getPlantName = function (id) {
        $scope.plantName = $.grep($scope.plantList, function (item) {
            return item.Value === id;
        })[0].Text;
    };

    $scope.Login = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.upanelLoginForm.$valid) {
            $scope.errorHide = true;
            $scope.getCompanyName($scope.companyId);
            $http({
                method: 'POST',
                url: 'account/hrms',
                params: {
                    'timezoneoffset': $scope.timezoneoffset,
                    'authToken': $scope.authenticationToken,
                    'groupId': $scope.companyGroupId,
                    'groupName': $scope.companyGroupName,
                    'userId': $scope.userId,
                    'password': $scope.password,
                    'remember': $scope.remember,
                    'companyId': $scope.companyId,
                    'companyName': $scope.companyName,
                    'plantId': $scope.plantId,
                    'plantName': $scope.plantName,
                    'moduleId': $scope.moduleId
                }
            }).then(function (response) {
                if (response.data.Error === true || response.data.Status === 'Fail') {
                    $scope.errorHide = false;
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                }
                else {
                    if (response.data.Status === 'Success') {
                        $window.location = 'http://' + response.data.Url
                            + '?id=' + response.data.Id
                            + '&userId=' + response.data.UserId
                            + '&userName=' + response.data.UserFullName
                            + '&groupId=' + $scope.companyGroupId
                            + '&groupName=' + $scope.companyGroupName
                            + '&companyId=' + $scope.companyId
                            + '&companyName=' + $scope.companyName
                            + '&plantId=' + $scope.plantId
                            + '&plantName=' + $scope.plantName
                            + '&controlAdmin=' + response.data.IsControlAdmin
                            + '&sysAdmin=' + response.data.IsSysAdmin
                            + '&powerUser=' + response.data.IsPowerUser
                            + '&employeeId=' + response.data.EmployeeId
                            + '&authenticationToken=' + $scope.authenticationToken;
                    }
                }
            }, function (response) {
                $scope.errorHide = false;
                $scope.errorText = response.statusText;
            });
        }
    };
}