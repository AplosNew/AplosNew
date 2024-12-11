'use strict';
ProcessConstraintController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProcessConstraintController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Process Constraint';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.PCList = [];
    $scope.path = 'Materials/MaterialMasterArticle/';
    $scope.getListUrl = $scope.path + 'GetPCList';
    $scope.getSeqUrl = $scope.path + 'GetAutoPCSequence';
    $scope.getVSeqUrl = $scope.path + 'GetAutoPCVSequence';
    $scope.saveUrl = $scope.path + 'CreateProcessConstraint';
    $scope.saveValueUrl = $scope.path + 'CreateProcessConstraintValue';
    $scope.deleteUrl = $scope.path + 'DeleteProcessConstraint/';
    $scope.deleteValueUrl = $scope.path + 'DeleteProcessConstraintValue/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.joiningParameterList = [
        { Value: ", ", Text: "Comma(,)" },
        { Value: ", ", Text: "Comma Space(, )" },
        { Value: " ", Text: "Space()" },
        { Value: "/", Text: "Slash(/)" },
        { Value: "-", Text: "Hyphen(-)" },
        { Value: ":", Text: "Colon(:)" }
    ];

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPCList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            $scope.PCList = response.data;
            $scope.GetSequence();
        });
    }
    $scope.getData();
    $scope.processList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processList = response;
    });
    $scope.processConstraint = {
        Id: null
        , Sequence: null
        , AttributeProperty: null
        , IsFixedNoOfCharacter: false
        , NoOfCharacter: 0
        , IsFreeField: true
        , IsPreDefinedField: true
        , IsMandatory: true
        , Active: true,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
        Description: null
    };
    $scope.processConstraintNew = Object.assign({}, $scope.processConstraint);

    $scope.attributePropertyList = [];
    cboService.getEnumCbo("enum/GetAttributePropertiesCbo", function (result) {
        $scope.attributePropertyList = result;
    });
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.processConstraintNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.processConstraintNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
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
                data: { 'data': $scope.processConstraintNew },
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
        if (!baseService.isUndefinedOrNull($scope.processConstraintNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.processConstraintNew.Id,
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

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.processConstraintNew = Object.assign({}, $scope.processConstraint);
        $scope.processConstraintNew.Sequence = seq;
    }

    $scope.processConstraintId = null;
    $scope.GetValueDetail = function (obj) {
        $scope.processConstraintId = obj.data.Id;
        $scope.getValueData();
        $scope.GetValueSequence();
        angular.element(document.querySelector('#valueDetailPopUp')).modal('show');
    }

    $scope.CloseValue = function () {
        angular.element(document.querySelector('#valueDetailPopUp')).modal('hide');
    }
    $scope.materialValue = {
        Id: null
        , ProcessConstraintId: $scope.processConstraintId
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

    $scope.materialValueAction = 'Save';

    $scope.GetValueSequence = function () {
        cboService.getSequence('Materials/MaterialMasterArticle/GetAutoPCVSequence?masterId=' + $scope.processConstraintId, function (data) {
            $scope.materialValueNew.Sequence = data;
        });
    };

    $scope.ModelValueList = [];
    $scope.getValueData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPCVList",
            data: { 'masterId': $scope.processConstraintId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelValueList = response.data;
        });
    }

    $scope.SaveValue = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ValueNewForm.$valid) {
            $scope.materialValueNew.ProcessConstraintId = $scope.processConstraintId;
            $http({
                method: 'POST',
                url: $scope.saveValueUrl,
                data: { 'data': $scope.materialValueNew, 'masterId': $scope.processConstraintId},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearValue();
                    $scope.getValueData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteValue = function () {
        if (!baseService.isUndefinedOrNull($scope.materialValueNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteValueUrl,
                data: { 'id': $scope.materialValueNew.Id, 'masterId': $scope.processConstraintId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearValue();
                    $scope.getValueData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearValue = function () {
        ClearValueFields($scope.GetValueSequence());
        return true;
    }

    function ClearValueFields(seq) {
        $scope.materialValueAction = 'Save';
        $scope.materialValue = {};
        $scope.materialValueNew = {
            Id: null
            , ProcessConstraintId: $scope.processConstraintId
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
    }

   

}