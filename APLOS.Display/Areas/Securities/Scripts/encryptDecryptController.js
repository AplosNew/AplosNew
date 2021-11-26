'use strict';
EncryptDecryptController.$inject = ["$scope", "$http"];
function EncryptDecryptController($scope, $http) {
    $scope.encryptDecrypt = {
        Encrypt: null,
        Decrypt: null
    };

    $scope.EncryptText = function () {
        if ($scope.encryptDecrypt.Decrypt == null)
            alert("Please fill  in the textbox");
        else {
            $http({
                method: "get"
                , url: "Securities/controladmin/encrypttext?txt=" + encodeURIComponent($scope.encryptDecrypt.Decrypt)
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.encryptDecrypt.Decrypt = null;
                $scope.encryptDecrypt.Encrypt = response.data;
            });
            return true;
        }
    }

    $scope.DecryptText = function () {
        if ($scope.encryptDecrypt.Encrypt == null)
            alert("Please fill  in the encrypt textbox");
        else {
            $http({
                method: "get"
                , url: "Securities/controladmin/decrypttext?decrypttxt=" + encodeURIComponent($scope.encryptDecrypt.Encrypt)
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.encryptDecrypt.Encrypt = null;
                $scope.encryptDecrypt.Decrypt = response.data;
            });
            return true;
        }
    }
}
