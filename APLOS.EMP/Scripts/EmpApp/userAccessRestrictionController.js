UserAccessRestrictionController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', '$cookies'];
function UserAccessRestrictionController($scope, $http, $location, $rootScope, $window, $compile, baseService, $cookies) {
    $scope.title = 'User Access Restriction Controller';
    $scope.getListUrl = 'useraccessrestriction/getlist?companyGroupId=' + $cookies.get('CompanyGroupId');
    $scope.employees = [];
    baseService.init($scope.getListUrl, null, 50, null, 'Name', 'Name');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employees = result.Rows;
                if (baseService.arrayLength($scope.restrictionList) > 0) {
                    for (var i = 0; i < baseService.arrayLength($scope.employees); i++) {
                        $scope.employees[i].Flag = FlagFororPagination($scope.restrictionList, $scope.employees[i].Id, $scope.employees[i].Flag)
                    }
                }
                $scope.flag = allTrue();
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    function FlagFororPagination(list, id, flag) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Id === id) return list[i].IsAccessRestricted;
        }
        return flag;
    }
    $scope.searchbyEmployeelist = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Company Name',
            'value': 'CompanyName'
        },
        {
            'name': 'Employee Name',
            'value': 'Name'
        },
        {
            'name': 'Email',
            'value': 'Email'
        },
        {
            'name': 'Mobile',
            'value': 'Mobile'
        },
        {
            'name': 'Status',
            'value': 'Status'
        },
        {
            'name': 'Access Restriction',
            'value': 'AccessRestricted'
        }
    ];

    $scope.restrictionList = [];
    $scope.SelectOrDeselect = function (id, event) {
        UnCheck(event)
        if (baseService.arrayLength($scope.restrictionList) > 0) {
            if (checkIfExist($scope.restrictionList, id)) {
                for (var i = 0; i < baseService.arrayLength($scope.restrictionList); i++) {
                    if ($scope.restrictionList[i].Id === id) {
                        $scope.restrictionList[i].IsAccessRestricted = event.currentTarget.checked;
                        return;
                    }
                }
            }
            else {
                $scope.restrictionList.push({
                    Id: id,
                    IsAccessRestricted: event.currentTarget.checked
                });
                return;
            }
        } else {
            $scope.restrictionList.push({
                Id: id,
                IsAccessRestricted: event.currentTarget.checked
            });
            return;
        }
    }
    function checkIfExist(list, value) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Id === value) return true;
        }
        return false;
    }


    // #region checkAll

    $scope.CheckAll = function (event, list) {
        var _isselected = event.target.checked;
        var _name = event.target.name;
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            list[i][_name] = _isselected;
            SelectOrDeselectOneClick(list[i].Id, _isselected)
        }
        console.log($scope.restrictionList);
    }
    function SelectOrDeselectOneClick(id, _isselected) {
        if (baseService.arrayLength($scope.restrictionList) > 0) {
            if (checkIfExist($scope.restrictionList, id)) {
                for (var i = 0; i < baseService.arrayLength($scope.restrictionList); i++) {
                    if ($scope.restrictionList[i].Id === id) {
                        $scope.restrictionList[i].IsAccessRestricted = _isselected;
                        return;
                    }
                }
            }
            else {
                $scope.restrictionList.push({
                    Id: id,
                    IsAccessRestricted: _isselected
                });
                return;
            }
        } else {
            $scope.restrictionList.push({
                Id: id,
                IsAccessRestricted: _isselected
            });
            return;
        }
    }
    function UnCheck(event) {
        var _isselected = event.target.checked;
        $scope.flag = allTrue();
    }
    function allTrue() {
        var flag = false;
        for (var i = 0; i < baseService.arrayLength($scope.employees); i++) {
            if ($scope.employees[i].Flag)
                flag = true;
            else
                return false;
        }
        return flag;
    }
    // #endregion

    $scope.Save = function () {
        try {
            $http({
                method: "post",
                url: 'useraccessrestriction/update',
                data: $scope.restrictionList,
                dataType: "json"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData();
                    $scope.restrictionList = [];
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.LogOff = function () {
        location.href = 'EmployeeAccess';
    }
};