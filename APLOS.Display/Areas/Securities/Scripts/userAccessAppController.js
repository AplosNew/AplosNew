'use strict';
UserAccessAppController.$inject = ['cboService', 'baseService', '$rootScope', '$scope', '$routeParams', '$location', '$http', '$filter'];
function UserAccessAppController(cboService, baseService, $rootScope, $scope, $routeParams, $location, $http, $filter) {
    $rootScope.title = "User AccessApp";
    $scope.tableShow = false;

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $http({
        method: 'GET',
        url: 'Securities/user/getlist'
    }).then(function successCallback(response) {
        $scope.userList = response.data;
    });

    $scope.userAccessApp = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        UserId: null,
        UserName: null,
        UserFullname: null,
        ModuleAppId: null,
        Active: null,
        Archive: null,
        AddedBy: null,
        AddedDate: null,
        UpdatedDate: null
    };

    $scope.search = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.userAccessAppForm.$valid) {
            $http.get('Securities/UserAccessApp/GetList?companyId=' + $scope.userAccessApp.CompanyId +
                '&&userId=' + $scope.userAccessApp.UserId)
                .then(function (response) {
                    $scope.userAccessApps = response.data;
                    $scope.tableShow = true;
                });
        }
    }
    function GetAppAccessWithCompany() {
        try {
            $scope.appAccessList = [];
            $http.get('Securities/UserAccessApp/GetListWithCompany?companyId=' + $scope.userAccessApp.CompanyId +
                '&&userId=' + $scope.userAccessApp.UserId)
                .then(function (response) {
                    $scope.appAccessList = response.data;
                });
        } catch (e) {
            throw e;
        }
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.userAccessAppsSaveList = [];
    $scope.Save = function () {
        $scope.userAccessAppsSaveList = [];
        angular.forEach($scope.userAccessApps, function (item) {
            $scope.userAccessAppsSaveList.push(
                {
                    CompanyId: $scope.userAccessApp.CompanyId,
                    UserId: $scope.userAccessApp.UserId,
                    ModuleAppId: item.ModuleAppId,
                    ModuleAppName: item.ModuleAppName,
                    Active: item.Active
                }
            );
        });
        $http({
            method: 'POST',
            url: 'Securities/useraccessapp/Create',
            data: $scope.userAccessAppsSaveList,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.userAccessAppsSaveList = [];
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    }

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
    }

    $scope.selectDoubleClick = function (data) {
        $scope.userAccessApp.UserId = data.Id;
        $scope.userAccessApp.UserName = data.UserId;
        $scope.userAccessApp.UserFullname = data.FullName;
        GetAppAccessWithCompany();
        $scope.closePopUp();
    }

    $scope.selectSingleClick = function (data) {
        $scope.rowSelected = data.UserId;
        $scope.valueData = data;
    }
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData)
        $scope.closePopUp();
    }
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }
}