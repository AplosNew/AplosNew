misConfig.$inject = ["$routeProvider"];
function misConfig($routeProvider) {
    $routeProvider
        .when("/at-a-glance", {
            templateUrl: "MIS/ManagementInformationSystem/Aplos",
            controller: "atAGlanceBIController"
        })
        .when("/account-status-bi", {
            templateUrl: "MIS/ManagementInformationSystem/AccountStatusBI",
            controller: "accountStatusBIController"
        })
        .when("/order-status-bi", {
            templateUrl: "MIS/ManagementInformationSystem/OrderStatusBI",
            controller: "orderStatusBIController"
        })
        .when("/manpower-status-bi", {
            templateUrl: "MIS/ManagementInformationSystem/ManpowerStatusBI",
            controller: "manpowerStatusBIController"
        })
        ;
} 