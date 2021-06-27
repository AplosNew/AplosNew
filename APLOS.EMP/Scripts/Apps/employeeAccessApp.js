var employeeAccessApp = angular.module('employeeAccessApp', ['angularUtils.directives.dirPagination'])
    .controller('AplosEmpFieldController', AplosEmpFieldController)
    .controller('AplosEmpFieldTagController', AplosEmpFieldTagController)
    .controller('EmployeeLinkController', EmployeeLinkController)
    .controller('UserAccessController', UserAccessController)
    .controller('cpanelLoginController', CPanelLoginController)
    .controller('employeeProfileFromExcelController', employeeProfileFromExcelController)
    .directive('compile', ['$compile', function ($compile) {
        return function (scope, element, attrs) {
            scope.$watch(
                function (scope) {
                    return scope.$eval(attrs.compile);
                },
                function (value) {
                    element.html(value);
                    $compile(element.contents())(scope);
                }
            );
        };
    }])
    .directive('showErrors', ['$rootScope', function ($rootScope) {
        return {
            restrict: 'A',
            require: '^form',
            link: function (scope, el, attrs, formCtrl) {
                var inputEl = el[0].querySelector('[name]');
                var inputNgEl = angular.element(inputEl);
                var inputName = inputNgEl.attr('name');
                scope.$on('show-errors-check-validity', function () {
                    $rootScope.testInputValid = formCtrl[inputName].$invalid;
                    if ($rootScope.testInputValid) {
                        $rootScope.hasInputInvalid = inputName;
                    }
                    el.toggleClass('has-error', formCtrl[inputName].$invalid);
                    el.toggleClass('help-block', formCtrl[inputName].$invalid);
                    el.find('.help-block').remove();
                    el.find('.show-message').append('<p class="help-block">' + inputName + ' is required.</p>');
                });
                scope.$on('show-errors-reset', function () {
                    $timeout(function () {
                        el.removeClass('has-error');
                        el.removeClass('help-block');
                        el.find('.show-message').remove('<p class="wrn-text help-block">' + inputName + ' is required.</p>');
                    }, 0, false);
                });
                inputNgEl.bind('blur', function () {
                    el.toggleClass('has-error', formCtrl[inputName].$invalid);
                })
            }
        }
    }])
    .filter('dateFilter', dateFilter)
    .filter('dateFiltering', dateFiltering)
    .filter('safecontent', safecontent)
    .directive('empBody', empBody)
    .directive('datepicker', datepicker)
    .directive('togglable', togglable)
    .directive('showErrors', showErrors)
    .directive('archiveRow', archiveRow)
    .directive('confirmModal', confirmModal)
    .directive('loader', loader)
    .directive('stringToNumber', stringToNumber)
    .directive('apanelBody', apanelBody)
    .directive('inputMaxLengthNumber', inputMaxLengthNumber)
    .directive('confirmCancel', confirmCancel)
    .directive('input', inputFocus)
    .directive('ngEnter', ngEnter)
    .directive('nDecimals', nDecimals)
    .directive('confirmArchive', confirmArchive)
    .directive('genericConfirmPopUp', genericConfirmPopUp)
    .directive('confirmArchiveGeneric', confirmArchiveGeneric)
    .directive('tooltip', tooltip)
    .directive('onlyNumbers', onlyNumbers)
    .directive('modalTable', modalTable)
    .directive('dynamic', dynamic)
    .directive("ngFileSelect", ngFileSelect)
    .directive('compile', compile)
    .factory('errorInterceptor', errorInterceptor)
    .factory('baseService', baseService)
    .factory('cboService', cboService)
    .factory('fileReader', fileReader)
    .factory('dataShare', dataShare)
    .constant('commonMessage', commonMessage)
    ;
