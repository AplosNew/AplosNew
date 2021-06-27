'use strict';
PartyAccountGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function PartyAccountGroupController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Party Account Group';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.partyAccountGroups = [];
    $scope.path = 'Parties/partyaccountgroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno, accountType) {
        $rootScope.parameters.AccountType = $scope.partyAccountGroup.AccountType;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.partyAccountGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.partyAccountGroup = {
        Id: null,
        PartnerDeterminationProcedureId: null,
        AccountType: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null
        , IsInterCompany: false
    };

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.partyAccountGroup.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.partyAccountGroupTypeList = [];
    $http({
        method: 'GET',
        url: 'Enum/getpartyaccountgrouptypelistcbo'
    }).then(function (response) {
        $scope.partyAccountGroupTypeList = response.data;
    });

    $scope.partnerDetarminationProcedureList = [];
    $http.get('Parties/partnerdeterminationprocedure/getcbo/')
        .then(function (response) {
            $scope.partnerDetarminationProcedureList = response.data;
        });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getpartyAccountGroup = angular.copy($scope.partyAccountGroups[$scope.index]);
        $scope.partyAccountGroup = $scope.getpartyAccountGroup;
        $scope.partyAccountGroup.AddedDate = $filter('dateFilter')($scope.partyAccountGroup.AddedDate);
        $scope.partyAccountGroup.UpdatedDate = $filter('dateFilter')($scope.partyAccountGroup.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.partyAccountGroupForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.partyAccountGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.partyAccountGroups.push(response.data.PartyAccountGroup);
                        baseService.paginationAdd();
                        $scope.partyAccountGroups = $filter('orderBy')($scope.partyAccountGroups, 'Sequence');
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
                    data: $scope.partyAccountGroup,
                    dataType: 'JSON'
                }).then(function successCallBack(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.partyAccountGroups[$scope.index] = $scope.partyAccountGroup;
                            $scope.partyAccountGroups = $filter('orderBy')($scope.partyAccountGroups, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.partyAccountGroup.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.partyAccountGroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.partyAccountGroups.splice($scope.index, 1);
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
        $scope.AccountType = $scope.partyAccountGroup.AccountType;
        $scope.partyAccountGroup = {};
        $scope.partyAccountGroup.AccountType = $scope.AccountType;
        $scope.partyAccountGroup.Sequence = seq;
        $scope.partyAccountGroup.Active = true;
    }
}