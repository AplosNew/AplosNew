"use strict";
plantSelectionController.$inject = ["$window", "$scope", "$rootScope", "$routeParams", "$http", "$filter", '$location', '$timeout', '$cookies','SignalRInit'];
function plantSelectionController($window, $scope, $rootScope, $routeParams, $http, $filter, $location, $timeout, $cookies, SignalRInit) {
    $rootScope.title = "Plant::Login";
    $scope.errorText = null;
    $rootScope.ShowHomeButton = false;
    $rootScope.ShowFavouriteMenu = false;
    $rootScope.plantName = null;
    $rootScope.isLeftMenuHide = true;
    $cookies.remove("plantName");
    $scope.HideSideBar = function () {
        angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
        angular.element('.sidebar').toggleClass('tiny-sidebar');
        angular.element('.navbar-site').addClass('navbar-site-full');
        $timeout(function () {
            $rootScope.ShowFavouriteMenu = false;
            angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
        }, 300);
    };
    $scope.HideSideBar();

    $http.get("Securities/UserAccessPlant/GetPlantList/")
        .then(function (response) {
            $scope.plantList = response.data;
            //if ($scope.plantList.length === 1) {
            //    $scope.selectPlant($scope.plantList[0].PlantId, $scope.plantList[0].PlantName);
            //    $cookies.put("plantName", $scope.plantList[0].PlantName);
            //}
        });
    
  

    $scope.selectPlant = function (plantId, plantName) {
        $rootScope.plantName = plantName;

        $rootScope.isLeftMenuHide = false;
        $http({
            method: "POST",
            url: "UPanel/PlantSelection",
            params: {
                "plantId": plantId,
                "plantName": plantName
            }
        }).then(function (response) {
            if (response.data.Error === true) {
                $scope.errorText = response.data.ErrorText || response.data.Message;
            }
            else {
                $scope.clearMsg();
                $window.plantId = plantId;
                $cookies.put("plantId", plantId);
                $cookies.put("plantName", plantName);
                $rootScope.ShowHideSideBar();
                $rootScope.ShowHomeButton = true;
                SignalRInit.connect();
                $location.path("/dashboard");
            }
        }, function (response) {
            $scope.errorText = response.statusText;
        });
    }

    $scope.clearMsg = function () {
        $scope.errorText = null;
    };
}