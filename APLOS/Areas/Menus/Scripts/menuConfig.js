MenuConfig.$inject = ['$routeProvider', '$locationProvider'];
function MenuConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/menu-frame', {
            templateUrl: 'Menus/menuframe',
            controller: 'menuFrameController'
        })
        .when('/menu-group', {
            templateUrl: 'Menus/menugroup',
            controller: 'menuGroupController'
        })
        .when('/menu-sub-group', {
            templateUrl: 'Menus/menusubgroup',
            controller: 'menuSubGroupController'
        })
        .when('/menu-item', {
            templateUrl: 'Menus/menuItem',
            controller: 'menuItemController'
        })
        .when('/menu-master', {
            templateUrl: 'Menus/menumaster',
            controller: 'menuMasterController'
        })
        .when('/menu-master-edit/:id', {
            templateUrl: 'Menus/menumaster/edit',
            controller: 'menuMasterEditController'
        })
        .when('/menu', {
            templateUrl: 'Menus/menu',
            controller: 'menuController'
        })
        .when('/menu-action', {
            templateUrl: 'Menus/menuaction',
            controller: 'menuActionController'
        })
        .when('/company-group-menu-master', {
            templateUrl: 'Menus/companygroupmenumaster',
            controller: 'companyGroupMenuMasterController'
        }).when('/menu-new', {
            templateUrl: 'Menus/menucreation/Index',
            controller: 'menuCreationController'
        }).when('/menu-sync', {
            templateUrl: 'Menus/MenuSync',
            controller: 'menuSyncController'
        });
}