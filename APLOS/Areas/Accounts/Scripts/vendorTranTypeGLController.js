'use strict';
vendorTranTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function vendorTranTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Vendor Tran Type GL";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.vendorTranTypeGivenGLList = [];
    $scope.vendorTranTypeGivenGLWithCombineList = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.saveUrl = $scope.path + 'SaveFinancingTypeGL';
    $scope.vendorTranTypeGivenGL = {
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

    $scope.debitNoteTypeGivenList = [];
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
                if ($scope.tempList[i].debitNoteTypeGivenId === data.debitNoteTypeGivenId) {
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
            if ($scope.vendorTranTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetVendorTranTypeGLAllList?coaId=' + $scope.vendorTranTypeGivenGL.COAId;
        }
        if (str === 'notassing') {
            if ($scope.vendorTranTypeGivenGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetVendorTranTypeGLNotAssingList?coaId=' + $scope.vendorTranTypeGivenGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.vendorTranTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/FinancingType/GetVendorTranTypeGLAssingList?coaId=' + $scope.vendorTranTypeGivenGL.COAId;
        }
        $scope.vendorTranTypeGivenGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'AssetUserName', 'AssetUserName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.vendorTranTypeGivenGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.vendorTranTypeGivenGLWithCombineList.length; i++) {
                        $scope.vendorTranTypeGivenGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.vendorTranTypeGivenGLWithCombineList[i].InvestmentTypeGivenId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

   
    

    // #region ******LiablityType GL******
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
        if ($scope.vendorTranTypeGivenGL.COAId === null || $scope.vendorTranTypeGivenGL.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAssetLiabilityGLListExceptRecon?coaId=' + $scope.vendorTranTypeGivenGL.COAId;
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
        $scope.vendorTranTypeGivenGL.LiabilityGLId = x.GLGeneralInfoId;
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.vendorTranTypeGivenGL.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.vendorTranTypeGivenGL.LiabilityBudgetMasterId = null;
        $scope.vendorTranTypeGivenGL.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.vendorTranTypeGivenGL.COAId, $scope.vendorTranTypeGivenGL.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.vendorTranTypeGivenGL.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
        });
    };
    // #endregion


    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.debitNoteTypeGivenName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.vendorTranTypeGivenGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.vendorTranTypeGivenGLWithCombineList[i].Id == $scope.glUntagId) {
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
        $scope.vendorTranTypeGivenGLWithCombineList[i] = {
            AssetUserName: $scope.vendorTranTypeGivenGLWithCombineList[i].AssetUserName,
            COAId: $scope.vendorTranTypeGivenGLWithCombineList[i].COAId,
            COAName: $scope.vendorTranTypeGivenGLWithCombineList[i].COAName,
            Code: $scope.vendorTranTypeGivenGLWithCombineList[i].Code,
            FinancingTypeId: $scope.vendorTranTypeGivenGLWithCombineList[i].FinancingTypeId
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
                if (response.data.Error == true) {
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
        $scope.vendorTranTypeGivenGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {

                if ($scope.vendorTranTypeGivenGL.LiabilityGLId !== null) {
                    item.LiabilityGLId = $scope.vendorTranTypeGivenGL.LiabilityGLId;
                }
                if ($scope.vendorTranTypeGivenGL.LiabilityActivityId !== null) {
                    item.LiabilityActivityId = $scope.vendorTranTypeGivenGL.LiabilityActivityId;
                }
                if ($scope.vendorTranTypeGivenGL.LiabilityBudgetMasterId !== null) {
                    item.LiabilityBudgetMasterId = $scope.vendorTranTypeGivenGL.LiabilityBudgetMasterId;
                }
                item.COAId = $scope.vendorTranTypeGivenGL.COAId;
                $scope.vendorTranTypeGivenGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.vendorTranTypeGivenGLListForSave.length < 1) {
            return showresult("please select debit note type given!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.vendorTranTypeGivenGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'financingTypeGLList': $scope.vendorTranTypeGivenGLListForSave
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
                $scope.getdebitNoteTypeGivenWithCoa('all');
            }
        } else {
            $scope.getdebitNoteTypeGivenWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshLiabilityGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.vendorTranTypeGivenGL = { COAId: $scope.vendorTranTypeGivenGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.vendorTranTypeGivenGLWithCombineList = [];
    }
}