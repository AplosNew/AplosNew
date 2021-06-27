changePinController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService', '$cookies'];
function changePinController($scope, $http, $location, $rootScope, $window, baseService, $cookies) {
    $scope.title = 'Login';
    $scope.errorText = null;

    $scope.model = {
        Id: null,
        Pin: null
    };

    $scope.Login = function () {
        try {
            $scope.errorText = null;
            $http({
                method: 'POST',
                url: '',
                //url: 'Recruitments/home/changepin',
                params: {
                    'id': $scope.model.Id,
                    'pin': $scope.model.Pin,
                },
                contentType: "application/json; charset=utf-8",
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                else {
                    $window.location = 'aplos?id=' + $scope.model.Id;
                    $cookies.put('empId', '');
                    $cookies.put('empId', $scope.model.Id);
                }
            }, function errorCallback(response) {
                $scope.errorText = response.data.ErrorText || response.data.Message;
            });
        } catch (e) {
            $scope.errorText = e;
        }
    };
};