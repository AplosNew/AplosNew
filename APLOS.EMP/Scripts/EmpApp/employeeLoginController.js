EmployeeLoginController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService', '$cookies'];
function EmployeeLoginController($scope, $http, $location, $rootScope, $window, baseService, $cookies) {
    $scope.title = 'Login';
    $scope.errorHide = true;
    $scope.errorText = '';

    $scope.employee = {
        Id: null,
        IsFirstlogin: false,
        InitialPIN: null
    };

    $scope.Login = function () {
        try {
            $http.get('employee/getlist?id=' + $scope.employee.Id + '&initialpin=' + encodeURIComponent($scope.employee.InitialPIN))
                .then(function (response) {
                    if (response.data.Rows.length > 0) {
                        $cookies.put('companyGroupId', response.data.Rows[0].CompanyGroupId);
                        $cookies.put('groupName', response.data.Rows[0].GroupName);
                        $cookies.put('logoFileName', response.data.Rows[0].LogoFileName);
                        $cookies.put('documentFolderName', response.data.Rows[0].DocumentFolderName);
                        $cookies.put('employeeId', $scope.employee.Id);
                        $cookies.put('employeeName', response.data.Rows[0].EmployeeName);
                        $cookies.put('employeePin', response.data.Rows[0].EmployeePin);

                        if (!$scope.employee.IsFirstlogin || $scope.employee.IsFirstlogin === null) {
                            location.href = 'employee/pinchange';
                        }
                        else location.href = 'employee';
                        $scope.errorHide = true;
                        $scope.errorText = '';
                    }
                    else {
                        $scope.errorHide = false;
                        $scope.errorText = 'Invalid employee or pin';
                    }
                });
        } catch (e) {
            $scope.errorHide = false;
            $scope.errorText = e;
        }
    };
};