/// <reference path="../angular-constant-path.js" />
'use strict';
var ppanelApp = angular.module('ppanelApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'toaster', 'ui.calendar', 'ui.bootstrap', "ejangular"])
    .controller('ppanelLogoutController', ppanelLogoutController)
    .controller('parentsPasswordChangeController', parentsPasswordChangeController)
    .controller("myParentsCalendarController", myParentsCalendarController)
    //.controller("TeacherScheduleController", TeacherScheduleController)
    .controller("POParameterChangeController", POParameterChangeController)
    .controller("partyBaseController", partyBaseController)

    //#endregion

    .config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {
        $routeProvider
            .when('/', {
                templateUrl: 'MyParents/dashboard'
            })
            .when('tpanel', {
                templateUrl: 'MyParents/dashboard'
            })
            .when('/dashboard', {
                templateUrl: 'MyParents/dashboard'
            })
            
            .when('/login', {
                templateUrl: 'MyParents/login',
                controller: 'portalLoginController'
            })
            .when('/password-change/:id', {
                templateUrl: 'MyParents/passwordchange',
                controller: 'parentsPasswordChangeController'
            })
            .when('/employee-calendar', {
                templateUrl: 'MyParents/Calendar',
                controller: 'myParentsCalendarController'
            })
           

            .when('/po-parameter', {
                templateUrl: 'Products/POParameterChange/Aplos',
                controller: 'POParameterChangeController'
            })

            .when('/logout', {
                template: ' ',
                controller: 'ppanelLogoutController'
            })
           
            .otherwise({
                redirectTo: 'MyParents/login'
            });


    }])
    .run(['$rootScope', '$timeout', '$cookies', '$window', "$filter", "$http", function ($rootScope, $timeout, $cookies, $window, $filter, $http) {
        $rootScope.title = 'MyParents';
        $rootScope.bootPoint = '#!/';

        $window.employeeId = $cookies.get("MyParentsemployeeId");
        $window.employeeName = $cookies.get("MyParentsemployeeName");
        $window.companyGroupId = $cookies.get("MyParentsgroupId");
        $window.companyId = $cookies.get("MyParentscompanyId");
        $window.plantId = $cookies.get("MyParentsplantId");
        $rootScope.plantName = $cookies.get("MyParentsplantName");
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