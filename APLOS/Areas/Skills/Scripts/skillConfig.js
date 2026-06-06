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
        .when('/skill-dev-master', {
            templateUrl: 'skills/skill/SkillDevelopmentMaster',
            controller: 'SkillDevelopmentMasterController'
        })
        .when('/skill-planning', {
            templateUrl: 'skills/skill/Planning',
            controller: 'skillPlanningController'
        })
        ;
}
SkillConfig.$inject = ['$routeProvider', '$locationProvider'];