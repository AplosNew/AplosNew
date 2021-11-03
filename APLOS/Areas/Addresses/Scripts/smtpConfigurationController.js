'use strict';
smtpConfigurationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService',  '$http', '$filter', 'cboService'];
function smtpConfigurationController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = 'SMTP Configuration';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.areas = [];
    $scope.cityList = [];
    $scope.path = 'addresses/smtpConfiguration/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.areaNew = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        CompanyName: null,
        Email: null,
        Host: null,
        Port: null,
        MailingUserName: null,
        Password: null,
        WebDomain: null,
        IsSSL: true
    };
    $scope.area = Object.assign({}, $scope.areaNew);
    baseService.init($scope.getListUrl, null, null, null, 'CompanyName', 'CompanyName');
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.area.CompanyGroupId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.areas = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.companyGroupOnChange = function () {
        $scope.getData();
        cboService.getCompanyGroupCompanyCbo($scope.area.CompanyGroupId, function (result) {
            $scope.companyList = result;
        });
    };
    $rootScope.searchByList = [
        {
            'name': 'Company',
            'value': 'CompanyName'
        },
        {
            'name': 'MailingUserName',
            'value': 'MailingUserName'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.areas[$scope.index], $scope.areaNew);
        angular.copy($scope.areaNew, $scope.area);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.areaForm.$valid) {
            angular.copy($scope.area, $scope.areaNew);
            var company = document.getElementById('companyId').options[document.getElementById('companyId').selectedIndex].text;

            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'addresses/smtpConfiguration/create',
                    data: $scope.areaNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.copy(response.data.SMTPConfiguration, $scope.areaNew);
                        $scope.areaNew.CompanyName = company;
                        $scope.areas.push($scope.areaNew);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'addresses/smtpConfiguration/edit',
                    data: $scope.areaNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.areaNew.CompanyName = company;
                            angular.copy($scope.areaNew, $scope.areas[$scope.index]);
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.area.Id)) {
            $http({
                method: 'POST',
                url: 'addresses/smtpConfiguration/delete/' + $scope.area.Id,
                datatype: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.error === true) {
                    showresult(response.data.message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.areas.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.areaNew = {};
        $scope.area = { CompanyGroupId: $scope.area.CompanyGroupId, IsSSL: true };
    }
}