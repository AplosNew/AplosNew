'use strict';
ScatteredWeekOffController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function ScatteredWeekOffController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Scattered Week Off';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/ScatteredWeekOff/';

    //Tabs Changes
    function Tabs() {
        var bindAll = function () {
            var menuElements = document.querySelectorAll('[data-tab]');
            for (var i = 0; i < menuElements.length; i++) {
                menuElements[i].addEventListener('click', change, false);
            }
        }

        var clear = function () {
            var menuElements = document.querySelectorAll('[data-tab]');
            for (var i = 0; i < menuElements.length; i++) {
                menuElements[i].classList.remove('active');
                var id = menuElements[i].getAttribute('data-tab');
                document.getElementById(id).classList.remove('active');
            }
        }

        var change = function (e) {
            clear();
            e.target.classList.add('active');
            var id = e.currentTarget.getAttribute('data-tab');
            document.getElementById(id).classList.add('active');
            if ($rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }

        bindAll();
    }
    var connectTabs = new Tabs();
    
}