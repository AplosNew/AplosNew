addressConfig.$inject = ['$routeProvider', '$locationProvider'];
function addressConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/continent', {
            templateUrl: 'Addresses/Continent/Continent',
            controller: 'continentController'
        })
        .when('/country', {
            templateUrl: 'Addresses/Country/Country',
            controller: 'countryController'
        })
        .when('/state', {
            templateUrl: 'Addresses/State/State',
            controller: 'stateController'
        })
        .when('/city', {
            templateUrl: 'Addresses/City/City',
            controller: 'cityController'
        })
        .when('/area', {
            templateUrl: 'Addresses/Area/Area',
            controller: 'areaController'
        })
        .when('/district', {
            templateUrl: 'Addresses/District/District',
            controller: 'districtController'
        })
        .when('/postoffice', {
            templateUrl: 'Addresses/PostOffice/PostOffice',
            controller: 'postOfficeController'
        })
        .when('/policestation', {
            templateUrl: 'Addresses/PoliceStation/PoliceStation',
            controller: 'policeStationController'
        })
        .when('/smtp-config', {
            templateUrl: 'Addresses/SMTPConfiguration/SMTPConfiguration',
            controller: 'smtpConfigurationController'
        });
}