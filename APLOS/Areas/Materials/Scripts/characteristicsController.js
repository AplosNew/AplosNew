'use strict';
CharacteristicsController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function CharacteristicsController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Characteristics";
    $scope.Action = 'Save';
    $scope.characterlist = [];
    $scope.lengthCheck = false;
    $scope.index = -1;
    $scope.path = 'Materials/characteristics/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.characterlist = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchCharacteristicsByList = [
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
    $scope.characteristics = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        ValueAssignmentLevel: null,
        Remarks: null,
        Description: null,
        AttributeProperty: true,
        IsFixedNoOfCharacter: false,
        NoOfCharacter: 1,
        IsFreeField: true,
        IsPreDefinedField: true,
        IsMandatory: true,
        Active: true
    };
    $scope.characteristicsNew = Object.assign({}, $scope.characteristics);
    function GetSequence() {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.characteristicsNew.Sequence = response.data;
            });
    }
    GetSequence();
    cboService.getEnumCbo("enum/GetAttributePropertiesCbo", function (result) {
        $scope.attributePropertyList = result;
        $scope.characteristicsNew.AttributeProperty = $scope.attributePropertyList[0].Value;
    });
    cboService.getEnumCbo("enum/GetValueAssignmentCbo", function (result) {
        $scope.valueAssignmentList = result;
        $scope.characteristicsNew.ValueAssignmentId = $scope.valueAssignmentList[0].Value;
    });
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.characteristics = $scope.characterlist[$scope.index];
        $scope.characteristicsNew = Object.assign({}, $scope.characteristics);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            //var ch = parse($scope.characteristicsNew.NoOfCharacter);
            if ($scope.characteristicsNew.NoOfCharacter > 10)
                throw 'No of character can not be grater than 10...!';
            if ($scope.characteristicsNew.IsFreeField == false && $scope.characteristicsNew.IsPreDefinedField == false)
                throw 'Please select free field or pre-defined field or both';
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.characteristicsNewForm.$valid) {
                angular.copy($scope.characteristicsNew, $scope.characteristics);
                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.characteristics,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.characterlist.push(response.data.Characteristics);
                            $scope.characterlist = $filter('orderBy')($scope.characterlist, 'StandardName');
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.characteristics,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.getData();
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.characterlist[$scope.index] = $scope.characteristics;
                                $scope.characterlist = $filter('orderBy')($scope.characterlist, 'StandardName');
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.characteristicsNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.characteristicsNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.characterlist.splice($scope.index, 1);
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
        $scope.Characteristics = {};
        $scope.characteristicsNew = {
            Sequence: seq
            , NoOfCharacter: 1
            , IsFixedNoOfCharacter: false
            , IsFreeField: true
            , IsPreDefinedField: true
            , IsMandatory: true
            , Active: true
        };
    }
};
