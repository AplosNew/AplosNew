'use strict';
var recruitmentApp = angular.module('recruitmentApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'toaster'])
    .controller('preRecruitmentLoginController', preRecruitmentLoginController)
    .controller('changePinController', changePinController)
    .controller('preRecruitmentController', preRecruitmentController)
    .config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {
        $routeProvider
           
            .when('/', {
                templateUrl: function (params) {
                    
                    return 'recruitments/home/Aplos';
                }
            })
            .when('/recruitments', {
                templateUrl: function (params) {
                    var id = '';
                    if (params.id === undefined) {
                        id = '';
                    }
                    else
                        id = params.id;
                    return 'recruitments/home/aplos?u=' + id;
                }
            })
            
            .when('/changepin', {
                templateUrl: function (params) {
                    controller: 'changePinController';
                    return 'recruitments/home/changepin?id=' + params.id;
                }
            })
            .when('/logout', {
                template: ' ',
                controller: 'preRecruitmentLogoutController'
            })
            .otherwise({
                redirectTo: 'recruitments/home/Login'
            });
    }])
    .run(['$rootScope', '$cookies', function ($rootScope, $cookies) {
        $rootScope.title = 'Pre-Recruitment';
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
    .factory('addressService', addressService)
    .constant('commonMessage', {
        appName: 'Aplos ERP',
        appVersion: 2.0,
        primaryKeyNullMessage: 'Please select any Rows.',
        NetworkError: 'Error occur, please try again.'
    })
    ;