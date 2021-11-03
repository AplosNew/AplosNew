'use strict';
TaxCategoryGLOutputController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TaxCategoryGLOutputController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "TaxCategory Account Determinate";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.taxCategoryGLOutputList = [];
    $scope.taxCategoryGLOutputWithCombineList = [];
    $scope.path = 'accounts/taxCategoryGLOutput/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateTaxCategoryDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.taxCategoryGLOutput = {
        Id: null,
        CountryId: null,
        TaxCategoryId: null,
        GLGeneralInfoId: null,
        COAId: null,
        TaxCategoryMasterId: null,
        BudgetMasterId: null,
        ActivityId: null,
        InputTaxOutPutTax: "Output",
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

    $scope.budgetList = [];
    function getBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryGLOutput.COAId, $scope.taxCategoryGLOutput.GLGeneralInfoId, function (result) {
            $scope.budgetList = result;
        });
    }
    $scope.budgetActivityList = [];
    $scope.getBudgetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryGLOutput.BudgetMasterId, function (result) {
            $scope.budgetActivityList = result;
        });
    };
    /*******************CBO-END***************/

    $scope.getDataWithCoaChange = function () {
        baseService.init('accounts/taxCategoryGLOutput/getlistwithcombineCoa', null, null, null, "TaxCategoryName", "TaxCategoryName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCategoryGLOutputWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryGLOutputWithCombineList.length; i++) {
                        $scope.taxCategoryGLOutputWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryGLOutputWithCombineList[i].TaxCategoryId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    // $scope.getDataWithCoaChange();
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
        if ($scope.taxCategoryGLOutput.CountryId === null || $scope.taxCategoryGLOutput.CountryId === undefined) {
            return ShowResult("Select Country first", 'failure');
        }
        //IdList();
        if (str === 'all') {
            if ($scope.taxCategoryGLOutput.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGLOutput/getlistwithcombine?coaId=' + $scope.taxCategoryGLOutput.COAId + '&countryId=' + $scope.taxCategoryGLOutput.CountryId + '&inputOutput=' + $scope.taxCategoryGLOutput.InputTaxOutPutTax;
        }
        if (str === 'notassing') {
            if ($scope.taxCategoryGLOutput.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGLOutput/getlistwithcombinenotassing?coaId=' + $scope.taxCategoryGLOutput.COAId + '&countryId=' + $scope.taxCategoryGLOutput.CountryId + '&inputOutput=' + $scope.taxCategoryGLOutput.InputTaxOutPutTax;
        }
        if (str === 'assing') {
            if ($scope.taxCategoryGLOutput.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/taxCategoryGLOutput/getlistwithcombineassing?coaId=' + $scope.taxCategoryGLOutput.COAId + '&countryId=' + $scope.taxCategoryGLOutput.CountryId + '&inputOutput=' + $scope.taxCategoryGLOutput.InputTaxOutPutTax;
        }
        $scope.taxCategoryGLOutputWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'TaxCategoryName', 'TaxCategoryName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCategoryGLOutputWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryGLOutputWithCombineList.length; i++) {
                        $scope.taxCategoryGLOutputWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryGLOutputWithCombineList[i].TaxCategoryId);
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
            if (value == "''") {
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
    $scope.GetReconAssetTypeList = function () {
        if ($scope.taxCategoryGLOutput.COAId === null || $scope.taxCategoryGLOutput.COAId === undefined)
            return ShowResult("Select COA first.", 'failure');
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryGLOutput.COAId;
        $scope.GetReconTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.reconTypeListParameters)
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
    $scope.closeReconTypeListPopUpSelected = function () {
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#ReconTypeListPopUp')).modal('hide');
        }
    };
    $scope.setReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.AssetGLSelectedData = x;
        $scope.ReconAssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.taxCategoryGLOutput.GLGeneralInfoId = x.GLGeneralInfoId;
        getBudget();
    };

    $scope.refreshAssetGL = function () {
        $scope.ReconAssetGLInof = null;
        $scope.taxCategoryGLOutput.GLGeneralInfoId = null;
        $scope.budgetList = [];
        $scope.budgetActivityList = [];
        $scope.taxCategoryGLOutput.BudgetMasterId = null;
        $scope.taxCategoryGLOutput.ActivityId = null;
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
        for (var i = 0; i < $scope.taxCategoryGLOutputWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.taxCategoryGLOutputWithCombineList[i].Id === $scope.glUntagId) {
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
        $scope.taxCategoryGLOutputWithCombineList[i] = {
            Id: null,
            TaxCategoryName: $scope.taxCategoryGLOutputWithCombineList[i].TaxCategoryName,
            COAId: $scope.taxCategoryGLOutputWithCombineList[i].COAId,
            COAName: $scope.taxCategoryGLOutputWithCombineList[i].COAName,
            Code: $scope.taxCategoryGLOutputWithCombineList[i].Code,
            TaxCategoryId: $scope.taxCategoryGLOutputWithCombineList[i].TaxCategoryId
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
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.tempList.length; i++) {
                        if ($scope.tempList[i].Id === id) {
                            document.getElementById($scope.tempList[i].TaxCategoryId).checked = false;
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
        $scope.taxCategoryGLOutputListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.taxCategoryGLOutput.GLGeneralInfoId != null) {
                    item.GLGeneralInfoId = $scope.taxCategoryGLOutput.GLGeneralInfoId;
                }
                if ($scope.taxCategoryGLOutput.ActivityId != null) {
                    item.ActivityId = $scope.taxCategoryGLOutput.ActivityId;
                }
                if ($scope.taxCategoryGLOutput.BudgetMasterId != null) {
                    item.BudgetMasterId = $scope.taxCategoryGLOutput.BudgetMasterId;
                }
                item.COAId = $scope.taxCategoryGLOutput.COAId;
                item.InputTaxOutPutTax = $scope.taxCategoryGLOutput.InputTaxOutPutTax;
                item.CountryId = $scope.taxCategoryGLOutput.CountryId;
                item.TaxType = $scope.taxCategoryGLOutput.TaxType;
                $scope.taxCategoryGLOutputListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.taxCategoryGLOutputListForSave.length < 1) {
            return ShowResult("Please select Tax Category!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.taxCategoryGLOutput.GLGeneralInfoId)) {
            return ShowResult("Please select Liability GL!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxCategoryGLOutputForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'taxCategoryGLOutput': $scope.taxCategoryGLOutputListForSave
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
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
        var tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.taxCategoryGLOutput = {
            CountryId: $scope.taxCategoryGLOutput.CountryId
            , COAId: $scope.taxCategoryGLOutput.COAId
            , InputTaxOutPutTax: $scope.taxCategoryGLOutput.InputTaxOutPutTax
        };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.taxCategoryGLOutputWithCombineList = [];
    }
}