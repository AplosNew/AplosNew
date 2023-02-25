dailyAttendanceInOutLoginController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService', '$cookies'];
function dailyAttendanceInOutLoginController($scope, $http, $location, $rootScope, $window, baseService, $cookies) {
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
                url: 'dailyattendance/home/login',
                params: {
                    'id': $scope.model.Id,
                    'pin': $scope.model.Pin
                },
                contentType: "application/json; charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                else {
                    if (response.data.IsFirstLogin) {
                        $window.location = 'dailyattendance/';
                        $cookies.put('empId', '');
                        $cookies.put('empId', $scope.model.Id);
                    }
                    else
                        
                        location.href = 'dailyattendance/home/changepin?id=' + $scope.model.Id;
                        //$window.location = 'recruitments/home/changepin?id=' + $scope.model.Id;
                }
            }, function errorCallback(response) {
                $scope.errorText = response.data.ErrorText || response.data.Message;
            });
        } catch (e) {
            $scope.errorText = e;
        }
    };
}