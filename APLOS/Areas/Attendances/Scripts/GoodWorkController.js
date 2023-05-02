'use strict';
GoodWorkController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GoodWorkController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Good Work';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateUserEditControl';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    //$scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteChildUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.Action = 'Save';
    $scope.passwordShow = true;

    $scope.ModelTemp = {
        Id: null,
        UserId: null,
        UserName: null,
        FullName: null,
        Password: null,
        RePassword: null
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
        $scope.getHrefList();
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
    $scope.removeRow = function (data) {
        /* $scope.HrefDataList.splice(index, 1);*/

        $http({
            method: 'GET',
            url: 'TaskManagement/TaskAppliedOn/DeleteChildUrl?Id=' + data.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getHrefList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.HrefDataList = [];
    $scope.getHrefList = function () {
        $http({
            method: "get",
            url: "TaskManagement/TaskAppliedOn/GetHrefDatasList?hrefId=" + $scope.ModelNew.UserId
        }).then(function successCallback(response) {
            $scope.HrefDataList = response.data;
        });
    };

    //***********************************Href ********************************************************//
    $scope.hrefvalueData = '';
    $scope.popUpHrefParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Id',
        searchBy: "Id",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUpHref = function () {
        $scope.popUpHrefDataList = [];
        $scope.popUpUrl = 'TaskManagement/TaskAppliedOn/GetHreflist';
        $scope.getPopUpHrefData = function (data) {
            baseService.paginationBase($scope.popUpUrl, data, $scope.popUpHrefParameters)
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


    $scope.setSelected = function (data) {
        $scope.selectHrefDoubleClick(data);
    };

    $scope.selectHrefDoubleClick = function (a) {

        var obj = {};
        obj.Id = null;
        obj.HrefId = a.Id;
        obj.Href = a.Href;
        obj.Controller = a.Controller;
        obj.Description = a.Description;

        $scope.HrefDataList.push(obj);
        obj = {};

        $scope.closeHrefPopUp();
    };

    function checkProcessExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].HrefId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.selectHrefSingleClick = function (data) {
        $scope.hrefrowSelected = data.Id;
        $scope.ModelNew.Href = data.Href;
        $scope.hrefvalueData = data;
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


    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
       
            if ($scope.ModelNewForm.$valid) {
                if ($scope.ModelNew.Password == $scope.ModelNew.RePassword) {

                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'data': $scope.ModelNew
                            , 'userECDetail': $scope.HrefDataList
                        },
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
                else {
                    ShowResult('Password and Confirm Password does not match!', 'failure');
                }
            }

        } catch (e) {
            ShowResult(e, 'failure');
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

    $scope.ModelDetailList = [];
    $scope.GetUserEditControlDetailData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetUserEditControlDetailList?userEditControlId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelDetailList = response.data;
        });
    }
    //$scope.getUserEditControlDetailData();

    $scope.GetDblClick = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.RePassword = args.data.Password;
        $scope.getData();
        $scope.getHrefList();
        //$scope.GetUserEditControlDetailData($scope.ModelNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: 'TaskManagement/TaskAppliedOn/Delete',
                data: {'Id': $scope.ModelNew.Id},
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
        $scope.HrefDataList = [];
        //$scope.getHrefList = [];
        return true;
    };


}