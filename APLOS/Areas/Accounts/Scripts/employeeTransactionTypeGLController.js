'use strict';
employeeTransactionTypeGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http'];
function employeeTransactionTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = 'Employee Transaction Type GL';
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.employeeTransactionTypeGLList = [];
    $scope.employeeTransactionTypeGLWithCombineList = [];
    $scope.path = 'accounts/EmployeeTransaction/';
    $scope.saveUrl = $scope.path + 'SaveEmployeeTransactionTypeGL';
    $scope.deleteUrl = $scope.path + 'DeleteEmployeeTransactionTypeGL';

    $scope.employeeTransactionTypeGL = {
        Id: null,
        CountryId: null,
        EmployeeTransactionTypeId: null,
        AdvanceGLId: null,
        PayableGLId: null,
        AdvanceBudgetMasterId: null,
        AdvanceActivityId: null,
        PayableBudgetMasterId: null,
        PayableActivityId: null,
        COAId: null,
        IsExpensesBooking: false
    };

    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector('#itemsearchpopup')).modal('show');
    };

    $scope.employeeTransactionTypeList = [];
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
                if ($scope.tempList[i].EmployeeTransactionTypeId === data.EmployeeTransactionTypeId) {
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
            if ($scope.employeeTransactionTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/EmployeeTransaction/GetEmployeeTransactionTypeGLAllList?coaId=' + $scope.employeeTransactionTypeGL.COAId + '&countryId=' + $scope.employeeTransactionTypeGL.CountryId + '&inputOutput=' + $scope.employeeTransactionTypeGL.InputTaxOutPutTax;
        }
        if (str === 'notassing') {
            if ($scope.employeeTransactionTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/EmployeeTransaction/GetEmployeeTransactionTypeGLNotAssingList?coaId=' + $scope.employeeTransactionTypeGL.COAId;
        }
        if (str === 'assing') {
            if ($scope.employeeTransactionTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/EmployeeTransaction/GetEmployeeTransactionTypeGLAssingList?coaId=' + $scope.employeeTransactionTypeGL.COAId;
        }
        $scope.employeeTransactionTypeGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'EmployeeTransactionTypeName', 'EmployeeTransactionTypeName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.employeeTransactionTypeGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.employeeTransactionTypeGLWithCombineList.length; i++) {
                        $scope.employeeTransactionTypeGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.employeeTransactionTypeGLWithCombineList[i].EmployeeTransactionTypeId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    function IdList() {
        $scope.employeeTransactionTypeIdstr = createIdList(validListWithStr($scope.employeeTransactionTypeClassNewList, $scope.fixedassetClassIds));
    }

    function createIdList(list) {
        var value = "''";
        for (var i = 0; i < list.length; i++) {
            if (value === "''") {
                value = "'" + list[i].Value + "'";
            } else {
                value += ",'" + list[i].Value + "'";
            }
        }
        return value;
    }

    $scope.searchReconypeByList = [
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
    $scope.recontypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetReconTypeList = function () {
        if ($scope.employeeTransactionTypeGL.COAId === null || $scope.employeeTransactionTypeGL.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetEmployeeReconAssetWise?coaId=' + $scope.employeeTransactionTypeGL.COAId;
        $scope.GetReconTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.recontypeListParameters)
                .then(function (data) {
                    $scope.ReconTypeGLList = data.Rows;
                    $scope.recontypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ReconTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetReconTypeListData();
    };

    $scope.closeReconTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#ReconTypeListPopUp')).modal('hide');
        }
    };

    $scope.setReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.ReconAssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.employeeTransactionTypeGL.AdvanceGLId = x.GLGeneralInfoId;
        $scope.getCboBudgetByGLId();
    };

    $scope.refreshAssetGL = function () {
        $scope.ReconAssetGLInof = null;
        $scope.employeeTransactionTypeGL.GLGeneralInfoId = null;
        $scope.advanceBudgetList = [];
        $scope.advanceActivityList = [];
        $scope.employeeTransactionTypeGL.AdvanceBudgetMasterId = null;
        $scope.employeeTransactionTypeGL.AdvanceActivityId = null;
    };

    $scope.advanceBudgetList = [];
    $scope.getCboBudgetByGLId = function () {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.employeeTransactionTypeGL.COAId, $scope.employeeTransactionTypeGL.AdvanceGLId, function (result) {
            $scope.advanceBudgetList = result;
        });
    };

    $scope.advanceActivityList = [];
    $scope.getAdvanceActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.employeeTransactionTypeGL.AdvanceBudgetMasterId, function (result) {
            $scope.advanceActivityList = result;
        });
    };

    $scope.searchPayableTypeByList = [
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
    $scope.payableTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetPayableTypeList = function () {
        if ($scope.employeeTransactionTypeGL.COAId === null || $scope.employeeTransactionTypeGL.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetEmployeeReconLiabilityCOAWise?coaId=' + $scope.employeeTransactionTypeGL.COAId;
        $scope.GetPayableTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.payableTypeListParameters)
                .then(function (data) {
                    $scope.PayableTypeGLList = data.Rows;
                    $scope.payableTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#PayableTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetPayableTypeListData();
    };
    $scope.closePayableTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#PayableTypeListPopUp')).modal('hide');
        }
    };
    $scope.setLiabilityGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.PayableGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.employeeTransactionTypeGL.PayableGLId = x.GLGeneralInfoId;
        getPayableBudget();
    };
    $scope.refreshPayableGL = function () {
        $scope.PayableGLInof = null;
        $scope.employeeTransactionTypeGL.PayableGLId = null;
        $scope.payableBudgetList = [];
        $scope.payableActivityList = [];
        $scope.employeeTransactionTypeGL.PayableBudgetMasterId = null;
        $scope.employeeTransactionTypeGL.PayableActivityId = null;
    };

    $scope.payableBudgetList = [];
    function getPayableBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.employeeTransactionTypeGL.COAId, $scope.employeeTransactionTypeGL.PayableGLId, function (result) {
            $scope.payableBudgetList = result;
        });
    }

    $scope.payableActivityList = [];
    $scope.getPayableActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.employeeTransactionTypeGL.PayableBudgetMasterId, function (result) {
            $scope.payableActivityList = result;
        });
    };

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.EmployeeTransactionTypeName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };

    $scope.removeRow = function () {
        for (var i = 0; i < $scope.employeeTransactionTypeGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.employeeTransactionTypeGLWithCombineList[i].Id === $scope.glUntagId) {
                    $scope.unTagGL($scope.glUntagId, i);
                    break;
                }
            } else {
                unTagFromList($scope.glUntagIndex);
                $scope.glUntagIndex = -1;
                break;
            }
        }
        $scope.glUntagId = null;
        $scope.glUntagIndex = -1;
    };
    function unTagFromList(i) {
        $scope.employeeTransactionTypeGLWithCombineList[i].Id = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].AdvanceGLId = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].PayableGLId = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].AdvanceBudgetMasterId = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].AdvanceActivityId = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].PayableBudgetMasterId = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].PayableActivityId = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].AssetGLCode = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].AssetGLText = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].LiabilityGLCode = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].LiabilityGLText = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].AdvanceBudgetName = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].AdvanceActivityName = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].PayableBudgetName = null;
        $scope.employeeTransactionTypeGLWithCombineList[i].PayableActivityName = null;
    }

    $scope.unTagGL = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + '/delete',
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
                            document.getElementById($scope.tempList[i].EmployeeTransactionTypeId).checked = false;
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
        $scope.employeeTransactionTypeGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.employeeTransactionTypeGL.AdvanceGLId !== null) {
                    item.AdvanceGLId = $scope.employeeTransactionTypeGL.AdvanceGLId;
                }
                if ($scope.employeeTransactionTypeGL.PayableGLId !== null) {
                    item.PayableGLId = $scope.employeeTransactionTypeGL.PayableGLId;
                }
                if ($scope.employeeTransactionTypeGL.AdvanceActivityId !== null) {
                    item.AdvanceActivityId = $scope.employeeTransactionTypeGL.AdvanceActivityId;
                }
                if ($scope.employeeTransactionTypeGL.AdvanceBudgetMasterId !== null) {
                    item.AdvanceBudgetMasterId = $scope.employeeTransactionTypeGL.AdvanceBudgetMasterId;
                }
                if ($scope.employeeTransactionTypeGL.PayableActivityId !== null) {
                    item.PayableActivityId = $scope.employeeTransactionTypeGL.PayableActivityId;
                }
                if ($scope.employeeTransactionTypeGL.PayableBudgetMasterId !== null) {
                    item.PayableBudgetMasterId = $scope.employeeTransactionTypeGL.PayableBudgetMasterId;
                }
                item.COAId = $scope.employeeTransactionTypeGL.COAId;
                $scope.employeeTransactionTypeGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.employeeTransactionTypeGLListForSave.length < 1) {
            return ShowResult('Please select Employee Transaction Type!', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.employeeTransactionTypeGL.AdvanceGLId) || baseService.isUndefinedOrNull($scope.employeeTransactionTypeGL.PayableGLId)) {
            return ShowResult('Please select Advance and Payable both side GL!', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.employeeTransactionTypeGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'employeeTransactionTypeGL': $scope.employeeTransactionTypeGLListForSave
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
                $scope.getEmployeeTransactionTypeWithCoa('all');
            }
        } else {
            $scope.getEmployeeTransactionTypeWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.refreshPayableGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.employeeTransactionTypeGL = { COAId: $scope.employeeTransactionTypeGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.employeeTransactionTypeGLWithCombineList = [];
    }
}