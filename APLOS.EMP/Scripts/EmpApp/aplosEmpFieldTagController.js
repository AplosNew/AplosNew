AplosEmpFieldTagController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', 'baseService'];
function AplosEmpFieldTagController($scope, $http, $location, $rootScope, $window, baseService) {
    $scope.title = 'Employee Field Tag';
    $scope.Action = "Save";
    $scope.CompanyGroupId = null;
    $scope.companyGroups = [];
    function getCompanyGroupData() {
        $http({
            method: 'GET',
            url: 'AplosEmpFieldTag/GetCompanyGroupCbo'
        }).then(function successCallback(response) {
            $scope.companyGroups = response.data;
            getData();
        })
    }
    getCompanyGroupData();
    function getData() {
        $http({
            method: 'GET',
            url: 'AplosEmpField/GetCbo'
        }).then(function successCallback(response) {
            $scope.employeeFields = response.data;
            //$scope.employeeTagList[0].AplosEmpFieldId = null;
        })
    }
    $scope.employeeTagList = [];
    function getDefaultValue() {
        $scope.employeeTagList = [];
        for (var i = 1; i < 11; i++) {
            $scope.employeeTagList.push({
                Id: null,
                Sequence: null,
                ColumnName: "Col" + [i],
                IsAplicable: false,
                AplosEmpFieldId: null,
                ClientColumnId: null,
                ClinetColumnName: null,
                CompanyGroupId: null
            })
        }
    }
    getDefaultValue();
    $scope.getEmpTag = function () {
        if (!baseService.isUndefinedOrNull($scope.CompanyGroupId)) {
            $http({
                method: 'GET',
                url: 'AplosEmpFieldTag/GetList?companyGroupId=' + $scope.CompanyGroupId
            }).then(function successCallback(response) {
                // $scope.employeeTagList = response.data.Rows;
                angular.forEach(response.data.Rows, function (item) {
                    for (var i = 0; i < $scope.employeeTagList.length; i++) {
                        if (item.ColumnName === $scope.employeeTagList[i].ColumnName) {
                            $scope.employeeTagList[i] = item;
                        }
                    }
                })
                for (var i = 0; i < $scope.employeeTagList.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.employeeTagList[i].AplosEmpFieldId)) {
                        $scope.employeeTagList[i].AplosEmpFieldId = $scope.employeeTagList[i].AplosEmpFieldId.toString();
                    }
                }
                if ($scope.employeeTagList.length < 1) {
                    getDefaultValue();
                }
            });
        } else {
            getDefaultValue();
        }
    };
    $scope.checkDuplicate = function (id, index) {
        try {
            for (var i = 0; i < $scope.employeeTagList.length; i++) {
                if (index != i && $scope.employeeTagList[i].AplosEmpFieldId === id) {
                    return true;
                }
            }
            return false;
        } catch (e) {
            throw e;
        }
    }
    function validateAplicable(list, index) {
        try {
            for (var i = index; i > 0; i--) {
                if (list[i].IsAplicable === false && !baseService.isUndefinedOrNull(list[i].AplosEmpFieldId) && !baseService.isUndefinedOrNull(list[i].ClientColumnId)) {
                    throw "Columns should be applicable sequentially.";
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CompanyGroupId)) {
                throw "CompanyGroup required";
            };
            if ($scope.Action === 'Save' && $scope.employeeTagList.length > 0) {
                angular.forEach($scope.employeeTagList, function (item, i) {
                    item.CompanyGroupId = $scope.CompanyGroupId;
                    if (item.IsAplicable && baseService.isUndefinedOrNull(item.AplosEmpFieldId) && baseService.isUndefinedOrNull(item.ClientColumnId)) {
                        item.IsAplicable = false;
                    }
                    if (item.IsAplicable && !baseService.isUndefinedOrNull(item.AplosEmpFieldId) && !baseService.isUndefinedOrNull(item.ClientColumnId)) {
                        item.Sequence = i + 1;
                        validateAplicable($scope.employeeTagList, i);
                    }
                    if (!baseService.isUndefinedOrNull(item.AplosEmpFieldId)) {
                        if ($scope.checkDuplicate(item.AplosEmpFieldId, i)) {
                            throw "Aplos name can not be duplicate";
                        };
                    }
                });
                $http({
                    method: 'post',
                    url: "/AplosEmpFieldTag/Create",
                    data: $scope.employeeTagList,
                    dataType: 'json'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getData();
                        $scope.getEmpTag();
                    }
                }), function errorCallBack(response) {
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.LogOff = function () {
        location.href = 'CPanel';
    }
}