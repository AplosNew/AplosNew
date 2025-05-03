function SkillConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/skill-category', {
            templateUrl: 'skills/skillcategory/aplos',
            controller: 'skillCategoryController'
        })
        .when('/skill', {
            templateUrl: 'skills/skill/aplos',
            controller: 'skillController'
        })
        
        ;
}
SkillConfig.$inject = ['$routeProvider', '$locationProvider'];