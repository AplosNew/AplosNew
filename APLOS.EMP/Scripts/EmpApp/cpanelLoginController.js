CPanelLoginController.$inject = ['$scope', '$http', '$location', '$rootScope', '$window', 'baseService'];
function CPanelLoginController($scope, $http, $location, $rootScope, $window, baseService) {
    $scope.title = 'Login';
    $scope.errorHide = true;
    $scope.errorText = '';

    $scope.cPanellogin = {
        ID: null,
        PIN: null
    };

    $scope.Login = function () {
        $http({
            method: 'GET',
            url: 'cpanel/cpanellogin?id=' + $scope.cPanellogin.ID + '&pin=' + $scope.cPanellogin.PIN,
            dataType: 'json'
        }).then(function successCallback(response) {
            if (response.data.Flag === false) {
                $scope.errorHide = false;
                $scope.errorText = response.data.Message;
            }
            else
                location.href = 'PhoenixLogin';
        }, function errorCallback(response) {
            $scope.errorHide = false;
            $scope.errorText = response.data.Message;
        });
    };

    $scope.LogOff = function () {
        location.href='CPanel';
    }
};