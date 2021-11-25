'use strict';
userAccessPlantController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http','cboService'];
function userAccessPlantController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, cboService) {
    $rootScope.title = "User Access Plant";
    $scope.path = 'Securities/UserAccessPlant/';
    $scope.plantList = [];
    $scope.userRole = {
        Id: null
        , CompanyGroupId: null
        , CompanyId: null
        , UserId: null
        , UserName: null
        , FullName: null
    }

    // #region DDL
    //function getCompanyCbo() {
    //    $scope.userRole.CompanyId = null;
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'GetCompanyCboByUser?userId=' + $scope.userRole.UserId
    //    }).then(function successCallback(response) {
    //        $scope.companyList = response.data;
    //        $scope.userRole;
    //    });
    //}

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    // #endregion

    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getlist?companyId=' + $scope.userRole.CompanyId + '&userId=' + $scope.userRole.UserId
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
        });
    };

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'create',
            data: $scope.plantList,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.getData();
                ShowResult(response.data.Message, 'success');
            }
        });
        return true;
    };

    // #region User
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
        if (data.SysAdmin) //return ShowResult('This user can not allowed for plant assigning', 'failure', 'popUpId');
            return ShowResult("User [" + data.UserId + "] is [" + data.UserType + "], so plant assigning is not required.", 'failure', 'popUpId')
        $scope.userRole.UserId = data.Id;
        $scope.userRole.UserName = data.UserId;
        $scope.userRole.FullName = data.FullName;
        //getCompanyCbo();
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
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }
    // #endregion User
}