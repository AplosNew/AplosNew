'use strict';
var dailyattendanceApp = angular.module('dailyattendanceApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'toaster'])
    .controller('dailyAttendanceInOutLoginController', dailyAttendanceInOutLoginController)
    .controller('changePinController', changePinController)
    .controller('dailyAttdInOutController', dailyAttdInOutController)
    .config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {
        $routeProvider
           
            .when('/', {
                templateUrl: function (params) {
                    
                    return 'dailyattendance/home/Aplos';
                }
            })
            .when('/dailyattendance', {
                templateUrl: function (params) {
                    var id = '';
                    if (params.id === undefined) {
                        id = '';
                    }
                    else
                        id = params.id;
                    return 'dailyattendance/home/Aplos?u=' + id;
                }
            })
            
            
            .when('/logout', {
                template: ' ',
                controller: 'preRecruitmentLogoutController'
            })
            .otherwise({
                redirectTo: 'Login'
            });
    }])
    .run(['$rootScope', '$cookies', function ($rootScope, $cookies) {
        $rootScope.title = 'Daily-Attendance';
        $rootScope.bootPoint = '#!/';
        $rootScope.empid = $cookies.get('empId');
    }])
    .filter('dateFilter', dateFilter)
    .filter('dateFiltering', dateFiltering)
    .filter('safecontent', safecontent)
    .directive('togglable', togglable)
    .directive('showErrors', showErrors)
    .directive('loader', loader)
    .directive('tooltip', tooltip)
    .directive('ngEnter', ngEnter)
    .directive('input', inputFocus)
    .directive('ngFileSelect', ngFileSelect)
    .directive('datepicker', datepicker)
    .directive('confirmArchive', confirmArchive)
    .directive('confirmArchiveGeneric', confirmArchiveGeneric)
    .directive('onlyNumbers', onlyNumbers)
    .directive('nDecimals', nDecimals)
    .factory('cboService', cboService)
    .factory('baseService', baseService)
    .factory('errorInterceptor', errorInterceptor)
    .factory('fileReader', fileReader)
    .constant('commonMessage', {
        appName: 'Aplos ERP',
        appVersion: 2.0,
        primaryKeyNullMessage: 'Please select any Rows.',
        NetworkError: 'Error occur, please try again.'
    })
    ;