CurrencyConfig.$inject = ['$routeProvider', '$locationProvider'];
function CurrencyConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/currency', {
            templateUrl: 'Currencies/Currency/Aplos',
            controller: 'currencyController'
        })
        .when('/company-group-currency', {
            templateUrl: 'Currencies/Currency/CompanyGroupCurrency',
            controller: 'companyGroupCurrencyController'
        })
        .when('/company-parallel-currency', {
            templateUrl: 'Currencies/Currency/CompanyParallelCurrency',
            controller: 'companyParallelCurrencyController'
        })
        .when('/transaction-currency', {
            templateUrl: 'Currencies/Currency/TransactionCurrency',
            controller: 'currencyTransactionController'
        })
        .when('/exchange-rate', {
            templateUrl: 'Currencies/Currency/ExchangeRate',
            controller: 'exchangeRateController'
        })
        ;
}