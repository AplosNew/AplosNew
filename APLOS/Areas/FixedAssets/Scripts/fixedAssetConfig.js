fixedAssetConfig.$inject = ['$routeProvider', '$locationProvider'];
function fixedAssetConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/fixed-asset', {
            templateUrl: 'FixedAssets/fixedasset',
            controller: 'fixedAssetController'
        })
        .when('/fixed-asset-category', {
            templateUrl: 'FixedAssets/FixedAssetCategory/Aplos',
            controller: 'fixedAssetCategoryController'
        })
        .when('/fixed-asset-sub-category', {
            templateUrl: 'FixedAssets/FixedAssetSubCategory/Aplos',
            controller: 'fixedAssetSubCategoryController'
        })
        .when('/fixed-asset-master', {
            templateUrl: 'FixedAssets/fixedassetmaster/aplos',
            controller: 'fixedAssetMasterController'
        })
        .when('/fixed-asset-register', {
            templateUrl: 'FixedAssets/FixedAssetRegister/Aplos',
            controller: 'fixedAssetRegisterController'
        })
        .when('/fixed-asset-class', {
            templateUrl: 'FixedAssets/FixedAssetClass/Aplos',
            controller: 'fixedAssetClassController'
        })
        .when('/fixed-asset-subclass', {
            templateUrl: 'FixedAssets/FixedAssetSubClass/Aplos',
            controller: 'fixedAssetSubClassController'
        })
        .when('/fixed-assetmaster-machinetype', {
            templateUrl: 'FixedAssets/fixedassetmastermachinetype',
            controller: 'fixedAssetMasterMachineTypeController'
        })
        .when('/depreciation-rule', {
            templateUrl: 'FixedAssets/FixedAssetDepreciationRule/aplos',
            controller: 'fixedAssetDepreciationRuleController'
        })
        .when('/company-depreciation-rule', {
            templateUrl: 'FixedAssets/CompanyFixedAssetDepreciationRule/aplos',
            controller: 'companyFixedAssetDepreciationRuleController'
        })
        .when('/fixed-asset-attribute', {
            templateUrl: 'fixedassets/fixedassetattribute/aplos',
            controller: 'fixedAssetAttributeController'
        })
        .when('/fixed-asset-attribute-value', {
            templateUrl: 'FixedAssets/fixedassetattributevalue/aplos',
            controller: 'fixedAssetAttributeValueController'
        })
        .when('/assetitem-article', {
            templateUrl: 'FixedAssets/AssetItemArticle/aplos',
            controller: 'assetItemArticleController'
        })
        .when('/assetitem-characteristics', {
            templateUrl: 'FixedAssets/AssetItemCharacteristics/aplos',
            controller: 'assetItemCharacteristicsController'
        })
        .when('/fixedAssetMaster-gl', {
            templateUrl: 'FixedAssets/FixedAssetMasterGL/aplos',
            controller: 'fixedAssetMasterGLController'
        })
        .when('/fixedAssetMaster-budget-tag', {
            templateUrl: 'FixedAssets/FixedAssetMasterBudgetTag/aplos',
            controller: 'fixedAssetMasterBudgetTagController'
        })
        .when('/fixed-asset-register-expense', {
            templateUrl: 'FixedAssets/FixedAssetRegister/FixedAssetRegisterExpenseReport',
            controller: 'fixedAssetExpenseReportController'
        })
        .when('/fixed-asset-register-jvob', {
            templateUrl: 'FixedAssets/FixedAssetRegister/FixedAssetRegisterJVOB',
            controller: 'fixedAssetRegisterJVOBController'
        })
        .when('/fixed-asset-register-jv', {
            templateUrl: 'FixedAssets/FixedAssetRegister/FixedAssetRegisterJV',
            controller: 'fixedAssetRegisterJVController'
        })

        .when('/subAssetType', {
            templateUrl: 'FixedAssets/SubAssetType/aplos',
            controller: "subAssetTypeController"
        })

        .when('/fixed-asset-register-aucjv', {
            templateUrl: 'FixedAssets/FixedAssetRegister/FixedAssetRegisterAUCJV',
            controller: 'fixedAssetRegisterAUCJVController'
        })

        .when("/auc-capitalize-grnbass", {
            templateUrl: "FixedAssets/FixedAssetRegister/FixedAssetAUCCapitalizeGRNBass",
            controller: "fixedAssetAUCCapitalizeGRNBassController"
        })

        .when("/issue-auccapitalize", {
            templateUrl: "FixedAssets/FixedAssetRegister/IssueAUCCapitalize",
            controller: "issueAUCCapitalizeController"
        })

        .when("/capitalized-AssetRegister", {
            templateUrl: "FixedAssets/FixedAssetRegister/CapitalizedFixedAssetRegister",
            controller: "capitalizedFixedAssetRegisterController"
        })
        .when("/fixedAsset-depreciation-process", {
            templateUrl: "FixedAssets/FixedAssetRegister/FixedAssetDepreciationProcess",
            controller: "fixedAssetDepreciationProcessController"
        })

        .when("/capitalized-NonAsset-Register", {
            templateUrl: "FixedAssets/FixedAssetRegister/NonAssetRegister",
            controller: "nonAssetRegisterController"
        })

        .when("/fixed-assets-Register-report", {
            templateUrl: "FixedAssets/FixedAssetRegister/FixedAssetsRegisterReport",
            controller: "FixedAssetsRegisterReportController"
        })
        .when("/faRegister-dispose-report", {
            templateUrl: "FixedAssets/FixedAssetRegister/FixedAssetsRegisterDisposeReport",
            controller: "FixedAssetsRegisterDisposedReportController"
        })

        .when("/expense-capitalize", {
            templateUrl: "FixedAssets/FixedAssetRegister/ExpensesCapitalized",
            controller: "expensesCapitalizedController"
        })

        .when("/fixedasset-dispose", {
            templateUrl: "FixedAssets/FixedAssetRegister/FixedAssetDispose",
            controller: "fixedAssetDisposeController"
        })
        .when("/fixedasset-dispose-post", {
            templateUrl: "FixedAssets/FixedAssetRegister/FixedAssetDisposePost",
            controller: "fixedAssetDisposePostController"
        })

        .when("/fixedasset-depreciation-post", {
            templateUrl: "FixedAssets/FixedAssetRegister/FixedAssetDepreciationPost",
            controller: "fixedAssetDepreciationPostController"
        })

        .when("/generalledger-vs-fixedassets", {
            templateUrl: "FixedAssets/FixedAssetRegister/GLvsFA",
            controller: "generalLedgerVSfixedAssetsController"
        })

        .when("/entity-fixed-assets-register", {
            templateUrl: "FixedAssets/EntityFixedAssetsRegister/Aplos",
            controller: "entityFixedAssetsRegisterController"
        })

        .when("/fa-register", {
            templateUrl: "FixedAssets/FixedAssetRegister/FARegister",
            controller: "faRegisterController"
        })
        .when("/fa-register-posting", {
            templateUrl: "FixedAssets/FixedAssetRegister/CapitalizeAssetRegisterPosting",
            controller: "CapitalizeAssetRegisterPostingController"
        })
        .when("/add-info-update", {
            templateUrl: "FixedAssets/FixedAssetMaster/AdditionalInfoUpdate",
            controller: "AdditionalInfoUpdateController"
        })
        .when("/fa-depreciation-process", {
            templateUrl: "FixedAssets/FixedAssetRegister/AssetDepreciationProcess",
            controller: "assetDepreciationProcessController"
        })
        .when("/fa-depreciation-post", {
            templateUrl: "FixedAssets/FixedAssetRegister/AssetDepreciationPost",
            controller: "assetDepreciationPostController"
        })
        .when("/fa-dispose", {
            templateUrl: "FixedAssets/FixedAssetRegister/AssetDispose",
            controller: "assetDisposeController"
        })
        .when("/fa-register-report", {
            templateUrl: "FixedAssets/FixedAssetRegister/AssetsRegisterReport",
            controller: "AssetsRegisterReportController"
        })
        .when("/fa-depreciation-report", {
            templateUrl: "FixedAssets/FixedAssetRegister/AssetsDepreciationReport",
            controller: "assetsDepreciationReportController"
        })

        ;
}