'use strict';
function ManagementChartofAccountFAController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Management Chart Of Account';
    $scope.Action = 'Save';
    $scope.coaRDList = [];
    $scope.col1List = [];
    $scope.col2List = [];
    $scope.col3List = [];
    $scope.index = -1;
    $scope.managementChartOfAccFAs = [];
    $scope.path = 'accounts/managementchartofaccount/';
    $scope.getListUrl = $scope.path + 'getmanagementchartofaccountlistbyfixedasset';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "APLOS1RDId", "APLOS1RDId");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.managementChartOfAccFAs = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'COA Relation Data',
            'value': 'APLOS1RDId'
        },
        {
            'name': 'FixedAsset Category',
            'value': 'Col1'
        },
        {
            'name': 'FixedAsset SubCategory',
            'value': 'Col2'
        },
        {
            'name': 'FixedAsset',
            'value': 'Col3'
        }
    ];

    $scope.managementChartOfAccFA = {
        Id: null,
        APLOS1RDId: null,
        Col1: null,
        Col2: null,
        Col3: null,
        UserName: null,
        FixedAssetName: null,
        FixedAssetCategoryName: null,
        FixedAssetSubCategoryName: null,
        ResponsiblePerson: null,
        EffectiveDate: null,
        Type: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $http({
        method: 'GET',
        url: 'accounts/chartofaccountrelationshipdata/chartofaccountrelationshipdatacbo'
    }).then(function successCallback(response) {
        $scope.coaRDList = response.data;
    });

    $http({
        method: 'GET',
        url: 'fixedassets/fixedassetcategory/getfixedassetcategorylist'
    }).then(function successCallback(response) {
        $scope.col1List = response.data;
    });

    $http({
        method: 'GET',
        url: 'fixedassets/fixedassetsubcategory/getfixedassetsubcategorylist'
    }).then(function successCallback(response) {
        $scope.col2List = response.data;
    });
    $http({
        method: 'GET',
        url: 'fixedassets/fixedasset/GetFixedAssetList'
    }).then(function successCallback(response) {
        $scope.col3List = response.data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.managementChartOfAccFA = $scope.managementChartOfAccFAs[$scope.index];
        $scope.managementChartOfAccFA.AddedDate = $filter('dateFilter')($scope.managementChartOfAccFA.AddedDate);
        $scope.managementChartOfAccFA.UpdatedDate = $filter('dateFilter')($scope.managementChartOfAccFA.UpdatedDate);
        $scope.managementChartOfAccFA.EffectiveDate = $filter('dateFilter')($scope.managementChartOfAccFA.EffectiveDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.UserName = $("#APLOS1RDId option:selected").text();
        $scope.FixedAssetCategoryName = $("#Col1 option:selected").text();
        $scope.FixedAssetSubCategoryName = $("#Col2 option:selected").text();
        $scope.FixedAssetName = $("#Col3 option:selected").text();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.managementChartOfAccFAForm.$valid) {
            if ($scope.Action == 'Save') {
                $scope.managementChartOfAccFA.Type = 'FA',
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.managementChartOfAccFA,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.managementChartOfAccFA = response.data.ManagementChartofAccount;
                            $scope.managementChartOfAccFA.UserName = $scope.UserName;
                            $scope.managementChartOfAccFA.FixedAssetName = $scope.FixedAssetName;
                            $scope.managementChartOfAccFA.FixedAssetCategoryName = $scope.FixedAssetCategoryName;
                            $scope.managementChartOfAccFA.FixedAssetSubCategoryName = $scope.FixedAssetSubCategoryName;
                            $scope.managementChartOfAccFAs.push($scope.managementChartOfAccFA)
                            baseService.paginationAdd();
                            ClearFields();
                        }
                    });
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.managementChartOfAccFA,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.managementChartOfAccFA.UserName = $scope.UserName;
                            $scope.managementChartOfAccFA.FixedAssetName = $scope.FixedAssetName;
                            $scope.managementChartOfAccFA.FixedAssetCategoryName = $scope.FixedAssetCategoryName;
                            $scope.managementChartOfAccFA.FixedAssetSubCategoryName = $scope.FixedAssetSubCategoryName;
                            $scope.managementChartOfAccFAs[$scope.index] = $scope.managementChartOfAccFA;
                        }
                        ClearFields();
                    }
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.managementChartOfAccFA.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.managementChartOfAccFA.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.managementChartOfAccFAs.splice($scope.index, 1);
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
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.managementChartOfAccFA = {};
        $scope.managementChartOfAccFA.Sequence = seq;
        $scope.managementChartOfAccFA.Active = true;
    }
};
ManagementChartofAccountFAController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];