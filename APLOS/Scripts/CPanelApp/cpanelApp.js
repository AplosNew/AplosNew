'use strict';
angular.module('cpanelApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'angucomplete-alt', 'ejangular'])
    .controller('accCutOffDateOpeningBalanceController', accCutOffDateOpeningBalanceController)
    .controller('hrCutOffDateOpeningBalanceController', hrCutOffDateOpeningBalanceController)
    .controller('smtpConfigurationController', smtpConfigurationController)
    .controller('areaController', areaController)
    .controller('cityController', cityController)
    .controller('continentController', continentController)
    .controller('countryController', countryController)
    .controller('stateController', stateController)
    .controller('districtController', districtController)
    .controller('postOfficeController', postOfficeController)
    .controller('policeStationController', policeStationController)
    .controller('companyController', CompanyController)
    .controller('companyGroupController', CompanyGroupController)
    .controller('plantController', PlantController)
    .controller('lineController', LineController)
    .controller('organizationCategoryController', OrganizationCategoryController)
    .controller('organizationClassController', OrganizationClassController)
    .controller('unitController', UnitController)
    .controller('companyCoaController', CompanyCoaController)
    .controller('entityRelationshipController', EntityRelationshipController)
    .controller('positionRelationshipController', positionRelationshipController)
    .controller('moduleController', ModuleController)
    .controller('subModuleController', SubModuleController)
    .controller('moduleExtendedController', ModuleExtendedController)
    .controller('companyGroupModuleController', CompanyGroupModuleController)
    .controller('moduleAppController', ModuleAppController)
    .controller('companyGroupModuleAppController', CompanyGroupModuleAppController)
    .controller('prerecruitmentUrlController', PrerecruitmentUrlController)
    .controller('companyGroupCurrencyController', CompanyGroupCurrencyController)
    .controller('currencyController', CurrencyController)
    .controller('menuFrameController', MenuFrameController)
    .controller('menuGroupController', MenuGroupController)
    .controller('menuItemController', MenuItemController)
    .controller('menuMasterController', MenuMasterController)
    .controller('menuMasterEditController', MenuMasterEditController)
    .controller('menuSubGroupController', MenuSubGroupController)
    .controller('menuController', MenuController)
    .controller('menuActionController', MenuActionController)
    .controller('menuCreationController', MenuCreationController)
    .controller('companyGroupMenuMasterController', CompanyGroupMenuMasterController)
    .controller('controlAdminController', ControlAdminController)
    .controller('controlAdminPasswordResetController', ControlAdminPasswordResetController)
    .controller('systemAdminController', SystemAdminController)
    .controller('systemAdminAuthtokenController', SystemAdminAuthtokenController)
    .controller('systemAdminResetController', SystemAdminResetController)
    .controller('authTokenLockLogController', AuthTokenLockLogController)
    .controller('userLockLogController', UserLockLogController)
    .controller('encryptDecrypttController', EncryptDecryptController)
    .controller('sysAuthTokenLockController', SysAuthTokenLockController)
    .controller('sysLockController', SysLockController)
    .controller('cPasswordChangeController', CPasswordChangeController)
    .controller('uOMDimensionController', UOMDimensionController)
    .controller('plantConfigController', PlantConfigController)
    .controller('prdOrdSettingController', prdOrdSettingController)
    .controller('languageController', languageController)
    .controller('localLanguageLabelController', localLanguageLabelController)
    .controller('accountTypeController', AccountTypeController)
    .controller('businessProcessController', BusinessProcessController)
    .controller('holidayCategoryController', HolidayCategoryController)
    .controller('accessLogController', AccessLogController)
    .controller('actionLogController', ActionLogController)
    .controller('errorLogController', ErrorLogController)
    .controller('mailLogController', mailLogController)
    .controller('qualificationLevelController', QualificationLevelController)
    .controller('qualificationStreamController', QualificationStreamController)
    .controller('employeeJobDescriptionController', employeeJobDescriptionController)
    .controller('bloodGroupController', bloodGroupController)
    .controller('civilStatusController', civilStatusController)
    .controller('religionController', religionController)
    .controller('mailSendController', mailSendController)
    .controller('cpanelLoginController', cpanelLoginController)
    .controller('cpanelLogoutController', cpanelLogoutController)
    .controller('queryEditorController', queryEditorController)
    .controller('showAllUserController', showAllUserController)
    .controller('buyerActivityController', buyerActivityController)
    .controller('inquiryActivityController', inquiryActivityController)
    .controller('salutationController', salutationController)
    .controller('chartOfAccountRelationshipController', ChartOfAccountRelationshipController)
    .controller('relationshipController', relationshipController)
    .controller('professionController', professionController)
    .controller('rptConfigTemplateController', rptConfigTemplateController)
    .controller('notificationURLController', notificationURLController)
    .controller('menuSyncController', menuSyncController)
    .controller("deleteAccCutOffDateBackDataController", deleteAccCutOffDateBackDataController)
    .controller("PositionGroupingDataController", PositionGroupingDataController)
    .controller("companyParallelCurrencyController", CompanyParallelCurrencyController)
    .controller("syncURLController", syncURLController)
    .controller("LabelListController", LabelListController)
    .controller('EmployeeCodeGenerationController', EmployeeCodeGenerationController)
    .controller('WeekDefinitionController', WeekDefinitionController)
    .controller('EmployeeCodeTypeController', EmployeeCodeTypeController)
    .controller('contractFundUtilizationController', contractFundUtilizationController)
    .controller('OrderLineCostingItemController', OrderLineCostingItemController)
    .controller("voucherTypeController", VoucherTypeController)
    .controller("CostingComponentController", CostingComponentController)
    .controller("LICController", LICController)
    .controller("voucherTypeMatrixController", VoucherTypeMatrixController)
    .controller("voucherTypeConfigController", voucherTypeConfigController)
    .controller("defineEnumController", defineEnumController)
    .controller("RemarksControlController", RemarksControlController)

    .config(HumanResourceConfig)
    .config(accountConfig)
    .config(addressConfig)
    .config(employeeConfig)
    .config(CurrencyConfig)
    .config(MenuConfig)
    .config(ModuleConfig)
    .config(OrganizationConfig)
    .config(SecurityConfig)
    .config(SetupConfig)
    .config(LogsConfig)
    .config(employeeConfig)
    .config(CommercialConfig)
    .config(CostingsConfig)
    .config(OrderManagementConfig)
    .config(['$routeProvider', '$locationProvider', '$httpProvider', function cpanelConfig($routeProvider, $locationProvider, $httpProvider) {
        $httpProvider.interceptors.push('errorInterceptor');
        $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
        $routeProvider
            .when('cpanel', {
                templateUrl: 'cpanel/dashboard'
            })
            .when('/', {
                templateUrl: 'cpanel/dashboard'
            })
            .when('/dashboard', {
                templateUrl: 'cpanel/dashboard'
            })
            .when('/login', {
                templateUrl: 'cpanel/login',
                controller: 'cpanelLoginController'
            })
            .when('/logout', {
                template: ' ',
                controller: 'cpanelLogoutController'
            })
            .when('/404/:msg', {
                templateUrl: function (params) {
                    return 'error/httperror404?message=' + params.msg;
                }
            })
            .when('/405/:msg', {
                templateUrl: function (params) {
                    return 'error/httperror405?message=' + params.msg;
                }
            })
            .when('/query-editor', {
                templateUrl: 'cpanel/QueryEditor',
                controller: 'queryEditorController'
            })
            .otherwise({
                redirectTo: 'cpanel/login'
            });
    }])
    .run(['$rootScope', function ($rootScope) {
        $rootScope.title = 'cPanel';
        $rootScope.bootPoint = '#!/';
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
    .filter('safecontent', safecontent)
    .filter('dateFiltering', dateFiltering)
    .filter('dateFilter', dateFilter)
    .filter('sumByKey', sumByKey)
    .filter('myDate', myDate)
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
    .factory('errorInterceptor', errorInterceptor)
    .factory('baseService', baseService)
    .factory('cboService', cboService)
    .factory('fileReader', fileReader)
    .factory('exportToExcel', exportToExcel)
    .factory('addressService', addressService)
    .constant('commonMessage', commonMessage)
    ;