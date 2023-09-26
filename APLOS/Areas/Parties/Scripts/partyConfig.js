PartyConfig.$inject = ['$routeProvider', '$locationProvider'];
function PartyConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/party-group', {
            templateUrl: 'Parties/PartyGroup/aplos',
            controller: 'partyGroupController'
        })
        .when('/party-group-category', {
            templateUrl: 'Parties/PartyGroup/PartyGroupCategory',
            controller: 'partyGroupCategoryController'
        })
        .when('/party-group-sub-category', {
            templateUrl: 'Parties/PartyGroup/PartyGroupSubCategory',
            controller: 'partyGroupSubCategoryController'
        })
        .when('/party-group-class', {
            templateUrl: 'Parties/PartyGroup/PartyGroupClass',
            controller: 'partyGroupClassController'
        })
        .when('/party-category', {
            templateUrl: 'Parties/Party/PartyCategory',
            controller: 'partyCategoryController'
        })
        .when('/party-sub-category', {
            templateUrl: 'Parties/Party/PartySubCategory',
            controller: 'partySubCategoryController'
        })
        .when('/party', {
            templateUrl: 'Parties/Party/Aplos',
            controller: 'partyController'
        })
        .when('/party-company', {
            templateUrl: 'Parties/InterCompanyParty/InterCompany',
            controller: 'interCompanyPartyController'
        })
        .when('/party-other', {
            templateUrl: 'Parties/Party/Other',
            controller: 'otherController'
        })
        .when('/party-account-group', {
            templateUrl: 'Parties/PartyAccountGroup/Aplos',
            controller: 'partyAccountGroupController'
        })
        .when('/party-account-groupgl', {
            templateUrl: 'parties/PartyAccountGroup/PartyAccountGroupGL',
            controller: 'partyAccountGroupGLController'
        })
        .when('/party-mapping', {
            templateUrl: 'parties/Party/PartyMapping',
            controller: 'partyMappingController'
        })
        .when('/party-director', {
            templateUrl: 'Parties/Party/Director',
            controller: 'directorController'
        })
        .when('/party-report', {
            templateUrl: 'Parties/party/PartyReport',
            controller: 'partyReportController'
        })
        .when('/inter-party-report', {
            templateUrl: 'Parties/PartyReport/interpartyLeadger',
            controller: 'interpartyLedgerReportController'
        })
        .when('/party-contact-person', {
            templateUrl: 'Parties/partycontactperson/aplos',
            controller: 'partyContactPersonController'
        })
        .when('/partner-function', {
            templateUrl: 'Parties/partnerfunction/aplos',
            controller: 'partnerFunctionController'
        })
        .when('/partner-determination-procedure', {
            templateUrl: 'Parties/partnerdeterminationprocedure/aplos',
            controller: 'partnerDeterminationProcedureController'
        })
        .when('/pdpfunction', {
            templateUrl: 'Parties/pdpfunction/aplos',
            controller: 'pDPFunctionController'
        })
        .when('/workcenterbuyertag', {
            templateUrl: 'WorkCenters/workcenterbuyertag/aplos',
            controller: 'workCenterBuyerTagController'
        })
        .when('/buyercategory', {
            templateUrl: 'Parties/buyercategory/aplos',
            controller: 'buyerCategoryController'
        })
        .when('/buyer', {
            templateUrl: 'Parties/buyer/aplos',
            controller: 'buyerController'
        })
        .when('/buyer-brand', {
            templateUrl: 'Parties/buyerbrand/aplos',
            controller: 'buyerBrandController'
        })
        .when('/buyer-department', {
            templateUrl: 'Parties/BuyerDepartment/aplos',
            controller: 'buyerDepartmentController'
        })
        .when('/buyer-division', {
            templateUrl: 'Parties/BuyerDivision/aplos',
            controller: 'buyerDivisionController'
        })
        .when('/intermediateItem-entity', {
            templateUrl: 'Parties/IntermediateItemEntity/aplos',
            controller: 'intermediateItemEntityController'
        })
        .when('/intermediate-item', {
            templateUrl: 'Parties/IntermediateItem/aplos',
            controller: 'intermediateItemController'
        })
        .when('/buyer-master', {
            templateUrl: 'Parties/BuyerMaster/aplos',
            controller: 'buyerMasterController'
        })
        .when('/buyer-program', {
            templateUrl: 'Parties/BuyerProgram/aplos',
            controller: 'buyerProgramController'
        })
        .when("/party-ledger", {
            templateUrl: "Parties/PartyReport/PartyLedgerReport",
            controller: "partyLedgerReportController"
        })
        .when("/party-ledger-outstanding", {
            templateUrl: "Parties/PartyReport/PartyOutstandingLedgerReport",
            controller: "partyLedgerOutstandingReportController"
        })
        .when("/party-ob-ledger", {
            templateUrl: "Parties/PartyReport/PartyOpeningBalanceLedger",
            controller: "partyOpeningBalanceLedgerController"
        })
        .when("/party-outstanding-report", {
            templateUrl: "Parties/PartyReport/PartyOutstandingReport",
            controller: "partyOutstandingReportController"
        })
        .when("/party-payment-status-report", {
            templateUrl: "Parties/PartyReport/PartyPaymentStatusReport",
            controller: "partyPaymentStatusReportController"
        })
        .when("/party-approve", {
            templateUrl: "Parties/Party/Approve",
            controller: "partyApprovalController"
        })



        ;


}