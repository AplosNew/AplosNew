"use strict";
var mpanelApp = angular
    .module("mpanelApp", ["ngRoute", "ngCookies", "angularUtils.directives.dirPagination", "toaster", "angucomplete-alt", "angularjs-dropdown-multiselect","ejangular"])
    .controller("paymentTermController", PaymentTermController)
    .controller("customerInvoiceOpeningBalanceController", customerInvoiceOpeningBalanceController)
    .controller("customerAdvanceOpeningBalanceController", customerAdvanceOpeningBalanceController)
    .controller("vendorInvoiceOpeningBalanceController", vendorInvoiceOpeningBalanceController)
    .controller("vendorAdvanceOpeningBalanceController", vendorAdvanceOpeningBalanceController)
    .controller("securityDepositGivenOpeningBalanceController", securityDepositGivenOpeningBalanceController)
    .controller("securityDepositTakenOpeningBalanceController", securityDepositTakenOpeningBalanceController)
    .controller("loanTakenOpeningBalanceController", loanTakenOpeningBalanceController)
    .controller("loanGivenOpeningBalanceController", loanGivenOpeningBalanceController)
    .controller("investmentTakenOpeningBalanceController", investmentTakenOpeningBalanceController)
    .controller("investmentGivenOpeningBalanceController", investmentGivenOpeningBalanceController)
    .controller("interInvestmentGivenOpeningBalanceController", interInvestmentGivenOpeningBalanceController)
    .controller("interPlantInvestmentTakenOpeningBalanceController", interPlantInvestmentTakenOpeningBalanceController)
    .controller("interCompanyInvestmentTakenOpeningBalanceController", interCompanyInvestmentTakenOpeningBalanceController)
    .controller("employeeAdvanceOpeningBalanceController", employeeAdvanceOpeningBalanceController)
    .controller("employeePayableOpeningBalanceController", employeePayableOpeningBalanceController)
    .controller("interTransactionGivenOpeningBalanceController", interTransactionGivenOpeningBalanceController)
    .controller("interPlantTransactionTakenOpeningBalanceController", interPlantTransactionTakenOpeningBalanceController)
    .controller("interCompanyTransactionTakenOpeningBalanceController", interCompanyTransactionTakenOpeningBalanceController)
    .controller("journalOpeningBalanceController", journalOpeningBalanceController)
    .controller("advanceJournalOpeningBalanceController", advanceJournalOpeningBalanceController)
    .controller("openingBalanceReportController", openingBalanceReportController)
    .controller("budgetMasterController", budgetMasterController)
    .controller("budgetMasterFARegisterController", budgetMasterFARegisterController)
    .controller("annualBudgetController", annualBudgetController)
    .controller("fiscalYearBaseController", fiscalYearBaseController)
    .controller('TaskAppliedOnController', TaskAppliedOnController)
    .controller('TaskDependentDatesController', TaskDependentDatesController)
    .controller('hourlyLeaveReasonController', hourlyLeaveReasonController)
    .controller('AttendanceRawDataDeleteController', AttendanceRawDataDeleteController)
    .controller('DailyAllowanceSettingController', DailyAllowanceSettingController)
    .controller('DateRangeWiseAttendanceUnLockController', DateRangeWiseAttendanceUnLockController)
    // Banks
    .controller("bankBaseController", bankBaseController)
    .controller("bankOpeningBalanceController", bankOpeningBalanceController)
    .controller("cashBaseController", cashBaseController)
    .controller("cashOpeningBalanceController", cashOpeningBalanceController)
    .controller('IndividualAttendanceUnLockController', IndividualAttendanceUnLockController)
    .controller("fixedAssetRegisterController", fixedAssetRegisterController)
    .controller("fixedAssetRegisterJVOBController", fixedAssetRegisterJVOBController)
    .controller("fixedAssetRegisterJVController", fixedAssetRegisterJVController)
    .controller("fixedAssetRegisterAUCJVController", fixedAssetRegisterAUCJVController)
    .controller("fixedAssetMasterOpeningBalanceController", fixedAssetMasterOpeningBalanceController)
    .controller("assetItemController", AssetItemController)
    .controller("assetItemArticleController", assetItemArticleController)
    .controller("operationController", OperationController)
    .controller("operationVariationController", operationVariationController)
    .controller("entityOperationSettingsController", entityOperationSettingsController)
    .controller("thirdPartyOperationController", ThirdPartyOperationController)
    .controller("skillController", SkillController)
    .controller("machineController", machineController)
    .controller("interCompanyPartyController", InterCompanyPartyController)
    .controller("partyController", PartyController)
    .controller("partyBaseController", partyBaseController)
    .controller('partyGroupController', PartyGroupController)
    .controller('partyGroupCategoryController', PartyGroupCategoryController)
    .controller('partyGroupSubCategoryController', PartyGroupSubCategoryController)
    .controller('partyGroupClassController', PartyGroupClassController)
    .controller('partyMappingController', partyMappingController)
    .controller("partyReportController", partyReportController)
    //.controller("buyerStyleController", BuyerStyleController)
    .controller("workCenterBuyerTagController", WorkCenterBuyerTagController)
    .controller("timeCaptureController", TimeCaptureController)
    .controller("bulletinController", BulletinController)
    .controller("operationVideoUploadController", OperationVideoUploadController)
    .controller("characteristicsValueController", CharacteristicsValueController)
    .controller("defectCodeController", DefectCodeController)
    .controller("materialAttributeValueController", MaterialAttributeValueController)
    .controller("baseMaterialAndArticleController", baseMaterialAndArticleController)
    .controller("baseAttributeAndCharacteristicsValueController", baseAttributeAndCharacteristicsValueController)
    .controller("materialMasterController", MaterialMasterController)
    .controller("fabricRollManagementSettingsController", fabricRollManagementSettingsController)
    .controller("materialMasterArticleController", materialMasterArticleController)
    .controller("materialGroupMasterController", MaterialGroupMasterController)
    .controller("materialAttributeMasterController", MaterialAttributeMasterController)
    .controller("ourStyleController", OurStyleController)
    .controller("characteristicsWisePropertiesController", CharacteristicsWisePropertiesController)
    .controller("materialMasterReportController", MaterialMasterReportController)
    .controller("materialStockController", materialStockController)
    .controller("productCategoryController", ProductCategoryController)
    .controller("productSubCategoryController", ProductSubCategoryController)
    .controller("productController", ProductController)
    .controller("productSubCategoryAttributeController", ProductSubCategoryAttributeController)
    .controller("productMasterController", ProductMasterController)
    .controller("productDefinitionController", productDefinitionController)
    .controller("shiftAssignmentController", shiftAssignmentController)
    .controller("unitController", UnitController)
    .controller("divisionController", DivisionController)
    .controller("companyDivisionController", CompanyDivisionController)
    .controller("subDivisionController", SubDivisionController)
    .controller("companySubDivisionController", CompanySubDivisionController)
    .controller("departmentController", DepartmentController)
    .controller("companyDepartmentController", CompanyDepartmentController)
    .controller("sectionController", SectionController)
    .controller("companySectionController", CompanySectionController)
    .controller("subSectionController", SubSectionController)
    .controller("companySubSectionController", CompanySubSectionController)
    .controller("lineController", LineController)
    .controller("companyLineController", CompanyLineController)
    .controller("testingController", TestingController)
    .controller("testingStandardController", TestingStandardController)
    .controller("testingStandardReportController", TestingStandardReportController)
    .controller("recruitmentPlanningController", recruitmentPlanningController)
    .controller("designationMasterController", DesignationMasterController)
    .controller("companyDesignationController", CompanyDesignationController)
    .controller("processSetReportController", ProcessSetReportController)
    .controller("shipModeController", ShipModeController)
    .controller("portController", PortController)
    .controller("destinationController", DestinationController)
    //.controller("employeeJobDescriptionController", employeeJobDescriptionController)
    .controller("currencyBaseController", currencyBaseController)
    .controller("baseOpeningBalanceController", baseOpeningBalanceController)
    .controller("employeeInformationController", employeeInformationController)
    .controller("employeeBaseController", employeeBaseController)
    .controller("employeeBankInformationController", employeeBankInformationController)
    .controller('biometricDeviceAsAccessListController', biometricDeviceAsAccessListController)
    .controller('biometricDeviceAsShortLeaveController', biometricDeviceAsShortLeaveController)
    .controller("mpanelDashboardController", mpanelDashboardController)
    .controller("plantSelectionController", plantSelectionController)
    .controller("salaryFixationController", salaryFixationController)
    .controller("companyTaxContributionController", companyTaxContributionController)
    .controller("plantWiseTermsAndConditionsController", plantWiseTermsAndConditionsController)
    .controller("plantWiseLetterTemplateController", plantWiseLetterTemplateController)
    .controller("interLoanGivenOpeningBalanceController", interLoanGivenOpeningBalanceController)
    .controller("interPlantLoanTakenOpeningBalanceController", interPlantLoanTakenOpeningBalanceController)
    .controller("interCompanyLoanTakenOpeningBalanceController", interCompanyLoanTakenOpeningBalanceController)
    .controller("routeController", routeController)
    .controller('employeedocumentAddRemoveController', employeedocumentAddRemoveController)
    .controller("materialMasterOpeningBalanceController", materialMasterOpeningBalanceController)
    .controller('serviceMasterController', ServiceMasterController)
    .controller('companyServiceMasterController', companyServiceMasterController)
    .controller('employeeSalaryRuleEditableController', EmployeeSalaryRuleEditableController)
    .controller('leaveOpeningBalanceController', LeaveOpeningBalanceController)
    .controller("accessControllerEmployeeTagController", accessControllerEmployeeTagController)
    .controller("employeeDeviceController", employeeDeviceController)
    .controller('manpowerBudgetController', manpowerBudgetController)
    .controller('buyerMasterController', BuyerMasterController)
    .controller('userPasswordChangeController', UserPasswordChangeController)
    .controller("mpanelLoginController", mpanelLoginController)
    .controller("mpanelLogoutController", mpanelLogoutController)
    .controller('pFEmployeeAppliedController', PFEmployeeAppliedController)
    .controller('pFEmployeeVoluntaryValueController', PFEmployeeVoluntaryValueController)
    .controller('payrollGroupMasterController', payrollGroupMasterController)
    .controller("paidHoursEmployeeAssignController", paidHoursEmployeeAssignController)
    .controller("employeeIdCardController", employeeIdCardController)
    //.controller("buttonSalesOrderPackingListController", ButtonSalesOrderPackingListController)
    .controller("compliedshiftController", CompliedShiftController)
    .controller("compliedShiftGroupingController", CompliedShiftGroupingController)
    .controller("compliedShiftAssignmentController", compliedShiftAssignmentController)
    .controller("BonusPolicyMonthlyRetainEligibleEmployeeController", BonusPolicyMonthlyRetainEligibleEmployeeController)
    .controller("operationMotionController", operationMotionController)
    .controller("employeeReportInfoController", employeeReportInfoController)
    .controller("fabricRollMasterController", fabricRollMasterController)
    .controller("machineAttributeController", machineAttributeController)
    .controller("stitchCodeController", stitchCodeController)
    .controller("operationMotionController", operationMotionController)
    .controller("glMappingController", glMappingController)
    .controller("productionSystemController", productionSystemController)
    .controller("customerDivisionController", customerDivisionController)
    .controller("complianceShiftRotationController", complianceShiftRotationController)
    .controller("workCenterMasterController", WorkCenterMasterController)
    .controller("dailyComplianceReportController", dailyComplianceReportController)
    .controller("employeeAttendanceGroupController", employeeAttendanceGroupController)
    .controller('AttendanceDeviceZoneController', AttendanceDeviceZoneController)
    .controller('materialMasterWithProductMasterController', materialMasterWithProductMasterController)
    .controller("EmployeeLockAndUnLockController", EmployeeLockAndUnLockController)
    //.controller("PlantWiseAttendanceLockController", PlantWiseAttendanceLockController)
    .controller("PlantWiseAttendanceUnLockController", PlantWiseAttendanceUnLockController)
    .controller("EmployeeAndPlantWiseAttendanceUnLockController", EmployeeAndPlantWiseAttendanceUnLockController)
    .controller("EmployeeProfileUnApprovalController", EmployeeProfileUnApprovalController)
    .controller("EmployeeProfileApprovalController", EmployeeProfileApprovalController)
    .controller("purchaseOrderGroupController", purchaseOrderGroupController)
    .controller("ProcurementController", ProcurementController)
    .controller("MaterialBudgetController", MaterialBudgetController)
    .controller("runningOrderParametersController", runningOrderParametersController)
    .controller("employeeAdvanceRequisitionController", employeeAdvanceRequisitionController)
    .controller('issueStandardController', issueStandardController)
    .controller('plantWiseGateController', plantWiseGateController)
    .controller('authorizationConfigController', authorizationConfigController)
    .controller('issueStandardController', issueStandardController)
    .controller('entityTaskController', entityTaskController)
    .controller('exceptionEmployeeController', exceptionEmployeeController)
    .controller('DailyAllowanceController', DailyAllowanceController)
    .controller('TaskCategoryIssueController', TaskCategoryIssueController)
    .controller('TaskSubCategoryController', TaskSubCategoryController)
    .controller('TaskSubCategoryIssueController', TaskSubCategoryIssueController)
    .controller('TaskSubCategoryToDoController', TaskSubCategoryToDoController)
    .controller('TaskCategoryController', TaskCategoryController)
    .controller('TaskSubCategoryController', TaskSubCategoryController)
    .controller('TaskCategoryToDoController', TaskCategoryToDoController)
    .controller('TaskCategoryToDoController', TaskCategoryToDoController)
    .controller('TaskSubCategoryToDoController', TaskSubCategoryToDoController)
    .controller('DailyAllowanceRateEmpWiseController', DailyAllowanceRateEmpWiseController)
    .controller('issueImportanceController', issueImportanceController)
    .controller("machineMasterUIController", machineMasterUIController)
    .controller('skillGroupingController', skillGroupingController)
    .controller("OperationMasterController", OperationMasterController)
    .controller("allowanceDailyController", allowanceDailyController)
    .controller('qualityStdSetController', qualityStdSetController)
    .controller('issueStandardController', issueStandardController)
    .controller('issueGroupController', issueGroupController)

    .controller('costingItemController', costingItemController)
    .controller('TaskMasterCreationController', TaskMasterCreationController)
    .controller('TaskTemplateController', TaskTemplateController)
    .controller('leavePolicyController', leavePolicyController)
    .controller('EncashmentController', EncashmentController)
    .controller('SalaryStructureDataUploadController', SalaryStructureDataUploadController)
    .controller("stoppageController", stoppageController)
    .controller("ExceptionForHolidayController", ExceptionForHolidayController)
    .controller("LayOffController", LayOffController)
    .controller("shiftTimeChangeController", shiftTimeChangeController)
    .controller("attendanceBonusPolicyController", attendanceBonusPolicyController)
    .controller("nonFinancialMaterialOpeningBalancePostController", nonFinancialMaterialOpeningBalancePostController)
    .controller('quickCostingMasterController', quickCostingMasterController)
    .controller('AttendanceRawDataUploadController', AttendanceRawDataUploadController)
    .controller('EmployeeProfileUploadController', EmployeeProfileUploadController)
    .controller('PhysicalStockAdjustmentMasterController', PhysicalStockAdjustmentMasterController)
    .controller('ActivityMasterController', ActivityMasterController)
    .controller('disciplinaryActionCriticalityController', disciplinaryActionCriticalityController)
    .controller('QMSDefectMasterController', QMSDefectMasterController)
    
    .controller('leaveTypeController', leaveTypeController)
    .controller('jobLocationController', jobLocationController)
    
    
    .controller('QMSMasterController', QMSMasterController)
    
    .controller('SecretarialDocumentCategoryController', SecretarialDocumentCategoryController)
    .controller('SecretarialDocumentSubCategoryController', SecretarialDocumentSubCategoryController)


    
    .controller('disciplinaryActionCategoryController', disciplinaryActionCategoryController)
    .controller('RestTypeController', RestTypeController)
    .controller('complianceAttendanceSettingController', complianceAttendanceSettingController)

    .controller('ICSMasterController', ICSMasterController)
    .controller('FarmerMasterController', FarmerMasterController)
    .controller('CropMasterController', CropMasterController)

    .controller('BOMMasterController', BOMMasterController)
    .controller('FinalSettlementDeductionHeadController', FinalSettlementDeductionHeadController)
    .controller('DeviceRawDataDownloadController', DeviceRawDataDownloadController)
    

    .controller('CurrencyRuleController', CurrencyRuleController)
    .controller('SandWichLeaveOnHolidayController', SandWichLeaveOnHolidayController)

    .controller('otSlabController', otSlabController)
    .controller('PFPolicyController', PFPolicyController)
    .controller('gratuityPolicyController', gratuityPolicyController)


    .config(CostingsConfig)
    .config(CommercialConfig)
    .config(HumanResourceConfig)
    .config(leaveConfig)
    .config(IssueTrackerConfig)
    .config(BiometricConfig)
    .config(AccessControllerConfig)
    .config(accountConfig)
    .config(WorkCenterConfig)
    .config(OrderManagementConfig)
    .config(bankConfig)
    .config(fixedAssetConfig)
    .config(IEConfig)
    .config(MachineConfig)
    .config(MaterialConfig)
    .config(OrganizationConfig)
    .config(SetupConfig)
    .config(ProductConfig)
    .config(PartyConfig)
    .config(ProcessConfig)
    .config(ProductionsConfig)
    .config(SkillConfig)
    .config(employeeConfig)
    .config(PayrollsConfig)
    .config(SecurityConfig)
    .config(IssueTrackerConfig)
    .config(TaskManagementConfig)
    .config(qmsConfig)
    .config(FarmingConfig)
    .config(["$routeProvider", "$locationProvider", "$httpProvider", function apanelConfig($routeProvider, $locationProvider, $httpProvider) {
        $httpProvider.interceptors.push("errorInterceptor");
        $httpProvider.defaults.headers.common["X-Requested-With"] = "XMLHttpRequest";
        $routeProvider
            .when("/", {
                templateUrl: "MPanel/PlantSelection",
                controller: "plantSelectionController"
            })
            .when("/plant-selection", {
                templateUrl: "MPanel/PlantSelection",
                controller: "plantSelectionController"
            })
            .when("/dashboard", {
                templateUrl: "MPanel/dashboard",
                controller: "mpanelDashboardController"
            })
            .when("/login", {
                templateUrl: "aPanel/login",
                controller: "mpanelLoginController"
            })
            .when("/logout", {
                template: " ",
                controller: "mpanelLogoutController"
            })
            .when("/404/:msg", {
                templateUrl: function (params) {
                    return "error/httperror404?message=" + params.msg;
                }
            })
            .when("/405/:msg", {
                templateUrl: function (params) {
                    return "error/httperror405?message=" + params.msg;
                }
            })
            .otherwise({
                redirectTo: "/portal"
            });
    }])
    .run(["$rootScope", "$cookies", "$window", "$filter", function ($rootScope, $cookies, $window, $filter) {
        $rootScope.title = "Master";
        $rootScope.plantName = $cookies.get("plantName");
        $rootScope.bootPoint = "#!/";
        $window.companyGroupId = $cookies.get("groupId");
        $window.authenticationToken = $cookies.get("authToken");
        $window.companyId = $cookies.get("companyId");
        $window.employeeId = $cookies.get("employeeId");
        $window.plantId = $cookies.get("plantId");
        $rootScope.CompanyLogo = null;
        $rootScope.CompanyFullName = null;
        $rootScope.companyGroupLogo = virtualPath.LogoOrImage + $cookies.get("gImage");
        $rootScope.userImage = virtualPath.EmployeeImage + $cookies.get("userImage");
      
        $rootScope.showMenu = "Module";
        $rootScope.menuModuleId = null;
        $rootScope.isLeftMenuHide = $rootScope.plantName === null || $rootScope.plantName === undefined ? true : false;
        $rootScope.moduleShowHide = function () {
            $rootScope.menuModuleName = null;
            if ($rootScope.showMenu === "Menu")
                $rootScope.showMenu = "Module";
        };

        $rootScope.isCompanyImageFound = function () {
            var img = new Image();
            var imgUrl = "POPResources/Organizations/" + $cookies.get("CompanyImage");
            img.src = imgUrl;
            img.onload = function () {
                $rootScope.CompanyLogo = $cookies.get("CompanyImage");
                $rootScope.CompanyFullName = null;
            }
            img.onerror = function () {
                $rootScope.CompanyFullName = $cookies.get("CompanyFullName");
                $rootScope.CompanyLogo = null;
             
            }
        }
        $rootScope.isCompanyImageFound();



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
        };
        $rootScope.mpanelMenu = function (id, name) {
            $rootScope.showMenu = "Menu";
            $rootScope.menuModuleId = id;
            $rootScope.menuModuleName = name;
            $rootScope.menuFrames = $filter("filter")($rootScope.menuFrameList, { ModuleId: id }, true);
            setTimeout(function () {
                $rootScope.$apply(function () {
                    angular.element(".main-nav").vmenuModule({
                        Speed: 400,
                        autostart: false,
                        autohide: true
                    });
                });
            }, 100);
        };
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


        $rootScope.$on('$routeChangeStart', function ($event, next, current) {
            var href = next.$$route.originalPath;
            $rootScope.ChangeHref(href);
        });


        $rootScope.SelectedHref = null;
        $rootScope.ChangeHref = function (href) {
            if (!$rootScope.ListMenuSearch || $rootScope.ListMenuSearch.length == 0) {
                try {
                    $rootScope.SelectedHref = $cookies.get("mpanelMenuHelpDocInternalName");
                } catch (e) {

                }

            }
            else {

                $rootScope.SelectedHref = null;
                for (var i = 0; i < $rootScope.ListMenuSearch.length; i++) {
                    if ('/' + $rootScope.ListMenuSearch[i].Href == href) {
                        $rootScope.SelectedHref = $rootScope.ListMenuSearch[i].MenuHelpDocInternalName;
                        $cookies.put("mpanelMenuHelpDocInternalName", $rootScope.ListMenuSearch[i].MenuHelpDocInternalName);
                        break;
                    }
                }
            }
        }

        $rootScope.DownloadDocumentationFile = function () {
            if (!$rootScope.SelectedHref)
                return;

            try {
                var file_src = 'OrderManagements/productionOrderReports/LoadPdfDocumentation?href=' + $rootScope.SelectedHref
                $rootScope.report(file_src);

            } catch (e) {

            }

        }


        $rootScope.report = function (file_src)
        {
            $("#iframe_div_for_report").empty();
            var frame = $('<iframe id="report">')
                .attr('height', '0px')
                .attr('visibility', 'hidden')
                .attr('width', '0px');
            frame.on('load', function ()
            {

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
    .filter("dateFilter", dateFilter)
    .filter("dateFiltering", dateFiltering)
    .filter("safecontent", safecontent)
    .filter("sumByKey", sumByKey)
    .filter("filterMultiple", filterMultiple)
    .filter("makePositive", makePositive)
    .filter("setDecimal", setDecimal)
    .directive("panelBody", panelBody)
    .directive("panelMenu", panelMenu)
    .directive("nDecimals", nDecimals)
    .directive("datepicker", datepicker)
    .directive("togglable", togglable)
    .directive("showErrors", showErrors)
    .directive("compile", compile)
    .directive("archiveRow", archiveRow)
    .directive("confirmModal", confirmModal)
    .directive("confirmArchive", confirmArchive)
    .directive("confirmArchiveGeneric", confirmArchiveGeneric)
    .directive("loader", loader)
    .directive("tooltip", tooltip)
    .directive("stringToNumber", stringToNumber)
    .directive("ngFileSelect", ngFileSelect)
    .directive("input", inputFocus)
    .directive("textarea", inputFocus)
    .directive("select", inputFocus)
    .directive("input", CodeChecker)
    .directive("ngEnter", ngEnter)
    .directive("inputMaxLengthNumber", inputMaxLengthNumber)
    .directive("confirmCancel", confirmCancel)
    .directive("onlyNumbers", onlyNumbers)
    .directive("modalTable", modalTable)
    .directive("manualValidation", manualValidation)
    .directive("capitalize", capitalize)
    .factory("errorInterceptor", errorInterceptor)
    .factory("baseService", baseService)
    .factory("cboService", cboService)
    .factory('factoryService', factoryService)
    .factory("fileReader", fileReader)
    .factory('addressService', addressService)
    .factory('bankService', bankService)
    .constant("commonMessage", commonMessage)
    .factory("accountService", accountService)
    ;