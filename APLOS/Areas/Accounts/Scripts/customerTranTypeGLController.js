'use strict';
customerTranTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function customerTranTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Customer Tran GL";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.creditNoteTypeGivenGLList = [];
    $scope.customerTranTypeGivenGLWithCombineList = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.saveUrl = $scope.path + 'SaveFinancingTypeGL';
    $scope.customerTranTypeGivenGL = {
        Id: null,
        CountryId: null,
        AssetGLId: null,
        AssetBudgetMasterId: null,
        AssetActivityId: null,
        RevenueGLId: null,
        RevenueBudgetMasterId: null,
        RevenueActivityId: null,
        COAId: null,
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

    $scope.creditNoteTypeGivenList = [];
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
                if ($scope.tempList[i].CreditNoteTypeGivenId === data.CreditNoteTypeGivenId) {
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
            if ($scope.customerTranTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetCustomerTranTypeGLAllList?coaId=' + $scope.customerTranTypeGivenGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.customerTranTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetCustomerTranTypeGLNotAssingList?coaId=' + $scope.customerTranTypeGivenGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.customerTranTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetCustomerTranTypeGLAssingList?coaId=' + $scope.customerTranTypeGivenGL.COAId;
        }
        $scope.customerTranTypeGivenGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'AssetUserName', 'AssetUserName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.customerTranTypeGivenGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.customerTranTypeGivenGLWithCombineList.length; i++) {
                        $scope.customerTranTypeGivenGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.customerTranTypeGivenGLWithCombineList[i].InvestmentTypeGivenId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
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
        if ($scope.customerTranTypeGivenGL.COAId === null || $scope.customerTranTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityCOAWise?coaId=' + $scope.customerTranTypeGivenGL.COAId;
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
        $scope.customerTranTypeGivenGL.LiabilityGLId = x.GLGeneralInfoId;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.customerTranTypeGivenGL.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.customerTranTypeGivenGL.LiabilityBudgetMasterId = null;
        $scope.customerTranTypeGivenGL.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.customerTranTypeGivenGL.COAId, $scope.customerTranTypeGivenGL.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.customerTranTypeGivenGL.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
        });
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
        if ($scope.customerTranTypeGivenGL.COAId === null || $scope.customerTranTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetLiabilityGLListExceptRecon?coaId=' + $scope.customerTranTypeGivenGL.COAId;
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
        $scope.customerTranTypeGivenGL.AssetGLId = x.GLGeneralInfoId;
        getAssetBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.AssetGLInof = null;
        $scope.customerTranTypeGivenGL.AssetGLId = null;
        $scope.assetBudgetList = [];
        $scope.assetActivityList = [];
        $scope.customerTranTypeGivenGL.AssetBudgetMasterId = null;
        $scope.customerTranTypeGivenGL.AssetActivityId = null;
    };

    $scope.assetBudgetList = [];
    function getAssetBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.customerTranTypeGivenGL.COAId, $scope.customerTranTypeGivenGL.AssetGLId, function (result) {
            $scope.assetBudgetList = result;
        });
    }

    $scope.assetActivityList = [];
    $scope.getAssetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.customerTranTypeGivenGL.AssetBudgetMasterId, function (result) {
            $scope.assetActivityList = result;
        });
    };





    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.CreditNoteTypeGivenName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.customerTranTypeGivenGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.customerTranTypeGivenGLWithCombineList[i].Id == $scope.glUntagId) {
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
        $scope.customerTranTypeGivenGLWithCombineList[i] = {
            AssetUserName: $scope.customerTranTypeGivenGLWithCombineList[i].AssetUserName,
            COAId: $scope.customerTranTypeGivenGLWithCombineList[i].COAId,
            COAName: $scope.customerTranTypeGivenGLWithCombineList[i].COAName,
            Code: $scope.customerTranTypeGivenGLWithCombineList[i].Code,
            FinancingTypeId: $scope.customerTranTypeGivenGLWithCombineList[i].FinancingTypeId
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
        $scope.customerTranTypeGivenGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.customerTranTypeGivenGL.AssetGLId !== null) {
                    item.AssetGLId = $scope.customerTranTypeGivenGL.AssetGLId;
                }
                if ($scope.customerTranTypeGivenGL.AssetBudgetMasterId !== null) {
                    item.AssetBudgetMasterId = $scope.customerTranTypeGivenGL.AssetBudgetMasterId;
                }
                if ($scope.customerTranTypeGivenGL.AssetActivityId !== null) {
                    item.AssetActivityId = $scope.customerTranTypeGivenGL.AssetActivityId;
                }
                item.COAId = $scope.customerTranTypeGivenGL.COAId;
                $scope.customerTranTypeGivenGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.customerTranTypeGivenGLListForSave.length < 1) {
            return showresult("please select credit note type given!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.customerTranTypeGivenGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'financingTypeGLList': $scope.customerTranTypeGivenGLListForSave
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
                $scope.getCreditNoteTypeGivenWithCoa('all');
            }
        } else {
            $scope.getCreditNoteTypeGivenWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.customerTranTypeGivenGL = { COAId: $scope.customerTranTypeGivenGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.customerTranTypeGivenGLWithCombineList = [];
    }
}