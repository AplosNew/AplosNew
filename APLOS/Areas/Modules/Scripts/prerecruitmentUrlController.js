'use strict';
PrerecruitmentUrlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PrerecruitmentUrlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Module Extended";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Modules/prerecruitmentUrl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'save';
    $scope.prerecruitmentUrl = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        ModuleId: null,
        Url: null,
        Remarks: null,
        Active: false,
        AddedBy: null,
        AddedDate: new Date(),
        UpdatedDate: new Date()
    };

    cboService.getCboCompanyGroup(function (data) {
        $scope.companyGroupList = data;
    });

    $scope.getCboCompanyByCompanyGroup = function (companyGroupId) {
        cboService.getCboCompanyByCompanyGroup(companyGroupId, function (data) {
            $scope.companyList = data;
        });
    };

    $scope.prerecruitmentUrls = [];

    $scope.getList = function () {
        $http({
            method: 'GET',
            url: 'Modules/PrerecruitmentUrl/GetList?companyGroupId=' + $scope.prerecruitmentUrl.CompanyGroupId + '&companyId=' + $scope.prerecruitmentUrl.CompanyId
        }).then(function successCallback(response) {
            $scope.prerecruitmentUrls = response.data;
        });
    };
    function validate(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].Active && baseService.isUndefinedOrNull(list[i].Url)) {
                    throw "Url can not be empty";
                } else if (baseService.isUndefinedOrNull(list[i].Url) === false) {
                    if (baseService.isUndefinedOrNull(list[i].Active) === false) {
                        if (list[i].Active === false) {
                            throw "select check box";
                        }
                    } else {
                        throw "select check box";
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.Save = function () {
        try {
            validate($scope.prerecruitmentUrls)
            if ($scope.prerecruitmentUrls.length > 0) {
                angular.forEach($scope.prerecruitmentUrls, function (item, index) {
                    item.ModuleId = $scope.prerecruitmentUrl.ModuleId;
                });
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.prerecruitmentUrls,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            } else {
                ShowResult(commonMessage.NullValueCheck, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}