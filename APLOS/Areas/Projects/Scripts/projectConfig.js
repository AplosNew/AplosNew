function ProjectConfig($routeProvider, $locationProvider) {
    $routeProvider
     .when('/project-planning-category', {
         templateUrl: 'Projects/projectplanningcategory/aplos',
         controller: 'projectPlanningCategoryController'
     })
    .when('/project-planning-sub-category', {
        templateUrl: 'Projects/projectplanningsubcategory/aplos',
        controller: 'projectPlanningSubCategoryController'
    })
        .when('/project-planning', {
            templateUrl: 'Projects/projectplanning/aplos',
            controller: 'projectPlanningController'
        })
        .when('/project-planning-purchase-order', {
            templateUrl: 'Projects/projectPlanningpurchaseorder',
            controller: 'projectPlanningPurchaseOrderController'
        })
        .when('/project-planning-requisition', {
            templateUrl: 'Projects/projectPlanningRequisition',
            controller: 'projectPlanningRequisitionController'
        })
}
ProjectConfig.$inject = ['$routeProvider', '$locationProvider'];