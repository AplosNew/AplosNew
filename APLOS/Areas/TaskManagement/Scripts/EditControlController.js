/// <reference path="../angular-constant-path.js" />
'use strict';
EditControlController.$inject = ['$scope', '$rootScope', '$routeParams', '$http', '$filter', '$window', '$cookies'];
function EditControlController($scope, $rootScope, $routeParams, $http, $filter, $window, $cookies) {
    $rootScope.title = 'Portal::Login';
    $scope.servicepanel = 'portal';
    $scope.returnUrl = $routeParams.returnUrl;
    $scope.timezoneoffset = new Date().getTimezoneOffset();
    $scope.employeeId = null;
    $scope.remember = null;
    $scope.errorText = null;
    $scope.employeeName = null;
    $scope.companyGroupLogo = 'organization-alt.png';
    $scope.Login = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.loginForm.$valid) {
            $scope.errorText = null;
            if (!Number.isInteger(Number($scope.employeeId))) return $scope.errorText = 'Invalid employee id or pin.';
            if (!Number.isInteger(Number($scope.password))) return $scope.errorText = 'Invalid employee id or pin.';

            if (!navigator.onLine)
                return $scope.errorText = 'No internet connection. Please check your internet connection.';
            $http({
                method: 'POST',
                url: 'MyApp/Login',
                params: {
                    'timezoneoffset': $scope.timezoneoffset
                    , 'employeeId': $scope.employeeId
                    , 'password': $scope.password
                    , 'remember': $scope.remember
                }
            }).then(function successCallback(response) {
                if (response.data.result.Error === true || response.data.result.Status === 'Fail') {
                    $scope.errorText = response.data.result.ErrorText || response.data.result.Message;
                }
                else {
                    if (response.data.result.Status === 'Success') {
                        $cookies.put('MyApppanel', 'epanel');
                        $cookies.put("MyAppemployeeId", response.data.result.EmployeeId);
                        $cookies.put("MyAppgroupId", response.data.result.CompanyGroupId);
                        $cookies.put("MyAppcompanyId", response.data.result.CompanyId);
                        $cookies.put("MyAppplantId", response.data.result.PlantId);
                        $cookies.put("MyAppplantName", response.data.result.PlantName);
                        $cookies.put("MyAppemployeeName", response.data.result.EmployeeName);

                        $cookies.put("MyAppuserImage", response.data.profile.EmpPicPath);
                        $window.location = 'epanel#!/task-list';
                    }
                }
            }, function errorCallback(response) {
                $scope.errorText = response.statusText;
            });
        }
    };

    $scope.clearMsg = function () {
        $scope.errorText = null;
    };

    $scope.companyGroupName = null;
    $scope.companyGroupLogo = "images/group-alt.png";
    $http.get("Organizations/companygroup/GetNameAndLogoDefault")
        .then(function (response) {
            if (response.data[0] !== null) {
                $scope.companyGroupName = response.data[0];
            }
            else {
                $scope.companyGroupName = "Company group name not found!";
            }
            if (response.data[1] !== null) {
                $scope.companyGroupLogo = virtualPath.LogoOrImage + response.data[1];
                $cookies.put("gImage", response.data[1]);
            }
        });

}