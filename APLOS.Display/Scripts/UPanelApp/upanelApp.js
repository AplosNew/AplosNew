"use strict";
var upanelApp = angular
    .module("upanelApp", ["ngRoute", "ngCookies", "angularUtils.directives.dirPagination", "toaster", "angucomplete-alt", "angularjs-dropdown-multiselect", "ejangular"])

    .controller("LineDesignerController", LineDesignerController)
    .controller("DailyProductionDisplayController", DailyProductionDisplayController)

    .config(IEConfig)
    .config(SecurityConfig)


    .config(["$routeProvider", "$locationProvider", "$httpProvider", function apanelConfig($routeProvider, $locationProvider, $httpProvider) {
        // $httpProvider.interceptors.push("errorInterceptor");
        $httpProvider.defaults.headers.common["X-Requested-With"] = "XMLHttpRequest";

    }])
    .run(["$rootScope", "$cookies", "$window", "$location", "$filter", "baseService", "$http", '$sce', 'SignalRInit',
        function ($rootScope, $cookies, $window, $location, $filter, baseService, $http, $sce, SignalRInit) {
            function getCookie(cname) {
                var name = cname + "=";
                var decodedCookie = decodeURIComponent(document.cookie);
                var ca = decodedCookie.split(';');
                for (var i = 0; i < ca.length; i++) {
                    var c = ca[i];
                    while (c.charAt(0) === ' ') {
                        c = c.substring(1);
                    }
                    if (c.indexOf(name) === 0)
                        return c.substring(name.length, c.length);
                }
                return "";
            }

            function isImage(src) {

                var deferred = $q.defer();

                var image = new Image();
                image.onerror = function () {
                    deferred.resolve(false);
                };
                image.onload = function () {
                    deferred.resolve(true);
                };
                image.src = src;

                return deferred.promise;
            }
            $rootScope.ShowHomeButton = true;
            $rootScope.FormTitle = "Application";
            $rootScope.FormIcon = null;
            $rootScope.NotificationMessage = '';
            $rootScope.plantName = $cookies.get("plantName");
            $rootScope.bootPoint = "#!/";
            $window.companyGroupId = $cookies.get("groupId");
            $window.authenticationToken = $cookies.get("authToken");
            $window.companyId = $cookies.get("companyId");
            $window.plantId = $cookies.get("plantId");
            $window.employeeId = $cookies.get("employeeId");
            $rootScope.CompanyLogo = null;
            $rootScope.CompanyFullName = null;
            $rootScope.companyGroupLogo = virtualPath.LogoOrImage + $cookies.get("gImage");
            $rootScope.userImage = virtualPath.EmployeeImage + $cookies.get("userImage");
            $rootScope.showMenu = "Module";
            $rootScope.menuModuleId = null;
            $rootScope.isLeftMenuHide = $rootScope.plantName === null || $rootScope.plantName === undefined ? true : false;
            $rootScope.ShowFavouriteMenu = $rootScope.isLeftMenuHide;
            SignalRInit.connect();

            $rootScope.moduleShowHide = function () {

                $rootScope.menuModuleName = null;
                if ($rootScope.showMenu === "Menu")
                    $rootScope.showMenu = "Module";
            };
            $rootScope.getPageTitle = function (name) {
                $rootScope.FormTitle = name;
            };

            $rootScope.CurrentMenuMasterId = null;
            $rootScope.CurrentHref = '';
            $rootScope.$on('$routeChangeStart', function ($event, next, current) {
                try {
                    var href = next.$$route.originalPath;
                    $rootScope.FormIcon = null;
                    $rootScope.ChangeHref(href);
                } catch (e) {

                }

            });


            setInterval(function () {
                $('#datetimedisplay').text(moment(new Date()).format('DD-MMM-yyyy hh:mm:ss A'));
            }, 1000);
            $rootScope.$on('$viewContentLoaded', function () {
                //Here, our content is fully loaded !!
                if ($rootScope.ListMenuSearch.length > 0)
                    if ('/' + $window.location.href.substr(window.location.href.lastIndexOf('/') + 1).toLowerCase() != $rootScope.CurrentHref.toLowerCase())
                        $rootScope.ChangeHref('/' + $window.location.href.substr(window.location.href.lastIndexOf('/') + 1));


                if ($("h3 .glyphicon").hasClass('glyphicon')) {
                    $("h3 .glyphicon").removeAttr('class');
                    $("h3").css("padding-left", "4px");
                }
            });





            angular.isUndefinedOrNull = function (val) {
                return angular.isUndefined(val) || val === null || val === "";
            };

            $rootScope.template =
                '<div class="row" style="display:inline-box;">'
                + '    <div style="float:left;padding-left:10px;" class="glyphicon glyphicon-list"> '
                + '    </div>                                                                              '
                + '    <div style="float:left;padding-left:10px;">                                          '
                + '        ${Item}                                                                        '
                + '        </div>                                                                          '
                + '</div>                                                                                  ';
            $rootScope.tocode = function (args) {
                location.href = $rootScope.bootPoint + args.item.Href;
                $("#AutoCompleteMenuSearch").ejAutocomplete("clearText");
            }

            $rootScope.report = function (file_src) {
                $("#iframe_div_for_report").empty();
                var frame = $('<iframe id="report">')
                    .attr('height', '0px')
                    .attr('visibility', 'hidden')
                    .attr('width', '0px');
                frame.on('load', function () {

                    try {
                        var text = angular.fromJson($('#report')[0].contentDocument.body.innerText);

                        if (text.hasOwnProperty('Message')) {
                            if (angular.isUndefinedOrNull(text.Message) === false) {
                                $('<div id="message">').attr('height', '0px')
                                    .attr('visibility', 'hidden')
                                    .attr('width', '0px').appendTo('#iframe_div_for_report');
                                $("#message").ejDialog({
                                    title: "Error"
                                });
                                $("#message").ejDialog("setContent", text.Message);

                            }
                        }
                        else {
                            var text1 = $('#report')[0].contentDocument.body.innerText;

                            $('<div id="message">').attr('height', '0px')
                                .attr('visibility', 'hidden')
                                .attr('width', '0px').appendTo('#iframe_div_for_report');
                            $("#message").ejDialog({
                                title: "Error"
                            });
                            $("#message").ejDialog("setContent", text1);
                        }

                    } catch (e) {


                    }

                });


                frame.attr('src', file_src);
                frame.appendTo('#iframe_div_for_report');
            };
        }])
    .filter("unique", unique)
    .filter("dateFilter", dateFilter)
    .filter("dateFiltering", dateFiltering)
    .filter("trustUrl", trustUrl)
    .filter("safecontent", safecontent)
    .filter("sumByKey", sumByKey)
    .filter("setDecimal", setDecimal)
    .filter("groupBy", groupBy)
    .filter("makePositive", makePositive)
    .filter("searchFilter", searchFilter)
    .directive("panelBody", panelBody)
    .directive("panelMenu", panelMenu)
    .directive("nDecimals", nDecimals)
    .directive("datepicker", datepicker)
    .directive("monthpicker", monthpicker)
    .directive("togglable", togglable)
    .directive("showErrors", showErrors)
    .directive("compile", compile)
    .directive("archiveRow", archiveRow)
    .directive("confirmModal", confirmModal)
    .directive("confirmArchive", confirmArchive)
    .directive("confirmArchiveGeneric", confirmArchiveGeneric)
    .directive("loader", loader)
    .directive("tooltip", tooltip)
    .directive("input", inputFocus)
    .directive("textarea", inputFocus)
    .directive("select", inputFocus)
    .directive("input", CodeChecker)
    .directive("ngEnter", ngEnter)
    .directive("stringToNumber", stringToNumber)
    .directive("inputMaxLengthNumber", inputMaxLengthNumber)
    .directive("confirmCancel", confirmCancel)
    .directive("onlyNumbers", onlyNumbers)
    .directive("ngFileSelect", ngFileSelect)
    .directive("modalTable", modalTable)
    .directive("manualValidation", manualValidation)
    .directive("popover", popover)
    .directive("capitalize", capitalize)
    .directive("expand", expand)
    .directive("childExpand", childExpand)
    .factory("errorInterceptor", errorInterceptor)
    .factory("baseService", baseService)
    .factory("cboService", cboService)
    .factory("factoryService", factoryService)
    .factory("fileReader", fileReader)
    .factory('signalR', signalR)
    .factory("SignalRInit", SignalRInit)
    .constant("commonMessage", commonMessage)

    ;