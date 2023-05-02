/// <reference path="../../../scripts/angular-cbo-factory.js" />
Packing.$inject = ['$routeProvider', '$locationProvider'];
function PackingConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/web-packing', {
            templateUrl: 'packing/WebBasedPacking/Aplos',
            controller: 'WebBasedPackingController'
        })
}