'use strict';
fixedAssetAttributeValueController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function fixedAssetAttributeValueController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'FixedAsset Attribute Value';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'fixedassets/fixedassetattributevalue/';
    $scope.getListUrl = $scope.path + 'GetList';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.process = {
        Id: null,
        CompanyGroupId: null,
        FixedAssetAttributeId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
        Description: null,
        IsDefault: false,
        Active: true
    };

    $scope.processNew = Object.assign({}, $scope.process);

    $scope.getData = function (pageno) {
        $rootScope.parameters.fixedAssetAttributeId = $scope.processNew.FixedAssetAttributeId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.processes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.fixedAssetAttributeList = [];
    $http.get('fixedassets/fixedassetattribute/getcbo')
        .then(function successCallback(response) {
            $scope.fixedAssetAttributeList = response.data;
        }, function errorCallback(response) {
            ShowResult(response, 'failure');
        });
    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.processNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.processes[$scope.index], $scope.process)
        $scope.processNew = Object.assign({}, $scope.process);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            angular.copy($scope.processNew, $scope.process);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.process,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.process = response.data.FixedAssetAttributeValue;
                        $scope.processes.push($scope.process);
                        $scope.processes = $filter('orderBy')($scope.processes, 'Sequence');
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.process,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            angular.copy($scope.process, $scope.processes[$scope.index]);
                            $scope.processes = $filter('orderBy')($scope.processes, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.processNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.processNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.processes.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.process = {};
        $scope.processNew = { FixedAssetAttributeId: $scope.processNew.FixedAssetAttributeId };
        $scope.processNew.Sequence = seq;
        $scope.processNew.Active = true;
        $scope.processNew.IsDefault = false;
    }
}