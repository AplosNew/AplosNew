'use strict';
downloadAuthTokenController.$inject = ['commonMessage', '$scope', '$rootScope', '$routeParams', '$http', '$filter'];
function downloadAuthTokenController(commonMessage, $scope, $rootScope, $routeParams, $http, $filter) {
    $rootScope.title = 'Download AuthToken';
    $scope.errorText = null;
    $scope.model = {
        username: null
        , password: null
        , dateOfBirth: null
        , email: null
        , captcha: null
    };
    $scope.download = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            $http({
                method: 'POST',
                url: 'Download/AuthToken',
                data: {
                    username: $scope.model.username
                    , password: $scope.model.password
                    , dateOfBirth: $scope.model.dateOfBirth
                    , email: $scope.model.email
                    , captcha: $scope.model.captcha
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    return $scope.errorText = response.data.Message;
                else {
                    $scope.errorText = null;
                    var data = response.data, fileName = "secureclientconfig";
                    saveData(data, fileName);
                }
            }, function errorCallback(response) {
                return $scope.errorText = response.data.Message;
            });
        }
    };

    var saveData = (function () {
        var a = document.createElement("a");
        document.body.appendChild(a);
        a.style = "display: none";
        return function (data, fileName) {
            var json = data,
                blob = new Blob([json], { type: "octet/stream" }),
                url = window.URL.createObjectURL(blob);
            a.href = url;
            a.download = fileName;
            a.click();
            window.URL.revokeObjectURL(url);
        };
    }());
}