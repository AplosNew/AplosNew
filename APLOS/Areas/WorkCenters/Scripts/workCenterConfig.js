WorkCenterConfig.$inject = ['$routeProvider', '$locationProvider'];
function WorkCenterConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/work-center-category', {
            templateUrl: 'WorkCenters/workcentercategory/aplos',
            controller: 'workCenterCategoryController'
        })
        .when('/work-center-sub-category', {
            templateUrl: 'WorkCenters/workcentersubcategory/aplos',
            controller: 'workCenterSubCategoryController'
        })
        .when('/workcenterbuyer', {
            templateUrl: 'WorkCenters/workcenterbuyertag/aplos',
            controller: 'workCenterBuyerTagController'
        })
        .when('/work-center-master', {
            templateUrl: 'WorkCenters/workcentermaster/aplos',
            controller: 'workCenterMasterController'
        })
        .when('/work-station-daily', {
            templateUrl: 'WorkCenters/workstationdaily/aplos',
            controller: 'workStationDailyController'
        })
        .when('/work-center-group', {
            templateUrl: 'WorkCenters/WorkCenterMaster/WCGroup',
            controller: 'workCenterGroupController'
        })
        ;
}