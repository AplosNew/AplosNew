'use strict';
baseAttributeAndCharacteristicsValueController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter'];
function baseAttributeAndCharacteristicsValueController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter) {

    // #region Attribute Value

    $scope.searchvalueList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StanderName',
            'value': 'StanderName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];

    $scope.attributeValuePopUp = function (data) {
        $scope.valueParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Code'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.valueList = [];
        $scope.materialAttributeValueUrl = 'Materials/MaterialAttributeValue/GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.assignment = data.ValueAssignmentLevel;
            $scope.valueParameters.materialMasterId = data.MaterialMasterId;
            $scope.valueParameters.attributeId = data.MaterialAttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };

    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };

    $scope.searchCharFilterList = [
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

    $scope.charValuePopUp = function (data) {
        $scope.charValueParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Code'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.charDataList = [];
        $scope.charValueCharName = data.UserName;
        baseService.setCurrentPage('charDataList');
        $scope.url = 'Materials/CharacteristicsValue/getcharacteristicsvaluesearchdata/';
        $scope.getSearchCharData = function (pageno) {
            $scope.charValueParameters.assignment = data.ValueAssignmentLevel;
            $scope.charValueParameters.materialMasterId = data.MaterialMasterId;
            $scope.charValueParameters.charId = data.CharacteristicsId;
            baseService.paginationBase($scope.url, pageno, $scope.charValueParameters)
                .then(function (result) {
                    $scope.dataPlate = result.Rows;
                    $scope.charDataList = result.Rows;
                    $scope.charValueParameters.total_count = result.Total;
                    angular.element(document.querySelector('#searchcharactervaluepopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getSearchCharData();
    };

    $scope.closeCharValuePopUp = function () {
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
        CloseModalShowResult('searchcharactervaluepopup');
    };
}