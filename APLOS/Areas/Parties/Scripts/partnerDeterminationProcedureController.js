'use strict';
function PartnerDeterminationProcedureController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'PartnerDeterminationProcedure';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.partnerDeterminationProcedures = [];
    $scope.path = 'Parties/partnerdeterminationprocedure/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.partnerDeterminationProcedures = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.partnerDeterminationProcedure = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        IsMendatory: true,
        IsModificationAllow: true,
        IsDefaultValue: true,
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
                $scope.partnerDeterminationProcedure.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.getpartnerDeterminationProcedure = angular.copy($scope.partnerDeterminationProcedures[$scope.index]);
        $scope.partnerDeterminationProcedure = $scope.getpartnerDeterminationProcedure;
        $scope.partnerDeterminationProcedure.AddedDate = $filter('dateFilter')($scope.partnerDeterminationProcedure.AddedDate);
        $scope.partnerDeterminationProcedure.UpdatedDate = $filter('dateFilter')($scope.partnerDeterminationProcedure.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.partnerDeterminationProcedureForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.partnerDeterminationProcedure,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.partnerDeterminationProcedures.push(response.data.PartnerDeterminationProcedure);
                        baseService.paginationAdd();
                        $scope.partnerDeterminationProcedures = $filter('orderBy')($scope.partnerDeterminationProcedures, 'Sequence');
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
                    data: $scope.partnerDeterminationProcedure,
                    dataType: 'JSON'
                }).then(function successCallBack(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.partnerDeterminationProcedures[$scope.index] = $scope.partnerDeterminationProcedure;
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
        if (!baseService.isUndefinedOrNull($scope.partnerDeterminationProcedure.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.partnerDeterminationProcedure.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.partnerDeterminationProcedures.splice($scope.index, 1);
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
        $scope.partnerDeterminationProcedure = {};
        $scope.partnerDeterminationProcedure.Sequence = seq;
        $scope.partnerDeterminationProcedure.Active = true;
    }
}
PartnerDeterminationProcedureController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];