'use strict';
taxCategoryRCMOutputController.$inject = ['addressService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function taxCategoryRCMOutputController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "TaxCategory Account Determinate";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.taxCategoryRCMOutputList = [];
    $scope.taxCategoryRCMOutputWithCombineList = [];
    $scope.path = 'accounts/taxCategoryGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateTaxCategoryDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.taxCategoryRCMOutput = {  
        Id: null,
        CountryId: null,
        TaxCategoryId: null,
        COAId: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        LiabilityGLId: null,
        LiabilityBudgetMasterId: null,
        LiabilityActivityId: null,
        InputTaxOutPutTax: "Output",
        TaxType: 'Excluded'
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
                    $scope.taxCategoryRCMOutputWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryRCMOutputWithCombineList.length; i++) {
                        $scope.taxCategoryRCMOutputWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryRCMOutputWithCombineList[i].TaxCategoryId);
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
        // $scope.tempList.splice($scope.tempList.indexOf(TaxCategoryId), 1);
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
        if ($scope.taxCategoryRCMOutput.CountryId === null || $scope.taxCategoryRCMOutput.CountryId === undefined) {
            return ShowResult("Select Country first", 'failure');
        }
        //IdList();
        if (str === 'all') {
            if ($scope.taxCategoryRCMOutput.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/GetListWithCombineExcludedOutputTax?coaId=' + $scope.taxCategoryRCMOutput.COAId + '&countryId=' + $scope.taxCategoryRCMOutput.CountryId + '&inputOutput=' + $scope.taxCategoryRCMOutput.InputTaxOutPutTax;
        }
        if (str === 'notassing') {
            if ($scope.taxCategoryRCMOutput.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/GetListWithCombineNotAssingExcludedOutput?coaId=' + $scope.taxCategoryRCMOutput.COAId + '&countryId=' + $scope.taxCategoryRCMOutput.CountryId + '&inputOutput=' + $scope.taxCategoryRCMOutput.InputTaxOutPutTax;
        }
        if (str === 'assing') {
            if ($scope.taxCategoryRCMOutput.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/GetListWithCombineAssingExcludedOutput?coaId=' + $scope.taxCategoryRCMOutput.COAId + '&countryId=' + $scope.taxCategoryRCMOutput.CountryId + '&inputOutput=' + $scope.taxCategoryRCMOutput.InputTaxOutPutTax;
        }
        $scope.taxCategoryRCMOutputWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'TaxCategoryName', 'TaxCategoryName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCategoryRCMOutputWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryRCMOutputWithCombineList.length; i++) {
                        $scope.taxCategoryRCMOutputWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryRCMOutputWithCombineList[i].TaxCategoryId);
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
        order: 'ASC',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetReconAssetTypeList = function () {
        if ($scope.taxCategoryRCMOutput.COAId === null || $scope.taxCategoryRCMOutput.COAId === undefined)
            return ShowResult("Select COA first.", 'failure');
        $scope.GetReconTypeListData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetAssetGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryRCMOutput.COAId, pageno, $scope.reconTypeListParameters)
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

    $scope.closeReconTypeListPopUpSelected = function (x) {
        if ($scope.rowSelected != null) {
            $scope.AssetGLSelectedData = x;
            $scope.ReconAssetGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.taxCategoryRCMOutput.GLGeneralInfoId = x.GLGeneralInfoId;
            getBudget();
            angular.element(document.querySelector('#ReconTypeListPopUp')).modal('hide');
        }
    };

    $scope.setReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.taxCategoryRCMOutput.GlType = 'FixedAsset';
    };

    $scope.refreshAssetGL = function () {
        $scope.ReconAssetGLInof = null;
        $scope.taxCategoryRCMOutput.GLGeneralInfoId = null;
        $scope.budgetList = [];
        $scope.budgetActivityList = [];
        $scope.taxCategoryRCMOutput.BudgetMasterId = null;
        $scope.taxCategoryRCMOutput.ActivityId = null;
    };

    $scope.budgetList = [];
    function getBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryRCMOutput.COAId, $scope.taxCategoryRCMOutput.GLGeneralInfoId, function (result) {
            $scope.budgetList = result;
        });
    }

    $scope.budgetActivityList = [];
    $scope.getBudgetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryRCMOutput.BudgetMasterId, function (result) {
            $scope.budgetActivityList = result;
        });
    };

    // #endregion

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
        if ($scope.taxCategoryRCMOutput.COAId === null || $scope.taxCategoryRCMOutput.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryRCMOutput.COAId;
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
        if ($scope.rowSelected != null) {
            angular.element(document.querySelector('#liabilityTypeListPopUp')).modal('hide');
        }
    };

    $scope.setLiabilityGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.LiabilityGLInof = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.taxCategoryRCMOutput.LiabilityGLId = x.GLGeneralInfoId;
        $scope.taxCategoryRCMOutput.GlType = 'Laibality';
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.taxCategoryRCMOutput.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.taxCategoryRCMOutput.LiabilityBudgetMasterId = null;
        $scope.taxCategoryRCMOutput.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryRCMOutput.COAId, $scope.taxCategoryRCMOutput.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryRCMOutput.LiabilityBudgetMasterId, function (result) {
            $scope.liabilityActivityList = result;
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
        for (var i = 0; i < $scope.taxCategoryRCMOutputWithCombineList.length; i++) {
            if ($scope.glUntagId != null) {
                if ($scope.taxCategoryRCMOutputWithCombineList[i].Id == $scope.glUntagId) {
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
        $scope.taxCategoryRCMOutputWithCombineList[i] = {
            Id: null,
            TaxCategoryName: $scope.taxCategoryRCMOutputWithCombineList[i].TaxCategoryName,
            COAId: $scope.taxCategoryRCMOutputWithCombineList[i].COAId,
            COAName: $scope.taxCategoryRCMOutputWithCombineList[i].COAName,
            Code: $scope.taxCategoryRCMOutputWithCombineList[i].Code,
            TaxCategoryId: $scope.taxCategoryRCMOutputWithCombineList[i].TaxCategoryId
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
        $scope.taxCategoryRCMOutputListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.taxCategoryRCMOutput.GLGeneralInfoId != null) {
                    item.GLGeneralInfoId = $scope.taxCategoryRCMOutput.GLGeneralInfoId;
                }
                if ($scope.taxCategoryRCMOutput.ActivityId != null) {
                    item.ActivityId = $scope.taxCategoryRCMOutput.ActivityId;
                }
                if ($scope.taxCategoryRCMOutput.BudgetMasterId != null) {
                    item.BudgetMasterId = $scope.taxCategoryRCMOutput.BudgetMasterId;
                }
                if ($scope.taxCategoryRCMOutput.LiabilityGLId != null) {
                    item.LiabilityGLId = $scope.taxCategoryRCMOutput.LiabilityGLId;
                }
                if ($scope.taxCategoryRCMOutput.LiabilityActivityId != null) {
                    item.LiabilityActivityId = $scope.taxCategoryRCMOutput.LiabilityActivityId;
                }
                if ($scope.taxCategoryRCMOutput.LiabilityBudgetMasterId != null) {
                    item.LiabilityBudgetMasterId = $scope.taxCategoryRCMOutput.LiabilityBudgetMasterId;
                }
                item.COAId = $scope.taxCategoryRCMOutput.COAId;
                item.InputTaxOutPutTax = $scope.taxCategoryRCMOutput.InputTaxOutPutTax;
                item.CountryId = $scope.taxCategoryRCMOutput.CountryId;
                item.TaxType = 'Excluded';
                $scope.taxCategoryRCMOutputListForSave.push(item);
            }
        })
    }

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.taxCategoryRCMOutputListForSave.length < 1) {
            return ShowResult("Please select Tax Category!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.taxCategoryRCMOutput.GLGeneralInfoId)) {
            return ShowResult("Please select Asset GL!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.taxCategoryRCMOutput.LiabilityGLId)) {
            return ShowResult("Please select Liability GL!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxCategoryRCMOutputForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'taxCategoryGL': $scope.taxCategoryRCMOutputListForSave
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
        $scope.refreshLiabilityGL();
        var tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.taxCategoryRCMOutput = {
            CountryId: $scope.taxCategoryRCMOutput.CountryId
            , COAId: $scope.taxCategoryRCMOutput.COAId
            , InputTaxOutPutTax: $scope.taxCategoryRCMOutput.InputTaxOutPutTax
        };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.taxCategoryRCMOutputWithCombineList = [];
    }
}