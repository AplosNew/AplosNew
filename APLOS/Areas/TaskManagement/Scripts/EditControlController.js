"use strict";
EditControlController.$inject = ["$scope", "$rootScope", "$routeParams", "$http", "$filter", "$window", "$cookies", "cboService"];
function EditControlController($scope, $rootScope, $routeParams, $http, $filter, $window, $cookies, cboService) {
    $rootScope.title = "Login";
    $scope.authenticationToken = angular.element(document.querySelector('#authToken')).val();
    $scope.companyGroupId = angular.element(document.querySelector('#groupId')).val();
    $scope.companyGroupName = null;
    $scope.servicepanel = "uPanel";
    $scope.returnUrl = $routeParams.returnUrl;   
    $scope.userId = null;
    $scope.password = null;
    $scope.remember = null;  
    $scope.errorText = null;
    $scope.companyId = null;
    $scope.companyName = null;
    $scope.companyGroupLogo = "images/group-alt.png";
    $rootScope.isLeftMenuHide = true;

    $rootScope.ShowFavouriteMenu = false;
    $scope.HideSideBar = function () {
        angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
        angular.element('.sidebar').toggleClass('tiny-sidebar');
            $rootScope.ShowFavouriteMenu = false;

        angular.element('.navbar-site').addClass('navbar-site-full');
        angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
        //$timeout(function () {
        //    $rootScope.ShowFavouriteMenu = false;
        //    angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
        //}, 300);
    };
    $scope.HideSideBar();

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
        $scope.errorText = null;
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.loginForm.$valid) {
           
            $http({
                method: "POST",
                url: "EditControl/login",
                params: {
                     "authToken": $scope.authenticationToken,
                    "userId": $scope.userId,
                    "password": $scope.password,
                    "remember": $scope.remember,
                    "groupId": $scope.companyGroupId,
                    "groupName": $scope.companyGroupName,
                    "companyId": $scope.companyId,
                    "companyName": $scope.companyName
                }
            }).then(function successCallback(response) {
                if (response.data.result.ErrorText === "Invalid username or password!" || response.data.result.Status === "Fail") {
                    $scope.errorText = response.data.result.ErrorText;
                }
                else {
                    if (response.data.result.Status === "Success") {
                        $cookies.put("panel", "upanel");
                        $cookies.put("authToken", $scope.authenticationToken);
                        $cookies.put("employeeId", response.data.UserId);

                        $cookies.put("groupId", $scope.companyGroupId);
                        $cookies.put("companyId", $scope.companyId);

                        $cookies.put("CompanyFullName", response.data.CompanyFullName);

                        //$window.location = "applicationpanel#!/task-manag-report";
                        $window.location = 'ppanel#!/teacher-schedule';
                    }
                }
            }, function errorCallback(response) {
                $scope.errorText = response.data.result.ErrorText;
            });
        }
        return true;
    };

    $scope.clearMsg = function () {
        $scope.errorText = null;
    };
}