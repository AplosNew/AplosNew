'use strict';
securityTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http'];
function securityTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = "Security Type GL";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.securityTypeGivenGLList = [];
    $scope.securityTypeGivenGLWithCombineList = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.saveUrl = $scope.path + 'SaveFinancingTypeGL';
    $scope.securityTypeGivenGL = {
        Id: null,
        CountryId: null,
        SecurityTypeGivenId: null,
        AssetGLId: null,
        AssetBudgetMasterId: null,
        AssetActivityId: null,
        RevenueGLId: null,
        RevenueBudgetMasterId: null,
        RevenueActivityId: null,
        COAId: null,
        SecurityTypeTakenId: null,
        ExpensesGLId: null,
        ExpensesBudgetMasterId: null,
        ExpensesActivityId: null,
        LiabilityGLId: null,
        LiabilityBudgetMasterId: null,
        LiabilityActivityId: null
    };

    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector('#itemsearchpopup')).modal('show');
    };

    $scope.securityTypeGivenList = [];
    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        if (event.currentTarget.checked) {
            $scope.tempList.push(data);
        }
        else {
            for (var i = 0; i < $scope.tempList.length; i++) {
                if ($scope.tempList[i].SecurityTypeGivenId === data.SecurityTypeGivenId) {
                    $scope.tempList.splice(i, 1);
                }
                break;
            }
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    $scope.showAll = function (str) {
        if (str === 'all') {
            if ($scope.securityTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetSecurityTypeGLAllList?coaId=' + $scope.securityTypeGivenGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.securityTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetSecurityTypeGLNotAssingList?coaId=' + $scope.securityTypeGivenGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.securityTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetSecurityTypeGLAssingList?coaId=' + $scope.securityTypeGivenGL.COAId;
        }
        $scope.securityTypeGivenGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'AssetUserName', 'AssetUserName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.securityTypeGivenGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.securityTypeGivenGLWithCombineList.length; i++) {
                        $scope.securityTypeGivenGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.securityTypeGivenGLWithCombineList[i].SecurityTypeGivenId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.searchAssetTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.assetTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getAssetTypeList = function () {
        if ($scope.securityTypeGivenGL.COAId === null || $scope.securityTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetCOAWiseExceptRecon?coaId=' + $scope.securityTypeGivenGL.COAId;
        $scope.getAssetTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.assetTypeListParameters)
                .then(function (data) {
                    $scope.assetTypeGLList = data.Rows;
                    $scope.assetTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#assetTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getAssetTypeListData();
    };

    $scope.closeAssetTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#assetTypeListPopUp')).modal('hide');
        }
    };

    $scope.setAssetGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.AssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.securityTypeGivenGL.AssetGLId = x.GLGeneralInfoId;
        getAssetBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.AssetGLInof = null;
        $scope.securityTypeGivenGL.AssetGLId = null;
        $scope.assetBudgetList = [];
        $scope.assetActivityList = [];
        $scope.securityTypeGivenGL.AssetBudgetMasterId = null;
        $scope.securityTypeGivenGL.AssetActivityId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.securityTypeGivenGL.COAId, $scope.securityTypeGivenGL.AssetGLId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.securityTypeGivenGL.AssetBudgetMasterId, function (result) {
            $scope.assetActivityList = result;
        });
    };

    $scope.searchLiabilityTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.liabilityTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getLiabilityTypeList = function () {
        if ($scope.securityTypeGivenGL.COAId === null || $scope.securityTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first.', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityCOAWise?coaId=' + $scope.securityTypeGivenGL.COAId;
        $scope.getLiabilityTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.liabilityTypeListParameters)
                .then(function (data) {
                    $scope.liabilityTypeGLList = data.Rows;
                    $scope.liabilityTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#liabilityTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getLiabilityTypeListData();
    };

    $scope.closeLiabilityTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#liabilityTypeListPopUp')).modal('hide');
        }
    };

    $scope.setLiabilityGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.LiabilityGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.securityTypeGivenGL.LiabilityGLId = x.GLGeneralInfoId;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.securityTypeGivenGL.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.securityTypeGivenGL.LiabilityBudgetMasterId = null;
        $scope.securityTypeGivenGL.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.securityTypeGivenGL.COAId, $scope.securityTypeGivenGL.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.securityTypeGivenGL.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
        });
    };

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.SecurityTypeGivenName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };

    $scope.removeRow = function () {
        for (var i = 0; i < $scope.securityTypeGivenGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.securityTypeGivenGLWithCombineList[i].Id === $scope.glUntagId) {
                    $scope.unTagGL($scope.glUntagId, i);
                    break;
                }
            } else {
                unTagFromList($scope.glUntagIndex);
                $scope.glUntagIndex = -1;
                break;
            }
        }
        $scope.mauid = null;
        $scope.mauindex = -1;
    };

    function unTagFromList(i) {
        $scope.securityTypeGivenGLWithCombineList[i] = {
            AssetUserName: $scope.securityTypeGivenGLWithCombineList[i].AssetUserName,
            COAId: $scope.securityTypeGivenGLWithCombineList[i].COAId,
            COAName: $scope.securityTypeGivenGLWithCombineList[i].COAName,
            Code: $scope.securityTypeGivenGLWithCombineList[i].Code,
            FinancingTypeId: $scope.securityTypeGivenGLWithCombineList[i].FinancingTypeId
        };
    }

    $scope.unTagGL = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/DeleteFinancingTypeGL',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.tempList.length; i++) {
                        if ($scope.tempList[i].Id === id) {
                            document.getElementById($scope.tempList[i].FinancingTypeId).checked = false;
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }
                    unTagFromList(index);
                    $scope.glUntagIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.addGlForSelectble = function () {
        $scope.securityTypeGivenGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.securityTypeGivenGL.AssetGLId !== null) {
                    item.AssetGLId = $scope.securityTypeGivenGL.AssetGLId;
                }
                if ($scope.securityTypeGivenGL.AssetActivityId !== null) {
                    item.AssetActivityId = $scope.securityTypeGivenGL.AssetActivityId;
                }
                if ($scope.securityTypeGivenGL.AssetBudgetMasterId !== null) {
                    item.AssetBudgetMasterId = $scope.securityTypeGivenGL.AssetBudgetMasterId;
                }
                if ($scope.securityTypeGivenGL.RevenueGLId !== null) {
                    item.RevenueGLId = $scope.securityTypeGivenGL.RevenueGLId;
                }
                if ($scope.securityTypeGivenGL.RevenueActivityId !== null) {
                    item.RevenueActivityId = $scope.securityTypeGivenGL.RevenueActivityId;
                }
                if ($scope.securityTypeGivenGL.RevenueBudgetMasterId !== null) {
                    item.RevenueBudgetMasterId = $scope.securityTypeGivenGL.RevenueBudgetMasterId;
                }
                if ($scope.securityTypeGivenGL.LiabilityGLId !== null) {
                    item.LiabilityGLId = $scope.securityTypeGivenGL.LiabilityGLId;
                }
                if ($scope.securityTypeGivenGL.LiabilityActivityId !== null) {
                    item.LiabilityActivityId = $scope.securityTypeGivenGL.LiabilityActivityId;
                }
                if ($scope.securityTypeGivenGL.LiabilityBudgetMasterId !== null) {
                    item.LiabilityBudgetMasterId = $scope.securityTypeGivenGL.LiabilityBudgetMasterId;
                }
                if ($scope.securityTypeGivenGL.ExpensesGLId !== null) {
                    item.ExpensesGLId = $scope.securityTypeGivenGL.ExpensesGLId;
                }
                if ($scope.securityTypeGivenGL.ExpensesActivityId !== null) {
                    item.ExpensesActivityId = $scope.securityTypeGivenGL.ExpensesActivityId;
                }
                if ($scope.securityTypeGivenGL.ExpensesBudgetMasterId !== null) {
                    item.ExpensesBudgetMasterId = $scope.securityTypeGivenGL.ExpensesBudgetMasterId;
                }
                item.COAId = $scope.securityTypeGivenGL.COAId;
                $scope.securityTypeGivenGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.securityTypeGivenGLListForSave.length < 1) {
            return ShowResult("Please select Security Type!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.securityTypeGivenGL.AssetGLId) && baseService.isUndefinedOrNull($scope.securityTypeGivenGL.RevenueGLId)) {
            return ShowResult("Please select Asset and Revenue both side GL!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form0.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'financingTypeGLList': $scope.securityTypeGivenGLListForSave
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };

    $scope.btnSet = '';
    $scope.setActiveBtn = function (str) {
        $scope.btnSet = str;
    };

    $scope.getAllWithCoa = function () {
        if ($scope.btnSet !== '') {
            if ($scope.btnSet === 'all') {
                $scope.getSecurityTypeGivenWithCoa('all');
            }
        } else {
            $scope.getSecurityTypeGivenWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.refreshLiabilityGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.securityTypeGivenGL = { COAId: $scope.securityTypeGivenGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.securityTypeGivenGLWithCombineList = [];
    }
}