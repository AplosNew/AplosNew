'use strict';
TaxCategoryGLController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function TaxCategoryGLController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Tax Category GL";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.taxCategoryGLList = [];
    $scope.taxCategoryGLWithCombineList = [];
    $scope.expensesTypeGLList = [];
    $scope.path = 'accounts/taxCategoryGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateTaxCategoryDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.taxCategoryGL = {
        Id: null,
        CountryId: null,
        TaxCategoryId: null,
        GLGeneralInfoId: null,
        COAId: null,
        TaxCategoryMasterId: null,
        BudgetMasterId: null,
        ActivityId: null,
        InputTaxOutPutTax: "Input",
        TaxType: null
    };
    $scope.taxCategoryWithholdGL = {
        Id: null,
        CountryId: null,
        TaxCategoryId: null,
        GLGeneralInfoId: null,
        COAId: null,
        TaxCategoryMasterId: null,
        BudgetMasterId: null,
        ActivityId: null,
        InputTaxOutPutTax: "Input",
        TaxType: null
    };
    $scope.taxCategoryExpensesGL = {
        Id: null,
        CountryId: null,
        TaxCategoryId: null,
        GLGeneralInfoId: null,
        COAId: null,
        TaxCategoryMasterId: null,
        BudgetMasterId: null,
        ActivityId: null,
        InputTaxOutPutTax: "Input",
        TaxType: null
    };
    
    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector('#itemsearchpopup')).modal('show');
    };

    $scope.taxCategoryList = [];
    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
    });

    $scope.getDataWithCoaChange = function () {
        baseService.init('accounts/taxCategoryGL/getlistwithcombineCoa', null, null, null, "TaxCategoryName", "TaxCategoryName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCategoryGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryGLWithCombineList.length; i++) {
                        $scope.taxCategoryGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryGLWithCombineList[i].TaxCategoryId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        if (event.currentTarget.checked) {
            $scope.tempList.push(data);
        }
        else {
            for (var i = 0; i < $scope.tempList.length; i++) {
                if ($scope.tempList[i].TaxCategoryId === data.TaxCategoryId) {
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
        if ($scope.taxCategoryGL.CountryId === null || $scope.taxCategoryGL.CountryId === undefined) {
            return ShowResult("Select Country first.", 'failure');
        }
        if (str === 'all') {
            if ($scope.taxCategoryGL.COAId === null) {
                return ShowResult('Select COA first.', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/getlistwithcombine?coaId=' + $scope.taxCategoryGL.COAId + '&countryId=' + $scope.taxCategoryGL.CountryId + '&inputOutput=' + $scope.taxCategoryGL.InputTaxOutPutTax;
        }
        if (str === 'notassing') {
            if ($scope.taxCategoryGL.COAId === null) {
                return ShowResult('Select COA first.', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/getlistwithcombinenotassing?coaId=' + $scope.taxCategoryGL.COAId + '&countryId=' + $scope.taxCategoryGL.CountryId + '&inputOutput=' + $scope.taxCategoryGL.InputTaxOutPutTax;
        }
        if (str === 'assing') {
            if ($scope.taxCategoryGL.COAId === null) {
                return ShowResult("Select COA first.", 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/getlistwithcombineassing?coaId=' + $scope.taxCategoryGL.COAId + '&countryId=' + $scope.taxCategoryGL.CountryId + '&inputOutput=' + $scope.taxCategoryGL.InputTaxOutPutTax;
        }
        $scope.taxCategoryGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'TaxCategoryName', 'TaxCategoryName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCategoryGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryGLWithCombineList.length; i++) {
                        $scope.taxCategoryGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryGLWithCombineList[i].TaxCategoryId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    function IdList() {
        $scope.taxCategoryIdstr = createIdList(validListWithStr($scope.taxCategoryClassNewList, $scope.fixedassetClassIds));
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

    $scope.reconTypeListSearchByList = [
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
    $scope.reconTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.reconWithholdTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetReconAssetTypeList = function () {
        if ($scope.taxCategoryGL.COAId === null || $scope.taxCategoryGL.COAId === undefined)
            return ShowResult("Select COA first", 'failure');
        $scope.GetReconTypeListData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetAssetGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryGL.COAId, pageno, $scope.reconTypeListParameters)
                .then(function (data) {
                    $scope.ReconTypeGLList = data.Rows;
                    $scope.reconTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ReconTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetReconTypeListData();
    };


    $scope.GetReconWithholdTypeList = function () {
        if ($scope.taxCategoryGL.COAId === null || $scope.taxCategoryGL.COAId === undefined)
            return ShowResult("Select COA first", 'failure');
        $scope.GetReconWithholdTypeListData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetLiabilityGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryGL.COAId, pageno, $scope.reconWithholdTypeListParameters)
                .then(function (data) {
                    $scope.ReconWithholdTypeGLList = data.Rows;
                    $scope.reconWithholdTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ReconWithholdGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetReconWithholdTypeListData();
    };

    $scope.closeReconTypeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            $scope.AssetGLSelectedData = x;
            $scope.ReconAssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.taxCategoryGL.GLGeneralInfoId = x.GLGeneralInfoId;
            getBudget();
            angular.element(document.querySelector('#ReconTypeListPopUp')).modal('hide');
        }
    };

    $scope.setReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
    };


    $scope.closeReconWithholdListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            $scope.WithholdGLSelectedData = x;
            $scope.ReconWithholdGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.taxCategoryWithholdGL.GLGeneralInfoId = x.GLGeneralInfoId;
            getWithholdBudget();
            angular.element(document.querySelector('#ReconWithholdGLListPopUp')).modal('hide');
        }
    };

    $scope.setReconWithholdGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
    };

    $scope.refreshAssetGL = function () {
        $scope.ReconAssetGLInof = null;
        $scope.taxCategoryGL.GLGeneralInfoId = null;
        $scope.budgetList = [];
        $scope.budgetActivityList = [];
        $scope.taxCategoryGL.BudgetMasterId = null;
        $scope.taxCategoryGL.ActivityId = null;
    };

    $scope.refreshWithholdGL = function () {
        $scope.ReconWithholdGLInfo = null;
        $scope.taxCategoryWithholdGL.GLGeneralInfoId = null;
        $scope.budgetList = [];
        $scope.budgetActivityList = [];
        $scope.taxCategoryWithholdGL.BudgetMasterId = null;
        $scope.taxCategoryWithholdGL.ActivityId = null;
    };
    $scope.budgetList = [];
    function getBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryGL.COAId, $scope.taxCategoryGL.GLGeneralInfoId, function (result) {
            $scope.budgetList = result;
        });
    }

    $scope.budgetActivityList = [];
    $scope.getBudgetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryGL.BudgetMasterId, function (result) {
            $scope.budgetActivityList = result;
        });
    };

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.TaxCategoryName + ' ]?';
        angular.element(document.querySelector('#glUntag')).modal('show');
    };

    $scope.removeRow = function () {
        for (var i = 0; i < $scope.taxCategoryGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.taxCategoryGLWithCombineList[i].Id === $scope.glUntagId) {
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
        $scope.taxCategoryGLWithCombineList[i] = {
            Id: null,
            TaxCategoryName: $scope.taxCategoryGLWithCombineList[i].TaxCategoryName,
            COAId: $scope.taxCategoryGLWithCombineList[i].COAId,
            COAName: $scope.taxCategoryGLWithCombineList[i].COAName,
            Code: $scope.taxCategoryGLWithCombineList[i].Code,
            TaxCategoryId: $scope.taxCategoryGLWithCombineList[i].TaxCategoryId
        };
    }

    $scope.unTagGL = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/delete',
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
                            document.getElementById($scope.tempList[i].SecurityTypeTakenId).checked = false;
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
        $scope.taxCategoryGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.taxCategoryGL.GLGeneralInfoId !== null) {
                    item.GLGeneralInfoId = $scope.taxCategoryGL.GLGeneralInfoId;
                }
                if ($scope.taxCategoryGL.ActivityId !== null) {
                    item.ActivityId = $scope.taxCategoryGL.ActivityId;
                }
                if ($scope.taxCategoryGL.BudgetMasterId !== null) {
                    item.BudgetMasterId = $scope.taxCategoryGL.BudgetMasterId;
                }
                if ($scope.taxCategoryWithholdGL.GLGeneralInfoId !== null) {
                    item.WithholdGLId = $scope.taxCategoryWithholdGL.GLGeneralInfoId;
                }
                if ($scope.taxCategoryWithholdGL.ActivityId !== null) {
                    item.WithholdActivityId = $scope.taxCategoryWithholdGL.ActivityId;
                }
                if ($scope.taxCategoryWithholdGL.BudgetMasterId !== null) {
                    item.WithholdBudgetMasterId = $scope.taxCategoryWithholdGL.BudgetMasterId;
                }


                if ($scope.taxCategoryExpensesGL.GLGeneralInfoId !== null) {
                    item.ExpensesGLId = $scope.taxCategoryExpensesGL.GLGeneralInfoId;
                }
                if ($scope.taxCategoryExpensesGL.ActivityId !== null) {
                    item.ExpensesActivityId = $scope.taxCategoryExpensesGL.ActivityId;
                }
                if ($scope.taxCategoryExpensesGL.BudgetMasterId !== null) {
                    item.ExpensesBudgetMasterId = $scope.taxCategoryExpensesGL.BudgetMasterId;
                }


                item.COAId = $scope.taxCategoryGL.COAId;
                item.InputTaxOutPutTax = $scope.taxCategoryGL.InputTaxOutPutTax;
                item.CountryId = $scope.taxCategoryGL.CountryId;
                item.TaxType = $scope.taxCategoryGL.TaxType;
                $scope.taxCategoryGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.taxCategoryGLListForSave.length < 1) {
            return ShowResult("Please select Tax Category!", 'failure');
        }
        //if (baseService.isUndefinedOrNull($scope.taxCategoryGL.GLGeneralInfoId)) {
        //    return ShowResult("Please select Asset GL!", 'failure');
        //}
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxCategoryGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'taxCategoryGL': $scope.taxCategoryGLListForSave
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
        if ($scope.btnSet != '') {
            if ($scope.btnSet === 'all') {
                $scope.getTaxCategoryWithCoa('all');
            }
        } else {
            $scope.getTaxCategoryWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.refreshWithholdGL();
        $scope.refreshExpensesGL();
        var tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.taxCategoryGL = {
            CountryId: $scope.taxCategoryGL.CountryId
            , COAId: $scope.taxCategoryGL.COAId
            , InputTaxOutPutTax: $scope.taxCategoryGL.InputTaxOutPutTax
        };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.taxCategoryGLWithCombineList = [];
    }

    $scope.withholdBudgetList = [];
    function getWithholdBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryGL.COAId, $scope.taxCategoryWithholdGL.GLGeneralInfoId, function (result) {
            $scope.withholdBudgetList = result;
        });
    }

    $scope.withholdBudgetActivityList = [];
    $scope.getWithholdBudgetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryWithholdGL.BudgetMasterId, function (result) {
            $scope.withholdBudgetActivityList = result;
        });
    };


    $scope.searchExpensesTypeByList = [
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

    $scope.expensesTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetExpensesTypeList = function () {
        if ($scope.taxCategoryGL.COAId === null || $scope.taxCategoryGL.COAId === undefined)
            return ShowResult("Select COA first", 'failure');
        $scope.getExpensesTypeListData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetExpenseGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryGL.COAId, pageno, $scope.expensesTypeListParameters)
                .then(function (data) {
                    $scope.expensesTypeGLList = data.Rows;
                    $scope.expensesTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#expensesTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getExpensesTypeListData();
    };

    $scope.setExpensesGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.ExpensesGLSelectedData = x;
        $scope.closeExpensesTypeListPopUpSelected(x);
    };

    $scope.refreshExpensesGL = function () {
        $scope.ExpensesGLInfo = null;
        $scope.taxCategoryExpensesGL.GLGeneralInfoId = null;
        $scope.budgetList = [];
        $scope.budgetActivityList = [];
        $scope.taxCategoryExpensesGL.BudgetMasterId = null;
        $scope.taxCategoryExpensesGL.ActivityId = null;
    };


    $scope.closeExpensesTypeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            $scope.ExpensesGLInfo = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.taxCategoryExpensesGL.GLGeneralInfoId = x.GLGeneralInfoId;
            getExpensesBudget();
            angular.element(document.querySelector('#expensesTypeListPopUp')).modal('hide');
        }
    };

    $scope.expensesBudgetList = [];
    function getExpensesBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryGL.COAId, $scope.taxCategoryExpensesGL.GLGeneralInfoId, function (result) {
            $scope.expensesBudgetList = result;
        });
    }

    $scope.expensesBudgetActivityList = [];
    $scope.getExpensesBudgetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryExpensesGL.BudgetMasterId, function (result) {
            $scope.expensesBudgetActivityList = result;
        });
    };


}