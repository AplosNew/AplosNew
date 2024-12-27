misConfig.$inject = ["$routeProvider"];
function misConfig($routeProvider) {
    $routeProvider
        .when("/at-a-glance", {
            templateUrl: "MIS/ManagementInformationSystem/Aplos",
            controller: "atAGlanceController"
        })
        ;
} 