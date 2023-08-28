"use strict";
upanelLoginController.$inject = ["$scope", "$rootScope", "$routeParams", "$http", "$filter", "$window", "$cookies", "cboService"];
function upanelLoginController($scope, $rootScope, $routeParams, $http, $filter, $window, $cookies, cboService) {
    $rootScope.title = "Application::Login";
    $scope.authenticationToken = angular.element(document.querySelector('#authToken')).val();
    $scope.companyGroupId = angular.element(document.querySelector('#groupId')).val();
    $scope.companyGroupName = null;
    $scope.servicepanel = "uPanel";
    $scope.returnUrl = $routeParams.returnUrl;
    $scope.timezoneoffset = new Date().getTimezoneOffset();
    $scope.userId = null;
    $scope.password = null;
    $scope.remember = null;
    $scope.companyId = null;
    $scope.companyName = null;
    $scope.companyGroupLogo = "images/group-alt.png";
    $scope.errorText = null;

    $http.get("Organizations/companygroup/getnameandlogo/" + $scope.companyGroupId)
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
    $scope.companyList = [];

    cboService.getCompanyGroupCompanyCbo($scope.companyGroupId, function (result) {
        $scope.companyList = result;
        $scope.companyId = $scope.companyList.length === 1 ? $scope.companyList[0].Value : null;
    });

    $scope.getCompanyName = function (id) {
        $scope.companyName = $.grep($scope.companyList, function (item) {
            return item.Value === id;
        })[0].Text;
    };

    $scope.Login = function () {
        $cookies.put("CompanyFullName", null);
        $cookies.put("CompanyImage", null);
        $scope.errorText = null;
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.upanelLoginForm.$valid) {
            $scope.getCompanyName($scope.companyId);
            //if (!navigator.onLine)
            //    return $scope.errorText = "No internet connection.Please check your internet connection.";
            $http({
                method: "POST",
                url: "UPanel/login",
                params: {
                    "timezoneoffset": $scope.timezoneoffset,
                    "authToken": $scope.authenticationToken,
                    "groupId": $scope.companyGroupId,
                    "groupName": $scope.companyGroupName,
                    "userId": $scope.userId,
                    "password": $scope.password,
                    "remember": $scope.remember,
                    "companyId": $scope.companyId,
                    "companyName": $scope.companyName
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true || response.data.Status === "Fail") {
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                }
                else {
                    if (response.data.Status === "Success") {
                        $cookies.put("panel", "upanel");
                        $cookies.put("authToken", $scope.authenticationToken);
                        $cookies.put("employeeId", response.data.EmployeeId);
                        $cookies.put("FullName", response.data.UserFullName);
                        $cookies.put("groupId", $scope.companyGroupId);
                        $cookies.put("companyId", $scope.companyId);

                        $cookies.put("CompanyFullName", response.data.CompanyFullName);
                        $cookies.put("CompanyImage", response.data.CompanyImage);

                        if (response.data.Image === null) {
                            $cookies.put("userImage", "altUser.jpg");
                        } else
                            $cookies.put("userImage", response.data.Image);

                       
                        $window.location = "applicationpanel";
                      
                    }
                }
            }, function errorCallback(response) {
                $scope.errorText = response.statusText;
            });
        }
        return true;
    };

    $scope.clearMsg = function () {
        $scope.errorText = null;
    };
}