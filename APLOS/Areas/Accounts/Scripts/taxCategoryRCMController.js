'use strict';
TaxCategoryRCMController.$inject = ['addressService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TaxCategoryRCMController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "TaxCategory Account Determinate";
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.taxCategoryRCMList = [];
    $scope.taxCategoryRCMWithCombineList = [];
    $scope.path = 'accounts/taxCategoryGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'UpdateTaxCategoryDeterminate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.taxCategoryRCM = {
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
        InputTaxOutPutTax: "Input",
        TaxType: 'RCM'
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
                    $scope.taxCategoryRCMWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryRCMWithCombineList.length; i++) {
                        $scope.taxCategoryRCMWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryRCMWithCombineList[i].TaxCategoryId);
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
        if ($scope.taxCategoryRCM.CountryId === null || $scope.taxCategoryRCM.CountryId === undefined) {
            return ShowResult("Select Country first", 'failure');
        }
        //IdList();
        if (str === 'all') {
            if ($scope.taxCategoryRCM.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/GetListWithCombineRCM?coaId=' + $scope.taxCategoryRCM.COAId + '&countryId=' + $scope.taxCategoryRCM.CountryId + '&inputOutput=' + $scope.taxCategoryRCM.InputTaxOutPutTax;
        }
        if (str === 'notassing') {
            if ($scope.taxCategoryRCM.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/GetListWithCombineNotAssingRCM?coaId=' + $scope.taxCategoryRCM.COAId + '&countryId=' + $scope.taxCategoryRCM.CountryId + '&inputOutput=' + $scope.taxCategoryRCM.InputTaxOutPutTax;
        }
        if (str === 'assing') {
            if ($scope.taxCategoryRCM.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'accounts/taxCategoryGL/GetListWithCombineAssingRCM?coaId=' + $scope.taxCategoryRCM.COAId + '&countryId=' + $scope.taxCategoryRCM.CountryId + '&inputOutput=' + $scope.taxCategoryRCM.InputTaxOutPutTax;
        }
        $scope.taxCategoryRCMWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'TaxCategoryName', 'TaxCategoryName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxCategoryRCMWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.taxCategoryRCMWithCombineList.length; i++) {
                        $scope.taxCategoryRCMWithCombineList[i].Flag = getActive($scope.tempList, $scope.taxCategoryRCMWithCombineList[i].TaxCategoryId);
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
        if ($scope.taxCategoryRCM.COAId === null || $scope.taxCategoryRCM.COAId === undefined)
            return ShowResult("Select COA first.", 'failure');
        $scope.GetReconTypeListData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetAssetGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryRCM.COAId, pageno, $scope.reconTypeListParameters)
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
            $scope.taxCategoryRCM.GLGeneralInfoId = x.GLGeneralInfoId;
            getBudget();
            angular.element(document.querySelector('#ReconTypeListPopUp')).modal('hide');
        }
    };

    $scope.setReconTypeGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.taxCategoryRCM.GlType = 'FixedAsset';
    };

    $scope.refreshAssetGL = function () {
        $scope.ReconAssetGLInof = null;
        $scope.taxCategoryRCM.GLGeneralInfoId = null;
        $scope.budgetList = [];
        $scope.budgetActivityList = [];
        $scope.taxCategoryRCM.BudgetMasterId = null;
        $scope.taxCategoryRCM.ActivityId = null;
    };

    $scope.budgetList = [];
    function getBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryRCM.COAId, $scope.taxCategoryRCM.GLGeneralInfoId, function (result) {
            $scope.budgetList = result;
        });
    }

    $scope.budgetActivityList = [];
    $scope.getBudgetActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryRCM.BudgetMasterId, function (result) {
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
        if ($scope.taxCategoryRCM.COAId === null || $scope.taxCategoryRCM.COAId === undefined) {
            return ShowResult('Select COA first', 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetLiabilityGLCOAWiseTaxRecon?coaId=' + $scope.taxCategoryRCM.COAId;
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
        $scope.taxCategoryRCM.LiabilityGLId = x.GLGeneralInfoId;
        $scope.taxCategoryRCM.GlType = 'Laibality';
        getLiabilityBudget();
    };

    $scope.refreshLiabilityGL = function () {
        $scope.LiabilityGLInof = null;
        $scope.taxCategoryRCM.LiabilityGLId = null;
        $scope.liabilityBudgetList = [];
        $scope.liabilityActivityList = [];
        $scope.taxCategoryRCM.LiabilityBudgetMasterId = null;
        $scope.taxCategoryRCM.LiabilityActivityId = null;
    };

    $scope.liabilityBudgetList = [];
    function getLiabilityBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.taxCategoryRCM.COAId, $scope.taxCategoryRCM.LiabilityGLId, function (result) {
            $scope.liabilityBudgetList = result;
        });
    }

    $scope.liabilityActivityList = [];
    $scope.getLiabilityActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.taxCategoryRCM.LiabilityBudgetMasterId, function (result) {
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
        for (var i = 0; i < $scope.taxCategoryRCMWithCombineList.length; i++) {
            if ($scope.glUntagId != null) {
                if ($scope.taxCategoryRCMWithCombineList[i].Id == $scope.glUntagId) {
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
        $scope.taxCategoryRCMWithCombineList[i] = {
            Id: null,
            TaxCategoryName: $scope.taxCategoryRCMWithCombineList[i].TaxCategoryName,
            COAId: $scope.taxCategoryRCMWithCombineList[i].COAId,
            COAName: $scope.taxCategoryRCMWithCombineList[i].COAName,
            Code: $scope.taxCategoryRCMWithCombineList[i].Code,
            TaxCategoryId: $scope.taxCategoryRCMWithCombineList[i].TaxCategoryId
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
        $scope.taxCategoryRCMListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.taxCategoryRCM.GLGeneralInfoId != null) {
                    item.GLGeneralInfoId = $scope.taxCategoryRCM.GLGeneralInfoId;
                }
                if ($scope.taxCategoryRCM.ActivityId != null) {
                    item.ActivityId = $scope.taxCategoryRCM.ActivityId;
                }
                if ($scope.taxCategoryRCM.BudgetMasterId != null) {
                    item.BudgetMasterId = $scope.taxCategoryRCM.BudgetMasterId;
                }
                if ($scope.taxCategoryRCM.LiabilityGLId != null) {
                    item.LiabilityGLId = $scope.taxCategoryRCM.LiabilityGLId;
                }
                if ($scope.taxCategoryRCM.LiabilityActivityId != null) {
                    item.LiabilityActivityId = $scope.taxCategoryRCM.LiabilityActivityId;
                }
                if ($scope.taxCategoryRCM.LiabilityBudgetMasterId != null) {
                    item.LiabilityBudgetMasterId = $scope.taxCategoryRCM.LiabilityBudgetMasterId;
                }
                item.COAId = $scope.taxCategoryRCM.COAId;
                item.InputTaxOutPutTax = $scope.taxCategoryRCM.InputTaxOutPutTax;
                item.CountryId = $scope.taxCategoryRCM.CountryId;
                item.TaxType = 'RCM';
                $scope.taxCategoryRCMListForSave.push(item);
            }
        })
    }

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.taxCategoryRCMListForSave.length < 1) {
            return ShowResult("Please select Tax Category!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.taxCategoryRCM.GLGeneralInfoId)) {
            return ShowResult("Please select Asset GL!", 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.taxCategoryRCM.LiabilityGLId)) {
            return ShowResult("Please select Liability GL!", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxCategoryRCMForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'taxCategoryGL': $scope.taxCategoryRCMListForSave
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
        $scope.taxCategoryRCM = {
            CountryId: $scope.taxCategoryRCM.CountryId
            , COAId: $scope.taxCategoryRCM.COAId
            , InputTaxOutPutTax: $scope.taxCategoryRCM.InputTaxOutPutTax
        };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.taxCategoryRCMWithCombineList = [];
    }
}