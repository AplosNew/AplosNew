SecurityConfig.$inject = ['$routeProvider', '$locationProvider'];
function SecurityConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/control-admin', {
            templateUrl: 'Securities/controladmin/aplos',
            controller: 'controlAdminController'
        })
        .when('/control-admin-reset/:userId', {
            templateUrl: 'Securities/controladmin/reset',
            controller: 'controlAdminPasswordResetController'
        })
        .when('/system-admin', {
            templateUrl: 'Securities/systemadmin/aplos',
            controller: 'systemAdminController'
        })
        .when('/system-admin-reset/:id', {
            templateUrl: 'Securities/systemadmin/reset',
            controller: 'systemAdminResetController'
        })
        .when('/system-admin-authtoken/:id', {
            templateUrl: 'Securities/systemadmin/authtokenchange',
            controller: 'systemAdminAuthtokenController'
        })
        .when('/sys-authtoken-lock', {
            templateUrl: 'Securities/systemadmin/sysadminauthtokenunlock',
            controller: 'sysAuthTokenLockController'
        })
        .when('/sys-user-lock', {
            templateUrl: 'Securities/systemadmin/sysunlock',
            controller: 'sysLockController'
        })
        .when('/authtoken-lock-log/:id', {
            templateUrl: 'Securities/authTokenLockLog/aplos',
            controller: 'authTokenLockLogController'
        })
        .when('/user-lock-log/:id', {
            templateUrl: 'Securities/userlocklog/aplos',
            controller: 'userLockLogController'
        })
        .when('/encrypt-decrypt', {
            templateUrl: 'Securities/controladmin/encryptdecrypt',
            controller: 'encryptDecrypttController'
        })
        .when('/password-change/:id', {
            templateUrl: 'Securities/controladmin/change',
            controller: 'cPasswordChangeController'
        })
        .when('/credential-policy', {
            templateUrl: 'Securities/credentialpolicy/aplos',
            controller: 'credentialPolicyController'
        })
        .when('/user', {
            templateUrl: 'Securities/user/aplos',
            controller: 'userController'
        })
        .when('/user-password-reset/:id', {
            templateUrl: 'Securities/user/reset',
            controller: 'userPasswordResetController'
        })
        .when('/user-authtoken-change/:id', {
            templateUrl: 'Securities/user/authtokenchange',
            controller: 'userAuthtokenController'
        })
        .when('/user-password-change/:id', {
            templateUrl: 'Securities/user/passwordchange',
            controller: 'userPasswordChangeController'
        })
        .when('/authtoken-lock-log/:id', {
            templateUrl: 'Securities/authTokenLockLog/aplos',
            controller: 'authTokenLockLogController'
        })
        .when('/user-lock-log/:id', {
            templateUrl: 'Securities/userlocklog/aplos',
            controller: 'userLockLogController'
        })
        .when('/employee-mobile-apps-authorization', {
            templateUrl: 'Securities/employeeMobileAppsAuthorization/aplos',
            controller: 'employeeMobileAppsAuthorizationController'
        })
        .when('/mobile-apps', {
            templateUrl: 'Securities/employeeMobileAppsAuthorization/MobileApp',
            controller: 'employeeMobileAppsAuthorizationNewController'
        })
        .when('/authtoken-lock', {
            templateUrl: 'Securities/user/authtokenunlock',
            controller: 'authTokenLockController'
        })
        .when('/user-lock', {
            templateUrl: 'Securities/user/userunlock',
            controller: 'userLockController'
        })
        .when('/role', {
            templateUrl: 'Securities/role/aplos',
            controller: 'roleController'
        })
        .when('/role-detail', {
            templateUrl: 'Securities/roledetail/aplos',
            controller: 'roleDetailController'
        })
        .when('/role-detail-action', {
            templateUrl: 'Securities/roledetail/roledetailaction',
            controller: 'roleDetailActionController'
        })
        .when('/role-mapping-position', {
            templateUrl: 'Securities/rolemapping/rolemappingposition',
            controller: 'roleMappingPositionController'
        })
        .when('/role-mapping-manpower-budget', {
            templateUrl: 'Securities/rolemapping/rolemappingmanpowerbudget',
            controller: 'roleMappingManPowerBudgetController'
        })
        .when('/user-role', {
            templateUrl: 'Securities/userrole/aplos',
            controller: 'userRoleController'
        })
        .when('/user-role-detail', {
            templateUrl: 'Securities/userroledetail/aplos',
            controller: 'userRoleDetailController'
        })
        .when('/user-access-plant', {
            templateUrl: 'Securities/userAccessPlant',
            controller: 'userAccessPlantController'
        })
        .when('/additional-role', {
            templateUrl: 'Securities/userroledetail/additionalrole',
            controller: 'additionalRoleController'
        })
        .when('/additional-role-action', {
            templateUrl: 'Securities/userroledetail/additionalroleaction',
            controller: 'additionalRoleActionController'
        })
        .when('/user-access-app', {
            templateUrl: 'Securities/useraccessapp/aplos',
            controller: 'userAccessAppController'
        })
        .when('/show-all-user', {
            templateUrl: 'Securities/SystemAdmin/showalluser',
            controller: 'showAllUserController'
        })
        .when('/user-entity', {
            templateUrl: 'Securities/UserEntity',
            controller: 'userEntityController'
        })
        .when('/sync-url', {
            templateUrl: 'Securities/SyncURL',
            controller: 'syncURLController'
        })
        .when('/menu-user-code', {
            templateUrl: 'Securities/MenuUserCode',
            controller: 'menuUserCodeController'
        })
        .when('/lic', {
            templateUrl: 'Securities/LIC/Aplos',
            controller: 'LICController'
        })
        .when('/user-app-authentication', {
            templateUrl: 'Securities/UserAppAuthentication/Aplos',
            controller: 'UserAppAuthenticationController'
        })
        .when('/app-role', {
            templateUrl: 'Securities/AppRole/Aplos',
            controller: 'AppRoleController'
        })
        .when('/user-app-role', {
            templateUrl: 'Securities/UserAccessAppRole/Aplos',
            controller: 'UserAccessAppRoleController'
        })
        ;
}