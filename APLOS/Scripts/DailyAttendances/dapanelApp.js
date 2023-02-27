/// <reference path="../angular-constant-path.js" />
'use strict';
var dapanelApp = angular.module('dapanelApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'toaster', 'ui.calendar', 'ui.bootstrap', "ejangular"])
    .controller('dapanelLogoutController', dapanelLogoutController)
    .controller('daPasswordChangeController', daPasswordChangeController)
    .controller("taskListController", taskListController)
    //.controller("TeacherScheduleController", TeacherScheduleController)

    //#endregion

    .config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {
        $routeProvider
            .when('/', {
                templateUrl: 'DailyAttendances/dashboard'
            })
            .when('dapanel', {
                templateUrl: 'DailyAttendances/dashboard'
            })
            .when('/dashboard', {
                templateUrl: 'DailyAttendances/dashboard'
            })
            
            .when('/login', {
                templateUrl: 'DailyAttendances/login',
                controller: 'portalLoginController'
            })
            .when('/password-change/:id', {
                templateUrl: 'DailyAttendances/passwordchange',
                controller: 'daPasswordChangeController'
            })
            //.when('/employee-calendar', {
            //    templateUrl: 'DailyAttendances/Calendar',
            //    controller: 'myParentsCalendarController'
            //})
            .when('/task-list', {
                templateUrl: 'TaskManagement/TaskList/',
                controller: 'taskListController'
            })

            //.when('/teacher-schedule', {
            //    templateUrl: 'TaskManagement/TeacherSchedule/Aplos',
            //    controller: 'TeacherScheduleController'
            //})

            .when('/logout', {
                template: ' ',
                controller: 'dapanelLogoutController'
            })
           
            .otherwise({
                redirectTo: 'DailyAttendances/login'
            });
    }])
    .run(['$rootScope', '$timeout', '$cookies', '$window', "$filter", "$http", function ($rootScope, $timeout, $cookies, $window, $filter, $http) {
        $rootScope.title = 'DailyAttendances';
        $rootScope.bootPoint = '#!/';

        $window.employeeId = $cookies.get("employeeId");
        $window.employeeName = $cookies.get("employeeName");
        $window.companyGroupId = $cookies.get("groupId");
        $window.companyId = $cookies.get("companyId");
        $window.plantId = $cookies.get("plantId");
        $rootScope.plantName = $cookies.get("plantName");
        $rootScope.companyGroupLogo = virtualPath.LogoOrImage + $cookies.get("gImage");

        $rootScope.Message = '';
        $rootScope.HeaderText = '';
        $rootScope.ShowError = function (message, headerText) {
            $rootScope.Message = message;
            $rootScope.HeaderText = headerText;
            $("#dialogMessage").ejDialog("setTitle", headerText);
            $("#dialogMessage").ejDialog("open");
        }
        $rootScope.MyAppuserImage = virtualPath.EmployeePic;
    }])
    .filter('safecontent', safecontent)
    .filter('dateFiltering', dateFiltering)
    .filter('dateFilter', dateFilter)

    .filter('myDate', myDate)
    .filter("sumByKey", sumByKey)
    .filter('find', find)
    .directive('panelBody', panelBody)
    .directive('datepicker', datepicker)
    .directive('togglable', togglable)
    .directive('showErrors', showErrors)
    .directive('compile', compile)
    .directive('archiveRow', archiveRow)
    .directive('nDecimals', nDecimals)
    .directive('onlyNumbers', onlyNumbers)
    .directive('confirmModal', confirmModal)
    .directive('confirmArchive', confirmArchive)
    .directive('loader', loader)
    .directive('tooltip', tooltip)
    .directive('input', inputFocus)
    .directive('textarea', inputFocus)
    .directive('select', inputFocus)
    .directive('input', CodeChecker)
    .directive('dateFormatter', dateFormatter)
    .directive('ngEnter', ngEnter)
    .directive('ngFileSelect', ngFileSelect)
    .directive('confirmArchiveGeneric', confirmArchiveGeneric)
    .directive('headerSearch', headerSearch)
    .directive("capitalize", capitalize)
    .factory('errorInterceptor', errorInterceptor)
    .factory('baseService', baseService)
    .factory('cboService', cboService)
    .factory('fileReader', fileReader)
    .factory('exportToExcel', exportToExcel)
    .filter("setDecimal", setDecimal)
    .factory("accountService", accountService)
    .factory('addressService', addressService)
    .factory('signalR', signalR)
    .constant('commonMessage', {
        appName: 'aPOP',
        appVersion: 2.0,
        primaryKeyNullMessage: 'Please select any Rows.',
        NetworkError: 'Error occur, please try again.'
    })
    ;