'use strict';
MaterialAttributeValueController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MaterialAttributeValueController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Material Attribute Value";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.materialValues = [];
    $scope.path = 'Materials/materialattributevalue/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function () {
        $scope.GetDataList = function (pageno) {
            $rootScope.parameters.materialAttributeId = $scope.materialValueNew.MaterialAttributeId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.materialValues = [];
                    $scope.materialValues = result.Rows;
                    $scope.GetSequence();
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetDataList();
    };
    $scope.materialValue = {
        Id: null
        , CompanyGroupId: null
        , MaterialAttributeId: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , IsDefault: false
        , Active: true
    };
    $scope.materialValueNew = angular.copy($scope.materialValue);

    $scope.materialAttributeList = [];
    $http({
        method: 'GET',
        url: 'Materials/materialattribute/getcbo/',
        params: { 'valueAssignment': 'G' }
    }).then(function successCallback(response) {
        $scope.materialAttributeList = response.data;
    });

    $rootScope.searchmaterialAVByList = [
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
            'name': 'User Define Name',
            'value': 'UserName'
        }
    ];

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl + '?materialAttributeId=' + $scope.materialValueNew.MaterialAttributeId + '&materialId=')
            .then(function (response) {
                $scope.materialValueNew.Sequence = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.materialValue = $scope.materialValues[$scope.index];
        $scope.materialValueNew = angular.copy($scope.materialValue);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialValueNewForm.$valid) {
            angular.copy($scope.materialValueNew, $scope.materialValue);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.materialValue,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.materialValues.push(response.data.MaterialAttributeValue);
                        $scope.materialValues = $filter('orderBy')($scope.materialValues, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.materialValue,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.materialValues[$scope.index] = $scope.materialValue;
                            $scope.materialValues = $filter('orderBy')($scope.materialValues, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialValue.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialValue.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialValues.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.materialValue = {};
        $scope.materialValueNew = {
            MaterialAttributeId: $scope.materialValueNew.MaterialAttributeId
            , Sequence: seq, Active: true, IsDefault: false
        };
    }
};
