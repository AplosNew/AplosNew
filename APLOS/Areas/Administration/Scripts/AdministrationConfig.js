AdministrationConfig.$inject = ['$routeProvider', '$locationProvider'];
function AdministrationConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/vehicle-movement-locations', {
            templateUrl: 'Administration/VehicleMovementLocations',
            controller: 'VehicleMovementLocationsController'
        })

        .when('/services-approving-authority', {
            templateUrl: 'Administration/ServicesApprovingAuthority',
            controller: 'ServicesApprovingAuthorityController'
        })

        .when('/visitor-list-report', {
            templateUrl: 'Administration/VisitorListReport',
            controller: 'VisitorListReportController'
        })
        .when('/contract-master', {
            templateUrl: 'Administration/GeneralContractItemMaster',
            controller: 'GeneralContractItemMasterController'
        })
        .when('/general-contract', {
            templateUrl: 'Administration/GeneralContract',
            controller: 'GeneralContractController'
        })
        .when('/contract-entry', {
            templateUrl: 'Administration/GeneralContractEntry',
            controller: 'GeneralContractEntryController'
        })
        .when('/contract-report', {
            templateUrl: 'Administration/GeneralContractReport',
            controller: 'GeneralContractReportController'
        })
        .when('/generalcontract-check', {
            templateUrl: 'Administration/GeneralContractChecked',
            controller: 'GeneralContractCheckedController'
        })
        .when('/generalcontract-approved', {
            templateUrl: 'Administration/GeneralContractApproved',
            controller: 'GeneralContractApprovedController'
        })
        .when('/general-approved', {
            templateUrl: 'Administration/GeneralCheckedApproved/GeneralApproved',
            controller: 'GeneralApprovedController'
        })

        .when('/app-general-approved', {
            templateUrl: 'Administration/GeneralCheckedApproved/GeneralApprovedApplication',
            controller: 'GeneralApprovedApplicationController'
        })

        .when('/asset-management', {
            templateUrl: 'Administration/AssetManagement/Aplos',
            controller: 'AssetManagementController'
        })
        .when('/Emp-assettransaction', {
            templateUrl: 'Administration/EmpDocAssetTransection/Aplos',
            controller: 'EmpDocAssetTransectionController'
        })
};