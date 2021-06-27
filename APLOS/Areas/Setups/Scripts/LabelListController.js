'use strict';
LabelListController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function LabelListController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "LabelList";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.brands = [];
    $scope.path = 'Setups/LabelList/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'GetAutoSequence';

    $scope.model = {
        Id: null, Sequence: 0, Code: null, ShortName: null, Description: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
   
    $scope.modelNew = Object.assign({}, $scope.model);
    $scope.searchCol = "";
    $scope.searchVal = "";
    $scope.PRsearchBy = "Id";
    $scope.PRsearch = "";
    $scope.PRFilterList = [
        { 'name': 'Sequence', 'value': 'Sequence' },
        { 'name': 'Code', 'value': 'Code' },
        { 'name': 'Short Name', 'value': 'ShortName' },
    ];

    $scope.getData = function () {
        $scope.masterDataList = [];
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.getListUrl + "?column=" + $scope.PRsearchBy + "&value=" + $scope.PRsearch
        }).then(function successCallback(response) {
            $scope.masterDataList = response.data;
        });
    };
    $scope.getData();

    $scope.LabelNameList = [];
    cboService.getEnumCbo("enum/GetCboLabelNameInLocalLanguage", function (result) {
        $scope.LabelNameList = result;
    });

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.modelNew.Sequence = response.data[0].Sequence;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (obj) {
        $scope.model = obj.data;
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.LabelForm.$valid) {
            angular.copy($scope.modelNew, $scope.model);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'entity': $scope.model },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.GetSequence();
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'entity': $scope.model },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.GetSequence();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.model = {};
        $scope.modelNew = {};
        $scope.GetSequence();
    }
}