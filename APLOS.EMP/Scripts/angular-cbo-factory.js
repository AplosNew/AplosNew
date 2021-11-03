cboService.$inject = ['$http', '$window', '$rootScope', 'baseService'];
function cboService($http, $window, $rootScope, baseService) {
    var service = {
        getEnumCbo: getEnumCbo
        , getSequence: getSequence
        , getCboVoucherType: getCboVoucherType
        , getCboVoucherTypeAccountReceivableList: getCboVoucherTypeAccountReceivableList
        , getCboPositionByEntityId: getCboPositionByEntityId
        , getCboRoleByCompanyGroup: getCboRoleByCompanyGroup
        , getCboPositionByCompanyGroup: getCboPositionByCompanyGroup
        , getCboVendorByCompany: getCboVendorByCompany
        , getCboDesignationByCompanyGroup: getCboDesignationByCompanyGroup
        , getCboRecruitmentProcess: getCboRecruitmentProcess
        , getCboLanguage: getCboLanguage
        , getCboShiftDefinationByPlant: getCboShiftDefinationByPlant
        , getCboLineByCompany: getCboLineByCompany
        , getCboDesignationGroupByCompanyGroup: getCboDesignationGroupByCompanyGroup
        , getCboSubSectionByCompanyGroup: getCboSubSectionByCompanyGroup
        , getCboSubSectionByCompany: getCboSubSectionByCompany
        , getCboSectionByCompanyGroup: getCboSectionByCompanyGroup
        , getCboSectionByCompany: getCboSectionByCompany
        , getCboDepartmentByCompanyGroup: getCboDepartmentByCompanyGroup
        , getCboDepartmentByCompany: getCboDepartmentByCompany
        , getCboSubDivisionByCompany: getCboSubDivisionByCompany
        , getCboDivisionByCompanyGroup: getCboDivisionByCompanyGroup
        , getCboDivisionByCompany: getCboDivisionByCompany
        , getCboEmployeeGroupByCompanyGroup: getCboEmployeeGroupByCompanyGroup
        // Budget & Activity
        , getCboFiscalYear: getCboFiscalYear
        , getCboActivity: getCboActivity
        , getCboActivityCompanyGroup: getCboActivityCompanyGroup
        , getCboActivityByEmployee: getCboActivityByEmployee
        , getCboActivityPhoneByEmployeeActivity: getCboActivityPhoneByEmployeeActivity
        , getCboBudgetByEmployeeActivity: getCboBudgetByEmployeeActivity
        , getCboBudgetMasterByCompanyGroup: getCboBudgetMasterByCompanyGroup
        , getCboRoutineBudgetMasterByEntityAndFY: getCboRoutineBudgetMasterByEntityAndFY
        // Module
        , getCboModule: getCboModule
        , getCboModuleByCompanyGroup: getCboModuleByCompanyGroup
        , getCboSubModule: getCboSubModule
        , getCboSubModuleByModule: getCboSubModuleByModule
        , getPlantShiftCbo: getPlantShiftCbo
        , getEntityPlantShiftCbo: getEntityPlantShiftCbo
        , getBudgetCboByEmployee: getBudgetCboByEmployee
        , getCompanyGroupCurrencyCbo: getCompanyGroupCurrencyCbo
        , getParallelCurrency: getParallelCurrency
        , getCboTransactionCurrency: getCboTransactionCurrency
        , getCboUnit: getCboUnit
        , getCboUnitByCompanyGroup: getCboUnitByCompanyGroup
        , getCboUnitByCompany: getCboUnitByCompany
        , getCboCompany: getCboCompany
        , getCboCompanyByCompanyGroup: getCboCompanyByCompanyGroup
        , getCboPlant: getCboPlant
        , getCboPlantByCompanyGroup: getCboPlantByCompanyGroup
        , getCboPlantByCompany: getCboPlantByCompany
        , getCompanyGroupCbo: getCompanyGroupCbo
        , getCompanyGroupCompanyCbo: getCompanyGroupCompanyCbo
        , getCompanyLineCbo: getCompanyLineCbo
        , getEntityCompanyLineCbo: getEntityCompanyLineCbo
        , getCboEntityByCompany: getCboEntityByCompany
        , getCboProductionEntityByPlant: getCboProductionEntityByPlant
        , getCboEntityByPlant: getCboEntityByPlant
        , getCboEntityByCompanyGroup: getCboEntityByCompanyGroup
        , getCboEntityExceptionByCompany: getCboEntityExceptionByCompany
        , getCboEntityAndPositionRelationshipByCompanyGroupAndCompany: getCboEntityAndPositionRelationshipByCompanyGroupAndCompany
        , getCboEntityLineById: getCboEntityLineById
        , getShipModeCbo: getShipModeCbo
        , getCountryCbo: getCountryCbo
        , getCountryByContinentCbo: getCountryByContinentCbo
        , getPotitionList: getPotitionList
        , getFixedAssetList: getFixedAssetList
        , getFixedAssetClassList: getFixedAssetClassList
        , getFixedAssetSubClassList: getFixedAssetSubClassList
        , getFixedAssetCategoryList: getFixedAssetCategoryList
        , getFixedAssetSubCategoryList: getFixedAssetSubCategoryList
        , getFixedAssetItemList: getFixedAssetItemList
        , getFixedAssetMasterList: getFixedAssetMasterList
        , getCboBuyer: getCboBuyer
        , getWashOperationCbo: getWashOperationCbo
        , jobDescriptionCategoryList: jobDescriptionCategoryList
        , jobDescriptionSubCategoryList: jobDescriptionSubCategoryList
        , jobDescriptionItemList: jobDescriptionItemList
        , loadProcessEntityWiseCbo: loadProcessEntityWiseCbo
        , loadUtilityCbo: loadUtilityCbo
        , loadUomUtilityCbo: loadUomUtilityCbo
        , loadSubprocessCbo: loadSubprocessCbo
        , loadProcessWithCompanyCbo: loadProcessWithCompanyCbo
        , loadProcessCbo: loadProcessCbo
        , loadOperationCbo: loadOperationCbo
        , getCboRecruitmentProcessSetByCompanyGroup: getCboRecruitmentProcessSetByCompanyGroup
        , getCboRecruitmentGroupByPlant: getCboRecruitmentGroupByPlant
        , getCboManpowerBudgetByCompanyAndPlant: getCboManpowerBudgetByCompanyAndPlant
        , getCboBrand: getCboBrand
        , getCboEntityProductionByCompanyGroup: getCboEntityProductionByCompanyGroup
        , getCboReligion: getCboReligion
        , getCboBloodGroup: getCboBloodGroup
        , getCboPostOffice: getCboPostOffice
        , getCboThana: getCboThana
        , getCboDistrict: getCboDistrict
        , getCboCity: getCboCity
        , getCboArea: getCboArea
        , getCboQualificationLevel: getCboQualificationLevel
        , getCboQualificationStream: getCboQualificationStream
        , getCboChartOfAccount: getCboChartOfAccount
        , getCboDepreciationRule: getCboDepreciationRule
        // Accounts
        , getCboChartOfAccountLevel1: getCboChartOfAccountLevel1
        , getCboChartOfAccountLevel2: getCboChartOfAccountLevel2
        , getCboChartOfAccountLevel3: getCboChartOfAccountLevel3
        , getCboChartOfAccountLevel4: getCboChartOfAccountLevel4
        , getCboChartOfAccountLevel5: getCboChartOfAccountLevel5
        , getCboChartOfAccountLevel6: getCboChartOfAccountLevel6
        , getCboGivenDiscription: getCboGivenDiscription
        , getCboLegalDesignation: getCboLegalDesignation
        , getTaxCategoryCboByCountry: getTaxCategoryCboByCountry
        , getTaxCodeCbo: getTaxCodeCbo
        // WorkCenter
        , getCboWorkCenterMaster: getCboWorkCenterMaster
        , getCboWorkCenterMasterByEntity: getCboWorkCenterMasterByEntity
        // Projects
        , getCboProjectPlanningCategory: getCboProjectPlanningCategory
        , getCboProjectPlanningSubCategory: getCboProjectPlanningSubCategory
        , getCboProjectPlanning: getCboProjectPlanning
        , getCivilStatus: getCivilStatus
        // CostCenter
        , getCboCostCenterCategory: getCboCostCenterCategory
        , getCboCostCenterSubCategory: getCboCostCenterSubCategory
        , getCboServiceCategory: getCboServiceCategory
        , getCboServiceSubCategory: getCboServiceSubCategory
        // PartyGroup
        , getCboServicePartyGroupCategory: getCboServicePartyGroupCategory
        , getCboServicePartyGroupSubCategory: getCboServicePartyGroupSubCategory
        , getCboServicePartyGroupPreferenceCategory: getCboServicePartyGroupPreferenceCategory
        , getCboServicePartyGroupClass: getCboServicePartyGroupClass
        , getCboEntityByCompanyWise: getCboEntityByCompanyWise
        , getCboBudgetByGLId: getCboBudgetByGLId
        , getCboSalutaion: getCboSalutaion
        , getTestingCategoryCbo: getTestingCategoryCbo
        , getPaymentModeCbo: getPaymentModeCbo
        , getUoMCbo: getUoMCbo
        , getHNSCbo: getHNSCbo
        , getTestinStdCbo: getTestinStdCbo
        , getCboSalesType: getCboSalesType
        , getUoMCboByMaterialGroup: getUoMCboByMaterialGroup
        , getCboSalesOrganisationByPlant: getCboSalesOrganisationByPlant
        , getPackingFromCboByCompanyGroup: getPackingFromCboByCompanyGroup
    };


    function getCboSalesOrganisationByPlant(plantId, callback) {
        $http.get('/Organizations/SalesOrganisation/GetCboByPlant?plantId=' + plantId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboSalesType(callback) {
        $http.get('/Setups/SalesType/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get all Salutaion.
    function getCboSalutaion(callback) {
        $http.get('/employees/salutation/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboBudgetByGLId(glId, callback) {
        $http.get('/Budgets/BudgetMaster/GetCboByGLId?glId=' + glId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboServicePartyGroupCategory(callback) {
        $http.get('/parties/partygroupcategory/getpartygroupcategorycbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboServicePartyGroupSubCategory(callback) {
        $http.get('/parties/partygroupsubcategory/getpartygroupsubcategorycbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboServicePartyGroupPreferenceCategory(callback) {
        $http.get('/parties/partygrouppreferencecategory/getpartygroupprefercategorycbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboServicePartyGroupClass(callback) {
        $http.get('/parties/partygroupclass/getpartygroupclasscbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboTransactionCurrency(callback) {
        $http.get('/currencies/transactioncurrency/gettrancurrencylist')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    //WorkCenterMaster
    function getCboWorkCenterMaster(callback) {
        $http.get('/WorkCenters/WorkCenterMaster/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    //WorkCenterMaster
    function getCboWorkCenterMasterByEntity(entityId, callback) {
        $http.get('/WorkCenters/WorkCenterMaster/GetCboList?entityId=' + entityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCivilStatus(callback) {
        $http.get('/employees/civilstatus/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboFiscalYear(entityId, callback) {
        if (baseService.isUndefinedOrNull(entityId)) {
            if (!baseService.isUndefinedOrNull($window.entityId)) {
                entityId = $window.entityId;
            }
            else
                entityId = null;
        }
        $http.get('/accounts/companyfiscalyear/getfiscalyearbyentity?entityId=' + entityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    //function getCboFiscalYear(callback) {
    //    $http.get('/accounts/companyfiscalyear/getcompanyfiscalyearcbo')
    //        .then(
    //        function successCallback(response) {
    //            callback(response.data);
    //        },
    //        function errorCallback(response) {
    //            ShowResult(response, 'failure');
    //        });
    //};

    function getCboLegalDesignation(callback) {
        $http.get('/Employees/RecruitmentConfirmation/GetLegalDesignationCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboGivenDiscription(callback) {
        $http.get('/Employees/RecruitmentConfirmation/GetGivenDesignationCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboChartOfAccountLevel6(callback) {
        $http.get('/Accounts/ChartOfAccountLevel5/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboChartOfAccountLevel5(callback) {
        $http.get('/Accounts/ChartOfAccountLevel5/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboChartOfAccountLevel4(callback) {
        $http.get('/Accounts/ChartOfAccountLevel4/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboChartOfAccountLevel3(callback) {
        $http.get('/Accounts/ChartOfAccountLevel3/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboChartOfAccountLevel2(callback) {
        $http.get('/Accounts/ChartOfAccountLevel2/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboChartOfAccountLevel1(callback) {
        $http.get('/Accounts/ChartOfAccountLevel1/GetCbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboBudgetMasterByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/Budgets/BudgetMaster/getCboBudgetMasterByCompanyGroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboRoutineBudgetMasterByEntityAndFY(entityId, fiscalYearId, callback) {
        $http.get('/Budgets/RoutineBudget/GetCboRoutineBudget?entityId=' + entityId + '&&fiscalYearId=' + fiscalYearId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboEntityProductionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/entity/getcboproductionbycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboEntityByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/entity/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboEntityAndPositionRelationshipByCompanyGroupAndCompany(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/positionrelationship/getentityandpositionrelationship?companyGroupId=' + companyGroupId + '&companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get position manpower budget by company and plant id.
    function getCboManpowerBudgetByCompanyAndPlant(companyId, plantId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/positions/manpowerbudget/getcbobycompanyandplant?companyId=' + companyId + '&plantid=' + plantId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboRecruitmentGroupByPlant(plantId, callback) {
        $http.get('/employees/recruitmentgroup/getcbo?plantId=' + plantId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboRecruitmentProcessSetByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/employees/recruitmentprocessset/getcbo?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get voucher type cbo list.
    function getCboVoucherType(callback) {
        $http.get('/accounts/vouchertype/getvouchertypecbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboVoucherTypeAccountReceivableList(callback) {
        $http.get('/accounts/vouchertypematrix/GetCboVoucherTypeAccountReceivableList')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboPositionByEntityId(entityId, callback) {
        $http.get('/organizations/Position/getcbobyentity?entityid=' + entityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get role by company group Id.
    function getCboRoleByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/securities/role/getrolebycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get position realation data by company group Id.
    function getCboPositionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/Position/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get vendor by company Id.
    function getCboVendorByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/parties/party/getvendorcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get designation by companyGroupId.
    function getCboDesignationByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/designations/designation/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get RecruitmentProcessSet cbo list.
    function getCboRecruitmentProcess(callback) {
        $http.get('/employees/recruitmentprocess/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get Language cbo list.
    function getCboLanguage(callback) {
        $http.get('/setups/language/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get ShiftDefination by plantId.
    function getCboShiftDefinationByPlant(plantId, callback) {
        $http.get('/attendances/shiftdefination/getcbobyplant?plantid=' + plantId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get line by companyId.
    function getCboLineByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/line/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get designation group by companyGroupId.
    function getCboDesignationGroupByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/designations/designationgroup/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get sub section by companyGroupId.
    function getCboSubSectionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/subsection/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get sub section by companyId.
    function getCboSubSectionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/subsection/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get section by companyGroupId.
    function getCboSectionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/section/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get section by companyId.
    function getCboSectionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/section/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get department by companyGroupId.
    function getCboDepartmentByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/department/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get department by companyId.
    function getCboDepartmentByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/department/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get sub division by companyId.
    function getCboSubDivisionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/subdivision/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get sub division by companyGroupId.
    function getCboDivisionByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/subdivision/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get division by companyId.
    function getCboDivisionByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/division/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get employee group by companyGroupId.
    function getCboEmployeeGroupByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/employees/employeegroup/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get all enums list.
    function getEnumCbo(url, callback) {
        $http.get(url)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get all sequence list.
    function getSequence(url, callback) {
        $http.get(url)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get all activity cbo list.
    function getCboActivity(callback) {
        $http.get('/budgets/activity/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get activity list by company group id
    function getCboActivityCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/budgets/companygroupactivity/getcbo?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get all module cbo list.
    function getCboModule(callback) {
        $http.get('/modules/module/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get module list by company group id.
    function getCboModuleByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/modules/module/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get all sub module cbo list.
    function getCboSubModule(callback) {
        $http.get('/modules/submodule/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get sub module cbo list by module.
    function getCboSubModuleByModule(moduleId, callback) {
        $http.get('/modules/submodule/getcbobymodule?moduleId=' + moduleId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get employee activity list.
    function getCboActivityByEmployee(employeeId, callback) {
        $http.get('/budgets/activity/getactivitycbobyemployee?employeeId=' + employeeId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get employee activity phone list.
    function getCboActivityPhoneByEmployeeActivity(employeeId, activityId, callback) {
        $http.get('/budgets/activityphone/getactivityphonebyemployeeactivity?employeeId=' + employeeId + '&activityId=' + activityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getPlantShiftCbo(plantId, callback) {
        $http.get('/attendances/shiftdefination/getcbo?plantid=' + plantId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Shift by entity structure data id inside Plant id.
    function getEntityPlantShiftCbo(entityId, callback) {
        $http.get('/attendances/shiftdefination/getentityplantshiftcbo?entityId=' + entityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // get employee budget list by employeeId/ManpowerBudget/Position
    function getBudgetCboByEmployee(employeeId, callback) {
        $http.get('/budgets/budgetmaster/getbudgetcbobyemployee?employeeId=' + employeeId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // get employee budget list by employeeId/ManpowerBudget/Position and activity Id.
    function getCboBudgetByEmployeeActivity(employeeId, activityId, callback) {
        $http.get('/budgets/budgetmaster/getbudgetcbobyemployeeactivity?employeeId=' + employeeId + '&activityId=' + activityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getCompanyGroupCurrencyCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/currencies/companygroupcurrency/getcbo?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get all unit.
    function getCboUnit(callback) {
        $http.get('/organizations/unit/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get unit by companyGroupId.
    function getCboUnitByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/unit/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get unit by companyId.
    function getCboUnitByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/unit/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get all company list.
    function getCboCompany(callback) {
        $http.get('/organizations/company/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get company by companyGroupId.
    function getCboCompanyByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/company/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get all plant.
    function getCboPlant(callback) {
        $http.get('/organizations/plant/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    //Get all Brand List
    function getCboBrand(callback) {
        $http.get('/setups/brand/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    // Get plant by companyGroupId.
    function getCboPlantByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/plant/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get plant by companyId.
    function getCboPlantByCompany(companyId, callback) {
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/plant/getcbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCompanyGroupCbo(callback) {
        $http.get('/organizations/companygroup/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCompanyGroupCompanyCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/company/getcbobycompanygroup?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Line by company id
    function getCompanyLineCbo(companyId, callback) {
        $http.get('/organizations/companyline/getcbo?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Line by entity company id wise.
    function getEntityCompanyLineCbo(entityId, callback) {
        $http.get('/organizations/companyline/getentitycompanylinecbo?entityId=' + entityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Line by line of entity.
    function getCboEntityLineById(entityId, callback) {
        $http.get('/organizations/entityline/getcboentitylinebyid?entityId=' + entityId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboEntityByCompany(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/organizations/entity/getcboproduction?companyGroupId=' + companyGroupId + '&companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getCboEntityByCompanyWise(companyGroupId, companyId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        if (baseService.isUndefinedOrNull(companyId)) {
            if (!baseService.isUndefinedOrNull($window.companyId)) {
                companyId = $window.companyId;
            }
            else
                companyId = null;
        }
        $http.get('/organizations/entity/GetCboByCompany?companyGroupId=' + companyGroupId + '&companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getCboProductionEntityByPlant(plantId, callback) {
        $http.get('/organizations/entity/getcboproductionbyplant?plantid=' + plantId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboEntityByPlant(plantId, callback) {
        $http.get('/organizations/entity/GetCboByPlant?plantid=' + plantId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCboEntityExceptionByCompany(companyId, callback) {
        $http.get('/organizations/entity/getexceptioncbobycompany?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getShipModeCbo(callback) {
        $http.get('/ordermanagements/shipmode/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCountryCbo(callback) {
        $http.get('/addresses/country/getcountrycbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function getCountryByContinentCbo(continentId, callback) {
        $http.get('/addresses/country/getcountrycbobycontinent?continentId=' + continentId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getPotitionList(id, callback) {
        $http.get('/budgets/activitymaster/getpotitionlist?id=' + id)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // fixedAsset
    function getFixedAssetList(callback) {
        $http.get('/fixedassets/fixedasset/getfixedassetlist')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // fixedAssetClass
    function getFixedAssetClassList(callback) {
        $http.get('/fixedassets/fixedassetclass/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // fixedAssetSubClass
    function getFixedAssetSubClassList(callback) {
        $http.get('/fixedassets/fixedassetsubclass/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // fixedAssetCategory
    function getFixedAssetCategoryList(callback) {
        $http.get('/fixedassets/fixedassetcategory/getfixedassetcategorylist')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    //fixedAssetSubCategory
    function getFixedAssetSubCategoryList(callback) {
        $http.get('/fixedassets/fixedassetsubcategory/getfixedassetsubcategorylist')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // fixedAssetRegister
    function getFixedAssetItemList(callback) {
        $http.get('/fixedassets/fixedassetregister/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // fixedAssetMaster
    function getFixedAssetMasterList(callback) {
        $http.get('/fixedassets/fixedassetmaster/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Employees
    function jobDescriptionCategoryList(callback) {
        $http.get('/employees/jobdescriptioncategory/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    function jobDescriptionSubCategoryList(callback) {
        $http.get('/employees/jobdescriptionsubcategory/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function jobDescriptionItemList(callback) {
        $http.get('/employees/jobdescriptionitem/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function loadUtilityCbo(callback) {
        $http.get('/processes/utility/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    //Unit of Measurement
    function getUoMCbo(callback) {
        $http.get('/Setups/unitofmeasurement/getcbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function loadUomUtilityCbo(callback) {
        $http.get('/setups/unitofmeasurement/getunitofmeasurementcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function loadSubprocessCbo(processid, callback) {
        $http.get('/processes/CompanySubProcess/getcbo?processid=' + processid)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function loadProcessWithCompanyCbo(companyId, callback) {
        $http.get('/processes/Process/getcbo?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function loadProcessCbo(callback) {
        $http.get('/processes/Process/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function loadOperationCbo(subprocessid, callback) {
        $http.get('/machines/operation/getoperationcbo?subprocessid=' + subprocessid)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getCboBuyer(callback) {
        $http.get('/parties/buyer/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    // Get Religion cbo list.
    function getCboReligion(callback) {
        $http.get('/setups/religion/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get BloodGroup cbo list.
    function getCboBloodGroup(callback) {
        $http.get('/employees/bloodgroup/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get PostOffice cbo list.
    function getCboPostOffice(callback) {
        $http.get('/employees/civilstatus/getpocbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get position sub category cbo list.
    function getParallelCurrency(companyId, callback) {
        $http.get('/Currencies/CompanyParallelCurrency/CurrencyParallel?companyId=' + companyId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    // Get Thana cbo list.
    function getCboThana(callback) {
        $http.get('/employees/civilstatus/getpscbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get District cbo list.
    function getCboDistrict(callback) {
        $http.get('/employees/civilstatus/getdscbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get City cbo list.
    function getCboCity(callback) {
        $http.get('/addresses/city/getcitycbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    // Get Area cbo list.
    function getCboArea(callback) {
        $http.get('/addresses/area/getareacbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get QualificationLevel cbo list.
    function getCboQualificationLevel(callback) {
        $http.get('/employees/qualificationlevel/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get QualificationStream cbo list.
    function getCboQualificationStream(callback) {
        $http.get('/employees/qualificationstream/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get ChartOfAccount cbo list.
    function getCboChartOfAccount(callback) {
        $http.get('/accounts/coa/getcoacbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    // Get DepreciationRule cbo list.
    function getCboDepreciationRule(callback) {
        $http.get('/fixedassets/fixedAssetdepreciationrule/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    //Project
    function getCboProjectPlanningCategory(callback) {
        $http.get('/projects/projectplanningcategory/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    function getCboProjectPlanningSubCategory(callback) {
        $http.get('/projects/projectplanningsubcategory/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    function getCboProjectPlanning(callback) {
        $http.get('/projects/projectplanning/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    //HNS Code
    function getHNSCbo(callback) {
        $http.get('/Setups/hsncode/getcbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    //Production
    function getUoMCboByMaterialGroup(materilaGroupId, callback) {
        $http.get('/productions/salesorderlinear/getmguomlist?mgid=' + materilaGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    function getWashOperationCbo(recipewashsubprocessid, callback) {
        $http.get('/productions/recipewashmaster/getWashOperationCbo?recipewashsubprocessid=' + recipewashsubprocessid)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    function loadProcessEntityWiseCbo(entityid, callback) {
        $http.get('/productions/recipewashmaster/loadprocessentitywisecbo?entityid=' + entityid)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    //Packing From
    function getPackingFromCboByCompanyGroup(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId))
                companyGroupId = $window.companyGroupId;
            else
                companyGroupId = null;
        }
        $http.get('/materials/packingform/getcbo?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    //Testing Std
    function getTestinStdCbo(companyGroupId, callback) {
        if (baseService.isUndefinedOrNull(companyGroupId)) {
            if (!baseService.isUndefinedOrNull($window.companyGroupId)) {
                companyGroupId = $window.companyGroupId;
            }
            else
                companyGroupId = null;
        }
        $http.get('/Setups/testingstandard/getcbo?companyGroupId=' + companyGroupId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    //CostCenter
    function getCboCostCenterCategory(callback) {
        $http.get('/Setups/CostCenterCategory/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    function getCboCostCenterSubCategory(callback) {
        $http.get('/Setups/CostCenterSubCategory/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    //Service
    function getCboServiceCategory(callback) {
        $http.get('/Setups/ServiceCategory/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };
    function getCboServiceSubCategory(callback) {
        $http.get('/Setups/ServiceSubCategory/GetCbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getTaxCodeCbo(callback) {
        $http.get('/accounts/taxcategory/getcbo/')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getTaxCategoryCboByCountry(countryId, callback) {
        $http.get('/accounts/taxcategory/getcbo?countryId=' + countryId)
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    };

    function getTestingCategoryCbo(callback) {
        $http.get('/setups/testingcategory/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    function getPaymentModeCbo(callback) {
        $http.get('/setups/PaymentMode/getcbo')
            .then(
            function successCallback(response) {
                callback(response.data);
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    return service;
}