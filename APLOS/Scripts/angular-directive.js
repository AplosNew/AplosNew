function ngFileSelect() {
    return {
        link: function ($scope, el) {
            el.bind('change', function (e) {
                $scope.file = (e.srcElement || e.target).files[0];
                $scope.getFile();
            });
        }
    };
}
function dbl(val) {
    if (!isNaN(parseFloat(val)) && isFinite(val))
        return parseFloat(val);

    return 0;
}
function ngFileSelectMultiple() {
    return {
        link: function ($scope, el) {
            el.bind('change', function (e) {
                $scope.file = (e.srcElement || e.target).files[0];
                $scope.getFile();
            });
        }
    };
}

function ngEnter() {
    return function (scope, element, attrs) {
        element.bind('keydown keypress', function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });
                event.preventDefault();
            }
        });
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
        element.attr('autocomplete', 'off');
        element
            .addClass('datepicker')
            .datepicker({
                format: 'dd-M-yyyy'
                , autoclose: true
                , reset: true
                , todayHighlight: true
                , orientation: 'bottom'
                , startDate: attrs.startDate
                , endDate: attrs.endDate
            }).on('changeDate', function (event) {
                scope.$apply(function () {
                    ngModel.$setViewValue(event.date);
                    scope.$eval(attrs.datepicker);
                });
            });
        ngModel.$render = function () {
            element.datepicker('update', ngModel.$viewValue || '');
        };
        $timeout(function () {
            element.datepicker('update', value || '');
        }, 1);
    }
}

monthpicker.$inject = ['$parse', '$timeout'];
function monthpicker($parse, $timeout) {
    //http://jsfiddle.net/k5zookLt/1910/
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
            .datepicker({
                format: 'M-yyyy'
                , autoclose: true
                , reset: true
                , orientation: 'bottom'
                , startView: attrs.startView
                , minViewMode: 1
            }).on('changeDate', function (event) {
                scope.$apply(function () {
                    ngModel.$setViewValue(event.date);
                    scope.$eval(attrs.datepicker);
                });
            });
        ngModel.$render = function () {
            element.datepicker('update', ngModel.$viewValue || '');
        };
        $timeout(function () {
            element.datepicker('update', value || '');
        }, 1);
    }
}

togglable.$inject = ['$rootScope', '$sce'];
function togglable($rootScope, $sce) {
    var templateText = '<h3 class="site-heading"  ng-click="$root.toggle()"><span class="glyphicon glyphicon-info-sign"></span> {{name}}<a class="form-collapse pull-right" ng-class="{ collapse: isCollapsed }" ></a></h3>';

    if ($rootScope.FormTitle) {
        templateText = '<h3 class="site-heading"  ng-click="$root.toggle()"><span style="padding-left:4px;">{{name}}</span><a class="form-collapse pull-right" ng-class="{ collapse: isCollapsed }" ></a></h3>';
    }

    var directive = {
        restrict: 'E',
        template: templateText,
        replace: true,
        scope: { name: '@' },
        controller: function ($scope) {
            //$scope.iconcode = $sce.trustAsHtml($scope.iconcode);
            $rootScope.isCollapsed = false;
            var $content = angular.element('.form-elements');
            var $list = angular.element('.aplos-grid-toggle');
            $content.hide();
            $list.show();

            //try { } catch{ }
            $rootScope.toggle = function () {
                angular.element(".aplos-grid-toggle").toggleClass("collapse");
                angular.element(".form-collapse").toggleClass("expanded");
                $rootScope.isCollapsed = angular.element(".form-collapse").hasClass("expanded") ? true : false;
                $content.slideToggle();

                var scp = $rootScope.$$childTail;
                for (var prop in scp) {
                    try {
                        if ($rootScope.$$childTail[prop].pluginName == 'ejGrid') {
                            var gridObj = $("#" + prop).data("ejGrid");
                            gridObj.windowonresize();
                            gridObj.refreshContent(true);

                        }
                    } catch (e) {

                    }
                }

            };
        }
    };
    return directive;
}

function manualValidation(parentDivId, isInvalid, errorMessage) {
    var el = angular.element('#' + parentDivId);
    if (angular.isUndefined(el[0]))
        return false;
    el.toggleClass('has-error', isInvalid)
        .toggleClass('help-block', isInvalid);
    var inputEl = angular.element(el[0].querySelector('[name]'));
    inputEl.attr('manualInvalid', isInvalid)
        .attr('manualInvalidMsg', errorMessage);
    if (isInvalid) {
        el.find('.help-block').remove();
        el.find('.show-message').append('<p class="help-block">' + errorMessage + '</p>');
    }
    else {
        el.find('.help-block').remove();
    }
    return isInvalid;
}

showErrors.$inject = ['$rootScope'];
function showErrors($rootScope) {
    return {
        restrict: 'A',
        require: '^form',
        link: function (scope, el, attrs, formCtrl) {
            var inputNgEl = angular.element(el[0].querySelector('[name]'));
            var inputName = inputNgEl.attr('name');
            scope.$on('show-errors-check-validity', function () {
                var isInvalid = false;
                var msg = '';
                if (!angular.isUndefined(inputNgEl.attr('manualInvalid')) && !inputNgEl.attr('manualInvalid')) {
                    isInvalid = true;
                    msg = inputNgEl.attr('manualInvalidMsg');
                }
                else {
                    isInvalid = formCtrl[inputName].$invalid;
                    msg = inputName + ' is required.';
                }
                el.toggleClass('has-error', isInvalid)
                    .toggleClass('help-block', isInvalid)
                    .find('.help-block').remove();
                var list = el.find('.show-message');
                for (var i = 0; i < list.length; i++) {
                    var input = angular.element(list[i].querySelector('[name]'));
                    angular.element(list[i]).append('<p class="help-block">' + input.attr('name') + ' is required.' + '</p>');
                }
            });
            scope.$on('show-errors-reset', function () {
                $timeout(function () {
                    el.removeClass('has-error');
                    el.removeClass('help-block');
                    el.find('.show-message').remove('<p class="wrn-text help-block">' + inputName + ' is required.</p>');
                }, 0, false);
            });
            inputNgEl.bind('blur', function () {
                var isInvalid = false;
                if (formCtrl[inputName].$invalid)
                    isInvalid = true;
                else if (!angular.isUndefined(inputNgEl.attr('manualInvalid')) && !inputNgEl.attr('manualInvalid'))
                    isInvalid = true;
                else
                    isInvalid = false;
                el.toggleClass('has-error', isInvalid);
            });
        }
    };
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
    };
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
            callbackbuttonleft: '&ngClickLeftButton',
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
            '<button type="button" class="btn btn-default" data-ng-click="callbackbuttonleft()" data-dismiss="modal">No</button>' +
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
    };

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
    };
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
            '<i class="glyphicon glyphicon-warning-sign"></i>  {{title}}' +
            '</div>' +
            '<div class="modal-body"><p class="text-warning" ng-bind-html="message | safecontent"> <b>{{body}}</b></p></div>' +
            '<div class="modal-footer common-btn">' +
            '<button type="button" class="btn btn-default" data-ng-click="callbackbuttonleft()" data-dismiss="modal">No</button>' +
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
    };
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
    };
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
            $(element).on('click', function () {
                $(element).tooltip('hide');
            });
        }
    };
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

panelBody.$inject = ['$timeout', '$rootScope', '$window'];
function panelBody($timeout, $rootScope, $window) {
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
        controller: function ($scope, $rootScope, $element) {

            $rootScope.RenderGrid = function (args) {

                try {
                    var scrollerwidth = $("#" + args.target.id).parent().width();
                    var gridObjThis = $("#" + args.target.id).ejGrid("instance");
                    gridObjThis.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 0 } });//pass the obtainer width and height to gridmodel options

                } catch (e) {

                }
            }
            $rootScope.CurrentScope = null;
            $rootScope.resizeImage = function (svg, height, width, scalePercent) {
                try {
                    svg = $(svg)[0];

                    svg.removeAttribute('width');
                    svg.removeAttribute('height');

                    if (!scalePercent) {
                        svg.setAttribute('viewBox', '0 0 ' + width + ' ' + height);
                        svg.setAttribute('width', width + 'pt');
                        svg.setAttribute('height', height + 'pt');

                    }
                    else {
                        svg.setAttribute('viewBox', '0 0 ' + width + ' ' + height);
                        svg.setAttribute('width', scalePercent);
                        svg.setAttribute('height', scalePercent);
                        svg.setAttribute('preserveAspectRatio', 'xMinYMin meet')
                    }

                    return svg.outerHTML;
                } catch (e) {
                    return svg;
                }

            }

            $scope.ShowHideSideBar = function () {
                angular.element('.main').toggleClass('col-md-12 col-md-10 col-md-offset-2 col-sm-offset-3');
                angular.element('.sidebar').toggleClass('tiny-sidebar');
                angular.element('.navbar-site').toggleClass('navbar-site-full');
                $timeout(function () {
                    angular.element('.alert-site').css({ 'width': angular.element('.navbar-site').css('width'), 'left': angular.element('.navbar-site').css('margin-left') });
                    window.dispatchEvent(new Event('resize'));


                    var scp = $rootScope.$$childTail;
                    for (var prop in scp) {
                        try {
                            if ($rootScope.$$childTail[prop].pluginName == 'ejGrid') {

                                try {
                                    var scrollerwidth = $("#" + prop).parent().width();
                                    if (scrollerwidth > 100) {
                                        if (scrollerwidth > $("#" + prop).width()) {
                                            if (($("#" + prop).width() + 4) < scrollerwidth) {
                                                var gridObjThis = $("#" + prop).ejGrid("instance");
                                                gridObjThis.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 4, height: 0 } });//pass the obtainer width and height to gridmodel options
                                            }
                                        }
                                        else {
                                            if ((scrollerwidth + 4) < $("#" + prop).width()) {
                                                var gridObjThis = $("#" + prop).ejGrid("instance");
                                                gridObjThis.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 4, height: 0 } });//pass the obtainer width and height to gridmodel options
                                            }
                                        }
                                    }

                                    //var gridObj = $("#" + prop).data("ejGrid");
                                    //gridObj.windowonresize();
                                    //gridObj.refreshContent(true);
                                } catch (e) {

                                }
                            }
                        } catch (e) {

                        }
                    }
                }, 300);

            };
            setInterval(function () {
                try {
                    $scope.$apply();
                } catch (e) {

                }

                try {
                    var xx = document.querySelectorAll('[ej-grid]');
                    //window.dispatchEvent(new Event('resize'));
                    for (var i = 0; i < xx.length; i++) {


                        try {
                            var prop = xx[i].id;

                            try {
                                var scrollerwidth = $("#" + prop).parent().width();
                                if (scrollerwidth > 100) {
                                    //if (scrollerwidth > $("#" + prop).width()) {
                                    //if (($("#" + prop).width() + 4) < scrollerwidth) {
                                    var gridObjThis = $("#" + prop).ejGrid("instance");
                                    gridObjThis.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 4, height: 0 } });//pass the obtainer width and height to gridmodel options
                                    //    }
                                    //}
                                    //else {
                                    //    if ((scrollerwidth + 4) < $("#" + prop).width()) {
                                    //        var gridObjThis = $("#" + prop).ejGrid("instance");
                                    //        gridObjThis.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 4, height: 0 } });//pass the obtainer width and height to gridmodel options
                                    //    }
                                    //}
                                }

                                //var gridObj = $("#" + prop).data("ejGrid");
                                //gridObj.windowonresize();
                                //gridObj.refreshContent();
                            } catch (e) {


                            }
                        } catch (e) {

                        }
                    }

                } catch (e) {

                }

            }, 3000);
            angular.element(".main-nav li a").on('click', function () {
                $('div.alert').remove();
            });
        }
    };
}

panelMenu.$inject = ['$http', '$timeout', '$rootScope', '$cookies', '$sce'];
function panelMenu($http, $timeout, $rootScope, $cookies, $sce) {
    return {
        link: function ($scope) {
            $rootScope.menuModuleList = [];
            $rootScope.menuFrameList = [];
            $rootScope.ListMenuSearch = [];

            var BlankMenuIcon = '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="10pt" height="10pt" viewBox="0 0 10 10" version="1.1"><g id="surface1"><path style=" stroke:none;fill-rule:nonzero;fill:rgb(100%,100%,100%);fill-opacity:1;" d="M 0.9375 1.617188 L 0.9375 8.386719 C 0.9375 8.757812 1.242188 9.0625 1.617188 9.0625 L 8.386719 9.0625 C 8.757812 9.0625 9.0625 8.761719 9.0625 8.386719 L 9.0625 1.617188 C 9.0625 1.242188 8.761719 0.9375 8.386719 0.9375 L 1.617188 0.9375 C 1.242188 0.9375 0.9375 1.242188 0.9375 1.617188 Z M 2.679688 7.417969 C 2.425781 7.453125 2.210938 7.234375 2.242188 6.980469 C 2.265625 6.808594 2.40625 6.664062 2.578125 6.644531 C 2.835938 6.613281 3.050781 6.828125 3.015625 7.082031 C 2.996094 7.257812 2.855469 7.398438 2.679688 7.417969 Z M 2.679688 5.386719 C 2.425781 5.421875 2.210938 5.203125 2.242188 4.949219 C 2.265625 4.773438 2.40625 4.632812 2.578125 4.613281 C 2.835938 4.582031 3.050781 4.796875 3.015625 5.050781 C 2.996094 5.226562 2.855469 5.367188 2.679688 5.386719 Z M 2.679688 3.355469 C 2.425781 3.390625 2.210938 3.171875 2.242188 2.917969 C 2.265625 2.742188 2.40625 2.601562 2.578125 2.582031 C 2.835938 2.550781 3.050781 2.765625 3.015625 3.019531 C 2.996094 3.195312 2.855469 3.335938 2.679688 3.355469 Z M 7.515625 7.304688 L 4 7.304688 C 3.847656 7.304688 3.726562 7.179688 3.726562 7.03125 C 3.726562 6.882812 3.847656 6.757812 4 6.757812 L 7.515625 6.757812 C 7.664062 6.757812 7.789062 6.882812 7.789062 7.03125 C 7.789062 7.179688 7.664062 7.304688 7.515625 7.304688 Z M 7.515625 5.273438 L 4 5.273438 C 3.847656 5.273438 3.726562 5.148438 3.726562 5 C 3.726562 4.851562 3.847656 4.726562 4 4.726562 L 7.515625 4.726562 C 7.664062 4.726562 7.789062 4.851562 7.789062 5 C 7.789062 5.148438 7.664062 5.273438 7.515625 5.273438 Z M 7.515625 3.242188 L 4 3.242188 C 3.847656 3.242188 3.726562 3.117188 3.726562 2.96875 C 3.726562 2.820312 3.847656 2.695312 4 2.695312 L 7.515625 2.695312 C 7.664062 2.695312 7.789062 2.820312 7.789062 2.96875 C 7.789062 3.117188 7.664062 3.242188 7.515625 3.242188 Z M 7.515625 3.242188 "/></g></svg>';
            function rec(model) {

                for (var i = 0; i < model.length; i++) {

                }
            }

            $http({
                method: 'GET',
                url: 'Securities/userrole/getmenus?panel=' + $cookies.get('panel')
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    $rootScope.menuFrameList = response.data;

                    var iconHeight = 10;
                    var iconWidth = 10;
                    var OriginalIcon = null;

                    var linktext = '';
                    for (var MF = 0; MF < $rootScope.menuFrameList.length; MF++) {

                        var menuFrame = $rootScope.menuFrameList[MF];
                        linktext = menuFrame.ModuleName;
                        linktext += "/" + menuFrame.MenuFrameName;

                        for (var MG = 0; MG < menuFrame.MenuGroups.length; MG++) {
                            var MenuGroups = menuFrame.MenuGroups[MG];
                            linktext += "/" + MenuGroups.MenuGroupName;


                            for (var MSG = 0; MSG < MenuGroups.MenuSubGroups.length; MSG++) {

                                var MenuSubGroups = MenuGroups.MenuSubGroups[MSG];
                                linktext += "/" + MenuSubGroups.MenuSubGroupName;

                                for (var ITEM = 0; ITEM < MenuSubGroups.MenuItems.length; ITEM++) {
                                    linktext += "/" + MenuSubGroups.MenuItems[ITEM].MenuItemName;

                                    var LASTITEM = MenuSubGroups.MenuItems[ITEM];



                                    if (MenuSubGroups.MenuItems[ITEM].Image) {
                                        OriginalIcon = MenuSubGroups.MenuItems[ITEM].Image;
                                        var tempImage = $rootScope.resizeImage(MenuSubGroups.MenuItems[ITEM].Image, iconHeight, iconWidth, null);
                                        MenuSubGroups.MenuItems[ITEM].Image = $sce.trustAsHtml(tempImage);
                                    }
                                    else {
                                        OriginalIcon = BlankMenuIcon;
                                        var tempImage = $rootScope.resizeImage(BlankMenuIcon, iconHeight, iconWidth, null);
                                        MenuSubGroups.MenuItems[ITEM].Image = $sce.trustAsHtml(tempImage);
                                    }


                                    linktext = menuFrame.ModuleName + "/" +
                                        menuFrame.MenuFrameName + "/" +
                                        MenuGroups.MenuGroupName + "/" + MenuSubGroups.MenuSubGroupName + "/" + LASTITEM.MenuItemName;


                                    $rootScope.ListMenuSearch.push({ Image: OriginalIcon, Remarks: MenuSubGroups.MenuItems[ITEM].Remarks, MenuHelpDoc: MenuSubGroups.MenuItems[ITEM].MenuHelpDoc, MenuHelpDocName: MenuSubGroups.MenuItems[ITEM].MenuHelpDocName, MenuHelpDocInternalName: MenuSubGroups.MenuItems[ITEM].MenuHelpDocInternalName, Code: MenuSubGroups.MenuItems[ITEM].Code, Item: linktext, Href: MenuSubGroups.MenuItems[ITEM].Href })
                                }

                            }


                            for (var ITEM = 0; ITEM < MenuGroups.MenuItems.length; ITEM++) {

                                linktext = menuFrame.ModuleName + "/" +
                                    menuFrame.MenuFrameName + "/" +
                                    MenuGroups.MenuGroupName + "/" + MenuGroups.MenuItems[ITEM].MenuItemName;


                                if (MenuGroups.MenuItems[ITEM].Image) {
                                    OriginalIcon = MenuGroups.MenuItems[ITEM].Image;
                                    var tempImage = $rootScope.resizeImage(MenuGroups.MenuItems[ITEM].Image, iconHeight, iconWidth, null);
                                    MenuGroups.MenuItems[ITEM].Image = $sce.trustAsHtml(tempImage);
                                }
                                else {
                                    OriginalIcon = BlankMenuIcon;
                                    var tempImage = $rootScope.resizeImage(BlankMenuIcon, iconHeight, iconWidth, null);
                                    MenuGroups.MenuItems[ITEM].Image = $sce.trustAsHtml(tempImage);
                                }

                                $rootScope.ListMenuSearch.push({ Image: OriginalIcon, Remarks: MenuGroups.MenuItems[ITEM].Remarks, MenuHelpDoc: MenuGroups.MenuItems[ITEM].MenuHelpDoc, MenuHelpDocName: MenuGroups.MenuItems[ITEM].MenuHelpDocName, MenuHelpDocInternalName: MenuGroups.MenuItems[ITEM].MenuHelpDocInternalName, Code: MenuGroups.MenuItems[ITEM].Code, Item: linktext, Href: MenuGroups.MenuItems[ITEM].Href })

                            }

                        }


                        for (var ITEM = 0; ITEM < menuFrame.MenuItems.length; ITEM++) {
                            linktext = menuFrame.ModuleName + "/" +
                                menuFrame.MenuFrameName + "/" + menuFrame.MenuItems[ITEM].MenuItemName;



                            if (menuFrame.MenuItems[ITEM].Image) {
                                OriginalIcon = menuFrame.MenuItems[ITEM].Image;
                                var tempImage = $rootScope.resizeImage(menuFrame.MenuItems[ITEM].Image, iconHeight, iconWidth, null);
                                menuFrame.MenuItems[ITEM].Image = $sce.trustAsHtml(tempImage);
                            }
                            else {
                                OriginalIcon = BlankMenuIcon;
                                var tempImage = $rootScope.resizeImage(BlankMenuIcon, iconHeight, iconWidth, null);
                                menuFrame.MenuItems[ITEM].Image = $sce.trustAsHtml(tempImage);
                            }

                            $rootScope.ListMenuSearch.push({ Image: OriginalIcon, Remarks: menuFrame.MenuItems[ITEM].Remarks, MenuHelpDoc: menuFrame.MenuItems[ITEM].MenuHelpDoc, MenuHelpDocName: menuFrame.MenuItems[ITEM].MenuHelpDocName, MenuHelpDocInternalName: menuFrame.MenuItems[ITEM].MenuHelpDocInternalName, Code: menuFrame.MenuItems[ITEM].Code, Item: linktext, Href: menuFrame.MenuItems[ITEM].Href })
                        }

                    }



                    for (var t = 0; t < $rootScope.menuFrameList.length; t++) {
                        var flag = false;
                        for (var a = 0; a < $rootScope.menuModuleList.length; a++) {
                            if ($rootScope.menuFrameList[t].ModuleId === $rootScope.menuModuleList[a].ModuleId) {
                                flag = true;
                                break;
                            }
                            else
                                flag = false;
                        }
                        if (!flag)
                            $rootScope.menuModuleList.push({
                                ModuleId: $rootScope.menuFrameList[t].ModuleId
                                , ModuleName: $rootScope.menuFrameList[t].ModuleName
                            });
                    }
                    $rootScope.menuModuleList;
                    setTimeout(function () {
                        $scope.$apply(function () {
                            angular.element('.main-nav').vmenuModule({
                                Speed: 400
                                , autostart: false
                                , autohide: true
                            });
                        });
                    }, 100);
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });


        }
    };



}

// for text box focus and remove white space.
function inputFocus() {
    return {
        restrict: 'E'
        , require: '?ngModel'
        , scope: {
            inputid: '@'
        }
        , link: function ($scope, $elem, $attrs, ngModel, ngModelCtrl) {
            $elem.bind('keydown', function (event) {
                if ($attrs.inputid !== undefined) return;
                var code = event.keyCode || event.which;
                if (code === 13) {
                    $scope.$apply(function () {
                        $scope.$eval($attrs.aplosFocus);
                    });
                    angular.element(document.querySelectorAll("[tabindex='" + (parseInt($attrs.tabindex) + 1) + "']")).focus();
                    event.preventDefault();
                }
            });
            $elem.bind('blur', function () {
                if ($attrs.inputid !== undefined) return;
                $elem.val($elem.val()
                    .replace(/\s+/g, ' ')
                    //.replace(/(^\s*)|(\s*$)/gi, "")
                    //.replace(/[ ]{2,}/gi, " ")
                    //.replace(/\n /, "\n")
                );
            });
        }
    };
}

function CodeChecker() {
    return {
        restrict: 'E',
        require: '?ngModel',
        link: function ($scope, $elem, $attrs, ngModel, ngModelCtrl) {
            if ($elem[0].name === 'Code')
                $elem.bind('keypress', function (event) {
                    if ($elem[0].value.length === 0 && event.which === 48) {
                        return false;
                    }
                });
        }
    };
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
    };

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
                    return parseInt(transformedInput);
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
        restrict: 'E'
        , replace: true
        , scope: {
            modalid: '@'
            , title: '@'
            , searchParameters: '='
            , excluedColumnList: '='
            , fieldList: '='
            , dataList: '='
            , searchFunction: '='
            , singleClick: '&'
            , doubleClick: '&'
            , rightButton: '='
            , leftButton: '='
            , callbackbuttonleft: '&ngClickLeftButton'
            , callbackbuttoncancel: '&ngClickCancel'
        }
        , link: function (scope) {
            scope.hideColumn = function (value, columnList) {
                var show = true;
                if (columnList !== undefined && columnList.length > 0) {
                    for (var i = 0; i < columnList.length; i++) {
                        if (columnList.indexOf(value) !== -1) {
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
            };
        }
        , template: '<div class="modal fade site-modal in" id="{{modalid}}" role="dialog" data-backdrop="static" tabindex="-1" >' +
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
            '<option ng-repeat="option in fieldList|orderBy:\'Text\'" value="{{option.Value}}" ng-show="{{hideColumn(option.Value,excluedColumnList)}}">{{option.Text}}</option>' +
            '</select>' +
            '</div>' +
            '</div>' +
            '<div class="col-xs-12 col-sm-7 col-md-7">' +
            '<input type="text" class="form-control" placeholder="Search" ng-model="searchParameters.search" ng-enter="searchFunction();">' +
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
            '<th ng-repeat="y in fieldList" ng-show="{{hideColumn(y.Value, excluedColumnList)}}">{{y.Text}}</th>' +
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

// Plz don't remove this parameter from link: function (scope, element, attrs, ngModelCtrl)
function nDecimals() {
    return {
        require: '?ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            if (!ngModelCtrl)
                return;
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
                return parseFloat(clean);
            });
            element.bind('keypress', function (event) {
                if (event.keyCode === 32) {
                    event.preventDefault();
                }
            });
        }
    };
}

popover.$inject = ['$compile'];
function popover($compile) {
    return {
        restrict: 'A',
        link: function (scope, elem) {
            var content = $("#popover-content").html();
            var compileContent = $compile(content)(scope);
            var title = $("#popover-head").html();
            var options = {
                content: compileContent,
                html: true,
                title: title
            };
            $(elem).popover(options);
        }
    };
}

//https://gist.github.com/CMCDragonkai/6282750
headerSearch.$inject = ['$rootScope'];
function headerSearch($rootScope) {
    var directive = {
        restrict: 'E',
        replace: true,
        scope: {
            ddlModel: '@',
            searchByList: '=',
            searchModel: '&',
            searchFunction: '&ngSearch'
        },
        link: function (scope, element, attributes, $root) {
            //if (scope.ddlModel === undefined || scope.ddlModel === '') scope.ddlModel = '$root.parameters.searchBy';
            attributes.$observe('ddlModel', function (value) {
                if (scope.ddlModel === undefined || scope.ddlModel === '') scope.ddlModel = '$root.parameters.searchBy';
            });

            if (scope.searchModel === undefined || scope.searchModel === '') scope.searchModel = '$root.parameters.search';
            if (scope.searchFunction === undefined || scope.searchFunction === '') scope.searchFunction = 'getData()';
            //console.log(scope.ddlModel);
        },
        template: '<div class="row text-right">' +
            '<div class="col-xs-12 col-sm-12 col-sm-12">' +
            '<div class="search-form">' +
            '<div class="row">' +
            '<div class="col-xs-12 col-sm-3 col-md-3"><div class="select-style"><select ng-model="ddlModel"><option ng-repeat="option in searchByList" value="{{option.value}}">{{ option.name }}</option></select></div></div>' +
            '<div class="col-xs-12 col-sm-7 col-md-7"><input type="text" class="form-control" placeholder="Search" ng-model="searchModel" ng-enter="searchFunction();"></div>' +
            '<div class="col-xs-12 col-sm-2 col-md-2"><button type="button" ng-click="searchFunction();" class="btn"><i class="glyphicon glyphicon-search"></i> Search</button></div>' +
            '</div></div></div></div>'
    };
    return directive;
}

function capitalize() {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, modelCtrl) {
            var capitalize = function (inputValue) {
                if (inputValue === undefined) inputValue = '';
                var capitalized = inputValue.toUpperCase();
                if (capitalized !== inputValue) {
                    // see where the cursor is before the update so that we can set it back
                    var selection = element[0].selectionStart;
                    modelCtrl.$setViewValue(capitalized);
                    modelCtrl.$render();
                    // set back the cursor after rendering
                    element[0].selectionStart = selection;
                    element[0].selectionEnd = selection;
                }
                return capitalized;
            };
            modelCtrl.$parsers.push(capitalize);
            capitalize(scope[attrs.ngModel]); // capitalize initial value
        }
    };
}

function expand() {
    function link(scope, element, attrs) {
        scope.$on('onExpandAll', function (event, args) {
            //alert("Hi,I am working!");
            scope.expanded = args.expanded;
        });
    }
    return {
        link: link
    };
}

function childExpand() {
    function link(scope, element, attrs) {
        scope.$on('onExpandAll', function (event, args) {
            //alert("Hi,I am working!");
            scope.childExpanded = args.childExpanded;
        });
    }
    return {
        link: link
    };
} 