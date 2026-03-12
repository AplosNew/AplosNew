TaskManagementConfig.$inject = ['$routeProvider', '$locationProvider'];
function TaskManagementConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/task-master-creation', {
            templateUrl: 'TaskManagement/TaskMasterCreation/',
            controller: 'TaskMasterCreationController'
        }).when('/task-category-tna', {
            templateUrl: 'TaskManagement/TaskCategory/',
            controller: 'TaskCategoryController'
        })
        .when('/task-manager-dashboard', {
            templateUrl: 'TaskManagement/TaskManagerDashboard/',
            controller: 'taskManagerDashboardController'
        })

        .when('/task-category-issue', {
            templateUrl: 'TaskManagement/TaskCategory/',
            controller: 'TaskCategoryIssueController'
        }).when('/task-category-todo', {
            templateUrl: 'TaskManagement/TaskCategory/',
            controller: 'TaskCategoryToDoController'
        }).when('/task-subcategory-tna', {
            templateUrl: 'TaskManagement/TaskSubCategory/',
            controller: 'TaskSubCategoryController'
        }).when('/task-subcategory-issue', {
            templateUrl: 'TaskManagement/TaskSubCategory/',
            controller: 'TaskSubCategoryIssueController'
        }).when('/task-subcategory-todo', {
            templateUrl: 'TaskManagement/TaskSubCategory/',
            controller: 'TaskSubCategoryToDoController'
        }).when('/task-template', {
            templateUrl: 'TaskManagement/TaskTemplate/',
            controller: 'TaskTemplateController'
        }).when('/task-applied-on', {
            templateUrl: 'TaskManagement/TaskAppliedOn/',
            controller: 'TaskAppliedOnController'
        })
        .when('/task-dependent-dates', {
            templateUrl: 'TaskManagement/TaskDependentDates/',
            controller: 'TaskDependentDatesController'
        })
        .when('/entity-task', {
            templateUrl: 'TaskManagement/entityTask/aplos',
            controller: 'entityTaskController'
        })
        .when('/task-schedule', {
            templateUrl: 'TaskManagement/TaskSchedule/aplos',
            controller: 'TaskScheduleController'
        })
        .when('/tna-reports', {
            templateUrl: 'TaskManagement/TNAReports/aplos',
            controller: 'TNAReportsController'
        })
        .when('/task-status-reports', {
            templateUrl: 'TaskManagement/TNAStatusReports/aplos',
            controller: 'TNAStatusReportsController'

        }).when('/tna-transfer', {
            templateUrl: 'TaskManagement/TaskReplacement/aplos',
            controller: 'TaskReplacementController'
        }).when('/issue-status-reports', {
            templateUrl: 'TaskManagement/IssueStatusReports/aplos',
            controller: 'IssueStatusReportsController'

        })
        .when('/task-manag-report', {
            templateUrl: 'TaskManagement/TaskManagementReport/aplos',
            controller: 'TaskManagementReportController'

        })
        .when('/user-edit-control', {
            templateUrl: 'TaskManagement/TaskAppliedOn/UserUnit',
            controller: 'UserEditControlController'
        })
        .when('/edit-control', {
            templateUrl: 'TaskManagement/EditControl/EditCtrl',
            controller: 'EditControlController'
        })

        .when('/task-closer', {
            templateUrl: 'TaskManagement/TaskCloserMaster/Aplos',
            controller: 'TaskCloserMasterController'
        })
        .when('/task-type', {
            templateUrl: 'TaskManagement/Tasktype/Aplos',
            controller: 'taskTypeController'
        })
        ;
}