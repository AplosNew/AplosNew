'use strict';
PettyCashMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PettyCashMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Petty Cash';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/PettyCashMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "StandardRate"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'Category', name: "Category" }, { value: 'SubCategory', name: "Sub Category" }, { value: 'Item', name: "Item" }, { value: 'StandardRate', name: "Standard Rate" }, { value: 'StandardRateBookCurrency', name: "Standard Rate Book Currency" }, { value: 'Remarks', name: "Remarks" }];


    $scope.ActionGroup = 'Save';
    $scope.ModelListGroup = [];
    $scope.getListUrlGroup = $scope.path + 'getlistGroup';
    $scope.getSeqUrlGroup = $scope.path + 'getautosequenceGroup';
    $scope.saveUrlGroup = $scope.path + 'createGroup';
    $scope.deleteUrlGroup = $scope.path + 'deleteGroup/';
    $scope.searchByGroup = "UserName"; $scope.searchgroup = "";
    $scope.searchByListGroup = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.getDataGroup = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetListGroup",
            data: { column: $scope.searchByGroup, value: $scope.searchgroup },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListGroup = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequenceGroup();
        });
    }
    $scope.getDataGroup();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        Category: null,
        SubCategory: null,
        Item: null,
        StandardRate: null,
        StandardRateBookCurrency: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelTempGroup = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        UserName: null,
        Description: null,
        StandardName: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewGroup = Object.assign({}, $scope.ModelTempGroup);

    $scope.GetSequence = function () {
        $http({
            method: 'GET',
            url: 'Accounts/PettyCashMaster/GetAutoSequence/'
        }).then(function successCallback(response) {
            $scope.ModelTemp.Sequence = response.data;
            $scope.ModelNew.Sequence = response.data;
        });
    };
    $scope.GetSequence();

    
    $scope.GetSequenceGroup = function () {
        $http({
            method: 'GET',
            url: 'Accounts/PettyCashMaster/GetAutoSequenceGroup/'
        }).then(function successCallback(response) {
            $scope.ModelNewGroup.Sequence = response.data;
        });
    };
    $scope.GetSequenceGroup();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetGroup = function (args) {

        $scope.ModelNewGroup = Object.assign({}, args.data);
        $scope.ActionGroup = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

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
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.SaveGroup = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.PettyGroupForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlGroup,
                data: { 'data': $scope.ModelNewGroup },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    $scope.ClearGroup = function () {
        ClearFieldsGroup($scope.GetSequenceGroup());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    function ClearFieldsGroup(seq) {
        $scope.ActionGroup = 'Save';
        $scope.ModelNewGroup = Object.assign({}, $scope.ModelTempGroup);
        $scope.ModelNewGroup.SequenceGroup = seq;
    }
}