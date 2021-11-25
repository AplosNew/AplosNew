
'use strict';
userEntityController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http','$window'];
function userEntityController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $window) {
    $rootScope.title = "User|Entity";
    $scope.path = 'Securities/UserEntity/';
    $scope.plantList = [];
    $scope.model = {
        Id: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
        , UserId: null
        , UserName: null
        , FullName: null
    };

    // #region DDL
    //function getCompanyCbo() {
    //    $scope.entityList = [];
    //    $scope.model.PlantId = null;
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'GetPlantCboByUser?userId=' + $scope.model.UserId
    //    }).then(function successCallback(response) {
    //        $scope.plantList = response.data;
    //    });
    //}

    $scope.complanyList = [];
    cboService.getCompanyGroupCompanyCbo($window.companyGroupId, function (result) {
        $scope.complanyList = result;
    });

    $scope.plantList = [];
    $scope.getPlantByCompany = function () {
        if (baseService.isUndefinedOrNull($scope.model.UserId)) {
            ShowResult("Select User.", 'failure');
            return false;
        }
        cboService.getCboPlantByCompany($scope.model.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    // #endregion

    $scope.getData = function () {
        $scope.entityList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getlist?userId=' + $scope.model.UserId + '&plantId=' + $scope.model.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };

    $scope.Save = function () {
        if (baseService.isUndefinedOrNull($scope.model.UserId)) {
            ShowResult("Select User.", 'failure');
                return false;
        }
        for (var i = 0; i < $scope.entityList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.entityList[i].UserId)) {
                $scope.entityList[i].UserId = $scope.model.UserId;
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + 'create',
            data: $scope.entityList,
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
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'UserId'
        , searchBy: "UserId"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
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
        if (data.SysAdmin) //return ShowResult('This user can not allowed for plant assigning', 'failure', 'popUpId');
            return ShowResult("User [" + data.UserId + "] is [" + data.UserType + "], so entity assigning is not required.", 'failure', 'popUpId')
        $scope.model.UserId = data.Id;
        $scope.model.UserName = data.UserId;
        $scope.model.FullName = data.FullName;
        //getCompanyCbo();
        $scope.closePopUp();
        cboService.getCboPlantByCompany($scope.model.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.selectSingleClick = function (data) {
        $scope.rowSelected = data.UserId;
        $scope.valueData = data;
    };

    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    // #endregion User
}