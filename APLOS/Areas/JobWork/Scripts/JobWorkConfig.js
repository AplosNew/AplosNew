JobWorkConfig.$inject = ['$routeProvider', '$locationProvider'];
function JobWorkConfig($routeProvider, $locationProvider) {
    $routeProvider       
        .when('/jobwork-item', {
            templateUrl: 'JobWork/jobworkitem/aplos',
            controller: 'jobWorkItemController'
        })
        .when('/jobwork-activity', {
            templateUrl: 'JobWork/jobworkactivity/aplos',
            controller: 'jobWorkActivityController'
        })
        .when('/jobwork-location', {
            templateUrl: 'JobWork/jobworklocation/aplos',
            controller: 'jobWorkLocationController'
        })
        .when('/valueadded-master', {
            templateUrl: 'JobWork/jobworkvalueaddedmaster/aplos',
            controller: 'jobWorkValueAddedMasterController'
        })
        .when('/transformation-master', {
            templateUrl: 'JobWork/jobworktransformationmaster/aplos',
            controller: 'jobWorkTransformationMasterController'
        })
        .when('/valueadded-contract', {
            templateUrl: 'JobWork/JobWorkValueAddedContract/aplos',
            controller: 'JobWorkValueAddedContractController'
        })
        .when('/jobwork-issue-return', {
            templateUrl: 'JobWork/JobWorkIssueReturn/aplos',
            controller: 'JobWorkIssueReturnController'
        })
        .when('/confirm-issue', {
            templateUrl: 'JobWork/JobWorkIssueReturnConfirmation/aplos',
            controller: 'JobWorkIssueReturnConfirmationController'
        })
        .when('/material-reconcilation-report', {
            templateUrl: 'JobWork/MaterialReconcilationReport/aplos',
            controller: 'MaterialReconcilationReportController'
        })
        .when('/receipt', {
            templateUrl: 'JobWork/JobWorkReceiptValueAdded/aplos',
            controller: 'JobWorkReceiptValueAddedController'
        })
        .when('/jobwork-register', {
            templateUrl: 'JobWork/JobWorkRegister/aplos',
            controller: 'JobWorkRegisterController'
        })

        //--------------------
        .when('/jw-activity', {
            templateUrl: 'JobWork/JWActivity',
            controller: 'jwActivityController'
        })
        .when('/jw-location', {
            templateUrl: 'JobWork/JWLocation',
            controller: 'jwLocationController'
        })
        .when('/jw-transformation', {
            templateUrl: 'JobWork/JWTransformationMaster',
            controller: 'jwTransformationMasterController'
        })   
        .when('/jw-item', {
            templateUrl: 'JobWork/JWItem',
            controller: 'jwItemController'
        }) 
        .when('/jw-po', {
            templateUrl: 'JobWork/JWTransformationPurchaseOrder',
            controller: 'jwTransformationPurchaseOrderController'
        })
        .when('/jw-po-issue', {
            templateUrl: 'JobWork/JWPOIssue',
            controller: 'jwPOIssueController'
        })
        //--------------------
        ;
}