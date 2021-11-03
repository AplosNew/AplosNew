'use strict';
rptConfigTemplateController.$inject = ['cboService','commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function rptConfigTemplateController(cboService,commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Report Configuration Template";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.rptConfigTemplates = [];
    $scope.path = 'Setups/rptconfigtemplate/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'Type', 'Type');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.rptConfigTemplates = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.rptConfigTemplate = {
        Id: null
        , Type: null
        , PlantId: null
        , Language: null
        , FormatName: null
        , TemplateFileName: null
    };

    cboService.getCboPlant(function (result) {
        $scope.PlantList = result;
    });

    cboService.getCboLanguage(function (result) {
        $scope.LanguageList = result;
    });

    $scope.reportTypeList = [];
    cboService.getEnumCbo('enum/GetLetterTypeCbo', function (result) {
        $scope.reportTypeList = result;
    });

    $scope.searchByList = [
        {
            'name': 'Type',
            'value': 'Type'
        },
        {
            'name': 'Plant',
            'value': 'PlantName'
        },
        {
            'name': 'Language',
            'value': 'Language'
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.rptConfigTemplate = $scope.rptConfigTemplates[$scope.index];
        $scope.rptConfigTemplate.AddedDate = $filter('dateFilter')($scope.rptConfigTemplate.AddedDate);
        $scope.rptConfigTemplate.UpdatedDate = $filter('dateFilter')($scope.rptConfigTemplate.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.rptConfigTemplateForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.rptConfigTemplate,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.rptConfigTemplates.push(response.data.RptConfigTemplate);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.rptConfigTemplate,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.rptConfigTemplates[$scope.index] = $scope.rptConfigTemplate;
                        }
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.rptConfigTemplate.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.rptConfigTemplate.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.rptConfigTemplates.splice($scope.index, 1);
                    baseService.paginationRemove();
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
        $scope.rptConfigTemplate = {};
    }
}