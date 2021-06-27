TaskSchedulerConfig.$inject = ['$routeProvider', '$locationProvider'];
function TaskSchedulerConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/task-schedule', {
            templateUrl: 'TaskScheduler/TaskSchedule/Aplos',
            controller: 'taskScheduleController'
        })

        ;
}