'use strict';
localLanguageLabelController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function localLanguageLabelController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Language Information';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.languages = [];
    $scope.path = 'Setups/LocalLanguage/';
    $scope.saveUrl = $scope.path + 'CreateLabel';
    $scope.updateUrl = $scope.path + 'EditLabel';
    $scope.deleteUrl = $scope.path + 'DeleteLabel/';
    $scope.getListUrl = $scope.path + 'GetLabelList';
    $("select.form-control").select2({
        placeholder: "Select an option"
        , allowClear: true
    });

    $scope.locallanguageLabel = {
        Id: null
        , LanguageId: null
        , LabelName: null
        , Name: null
    };

    $scope.getLabelList = function () {
        $scope.locallanguageLabels = [];
        baseService.init($scope.getListUrl, null, null, null, 'Name', 'Name');
        $scope.getData = function (pageno) {
            $rootScope.parameters.languageId = $scope.locallanguageLabel.LanguageId;
            baseService.pagination(pageno)
                .then(function (data) {
                    $scope.locallanguageLabels = data.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.getData();

    };

    $scope.searchByList = [
        {
            'name': 'Language',
            'value': 'LanguageName'
        },
        {
            'name': 'Label',
            'value': 'LabelName'
        },
        {
            'name': 'Name',
            'value': 'Name'
        }

    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.locallanguageLabel = $scope.locallanguageLabels[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    cboService.getEnumCbo("enum/GetCboLabelNameInLocalLanguage", function (result) {
        $scope.localLanguageList = result;
    });

    $scope.languageList = [];
    cboService.getCboLanguage(function (data) {
        $scope.languageList = data;
    });

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: $scope.locallanguageLabel
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: $scope.locallanguageLabel
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.locallanguageLabel.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.locallanguageLabel.Id,
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
        $scope.locallanguageLabel.LanguageId = null;
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.locallanguageLabel = { LanguageId: $scope.locallanguageLabel.LanguageId };
    }
}