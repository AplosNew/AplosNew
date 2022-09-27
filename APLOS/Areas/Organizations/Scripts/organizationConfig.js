OrganizationConfig.$inject = ['$routeProvider', '$locationProvider'];
function OrganizationConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/entity-relationship', {
            templateUrl: 'Organizations/Entity/EntityRelationship',
            controller: 'entityRelationshipController'
        })
        .when('/entity', {
            templateUrl: 'Organizations/Entity/Aplos',
            controller: 'entityController'
        })
        .when('/entity-line', {
            templateUrl: 'Organizations/entityline',
            controller: 'entityLineController'
        })
        .when('/entity-allowance', {
            templateUrl: 'Organizations/Entity/allowance',
            controller: 'entityAllowanceController'
        })
        .when('/entity-costcenter', {
            templateUrl: 'Organizations/entitycostcenter',
            controller: 'entityCostCenterController'
        })
        .when('/position-relationship', {
            templateUrl: 'Organizations/Position/PositionRelationship',
            controller: 'positionRelationshipController'
        })
        .when('/position', {
            templateUrl: 'Organizations/Position/Aplos',
            controller: 'positionController'
        })
        .when('/position-allowance', {
            templateUrl: 'Organizations/Position/Allowance',
            controller: 'positionAllowanceController'
        })
        .when('/position-budget-master/:id/:name', {
            templateUrl: 'Organizations/position/budgetmaster',
            controller: 'positionBudgetMasterController'
        })
        .when('/position-budget-master-activity/:positionId/:positionName/:budgetMasterId/:budgetMasterName', {
            templateUrl: 'Organizations/position/budgetmasterActivity',
            controller: 'positionBudgetMasterActivityController'
        })
        .when('/organization-category', {
            templateUrl: 'Organizations/company/OrganizationCategory',
            controller: 'organizationCategoryController'
        })
        .when('/organization-class', {
            templateUrl: 'Organizations/company/organizationclass',
            controller: 'organizationClassController'
        })
        .when('/company-group', {
            templateUrl: 'Organizations/companygroup',
            controller: 'companyGroupController'
        })
        .when('/company', {
            templateUrl: 'Organizations/company',
            controller: 'companyController'
        })
        .when('/update-company', {
            templateUrl: 'Organizations/company/updatecompany',
            controller: 'updateCompanyController'
        })
        .when('/update-company-group', {
            templateUrl: 'Organizations/companygroup/updatecompanygroup',
            controller: 'updateCompanyGroupController'
        })
        .when('/division', {
            templateUrl: 'Organizations/division',
            controller: 'divisionController'
        })
        .when('/plant', {
            templateUrl: 'Organizations/plant',
            controller: 'plantController'
        })
        .when('/update-plant', {
            templateUrl: 'Organizations/plant/updateplant',
            controller: 'updatePlantController'
        })
        .when('/sub-division', {
            templateUrl: 'Organizations/subdivision',
            controller: 'subDivisionController'
        })
        .when('/company-sub-division', {
            templateUrl: 'Organizations/companysubdivision',
            controller: 'companySubDivisionController'
        })
        .when('/company-division', {
            templateUrl: 'Organizations/companydivision',
            controller: 'companyDivisionController'
        })
        .when('/department', {
            templateUrl: 'Organizations/department',
            controller: 'departmentController'
        })
        .when('/company-department', {
            templateUrl: 'Organizations/companydepartment',
            controller: 'companyDepartmentController'
        })
        .when('/company-structure-relation', {
            templateUrl: 'Organizations/companystructurerelation',
            controller: 'companyStructureRelationController'
        })
        .when('/company-structure-setup', {
            templateUrl: 'Organizations/companystructuresetup',
            controller: 'companyStructureSetupController'
        })
        .when('/uom-dimension', {
            templateUrl: 'Setups/uomdimension',
            controller: 'uOMDimensionController'
        })
        .when('/unit', {
            templateUrl: 'Organizations/unit',
            controller: 'unitController'
        })
        .when('/division', {
            templateUrl: 'Organizations/division',
            controller: 'divisionController'
        })
        .when('/division-master', {
            templateUrl: 'Organizations/divisionmaster',
            controller: 'divisionMasterController'
        })
        .when('/sub-division', {
            templateUrl: 'Organizations/subdivision',
            controller: 'subDivisionController'
        })
        .when('/department-master', {
            templateUrl: 'Organizations/departmentmaster',
            controller: 'departmentMasterController'
        })
        .when('/section', {
            templateUrl: 'Organizations/section',
            controller: 'sectionController'
        })
        .when('/company-section', {
            templateUrl: 'Organizations/companysection',
            controller: 'companySectionController'
        })
        .when('/sub-section', {
            templateUrl: 'Organizations/subsection',
            controller: 'subSectionController'
        })
        .when('/company-sub-section', {
            templateUrl: 'Organizations/companysubsection',
            controller: 'companySubSectionController'
        })
        .when('/line', {
            templateUrl: 'Organizations/line',
            controller: 'lineController'
        })
        .when('/company-line', {
            templateUrl: 'Organizations/companyline',
            controller: 'companyLineController'
        })
        .when('/subsection-structure', {
            templateUrl: 'Organizations/subsectionstructure',
            controller: 'subsectionStructureController'
        })
        .when('/company-coa', {
            templateUrl: 'Organizations/company/companycoa',
            controller: 'companyCoaController'
        })
        .when('/purchase-organisation', {
            templateUrl: 'Organizations/purchaseorganisation',
            controller: 'purchaseOrganisationController'
        })
        .when('/purchase-group', {
            templateUrl: 'Organizations/purchasegroup',
            controller: 'purchaseGroupController'
        })
        .when('/sales-organisation', {
            templateUrl: 'Organizations/salesorganisation',
            controller: 'salesOrganisationController'
        })
        .when('/sales-group', {
            templateUrl: 'Organizations/salesgroup',
            controller: 'salesGroupController'
        })
        .when('/division-master', {
            templateUrl: 'Organizations/divisionmaster',
            controller: 'divisionMasterController'
        })
        .when('/department-master', {
            templateUrl: 'Organizations/departmentmaster',
            controller: 'departmentMasterController'
        })
        .when('/section-master', {
            templateUrl: 'Organizations/sectionmaster',
            controller: 'sectionMasterController'
        })
        .when('/manpower-budget', {
            templateUrl: 'Organizations/manpowerBudget',
            controller: 'manpowerBudgetController'
        })
        .when('/manpower-budget-allowance', {
            templateUrl: 'Organizations/manpowerBudget/allowance',
            controller: 'manpowerBudgetAllowanceController'
        })
        .when('/recruitment-planning', {
            templateUrl: 'Organizations/recruitmentplanning',
            controller: 'recruitmentPlanningController'
        })
        .when('/costcenter-category', {
            templateUrl: 'Organizations/costcentercategory',
            controller: 'costCenterCategoryController'
        })
        .when('/costcenter-subcategory', {
            templateUrl: 'Organizations/costcentersubcategory',
            controller: 'costCenterSubCategoryController'
        })
        .when('/cost-center-company-extension', {
            templateUrl: 'Organizations/companyCostCenter',
            controller: 'companyCostCenterController'
        })
        .when('/cost-center', {
            templateUrl: 'Organizations/costcenter',
            controller: 'costCenterController'
        })
        .when('/plantdesignationgroup-salaryrule', {
            templateUrl: 'Organizations/plantDesignationGroupSalaryRule',
            controller: 'plantDesignationGroupSalaryRuleController'
        })
        .when('/plantSalaryHead-sequence', {
            templateUrl: 'Organizations/plantSalaryHeadSequence',
            controller: 'plantSalaryHeadSequenceController'
        })
        .when('/manpower-budget-master/:id/:name', {
            templateUrl: 'Organizations/manpowerbudget/budgetmaster',
            controller: 'manpowerBudgetBudgetMasterController'
        })
        .when('/manpower-budget-master-activity/:manpowerBudgetId/:manpowerBudgetName/:budgetMasterId/:budgetMasterName', {
            templateUrl: 'Organizations/manpowerbudget/budgetmasterActivity',
            controller: 'manpowerBudgetBudgetMasterActivityController'
        })
        .when('/designation-group', {
            templateUrl: 'Organizations/DesignationMaster/DesignationGroup',
            controller: 'designationGroupController'
        })
        .when('/designation', {
            templateUrl: 'Organizations/DesignationMaster/designation',
            controller: 'designationController'
        })
        .when('/designation-master', {
            templateUrl: 'Organizations/designationmaster',
            controller: 'designationMasterController'
        })
        .when('/company-designation', {
            templateUrl: 'Organizations/companydesignation',
            controller: 'companyDesignationController'
        })
        .when('/legal-designation', {
            templateUrl: 'Organizations/DesignationMaster/legaldesignation',
            controller: 'legalDesignationController'
        })
        .when('/position-grouping-data', {
            templateUrl: 'Organizations/PositionGroupingData/aplos',
            controller: 'PositionGroupingDataController'
        })
        .when('/designation-budget', {
            templateUrl: 'Organizations/DesignationBudget/aplos',
            controller: 'DesignationBudgetController'
        })
        .when('/position-wise-mpstatus', {
            templateUrl: 'Organizations/PositionWiseMPStatus/Aplos',
            controller: 'PositionWiseMPStatusController'
        })
        ;
}