'use strict';
UserEditControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UserEditControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'User Edit Control';
    $scope.ModelList = [];
    $scope.path = 'TaskManagement/TaskAppliedOn/';
    $scope.saveUrl = $scope.path + 'CreateUserEditControl';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.Action = 'Save';

    $scope.ModelTemp = {
        Id: null,
        UserId: null,
        UserName: null,
        FullName: null,
        HrefId:null,
        Href: null,
        Password: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

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
            return ShowResult("User [" + data.UserId + "] is [" + data.UserType + "], so role is not required.", 'failure', 'popUpId')
        $scope.ModelNew.UserId = data.Id;
        $scope.ModelNew.UserName = data.UserId;
        $scope.ModelNew.FullName = data.FullName;
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
    
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };


    $scope.getData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetUserEditControlList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
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
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        return true;
    };

     //***********************************Href ********************************************************//
    $scope.hrefvalueData = '';
    $scope.popUpHrefParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'MenuMasterId',
        searchBy: "MenuMasterId",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUpHref = function () {
        $scope.popUpHrefDataList = [];
        $scope.popUpUrl = 'TaskManagement/TaskAppliedOn/GetHreflist';
        $scope.getPopUpHrefData = function (data) {
            baseService.paginationBase($scope.popUpUrl, data ,$scope.popUpHrefParameters)
                .then(function (result) {
                    $scope.popUpHrefDataList = result.Rows;
                    $scope.popUpHrefParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUphrefId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUphrefId')).modal('show');
        $scope.getPopUpHrefData();
    };

    $scope.selectHrefDoubleClick = function (data) {
        //if (data.SysAdmin)
        //    return ShowResult("User [" + data.UserId + "] is [" + data.UserType + "], so role is not required.", 'failure', 'popUpId')
        $scope.ModelNew.HrefId = data.MenuMasterId;
        $scope.ModelNew.Href = data.Href;
        $scope.getData();
        $scope.closeHrefPopUp();
    };
    $scope.selectHrefSingleClick = function (data) {
        $scope.hrefrowSelected = data.MenuMasterId;
        $scope.hrefvalueData = data;
        $scope.ModelNew.HrefId = data.MenuMasterId;
        $scope.ModelNew.Href = data.Href;
    };

    $scope.selectByButtonHref = function () {
        if (baseService.isUndefinedOrNull($scope.hrefvalueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUphrefId');
        }
        $scope.selectHrefDoubleClick($scope.hrefvalueData)
        $scope.closeHrefPopUp();
    };
    $scope.closeHrefPopUp = function () {
        $scope.hrefvalueData = '';
        angular.element(document.querySelector('#popUphrefId')).modal('hide');
    };
     //***********************************Href ********************************************************//
}