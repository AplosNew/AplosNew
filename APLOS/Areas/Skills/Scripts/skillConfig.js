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
        .when('/skill-upload', {
            templateUrl: 'skills/skill/SkillUpload',
            controller: 'skillUploadController'
        })
        ;
}
SkillConfig.$inject = ['$routeProvider', '$locationProvider'];