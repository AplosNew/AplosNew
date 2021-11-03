AplosEmpFieldController.$inject = ['$scope', '$http'];
function AplosEmpFieldController($scope, $http) {
    $scope.Action = 'Save';
    $scope.AplosEmpField = {
        Id: null,
        InterfaceIdField: null,
        InterfaceFieldName: null,
        AplosColumnId: null,
        AplosColumnName: null,
        Active: true
    };
    $scope.employeeFields = [];
    function getData() {
        $http({
            method: 'GET',
            url: 'AplosEmpField/GetList'
        }).then(function successCallback(response) {
            $scope.employeeFields = response.data.Rows;
        })
    }
    getData();
    $scope.Save = function () {
        if ($scope.Action === 'Save') {
            $http({
                method: 'post',
                url: 'AplosEmpField/Create',
                data: $scope.AplosEmpField,
                dataType: 'json'
            }).then(function successCallback(response) {
                $scope.buttonDisable = true;
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.buttonDisable = false;
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AplosEmpField = {};
                    getData();
                    $scope.buttonDisable = false;
                }
            }), function errorCallBack(response) {
            }
        } else if ($scope.Action === 'Update') {
            $http({
                method: 'post',
                url: 'AplosEmpField/Edit',
                data: $scope.AplosEmpField,
                dataType: 'json'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AplosEmpField = {};
                    getData();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    };
    $scope.Delete = function () {
        if ($scope.AplosEmpField.Id != undefined || $scope.AplosEmpField.Id != null) {
            $http({
                method: 'post',
                url: 'AplosEmpField/Delete?id=' + $scope.AplosEmpField.Id,
                dataType: 'json'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AplosEmpField = {};
                    getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.getData = function (data) {
        var tempData = angular.copy(data);
        $scope.AplosEmpField = tempData;
        $scope.Action = 'Update';
    }
    $scope.Clear = function () {
        $scope.AplosEmpField = { Active: true };
        $scope.Action = 'Save';
    }

    $scope.LogOff = function () {
        location.href = 'CPanel';
    }
};