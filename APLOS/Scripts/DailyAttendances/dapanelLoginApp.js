'use strict';
var dapanelLoginApp = angular.module('dapanelLoginApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'toaster'])
    .controller('dapanelLoginController', dapanelLoginController)
    .run(['$rootScope', function ($rootScope) {
        $rootScope.title = 'aPOP';
    }])
    .directive('datepicker', datepicker)
    .directive('togglable', togglable)
    .directive('showErrors', showErrors)
    .directive('loader', loader)
    .directive('tooltip', tooltip)
    .directive('ngEnter', ngEnter)
    .directive('input', inputFocus)
    .factory('cboService', cboService)
    .factory('baseService', baseService)
    .factory('errorInterceptor', errorInterceptor)
    .constant('commonMessage', {
        appName: 'aPOP',
        appVersion: 2.0,
        primaryKeyNullMessage: 'Please select any Rows.',
        NetworkError: 'Error occur, please try again.'
    });