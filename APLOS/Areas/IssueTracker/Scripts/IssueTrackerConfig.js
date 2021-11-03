IssueTrackerConfig.$inject = ['$routeProvider'];
function IssueTrackerConfig($routeProvider) {
    $routeProvider
        .when('/issue-category', {
            templateUrl: 'IssueTracker/IssueCategory/Aplos',
            controller: 'issueCategoryController'
        })
        .when('/issue-subcategory', {
            templateUrl: 'IssueTracker/IssueSubCategory/Aplos',
            controller: 'issueSubCategoryController'
        })
        .when('/issue-importance', {
            templateUrl: 'IssueTracker/IssueImportance/Aplos',
            controller: 'issueImportanceController'
        })
        .when('/issue-standard', {
            templateUrl: 'IssueTracker/IssueStandard/Aplos',
            controller: 'issueStandardController'
        })
        .when('/issue-transaction', {
            templateUrl: 'IssueTracker/IssueTransaction/Aplos',
            controller: "issueTransactionController"
        })
        .when('/issue-ref', {
            templateUrl: 'IssueTracker/IssueRef/Aplos',
            controller: "issueRefController"
        })
        //.when('/subAssetType', {
        //    templateUrl: 'IssueTracker/SubAssetType/Aplos',
        //    controller: "subAssetTypeController"
        //})
        .when('/issue-audit', {
            templateUrl: 'IssueTracker/IssueAudit/Aplos',
            controller: "issueAuditController"
        })
        .when('/issue-subtask', {
            templateUrl: 'IssueTracker/IssueSubTask/Aplos',
            controller: "issueSubTaskController"
        })
        .when('/issue-internal-audit', {
            templateUrl: 'IssueTracker/IssueInternalAudit/Aplos',
            controller: "issueInternalAuditController"
        })
        .when('/issue-update-audit', {
            templateUrl: 'IssueTracker/IssueInternalAudit/Aplos',
            controller: "issueUpdateAuditController"
        })
        .when('/issue-group', {
            templateUrl: 'IssueTracker/IssueGroup/Aplos',
            controller: "issueGroupController"
        })

        .when('/issue-report', {
            templateUrl: 'IssueTracker/IssueTransaction/IssueReport',
            controller: "issueReportController"
        })

        .when('/secretarial-document-category', {
            templateUrl: 'IssueTracker/SecretarialDocumentCategory/Aplos',
            controller: "SecretarialDocumentCategoryController"
        })


        .when('/secretarial-document-subcategory', {
            templateUrl: 'IssueTracker/SecretarialDocumentSubCategory/Aplos',
            controller: "SecretarialDocumentSubCategoryController"
        })

        ;
}