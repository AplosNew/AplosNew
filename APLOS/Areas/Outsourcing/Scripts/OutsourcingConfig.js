OutsourcingConfig.$inject = ['$routeProvider', '$locationProvider'];
function OutsourcingConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/jobwork-item', {
            templateUrl: 'Outsourcing/jobworkitem/aplos',
            controller: 'jobWorkItemController'
        })
        .when('/jobwork-activity', {
            templateUrl: 'Outsourcing/jobworkactivity/aplos',
            controller: 'jobWorkActivityController'
        })
        .when('/jobwork-location', {
            templateUrl: 'Outsourcing/jobworklocation/aplos',
            controller: 'jobWorkLocationController'
        })
        .when('/valueadded-master', {
            templateUrl: 'Outsourcing/jobworkvalueaddedmaster/aplos',
            controller: 'jobWorkValueAddedMasterController'
        })
        .when('/transformation-master', {
            templateUrl: 'Outsourcing/jobworktransformationmaster/aplos',
            controller: 'jobWorkTransformationMasterController'
        })
        .when('/valueadded-contract', {
            templateUrl: 'Outsourcing/JobWorkValueAddedContract/aplos',
            controller: 'JobWorkValueAddedContractController'
        })
        .when('/os-issue-return', {
            templateUrl: 'Outsourcing/OSIssueReturn/aplos',
            controller: 'OSIssueReturnController'
        })
        .when('/confirm-issue', {
            templateUrl: 'Outsourcing/JobWorkIssueReturnConfirmation/aplos',
            controller: 'JobWorkIssueReturnConfirmationController'
        })
        .when('/material-reconcilation-report', {
            templateUrl: 'Outsourcing/MaterialReconcilationReport/aplos',
            controller: 'MaterialReconcilationReportController'
        })
        .when('/receipt', {
            templateUrl: 'Outsourcing/OSReceiptValueAdded/aplos',
            controller: 'OSReceiptValueAddedController'
        })
        .when('/receive-billing', {
            templateUrl: 'Outsourcing/OSReceiveBilling/aplos',
            controller: 'OSReceiveBillingController'
        })
        .when('/jobwork-register', {
            templateUrl: 'Outsourcing/JobWorkRegister/aplos',
            controller: 'JobWorkRegisterController'
        })

        //--------------------
        //.when('/jw-activity', {
        //    templateUrl: 'Outsourcing/JWActivity/aplos',
        //    controller: 'jwActivityController'
        //})
        //.when('/jw-location', {
        //    templateUrl: 'Outsourcing/JWLocation/aplos',
        //    controller: 'jwLocationController'
        //})
        //.when('/jw-transformation', {
        //    templateUrl: 'Outsourcing/JWTransformationMaster',
        //    controller: 'jwTransformationMasterController'
        //})
        //.when('/jw-item', {
        //    templateUrl: 'Outsourcing/JWItem/aplos',
        //    controller: 'jwItemController'
        //})
        .when('/os-po', {
            templateUrl: 'Outsourcing/OSTransformationPO',
            controller: 'OSTransformationPOController'
        })
        .when('/jw-po-issue', {
            templateUrl: 'Outsourcing/JWPOIssue',
            controller: 'jwPOIssueController'
        })

        .when('/os-issue-register', {
            templateUrl: 'Outsourcing/OSissueRegister',
            controller: 'OSissueRegisterController'
        })

        //--------------------
        .when('/outsource-billing-post', {
            templateUrl: 'Outsourcing/OutSourceBillingPost/aplos',
            controller: 'OutsourceBillingPostController'
        })

      /*  .when('/jobwork-entry', {
            templateUrl: 'Outsourcing/JobWorkEntry/aplos',
            controller: 'JobWorkEntryController'
        })*/
        ;
}