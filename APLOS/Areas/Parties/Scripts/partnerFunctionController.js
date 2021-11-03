'use strict';
function PartnerFunctionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Partner Function';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.partnerFunctions = [];
    $scope.path = 'Parties/partnerfunction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $rootScope.parameters.AccountType = $scope.partnerFunction.AccountType;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.partnerFunctions = result.Rows;
                console.log($scope.partnerFunctions);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        $scope.changeOnAccountType($scope.partnerFunction.AccountType);
    };

    $scope.partnerFunction = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        AccountType: null,
        AssignmentType: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null
    };

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.partnerFunction.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.partyAccountGroupTypeList = [];
    $http({
        method: 'GET', url: 'Enum/getpartyaccountgrouptypelistcbo'
    }).then(function (response) {
        $scope.partyAccountGroupTypeList = response.data;
    });

    $scope.PartnerFunctionList = [];

    $scope.changeOnAccountType = function (type) {
        if (type == 'Vendor') {
            $http({
                method: 'GET',
                url: 'Enum/getvendorpartnerfunctionlistcbo'
            }).then(function (response) {
                $scope.PartnerFunctionList = response.data;
            });
        } else {
            $http({
                method: 'GET',
                url: 'Enum/getcustomerpartnerfunctionlistcbo'
            }).then(function (response) {
                $scope.PartnerFunctionList = response.data;
            });
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getpartnerFunction = angular.copy($scope.partnerFunctions[$scope.index]);
        $scope.partnerFunction = $scope.getpartnerFunction;
        $scope.partnerFunction.AddedDate = $filter('dateFilter')($scope.partnerFunction.AddedDate);
        $scope.partnerFunction.UpdatedDate = $filter('dateFilter')($scope.partnerFunction.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.partnerFunctionForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.partnerFunction,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.partnerFunctions.push(response.data.PartnerFunction);
                        baseService.paginationAdd();
                        $scope.partnerFunctions = $filter('orderBy')($scope.partnerFunctions, 'Sequence');
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.partnerFunction,
                    dataType: 'JSON'
                }).then(function successCallBack(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.partnerFunctions[$scope.index] = $scope.partnerFunction;
                            $scope.partnerFunctions = $filter('orderBy')($scope.partnerFunctions, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.partnerFunction.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.partnerFunction.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.partnerFunctions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.AccountType = $scope.partnerFunction.AccountType;
        $scope.partnerFunction = {};
        $scope.partnerFunction.AccountType = $scope.AccountType;
        $scope.partnerFunction.Sequence = seq;
        $scope.partnerFunction.Active = true;
    }
}
PartnerFunctionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];