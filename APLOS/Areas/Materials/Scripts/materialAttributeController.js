'use strict';
MaterialAttributeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function MaterialAttributeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Material Attribute";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.materialAttributes = [];
    $scope.path = 'Materials/materialattribute/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.materialAttributes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.materialAttribute = {
        Id: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Remarks: null
        , Description: null
        , ValueAssignmentLevel: null
        , AttributeProperty: null
        , NoOfCharacter: 0
        , IsFixedNoOfCharacter: false
        , IsFreeField: true
        , IsPreDefinedField: true
        , IsMandatory: true
        , Active: true
    };
    $scope.materialAttributeNew = Object.assign({}, $scope.materialAttribute);

    $scope.attributePropertyList = [];
    cboService.getEnumCbo("enum/GetAttributePropertiesCbo", function (result) {
        $scope.attributePropertyList = result;
        $scope.materialAttributeNew.AttributeProperty = $scope.attributePropertyList[0].Value;
    });
    cboService.getEnumCbo("enum/GetValueAssignmentCbo", function (result) {
        $scope.valueAssignmentList = result;
        $scope.materialAttributeNew.ValueAssignmentId = $scope.valueAssignmentList[0].Value;
    });

    function GetSequence() {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.materialAttributeNew.Sequence = response.data;
            });
    }
    GetSequence();
    $rootScope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StandardName',
            'value': 'StandardName'
        },
        {
            'name': 'User Define Name',
            'value': 'UserName'
        },
        {
            'name': 'Value Assignment Level',
            'value': 'ValueAssignmentLevel'
        },
        {
            'name': 'Attribute Property',
            'value': 'AttributeProperty'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.materialAttribute = $scope.materialAttributes[$scope.index];
        $scope.materialAttributeNew = Object.assign({}, $scope.materialAttribute);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            angular.copy($scope.materialAttributeNew, $scope.materialAttribute);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.materialAttribute,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.materialAttributes.push(response.data.MaterialAttribute);
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
                    data: $scope.materialAttribute,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.materialAttributes[$scope.index] = $scope.materialAttribute;
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
        if (!baseService.isUndefinedOrNull($scope.materialAttributeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialAttributeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialAttributes.splice($scope.index, 1);
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
        ClearFields(GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.materialAttribute = {};
        $scope.materialAttributeNew = {
            Sequence: seq
            , NoOfCharacter: 0
            , IsFixedNoOfCharacter: false
            , IsFreeField: true
            , IsPreDefinedField: true
            , IsMandatory: true
            , Active: true
        };
    }
}
