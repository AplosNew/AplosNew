'use strict';
accountGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function accountGroupController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Account Group';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.accountgroups = [];
    $scope.getListUrl = 'accounts/accountgroup/getaccountgrouplist/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Code');
    $scope.getData = function (pageno) {
        $rootScope.parameters.COAId = $scope.accountgroup.COAId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.accountgroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $rootScope.searchaccountgroupByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'From Number Range',
            'value': 'FromNumberRange'
        }
    ];

    $scope.accountgroup = {
        Id: null,
        CompanyGroupId: null,
        COAId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        AccountTypeId: null,
        FromNumberRange: null,
        ToNumberRange: null,
        GLType: null,
        BalanceType: null,
        Description: null,
        Active: true
    };

    $scope.accountgrouptypeList = [];
    $http({
        method: 'GET',
        url: 'Enum/getaccountgrouptypelistcbo/'
    }).then(function successCallback(response) {
        $scope.accountgrouptypeList = response.data;
    });

    $scope.coaaccounttypeList = [];
    $http({
        method: 'GET',
        url: 'accounts/accounttype/GetCbo/'
    }).then(function successCallback(response) {
        $scope.coaaccounttypeList = response.data;
    });

    $scope.accountgroupbalancetypeList = [];
    $http({
        method: 'GET',
        url: 'Enum/getaccountgroupbalancetypelistcbo/'
    }).then(function successCallback(response) {
        $scope.accountgroupbalancetypeList = response.data;
    });

    $scope.cOAList = [];
    $http({
        method: 'GET',
        url: 'accounts/coa/getcoacbo/'
    }).then(function successCallback(response) {
        $scope.cOAList = response.data;
    });

    $scope.companyGroupList = [];
    $http({
        method: 'GET',
        url: 'Organizations/companygroup/getcbo'
    }).then(function successCallback(response) {
        $scope.companyGroupList = response.data;
    });

    $scope.checkBigErrorc = false;
    $scope.checkBig = function () {
        if ($scope.accountgroup.ToNumberRange > $scope.accountgroup.FromNumberRange) {
            $scope.checkBigErrorc = false;
            return true;
        } else {
            $scope.checkBigErrorc = true;
            if ($scope.accountgroup.ToNumberRange !== null) {
                $scope.checkBigError = 'From Number Must be bigger';
            } else {
                $scope.checkBigError = '';
            }
            return false;
        }
    };

    $scope.getLength = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/coa/GetGLLengthCbo?id=' + id
        }).then(function successCallback(response) {
            $scope.maxLength = response.data[0]['LengthOfGL'];
        });
    };

    $scope.getUserName = function (item) {
        if (item === 'Asset') {
            $scope.accountgroup.GLType = 'Balance Sheet';
            $scope.accountgroup.BalanceType = 'Debit';
        }
        else if (item === 'Liability') {
            $scope.accountgroup.GLType = 'Balance Sheet';
            $scope.accountgroup.BalanceType = 'Credit';
        }
        else if (item === 'Equity') {
            $scope.accountgroup.GLType = 'Balance Sheet';
            $scope.accountgroup.BalanceType = 'Credit';
        }
        else if (item === 'Income') {
            $scope.accountgroup.GLType = 'Income Sheet';
            $scope.accountgroup.BalanceType = 'Credit';
        }
        else if (item === 'Expense') {
            $scope.accountgroup.GLType = 'Income Sheet';
            $scope.accountgroup.BalanceType = 'Debit';
        }
    };

    $scope.onCOAChangeSequence = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/accountgroup/getautosequence?coaid=' + item
        }).then(function successCallback(response) {
            $scope.accountgroup.Sequence = response.data;
            ClearFields();
        });
    };

    $scope.maxLengthCheck = function (object) {
        if (object.value.length > object.maxLength)
            object.value = object.value.slice(0, object.maxLength);
    };

    $scope.isNumeric = function (evt) {
        var theEvent = evt || window.event;
        var key = theEvent.keyCode || theEvent.which;
        key = String.fromCharCode(key);
        var regex = /[0-9]|\./;
        if (!regex.test(key)) {
            theEvent.returnValue = false;
            if (theEvent.preventDefault) theEvent.preventDefault();
        }
    };

    $scope.Get = function (id, index) {
        $scope.disableField = true;
        $scope.index = index;
        $scope.accountgroup = $scope.accountgroups[$scope.index];
        $scope.accountgroup.AddedDate = $filter('dateFilter')($scope.accountgroup.AddedDate);
        $scope.accountgroup.UpdatedDate = $filter('dateFilter')($scope.accountgroup.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.accountgroupForm.$valid & $scope.checkBig()) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/accountgroup/create',
                    data: $scope.accountgroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.accountgroups.push(response.data.AccountGroup);
                        baseService.paginationAdd();
                        $scope.accountgroups = $filter('orderBy')($scope.accountgroups, 'Sequence');
                        ClearFields(response.data.Sequence);
                    }
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/accountgroup/edit',
                    data: $scope.accountgroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.accountgroups[$scope.index] = $scope.accountgroup;
                            $scope.accountgroups = $filter('orderBy')($scope.accountgroups, 'Sequence');
                        }
                        ClearFields(data.Sequence);
                    }
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.accountgroup.Id)) {
            $http({
                method: 'POST',
                url: 'accounts/accountgroup/delete/' + $scope.accountgroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.accountgroups.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.accountgroup = { COAId: $scope.accountgroup.COAId };
        $scope.accountgroup.Active = true;
    }
}