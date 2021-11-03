EmployeeAccessLoginController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService', '$cookies'];
function EmployeeAccessLoginController($scope, $http, $location, $rootScope, $window, baseService, $cookies) {
    $scope.title = 'Login';
    $scope.errorHide = true;
    $scope.errorText = '';

    $scope.employee = {
        Id: null,
        InitialPIN: null
    };

    $scope.Login = function () {
        try {
            $scope.errorHide = true;
            $http({
                method: 'POST',
                url: 'employee/getemployeelist',
                params: {
                    'id': $scope.employee.Id,
                    'initialpin': $scope.employee.InitialPIN
                },
                contentType: "application/json; charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.errorHide = false;
                    $scope.errorText = response.data.ErrorText || response.data.Message;
                }
                else {
                    $cookies.put('CompanyGroupId', response.data.CompanyGroupId);
                    $cookies.put('LogoFileName', response.data.LogoFileName);
                    $cookies.put('Id', response.data.Id);
                    $cookies.put('EmployeeName', response.data.Name);
                    location.href = 'DashBoard/Index';
                    $scope.errorHide = true;
                    $scope.errorText = '';
                }
            });
        } catch (e) {
            $scope.errorHide = false;
            $scope.errorText = e;
        }
    };

    $scope.LogOff = function () {
        location.href = 'EmployeeAccess';
    }
};