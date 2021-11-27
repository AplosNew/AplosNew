"use strict";
upanelDashboardController.$inject = ["$scope", "$rootScope", "$routeParams", "$http"];
function upanelDashboardController($scope, $rootScope, $routeParams, $http) {
    //$rootScope.showMenu = "Module";
    $rootScope.menuModuleId = null;
    //$rootScope.isLeftMenuHide = false;
    $scope.holidayList = [];

    function getHolidayList(plantId) { 
        $http({
            method: "GET",
            url: "Setups/OffDayMaster/GetGovHolidayList"
        }).then(function (response) {
            $scope.holidayList = response.data;
        });
        $rootScope.SelectedHref = '';
        $rootScope.GetShowHideFavouriteMenu();
    };
    getHolidayList();
}