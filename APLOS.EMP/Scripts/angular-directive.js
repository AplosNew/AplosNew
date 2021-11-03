function ngFileSelect() {
    return {
        link: function ($scope, el) {
            el.bind("change", function (e) {
                $scope.file = (e.srcElement || e.target).files[0];
                $scope.getFile();
            });
        }
    }
}
function ngFileSelectMultiple() {
    return {
        link: function ($scope, el) {
            el.bind("change", function (e) {
                $scope.file = (e.srcElement || e.target).files[0];
                $scope.getFile();
            });
        }
    }
}

function ngEnter() {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });
                event.preventDefault();
            }
        });
    };
}
empBody.$inject = ['$timeout'];
function empBody($timeout) {
    return {
        link: function () {
            angular.element('.navbar-site').css('width', angular.element('.navbar-site').width() - angular.element('.sidebar').width());
            angular.element('.navbar-site').css('margin-left', angular.element('.sidebar').width());
            angular.element('.main-nav').vmenuModule({
                Speed: 400,
                autostart: false,
                autohide: true
            });
        },
        controller: function ($scope, $element) {
            $scope.ShowHideSideBar = function () {
                angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
                angular.element('.sidebar').toggleClass('tiny-sidebar');
                angular.element('.navbar-site').toggleClass('navbar-site-full');
                $timeout(function () {
                    angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
                }, 300);
            }
            angular.element(".main-nav li a").on('click', function () {
                $("div.alert").remove();
            });
        }
    };
}

datepicker.$inject = ['$parse', '$timeout'];
function datepicker($parse, $timeout) {
    var directive = {
        restrict: 'A',
        link: datepickerLink,
        require: '?ngModel'
    };
    return directive;
    function datepickerLink(scope, element, attrs, ngModel) {
        if (!ngModel) return;
        var getter = $parse(attrs.ngModel);
        var value = getter(scope);
        element
            .addClass('datepicker')
            .datepicker({ format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, orientation: 'bottom' })
            .datepicker().on('changeDate', function (event) {
                scope.$apply(function () {
                    ngModel.$setViewValue(event.date);
                });
            });
        ngModel.$render = function () {
            element.datepicker('update', ngModel.$viewValue || '');
        };
        $timeout(function () {
            element.datepicker('update', value || '');
        }, 1)
    }
}

togglable.$inject = ['$rootScope'];
function togglable($rootScope) {
    var directive = {
        restrict: 'E',
        template: '<h3 class="site-heading" ng-click="$root.toggle()"><span class="glyphicon glyphicon-info-sign"></span> {{name}}<a class="form-collapse pull-right" ng-class="{ collapse: isCollapsed }" ></a></h3>',
        replace: true,
        scope: { name: '@' },
        controller: function ($scope) {
            $rootScope.isCollapsed = false;
            var $content = angular.element(".form-elements");
            var $list = angular.element(".aplos-grid-toggle");
            $content.hide();
            $list.show();
            $rootScope.toggle = function () {
                angular.element(".aplos-grid-toggle").toggleClass("collapse");
                angular.element(".form-collapse").toggleClass("expanded");
                $rootScope.isCollapsed = angular.element(".form-collapse").hasClass("expanded") ? true : false;
                $content.slideToggle();
            };
        }
    };
    return directive;
}

showErrors.$inject = ['$rootScope'];
function showErrors($rootScope) {
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
}

dynamic.$inject = ['$compile'];
function dynamic($compile) {
    return {
        restrict: 'A',
        replace: true,
        link: function (scope, ele, attrs) {
            scope.$watch(attrs.dynamic, function (html) {
                ele.html(html);
                $compile(ele.contents())(scope);
            });
        }
    };
}

compile.$inject = ['$compile'];
function compile($compile) {
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
}

function archiveRow() {
    return {
        priority: 1,
        terminal: true,
        link: function ($scope, element, attr) {
            var clickAction = attr.ngClick;
            var msg = attr.archiveRow || 'Are you sure to delete?';
            element.bind('click', function () {
                if (window.confirm(msg)) {
                    $scope.$eval(clickAction);
                }
            });
        }
    }
}

function confirmModal() {
    return {
        restrict: 'E',
        replace: true,
        template: '<div class="modal fade site-modal in" id="confirmPopUp" role="dialog" data-backdrop="static">' +
        '<div class="modal-dialog modal-sm">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
        '<h4 class="modal-title"><i class="glyphicon glyphicon-warning-sign"></i> Delete</h4>' +
        '</div>' +
        '<div class="modal-body"><p class="text-warning">Are you sure to delete ?</p></div>' +
        '<div class="modal-footer  common-btn">' +
        '<button type="button" class="btn btn-default" data-dismiss="modal">No</button>' +
        '<button type="button" data-dismiss="modal" id="btnConfirm" ng-click="Delete()" class="btn btn-default">Yes</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>'
    };
}

confirmArchive.$inject = ['$rootScope'];
function confirmArchive($rootScope) {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            title: '@',
            message: '@',
            body: '@',
            callbackbuttonright: '&ngClickRightButton',
            callBackMethod: '&removeRow'
        },
        template: '<div class="modal fade site-modal in" id="archivePopUp" role="dialog" data-backdrop="static">' +
        '<div class="modal-dialog modal-sm">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
        '<h4 class="modal-title"><i class="glyphicon glyphicon-warning-sign"></i> {{title}}</h4>' +
        '</div>' +
        '<div class="modal-body"><p class="text-warning">{{message}} <b>{{body}}</b></p></div>' +
        '<div class="modal-footer common-btn">' +
        '<button type="button" class="btn btn-default" data-dismiss="modal">No</button>' +
        '<button type="button" data-dismiss="modal" id="btnConfirm" data-ng-click="callbackbuttonright()" class="btn btn-default">Yes</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>',
        controller: function ($scope) {
            $rootScope.passValue = function (id, $index) {
                //$scope.body = id;
                $rootScope.id = id;
                $rootScope.index = $index;
            };
        }
    }
}

function confirmVoucherdtails($rootScope) {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            title: '@',
            message: '@',
            body: '@',
            callbackbuttonright: '&ngClickRightButton',
            callBackMethod: '&removeRow'
        },
        template: '<div class="modal fade site-modal in" id="archivePopUp" role="dialog" data-backdrop="static">' +
        '<div class="modal-dialog modal-sm">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
        '<h4 class="modal-title"><i class="glyphicon glyphicon-warning-sign"></i> {{title}}</h4>' +
        '</div>' +
        '<div class="modal-body"><p class="text-warning">{{message}} <b>{{body}}</b></p></div>' +
        '<div class="modal-footer common-btn">' +
        '<button type="button" class="btn btn-default" data-dismiss="modal">No</button>' +
        '<button type="button" data-dismiss="modal" id="btnConfirm" data-ng-click="callbackbuttonright()" class="btn btn-default">Yes</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>',
        controller: function ($scope) {
            $rootScope.passVoucherValue = function (id, $index) {
                //$scope.body = id;
                $rootScope.id = id;
                $rootScope.index = $index;
            };
        }
    }
}

confirmArchiveGeneric.$inject = ['$rootScope'];
function confirmArchiveGeneric($rootScope) {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            modalid: '@',
            title: '@',
            message: '@',
            body: '@',
            callbackbuttonleft: '&ngClickLeftButton',
            callbackbuttonright: '&ngClickRightButton',
            callBackMethod: '&removeRow'
        },
        template: '<div class="modal fade site-modal in" id="{{modalid}}" role="dialog" data-backdrop="static">' +
        '<div class="modal-dialog modal-sm">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-ng-click="callbackbuttonleft()">&times;</button>' +
        '<h4 class="modal-title"><i class="glyphicon glyphicon-warning-sign"></i> {{title}}</h4>' +
        '</div>' +
        '<div class="modal-body"><p class="text-warning" ng-bind-html="message | safecontent"> <b>{{body}}</b></p></div>' +
        '<div class="modal-footer common-btn">' +
        '<button type="button" class="btn btn-default" data-ng-click="callbackbuttonleft()">No</button>' +
        '<button type="button" data-dismiss="modal" id="btnConfirm" data-ng-click="callbackbuttonright()" class="btn btn-default">Yes</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>',
        controller: function ($scope) {
            $rootScope.passValue = function (id, $index) {
                //$scope.body = id;
                $rootScope.id = id;
                $rootScope.index = $index;
            };
        }
    }
}

genericConfirmPopUp.$inject = ['$rootScope'];
function genericConfirmPopUp($rootScope) {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            modalid: '@',
            title: '@',
            message: '@',
            body: '@',
            callbackbuttonleft: '&ngClickLeftButton',
            callbackbuttonright: '&ngClickRightButton',
            callBackMethod: '&removeRow'
        },
        template: '<div class="modal fade site-modal in" id="{{modalid}}" role="dialog" data-backdrop="static">' +
        '<div class="modal-dialog modal-sm">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-ng-click="callbackbuttonleft();">&times;</button>' +
        '<h4 class="modal-title"><i class="glyphicon glyphicon-warning-sign"></i> {{title}}</h4>' +
        '</div>' +
        '<div class="modal-body"><p class="text-warning" ng-bind-html="message | safecontent"> <b>{{body}}</b></p></div>' +
        '<div class="modal-footer common-btn">' +
        '<button type="button" class="btn btn-default" data-ng-click="callbackbuttonleft()">No</button>' +
        '<button type="button" data-dismiss="modal" id="btnConfirm" data-ng-click="callbackbuttonright()" class="btn btn-default">Yes</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>',
        controller: function ($scope) {
            $rootScope.passValue = function (id, $index) {
                $rootScope.id = id;
                $rootScope.index = $index;
            };
        }
    }
}
function loader($http) {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs) {
            scope.isLoading = function () {
                return $http.pendingRequests.length > 0;
            };
            scope.$watch(scope.isLoading, function (v) {
                if (v) {
                    elm.show();
                } else {
                    elm.hide();
                }
            });
        }
    }
}

function tooltip() {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            $(element).hover(function () {
                // on mouseenter
                $(element).tooltip('show');
            }, function () {
                // on mouseleave
                $(element).tooltip('hide');
            });
        }
    }
}

function KeyEnterFocus() {
    //var directive = {
    //    restrict: 'E',
    //    link: KeyEnterFocusLink
    //};
    //return directive;

    //function KeyEnterFocusLink($scope, elem, attrs) {
    //    elem.bind('keydown', function (event) {
    //        var code = event.keyCode || event.which;
    //        if (code === 13) {
    //            $scope.$apply(function () {
    //                $scope.$eval(attrs.aplosFocus);
    //            });
    //            angular.element(document.querySelectorAll("[tabindex='" + (parseInt(attrs.tabindex) + 1) + "']")).focus();
    //            event.preventDefault();
    //        }
    //    });
    //}
}

function cpanelBody() {
    return {
        templateUrl: '/cpanel/home/cpanelbody',
        link: function () {
            angular.element('.navbar-site').css('width', angular.element('.navbar-site').width() - angular.element('.sidebar').width());
            angular.element('.navbar-site').css('margin-left', angular.element('.sidebar').width());
            angular.element('.main-nav').vmenuModule({
                Speed: 400,
                autostart: false,
                autohide: true
            });
        },
        controller: function ($scope, $element) {
            $scope.ShowHideSideBar = function () {
                angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
                angular.element('.sidebar').toggleClass('tiny-sidebar');
                angular.element('.navbar-site').toggleClass('navbar-site-full');
                $timeout(function () {
                    angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
                }, 300);
            }
            angular.element(".main-nav li a").on('click', function () {
                $("div.alert").remove();
            });
        }
    };
}

function apanelBody() {
    return {
        templateUrl: '/apanel/home/apanelbody',
        link: function () {
            angular.element('.navbar-site').css('width', angular.element('.navbar-site').width() - angular.element('.sidebar').width());
            angular.element('.navbar-site').css('margin-left', angular.element('.sidebar').width());
            angular.element('.main-nav').vmenuModule({
                Speed: 400,
                autostart: false,
                autohide: true
            });
        },
        controller: function ($scope, $element, $timeout) {
            $scope.ShowHideSideBar = function () {
                angular.element(".navbar-site").toggleClass('navbar-site-full');
                angular.element(".sidebar").toggleClass('tiny-sidebar');
                angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
                $timeout(function () {
                    angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
                }, 300);
            }
            angular.element(".main-nav li a").on('click', function () {
                $("div.alert").remove();
            });
        }
    };
}

mpanelBody.$inject = ['$http', '$timeout'];
function mpanelBody($http, $timeout) {
    return {
        templateUrl: '/mpanel/home/mpanelbody',
        link: function ($scope) {
            angular.element('.navbar-site').css('width', angular.element('.navbar-site').width() - angular.element('.sidebar').width());
            angular.element('.navbar-site').css('margin-left', angular.element('.sidebar').width());
            $scope.menuFrames = [];
            $http({
                method: 'GET',
                url: '/securities/userrole/getmenus?panel=Master'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.menuFrames = response.data;
                    //console.log(response.data);
                    setTimeout(function () {
                        $scope.$apply(function () {
                            angular.element('.main-nav').vmenuModule({
                                Speed: 400,
                                autostart: false,
                                autohide: true
                            });
                        });
                    }, 100);
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
        },
        controller: function ($scope, $element) {
            $scope.ShowHideSideBar = function () {
                angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
                angular.element('.sidebar').toggleClass('tiny-sidebar');
                angular.element('.navbar-site').toggleClass('navbar-site-full');
                $timeout(function () {
                    angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
                }, 300);
            }
            angular.element(".main-nav li a").on('click', function () {
                $("div.alert").remove();
            });
        }
    };
}

upanelBody.$inject = ['$http', '$timeout'];
function upanelBody($http, $timeout) {
    return {
        templateUrl: '/upanel/home/upanelbody',
        link: function ($scope) {
            angular.element('.navbar-site').css('width', angular.element('.navbar-site').width() - angular.element('.sidebar').width());
            angular.element('.navbar-site').css('margin-left', angular.element('.sidebar').width());
            $scope.menuFrames = [];
            $http({
                method: 'GET',
                url: '/securities/userrole/getmenus?panel=Application'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(data.Message, 'failure');
                }
                else {
                    $scope.menuFrames = response.data;
                    setTimeout(function () {
                        $scope.$apply(function () {
                            angular.element('.main-nav').vmenuModule({
                                Speed: 400,
                                autostart: false,
                                autohide: true
                            });
                        });
                    }, 100);
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
        },
        controller: function ($scope, $element) {
            $scope.ShowHideSideBar = function () {
                angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
                angular.element('.sidebar').toggleClass('tiny-sidebar');
                angular.element('.navbar-site').toggleClass('navbar-site-full');
                $timeout(function () {
                    angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
                }, 300);
            }
            angular.element(".main-nav li a").on('click', function () {
                $("div.alert").remove();
            });
        }
    };
}

// for text box focus and remove white space.
function inputFocus() {
    return {
        restrict: 'E',
        require: '?ngModel',
        link: function ($scope, elem, attrs) {
            elem.bind('keydown', function (event) {
                var code = event.keyCode || event.which;
                if (code === 13) {
                    $scope.$apply(function () {
                        $scope.$eval(attrs.aplosFocus);
                    });
                    angular.element(document.querySelectorAll("[tabindex='" + (parseInt(attrs.tabindex) + 1) + "']")).focus();
                    event.preventDefault();
                }
            });
            //elem.bind('mouseleave', function () {
            elem.bind('blur', function () {
                elem.val(elem.val()
                    //.replace(/[\s]/g, ' ')
                    .replace(/(^\s*)|(\s*$)/gi, "")
                    //.replace(/[ ]{2,}/gi, " ")
                    .replace(/\n /, "\n")
                );
            });
        }
    }
}

function stringToNumber() {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            ngModel.$parsers.push(function (value) {
                return '' + value;
            });
            ngModel.$formatters.push(function (value) {
                return parseFloat(value);
            });
        }
    };
}

function inputMaxLengthNumber() {
    return {
        require: 'ngModel',
        restrict: 'A',
        link: function (scope, element, attrs, ngModelCtrl) {
            function fromUser(text) {
                var maxlength = Number(attrs.maxlength);
                if (String(text).length > maxlength) {
                    ngModelCtrl.$setViewValue(ngModelCtrl.$modelValue);
                    ngModelCtrl.$render();
                    return ngModelCtrl.$modelValue;
                }
                return text;
            }
            ngModelCtrl.$parsers.push(fromUser);
        }
    };
}

function dateFormatter() {
    return {
        restrict: 'E',
        link: formateDate
    }

    function formateDate(dateObject) {
        if (dateObject) {
            var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            $scope.d = new Date(dateObject);
            $scope.day = $scope.d.getDate();
            $scope.month = $scope.d.getMonth() + 1;
            $scope.year = $scope.d.getFullYear();
            if ($scope.day < 10) {
                $scope.day = "0" + $scope.day;
            }
            if ($scope.month < 10) {
                $scope.month = "0" + $scope.month;
            }
            $scope.date = $scope.day + "-" + months[$scope.month - 1] + "-" + $scope.year;
            return $scope.date;
        }
    }
}

confirmCancel.$inject = ['$rootScope'];
function confirmCancel($rootScope) {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            message: '@',
            title: '@',
            callbackbuttonright: '&ngClickRightButton',
            callBackMethod: '&removeRow'
        },
        template: '<div class="modal fade site-modal in" id="cancelPopUp" role="dialog" data-backdrop="static">' +
        '<div class="modal-dialog modal-sm">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
        '<h4 class="modal-title"><i class="glyphicon glyphicon-warning-sign"></i> {{title}}</h4>' +
        '</div>' +
        '<div class="modal-body"><p class="text-warning">{{message}}?</p></div>' +
        '<div class="modal-footer common-btn">' +
        '<button type="button" class="btn btn-default" ng-click="$root.cancelModal()">No</button>' +
        '<button type="button" data-dismiss="modal" id="btnConfirm" data-ng-click="callbackbuttonright()" class="btn btn-default">Yes</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>',
        controller: function ($scope) {
            $rootScope.cancelModal = function () {
                angular.element(document.querySelector('#cancelPopUp')).modal('hide');

            };

        }
    };
}

//check input number
onlyNumbers.$inject = ['$rootScope'];
function onlyNumbers($rootScope) {
    return {
        require: 'ngModel',
        link: function (scope, element, attr, ngModelCtrl) {
            function fromUser(text) {
                if (attr.min !== text.length)
                    $rootScope.lengthCheck = true;
                else
                    $rootScope.lengthCheck = false;
                if (text) {
                    var transformedInput = text.replace(/[^-0-9]/g, '');
                    var negativeCheck = transformedInput.split('-');
                    if (!angular.isUndefined(negativeCheck[1])) {
                        negativeCheck[1] = negativeCheck[1].slice(0, negativeCheck[1].length);
                        transformedInput = negativeCheck[0] + '-' + negativeCheck[1];
                        if (negativeCheck[0].length > 0) {
                            transformedInput = negativeCheck[0];
                        }

                    }
                    if (transformedInput !== text) {
                        ngModelCtrl.$setViewValue(transformedInput);
                        ngModelCtrl.$render();
                    }
                    return transformedInput;
                }
                return undefined;
            }
            ngModelCtrl.$parsers.push(fromUser);
        }
    };
}

modalTable.$inject = ['$rootScope'];
function modalTable($rootScope) {
    var directive = {
        restrict: 'E',
        replace: true,
        scope: {
            modalid: '@',
            title: '@',
            searchParameters: '=',
            excluedColumnList: '=',
            fieldList: '=',
            dataList: '=',
            searchFunction: '=',
            singleClick: '&',
            doubleClick: '&',
            rightButton: '=',
            leftButton: '=',
            callbackbuttonleft: '&ngClickLeftButton',
            callbackbuttoncancel: '&ngClickCancel'
        },
        link: function (scope) {
            scope.hideColumn = function (value, columnList) {
                var show = true;
                if (columnList !== undefined && columnList.length > 0) {
                    for (var i = 0; i < columnList.length; i++) {
                        if (columnList.indexOf(value) != -1) {
                            show = false;
                            return show;
                        }
                        else {
                            show = true;
                            return show;
                        }
                    }
                }
                return show;
            }
        },
        template: '<div class="modal fade site-modal in" id="{{modalid}}" role="dialog" data-backdrop="static" tabindex="-1" >' +
        '<div class="modal-dialog modal-lg">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '{{title}}' +
        '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
        '</div>' +
        '<div class="modal-body">' +
        '<div class="row text-right">' +
        '<div class="col-xs-12 col-sm-12 col-sm-12 ">' +
        '<div class="search-form">' +
        '<div class="row">' +
        '<div class="col-xs-12 col-sm-3 col-md-3">' +
        '<div class="select-style">' +
        '<select ng-model="searchParameters.searchBy">' +
        '<option ng-repeat="option in fieldList" value="{{option.Value}}">{{option.Text}}</option>' +
        '</select>' +
        '</div>' +
        '</div>' +
        '<div class="col-xs-12 col-sm-7 col-md-7">' +
        '<input type="text" class="form-control" placeholder="Search" ng-model="searchParameters.search">' +
        '</div>' +
        '<div class="col-xs-12 col-sm-2 col-md-2">' +
        '<button type="button" data-ng-click="searchFunction()" class="btn"><i class="glyphicon glyphicon-search"></i> Search</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '<div class="table-responsive modal-table">' +
        '<table class="table table-striped">' +
        '<thead>' +
        '<tr>' +
        '<th ng-repeat="y in fieldList" ng-show="{{hideColumn(y.Value,excluedColumnList)}}">{{y.Text}}</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody>' +
        '<tr dir-paginate="x in dataList|itemsPerPage:searchParameters.pageSize" total-items="searchParameters.total_count" ' +
        'ng-click="singleClick({data:x});" ng-dblclick="doubleClick({data:x});" pagination-id="metaData.name +\'dataList\'">' +
        '<td ng-repeat="y in fieldList" ng-show="{{hideColumn(y.Value,excluedColumnList)}}">{{x[y.Value]}}</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>' +
        '<dir-pagination-controls max-size="10" ' +
        'pagination-id="metaData.name +\'dataList\'" ' +
        'direction-links="true" ' +
        'boundary-links="true"  ' +
        'on-page-change="searchFunction(newPageNumber)"> ' +
        '</dir-pagination-controls>' +
        '</div>' +
        '<div class="common-btn">' +
        '<ul class="list-inline text-right">' +
        '<li><button ng-show="leftButton" data-ng-click="callbackbuttonleft()" class="btn"><i class=""></i> Select</button></li>' +
        '<li><button ng-show="rightButton" data-ng-click="callbackbuttoncancel()" class="btn btn-default">Cancel</button></li>' +
        '</ul>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>'
    };
    return directive;
}

function nDecimals() {
    return {
        require: '?ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            if (!ngModelCtrl) {
                return;
            }

            ngModelCtrl.$parsers.push(function (val) {
                if (angular.isUndefined(val)) {
                    val = '';
                }
                var maxDecimal = Number(element.attr('n-Decimals'));
                var clean = val.replace(/[^-0-9\.]/g, '');
                var negativeCheck = clean.split('-');
                var decimalCheck = clean.split('.');
                if (!angular.isUndefined(negativeCheck[1])) {
                    negativeCheck[1] = negativeCheck[1].slice(0, negativeCheck[1].length);
                    clean = negativeCheck[0] + '-' + negativeCheck[1];
                    if (negativeCheck[0].length > 0) {
                        clean = negativeCheck[0];
                    }
                }
                if (!angular.isUndefined(decimalCheck[1])) {
                    decimalCheck[1] = decimalCheck[1].slice(0, maxDecimal);
                    clean = decimalCheck[0] + '.' + decimalCheck[1];
                }
                if (val !== clean) {
                    ngModelCtrl.$setViewValue(clean);
                    ngModelCtrl.$render();
                }
                return clean;
            });
            element.bind('keypress', function (event) {
                if (event.keyCode === 32) {
                    event.preventDefault();
                }
            });
        }
    };
}


//function showErrors() {
//    return {
//        restrict: 'A',
//        require: '^form',
//        link: function (scope, el, attrs, formCtrl) {
//            var inputEl = el[0].querySelector('[name]');
//            var inputNgEl = angular.element(inputEl);
//            var inputName = inputNgEl.attr('name');
//            scope.$on('show-errors-check-validity', function () {
//                var $invalidFields = el.fv.getInvalidFields().eq(0);
//                // Get the tab that contains the first invalid field
//                var $tabPane = $invalidFields.parents('.tab-pane'),
//                    invalidTabId = $tabPane.attr('id');
//                // If the tab is not active
//                if (!$tabPane.hasClass('active')) {
//                    // Then activate it
//                    $tabPane.parents('.tab-content')
//                            .find('.tab-pane').each(function (index, tab) {
//                                console.log(tab);
//                                var tabId = $(tab).attr('id'),
//                                    $li = $('a[href="#' + tabId + '"][data-toggle="tab"]').parent();

//                                if (tabId === invalidTabId) {
//                                    // activate the tab pane
//                                    $(tab).addClass('active');
//                                    // and the associated <li> element
//                                    $li.addClass('active');
//                                } else {
//                                    $(tab).removeClass('active');
//                                    $li.removeClass('active');
//                                }
//                            });
//                    // Focus on the field
//                    $invalidFields.focus();
//                }
//                el.toggleClass('has-error', formCtrl[inputName].$invalid);
//                el.toggleClass('help-block', formCtrl[inputName].$invalid);
//                el.find('.help-block').remove();
//                el.find('.show-message').append('<p class="help-block">' + inputName + ' is required.</p>');
//            });
//            scope.$on('show-errors-reset', function () {
//                $timeout(function () {
//                    el.removeClass('has-error');
//                    el.removeClass('help-block');
//                    el.find('.show-message').remove('<p class="wrn-text help-block">' + inputName + ' is required.</p>');
//                }, 0, false);
//            });
//            inputNgEl.bind('blur', function () {
//                el.toggleClass('has-error', formCtrl[inputName].$invalid);
//            })
//        }
//    }
//}


////Tab validation

//$(document).ready(function () {
//    $('#plantForm')
//        .on('err.field.fv', function (e, data) {
//            var $invalidFields = data.fv.getInvalidFields().eq(0);
//            // Get the tab that contains the first invalid field
//            var $tabPane = $invalidFields.parents('.tab-pane'),
//                invalidTabId = $tabPane.attr('id');
//            // If the tab is not active
//            if (!$tabPane.hasClass('active')) {
//                // Then activate it
//                $tabPane.parents('.tab-content')
//                        .find('.tab-pane').each(function (index, tab) {
//                            console.log(tab);
//                            var tabId = $(tab).attr('id'),
//                                $li = $('a[href="#' + tabId + '"][data-toggle="tab"]').parent();

//                            if (tabId === invalidTabId) {
//                                // activate the tab pane
//                                $(tab).addClass('active');
//                                // and the associated <li> element
//                                $li.addClass('active');
//                            } else {
//                                $(tab).removeClass('active');
//                                $li.removeClass('active');
//                            }
//                        });
//                // Focus on the field
//                $invalidFields.focus();
//            }
//        });
//});