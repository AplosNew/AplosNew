'use strict';
CharacteristicsValueController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function CharacteristicsValueController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Characteristics Value";
    $scope.Action = 'Save';
    $scope.characteristicsValueList = [];
    $scope.index = -1;
    $scope.path = 'Materials/characteristicsvalue/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.onChangeGetData = function () {
        baseService.init($scope.getListUrl, null, null, null, null, 'Code');
        $scope.getData = function (pageno) {
            $rootScope.parameters.characteristicsId = $scope.characteristicsvalueNew.CharacteristicsId;
            $rootScope.parameters.ids = '';
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.characteristicsValueList = result.Rows;
                    $scope.GetSequence();
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
	
    $scope.searchCharacteristicsValueByList = [
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
        }
    ];
    $http({
        method: 'GET',
        url: 'Materials/characteristics/getcbo/',
        params: { 'valueAssignment': 'G' }
    }).then(function successCallback(response) {
        $scope.characterList = response.data;
    });

    //onChange Start
    $scope.characChange = function (id) {
        $http({
            method: 'GET',
            url: 'Materials/characteristics/getforcharacteristicsvalue?characteristicsId=' + id
        }).then(function successCallback(response) {
            $scope.codeValidationData = response.data;
        });
    };
    //onChange End
    $scope.characterList = [];
    $scope.characteristicsvalue = {
        Id: null,
        CharacteristicsId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        IsDefault: false,
        Active: true
    };
    $scope.characteristicsvalueNew = Object.assign({}, $scope.characteristicsvalue);
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl + '?characteristicsId=' + $scope.characteristicsvalueNew.CharacteristicsId + '&materialId=')
            .then(function (response) {
                $scope.characteristicsvalueNew.Sequence = response.data;
            });
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.characteristicsvalue = $scope.characteristicsValueList[$scope.index];
        $scope.characteristicsvalueNew = Object.assign({}, $scope.characteristicsvalue);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            //if ($scope.characteristicsvalueNew.Code !== null) {
            //    if ($scope.codeValidationData.IsFixedNoOfCharacter) {
            //        if ($scope.characteristicsvalueNew.Code.length > $scope.codeValidationData.NoOfCharacter
            //            && $scope.characteristicsvalueNew.Code.length < $scope.codeValidationData.NoOfCharacter)
            //            throw 'Code must be [' + $scope.codeValidationData.NoOfCharacter + '] character';
            //    }
            //    else if (!$scope.codeValidationData.IsFixedNoOfCharacter
            //        && ($scope.characteristicsvalueNew.Code.length > $scope.codeValidationData.NoOfCharacter)) {
            //        throw 'Code can not be greater than ' + $scope.codeValidationData.NoOfCharacter;
            //    }
            //}
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.characteristicsvalueForm.$valid) {
                angular.copy($scope.characteristicsvalueNew, $scope.characteristicsvalue);
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.characteristicsvalue,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.characteristicsValueList.push(response.data.CharacteristicsValue);
                            $scope.characteristicsValueList = $filter('orderBy')($scope.characteristicsValueList, 'Sequence');
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.characteristicsvalue,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.characteristicsValueList[$scope.index] = $scope.characteristicsvalue;
                                $scope.characteristicsValueList = $filter('orderBy')($scope.characteristicsValueList, 'Sequence');
                            }
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.characteristicsvalueNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.characteristicsvalueNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.characteristicsValueList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };;

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.characteristicsvalue = {};
        $scope.characteristicsvalueNew = {
            CharacteristicsId: $scope.characteristicsvalueNew.CharacteristicsId
            , Sequence: seq, Active: true, IsDefault: false
        };
    }
};