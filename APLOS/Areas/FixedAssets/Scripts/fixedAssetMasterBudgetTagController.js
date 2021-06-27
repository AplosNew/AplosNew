'use strict';
fixedAssetMasterBudgetTagController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$http', '$filter'];
function fixedAssetMasterBudgetTagController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $http, $filter) {
    $rootScope.title = "FixedAsset MasterBudget Tag";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.fixedAssetMasterBudgetTagList = [];
    $scope.path = 'FixedAssets/fixedAssetMasterBudgetTag/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.fixedAssetMasterBudgetTag = {
        Id: null,
        COAId: null,
        FixedAssetMasterId: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.fixedAssetMasterBudgetTagNew = Object.assign({}, $scope.fixedAssetMasterBudgetTag);

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.fixedAssetMasterCboList = [];
    cboService.getFixedAssetMasterList(function (result) {
        $scope.fixedAssetMasterCboList = result;
    });

    $scope.selectChValueId = function (data) {
        try {
            if (data.FixedAssetMasterId !== null || data.FixedAssetMasterId !== '') {
                if (checkExistTempList($scope.tempList, data.BudgetMasterId) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].BudgetMasterId === data.BudgetMasterId) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                    if ($scope.tempList[i].BudgetMasterId === data.BudgetMasterId) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempList(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].BudgetMasterId === id) {
                return true;
            }
        }
        return false;
    }
    function cacheFixedAssetMasterValue(list, BudgetMasterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].BudgetMasterId === BudgetMasterId) {
                return list[i].FixedAssetMasterId;
            }
        }
        //return null;
    }
    $scope.searchByList = [
        {
            'name': 'Budget',
            'value': 'BudgetName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
        ,
        {
            'name': 'Budget Category',
            'value': 'BudgetCategoryName'
        }
        ,
        {
            'name': 'Budget SubCategory',
            'value': 'BudgetSubCategoryName'
        }
        , {
            'name': 'Ref No',
            'value': 'RefNo'
        }
    ];

    $scope.fixedAssetMasterBudgetTagListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'GLGeneralInfoName, BudgetName',
        searchBy: 'BudgetName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getfixedAssetMasterBudgetTag = function () {
        baseService.setCurrentPage('fixedAssetMasterBudgetTagList');
        $scope.loadData = function (pageno) {
            baseService.paginationBase($scope.path + 'GetFixedAssetMasterBudgetTagList?coaId=' + $scope.fixedAssetMasterBudgetTagNew.COAId, pageno, $scope.fixedAssetMasterBudgetTagListParameters)
                .then(function (result) {
                    $scope.fixedAssetMasterBudgetTagList = result.Rows;
                    $scope.fixedAssetMasterBudgetTagListParameters.total_count = result.Total;
                    angular.forEach($scope.fixedAssetMasterBudgetTagList, function (item, i) {
                        if (checkExistTempList($scope.tempList, item.BudgetMasterId)) {
                            item.FixedAssetMasterId = cacheFixedAssetMasterValue($scope.tempList, item.BudgetMasterId);
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.loadData();
    };

    $scope.getFATagValue = function () {
        $scope.tempList = [];
        $scope.getfixedAssetMasterBudgetTag();
    }

    function setDataForSave(list) {
        angular.forEach(list, function (item) {
            item.BudgetMasterId = item.BudgetMasterId;
            $scope.fixedAssetMasterBudgetTagSaveList.push(item);
        });
    }
    $scope.fixedAssetMasterBudgetTagSaveList = [];
    $scope.Save = function () {
        try {
            angular.copy($scope.fixedAssetMasterBudgetTagNew, $scope.fixedAssetMasterBudgetTag);
            $scope.fixedAssetMasterBudgetTagSaveList = [];
            setDataForSave($scope.tempList);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'fixedAssetMasterBudgetTag': $scope.fixedAssetMasterBudgetTagSaveList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getfixedAssetMasterBudgetTag();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.fixedAssetMasterBudgetTag = { COAId: $scope.fixedAssetMasterBudgetTag.COAId };
        $scope.fixedAssetMasterBudgetTagNew = { COAId: $scope.fixedAssetMasterBudgetTagNew.COAId };
        $scope.tempList = [];
    }
}