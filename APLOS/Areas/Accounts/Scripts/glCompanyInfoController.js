'use strict';
GLCompanyInfoController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'toaster', '$compile'];
function GLCompanyInfoController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, toaster, $compile) {
    $rootScope.title = 'GL Item';
    $scope.Action = 'Save';
    $scope.path = 'accounts/getglcompanyinfolist/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.glItems = [];

    $scope.glcominfo = {
        Id: null,
        Sequence: null,
        GLGeneralInfoId: null,
        CompanyId: null,
        COAId: null,
        CurrencyId: null,
        TaxCategory: null,
        PostingWithoutTaxAllow: true,
        AlternativeGL: null,
        AlternativeCOAId: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.cOAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.cOAList = result;
    });

    $scope.companyList = [];
    $scope.onCOAChange = function (coaId) {
        cboService.getCboCompanyByCOA(coaId, function (result) {
            $scope.companyList = result;
        });
    };

    $scope.searchByGlList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Account Code',
            'value': 'AccountCode'
        },
        {
            'name': 'GL Name',
            'value': 'UserName'
        },
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        }
    ];
    $scope.onCompanyChange = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/glitem/GetGlCompanyConfigList?companyId=' + item
        }).then(function successCallback(response) {
            $scope.glcominfoList = response.data;
        });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.glinfoNewForm1.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/glitem/glcompanyinfoInsert',
                    data: {
                        'glcominfolist': JSON.stringify($scope.glcominfoList),
                        'companyId': $scope.glcominfo.CompanyId
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        $scope.getData();
                    }
                });
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/glitem/edit',
                    data: {
                        'glGeneralInfo': $scope.glinfo,
                        'glAccountType': $scope.glaccounttypies
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.glItems[$scope.index] = $scope.glinfo;
                        }
                        $scope.getData();
                        ClearFields($scope.onCOAChangeSequence($scope.glinfoNew.COAId));
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.glinfo.Id)) {
            $http({
                method: 'POST',
                url: "accounts/glitem/delete/" + $scope.glinfo.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.glItems.splice($scope.index, 1);
                    baseService.paginationAdd();
                    $scope.getData();
                    ClearFields($scope.onCOAChangeSequence($scope.glinfoNew.COAId));
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }

    $scope.index = -1;
    $scope.Get = function (id, index, COAId) {
        $scope.index = index;
        $scope.glinfo = $scope.glItems[$scope.index];
        $scope.glinfoNew = Object.assign({}, $scope.glinfo);
        $scope.glinfo.AddedDate = $filter('dateFilter')($scope.glinfo.AddedDate);
        $scope.glinfo.UpdatedDate = $filter('dateFilter')($scope.glinfo.UpdatedDate);
        $scope.Action = "Update";
        $scope.GetGLAccountTypeByGLId(id);
        $scope.checkIsLevelMandatoryUpdate($scope.glinfo.COAId);
        $scope.GetAccountGroupNumberChange($scope.glinfo.AccountGroupId);
        $scope.showMsg = null;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.pop = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
    }
}