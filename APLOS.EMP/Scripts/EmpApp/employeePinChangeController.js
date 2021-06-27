EmployeePinChangeController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService','$cookies'];
function EmployeePinChangeController($scope, $http, $location, $rootScope, $window, baseService, $cookies) {
    $scope.title = 'Login';
    $scope.errorHide = true;
    $scope.errorText = '';

    $scope.employee = {
        Id: $window.employeeId,
        NewPIN: null
    };
    $scope.PinChange = function () {
        try {

            $http({
                method: "post",
                url: 'employee/updatepin',
                data: {
                    'id': $scope.employee.Id,
                    'newPin': $scope.employee.NewPIN
                },
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    $scope.errorHide = false;
                    $scope.errorText = response.data.Message;
                }
                else {
                    $scope.errorHide = true;
                    $scope.errorText = '';
                    location.href = 'employee/login';
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            $scope.errorHide = false;
            $scope.errorText = e;
        }
    };

    $scope.LogOff = function () {
        location.href = 'CPanel';
    }
};