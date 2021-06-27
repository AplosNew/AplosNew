function FarmingConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/farming-crop-type', {
            templateUrl: 'Farming/CropType/Aplos',
            controller: 'CropTypeController'
        })
        .when('/farming-crop-category', {
            templateUrl: 'Farming/CropCategory/Aplos',
            controller: 'CropCategoryController'
        })

        .when('/farming-crop-subcategory', {
            templateUrl: 'Farming/CropSubCategory/Aplos',
            controller: 'CropSubCategoryController'
        })

        .when('/farming-process', {
            templateUrl: 'Farming/FarmingProcess/Aplos',
            controller: 'FarmingProcessController'
        })


        .when('/farming-land-category', {
            templateUrl: 'Farming/LandCategory/Aplos',
            controller: 'LandCategoryController'
        })

        .when('/crop-master', {
            templateUrl: 'Farming/CropMaster/Aplos',
            controller: 'CropMasterController'
        })

        .when('/farmer-master', {
            templateUrl: 'Farming/FarmerMaster/Aplos',
            controller: 'FarmerMasterController'
        })

        .when('/ics-master', {
            templateUrl: 'Farming/ICSMaster/Aplos',
            controller: 'ICSMasterController'
        })

        .when('/taluk', {
            templateUrl: 'Farming/Taluk/Aplos',
            controller: 'TalukController'
        })

        .when('/village', {
            templateUrl: 'Farming/Village/Aplos',
            controller: 'VillageController'
        })

        .when('/crop-planning', {
            templateUrl: 'Farming/CropPlanning/Aplos',
            controller: 'CropPlanningController'
        })

        .when('/farming-category', {
            templateUrl: 'Farming/FarmingCategory/Aplos',
            controller: 'FarmingCategoryController'
        })

        .when('/crop-rate-location', {
            templateUrl: 'Farming/CropRateLocation/Aplos',
            controller: 'CropRateLocationController'
        })

        .when('/sauda-booking', {
            templateUrl: 'Farming/PurchaseBookingSoda/Aplos',
            controller: 'PurchaseBookingSodaController'
        })

        .when('/daily-crop-rate', {
            templateUrl: 'Farming/DailyCropRate/Aplos',
            controller: 'DailyCropRateController'
        })

        .when('/sauda-confirmation', {
            templateUrl: 'Farming/Confirmation/Aplos',
            controller: 'ConfirmationController'
        })

        .when('/sauda-approval', {
            templateUrl: 'Farming/Approval/Aplos',
            controller: 'ApprovalController'
        })

        .when('/sauda-payment', {
            templateUrl: 'Farming/Payment/Aplos',
            controller: 'PaymentController'
        })

        .when('/transaction-type', {
            templateUrl: 'Farming/TransactionType/Aplos',
            controller: 'TransactionTypeController'
        })

        .when('/sauda-voucher', {
            templateUrl: 'Farming/Voucher/Aplos',
            controller: 'VoucherController'
        })
        .when('/farming-dashboard', {
            templateUrl: 'Farming/FarmingDashboard/Aplos',
            controller: 'FarmingDashboardController'
        })
}
FarmingConfig.$inject = ['$routeProvider', '$locationProvider'];