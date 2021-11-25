'use strict';
AdditionalRoleController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'dataShare'];
function AdditionalRoleController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, dataShare) {
    $rootScope.title = "Additional Role";
    $scope.Action = 'Save';
    $scope.inactive = false;
    $scope.index = -1;
    $scope.roleDetails = [];

    $scope.getData = function (data) {
        if (!baseService.isUndefinedOrNull(data))
            $scope.userRoleDetail.UserId = data.Id;

        if (baseService.isUndefinedOrNull($scope.userRoleDetail.UserId))
            ShowResult('Please select user', 'failure');

        if (baseService.isUndefinedOrNull($scope.oldCompanyIdForEdit)) {
            $scope.roleDetails = [];
            $http({
                method: 'GET',
                url: 'Securities/userroledetail/getmenuframelist',
                params: {
                    'userId': $scope.userRoleDetail.UserId,
                    'companyId': $scope.userRoleDetail.CompanyId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.roleDetails = response.data;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
    };

    $scope.userRoleDetailNew = {
        Id: null
        , UserId: null
        , UserName: null
        , FullName: null
        , CompanyId: null
        , CompanyName: null
        , RoleId: null
        , RoleName: null
        , UserAccessId: null
        , MenuMasterId: null
        , ModuleId: null
        , ModuleName: null
        , MenuFrameId: null
        , MenuFrameName: null
        , MenuActionId: null
        , Active: true
    };

    $scope.userRoleDetail = Object.assign({}, $scope.userRoleDetailNew);
    if ($window.UserRoleUserId !== null) {
        $scope.userRoleDetail.UserId = $window.UserRoleUserId;
        $scope.userRoleDetail.UserName = $window.userName;
        $scope.userRoleDetail.FullName = $window.UserRoleFullName;
        $scope.userRoleDetail.CompanyId = $window.UserRoleCompanyId;
        $scope.roleDetails = $window.RoleDetails;

        $window.UserRoleUserId = null;
        $window.userName = null;
        $window.UserRoleFullName = null;
        $window.UserRoleCompany = null;
        $scope.getData();
    }
    // #region DDL

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $http({
        method: 'GET',
        url: 'Securities/user/getuserlistwithoutsysadmin'
    }).then(function successCallback(response) {
        $scope.userList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Modules/companygroupmodule/getmodulebycompanygroupcbo'
    }).then(function successCallback(response) {
        $scope.moduleList = response.data;
    });

    // #endregion

    //***********************************User ********************************************************//
    $rootScope.searchByUserList = [
        {
            'name': 'UserId',
            'value': 'UserId'
        },
        {
            'name': 'Employee Id',
            'value': 'EmployeeId'
        },
        {
            'name': 'User Type',
            'value': 'UserType'
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
    }

    $scope.selectDoubleClick = function (data) {
        if (data.SysAdmin)
            //return ShowResult('This user can not allowed for role assigning', 'failure', 'popUpId');
            return ShowResult("User [" + data.UserId + "] is [" + data.UserType + "], so role assigning is not required.", 'failure', 'popUpId')
        $scope.userRoleDetail.UserId = data.Id;
        $scope.userRoleDetail.UserName = data.UserId;
        $scope.userRoleDetail.FullName = data.FullName;
        $scope.closePopUp();
    }

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

    //***********************************User ********************************************************//
    $scope.oldCompanyIdForEdit = null;
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.inactive = true;
        $scope.userRoleDetailNew = $scope.roleDetails[$scope.index];
        $scope.userRoleDetailNew.FullName = $scope.userRoleDetail.FullName;
        $scope.userRoleDetail = angular.copy($scope.userRoleDetailNew);
        $scope.oldCompanyIdForEdit = $scope.userRoleDetail.CompanyId;
        $scope.Action = 'Update';
    };
    $scope.getUserName = function () {
        // Set user id in window element.
        $window.UserRoleUserId = $scope.userRoleDetail.UserId;
        $http({
            method: 'GET',
            url: 'Securities/user/getfullname?id=' + $scope.userRoleDetail.UserId
        }).then(function successCallback(response) {
            $scope.userRoleDetail.FullName = response.data;
            $window.UserRoleFullName = response.data;
        });
    };

    $scope.menuFarmeGet = function () {
        $http({
            method: 'GET',
            url: 'Menus/menumaster/getmenuframebymoduleidcbo?moduleId=' + $scope.userRoleDetail.ModuleId
        }).then(function successCallback(response) {
            $scope.menuFrameList = response.data;
        });
    };

    $scope.Clear = function () {
        $scope.userRoleDetail =
            {
                UserId: $scope.userRoleDetailNew.UserId
                , UserName: $scope.userRoleDetailNew.UserName
                , FullName: $scope.userRoleDetailNew.FullName
                , CompanyId: $scope.userRoleDetailNew.CompanyId
            };
        $scope.userRoleDetailNew = {};
        $scope.inactive = false;
        $scope.oldCompanyIdForEdit = null;
        $scope.Action = 'Save';
        $scope.index = -1;
    };

    $scope.ClearFields = function () {
        $scope.userRoleDetail = {};
        $scope.userRoleDetailNew = {};
        $scope.roleDetails = [];
        $scope.inactive = false;
        $scope.oldCompanyIdForEdit = null;
        $scope.Action = 'Save';
    };

    $scope.Add = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.userRoleDetailForm.$valid) {
                if ($scope.Action == 'Save') {
                    if (baseService.isUndefinedOrNull($scope.userRoleDetail.UserId)) {
                        throw 'Please select user';
                    }
                    if (baseService.isUndefinedOrNull($scope.userRoleDetail.CompanyId)) {
                        throw 'Please select company';
                    }
                    if (baseService.isUndefinedOrNull($scope.userRoleDetail.ModuleId)) {
                        throw 'Please select module';
                    }
                    if (baseService.isUndefinedOrNull($scope.userRoleDetail.MenuFrameId)) {
                        throw 'Please select menuframe';
                    }
                    var isAvailable = false;
                    $scope.moduleName = document.getElementById("moduleId").options[document.getElementById('moduleId').selectedIndex].text;
                    $scope.menuframeName = document.getElementById("menuFrameId").options[document.getElementById('menuFrameId').selectedIndex].text;
                    for (var i = 0; i < $scope.roleDetails.length; i++) {
                        if ($scope.roleDetails[i].ModuleId == $scope.userRoleDetail.ModuleId) {
                            if ($scope.roleDetails[i].MenuFrameId == $scope.userRoleDetail.MenuFrameId) {
                                throw 'This combination has been already taken for this role.!';
                            }
                        }
                    }
                    this.userRoleDetail.ModuleName = $scope.moduleName;
                    this.userRoleDetail.MenuFrameName = $scope.menuframeName;
                    angular.copy($scope.userRoleDetail, $scope.userRoleDetailNew);
                    $scope.roleDetails.push($scope.userRoleDetailNew);
                    $scope.Clear();
                }
                else {
                    angular.copy($scope.userRoleDetail, $scope.userRoleDetailNew);
                    $http({
                        method: 'POST',
                        url: 'Securities/userroledetail/additionalroleupdate',
                        data: {
                            'userRoleDetail': $scope.userRoleDetailNew
                            , 'companyId': $scope.oldCompanyIdForEdit
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            if ($scope.index > -1) {
                                $scope.roleDetails[$scope.index] = $scope.userRoleDetailNew;
                                ShowResult(response.data.Message, 'success');
                            }
                            $scope.Clear();
                        }
                    }, function errorCallback(response) {
                        ShowResult(status.Message, 'failure');
                    });
                }
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    };

    $scope.valuePassInDelModal = function (data, index) {
        $scope.moduleId = data.ModuleId;
        $scope.menuFrameId = data.MenuFrameId;
        $scope.userId = data.UserId;
        $scope.companyId = data.CompanyId;
        $scope.index = index;
        $scope.message_confirmation = 'Are you sure want to delete this [ ' + data.UserId + ' ] user role.?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        $http({
            method: 'POST',
            url: 'Securities/userroledetail/additionalroledelete',
            data: {
                'moduleId': $scope.moduleId
                , 'menuFrameId': $scope.menuFrameId
                , 'userId': $scope.userId
                , 'companyId': $scope.companyId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                if ($scope.index > -1) {
                    $scope.roleDetails.splice($scope.index, 1);
                    ShowResult(response.data.Message, 'success');
                }
                $scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(status.Message, 'failure');
        });
    };
    $scope.send = function (data) {
        data.UserId = $scope.userRoleDetail.UserId;
        data.FullName = $scope.userRoleDetail.FullName;
        data.UserName = $scope.userRoleDetail.UserName;
        $window.RoleDetails = $scope.roleDetails;

        data.CompanyName = document.getElementById("companyId").options[document.getElementById('companyId').selectedIndex].text;
        dataShare.sendData(data);
    };
}