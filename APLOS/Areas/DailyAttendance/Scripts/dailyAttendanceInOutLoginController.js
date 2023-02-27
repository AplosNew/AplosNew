dailyAttendanceInOutLoginController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService', '$cookies'];
function dailyAttendanceInOutLoginController($scope, $http, $location, $rootScope, $window, baseService, $cookies) {
    $scope.title = 'Login';
    $scope.path = 'dailyattendance/home/';
    $scope.errorText = null;

    $scope.model = {
        Id: null,
        //Pin: null
    };

    $scope.Login = function () {        
        try {
            $scope.errorText = null;
            $http({
                method: 'POST',
                url: $scope.path + 'LoginDailyAttendance',
                params: {
                    'id': $scope.model.Id,
                    
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                else {

                    $window.location = 'dailyattendance/home/Aplos?u=' + $scope.model.Id;
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
}