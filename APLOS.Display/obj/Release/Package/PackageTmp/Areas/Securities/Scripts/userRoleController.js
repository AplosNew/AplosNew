'use strict';
UserRoleController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'dataShare', '$window'];
function UserRoleController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, dataShare, $window) {
    $rootScope.title = "User Access";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.inactive = false;
    $scope.userRoles = [];
    $scope.path = 'Securities/userrole/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'update';
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl, null, 10, null, 'CompanyName', 'UserId');
    $scope.getData = function (pageno) {
        $rootScope.parameters.search = $scope.userRole.UserId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.userRoles = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.userRoleNew = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        CompanyName: null,
        UserId: null,
        UserName: null,
        RoleId: null,
        RoleName: null,
        FullName: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    }
    $scope.userRole = Object.assign({}, $scope.userRoleNew);

    if (!baseService.isUndefinedOrNull($window.userId)) {
        $scope.userRole.UserId = $window.userId;
        $scope.userRole.UserName = $window.userName;
        $scope.userRole.FullName = $window.fullName;
        $scope.getData();
        $window.userId = null;
        $window.userName = null;
    }

    // #region DDL
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    cboService.getCboRoleByCompanyGroup(null, function (result) {
        $scope.roleList = result;
    });
    $http({
        method: 'GET',
        url: 'Securities/user/getuserlistwithoutsysadmin'
    }).then(function successCallback(response) {
        $scope.userList = response.data;
    });
    // #endregion

    //***********************************User ********************************************************//
    $rootScope.searchByUserList = [
        {
            'name': 'Username',
            'value': 'UserId'
        },
        {
            'name': 'User Type',
            'value': 'UserType'
        },
        {
            'name': 'Employee Id',
            'value': 'EmployeeId'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        },
        {
            'name': 'AuthToken',
            'value': 'AuthToken'
        }
    ];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserId',
        searchBy: "UserId",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpDataList = [];
        $scope.popUpUrl = 'Securities/user/getlist';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        if (data.SysAdmin)
            return ShowResult("User [" + data.UserId + "] is [" + data.UserType +"], so role is not required.", 'failure', 'popUpId')
        $scope.userRole.UserId = data.Id;
        $scope.userRole.UserName = data.UserId;
        $scope.userRole.FullName = data.FullName;
        $scope.getData();
        $scope.closePopUp();
    };
    $scope.selectSingleClick = function (data) {
        $scope.rowSelected = data.UserId;
        $scope.valueData = data;
    };
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData)
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    //***********************************User ********************************************************//
    $scope.oldCompanyIdForEdit = null;
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.userRoleNew = $scope.userRoles[$scope.index];
        //$scope.userRoleNew.FullName = $scope.userRole.FullName;
        $scope.userRole = angular.copy($scope.userRoleNew);
        $scope.oldCompanyIdForEdit = $scope.userRole.CompanyId;
        $scope.userRole.AddedDate = $filter('dateFilter')($scope.userRole.AddedDate);
        $scope.userRole.UpdatedDate = $filter('dateFilter')($scope.userRole.UpdatedDate);
        $scope.inactive = true;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.userRoleForm.$valid) {
            angular.copy($scope.userRole, $scope.userRoleNew);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.userRoleNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'userRole': $scope.userRoleNew
                        , 'companyId': $scope.oldCompanyIdForEdit
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    }

    $scope.valuePassInDelModal = function (data, index) {
        $scope.userRoleNew = data;
        $scope.index = index;
        $scope.message_confirmation = 'Are you sure want to delete this [ ' + data.UserId + ' ] user role..?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        $http({
            method: 'POST',
            url: $scope.deleteUrl,
            data: $scope.userRoleNew,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                if ($scope.index > -1) {
                    $scope.userRoles.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ShowResult(response.data.Message, 'success');
                }
                $scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(status.Message, 'failure');
        });
    };

    $scope.Clear = function () {
        $scope.userRole =
            {
                UserId: $scope.userRole.UserId
                , UserName: $scope.userRole.UserName
                , FullName: $scope.userRole.FullName
                , Active: true
            };
        $scope.userRoleNew = {};
        $scope.inactive = false;
        $scope.Action = 'Save';
        $scope.index = -1;
        $scope.oldCompanyIdForEdit = null;
    }
    $scope.ClearFields = function () {
        $scope.Action = 'Save';
        $scope.userRole = {};
        $scope.userRoleNew = {};
        $scope.userRole.Active = true;
        $scope.userRoles = [];
        $scope.inactive = false;
        $scope.oldCompanyIdForEdit = null;
        $scope.valueData = null;
        $scope.popUpDataList = null;
    }

    // #region Data send to urd
    $scope.send = function (data) {
        $window.userId = $scope.userRole.UserId;
        $window.fullName = $scope.userRole.FullName;
        $window.userName = $scope.userRole.UserName;
        dataShare.sendData(data);
        $window.location = $rootScope.bootPoint + 'user-role-detail';

    };
    // #endregion
}